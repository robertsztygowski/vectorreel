# Infrastructure Notes — GCP Access & Decisions

> Operational notes for mdreel infrastructure. Companion to ARCHITECTURE.md.
> Last verified: 2026-07-17.

## Spending guardrails  ✅ 2026-07-17

The real brake on runaway cost is **per-service `--max-instances` caps + `--min-instances=0`
(scale-to-zero)**, not the budget alert (which only notifies). Both are in place:

| Guardrail | Value |
|---|---|
| **Budget alert** | `mdreel-monthly-1000usd-equiv` on billing account `01BC4B-E1B72F-0D896D`, scoped to project `92936629017`, thresholds 50% / 90% / 100%. **Amount: 4000 PLN** — the billing account currency is **PLN**, so the founder-set ~$1000/month (GCP budget memo, 2026-07-17) is expressed as its PLN equivalent (~4 PLN/USD). |
| **Cloud Run caps** | `vectorreel-web` max 3, `vectorreel-api` max 2, `vectorreel-worker` max 1; **`--min-instances=0` (scale-to-zero) on all three**. |

`scripts/preflight.sh` asserts the caps (`check_run_caps`) on every deployed service — a service
that has drifted off `min=0` or its expected max fails preflight. `billingbudgets.googleapis.com`
was enabled 2026-07-17 to create the budget (no idle cost).

## Current GCP project (MVP — provisional)

| Item | Value |
|---|---|
| **Project ID** | `tensile-runway-442915-j6` |
| **Project name** | Propspire |
| **State** | ACTIVE |
| **Billing** | Enabled — account `billingAccounts/01BC4B-E1B72F-0D896D` |
| **Auth account** | `info@propspire.com` (active gcloud identity) |
| **gcloud SDK** | 562.0.0 |

**Decision (2026-07-14):** Proceed on the existing `tensile-runway-442915-j6` project **for now**.
Access verified: project reachable, billing on, core APIs enabled.

### APIs already enabled (relevant to mdreel)
- `aiplatform.googleapis.com` — Vertex AI (Gemini)
- `run.googleapis.com` — Cloud Run
- `sqladmin.googleapis.com` / `sql-component.googleapis.com` — Cloud SQL
- `storage.googleapis.com` (+ storage-api, storage-component) — GCS
- `artifactregistry.googleapis.com` — container images
- `cloudbuild.googleapis.com` — Cloud Run source deploys (web)
- `secretmanager.googleapis.com` — enabled 2026-07-17 (DB connection secret; later Stripe keys)

## Database decision — Phase 4

**One shared Postgres instance is the product source of truth** (METRICS.md §6.2): application
events, tenants, payments, usage ledger, and ad-spend ledger all join in our own data. Local
development uses the docker-compose `vectorreel` database. Production uses the smallest practical
Cloud SQL for PostgreSQL instance in `europe-central2`, keeping the resource explicitly EU-pinned
(CLAUDE.md rule 2). **Provisioned 2026-07-17 — see “Cloud SQL — `mdreel-db`” under Deployed
services below.**

Umami in Phase 5 shares this same Postgres instance and must not create a second idle database
(METRICS.md §6.2). Stripe API keys and the Stripe webhook signing secret live in Secret Manager;
never put them in repo files or Cloud Run plaintext env vars.

## Pipeline storage & model config — Phase 3

The pipeline (`src/Infrastructure`, wired via `AddPipelineInfrastructure`) is configured entirely
from env/config, EU-pinned per CLAUDE.md rule 2:

- **Object storage (`IObjectStorage`)** — GCS via `Google.Cloud.Storage.V1`, ADC only (no JSON
  keys, rule 1). Two buckets, both `europe-central2`: `raw-videos-eu` (uploaded source, deleted
  after Stage D) and `outputs-eu` (`output.md` + `output.json`). Set `Gcs__EmulatorHost` to use
  fake-gcs-server locally (buckets auto-created in emulator mode); unset in prod → real GCS.
