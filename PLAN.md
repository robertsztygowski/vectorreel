# PLAN — Benchmark, Demand Instrument & MVP Implementation

> **Living doc, and the authority on sequencing — what to do next, in what order.**
> ARCHITECTURE.md is the authority on the *target design*; this file owns the *order it gets
> built in*. Numbers and decision rules live in METRICS.md and are never restated here.
> Living-doc rules apply (DEVELOPMENT.md §7).

---

# 🎯 STATUS — read this first

**The goal we steer by: ≈2–3 retained paying accounts** (METRICS.md **N1a**). That covers infra.
**The founder does not need a salary from this in the near term**, so N1a — not the job-replacement
figure (N1b) — is the near-term target. **Do not plan against N1b.**

**The bet.** EU teams building internal AI assistants have recordings their assistant cannot see.
We turn those into timestamped Markdown it can cite, without the footage leaving the EU.

**What is already settled.** Feasibility and margin. The pipeline works and COGS is measured at
~7× inside the cheapest price (METRICS.md §1.2, A8). **Further cost engineering is procrastination
in a lab coat.**

**What is not settled — and it is everything that matters.** Demand, pricing, retention, and above
all **whether anyone can be reached at all.** All five open assumptions live in **METRICS.md §2**.

> ## 🚨 The one risk that governs the plan: **A5 — distribution.**
> The founder has ruled out outreach, so the motion is **self-serve / inbound** — traffic is the
> long pole, and A1–A4 are each only *reachable* through it.
>
> **⇒ The sequencing rule — REVERSED 2026-07-15: ship the MVP first (Phases 2–4), then launch
> everything at once (Phase 5).** The old rule ("start distribution first, in parallel") would have
> pointed the one-shot launch surfaces (HN, LinkedIn, Reddit) at an email-capture box converting in
> the low assumed range, instead of at a live product — gallery, trial credit, checkout — which is
> what gives cold traffic something to convert on (METRICS.md §1.6). The ad-tranche gate ("page **and gallery**
> live") already contradicted the old sequencing (old 0.3 open question #1); launching after the
> MVP resolves it by construction, and the two tranches merge into one (METRICS.md N26/N27).
>
> **The price of the reversal is a new zombie surface: the build itself.** The T-clock does not
> start until launch, so the MVP is gated by a hard **ship-by date (METRICS.md §2.2 SB)** — if the
> date arrives first, **launch with whatever exists.** "Further engineering is procrastination in a
> lab coat" now applies to Phases 2–4.

**The one number to hold in your head: METRICS.md N15.** It is doing double duty — **the traffic
that pays for the infra (N1a) and the traffic that tells you whether to continue (the A2 sample
floor) are the same traffic.** It is a good-post-sized number, not a content-engine-sized number.

**Where we are now.** Phase 0 ✅, Phase 0.1 ✅, Phase 0.2 ✅, **Phase 1 ✅ done** — `src/Core` and Stage A
are built, and the weakest measured category has a fix (METRICS.md **N7c**) whose *effect on the model*
is still unproven. Phase 0.3 as a standalone phase is **superseded** — its content lives in
**Phase 5 — LAUNCH**. **Phase 2 ✅ built and 🔍 founder-reviewed 2026-07-15** — design approved
as the baseline, **but the review revised the product scope** (free tool DROPPED, two-plan pricing,
trial credit, panel + auth + docs screens added — the full verdict is in the Phase 2 revision
block).
**Next: Phase 2R — build the revised screens.** Then contracts (2.5), the pipeline (3), payments
(4), launch (5). The clock (METRICS.md §2.2) starts at the Phase 5 launch; the ship-by gate
(METRICS.md §2.2 SB) makes sure that day comes.

> ⚠️ **Two things 0.2 changed that you must not carry forward unread.**
> **(1) The output side of the pipeline is not bounded by segment length** — dense slides overflow
> the token cap even at 10 minutes, and the naive fix costs ~13× (METRICS.md **N4d**). A Phase 3
> requirement, not a detail.
> **(2) The weakest content category is the one the paying product ingests** — continuous screen
> recordings under-segment (METRICS.md **N7b**, **N32**). The *free* path is strong; the *paid* path
> is the one that needs work. That is the opposite of the order we'd have guessed.

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
| **A5** | Distribution works. **Top risk.** | **Phase 5 (launch) onward** — gated by the ship-by date (METRICS.md §2.2 SB) |
| **A1** | EU residency is a purchase driver | Phase 5 landing-page headline A/B, on cold T-LAUNCH traffic (METRICS.md N27) |
| **A2** | Buyers buy rather than DIY | Phase 4 ships the Stripe checkout; the read accrues from the Phase 5 launch |
| **A3** | Recurring flow, not one-time backfill | Phase 4 instruments cohort hour-decay — **plus the N20 signup field, which reads it two months earlier** |
| **A4** | Output is citable in a KB | **Accuracy ✅ measured in Phase 0.2** (METRICS.md N30–N32) — but *trust* is only proven by `upload_repeat`, built in Phase 3, read post-launch. Accuracy is necessary, not sufficient. |

⚠️ **Do not invoke a decision rule below its sample floor (METRICS.md §2.1).** The most likely way
this plan fails is not a bad number — it is a *verdict called on 40 visitors*.

---

## Test assets

| File | Properties | Role |
|---|---|---|
| `~/Downloads/Isolation Component V2 demo.mp4` | 50:40, 1080p @ 16 fps, spoken audio, 293 MB | **Company-internal — NOT publishable.** Long-form demo screen-recording; cost benchmark; private golden-test source |
| `~/Downloads/HabiCen Ticket System.mp4` | 1:52, silent AAC track, 99 MB | **Company-internal — NOT publishable.** Edge case: audio stream present, nothing spoken — must output no transcript, not hallucinate one |
| **Public CC BY corpus** — `experiments/001-*/out/corpus.json` | 5 videos, licence-verified via the YouTube Data API | ✅ **Publishable.** Closed the A4 category gap; rendered demos in `out/corpus_md/`. Reached **only** through Vertex `fileUri` — we never hold the bytes. |
| **`tests/fixtures/videos/`** | 3 × 90 s clips, 5.4 MB, CC BY / public domain | ✅ **Committed.** Golden tests are now reproducible by anyone who clones the repo. |

Both internal videos are unshareable and too large to commit — that is why the public corpus exists.

⚠️ **The corpus and the fixtures come from different sources on purpose.** The corpus is CC BY
*YouTube* video, and it is only ever reached through `fileData.fileUri`. **Fixtures must live in the
repo, so they cannot come from YouTube at all** — that would mean downloading bytes (CLAUDE.md
rule 8), and a CC licence on the content does not override the platform's ToS on the delivery. The
fixtures are therefore sourced from FOSDEM / NASA / Wikimedia, which publish the media files
themselves. See `tests/fixtures/videos/README.md`.

---

## Phase 0 — Benchmark experiment (`experiments/001-gemini-video-benchmark/`) ✅ DONE 2026-07-14

> **Completed.** All 4 questions answered — see `experiments/001-gemini-video-benchmark/out/RESULTS.md`.
> Headlines: `gemini-2.5-flash` @ `europe-central2` confirmed; COGS measured and comfortably inside
> the guardrail (METRICS.md §1.2, N8); no hallucinated speech on silent video; the static-content
> lever is large on real demo footage. Spend: €1.91 of €5.

**Two findings graduated late (from the Workflow-1 memo, not the original RESULTS.md):**

1. **~8% degenerate-generation rate.** `seg4_configA` (474 s, 61k output tokens) and
   `seg2_configB` (323 s, 63k thinking tokens) ran away. This is the source of the observed
   1.3× retry overhead and it will break the <15 min/video-hour SLO. → hard output-token caps
   + timeouts are now a Phase 3 requirement.
2. **The cost ledger is missing ~30% of true COGS.** ffmpeg transcode/segmentation on Cloud Run
   is ~€0.15/video-hour (estimate) and is not metered anywhere. → CLAUDE.md rule 6 extended to
   compute, not just LLM calls. Phase 3 requirement.

---

## Phase 0.1 — YouTube-ingestion spike ✅ DONE 2026-07-14

> **Completed.** All 3 questions answered — see `experiments/001-gemini-video-benchmark/out/YOUTUBE.md`.
> Spend: €0.60 of €1.

1. **EU acceptance: ✅ yes.** Both `europe-central2` and `europe-west3` accept a YouTube
   `fileData.fileUri` and genuinely fetch the video. No US region needed. (`mimeType` is mandatory
   even for YouTube URLs.)
2. **Offset segmentation: ✅ yes, server-side.** Equal-length windows cost identical tokens at any
   position; tokens scale linearly with length. **Long videos segment without ever holding the
   bytes** — the public path needs no Stage A, no ffmpeg, no GCS round-trip.
3. **Cost: ✅ measured, and better than assumed** (METRICS.md §1.2b). The premise that the cost lever
   was *gone* on this path was **half wrong**: Stage A's per-segment static routing is indeed
   impossible, but `mediaResolution` is a **request-level** knob needing no local analysis, so the
   whole public path can just run low-res — **N4b, at ~⅔ the default-resolution cost, with quality
   intact.** This is what sizes N10.

**Three findings that become Phase 3 requirements:**

- 🚨 **The coverage guard must divide by the *fetched* duration, not the requested window.** Vertex
  clamps the window to the video's end, so on a video shorter than the window the Phase-0 guard sees
  false under-generation and **retries + double-bills**. Observed on a 59 s video.
- 💡 **A video's true duration is free** — it falls out of the token count (METRICS.md §1.2b). Ask for
  more than you expect and read the length off the bill. **No YouTube Data API, no scrape** (rule 8).
- **Hitting the `thinkingBudget` cap is not a failure** — 5 of 8 calls hit it and all returned good
  output. ARCHITECTURE §3 said to treat a cap-hit as a segment failure; that rule is now corrected.

⚠️ **Known constraint, do not design around it:** VPC Service Controls **disables `fileUri`
media URLs entirely.** If the project is ever locked down for enterprise compliance, the
YouTube path dies. This is one more reason it belongs in marketing, **never** in the core
paid product.

---

## Phase 0.2 — Public benchmark & demo corpus ✅ DONE 2026-07-14

> **Completed.** See `experiments/001-gemini-video-benchmark/out/CORPUS.md`.
> Spend: **€3.07** (€3 envelope + an approved €0.20 top-up — the cap halted the run mid-render,
> which is the cap working).

**5 CC BY videos, every one verified through the YouTube Data API** (`status.license ==
"creativeCommon"`), not the CC search filter. `out/corpus.json` is the audit trail. 🚧 The gate
immediately confirmed 0.1's warning as fact: **all 7 spike URLs were standard-licence — none was
ever publishable.**

1. **A4 category verdicts: ✅ gap closed** (METRICS.md **N30–N32**). Graded by *independent
   re-probe* — a second, differently-prompted call seeked to each block's own claimed timestamp, so
   the output is graded against a fresh look at the video rather than against itself. Slide talks
   and talking heads are **strong** (99%/97% timestamp anchoring, 0 hallucinated on-screen text in
   15 probes). On bare studio footage the model **abstains rather than invents** — the mirror of
   Phase 0's silent-video result.
2. **Publishable artifacts: ✅** `out/corpus_md/` — the FOSDEM talk rendered **end-to-end**, plus
   four 10-minute excerpts. Attribution ships *inside* each file. **This is the raw material for
   0.3 and it is ready.**
3. **Committable fixtures: ✅ but re-scoped** — `tests/fixtures/videos/`, three 90 s clips, 5.4 MB.
   ⚠️ **Not from YouTube: that would mean downloading bytes (CLAUDE.md rule 8).** A CC licence on
   the content does not override YouTube's ToS on the delivery. Sourced from FOSDEM / NASA /
   Wikimedia, which publish the files themselves.

**The two findings that reshape the phases below:**

- 🚨 **Segment length does not bound Stage B's output — text density does.** A 15-min window blew
  the output cap on *both* slide talks; the dense middle of one blew it **even at 10 min**. The
  pipeline must **react** (halve and re-run), and the naive version of that costs **~13×**
  (METRICS.md **N4d**). → Phase 3.
- 🚨 **The weakest category is the paid product's own content type.** Continuous screen recordings
  under-segment into ~86 s blocks — vague citations, exactly where the ICP lives (METRICS.md
  **N7b**). The public YouTube path is *strong*; the private path is the one that needs the work.
  → Phase 1 (Stage A static detection can force block boundaries — the private path holds the bytes,
  so it can fix what the public path cannot).

---

## Phase 0.3 — ⛔ SUPERSEDED 2026-07-15 → folded into Phase 5 — LAUNCH

**Decision (2026-07-15): ship the MVP first (Phases 2–4), then launch everything at once.**
Rationale in the STATUS block. Everything this phase contained — the headline A/B, measurement,
the artifact post, the ad tranche — lives in **Phase 5** below, unchanged in substance. The move
settled three things:

1. The old open question #1 (the ad gate needed the gallery, then a later deliverable) is resolved
   by construction: at launch the page, gallery, tool and checkout are all live, and the two ad
   tranches merged into one (METRICS.md N26/N27).
2. **Analytics: Umami, self-hosted in the EU** (decided 2026-07-15; was Plausible — METRICS.md
   §6.2, CLAUDE.md rule 10). Self-hosting also removes the "create the analytics account"
   founder-only item.
3. The build is now the thing that can rot, so it got the deadline: **the ship-by gate
   (METRICS.md §2.2 SB).**

The consent question (old open question #2 — whether first-party UTM attribution persisted to
`payment_succeeded` is "strictly necessary" under GDPR, vs. the `sessionStorage`-at-signup route)
is **still open and still the founder's**; it is carried forward as a Phase 5 pre-flight item.

✅ The domain gate remains cleared — `https://mdreel.com` is live (INFRA.md).

---

## Phase 1 — Core + Stage A ✅ DONE 2026-07-14

> **Completed.** `src/Core` (domain, the four provider seams, pipeline skeleton) + Stage A for real:
> ffprobe/ffmpeg wrapper, static-content detection, **forced block boundaries**, segmentation, split
> policy, compute cost ledger. 77 tests green (42 unit on synthesized frames, 35 integration on the
> committed CC clips). docker-compose.yml landed. **Spend: €0** — Stage A is pure local compute.

1. **The cost lever survived the port, exactly.** The C# static-content detector reproduces the Phase-0
   measurement on the same recording — same static share, same run count. That test is the fidelity
   gate on **METRICS.md N4**: if it ever goes red, the blended cost figure is quietly wrong and nothing
   else would say so.
2. 🚨 **N7b is addressed — and the mechanism is the opposite of what this plan assumed.** We expected
   static-content detection to force the boundaries. **It cannot: the window that produced the failure
   barely moves at all** (METRICS.md **N7c** owns the figures). It is a presenter talking over a frozen
   IDE. Stillness is *why* the model is blind there, so it cannot also be the cure. Boundaries are
   forced on **elapsed narration** and suppressed on **silence** — a signal that needs the audio, hence
   the bytes, hence the private path. The gain on that window is in METRICS.md **N7c**.
3. **Suppressing boundaries inside static runs was tried and rejected.** It reads as obviously sensible
   ("a slide held for minutes is one block"), it is right about slide talks, and it leaves **160-second
   blocks on the ICP's own footage** — worse than the status quo it was meant to fix. The measurement is
   the only reason we know.

⚠️ **What Phase 1 did NOT prove: that Stage B *obeys* the boundaries.** It ships a deterministic input
whose effect on the model is **unmeasured** — that needs a Vertex call. **Do not treat N7b as closed.**

**Judgment calls made unsupervised are listed in `HANDOFF.md` at the repo root — read it before
Phase 3.** (HANDOFF.md predates the 2026-07-15 renumbering and calls the pipeline phase "Phase 2".)

## Phase 2 — Frontend look & feel (mock-first, no product backend) ✅ BUILT 2026-07-15

> **Built, not yet signed off.** All 7 screens run end-to-end on committed fixtures — verified via
> `next build`/`npm test`/lint clean, a local Docker smoke test (the exact multi-stage image Cloud
> Run will build), and a live job-flow check confirming a YouTube-tool download is **byte-identical**
> to its source fixture. **Not yet redeployed** (CLAUDE.md rule 5 — deploy is deliberate) and **not
> yet visually reviewed by the founder** — the phase's own exit criterion ("founder has seen it and
> signed off on the look") is the one item still open. INFRA.md has the new Dockerfile/build notes
> and the redeploy command (unchanged target).

**Goal: see the whole product before the backend exists.** Every screen in `web/` (Next.js),
running entirely on **committed fixtures** — the real Phase-0.2 corpus outputs are the mock data,
so the look-and-feel review happens on genuine product output, not lorem ipsum.

> ⚠️ **The numbered list below is the v1 as-built record — what the founder review looked at.
> The review block after it REVISES this scope** (free tool gone, two-plan pricing, panel + docs
> added). Phase 2R builds the delta; read the review block as authoritative.

1. **Screens:** landing page (both headline arms wired, arm assignment stubbed) · free YouTube
   tool (paste URL → mocked job progress → real Markdown from `experiments/001-*/out/corpus_md/`) ·
   gallery (index + detail pages fed by the corpus files, attribution + original video embedded) ·
   magic-link signup (stub, including the N20 one-field question) · upload + job status · output
   viewer/download · **pricing page — ONE plan + the free tool, not tiers** (decided 2026-07-15;
   tiers presuppose the A3 answer we do not have — METRICS.md A3).
2. **Plumbing that cannot be bolted on later, built now even though it fires nothing:**
   first-touch UTM capture persisted first-party (METRICS.md §6.3), `ab_arm` assignment, and the
   METRICS.md §3 event names as a typed client module (stub transport). Without this, real CAC is
   uncomputable forever (METRICS.md N29).
3. **No product backend.** A thin mock API inside `web/` serving the fixtures is fine — it becomes
   the first consumer of the Phase 2.5 contract.

Model note: Sonnet-OK UI phase. Exit: every screen navigable end-to-end on mocks; founder has
seen it and signed off on the look.

### 🔍 Founder review 2026-07-15 — verdict, and a revised scope (Phase 2R)

**Design: approved as the baseline** (screenshots in `experiments/screenshots/`). The gallery page
and the terminal-framed Markdown preview in particular are keepers. Two defects: a glitched shared
footer (overlapping duplicate logos + stray white box — seen on the free-tool page; verify it is
not global) and the job page header still reading "Processing your video" after completion.

**Scope revisions (all founder decisions, 2026-07-15):**

1. 🚨 **The free YouTube tool is DROPPED.** A public compute endpoint is a bot/abuse surface and an
   ops tax that a fixed base this small cannot carry — even capped. **No visitor-triggered
   processing exists anywhere public** (METRICS.md N10 is rescoped accordingly; N11 retired).
   Remove the page, the nav item, the footer links, and the pricing-page card.
2. **The gallery stays and takes over the tool's funnel job** — it demos both *that* we process
   YouTube video and *what* the output looks like, at zero compute per visitor. Showcase-only:
   produced by us, never an input box. (BUSINESS_MODEL §10 boundary reaffirmed.)
3. **Pricing: two plans, no free tier** — a small plan with a **hard cap** and a larger plan with
   **metered overage** (prices live in BUSINESS_MODEL §6). Signup grants a **one-time trial credit
   (METRICS.md N33)** replacing "2 h free" — the hero CTA, CTA banner, and all "free" copy need
   rewording.
4. **NEW screens — sign-in/signup (magic link) + the authenticated panel:** process a new video
   (upload), a job list of processed videos, and content management (view / download / delete).
   This overrides the old "no dashboard, no job list" exclusion.
5. **NEW screen — docs:** REST API + webhooks + MCP, **all three shipping in the MVP** (the MCP
   server itself lands in Phase 4 as a thin layer over the API; it is the **first candidate to cut**
   if the SB gate tightens).

Exit criterion unchanged: revised screens navigable on mocks, founder signs off again.

**Starter prompt (Phase 2R):**
> Phase 2R — revised frontend scope. Read PLAN.md (STATUS, then the Phase 2 founder-review block),
> ARCHITECTURE §4–§5, METRICS.md §3 + N33, BUSINESS_MODEL §6, and CLAUDE.md rule 10. **Plan mode
> first.** Remove the free-tool page/nav/footer/pricing references; rebuild pricing as the two
> BUSINESS_MODEL §6 plans + N33 trial credit (no free tier — re-copy the hero and CTA banner);
> add sign-in/signup, the authenticated panel (upload · job list · manage/download/delete, all on
> mocked fixtures), and the docs page (REST + webhooks + MCP). Fix the shared-footer glitch and the
> job-done header copy. Still no product backend, no Vertex calls. No US-hosted anything (rule 10).

## Phase 2.5 — Freeze the contracts (frontend ⇄ backend)

Both contracts already exist as drafts — the work is **ratification, not invention**:

1. **Markdown output contract** — ARCHITECTURE §4, reviewed against the real corpus outputs; fix
   any drift between the spec and what the pipeline actually produced, then freeze. Consistency of
   this schema across all files is a product feature — downstream RAG depends on it.
2. **API contract** — ARCHITECTURE §5 trimmed to the MVP subset: the public YouTube-tool endpoint
   + `POST /uploads`, `POST /jobs`, `GET /jobs/{id}` (status/polling shape), `GET
   /jobs/{id}/output.md|json`, `DELETE /jobs/{id}` (erasure is not deferrable — GDPR).
3. **Deliverables:** updated ARCHITECTURE §4/§5, a committed JSON schema + sample fixtures under
   `tests/fixtures/`, and the Phase 2 frontend re-pointed at those fixtures — **the mocks consuming
   the ratified fixtures is the proof the contract is real.**

**Starter prompt:**
> Phase 2.5 — freeze the frontend⇄backend contracts. Read PLAN.md (Phase 2.5), ARCHITECTURE §4–§5,
> and the rendered outputs in `experiments/001-*/out/corpus_md/`. **Plan mode first.** Ratify the
> Markdown output contract and the MVP API subset, commit schema + fixtures, re-point the Phase 2
> mocks at them, and update ARCHITECTURE §4/§5 in the same commit.

## Phase 3 — Simplest working pipeline (merges old "Stages B/C/D" + the thin trial slice)

**The product is the validation instrument. Build only the instrument** — Stage B against Vertex
with structured output; record/replay fixtures + `tests/Live/`; Stage C fusion; Stage D persist +
output renderer; source deletion. Both ingestion paths, wired to the Phase 2 frontend, in the
simplest form that honours the guards below.

🚨 **First job of the phase, before any of the below: does Stage B actually honour Stage A's block
boundaries?** Phase 1 can prove they are *emitted* (METRICS.md N7c); only a Vertex call proves they are
*obeyed*. Put the boundaries in the prompt, run the window that failed, and count the blocks. **If the
model ignores them, the fallback is to cut real segments at the boundaries** — which forces the issue
at the price of more calls. Everything else in this phase is cheaper than this question.

**Build order inside the phase: YouTube path first — it is the smaller half.** No Stage A, no
upload, no GCS round-trip: Stage B directly on `fileData.fileUri` + offset segmentation → C → D.
🚨 **Internal-only since the 2026-07-15 review: the free tool is dropped, so this path has NO
public endpoint.** It exists to produce gallery content, run by us. The day-one abuse-control
workload (caps, per-IP limits, video-ID cache) went with it — METRICS.md N10 now reads "any
unauthenticated compute spend is a bug".

- **Process the gallery corpus — 10–25 curated CC-licensed talks** — as the pipeline's shakedown
  run. Pages exist since Phase 2; they go *public* at Phase 5. **Not a scaled content farm:**
  curated + CC-licensed + attributed, or Google's scaled-content-abuse policy and rights-holders
  make it a fight we cannot afford.

**Then the private path — the real test:** magic-link signup → signed upload → Stage A (✅ built in
Phase 1) → B → C → D → Markdown back in the panel (job list, download) + by email. Trial credit
per METRICS.md N33. Nothing else.

**Hard requirements carried in from the benchmark phases:**

- **Hard output-token caps + request timeouts on Stage B.** ~8% of benchmark calls ran away
  (61k output tokens / 63k thinking tokens). Untreated, this breaks the <15 min SLO and is the
  1.3× retry overhead.
- **Per-job cost ledger meters compute, not just LLM calls.** ffmpeg/Cloud Run transcode is
  ~30% of true COGS and is currently invisible. CLAUDE.md rule 6 now covers it.
- 🚨 **Split on overflow — and size segments by density *up front*** (Phase 0.2). `MAX_TOKENS` is
  deterministic: retrying it unchanged just bills twice. Halving works, but the naive loop pays for
  the failed parent **and** both halves (METRICS.md **N4d**, ~13×). **Shipping the naive loop is
  shipping N4d.** → ARCHITECTURE §3.
- 🚨 **Two-sided coverage guard + per-timestamp normalization** (Phase 0.2). The model mixes
  `mm:ss:cs` and `hh:mm:ss` *within one response*; a one-sided guard accepted blocks at `04:48:10`
  in a 15-minute clip. **A citation to a timestamp that does not exist is worse than no citation.**

Infra: Cloud Run + GCS + in-process queue. **No Cloud SQL, no Cloud Tasks, no Terraform yet** —
Cloud SQL idles at ~€25–50/mo, which is a meaningful fraction of the entire fixed base we are
trying to cover (METRICS.md N2/N1a). **At survival scale, an idle database is a real tax.**

**Explicitly NOT built** (revised 2026-07-15 — the panel, two-plan checkout and MCP server moved
*into* the MVP): connectors, DPA self-service flow, usage/cost-analytics page beyond the panel's
job list, team/multi-user management, SSO.

**The funnel:** browse the gallery → verify real output on a talk you already know →
*"try it on your own recording"* → signup + trial credit (METRICS.md N33) → two-plan checkout.

> **The discipline to hold: YouTube processing is showcase-only — the gallery we curate and
> produce ourselves; the paid product is private recordings.** The moment customers can buy
> YouTube processing, we have changed businesses — and walked into the BUSINESS_MODEL §8
> buyer-confusion risk ("is this a YouTube tool?").
> YouTube ingestion *cannot* serve the ICP anyway: Vertex only accepts public videos or ones
> owned by **our** GCP account, so customers' unlisted/private recordings will never work.
> Reaffirmed at the 2026-07-15 review: no YT input box anywhere, public or paid.

**Starter prompt:**
> Phase 3 — the pipeline, simplest working form. Read PLAN.md (STATUS, Phase 3), **HANDOFF.md**
> (it calls this phase "Phase 2"), ARCHITECTURE §3, METRICS.md N7c/N4d, and CLAUDE.md
> rules 8–9. **Plan mode first.** First job: prove Stage B obeys Stage A's boundaries (one Vertex
> call, the window that failed). Then the YouTube path end-to-end as the internal gallery-production
> runner — it has NO public endpoint; then the private upload path wired to the Phase 2R panel.
> Every Stage B call sets `maxOutputTokens`, a bounded `thinkingBudget`, and a timeout (rule 9).
> Never download YouTube bytes (rule 8).

