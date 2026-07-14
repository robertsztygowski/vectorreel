# PLAN — Benchmark, Demand Instrument & MVP Implementation

> **Living doc, and the authority on sequencing — what to do next, in what order.**
> ARCHITECTURE.md is the authority on the *target design*; this file owns the *order it gets
> built in*. Numbers and decision rules live in METRICS.md and are never restated here.
> Living-doc rules apply (DEVELOPMENT.md §7).

---

# 🎯 STATUS — read this first

**The goal we steer by: ≈2–3 retained paying accounts** (METRICS.md **N1a**). That covers infra.
**The founder does not need a salary from this in the near term**, so N1a — not the 47-account
job-replacement figure (N1b) — is the near-term target. **Do not plan against N1b.**

**The bet.** EU teams building internal AI assistants have recordings their assistant cannot see.
We turn those into timestamped Markdown it can cite, without the footage leaving the EU.

**What is already settled.** Feasibility and margin. The pipeline works and COGS is measured at
~7× inside the cheapest price (METRICS.md §1.2, A8). **Further cost engineering is procrastination
in a lab coat.**

**What is not settled — and it is everything that matters.** Demand, pricing, retention, and above
all **whether anyone can be reached at all.** All five open assumptions live in **METRICS.md §2**.

> ## 🚨 The one risk that governs the plan: **A5 — distribution.**
> The founder has ruled out outreach, so the motion is **self-serve / inbound**. That makes
> **traffic the long pole**: A1, A2, A3 and A4 are each only *reachable* through it, and content
> compounds over **months**. Building for three months and *then* starting to publish serializes
> the two slowest processes in the company.
>
> **⇒ The governing rule: start the slowest thing (distribution) first, and run it in parallel
> with all engineering.** That single rule is why the phases below are ordered as they are.

**The one number to hold in your head: METRICS.md N15.** It is doing double duty — **the traffic
that pays for the infra (N1a) and the traffic that tells you whether to continue (the A2 sample
floor) are the same traffic.** It is a good-post-sized number, not a content-engine-sized number.

**Where we are now.** Phase 0 ✅ done. **Next: Phase 0.1** (YouTube `fileUri` spike, ½ session,
≲ €1) — it is the critical path, because 0.1 gates 0.2, and 0.2 gates the entire demand
instrument in 0.3.

> ### 🧟 **How this ends — and with no salary to burn, this is the rule that matters most.**
> At N1a the business is cash-flow-positive and could run **forever** on ~€300/mo. Nothing forces a
> stop. **The scarce resource is no longer money — it is founder-hours, and they appear on no
> dashboard.** So the kill criterion is a **calendar deadline (METRICS.md §2.2 — T0 + 9 months)**,
> not a cash threshold.
>
> **If N15 is not reached by T after a sustained publishing effort — stop.** Not "reduce scope",
> not "try one more channel". Stop. **The failure mode this guards against is not going broke; it
> is a zombie that quietly eats three years of evenings.**

---

## How to use this file

Start each phase in a **clean Claude Code session** with the starter prompt given below.
CLAUDE.md auto-loads the hard rules; the prompt only needs to point at the phase.

- Model: **Fable 5** for pipeline/IP phases, **Sonnet 5** acceptable for CRUD/UI phases.
- Use **plan mode** at the start of every phase before writing code.
- Every phase ends with the definition of done (CLAUDE.md rule 4) and a commit.

---

## Which phase settles which assumption

**The assumptions themselves, their evidence, and their decision rules live in METRICS.md §2 —
they are not restated here.** This table is only the map from assumption → phase.

| | Assumption (METRICS.md §2) | Settled in |
|---|---|---|
| **A5** | Distribution works. **Top risk.** | **Phase 0.3 onward** |
| **A1** | EU residency is a purchase driver | Phase 0.3 landing-page headline A/B |
| **A2** | Buyers buy rather than DIY | Phase 4 Stripe checkout |
| **A3** | Recurring flow, not one-time backfill | Phase 4 cohort hour-decay — **plus the N20 signup field, which reads it two months earlier** |
| **A4** | Output is citable in a KB | Phase 0.2 public benchmark corpus; extended by `upload_repeat` in Phase 3 |

