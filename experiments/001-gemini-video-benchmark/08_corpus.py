"""Phase 0.2 — licence gate for the public benchmark & demo corpus (PLAN.md).

Nothing may enter the corpus on a guess. A video is admissible ONLY if the YouTube Data
API reports status.license == "creativeCommon" — i.e. the uploader ticked YouTube's
"Creative Commons Attribution (reuse allowed)" flag, which is the only thing that grants
us the right to republish derived Markdown with attribution.

This is metadata only. It fetches no bytes and is not an ingestion path (CLAUDE.md rule 8):
the *only* way video content reaches us is Vertex fileData.fileUri. The YouTube CC search
filter is a discovery hint, not evidence; this script is the evidence, and out/corpus.json
is the audit trail behind every published artifact.

Also records the attribution line each video requires — CC BY is a licence, not a donation.

Usage:  python 08_corpus.py            # verify CANDIDATES, write out/corpus.json
        python 08_corpus.py <id> ...   # verify ad-hoc ids
"""

import json
import os
import re
import sys
from pathlib import Path

import requests

from common import OUT

API = "https://www.googleapis.com/youtube/v3/videos"

# Candidates are *proposals*. The gate below decides which survive; a rejected id stays in
# the record with its reason, so the next person does not re-propose it.
#
# The five were picked from 34 API-verified CC BY videos. Selection criteria, in order:
#   1. The category must be the one A4 is missing (PLAN.md Phase 0.2).
#   2. Prefer EU conferences and technical content — these outputs ARE the Phase 0.3 demo
#      material, aimed at a .NET/architecture audience. A CC licence makes a video legal
#      to publish; relevance makes it worth publishing.
#   3. The two talking-head videos deliberately differ in on-screen text: a bare studio
#      two-shot (near-zero text) and a remote video call (sparse lower-thirds). The first
#      tests whether the model INVENTS text where there is none; the second tests whether
#      it still FINDS text when there is barely any. A corpus with only one of those
#      answers half of Q3.
CANDIDATES = {
    # slide-heavy conference talks — the missing "slide-talk" category
    "slide_talk": [
        "5si4zkAngpA",   # EuroPython — Exploring the CPython JIT (29:42)
        "JvbBFwlqxeI",   # FOSDEM — How (Not) To Containerise Securely (29:26)
    ],
    # talking-head — the missing category, and the on-screen-text hallucination test
    "talking_head": [
        "KL7WBjAuTMg",   # Conversations with Tyler — Dave Baszucki, Roblox (57:56)
        "gRFaow12xo0",   # Access Now — Timnit Gebru & Melissa Chan (30:04)
    ],
    # long screen-recorded tutorial — the public analogue of the internal 50:40 demo
    "screencast": [
        "rAl-9HwD858",   # Jon Gjengset — Crust of Rust: Lifetime Annotations (93:23)
    ],
}


def _api_key() -> str:
    key = os.environ.get("YOUTUBE_API_KEY")
    if not key:
        env = Path(__file__).resolve().parents[2] / ".env"
        if env.exists():
            m = re.search(r"^YOUTUBE_API_KEY=(.+)$", env.read_text(encoding="utf-8"), re.M)
            key = m.group(1).strip() if m else None
    if not key:
        sys.exit("YOUTUBE_API_KEY not set (.env or environment) — see .env.example")
    return key


def _iso8601_seconds(dur: str) -> int:
    m = re.fullmatch(r"P(?:(\d+)D)?T(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?", dur or "")
    if not m:
        return 0
    d, h, mi, s = (int(x) if x else 0 for x in m.groups())
    return ((d * 24 + h) * 60 + mi) * 60 + s


def fetch(video_ids: list[str]) -> dict[str, dict]:
    """videos.list in batches of 50. Returns {id: raw item}."""
    items = {}
    for i in range(0, len(video_ids), 50):
        batch = video_ids[i:i + 50]
        r = requests.get(API, timeout=60, params={
            "part": "snippet,status,contentDetails",
            "id": ",".join(batch),
            "key": _api_key(),
        })
        if r.status_code != 200:
            sys.exit(f"YouTube Data API HTTP {r.status_code}: {r.text[:500]}")
        for it in r.json().get("items", []):
            items[it["id"]] = it
    return items


def verify(candidates: dict[str, list[str]]) -> dict:
    flat = [(cat, vid) for cat, ids in candidates.items() for vid in ids]
    items = fetch([vid for _, vid in flat])

    accepted, rejected = [], []
    for cat, vid in flat:
        it = items.get(vid)
        if not it:
            rejected.append({"video_id": vid, "category": cat,
                             "reason": "not found / private / deleted"})
            continue
        snip, status, cd = it["snippet"], it["status"], it["contentDetails"]
        licence = status.get("license")
        duration_s = _iso8601_seconds(cd.get("duration", ""))
        rec = {
            "video_id": vid,
            "category": cat,
            "url": f"https://www.youtube.com/watch?v={vid}",
            "title": snip.get("title"),
            "channel": snip.get("channelTitle"),
            "published_at": snip.get("publishedAt"),
            "duration_s": duration_s,
            "licence": licence,
            "privacy": status.get("privacyStatus"),
            "embeddable": status.get("embeddable"),
            "attribution": (
                f'"{snip.get("title")}" by {snip.get("channelTitle")} '
                f"(https://www.youtube.com/watch?v={vid}), CC BY 3.0"
            ),
        }
        # Every one of these is load-bearing: a non-CC licence means we may not republish;
        # a non-public video means Vertex cannot fetch it (it accepts public URLs only);
        # non-embeddable means the gallery cannot show the original next to our Markdown,
        # which is exactly what makes the attribution honest.
        problems = []
        if licence != "creativeCommon":
            problems.append(f"licence={licence!r} (need 'creativeCommon')")
        if status.get("privacyStatus") != "public":
            problems.append(f"privacy={status.get('privacyStatus')!r}")
        if not status.get("embeddable"):
            problems.append("not embeddable")
        if duration_s == 0:
            problems.append("zero/unknown duration (live?)")
        if problems:
            rejected.append({**rec, "reason": "; ".join(problems)})
        else:
            accepted.append(rec)

    return {"accepted": accepted, "rejected": rejected}


def _mmss(s: int) -> str:
    return f"{s // 60}:{s % 60:02d}"


if __name__ == "__main__":
    if len(sys.argv) > 1:
        cand = {"adhoc": sys.argv[1:]}
    else:
        cand = CANDIDATES
    result = verify(cand)

    print("=== ACCEPTED (status.license == creativeCommon) ===")
    for r in result["accepted"]:
        print(f"  [{r['category']:>12}] {r['video_id']}  {_mmss(r['duration_s']):>6}  "
              f"{r['channel']} — {r['title'][:60]}")
    print("\n=== REJECTED ===")
    for r in result["rejected"]:
        print(f"  [{r.get('category', '?'):>12}] {r['video_id']}  {r['reason']}"
              f"  {r.get('title', '')[:50]}")

    n_ok = len(result['accepted'])
    print(f"\n{n_ok} accepted / {n_ok + len(result['rejected'])} candidates")
    if len(sys.argv) == 1:
        (OUT / "corpus.json").write_text(
            json.dumps(result, indent=2, ensure_ascii=False), encoding="utf-8")
        print(f"wrote {OUT / 'corpus.json'}")
