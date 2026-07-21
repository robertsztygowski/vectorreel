"""M1a — topic-first CC-BY source discovery + licence audit trail for collection #1
(AI engineering: production RAG / agents / evals / LLM-ops).

CLAUDE.md rule 8: we NEVER download YouTube bytes. The only YouTube access here is the
read-only **YouTube Data API v3** (metadata). No yt-dlp, no scraping, no watch-page HTML.

Licence rule (DECISIONS.md D3/D4): a video is eligible for the `full` tier ONLY if
`videos.list?part=status` returns `status.license == "creativeCommon"`. The API field is the
evidence — a conference website claiming "CC" is not. Everything else is `reference` at best
(index entry only: title/speaker/event/year/tags/deep links, no derived text).

Quota: search.list = 100 units/call, videos.list = 1 unit/call, default 10,000 units/day.
Discovery results are cached in out/raw_search.json so re-runs cost ~0 search quota.
Use --refresh to force new search calls.

Usage:
    python yt_licence_audit.py discover      # search.list -> out/raw_search.json (cached)
    python yt_licence_audit.py verify        # videos.list -> out/candidates.json
    python yt_licence_audit.py build         # curation.json + candidates -> out/corpus.json
    python yt_licence_audit.py all
"""

from __future__ import annotations

import argparse
import json
import os
import re
import sys
import time
from datetime import date, datetime, timezone
from pathlib import Path
from urllib.parse import urlencode
from urllib.request import urlopen
from urllib.error import HTTPError

HERE = Path(__file__).resolve().parent
EXP = HERE.parent
OUT = EXP / "out"
OUT.mkdir(parents=True, exist_ok=True)
REPO = EXP.parent.parent

API = "https://www.googleapis.com/youtube/v3"

RAW_SEARCH = OUT / "raw_search.json"
CANDIDATES = OUT / "candidates.json"
CURATION = EXP / "curation.json"
CORPUS = OUT / "corpus.json"

# ---------------------------------------------------------------- quota accounting

class Quota:
    def __init__(self) -> None:
        self.units = 0
        self.search_calls = 0
        self.exhausted = False

    def spend(self, n: int, kind: str) -> None:
        self.units += n
        if kind == "search":
            self.search_calls += 1


QUOTA = Quota()
MAX_SEARCH_CALLS = 40  # hard budget (4,000 units)


def api_key() -> str:
    env = REPO / ".env"
    if env.exists():
        for line in env.read_text(encoding="utf-8").splitlines():
            line = line.strip()
            if line.startswith("YOUTUBE_API_KEY="):
                return line.split("=", 1)[1].strip().strip('"').strip("'")
    k = os.environ.get("YOUTUBE_API_KEY", "")
    if not k:
        sys.exit("YOUTUBE_API_KEY not found in .env or environment")
    return k


KEY = None


def get(endpoint: str, **params):
    """One Data API GET. Returns (json, ok). Sets QUOTA.exhausted on quotaExceeded."""
    global KEY
    if KEY is None:
        KEY = api_key()
    params["key"] = KEY
    url = f"{API}/{endpoint}?{urlencode(params)}"
    try:
        with urlopen(url, timeout=30) as r:
            return json.loads(r.read().decode("utf-8")), True
    except HTTPError as e:
        body = e.read().decode("utf-8", "replace")
        if "quotaExceeded" in body or e.code == 403 and "quota" in body.lower():
            QUOTA.exhausted = True
            print("!! YouTube API quota exhausted — stopping discovery here.", file=sys.stderr)
        else:
            print(f"!! HTTP {e.code} on {endpoint}: {body[:300]}", file=sys.stderr)
        return None, False


# ---------------------------------------------------------------- discovery

