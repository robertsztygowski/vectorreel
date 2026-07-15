#!/usr/bin/env bash
# fetch-calibration.sh — pull the private calibration recording into a stable local dir.
#
# WHY THIS EXISTS
#   Stage A's most important tests grade it against the ICP's own content — the 50-minute internal
#   demo, and the 12.8-minute window (seg2) that reproduced the under-segmentation failure (METRICS.md
#   N7b). That footage is company-confidential and ~293 MB, so it is NOT in the repo. Those tests skip
#   when it is absent (see tests/Integration/CalibrationFixtures.cs), which keeps a fresh clone green —
#   but a skip is a test that did not run. This script makes them runnable.
#
#   The N4 static-lever fidelity gate does NOT need this: it runs from a committed ~38 KB derived
#   fixture (tests/Unit/StageA/N4FidelityTests.cs). Only the *cue* acceptance tests need the video.
#
# WHAT IT DOES
#   1. Downloads the master demo from the EU dev bucket (ADC — no key files; CLAUDE.md rule 1).
#   2. Regenerates seg2 locally from the demo — seg2 is just a window of it, so it is never stored.
#   Both land in .calibration/ (gitignored), where the tests look for them automatically.
#
# PREREQ: gcloud auth application-default login   (EU dev project; ffmpeg on PATH)

set -euo pipefail
cd "$(dirname "$0")/.."

BUCKET="gs://tensile-runway-442915-j6-vectorreel-dev"
DIR=".calibration"
DEMO="$DIR/isolation-demo.mp4"
SEG2="$DIR/seg2_720p.mp4"

mkdir -p "$DIR"

if [[ -f "$DEMO" ]]; then
  echo "✓ demo already present ($DEMO)"
else
  echo "↓ fetching demo from $BUCKET/calibration/ ..."
  gcloud storage cp "$BUCKET/calibration/isolation-demo.mp4" "$DEMO"
fi

# seg2 = SEGMENTS[1] from experiments/001-*/01_prep.py: a 770 s window at offset 750 s, scaled to 720p.
# Regenerating it from the master is exact enough — the cue-density assertions are robust to reencoding
# (a threshold sweep moved the result only a few percent), and it means seg2 need never be stored.
if [[ -f "$SEG2" ]]; then
  echo "✓ seg2 already present ($SEG2)"
else
  echo "✂ cutting seg2 from the demo (offset 750 s, 770 s, 720p) ..."
  ffmpeg -y -loglevel error -ss 750 -i "$DEMO" -t 770 \
    -vf "scale=-2:720" -c:v libx264 -crf 28 -preset veryfast -c:a copy "$SEG2"
fi

echo
echo "✓ calibration assets ready in $DIR/ — the calibration tests will now run instead of skip:"
echo "    dotnet test tests/Integration --filter Category!=Live"