## Phase 4 — Payments + instrumentation (the pricing page goes live)

- **Two Stripe checkouts — the two plans** (revised 2026-07-15; prices and caps live in
  BUSINESS_MODEL §6): the small plan with a **hard cap** (processing pauses at the limit), the
  larger with **metered overage**. **No free tier**; signup grants the one-time trial credit
  (METRICS.md **N33**). → **This is the A2 test.** Nobody buys either = it's a vitamin, not a
  painkiller (`checkout_clicked` carries the plan). The Phase 2R pricing page goes live with real
  checkout.
- **Instrumentation — the whole point of the phase.** Events: signup, upload,
  download, **second upload**, checkout click — the METRICS.md §3 schema, flowing into our own
  Postgres (the §6.2 source of truth). The Phase 2 client module gets its real transport here.
  Where that Postgres lives (smallest Cloud SQL vs. a container next to the worker) is decided at
  the top of this phase — and Umami (Phase 5) shares the same instance, never a second one
  (METRICS.md §6.2).
- **Docs + MCP go live** (revised 2026-07-15): the docs page documents the real REST API +
  webhooks, and the **MCP server ships as a thin layer over that API** (ARCHITECTURE §5). ⚠️ **The
  MCP server is the first candidate to cut if the SB gate (METRICS.md §2.2) tightens** — the API
  and webhooks are not.
