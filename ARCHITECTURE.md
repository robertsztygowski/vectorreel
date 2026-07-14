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
   (public)     │      no bytes ever downloaded — Google fetches   ◄── PUBLIC PATH (free tool + gallery)      │
                └─────────────────────────────────────────────────────────────────────────────────────────────┘
```

**Two ingestion paths, deliberately asymmetric** (see BUSINESS_MODEL §10 — this is a hard boundary):

| | **Private path** (upload) | **Public path** (YouTube) |
|---|---|---|
| Purpose | **The paid product.** Internal recordings. | **Distribution only** — free tool + gallery (DISTRIBUTION.md). Never a paid tier. |
| Ingestion | Signed upload → GCS URI | `fileData.fileUri` = YouTube URL. **We never download bytes.** |
| Stage A | Yes — ffmpeg, segmentation, static detection | **No.** No local bytes → no ffmpeg → **the static-content cost lever is unavailable.** |
| Segmentation | ffmpeg cuts | `videoMetadata.startOffset/endOffset` (verify: PLAN.md Phase 0.1) |
| Cost | All-in, blended (METRICS.md N6) | **Materially higher — no static lever** (METRICS.md §1.2), hence hard abuse caps + result caching (METRICS.md N10) |
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
| **Analytics** | **Plausible (EU-hosted), cookieless** — pre-signup only. Post-signup truth is our own `events` table. | **CLAUDE.md rule 10 — no US analytics, ever.** Cookieless ⇒ **no consent banner ⇒ no funnel tax**, and it becomes a line on `/gdpr` rather than a liability. METRICS.md §6. |

Org policy: `constraints/gcp.resourceLocations` restricted to EU. All service accounts least-privilege. CMEK: not in MVP, keep design compatible.

## 3. Processing pipeline (the core IP)

### Stage A — Probe & prepare (CPU, ffmpeg)
1. `ffprobe`: duration, resolution, audio streams, container sanity. Reject/flag corrupt files early.
2. Normalize: transcode to a processing-friendly rendition if needed (target ≤ 720p for analysis; slide-heavy content survives lower resolution and it cuts token cost massively).
3. **Segmentation:** split into segments of ~10–15 min with ~15–30 s overlap (overlap prevents context loss at boundaries).
4. **Static-content detection (cost control):** compute frame-difference / perceptual-hash timeline. Segments where the image is nearly static (one slide, talking head) are flagged `low_motion` → processed with lower video fps sampling. This single mechanism is expected to cut Gemini video-token cost by a large factor on typical corporate content. Emit per-segment sampling config.

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
- **Treat a cap-hit as a segment failure and retry once with a tighter budget**, rather than
  accepting truncated JSON. Record every runaway in the cost ledger — this rate is a monitored metric.

**Public-path variant (YouTube).** Same schema, same guards. Differences: input is
`fileData.fileUri` (YouTube URL) + `videoMetadata.startOffset/endOffset` instead of a GCS segment
URI; no Stage A means **no sampling config and no static-content lever** — the call runs at
default media resolution and costs accordingly. Public videos only. See §1.

### Stage C — Fusion pass (Vertex Gemini, text-only)
- Input: ordered segment JSONs.
- Tasks: merge overlap duplicates, detect topic boundaries (not segment boundaries), produce final document per the **output contract** below, generate frontmatter (title, summary, tags, language, duration).
- For very long videos, fuse hierarchically (segments → chapters → document).

### Stage D — Persist & notify
- Write `output.md` (+ `output.json` with the raw structured blocks — cheap to keep, valuable for API consumers) to `outputs-eu`.
- Update job status, record token usage + computed cost in the per-job ledger.
- Fire webhook (HMAC-signed) if configured; UI polls job status otherwise.
- **Delete source video and intermediate segments** (default; tenant-configurable retention up to N days). Bucket lifecycle rule as a safety net.

## 4. Output contract (Markdown)

Consistency of this schema across all files is a product feature — downstream RAG depends on it.

```markdown
---
title: "Q3 Platform Demo — Billing Module"
source_filename: "demo-billing.mp4"
duration: "00:47:12"
language: "en"
processed_at: "2026-07-14T10:22:00Z"
generator: "<product>@<version>"
summary: "One-paragraph abstract of the video."
tags: [demo, billing, azure]
---

# Q3 Platform Demo — Billing Module

## [00:00:00] Introduction
**Spoken:** …cleaned transcript…

