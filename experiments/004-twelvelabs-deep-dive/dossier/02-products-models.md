# 02 — TwelveLabs: Products & Models

> 🧊 Point-in-time (2026-07-20), never authoritative. Cite every claim (URL + date or asset file).
> Grades: **Q** primary, **T** third-party, **E** estimate.

## 1. Product overview (what TwelveLabs sells)

TwelveLabs sells artificial intelligence video understanding via REST APIs, SDKs (Python and Node.js), and an official Model Context Protocol (MCP) server. Their product offering is structurally divided into two major layers:
1. **Models (Core APIs)**: For task-specific developer integration.
   - **Search API**: Multimodal any-to-video semantic retrieval.
   - **Analyze API**: Prompt-based text generation, summarization, and video segmentation.
   - **Embed API**: Generation of vector embeddings for video, audio, image, and text.
2. **Agents (High-level Orchestration)**:
   - **Jockey** (launched in research preview on July 15, 2026): A unified agentic system that reasons across video/image knowledge stores to perform multi-step planning, highlight compilation, and cross-video subject tracking.

TwelveLabs' developer experience is heavily engineered around the concept of "Indexes"—logical folders that store video assets and their processed multi-dimensional embeddings to enable fast, low-latency search and reasoning.

## 2. Marengo — embeddings / search model family

The **Marengo** family is TwelveLabs' specialized suite of multimodal video embedding and search models. It processes video frames, motion, temporal relationships, audio, speech, and on-screen text into a unified vector representation.

- **Current Version**: **Marengo 3.0** (Generally Available since November 17, 2025).
- **Sunsets / Deprecations**: **Marengo 2.7** was officially deprecated on February 28, 2026, and sunsetted on March 30, 2026 (7:00 PM PT). All Marengo 2.7 indexes were retired; embeddings generated under v2.7 are entirely incompatible with v3.0 and must be regenerated.
- **Capabilities**:
  - **Composed Search**: Allows combining text descriptions with up to 10 images in a single query (GA November 17, 2025; R2 updated October 14, 2025; multi-image search added March 11, 2026).
  - **Cinematography & Motion Understanding**: Enhanced recognition of camera terms (zoom, pan, tracking shot) and sports movements.
  - **Sports Intelligence**: Built-in dynamics tracking for soccer, basketball, baseball, ice hockey, and American football.
  - **Entity Search**: Tracks specific people across video libraries (GA November 17, 2025).
  - **Audio & Transcription Control**: Spoken-word searches can be isolated using lexical or semantic matching.
- **Dimensionality**: **512-dimensional embeddings** in Marengo 3.0 (optimized down from 1024-dimensions in Marengo 2.7) for faster retrieval and reduced storage.
- **Language Coverage**: Supports querying in **36 languages plus English** (37 total, expanded from 12+English in Marengo 2.7).
- **Limits**: Video duration from **4 seconds to 4 hours**, file size **≤ 4 GB** (up to 6 GB via S3/Amazon Bedrock), resolutions from 360x360 to 5184x2160, and aspect ratios between 1:1 and 1:2.4 (or 2.4:1 and 1:1).

## 3. Pegasus — analyze / generate model family

The **Pegasus** family is TwelveLabs' generative video-to-text language model. It integrates visual, audio, and speech details to produce detailed, context-aware textual output.

- **Current Version**: **Pegasus 1.5** (Generally Available since April 20, 2026).
- **Previous Version**: **Pegasus 1.2** remains active as an option for general analysis only.
- **Capabilities (Pegasus 1.5)**:
  - **Video Segmentation**: Automatically slices videos into logical segments (editorial narratives, speaker shifts, sports plays, brand/product appearances) and populates a user-defined typed metadata schema for each segment.
  - **General Analysis**: Real-time prompt-based text generation (summaries, custom Q&A, company memos) with no pre-indexing of the video asset required.
  - **Multimodal Prompting**: Accepts up to 4 reference images in a prompt (using `<@image_name>` placeholders) to guide analysis or identify specific visual entities.
  - **Video Clipping**: Restricts analysis to a specific slice of the video (minimum 4 seconds).
- **Context Window**:
  - **Pegasus 1.5**: **261,120 tokens** shared context window (covering video, audio transcription, prompts, reference images, schemas, and output).
  - **Pegasus 1.2**: Rigidly limited to a **2,000-token prompt** and a **4,096-token maximum response**.
- **Output Capacity**: Supports generated outputs up to **98,304 tokens** (upgraded from 65,536 on May 28, 2026). Truncated outputs return `finish_reason: "length"`.
- **Language Coverage**: Full support for English; partial support for 12 languages (Arabic, Chinese, French, German, Italian, Japanese, Korean, Portuguese, Russian, Spanish, Thai, Vietnamese).

## 4. Modalities & what "structured output" actually looks like

TwelveLabs handles structured video understanding through two distinct paradigms:

1. **Structured responses via Custom JSON Schemas (Pegasus 1.2 & 1.5)**
   Users pass a standard JSON schema within the `response_format` parameter (`type: "json_schema"`) to `/analyze` or `/analyze/tasks`. The model outputs a matching, validated JSON object.
   - *Example schema structure*:
     ```json
     {
       "type": "object",
       "properties": {
         "summary": { "type": "string" },
         "chapters": {
           "type": "array",
           "items": {
             "type": "object",
             "properties": {
               "chapter_title": { "type": "string" },
               "chapter_summary": { "type": "string" },
               "start": { "type": "number" },
               "end": { "type": "number" }
             },
             "required": ["chapter_title", "chapter_summary", "start", "end"]
           }
         }
       },
       "required": ["summary", "chapters"]
     }
     ```
   - Timestamps can be explicitly formatted as `seconds`, `hh:mm:ss`, or `hh:mm:ss.fff` using the parameter `{"type": "timestamp", "format": "<format>"}` (introduced May 22, 2026).

2. **Video Segmentation (Pegasus 1.5)**
   Using `analysis_mode: "time_based_metadata"`, users define custom segment classifications (up to 10 per request) under `segment_definitions` with up to 20 custom metadata fields each. The platform returns a timestamped array of segments matching the defined criteria.
   - *Pricing Caveat*: For paid plans, video segmentation is billed as: `billable video duration * number of segment definitions` (effective May 7, 2026).

## 5. Model routing / availability (native API vs AWS Bedrock)

TwelveLabs models are accessible both via their native first-party cloud platform and through partnership integrations with AWS:

- **Native API**: Always hosts the latest models and features first, including Pegasus 1.5, direct/multipart asset uploads, batch analysis (up to 1,000 requests, introduced June 18, 2026), and the Jockey agent preview.
- **AWS Bedrock Availability**:
  - **Marengo 3.0** is available on Amazon Bedrock (since October 29, 2025) as `twelvelabs.marengo-embed-3-0-v1:0` (512-dimensional output, supporting S3 URIs or base64 input). Available in US East (N. Virginia), Europe (Ireland), and Asia Pacific (Seoul).
  - **Pegasus 1.2** is available on Amazon Bedrock (since October 30, 2025) as `twelvelabs.pegasus-1-2-v1:0`. It supports S3 URIs or base64 inputs up to 1 hour and 2 GB.
  - **Inference Routing on AWS**: Bedrock supports global cross-region inference profiles and geographic cross-region inference (e.g., `eu.twelvelabs.pegasus-1-2-v1:0` for routing strictly within EU regions to address data residency/GDPR requirements).
  - **Pegasus 1.5 availability on Bedrock**: [SPECULATION] Pegasus 1.5 is currently not documented as available on Bedrock; only Pegasus 1.2 is explicitly documented.

## Evidence log

| # | Claim | Source URL | Checked | Grade |
|---|---|---|---|---|
| 1 | Jockey Agentic System released in research preview on July 15, 2026 | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 2 | Marengo 3.0 release date November 17, 2025 | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 3 | Marengo 2.7 deprecated Feb 28, 2026; sunsetted March 30, 2026 | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 4 | Marengo 3.0 dimensions: 512, language support: 36 + English (37 total) | https://docs.twelvelabs.io/docs/concepts/models/marengo.md | 2026-07-20 | Q |
| 5 | Marengo 3.0 constraints: 4s to 4h, <= 4 GB size, resolution 360x360 to 5184x2160, FFmpeg formats | https://docs.twelvelabs.io/docs/concepts/models/marengo.md | 2026-07-20 | Q |
| 6 | Pegasus 1.5 GA on April 20, 2026 with video segmentation; general analysis April 27, 2026 | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 7 | Pegasus 1.5 context window: 261,120 tokens; max output 98,304 tokens (since May 28, 2026) | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 8 | Pegasus 1.2 limits: 2,000 prompt, 4,096 response | https://docs.twelvelabs.io/docs/concepts/models/pegasus.md | 2026-07-20 | Q |
| 9 | Pegasus 1.5 constraints: 4s to 2 hours, <= 2 GB size | https://docs.twelvelabs.io/docs/concepts/models/pegasus.md | 2026-07-20 | Q |
| 10 | Structured responses JSON schemas and timestamp formatting (seconds, hh:mm:ss, hh:mm:ss.fff) | https://docs.twelvelabs.io/docs/guides/analyze-videos/structured-responses.md | 2026-07-20 | Q |
| 11 | Video segmentation definitions (up to 10 segment types, up to 20 fields each) | https://docs.twelvelabs.io/docs/guides/segment-videos.md | 2026-07-20 | Q |
| 12 | Video segmentation pricing on paid plans (duration * segment definitions) | https://docs.twelvelabs.io/docs/get-started/release-notes.md | 2026-07-20 | Q |
| 13 | Marengo 3.0 and Pegasus 1.2 on AWS Bedrock (including cross-region geo-routing for EU) | https://docs.twelvelabs.io/docs/cloud-partner-integrations/amazon-bedrock/analyze-videos.md | 2026-07-20 | Q |
| 14 | Pegasus 1.5 on Bedrock unavailable as of today | https://docs.twelvelabs.io/docs/cloud-partner-integrations/amazon-bedrock/analyze-videos.md | 2026-07-20 | Q |
