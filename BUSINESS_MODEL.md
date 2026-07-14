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

> ⚠️ **Prices above are hypotheses and are now explicitly gated on the A3 finding** (see §8).
> If usage turns out to be one-time **backfill** rather than recurring **flow**, subscription
> tiers are the wrong instrument entirely and this table becomes a **prepaid credit pack**
> (*"€200 for 20 hours"*). Do not lock these prices until PLAN.md Phase 5.

### Unit economics — measured, and no longer the risk

Guardrail was: COGS < 20% of price/hour. **Measured 2026-07-14** (experiments/001,
`gemini-2.5-flash` @ `europe-central2`, real 50-min demo recording):

| Component | €/video-hour | €/video-min | |
|---|---|---|---|
| Stage B, blended (Stage A routes ~67% static content to the cheap config) | 0.261 | 0.0044 | measured |
| × 1.3 retry overhead (see the ~8% degenerate-generation finding, PLAN.md Phase 0) | 0.340 | 0.0057 | measured |
| Stage C fusion | 0.110 | 0.0018 | measured |
| **LLM subtotal** | **0.45** | **0.0075** | **measured** |
| ffmpeg transcode/segmentation on Cloud Run | 0.15 | 0.0025 | ⚠️ **estimate — not yet metered** |
| GCS transient + orchestration | 0.05 | 0.0008 | ⚠️ **estimate** |
| **All-in COGS** | **≈ 0.65** | **≈ 0.011** | |

> ⚠️ **The ledger currently omits ~30% of true COGS.** ffmpeg/Cloud Run compute is real money
> and is metered nowhere. CLAUDE.md rule 6 is hereby extended from "every LLM call" to
> "every LLM call **and every compute step**". Implemented in PLAN.md Phase 2.

**🚩 Break-even price: €0.011/video-minute (€0.65/video-hour) all-in** — €0.0075/min LLM-only.

| Price point | €/video-min | Gross margin |
|---|---|---|
| API PAYG €0.15/min | 0.150 | 92.7% |
| Pro overage (€8/h) | 0.133 | 91.8% |
| Pro €149 @ full 25 h | 0.099 | 89.0% |
| Business overage (€6/h) | 0.100 | 89.0% |
| **Business €690 @ full 150 h** (worst case) | **0.077** | **85.7%** |

**The lowest realizable price sits 7× above break-even. Gemini pricing would have to rise ~6×
before the cheapest tier stops earning.** The €1.50/h guardrail holds with enormous headroom.

**Conclusion: COGS is a solved problem and is not a risk.** Further cost engineering optimizes
the one variable that is already safe. The binding constraint is **~47 retained paying accounts**
(≈ €176/mo contribution each) to cover ~€300/mo fixed infra + an €8,000/mo founder salary —
that, not COGS, is the finish line.

Caveats: numbers cover one content category (demo screen-recording); slide-talk and
talking-head are closed in PLAN.md Phase 0.2. Note also that the **YouTube ingestion path
cannot use the static-content lever** (no local bytes → no ffmpeg → no static detection), so it
costs the full ~€0.45/video-hour — which is why the free public YouTube tool needs hard abuse
caps and result caching (PLAN.md Phase 3).

## 7. Go-to-market

1. **Design partners first (weeks 0–8):** 3–5 companies from the founder's network (Denmark/Poland). Offer: free/cheap processing of a real video library in exchange for feedback, a testimonial, and pricing validation. One question validates the whole thing: *"You have recordings your AI agents can't see — will you pay to turn them into searchable docs that never leave the EU?"*
2. **Developer-led motion:** public API docs, an honest technical blog (chunking strategy, cost engineering, GDPR architecture), llms.txt, MCP server. Channels: HN, r/LocalLLaMA, r/RAG, LinkedIn (founder has an audience in the .NET/architecture space).
3. **Compliance-led landing page:** /security and /gdpr pages are sales collateral for the buyer's DPO — subprocessor list, retention policy, DPA download, data-flow diagram.
4. **Later:** connectors marketplace (Teams/Zoom/Drive) as the expansion wedge; self-hosted enterprise edition as the top tier.

## 8. Key risks & mitigations

> Re-ranked 2026-07-14 after `experiments/workflow1-decision-memo.md`. **Every high-importance /
> weak-evidence risk is Value or Business viability. Feasibility — the axis a technical founder
> de-risks first — is the one quadrant already closed.**

