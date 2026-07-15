# Google Cloud Video Intelligence API — ring: infra · verified: 2026-07-15 · confidence: high

## 1. Vitals
- **HQ/jurisdiction:** Google LLC (Alphabet), Mountain View, CA, USA. US-headquartered; EU processing is a regional option, not a corporate posture.
- **Founded:** Video Intelligence API launched GA **2018** (announced beta 2017). Alphabet is a public company; funding/headcount at the parent level, not the product.
- **Funding:** N/A — Alphabet is publicly traded (NASDAQ: GOOGL). No product-specific funding.
- **Headcount (product team):** unknown (not separately disclosed).
- **Status:** **Flat / mature-declining (legacy)**. This is a pre-LLM computer-vision API. It has not received meaningful new features in years, and Google's own **Celebrity Recognition sub-feature was deprecated (removed after 2025-09-16)**. Google's active investment is now in **Vertex AI Gemini** multimodal video understanding, which cannibalizes this product for the "video → structured output" use case. Treat as a stable-but-frozen infra primitive, not a growing threat.

## 2. Business model
**Model type:** Pure **usage-based** (per-minute of video annotated, per feature). No subscription, no seats. It is a raw GCP API a dev team wires up themselves — a DIY building block, not a product.

Pricing ladder — **billed per minute, partial minutes rounded up, first 1,000 min/month free per feature.** Prices are USD from the vendor page; €/video-hour computed at USD→EUR ≈ 0.90 (×60 min/hr × 0.90).

| Plan (feature) | Price (USD/min) | Included | Effective €/video-h | Overage |
|---|---|---|---|---|
| Text detection (on-screen OCR) | $0.15/min | first 1,000 min/mo free | **€8.10/h** | linear, same rate |
| Speech transcription (en-US) | $0.048/min | 1,000 min/mo free | **€2.59/h** | linear |
| Label detection | $0.10/min | 1,000 min/mo free | €5.40/h | linear |
| Object tracking | $0.15/min | 1,000 min/mo free | €8.10/h | linear |
| Shot detection | $0.05/min (free w/ label) | 1,000 min/mo free | €2.70/h | linear |
| Logo / Face / Person detection | $0.10–0.15/min | 1,000 min/mo free | €5.40–8.10/h | linear |

**To replicate mdreel's core "spoken + verbatim on-screen text" you must buy at least Text Detection + Speech Transcription = $0.198/min ≈ €10.69/video-hour** in raw API fees, before any compute/storage/glue/engineering, and the output is still fragmented JSON, not structured Markdown.

