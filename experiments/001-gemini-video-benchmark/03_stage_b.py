"""Stage B runner: one segment -> structured JSON (ARCHITECTURE.md §3 schema).

Saves out/<label>.json (raw model output) and out/<label>.norm.json (block timestamps
converted from clip-relative to global video time). Usable as a module (run_stage_b)
or CLI:  python 03_stage_b.py gs://bucket/seg.mp4 <segment_start_sec> <label> [fps]
"""

import json
import sys

from common import (OUT, build_stage_b_body, fetched_duration_s, gemini_call, hhmmss,
                    normalize_times, response_text)


def _clip_times(parsed: dict, segment_start_s: int, clip_dur_s: int | None) -> list[int]:
    """Clip-relative block seconds. The model sometimes anchors block times at the
    segment_start given in the prompt (absolute in the full video) — detect and shift."""
    times = normalize_times([b["t"] for b in parsed.get("blocks", [])], None)
    if times and segment_start_s > 0 and abs(times[0] - segment_start_s) <= 30:
        times = [t - segment_start_s for t in times]
    else:
        times = normalize_times([b["t"] for b in parsed.get("blocks", [])], clip_dur_s)
    return times


def run_stage_b(gcs_uri: str, segment_start_s: int, label: str, config: str,
                fps: float | None = None, media_resolution: str | None = None,
                clip_dur_s: int | None = None,
                youtube_window: tuple[int, int] | None = None,
                derive_clip_dur: bool = False,
                _attempt: int = 1) -> dict:
    """gcs_uri is a gs:// segment, or a YouTube watch URL when youtube_window is set —
    on the public path there is no ffmpeg, so the clip is cut server-side by offsets.

    derive_clip_dur: read the clip's real length back off the bill instead of trusting the
    requested window. Mandatory on the YouTube path, where Vertex clamps the window to the
    end of the video and a short video would otherwise fail the coverage guard and be
    retried at full price (Phase 0.1, out/YOUTUBE.md pathology 1)."""
    seg_start = hhmmss(segment_start_s)
    start_off, end_off = youtube_window or (None, None)
    body = build_stage_b_body(gcs_uri, seg_start, fps=fps,
                              media_resolution=media_resolution,
                              start_offset_s=start_off, end_offset_s=end_off)
    print(f"{label}: {gcs_uri} (start {seg_start}, fps={fps or 'default'}, "
          f"res={media_resolution or 'default'}, attempt {_attempt})")
    data = gemini_call(body, label=label, config=config)
    text = response_text(data)
    # per-attempt, because a retry used to overwrite the evidence of the failure that
    # caused it — and the failed response is the one you actually want to read
    (OUT / f"{label}.raw.a{_attempt}.txt").write_text(text, encoding="utf-8")

    if derive_clip_dur:
        real = fetched_duration_s(data.get("usageMetadata", {}), media_resolution)
        if clip_dur_s and real < clip_dur_s * 0.98:
            print(f"  fetched {real:.0f}s of the requested {clip_dur_s}s window "
                  f"— video ends early; coverage measured against {real:.0f}s")
        clip_dur_s = round(real)

    finish = data["candidates"][0].get("finishReason", "?")
    covered = None
    try:
        parsed = json.loads(text)
        if clip_dur_s and parsed.get("blocks"):
            covered = _clip_times(parsed, segment_start_s, clip_dur_s)[-1] / clip_dur_s
    except json.JSONDecodeError as e:
        parsed = None
        print(f"  finishReason={finish}, JSON parse failed: {e}")
    # Generation guard: truncated JSON, or a clip barely covered, or timestamps that run
    # PAST THE END of the clip. The upper bound is not symmetry for its own sake: Phase 0.2
    # accepted a 15-minute segment whose blocks ran to 04:48:10 (coverage 1921%) because the
    # guard only ever looked down. A citation to a timestamp that does not exist is worse
    # than no citation — it is the exact failure A4 is meant to catch.
    bad = parsed is None or (clip_dur_s and clip_dur_s > 120
                             and (len(parsed["blocks"]) < 3
                                  or covered < 0.6 or covered > 1.05))
    if bad:
        # A MAX_TOKENS overflow is deterministic: the same clip at the same config will
        # overflow again. Phase 0.2 paid twice for exactly that on both slide talks
        # (~EUR 0.70 for zero output). Retrying is only rational for a transient failure;
        # this one needs a SMALLER WINDOW, which is the caller's decision, not ours.
        if finish == "MAX_TOKENS":
            raise RuntimeError(
                f"{label}: output hit maxOutputTokens ({clip_dur_s}s clip) — not retried, "
                f"the retry would overflow identically. Segment the clip and re-run.")
        if _attempt >= 2:
            raise RuntimeError(f"{label}: bad output twice (finish={finish}, "
                               f"coverage={covered}), giving up")
        print(f"  {label}: under-generated (finish={finish}, blocks="
              f"{len(parsed['blocks']) if parsed else '-'}, coverage={covered}), retrying")
        return run_stage_b(gcs_uri, segment_start_s, label, config, fps=fps,
                           media_resolution=media_resolution, clip_dur_s=clip_dur_s,
                           youtube_window=youtube_window, derive_clip_dur=derive_clip_dur,
                           _attempt=_attempt + 1)
    (OUT / f"{label}.json").write_text(
        json.dumps(parsed, indent=2, ensure_ascii=False), encoding="utf-8")

    norm = dict(parsed)
    times = _clip_times(parsed, segment_start_s, clip_dur_s)
    norm["blocks"] = [
        {**b, "t": hhmmss(segment_start_s + s)}
        for b, s in zip(parsed.get("blocks", []), times)
    ]
    (OUT / f"{label}.norm.json").write_text(
        json.dumps(norm, indent=2, ensure_ascii=False), encoding="utf-8")

    n_spoken = sum(1 for b in parsed["blocks"] if (b.get("spoken") or "").strip())
    print(f"  {len(parsed['blocks'])} blocks ({n_spoken} with speech), "
          f"language={parsed.get('language')}, coverage="
          f"{f'{covered:.0%}' if covered is not None else 'n/a'}")
    return parsed


if __name__ == "__main__":
    uri, start, label = sys.argv[1], int(sys.argv[2]), sys.argv[3]
    fps = float(sys.argv[4]) if len(sys.argv) > 4 else None
    run_stage_b(uri, start, label, config="adhoc", fps=fps)