- **Vertex (`IVideoAnalyzer` / `ITextFuser`)** — `gemini-2.5-flash` @ `europe-central2`, fallback
  `europe-west3` (`VertexOptions__FallbackRegion`); Stage B→C back-to-back can trip
  `429 RESOURCE_EXHAUSTED` on the primary region. **Implemented (hardening):** the analyzer/fuser
  retry the primary region then fall back to `europe-west3` on 429 (`VertexOptions__MaxRetriesPerRegion`,
  `VertexOptions__RetryDelay`), EU-only; the region that served the call is recorded in the ledger
  (`CostEntry.Region`).
- **`PipelineModel__Mode`** selects the seam implementations: `fake` (deterministic offline
  stand-in — the default, used by the E2E stack and host-side dev, zero spend), `live` (real
  Vertex — prod, the gallery worker, `tests/Live/`), `replay` (committed `tests/fixtures/llm/`
  fixtures), `record` (real Vertex + writes fixtures). LLM metering fires whenever a real Vertex
  call is made (the region-aware analyzer/fuser stamp `CostEntry.Region`; fake/replay leave it null,
  so they meter nothing). `PipelineModel__StageRawUploadsToObjectStorage` gates whether the private
  path stages raw bytes into `raw-videos-eu` (null ⇒ stage iff `Mode != fake`).

## APIs enabled for the queue flip (M5, 2026-07-17)
- `cloudtasks.googleapis.com` — **enabled**; backs the live `webhook-deliveries` queue (see the
  `vectorreel-api` “Task queue / webhooks” row below). Founder-approved this continuously-billing
  resource on 2026-07-17; `scripts/deploy.sh api` enables it idempotently.

Enable manually (normally handled by `scripts/deploy.sh api`) with:
```bash
gcloud services enable cloudtasks.googleapis.com --project tensile-runway-442915-j6
```

## Deployed services

> **2026-07-17 — deploy-path proof (deliberate, founder-authorized rule 5 override).** The API and
> Worker were deployed from local for the first time, and Cloud SQL was provisioned, as a one-off
> to prove the deploy mechanism end-to-end (green deploy + `/health` + a real DB round-trip). This
> does NOT change CLAUDE.md rule 5 — deploys remain deliberate, from local, after `tests/Live/`
> passes. Both services run `PipelineModel__Mode=fake` (zero Vertex spend); the API is Cloud
> SQL–backed, the Worker uses the in-memory ledger and its gallery runner is disabled.
>
> **2026-07-17 (extended) — 8-hour autonomous build run.** The founder authorized, **for that run
> only**, deploying to Cloud Run from local via `scripts/deploy.sh` after the definition-of-done
> gate passes (CLAUDE.md rule 4), to ship the auth / analytics / payments milestones. Still
> deliberate, still from local, still `PipelineModel__Mode=fake` (no Vertex spend), Stripe **test
> mode only**. **No CI auto-deploy was enabled.** The standing rule 5 is unchanged outside that run.
>
> **2026-07-18 — legal-pack run (rule 5 override, web-only).** The founder authorized, **for that
> run only**, redeploying `vectorreel-web` from local via `scripts/deploy.sh web` after the
> definition-of-done gate passes, to ship the B2B legal pack (`mdreel.com/legal/*`). The run touched
> **only `web/` + docs and created no new GCP resources** — a redeploy of the existing web service in
> `europe-west1` was the sole infra action. No CI auto-deploy was enabled; standing rule 5 unchanged.

### Deploy automation — `scripts/` (added 2026-07-17)

