# Business Model — Video-to-Markdown for AI Knowledge Bases

> Working name: TBD (referred to as "the Product" below)
> Status: pre-MVP. This document captures validated decisions and assumptions to guide implementation.

## 1. One-liner

**Turn any company video into clean, structured, timestamped Markdown that AI agents can actually use — processed entirely in the EU.**

## 2. Problem

Companies sit on hours of high-value video: internal trainings, product demos, sales calls, all-hands recordings, onboarding sessions, Teams/Zoom meetings. They are building AI knowledge bases and agents (RAG, MCP-connected assistants), but:

- Agents and RAG pipelines are text-centric. Video is a dead asset for them.
- Plain transcription loses most of the value: what is *shown on screen* (slides, UI walkthroughs, diagrams, code, on-screen text) never makes it into the transcript.
- Wiring up vision models, sampling frames, deduplicating, and merging audio + visual context into one coherent document is non-trivial engineering that every team currently rebuilds badly.
- EU companies additionally face a GDPR wall: internal recordings contain personal data (voices, faces, names, customer info) and cannot be shipped to US-based processors without friction from DPOs and legal.

## 3. Solution

Input: a video file (upload via UI, API, or connector).
Output: one structured Markdown document per video, containing:

- YAML frontmatter (title, duration, language, source, processing date, tags)
- Topic-based sections with `[hh:mm:ss]` timestamps
- Spoken content (cleaned transcript, speaker-attributed where possible)
- **Visual content**: on-screen text (slides, UI labels, code), described scenes, demo steps
- Clear separation of "spoken" vs "shown on screen" so downstream RAG can weight them
- Consistent schema across all files — output is designed to be consumed by LLMs/RAG, not (only) humans

Delivery: web UI for manual use + first-class REST API (and later an MCP server) for pipeline integration. Output is plain files — **no proprietary retrieval stack, no lock-in**. Customers own the Markdown and put it in their own repo, S3/GCS, Obsidian, SharePoint, or vector DB.

## 4. Positioning & differentiation

Direct competitor: **Cloudglue** (US, launched 2025; multimodal transcripts, extraction API, MCP server). Their existence validates demand. We do not out-feature them; we out-position them:

| Axis | Cloudglue / US tools | The Product |
|---|---|---|
| Data location | US cloud | EU regions only (GCP europe-*), EU data residency commitments |
| Legal | US processor, CLOUD Act exposure | EU-focused DPA, short subprocessor list, honest sovereignty roadmap |
| Output model | Collections + their retrieval/chat stack | Plain Markdown files, bring-your-own-RAG, zero lock-in |
| Source retention | Stores video library | **Source video deleted after processing by default** (configurable) |
| Buyer message | Developer platform | "Feed your videos to your agents without leaving the EU" |

Honesty rule for all marketing/compliance copy: current stack is **EU data residency on GCP (Google DPA, Vertex AI no-training terms, EU-regional processing)** — *not* full EU sovereignty (Google is a US company subject to CLOUD Act). State this plainly on the /gdpr page and publish a roadmap item: "EU-owned infrastructure option" (OVHcloud/Scaleway) and later a self-hosted enterprise edition. Honesty here is a sales asset; overclaiming gets destroyed by any competent DPO.

## 5. Target customers (initial ICP)

1. **Software companies / IT departments (50–500 employees) building internal AI assistants** — have Teams/Zoom recordings, demo libraries, sprint reviews; have an "AI adoption" mandate; developer buyer, uses the API.
2. **L&D / training teams** in EU mid-market companies — training video libraries they want searchable/agent-usable; UI buyer.
3. **AI consultancies & software houses** building RAG/agents for their clients — resell processing as part of delivery; API + volume pricing.

Beachhead geography: Poland + Nordics/DACH (founder network: Denmark via Conscensia/Unik). GDPR sensitivity is highest and willingness to pay for EU processing is real in these markets.

## 6. Pricing (initial hypothesis — validate with design partners)

Metric: **hours of video processed**. Simple, scales with value, easy to predict.