| Risk | Mitigation |
|---|---|
| 🚨 **A5 — Distribution doesn't work (TOP RISK).** GTM is self-serve/inbound with no founder outreach, so traffic is the long pole. The funnel needs ~2,000–5,000 qualified visitors to yield ~5 paying customers; content compounds over months. At Pro's ~€131/mo contribution, **any CAC above ~€800 breaks payback, and outbound sales is arithmetically impossible at this ACV.** | Start distribution *first* and run it parallel to all engineering (PLAN.md Phase 0.3 — the plan was reordered around exactly this). Free public YouTube tool + curated gallery = the product markets itself. LinkedIn (.NET/architecture audience) is the cheapest channel by an order of magnitude. |
| 🚨 **A3 — Backfill, not flow.** If the job is "process our 200 h archive", revenue spikes for one month and collapses. **Steady-state ARPA is plausibly 3–5× below acquisition-month ARPA**, which makes subscription pricing a category error no engineering can fix. | Instrument cohort hour-decay (week 1 vs. week 4) **before the first user** — PLAN.md Phase 4. It cannot be answered retroactively. If month-2 < 20% of month-1: switch to **prepaid credit packs**, not monthly tiers. |
| 🚨 **A1 — EU residency is a checkbox, not a purchase driver.** The entire differentiator, squeezed from both sides: a serious DPO notes GCP is US-controlled (CLOUD Act — see §4) and may reject "EU region" outright; a buyer who doesn't care won't pay a premium over Cloudglue. The wedge lives in a narrow band nobody has confirmed exists. | Landing-page headline A/B (PLAN.md Phase 0.3): EU-lead vs. capability-lead. **If the EU arm doesn't clearly win, move all positioning to the capability story.** |
| Gemini/OpenAI make DIY trivial ("just send the video to the model") | Value shifts to pipeline: chunking of long videos, cost control, consistent schema, batch reliability, compliance packaging. Sell the boring hard parts. **Note the sharp edge: ICP #1 and #3 are dev teams building AI assistants — the audience best equipped to build this themselves. Tested by the Stripe checkout in PLAN.md Phase 4.** |
| Cloudglue moves into EU | Speed + EU-owned-infra roadmap + no-lock-in file model they won't copy (it undermines their platform play). |
| Output quality disappoints on messy real-world video | Design-partner phase exists exactly to tune prompts/schema on real corporate footage before public launch. |
| Vertex model pricing/availability changes | Provider abstraction layer from day one (see ARCHITECTURE.md); Mistral (EU) as planned second backend. |
| Buyer confusion: "is this a transcription tool?" | All messaging anchors on *agents/knowledge bases*, never on "transcription". Demo shows RAG answering a question only answerable from on-screen content. |

## 9. Success criteria for MVP (first 90 days after launch)

> Rewritten 2026-07-14. The old criteria assumed **design-partner outreach**, which the founder
> has ruled out — the motion is self-serve/inbound. Every criterion below is therefore
> measurable **without a single conversation**.

- ~~COGS < €1.50/h~~ ✅ **Already met — €0.65/h all-in, measured 2026-07-14.** Retired as a goal.
- **Traffic (A5, top risk):** ≥ 2,000 qualified visitors reached inbound. *If this fails, nothing
  else matters — the business dies here first.*
- **A1:** the EU-lead headline arm clearly beats the capability-lead arm. (If not: reposition.)
- **A2:** ≥ 5% of trials click through to Stripe checkout, and **≥ 1 person actually pays.**
- **A3:** cohort hour-decay measured. Month-2 hours ≥ 20% of month-1 ⇒ subscription is valid;
  below ⇒ switch to prepaid credit packs.
- **A4:** output confirmed citable across all three content categories (screen-recording,
  slide-talk, talking-head).
- Time from upload to Markdown for a 1 h video: < 15 min. *(Gated on the Phase 2 output-token
  caps — ~8% of benchmark calls ran away and would blow this SLO.)*

**The number that actually matters:** ~47 retained paying accounts = founder salary + infra.

## 10. Explicit non-goals for MVP

- **YouTube processing is never a paid feature.** The free public YouTube tool + curated gallery
  exist for *distribution only* (A5) — paste a URL, see real output, zero friction, zero trust
  required. **The paid product is private recordings.** Two hard reasons this is not negotiable:
  (1) Vertex only ingests videos that are public or owned by **our** GCP account, so a customer's
  unlisted/internal recordings can *never* work this way; (2) the moment it becomes a paid tier
  we have changed businesses and walked into the §8 buyer-confusion risk — *"is this a YouTube
  tool?"* is a worse question than *"is this a transcription tool?"*
- No self-hosted edition (documented as roadmap; architecture must not preclude it).
- No connectors (upload + API only).
- **No plan tiers, no metered overage, no dashboard, no Terraform/Cloud SQL** until there is a
  paying customer (PLAN.md "Deferred"). Cloud SQL alone idles at ~€25–50/mo against a ~€300/mo
  fixed base.
- No built-in chat/RAG/search over processed videos — we produce files, we do not compete with the customer's retrieval stack.
- No fine-tuning, no training on customer data — ever, contractually.
- No SOC 2 / ISO 27001 certification yet — design for auditability, certify when a customer demands it.