⚠️ **Do not invoke a decision rule below its sample floor (METRICS.md §2.1).** The most likely way
this plan fails is not a bad number — it is a *verdict called on 40 visitors*.

---

## Test assets

| File | Properties | Role |
|---|---|---|
| `~/Downloads/Isolation Component V2 demo.mp4` | 50:40, 1080p @ 16 fps, spoken audio, 293 MB | **Company-internal — NOT publishable.** Long-form demo screen-recording; cost benchmark; private golden-test source |
| `~/Downloads/HabiCen Ticket System.mp4` | 1:52, silent AAC track, 99 MB | **Company-internal — NOT publishable.** Edge case: audio stream present, nothing spoken — must output no transcript, not hallucinate one |
| **Public CC-licensed YouTube corpus** (Phase 0.2) | 4–6 videos | **Publishable.** Closes the A4 category gap, seeds `tests/fixtures/videos/`, *and* becomes the public demo. See Phase 0.2. |

Both internal videos are unshareable and too large to commit. This is why the public corpus
exists: it solves the demo problem, the benchmark-category gap, and the committable-fixture
problem in one move.

---

## Phase 0 — Benchmark experiment (`experiments/001-gemini-video-benchmark/`) ✅ DONE 2026-07-14

> **Completed.** All 4 questions answered — see `experiments/001-gemini-video-benchmark/out/RESULTS.md`.
> Headlines: `gemini-2.5-flash` @ `europe-central2` confirmed; €0.38/video-hour default
> (€0.21 low-res, blended ~€0.25–0.30) vs €1.50 guardrail; no hallucinated speech on silent
> video; 67% static-content lever. Spend: €1.91 of €5.

**Two findings graduated late (from the Workflow-1 memo, not the original RESULTS.md):**

1. **~8% degenerate-generation rate.** `seg4_configA` (474 s, 61k output tokens) and
   `seg2_configB` (323 s, 63k thinking tokens) ran away. This is the source of the observed
   1.3× retry overhead and it will break the <15 min/video-hour SLO. → hard output-token caps
   + timeouts are now a Phase 2 requirement.
2. **The cost ledger is missing ~30% of true COGS.** ffmpeg transcode/segmentation on Cloud Run
   is ~€0.15/video-hour (estimate) and is not metered anywhere. → CLAUDE.md rule 6 extended to
   compute, not just LLM calls. Phase 2 requirement.

---

## Phase 0.1 — YouTube-ingestion spike (½ session, ≲ €1) 🔜 NEXT

**Goal: verify the one fact the next three phases depend on.** Same methodology as Phase 0 —
prove EU availability *before* designing around it.

Vertex Gemini can ingest a YouTube URL directly via `fileData.fileUri` (public videos only,
one per request) — meaning **we never download the video; Google fetches it.** No `yt-dlp`,
no YouTube ToS exposure. That matters for a brand selling trust to DPOs.

Questions:

1. Does **`europe-central2`** accept a YouTube `fileUri`? (Fallback `europe-west3`.) If YouTube
   URLs only resolve in `us-*`, Phases 0.2–0.3 need rethinking.
2. Does **`videoMetadata.startOffset/endOffset`** clip a YouTube URL by time range? If yes,
   long videos can be segmented **without ever holding the bytes** → the YouTube path becomes a
   simpler parallel pipeline (no Stage A, offsets instead of ffmpeg). If no, we're capped at
   whatever fits one request.
3. Confirm cost. **Stage A cannot run on a YouTube URL** (no local bytes → no ffmpeg → no
   static-content detection), so the ~67% static-content lever is **unavailable** on this path.
   Expect the **un-blended** figure, materially above the blended one (METRICS.md §1.2). Verify —
   this is what sizes the abuse caps (METRICS.md N10).

⚠️ **Known constraint, do not design around it:** VPC Service Controls **disables `fileUri`
media URLs entirely.** If the project is ever locked down for enterprise compliance, the
YouTube path dies. This is one more reason it belongs in marketing, **never** in the core
paid product.

**Starter prompt:**
> Read PLAN.md Phase 0.1. Extend the existing `experiments/001-gemini-video-benchmark/`
> harness with a YouTube `fileData.fileUri` path. Answer the 3 questions. Plan mode first.
> Keep spend under €1.

