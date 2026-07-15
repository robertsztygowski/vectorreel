# Azure AI Video Indexer — ring: infra · verified: 2026-07-15 · confidence: high

## 1. Vitals

**HQ/jurisdiction:** Microsoft Corporation, Redmond, WA, USA — Azure AI Video Indexer is a managed cloud service within the Azure AI (Foundry Tools) portfolio, not a standalone entity. European operations delivered from EU Azure datacentres (West Europe / Netherlands, North Europe / Ireland, Germany West Central, Sweden Central, Norway East, France Central, Poland Central). [Q]

**Founded/launched:** Video Indexer launched as a public preview in 2017 (as part of Azure Media Services); rebranded "Azure AI Video Indexer" and became part of Azure AI Foundry Tools. Azure Media Services dependency retired June 30 2024; VI moved to standalone ARM resource. [Q — launch year inferred from pricing effectiveStartDate 2022-02-01 for earliest SKUs; rebranding timeline from release-notes; E for exact preview launch year]

**Public status:** Wholly owned product line of Microsoft Corporation (NASDAQ: MSFT). No separate headcount reported — N/A. [Q]

**Status + why:** Active and being invested in. July 2026 docs timestamp confirms active development (release-notes last commit 2026-06-16, overview updated 2026-07-13). July 2024 shift from Azure Media Services to standalone ARM resource modernised the product. Generative AI features (multimodal summarisation with GPT-4o/Phi, Bring Your Own Model) added Nov 2024 – May 2025 signal active product investment. [Q]

---

## 2. Business model

**Model type:** Consumption/metered SaaS API. Per-input-minute billing; audio and video analysis billed separately and combinable. No subscriptions, no seats, no minimum commit on retail tier. [Q]

**Pricing table** — West Europe (EU West) region, from Azure Retail Prices API queried 2026-07-15; USD→EUR ×0.90 as stated. "Indexing Analysis" SKUs are the current pricing generation (introduced Feb 2023; Basic Video effective Mar 2024 in EU West). [Q for raw prices, E for combined rows]

| Plan / Preset | USD/min | EUR/min (×0.90) | Effective €/video-hour | What's included |
|---|---|---|---|---|
| Basic Audio Indexing | $0.0126 | €0.01134 | **€0.68** | Transcript, translation, closed captions |
| Standard Audio Indexing | $0.024 | €0.0216 | **€1.30** | + Speaker ID, language detect, sentiment, keywords, named entities, topics |
| Advanced Audio Indexing | $0.04 | €0.036 | **€2.16** | + Audio event detection |
| Basic Video Indexing | $0.045 | €0.0405 | **€2.43** | Object detection, labels, OCR, keyframes, scene/shot detection |
| Standard Video Indexing | $0.09 | €0.081 | **€4.86** | + Face/celebrity recognition, OCR-based keywords/named entities, topics, moderation |
| Advanced Video Indexing | $0.15 | €0.135 | **€8.10** | + Observed people, clothing detection, clapperboard, digital patterns, textual logo |
| Video Modification | $0.01 | €0.009 | **€0.54** | Editing/stitching operations |
| **Standard Audio + Standard Video (combined — realistic ICP use case)** | **$0.114** | **€0.1026** | **€6.16** | Full transcript + speaker ID + OCR + all mid-tier insights [E — sum of two separate meters] |
| **Basic Audio + Basic Video (minimum useful combined)** | **$0.0576** | **€0.05184** | **€3.11** | Transcript + OCR + basic video [E] |
| **Advanced Audio + Advanced Video (maximum)** | **$0.19** | **€0.171** | **€10.26** | Everything [E] |

**Entry price:** First paid minute after free tier exhausted; no minimum monthly spend. [Q]

**Free tier/trial:** Up to 10 hours free for website users; up to 40 hours (2,400 minutes) free for API users, on a trial account (no Azure subscription needed). Trial account deleted if unused for 12 months. No time limit otherwise stated. [Q]

**Enterprise motion:** No published volume discounts visible in the retail prices API (single price tier, `tierMinimumUnits: 0`). Enterprise customers likely negotiate via Microsoft EA/MCA agreements; no public evidence of committed-use discounts for Video Indexer specifically. [Q for absence of tiers; E for EA negotiation path]