| Script | Does |
|---|---|
| `scripts/preflight.sh` | PASS/FAIL table: gcloud auth, ADC, project, required APIs (enables secretmanager if missing), docker, and ensures buckets `raw-videos-eu`/`outputs-eu` exist in `europe-central2`. |
| `scripts/provision-cloudsql.sh` | Idempotent (describe-before-create): `mdreel-db` instance, `vectorreel` database, `mdreel_app` user with generated password stored ONLY in Secret Manager (`mdreel-postgres-connection`, EU-replicated), IAM for the Cloud Run runtime SA. Prints the connection name. |
| `scripts/teardown-cloudsql.sh` | **Stops the Cloud SQL bill in one command** — deletes the instance + secret. |
| `scripts/deploy.sh [web\|api\|worker\|all]` | web via `gcloud run deploy --source web`; api/worker via local `docker build -f src/<X>/Dockerfile .` (repo-root context — `--source` can't use a nested Dockerfile) → push to Artifact Registry `cloud-run-source-deploy` → `gcloud run deploy --image`. All EU, `--min-instances=0`. API gets `--add-cloudsql-instances` + `POSTGRES_CONNECTION` injected via `--set-secrets` (never a plaintext env var). |
| `scripts/smoke-remote.sh` | Asserts: `/health` 200, web 200 (run.app + mdreel.com), DB round-trip (`POST /api/v1/events` → 202), Cloud SQL tables exist (via Cloud SQL Auth Proxy in docker — IAM-authed over outbound 3307; direct 5432 is often firewalled), all services Ready + EU, instance RUNNABLE + EU, buckets EU. Non-zero exit on any failure. |

All parameterized via `PROJECT`/`RUN_REGION`/`DATA_REGION`/… env vars with the defaults above.

### Cloud SQL — `mdreel-db` (Postgres)  ✅ provisioned 2026-07-17

| Item | Value |
|---|---|
| **Instance** | `mdreel-db` — POSTGRES_16, `db-f1-micro` (shared-core), 10 GB HDD, zonal (no HA) — the smallest possible footprint |
| **Region** | `europe-central2` (EU, rule 2) |
| **Connection name** | `tensile-runway-442915-j6:europe-central2:mdreel-db` |
| **Database / user** | `vectorreel` / `mdreel_app` |
| **Credential** | Full Npgsql connection string in Secret Manager secret `mdreel-postgres-connection` (user-managed replication, `europe-central2`). Never in git, never a plaintext env var (rule 1). |
| **Schema** | Self-created by the API on first DB use (`PostgresSchema.EnsureAsync`) — verified live 2026-07-17: `tenants`, `users`, `events`, `usage_ledger`, `ad_spend`, `payments`, `subscriptions`, `webhook_deliveries` (M5). |
| **⚠️ Cost** | **A real recurring bill** — the fixed-base tax METRICS.md N2 warns about, though this tier sits well below the range N2 prices in. **Teardown: `scripts/teardown-cloudsql.sh`** (deletes instance + secret). |

### `vectorreel-api` — backend API (Cloud Run)  ✅ deployed 2026-07-17

| Item | Value |
|---|---|
| **Region** | `europe-west1` · **URL** https://vectorreel-api-92936629017.europe-west1.run.app |
| **Revision** | `vectorreel-api-00009-h47` (image `cloud-run-source-deploy/vectorreel-api:5c7fdec` — unchanged since the M5 flip; revisions 00006–00009 were env/secret-only updates for the Stripe activation, 2026-07-18) |
| **Container** | `src/Api/Dockerfile` (repo-root build context) |
| **Config** | `PipelineModel__Mode=fake` (no Vertex spend; Stages B–D stubbed), **payments LIVE in Stripe test mode** (2026-07-18: `PAYMENTS_MODE` removed, `STRIPE_PRICE_PRO`/`STRIPE_PRICE_BUSINESS`/`STRIPE_PRICE_STARTER` set, `APP_BASE_URL=https://mdreel.com` so Checkout redirects land on `/billing/success|cancelled` instead of the `http://localhost` default), `--add-cloudsql-instances` + `POSTGRES_CONNECTION` from Secret Manager (unix-socket `Host=/cloudsql/…`), Stripe secrets `STRIPE_SECRET_KEY`/`STRIPE_WEBHOOK_SECRET` from Secret Manager (see below), `--min-instances=0`, `--allow-unauthenticated` |
| **Stripe secrets (M4, real test-mode values 2026-07-18)** | `mdreel-stripe-secret-key` (v2: `sk_test_…`) + `mdreel-stripe-webhook-secret` (v3: `whsec_…`) in Secret Manager (`europe-central2`, user-managed replication), wired via `--set-secrets` as `latest`; the runtime SA has `secretAccessor`. Test mode only — a `sk_live_…` key is a hard stop. ⚠️ **Webhook endpoint is pinned to Stripe API version `2025-09-30.clover`** to match the pin inside `Stripe.net 49.0.0`: the sandbox's default (`2026-06-24.dahlia`) makes `EventUtility.ConstructEvent` throw on every event → 400s. If the endpoint is ever recreated from the dashboard it inherits the account default again and webhooks silently break — recreate via API with `api_version=2025-09-30.clover` (and bump the pin when upgrading Stripe.net). Subscribed events: `checkout.session.completed`, `customer.subscription.updated`, `customer.subscription.deleted` (`payment_intent.succeeded` was dropped 2026-07-18 — a `PaymentIntent` carries no `tenant_id` metadata, so the handler 404s and Stripe retries forever; `checkout.session.completed` already does all the work). Verified e2e in prod: signup → checkout 201 → `4242…` test card → webhook 200 → billing portal 201 with a real portal URL. |
| **Task queue / webhooks (M5, flipped to Cloud Tasks 2026-07-17)** | `ITaskQueue` binds to **`CloudTasksQueue`** whenever `CloudTasks__ProjectId`/`QueueName`/`TargetBaseUrl` are set (the deployed api) — else `InProcessQueue` (local/CI/E2E, so tests stay hermetic: no ADC, no cloud). Deliveries enqueue to the **`webhook-deliveries`** queue in `europe-west1`; Cloud Tasks pushes `POST /internal/webhook-deliveries/{id}/attempt` carrying an **OIDC token** minted as the api runtime SA. That endpoint sits outside the `/api/v1` gate and self-validates the token (Google-signed, `email` = the SA, `aud` = the api URL) — unauthenticated calls get 401. `GoogleCloudTasksTransport` (`Google.Cloud.Tasks.V2`) is the real `ICloudTasksTransport`. Webhook delivery unchanged: `webhook_deliveries` table, HMAC-SHA256 signature (`X-Mdreel-Signature: sha256=…`, ARCHITECTURE §6), backoff `10·2^(n-1)`s capped 1h. **IAM** (set idempotently by `deploy.sh`): api SA gets `cloudtasks.enqueuer` on the queue + `iam.serviceAccountUser` on itself (actAs for the OIDC token); the Cloud Tasks service agent gets `iam.serviceAccountTokenCreator` on the api SA. **Cost:** the queue is pay-per-operation with a generous free tier and scales to zero — no new fixed base (METRICS.md **N2**). |
| **Verified** | `/health` 200; `POST /api/v1/events` 202 with Postgres persistence (schema auto-created) |

### `vectorreel-worker` — gallery worker (Cloud Run)  ✅ deployed 2026-07-17

| Item | Value |
|---|---|
| **Region** | `europe-west1` · **URL** https://vectorreel-worker-92936629017.europe-west1.run.app |
| **Revision** | `vectorreel-worker-00001-4g2` (image `cloud-run-source-deploy/vectorreel-worker:7ebcfbe`) |
| **Container** | `src/Worker/Dockerfile` (added 2026-07-17; repo-root context, `dotnet/runtime` + ffmpeg). Cloud Run services must listen on `$PORT`, so the Worker gained a minimal `HealthListener` (raw TCP 200-responder, active only when `PORT` is set — inert locally/in tests). |
| **Config** | `PipelineModel__Mode=fake`, `YouTubeGalleryRunner` disabled (default), in-memory ledger, no DB, `--min-instances=0` |

### `mdreel-umami` — analytics (Cloud Run)  ✅ deployed 2026-07-17  (M3, rule 10)

Self-hosted, cookieless, **EU-only** analytics — the rule-10 replacement for US SaaS trackers, so
**no consent banner**. Provisioned by `scripts/provision-umami.sh` (idempotent).

| Item | Value |
|---|---|
| **Service** | `mdreel-umami` · **Region** `europe-west1` · **URL** https://mdreel-umami-92936629017.europe-west1.run.app |
| **Image** | `ghcr.io/umami-software/umami:postgresql-latest` (deployed straight from GHCR) |
| **Scaling** | `--min-instances=0` (scale-to-zero) · `--max-instances=1` — no new fixed base cost (METRICS.md **N2**) |
| **Database** | own `umami` DB + least-privilege `umami_app` role on the **shared** `mdreel-db` instance (rule 10: never a second DB instance). `umami_app` owns only its DB/schema and has no privileges on product tables. |
| **Secrets** | `DATABASE_URL=mdreel-umami-database-url:latest`, `APP_SECRET=mdreel-umami-app-secret:latest` (both Secret Manager, `europe-central2`); Cloud SQL via `--add-cloudsql-instances` unix socket |
| **Website** | `mdreel.com`, id `d146675e-e289-439b-baba-901a21560db0` (not a secret). Tracking script wired into `web/app/layout.tsx` via `NEXT_PUBLIC_UMAMI_*` (Dockerfile ARG defaults); e2e build sets them empty so tests never hit the analytics origin. |
| **Admin login** | `admin`; the default `umami` password was **rotated 2026-07-17** to a strong value in Secret Manager `mdreel-umami-admin-password` (retrieve with `gcloud secrets versions access latest --secret mdreel-umami-admin-password`). |
| **Verified** | page_view via the collect API → Cloud SQL → dashboard stats returned `pageviews:1, visitors:1`. |

**stats.mdreel.com** ✅ **LIVE 2026-07-20** — founder added the Cloudflare CNAME `stats` →
`ghs.googlehosted.com` (DNS-only) 2026-07-18; cert issued; tracker switched to the custom origin
(`NEXT_PUBLIC_UMAMI_SCRIPT_URL=https://stats.mdreel.com`, web revision `vectorreel-web-00009-c8q`).
Verified: `stats.mdreel.com/script.js` 200, live page references only the new origin, and a
pageview POSTed via `stats.mdreel.com/api/send` returned 200.

### GCS buckets  ✅ created 2026-07-17

`raw-videos-eu` and `outputs-eu`, both `europe-central2`, uniform bucket-level access
(ARCHITECTURE.md §2 / “Pipeline storage & model config” above). Idle-empty in fake mode.

### What a real (non-fake) production deploy still needs

Not provisioned — follow-up work, deliberately excluded from the 2026-07-17 deploy-path proof:
runtime service-account IAM for Vertex + GCS (least-privilege, not the default compute SA),
`PipelineModel__Mode=live`, Stripe secrets in Secret Manager, and a dedicated `vectorreel-eu`
project (open flag 1 below).
(Umami analytics on the shared Postgres — rule 10 — is now provisioned; see `mdreel-umami` above.
Cloud Tasks — `cloudtasks.googleapis.com` — is now enabled and live; see the `vectorreel-api`
task-queue row above.)

### `vectorreel-web` — frontend (Cloud Run)
| Item | Value |
|---|---|
| **Service** | `vectorreel-web` |
| **Region** | `europe-west1` (Belgium, EU) — **moved here 2026-07-15**; `europe-central2`/`europe-west3` are NOT among the 8 Cloud Run domain-mapping-supported regions, so a native mapping needs an EU region that is |
| **URL** | https://vectorreel-web-92936629017.europe-west1.run.app |
| **Access** | public (`--allow-unauthenticated`) |
| **Container** | Next.js 15 standalone server — multi-stage `node:20-alpine` build (`web/Dockerfile`), port 8080. **Replaced the nginx static site 2026-07-15 (PLAN.md Phase 2)**; `nginx.conf` removed, Next.js serves itself. |
| **Scaling** | min 0 (scale-to-zero); deploy defaults otherwise |
| **Source** | `web/` — Next.js 15 App Router (`app/`, `components/`, `lib/`), `fixtures/` (a committed mirror of `experiments/001-*/out/`, regenerated by `scripts/sync-fixtures.mjs` — never hand-edited), `Dockerfile` |
| **Deployed** | 2026-07-18, revision `vectorreel-web-00008-d5k` — legal pack live at `mdreel.com/legal/{terms,privacy,imprint,dpa,subprocessors,acceptable-use}` (six B2B pages, footer trust column + signup/checkout agree-notice wired). `scripts/smoke-remote.sh` extended with a six-URL legal check (now 23/0). Earlier: `vectorreel-web-00007-l2g` — M6 real-API panel wire-up. The upload/job panel now runs the real ARCHITECTURE §5 funnel (create upload → PUT bytes → create job → poll → download `output.md`) same-origin via `web/middleware.ts`, and checkout posts same-origin `requestCheckout` so the keyless api (`PAYMENTS_MODE=disabled`) degrades to a clean 503 note. The mock `/api/*` routes remain (they back `web/lib/contracts.test.ts`); the `/app` library table still renders the corpus "sample library" (real-`/api/v1/jobs` list is next-run backlog). Earlier baseline: `vectorreel-web-00002-9lc` (Phase 2R) via `scripts/deploy.sh web` |

✅ **The stale `vectorreel-web` copy in `europe-central2` (the original) was deleted 2026-07-17**
(`gcloud run services delete vectorreel-web --region europe-central2`), after verifying
`mdreel.com` serves from west1.

Build artifacts land in Artifact Registry repo `cloud-run-source-deploy` (auto-created per region).

**Local smoke test before redeploying** (build + run the exact image Cloud Run will build):
```bash
docker build -t vectorreel-web-test web
docker run -p 8080:8080 vectorreel-web-test
```

**Redeploy after editing `web/`:** `scripts/deploy.sh web` (wraps the command below):
```bash
gcloud run deploy vectorreel-web --source web --region europe-west1 \
  --allow-unauthenticated --project tensile-runway-442915-j6 --quiet
```

### Custom domain — `mdreel.com`  ✅ LIVE 2026-07-15  (Route A: Google-served, Cloudflare DNS-only)

**`https://mdreel.com` serves the landing page over HTTPS.** Google-managed cert (issuer: Google
Trust Services `WR3`, `CN=mdreel.com`), backed by `vectorreel-web` in `europe-west1`. **Cloudflare is
DNS-only (grey cloud)** — no US-jurisdiction proxy in the request path; Google (EU) serves both the
page and the TLS. **This clears the domain gate for launch — PLAN.md Phase 5, formerly Phase 0.3**
(measurement, ads, first post).

**How it was set up (for reference / reproduction):**
- Cloudflare zone `mdreel.com` (Free); registrar nameservers → Cloudflare's two.
- Cloud Run domain mapping created in the **Cloud Console** (the dev SDK lacks `gcloud beta`):
  service `vectorreel-web` @ `europe-west1`, domains `mdreel.com` + `www.mdreel.com`; ownership
  verified via a Search Console TXT.
- Records in Cloudflare, all **DNS-only (grey cloud)** — flipping these off *Proxied* was the one
  gotcha: while proxied, Google cannot validate/issue the cert (and it would put a US proxy in path):
  | Name | Type | Value |
  |---|---|---|
  | `@` | A | `216.239.32.21` · `216.239.34.21` · `216.239.36.21` · `216.239.38.21` |
  | `@` | AAAA | `2001:4860:4802:32::15` · `2001:4860:4802:34::15` · `2001:4860:4802:36::15` · `2001:4860:4802:38::15` |
  | `www` | CNAME | `ghs.googlehosted.com` |
  | `@` | TXT | `google-site-verification=…` (kept DNS-only) |
- Cert provisioned ~1 min after the records went grey (Google quotes up to 24 h).

> ⚠️ The Cloud Run **service is still named `vectorreel-web`** — the code/service rename (C#
> namespaces, assembly names, service name) is **deferred to a dedicated refactor**. Domain and
> service name are independent; the mapping works regardless of the service's internal name.

### Custom domain — `api.mdreel.com`  ✅ LIVE 2026-07-20

The web client still uses the Next.js middleware auth proxy (same-origin `mdreel.com/api/v1/*` →
`vectorreel-api`, wired by `API_ORIGIN` in `deploy.sh`); the custom domain is direct-access
convenience for the api.

- Cloud Run domain mapping `api.mdreel.com` → `vectorreel-api` @ `europe-west1` created via the
  **Cloud Run Admin REST API** (`domains.cloudrun.com/v1` domainmappings) — the dev SDK lacks
  `gcloud beta` *and* its GA `run domain-mappings` is the namespace/GKE variant, so the REST call
  with an access token is the reproducible path:
  ```
  curl -X POST -H "Authorization: Bearer $(gcloud auth print-access-token)" \
    -H 'Content-Type: application/json' \
    https://europe-west1-run.googleapis.com/apis/domains.cloudrun.com/v1/namespaces/tensile-runway-442915-j6/domainmappings \
    -d '{"apiVersion":"domains.cloudrun.com/v1","kind":"DomainMapping","metadata":{"name":"api.mdreel.com"},"spec":{"routeName":"vectorreel-api"}}'
  ```
- Status: **`Ready=True`** — founder added the Cloudflare record 2026-07-18 (zone `mdreel.com`,
  **DNS-only / grey cloud**, same rule as `www`); cert issued and `https://api.mdreel.com/health`
  returns 200:

  | Name | Type | Value |
  |---|---|---|
  | `api` | CNAME | `ghs.googlehosted.com` |
  | `stats` | CNAME | `ghs.googlehosted.com` |

## Open flags to revisit before / at production

1. **Shared project, not dedicated.** `tensile-runway-442915-j6` is the "Propspire" project and
   already runs other workloads (BigQuery, storage, etc.). The architecture's EU region-pinning
   org policy (`constraints/gcp.resourceLocations`) **cannot be safely applied** here — it would
   constrain Propspire's existing resources too.
   - **TODO before production:** create a dedicated `vectorreel-eu` project so EU data-residency
     (the core product positioning) can be enforced at the org-policy level. Project creation
     likely needs elevated org/billing rights — get exact `gcloud projects create` commands ready.
   - **Cleanup done 2026-07-15:** the unrelated `hamster`, `lynx`/zaileposzlo.pl and `stt-mbank`
     workloads were deleted (Cloud Run services, the `lynx-db` Cloud SQL instance, two Artifact
     Registry repos, three buckets, five secrets, two service accounts). The project is now
     effectively mdreel-only, but it is **still the Propspire billing project** — the dedicated-
     project TODO above stands.
     ⚠️ Left in place, ownership unconfirmed: the `resend-api-key` secret and the `github-actions`
     ("Orca") service account — review whether either is still needed.

2. **Auth identity.** gcloud is authenticated as `info@propspire.com`, while the founder's
   personal email is `robertsztygowski@gmail.com`. Confirm which identity/service account owns
   mdreel infra long-term; provision a least-privilege service account for CI/deploys rather
   than using a human account.

3. **Region pinning still applies at resource level.** Even without the org policy, pin every
   resource (buckets, Cloud SQL, Cloud Run, Vertex endpoints) to EU regions
   (`europe-central2` / `europe-west3`) explicitly in Terraform. Verify Gemini model
   availability in the chosen EU region at implementation time (ARCHITECTURE.md §2).
