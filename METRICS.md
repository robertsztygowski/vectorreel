# METRICS — every load-bearing number, and what it makes us do

> **Living doc. This file is the single source of truth for:**
> **(a) every load-bearing number in the business, (b) the five open assumptions, (c) the decision
> rule attached to each.**
>
> 🚨 **No other document may restate a value from §1 or a rule from §2.** Reference them by name and
> cite this file — *"below the break-even price (METRICS.md N6)"*. `scripts/check-docs.sh` enforces
> it. **Why:** on 2026-07-14 a superseded memo contradicted four living docs because the same
> numbers had been copied into all of them and only some were updated. **A number that lives in
> five files has five chances to be wrong and one chance to be right.**
>
> Companions: DISTRIBUTION.md (the funnel that *derives* the traffic numbers) · PLAN.md (the phases
> that *produce* them). Created 2026-07-14 from `experiments/agents/out/w1-decision-memo.md`.

---

## 1. The numbers registry

**`measured`** = we ran it and have the receipt. **`assumed`** = an industry-typical guess standing
in for evidence — replace it the moment there is data, and **never make an irreversible decision on
one.** **`derived`** = computed from the rows above it.

### 1.1 The goal — **two milestones, and they are 20× apart**

The founder **does not need a salary from this in the near term** (decided 2026-07-14). That single
fact changes the shape of the business more than any engineering decision in this repo, so it is
recorded as an explicit input (N3) rather than left implicit.

| # | Number | Value | Status | Meaning |
|---|---|---|---|---|
| **N1a** | **🎯 SURVIVAL — the goal the plan steers by** | **≈ 2–3 retained paying accounts** | derived | N2 ÷ N0. Covers infra. **At N1a the business is cash-flow-positive and can compound indefinitely — there is no burn clock.** |
| **N1b** | **🏁 JOB REPLACEMENT — the destination** | **≈ 63 retained paying accounts** | derived | (N2 + N3) ÷ N0. **Not the near-term target. Do not plan against it.** |
| **N0** | **Contribution per retained account** | **≈ €131/mo** | **assumed — conservative** | **Pinned 2026-07-14.** Pro (€149/mo) at full utilization, minus COGS. **The multiplier behind N1a, N1b and every CAC figure in §1.6 — so it is the highest-leverage assumption in this file.** Deliberately the *conservative* end: if the business works at N0 it works at any realistic blend. ⚠️ *A €176 blended figure previously circulated; it had been reverse-engineered to make N1b land on 47, which is backwards. Retired.* |
| N2 | Fixed infra cost | ≈ €300/mo | assumed | The base burn to beat. Why Cloud SQL (~€25–50/mo idling at zero users) is deferred. |
| N3 | Founder salary target | €8,000/mo | given · **deferred** | **Not required in the near term.** It is ~96% of N1b — i.e. **this one input generates the entire difference between N1a and N1b, and between a business that needs a content engine and one that needs a good month.** |

> 🚩 **The trap this replaces.** Planning against N1b implied needing **tens of thousands** of
> cumulative visitors (N15b) — a content-engine-sized number that made A5 look nearly unwinnable.
> **That was an artifact of the salary line, not of the market.** Steering by N1a, the traffic
> required to *survive* is roughly the same as the traffic required to *learn* (§1.4) — which is a
> far better place to stand.
>
> ⚠️ **But nothing about the evidence gets cheaper.** The sample floors in §2.1 are set by
> statistics, not by the cost base. **Removing the salary lowers the bar for getting paid; it does
> not lower the bar for knowing whether this works.**

### 1.2 Unit economics — **measured 2026-07-14**

`experiments/001`, `gemini-2.5-flash` @ `europe-central2`, on a real 50-min demo screen-recording.

| # | Component | €/video-hour | €/video-min | Status |
|---|---|---|---|---|
| | Stage B, blended (Stage A routes ~67% static content to the cheap config) | 0.261 | 0.0044 | measured |
| | × 1.3 retry overhead (caused by N7) | 0.340 | 0.0057 | measured |
| | Stage C fusion | 0.110 | 0.0018 | measured |
| **N4** | **LLM subtotal** | **0.45** | **0.0075** | **measured** |
| N5 | ffmpeg transcode/segmentation on Cloud Run | 0.15 | 0.0025 | ⚠️ **estimate — not yet metered** |
| | GCS transient + orchestration | 0.05 | 0.0008 | ⚠️ estimate |
| **N6** | **🚩 All-in COGS = the break-even price** | **≈ 0.65** | **≈ 0.011** | measured + estimate |

