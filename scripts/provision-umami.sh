#!/usr/bin/env bash
# provision-umami.sh — idempotently provision Umami's database on the SHARED mdreel Cloud SQL
# instance (CLAUDE.md rule 10: analytics is self-hosted EU only; never a second DB instance).
#
#   scripts/provision-umami.sh
#
# Creates a least-privilege `umami_app` role, its own `umami` database (which it owns), and stores
# the Prisma DATABASE_URL + APP_SECRET in Secret Manager. All DDL runs through the Cloud SQL Auth
# Proxy (docker, IAM-authenticated on outbound 3307) because direct 5432 is firewalled. Passwords
# are never printed or written to disk.
#
# Least privilege: `umami_app` owns only the `umami` database and its public schema, and is granted
# no privileges on any product object, so it cannot read product data. (It is not blocked from
# merely opening a connection to `vectorreel` — that would mean revoking PUBLIC CONNECT on the live
# product database, too risky to automate unattended for negligible gain; noted for later hardening.)

set -euo pipefail
cd "$(dirname "$0")/.."

PROJECT="${PROJECT:-tensile-runway-442915-j6}"
DATA_REGION="${DATA_REGION:-europe-central2}"
SQL_INSTANCE="${SQL_INSTANCE:-mdreel-db}"
PRODUCT_DB="${SQL_DATABASE:-vectorreel}"
PRODUCT_USER="${SQL_USER:-mdreel_app}"
UMAMI_DB="${UMAMI_DB:-umami}"
UMAMI_USER="${UMAMI_USER:-umami_app}"
DB_URL_SECRET="${DB_URL_SECRET:-mdreel-umami-database-url}"
APP_SECRET_SECRET="${APP_SECRET_SECRET:-mdreel-umami-app-secret}"

proxy_container="mdreel-umami-sql-proxy"
proxy_network="mdreel-umami-provision"

cleanup() {
  docker rm -f "$proxy_container" >/dev/null 2>&1 || true
  docker network rm "$proxy_network" >/dev/null 2>&1 || true
}
trap cleanup EXIT

generate_secret() {
  # URL-safe (no / + =) so it drops straight into a Prisma connection string without escaping.
  if command -v openssl >/dev/null 2>&1; then
    openssl rand -base64 48 | tr -d '/+=' | cut -c1-32
    return
  fi
  python -c 'import secrets,string; a=string.ascii_letters+string.digits; print("".join(secrets.choice(a) for _ in range(32)))'
}

psql_stdin() {
  # $1 = database to connect to; SQL on stdin, run as the Cloud SQL superuser `postgres`.
  local db="$1"
  docker run --rm -i --network "$proxy_network" \
    -e PGPASSWORD="$postgres_pw" \
    postgres:16-alpine \
    psql -v ON_ERROR_STOP=1 "host=$proxy_container dbname=$db user=postgres" >/dev/null
}

echo "Provisioning Umami DB on shared instance (project=$PROJECT, instance=$SQL_INSTANCE)"

if gcloud secrets describe "$DB_URL_SECRET" --project "$PROJECT" >/dev/null 2>&1; then
  echo "Secret $DB_URL_SECRET exists; assuming umami DB/user already provisioned. Ensuring IAM only."
