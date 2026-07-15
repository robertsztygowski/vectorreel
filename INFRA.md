# Infrastructure Notes — GCP Access & Decisions

> Operational notes for mdreel infrastructure. Companion to ARCHITECTURE.md.
> Last verified: 2026-07-15.

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

### APIs NOT yet enabled (needed later — enable when we reach them)
- `cloudtasks.googleapis.com` — stage queues (Stage A→B→C→D)
- `cloudbuild.googleapis.com` — CI container builds (if used)
- `secretmanager.googleapis.com` — API-key secrets, Stripe keys, webhook secrets

Enable with:
```bash
gcloud services enable cloudtasks.googleapis.com secretmanager.googleapis.com \
  cloudbuild.googleapis.com --project tensile-runway-442915-j6
```

## Deployed services

### `vectorreel-web` — landing page (Cloud Run)
| Item | Value |
|---|---|
| **Service** | `vectorreel-web` |
| **Region** | `europe-central2` (Warsaw, EU) |
| **URL** | https://vectorreel-web-92936629017.europe-central2.run.app |
| **Access** | public (`--allow-unauthenticated`) |
| **Container** | nginx:1.27-alpine serving static site on port 8080 |
| **Scaling** | min 0 / max 3 instances, 256Mi / 1 CPU (scale-to-zero) |
| **Source** | `web/` (index.html, styles.css, favicon.svg, Dockerfile, nginx.conf) |
| **Deployed** | 2026-07-14, first revision `vectorreel-web-00001-lvc` |

Build artifacts land in Artifact Registry repo `cloud-run-source-deploy` (auto-created, europe-central2).

**Redeploy after editing `web/`:**
```bash
gcloud run deploy vectorreel-web --source web --region europe-central2 \
  --allow-unauthenticated --project tensile-runway-442915-j6 --quiet
```

### Custom domain — `mdreel.com`

**Product name: `mdreel`. Domain `mdreel.com` purchased 2026-07-15** (founder, personal registrar).
Remaining, in order:
- **DNS delegation + Search Console verification** — founder-only (registrar access).
- **Map `mdreel.com` (apex + `www`) to the `vectorreel-web` Cloud Run service** via Cloud Run domain
  mappings, `europe-central2`.

This domain is the gate on PLAN.md Phase 0.3 (measurement, ads, first post).

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

2. **Auth identity.** gcloud is authenticated as `info@propspire.com`, while the founder's
   personal email is `robertsztygowski@gmail.com`. Confirm which identity/service account owns
   mdreel infra long-term; provision a least-privilege service account for CI/deploys rather
   than using a human account.

3. **Region pinning still applies at resource level.** Even without the org policy, pin every
   resource (buckets, Cloud SQL, Cloud Run, Vertex endpoints) to EU regions
   (`europe-central2` / `europe-west3`) explicitly in Terraform. Verify Gemini model
   availability in the chosen EU region at implementation time (ARCHITECTURE.md §2).
