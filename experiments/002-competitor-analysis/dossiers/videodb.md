# VideoDB — ring: direct · verified: 2026-07-15 · confidence: med

## 1. Vitals

| Field | Detail | Grade |
|---|---|---|
| **HQ / Jurisdiction** | India-founded team; product incorporated likely as Delaware C-Corp (US); no explicit jurisdiction stated on site | E — inferred from team/investor context; no public incorporation doc |
| **Founded** | Unknown — GitHub org activity and product maturity suggest 2023; no founding date published | E |
| **Funding** | No public rounds announced on company site or discoverable via Crunchbase (403 gate); likely pre-seed/seed stage bootstrapped or angel-funded | E — derived from absence of announced rounds + startup stage signals |
| **Investors** | Unknown; no investors named publicly | — |
| **Headcount** | Unknown; company page says "a team of engineers" with no count; likely <20 FTE | E — inferred from stage, product scope, and GitHub contributor footprint |
| **Status** | Active, shipping. Meaningfully repositioned in 2025–2026 from "video database for AI apps" to "agentic perception layer" — responding to agent-wave demand. Self-described: *"The perception, memory, and action for AI agents."* | Q |

---

## 2. Business model

**Model type:** Consumption/credit-based SaaS. Pay-as-you-go credits consumed against a published unit rate card. No seats or per-user fees. Free tier requires no credit card; Pro is $20/month subscription that provides $20 rolling credit with top-up. Enterprise is negotiated.

**Full pricing table** (USD→EUR conversion rate: 0.90; source: www.videodb.io/pricing, checked 2026-07-15):

| Plan | Price (USD/mo) | Price (EUR/mo ×0.90) | Included | Effective €/video-h | Overage |
|---|---|---|---|---|---|
| **Free** | $0 | €0 | $20 one-time starter credit; no credit card required | €0 until credit exhausted; then PAYG unit rates apply | PAYG at unit rates below |
| **Pro** | $20 | €18 | $20 monthly rolling credit (unused rolls over); no rate limits; auto-recharge; priority support via Email + Slack Connect | **€1.11–2.13/hr** (E — see derivation) | Unlimited top-ups; auto-recharge enabled |
| **Enterprise** | Custom | Custom | Hybrid/on-premise deploy; custom models & fine-tuning; 99.9% SLA; dedicated support; expert consulting | Negotiated | Negotiated |

**Live unit rate card** (all USD, source: www.videodb.io/pricing.md, checked 2026-07-15): [Q]

*See (Ingest & Process):*
- Realtime ingest: $0.084/hour
- File uploads: $0.09/GB
- Transcoding SD (360p): $0.0040/min
- Transcoding HD (720p/1080p): $0.0090/min

*Understand (Indexing & Search):*
- Transcription (spoken index): $0.01/min
- Scene processing (segmentation + embedding): $0.003/scene
- Search query: $1.50/1,000 queries
- LLM/VLM tokens — Basic: $0.0016/1K; Advanced: $0.0065/1K; Ultra: $0.00875/1K

*Remember (Storage):*
- Media storage: $0.03/GB/month
- Index storage: $0.0005/min/month

*Act (Editing & Generation):*
- Inline edit: $0.004/min | Overlay edit: $0.01/min | Resize: $0.01/min
- Dubbing: $0.15/min | Translation: $0.02/min | Audio gen: $0.12/min | Video gen: $0.50/sec | Image gen (Ultra): $0.19/img

*Delivery:*
- Video streaming: $0.07/GB | Audio & image hosting: $0.06/GB | Downloads (720p): $0.03/min

**Effective €/video-hour derivation** (E — computed from published unit rates, 1-hour meeting video):

- Scenario A — transcript-only (spoken index, 1GB file, HD transcode):
  Upload ($0.09) + HD transcode ($0.54) + transcription ($0.60) = **$1.23 → €1.11/hr**
- Scenario B — transcript + scene index (1 scene/30 s = 120 scenes, Advanced VLM at ~1K tokens/scene):
  Scenario A + scene processing (120 × $0.003 = $0.36) + VLM tokens (120K × $0.0065 = $0.78) = **$2.37 → €2.13/hr**
