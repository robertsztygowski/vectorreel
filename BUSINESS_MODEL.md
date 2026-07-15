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

### The positioning statement

> **For EU engineering teams building an internal AI assistant, mdreel turns the recordings
> your assistant can't see — demos, trainings, incident reviews — into timestamped Markdown it can
> cite, without the footage ever leaving the EU.**

⚠️ **Note where the EU clause sits: second, as a qualifier.** That is the honest reflection of A1's
evidence level. **It gets promoted to the front of the sentence if and only if the EU arm wins the
headline A/B (METRICS.md A1); if it loses, it is cut from the positioning entirely and demoted to a
trust section.** Never say "EU-sovereign" (see the honesty rule below). Never say "transcription".

### Differentiation

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

### 5a. Market size

**→ METRICS.md §1.5 (N16–N19).** Bottom-up, no top-down market-report figures — for a wedge this
specific they are noise. Every input is an assumption.

Two things that shape strategy rather than sizing, and belong here:

- **The TAM is small enough to decide how this company is funded.** It is a good bootstrapped
  business and a bad venture business. **Fund it with customers, not a seed round** — that one
  fact should govern how much is spent, how fast, and on what.
- **The EU-premium segment exists only if A1 holds.** If EU residency is not a purchase driver
  there is no premium, and we compete on features against Cloudglue inside the broader SAM — a
  materially worse business. **That is why the A1 headline A/B is the cheapest high-value
  experiment available.**

## 6. Pricing (initial hypothesis — validate against A3, not with design partners)

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
> (*"€200 for 20 hours"*). Do not lock these prices until PLAN.md Phase 6. The MVP ships a
> **one-plan pricing page + the free tool** (PLAN.md Phase 4), not this table.

### Unit economics — measured, and no longer the risk

**→ METRICS.md §1.2 (N4–N6) for every figure, and §1.3 (N8) for the guardrail.** Not restated here.

What matters at the business level, and only this:

- **Every list price above sits ~7× or more above break-even.** Even the worst case — Business at
  full utilization — earns ~86% gross margin. Gemini pricing would have to rise ~6× before the
  cheapest tier stops earning.
- ✅ **COGS is a solved problem and is not a risk.** The old *"COGS < 20% of price"* guardrail is
  retired as a goal and demoted to a regression alarm. **Further cost engineering optimizes the one
  variable that is already safe.**
- 🎯 **The binding constraint is retained paying accounts (METRICS.md N1a), not COGS.** Everything
  in this document should be read against it.
- ⚠️ **Two caveats that could still move the numbers:** the ledger currently omits ~30% of true
  COGS (ffmpeg/Cloud Run compute is metered nowhere — CLAUDE.md rule 6, closed in PLAN.md Phase 3),
  and the figures cover **one** content category so far (PLAN.md Phase 0.2 closes the other two).

## 7. Go-to-market

> **Rewritten 2026-07-14.** The previous plan opened with *"Design partners first — 3–5 companies
> from the founder's network."* **That is outreach, and the founder has ruled it out (no time).**
> The motion is **self-serve / inbound**. → **DISTRIBUTION.md is authoritative**; this is the summary.

**The consequence, stated plainly:** with no outreach, **traffic becomes the long pole and the
top risk (A5)** — every other question is only *reachable* through it. **Sequencing reversed
2026-07-15:** the MVP ships first and everything launches at once (PLAN.md Phase 5), so the first
one-shot wave of posts lands on a live tool rather than an email box; the build is held to the
hard ship-by gate (METRICS.md §2.2 SB). → DISTRIBUTION.md for the reasoning.

1. **The product is the marketing.** Free public YouTube tool → paste a URL, get real Markdown in
   60 s, no signup, no trust required. It replaces *"upload your confidential internal recording
   to a stranger's website"* as the first ask — which was the biggest leak in the old funnel.
   Then: *"now try it on your own recording"* → 2 h free → one Stripe link.
2. **Curated public gallery** — 10–25 processed CC-licensed talks; a compounding SEO asset and a
   permanent live demo. **Curated, attributed — never a scaled transcript farm** (CLAUDE.md r8).
3. **Developer-led content:** honest technical blog (chunking, cost engineering, GDPR
   architecture — *sell the boring hard parts*), public API docs, llms.txt, MCP server later.
   Channels: **LinkedIn first** (founder's existing .NET/architecture audience — cheapest by an
   order of magnitude; *broadcasting is not outreach*), then HN, r/RAG, r/LocalLLaMA.
4. **Compliance-led landing page:** /security and /gdpr as collateral for the buyer's DPO.
   **Gated on A1** — if the EU headline arm loses the A/B, this stops being the lead message.
5. **Later:** connectors marketplace as the expansion wedge; self-hosted enterprise edition on top.

## 8. Key risks & mitigations

> **The five open assumptions (A1–A5), their evidence, and their decision rules live in
> METRICS.md §2 — not here.** Every high-importance / weak-evidence risk is **Value** or
> **Business viability**. Feasibility — the axis a technical founder de-risks first — is the one
> quadrant already closed. **A5 (distribution) is the top risk, and it gates the other four.**

Risks *not* on the assumption list, and how we answer them:

| Risk | Mitigation |
|---|---|
| **Gemini/OpenAI make DIY trivial** ("just send the video to the model") | Value shifts to the pipeline: chunking long video without losing boundary context, cost control, a consistent schema across hundreds of files, batch reliability, compliance packaging. **Sell the boring hard parts.** ⚠️ **The sharp edge:** ICP #1 and #3 are dev teams building AI assistants — *the audience best equipped to build this themselves.* This is A2, and only the Stripe checkout settles it. |
| **Cloudglue moves into the EU** | Speed, the EU-owned-infra roadmap, and a no-lock-in file model they won't copy — it would undermine their own platform play. |
| **Output quality disappoints on messy real-world video** | ⚠️ **This mitigation is now weaker than it was.** It used to read *"the design-partner phase tunes prompts on real corporate footage before launch"* — **there is no design-partner phase; outreach is ruled out.** What remains: the public CC corpus (PLAN.md Phase 0.2) tunes against three content categories, and `upload_repeat` (METRICS.md A4) detects failure *after* launch rather than before it. **We now find out in production. Accept that consciously.** |
| **Vertex model pricing/availability changes** | Provider abstraction from day one (ARCHITECTURE.md); Mistral (EU) as the planned second backend. |
| **Buyer confusion: "is this a transcription tool?"** | All messaging anchors on *agents / knowledge bases* — **never on "transcription"**. The demo shows a RAG answering a question that is only answerable from on-screen content. |

## 9. Success criteria

**→ METRICS.md.** Two milestones, 20× apart, and **only the first one is the plan:**

- **🎯 N1a — survival:** a handful of retained paying accounts, covering infra. **The founder needs
  no salary from this near-term**, so this is the near-term target and the whole plan steers by it.
- **🏁 N1b — job replacement:** the *destination*. **Do not plan against it** — doing so implies an
  order of magnitude more traffic and makes the top risk look unwinnable for reasons that come from
  the salary line, not from the market.

Pass/fail rules for A1–A5 are METRICS.md §2; none may be invoked below its sample floor (§2.1); and
**A5's kill criterion is a calendar deadline (§2.2), because with no salary to burn, nothing else
forces a stop.**

Nothing else is a success criterion. **Traffic, signups and enthusiasm are not success** — they are
inputs to A5 and A2, and METRICS.md §6 lists them as anti-metrics for a reason.

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
