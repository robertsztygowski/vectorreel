# Architecture — Video-to-Markdown for AI Knowledge Bases (EU SaaS on GCP)

> Companion to BUSINESS_MODEL.md. This is the implementation blueprint for the MVP.
> Constraint: **GCP only for now** (existing credits). All resources pinned to EU regions.
> Design rule: every external dependency (LLM provider, storage, queue) sits behind an interface, because migration to EU-owned infrastructure (Scaleway/OVH) and/or Mistral, plus a future self-hosted edition, are on the roadmap.

## 1. High-level flow

```
                ┌──────────────────────────── GCP (europe-central2 / europe-west3) ───────────────────────────┐
                │                                                                                             │
 User / API ────┼─► API service (Cloud Run) ──► Postgres  [jobs, tenants, usage]                              │
   │            │        │                                                                                    │
   │  signed    │        ├─► creates Job ──► queue ──► Worker service (Cloud Run, CPU)                        │
   └─ upload ───┼─► GCS bucket (raw-videos, EU, lifecycle: delete after processing)                           │
                │                                   │                                                         │
                │                                   ▼                                                         │
                │                     ┌─ Stage A: probe & prepare (ffmpeg)      ◄── PRIVATE PATH (paid)       │
                │                     ├─ Stage B: segment analysis (Vertex AI Gemini, native video input)     │
                │                     ├─ Stage C: fusion pass (Vertex AI Gemini, text)                        │
                │                     └─ Stage D: persist output ──► GCS bucket (outputs, EU) + DB metadata   │
                │                                   │                                                         │
                │                                   └─► webhook to customer / UI download / GET via API       │
                │                                                                                             │
 YouTube URL ───┼─► ✂ NO Stage A ─► Stage B (fileData.fileUri + videoMetadata offsets) ─► C ─► D              │
   (public)     │      no bytes ever downloaded — Google fetches   ◄── PUBLIC PATH (gallery production, internal-only) │
                └─────────────────────────────────────────────────────────────────────────────────────────────┘
```

**Two ingestion paths, deliberately asymmetric** (see BUSINESS_MODEL §10 — this is a hard boundary):

| | **Private path** (upload) | **Public path** (YouTube) |
|---|---|---|
| Purpose | **The paid product.** Internal recordings. | **Distribution only** — producing the curated gallery, run by us (DISTRIBUTION.md). **The free tool was dropped 2026-07-15: this path has NO public endpoint.** Never a paid tier. |
| Ingestion | Signed upload → GCS URI | `fileData.fileUri` = YouTube URL (+ mandatory `mimeType`). **We never download bytes.** ✅ Verified in both EU regions, Phase 0.1. |
| Stage A | Yes — ffmpeg, segmentation, static detection | **No.** No local bytes → no ffmpeg → no *per-segment* static detection. But see Cost: a **request-level** resolution knob survives. |
| Segmentation | ffmpeg cuts | ✅ **`videoMetadata.startOffset/endOffset`, confirmed server-side** (Phase 0.1): equal-length windows cost identical tokens at any position, tokens scale linearly with length. **Long videos segment without ever holding the bytes.** |
| Cost | All-in, blended (METRICS.md N6) | **Its own profile — METRICS.md §1.2b.** Cheaper than feared: `mediaResolution: LOW` needs no local analysis, so it applies here (N4b). With no public endpoint, spend is bounded by our own gallery runs (METRICS.md N10: any unauthenticated compute spend is a bug). |
| Constraint | — | Vertex accepts **public videos only, or ones owned by *our* GCP account** → customers' unlisted/private recordings can **never** use this path. |

⚠️ **VPC Service Controls disables `fileUri` media URLs entirely.** If the project is ever locked
down for enterprise compliance, the public path dies. Keep it isolated from the paid product.

## 2. Stack decisions (MVP)