- Plus recurring: index storage ~$0.03/hr/month; media storage ~$0.02/hr/month
- Search and streaming are billed additionally on top

**Entry price:** $0 (credit-gated). Paid entry: $20/month. [Q]

**Free tier/trial shape:** $20 one-time starter credits, first 50 uploads free, no credit card required. [Q]

**Enterprise motion:** Inbound "Talk to us" CTA on pricing page; no outbound sales evidence; engineering-led PLG primary motion. [Q/E]

**What they meter that mdreel doesn't:**
- Storage per GB-month (media + index separately)
- Streaming delivery bandwidth ($/GB)
- Search queries ($/1K)
- Video editing (per edited minute)
- Real-time stream ingest ($/hour of live stream)
- Token consumption for VLM scene description
- Dubbing, translation, video generation

---

## 3. Product & features

- [x] **Transcript** — `video.index_spoken_words()` produces ASR transcript with word-level, sentence-level, or time-based timestamps. Languages: English (US/UK/AU), Spanish, French, German, Hindi, Japanese, Chinese, Korean, Russian, and more via language codes. [Q — docs.videodb.io/pages/understand/indexing-pipelines/create-an-index.md]

- [~] **Verbatim ON-SCREEN text (slides/code/UI)** — No dedicated OCR pipeline. On-screen text is captured via VLM scene indexing when the prompt explicitly requests it (e.g., `prompt="Give the content written on the slides, output None if it isn't the slides."`). This uses a vision language model (VLM) — not character-level OCR — so output is interpretive rather than verbatim; code, exact formatting, and low-contrast text may be lost or paraphrased. The blog post "MP4 is Wrong Primitive" illustrates: `text_index = video.index_scenes(prompt="Extract on-screen text")`. This is fundamentally different from deterministic OCR. [Q — docs.videodb.io/examples-and-tutorials/video-rag/use-case-conference-slides.md; Q — videodb.io/blogs/mp4-is-wrong-primitive]

- [x] **Visual descriptions** — `video.index_scenes(prompt=...)` with configurable shot-based or time-based frame extraction; multi-index per video supported; prompt engineering determines granularity and domain focus. [Q — docs.videodb.io SDK docs]

- [x] **Timestamps** — Second-level precision (e.g., `shot.start = 45.2`, `shot.end = 52.8`). Word-level granularity available for spoken index (`Segmenter.word`). [Q — docs.videodb.io]

- [ ] **Structured/Markdown output** — No Markdown export. Results are SDK objects (`shot.start`, `shot.end`, `shot.text`, `shot.search_score`) and REST JSON. No portable artifact produced. The output is a queryable database with streaming playback URLs — not a document. [E — inferred from entire SDK API surface; confirmed by absence of any export-to-Markdown feature in docs/API reference]

- [x] **JSON/API** — Full REST API documented at docs.videodb.io/api-reference/introduction; Python SDK (pip install videodb) and Node.js/TypeScript SDK (npm install videodb). [Q]

- [x] **Webhooks** — `callback_url` parameter on indexing operations for async completion notification; `create_alert()` with `webhook_url` for real-time event alerts on live streams; WebSocket URLs available. [Q — docs.videodb.io SDK docs]

- [x] **MCP server** — `videodb-director-mcp` installable via `uvx videodb-director-mcp --api-key=KEY`; connects Director backend to Claude Code, Cursor, Codex and other AI IDEs. [Q — github.com/video-db/agent-toolkit]

- [x] **Connectors** — YouTube URL ingest, public HTTP URL, local file upload, RTSP stream, desktop screen/mic/camera capture (Capture SDK), meeting recording API. [Q — docs.videodb.io SDK docs]

- [?] **Speaker ID / diarization** — Not mentioned in any documentation reviewed; transcript does not appear to separate speakers. Unknown. [E — absence of mention in indexing docs, SDK reference, and rate card]