- **Entry price point:** effectively $0 to start (1,000 free min/feature/month); no minimum, no contract.
- **Free tier/trial shape:** 1,000 minutes per feature per month, permanently free (plus GCP's general $300 new-account credit).
- **Enterprise motion:** Self-serve/API-first; **volume discounts >100,000 min/month via contact-sales**.
- **Meters mdreel doesn't:** per-feature metering (each analysis type billed separately — stacking features multiplies cost), plus separate **Compute Engine + Cloud Storage** charges the customer pays on top.

## 3. Product & features — checklist
- [x] transcript — Speech Transcription feature (en-US billed; other langs supported)
- [x] verbatim ON-SCREEN text (slides/code/UI) — **Text Detection (OCR)**, but **raw fragments per frame**, not layout/reading-order; no code/UI awareness
- [x] visual descriptions — Label Detection, Object Tracking, Shot Detection (labels/tags, NOT natural-language scene descriptions)
- [x] timestamps — **seconds w/ nanosecond precision** (`0.833333s`), frame-level `timeOffset` + `startTimeOffset`/`endTimeOffset` segments
- [ ] structured/Markdown output — **JSON only; no Markdown, no document assembly**
- [x] JSON/API — REST + gRPC, client libraries
- [ ] webhooks — no; async long-running-operation polling
- [ ] MCP server — no
- [ ] connectors — no (raw API; input via Cloud Storage URI or inline bytes)
- [ ] speaker ID — speaker diarization exists in Cloud Speech-to-Text but is **not** a first-class Video Intelligence output; effectively no
- [x] languages — Text Detection: Cloud Vision language set; Speech: en-US priced, other langs supported
- [?] max video length — not prominently stated on pricing page (async supports large files via GCS); unknown exact cap
- [ ] processing-speed claims — none published; async batch, not real-time (streaming variant exists for label/shot/object/explicit only)
- [x] retention/erasure controls — inherits GCP data-governance/CMEK/retention controls
- [x] self-host option — no (managed API); but **regional pinning** to europe-west1 available
- [?] speaker ID — see above

**What the output actually looks like:** per-detected-text-string JSON objects — `{ text: "Hair Salon", segments: [{ confidence, frames: [{ timeOffset, rotatedBoundingBox: { normalizedVertices } }] }] }`. It emits **isolated OCR fragments with bounding boxes tracked across frames**, one entry per recognized text run, with no notion of slide structure, code blocks, reading order, or spoken-vs-shown separation. A developer must post-process this JSON heavily to get anything resembling a usable document — exactly the assembly layer mdreel sells.

## 4. Size & customer base
- **Case studies/logos:** unknown at product level; media & entertainment is the marketed vertical (cast-list extraction, burnt-in subtitle detection). No public logo wall specific to this API.
- **Review counts/ratings:** unknown as a standalone product (subsumed under general "Google Cloud AI" G2/Capterra listings; no clean Video-Intelligence-specific rating).
- **Web-traffic estimate:** unknown (docs traffic not separable from cloud.google.com).
- **GitHub/community:** client libraries in `googleapis/googleapis`; no independent star count meaningful for a Google-owned proto. Developer community exists via Cloud forums.
- **Hiring signals:** none specific; product appears in maintenance mode.

## 5. GTM & distribution
- **Channels:** GCP console self-serve, extensive docs/tutorials SEO (ranks for "detect text in video", "OCR video frames", "video annotation API"), Google Cloud sales for volume/enterprise. No free consumer tool, no gallery, no community-led growth — distribution is Google's platform gravity.
- **Positioning (verbatim, from docs):** Text Detection *"performs Optical Character Recognition (OCR) to detect visible text from frames in a video, or video segments, and returns the detected text along with information about the frame-level location and timestamp."* Product framed for *"media & entertainment use cases"* (cast lists, subtitle detection, content compliance).
- **Who the pricing page talks to:** the **developer/ML engineer** — per-feature per-minute SKUs, `location_id` params, free-minute quotas. Zero language aimed at team leads, L&D, or DPOs.

## 6. EU/GDPR posture
- **Hosting regions:** Supported annotation regions include **europe-west1** (Belgium); pin via `location_id`. If unspecified, region derives from the video's storage location.
- **DPA available:** Yes — Google Cloud's standard **Cloud Data Processing Addendum** covers all GCP services including this API.
- **Subprocessor list:** Google publishes a GCP-wide subprocessor list (long, Google-entity-heavy).
- **No-training terms:** GCP standard terms — customer data not used to train Google's models without consent (per Cloud DPA); this is a discriminative CV model, not a generative one being fine-tuned on inputs.
- **Certifications:** Full GCP stack — SOC 1/2/3, ISO 27001/27017/27018, ISO 27701, C5, etc.
- **Residency premium:** **No** — europe-west1 processing is the **same price** as US; region is a free parameter. EU residency is available but neither charged nor marketed as a differentiator. **A US parent company under EU DPA scrutiny (Schrems/US-transfer risk) — the exact anxiety mdreel's EU-native positioning exploits.**

## 7. Threat assessment
- **Overlap with mdreel's ICP:** **Low-to-medium.** Same raw capability (OCR-on-video + transcription) but a totally different buyer. It sells to developers who will build the pipeline; mdreel sells the finished pipeline + Markdown output to teams who won't. It's the "infra ring" a DIY team would assemble — a *make-vs-buy* alternative, not a competing product.
- **What they'd have to do to kill mdreel (+ likelihood):** Ship a managed "video → structured Markdown for RAG" product with slide/code-aware layout, spoken-vs-shown separation, EU-native DPA marketing, and no per-feature meter. **Unlikely from *this* API** (it's frozen/legacy). The real Google threat vector is **Vertex Gemini multimodal**, not Video Intelligence — this specific product is aging out.
- **What mdreel does that they structurally can't (easily):** (1) **Assembly** — reading-order, slide/code/UI structure, spoken-vs-shown separation into portable Markdown; Video Intelligence returns disconnected bounding-box fragments. (2) **Single all-in price** (€0.65/h COGS, one Pro plan) vs stacking $0.198/min ≈ €10.69/h of raw feature SKUs before glue. (3) **EU-native compliance narrative** to DPOs — impossible for a US-parent API. (4) **No engineering required** for the buyer.
- **What mdreel should steal:** (a) **Frame-level nanosecond timestamp precision** as a spec bar; (b) the **transparent per-minute mental model** developers already trust (mdreel's per-hour caps map cleanly onto it); (c) their **cost anchor** as sales ammunition — "raw Google Video Intelligence is ~€10.69/video-hour in API fees alone and gives you JSON fragments; mdreel gives you finished Markdown."

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | Text detection $0.15/min after 1,000 free min | https://cloud.google.com/video-intelligence/pricing | 2026-07-15 | Q |
| 2 | Speech transcription $0.048/min (en-US), 1,000 free min | https://cloud.google.com/video-intelligence/pricing | 2026-07-15 | Q |
| 3 | Label $0.10, Object tracking $0.15, Shot $0.05 (free w/ label), Face/Person $0.10/min | https://cloud.google.com/video-intelligence/pricing | 2026-07-15 | Q |
| 4 | First 1,000 min/mo free per feature; partial minutes rounded up | https://cloud.google.com/video-intelligence/pricing | 2026-07-15 | Q |
| 5 | Volume discounts >100,000 min/mo via contact sales | https://cloud.google.com/video-intelligence/pricing | 2026-07-15 | Q |
| 6 | Celebrity recognition deprecated after 2025-09-16 | https://cloud.google.com/video-intelligence/pricing | 2026-07-15 | Q |
| 7 | Streaming annotation only for label/shot/explicit/object (higher rates) | https://cloud.google.com/video-intelligence/pricing | 2026-07-15 | Q |
| 8 | Text Detection = OCR of visible text with frame-level location + timestamp | https://docs.cloud.google.com/video-intelligence/docs/feature-text-detection | 2026-07-15 | Q |
| 9 | Output = text fragments per frame w/ rotatedBoundingBox, not reading-order layout | https://docs.cloud.google.com/video-intelligence/docs/text-detection | 2026-07-15 | Q |
| 10 | Timestamps in seconds w/ nanosecond precision (e.g. 0.833333s) | https://docs.cloud.google.com/video-intelligence/docs/text-detection | 2026-07-15 | Q |
| 11 | Text detection languages = Cloud Vision API supported languages | https://docs.cloud.google.com/video-intelligence/docs/feature-text-detection | 2026-07-15 | Q |
| 12 | Supported regions include europe-west1; region set via location_id | https://cloud.google.com/video-intelligence/docs/regionalization | 2026-07-15 | T (Google docs via search summary) |
| 13 | Marketed for media & entertainment (cast lists, burnt-in subtitles, compliance) | https://docs.cloud.google.com/video-intelligence/docs/feature-text-detection | 2026-07-15 | Q |
| 14 | €/h figures derived: min×60×0.90 USD→EUR (e.g., text 0.15×60×0.9=€8.10/h; text+speech €10.69/h) | (derivation) | 2026-07-15 | E |
| 15 | Product GA 2018 / pre-LLM CV API, no per-product funding (Alphabet public) | general knowledge; Google Cloud launch history | 2026-07-15 | E |
| 16 | GCP certifications SOC/ISO and Cloud DPA apply | https://cloud.google.com/terms/data-residency | 2026-07-15 | T (GCP terms via search) |