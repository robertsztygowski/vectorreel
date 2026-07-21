# mdreel ~8h autonomous run — coordinator launch prompt (CORE CORRECTNESS)

> **How to use:** open a fresh agent session in the repo root, enable `/autopilot`, `/keep-alive`,
> raise `/limits`, then paste everything below the line as the first message.
>
> **Why this run exists.** The 2026-07-21 first-collection run shipped a browsable collection and
> then the founder opened one session document and found it was wrong: a 25-minute talk whose last
> timestamp was 09:16. The pipeline's *one job* — video in, correct timestamped Markdown/JSON out —
> is not proven, and the test suite is structurally incapable of proving it. **This run makes the
> core provably correct, or reports honestly that it is not.**

---

Fleet deployed: you are the **marathon coordinator** for an ~8-hour autonomous run on mdreel
(repo `robertsztygowski/vectorreel`, branch `main`). Work fully autonomously — the founder is away
and does NOT want to be involved. Before starting, read **PLAN.md STATUS (especially the two
🚨 KNOWN DEFECT blocks at the top of the 2026-07-21 report)**, ARCHITECTURE.md §3 (Stages A–D) and
§4 (the output contract), INFRA.md (Vertex capacity), TESTING.md, METRICS.md (cite numbers **by
name**, never restate), `tests/fixtures/videos/README.md`, and CLAUDE.md.

**The goal, stated as a falsifiable claim you must either prove or refute:**

> *For any supported input video, on **both** ingestion paths, mdreel produces a §4 document whose
> sections span the whole video, whose timestamps are absolute and monotonic, and whose content at
> each timestamp is what is actually there.*

Nothing in this run ships to a customer. **Correctness is the deliverable.** Performance and
reliability are a real but **secondary** goal (M6) — do not trade correctness for them.

## Your role: coordinator, not coder

- You sequence milestones, launch worker sub-agents (complete context per prompt), verify their
  output, commit, and move on. Prefer delegating implementation; keep your own context for
  orchestration, verification and recovery. ⚠️ **Set a checkpoint expectation in every worker
  prompt** (files written + tests run); if a worker returns nothing usable, re-implement directly
  rather than re-delegating.
- Track everything in todos (one per milestone task, kebab-case ids, explicit dependencies).
- Each milestone ends in 1–3 commits direct to main — **git history IS the checkpoint.**
- Update the PLAN.md STATUS block at every milestone boundary, in the same commit.

## Authorization contract (founder-approved for THIS RUN only)

1. **🚨 Vertex spend: NO BUDGET CAP for this run.** The founder has explicitly lifted it. Correctness
   is worth more than the money. Conditions, all still mandatory:
   - Every LLM call **and** compute step recorded in the ledger (CLAUDE.md rule 6). The ledger now
     prices per modality and carries microcents — use it, and **report actual spend in the final
     report** even though nothing caps it.
   - Rule 9 still applies to every Stage B call: `maxOutputTokens`, bounded `thinkingBudget`, and a
     wall-clock timeout **sized for the path** (the YouTube path measured 24–70 s per 10-min segment;
     the 90 s default was sized for the private path and is too tight there — ARCHITECTURE §3).
   - "No cap" is not "no attention": if a single video's cost looks like the METRICS.md **N4d**
     over-segmentation signature, stop and investigate rather than paying it repeatedly.
2. **🚨 Rule 8 is ABSOLUTE and is NOT relaxed by this run.** **Never download YouTube bytes.** No
   `yt-dlp`, no scraping, no "just this once for a test fixture", no third-party mirror of a YouTube
   video. YouTube is reachable **only** through Vertex `fileData.fileUri`. This run needs real video
   *files*, and it gets them from publishers that serve the media themselves — see "Where video
   files come from" below. If you find yourself reasoning toward a YouTube download, **stop: that is
   the hard stop, and the answer is a different source.**