- 🚨 **Cohort hour-decay, instrumented before the first user.** Hours uploaded in week 1 vs.
  week 4, per signup cohort. **This is the A3 test, and A3 decides what business we are in**
  (decision rule: METRICS.md A3). If it says backfill, the correct product is a **prepaid credit
  pack**, not a monthly subscription. **The question cannot be answered retroactively — instrument
  it first or lose the data forever.**
- **The N20 signup field** (METRICS.md) ships here too: archive-hours vs monthly-hours, one
  skippable control. **It reads A3 roughly two months before the cohort data can.**
- **First-touch UTM attribution verified end-to-end:** the Phase 2 capture must land on the tenant
  row and survive to `payment_succeeded` (METRICS.md §6.3) — test it before launch, not after.

**Starter prompt:**
> Phase 4 — payments + instrumentation. Read PLAN.md (Phase 4), METRICS.md §3 + §6 + N20/N29/N33,
> BUSINESS_MODEL §6, and ARCHITECTURE §5–§6. **Plan mode first.** Two Stripe checkouts (small plan
> hard-capped, larger with overage) + the N33 trial credit; the §3 event schema into our own
> Postgres; cohort hour-decay + the N20 field instrumented before any user exists; UTM survival to
> `payment_succeeded` proven by a test; docs page + MCP server (thin layer over the REST API — cut
> it first if the SB gate tightens).

