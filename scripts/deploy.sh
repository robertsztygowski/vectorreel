#!/usr/bin/env bash
# deploy.sh — deploy mdreel services to EU Cloud Run.
#
#   scripts/deploy.sh web
#   scripts/deploy.sh api
#   scripts/deploy.sh worker
#   scripts/deploy.sh all
#
# Web uses Cloud Run source deploy from web/. API and worker build locally from repo-root context,
# push to Artifact Registry, then deploy images. API requires scripts/provision-cloudsql.sh first.

set -euo pipefail
cd "$(dirname "$0")/.."

PROJECT="${PROJECT:-tensile-runway-442915-j6}"
RUN_REGION="${RUN_REGION:-europe-west1}"
DATA_REGION="${DATA_REGION:-europe-central2}"
SQL_INSTANCE="${SQL_INSTANCE:-mdreel-db}"
SQL_DATABASE="${SQL_DATABASE:-vectorreel}"
SQL_USER="${SQL_USER:-mdreel_app}"
SECRET_NAME="${SECRET_NAME:-mdreel-postgres-connection}"
CLOUDTASKS_QUEUE="${CLOUDTASKS_QUEUE:-webhook-deliveries}"

cmd="${1:-}"
shift || true

AR="$RUN_REGION-docker.pkg.dev/$PROJECT/cloud-run-source-deploy"
TAG="$(git rev-parse --short HEAD)"

usage() {
  sed -n '2,13p' "$0"
}

describe_deploy() {
  local service="$1"
  local revision
  local url

  read -r revision url < <(gcloud run services describe "$service" \
    --region "$RUN_REGION" \
    --project "$PROJECT" \
    --format='value(status.latestReadyRevisionName,status.url)')
  echo "$service revision=$revision url=$url"
}

require_api_infra() {
  if ! gcloud sql instances describe "$SQL_INSTANCE" --project "$PROJECT" >/dev/null 2>&1; then
    echo "Cloud SQL instance $SQL_INSTANCE not found. Run scripts/provision-cloudsql.sh first." >&2
    exit 1
  fi

  if ! gcloud secrets describe "$SECRET_NAME" --project "$PROJECT" >/dev/null 2>&1; then
    echo "Secret $SECRET_NAME not found. Run scripts/provision-cloudsql.sh first." >&2
    exit 1
  fi

  gcloud sql instances describe "$SQL_INSTANCE" \
    --project "$PROJECT" \
    --format='value(connectionName)'
}

# Stripe secrets were created EMPTY (whitespace placeholder) pre-activation; since 2026-07-18 they
# hold real test-mode values (INFRA.md). This stays idempotent: it only creates missing secrets and
# (re)grants accessor — it never overwrites existing versions. Whitespace reads as "unset", so a
# fresh environment cleanly returns 503 from checkout/portal until real keys land.
ensure_stripe_secrets() {
  local project_number compute_sa secret
  project_number="$(gcloud projects describe "$PROJECT" --format='value(projectNumber)')"
  compute_sa="$project_number-compute@developer.gserviceaccount.com"

  for secret in mdreel-stripe-secret-key mdreel-stripe-webhook-secret; do
    if ! gcloud secrets describe "$secret" --project "$PROJECT" >/dev/null 2>&1; then
      echo "Creating empty secret $secret in $DATA_REGION..."
      gcloud secrets create "$secret" \
        --replication-policy=user-managed \
        --locations="$DATA_REGION" \
        --project "$PROJECT" --quiet
      printf '\n' | gcloud secrets versions add "$secret" --data-file=- --project "$PROJECT" --quiet >/dev/null
    fi
    gcloud secrets add-iam-policy-binding "$secret" \
      --member="serviceAccount:$compute_sa" \
      --role="roles/secretmanager.secretAccessor" \
      --project "$PROJECT" --quiet >/dev/null
  done
}

