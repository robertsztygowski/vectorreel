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

**Cost — comfortably better than assumed.** Every produced session measured **below METRICS.md
N4c**, consistently, across sources of different length and density. Nothing came close to the
METRICS.md **N4d** over-segmentation failure mode, and the per-session abort ceiling never fired.
The full corpus projects to a few euro. **Cost engineering on this path would be procrastination in
a lab coat** — PLAN.md already said that about the pipeline generally, and this run confirms it for
collection production specifically.

**Throughput — the real constraint, and it was not on anyone's list.** Vertex returns
`429 RESOURCE_EXHAUSTED` in *both* EU regions under sustained batch load. INFRA.md owns the
measurements; the short version is that this is **Dynamic Shared Quota contention, not an exhausted
allowance** (we run at ~1% of our own limit), the binding limit is **non-adjustable**, and it
refreshes per minute with no daily cap. So a batch is never blocked, only slowed — but "only
slowed" turned a projected 1.5-hour job into a multi-hour one.

⇒ **The weekly batch is bounded by wall-clock, not by euros.** That inverts the planning
assumption. DISTRIBUTION.md's cut rule ("batch size shrinks, quality gates never") is the right
lever, and the thing it should be sized against is elapsed time, not spend.

---

## 2. Four defects the run found — none of which any test would have caught

Every one of these was invisible until the pipeline met a real video. All four are fixed and
regression-tested; they are recorded here because the *pattern* matters more than the individual
bugs.

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
