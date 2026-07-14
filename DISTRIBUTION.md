# DISTRIBUTION — how vectorreel gets in front of people

> Living doc. Owns **A5 — the top risk in the business.** Companion to BUSINESS_MODEL.md (§7 is
> the strategic sketch; this file is the operating plan) and PLAN.md Phase 0.3.
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

**The corollary that reorders the roadmap:** content compounds over *months*. Building for three
months and *then* starting to publish serializes the two slowest processes in the company.
**Distribution starts at PLAN.md Phase 0.3, in parallel with all engineering.**

## The core insight: the product is the marketing

The free public YouTube tool is not a demo — **it is the distribution engine**, and it exists for
this reason alone.

**The funnel:**

```
  paste a YouTube URL  ──►  see real Markdown in 60s  ──►  "now try it on your own recording"
   (free, no signup,          (the wow, on content         (magic-link, 2h free)
    zero trust required)       they already know)                    │
                                                                     ▼
                                                            one Stripe payment link
```

Why this ordering matters: the old funnel opened with *"upload your confidential internal
recording to a stranger's website."* That is a brutal first ask and was the biggest leak in the
design. Pasting a public YouTube link costs the visitor nothing and risks nothing — and because
they already know the video, they can **verify the output is good** without trusting us at all.

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
attributed, original video embedded, human-selected.** See CLAUDE.md rule 8.

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
Launch surface for the artifact post and the free tool. **One-shot, non-compounding** — treat as
spikes, not a strategy. A launch post is not a distribution plan.

### 5. `llms.txt` + public API docs
Long game: be the thing an agent recommends.

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

## The launch artifact (Phase 0.3) — already written

Side-by-side: **plain transcript vs. vectorreel Markdown**, plus a RAG answering a question that
is **only** answerable from on-screen content. This is the entire pitch in one image.

Previously blocked — the benchmark ran on company-internal video that cannot be shared. **The
public CC corpus (PLAN.md Phase 0.2) unblocks it.**

## The A1 experiment — is EU actually the wedge?

Landing-page headline A/B (PLAN.md Phase 0.3):

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
