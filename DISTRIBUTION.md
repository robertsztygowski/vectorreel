# DISTRIBUTION — how mdreel gets in front of people

> Living doc. Owns **A5 — the top risk in the business.** Companion to BUSINESS_MODEL.md (§7 is
> the strategic sketch; this file is the operating plan) and PLAN.md Phase 5 (launch).
> Created 2026-07-14 from `experiments/agents/out/w1-decision-memo.md`.

## Why this document exists

The founder has ruled out customer outreach (no time). That is a legitimate bootstrapper's
choice — a funnel compounds, cold emails don't — but it has a hard consequence:

> **Traffic is now the long pole, and inbound traffic is the thing that kills this company
> first.** Every other risk (does the output work, will they pay, is EU a wedge) is only
> *reachable* through traffic. Feasibility and margin are already measured and safe.

Distribution used to be four bullets in BUSINESS_MODEL §7. It is now the whole game, so it gets
its own document.

## The traffic math — the number that governs everything

**→ METRICS.md §1.4 (N12–N15).** The rates are not restated here; they are **assumptions**, and the
whole plan is sensitive to them.

What the math *means*, which is this document's job:

**🎯 One target, two payoffs (METRICS.md N15).** The traffic needed to cover infra (N1a) and the
traffic needed to reach the A2 sample floor land in the *same range*. **The visitors that pay for
the servers are the same visitors that tell you whether to continue.** That coincidence is the best
structural fact this business has, and it exists only because the founder deferred the salary — the
old target (N1b) implied an order of magnitude more traffic and made A5 look nearly unwinnable.
**That was an artifact of the salary line, not of the market.**

**So the goal is a good-post-sized number, not a content-engine-sized number.** One HN front page,
one LinkedIn post that lands, or a gallery that quietly ranks over two quarters.

⚠️ **This does not make A5 easy — it changes its shape.** Content compounds only with *sustained*
effort, and sustained effort is exactly what a founder with a day job does not reliably have.
**The question stopped being *"can I afford to wait?"* and became *"will I still be publishing in
month nine?"*** That is what the time-box (METRICS.md §2.2) tests, and the answer is allowed to be no.