---

## Phase 0.2 — Public benchmark & demo corpus (1 session, ≲ €3)

**Goal: one set of videos that closes A4, produces publishable demos, and seeds committable
test fixtures.** The internal videos can do none of these — they are unshareable.

Pick **4–6 Creative-Commons-licensed** YouTube videos (YouTube has a CC license filter — use it):

- 2 × **conference talk** (slide-heavy) — closes the missing "slide-talk" category (A4)
- 2 × **interview/podcast** (talking-head) — closes the missing "talking-head" category (A4)
- 1 × long **screen-recorded tutorial** — public analogue of the internal demo video

Run each through the Phase 0.1 YouTube path with the ARCHITECTURE §3 Stage B schema.
Deliverables:

1. **A4 verdict per content category** — is `on_screen_text` verbatim? Are timestamps accurate?
   Any hallucination on the talking-head footage (which has almost no on-screen text — the
   inverse of the silent-video edge case)?
2. **Publishable `output.md` per video** → raw material for Phase 0.3.
3. **CC-licensed clips committed to `tests/fixtures/videos/`** — fixes the "test assets are
   293 MB and unshareable" problem; makes golden tests reproducible by anyone.

**Starter prompt:**
> Read PLAN.md Phase 0.2 and experiments/001-*/RESULTS.md. Select the CC-licensed corpus,
> run the Stage B benchmark on each, produce the A4 category verdicts. Plan mode first.

---

## Phase 0.3 — Demand instrument: publish (no product code) 🚨 THE LONG POLE

**This is the phase that decides the business, and it must start now and run in parallel with
every engineering phase below.** Do not serialize it after the build.

Rationale: with no outreach, the funnel needs a **content-engine-sized** number of qualified
visitors to produce a handful of paying customers (METRICS.md §1.4, N14–N15 — and note both input
rates are *assumptions*). Content compounds over **months**. Building for three months and *then*
starting to post serializes the two slowest processes in the company.

Work:

1. **Landing page headline A/B** (page is already deployed on Cloud Run):
   - Arm A — *"Your recordings never leave the EU."*
   - Arm B — *"Your AI assistant can't see what's on screen in your videos."*
   - **This is the A1 test.** If Arm A does not clearly beat Arm B, **EU is a checkbox, not a
     wedge**, and all positioning moves to the capability story. Directional, not significant.
2. **Email capture.** Nothing more.
3. **The artifact post** — the single best inbound asset, and it's already written: side-by-side
   *plain transcript vs. vectorreel Markdown*, plus a RAG answering a question that is **only**
   answerable from on-screen content. Now built on a **public** video from Phase 0.2, so it can
   actually be shared. Channels: LinkedIn (existing .NET/architecture audience — the cheapest
   traffic source available, by an order of magnitude), HN, r/RAG, r/LocalLLaMA.

> **Broadcasting is not outreach.** This requires zero 1:1 contact with anyone.

---

## Phase 1 — Core + Stage A

`src/Core` (domain, provider interfaces, pipeline skeleton) + Stage A (ffprobe/ffmpeg wrapper,
segmentation, static detection — port learnings from Phase 0). Golden tests on the **public CC
clips** from Phase 0.2. docker-compose.yml (postgres + fake-gcs-server) lands here.

**Starter prompt:**
> Read PLAN.md Phase 1, ARCHITECTURE.md §3 Stage A, experiments/001-*/RESULTS.md.
> Plan mode first, TDD for segmentation/static-detection per DEVELOPMENT.md §4.

## Phase 2 — Stages B/C/D

Stage B against Vertex with structured output; record/replay fixtures + `tests/Live/`;
Stage C fusion; Stage D persist + output renderer; source deletion.

**Two new hard requirements from the Phase 0 late findings:**

- **Hard output-token caps + request timeouts on Stage B.** ~8% of benchmark calls ran away
  (61k output tokens / 63k thinking tokens). Untreated, this breaks the <15 min SLO and is the
  1.3× retry overhead.
- **Per-job cost ledger meters compute, not just LLM calls.** ffmpeg/Cloud Run transcode is
  ~30% of true COGS and is currently invisible. CLAUDE.md rule 6 now covers it.