# Topic vocabulary for collection #1. Kept narrow on purpose: the funnel is
# licence -> topic fit -> ICP recognition, so these are the topic-fit terms.
CC_QUERIES = [
    "RAG retrieval augmented generation",
    "LLM in production",
    "AI agents production",
    "LLM evaluation evals",
    "vector database search embeddings",
    "large language model fine-tuning",
    "prompt engineering",
    "LLMOps MLOps LLM",
    "open source LLM inference serving",
    "embeddings semantic search",
    "AI engineering",
    "chatbot LLM architecture",
    "model context protocol MCP",
    "LLM observability monitoring",
    "generative AI application",
    "local LLM llama.cpp ollama",
    "AI agent framework langchain",
    "GPU inference optimization LLM",
]

# Channels known (claimed) to publish CC — verified per-video regardless.
# NOTE: the M1a brief listed FOSDEM as UCLTNKXNBmnKiCr-yYICcXJA. That id resolves to nothing
# (channels.list returns an empty items array) — the real FOSDEM channel is UC9NuJImUbaSNKiwF2bdSfAw.
# The bad id silently returned 0 results, which is exactly how a "barren channel" verdict gets
# manufactured out of a typo. Every channel id below is checked against channels.list.
CC_CHANNELS = {
    "UC9NuJImUbaSNKiwF2bdSfAw": "FOSDEM",
}
CHANNEL_QUERIES = [
    (cid, q) for cid in CC_CHANNELS
    for q in ("LLM", "AI agents", "RAG retrieval", "inference", "vector search", "evaluation")
]

# Reference-tier discovery: any licence, the talks that make the collection cover its
# subject rather than its licences.
REF_QUERIES = [
    "building LLM applications in production conference talk",
    "AI engineer summit agents",
    "RAG at scale conference talk",
    "LLM evaluation in production talk",
    "agentic systems engineering talk",
    "vector search production talk",
]


def discover(refresh: bool = False) -> dict:
    raw = json.loads(RAW_SEARCH.read_text(encoding="utf-8")) if RAW_SEARCH.exists() else {}
    raw.setdefault("cc", {})
    raw.setdefault("ref", {})
    raw.setdefault("meta", {})

    def run(bucket: str, label: str, **params):
        if not refresh and label in raw[bucket]:
            return
        if QUOTA.exhausted or QUOTA.search_calls >= MAX_SEARCH_CALLS:
            return
        js, ok = get("search", part="snippet", type="video", maxResults=50,
                     order="relevance", **params)
        QUOTA.spend(100, "search")
        if not ok:
            return
        ids = [it["id"]["videoId"] for it in js.get("items", []) if it.get("id", {}).get("videoId")]
        raw[bucket][label] = ids
        print(f"  [{bucket}] {label}: {len(ids)} ids  (quota {QUOTA.units}u)")
        time.sleep(0.2)

    print("== discovery: CC-BY filtered searches ==")
    for q in CC_QUERIES:
        run("cc", f"q:{q}", q=q, videoLicense="creativeCommon")
    for cid, q in CHANNEL_QUERIES:
        run("cc", f"ch:{cid}:{q}", q=q, channelId=cid, videoLicense="creativeCommon")

    print("== discovery: any-licence searches (reference tier) ==")
    for q in REF_QUERIES:
        run("ref", f"q:{q}", q=q)

    raw["meta"] = {
        "last_run": datetime.now(timezone.utc).isoformat(timespec="seconds"),
        "search_calls_this_run": QUOTA.search_calls,
        "quota_units_this_run": QUOTA.units,
        "quota_exhausted": QUOTA.exhausted,
    }
    RAW_SEARCH.write_text(json.dumps(raw, indent=2), encoding="utf-8")
    n_cc = len({v for ids in raw["cc"].values() for v in ids})
    n_ref = len({v for ids in raw["ref"].values() for v in ids})
    print(f"discovery: {n_cc} unique CC-filtered ids, {n_ref} unique any-licence ids; "
          f"{QUOTA.search_calls} search calls, {QUOTA.units} units this run")
    return raw


# ---------------------------------------------------------------- verification