## [00:03:40] Invoice workflow walkthrough
**Spoken:** …
**On screen:**
> Invoices → Create → "Apply proration" checkbox; code: `InvoiceService.CreateAsync(...)`
**Visual:** Presenter navigates the admin panel, opens the invoice editor…
```

Rules: every section anchored with `[hh:mm:ss]`; `On screen` is verbatim; `Visual` is a description; no hallucinated names — unknown speakers stay "Speaker N"; deterministic heading structure (H1 title, H2 timestamped topics).

## 5. API (v1, REST)

Base: `/api/v1`, auth: `Authorization: Bearer <api_key>`.

| Method & path | Purpose |
|---|---|
| `POST /uploads` | Returns GCS signed URL + `uploadId` (resumable upload; max size per plan). |
| `POST /jobs` | Body: `{ uploadId | sourceUrl, options { language_hint, retention_days, webhook_url, quality: standard|high } }` → `202 { jobId }`. |
| `GET /jobs/{id}` | Status: `queued | processing (stage, progress) | done | failed`, cost, duration. |
| `GET /jobs/{id}/output.md` / `output.json` | Signed download or inline. |
| `DELETE /jobs/{id}` | Erasure: deletes outputs + metadata (GDPR right-to-erasure endpoint). |
| `GET /usage` | Hours processed this period, remaining quota. |
| `POST /webhooks/test` | Verify webhook config (HMAC signature). |

Errors: RFC 7807 problem+json. Rate limits per key. OpenAPI spec published; docs page includes `llms.txt`.

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
  payments, joined on our data** (METRICS.md §6.3). **Design this in at Phase 3 — it cannot be
  bolted on at Phase 4.**
- 🚨 **`ad_spend`.** Ad euros are a real cost that is currently metered nowhere — **the same bug as
  the ffmpeg omission.** Contribution per account is a fiction if acquisition cost is missing from
  it. CLAUDE.md rule 6 now covers ad spend as well as LLM calls and compute.

## 7. GDPR / security architecture (product features, not paperwork)

- **Region pinning:** all compute, storage, and Vertex endpoints in EU; enforced by org policy + Terraform.
- **Retention by design:** source video deleted post-processing by default; configurable; documented data-flow diagram on /gdpr page.
- **Erasure:** `DELETE /jobs/{id}` cascades storage + DB; audit-logged.
- **No training on customer data:** Vertex AI standard terms + contractual clause in our DPA; stated publicly.
- **Subprocessor list (short, published):** Google Cloud (EU regions), Stripe, email provider,
  **Plausible (EU-hosted analytics)**. That's it for MVP — and the shortness is itself the product.
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
  call" to "every LLM call **and every compute step**". → PLAN.md Phase 2.
- **Levers, in order of impact:**
  1. **Static-segment low media resolution** (`mediaResolution: MEDIA_RESOLUTION_LOW`) — measured
     ~45% cheaper. **fps-reduction was tested and rejected:** it destabilizes timestamps and
     coverage. ~67% of a real demo recording is static (runs ≥ 10 s), which is what makes this
     lever large. **Unavailable on the public YouTube path** (no local bytes → no ffmpeg → no
     static detection) — see §1.
  2. ≤720p analysis rendition.
  3. Flash-tier model for Stage B; a better model only for Stage C fusion, and only if quality
     demands it.
  4. Batch/queue smoothing to stay inside provider rate limits.
  5. **Bounded `thinkingBudget`** — unbounded thinking blew one benchmark call to 63k thought
     tokens at ~3× cost. See the mandatory guards in §3.
- **Two correctness requirements that fall out of the benchmark** (`experiments/001-*/RESULTS.md`):
  Stage B must **deterministically normalize model timestamps** (formats drift between
  mm:ss:centiseconds and absolute-vs-relative anchoring) and **retry on under-coverage** (blocks
  cluster at clip start on continuous demo footage).

## 9. Build order

**→ PLAN.md is the sole authority on sequencing.** It used to be mirrored here, which is exactly
how two documents drift apart. ARCHITECTURE.md owns the *target design*; PLAN.md owns the *order it
gets built in*.

## 10. Deliberately deferred

**Until there is a paying customer:** Terraform / Cloud SQL / Cloud Tasks (Cloud SQL idles at
~€25–50/mo against a ~€300/mo fixed base — real burn at zero users), plan tiers + metered overage,
dashboard / job list / usage UI, markdown preview, DPA self-service flow.

**Roadmap proper:** connectors (Teams/Zoom/Drive), MCP server (thin layer over the API — good
v1.1), speaker diarization improvements, multi-language UI, SSO/SAML, CMEK, SOC 2, self-hosted
edition (the provider-abstraction seam and stateless workers keep it feasible), Mistral backend,
EU-owned infra migration.

**Never (hard boundary, BUSINESS_MODEL §10):** YouTube processing as a *paid* feature.