**What they meter that mdreel doesn't:** Audio and video billed as separate meters — a team that only needs transcripts but not OCR pays less ($0.0126–$0.04/min). Teams that need everything pay per component stacked. mdreel's flat-rate packaging bundles spoken-vs-shown into one deliverable at one price. [E]

---

## 3. Product & features

- [x] **Transcript** — All audio presets (Basic, Standard, Advanced). Sub-sentence segments with confidence score, speakerId, language, and millisecond-precision timestamps (e.g., `"start": "0:00:05.75"`). Output is JSON array, not formatted prose. [Q]
- [x] **Verbatim ON-SCREEN text (slides/code/UI) — OCR** — Available in all video presets (Basic, Standard, Advanced Video). Uses Azure AI Vision OCR. Extracts printed and handwritten text in 50+ languages from any visible frame (slides, street signs, whiteboards, UI). 50,000-word limit per indexed video. Output is JSON with verbatim text string, confidence score, bounding-box coordinates (left/top/width/height in pixels), language code, and one or more time-instance ranges. OCR and transcript are **separate arrays in the same JSON** — there is no semantic "spoken vs shown" separation layer; the consumer must manually cross-reference. [Q]
- [~] **Visual descriptions** — Labels identification (visual objects and actions) present in all video presets, translated into 50+ languages. Full AI image captioning/scene descriptions are not a listed insight type; summarisation is available via multimodal GPT-4o/Phi add-on (GA Feb 2025) but is a separate call and requires Azure OpenAI integration. [Q]
- [x] **Timestamps (granularity?)** — Sub-second precision throughout. Transcript instances at ~0.25 s resolution (e.g., `"adjustedStart": "0:00:45.5", "adjustedEnd": "0:00:46"`); OCR instances similarly. All insights share a common timeline in the JSON. [Q]
- [ ] **Structured/Markdown output** — Not available. Output is exclusively a proprietary JSON "insights" object. Web portal provides a visual explorer. Captions can be downloaded as SRT or VTT. No Markdown export. No portable deliverable format. [Q]
- [x] **JSON/API** — Full REST API at `api-portal.videoindexer.ai`. Upload-video, Get-Video-Index, Get-Video-Transcript-Text, Delete-Video, Create-Prompt-Content, Create-Video-Summary, and 50+ more operations. ARM resource management via Azure Resource Manager. Rate limit: 10 req/s, 120 req/min. SDKs via Azure SDK ecosystem. [Q]
- [x] **Webhooks** — Callback URL parameter on Upload-Video API call; receives POST notification on indexing state changes and person-identification events. Also supports Azure Logic Apps and Azure Functions event-driven patterns. [Q]
- [ ] **MCP server** — Not documented anywhere in official docs reviewed 2026-07-15. [unknown]
- [x] **Connectors** — Azure Logic Apps connector (official); Power Automate connector (official); Azure Functions integration (official samples). ARM/Bicep deployment templates in GitHub samples. No documented Zapier/Make connectors. [Q]
- [x] **Speaker ID** — Speaker diarization in Standard and Advanced Audio presets. Each transcript line tagged with `speakerId` (e.g., Speaker #1, Speaker #2). Distinct from face-based celebrity/person ID. [Q]
- [x] **Languages** — 70+ source languages for transcription (full list in language-support doc). 50+ for translation. Auto-detect single language (LID) or multi-language (MLID) up to 10 languages simultaneously. OCR supports 50+ languages. [Q]
- [x] **Max video length** — 6 hours for all presets except Basic Audio which supports up to 12 hours. Upload from URL: 30 GB limit. Upload from device: 2 GB limit. Minimum: 2 seconds. [Q]
- [~] **Processing-speed claims** — No SLA or published throughput figure. Documentation states: "The amount of time it takes to index a video or audio file varies … depends on multiple parameters. We recommend that you run a few test files with your own content and take an average." [Q — no specific speed claim]
- [x] **Retention/erasure controls** — `retentionPeriod` API parameter (1–7 days) auto-deletes video and all associated assets after indexing. Manual delete via portal UI or Delete-Video / Delete-Video-Source-File API. Media in customer's own Azure Storage account (customer controls deletion independently). Insights metadata in Microsoft-managed storage (deleted via API). [Q]
- [x] **Self-host option (Arc)** — "Azure AI Video Indexer enabled by Arc" runs as an Azure Arc extension on Arc-enabled Kubernetes clusters on-premises or any edge environment. **Gated: requires prior subscription approval** (sign up at `aka.ms/vi-register`). Supports only Basic Audio and Basic Video presets (not Standard or Advanced). Minimum hardware: 32-core CPU, 64 GB RAM per node (production). Control-plane telemetry goes to Azure cloud for billing/monitoring; no customer data or insights leave the edge. Same per-minute pricing as cloud. Validated on Azure Local; compatible with any Kubernetes. [Q]

**What the output actually looks like:** The primary deliverable is a single large JSON "insights" object returned by the `Get-Video-Index` API call. It contains typed arrays — `transcript` (segments with speaker, text, confidence, timestamps), `ocr` (elements with text, bounding box, timestamps), `faces`, `labels`, `keywords`, `topics`, `namedEntities`, `sentiments`, and more — all on a shared timeline. The transcript and OCR arrays are parallel structures that must be manually combined by the developer to produce any human-readable artefact. A secondary "Prompt Content" API can reformat insights into an LLM-ready string (supporting GPT-4o, Phi, Llama2 styles), but this is a lossy summarisation, not a lossless structured document. The web portal at `videoindexer.ai` renders all insights visually with a video player, but there is no export to Markdown, Word, or PDF. [Q]

---

## 4. Size & customer base

Microsoft does not break out Video Indexer revenue, user counts, or customer names separately from the Azure AI/Foundry Tools portfolio. No individual logos or case studies are published on the product page. [Q for absence; E for inference]

**GitHub samples repo** (`Azure-Samples/azure-video-indexer-samples`): Active repo with Microsoft-maintained examples for ARM/Bicep deploy, API integration, Arc extension, Logic Apps, BYO model, and VideoQnA demo. [Q]

**Reviews:** G2, Capterra, TrustRadius do not show a distinct, well-reviewed Video Indexer category as of 2026-07-15 (checked indirectly via product positioning; no dated third-party review corpus verified). [unknown — further research needed]

**Community:** No dedicated community forum; Microsoft Q&A and Stack Overflow tags exist but volume unverified. Blog posts tagged `azure.microsoft.com/blog/tag/video-indexer`. [E]

**Hiring:** N/A — embedded in Microsoft's broader Azure AI engineering organisation; no public VI-specific headcount signals. [Q]

---

## 5. GTM & distribution

**Channels:** Azure Marketplace (ARM resource creation); Azure Portal (`Create a resource → Azure AI Video Indexer`); direct API via developer portal (`api-portal.videoindexer.ai`); free trial at `videoindexer.ai` (no Azure subscription required for trial). Sold as part of Azure AI Foundry Tools / Azure AI services umbrella. Reaches customers through Microsoft's EA agreements, CSP channel partners, and Azure credit programmes. [Q]

**Positioning sentence (verbatim from overview page, 2026-07-13):** *"Azure AI Video Indexer is a comprehensive AI solution that enables organizations to extract deep insights from video (live and uploaded) and audio content. It uses advanced machine learning and generative AI models and supports a wide range of capabilities including transcription, translation, object detection, and video summarization."* [Q]

**Who the pricing page talks to:** Azure developers and architects building indexing pipelines at scale. The free trial is explicitly for "website users" (explorers) vs "API users" (builders). The at-scale guidance, ARM/Bicep templates, and callback URL pattern all assume a developer integrating VI into a larger system — not an end-user workflow tool. [Q]

---

## 6. EU/GDPR posture

**Hosting regions (EU):** West Europe (Netherlands), North Europe (Ireland), Germany West Central (Frankfurt), Sweden Central, Norway East, France Central, Poland Central — all confirmed in the Azure Retail Prices API response for this product. EU West is the primary region (isPrimaryMeterRegion: true for EU West in the API). [Q]

**EU Data Boundary:** Azure falls under Microsoft's EU Data Boundary commitment (in force since January 2023), which covers all Azure enterprise online services. Under this commitment, Customer Data and Professional Services Data are stored and processed within EU/EFTA datacentres. The EU Data Boundary documentation (updated 2025-02-26) confirms Azure is an EU Data Boundary Service. Video Indexer, as an Azure AI service under Azure, is covered. [Q]

**Important nuance:** Media files are stored in the **customer's own Azure Storage account** (customer controls). Indexing insights and metadata are stored in **Microsoft-managed storage accounts** (customer does not directly control the storage, but accesses via API; Microsoft states no charge for this storage). Both are within EU Data Boundary when deploying to EU regions. [Q]

**DPA:** Microsoft's Online Services Data Protection Addendum (DPA), incorporated by reference into the Microsoft Products and Services Agreement / Enterprise Agreement, provides GDPR Article 28 processor commitments. Covers purpose limitation, data subject rights, sub-processor disclosure, breach notification. Available at microsoft.com/licensing. [Q]

**Subprocessors:** Microsoft's standard Azure subprocessor list applies; not Video Indexer-specific. Publicly disclosed and updated quarterly. [Q]

**No-training commitment:** Microsoft's standard commitment for Azure AI services: customer data is not used to train Microsoft AI models. Stated in the DPA and Online Services Terms. No Video Indexer-specific carve-out found. [Q]

**Certifications:** Azure (and by extension Video Indexer) holds ISO 27001, ISO 27017, ISO 27018, SOC 1 Type 2, SOC 2 Type 2, SOC 3, CSA STAR Level 2, GDPR, PCI DSS, and dozens of national/industry certifications. The service is available in Azure Government (US GovCloud) with additional controls. [Q]

**Residency premium:** No residency-specific premium charged — EU West and US East prices for the "Indexing Analysis" SKUs are identical ($0.0126 Basic Audio, $0.024 Standard Audio, $0.09 Standard Video as of 2026-07-15 API query). Some peripheral regions (Norway East, Switzerland) show slightly higher prices (e.g., Advanced Audio Indexing Analysis $0.0572 NO East vs $0.04 US/EU West). [Q]

**Private endpoints:** GA since March 2025. Allows VNet-only access to the Video Indexer API; data traverses Microsoft backbone, not public internet. Web portal and some widget types not supported with private endpoints. [Q]

---

## 7. Threat assessment

**ICP overlap + why:** Very high overlap with the developer persona in a 50–500 person EU software/IT company. Any team with Azure spend and a developer comfortable with REST APIs can activate the free trial in minutes, index corporate training videos, and receive transcript + OCR JSON within an hour — with zero procurement friction. The service is positioned precisely at "developers who want to extract insights at scale," which is exactly who would evaluate mdreel. A DPO at a mid-sized EU tech firm will find Azure's EU Data Boundary, ISO 27018, and DPA checkbox-compatible with GDPR requirements.

**What they'd need to kill mdreel + likelihood:**
1. Add a Markdown export mode that interleaves transcript and OCR into a structured, spoken-vs-shown document (effort: low for Microsoft, a weekend project for a competent dev). Likelihood in 12 months: **low** — this is not on any public roadmap and the service is clearly positioned as a data-extraction API, not a document-production tool.
2. Price further down. Already competitive; unlikely to move significantly. Likelihood: **low**.
3. Explicitly market to L&D/knowledge-base use cases in EU SMBs (not currently a focus area). Likelihood: **medium** over 24 months as generative AI for enterprise video heats up.

**What mdreel structurally does that Video Indexer cannot (without significant DIY):**
- Produces a **portable, human-readable Markdown document** combining spoken transcript and on-screen text in a single coherent narrative — ready to drop into Obsidian, Notion, or a RAG pipeline. Video Indexer produces a JSON blob requiring custom parsing.
- Enforces **spoken-vs-shown semantic separation** as a first-class feature — the mdreel output explicitly labels "Spoken:" vs "Shown:" content. Video Indexer emits `transcript[]` and `ocr[]` as parallel arrays with no semantic bridging layer.
- **No Azure dependency** — mdreel output is plain Markdown files; no Azure Storage, no Azure account, no ARM resources. Video Indexer bakes in Azure Storage, Azure Arc Kubernetes, Azure Logic Apps, etc.
- **EU-native company** and EU-only processing, not "big US cloud that has an EU region." For DPOs sensitive to Schrems II residual risk or preferring non-US vendors, this matters beyond checkbox compliance.

**What mdreel should steal from Video Indexer:**
- **Generous free trial structure** — 40 hours API-free with no credit card is a strong activation driver; mdreel should match or beat this.
- **Callback/webhook pattern** — Video Indexer's `callbackUrl` on upload is clean UX for async pipelines; mdreel should ensure webhook notification is a first-class feature.
- **Preset tiers with transparent pricing** — The Basic/Standard/Advanced naming convention communicates value-vs-cost tradeoffs cleanly; mdreel's pricing story should be equally legible.
- **Explicit "no streaming = no encoding charge" option** — Cost-conscious ICP devs will appreciate granular billing control; mdreel could offer a "transcript-only" mode that skips frame-level OCR for audio-only recordings.

---

## 8. Evidence log

| # | Claim | Source URL | Checked | Grade |
|---|---|---|---|---|
| 1 | Free trial: 10 h website / 40 h API; paid = unlimited with Azure subscription | https://azure.microsoft.com/en-us/pricing/details/video-indexer/ | 2026-07-15 | Q |
| 2 | Pricing is per input minute; audio and video billed separately; Basic/Standard/Advanced presets for each | https://azure.microsoft.com/en-us/pricing/details/video-indexer/ | 2026-07-15 | Q |
| 3 | West Europe pricing (all 10 SKUs incl. Basic Audio $0.0126, Standard Audio $0.024, Advanced Audio $0.04, Basic Video $0.045, Standard Video $0.09, Advanced Video $0.15) from Azure Retail Prices API | https://prices.azure.com/api/retail/prices?api-version=2023-01-01-preview&$filter=contains(productName,%20%27Video%20Indexer%27)%20and%20armRegionName%20eq%20%27westeurope%27 | 2026-07-15 | Q |
| 4 | Basic Video Indexing Analysis in EU West effective 2024-03-01; Standard/Advanced Video from 2023-01-01/2023-02-01 | Azure Retail Prices API (same URL as #3) — effectiveStartDate fields | 2026-07-15 | Q |
| 5 | No volume discount tiers visible (tierMinimumUnits: 0.0 for all SKUs) | Azure Retail Prices API (same URL as #3) | 2026-07-15 | Q |
| 6 | OCR available in Basic/Standard/Advanced Video presets; 50,000-word limit | https://learn.microsoft.com/en-us/azure/azure-video-indexer/avi-support-matrix | 2026-07-15 | Q |
| 7 | OCR output JSON shape: id, text, confidence, left/top/width/height, angle, language, instances with adjustedStart/adjustedEnd | https://learn.microsoft.com/en-us/azure/azure-video-indexer/ocr-insight | 2026-07-15 | Q |
| 8 | OCR extracts text from 50+ languages (printed and handwritten); uses Azure AI Vision OCR tech | https://learn.microsoft.com/en-us/azure/azure-video-indexer/ocr-insight | 2026-07-15 | Q |
| 9 | Transcript JSON shape: text, confidence, speakerId, language, instances with ms-precision timestamps | https://learn.microsoft.com/en-us/azure/azure-video-indexer/transcription-translation-lid-insight | 2026-07-15 | Q |
| 10 | Transcript and OCR are separate arrays in the JSON insights object — no spoken-vs-shown semantic merging | https://learn.microsoft.com/en-us/azure/azure-video-indexer/insights-overview | 2026-07-15 | Q |
| 11 | Insights stored in Microsoft-managed storage; media stored in customer's own Azure Storage | https://learn.microsoft.com/en-us/azure/azure-video-indexer/indexing-configuration-guide | 2026-07-15 | Q |
| 12 | retentionPeriod param (1–7 days); Delete-Video and Delete-Video-Source-File APIs | https://learn.microsoft.com/en-us/azure/azure-video-indexer/indexing-configuration-guide | 2026-07-15 | Q |
| 13 | Speaker ID (diarization) in Standard and Advanced Audio presets | https://learn.microsoft.com/en-us/azure/azure-video-indexer/indexing-configuration-guide | 2026-07-15 | Q |
| 14 | Video max duration 6 hours (12 h Basic Audio); URL upload limit 30 GB | https://learn.microsoft.com/en-us/azure/azure-video-indexer/avi-support-matrix | 2026-07-15 | Q |
| 15 | API rate limit: 10 req/s, 120 req/min; callbackUrl parameter on upload | https://learn.microsoft.com/en-us/azure/azure-video-indexer/considerations-when-use-at-scale | 2026-07-15 | Q |
| 16 | Callback URL: POST on indexing state change and person-identification events | https://learn.microsoft.com/en-us/azure/azure-video-indexer/considerations-when-use-at-scale | 2026-07-15 | Q |
| 17 | Logic Apps / Power Automate connectors; Azure Functions integration | https://learn.microsoft.com/en-us/azure/azure-video-indexer/faq | 2026-07-15 | Q |
| 18 | 70+ source languages for transcription; 50+ for translation; MLID up to 10 langs simultaneously | https://learn.microsoft.com/en-us/azure/azure-video-indexer/language-support | 2026-07-15 | Q |
| 19 | Arc extension: runs on Azure Arc-enabled Kubernetes; Basic presets only; gated sign-up required | https://learn.microsoft.com/en-us/azure/azure-video-indexer/arc/azure-video-indexer-enabled-by-arc-overview | 2026-07-15 | Q |
| 20 | Arc minimum hardware: 32 cores, 64 GB RAM (production); same per-minute pricing as cloud | https://learn.microsoft.com/en-us/azure/azure-video-indexer/arc/azure-video-indexer-enabled-by-arc-overview | 2026-07-15 | Q |
| 21 | Arc: no customer data or insights sent to cloud; only control-plane/billing telemetry | https://learn.microsoft.com/en-us/azure/azure-video-indexer/arc/azure-video-indexer-enabled-by-arc-overview | 2026-07-15 | Q |
| 22 | Private endpoints GA March 2025; VNet isolation via Private Link | https://learn.microsoft.com/en-us/azure/azure-video-indexer/release-notes | 2026-07-15 | Q |
| 23 | EU Data Boundary: Azure is an EU Data Boundary Service covering all EU/EFTA countries; last updated 2025-02-26 | https://learn.microsoft.com/en-us/privacy/eudb/eu-data-boundary-learn | 2026-07-15 | Q |
| 24 | EU Data Boundary: Austria, Belgium, Denmark, Finland, France, Germany, Greece, Ireland, Italy, Netherlands, Norway, Poland, Spain, Sweden, Switzerland datacentres | https://learn.microsoft.com/en-us/privacy/eudb/eu-data-boundary-learn | 2026-07-15 | Q |
| 25 | Trial account: 2,400 free indexing minutes; deleted if unused 12 months | https://learn.microsoft.com/en-us/azure/azure-video-indexer/create-account | 2026-07-15 | Q |
| 26 | No structured Markdown output; captions exportable as SRT/VTT; primary output is JSON | https://learn.microsoft.com/en-us/azure/azure-video-indexer/ocr-insight and /insights-overview | 2026-07-15 | Q |
| 27 | Prompt Content API supports GPT-4o, GPT-4o Mini, Phi3, Phi3.5, Llama2 for LLM-ready reformatting (GA) | https://learn.microsoft.com/en-us/azure/azure-video-indexer/release-notes (Nov 2024 entry) | 2026-07-15 | Q |
| 28 | Multimodal Video Summarisation GA (February 2025) using Azure OpenAI + Phi | https://learn.microsoft.com/en-us/azure/azure-video-indexer/release-notes (Feb 2025 entry) | 2026-07-15 | Q |
| 29 | Overview docs last updated 2026-07-13 confirming active development | https://learn.microsoft.com/en-us/azure/azure-video-indexer/video-indexer-overview (ms.date field) | 2026-07-15 | Q |
| 30 | Face Identify and Celebrity Recognition are Limited Access features requiring registration application | https://learn.microsoft.com/en-us/azure/azure-video-indexer/limited-access-features | 2026-07-15 | Q |
| 31 | Positioning verbatim quote from overview page | https://learn.microsoft.com/en-us/azure/azure-video-indexer/video-indexer-overview | 2026-07-15 | Q |
| 32 | USD→EUR conversion rate ×0.90 applied per dossier instructions | — | — | E |
| 33 | No MCP server documented in official docs | Multiple Microsoft Learn doc pages reviewed 2026-07-15 | 2026-07-15 | E (absence of evidence) |
| 34 | Combined Standard Audio + Standard Video cost = $0.114/min, €6.16/video-hour | Derived from #3 ($0.024 + $0.09 = $0.114 × 60 × 0.90) | 2026-07-15 | E |
| 35 | No published processing-speed SLA or throughput figure | https://learn.microsoft.com/en-us/azure/azure-video-indexer/faq | 2026-07-15 | Q |