**Second ingestion path:** the YouTube path (Stage B directly on `fileData.fileUri`, no Stage A,
offset-based segmentation) lands here as a parallel, simpler pipeline. Free/public only.

## Phase 3 — Thin trial slice (collapses old Phases 1 / 4 / 5)

**The product is the validation instrument. Build only the instrument.**

- **Free YouTube tool — the top-of-funnel hook.** Paste a YouTube URL → get Markdown. Zero
  friction, zero trust required, wow in 60 seconds. This replaces "upload your confidential
  internal recording to a stranger's website" as the first ask — which was a brutal opening
  move and the biggest leak in the old funnel.
  - ⚠️ **Abuse controls, day one:** cap at first ~10–15 min, **cache by video ID**, rate-limit
    per IP. The YouTube path can't use the static-content lever, so it runs at the un-blended cost
    (METRICS.md §1.2) — **1,000 abusive hour-long videos is real money against a fixed base this
    small.** Enforce METRICS.md N10 (daily spend) and N11 (cache-hit rate); caching makes a popular
    gallery page cost ≈ €0 on repeat views.
- **Curated public gallery — 10–25 processed talks**, each with attribution + the original video
  embedded. **Not a scaled content farm.** Mass auto-generated transcript pages are exactly what
  Google's scaled-content-abuse policy targets, and republishing copyrighted transcripts at
  volume is a fight we cannot afford. Curated + CC-licensed + attributed is defensible.
- **The real test:** magic-link signup → upload **your own** recording → Markdown back by email.
  2 h free. Nothing else.

Infra: Cloud Run + GCS + in-process queue. **No Cloud SQL, no Cloud Tasks, no Terraform yet** —
Cloud SQL idles at ~€25–50/mo, which is real burn for a bootstrapped business whose break-even
is ~47 accounts.

**Explicitly NOT built:** dashboard, job list, usage page, markdown preview, connectors, plan
tiers, DPA self-service flow, MCP server.

**The funnel:** paste a YT link → see real output → *"now try it on your own recording"* →
signup, 2 h free → payment.

> **The discipline to hold: the YouTube tool is free and public; the paid product is private
> recordings.** The moment YouTube processing becomes a paid tier, we have changed businesses —
> and walked into the BUSINESS_MODEL §8 buyer-confusion risk ("is this a YouTube tool?").
> YouTube ingestion *cannot* serve the ICP anyway: Vertex only accepts public videos or ones
> owned by **our** GCP account, so customers' unlisted/private recordings will never work.

## Phase 4 — Payment link + funnel instrumentation

- **ONE Stripe payment link.** Not three tiers. Not metered overage. Not a free-trial gate.
  → **This is the A2 test.** Nobody clicks it = it's a vitamin, not a painkiller.
- **Instrumentation — the whole point of the phase.** Events: signup, YT-tool use, upload,
  download, **second upload**, checkout click.
- 🚨 **Cohort hour-decay, instrumented before the first user.** Hours uploaded in week 1 vs.
  week 4, per signup cohort. **This is the A3 test, and A3 decides what business we are in:**
  if month-2 hours < 20% of month-1, this is a **backfill business** and the correct product is
  a **prepaid credit pack** (*"€200 for 20 hours"*), **not** a €149/mo subscription. That
  question cannot be answered retroactively — instrument it first or lose the data forever.

## Phase 5 — Read the data → choose the pricing model

Was *"lock pricing / onboard design partner #1"*. Pricing is now an **output** of the cohort
data, not an input decided in advance. Rerun the benchmark matrix through the real pipeline;
set pricing from A3; write the verdict into BUSINESS_MODEL §6.

**Kill criteria — the honest version.** If, past the sample floors (METRICS.md §2.1): nobody pays,
the EU headline arm loses the A/B, and cohort hour-decay says backfill — **stop.** The TAM
(METRICS.md N16) does not justify pushing uphill against a DIY alternative our own customers are
qualified to build.

---

## Deferred until there is a paying customer

Terraform / Cloud SQL / Cloud Tasks · dashboard + usage UI · plan tiers + metered overage ·
connectors (Drive/SharePoint/Zoom) · MCP server · DPA self-service flow · self-hosted edition.
