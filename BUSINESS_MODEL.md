# Business Model — AI-Ready Knowledge Repositories from Video

> Working name: mdreel
> Status: pre-launch MVP. This document captures validated decisions and assumptions to guide implementation.

## 1. One-liner

**Turn your organization's video — demos, trainings, talks — into an AI-ready knowledge repository: hundreds of hours, organized and timestamped, that your AI assistant (and your team) can explore and cite. Processed entirely in the EU.**

> 🔄 **Pivot 2026-07-20 (founder-directed): outcome-first.** mdreel is no longer pitched as a
> *video → Markdown converter* — that sentence sells the mechanism. It is pitched as what the buyer
> ends up **owning**: an **AI-ready repository** of their video knowledge — sessions, topics,
> speakers, timeline, metadata — with **research-ready** navigation for humans and citable
> structure for agents. **Markdown is the implementation detail; repository-level usability is the
> product surface.** Nothing about the pipeline, ingestion model, or architecture changed
> (ARCHITECTURE.md is untouched by this pivot); what changed is the packaging, the narrative, and
> the demo (**public collections** — DISTRIBUTION.md). The per-file output contract (ARCHITECTURE
> §4) remains frozen; the repository contract that organizes those files is additive
> (ARCHITECTURE.md).

## 2. Problem

Companies sit on hours of high-value **asset video**: internal trainings, product demos, onboarding sessions, recorded walkthroughs, sprint reviews, conference talks. They are building AI knowledge bases and agents (RAG, MCP-connected assistants), but:

> ⚠️ **Asset video, not meetings — this is a hard scope line, set by competitor analysis
> (experiments/002-competitor-analysis, 2026-07-15).** The *meeting-recap* job (Teams/Zoom call
> summaries) is owned outright by the bundled suites already in the buyer's tenant (§8, threat #1).
> We do not fight there. Our ground is the video those tools never touch — demos, trainings,
> walkthroughs, talks — turned into a document an agent can cite. **Never say "meeting notes."**


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

