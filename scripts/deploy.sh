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

# Stripe secrets are created EMPTY (a whitespace placeholder) so the api can wire them at deploy
# time without holding real keys. Whitespace reads as "unset" (IsNullOrWhiteSpace) so — combined
# with PAYMENTS_MODE=disabled — checkout/portal cleanly return 503 until the founder adds a real
# test-mode key version (see PLAN.md NEEDS-FOUNDER) and redeploys.
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
  local conn_name

  conn_name="$(require_api_infra)"
  ensure_stripe_secrets
  echo "Building vectorreel-api:$TAG..."
  docker build -f src/Api/Dockerfile -t "$AR/vectorreel-api:$TAG" .
  docker push "$AR/vectorreel-api:$TAG"

  echo "Deploying vectorreel-api to $RUN_REGION..."
  gcloud run deploy vectorreel-api \
    --image "$AR/vectorreel-api:$TAG" \
    --region "$RUN_REGION" \
    --project "$PROJECT" \
    --allow-unauthenticated \
    --min-instances=0 \
    --max-instances=2 \
    --set-env-vars 'PipelineModel__Mode=fake,PAYMENTS_MODE=disabled' \
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
