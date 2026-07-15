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

**Where we are now.** Phase 0 ✅, Phase 0.1 ✅, Phase 0.2 ✅, **Phase 1 ✅ done** — `src/Core` and Stage A
are built, and the weakest measured category has a fix (METRICS.md **N7c**) whose *effect on the model*
is still unproven.
**Next: Phase 0.3 — the demand instrument. It is the long pole, it is unstarted, and everything else
waits on it.** ⚠️ Phase 1 was engineering; it moved **A5 not at all.** The clock (METRICS.md §2.2) does
not start until the first post ships, and no amount of pipeline quality starts it.

> ⚠️ **Two things 0.2 changed that you must not carry forward unread.**
> **(1) The output side of the pipeline is not bounded by segment length** — dense slides overflow
> the token cap even at 10 minutes, and the naive fix costs ~13× (METRICS.md **N4d**). A Phase 2
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
| **A5** | Distribution works. **Top risk.** | **Phase 0.3 onward** |
| **A1** | EU residency is a purchase driver | Phase 0.3 landing-page headline A/B |
| **A2** | Buyers buy rather than DIY | Phase 4 Stripe checkout |
| **A3** | Recurring flow, not one-time backfill | Phase 4 cohort hour-decay — **plus the N20 signup field, which reads it two months earlier** |
| **A4** | Output is citable in a KB | **Accuracy ✅ measured in Phase 0.2** (METRICS.md N30–N32) — but *trust* is only proven by `upload_repeat` in Phase 3. Accuracy is necessary, not sufficient. |

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
   + timeouts are now a Phase 2 requirement.
2. **The cost ledger is missing ~30% of true COGS.** ffmpeg transcode/segmentation on Cloud Run
   is ~€0.15/video-hour (estimate) and is not metered anywhere. → CLAUDE.md rule 6 extended to
   compute, not just LLM calls. Phase 2 requirement.

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

**Three findings that become Phase 2/3 requirements:**

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
  (METRICS.md **N4d**). → Phase 2.
- 🚨 **The weakest category is the paid product's own content type.** Continuous screen recordings
  under-segment into ~86 s blocks — vague citations, exactly where the ICP lives (METRICS.md
  **N7b**). The public YouTube path is *strong*; the private path is the one that needs the work.
  → Phase 1 (Stage A static detection can force block boundaries — the private path holds the bytes,
  so it can fix what the public path cannot).

---

## Phase 0.3 — Demand instrument: publish (no product code) 🚨 THE LONG POLE

**This is the phase that decides the business, and it must start now and run in parallel with
every engineering phase below.** Do not serialize it after the build.

Rationale: with no outreach, **traffic is the only way any evidence reaches us.** The target is
METRICS.md **N15** — and it is doing double duty: the visitors that cover infra (N1a) are the same
visitors that reach the A2 sample floor. **Both input rates are assumptions.** Content compounds
over **months**; building for three months and *then* starting to post serializes the two slowest
processes in the company.

⏱️ **T0 — the time-box (METRICS.md §2.2) starts on the day the first post ships.** Record the date.

Work:

1. **Landing page headline A/B** (page is already deployed on Cloud Run):
   - Arm A — *"Your recordings never leave the EU."*
   - Arm B — *"Your AI assistant can't see what's on screen in your videos."*
   - **This is the A1 test.** If Arm A does not clearly beat Arm B, **EU is a checkbox, not a
     wedge**, and all positioning moves to the capability story. Directional, not significant.
2. **Email capture.** Nothing more.
2b. **Measurement, before the first visitor** (METRICS.md §6):
   - **Plausible (EU-hosted, cookieless)** — no consent banner, no funnel tax. 🚨 **Never Google
     Analytics** (CLAUDE.md rule 10).
   - **UTM capture on first touch, persisted first-party.** It must survive to `payment_succeeded`
     or real CAC can never be computed. **Design it in now; it cannot be bolted on at Phase 4.**
   - The scoreboard shows **capture-rate by referrer** — the qualification proxy (METRICS.md §6.5) —
     and **displays the sample floor next to every number** (§6.6).
2c. **Ad tranche T-A** (METRICS.md N26): €300–400, one-time, Google Search, long-tail exact/phrase.
   **Buys evidence, not customers** — real CPCs, cost per capture, and a clean A1 verdict on *cold*
   traffic. Hard stop at budget. **Not before the page and gallery are live.**
3. **The artifact post** — the single best inbound asset, and it's already written: side-by-side
   *plain transcript vs. mdreel Markdown*, plus a RAG answering a question that is **only**
   answerable from on-screen content. ✅ **The material now exists and is legally shareable:**
   `experiments/001-*/out/corpus_md/JvbBFwlqxeI_full.md` — the FOSDEM talk, CC BY, processed
   end-to-end, attribution inside the file. Channels: LinkedIn (existing .NET/architecture audience
   — the cheapest traffic source available, by an order of magnitude), HN, r/RAG, r/LocalLLaMA.

