# 03 — TwelveLabs: API Reference Notes

> 🧊 Point-in-time (2026-07-20), never authoritative. Cite every claim (URL + date, or an
> `api-captures/` filename for observed behaviour). Grades: **Q** primary, **T** third-party.

## 1. Auth model (API keys, headers)

TwelveLabs uses API key authentication. Requests must include the API key in an HTTP header named `x-api-key` (or `Authorization: Bearer <key>` in legacy versions, but first-party documentation enforces `x-api-key` header).
- **Key Expiration**: New keys created after January 26, 2026, can have custom expiration periods: 3 months, 6 months, 12 months (default), a custom date, or "never expire." Previously, all keys rigidly expired after 90 days.

## 2. API surfaces

The TwelveLabs v1.3 REST API organizes resources under several primary surfaces:

### Indexes
Indexes are logical collections of videos.
- `POST /indexes`: Create a new index, specifying the index name and video understanding models (e.g. `marengo3.0` or `pegasus1.2`).
- `GET /indexes`: List indexes.
- `GET /indexes/{index_id}`: Retrieve detailed metadata for a single index.
- `DELETE /indexes/{index_id}`: Delete an index (and its associated video embeddings).

### Tasks / upload (video ingestion)
Video ingestion is an asynchronous pipeline.
- **Methods**:
  - **Direct Uploads**: For files up to 200 MB (local) or up to 4 GB (public URLs). Uses `POST /assets`.
  - **Multipart Uploads**: Recommended for local files over 200 MB; supports parallel chunks up to 10 GB (upgraded from 4 GB on July 7, 2026). Uses `POST /assets/multipart-uploads`.
- **Asynchronous Upload Validation**: Effective July 7, 2026, **every upload is processed asynchronously** and returns immediately with `status: "processing"`. Users must poll `GET /assets/{asset_id}` or `GET /tasks/{task_id}` until the status transitions to `ready` (or `failed` if the file is invalid/corrupt). Polling now applies to all file sizes.
- **YouTube Ingestion Support**: **NOT SUPPORTED.** The TwelveLabs documentation explicitly states: *"Video hosting platforms like YouTube and cloud storage sharing links are not supported."* Ingestion requires a direct link to a raw media file (e.g. `.mp4`, `.mp3`).

### Search
Provides multimodal, natural language search inside an Index.
- `POST /search`: Make any-to-video search requests.
  - **Parameters**: `query` (text description), `search_options` (e.g., `["visual", "conversation", "transcription"]`), `filter` (e.g., duration, metadata), `group_by` (flat list or grouped by video).
  - Supports composed query inputs combining text and up to 10 reference images (since March 11, 2026).
- `POST /knowledge-stores/{knowledge_store_id}/search`: Search across a structured knowledge store.

### Analyze / generate (summarize, chapter, highlight, custom)
Video comprehension and generation using Pegasus.
- `POST /analyze`: Synchronous analysis for videos up to 1 hour (supporting Base64 or URL).
- `POST /analyze/tasks`: Asynchronous analysis for videos up to 2 hours (released March 31, 2026).
- **Consolidation**: Predefined legacy endpoints `/gist` and `/summarize` were deprecated on January 7, 2026, and completely **sunsetted/removed on February 15, 2026**. All text generation, summarization, and chaptering must go through `/analyze` with a custom JSON schema or custom `segment_definitions`.

### Embed
Generates vector representations of video, audio, image, and text content.
- **Embed API v2** (introduced November 17, 2025):
  - `POST /embed-v2`: Synchronous embeddings for text, images, or files under 10 minutes.
  - `POST /embed-v2/tasks`: Asynchronous task creation for larger video/audio files up to 4 hours.
  - **Fused Embeddings** (introduced March 11, 2026): Allows fetching separate modality embeddings (video/audio) or a single combined fused embedding in the same response.

## 3. Rate limits, quotas, video constraints (length / size / format)