- [x] **Languages** — 10+ auto-detected and explicit code support: en, en_us, en_uk, en_au, es, fr, de, hi, ja, zh, ko, ru. [Q — docs.videodb.io create-an-index.md]

- [?] **Max video length** — Not specified in any public documentation. Free tier limits: first 50 uploads. No duration cap stated. [E — unknown; inference: storage and processing billed per unit so no hard cap at Free/Pro]

- [ ] **Processing-speed claims** — No SLA or throughput numbers published for file processing. Real-time latency claimed for live stream events but no ms/s numbers given. [E — inferred from absence]

- [~] **Retention/erasure controls** — `ephemeral=True` flag for live stream indexing (process without persisting); delete APIs exist for videos, collections, indexes, capture sessions. No explicit retention policy or automated erasure schedule documented. [Q — docs.videodb.io core-concepts/overview.md]

- [~] **Self-host option** — Enterprise tier lists "Hybrid / on-premise deploy." No self-host documentation publicly available; developer tier is cloud-only. [Q — videodb.io/pricing.md]

**What the output actually looks like (3 sentences):**
VideoDB produces no portable artifact — the output is a queryable index stored in their cloud database, accessed exclusively through the Python/Node.js SDK or REST API. A search call returns a `SearchResult` object containing a list of `Shot` items, each with `shot.start` (float seconds), `shot.end`, `shot.text` (the matched transcript segment or VLM scene description), and `shot.play()` which opens a streaming URL to the video clip. There is no export-to-file, no Markdown, no document: consumers must query the VideoDB API at retrieval time, making their knowledge base entirely dependent on VideoDB's cloud infrastructure and pricing remaining available — the structural definition of retrieval lock-in.

---

## 4. Size & customer base

- **Logo / case studies:** No customer logos or named case studies visible on the public site as of 2026-07-15. [E — inferred from absence on company, platform, and pricing pages]
- **Reviews (dated):** No G2, Capterra, or Trustpilot reviews discovered. No Product Hunt page found. [E]
- **Web traffic:** Unknown — no SimilarWeb or SEMrush data accessible. [E]
- **GitHub stars (checked 2026-07-15):**
  - `video-db/Director` (AI video agents framework): **⭐ 1,400+** [Q — github.com/video-db GitHub org page, which explicitly states "Director ⭐ 1.4k"]
  - `video-db/videodb-python`: Star badge present; exact count not rendered in text-fetched HTML; estimated **hundreds** based on SDK maturity and community signals [E]
  - `video-db/agent-toolkit`: Star badge present; count not extractable [E]
- **Discord:** Active community at discord.com/invite/py9P639jGz (member count not publicly shown without login). [Q — github.com/video-db]
- **Cookbook / demos:** `video-db/videodb-cookbook` on GitHub with Colab notebooks; labs.videodb.io showcases project gallery. [Q]
- **Hiring:** `/developers` page mentions a program to run meetups in developer cities; no formal job listings discovered. [Q/E]

---

## 5. GTM & distribution

**Channels:**
- **Developer PLG:** Free tier with $20 credits and no credit card; Colab notebooks; PyPI/npm distribution; GitHub public repos as primary discovery mechanism. [Q]
- **Agent ecosystem / MCP:** MCP server installation via `npx skills add video-db/skills` targets Claude Code, Cursor, Codex users directly — embedding in AI coding workflow. [Q]
- **Content / SEO:** Blog at videodb.io/blogs with philosophical posts ("MP4 Is the Wrong Primitive," "Why AI Agents Are Blind") targeting developer mindset; llms.txt / llms-full.txt for LLM-native discoverability. [Q]
- **Community:** Discord, YouTube demos playlist, Labs showcase, meetup sponsorship program. [Q]
- **Director open-source framework:** 1.4k-star repo that drives awareness of the underlying VideoDB infrastructure. [Q]

**Positioning sentence (verbatim from docs.videodb.io, checked 2026-07-15):**
> *"The perception, memory, and action for AI agents"* [Q]

