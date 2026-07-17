#!/usr/bin/env bash
# provision-cloudsql.sh — idempotently provision the smallest EU Cloud SQL Postgres footprint.
#
#   scripts/provision-cloudsql.sh
#
# Creates/ensures the Cloud SQL instance, database, app user, Secret Manager connection string,
# and IAM needed by Cloud Run. The generated password is never printed or written to disk.

set -euo pipefail
cd "$(dirname "$0")/.."

PROJECT="${PROJECT:-tensile-runway-442915-j6}"
RUN_REGION="${RUN_REGION:-europe-west1}"
DATA_REGION="${DATA_REGION:-europe-central2}"
SQL_INSTANCE="${SQL_INSTANCE:-mdreel-db}"
SQL_DATABASE="${SQL_DATABASE:-vectorreel}"
SQL_USER="${SQL_USER:-mdreel_app}"
SECRET_NAME="${SECRET_NAME:-mdreel-postgres-connection}"

generate_password() {
  if command -v openssl >/dev/null 2>&1; then
    openssl rand -base64 48 | tr -d '/+=' | cut -c1-32
    return
  fi

  if command -v python3 >/dev/null 2>&1; then
    python3 -c 'import secrets,string; alphabet=string.ascii_letters+string.digits; print("".join(secrets.choice(alphabet) for _ in range(32)))'
    return
  fi

  python -c 'import secrets,string; alphabet=string.ascii_letters+string.digits; print("".join(secrets.choice(alphabet) for _ in range(32)))'
}

instance_state() {
  gcloud sql instances describe "$SQL_INSTANCE" \
    --project "$PROJECT" \
    --format='value(state)' 2>/dev/null || true
}

wait_for_runnable() {
  local deadline=$((SECONDS + 1200))
  local state

  while (( SECONDS < deadline )); do
    state="$(instance_state)"
    if [[ "$state" == "RUNNABLE" ]]; then
      echo "Cloud SQL instance is RUNNABLE."
      return 0
    fi

    echo "Cloud SQL instance state is ${state:-unknown}; waiting 30s..."
    sleep 30
  done

  echo "Timed out waiting for Cloud SQL instance $SQL_INSTANCE to become RUNNABLE." >&2
  return 1
}

echo "Provisioning Cloud SQL (project=$PROJECT, data=$DATA_REGION, instance=$SQL_INSTANCE)"

if gcloud sql instances describe "$SQL_INSTANCE" --project "$PROJECT" >/dev/null 2>&1; then
  echo "Cloud SQL instance exists."
else
  echo "Creating Cloud SQL instance $SQL_INSTANCE in $DATA_REGION (minimal cost profile)..."
  gcloud sql instances create "$SQL_INSTANCE" \
    --database-version=POSTGRES_16 \
    --edition=enterprise \
    --tier=db-f1-micro \
    --region="$DATA_REGION" \
    --storage-type=HDD \
    --storage-size=10 \
    --availability-type=zonal \
    --project "$PROJECT" \
    --quiet
fi

wait_for_runnable

if gcloud sql databases describe "$SQL_DATABASE" \
    --instance="$SQL_INSTANCE" \
    --project "$PROJECT" >/dev/null 2>&1; then
  echo "Database $SQL_DATABASE exists."
else
  echo "Creating database $SQL_DATABASE..."
  gcloud sql databases create "$SQL_DATABASE" \
    --instance="$SQL_INSTANCE" \
    --project "$PROJECT" \
    --quiet
fi

if gcloud secrets describe "$SECRET_NAME" --project "$PROJECT" >/dev/null 2>&1; then
  echo "Secret $SECRET_NAME exists; assuming SQL user/password already provisioned."
else
  echo "Creating/updating SQL user and storing connection string in Secret Manager..."
  password="$(generate_password)"

  if gcloud sql users list \
      --instance="$SQL_INSTANCE" \
      --project "$PROJECT" \
      --format='value(name)' | grep -Fxq "$SQL_USER"; then
    gcloud sql users set-password "$SQL_USER" \
      --instance="$SQL_INSTANCE" \
      --password="$password" \
      --project "$PROJECT" \
      --quiet
  else
    gcloud sql users create "$SQL_USER" \
      --instance="$SQL_INSTANCE" \
      --password="$password" \
      --project "$PROJECT" \
      --quiet
  fi

  conn="Host=/cloudsql/$PROJECT:$DATA_REGION:$SQL_INSTANCE;Database=$SQL_DATABASE;Username=$SQL_USER;Password=$password"
  gcloud secrets create "$SECRET_NAME" \
    --replication-policy=user-managed \
    --locations="$DATA_REGION" \
    --project "$PROJECT" \
    --quiet >/dev/null
  printf '%s' "$conn" | gcloud secrets versions add "$SECRET_NAME" \
    --data-file=- \
    --project "$PROJECT" >/dev/null

  unset password conn
  echo "Secret $SECRET_NAME created."
fi

project_number="$(gcloud projects describe "$PROJECT" --project "$PROJECT" --format='value(projectNumber)')"
compute_sa="$project_number-compute@developer.gserviceaccount.com"

echo "Granting Cloud SQL Client to $compute_sa..."
gcloud projects add-iam-policy-binding "$PROJECT" \
  --member="serviceAccount:$compute_sa" \
  --role="roles/cloudsql.client" \
  --condition=None \
  --project "$PROJECT" \
  --quiet >/dev/null

echo "Granting Secret Accessor on $SECRET_NAME to $compute_sa..."
gcloud secrets add-iam-policy-binding "$SECRET_NAME" \
  --member="serviceAccount:$compute_sa" \
  --role="roles/secretmanager.secretAccessor" \
  --project "$PROJECT" \
  --quiet >/dev/null

connection_name="$(gcloud sql instances describe "$SQL_INSTANCE" --project "$PROJECT" --format='value(connectionName)')"
echo "Cloud SQL connection name: $connection_name"
echo "Cost warning: this Cloud SQL instance is a recurring bill; stop it with scripts/teardown-cloudsql.sh."
