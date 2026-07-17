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

## APIs NOT yet enabled (needed later — enable when we reach them)
- `cloudtasks.googleapis.com` — stage queues (Stage A→B→C→D)

Enable with:
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
| **Schema** | Self-created by the API on first DB use (`PostgresSchema.EnsureAsync`) — verified live 2026-07-17: `tenants`, `users`, `events`, `usage_ledger`, `ad_spend`, `payments`, `subscriptions`. |
| **⚠️ Cost** | **A real recurring bill** — the fixed-base tax METRICS.md N2 warns about, though this tier sits well below the range N2 prices in. **Teardown: `scripts/teardown-cloudsql.sh`** (deletes instance + secret). |

### `vectorreel-api` — backend API (Cloud Run)  ✅ deployed 2026-07-17

| Item | Value |
|---|---|
| **Region** | `europe-west1` · **URL** https://vectorreel-api-92936629017.europe-west1.run.app |
| **Revision** | `vectorreel-api-00001-r8n` (image `cloud-run-source-deploy/vectorreel-api:7ebcfbe`) |
| **Container** | `src/Api/Dockerfile` (repo-root build context) |
| **Config** | `PipelineModel__Mode=fake` (no Vertex spend; Stages B–D stubbed, payments fall back to `FakePaymentGateway` — no Stripe keys set), `--add-cloudsql-instances` + `POSTGRES_CONNECTION` from Secret Manager (unix-socket `Host=/cloudsql/…`), `--min-instances=0`, `--allow-unauthenticated` |
| **Verified** | `/health` 200; `POST /api/v1/events` 202 with Postgres persistence (schema auto-created) |

### `vectorreel-worker` — gallery worker (Cloud Run)  ✅ deployed 2026-07-17

| Item | Value |
|---|---|
| **Region** | `europe-west1` · **URL** https://vectorreel-worker-92936629017.europe-west1.run.app |
| **Revision** | `vectorreel-worker-00001-4g2` (image `cloud-run-source-deploy/vectorreel-worker:7ebcfbe`) |
| **Container** | `src/Worker/Dockerfile` (added 2026-07-17; repo-root context, `dotnet/runtime` + ffmpeg). Cloud Run services must listen on `$PORT`, so the Worker gained a minimal `HealthListener` (raw TCP 200-responder, active only when `PORT` is set — inert locally/in tests). |
| **Config** | `PipelineModel__Mode=fake`, `YouTubeGalleryRunner` disabled (default), in-memory ledger, no DB, `--min-instances=0` |

### GCS buckets  ✅ created 2026-07-17

`raw-videos-eu` and `outputs-eu`, both `europe-central2`, uniform bucket-level access
(ARCHITECTURE.md §2 / “Pipeline storage & model config” above). Idle-empty in fake mode.

### What a real (non-fake) production deploy still needs

Not provisioned — follow-up work, deliberately excluded from the 2026-07-17 deploy-path proof:
runtime service-account IAM for Vertex + GCS (least-privilege, not the default compute SA),
`PipelineModel__Mode=live`, Stripe secrets in Secret Manager, Cloud Tasks
(`cloudtasks.googleapis.com`), Umami on the shared Postgres (rule 10), and a dedicated
`vectorreel-eu` project (open flag 1 below).

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
| **Deployed** | 2026-07-17, revision `vectorreel-web-00002-9lc` — the current Phase 2R Next.js build (replaced the stale 2026-07-15 static-site revision via `scripts/deploy.sh web`) |

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