else
  connection_name="$(gcloud sql instances describe "$SQL_INSTANCE" --project "$PROJECT" --format='value(connectionName)')"

  umami_pw="$(generate_secret)"
  postgres_pw="$(generate_secret)"
  app_secret="$(generate_secret)"

  # Ensure the least-privilege app role and a known password for the superuser DDL session.
  if gcloud sql users list --instance="$SQL_INSTANCE" --project "$PROJECT" --format='value(name)' | grep -Fxq "$UMAMI_USER"; then
    gcloud sql users set-password "$UMAMI_USER" --instance="$SQL_INSTANCE" --password="$umami_pw" --project "$PROJECT" --quiet
  else
    gcloud sql users create "$UMAMI_USER" --instance="$SQL_INSTANCE" --password="$umami_pw" --project "$PROJECT" --quiet
  fi
  gcloud sql users set-password postgres --instance="$SQL_INSTANCE" --password="$postgres_pw" --project "$PROJECT" --quiet

  # Cloud SQL Auth Proxy (short-lived IAM token) for the DDL that gcloud cannot express.
  token="$(gcloud auth print-access-token)"
  docker network create "$proxy_network" >/dev/null 2>&1 || true
  docker rm -f "$proxy_container" >/dev/null 2>&1 || true
  docker run -d --name "$proxy_container" --network "$proxy_network" \
    gcr.io/cloud-sql-connectors/cloud-sql-proxy:latest \
    --address 0.0.0.0 --port 5432 --token "$token" \
    "$connection_name" >/dev/null

  # Wait for the proxy to accept connections.
  for attempt in 1 2 3 4 5 6 7 8; do
    if printf 'select 1;\n' | psql_stdin postgres 2>/dev/null; then break; fi
    sleep 5
  done

  # Cluster-level: create the umami DB owned by the least-privilege role. The product database is
  # deliberately left untouched (see header note).
  printf '%s\n' \
    "GRANT \"$UMAMI_USER\" TO postgres;" \
    "SELECT 'CREATE DATABASE \"$UMAMI_DB\" OWNER \"$UMAMI_USER\"' WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname='$UMAMI_DB')\\gexec" \
    "REVOKE ALL ON DATABASE \"$UMAMI_DB\" FROM PUBLIC;" \
    "GRANT CONNECT ON DATABASE \"$UMAMI_DB\" TO \"$UMAMI_USER\";" \
    | psql_stdin postgres

  # Inside the umami DB: hand the public schema to umami_app so Prisma migrations can create tables.
  printf '%s\n' \
    "ALTER SCHEMA public OWNER TO \"$UMAMI_USER\";" \
    "GRANT ALL ON SCHEMA public TO \"$UMAMI_USER\";" \
    | psql_stdin "$UMAMI_DB"

  cleanup

  # Prisma unix-socket connection string for Cloud Run (host=/cloudsql/<connection name>).
  db_url="postgresql://$UMAMI_USER:$umami_pw@localhost/$UMAMI_DB?host=/cloudsql/$connection_name"

  gcloud secrets create "$DB_URL_SECRET" --replication-policy=user-managed --locations="$DATA_REGION" --project "$PROJECT" --quiet >/dev/null
  printf '%s' "$db_url" | gcloud secrets versions add "$DB_URL_SECRET" --data-file=- --project "$PROJECT" >/dev/null

  gcloud secrets create "$APP_SECRET_SECRET" --replication-policy=user-managed --locations="$DATA_REGION" --project "$PROJECT" --quiet >/dev/null
  printf '%s' "$app_secret" | gcloud secrets versions add "$APP_SECRET_SECRET" --data-file=- --project "$PROJECT" >/dev/null

  unset umami_pw postgres_pw app_secret db_url
  echo "Secrets $DB_URL_SECRET and $APP_SECRET_SECRET created."
fi

project_number="$(gcloud projects describe "$PROJECT" --format='value(projectNumber)')"
compute_sa="$project_number-compute@developer.gserviceaccount.com"

for secret in "$DB_URL_SECRET" "$APP_SECRET_SECRET"; do
  echo "Granting Secret Accessor on $secret to $compute_sa..."
  gcloud secrets add-iam-policy-binding "$secret" \
    --member="serviceAccount:$compute_sa" \
    --role="roles/secretmanager.secretAccessor" \
    --project "$PROJECT" --quiet >/dev/null
done

echo "Umami database provisioning complete."
echo "Cost note: no new instance — umami shares mdreel-db (CLAUDE.md rule 10); the only new fixed"
echo "cost is the Cloud Run service, which runs at min-instances=0 (scales to zero)."