**N6 is the break-even price.** The lowest realizable list price (Business at full utilization,
€0.077/video-min → 85.7% margin) sits **~7× above it.** Gemini pricing would have to rise ~6×
before the cheapest tier stops earning.

> ✅ **COGS is solved and is not a risk.** These figures exist to catch *regressions* (N8), not to
> justify optimization work. **N1 is the constraint. Guard the margin; stop chasing it.**
>
> ⚠️ **N5 is an estimate, and it is ~30% of true COGS.** The ledger meters LLM calls and *not*
> compute. Until PLAN.md Phase 2 closes this, every figure here is optimistic by roughly a third
> (CLAUDE.md rule 6).
>
> ⚠️ **Scope:** one content category only (demo screen-recording). Slide-talk and talking-head are
> closed in PLAN.md Phase 0.2. The **public YouTube path cannot use the static-content lever** (no
> local bytes → no ffmpeg → no static detection), so it costs the full ~€0.45/video-hour — hence
> N10 and its hard abuse caps + result caching.

### 1.3 Operational thresholds — regression guards

| # | Metric | Threshold | Action when breached |
|---|---|---|---|
| **N7** | **Runaway-generation rate** — Stage B calls hitting `maxOutputTokens` / `thinkingBudget` | **> 8%** (measured baseline) | Guards regressed (ARCHITECTURE §3). Source of the 1.3× retry overhead in §1.2. |
| N8 | All-in COGS per video-hour (LLM **+ compute**) | > €1.50 | Guardrail breached — investigate. |
| N9 | Wall-clock per video-hour | > 15 min | SLO breach. Gated on the N7 guards — one unbounded call alone blows this. |
| N10 | Free YouTube tool spend | > €5/day | Abuse. Caps + cache are failing. |
| N11 | Gallery cache-hit rate | < 90% | Gallery pages must cost ≈ €0 on repeat views. |

### 1.4 Funnel — **every rate here is `assumed`. This is the weakest part of the business.**

| # | Step | Rate | Status |
|---|---|---|---|
| N12 | Qualified visitor → trial start | 2–5% | **assumed** — industry-typical |
| N13 | Trial → paid | 5–15% | **assumed** — industry-typical |
| **N14** | **⇒ Qualified visitors per paying customer** | **≈ 400–1,000** | derived (N12 × N13) |
| **N15** | **🎯 ⇒ Cumulative visitors to reach a VERDICT — and, separately, to reach N1a** | **≈ 2,000–5,000** | derived |
| N15b | ⇒ Cumulative visitors to reach N1b (job replacement) | ≈ 25,000–63,000 | derived |

> **🎯 N15 is the number the plan steers by, and it is doing double duty — which is the single most
> useful coincidence in this business.** The traffic needed to reach **N1a (survival)** and the
> traffic needed to reach the **A2 sample floor (~100 trials, §2.1)** land in the *same range*.
> **So the visitors that pay for the infra are the same visitors that tell you whether to
> continue.** One target, two payoffs.
>
> **That is a good-post-sized number, not a content-engine-sized number** — one HN front page, one
> LinkedIn post that lands, or a gallery that quietly ranks over two quarters. Compare N15b, which
> only becomes relevant once N1a is passed and the salary question reopens. **Do not plan against
> N15b.**
>
> ⚠️ **The whole plan is sensitive to N12/N13 and both are guesses.** That is exactly why A5 is the
> top risk — and why the *first* real traffic is worth more as a measurement than as revenue.
> → DISTRIBUTION.md.

### 1.5 Market — bottom-up; **every input an assumption**

| # | | Accounts | @ €2,400 blended ACV |
|---|---|---|---|
| **N16** | **TAM** — EU orgs ≥50 staff, with internal video, with an active AI/RAG initiative | 27,000 | **€65M/yr** |
| N17 | **SAM** — beachhead PL + Nordics + DACH (~30%) | 8,100 | €19M/yr |
| N18 | **SAM (narrow)** — where EU residency is an actual *decision driver* | 3,200 | €7.8M/yr |
| N19 | **SOM** — 3 yr, founder-led, no sales team | 125–200 | €300–500k ARR |