> **Broadcasting is not outreach.** This requires zero 1:1 contact with anyone.

### 🔑 Founder pre-flight — the agent cannot start these, and one of them gates everything

**✅ Domain gate cleared 2026-07-15 — `https://mdreel.com` is live** (INFRA.md: Cloudflare DNS-only →
Google-served, `europe-west1`, managed TLS). This **unblocks 2b, 2c and 3.** Remaining founder-only
items below.

| Founder-only | Why the agent can't |
|---|---|
| ~~Buy + verify the domain, delegate DNS~~ **✅ done 2026-07-15** (`mdreel.com` live) | *Was: your card, registrar, Search Console — now complete.* |
| **Create the Plausible account** (EU-hosted, paid) | Account + payment. *Agent then installs the script, defines events, builds the scoreboard.* |
| **Google Ads account + payment method** (2c) | Your card. *Agent can write the campaign, keywords, negatives — nothing spends without you.* |
| **Post to LinkedIn / HN / Reddit** | Your identity and your voice. *Agent drafts; you publish.* |

Everything else in this phase — the A/B, email capture, first-party UTM plumbing, storage, deploy —
is ordinary build work.

### ⚠️ Two open questions to settle *before* building — do not let a fresh session guess

1. **2c contradicts Phase 3.** The ad tranche is gated on "the page **and gallery** being live", but
   the gallery is a **Phase 3** deliverable. Either the gate is wrong or T-A is later than this phase
   implies. **Decide what T-A actually needs in order to buy honest evidence.**
2. 🚨 **The consent question, which our own copy invites scrutiny of.** Plausible is genuinely
   cookieless — but **first-party UTM attribution persisted all the way to `payment_succeeded` is not
   obviously "strictly necessary"** under GDPR, and a DPO reading our EU-residency claims is exactly
   the reader who will check. A clean route exists (hold attribution in `sessionStorage` and attach it
   to the form submission, tying it to a purpose the user actively initiates rather than to ambient
   tracking) — **but this is a decision about the product's legal posture, and it is the founder's.**

**Starter prompt:**
> Phase 0.3 — the demand instrument. Read PLAN.md (STATUS block, then Phase 0.3), METRICS.md §2.2 +
> §6 + N15/N20/N26, DISTRIBUTION.md, and CLAUDE.md rule 10. **Plan mode first.**
> This phase ships **no product code**: landing page + email capture + measurement + the post.
> Build **measurement before the page** (§6): Plausible, EU-hosted, cookieless; first-touch UTM
> persisted first-party so it can survive to `payment_succeeded` — **it cannot be bolted on at
> Phase 4, and without it real CAC is uncomputable forever** (METRICS.md N29). Every number on the
> scoreboard shows its sample floor beside it.
> The artifact post is built on `experiments/001-*/out/corpus_md/JvbBFwlqxeI_full.md` (CC BY —
> keep the attribution). **Stop and ask me** about the two open questions above; do not guess them.
> 🚨 No Google Analytics, no US-hosted anything, no consent banner (rule 10). If it phones home to
> the US, it does not ship.

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

**Judgment calls made unsupervised are listed in `HANDOFF.md` at the repo root — read it before Phase 2.**

## Phase 2 — Stages B/C/D

Stage B against Vertex with structured output; record/replay fixtures + `tests/Live/`;
Stage C fusion; Stage D persist + output renderer; source deletion.

🚨 **First job of the phase, before any of the below: does Stage B actually honour Stage A's block
boundaries?** Phase 1 can prove they are *emitted* (METRICS.md N7c); only a Vertex call proves they are
*obeyed*. Put the boundaries in the prompt, run the window that failed, and count the blocks. **If the
model ignores them, the fallback is to cut real segments at the boundaries** — which forces the issue
at the price of more calls. Everything else in this phase is cheaper than this question.

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
Cloud SQL idles at ~€25–50/mo, which is a meaningful fraction of the entire fixed base we are
trying to cover (METRICS.md N2/N1a). **At survival scale, an idle database is a real tax.**

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
  week 4, per signup cohort. **This is the A3 test, and A3 decides what business we are in**
  (decision rule: METRICS.md A3). If it says backfill, the correct product is a **prepaid credit
  pack**, not a monthly subscription. **The question cannot be answered retroactively — instrument
  it first or lose the data forever.**
- **The N20 signup field** (METRICS.md) ships here too: archive-hours vs monthly-hours, one
  skippable control. **It reads A3 roughly two months before the cohort data can.**

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
