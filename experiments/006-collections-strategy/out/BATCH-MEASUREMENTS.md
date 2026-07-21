# M3 — what manufacturing a real collection actually measured

> **Status: point-in-time record, 2026-07-21. NOT authoritative** (CLAUDE.md — `experiments/**`
> never is). Measurements taken during the first-collection run. If this file contradicts a living
> doc, the living doc wins. Numbers METRICS.md owns are cited **by name**, never restated here.
>
> Companion files: [`corpus.json`](./corpus.json) (licence audit trail),
> [`SOURCES.md`](./SOURCES.md) (the licence funnel, answering Q2),
> [`spot-verify-ai-engineering.json`](./spot-verify-ai-engineering.json) (per-session quality gate).

---

## 1. The headline: cost is not the problem, throughput is

This was the run that finally spent real inference money, and the two things it was supposed to
settle came out in opposite directions.

**Cost — cheap in absolute terms, but ABOVE N4c on the full batch.**

⚠️ **Correcting an earlier reading in this same run.** After the first five sessions the figures were
all under METRICS.md **N4c** and that was reported as the result. Across the completed batch it does
not hold: **16 sessions spanned 14 → 89 cents/video-hour and averaged ~45**, which is meaningfully
**above N4c**. The early number was a favourable sample generalised too soon — precisely the failure
the one-number-one-home discipline exists to prevent.

In absolute terms it is still cheap: **€4.65 for 16 sessions**, far below the METRICS.md **N8**
all-in guardrail, and the **N4d** per-session ceiling never fired.

🚩 **The overage is retry waste, and the natural experiment that proves it ran by accident.**
Retry attempts were raised from 3 to 8 to buy throughput, and **a failed segment that reached the
model still bills**. The first evidence was only correlational — the two most expensive sessions
(89 and 75 c/video-hour) were also the two slowest — with content density an equally plausible
cause.

**Then the two sessions that failed all 8 attempts were retried a few minutes later, on quieter
capacity, and became the two CHEAPEST in the batch:**

| Session | During the contended batch | Retried later |
|---|---|---|
| `OtgOdjQM-yo` (8 segments) | failed all 8 attempts | **3m 25s · 15 c/video-hour** |
| `6G_OptYhxJ4` (3 segments) | failed all 8 attempts | **1m 48s · 14 c/video-hour** |

Same videos, same segmentation, same prompts, ~90% cheaper and minutes instead of hours. **Content
density is ruled out**: an 8-segment video came in at 15 c/video-hour. What differed was only *when*
they ran. So the expensive sessions were expensive because they paid for rejected work, not because
they were hard.

⇒ **Retry aggressiveness trades euros for wall-clock, and past a point it buys neither.** Eight
attempts against sustained contention burned ~3 hours and ~€3 to produce nothing; two attempts and
a deferral would have cost minutes and produced the same result later, cheaper. **The right policy
is a cheap give-up plus a resumable retry** — which the batch already supports (`--only`), and which
this run demonstrated by accident rather than by design.

**Throughput — looked like a capacity wall, was actually our own client.** Vertex returns
`429 RESOURCE_EXHAUSTED` in *both* EU regions under batch load. That is **Dynamic Shared Quota
contention, not an exhausted allowance**: we run at ~1% of our own (non-adjustable) limit, and it
refreshes per minute with no daily cap (INFRA.md).

The tempting conclusion — *"Vertex cannot do the volume"* — is wrong, and the measurement that
disproves it is that **the 429 success rate is flat at ~25% at every concurrency level tried**
(1, 4, 12, 24). We are not causing the rejections, and pushing harder does not increase them. So
throughput scales with attempts in flight, and **the production path had been built strictly
sequential with multi-minute backoff** — the pathological client for a contention limit, where a
single rejection arriving in under a second stops the whole pipeline and then sleeps.