Rate limits are multi-dimensional, measuring Duration Per Day (DPD), Duration Per Hour (DPH), Requests Per Day (RPD), Requests Per Minute (RPM), Tokens Per Day (TPD), and Tokens Per Minute (TPM).
- **Free Plan**: Share a combined 10-hour limit across indexing and analysis.
- **Video Constraints**:
  - **Marengo 3.0**: 4 seconds to 4 hours, size ≤ 4 GB (6 GB S3/Bedrock), resolutions 360x360 to 5184x2160.
  - **Pegasus 1.5**: 4 seconds to 2 hours, size ≤ 2 GB.
- **Supported Formats**: Any container/format supported by FFmpeg. Raw direct URL media files required.

## 4. SDKs (Python, JS)

TwelveLabs maintains two official SDKs:
1. **Python SDK**: `twelvelabs` (e.g., `pip install twelvelabs`).
2. **Node.js/JavaScript SDK**: `twelvelabs-js` (e.g., `npm install twelvelabs-js`).
- **Pegasus 1.5 Type Changes**: Effective April 20, 2026, the `ResponseFormat` type was split into `SyncResponseFormat` (for synchronous `/analyze`) and `AsyncResponseFormat` (for asynchronous `/analyze/tasks` and video segmentation).

## 5. Webhooks / async callbacks

Webhooks allow receiving real-time HTTP POST notifications.
- **Supported APIs**: Available for the Search (indexing tasks) and Analyze APIs. Unavailable for the Embed API.
- **Integrity Validation**: Header includes `TL-Signature` containing Unix timestamp `t` and HMAC SHA-256 signature `v1` generated with the webhook's secret key.
- **Supported Events**:
  - `index.task.ready`
  - `index.task.failed`
  - `analyze.task.ready`
  - `analyze.task.failed`

## 6. Changelog / release cadence / deprecations

TwelveLabs maintains a highly active release cadence (roughly bi-weekly or monthly updates).
- **Major Deprecations & Sunsets (2025-2026)**:
  - **`/text-in-video` OCR Endpoint**: Fully deprecated. OCR is folded into Marengo visual embeddings, queryable via search, not extracted as raw text.
  - **`/gist` and `/summarize` Predefined Endpoints**: Sunsetted and removed on February 15, 2026.
  - **Marengo 2.7 Embedding Model**: Officially deprecated February 28, 2026, and sunsetted on March 30, 2026. All legacy indexes using v2.7 are inaccessible; embeddings must be completely regenerated under Marengo 3.0.
  - **Direct Deletion of Referenced Assets**: Since April 26, 2026, deleting an asset that is referenced by an indexed asset is denied by default (returns `409 Conflict`). Deletion requires passing `force=true`.

## Evidence log

| # | Claim | Source URL | Checked | Grade |
|---|---|---|---|---|
| 1 | Header authorization uses `x-api-key` | https://docs.twelvelabs.io/docs/get-started/quickstart.md | 2026-07-20 | Q |
| 2 | Custom key expiration (3m, 6m, 12m, custom, never) introduced January 26, 2026 | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 3 | Direct and multipart upload limits (up to 10GB for local files since July 7, 2026) | https://docs.twelvelabs.io/docs/concepts/upload-methods.md | 2026-07-20 | Q |
| 4 | Asynchronous upload validation for all sizes introduced July 7, 2026 | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 5 | YouTube and hosting platform URLs not supported | https://docs.twelvelabs.io/docs/concepts/models/marengo.md | 2026-07-20 | Q |
| 6 | Predefined `/gist` and `/summarize` sunsetted/removed on February 15, 2026 | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 7 | Embed API v2 async task endpoint `/embed-v2/tasks` handles up to 4 hours video/audio | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 8 | Fused embeddings and separate modality control introduced March 11, 2026 | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 9 | Pegasus 1.5 AsyncResponseFormat vs SyncResponseFormat type split on April 20, 2026 | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 10 | Webhook response signatures (`TL-Signature`, `t`, `v1` HMAC SHA-256) and events | https://docs.twelvelabs.io/docs/advanced/webhooks/response-schema.md | 2026-07-20 | Q |
| 11 | Marengo 2.7 sunsetted on March 30, 2026 | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 12 | Deletion of referenced assets blocked by default (returns `409 Conflict`) without `force=true` since April 26, 2026 | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
