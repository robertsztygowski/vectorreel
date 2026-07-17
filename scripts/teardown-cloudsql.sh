#!/usr/bin/env bash
# teardown-cloudsql.sh — delete the billable Cloud SQL instance and stored connection secret.
#
#   scripts/teardown-cloudsql.sh
#
# Cloud Run services running fake mode with min-instances=0 cost approximately nothing and are left
# alone. This only removes Cloud SQL and the Secret Manager secret; missing resources are tolerated.

set -euo pipefail
cd "$(dirname "$0")/.."

PROJECT="${PROJECT:-tensile-runway-442915-j6}"
RUN_REGION="${RUN_REGION:-europe-west1}"
DATA_REGION="${DATA_REGION:-europe-central2}"
SQL_INSTANCE="${SQL_INSTANCE:-mdreel-db}"
SQL_DATABASE="${SQL_DATABASE:-vectorreel}"
SQL_USER="${SQL_USER:-mdreel_app}"
SECRET_NAME="${SECRET_NAME:-mdreel-postgres-connection}"

deleted=0

if gcloud sql instances describe "$SQL_INSTANCE" --project "$PROJECT" >/dev/null 2>&1; then
  echo "Deleting Cloud SQL instance $SQL_INSTANCE..."
  gcloud sql instances delete "$SQL_INSTANCE" \
    --project "$PROJECT" \
    --quiet
  echo "Deleted Cloud SQL instance $SQL_INSTANCE."
  deleted=$((deleted + 1))
else
  echo "Cloud SQL instance $SQL_INSTANCE not found; skipped."
fi

if gcloud secrets describe "$SECRET_NAME" --project "$PROJECT" >/dev/null 2>&1; then
  echo "Deleting Secret Manager secret $SECRET_NAME..."
  gcloud secrets delete "$SECRET_NAME" \
    --project "$PROJECT" \
    --quiet
  echo "Deleted Secret Manager secret $SECRET_NAME."
  deleted=$((deleted + 1))
else
  echo "Secret Manager secret $SECRET_NAME not found; skipped."
fi

echo "Teardown complete ($deleted resource(s) deleted)."