Derivation: *450k EU orgs ≥50 employees* (⚠️ **unverified — check vs Eurostat SBS**) × *40% with
meaningful internal video* × *15% with an active AI/RAG initiative by 2027*. ACV set at €2,400,
deliberately **below** list price, because of **A3**.

> 🚩 **N16: a €65M TAM is a good bootstrapped business and a bad venture business.** Fund this with
> customers, not a seed round. This number should govern how much is spent, how fast, and on what.
>
> **N18 exists only if A1 holds.** If EU is not a purchase driver there is no premium, and we
> compete on features inside N17 against Cloudglue — a materially worse business. That is why the
> A1 experiment is the cheapest high-value test available.

### 1.6 Paid acquisition — the CAC ceiling and what it permits

> **→ DISTRIBUTION.md §Paid for the strategy. This section owns the arithmetic.**

**The ceiling.** A bootstrapper has **no capital to front-load acquisition**, so **payback period
governs, not LTV:CAC** — you cannot spend €400 today against €2,000 arriving over two years if you
do not have the €400. **Pinned rule: CAC payback ≤ 3 months.**

| # | Number | Value | Status |
|---|---|---|---|
| **N23** | **🚩 CAC ceiling** | **≈ €390** per paying customer | derived (3 × N0) |
| N24 | Clicks needed per paying customer | 1 ÷ (N12 × N13) — **83 to 1,000**, see below | derived |
| **N25** | **🚩 Max viable CPC** | **€0.39 – €4.73**, *entirely determined by conversion* | derived (N23 ÷ N24) |

**N24/N25 by scenario — this table is the whole paid-ads strategy:**