## Phase 5 — 🚀 LAUNCH (the old Phase 0.3, upgraded) — ⏱️ T0 starts here

**Everything goes live at once, against a full funnel.** ⏱️ **T0 — the time-box (METRICS.md §2.2)
starts on the day the first post ships. Record the date in METRICS.md §2.2.** If the ship-by gate
(METRICS.md §2.2 SB) fires first, this phase runs with whatever exists — that is the rule.

Work:

1. **Measurement, before the first visitor** (METRICS.md §6):
   - **Umami, self-hosted in the EU, cookieless** — no consent banner, no funnel tax; shares the
     Phase 4 Postgres instance (METRICS.md §6.2). 🚨 **Never Google Analytics** (CLAUDE.md rule 10).
   - The weekly scoreboard: signup-rate by referrer (the §6.5 qualification proxy), and **the
     sample floor displayed next to every number** (§6.6).
2. **Landing page headline A/B** — live arm assignment on the Phase 2 wiring:
   - Arm A — *"Your recordings never leave the EU."*
   - Arm B — *"Your AI assistant can't see what's on screen in your videos."*
   - **This is the A1 test.** If Arm A does not clearly beat Arm B, **EU is a checkbox, not a
     wedge**, and all positioning moves to the capability story. Directional, not significant.
3. **Gallery goes public** — the 10–25 talks processed in Phase 3, curated, attributed, originals
   embedded.