3. **Rule 2, hard**: EU only. Vertex `europe-central2` with the `europe-west3` fallback; buckets
   `europe-central2`. Any non-EU resource, **including the `global` Vertex endpoint**, = hard stop.
4. **Rule 1**: no secrets in git, no GCP JSON keys. ADC locally. Scan every diff before commit.
5. **No deploys this run.** Rule 5 stands unmodified — this is a correctness run, and nothing it
   produces should reach a deployed service. Deployed services stay `PipelineModel__Mode=fake`.
6. **No new continuously-billing resources.** Vertex inference is usage-based, not a new fixed base.
7. **Hard stops** (halt, write the report, wait): downloading YouTube bytes; any non-EU endpoint;
   a live Stripe key; destructive operation on prod data; git history rewrite; **making any GitHub
   repo public**; publishing anything.

## 🎥 Where video files come from (settled — do not relitigate)

The private path needs real bytes. The compliant sources, in priority order:

1. **`video.fosdem.org`** — FOSDEM self-hosts its recordings under CC-BY. The 2026-07-21 licence
   audit found FOSDEM's *YouTube* archive stops in 2020 precisely because recent editions live here.
   This is the richest legal source of real conference video files in Europe.
2. **`media.ccc.de`** (CCC), **Internet Archive**, **NASA**, **Wikimedia Commons** — all publish
   media files directly under redistributable licences.
3. **`tests/fixtures/videos/`** — three 90-second clips already committed, from exactly these
   sources. Read their README before adding more; it explains the rule-8 reasoning in full.

> 💡 **The highest-value property of this sourcing, and the reason it beats downloading from
> YouTube even if that were allowed:** many talks exist on **both** a self-hosted archive **and**
> YouTube. That gives you the *same talk through both ingestion paths*, which turns cross-path
> equivalence from an aspiration into a diffable assertion — and it is exactly the test that would
> have caught the timestamp defect on day one.

Verify and record the licence of every file you fetch (`corpus.json` audit-trail pattern). Do not
commit anything over ~3 MB; keep long-form sources out of git and reference them by URL in a
manifest, downloading to `.local-state/` at test time.

## 🐛 The defect this run exists to kill (already diagnosed — do not re-diagnose)

**Stage C fuses segment-relative block timestamps as if they were absolute video time.**

- `AnalyzedBlock.At` is an offset **within its segment** (`StageBRunner` sets it from the
  normalizer; the coverage guard divides by segment duration).
- **Nothing anywhere adds `SegmentStart`** — grep it: every use is ordering, never arithmetic.
- `VertexTextFuser.BuildPrompt` then tells the model *"timestamps are global video time"* and hands
  it raw offsets. The model believes the prompt and **merges what look like duplicate early
  timestamps across segments**, so later content is not merely mis-stamped — some is destroyed as a
  false duplicate.
- Symptom: a video is 3–4× longer than its last timestamp (10-minute segments, so a 40-minute video
  is four segments stacked onto the first ten).
- **`PrivatePipelineService` feeds the same fuser the same way — the paid path is equally broken.**

🚩 **Why no test caught it, which constrains how you fix it:** `FakeTextFuser` computes section
timestamps from `segment.SegmentStart` and therefore gets it *right*, while the real fuser does not.
Every unit, integration and E2E test runs on fake or replay. The suite is not merely missing a
case — **it is structurally incapable of seeing this class of bug.**

⇒ **A fix is not accepted until a test that does NOT use `FakeTextFuser` fails before it and passes
after it.** That is the acceptance criterion for M1, and it is non-negotiable.

## Settled decisions (do not relitigate)

- **Fix the offset at the source, not in the prompt.** Convert to absolute video time in the
  pipeline (`StageBRunner`/the runners) so `SegmentAnalysis.Blocks` carry absolute `At` values, and
  make the fusion prompt's claim true rather than asking the model to do arithmetic. A model told to
  add offsets will do it *mostly*, and "mostly" is indistinguishable from this bug.
  ⚠️ Whatever you choose, **`AnalyzedBlock.At` must have exactly one documented meaning** across
  Stage B's coverage guard (which needs offsets) and Stage C (which needs absolute). Today the same
  field silently means both. Name it, document it, and make the compiler or a test enforce it.