**Who the pricing page talks to:** Solo developers and indie developers (Free/Pro tiers explicitly labeled "Best for testing APIs" and "Best for indie devs and small teams"). Pricing scenarios reference cameras, archive search, and RAG bots — not enterprise business units or L&D teams. The page is engineering-facing throughout with code-centric language. [Q — videodb.io/pricing.md]

---

## 6. EU/GDPR posture

| Item | Finding | Grade |
|---|---|---|
| **HQ / legal entity jurisdiction** | Not explicitly stated; team signals India + US setup | E |
| **Hosting regions** | Not disclosed publicly; no region selector in console; likely US-based cloud (AWS/GCP) | E — inferred from absence of EU-region mention |
| **DPA available?** | No Data Processing Agreement found on any public page; no DPA link in pricing, docs, or company pages | E — inferred from absence |
| **Subprocessors disclosed?** | Not disclosed; docs reference third-party LLMs/VLMs for scene processing (token billing implies external model calls) | E |
| **No-training clause?** | Not found; Privacy Policy URL (videodb.io/privacy, videodb.io/privacy-policy, videodb.io/terms) returned 404 on 2026-07-15 | E — inferred from absence; absence of policy is itself a compliance red flag for EU buyers |
| **EU certifications** | None mentioned (no SOC 2, ISO 27001, GDPR certification claimed) | E |
| **EU data residency option** | Not offered at Free/Pro. Enterprise tier mentions "hybrid / on-premise deploy" which could enable self-hosted EU deployment, but no EU-cloud option documented | Q (enterprise claim) / E (residency inference) |
| **GDPR stance** | No GDPR-specific language found on site. The product is structurally designed to route all video and index data through VideoDB's cloud, making it incompatible with strict EU data residency requirements at self-service tiers | E |

**Summary:** VideoDB has no discoverable GDPR compliance posture at Free/Pro tiers. No privacy policy was accessible on 2026-07-15. Any EU enterprise customer with a DPO would require a DPA, subprocessor disclosure, and EU data residency guarantees — none of which are publicly available. This is the single largest structural barrier to selling into mdreel's ICP.

---

## 7. Threat assessment

**ICP overlap and why:**
VideoDB's natural buyer is a **US-based developer or startup building an AI agent application** that needs to process video programmatically — not an EU IT/L&D team with a DPO. Their pricing page scenarios (camera monitoring, archive RAG bots) and developer-facing copy align poorly with EU software teams trying to extract knowledge from internal training videos and meeting recordings for non-technical knowledge bases. Overlap exists only at the technical fringe: a developer at an EU software company building internal tools could adopt VideoDB for meeting search — but the GDPR posture, lock-in model, and absence of Markdown export would surface as blockers quickly.

**What they'd need to kill mdreel:**
1. EU data residency with auditable DPA and subprocessor list — years of compliance work for a US-first startup
2. A portable, exportable document artifact (Markdown or equivalent) that breaks their database lock-in model — architecturally antithetical to their value proposition
3. True verbatim OCR (not VLM-described on-screen text) for code, slides, and UI elements — requires a dedicated OCR pipeline, not just prompt-engineering VLMs
4. Spoken-vs-shown separation as a first-class concept — they conflate both in multimodal search results
5. Non-technical ICP sales motion (enterprise L&D/DPO buyer) vs. their pure PLG developer motion

**Likelihood:** Low in the 12–24 month window. Their pivot toward "agentic perception" (live streams, cameras, desktop agents) is taking them *further* from mdreel's document-knowledge-base use case, not closer.

**What mdreel structurally does that VideoDB cannot (by design):**
1. **Portable Markdown artifact** — mdreel's output travels with the customer; VideoDB's output is locked inside their cloud API forever
2. **EU-managed data residency** — mdreel can provide a DPA, EU processing, and auditable subprocessors; VideoDB cannot at self-service tiers
3. **Verbatim on-screen text** — dedicated OCR preserving exact code, slide text, and UI strings; VideoDB uses VLM interpretation which paraphrases
4. **Spoken-vs-shown as a structural output channel** — mdreel separates transcript from visual text; VideoDB merges them into one searchable index with no export separation
5. **Non-technical buyer accessibility** — Markdown is readable by a knowledge manager or L&D admin; VideoDB requires Python/Node.js SDK skills to consume outputs

