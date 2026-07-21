#!/usr/bin/env bash
# Produce a collection through the REAL pipeline, from a licence-verified source list.
#
# This is the only command that spends inference money on purpose. It is deliberately
# calibrate-first: run it with --calibrate 2, reconcile the measured rate against METRICS.md N4c,
# then run it again without the flag to produce the rest. Re-running is safe and free for anything
# already produced — the batch skips sources whose output.md is already in the bucket.
#
#   scripts/produce-collection.sh ai-engineering --calibrate 2
#   scripts/produce-collection.sh ai-engineering
#   scripts/produce-collection.sh ai-engineering --only VIDEOID1,VIDEOID2
#   scripts/produce-collection.sh ai-engineering --dry-run     # no Vertex calls, proves the wiring
#
set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$REPO_ROOT"

COLLECTION="${1:-}"
if [[ -z "$COLLECTION" ]]; then
  echo "usage: scripts/produce-collection.sh <collection-slug> [--calibrate N] [--only IDS] [--dry-run]" >&2
  exit 2
fi
shift

CORPUS="experiments/006-collections-strategy/out/corpus.json"
REPORT_DIR="experiments/006-collections-strategy/out"
CALIBRATE=""
ONLY=""
MODE="live"

while [[ $# -gt 0 ]]; do
  case "$1" in
    --calibrate) CALIBRATE="$2"; shift 2 ;;
    --only)      ONLY="$2"; shift 2 ;;
    --corpus)    CORPUS="$2"; shift 2 ;;
    --dry-run)   MODE="fake"; shift ;;
    *) echo "unknown flag: $1" >&2; exit 2 ;;
  esac
done

if [[ ! -f "$REPO_ROOT/$CORPUS" ]]; then
  echo "corpus not found: $CORPUS" >&2
  exit 1
fi

REPORT="$REPORT_DIR/batch-report-${COLLECTION}$([[ -n "$CALIBRATE" ]] && echo "-calibration" || echo "").json"

# `dotnet run --project` sets the working directory to the project, not the repo root, so both
# paths have to survive the move. On Windows the paths also have to leave Git Bash as Windows
# paths or .NET will not find them.
to_native() { command -v cygpath >/dev/null 2>&1 && cygpath -w "$1" || printf '%s' "$1"; }
CORPUS="$(to_native "$REPO_ROOT/$CORPUS")"
REPORT="$(to_native "$REPO_ROOT/$REPORT")"

export PipelineModel__Mode="$MODE"
export Gcs__Project="${GCP_PROJECT_ID:-tensile-runway-442915-j6}"

# Where produced documents land. Local by default, and that is deliberate: M4 builds a git
# repository out of these files, so a round-trip through GCS buys nothing, and this is public
# CC-BY material — the EU-residency reason `outputs-eu` exists is customer footage, which never
# touches this path. Resumability reads back through the same seam either way.
# Set STORAGE_MODE=gcs to write to the bucket instead.
if [[ "${STORAGE_MODE:-local}" == "gcs" ]]; then
  export Storage__Mode=gcs
else
  export Storage__LocalRoot="$(to_native "$REPO_ROOT/.local-state/collections")"
fi
# Config section is `Vertex` (VertexOptions.SectionName) — not `VertexOptions`. Getting this wrong
# is silent: binding simply finds nothing and the defaults apply.
export Vertex__Project="${GCP_PROJECT_ID:-tensile-runway-442915-j6}"
export Vertex__Region="${VERTEX_LOCATION:-europe-central2}"
export Vertex__FallbackRegion="${VERTEX_FALLBACK_REGION:-europe-west3}"   # EU only, rule 2

export CollectionProduction__Enabled=true
export CollectionProduction__Collection="$COLLECTION"
export CollectionProduction__CorpusPath="$CORPUS"
export CollectionProduction__ReportPath="$REPORT"
export CollectionProduction__OutputBucket="${OUTPUT_BUCKET:-outputs-eu}"
export CollectionProduction__OutputPrefix="collections"
export CollectionProduction__LowMediaResolution=true          # METRICS.md N4b
export CollectionProduction__SegmentLength="00:10:00"
export CollectionProduction__SegmentOverlap="00:00:20"
# Rule 9: no Stage B call is ever unguarded. The bound stays; only its size is calibrated.
#
# 🚩 Measured 2026-07-21: a real 10-minute YouTube segment takes 24–70 s, because Google fetches
# the video itself on the fileUri path. The 90 s default (StageBCallOptions.Default, sized for the
# private path) leaves no room for the 429 → europe-west3 fallback chain, so the guard fired mid-
# fallback and every segment failed. Timed-out calls are pure waste. 300 s covers two calls plus
# backoff and still bounds a runaway.
export CollectionProduction__StageB__MaxOutputTokens=12000
export CollectionProduction__StageB__ThinkingBudget=4096
export CollectionProduction__StageB__TimeoutSeconds="${STAGE_B_TIMEOUT_SECONDS:-300}"
# The N4d guard. Not a price ceiling — the signature of naive over-segmentation. Breaching it
# aborts the batch rather than paying for overflow retries across every remaining video.
export CollectionProduction__AbortOverCentsPerVideoHour="${ABORT_OVER_CENTS_PER_VIDEO_HOUR:-200}"

# 🚩 Gemini 2.5 on Vertex uses Dynamic Shared Quota: a 429 means the EU region is busy right now,
# shared across every customer — NOT that our allowance is spent. Measured 2026-07-21: we run at
# ~1% of our own (non-adjustable) limit, and the success rate is a flat ~25% at every concurrency
# level tried. So we are not the cause, pushing harder does not make it worse, and throughput is
# purely a function of how many attempts are in flight:
#
#     sequential          ~0.8 successful calls/min
#     24-way concurrency  ~8.0 successful calls/min
#
# Hence: concurrency high, backoff SHORT (a 429 arrives in under a second and costs nothing, so
# sleeping through contention forfeits throughput without reducing it), pacing OFF.
# (The `global` endpoint has more headroom and is a HARD STOP: it routes outside the EU, rule 2.)
export CollectionProduction__RetryAttempts="${RETRY_ATTEMPTS:-8}"
export CollectionProduction__RetryBackoff="${RETRY_BACKOFF:-00:00:05}"
export CollectionProduction__PaceBetweenSessions="${PACE_BETWEEN_SESSIONS:-00:00:00}"
export CollectionProduction__MaxConcurrentSegments="${MAX_CONCURRENT_SEGMENTS:-8}"

[[ -n "$CALIBRATE" ]] && export CollectionProduction__CalibrationCount="$CALIBRATE"
if [[ -n "$ONLY" ]]; then
  IFS=',' read -ra IDS <<< "$ONLY"
  for i in "${!IDS[@]}"; do
    export "CollectionProduction__OnlyVideoIds__${i}=${IDS[$i]}"
  done
fi

echo "collection : $COLLECTION"
echo "corpus     : $CORPUS"
echo "mode       : $MODE$([[ "$MODE" == "live" ]] && echo "  ← REAL VERTEX SPEND")"
echo "calibrate  : ${CALIBRATE:-no (full batch)}"
echo "report     : $REPORT"
echo

exec dotnet run --project src/Worker --no-launch-profile