4. **The artifact post** — the single best inbound asset, and it's already written: side-by-side
   *plain transcript vs. mdreel Markdown*, plus a RAG answering a question that is **only**
   answerable from on-screen content. ✅ **The material exists and is legally shareable:**
   `experiments/001-*/out/corpus_md/JvbBFwlqxeI_full.md` — the FOSDEM talk, CC BY, processed
   end-to-end, attribution inside the file. Channels: LinkedIn (existing .NET/architecture audience
   — the cheapest traffic source available, by an order of magnitude), HN, r/RAG, r/LocalLLaMA.
5. **Ad tranche T-LAUNCH** (METRICS.md N27): one-time, Google Search, long-tail exact/phrase.
   **Buys evidence, not customers** — real CPCs, cost per signup, a clean A1 verdict on *cold*
   traffic, and **measured N12**. Hard stop at budget. Per-keyword kill rule per METRICS.md §6.7.

> **Broadcasting is not outreach.** This requires zero 1:1 contact with anyone.

### 🔑 Founder pre-flight — the agent cannot do these

| Founder-only | Why the agent can't |
|---|---|
| ~~Buy + verify the domain~~ **✅ done 2026-07-15** (`mdreel.com` live, INFRA.md) | — |
| ~~Analytics account~~ **✅ obsolete** — Umami is self-hosted, no account exists | — |
| **Google Ads account + payment method** (T-LAUNCH) | Your card. *Agent writes the campaign, keywords, negatives — nothing spends without you.* |
| **Post to LinkedIn / HN / Reddit** | Your identity and your voice. *Agent drafts; you publish.* |
| 🚨 **The consent decision** (carried from old 0.3): is first-party UTM persisted to `payment_succeeded` "strictly necessary" under GDPR, or does attribution live in `sessionStorage` and attach only at signup? | **A decision about the product's legal posture — the founder's.** A DPO reading our EU-residency copy is exactly the reader who will check. |

