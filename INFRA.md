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
| **Region** | `europe-west1` (Belgium, EU) — **moved here 2026-07-15**; `europe-central2`/`europe-west3` are NOT among the 8 Cloud Run domain-mapping-supported regions, so a native mapping needs an EU region that is |
| **URL** | https://vectorreel-web-92936629017.europe-west1.run.app |
| **Access** | public (`--allow-unauthenticated`) |
| **Container** | nginx static site (`web/Dockerfile`, port 8080) |
| **Scaling** | min 0 (scale-to-zero); deploy defaults otherwise |
| **Source** | `web/` (index.html, styles.css, favicon.svg, Dockerfile, nginx.conf) |
| **Deployed** | 2026-07-15, revision `vectorreel-web-00001-8zw` |

⚠️ **A now-stale `vectorreel-web` copy still runs in `europe-central2`** (the original). **Delete it once
`mdreel.com` serves from west1:**
`gcloud run services delete vectorreel-web --region europe-central2 --project tensile-runway-442915-j6`

Build artifacts land in Artifact Registry repo `cloud-run-source-deploy` (auto-created per region).

**Redeploy after editing `web/`:**
```bash
gcloud run deploy vectorreel-web --source web --region europe-west1 \
  --allow-unauthenticated --project tensile-runway-442915-j6 --quiet
```

### Custom domain — `mdreel.com`  (Route A: Google-served, Cloudflare DNS-only)

**Product name: `mdreel`. Domain `mdreel.com` purchased 2026-07-15** (founder, personal registrar).
**Cloudflare is DNS-only (grey cloud)** — no US-jurisdiction proxy in the request path; Google (EU,
`europe-west1`) serves the page and provisions the TLS cert. Steps (🧑 = founder-only):

1. 🧑 **Cloudflare zone** — add `mdreel.com` (Free plan); at the registrar set the nameservers to
   Cloudflare's two. Wait for the zone to go **Active**.
2. 🧑 **Create the mapping in the Cloud Console** (the CLI needs `gcloud beta`, which is not
   installable in the dev SDK): Cloud Run → *Manage custom domains* → *Add mapping* → service
   `vectorreel-web`, region `europe-west1`, domains `mdreel.com` + `www.mdreel.com`. It runs Search
   Console verification inline — add the TXT it gives you to Cloudflare (grey cloud), then verify.
3. 🧑 **Add the returned records to Cloudflare, all DNS-only (grey cloud):**
   | Name | Type | Value |
   |---|---|---|
   | `@` | A | `216.239.32.21` · `216.239.34.21` · `216.239.36.21` · `216.239.38.21` |
   | `@` | AAAA | `2001:4860:4802:32::15` · `2001:4860:4802:34::15` · `2001:4860:4802:36::15` · `2001:4860:4802:38::15` |
   | `www` | CNAME | `ghs.googlehosted.com` |
4. Google auto-provisions the managed cert (15 min–24 h). Then delete the stale `europe-central2`
   copy (see above).

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