- **`FakeTextFuser` must stop being accidentally-correct.** Either make it as naive as the real
  fuser about offsets, or make it obviously a stub. A fake that is *more correct* than production
  hides production bugs — that is the lesson of this whole run, and it generalises to every seam.
- **`tests/Live/` is the home for real-Vertex assertions.** It already exists
  (`VertexStageBLiveTests`). Correctness claims about real model behaviour belong there, gated
  `Category=Live`, so the default suite stays hermetic and offline (TESTING.md).
- **Ground truth is human-checkable, not model-asserted.** Where a model grades a model, say so and
  treat the number as weak evidence. Prefer assertions a person could verify by opening the video at
  the timestamp: monotonicity, coverage, duration agreement, verbatim on-screen strings.
- **Do not weaken the §4 contract to make output pass.** If real output violates the contract, the
  output is wrong. `tests/fixtures/output/` and the schemas are the oracle — extend, never loosen.
- **`web/lib/repository.test.ts` accepts `MDREEL_REPOSITORY_DIR`** — point oracles at real generated
  artifacts, not only at fixtures. That trick found the anchor bug in seconds; reuse it.
- **No mocks in the correctness harness.** `PipelineModel__Mode=live` throughout M2–M5. Replay
  fixtures are for the *hermetic* suite, not for proving correctness.
- **Do not re-produce the 25-session collection this run** (founder decision). Prove the fix on a
  small golden set. Curation needs redoing anyway — the corpus was machine-selected on licence alone
  and fails the restored ICP-recognition criterion (DISTRIBUTION.md).
- **Concurrency and short backoff are settled** (INFRA.md): Vertex 429s are Dynamic Shared Quota
  contention, we sit at ~1% of a non-adjustable limit, and the success rate is flat at ~25% at every
  concurrency level. Do not re-derive this. Do not add pacing.
- Numbers live in METRICS.md; cite by name. `scripts/check-docs.sh` enforces it.

## Milestones (dependency order)

### M0 — Guardrails + reproduce the defect (do FIRST; nothing else starts until committed)
- `scripts/preflight.sh` green; ADC reaches Vertex in `europe-central2`; confirm no deployed service
  changes are in scope.
- **Write the failing test first.** A test that runs a **multi-segment** video through the *real*
  fuser (fake analyzer is fine; the fake **fuser** is not) and asserts the last section is within a
  tolerance of the video duration. Use **at least 4 segments** — a two-segment case can pass by
  luck when the second segment happens to start where the first ended. It must FAIL on current
  `main`. Commit it failing-but-skipped, or
  commit it red in a clearly-marked quarantine — the point is that the defect is captured as an
  executable fact before anyone touches the fix.
- Record the no-budget-cap authorization for this run in INFRA.md. Commit `test:`/`infra:`.

### M1 — 🎯 Fix absolute timestamps on BOTH paths (depends on M0) — **the milestone that matters**
- Make `SegmentAnalysis.Blocks[].At` absolute video time by the time Stage C sees it, on the YouTube
  runner **and** `PrivatePipelineService`. Keep the Stage B coverage guard working on offsets.
- Give the field one meaning. If two meanings are genuinely needed, use two types or two names —
  not one field and a convention.
- Fix `FakeTextFuser` so it can no longer mask this (see settled decisions).
- Tests: the M0 test goes green. Add: monotonic-across-segments, coverage-to-duration, and an
  overlap case where two segments describe the same moment (the 20 s overlap is deliberate — merging
  must key on *absolute* time).
- Doc: ARCHITECTURE §3/§4 states the absolute-time invariant explicitly.
- Commit `fix: absolute section timestamps across segment boundaries`.

