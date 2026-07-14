# Architecture — Video-to-Markdown for AI Knowledge Bases (EU SaaS on GCP)

> Companion to BUSINESS_MODEL.md. This is the implementation blueprint for the MVP.
> Constraint: **GCP only for now** (existing credits). All resources pinned to EU regions.
> Design rule: every external dependency (LLM provider, storage, queue) sits behind an interface, because migration to EU-owned infrastructure (Scaleway/OVH) and/or Mistral, plus a future self-hosted edition, are on the roadmap.

## 1. High-level flow

```
                ┌──────────────────────────── GCP (europe-central2 / europe-west3) ───────────────────────────┐
                │                                                                                             │
 User / API ────┼─► API service (Cloud Run) ──► Postgres (Cloud SQL)  [jobs, tenants, usage]                  │
   │            │        │                                                                                    │
   │  signed    │        ├─► creates Job ──► Cloud Tasks queue ──► Worker service (Cloud Run, CPU)            │
   └─ upload ───┼─► GCS bucket (raw-videos, EU, lifecycle: delete after processing)                           │
                │                                   │                                                         │
                │                                   ▼                                                         │
                │                     ┌─ Stage A: probe & prepare (ffmpeg)                                    │
                │                     ├─ Stage B: segment analysis (Vertex AI Gemini, native video input)     │
                │                     ├─ Stage C: fusion pass (Vertex AI Gemini, text)                        │
                │                     └─ Stage D: persist output ──► GCS bucket (outputs, EU) + DB metadata   │
                │                                   │                                                         │
                │                                   └─► webhook to customer / UI download / GET via API       │
                └─────────────────────────────────────────────────────────────────────────────────────────────┘
```

## 2. Stack decisions (MVP)

| Concern | Choice | Rationale |
|---|---|---|
| API + workers | **.NET 10** (ASP.NET Core minimal API + worker service), containerized on **Cloud Run** | Founder's primary stack; Cloud Run = zero infra ops, scale-to-zero fits bursty video workloads. |
| Frontend | **Next.js 15** on Cloud Run (or static + API) | Founder already uses this stack (Portfel.pl); thin UI layer over the API. |
| Queue | **Cloud Tasks** (one queue per stage) | Managed retries, rate limiting, no Kafka/RabbitMQ ops for MVP. |
| DB | **Cloud SQL for PostgreSQL** (smallest HA-less tier for MVP) | Jobs, tenants, API keys, usage metering. |
| Object storage | **GCS**, two buckets: `raw-videos-eu`, `outputs-eu` | Region-pinned; lifecycle rules for auto-deletion. |
| Video tooling | **ffmpeg / ffprobe** in worker image | Segmentation, audio extraction, normalization, static-segment detection. |
| AI | **Vertex AI, Gemini Flash (current generation)** with EU-regional endpoint; native video input from GCS URI | No self-managed GPUs, no separate STT+OCR+VLM pipeline in MVP. Verify model availability per EU region at implementation time. |
| Auth | API keys (hashed) for API; email magic link or Google OAuth for UI | Keep MVP simple; SSO/SAML is enterprise roadmap. |
| Payments | Stripe (EU entity, EUR) | Subscriptions + metered overage. |
| IaC | Terraform | Region pinning and org policy as code from day one. |
| Observability | Cloud Logging + OpenTelemetry traces; per-job cost ledger in DB | Founder's standard; per-job token/cost tracking is a product feature (usage page), not just ops. |

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
tenants(id, name, plan, created_at, retention_days_default, …)
users(id, tenant_id, email, role)
api_keys(id, tenant_id, hash, label, created_at, last_used_at, revoked_at)
jobs(id, tenant_id, status, stage, source_object, output_object, duration_sec,
     language, options_json, error_json, created_at, started_at, finished_at)
job_segments(job_id, index, status, sampling_fps, tokens_in, tokens_out, cost_cents, attempt)
usage_ledger(id, tenant_id, job_id, hours, cost_cents, billed_period)
webhooks(id, tenant_id, url, secret, events)
audit_log(id, tenant_id, actor, action, subject, at)   -- data access & deletion events
```

## 7. GDPR / security architecture (product features, not paperwork)

- **Region pinning:** all compute, storage, and Vertex endpoints in EU; enforced by org policy + Terraform.
- **Retention by design:** source video deleted post-processing by default; configurable; documented data-flow diagram on /gdpr page.
- **Erasure:** `DELETE /jobs/{id}` cascades storage + DB; audit-logged.
- **No training on customer data:** Vertex AI standard terms + contractual clause in our DPA; stated publicly.
- **Subprocessor list (short, published):** Google Cloud (EU regions), Stripe, email provider. That's it for MVP.
- **Tenant isolation:** row-level tenancy in DB; per-tenant GCS prefixes; signed URLs scoped and short-lived.
- **Encryption:** GCP default at-rest + TLS in transit; CMEK on roadmap.
- **Provider abstraction:** `IVideoAnalyzer` / `ITextFuser` interfaces; Vertex implementation now, Mistral (EU) implementation planned — this is also the seam for the future self-hosted edition.

## 8. Cost engineering (must-have, drives pricing)

- Per-job ledger of tokens and cents (surfaced to the customer as transparency, used internally to watch margin).
- Levers, in order of impact: (1) static-segment low-fps sampling, (2) ≤720p analysis rendition, (3) Flash-tier model for Stage B, better model only for Stage C fusion if quality demands, (4) batch/queue smoothing to stay within provider rate limits.
- Benchmark task (week 1): process 3 reference videos (slide talk, product demo screen-recording, talking-head meeting) at 3 sampling configs; record cost/hour and quality notes. **Pricing in BUSINESS_MODEL.md is provisional until this benchmark exists.**

## 9. MVP scope checklist (build order)

1. Terraform: project, region policy, buckets + lifecycle, Cloud SQL, Cloud Run services, Cloud Tasks queues, service accounts.
2. Worker: Stage A (ffprobe/ffmpeg + segmentation + static detection) with golden tests on sample videos.
3. Worker: Stage B against Vertex (structured output), single segment end-to-end.
4. Worker: Stage C fusion + output contract renderer (deterministic, snapshot-tested).
5. API: uploads, jobs, outputs, usage; API keys; webhook.
6. Minimal UI: login, upload, job list with status, markdown preview + download, usage page.
7. Stripe: plans + metered overage; free-trial gating (2 h).
8. /gdpr + /security pages, DPA template, subprocessor list.
9. Benchmark + pricing lock; onboard first design partner.

## 10. Deliberately deferred

Connectors (Teams/Zoom/Drive), MCP server (thin layer over the API — good v1.1), speaker diarization improvements, multi-language UI, SSO/SAML, CMEK, SOC 2, self-hosted edition (the provider-abstraction seam and stateless workers keep it feasible), Mistral backend, EU-owned infra migration.
