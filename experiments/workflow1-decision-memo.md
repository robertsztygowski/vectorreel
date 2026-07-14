# vectorreel — Workflow 1 Decision Memo

> **Date:** 2026-07-14 · **Question:** Should I build this at all?
> **Inputs:** assumption-mapping (VUBF) → before-you-build (pre-mortem) → startup-analyst
> (TAM/SAM/SOM + unit economics) → ux-researcher (validation design).
> Grounded in `experiments/001-gemini-video-benchmark/` (real Vertex spend, measured today).
> Point-in-time memo — not a living doc.

---

## Riskiest Assumptions

Every High-importance / Weak-evidence assumption is **Value** or **Business viability**.
Feasibility — the axis a technical founder de-risks first — is the one already closed.

| # | Assumption | VUBF | Evidence | Why it's the risk |
|---|---|---|---|---|
| **A1** | EU data residency is a **purchase driver**, not a checkbox | Value | **Weak** | The whole differentiator, squeezed from both sides: a serious DPO notes GCP is US-controlled (CLOUD Act) and may reject "EU region" outright; a buyer who doesn't care won't pay a premium over Cloudglue. The wedge lives in a narrow band nobody has confirmed exists. |
| **A2** | Buyers will **buy rather than DIY** | Value | **Weak** | Gemini ingests native video. The ICP is dev teams building AI assistants — the audience best equipped to build this themselves in a weekend. Selling a wrapper to wrapper-builders. |
| **A3** | Usage is recurring **flow**, not one-time **backfill** | Business | **None** | Unasked, and it decides whether this is a SaaS at all. Archive conversion = one €690 month, then collapse. |
| **A4** | Output is **citable** (accurate) in a knowledge base | Value | Medium | Benchmarked on *one* content type. A hallucinated timestamp poisons a KB — wrong is worse than absent, and trust doesn't return. |
| **A5** | CAC low enough for a €149–690/mo ACV | Business | **Weak** | Pro contributes ~€131/mo. CAC >€800 breaks payback; outbound sales is arithmetically impossible. Requires a self-serve funnel that doesn't exist. |
| ~~A8~~ | ~~EU model availability + COGS < €1.50/h~~ | Feasibility | **STRONG** | **Validated 2026-07-14.** Monitor, don't test. |

---

## Verdict

# 🟡 RESHAPE

**High product risk / low engineering risk.**

Not *DON'T* — the build is cheap, largely de-risked, and the missing evidence can only be
obtained by putting real output in front of real buyers. Not *BUILD* — the thing about to be
built (tiered self-serve SaaS: billing, plans, connectors, dashboard) rests on three
assumptions with **zero customer evidence**.

**The three reasons:**

1. **The wrong axis was de-risked — excellently.** Feasibility and margin are *measured*.
   Demand, pricing and retention have not one data point: no interview, no LOI, no euro.
2. **The differentiator is the weakest-evidenced and most attackable claim.** "EU residency"
   on GCP is precisely what a competent DPO takes apart — BUSINESS_MODEL.md §4 already
   concedes it. If EU is hygiene rather than a driver, vectorreel is a feature-equivalent
   Cloudglue with worse distribution.
3. **Backfill-vs-flow (A3) is unasked and silently determines the business model.**
   Subscription pricing on a one-time archive job is a category error engineering cannot fix.

**Reshape from** *"EU-residency video-to-Markdown SaaS with self-serve tiers"*
**to** *"design-partner-funded pipeline, sold by hand, priced after evidence."*

**Delay:** billing, plans/tiers, connectors, dashboard, MCP server, self-serve onboarding,
`/gdpr` DPA flow. All downstream of answers not yet held.

---

## Market & Unit Economics

### Sizing (bottom-up; no top-down market-report figures — they're noise at this wedge)

| | Accounts | @ €2,400 blended ACV |
|---|---|---|
| **TAM** — EU orgs ≥50 staff, with internal video, with an active AI/RAG initiative | 27,000 | **€65M/yr** (range €50–110M) |
| **SAM** — beachhead PL + Nordics + DACH (~30%) | 8,100 | **€19M/yr** |
| **SAM (narrow)** — where EU residency is an actual *decision driver* (~40%) | 3,200 | **€7.8M/yr** |
| **SOM** — 3yr, founder-led, no sales team | 125–200 | **€300–500k ARR** |

*Inputs (all assumptions, all to verify):* 450k EU orgs ≥50 employees (**verify vs Eurostat
SBS**) × 40% with meaningful internal video × 15% with an active AI/RAG initiative by 2027.
ACV set at €2,400 — deliberately **below** list price, because of A3.

> **A €65M TAM is a good bootstrapped business and a bad venture business.** Fund this with
> customers, not a seed round.

### Unit economics — LLM cost/video-min vs. price/video-min

| Component | €/video-hour | €/video-min | |
|---|---|---|---|
| Stage B, blended (67% routed to cheap config) | 0.261 | 0.0044 | measured |
| × 1.3 retry overhead | 0.340 | 0.0057 | measured |
| Stage C fusion | 0.110 | 0.0018 | measured |
| **LLM subtotal** | **0.45** | **0.0075** | **measured** |
| ffmpeg transcode/segment (Cloud Run) | 0.15 | 0.0025 | ⚠️ **estimate** |
| GCS transient + Postgres/orchestration | 0.05 | 0.0008 | ⚠️ **estimate** |
| **All-in COGS** | **≈ €0.65** | **≈ €0.011** | |

