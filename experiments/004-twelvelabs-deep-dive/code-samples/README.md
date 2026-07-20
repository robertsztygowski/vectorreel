# code-samples — TwelveLabs v1.3 API, runnable + observed outputs

> 🧊 Point-in-time (2026-07-20), never authoritative. All samples were run against the **live
> free-tier API** with the founder's account; outputs below are the real captured responses
> (full JSON in `../api-captures/`). The API key is read from `../.env.local` (gitignored) via
> `$TWELVELABS_API_KEY` — it is **never** hardcoded in these files and never appears in any capture.

## Files
- `run_pipeline.py` — end-to-end Python driver (create index → upload+index → poll → search →
  analyze → embed). Uses only the stdlib (`urllib`), no SDK. Writes redacted request/response
  pairs to `../api-captures/`. Re-run just the query steps by setting `TL_INDEX_ID` + `TL_VIDEO_ID`.
- `curl-samples.sh` — the same six calls as copy-paste `curl` one-liners with inline expected output.

## How to run (PowerShell)
```powershell
cd experiments/004-twelvelabs-deep-dive
$env:TWELVELABS_API_KEY = (Get-Content .env.local | ? {$_ -match '^TWELVELABS_API_KEY='}) -replace '^TWELVELABS_API_KEY=',''
python code-samples/run_pipeline.py
```

## Observed run — 2026-07-20 (60s Big Buck Bunny clip, 854×480, CC-BY)

| Step | Endpoint | Content-Type | Status | Latency | Notes |
|---|---|---|---|---|---|
| Create index | `POST /v1.3/indexes` | json | 201 | 0.6 s | `marengo3.0` + `pegasus1.2` |
| Upload+index | `POST /v1.3/tasks` | multipart | 201 | 1.9 s | async; returns task `_id` |
| Poll ready | `GET /v1.3/tasks/{id}` | — | 200 | — | **uploading→queued→indexing→ready in ~45 s** |
| Search | `POST /v1.3/search` | **multipart** | 200 | 0.7 s | 9 total results, ranked clips w/ start/end |
| Analyze | `POST /v1.3/analyze` | json | 200 | **12.8 s** | Pegasus; `usage` 22 in / 249 out tokens |
| Embed (text) | `POST /v1.3/embed` | **multipart** | 200 | 0.6 s | **512-dim** vector |

### Gotchas discovered (observed, not documented clearly)
1. **`/search` and `/embed` reject `application/json`** → `content_type_invalid`, must use
   `multipart/form-data`. `/analyze` is the opposite (JSON).
2. **`/analyze` streams NDJSON by default** (`event_type: text_generation` chunks); pass
   `"stream": false` for a single consolidated JSON.
3. **Video < 360×360 is rejected** at upload (`video_resolution_too_low`) — the classic
   320×180 Big Buck Bunny fails; must be ≥ 360p on the short side.
4. **`Marengo-retrieval-2.7` embedding model is rejected** ("no longer supported, use
   marengo3.0") — live confirmation of the Marengo 2.7 sunset.

### Sample Analyze output (verbatim excerpt — Pegasus described BBB accurately)
> "The video opens with a tranquil scene of a grassy field, a flowing stream, and a tree … the
> focus shifts to a large gray rabbit awakening in its burrow, stretching, and exploring …
> **Main Visual Chapters:** 1. [00:00~00:05] Introduction to the grassy field and stream. 2.
> [00:05~00:11] The bird singing and the title card. 3. [00:12~00:22] The rabbit's burrow …"

Full outputs: `../api-captures/0{1..6}-*.json` and `00-run-summary.json`.