def iso8601_to_s(d: str) -> int:
    m = re.match(r"P(?:(\d+)D)?T?(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?", d or "")
    if not m:
        return 0
    dd, h, mi, s = (int(x) if x else 0 for x in m.groups())
    return dd * 86400 + h * 3600 + mi * 60 + s


def verify(extra_ids: list[str] | None = None) -> dict:
    """videos.list on every discovered id. 1 unit per 50 ids. This is the evidence step."""
    raw = json.loads(RAW_SEARCH.read_text(encoding="utf-8"))
    ids = []
    origin = {}
    for bucket in ("cc", "ref"):
        for label, lst in raw.get(bucket, {}).items():
            for v in lst:
                origin.setdefault(v, []).append(f"{bucket}/{label}")
                if v not in ids:
                    ids.append(v)
    for v in (extra_ids or []):
        if v not in ids:
            ids.append(v)
            origin.setdefault(v, []).append("manual")

    out = {}
    for i in range(0, len(ids), 50):
        chunk = ids[i:i + 50]
        js, ok = get("videos", part="snippet,contentDetails,status", id=",".join(chunk))
        QUOTA.spend(1, "videos")
        if not ok:
            break
        for it in js.get("items", []):
            sn, st, cd = it["snippet"], it["status"], it["contentDetails"]
            out[it["id"]] = {
                "video_id": it["id"],
                "url": f"https://www.youtube.com/watch?v={it['id']}",
                "title": sn["title"],
                "channel": sn["channelTitle"],
                "channel_id": sn["channelId"],
                "published_at": sn["publishedAt"],
                "description_head": (sn.get("description") or "")[:400],
                "duration_s": iso8601_to_s(cd.get("duration", "")),
                "licence": st.get("license"),
                "privacy": st.get("privacyStatus"),
                "embeddable": st.get("embeddable", False),
                "found_via": origin.get(it["id"], []),
            }
    payload = {
        "verified_at": datetime.now(timezone.utc).isoformat(timespec="seconds"),
        "verified_via": "youtube.data.api.v3 videos.list part=snippet,contentDetails,status",
        "count": len(out),
        "requested": len(ids),
        "quota_units_this_run": QUOTA.units,
        "videos": out,
    }
    CANDIDATES.write_text(json.dumps(payload, indent=2, ensure_ascii=False), encoding="utf-8")
    cc = sum(1 for v in out.values() if v["licence"] == "creativeCommon")
    print(f"verify: {len(out)}/{len(ids)} resolved; {cc} creativeCommon, {len(out)-cc} youtube-standard")
    return payload


# ---------------------------------------------------------------- corpus build

VERIFIED_VIA = "youtube.data.api.v3 videos.list status.license"
MIN_FULL_S = 8 * 60
MAX_FLAG_S = 90 * 60