### M2 — Golden corpus, both paths, real bytes (depends on M0; parallel-safe with M1)
> 🚨 **The existing fixtures cannot test this and never could.** All three committed clips in
> `tests/fixtures/videos/` are **90 seconds** — single-segment. A single-segment video has no
> segment boundary, so it cannot exhibit a cross-boundary timestamp bug **by construction**. This is
> the same failure shape as the accidentally-correct fake fuser: the test asset is not merely
> missing a case, it is incapable of the case. **Length is a test dimension, not a detail.**

- **🎯 The anchor case is LONG: at least one source >60 minutes.** At 10-minute segments that is
  7+ segments, which is where the defect is loudest (7× compression) and where two other known
  risks bite simultaneously: METRICS.md **N4d** output-cap overflow on dense stretches, and the
  Stage B halving cascade. **A run that proves correctness only on short video has proved almost
  nothing** — the founder found the bug on a 25-minute talk, and 25 minutes is the *easy* case.
  Conference keynotes and workshop recordings on `media.ccc.de` and `video.fosdem.org` routinely
  run 60–120 minutes; prefer one with slides so on-screen text is checkable.
- Assemble **6–10 sources** in total, licence-verified and recorded (`corpus.json` pattern), and
  make the length distribution deliberate:

  | Case | Why it is in the set |
  |---|---|
  | **>60 min** (≥1, required) | 7+ segments. The anchor case — if this is right, the mechanism is right. |
  | **20–40 min** (≥2) | The shape of a normal conference talk, and where the founder found the bug. |
  | **<3 min** (≥1) | Single-segment. Guards against a "fix" that only works when boundaries exist. |
  | **Dual-source** (≥3, may overlap the above) | Same talk self-hosted **and** on YouTube → the cross-path equivalence pairs (M5). |
  | **Screen recording / live-coding** (≥1) | METRICS.md **N32** — the weakest measured category, and the ICP's own content type. |
  | **Silent or speechless** (≥1) | The Phase-0 edge case: must produce no transcript, not a hallucinated one. |

- Manifest + a fetch script into `.local-state/`; **commit the manifest, not the bytes** — a
  60-minute file does not belong in git. Record duration in the manifest so the harness can assert
  coverage against a known truth rather than against what the model reports.
- ⚠️ Long sources are also the expensive and slow ones. That is the point; the budget cap is lifted
  for exactly this. But run the long case **early** in the milestone, not last — if something breaks
  at segment 7, you want to know at hour two, not hour seven.
- Commit `test: golden correctness corpus (dual-path, licence-verified, long-form anchored)`.

### M3 — The correctness harness (depends on M1, M2)
- One command runs every golden source through **both** paths with `Mode=live` and asserts, per
  document: §4 contract validity · absolute monotonic timestamps · **coverage ≥ a stated fraction of
  true duration** · frontmatter duration agrees with the source · no section beyond the video end ·
  no duplicate timestamps.
- **Independent re-probe for content correctness**: for N sampled sections, a second differently-
  prompted Vertex call seeks to that timestamp and describes what is there **without seeing the
  claim** (the Phase-0.2 method behind METRICS.md N30–N32). Sample **content-blind**, and verify a
  random sample of *rejected* candidates too — surprising and fabricated correlate.
- Emit a machine-readable report + a human summary. **The harness must fail loudly on a bad
  document rather than reporting a percentage that looks fine.**
- Commit `test: end-to-end correctness harness (no mocks)`.

### M4 — Guards that make the defect unrepresentable (depends on M1)
- Stage C gains a **coverage guard**, the counterpart to Stage B's: a fused document that does not
  span the video is a failure, not a result. This is the guard whose absence let the bug ship.
- The §4 schema/validator asserts monotonic, in-range timestamps.
- Extend the spot-verify gate (`experiments/006-*/scripts/spot_verify.py`) to check **coverage** —
  today it samples sections that exist, so it would have passed this defect.
- Commit `feat: stage C coverage + timestamp-range guards`.