| Concern | Choice | Rationale |
|---|---|---|
| API + workers | **.NET 10** (ASP.NET Core minimal API + worker service), containerized on **Cloud Run** | Founder's primary stack; Cloud Run = zero infra ops, scale-to-zero fits bursty video workloads. |
| Frontend | **Next.js 15** on Cloud Run (or static + API) | Founder already uses this stack (Portfel.pl); thin UI layer over the API. |
| Queue | **Cloud Tasks** (one queue per stage) | Managed retries, rate limiting, no Kafka/RabbitMQ ops for MVP. |
| DB | **Cloud SQL for PostgreSQL** (smallest HA-less tier for MVP) | Jobs, tenants, API keys, usage metering. |
| Object storage | **GCS**, two buckets: `raw-videos-eu`, `outputs-eu` | Region-pinned; lifecycle rules for auto-deletion. |
| Video tooling | **ffmpeg / ffprobe** in worker image | Segmentation, audio extraction, normalization, static-segment detection. |
| AI | **Vertex AI, `gemini-2.5-flash` @ `europe-central2`**; native video input from GCS URI | No self-managed GPUs, no separate STT+OCR+VLM pipeline in MVP. Verified 2026-07-14 (experiments/001): `gemini-2.5-flash`, `-flash-lite`, `-2.5-pro` served in `europe-central2`; only `gemini-2.5-flash` in `europe-west3` (fallback); no Gemini 3 in either EU region yet. |
| Auth | API keys (hashed) for API; email magic link or Google OAuth for UI | Keep MVP simple; SSO/SAML is enterprise roadmap. |
| Payments | Stripe (EU entity, EUR) | Subscriptions + metered overage. |
| IaC | Terraform | Region pinning and org policy as code from day one. |
| Observability | Cloud Logging + OpenTelemetry traces; per-job cost ledger in DB | Founder's standard; per-job token/cost tracking is a product feature (usage page), not just ops. |
| **Analytics** | **Umami, self-hosted in the EU, cookieless** (decided 2026-07-15; was Plausible) — pre-signup only, shares the product's Postgres instance. Post-signup truth is our own `events` table. | **CLAUDE.md rule 10 — no US analytics, ever.** Cookieless ⇒ **no consent banner ⇒ no funnel tax**; self-hosted ⇒ analytics data never leaves our own EU infra and no subprocessor is added. METRICS.md §6. |

Org policy: `constraints/gcp.resourceLocations` restricted to EU. All service accounts least-privilege. CMEK: not in MVP, keep design compatible.

## 3. Processing pipeline (the core IP)

### Stage A — Probe & prepare (CPU, ffmpeg) ✅ built in Phase 1

**One ffmpeg pass extracts two signals, and the distinction between them is the whole design:**

| Signal | Question it answers | What it drives |
|---|---|---|
| **Stillness** (frames: 1 fps, 160×90 gray, mean-abs-diff vs the *previous* frame) | *Is the picture moving?* | **Cost** — a frozen stretch is analysed at `MEDIA_RESOLUTION_LOW` |
| **Silence** (audio: `silencedetect`) | *Is anyone talking?* | **Block boundaries** — a citation belongs where the narration pauses |

🚨 **They are not interchangeable, and assuming they were is the trap.** See §8 and METRICS.md N7b/N7c.

1. `ffprobe`: duration, resolution, audio streams, container sanity. Reject corrupt files early — a
   rejection here is a Vertex call never paid for. (Frame rate is a *rational*; one committed fixture
   is `19001/317`.)
2. Normalize: transcode to a processing-friendly rendition if needed (target ≤ 720p for analysis).
3. **Static-content detection (cost).** Frame-difference timeline; runs of near-static picture ≥ 10 s
   are routed to **`MEDIA_RESOLUTION_LOW`** — measured ~45% cheaper, quality intact.
   ⚠️ **Not lower fps.** Phase 0 tested fps reduction (config B) and **rejected** it: it destabilises
   timestamps and coverage. **Media resolution is the lever; sampling rate is not** (§8).
   🔒 The metric and thresholds are a **faithful port of the Phase-0 experiment and are not free
   parameters** — METRICS.md N4's blended cost is an output of this algorithm at exactly these values.
   A calibration test asserts the port still reproduces the original measurement.
4. **Forced block boundaries (citation granularity) — the fix for N7b.** Stage A computes the block
   boundaries and **hands them to Stage B**, so granularity stops depending on the model noticing
   anything. Rule: **force a boundary on elapsed speech, suppress it on silence, snap it to a pause,
   and follow the picture when it does move** (drift from an anchor frame, not from the previous one —
   frame-to-frame differencing cannot see slow continuous change).