| Plan | Price | Included | Notes |
|---|---|---|---|
| Free trial | €0 | 2 h of video, full quality | No credit card. Must produce a "wow" on the customer's own video. |
| Pro | €149/mo | 25 h/mo, UI + API, webhooks | Overage: €8/h. Target: single team, L&D dept. |
| Business | €690/mo | 150 h/mo, connectors (Drive/SharePoint/Zoom — post-MVP), DPA self-service, priority support | Overage: €6/h. |
| API pay-as-you-go | €0.15/min (€9/h) | No subscription | For developers embedding the product in pipelines; stickiest revenue. |
| Enterprise (later) | Annual contract | Custom volume, SSO, audit log, self-hosted option | Not in MVP scope. |

Unit economics guardrail: COGS target < 20% of price per hour. **Measured 2026-07-14** (experiments/001, `gemini-2.5-flash` @ `europe-central2`, real 50-min demo recording): **≈ €0.38/video-hour** at default quality (720p, 1 fps), ≈ €0.21/h at low media resolution, ≈ €0.25–0.30/h expected blended (Stage A routes ~67% static content to the cheap config), plus ~1.3× retry overhead observed. Stage C fusion adds ≈ €0.09 per 50-min video. Even at Pro overage (€8/h) COGS is ~5%; at API pay-as-you-go (€9/h) ~4% — the €1.50/h guardrail holds with 4–7× margin. Numbers cover one content category (demo screen-recording); re-verify on slide-talk and talking-head footage at Phase 7 before locking prices. Cost controls (static-segment detection, low-media-resolution sampling) remain a core engineering task.

## 7. Go-to-market

1. **Design partners first (weeks 0–8):** 3–5 companies from the founder's network (Denmark/Poland). Offer: free/cheap processing of a real video library in exchange for feedback, a testimonial, and pricing validation. One question validates the whole thing: *"You have recordings your AI agents can't see — will you pay to turn them into searchable docs that never leave the EU?"*
2. **Developer-led motion:** public API docs, an honest technical blog (chunking strategy, cost engineering, GDPR architecture), llms.txt, MCP server. Channels: HN, r/LocalLLaMA, r/RAG, LinkedIn (founder has an audience in the .NET/architecture space).
3. **Compliance-led landing page:** /security and /gdpr pages are sales collateral for the buyer's DPO — subprocessor list, retention policy, DPA download, data-flow diagram.
4. **Later:** connectors marketplace (Teams/Zoom/Drive) as the expansion wedge; self-hosted enterprise edition as the top tier.

## 8. Key risks & mitigations

| Risk | Mitigation |
|---|---|
| Gemini/OpenAI make DIY trivial ("just send the video to the model") | Value shifts to pipeline: chunking of long videos, cost control, consistent schema, batch reliability, compliance packaging. Sell the boring hard parts. |
| Cloudglue moves into EU | Speed + EU-owned-infra roadmap + no-lock-in file model they won't copy (it undermines their platform play). |
| Output quality disappoints on messy real-world video | Design-partner phase exists exactly to tune prompts/schema on real corporate footage before public launch. |
| Vertex model pricing/availability changes | Provider abstraction layer from day one (see ARCHITECTURE.md); Mistral (EU) as planned second backend. |
| Buyer confusion: "is this a transcription tool?" | All messaging anchors on *agents/knowledge bases*, never on "transcription". Demo shows RAG answering a question only answerable from on-screen content. |

## 9. Success criteria for MVP (first 90 days after launch)

- 3 design partners processed ≥ 10 h each and confirmed output is usable in their RAG/agent stack.
- ≥ 1 partner converts to paid at or above hypothesized Pro price.
- COGS per processed hour measured and < €1.50 at default quality settings.
- Time from upload to Markdown for a 1 h video: < 15 min.

## 10. Explicit non-goals for MVP

- No self-hosted edition (documented as roadmap; architecture must not preclude it).
- No connectors (upload + API only).
- No built-in chat/RAG/search over processed videos — we produce files, we do not compete with the customer's retrieval stack.
- No fine-tuning, no training on customer data — ever, contractually.
- No SOC 2 / ISO 27001 certification yet — design for auditability, certify when a customer demands it.