### M5 — Cross-path equivalence (depends on M1, M2, M3)
- For each dual-source talk, run YouTube `fileUri` **and** the private byte path, and assert the
  documents are *equivalent*: same duration, comparable section count, timestamps agreeing within a
  tolerance, same on-screen strings where both saw them.
- Where they differ, **say why** — different media resolution, Stage A cue forcing on the private
  path only, fetch clamping. Differences are findings, not noise; record them.
- ⚠️ This is the run's most likely source of a genuine surprise. Budget time for it.
- Commit `test: cross-path equivalence on the golden corpus`.

### M6 — Performance & reliability (SECONDARY — only after M1–M5 are green)
- Apply the concurrency lesson to the **private** path: `PrivatePipelineService.ExecuteStageBAsync`
  is still a sequential `foreach` (line ~475) and has the same starvation profile the collection
  batch had.
- Measure and record: wall-clock per video-hour on both paths vs METRICS.md **N9**; Stage B runaway
  rate vs **N7**; retry waste (a rejected segment still bills — the 2026-07-21 batch showed retry
  persistence, not content density, drove cost).
- Reliability: a failure classification (capacity vs quality vs contract) and what each should do.
  **Give up cheaply and defer** is the established answer for capacity (DISTRIBUTION.md).
- Commit `perf: private-path concurrency + measured reliability profile`.

### M7 — Stretch, only if everything above is green
- Re-produce **one** collection session end-to-end with the fixed pipeline and re-validate the
  repository against the §4b oracle, as proof the whole chain works. Do not re-produce all 25.

## Definition of done — EVERY milestone, before its commit (CLAUDE.md rule 4)

build clean (warnings-as-errors) → `dotnet test --filter Category!=Live` green (compose up) →
`cd web && npm test` green → `scripts/e2e.sh up && scripts/e2e.sh full` green → `dotnet format`
(format only your files) → no secrets in diff → impacted living doc updated in the same commit →
`scripts/check-docs.sh` passes → commit to main with `feat:`/`fix:`/`test:`/`perf:`/`docs:` prefix
and trailer `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` → push.
**`dotnet test tests/Live` runs before any milestone that claims real-model correctness.**
No deploys this run.

Machine quirks: run repo bash scripts via `& 'C:\Program Files\Git\bin\bash.exe' scripts/x.sh`
(plain `bash` is a broken WSL relay). Outbound TCP 5432 is blocked — DB checks go through the Cloud
SQL Auth Proxy pattern in `smoke-remote.sh`. A running `MdReel.Worker` **locks the build output**:
either stop it before rebuilding, or build to a separate path
(`--property:BaseOutputPath=.local-state/build/<name>/`, as `scripts/generate-collection-repo.sh`
does).

## Final report (end of run, or at any hard stop)

Write to PLAN.md STATUS and reply with: milestones completed with commits; **the falsifiable claim
at the top of this prompt, answered yes or no, with the evidence**; what failed and why; actual
Vertex spend (cite METRICS.md names — N4c/N4d/N7/N9 — never restate figures); the NEEDS-FOUNDER
checklist; and the ranked next-run backlog.

Three things to report explicitly:

- **Coverage, measured, ordered by source length.** For every golden source, on both paths: what
  fraction of true duration the document spans. This is the number the founder found wrong by hand;
  report it as a table, **longest source first**. If coverage degrades with length, that is the
  headline finding of the run and it means the fix is incomplete — a mechanism that works at 3
  segments and fails at 7 is a different bug wearing the same clothes.
- **Where the two paths disagree, and whether each difference is explained or unexplained.** An
  unexplained divergence is a finding, not a rounding error.
- **What the test suite still cannot see.** This run exists because a fake was more correct than
  production. Say plainly which other seams have that shape — `FakeVideoAnalyzer`, `InProcessQueue`,
  `FakePaymentGateway`, `LocalDirectoryObjectStorage` — and which of them could be hiding a
  production bug right now. **Do not fix them all; name them and rank them.**
