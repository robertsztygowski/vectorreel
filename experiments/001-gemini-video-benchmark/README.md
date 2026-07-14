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

`03_stage_b.py` is the shared Stage B runner (also a CLI for one-off calls).

## Outputs (committed)

`out/` — ledger.csv, per-call segment JSONs (raw + `.norm.json` with global timestamps),
static_timeline.json, matrix_summary.json, output.md, RESULTS.md.

Local `work/` (renditions, frames) and `.venv/` are gitignored. Source videos live in
`~/Downloads` and the dev GCS bucket during the experiment; bucket is emptied afterwards.
