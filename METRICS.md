# METRICS — what we measure, and what each number makes us do

> Living doc. The contract between the open assumptions and the code that answers them.
> Companion to PLAN.md Phase 4 (implementation) and DISTRIBUTION.md (the funnel).
> Created 2026-07-14 from `experiments/workflow1-decision-memo.md`.

## The rule that justifies this document

> **🚨 Cohort hour-decay cannot be reconstructed retroactively. Instrument it before the first
> user, or lose the answer forever.**

Every metric below is paired with a **decision rule** — what we *do* when the number lands. A
metric without a decision rule is a vanity metric; do not add one to this file.

---

## The five open assumptions and where each is settled

| | Assumption | Metric | Decision rule |
|---|---|---|---|
| **A5** | Distribution works (**top risk**) | Qualified visitors | < 2,000 reachable ⇒ **the business dies here.** Nothing downstream gets a vote. |
| **A1** | EU is a purchase driver, not a checkbox | Trial-start rate, headline A/B | Arm A (EU) doesn't clearly beat Arm B (capability) ⇒ **move all positioning to the capability story.** |
| **A2** | Buyers buy rather than DIY | Checkout clicks; **payments** | < 5% of trials reach checkout, or 0 payments after ~100 trials ⇒ **it's a vitamin. Stop.** |
| **A3** | Recurring **flow**, not one-time **backfill** | **Cohort hour-decay** | Month-2 hours < 20% of month-1 ⇒ **not a subscription business. Switch to prepaid credit packs.** |
| **A4** | Output is citable in a KB | Second-upload rate | < 30% of trial users upload a second video ⇒ **the output didn't earn trust.** |

---

## Event schema (stable contract — code references these names)

Emit with `tenant_id`, `user_id`, `session_id`, `occurred_at` (UTC), `referrer`, `ab_arm`.

| Event | Fires when | Carries |
|---|---|---|
| `page_view` | Landing / gallery page load | `path`, `ab_arm`, `referrer` |
| `yt_tool_used` | Free YouTube tool run | `video_id`, `duration_sec`, `cache_hit`, `cost_cents` |
| `signup` | Magic-link completed | `referrer`, `ab_arm` |
| `upload_started` | Private upload begins | `duration_sec` |
| `job_completed` | Markdown delivered | `job_id`, `duration_sec`, `cost_cents`, `wall_clock_sec` |
| `output_downloaded` | User retrieves the Markdown | `job_id` |
| **`upload_repeat`** | **2nd+ upload by the same tenant** | `n_th`, `days_since_first` — **the A4 signal** |
| `checkout_clicked` | Stripe link opened | — **the A2 signal** |
| `payment_succeeded` | Stripe confirms | `amount_cents` — **the only signal that isn't a proxy** |

**Derived, computed nightly:** `tenant_hours_by_week(tenant_id, week_index, hours)` — the A3 series.

---

## The metrics

### A5 — Traffic (top risk)
Qualified visitors, by referrer. Assumed funnel: **2–5% visitor→trial, 5–15% trial→paid ⇒
~2,000–5,000 visitors per ~5 paying customers.** These rates are **assumptions**; replace them
with measured values the moment there's data — the whole plan is sensitive to them.
**Watch trial-start rate *by referrer*:** it's how we learn whether the LinkedIn .NET audience is
the actual buyer or just a friendly crowd (DISTRIBUTION.md open questions).

### A1 — Headline A/B
Primary metric **trial-start rate**, not click-through — clicks measure curiosity, signups measure
intent. Directional, not significant; we need a signal, not a paper. **Do not rationalize a loss
for Arm A.** The entire differentiation strategy rests on this, which is exactly why it must be
allowed to fail.

### A3 — Cohort hour-decay 🚨
**The metric that decides what business this is.**

Group tenants by signup week. Plot **hours processed in week 1 vs. week 4**.

- **Month-2 ≥ 20% of month-1** ⇒ recurring flow. Subscription tiers (BUSINESS_MODEL §6) are valid.
- **Month-2 < 20% of month-1** ⇒ **backfill business.** A customer converts their 200 h archive in
  one €690 month and then trickles. **Steady-state ARPA lands 3–5× below acquisition-month ARPA**,
  and subscription pricing becomes a category error that no engineering can repair. The correct
  product is a **prepaid credit pack** (*"€200 for 20 hours"*).

Also capture at signup, if it can be asked in one field without friction:
**archive hours vs. hours recorded per month.** A ratio above ~20:1 predicts backfill before the
cohort data matures.

### A2 — Willingness to pay
`checkout_clicked` → `payment_succeeded`. **One Stripe link, not three tiers** — tiers presuppose
the A3 answer we don't have yet. Payment is the only non-proxy signal in this document; everything
else is a story we tell ourselves about intent.

### A4 — Output trust
**`upload_repeat` is the real quality metric.** Satisfaction surveys measure politeness; a second
upload measures trust. A user who uploads once and never returns did not find the output citable,
whatever they'd say in an interview.

---

## Operational / COGS metrics (guard the margin — don't chase it)

COGS is **measured and solved**: ≈ €0.65/video-hour all-in, ~7× inside the cheapest price point
(BUSINESS_MODEL §6). These exist to catch *regressions*, not to drive optimization work.

| Metric | Threshold | Action |
|---|---|---|
| **All-in COGS / video-hour** (LLM **+ ffmpeg compute**) | > €1.50 | Guardrail breached — investigate. Note the ledger must meter compute; it's ~30% of true COGS (ARCHITECTURE §8). |
| **Runaway-generation rate** (Stage B calls hitting `maxOutputTokens` / `thinkingBudget`) | > 8% (the measured baseline) | Guards regressed (ARCHITECTURE §3). This drives the 1.3× retry overhead. |
| **Wall-clock per video-hour** | > 15 min | SLO breach (BUSINESS_MODEL §9). |
| **Free YouTube tool spend / day** | > €5/day | Abuse. Caps + cache are failing (~€0.45/video-h — **no static lever on that path**). |
| **Gallery cache-hit rate** | < 90% | Gallery pages should cost ≈ €0 on repeat views. |

---

## Anti-metrics — do not track, do not celebrate

Impressions, likes, "engagement", trial *signups* absent second-uploads, and — most dangerously —
**enthusiasm**. Enthusiasm without payment is the single most common way a vitamin gets mistaken
for a painkiller, and it is precisely what the concierge test was designed to expose. We removed
the conversation, so **the Stripe link now has to carry that entire burden.**