| Scenario | visitor→trial | trial→paid | Clicks/customer | **Max viable CPC** |
|---|---|---|---|---|
| Pessimistic | 2% | 5% | 1,000 | **€0.39** — buys nothing |
| **Central (today's assumption)** | 3.5% | 7.5% | 381 | **€1.03** — buys nothing with volume |
| Optimistic | 5% | 15% | 133 | **€2.95** — marginal |
| **If the free YouTube tool works as designed** | **~12%** | 10% | **83** | **€4.73** — *buys real keywords* |

> ### 🚩 The finding: **Phase 3 decides whether paid acquisition is possible at all.**
>
> Affordable CPC swings **12×** across that table, and **conversion is not a market fact — it is a
> product fact.** A landing page with an email box converts cold search traffic at 2–5%. A
> zero-friction tool that hands a stranger real Markdown in 60 seconds, on a video they already
> know, can plausibly convert far higher. **That difference is what moves the affordable CPC from
> €1.03 (buys nothing) to €4.73 (buys real keywords).**
>
> **The free YouTube tool is therefore not merely a top-of-funnel hook. It is the precondition for
> paid acquisition existing as an option.**

> ### ⚠️ The scissors — why paid search cannot be the growth engine *today*
>
> At a **€1.03** max CPC you can only afford long-tail terms (*"video to markdown"*, *"transcript
> for RAG"*). **They are cheap precisely because almost nobody searches them.** A term with ~50
> searches/month yields a fraction of a customer per month — profitable and irrelevant.
>
> **The keywords we can afford have no volume. The keywords with volume we cannot afford.**
> ⇒ **Until N12 is measured and high, paid buys evidence, not customers.**

### 1.7 The ad tranches — episodic, gated, never a standing line item

> 🔁 **The loop that makes this a hard rule: a standing ad budget raises N1a itself.** Every ~€131/mo
> of ongoing ad spend adds **one more account** to the survival threshold. A permanent €300/mo ad
> line **doubles** the number of customers needed merely to survive. **Ads are experiments with a
> stop date — not a subscription.**

| # | Tranche | When | Budget | Buys an answer to | 🚨 Gate to proceed |
|---|---|---|---|---|---|
| **N26** | **T-A — price discovery** | After PLAN.md Phase 0.3 (page + gallery live) | **€300–400, one-time** | Real CPCs on our long-tail terms · cost per email capture · **a clean A1 verdict on *cold* traffic** | — |
| **N27** | **T-B — the decisive read** | After Phase 3 (YouTube tool + signup live) | **€500–800, one-time** | 🚨 **Measured N12.** Does the tool lift conversion into the range where N25 permits real keywords? | T-A found **any** keyword under the N25 line |
| **N28** | **T-C — scale or kill** | After Phase 4 **and** A3 has returned | CAC-driven | Real CAC. Scale or stop. | **A3 = flow, AND measured CAC < N23.** **If A3 = backfill, paid acquisition is dead** — the product becomes a prepaid credit pack sold organically. |

**Hard constraints.**

- **Spend nothing before the landing page *and* gallery exist** — otherwise you are paying for
  traffic with nothing to convert on.
- **Until the Stripe link ships (Phase 4), `trial→paid` (N13) cannot be measured at all.** T-A and
  T-B therefore buy **conversion and A1 — never CAC.** ⚠️ **Do not mistake a good cost-per-signup in
  T-B for a viable CAC.** That error is how a paid budget survives its own disproof.
- **Per-keyword kill rule, mechanical, no deliberation:** if a keyword's cost-per-signup implies a
  CAC above **N23**, pause it that week.

**Why paid is worth funding *now*, despite all of the above:** **N12 and N13 are guesses, and every
traffic number in this file derives from them.** Paid search replaces them with measured values in
~3 weeks; organic takes months. **And paid is a strictly *better instrument* than organic for A1:**
LinkedIn sends warm, biased visitors who already know the founder — a terrible sample for testing
whether "EU residency" converts a *stranger*. Search sends **cold, intent-matched, randomizable**
traffic. **For settling A1, €400 of search beats nine months of posting to people who like you.**

---

## 2. The five open assumptions — and the decision rule for each

| | Assumption | Risk | Evidence | Metric | 🚨 Decision rule |
|---|---|---|---|---|---|
| **A5** | **Distribution works** — traffic can be acquired inbound | Business | **Weak** · **TOP RISK** | Qualified visitors (N15) | **N15 not reached by the T-box deadline (§2.2) ⇒ the business dies here.** Nothing downstream gets a vote. |
| **A1** | EU residency is a **purchase driver**, not a checkbox | Value | **Weak** | `signup` rate, headline A/B | **Arm A (EU) doesn't clearly beat Arm B (capability) ⇒ move ALL positioning to the capability story.** Do not rationalize a loss. |
| **A2** | Buyers **buy** rather than **DIY** | Value | **Weak** | `checkout_clicked` → **`payment_succeeded`** | **< 5% of trials reach checkout, or 0 payments after ~100 trials ⇒ it's a vitamin. Stop.** |
| **A3** | Usage is recurring **flow**, not one-time **backfill** | Business | **None** | **Cohort hour-decay** | **Month-2 hours < 20% of month-1 ⇒ not a subscription business. Switch to prepaid credit packs.** |
| **A4** | Output is **citable** (accurate) in a knowledge base | Value | Medium | **`upload_repeat`** | **< 30% of trial users upload a second video ⇒ the output didn't earn trust.** |
| ~~A8~~ | ~~EU model availability + COGS < €1.50/h~~ | Feasibility | ✅ **STRONG** | §1.2 | **Validated 2026-07-14. Monitor (N8), don't test.** |

**Why they rank this way.** Every High-importance / Weak-evidence assumption is **Value** or
**Business viability**. Feasibility — the axis a technical founder de-risks first — is the one
quadrant already closed. And because the GTM is inbound with no outreach, **A1–A4 are each only
*reachable* through A5.** Traffic is not one risk among five; it is the gate on the other four.

### 2.1 🚨 The minimum-sample rule — read this before looking at any dashboard

**No rule in §2 may be invoked below its sample floor.**

| Rule | Sample floor | Below it, the only honest reading is |
|---|---|---|
| **A5** | A *sustained* content effort — months, not one post | "We haven't tried yet." |
| **A1** | **Hundreds of visitors per arm** | "That's a chart, not evidence." Arm A 3 signups vs Arm B 1 is coin-flipping. |
| **A2** | **~100 trials**, and a live checkout link | Zero payments with no link built is **not** an A2 result. It is the absence of one. |
| **A3** | Two cohorts ≥ 4 weeks apart — or the **N20** proxy | "Ask again in a month." |
| **A4** | ~20 trial users with a completed first job | — |

**DISTRIBUTION.md pre-commits us to not rationalizing an A1 *loss*. The symmetric discipline — and
the likelier failure — is celebrating a false *win*.** A founder looking at 40 visitors, 2 signups
and 0 payments will be powerfully tempted to read a verdict into it, and **every available verdict
is wrong.** Set the honest-read date in advance; call nothing before it.

### 2.2 🚨 The time-box — **T** — because there is no burn clock

| # | | Value |
|---|---|---|
| **T0** | **Start.** First publication of the demand instrument (PLAN.md Phase 0.3). | *set on the day — record it here* |
| **T1** | **Checkpoint.** Honest read. No verdict may be called before this. | **T0 + 4 months** |
| **T** | **🚨 DEADLINE.** A5 is settled here, one way or the other. | **T0 + 9 months** |

**Why this exists, and why it is a hard rule rather than a preference.**

Deferring the salary (N3) removes the burn clock. At **N1a** the business is cash-flow-positive and
can run **forever** on ~€300/mo. That is a genuine strategic advantage — nothing forces a panicked
decision — and it introduces a failure mode that did not previously exist:

> ### 🧟 **The zombie.**
> A business that covers its infra indefinitely, never grows, and quietly consumes the founder's
> evenings for three years. **With a salary target, the kill criteria fire by themselves — you run
> out of money and stop. With no salary, nothing forces the stop.**
>
> **The scarce resource is no longer euros. It is founder-hours, and they are not on any dashboard.**

Therefore: **A5's kill criterion is a calendar deadline, not a cash threshold.** If N15 has not been
reached by **T** after a *sustained* publishing effort — **stop.** Not "reduce scope", not "try one
more channel". Stop.

**And note what T is really testing.** Content compounds only with sustained effort, and sustained
effort is exactly what a founder with a day job does not reliably have. **A5 did not get easier when
the salary came off — it changed shape**, from *"can I afford to wait?"* to *"will I still be
publishing in month nine?"* **T is the honest test of that question, and the answer is allowed to
be no.**

---

## 3. Event schema (stable contract — code references these names)

Emit with `tenant_id`, `user_id`, `session_id`, `occurred_at` (UTC), `referrer`, `ab_arm`.

| Event | Fires when | Carries |
|---|---|---|
| `page_view` | Landing / gallery page load | `path`, `ab_arm`, `referrer` |
| `yt_tool_used` | Free YouTube tool run | `video_id`, `duration_sec`, `cache_hit`, `cost_cents` |
| `signup` | Magic-link completed | `referrer`, `ab_arm`, **`archive_hours`, `monthly_hours`** (N20) |
| `upload_started` | Private upload begins | `duration_sec` |
| `job_completed` | Markdown delivered | `job_id`, `duration_sec`, `cost_cents`, `wall_clock_sec` |
| `output_downloaded` | User retrieves the Markdown | `job_id` |
| **`upload_repeat`** | **2nd+ upload by the same tenant** | `n_th`, `days_since_first` — **the A4 signal** |
| `checkout_clicked` | Stripe link opened | — **the A2 signal** |
| **`checkout_abandoned`** | `checkout_clicked`, no payment inside 24 h | **`reason`** (N21) |
| `payment_succeeded` | Stripe confirms | `amount_cents` — **the only signal that isn't a proxy** |

**Derived, nightly:** `tenant_hours_by_week(tenant_id, week_index, hours)` — the A3 series.

> 🚨 **Cohort hour-decay cannot be reconstructed retroactively. Instrument it before the first user,
> or lose the answer forever.** This is the one irreversible item in the entire plan.

---

## 4. How each assumption is actually read

### A5 — Traffic
Qualified visitors, by `referrer`.

> 🚩 **Watch trial-start rate *by referrer* from the very first visitor.** It needs far less volume
> than the A/B and the effect size is potentially enormous: it is how we learn whether the founder's
> LinkedIn .NET/architecture audience is **the actual buyer** or **a friendly crowd that claps and
> doesn't convert.** *An owned audience that isn't the ICP is a vanity asset.* If LinkedIn bounces
> while r/RAG converts, the channel strategy is wrong — cheap to fix in week one, ruinous to
> discover after three months of content poured into the wrong pipe. **This is the highest-value
> thing the funnel can teach us early, and it is not on the assumption list.**

### A1 — Headline A/B
Primary metric is **`signup` rate, not click-through** — clicks measure curiosity, signups measure
intent. Directional, not significant: we need a signal, not a paper. Subject to the §2.1 floor.

**The A/B tests which sentence converts. It cannot tell us whether GDPR is a genuine blocker or a
form field, because *we* raised it.** → **N22** is the stronger instrument.

### A2 — Willingness to pay
`checkout_clicked` → `payment_succeeded`. **One Stripe link, not three tiers** — tiers presuppose
the A3 answer we do not have. **Payment is the only non-proxy signal in this document. Everything
else is a story we tell ourselves about intent.**

### A3 — Cohort hour-decay 🚨 *the metric that decides what business this is*
Group tenants by signup week; plot **hours processed in week 1 vs. week 4.**

- **Month-2 ≥ 20% of month-1** ⇒ recurring flow ⇒ subscription tiers (BUSINESS_MODEL §6) are valid.
- **Month-2 < 20% of month-1** ⇒ **backfill business.** A customer converts a 200 h archive in one
  big month, then trickles. **Steady-state ARPA lands 3–5× below acquisition-month ARPA**, and
  subscription pricing becomes a category error that no engineering can repair. The correct product
  is a **prepaid credit pack** (*"€200 for 20 hours"*).

### A4 — Output trust
**`upload_repeat` is the real quality metric.** Satisfaction surveys measure politeness; a second
upload measures trust. A user who uploads once and never returns did not find the output citable,
whatever they'd have said in an interview. **Removing the conversation made this instrument better,
not worse.**

---

## 5. Conversation-substitute instruments

> **Why this section exists.** The founder ruled out outreach, so there are no interviews and no
> concierge test. **That means the Stripe link now carries the entire burden of detecting a vitamin,
> and there is no human signal upstream of payment.** A funnel can tell you 95% didn't click
> checkout; it can never tell you whether that was price, trust, output quality, or *"nobody has
> ever asked me for this"* — four findings that demand four different responses. Below is the
> cheapest partial recovery. **All three are zero-outreach.**

| # | Instrument | Reads | Why it works |
|---|---|---|---|
| **N20** | **The signup one-field question.** On the magic-link screen, skippable: *"Roughly how much video do you have?"* → `[~__ h in the archive]` `[~__ h added per month]` | **A3 — two months early** | A ratio **> 20:1 predicts backfill** before cohort data can mature. **The highest-value non-code line in the plan:** one form field, answering the assumption that decides *what business this is*. **The skip rate is data too** — if 80% skip, they don't know their own numbers, which is itself a finding about a "we have a video problem" claim. |
| **N21** | **Checkout-abandon micro-survey.** One question, one click, on `checkout_abandoned`: *"What stopped you?"* → `Too expensive` · `Output wasn't good enough` · `I could build this myself` · `Not my decision` · `Just looking` | **A2 — the *why*** | **"I could build this myself" is the A2 kill signal**, and this is the only place in a conversation-free funnel where it can ever be said out loud. Five radio buttons is a poor substitute for an interview — and infinitely better than the zero we otherwise have. |
| **N22** | **The reply-to invitation.** Every lifecycle email ends: *"Reply and tell me what it got wrong — I read every one."* | **A1 (unprompted), A2, A4** | 🔑 **The founder ruled out *doing* outreach. He did not rule out *receiving* a reply.** Zero founder-hours until someone writes back; anyone who does is self-selected and high-intent. **Log unprompted GDPR mentions separately — an unprompted mention is a stronger A1 signal than the headline A/B**, which only tests a claim we raised ourselves. → DISTRIBUTION.md. |

---

## 6. Anti-metrics — do not track, do not celebrate

Impressions, likes, "engagement", trial *signups* absent second-uploads, and — most dangerously —
**enthusiasm.**

> **Enthusiasm without payment is the single most common way a vitamin gets mistaken for a
> painkiller**, and exposing it is exactly what the (now-cancelled) concierge test was for. We
> removed the conversation. **The Stripe link has to carry that entire burden. Respect it.**