# Cloud Tasks is the live webhook-delivery queue (M5 flip, founder-approved 2026-07-17 — see
# INFRA.md). Idempotent: enables the API, creates the EU queue, and grants the api runtime SA
# (default compute SA) the roles it needs to (a) enqueue tasks and (b) have Cloud Tasks mint an
# OIDC push token as itself, which the /internal endpoint validates. Prints the SA on stdout;
# all progress goes to stderr so the caller can capture only the email.
ensure_cloud_tasks() {
  local project_number compute_sa tasks_agent
  project_number="$(gcloud projects describe "$PROJECT" --format='value(projectNumber)')"
  compute_sa="$project_number-compute@developer.gserviceaccount.com"

  echo "Ensuring Cloud Tasks API + queue $CLOUDTASKS_QUEUE in $RUN_REGION..." >&2
  gcloud services enable cloudtasks.googleapis.com --project "$PROJECT" --quiet >&2

  # Provision (and get) the Cloud Tasks service agent so the token-creator grant below has a
  # principal to bind; falls back to the well-known name if the identity call is unavailable.
  tasks_agent="$(gcloud beta services identity create --service=cloudtasks.googleapis.com \
    --project "$PROJECT" --format='value(email)' 2>/dev/null || true)"
  if [[ -z "$tasks_agent" ]]; then
    tasks_agent="service-$project_number@gcp-sa-cloudtasks.iam.gserviceaccount.com"
  fi

  if ! gcloud tasks queues describe "$CLOUDTASKS_QUEUE" \
      --location "$RUN_REGION" --project "$PROJECT" >/dev/null 2>&1; then
    echo "Creating Cloud Tasks queue $CLOUDTASKS_QUEUE..." >&2
    gcloud tasks queues create "$CLOUDTASKS_QUEUE" \
      --location "$RUN_REGION" --project "$PROJECT" --quiet >&2
  fi

  gcloud tasks queues add-iam-policy-binding "$CLOUDTASKS_QUEUE" \
    --location "$RUN_REGION" --project "$PROJECT" \
    --member="serviceAccount:$compute_sa" \
    --role="roles/cloudtasks.enqueuer" --quiet >&2
  gcloud iam service-accounts add-iam-policy-binding "$compute_sa" \
    --member="serviceAccount:$compute_sa" \
    --role="roles/iam.serviceAccountUser" \
    --project "$PROJECT" --quiet >&2
  gcloud iam service-accounts add-iam-policy-binding "$compute_sa" \
    --member="serviceAccount:$tasks_agent" \
    --role="roles/iam.serviceAccountTokenCreator" \
    --project "$PROJECT" --quiet >&2

  echo "$compute_sa"
}

ensure_signed_uploads() {
  local compute_sa="$1"
  local cors_file lifecycle_file

  echo "Ensuring signed-upload IAM + raw bucket CORS..." >&2
  gcloud services enable iamcredentials.googleapis.com --project "$PROJECT" --quiet >&2
  gcloud iam service-accounts add-iam-policy-binding "$compute_sa" \
    --member="serviceAccount:$compute_sa" \
    --role="roles/iam.serviceAccountTokenCreator" \
    --project "$PROJECT" --quiet >&2

  mkdir -p .local-state
  cors_file=".local-state/raw-videos-cors.json"
  cat > "$cors_file" <<'JSON'
[
  {
    "origin": ["https://mdreel.com", "http://localhost:3000"],
    "method": ["PUT", "GET"],
    "responseHeader": ["Content-Type"],
    "maxAgeSeconds": 3600
  }
]
JSON
  gcloud storage buckets update "gs://raw-videos-eu" \
    --cors-file="$cors_file" \
    --project "$PROJECT" --quiet >&2
  rm -f "$cors_file"

  # Abandoned-upload backstop (ARCHITECTURE §3/§7): direct-to-GCS objects live under uploads/
  # until a job adopts them; if no job is ever created, nothing else deletes them. Scoped to the
  # uploads/ prefix so job-staged private/<job>/ objects (and any future retention feature) are
  # untouched. Signed URLs expire after 2h, so age 1d never races a legitimate upload.
  lifecycle_file=".local-state/raw-videos-lifecycle.json"
  cat > "$lifecycle_file" <<'JSON'
{
  "rule": [
    {
      "action": {"type": "Delete"},
      "condition": {"age": 1, "matchesPrefix": ["uploads/"]}
    }
  ]
}
JSON
  gcloud storage buckets update "gs://raw-videos-eu" \
    --lifecycle-file="$lifecycle_file" \
    --project "$PROJECT" --quiet >&2
  rm -f "$lifecycle_file"
}

deploy_web() {
  echo "Deploying vectorreel-web to $RUN_REGION..."
  # Same-origin auth proxy target (web/middleware.ts): the api service URL, so the Identity cookie
  # is first-party. Stable across api revisions; empty if the api is not deployed yet (web then
  # falls back to its mock /api routes).
  local api_url
  api_url="$(gcloud run services describe vectorreel-api \
    --region "$RUN_REGION" --project "$PROJECT" \
    --format='value(status.url)' 2>/dev/null || true)"

  local env_args=()
  if [[ -n "$api_url" ]]; then
    env_args=(--set-env-vars "API_ORIGIN=$api_url")
    echo "  API_ORIGIN=$api_url"
  fi

  gcloud run deploy vectorreel-web \
    --source web \
    --region "$RUN_REGION" \
    --project "$PROJECT" \
    --allow-unauthenticated \
    --min-instances=0 \
    --max-instances=3 \
    "${env_args[@]}" \
    --quiet
  describe_deploy "vectorreel-web"
}