**Starter prompt:**
> Phase 5 — LAUNCH. Read PLAN.md (STATUS, Phase 5), METRICS.md §2.2 + §6 + N15/N26/N27,
> DISTRIBUTION.md, and CLAUDE.md rule 10. **Plan mode first.** Measurement before the first
> visitor: Umami self-hosted EU (shared Postgres), scoreboard with sample floors. Then A/B live,
> gallery public, draft the artifact post + the ads campaign for founder review. **Stop and ask
> me** about the consent decision; do not guess it. Record T0 in METRICS.md §2.2 the day the first
> post ships. 🚨 No Google Analytics, no US-hosted anything, no consent banner (rule 10).

## Phase 6 — Read the data → choose the pricing model (was Phase 5)

Pricing is an **output** of the cohort data, not an input decided in advance. Rerun the benchmark
matrix through the real pipeline; set pricing from A3; write the verdict into BUSINESS_MODEL §6.
The one-plan pricing page from Phase 4 stays until this phase says otherwise.

**Kill criteria — the honest version.** If, past the sample floors (METRICS.md §2.1): nobody pays,
the EU headline arm loses the A/B, and cohort hour-decay says backfill — **stop.** The TAM
(METRICS.md N16) does not justify pushing uphill against a DIY alternative our own customers are
qualified to build.

---

## Deferred until there is a paying customer

Terraform / Cloud SQL / Cloud Tasks · dashboard + usage UI · plan tiers + metered overage ·
connectors (Drive/SharePoint/Zoom) · MCP server · DPA self-service flow · self-hosted edition.
