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

Phase 0.2 (public benchmark & demo corpus, PLAN.md) — results in `out/CORPUS.md`:

```powershell
.venv\Scripts\python 08_corpus.py            # licence gate: YouTube Data API -> out/corpus.json
.venv\Scripts\python 09_benchmark.py run     # Stage B over the corpus (600 s segments, LOW res)
.venv\Scripts\python 09_benchmark.py probe   # the independent re-probe that grades A4
.venv\Scripts\python 09_benchmark.py report  # verdicts -> out/corpus_verdict.json (free)
.venv\Scripts\python 10_render.py all        # publishable Markdown -> out/corpus_md/
```

**`08_corpus.py` is a gate, not a helper.** Nothing may be published unless the YouTube Data API
says `status.license == "creativeCommon"`. It needs `YOUTUBE_API_KEY` (see `.env.example`) — a
**metadata-only** key: it never fetches bytes and is not an ingestion path (CLAUDE.md rule 8).
The CC *search filter* is a discovery hint; this is the evidence.

`03_stage_b.py` is the shared Stage B runner (also a CLI for one-off calls); pass
`youtube_window=(start, end)` for the public path and `derive_clip_dur=True` to measure coverage
against what Vertex actually **fetched** rather than what you asked for.

⚠️ **The budget cap is real and it will stop you** — `spike_budget()` anchors to disk, so the phase
cap is cumulative across passes rather than being handed out fresh to each script. It halted the
0.2 render mid-flight. That is the feature.

## Outputs (committed)

`out/` — ledger.csv, per-call segment JSONs (raw + `.norm.json` with global timestamps),
static_timeline.json, matrix_summary.json, output.md, RESULTS.md, the Phase 0.1 artifacts
(`YOUTUBE.md`, `yt_*.json`), and the Phase 0.2 artifacts (`CORPUS.md`, `corpus*.json`,
`corpus_md/`).

Local `work/` (renditions, frames) and `.venv/` are gitignored. Source videos live in
`~/Downloads` and the dev GCS bucket during the experiment; bucket is emptied afterwards.
