#!/usr/bin/env python3
"""Per-session quality gate for a produced collection: the 3-timestamp spot-verify.

The method is the one Phase 0.2 used to grade METRICS.md N30-N32, and the reason it is trusted is
that it is an INDEPENDENT RE-PROBE. A second, differently-prompted call seeks to the block's own
claimed timestamp and describes what is there, with no sight of what the document claims. The
document is then graded against a fresh look at the video rather than against itself.

Grading a model's output with a model is a real weakness, so two things keep it honest:

  1. The prober never sees the claim. It cannot agree by suggestion.
  2. Sections are chosen deterministically but WITHOUT regard to how interesting they look. This is
     the same discipline as DECISIONS.md D11: checking only the impressive parts selects for
     exactly the claims most likely to be wrong, and never yields a base error rate.

Usage:
  python spot_verify.py --root <dir with <videoId>/output.md> [--collection ai-engineering]
                        [--samples 3] [--only VIDEOID,...]
"""
from __future__ import annotations

import argparse
import hashlib
import json
import os
import re
import subprocess
import sys
import time
import urllib.error
import urllib.request
from pathlib import Path

PROJECT = os.environ.get("GCP_PROJECT_ID", "tensile-runway-442915-j6")
REGIONS = ["europe-central2", "europe-west3"]  # EU only, CLAUDE.md rule 2
MODEL = "gemini-2.5-flash"

# A block is anchored if at least this many of its sampled sections check out. Two of three is a
# deliberate choice: one miss on a segment boundary is normal, two is a pattern.
PASS_THRESHOLD = 2

_token = {"value": None, "at": 0.0}


def token() -> str:
    if _token["value"] and time.time() - _token["at"] < 40 * 60:
        return _token["value"]
    out = subprocess.run(
        "gcloud auth application-default print-access-token",
        shell=True, capture_output=True, text=True, check=True,
    )
    lines = [l.strip() for l in out.stdout.splitlines() if l.strip() and "WARNING" not in l]
    _token.update(value=lines[-1], at=time.time())
    return _token["value"]


def call_vertex(parts: list[dict], max_tokens: int = 1200) -> dict:
    """One bounded Vertex call, EU regions only, with the rule-9 guards on every call."""
    body = {
        "contents": [{"role": "user", "parts": parts}],
        "generationConfig": {
            "temperature": 0.0,
            "maxOutputTokens": max_tokens,
            "thinkingConfig": {"thinkingBudget": 512},
            "responseMimeType": "application/json",
            "mediaResolution": "MEDIA_RESOLUTION_LOW",
        },
    }
    last = None
    for region in REGIONS:
        url = (f"https://{region}-aiplatform.googleapis.com/v1/projects/{PROJECT}"
               f"/locations/{region}/publishers/google/models/{MODEL}:generateContent")
        req = urllib.request.Request(
            url, data=json.dumps(body).encode(),
            headers={"Authorization": f"Bearer {token()}", "Content-Type": "application/json"},
        )
        try:
            with urllib.request.urlopen(req, timeout=300) as r:
                payload = json.load(r)
            text = "".join(
                p.get("text", "")
                for p in payload["candidates"][0]["content"]["parts"]
            )
            return {"ok": True, "data": json.loads(text), "region": region,
                    "usage": payload.get("usageMetadata", {})}
        except urllib.error.HTTPError as e:
            last = f"{e.code} {e.read()[:200]!r}"
            if e.code != 429:
                break
            time.sleep(20)
        except Exception as e:  # noqa: BLE001 - a probe failure must not kill the gate
            last = str(e)
            break
    return {"ok": False, "error": last}


SECTION_RE = re.compile(r"^## (\d{2}:\d{2}:\d{2}) (.+)$", re.M)


def parse_document(md: str) -> dict:
    fm = {}
    if md.startswith("---"):
        head = md.split("---", 2)[1]
        for line in head.splitlines():
            if ":" in line:
                k, v = line.split(":", 1)
                fm[k.strip()] = v.strip().strip('"')

    sections = []
    matches = list(SECTION_RE.finditer(md))
    for i, m in enumerate(matches):
        body = md[m.end(): matches[i + 1].start() if i + 1 < len(matches) else len(md)]
        sections.append({
            "timestamp": m.group(1),
            "heading": m.group(2).strip(),
            "spoken": _field(body, "Spoken"),
            "on_screen": _field(body, "On screen"),
            "visual": _field(body, "Visual"),
        })
    return {"frontmatter": fm, "sections": sections}


def _field(body: str, label: str) -> str:
    m = re.search(rf"\*\*{label}:\*\*(.*?)(?=\n\*\*|\Z)", body, re.S)
    if not m:
        return ""
    text = m.group(1).strip()
    return re.sub(r"^> ?", "", text, flags=re.M).strip()


def to_seconds(ts: str) -> int:
    h, m, s = (int(x) for x in ts.split(":"))
    return h * 3600 + m * 60 + s


def pick_sections(video_id: str, sections: list[dict], n: int) -> list[dict]:
    """Deterministic, content-blind sampling.

    Seeded by video id so a re-run checks the same sections, and chosen by position rather than by
    how interesting the text looks — surprising and fabricated correlate (DECISIONS.md D11), so
    picking the eye-catching sections would measure the opposite of what we want to know.
    """
    candidates = sections[1:] if len(sections) > n else sections
    if len(candidates) <= n:
        return candidates
    seed = int(hashlib.sha256(video_id.encode()).hexdigest()[:8], 16)
    step = len(candidates) / n
    return [candidates[int((seed % 97) / 97 * step + i * step) % len(candidates)] for i in range(n)]