| | before (sequential) | after (concurrent, 5 s backoff) |
|---|---|---|
| successful calls/min | ~0.8 | ~8.0 at 24-way |
| per session, production time | 8–40 min, with stalls | **~4 min** |
| sessions previously *given up on* | 2 | **both completed, <4 min each** |

⚠️ **But do not read ~4 min/session as ~4 min × N for a batch.** The completed run produced 16
sessions in **65 minutes of production time inside 243 minutes of wall clock**. The ~3-hour gap is
almost entirely **two videos that failed all 8 attempts**, plus retries on others. *Production is
fast; failure handling is what costs hours.* A batch's elapsed time is dominated by its worst
sources, not its average one — so the useful lever for a weekly batch is a cheaper give-up, not more
speed on the happy path.

⇒ **The weekly batch is bounded by wall-clock — and the wall-clock was ours to fix.** The honest
version of the earlier conclusion: sizing cadence against *spend* would still optimise the wrong
variable, but the elapsed-time figure to size against is the one after this fix, not before it.

⇒ **For the paid path** — the question this actually raised — a customer's 30-minute video is 3–6
segments run concurrently: **1–2 minutes**, comfortably inside the METRICS.md **N9** SLO. Our own
limit does not bind until roughly 60 segments/min (~20–25 concurrent in flight), which is where
Provisioned Throughput becomes a real decision rather than a premature one.

---

## 2. Four defects the run found — none of which any test would have caught

Every one of these was invisible until the pipeline met a real video. All four are fixed and
regression-tested; they are recorded here because the *pattern* matters more than the individual
bugs.

> 🚨 **A fifth defect was found by the founder AFTER this list was written, and it is the worst of
> them: Stage C fuses segment-relative timestamps as absolute, so every multi-segment session
> collapses onto its first ten minutes and its citations point at the wrong moments.** PLAN.md
> carries the full analysis. It belongs in the table below as the purest instance of the pattern:
> **`FakeTextFuser` computes timestamps from `SegmentStart` and therefore gets it right, while the
> real fuser does not** — so the entire test suite, which runs on fake and replay, is structurally
> incapable of seeing it. Not "no test covered this", but *no test **could***.

| # | Defect | Why no test caught it |
|---|---|---|
| 1 | **The cost ledger recorded calls but never euros.** Every `CostEntry.Cents` was `null` across Stages A, B and C. | The ledger was *structurally* correct and every test asserted on entry counts. "Reconcile against N4c" would have returned **zero and looked fine** — worse than an error, because a confident wrong number does not prompt investigation. |
| 2 | **The rule-9 wall-clock timeout was below the path's real latency.** A 10-minute YouTube segment takes 24–70 s because Google fetches the video itself; the 90 s default left no room for the 429 → `europe-west3` fallback, so the guard fired *mid-fallback* and failed every segment. | Fake and replay modes return instantly. The public path had **never run live** — every deployed service has been `PipelineModel__Mode=fake` since Phase 3. |
| 3 | **Stage C emitted §4-contract-violating documents** — section timestamps not strictly ascending. Two independent causes: the fusion model does not reliably order sections, and adjacent segments **overlap by design**, so two of them legitimately describe the same moment. | Fixture responses were single-section and already ordered. The overlap collision is only reachable on a multi-segment video. |
| 4 | **GitHub auto-anchors keep existing hyphens** (`AI-Infused` → `ai-infused`); the repository renderer stripped them, producing citation anchors that silently did not resolve. | The canonical fixture contains no hyphenated heading. Found only by pointing the existing §4b oracle at a **generated** repository instead of the example. |

> 🚩 **The reusable lesson.** Three of the four hid behind a fixture that was correct but
> unrepresentative, and the fourth hid behind a mode that never runs in anger. A contract is only
> worth having if the **thing we actually ship** is held to it — which is why
> `web/lib/repository.test.ts` now takes `MDREEL_REPOSITORY_DIR` and can be pointed at real output.

---

## 3. Cost, measured