deploy_api() {
  local conn_name compute_sa api_url env_vars

  conn_name="$(require_api_infra)"
  ensure_stripe_secrets
  compute_sa="$(ensure_cloud_tasks)"
  ensure_signed_uploads "$compute_sa"

  # Cloud Tasks pushes back to the api's own URL; resolve it before deploy. Present on every
  # redeploy (the service already exists); if somehow empty (first-ever create) we deploy with
  # InProcessQueue and a follow-up 'deploy api' flips it — the queue binding is config-gated.
  api_url="$(gcloud run services describe vectorreel-api \
    --region "$RUN_REGION" --project "$PROJECT" \
    --format='value(status.url)' 2>/dev/null || true)"

  # Payments went live (test mode) 2026-07-18 — never reset PAYMENTS_MODE here. Price IDs and the
  # checkout redirect base are part of the deploy config (public, non-secret; INFRA.md).
  env_vars='PipelineModel__Mode=fake,APP_BASE_URL=https://mdreel.com,Storage__Mode=gcs,Gcs__RawBucket=raw-videos-eu,Gcs__OutputBucket=outputs-eu'
  env_vars="$env_vars,STRIPE_PRICE_PRO=price_1TueKDCibBXSEilRzfAaVoID"
  env_vars="$env_vars,STRIPE_PRICE_BUSINESS=price_1TueKYCibBXSEilRzQNH9IiB"
  env_vars="$env_vars,STRIPE_PRICE_STARTER=price_1TueKoCibBXSEilR6G3s5OOe"
  if [[ -n "$api_url" ]]; then
    env_vars="$env_vars,CloudTasks__ProjectId=$PROJECT,CloudTasks__Location=$RUN_REGION,CloudTasks__QueueName=$CLOUDTASKS_QUEUE,CloudTasks__TargetBaseUrl=$api_url,CloudTasks__ServiceAccountEmail=$compute_sa"
    echo "  Cloud Tasks push target $api_url (OIDC SA $compute_sa)"
  else
    echo "  vectorreel-api URL not resolvable yet — deploying with InProcessQueue; re-run 'deploy api' to flip to Cloud Tasks."
  fi

  echo "Building vectorreel-api:$TAG..."
  docker build -f src/Api/Dockerfile -t "$AR/vectorreel-api:$TAG" .
  docker push "$AR/vectorreel-api:$TAG"

  echo "Deploying vectorreel-api to $RUN_REGION..."
  # tmpfs counts against RAM; Stage A holds one full video on /tmp, so 512Mi OOMs on real recordings.
  gcloud run deploy vectorreel-api \
    --image "$AR/vectorreel-api:$TAG" \
    --region "$RUN_REGION" \
    --project "$PROJECT" \
    --allow-unauthenticated \
    --min-instances=0 \
    --max-instances=2 \
    --memory=2Gi \
    --set-env-vars "$env_vars" \
    --add-cloudsql-instances "$conn_name" \
    --set-secrets "POSTGRES_CONNECTION=$SECRET_NAME:latest,STRIPE_SECRET_KEY=mdreel-stripe-secret-key:latest,STRIPE_WEBHOOK_SECRET=mdreel-stripe-webhook-secret:latest" \
    --quiet
  describe_deploy "vectorreel-api"
}

deploy_worker() {
  echo "Building vectorreel-worker:$TAG..."
  docker build -f src/Worker/Dockerfile -t "$AR/vectorreel-worker:$TAG" .
  docker push "$AR/vectorreel-worker:$TAG"

  echo "Deploying vectorreel-worker to $RUN_REGION..."
  gcloud run deploy vectorreel-worker \
    --image "$AR/vectorreel-worker:$TAG" \
    --region "$RUN_REGION" \
    --project "$PROJECT" \
    --allow-unauthenticated \
    --min-instances=0 \
    --max-instances=1 \
    --set-env-vars 'PipelineModel__Mode=fake' \
    --quiet
  describe_deploy "vectorreel-worker"
}

case "$cmd" in
  web|api|worker|all)
    gcloud auth configure-docker "$RUN_REGION-docker.pkg.dev" --project "$PROJECT" --quiet
    ;;
  *)
    usage
    exit 2
    ;;
esac

case "$cmd" in
  web)
    deploy_web
    ;;
  api)
    deploy_api
    ;;
  worker)
    deploy_worker
    ;;
  all)
    deploy_web
    deploy_api
    deploy_worker
    ;;
esac