**What mdreel should steal from VideoDB:**
1. **Ephemeral mode concept** — "process but don't persist" is a smart privacy-preserving option for GDPR-sensitive customers who want analysis without cloud retention; mdreel could offer a process-and-purge tier
2. **Multi-index / multi-perspective framing** — VideoDB's concept of creating multiple named indexes (safety, summary, text) on the same video is excellent for knowledge navigation; mdreel could expose semantic facets in its Markdown output (e.g., headers per concept domain, not just per timestamp)
3. **Callback / webhook on completion** — async processing notification for long videos is table-stakes for production integrations; if not already built, mdreel should prioritize
4. **llms.txt / agent-toolkit pattern** — publishing a machine-readable product context file for LLM IDEs is a zero-cost developer acquisition move that mdreel should copy
5. **Scene-level search result with playable link** — mdreel's Markdown output is portable but non-interactive; consider a companion viewer that deep-links Markdown timestamps back to video timestamps for validation UX

---

## 8. Evidence log

| # | Claim | Source URL | Checked | Grade |
|---|---|---|---|---|
| 1 | "The perception, memory, and action for AI agents" — positioning tagline | https://docs.videodb.io | 2026-07-15 | Q |
| 2 | "Realtime vision for AI agents." — homepage headline | https://www.videodb.io | 2026-07-15 | Q |
| 3 | Free plan: $20 one-time credits, no credit card required | https://www.videodb.io/pricing.md | 2026-07-15 | Q |
| 4 | Pro plan: $20/month, rolling credit, no rate limits, auto-recharge, Slack Connect support | https://www.videodb.io/pricing.md | 2026-07-15 | Q |
| 5 | Enterprise: hybrid/on-premise deploy, custom models, 99.9% SLA | https://www.videodb.io/pricing.md | 2026-07-15 | Q |
| 6 | Transcription rate: $0.01/min | https://www.videodb.io/pricing.md | 2026-07-15 | Q |
| 7 | Scene processing rate: $0.003/scene | https://www.videodb.io/pricing.md | 2026-07-15 | Q |
| 8 | Search query rate: $1.50/1,000 queries | https://www.videodb.io/pricing.md | 2026-07-15 | Q |
| 9 | File upload rate: $0.09/GB | https://www.videodb.io/pricing.md | 2026-07-15 | Q |
| 10 | HD transcoding rate: $0.0090/min | https://www.videodb.io/pricing.md | 2026-07-15 | Q |
| 11 | Media storage: $0.03/GB/month; index storage: $0.0005/min/month | https://www.videodb.io/pricing.md | 2026-07-15 | Q |
| 12 | Video streaming delivery: $0.07/GB | https://www.videodb.io/pricing.md | 2026-07-15 | Q |
| 13 | LLM/VLM token tiers: Basic $0.0016/1K, Advanced $0.0065/1K, Ultra $0.00875/1K | https://www.videodb.io/pricing.md | 2026-07-15 | Q |
| 14 | `video.index_spoken_words()` with `Segmenter.word/sentence/time` and language code support | https://docs.videodb.io/pages/understand/indexing-pipelines/create-an-index.md | 2026-07-15 | Q |
| 15 | Languages auto-detected: English, Spanish, French, German, Italian, Portuguese, Dutch; explicit codes for Hindi, Japanese, Chinese, Korean, Russian | https://docs.videodb.io/pages/understand/indexing-pipelines/create-an-index.md | 2026-07-15 | Q |
| 16 | On-screen text via VLM scene index prompt: `"Give the content written on the slides, output None if it isn't the slides."` | https://docs.videodb.io/examples-and-tutorials/video-rag/use-case-conference-slides.md | 2026-07-15 | Q |
| 17 | Blog shows: `text_index = video.index_scenes(prompt="Extract on-screen text")` — no dedicated OCR | https://www.videodb.io/blogs/mp4-is-wrong-primitive | 2026-07-15 | Q |
| 18 | Multimodal indexing example prompt: `"Describe the scene, people, and any visible text"` | https://docs.videodb.io/pages/understand/indexing-pipelines/multimodal-indexing.md | 2026-07-15 | Q |
| 19 | Search result shape: `shot.start`, `shot.end`, `shot.text`, `shot.search_score`, `shot.play()` → streaming URL | https://docs.videodb.io/pages/core-concepts/indexes-and-search.md | 2026-07-15 | Q |
| 20 | No Markdown export capability found in full API reference index | https://docs.videodb.io/llms.txt | 2026-07-15 | E |
| 21 | `callback_url` on indexing; `create_alert(webhook_url=...)` on live stream index | https://docs.videodb.io SDK Python reference | 2026-07-15 | Q |
| 22 | MCP server: `uvx videodb-director-mcp --api-key=KEY` | https://github.com/video-db/agent-toolkit | 2026-07-15 | Q |
| 23 | `npx skills add video-db/skills` — agent skill distribution | https://docs.videodb.io (homepage) | 2026-07-15 | Q |
| 24 | Ephemeral mode: `rtstream.index_visuals(ephemeral=True)` | https://docs.videodb.io/pages/core-concepts/overview.md | 2026-07-15 | Q |
| 25 | Director framework: ⭐ 1,400+ GitHub stars | https://github.com/video-db (org overview page) | 2026-07-15 | Q |
| 26 | Discord community at discord.com/invite/py9P639jGz | https://github.com/video-db | 2026-07-15 | Q |
| 27 | "We are a team of engineers who have spent years working at the intersection of AI, video systems, and cloud infrastructure." | https://www.videodb.io/company | 2026-07-15 | Q |
| 28 | No funding rounds announced; no investor names on site | https://www.videodb.io/company | 2026-07-15 | E |
| 29 | Privacy policy URL (videodb.io/privacy, videodb.io/privacy-policy) returned 404 | https://www.videodb.io/privacy | 2026-07-15 | E |
| 30 | Terms of service URL (videodb.io/terms, videodb.io/terms-of-service) returned 404 | https://www.videodb.io/terms | 2026-07-15 | E |
| 31 | No DPA, no GDPR statement, no EU hosting region, no certifications found on any page | https://www.videodb.io/pricing, /company, /platform | 2026-07-15 | E |
| 32 | Effective €/video-h transcript-only ~€1.11; full transcript+scene ~€2.13 — derived from published unit rates at USD→EUR 0.90 | https://www.videodb.io/pricing.md | 2026-07-15 | E |
| 33 | Speaker diarization not mentioned in SDK, docs, or rate card | https://docs.videodb.io/llms.txt (full API index) | 2026-07-15 | E |
| 34 | Meeting recording API exists: `record_meeting()` endpoint | https://docs.videodb.io/llms.txt | 2026-07-15 | Q |
| 35 | Platform page: "Hybrid / on-premise deploy" available at enterprise tier | https://www.videodb.io/pricing.md | 2026-07-15 | Q |
| 36 | First 50 uploads free, no credit card required | https://github.com/video-db/videodb-python (README) | 2026-07-15 | Q |
| 37 | Real-time alert payload format (JSON with timestamp, confidence, label) visible in docs code example | https://docs.videodb.io (homepage code sample) | 2026-07-15 | Q |
| 38 | Crunchbase.com/organization/videodb returned 403; no funding data extractable | https://www.crunchbase.com/organization/videodb | 2026-07-15 | E |
| 39 | Pricing page meta-description: "Transparent unit rates for ingest, indexing, search, streaming, and generation." | https://www.videodb.io/pricing (HTML head) | 2026-07-15 | Q |
| 40 | Dubbing rate: $0.15/min; Translation: $0.02/min; Video gen: $0.50/sec | https://www.videodb.io/pricing.md | 2026-07-15 | Q |

**Screenshot:** `assets/videodb-pricing-2026-07-15.png` (full-page pricing, captured via Playwright 2026-07-15).
