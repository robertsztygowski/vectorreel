#!/usr/bin/env bash
# TwelveLabs v1.3 REST API — curl samples (mdreel deep-dive 004, M4).
# Observed working against the live free-tier API on 2026-07-20.
#
# SECRETS: the key is read from $TWELVELABS_API_KEY (never hardcoded). Source it from
# the gitignored .env.local first, e.g.:
#   export TWELVELABS_API_KEY=$(grep '^TWELVELABS_API_KEY=' .env.local | cut -d= -f2)
#
# Key behavioural facts learned the hard way (see api-captures/):
#   - Auth header is  x-api-key: <key>   (NOT Authorization: Bearer).
#   - /search and /embed require multipart/form-data, NOT application/json.
#   - /analyze takes application/json and STREAMS NDJSON unless "stream": false.
#   - Uploaded video must be >= 360x360; <=4h / <=4GB (Marengo 3.0).
#   - Embedding model_name must be "marengo3.0" (Marengo-retrieval-2.7 is rejected).
set -euo pipefail
: "${TWELVELABS_API_KEY:?source it from .env.local}"
BASE=https://api.twelvelabs.io/v1.3
H="x-api-key: ${TWELVELABS_API_KEY}"

# 1. Create an index with Marengo 3.0 (search/embed) + Pegasus 1.2 (analyze).
curl -sS -X POST "$BASE/indexes" -H "$H" -H 'Content-Type: application/json' -d '{
  "index_name": "mdreel-curl-demo",
  "models": [
    {"model_name": "marengo3.0", "model_options": ["visual", "audio"]},
    {"model_name": "pegasus1.2", "model_options": ["visual", "audio"]}
  ]
}'
# -> 201 {"_id":"<INDEX_ID>", ...}

INDEX_ID=<paste from above>

# 2. Upload + index a local clip (async task). Returns a task _id immediately.
curl -sS -X POST "$BASE/tasks" -H "$H" \
  -F "index_id=${INDEX_ID}" \
  -F "video_file=@tmp/bbb-60s.mp4"
# -> 201 {"_id":"<TASK_ID>", "status":"uploading", ...}

TASK_ID=<paste from above>

# 3. Poll until status == "ready" (60s clip indexed in ~45s on 2026-07-20).
curl -sS "$BASE/tasks/${TASK_ID}" -H "$H"
# -> {"status":"ready", "video_id":"<VIDEO_ID>", ...}

VIDEO_ID=<paste from above>

# 4. Search — multipart/form-data. Returns ranked clips with start/end timestamps.
curl -sS -X POST "$BASE/search" -H "$H" \
  -F "index_id=${INDEX_ID}" \
  -F "query_text=a bunny in a green meadow" \
  -F "search_options=visual" \
  -F "group_by=clip" \
  -F "page_limit=5"
# -> 200 {"data":[{"rank":1,"start":12.9,"end":22.5,"video_id":"..."}, ...],
#         "page_info":{"total_results":9,"next_page_token":"...","page_expires_at":"..."}}

# 5. Analyze (Pegasus) — application/json, non-streaming.
curl -sS -X POST "$BASE/analyze" -H "$H" -H 'Content-Type: application/json' -d "{
  \"video_id\": \"${VIDEO_ID}\",
  \"prompt\": \"Summarize this video in 3 sentences, then list the main visual chapters with start and end timestamps.\",
  \"temperature\": 0.2,
  \"stream\": false
}"
# -> 200 {"id":"...","data":"<generated text w/ [MM:SS~MM:SS] chapters>",
#         "finish_reason":"stop","usage":{"output_tokens":249,"input_tokens":22}}

# 6. Text embedding — multipart/form-data. Returns a 512-dim float vector.
curl -sS -X POST "$BASE/embed" -H "$H" \
  -F "model_name=marengo3.0" \
  -F "text=a rabbit in a forest"
# -> 200 {"model_name":"marengo3.0","text_embedding":{"segments":[{"float":[... 512 floats ...]}]}}
