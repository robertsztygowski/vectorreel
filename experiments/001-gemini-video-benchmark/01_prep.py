"""Prep: cut segments (720p + 1080p), upload everything to the dev GCS bucket."""

import subprocess
from pathlib import Path

from common import BUCKET, WORK

DOWNLOADS = Path.home() / "Downloads"
LONG = DOWNLOADS / "Isolation Component V2 demo.mp4"      # 3039.55 s, 1920x1080 ~16fps
HABICEN = DOWNLOADS / "HabiCen Ticket System.mp4"          # 111.85 s, silent content

# ~770 s segments with 20 s overlap (PLAN.md Phase 0 step 1)
SEGMENTS = [(0, 770), (750, 1520), (1500, 2270), (2250, 3040)]


def run(cmd, **kw):
    print("+", cmd if isinstance(cmd, str) else " ".join(str(c) for c in cmd))
    subprocess.run(cmd, check=True, **kw)


def cut(src: Path, start: int, end: int, out: Path, height: int | None):
    if out.exists():
        print(f"  {out.name} exists, skipping")
        return
    vf = ["-vf", f"scale=-2:{height}"] if height else []
    crf = "28" if height else "22"
    run(["ffmpeg", "-y", "-loglevel", "error", "-ss", str(start), "-i", str(src),
         "-t", str(end - start), *vf, "-c:v", "libx264", "-crf", crf,
         "-preset", "veryfast", "-c:a", "copy", str(out)])


def main():
    for i, (s, e) in enumerate(SEGMENTS, 1):
        cut(LONG, s, e, WORK / f"seg{i}_720p.mp4", height=720)
    for i, (s, e) in enumerate(SEGMENTS[:2], 1):
        cut(LONG, s, e, WORK / f"seg{i}_1080p.mp4", height=None)

    for f in sorted(WORK.glob("seg*.mp4")):
        print(f"  {f.name}: {f.stat().st_size / 1e6:.1f} MB")

    run(f'gcloud storage cp "{WORK}\\seg*.mp4" gs://{BUCKET}/segments/', shell=True)
    run(f'gcloud storage cp "{HABICEN}" gs://{BUCKET}/habicen.mp4', shell=True)
    run(f"gcloud storage ls -l gs://{BUCKET}/**", shell=True)


if __name__ == "__main__":
    main()