5. **Segmentation:** ~10–15 min segments with ~15–30 s overlap, each cut **snapped to a block boundary**
   so it lands on a scene change or a pause rather than mid-sentence. Emit per-segment sampling config
   and the boundaries inside it (rebased into segment time — Stage B's prompt speaks in segment time).

> 🔑 **Why forcing boundaries works, and why it is the private path's exclusive advantage.**
> Continuous screen recordings under-segment (METRICS.md **N7b**) — and the reason is **not** that they
> change too fast. Phase 1 measured the window that produced the failure and found **it hardly changes
> at all** (METRICS.md **N7c**): a presenter talking over a frozen IDE, held still for minutes. The model
> has nothing to segment on, and **every pixel-based rule is blind for exactly the same reason it is.**
> Boundaries must
> therefore come from the *narration*, not the picture — which needs the audio, which needs the bytes.
> **The public YouTube path can never do this** (§1): no bytes, no ffmpeg, no audio timeline.
>
> ⚠️ **Suppressing boundaries inside static runs is the rule that looks right and is wrong.** It is
> correct for slide talks and it switches the fix off precisely where the ICP lives, leaving 160-second
> blocks — *worse* than the status quo. Tried, measured, rejected (METRICS.md N7c).
>
> ⚠️ **Phase 1 proves what Stage A *emits*, not what Stage B *obeys*.** That verdict needs a Vertex call
> and is the first thing Phase 3 must check. If the model ignores the cues, the fallback is cutting real
> segment boundaries at them — which forces the issue, at the price of more calls.

### Stage B — Segment analysis (Vertex Gemini, native video)
For each segment (parallel, bounded concurrency per tenant):
- Input: GCS URI of segment + sampling params from Stage A.
- One structured prompt requesting **JSON** (use Vertex structured output / response schema):

```json
{
  "segment_start": "hh:mm:ss",
  "language": "…",
  "blocks": [
    {
      "t": "hh:mm:ss",
      "spoken": "cleaned transcript of what is said",
      "speaker": "Speaker 1 | name if stated | null",
      "on_screen_text": "verbatim text visible on screen (slides, code, UI labels)",
      "visual": "brief description of what is shown / demonstrated",
      "kind": "slide | demo | talking_head | screen_share | whiteboard | other"
    }
  ],
  "segment_summary": "…"
}
```
- Retries with exponential backoff; a failed segment fails only itself (job resumes, never restarts from zero — idempotency key = jobId+segmentIndex).

**🚨 Runaway-generation guards (mandatory — not optional hardening).** The Phase-0 benchmark
showed **~8% of calls degenerate**: `seg4_configA` produced 61k output tokens in 474 s;
`seg2_configB` burned 63k *thinking* tokens in 323 s. Two of ~25 calls, at ~2.5× normal cost
each. **This is the source of the observed 1.3× retry overhead, and untreated it breaks the
<15 min/video-hour SLO.** Every Stage B call must therefore set:

- **`maxOutputTokens`** — hard cap, sized from the measured p99 (~8.6k out-tokens/call), not left to default.
- **`thinkingBudget`** — bounded. Unbounded thinking is what produced the 63k-thought-token call.
- **A wall-clock request timeout** — well below the point where a single segment can eat the job's SLO.
- **Treat a *degenerate* call as a segment failure** — truncated or invalid JSON — and retry once
  with a tighter budget. Record every runaway in the cost ledger; this rate is a monitored metric
  (METRICS.md N7).
  ⚠️ **Reaching the `thinkingBudget` cap is NOT by itself a failure.** Phase 0.1 hit the 4,096-token
  cap on 5 of 8 calls and all 8 returned good output (`finishReason: STOP`, 90–98% coverage). A
  bounded budget being reached is the guard doing its job. **Judge the output, not the budget.**

🚨 **On `MAX_TOKENS`, HALVE THE SEGMENT — never retry it unchanged.** An output-cap overflow is
**deterministic**: the same clip at the same config overflows again, so a naive retry buys nothing
and bills twice. Phase 0.2 paid for exactly that.

> **The lesson that cost the most to learn: segment length does not bound Stage B's output.**
> On-screen *text density* does — and that is not knowable before the call. A 15-minute window blew
> the cap on both slide talks; the dense middle of one blew it **even at 10 minutes**, and had to be
> split to ~75 s. So the pipeline cannot merely *choose* a good segment size up front; it must
> **react** to an overflow by splitting and re-running. Halving is the cheap correct response: no
> content analysis, guaranteed to terminate, and the pieces are exactly what Stage C already fuses.
>
> ⚠️ **But naive halving is a cost amplifier — you pay for the failed parent AND both halves**
> (METRICS.md **N4d**: ~13× on a dense talk). Phase 3 must **segment dense content shorter up
> front**, using halving only as the backstop. Shipping the naive loop is shipping N4d.

⚠️ **Guard coverage on BOTH sides.** An under-coverage guard alone is half a guard: Phase 0.2
accepted a 15-minute segment whose blocks ran to `04:48:10` (**1921% coverage**) because nothing
ever looked *up*. **A citation to a timestamp that does not exist is worse than no citation** — it
is precisely the failure A4 exists to catch.

⚠️ **Normalize timestamps PER TIMESTAMP, not per response.** The model does not merely drift
between `hh:mm:ss` and `mm:ss:centiseconds` — **it mixes both inside one response** (`00:14:87`
beside genuine `hh:mm:ss`). A whole-response format detector cannot represent that, silently picks
the wrong reading, and produces the impossible timestamps above. Decide each timestamp on its own
(a minute/second field > 59 cannot be `hh:mm:ss`) and let monotonicity + clip length choose.

**Public-path variant (YouTube).** Same schema, same guards. Input is `fileData.fileUri` (YouTube
URL, `mimeType` mandatory) + `videoMetadata.startOffset/endOffset` instead of a GCS segment URI.
Public videos only. See §1. Three requirements are specific to this path (all measured in Phase 0.1,
`experiments/001-*/out/YOUTUBE.md`):

- **Run it at `mediaResolution: MEDIA_RESOLUTION_LOW` by default.** No Stage A means no *per-segment*
  static routing — but media resolution is a **request-level** knob that needs no local analysis, and
  it holds quality on this content (METRICS.md N4b) and keeps gallery-production runs cheap (N4c).
- 🚨 **The coverage guard must divide by the FETCHED duration, not the requested window.** On this
  path the video's length is unknown up front, and Vertex clamps the window to the video's end.
  Divide the video token count by the fixed tokenization rate (METRICS.md §1.2b) to recover what was
  actually fetched. Getting this wrong means **every video shorter than the window is a false
  under-generation failure, retried and billed twice** — observed on a 59 s video during the spike.
- **Ask for more than you expect.** Over-requesting a window is free (you are billed only for the
  overlap with the real video), and the response tells you the true duration. **No YouTube Data API
  call and no metadata scrape is needed** — rule 8 stays intact.

### Stage C — Fusion pass (Vertex Gemini, text-only)
- Input: ordered segment JSONs.
- Tasks: merge overlap duplicates, detect topic boundaries (not segment boundaries), produce final document per the **output contract** below, generate frontmatter (title, summary, tags, language, duration).
- For very long videos, fuse hierarchically (segments → chapters → document).

### Stage D — Persist & notify
- Write `output.md` (+ `output.json`, the structured form of the same document — schema:
  `tests/fixtures/contracts/output.schema.json`) to `outputs-eu`. Raw Stage-B segment JSONs are
  pipeline internals, deleted with the other intermediates — the API never exposes them.
- Update job status, record token usage + computed cost in the per-job ledger.
- Fire webhook (HMAC-signed) if configured; UI polls job status otherwise.
- **Delete source video and intermediate segments** (default; tenant-configurable retention up to N days). Bucket lifecycle rule as a safety net.

## 4. Output contract (Markdown) — ❄️ frozen 2026-07-16 (Phase 2.5)

Consistency of this schema across all files is a product feature — downstream RAG depends on it.

**Normative artifacts** (this section is the human-readable statement of the same contract):
the JSON twin's schema is `tests/fixtures/contracts/output.schema.json`; the executable grammar
is `web/lib/outputDocument.ts` (strict parser + canonical renderer — the Phase 3 Stage D renderer
must reproduce its byte output exactly); the committed examples are `tests/fixtures/output/*.md`
(the real Phase-0.2 corpus, normalized) with their `.json` twins.

```markdown
---
title: "Q3 Platform Demo — Billing Module"
source: "demo-billing.mp4"
duration: "00:47:12"
language: "en"
processed_at: "2026-07-14T10:22:00Z"
generator: "mdreel@<version>"
summary: "One-paragraph abstract of the video."
tags: [demo, billing, azure]
---

# Q3 Platform Demo — Billing Module

## [00:00:00] Introduction

**Spoken:** …cleaned transcript…

## [00:03:40] Invoice workflow walkthrough

**Spoken:** …cleaned transcript; a dialogue puts each turn on its own line…

**On screen:**
> Invoices → Create → "Apply proration" checkbox
> code: InvoiceService.CreateAsync(...)

**Visual:** Presenter navigates the admin panel, opens the invoice editor…

---

## Source & licence

Generated by **mdreel** from your uploaded recording. The source video was deleted after
processing per the default retention policy.
```

Rules (each learned from real drift in the Phase-0.2 outputs — see the Phase 2.5 rulings in git):

- **LF line endings, one trailing newline.** (The corpus shipped CRLF and every consumer paid a
  normalization tax.)
- **Frontmatter:** exactly these eight keys, double-quoted scalars, `tags` as a flow list of
  lowercase slugs. `source` is the original upload filename on the private path, the canonical
  source URL on internal ingest (it was `source_filename`, a lie on the URL path — renamed at the
  freeze). `processed_at` is UTC ISO-8601. `generator` is `mdreel@<version>`; committed fixtures
  use the reserved `mdreel@0.0.0-fixture` so fixture bytes can never masquerade as pipeline output.
- **H1 equals `title` byte-for-byte**; every other heading is an H2.
- **Sections:** `## [hh:mm:ss] Topic` — brackets mandatory, timestamps strictly ascending.
- **Blocks:** per section, each of `**Spoken:**` / `**On screen:**` / `**Visual:**` at most once,
  in that order, at least one present; one blank line between blocks. `Spoken`/`Visual` text starts
  on the label's own line and may continue on following lines. `On screen` is **verbatim** screen
  text: the label stands alone, content is `> ` blockquote lines, one per capture — blockquoting is
  what lets verbatim content contain anything (headings, rules) without breaking the grammar.
- **Provenance:** every document ends with `---` then `## Source & licence` — attribution +
  licence for gallery outputs (matching `corpus.json`'s audit trail), the source-deletion/retention
  statement for private uploads. It is the only non-timestamped H2, always last, so RAG chunkers
  can strip or keep it deterministically.
- **No hallucinated names** — unknown speakers stay "Speaker N".

## 5. API (v1, REST) — ❄️ MVP subset frozen 2026-07-16 (Phase 2.5)

Base: `/api/v1`, auth: `Authorization: Bearer <api_key>`.

**Normative artifacts:** the response shapes live as JSON Schemas in `tests/fixtures/contracts/`
(`upload-created`, `job-created`, `job-status`, `job-list`, `problem`, `output`) — validated
against the web mocks by `web/lib/contracts.test.ts` today, and the source material for the
OpenAPI spec published in Phase 3/4. This table is the human-readable statement of the same
contract.

| Method & path | Purpose |
|---|---|
| `POST /uploads` | `201 { uploadId, uploadUrl }` — GCS signed URL (resumable upload; max size per plan). |
| `POST /jobs` | Body: `{ uploadId, options { language_hint, retention_days, webhook_url, quality: standard\|high } }` → `202 { jobId }`. |
| `GET /jobs` | The authenticated panel's job list: `{ jobs: [ { jobId, status, stage?, progress, source, duration_sec?, created_at, finished_at? } ] }`, newest first. Pagination is Phase 3, additive. |
| `GET /jobs/{id}` | `{ status: queued\|processing\|done\|failed, stage?: A–D, progress: 0–100, cost_cents?, duration_sec?, wall_clock_sec? }` — `stage` only while processing; cost/duration when known. |
| `GET /jobs/{id}/output.md` / `output.json` | The document, Markdown (§4) or its structured twin (`output.schema.json`). Signed download or inline. |
| `DELETE /jobs/{id}` | `204` — erasure: deletes outputs + metadata, audit-logged (GDPR right-to-erasure endpoint). |
| `GET /usage` | `{ period, hours_processed, hours_quota, hours_remaining }`. In the frozen contract; mocked no earlier than Phase 3 (the panel has no usage screen this phase). |
| `POST /webhooks/test` | `200 { delivered, signature }` — verify webhook config (HMAC, `sha256=…`). |

🚨 **There is no public/unauthenticated compute endpoint — by design, not omission** (free tool
dropped 2026-07-15; METRICS.md N10: any unauthenticated compute spend is a bug). The gallery is
pre-rendered; the internal YouTube path is a runner we invoke, not an API surface.

Errors: RFC 7807 problem+json (`problem.schema.json`), on every non-2xx response. Rate limits per
key. OpenAPI spec published (Phase 3/4, generated from the schemas above); docs page includes
`llms.txt` and covers **REST + webhooks + MCP**. **The MCP server ships in the MVP** (decided
2026-07-15) as a thin layer over this API — no separate business logic; PLAN.md Phase 4 owns the
build order and the cut rule.

## 6. Data model (initial)

```
tenants(id, name, plan, created_at, retention_days_default,
        -- 🔑 first-touch attribution: MUST survive to payment or real CAC is uncomputable
        first_utm_source, first_utm_medium, first_utm_campaign, first_utm_term,
        first_referrer, ab_arm, archive_hours, monthly_hours, …)
users(id, tenant_id, email, role)
api_keys(id, tenant_id, hash, label, created_at, last_used_at, revoked_at)
jobs(id, tenant_id, status, stage, source_object, output_object, duration_sec,
     language, options_json, error_json, created_at, started_at, finished_at)
job_segments(job_id, index, status, sampling_fps, tokens_in, tokens_out, cost_cents, attempt)
usage_ledger(id, tenant_id, job_id, hours, cost_cents, billed_period)
events(id, tenant_id, user_id, session_id, name, occurred_at, referrer, ab_arm, payload_json)
ad_spend(id, day, channel, campaign, keyword, cost_cents, clicks, impressions)  -- METRICS.md N29
webhooks(id, tenant_id, url, secret, events)
audit_log(id, tenant_id, actor, action, subject, at)   -- data access & deletion events
```

**Two things here exist for reasons that are not obvious from the schema:**

- 🔑 **`tenants.first_utm_*` / `first_referrer` / `ab_arm`.** Attribution must be **first-party** and
  must **survive from first touch all the way to `payment_succeeded`.** Ad platforms report their
  own conversions and are marking their own homework; **the only trustworthy CAC is our spend ÷ our
  payments, joined on our data** (METRICS.md §6.3). **Design this into the frontend at PLAN.md
  Phase 2 — it cannot be bolted on at Phase 4.**
- 🚨 **`ad_spend`.** Ad euros are a real cost that is currently metered nowhere — **the same bug as
  the ffmpeg omission.** Contribution per account is a fiction if acquisition cost is missing from
  it. CLAUDE.md rule 6 now covers ad spend as well as LLM calls and compute.

## 7. GDPR / security architecture (product features, not paperwork)

- **Region pinning:** all compute, storage, and Vertex endpoints in EU; enforced by org policy + Terraform.
- **Retention by design:** source video deleted post-processing by default; configurable; documented data-flow diagram on /gdpr page.
- **Erasure:** `DELETE /jobs/{id}` cascades storage + DB; audit-logged.
- **No training on customer data:** Vertex AI standard terms + contractual clause in our DPA; stated publicly.
- **Subprocessor list (short, published):** Google Cloud (EU regions), Stripe, email provider.
  That's it for MVP — and the shortness is itself the product. (Analytics is **self-hosted Umami**
  on our own EU GCP — not a subprocessor at all.)
- 🚨 **No US-based analytics or tracking, on any property, ever** (CLAUDE.md rule 10). **Google
  Analytics is prohibited** — ruled unlawful by several EU DPAs over US transfers. Analytics is
  **cookieless, so there is no consent banner.** Applies equally to heatmaps, session replay, chat
  widgets and marketing pixels. **We sell EU residency to DPOs; the same standard must hold in our
  own stack, or the differentiator is a lie a competent DPO will catch.**
- **Tenant isolation:** row-level tenancy in DB; per-tenant GCS prefixes; signed URLs scoped and short-lived.
- **Encryption:** GCP default at-rest + TLS in transit; CMEK on roadmap.
- **Provider abstraction:** `IVideoAnalyzer` / `ITextFuser` interfaces; Vertex implementation now, Mistral (EU) implementation planned — this is also the seam for the future self-hosted edition.

## 8. Cost engineering (the mechanisms — **not** the numbers)

> **All cost figures, thresholds and the break-even price live in METRICS.md §1.2–§1.3 and are not
> restated here.** This section owns the *mechanisms* that produce them.
>
> ✅ **COGS is measured, solved, and is not a business risk.** Every lever below exists to **guard**
> the margin against regression — **not to chase it.** Further cost engineering optimizes the one
> variable that is already safe; the binding constraint is METRICS.md N1.

- **Per-job ledger of tokens and cents.** Surfaced to the customer as transparency; used internally
  to watch margin. This is a product feature, not ops telemetry.
- 🚨 **The ledger must meter compute, not just LLM calls.** ffmpeg transcode/segmentation on Cloud
  Run is **~30% of true COGS and is currently metered nowhere** — every cost figure we hold is
  optimistic by roughly a third until this lands. CLAUDE.md rule 6 is extended from "every LLM
  call" to "every LLM call **and every compute step**". → PLAN.md Phase 3.
- **Levers, in order of impact:**
  1. **Low media resolution** (`mediaResolution: MEDIA_RESOLUTION_LOW`) — measured ~45% cheaper.
     **fps-reduction was tested and rejected:** it destabilizes timestamps and coverage.
     - *Private path:* applied **per segment**, to the ~67% of a real demo recording that is static
       (runs ≥ 10 s) — that share is what makes the lever large.
     - *Public path:* the **per-segment routing** is unavailable (no local bytes → no ffmpeg → no
       static detection), but the knob is set **per request** and needs no local analysis, so it is
       simply applied to the whole path (METRICS.md N4b). **The lever is not lost, only coarsened.**
  2. ≤720p analysis rendition.
  3. Flash-tier model for Stage B; a better model only for Stage C fusion, and only if quality
     demands it.
  4. Batch/queue smoothing to stay inside provider rate limits.
  5. **Bounded `thinkingBudget`** — unbounded thinking blew one benchmark call to 63k thought
     tokens at ~3× cost. See the mandatory guards in §3.
- **Correctness requirements that fall out of the benchmarks** (`experiments/001-*/`): Stage B must
  **normalize model timestamps per timestamp** (formats drift *and mix within one response*),
  **guard coverage on both sides** (under- *and* over-coverage), and **split on `MAX_TOKENS` rather
  than retry**. All three are specified in §3 — each was learned by paying for it.
- 🚨 **Continuous screen recordings are the weak category — and they are what the paying product
  ingests.** Blocks cluster and coarsen against slide talks (METRICS.md **N7b**, **N32**), so
  citations get vague exactly where the ICP lives. Seen in all three benchmark phases; it is
  reproducible, not noise.
  ✅ **Addressed in Phase 1 — but *not* the way this section previously assumed** (METRICS.md **N7c**
  owns the measurement). It said static-content detection should force the boundaries. **It cannot: the
  footage that fails hardly moves at all.** Stillness is what makes the model blind there, so it cannot
  also be the cure. Stage A forces boundaries on **elapsed narration** and suppresses them on
  **silence** — a signal that needs the *audio*, which needs the bytes, which is why the public path can
  never do it (§3).
  ⚠️ Still unproven: whether Stage B *honours* the boundaries. Phase 3's first job.

## 9. Build order

**→ PLAN.md is the sole authority on sequencing.** It used to be mirrored here, which is exactly
how two documents drift apart. ARCHITECTURE.md owns the *target design*; PLAN.md owns the *order it
gets built in*.

## 10. Deliberately deferred

*(Revised 2026-07-15 — moved INTO the MVP at the Phase 2 founder review: the two-plan checkout
(one hard-capped, one with metered overage — BUSINESS_MODEL §6), the authenticated panel (upload ·
job list · manage/download/delete), the docs page, and the MCP server.)*

**Until there is a paying customer:** Terraform / Cloud SQL / Cloud Tasks (Cloud SQL idles at
~€25–50/mo against a ~€300/mo fixed base — real burn at zero users), usage/cost-analytics UI
beyond the panel's job list, DPA self-service flow, team/multi-user management.

**Roadmap proper:** connectors (Teams/Zoom/Drive), speaker diarization improvements,
multi-language UI, SSO/SAML, CMEK, SOC 2, self-hosted edition (the provider-abstraction seam and
stateless workers keep it feasible), Mistral backend, EU-owned infra migration.

**Never (hard boundary, BUSINESS_MODEL §10):** YouTube processing as a *paid* feature — and since
2026-07-15, no public YouTube input box at all (the gallery is produced internally).