Per-session figures are in the batch report next to this file. What is worth stating in prose:

- **Every session came in under METRICS.md N4c.** The assumption held, on real material, at low
  media resolution.
- **The per-modality split was necessary, not fussy.** On a measured 10-minute segment at
  `MEDIA_RESOLUTION_LOW`, **audio priced higher than video** — video tokens collapse at low
  resolution and audio tokens do not. A blended input rate would have mispriced precisely the
  configuration production runs on. METRICS.md §1.2's "audio is now the floor" note is confirmed on
  the collection path.
- **Microcents are load-bearing.** One Stage B call is worth a fraction of a cent; a batch summed
  from per-call rounded cents reads **zero**. That is now a test, not a comment.

## 4. Wall-clock, and what it means for N9

Sessions that ran without contention completed well inside the METRICS.md **N9** SLO ratio.
Sessions that hit a 429 storm did not — but **that is not an N9 breach in the sense N9 exists to
catch**: N9 guards runaway generation, and this is queueing for shared capacity. Recording it here
so a later reader does not "fix" a capacity problem by loosening a generation guard.

**Segment checkpointing changed the economics of a retry.** Before it, a video that died at segment
5 restarted at 0 and re-bought everything it had already paid for. After it, one observed retry
completed in **1:35 instead of a fresh ~5 minutes**, because only the failed segment was re-run.

## 5. N7 / N7c — what this batch can and cannot say

- **N7 (Stage B runaway rate):** no session triggered the per-session cost ceiling, and no
  MAX_TOKENS halving cascade was observed. Consistent with N7 holding on this content type, at this
  segment length, at low media resolution. **Not a general verdict** — this corpus is conference
  talks, which METRICS.md N30 already scores as the strong category.
- **N7c (does Stage B *obey* forced cue boundaries?):** ⚠️ **still open, and this batch does not
  close it.** The YouTube path has no Stage A — no bytes, so no local analysis, so no forced cues to
  obey. The verdict N7c needs requires the **private** path on real footage. Anyone reading a green
  batch here as evidence for N7c would be reading the wrong path's result.

## 6. The quality gate — built, run, and **inconclusive**

The per-session gate is the 3-timestamp spot-verify, implemented as an **independent re-probe**: a
second, differently-prompted call seeks to the block's own claimed timestamp and describes what is
there **without seeing the claim**, so the document is graded against a fresh look at the video
rather than against itself. That is the method Phase 0.2 used to grade METRICS.md N30–N32.
Sections are sampled deterministically and **content-blind** — picking the eye-catching ones would
measure the opposite of what we want to know (DECISIONS.md D11).

**First run: 1 of 12 checks graded. Every one of the other 11 failed with `429`.**

⚠️ **That is a quota result, not a quality result, and it must not be read as one.** The gate was
running against the same shared regional capacity as the production batch. The single graded check
passed (anchored, no on-screen contradiction), which is one data point and worth nothing on its own.

Two corrections made rather than papered over:

1. The harness gave up after one pass over both EU regions. Since a 429 is a *wait*, not an
   exhaustion, it now retries in rounds with escalating backoff — waiting is the correct behaviour.
2. The parser was written against the Phase-0.2 corpus format and silently reported every real
   document as `UNPARSEABLE`: §4 headings are `## [hh:mm:ss] Heading` (bracketed) and the
   frontmatter key is `source`, not `source_filename`. **A quality gate that cannot read the
   artifact reports success at zero coverage** — the most dangerous failure mode a gate has.

**⇒ Re-run the gate when the batch is not competing for capacity, and treat its output as
unmeasured until then.** No session should be published on the strength of this run's reading.

## 7. The honest gap

The batch did not complete within the run. Sessions that failed were failed on **capacity**, not
quality — they remain in the corpus with a recorded retry command, and a later batch picks them up
for free because production is resumable. What shipped is a real, browsable, contract-valid
collection; what did not ship is its full intended size.