def build() -> dict:
    cand = json.loads(CANDIDATES.read_text(encoding="utf-8"))
    cur = json.loads(CURATION.read_text(encoding="utf-8"))
    vids = cand["videos"]
    verified_at = cand["verified_at"]

    full, reference, rejected = [], [], []

    def entry(vid: str, meta: dict, tier: str) -> dict | None:
        v = vids.get(vid)
        if not v:
            rejected.append({"video_id": vid, "title": meta.get("title", "?"),
                             "channel": meta.get("event", "?"),
                             "reason": "not resolvable via videos.list (deleted/private)"})
            return None
        # hard gates, re-applied at build time so the corpus can never drift from the API
        if tier == "full":
            if v["licence"] != "creativeCommon":
                rejected.append({"video_id": vid, "title": v["title"], "channel": v["channel"],
                                 "reason": f"licence={v['licence']} — not CC-BY, full tier denied (D4)"})
                return None
            if v["privacy"] != "public" or not v["embeddable"]:
                rejected.append({"video_id": vid, "title": v["title"], "channel": v["channel"],
                                 "reason": f"privacy={v['privacy']} embeddable={v['embeddable']}"})
                return None
            if v["duration_s"] < MIN_FULL_S:
                rejected.append({"video_id": vid, "title": v["title"], "channel": v["channel"],
                                 "reason": f"too short ({v['duration_s']}s) — lightning talk"})
                return None
        e = {
            "video_id": v["video_id"],
            "url": v["url"],
            "title": v["title"],
            "channel": v["channel"],
            "channel_id": v["channel_id"],
            "event": meta.get("event") or v["channel"],
            "year": int(v["published_at"][:4]) if not meta.get("year") else meta["year"],
            "published_at": v["published_at"],
            "duration_s": v["duration_s"],
            "licence": v["licence"],
            "licence_verified_via": VERIFIED_VIA,
            "licence_verified_at": verified_at,
            "privacy": v["privacy"],
            "embeddable": v["embeddable"],
            "attribution": f"\"{v['title']}\" by {v['channel']} ({v['url']}), CC BY 3.0"
                           if v["licence"] == "creativeCommon" else
                           f"\"{v['title']}\" by {v['channel']} ({v['url']}) — standard YouTube licence, index entry only",
            "topic_tags": meta.get("topic_tags", []),
            "speaker": meta.get("speaker"),
        }
        if v["duration_s"] > MAX_FLAG_S:
            e["cost_flag"] = f"long session ({round(v['duration_s']/60)} min) — Stage B cost risk"
        return e

    for vid, meta in cur.get("full", {}).items():
        e = entry(vid, meta, "full")
        if e:
            full.append(e)
    for vid, meta in cur.get("reference", {}).items():
        e = entry(vid, meta, "reference")
        if e:
            reference.append(e)
    for r in cur.get("rejected", []):
        v = vids.get(r["video_id"], {})
        rejected.append({"video_id": r["video_id"],
                         "title": r.get("title") or v.get("title", "?"),
                         "channel": r.get("channel") or v.get("channel", "?"),
                         "reason": r["reason"]})

    # Complete the audit trail: every resolved candidate that was neither accepted nor
    # explicitly rejected above gets a derived reason. The discards ARE the evidence
    # behind the topic choice (M1a brief), so they are recorded, not silently dropped.
    accepted_ids = {e["video_id"] for e in full} | {e["video_id"] for e in reference}
    named = accepted_ids | {r["video_id"] for r in rejected}
    for vid, v in vids.items():
        if vid in named:
            continue
        if v["privacy"] != "public" or not v["embeddable"]:
            reason = f"not publishable (privacy={v['privacy']}, embeddable={v['embeddable']})"
        elif v["duration_s"] < MIN_FULL_S:
            reason = f"too short for a session document ({v['duration_s']}s)"
        elif v["duration_s"] > 2 * MAX_FLAG_S:
            reason = f"not a session ({round(v['duration_s']/60)} min stream/playlist-length)"
        elif v["licence"] != "creativeCommon":
            reason = "licence=youtube — full tier denied (D4); not selected for reference tier"
        else:
            reason = "CC-BY but not selected: off-topic, duplicate coverage, or weaker than a peer session"
        rejected.append({"video_id": vid, "title": v["title"], "channel": v["channel"],
                         "reason": reason})

    considered = len(vids)
    licence_passed = sum(1 for v in vids.values() if v["licence"] == "creativeCommon")

    # Reproducible topic screen: session-shaped (public, embeddable, 8-120 min) AND the title
    # hits the AI-engineering vocabulary. This is the "topic fit" stage of the funnel; the
    # human curation step then picks the best non-duplicate coverage out of what survives.
    TOPIC_RE = re.compile(
        r"\b(rag|retrieval[- ]augmented|llm|llms|llmops|genai|generative ai|agent|agents|agentic|"
        r"eval|evals|evaluation|embedding|embeddings|vector (search|database|db)|semantic search|"
        r"fine[- ]?tun|prompt engineering|inference|mcp|model context protocol|"
        r"large language model)\b", re.I)

    def session_shaped(v):
        return (v["privacy"] == "public" and v["embeddable"]
                and MIN_FULL_S <= v["duration_s"] <= 120 * 60)

    session_cc = [v for v in vids.values() if v["licence"] == "creativeCommon" and session_shaped(v)]
    topic_cc = [v for v in session_cc if TOPIC_RE.search(v["title"])]
    session_any = [v for v in vids.values() if session_shaped(v)]
    topic_any = [v for v in session_any if TOPIC_RE.search(v["title"])]

    # The honest base rate. `licence_passed` below is inflated *by construction* — most
    # search.list calls carried videoLicense=creativeCommon, so of course most results are CC.
    # The un-filtered ("ref") searches are the only unbiased sample of what the topic's
    # relevance-ranked YouTube supply actually looks like. THIS is the Q2 number.
    raw = json.loads(RAW_SEARCH.read_text(encoding="utf-8"))
    unfiltered_ids = {v for ids in raw.get("ref", {}).values() for v in ids}
    unf = [vids[i] for i in unfiltered_ids if i in vids]
    unf_cc = [v for v in unf if v["licence"] == "creativeCommon"]
    base_rate = {
        "note": "unbiased sample: search.list WITHOUT videoLicense filter, relevance-ranked, "
                "on AI-engineering conference-talk queries. licence_passed below is NOT a base "
                "rate — it is inflated because most searches were CC-filtered on purpose.",
        "unfiltered_results": len(unf),
        "unfiltered_creative_commons": len(unf_cc),
        "cc_share_pct": round(100 * len(unf_cc) / len(unf), 1) if unf else None,
    }
    corpus = {
        "collection": "ai-engineering",
        "generated_at": date.today().isoformat(),
        "licence_rule": "full tier = status.license == creativeCommon only (DECISIONS.md D3/D4); "
                        "reference tier = index entry only, no derived text, any public video",
        "full": sorted(full, key=lambda e: (e["event"], e["published_at"])),
        "reference": sorted(reference, key=lambda e: (e["event"], e["published_at"])),
        "rejected": rejected,
        "licence_base_rate": base_rate,
        "funnel": {
            "candidates_considered": considered,
            "licence_passed": licence_passed,
            "session_shaped": len(session_any),
            "session_shaped_and_cc": len(session_cc),
            "topic_passed": len(topic_any),
            "topic_passed_and_cc": len(topic_cc),
            "accepted_full": len(full),
            "accepted_reference": len(reference),
            "rejected": len(rejected),
        },
    }
    corpus["funnel"]["full_to_reference_ratio"] = (
        round(len(full) / len(reference), 2) if reference else None)
    corpus["funnel"]["full_hours"] = round(sum(e["duration_s"] for e in full) / 3600, 2)
    corpus["funnel"]["distinct_full_channels"] = len({e["channel_id"] for e in full})
    CORPUS.write_text(json.dumps(corpus, indent=2, ensure_ascii=False), encoding="utf-8")
    f = corpus["funnel"]
    print(f"corpus: full={f['accepted_full']} ({f['full_hours']}h, "
          f"{f['distinct_full_channels']} channels)  reference={f['accepted_reference']}  "
          f"rejected={f['rejected']}  ratio={f['full_to_reference_ratio']}")
    return corpus


def main() -> None:
    ap = argparse.ArgumentParser()
    ap.add_argument("cmd", choices=["discover", "verify", "build", "all"])
    ap.add_argument("--refresh", action="store_true", help="force new search.list calls")
    ap.add_argument("--ids", default="", help="extra video ids to verify (comma-separated)")
    a = ap.parse_args()
    extra = [x.strip() for x in a.ids.split(",") if x.strip()]
    if a.cmd in ("discover", "all"):
        discover(refresh=a.refresh)
    if a.cmd in ("verify", "all"):
        verify(extra)
    if a.cmd in ("build", "all"):
        build()
    if QUOTA.exhausted:
        print("NOTE: run ended early on quota exhaustion — results are partial.", file=sys.stderr)


if __name__ == "__main__":
    main()
