# 001 — Gemini video benchmark (PLAN.md Phase 0)

Throwaway Python scripts validating the 4 riskiest assumptions before product code exists.
Escape-hatch rules apply (`experiments/README.md`): no style/test standards, never imported
by `src/`. Results graduate to RESULTS.md → BUSINESS_MODEL.md §6 / ARCHITECTURE.md §2+§8.

## Auth & cost

- Auth: your local `gcloud` user credentials (`gcloud auth print-access-token`), no key files.
- Every Vertex call appends to `out/ledger.csv` (per-modality tokens + USD). Budget ≲ €5.

## Run order

```powershell
py -m venv .venv && .venv\Scripts\pip install -r requirements.txt
.venv\Scripts\python 01_prep.py            # renditions + segments + upload to dev bucket
.venv\Scripts\python 02_static_timeline.py # Q4: static/active timeline of the long video
.venv\Scripts\python 04_matrix.py          # Q2: configs A/B/C on segments 1-2 (6 Vertex calls)
.venv\Scripts\python 05_habicen.py         # Q3: silent-video edge case
.venv\Scripts\python 06_fusion.py          # bonus: segments 3-4 + Stage C fusion -> out/output.md
```

Phase 0.1 (YouTube spike, PLAN.md) — results in `out/YOUTUBE.md`:

```powershell
.venv\Scripts\python 07_youtube.py acceptance  # Q1: do the EU regions accept a YouTube fileUri
.venv\Scripts\python 07_youtube.py offsets     # Q2: does startOffset/endOffset clip server-side
.venv\Scripts\python 07_youtube.py cost        # Q3: EUR/video-hour on the un-blended public path
```

Run them in that order — it is a safety property, not a preference. `countTokens` silently ignores
`fileData` media parts (it returns the text-only count), so acceptance and windowing can only be
proven with a *paid* call — and if offsets turned out to be ignored, one call on a long video would
process the entire thing. `acceptance` therefore probes with a 60 s low-res window and a 256-token
output cap, which bounds the blast radius to cents.

`03_stage_b.py` is the shared Stage B runner (also a CLI for one-off calls); pass
`youtube_window=(start, end)` for the public path.

## Outputs (committed)

`out/` — ledger.csv, per-call segment JSONs (raw + `.norm.json` with global timestamps),
static_timeline.json, matrix_summary.json, output.md, RESULTS.md, plus the Phase 0.1 artifacts
(`YOUTUBE.md`, `yt_*.json`).

Local `work/` (renditions, frames) and `.venv/` are gitignored. Source videos live in
`~/Downloads` and the dev GCS bucket during the experiment; bucket is emptied afterwards.