**What the customer accumulates — and what we actually sell (pivot 2026-07-20) — is the
repository those documents form:** an **AI-ready repository** with a stable, documented layout —
sessions, topic and speaker indexes, a timeline, machine-readable metadata (the repository
contract, ARCHITECTURE.md). One video gives you a document; a hundred give you a **research-ready
library** your assistant can navigate and cite across sessions. The schema-consistency bullet
above is what makes this possible: repository-level answers ("every time this API was demoed,
across three years of recordings") only exist because every file is shaped the same.

Delivery: web UI for manual use + first-class REST API (and later an MCP server) for pipeline integration. Output is plain files — **no proprietary retrieval stack, no lock-in**. Customers own the repository and put it in their own Git repo, S3/GCS, Obsidian, SharePoint, or vector DB.

## 4. Positioning & differentiation

### The positioning statement

> **For EU engineering teams building an internal AI assistant, mdreel turns the recordings
> your assistant can't see — demos, trainings, incident reviews — into an AI-ready knowledge
> repository it can explore and cite, without the footage ever leaving the EU.**
>
> *(Pivoted 2026-07-20 — outcome-first, §1. The pre-pivot form ended "…into timestamped Markdown
> it can cite"; the mechanism now stays out of the headline.)*

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
| Output model | Collections + their retrieval/chat stack | A portable repository of plain Markdown files (documented layout — ARCHITECTURE.md repository contract), bring-your-own-RAG, zero lock-in |
| Source retention | Stores video library | **Source video deleted after processing by default** (configurable) |
| Buyer message | Developer platform | "Feed your videos to your agents without leaving the EU" |

> **Trust/compliance surface is live (2026-07-18):** the "EU-focused DPA, short subprocessor list,
> honest sovereignty roadmap" row above is shipped as the public legal pack at `mdreel.com/legal/*`
> — Terms, Privacy, Imprint, DPA (GDPR Art. 28, incorporated by reference), Subprocessors,
> Acceptable Use. Source in `web/app/legal/`; research trail in
> `experiments/legal/2026-07-18-legal-pack-research.md`; Polish-lawyer review is a PLAN.md
> NEEDS-FOUNDER item.

**What is and isn't the wedge (competitor-informed 2026-07-15 — experiments/002-competitor-analysis):**

- ❌ **On-screen-text OCR is table-stakes, not a differentiator.** Cloudglue (Scene Text
  Recognition), Twelve Labs, Mixpeek and Azure AI Video Indexer all read on-screen text — and
  Azure does it EU-hosted at no premium. *"We read your slides"* invites *"so does Cloudglue."*
  **Never lead with the OCR capability.**
- ✅ **The wedge is the deliverable + the combination nobody else sells:** an **AI-ready
  repository you own** — portable, timestamped Markdown that separates *spoken* from *shown*, is
  schema-consistent across hundreds of files, organized by session/topic/speaker/timeline
  (the repository contract, ARCHITECTURE.md), carries **zero retrieval lock-in**, and is processed
  entirely in the EU with no human-in-the-loop and no non-EEA transfer. Every rival traps the same
  raw signals inside a proprietary collections/search index — a platform with gravity, not a
  repository you own. **Sell the repository** (pivot 2026-07-20, §1 — the pre-pivot form of this
  rule was "sell the artifact"; the repository *is* the artifact at library scale).
- 🛡️ **The one *durable* moat is no lock-in.** EU residency is perishable (a competitor is one
  `europe-west3` deploy from erasing it — §8) and, per the market, unpriceable (no one charges an EU
  premium; residency is a DPO *deal-unblocker*, not a markup). The portable-file / bring-your-own-RAG
  model is the one thing a platform competitor **structurally will not copy**, because it undermines
  their own retrieval revenue. **In the direct ring, lead with no lock-in; second the EU residency as
  the DPO's reason to say yes.**

Honesty rule for all marketing/compliance copy: current stack is **EU data residency on GCP (Google DPA, Vertex AI no-training terms, EU-regional processing)** — *not* full EU sovereignty (Google is a US company subject to CLOUD Act). State this plainly on the /gdpr page and publish a roadmap item: "EU-owned infrastructure option" (OVHcloud/Scaleway) and later a self-hosted enterprise edition. Honesty here is a sales asset; overclaiming gets destroyed by any competent DPO. **Competitor
analysis confirmed the sales-asset half is live, not theoretical:** EU-hosted incumbents' residency
claims *leak* under inspection — Amberscript ships files to non-EEA transcribers and its Summary
feature to a US LLM (experiments/002-competitor-analysis). An honest, non-leaking EU story is itself
a differentiator.

**Positioning lines per competitor ring (competitor-informed 2026-07-15 — the copy the mockup and
launch page must use):**

- **vs Cloudglue / Twelve Labs (structured-video, direct):** *"The same structured video
  understanding — but as an AI-ready repository your agent owns: portable Markdown, processed
  entirely in the EU, with no retrieval stack to lock you in."* (Match their capability; beat their
  lock-in and their DPO story.)
- **vs Amberscript / Happy Scribe (EU transcription):** *"Not a transcript — a knowledge repository
  that also captures what's on screen, with a residency story that doesn't leak to a US LLM or an
  offshore transcriber."* (Never compete on €/hour here.)
- **vs Gemini / Deepgram / AssemblyAI (infra / DIY):** *"You could wire this yourself; here's the
  pipeline you'd rebuild badly — boundary-safe chunking, runaway-generation guards, one consistent
  schema across every file, compliance packaged."* (Sell the boring hard parts.)
- **vs Copilot / Zoom / Meet (bundled substitutes):** *"Those summarize your meetings. This turns your
  demos, trainings and walkthroughs — the asset video they never touch — into something your AI
  assistant can cite."* (Change the job; never say "meeting notes.")

## 5. Target customers (initial ICP)

1. **Software companies / IT departments (50–500 employees) building internal AI assistants** — have demo libraries, training/onboarding recordings, recorded walkthroughs and conference talks (**asset video — not meeting recordings**, §2); have an "AI adoption" mandate; developer buyer, uses the API.
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

**The MVP ships exactly two plans plus a trial credit (decided 2026-07-15). No free tier, no free
tool.** The small plan is **hard-capped** (processing pauses at the limit); the larger one meters
overage past its cap.

| Plan | Price | Included | Notes |
|---|---|---|---|
| Trial credit | €0 | **1 h one-time at signup (METRICS.md N33)**, full quality | No credit card. Replaces the old "2 h free" trial. Must produce a "wow" on the customer's own video. |
| **Pro** *(MVP — the small plan)* | €149/mo | 25 h/mo, UI + API, webhooks, MCP | **Hard cap — no overage** (2026-07-15): processing pauses at the limit; upgrade to continue. Target: single team, L&D dept. |
| **Business** *(MVP — the larger plan)* | €690/mo | 150 h/mo, priority support | Overage: **€6/h** past the cap. Connectors (Drive/SharePoint/Zoom) and DPA self-service stay post-MVP. |
| API pay-as-you-go (later) | €0.15/min (€9/h) | No subscription | Not in MVP. For developers embedding the product in pipelines; stickiest revenue. |
| Enterprise (later) | Annual contract | Custom volume, SSO, audit log, self-hosted option | Not in MVP scope. |

> ⚠️ **Prices above are hypotheses and are now explicitly gated on the A3 finding** (see §8).
> If usage turns out to be one-time **backfill** rather than recurring **flow**, subscription
> tiers are the wrong instrument entirely and this table becomes a **prepaid credit pack**
> (*"€200 for 20 hours"*). Do not lock these prices until PLAN.md Phase 6. The MVP ships only the
> **two plans marked above + the N33 trial credit** (PLAN.md Phase 4), not the full table.

### The on-ramp canyon — and the €99 Starter fallback (competitor-informed 2026-07-15)

**Competitor analysis (experiments/002-competitor-analysis) surfaced our single weakest point vs the
field: the on-ramp.** Every rival's first *paid* step is a low, impulse-shaped buy — Happy Scribe
€8.50, Cloudglue Mini $15, Amberscript €19. Ours is **€149/mo**, reached only after a one-time trial
credit (N33). Cloudglue fills that gap with $15/$45 stepping-stones; we have a canyon between "1 h
free" and "€149/mo."

- **Launch default: hold the line at the two plans above (Pro / Business).** Per-hour sits inside
  the *structured-video* band the competitors price in (Cloudglue $4.80–7.50/h), and Pro carries the
  highest contribution → the highest CAC ceiling (METRICS.md N23), which matters because paid search
  is the A1 *instrument* (DISTRIBUTION.md).
- **Pre-build a €99/mo hard-capped Starter as a one-switch fallback, kept dark.** Flip to it *only*
  if the launch shows the €149 step throttling checkout — trial→checkout below the A2 floor
  (METRICS.md), or "too expensive" dominating inbound feedback (METRICS.md N21/N22).
- **Do NOT drop to a €59 impulse tier.** A DPO-gated B2B purchase is not a $15 impulse: €59 collapses
  the CAC ceiling (METRICS.md N23), invites bill-shopping against €2–4/h transcription, and starves
  the paid-search instrument. Modeled and rejected.

**Credit-pack pivot is de-risked, not a downgrade.** If cohort hour-decay says *backfill* (A3), the
prepaid-pack shape (*"€200 for 20 h"* above) is the **category-normal** form: Cloudglue's entire
ladder is already prepaid credit packs (*"subscriptions? contact us"*), and Amberscript / Happy
Scribe both sell top-up credits. Keep the packs pre-designed but dark; deploy only on the A3 signal.

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

1. **Public collections are the demo.** *(Pivoted 2026-07-20 — the "curated gallery" grew into
   **public collections**: the same curated, CC-licensed, attributed talks, now organized as
   full AI-ready repositories — sessions, topics, speakers, timeline — instead of a flat list of
   pages. A collection is the product surface itself, demonstrated in public.)* *(Earlier revision
   2026-07-15 — the free public YouTube tool was dropped: an open compute endpoint is a bot/abuse
   surface a fixed base this small cannot carry.)* Collections do the funnel job at zero compute
   per visitor: a skeptic explores a real repository built from talks they already know, no
   signup, no trust required — which still avoids *"upload your confidential internal recording
   to a stranger's website"* as the first ask. Then: *"get this for your own recordings"* →
   trial credit (METRICS.md N33) → two-plan checkout (§6). **The consume→convert hop is an
   explicit, instrumented product path** *(M7, 2026-07-21)*: every collection surface carries
   the same "build your own repository" CTA, and the hop is measured first-party only —
   `collection_session_view` → `collection_convert_click` → `signup_view` (self-hosted Umami,
   CLAUDE.md r10; source attribution via the UTM playbook in DISTRIBUTION.md).
2. **Curated public collections** — 10–25 processed CC-licensed talks organized into 3+ themed
   collections (DISTRIBUTION.md owns the list); a compounding SEO asset and a permanent live demo.
   **Curated, attributed — never a scaled transcript farm** (CLAUDE.md r8).
3. **Developer-led content:** honest technical blog (chunking, cost engineering, GDPR
   architecture — *sell the boring hard parts*), public API docs, llms.txt, MCP server later.
   Channels: **LinkedIn first** (founder's existing .NET/architecture audience — cheapest by an
   order of magnitude; *broadcasting is not outreach*), then HN, r/RAG, r/LocalLLaMA.
4. **Compliance-led landing page:** /security and /gdpr as collateral for the buyer's DPO.
   **Gated on A1** — if the EU headline arm loses the A/B, this stops being the lead message.
5. **Later:** connectors marketplace as the expansion wedge; self-hosted enterprise edition on top.

> ### 🪞 The asymmetry that makes publishing safe *(founder decision, 2026-07-21)*
>
> **The public collection shows the shape, and the shape is only complete on video you own.**
> Public artifacts **demonstrate**; private repositories **monetize**. The subscription buys access
> to the factory — **never** access to the published collections. Publishing is therefore a
> *distribution* model here, not a revenue one, and it must never drift into a media business
> (sponsorship and audience revenue are explicitly rejected).
>
> Consequence for where things live: **mdreel.com is the destination and GitHub is an export
> target.** The site is where the funnel is instrumented — a GitHub repo cannot carry Umami, a
> `collection_convert_click`, or any funnel at all — and distribution is the top risk. GitHub earns
> its place as **portability proof**: clone it, open it in Cursor or Claude Code, it is yours, no
> walled garden. That matters to a docs-as-code ICP, and it is worth nothing to A5.
>
> And the promise is **checkable** answers, never *correct* ones: every claim carries a timestamp
> into footage the reader can open. Correctness is A4, it is open, and METRICS.md **N32** says where
> it is weakest. Keep the two sentences separate and concrete — **the repository is the artifact;
> checkable answers are what it gives you.**

## 8. Key risks & mitigations

> **The five open assumptions (A1–A5), their evidence, and their decision rules live in
> METRICS.md §2 — not here.** Every high-importance / weak-evidence risk is **Value** or
> **Business viability**. Feasibility — the axis a technical founder de-risks first — is the one
> quadrant already closed. **A5 (distribution) is the top risk, and it gates the other four.**

Risks *not* on the assumption list, and how we answer them:

> **Threat ranking was reset by competitor analysis (experiments/002-competitor-analysis,
> 2026-07-15):** the #1 competitor is not another video-to-Markdown tool — it is the **bundled
> meeting-recap already in the buyer's tenant.** The rows below are ordered by that finding.

| Risk | Mitigation |
|---|---|
| **🥇 The bundled meeting-recap is the true default** — M365 Copilot / Teams Premium, Zoom AI Companion, Google Meet "take notes for me" are already in the tenant, already DPO-approved, at ~zero marginal cost, and own the *meeting-notes* job completely. | **Do not fight there. Change the job.** Anchor every surface on **asset video** — demos, trainings, onboarding, walkthroughs, talks — the video those tools never touch, turned into an **agent-citable document with verbatim on-screen text**. **Never say "meeting notes"** (§2). The moment the copy drifts to meetings, we lose to an $18/seat incumbent inside the account. |
| **Gemini/OpenAI make DIY trivial** ("just send the video to the model") | Value shifts to the pipeline: chunking long video without losing boundary context, cost control, a consistent schema across hundreds of files, batch reliability, compliance packaging. **Sell the boring hard parts.** ⚠️ **The sharp edge:** ICP #1 and #3 are dev teams building AI assistants — *the audience best equipped to build this themselves.* This is A2, and only the Stripe checkout settles it. |
| **Cloudglue moves into the EU** | Cloudglue is **one `europe-west3` deploy from erasing the residency edge**, and already out-executes us on distribution (MCP + live Playground + $15 on-ramp). So residency is **not** the durable answer: **lead with the no-lock-in portable-file model they structurally won't copy** (it undermines their own platform play), ship the EU-owned-infra roadmap (§4), and close the on-ramp gap (the €99 fallback, §6). |
| **On-screen-text OCR is not a unique wedge** — Cloudglue, Twelve Labs, Mixpeek and Azure Video Indexer all read on-screen text; AssemblyAI/Deepgram give EU residency free. | Never market OCR or "EU" *alone* as the differentiator — a technical buyer answers *"so does Cloudglue."* Sell the **combination + the portable artifact** (spoken/shown separated, schema-consistent, no lock-in, clean EU processing); treat residency as a **DPO deal-unblocker, not a markup** (§4). |
| **Output quality disappoints on messy real-world video** | ⚠️ **This mitigation is now weaker than it was.** It used to read *"the design-partner phase tunes prompts on real corporate footage before launch"* — **there is no design-partner phase; outreach is ruled out.** What remains: the public CC corpus (PLAN.md Phase 0.2) tunes against three content categories, and `upload_repeat` (METRICS.md A4) detects failure *after* launch rather than before it. **We now find out in production. Accept that consciously.** |
| **Vertex model pricing/availability changes** | Provider abstraction from day one (ARCHITECTURE.md); Mistral (EU) as the planned second backend. |
| **Buyer confusion: "is this a transcription tool?" — or worse, "is this a meeting-notes tool?"** | All messaging anchors on *agents / knowledge bases* and *asset video* — **never "transcription," never "meeting notes"** (the worse confusion — it walks us straight into threat #1). The demo shows a RAG answering a question that is only answerable from on-screen content. |

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

- **YouTube processing is never a paid feature — and since 2026-07-15 it has no public input box
  either.** The free tool was dropped (abuse/cost surface); the **public collections** (the
  curated gallery, evolved — §7), produced by us, are the only YouTube-facing surface and exist
  for *distribution only* (A5) — see real
  output, zero friction, zero trust required. **The paid product is private recordings.** Two hard
  reasons this is not negotiable:
  (1) Vertex only ingests videos that are public or owned by **our** GCP account, so a customer's
  unlisted/internal recordings can *never* work this way; (2) the moment it becomes a paid tier
  we have changed businesses and walked into the §8 buyer-confusion risk — *"is this a YouTube
  tool?"* is a worse question than *"is this a transcription tool?"*
- No self-hosted edition (documented as roadmap; architecture must not preclude it).
- No connectors (upload + API only).
- **No Terraform, no Cloud Tasks, no connectors** until there is a paying
  customer (PLAN.md "Deferred"). *(Revised 2026-07-15: the two-plan checkout — one hard-capped, one
  with overage — the authenticated panel, and the MCP server moved INTO the MVP; §6 and PLAN.md
  Phases 2R/4 own the details.)* *(Revised 2026-07-18: the launch legal pack ships a **self-serve
  DPA incorporated by reference** into the ToS — see the trust/compliance surface below and PLAN.md.
  A negotiated **click-to-sign / enterprise DPA signature flow** remains deferred until a customer
  demands it.)* Cloud SQL still enters only when the events store demands it —
  it idles at ~€25–50/mo against a ~€300/mo fixed base.
- No built-in chat/RAG/search over processed videos — we produce files, we do not compete with the
  customer's retrieval stack. *(Clarified by the 2026-07-20 pivot: the repository's static indexes
  — topics, speakers, timeline — are files inside the deliverable, not a hosted retrieval product.
  A hosted chat/search surface remains a non-goal.)*
- No fine-tuning, no training on customer data — ever, contractually.
- No SOC 2 / ISO 27001 certification yet — design for auditability, certify when a customer demands it.
