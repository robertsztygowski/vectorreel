# PLAN — Benchmark Experiment & MVP Implementation

> Session-by-session execution plan. Companion to ARCHITECTURE.md (§9 build order is the
> authority on *what*; this file adds *how to run the sessions*). Check off / update as
> phases complete — living doc rules apply (DEVELOPMENT.md §7).

## How to use this file

Start each phase in a **clean Claude Code session** with the starter prompt given below.
CLAUDE.md auto-loads the hard rules; the prompt only needs to point at the phase.

- Model: **Fable 5** for pipeline/IP phases (0–3), **Sonnet 5** acceptable for CRUD/UI phases (4–6).
- Use **plan mode** at the start of every phase before writing code.
- Every phase ends with the definition of done (CLAUDE.md rule 4) and a commit.

## Test assets

| File | Properties | Role |
|---|---|---|
| `~/Downloads/Isolation Component V2 demo.mp4` | 50:40, 1080p @ 16 fps, spoken audio, 293 MB | Long-form demo screen-recording; segmentation + cost benchmark; future golden-test source |
| `~/Downloads/HabiCen Ticket System.mp4` | 1:52, 1918×906 @ 30 fps, **silent** AAC track, 99 MB | Cheap iteration video; edge case: audio stream present but nothing spoken — pipeline must output no transcript, not hallucinate one |

Both are too large to commit — they live in a dev GCS bucket (EU) during work; small clips cut
from them may be committed to `tests/fixtures/videos/` later. Missing benchmark category
(ARCHITECTURE.md §8): a slide-deck talk and a talking-head meeting — add when available.

---

## Phase 0 — Benchmark experiment (`experiments/001-gemini-video-benchmark/`)

**Goal: validate the 4 riskiest assumptions before any product code exists.** Python throwaway
scripts (escape-hatch rules, `experiments/README.md`). Budget: ≲ €5 Vertex spend, one session.

Questions to answer (the deliverable is these numbers/verdicts):

1. **EU availability** — does Gemini Flash accept native video from a GCS URI in
   `europe-central2` (fallback `europe-west3`)? Which model versions exist there?
2. **Real cost per video-hour** at 3 sampling configs. Guardrail: COGS < €1.50/h
   (BUSINESS_MODEL.md §6 pricing is provisional until this number exists).
3. **Stage B quality** on real footage — `on_screen_text` verbatim? timestamps accurate?
   JSON schema (ARCHITECTURE.md §3 Stage B) adhered to? Silent video produces empty
   `spoken` fields, not hallucinations?
4. **Static-content lever size** — what fraction of a real demo recording is low-motion,
   i.e. how much can low-fps sampling actually save?

Steps:

1. Prep (local ffmpeg — installed): upload both videos to dev GCS bucket (EU);
   split long video into ~4 × 12–13 min segments with ~20 s overlap; make 720p renditions;
   frame-difference pass → static/active timeline (Q4).
2. Sampling matrix (Q2) on 1–2 segments only, extrapolate: A = default fps @720p,
   B = low fps @720p, C = default fps @1080p.
3. Stage B calls with the exact ARCHITECTURE.md JSON schema via Vertex structured output.
   Record tokens in/out, latency, cost per call.
4. Short video run (Q3 edge case): full pipeline on HabiCen — verify no hallucinated speech.
5. Quality spot-check: founder checks ~10 known moments against output, verdict per config.
6. Bonus: one Stage C fusion pass over best config → complete `output.md` for the 50-min
   video (first end-to-end artifact; seed for the future golden test).
7. Cleanup: delete videos from the bucket.

Deliverables: `RESULTS.md` (cost/quality table per config), sample outputs, full `output.md`.
**Graduating decisions (same commit):** real cost/hour → BUSINESS_MODEL.md §6; confirmed
region/model + default sampling config → ARCHITECTURE.md §2/§8.

Out of scope: .NET, Terraform, DB/queue/API, retention automation, diarization tuning,
Mistral, production prompt polish.

**Starter prompt:**
> Read PLAN.md Phase 0 and execute the benchmark experiment. Start in plan mode: first verify
> Gemini Flash model availability in europe-central2 from current gcloud auth, then plan the
> scripts. Videos are at the paths listed in PLAN.md. Keep total Vertex spend under ~€5;
> ask before exceeding.

---

## Phase 1 — Terraform foundation (ARCHITECTURE.md §9 step 1)

Buckets (`raw-videos-eu`, `outputs-eu`) + lifecycle rules, Cloud SQL (smallest tier),
Cloud Run services (placeholders), Cloud Tasks queues, least-privilege service accounts.
All in `infra/`, all regions explicit. Note: shared "Propspire" project for now — org-policy
region pinning deferred until dedicated project (INFRA.md flag 1); pin per-resource instead.

**Starter prompt:**
> Read PLAN.md Phase 1 and INFRA.md, then plan and build the Terraform foundation in infra/.
> Plan mode first. Shared project caveats are in INFRA.md — pin regions per resource.

## Phase 2 — Core + Stage A (§9 step 2)

First .NET code: `src/Core` (domain, provider interfaces, pipeline skeleton) + Stage A
(ffprobe/ffmpeg wrapper, segmentation, static detection — port learnings from Phase 0).
Golden tests on sample clips cut from the two test videos. docker-compose.yml
(postgres + fake-gcs-server) lands here.

**Starter prompt:**
> Read PLAN.md Phase 2, ARCHITECTURE.md §3 Stage A, and experiments/001-*/RESULTS.md.
> Plan mode first, TDD for segmentation/static-detection logic per DEVELOPMENT.md §4.

## Phase 3 — Stages B/C/D (§9 steps 3–4)

Stage B against Vertex with structured output (prompt/config graduated from Phase 0);
record/replay fixture machinery + `tests/Live/`; Stage C fusion; Stage D persist +
output-contract renderer (snapshot-tested, deterministic); per-job cost ledger;
source deletion. End of phase: full pipeline runs locally on both test videos.

## Phase 4 — API (§9 step 5)

Uploads (signed URLs), jobs, outputs, usage, API keys, webhooks (HMAC). Vertical slices
in `src/Api/Features/`. RFC 7807 errors. Integration tests against compose.

## Phase 5 — UI (§9 step 6) — Sonnet 5 fine

Next.js in `web/`: login (magic link or Google OAuth), upload, job list, markdown
preview/download, usage page.

## Phase 6 — Billing + compliance pages (§9 steps 7–8) — parallelizable

Stripe (plans, metered overage, 2 h free-trial gate); /gdpr + /security pages, DPA template,
subprocessor list. Independent tracks — candidate for parallel sessions / agent team.

## Phase 7 — Benchmark rerun + pricing lock, first design partner (§9 step 9)

Rerun Phase 0 matrix through the real pipeline; lock pricing in BUSINESS_MODEL.md;
onboard design partner #1.