PROBE_PROMPT = (
    "Watch this short clip and report only what you directly observe. Do not guess, do not "
    "summarize the wider talk, and do not infer content from the slide title. Reply as JSON: "
    '{"spoken_gist": "<one or two sentences of what is said>", '
    '"on_screen_text": "<verbatim text visible on screen, or empty string if none>", '
    '"visual": "<what is visible>"}'
)


def probe(source_url: str, start_s: int, window_s: int = 45) -> dict:
    return call_vertex([
        {"fileData": {"mimeType": "video/mp4", "fileUri": source_url},
         "videoMetadata": {"startOffset": f"{start_s}s", "endOffset": f"{start_s + window_s}s"}},
        {"text": PROBE_PROMPT},
    ])


JUDGE_PROMPT = """You are checking whether a documented claim about a video clip is supported by an independent observation of that same clip.

DOCUMENTED CLAIM (from the artifact under test):
{claim}

INDEPENDENT OBSERVATION (a separate viewing of the same clip, made without seeing the claim):
{observation}

Decide strictly. The claim does NOT need to match word for word - it needs to be ABOUT THE SAME MOMENT and not assert things the observation contradicts.

Reply as JSON:
{{"anchored": true|false,
  "on_screen_supported": "yes"|"no"|"not_claimed",
  "contradiction": "<the specific contradiction, or empty string>",
  "confidence": "high"|"low"}}

- "anchored" is false if the claim describes a clearly different moment or topic than the observation.
- "on_screen_supported" is "no" only if the claim quotes on-screen text that the observation says was not there. If the claim quoted no on-screen text, answer "not_claimed".
"""


def judge(section: dict, observation: dict) -> dict:
    claim = json.dumps({
        "heading": section["heading"],
        "spoken": section["spoken"][:1500],
        "on_screen_text": section["on_screen"][:1000],
        "visual": section["visual"][:800],
    }, ensure_ascii=False)
    return call_vertex([{"text": JUDGE_PROMPT.format(
        claim=claim, observation=json.dumps(observation, ensure_ascii=False))}], max_tokens=600)


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--root", required=True)
    ap.add_argument("--collection", default="ai-engineering")
    ap.add_argument("--samples", type=int, default=3)
    ap.add_argument("--only", default="")
    ap.add_argument("--out", default="")
    args = ap.parse_args()

    root = Path(args.root)
    only = {v for v in args.only.split(",") if v}
    docs = sorted(p for p in root.glob("*/output.md") if not only or p.parent.name in only)
    if not docs:
        print(f"no output.md under {root}", file=sys.stderr)
        return 1

    results = []
    for path in docs:
        video_id = path.parent.name
        doc = parse_document(path.read_text(encoding="utf-8"))
        source = doc["frontmatter"].get("source_filename", "")
        sections = doc["sections"]
        if not sections or not source:
            results.append({"video_id": video_id, "verdict": "unparseable"})
            print(f"{video_id}: UNPARSEABLE", flush=True)
            continue

        checks = []
        for section in pick_sections(video_id, sections, args.samples):
            start = to_seconds(section["timestamp"])
            obs = probe(source, start)
            if not obs["ok"]:
                checks.append({"timestamp": section["timestamp"], "error": obs["error"]})
                continue
            verdict = judge(section, obs["data"])
            if not verdict["ok"]:
                checks.append({"timestamp": section["timestamp"], "error": verdict["error"]})
                continue
            checks.append({
                "timestamp": section["timestamp"],
                "heading": section["heading"],
                "observation": obs["data"],
                **verdict["data"],
            })

        graded = [c for c in checks if "anchored" in c]
        anchored = sum(1 for c in graded if c.get("anchored"))
        on_screen_bad = sum(1 for c in graded if c.get("on_screen_supported") == "no")
        verdict = (
            "error" if not graded
            else "pass" if anchored >= min(PASS_THRESHOLD, len(graded)) and on_screen_bad == 0
            else "fail"
        )
        results.append({
            "video_id": video_id,
            "title": doc["frontmatter"].get("title", ""),
            "sections": len(sections),
            "sampled": len(checks),
            "graded": len(graded),
            "anchored": anchored,
            "on_screen_contradicted": on_screen_bad,
            "verdict": verdict,
            "checks": checks,
        })
        print(f"{video_id}: {verdict.upper()} ({anchored}/{len(graded)} anchored, "
              f"{on_screen_bad} on-screen contradictions)", flush=True)

    graded_sessions = [r for r in results if r.get("verdict") in {"pass", "fail"}]
    summary = {
        "collection": args.collection,
        "sessions": len(results),
        "graded": len(graded_sessions),
        "passed": sum(1 for r in graded_sessions if r["verdict"] == "pass"),
        "failed": sum(1 for r in graded_sessions if r["verdict"] == "fail"),
        "errored": sum(1 for r in results if r.get("verdict") in {"error", "unparseable"}),
        "total_checks": sum(len(r.get("checks", [])) for r in results),
        "anchored_checks": sum(r.get("anchored", 0) for r in results),
        "results": results,
    }
    out = Path(args.out) if args.out else root.parent / f"spot-verify-{args.collection}.json"
    out.write_text(json.dumps(summary, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"\n{summary['passed']}/{summary['graded']} sessions pass the gate; "
          f"{summary['anchored_checks']}/{summary['total_checks']} individual checks anchored.")
    print(f"report: {out}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
