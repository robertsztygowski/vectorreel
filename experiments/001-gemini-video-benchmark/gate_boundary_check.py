"""Phase 3 boundary gate (PLAN.md Phase 3, first job).

Question: does Stage B OBEY the block boundaries Stage A emits, when they are put in the prompt?
Phase 1 proved the boundaries are emitted (N7c); only a Vertex call proves they are obeyed.

Runs two real calls on the N7b failing window (seg2_720p.mp4, the 12.8-min man-talking-over-a-
frozen-IDE clip that returned <=3 blocks in 4/4 unguided runs):
  A) BASELINE  — the shipped benchmark prompt, no boundaries (reproduces the N7b bug).
  B) GUIDED    — the same prompt + Stage A's real forced boundaries as mandatory block starts.

Verdict: for each forced boundary, is there a model block within +/-TOL seconds? Report adherence
and total block counts for both. If GUIDED obeys, the design holds; if it ignores them, the
fallback is to cut real segments at the boundaries (more calls).
"""

import json
from pathlib import Path

from common import (OUT, STAGE_B_SCHEMA, VIDEO_TOKENS_PER_S, gemini_call, hhmmss,
                    normalize_times, response_text, spike_budget)

GCS_URI = "gs://tensile-runway-442915-j6-vectorreel-dev/gate/seg2_720p.mp4"
TOL_S = 6  # a model block this close to a forced boundary counts as obeying it
BOUNDARIES_FILE = OUT / "gate_seg2_boundaries.json"

BASE_RULES = """\
You are analyzing one segment of a longer video for a searchable knowledge base.
This segment starts at 00:00:00 in the full video.

Break the segment into blocks: start a new block whenever the content meaningfully changes
(new slide, new screen, new topic, new demonstrated action). Typical block length 20-90 s.

Rules:
- "t": timestamp of the block start WITHIN THIS CLIP, as hh:mm:ss with two-digit fields,
  e.g. "00:03:45" (00:00:00 = clip start). For clips under one hour the hours field stays "00".
- "spoken": cleaned transcript of what is actually said during the block. Empty string if silent.
- "speaker": name if stated; otherwise "Speaker 1", "Speaker 2", ... consistently; null if silent.
- "on_screen_text": important on-screen text VERBATIM. Empty string if none.
- "visual": one or two sentences describing what is shown.
- "kind": one of slide | demo | talking_head | screen_share | whiteboard | other.
- "segment_start": echo "00:00:00".
- "segment_summary": 2-4 sentences summarizing this segment.
- COVERAGE IS MANDATORY: the blocks together must cover the ENTIRE clip with no gaps.
"""

GUIDANCE = """\

MANDATORY BLOCK BOUNDARIES: a pre-analysis of the narration has already located the points where
the topic changes. You MUST start a new block at each of these timestamps (you may add finer
blocks between them, but you may NOT merge across them):
{boundary_list}
"""


def gen_config():
    return {
        "temperature": 0.2,
        "maxOutputTokens": 12000,
        "thinkingConfig": {"thinkingBudget": 4096},
        "mediaResolution": "MEDIA_RESOLUTION_LOW",
        "responseMimeType": "application/json",
        "responseSchema": STAGE_B_SCHEMA,
    }


def build_body(prompt: str) -> dict:
    return {
        "contents": [{
            "role": "user",
            "parts": [
                {"fileData": {"mimeType": "video/mp4", "fileUri": GCS_URI}},
                {"text": prompt},
            ],
        }],
        "generationConfig": gen_config(),
    }


def block_seconds(parsed: dict, clip_dur_s: int) -> list[int]:
    return normalize_times([b["t"] for b in parsed.get("blocks", [])], clip_dur_s)


def adherence(block_s: list[int], boundaries: list[int]) -> tuple[int, list[int]]:
    hit = [b for b in boundaries if any(abs(t - b) <= TOL_S for t in block_s)]
    return len(hit), [b for b in boundaries if b not in hit]


def run(label: str, prompt: str, clip_dur_s: int, boundaries: list[int]) -> dict:
    data = gemini_call(build_body(prompt), label=label, config="gate")
    text = response_text(data)
    (OUT / f"{label}.raw.txt").write_text(text, encoding="utf-8")
    finish = data["candidates"][0].get("finishReason", "?")
    parsed = json.loads(text)
    bs = block_seconds(parsed, clip_dur_s)
    hits, missed = adherence(bs, boundaries)
    print(f"\n[{label}] finish={finish} blocks={len(bs)} "
          f"boundary_adherence={hits}/{len(boundaries)}")
    print(f"  block starts (s): {bs}")
    if missed:
        print(f"  missed boundaries (s): {missed}")
    return {"label": label, "finish": finish, "n_blocks": len(bs),
            "block_starts_s": bs, "adherence": hits, "n_boundaries": len(boundaries),
            "missed_s": missed}


def main():
    spike_budget("phase3_gate", cap_usd=0.75)
    meta = json.loads(BOUNDARIES_FILE.read_text(encoding="utf-8"))
    clip_dur_s = round(meta["duration_s"])
    boundaries = [int(round(c)) for c in meta["timeline_cues_s"]]
    print(f"clip={clip_dur_s}s  forced boundaries ({len(boundaries)}): {boundaries}")

    boundary_list = "\n".join(f"  - {hhmmss(b)}" for b in boundaries)
    guided_prompt = BASE_RULES + GUIDANCE.format(boundary_list=boundary_list)

    results = {
        "baseline": run("gate_baseline", BASE_RULES, clip_dur_s, boundaries),
        "guided": run("gate_guided", guided_prompt, clip_dur_s, boundaries),
    }
    (OUT / "gate_verdict.json").write_text(
        json.dumps(results, indent=2, ensure_ascii=False), encoding="utf-8")

    b, g = results["baseline"], results["guided"]
    print("\n=== GATE VERDICT ===")
    print(f"baseline: {b['n_blocks']} blocks, {b['adherence']}/{b['n_boundaries']} boundaries hit")
    print(f"guided:   {g['n_blocks']} blocks, {g['adherence']}/{g['n_boundaries']} boundaries hit")
    ratio = g["adherence"] / g["n_boundaries"] if g["n_boundaries"] else 0
    if ratio >= 0.75 and g["n_blocks"] >= 0.75 * g["n_boundaries"]:
        print(f"PASS: Stage B OBEYS Stage A boundaries ({ratio:.0%} adherence). Design holds.")
    else:
        print(f"FAIL: guided adherence only {ratio:.0%}. Fallback = cut real segments at boundaries.")


if __name__ == "__main__":
    main()