> ⚠️ The stated framing (*"unit economics = LLM cost/video-min vs. price"*) **omits ~30% of
> COGS.** ffmpeg transcoding is real money and is absent from the ledger. Add it in Phase 4.

### 🚩 Break-even price per video-minute

### **€0.011 / video-minute** (€0.65/video-hour) — all-in
### €0.0075 / video-minute (€0.45/video-hour) — LLM-only

| Price point | €/video-min | Gross margin |
|---|---|---|
| API PAYG €0.15/min | 0.150 | 92.7% |
| Pro overage (€8/h) | 0.133 | 91.8% |
| Pro €149 @ full 25h | 0.099 | 89.0% |
| Business overage (€6/h) | 0.100 | 89.0% |
| **Business €690 @ full 150h** (worst case) | **0.077** | **85.7%** |

**The lowest realizable price sits 7× above break-even. Gemini pricing would need to rise ~6×
before the cheapest tier stops earning.** COGS is *solved*. Further cost engineering optimizes
the one variable already safe.

**The number that actually binds:** ~€176/mo contribution per retained account →
**≈47 retained paying accounts** covers ~€300/mo fixed infra + an €8,000/mo founder salary.
That is the finish line, not COGS.

**The real unit-economics risk is A3.** A customer who backfills 200h (one €690 month) then
flows 8h/mo settles at ~€72–149/mo. **Steady-state ARPA is plausibly 3–5× below
acquisition-month ARPA.** Any ARR projection built on list prices is fiction until A3 is answered.

---

## How We'll Validate

**Primary — Manual Concierge Test (no product built).** `experiments/001/` already produces a
complete `output.md` for a 50-min video. *That is the product for this test.*

1. Recruit 5 companies from the Denmark/Poland network. Ask for **one real internal recording**.
2. Run it by hand through the existing Python scripts. **~€0.40 Vertex spend/video (~€5 total).**
3. Hand back the Markdown. They drop it into **their own** RAG and ask 5 questions they care
   about — at least one answerable *only* from on-screen content.
4. **Then ask for money: "€200 to process your next 20 hours."**

| Result | Verdict |
|---|---|
| ≥2 of 5 prepay | **A2 validated** → build the thin slice |
| Enthusiasm, no payment | **A2 invalidated** → vitamin, not painkiller |
| "We'll just do it ourselves" | **A2 dead** → DIY gap too small |

**Isolating A1 — cold-outreach A/B (~zero cost, runs alongside).**
Arm A leads *"EU-only processing — your recordings never leave Europe."*
Arm B leads *"your AI assistant can't see what's on screen in your recordings."*
~100 sends/arm; directional, not significant.
**If Arm A does not clearly beat Arm B, EU is a checkbox, not a wedge — and the entire
positioning must move to the capability story.**

**Interview script (8–10 people; past-behaviour anchored, never pitch, never describe the product):**

1. *"Walk me through the last time someone needed information that only existed in a recording.
   What did they actually do?"* → pain + workaround. Kill: "they asked a colleague."
2. *"What's in your AI assistant's knowledge base today, and who did the work to get each source in?"*
   → is video even missed?
3. *"Has anything ever been blocked from, or pulled out of, that knowledge base — and why?"*
   → **the A1 test, unprompted.** If GDPR/DPO doesn't surface on its own, EU is hygiene.
4. *"Last time you onboarded an AI vendor touching your data — what did procurement/your DPO ask?
   What made a vendor easy or hard to approve?"* → was "US processor" ever an actual blocker?
5. *"If someone had to make your last 20 recordings searchable next week — who'd do it, how long,
   and out of what budget?"* → **the A2 test**, plus archive-size vs. monthly-flow for **A3**.

---

## Recommendation

**Proceed — but stop building the SaaS and start selling the pipeline by hand.**

The engineering instinct here has been excellent and is now producing diminishing returns:
COGS is measured at 7× headroom and further optimization is procrastination in a lab coat.
Every remaining risk sits on the far side of a conversation not yet had.

**Next 2 weeks — in order:**

1. **Run the concierge test.** 5 recordings, ~€5 of Vertex, hand-delivered Markdown, ending in
   a request for €200. *This is the whole memo.*
2. **Launch the outreach A/B** to settle whether EU is the wedge or a checkbox.
3. **Ask every conversation the A3 question** — archive hours vs. hours recorded per month.
4. **Do not write product code** until ≥2 of 5 prepay.

**Then, and only then:** build the thinnest slice that lets a design partner run their own
video through it — upload → pipeline → Markdown out. No billing, no tiers, no connectors.

**Kill criteria — the honest version.** If, after 10 interviews and 5 concierge deliveries:
nobody pays, GDPR never surfaces unprompted, and the answer to Q5 is consistently *"we'd just
pipe it to Gemini ourselves"* — **stop.** The €65M TAM does not justify pushing uphill against
a DIY alternative your own customers are qualified to build.

---

### Cheap-to-fix items surfaced along the way (independent of the verdict)

- **~8% degenerate-generation rate** in the benchmark ledger: `seg4_configA` (474s, 61k output
  tokens) and `seg2_configB` (323s, 63k thinking tokens) ran away — the source of the 1.3×
  retry overhead. Add hard output-token caps + timeouts before it meets the <15min SLO.
- **ffmpeg/Cloud Run compute is missing from the cost ledger** (~30% of true COGS). CLAUDE.md
  rule 6 says every LLM call is metered — extend that to the transcode step.
- **Two content categories still unbenchmarked** (slide-talk, talking-head) — A4 is only
  partially evidenced. PLAN.md already flags this.