**The sequencing decision — reversed 2026-07-15.** The original rule ("distribution starts first,
in parallel with all engineering") lost to a stronger fact about the launch surfaces themselves:
HN/LinkedIn/Reddit posts are largely **one-shot**, and pointing that first wave at an email-capture
box converts in the low assumed range, while a live paste-a-URL tool is the high-conversion
scenario that makes paid acquisition viable at all (METRICS.md §1.6). **So the MVP ships first and
everything launches at once — PLAN.md Phase 5.** The cost of the reversal is that the *build* can
now rot instead of the publishing; that risk is carried by the hard **ship-by gate (METRICS.md
§2.2 SB)** — if the date arrives first, launch with whatever exists.

## The core insight: the gallery is the demo

**Revised 2026-07-15 — the free YouTube tool was dropped.** An open compute endpoint is a bot and
abuse surface, and an ops tax, that a fixed base this small cannot carry — even capped. The job the
tool did in the funnel — *let a skeptic verify real output on a video they already know, before
trusting us with anything* — moves to the **curated gallery**, which does it at **zero compute per
visitor** (pre-rendered pages, produced by us; there is no public input box anywhere).

**The funnel:**

```
  browse the gallery      ──►  verify real Markdown on a  ──►  "try it on your own recording"
  (10–25 processed talks,       talk you already know           (magic-link signup +
   zero friction, zero cost)    — the wow, pre-rendered)         trial credit, METRICS.md N33)
                                                                          │
                                                                          ▼
                                                          two-plan checkout (BUSINESS_MODEL §6)
```

Why this ordering matters: the original funnel opened with *"upload your confidential internal
recording to a stranger's website."* That is a brutal first ask and was the biggest leak in the
design. The gallery keeps the fix — **verification without trust** — while removing the endpoint
that had to be defended. The trade: the visitor verifies on *our* examples, not a video of their
choosing; the trial credit is what closes that last gap, on their own footage.

> **⚠️ The gallery must out-work a live Playground (competitor-informed 2026-07-15 —
> experiments/002-competitor-analysis).** Cloudglue's demo is an interactive Playground where a
> skeptic pastes *their own* URL; ours is deliberately pre-rendered (no compute, no abuse surface —
> METRICS.md N10). That means the gallery carries **more proof burden**, so **curate it around talks
> the ICP already knows** (well-known conference talks in their domain) — the skeptic then verifies
> the output against ground truth they already hold in their head, which is the only substitute for
> "try it on your own video first."

## Channels, in priority order

### 1. LinkedIn — the founder's .NET/architecture audience
**Cheapest channel available, by an order of magnitude, and it already exists.** An owned
audience is the single biggest asset a bootstrapper has. Broadcasting to it is **not outreach** —
it requires zero 1:1 contact with anyone, so it does not violate the constraint that reshaped
this plan.

### 2. The curated gallery — a compounding SEO asset
10–25 processed public talks, each a page of structured, timestamped Markdown, each with
attribution and the original video embedded. Every page is (a) a working demo, (b) an SEO surface
for that talk's subject matter, (c) proof the output is real.

🚨 **Curated — never a scaled content farm.** Mass auto-generated transcript pages are precisely
what Google's scaled-content-abuse policy targets, and republishing copyrighted transcripts at
volume is a fight a bootstrapped company cannot afford. **Rules: CC-licensed sources only,
attributed, original video embedded, human-selected — and preferentially talks the ICP already
knows, so the page doubles as ground-truth proof (see the core-insight note above).** See CLAUDE.md rule 8.

### 3. The technical blog — sell the boring hard parts
The differentiation *is* the unglamorous engineering, so write about it (BUSINESS_MODEL §8 —
"Gemini makes DIY trivial" is answered by showing the parts DIY gets wrong):
- Chunking long video without losing context at segment boundaries
- Cost engineering: what our real all-in cost per video-hour consists of (METRICS.md §1.2), and the
  static-content lever
- Timestamp normalization (model output drifts across three different formats)
- Runaway generation: how ~8% of calls tried to emit 60k tokens, and how we capped them
- GDPR architecture, honestly — *including* the CLOUD Act limitation (BUSINESS_MODEL §4)

Honesty is the sales asset here. Overclaiming EU sovereignty gets destroyed by any competent DPO.

### 4. Communities — HN, r/RAG, r/LocalLLaMA
Launch surface for the artifact post and the gallery. **One-shot, non-compounding** — treat as
spikes, not a strategy. A launch post is not a distribution plan.

### 5. `llms.txt` + public API docs + MCP
Long game: be the thing an agent recommends. **Competitor analysis (experiments/002) shows this is a
*primary* channel for the direct ring, not just a long game** — Cloudglue, Twelve Labs, Mixpeek,
VideoDB all ship an MCP server + llms.txt precisely so an AI assistant recommends them inside the
IDE. For a bootstrapper with no outreach, "be what the agent recommends" is one of the cheapest
surfaces available. The MCP server lands in PLAN.md Phase 4 as a thin layer over the API; keep its
distribution value in mind if the SB gate (METRICS.md §2.2) forces a cut decision — the API +
llms.txt are the floor.

### 6. 🔑 Inbound replies — the one conversation channel that survives

Every lifecycle email ends with *"Reply and tell me what it got wrong — I read every one."*
(METRICS.md **N22**.)

**The founder ruled out *doing* outreach. He did not rule out *receiving* a reply.** This costs
**zero founder-hours until someone writes back**, and anyone who does is self-selected and
high-intent — so it does not violate the constraint that reshaped this plan, and it is the **only
channel through which a human sentence can still reach us.**

It matters most for **A1**. The headline A/B tests *which sentence converts*; it can never tell us
whether GDPR is a genuine blocker or a form field, **because we were the ones who raised it.** An
**unprompted** GDPR mention in an inbound reply is a strictly stronger A1 signal than the A/B.
**Log those separately.**

## The launch artifact (PLAN.md Phase 5) — already written

Side-by-side: **plain transcript vs. mdreel Markdown**, plus a RAG answering a question that
is **only** answerable from on-screen content. This is the entire pitch in one image.

Previously blocked — the benchmark ran on company-internal video that cannot be shared. **The
public CC corpus (PLAN.md Phase 0.2) unblocks it.**

## The A1 experiment — is EU actually the wedge?

Landing-page headline A/B (PLAN.md Phase 5):

- **Arm A —** *"Your recordings never leave the EU."*
- **Arm B —** *"Your AI assistant can't see what's on screen in your videos."*

Measure **trial-start rate, not click-through** — clicks measure curiosity, signups measure intent.
Directional, not statistically significant; we need a signal, not a paper.

> **🚨 The decision rule is METRICS.md A1** — *if Arm A does not clearly beat Arm B, all positioning
> moves to the capability story.* This is the cheapest possible test of the assumption the entire
> differentiation strategy rests on. **Do not skip it, and do not rationalize a loss.**
>
> ⚠️ **And do not celebrate a false win either.** The A/B is subject to the minimum-sample rule
> (METRICS.md §2.1): it needs **hundreds of visitors per arm.** At n=4 it is coin-flipping.

## Paid acquisition — **buy evidence, not customers**

**→ METRICS.md §1.6–§1.7 owns the arithmetic, the CAC ceiling (N23), the max viable CPC (N25) and
the tranche gates (N26–N28). This section owns the strategy.**

### The correction this section exists to make

A5 has long carried the line *"CAC above the ceiling breaks payback, and outbound sales is
arithmetically impossible at this ACV."* **That was true, and it was about outbound *sales* — a
founder's hours at €X/hour. It was never applied to paid ads, which have a completely different
cost structure. Nobody ran the numbers.** They are now run (METRICS.md §1.6), and the answer is
neither "yes" nor "no":

> **Paid search cannot be the growth engine at today's conversion rates — the keywords we can
> afford have no volume, and the ones with volume we cannot afford (the scissors, METRICS.md §1.6).
> But paid search is the single best *instrument* available for settling A1 and measuring N12, and
> it should be funded immediately for that purpose.**

### Three jobs money can do. Only one is justified today.

| Job | Verdict | Why |
|---|---|---|
| **1 — Buy evidence** | ✅ **Fund at launch (T-LAUNCH, METRICS.md N27)** | N12 and N13 are guesses and *every* traffic number derives from them. Paid replaces them with measured values in ~3 weeks; organic takes months. **And it is a better instrument than organic for A1:** LinkedIn sends warm, biased visitors who already know the founder — a terrible sample for testing whether "EU residency" converts a *stranger*. Search sends **cold, intent-matched, randomizable** traffic. |
| **2 — Buy customers** | 🛑 **Blocked on A3** | The CAC ceiling swings ~10× on whether usage is recurring flow or one-time backfill, and **A3 has zero evidence.** Scaling paid before A3 is how a bootstrapped company scales into a wall — it feels like growth right up until the cohort data lands. |
| **3 — Buy reach on LinkedIn** | ❌ **Don't** | €6–12/click to reach the .NET/architecture audience **already reached organically, for free.** The worst euro available. Revisit only if organic proves that audience converts *and* its reach is exhausted. |

**Meta / X / Instagram:** wrong buyer, wrong intent. No.
**Reddit (r/RAG, r/LocalLLaMA):** a cheap, interesting probe — and note it doubles as an **A2 stress
test**: the audience most able to build this themselves is the harshest possible test of whether
anyone will buy it.

### The portfolio — what each channel is actually *for*

| Channel | Role | Honest expectation inside the time-box (T) |
|---|---|---|
| **LinkedIn (organic)** | 🐴 **The workhorse.** Owned audience, predictable, free. | Carries most of the volume toward N15. |
| **Paid search** | 🔬 **The instrument.** Cold, clean, randomizable. | Settles A1 and measures N12. **Not a volume source.** |
| **HN / Reddit** | 🎟️ **The lottery ticket.** | Could clear N15 in a day; ~5–10% chance per submission. **Buy the ticket; do not budget for it.** |
| **Gallery + blog (SEO)** | 💰 **The annuity — which mostly matures *after* T.** | ⚠️ **Near-zero for the first 3–6 months; meaningful at 12–18.** The time-box is 9. |

> ### ⚠️ The consequence, stated plainly: **inside the time-box, broadcast carries the load and the
> compounding assets do not have time to compound.**
>
> **So during T, the gallery is not a traffic source — it is *proof*.** Its job is to make paid and
> LinkedIn traffic **convert**, by letting a skeptic verify the output on a talk they already know.
> Build it for that. Any SEO that arrives is a bonus that pays out only if the box is extended.

## Kill criteria — **a deadline, not a cash threshold**

**→ METRICS.md A5 + §2.2 (the time-box).** If N15 has not been reached by **T** after a *sustained*
publishing effort — **the business dies here, before product quality or pricing ever get a vote.**

🧟 **Why a deadline.** With the salary deferred, the business is cash-flow-positive at N1a and could
run **forever** on ~€300/mo. **Nothing forces a stop.** The old kill criteria fired by themselves —
you ran out of money. These don't. **The scarce resource is founder-hours, and they are on no
dashboard.** Without a date, the failure mode is not bankruptcy; it is a **zombie that quietly eats
three years of evenings.**

**Say so early and out loud, rather than building more product to avoid the finding.** That sentence
is the entire reason this document exists.

## Open questions

- Real conversion rates (replace the assumed 2–5% / 5–15% as soon as METRICS.md has data).
- Does the gallery actually rank, or does Google treat processed talks as derivative content?
- Is the .NET/architecture audience the *buyer* (dev teams building AI assistants), or just a
  friendly crowd that claps and doesn't convert? **This is the uncomfortable one — an owned
  audience that isn't the ICP is a vanity asset.** Watch trial-start rate by referrer.
