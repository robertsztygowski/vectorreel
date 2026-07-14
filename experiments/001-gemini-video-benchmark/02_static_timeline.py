"""Q4: static-content lever size. Frame-difference timeline of the long demo video.

1 fps, 160x90 grayscale frames -> mean abs diff between consecutive frames -> timeline.
A second is 'static' when the diff to the previous sampled frame is below THRESHOLD;
the lever is the fraction of the video inside static runs >= MIN_RUN_S (short blips
aren't worth switching sampling config for).
"""

import json
import subprocess
import sys
from pathlib import Path

import numpy as np

from common import OUT, WORK, hhmmss

W, H = 160, 90
THRESHOLD = 2.0   # mean abs pixel diff (0-255); report sensitivity around it
MIN_RUN_S = 10

VIDEO = Path.home() / "Downloads" / "Isolation Component V2 demo.mp4"
if len(sys.argv) > 1:
    VIDEO = Path(sys.argv[1])


def extract_frames() -> np.ndarray:
    raw = WORK / (VIDEO.stem + ".gray.raw")
    if not raw.exists():
        print(f"extracting 1fps {W}x{H} gray frames from {VIDEO.name} ...")
        subprocess.run([
            "ffmpeg", "-y", "-loglevel", "error", "-i", str(VIDEO),
            "-vf", f"fps=1,scale={W}:{H}", "-pix_fmt", "gray",
            "-f", "rawvideo", str(raw)], check=True)
    data = np.fromfile(raw, dtype=np.uint8)
    return data.reshape(-1, H, W)


def static_runs(static: np.ndarray, min_run: int) -> list[tuple[int, int]]:
    runs, start = [], None
    for i, s in enumerate(static):
        if s and start is None:
            start = i
        elif not s and start is not None:
            if i - start >= min_run:
                runs.append((start, i))
            start = None
    if start is not None and len(static) - start >= min_run:
        runs.append((start, len(static)))
    return runs


def main():
    frames = extract_frames().astype(np.int16)
    diffs = np.abs(np.diff(frames, axis=0)).mean(axis=(1, 2))  # diff[i] = frame i+1 vs i
    total = len(diffs)
    print(f"{total + 1} frames sampled; diff percentiles: "
          + ", ".join(f"p{p}={np.percentile(diffs, p):.2f}" for p in (10, 25, 50, 75, 90, 99)))

    for th in (1.0, 2.0, 3.0, 5.0):
        static = diffs < th
        runs = static_runs(static, MIN_RUN_S)
        lever = sum(e - s for s, e in runs) / total
        print(f"threshold {th}: {static.mean():.1%} static seconds, "
              f"{lever:.1%} in runs >= {MIN_RUN_S}s ({len(runs)} runs)")

    static = diffs < THRESHOLD
    runs = static_runs(static, MIN_RUN_S)
    lever = sum(e - s for s, e in runs) / total
    out = {
        "video": VIDEO.name,
        "sampled_seconds": int(total),
        "threshold": THRESHOLD,
        "min_run_s": MIN_RUN_S,
        "static_fraction_raw": round(float(static.mean()), 4),
        "static_fraction_in_runs": round(float(lever), 4),
        "runs": [{"start": hhmmss(s), "end": hhmmss(e), "dur_s": e - s} for s, e in runs],
    }
    path = OUT / f"static_timeline_{VIDEO.stem[:20].replace(' ', '_')}.json"
    path.write_text(json.dumps(out, indent=2), encoding="utf-8")
    print(f"\nQ4 verdict: {lever:.1%} of {VIDEO.name} sits in static runs >= {MIN_RUN_S}s "
          f"(threshold {THRESHOLD}). Written to {path.name}")


if __name__ == "__main__":
    main()
