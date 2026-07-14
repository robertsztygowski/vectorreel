# PLAN — Benchmark, Demand Instrument & MVP Implementation

> Session-by-session execution plan. Companion to ARCHITECTURE.md (§9 build order is the
> authority on *what*; this file adds *how to run the sessions*). Check off / update as
> phases complete — living doc rules apply (DEVELOPMENT.md §7).

## How to use this file

Start each phase in a **clean Claude Code session** with the starter prompt given below.
CLAUDE.md auto-loads the hard rules; the prompt only needs to point at the phase.

- Model: **Fable 5** for pipeline/IP phases, **Sonnet 5** acceptable for CRUD/UI phases.
- Use **plan mode** at the start of every phase before writing code.
- Every phase ends with the definition of done (CLAUDE.md rule 4) and a commit.

---

## Governing principle (added 2026-07-14, after `experiments/workflow1-decision-memo.md`)

The de-risking pass ("should I build this at all?") produced one structural finding:

> **Feasibility and margin are measured and safe. Demand, pricing and retention have zero
> evidence. The previous plan deferred all demand evidence to its final phase.**

Because the GTM is **self-serve/inbound, with no founder outreach**, traffic is the long pole —
it compounds over months. So the plan is reordered on one rule:

**Start the slowest thing (distribution) first, and run it in parallel with all engineering.**

The five open assumptions this plan exists to answer — full detail in the memo:

| | Assumption | Where it gets tested |
|---|---|---|
| **A1** | EU residency is a *purchase driver*, not a checkbox | Phase 0.3 landing-page headline A/B |
| **A2** | Buyers will buy rather than DIY with Gemini | Phase 4 Stripe checkout |
| **A3** | Usage is recurring **flow**, not one-time **backfill** | Phase 4 cohort hour-decay metric |
| **A4** | Output is accurate enough to be *citable* in a KB | Phase 0.2 public benchmark corpus |
| **A5** | **Distribution works** — traffic can be acquired inbound | Phase 0.3 onward. **Top risk.** |

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
   static-content detection), so the 67% cost lever is **unavailable** on this path. Expect the
   full ~€0.38–0.45/video-hour, not the blended €0.26. Verify.

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

Rationale: with no outreach, the funnel needs roughly **2,000–5,000 qualified visitors to
produce ~5 paying customers** (2–5% visitor→trial, 5–15% trial→paid). Content compounds over
months. Building for three months and *then* starting to post serializes the two slowest
processes in the company.

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
    per IP. The YouTube path can't use the static-content lever (~€0.45/video-hour), so
    1,000 abusive hour-long videos ≈ €450 — real money against a ~€300/mo fixed base. Caching
    makes a popular gallery page cost €0 on repeat views.
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

**Kill criteria — the honest version.** If, after ~100 trials: nobody pays, the EU headline arm
loses the A/B, and cohort hour-decay says backfill — **stop.** The ~€65M TAM does not justify
pushing uphill against a DIY alternative our own customers are qualified to build.

---

## Deferred until there is a paying customer

Terraform / Cloud SQL / Cloud Tasks · dashboard + usage UI · plan tiers + metered overage ·
connectors (Drive/SharePoint/Zoom) · MCP server · DPA self-service flow · self-hosted edition.
