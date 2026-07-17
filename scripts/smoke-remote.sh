#!/usr/bin/env bash
# smoke-remote.sh — remote Cloud Run smoke checks and EU infrastructure assertions.
#
#   scripts/smoke-remote.sh
#
# Checks Cloud Run URLs, API health, a Postgres-backed event insert, Cloud SQL tables, service
# readiness, regions, and bucket locations. Never prints the database password.
#
# The table check connects through the Cloud SQL Auth Proxy (docker, IAM-authenticated via local
# ADC on outbound port 3307) — direct 5432 is typically blocked by office/ISP firewalls and would
# need mutating the instance's authorized networks.

set -euo pipefail
cd "$(dirname "$0")/.."

PROJECT="${PROJECT:-tensile-runway-442915-j6}"
RUN_REGION="${RUN_REGION:-europe-west1}"
DATA_REGION="${DATA_REGION:-europe-central2}"
SQL_INSTANCE="${SQL_INSTANCE:-mdreel-db}"
SQL_DATABASE="${SQL_DATABASE:-vectorreel}"
SQL_USER="${SQL_USER:-mdreel_app}"
SECRET_NAME="${SECRET_NAME:-mdreel-postgres-connection}"

passed=0
failed=0
proxy_container="mdreel-smoke-sql-proxy"
proxy_network="mdreel-smoke"

pass() {
  local name="$1"
  local details="${2:-}"
  passed=$((passed + 1))
  printf '✓ %-52s %s\n' "$name" "$details"
}

fail() {
  local name="$1"
  local details="${2:-}"
  failed=$((failed + 1))
  printf '✗ %-52s %s\n' "$name" "$details"
}

cleanup_sql_proxy() {
  docker rm -f "$proxy_container" >/dev/null 2>&1 || true
  docker network rm "$proxy_network" >/dev/null 2>&1 || true
}
trap cleanup_sql_proxy EXIT

http_status() {
  local url="$1"
  curl -s -o /dev/null -w '%{http_code}' "$url" 2>/dev/null || true
}

service_url() {
  local service="$1"
  gcloud run services describe "$service" \
    --region "$RUN_REGION" \
    --project "$PROJECT" \
    --format='value(status.url)' 2>/dev/null || true
}

assert_http_200() {
  local name="$1"
  local url="$2"
  local code

  code="$(http_status "$url")"
  if [[ "$code" == "200" ]]; then
    pass "$name" "$url"
  else
    fail "$name" "HTTP ${code:-000} at $url"
  fi
}

assert_auth_wired() {
  local api_url="$1"
  local code

  # Non-mutating: a bad-credential login proves the Identity API is mounted and rejecting, without
  # creating a throwaway tenant. MapIdentityApi answers 401 (some builds 400) for unknown users.
  code="$(curl -s -o /dev/null -w '%{http_code}' \
    -X POST "$api_url/api/v1/auth/login?useCookies=true" \
    -H 'Content-Type: application/json' \
    -d '{"email":"smoke-nobody@example.invalid","password":"wrong-Password-1!"}' 2>/dev/null || true)"

  if [[ "$code" == "401" || "$code" == "400" ]]; then
    pass "Auth API mounted (login rejects bad creds)" "HTTP $code"
  else
    fail "Auth API mounted (login rejects bad creds)" "HTTP ${code:-000} (expected 400/401)"
  fi
}

assert_db_roundtrip() {
  local api_url="$1"
  local session_id="smoke-$(date +%s)"
  local response
  local code
  local body

  response="$(curl -sS -w $'\n%{http_code}' \
    -X POST "$api_url/api/v1/events" \
    -H 'Content-Type: application/json' \
    -d "{\"name\":\"page_view\",\"session_id\":\"$session_id\"}" 2>/dev/null || printf '\n000')"
  code="${response##*$'\n'}"
  body="${response%$'\n'*}"

  if [[ "$code" =~ ^2 ]] && grep -q '"eventId"' <<<"$body"; then
    pass "API DB round-trip" "POST /api/v1/events returned $code"
  else
    fail "API DB round-trip" "POST /api/v1/events returned ${code:-000}"
  fi
}

assert_cloudsql_tables() {
  local conn
  local pw
  local token
  local tables=""
  local required
  local attempt

  conn="$(gcloud secrets versions access latest \
    --secret="$SECRET_NAME" \
    --project "$PROJECT" 2>/dev/null)" || return 1
  pw="$(printf '%s' "$conn" | tr ';' '\n' | sed -n 's/^Password=//p')"
  [[ -n "$pw" ]] || return 1

  # Short-lived access token for the Cloud SQL Auth Proxy (IAM-authenticated; outbound 3307).
  token="$(gcloud auth print-access-token 2>/dev/null)" || return 1

  docker network create "$proxy_network" >/dev/null 2>&1 || true
  docker rm -f "$proxy_container" >/dev/null 2>&1 || true
  docker run -d --name "$proxy_container" --network "$proxy_network" \
    gcr.io/cloud-sql-connectors/cloud-sql-proxy:latest \
    --address 0.0.0.0 --port 5432 --token "$token" \
    "$PROJECT:$DATA_REGION:$SQL_INSTANCE" >/dev/null || return 1

  for attempt in 1 2 3 4 5 6; do
    tables="$(docker run --rm --network "$proxy_network" \
      -e PGPASSWORD="$pw" \
      postgres:16-alpine \
      psql "host=$proxy_container dbname=$SQL_DATABASE user=$SQL_USER" \
        -tAc "select table_name from information_schema.tables where table_schema='public' order by 1" 2>/dev/null)" && break
    tables=""
    sleep 5
  done
  [[ -n "$tables" ]] || return 1
  tables="${tables//$'\r'/}"

  for required in events tenants payments usage_ledger; do
    if ! grep -Fxq "$required" <<<"$tables"; then
      return 1
    fi
  done
}
assert_run_service_ready() {
  local service="$1"
  local ready

  ready="$(gcloud run services describe "$service" \
    --region "$RUN_REGION" \
    --project "$PROJECT" \
    --format='value(status.conditions[0].status)' 2>/dev/null || true)"

  if [[ "$ready" == "True" && "$RUN_REGION" == europe-* ]]; then
    pass "Cloud Run $service Ready in EU" "$RUN_REGION"
  else
    fail "Cloud Run $service Ready in EU" "ready=${ready:-unknown}, region=$RUN_REGION"
  fi
}

assert_cloudsql() {
  local state
  local region

  state="$(gcloud sql instances describe "$SQL_INSTANCE" \
    --project "$PROJECT" \
    --format='value(state)' 2>/dev/null || true)"
  region="$(gcloud sql instances describe "$SQL_INSTANCE" \
    --project "$PROJECT" \
    --format='value(region)' 2>/dev/null || true)"

  if [[ "$state" == "RUNNABLE" && "$region" == "$DATA_REGION" ]]; then
    pass "Cloud SQL RUNNABLE in data region" "$region"
  else
    fail "Cloud SQL RUNNABLE in data region" "state=${state:-unknown}, region=${region:-unknown}"
  fi
}

assert_bucket() {
  local bucket="$1"
  local expected
  local location

  expected="$(printf '%s' "$DATA_REGION" | tr '[:lower:]' '[:upper:]')"
  location="$(gcloud storage buckets describe "gs://$bucket" \
    --project "$PROJECT" \
    --format='value(location)' 2>/dev/null || true)"

  if [[ "$location" == "$expected" ]]; then
    pass "Bucket gs://$bucket in data region" "$location"
  else
    fail "Bucket gs://$bucket in data region" "location=${location:-missing}, expected=$expected"
  fi
}

echo "Remote smoke (project=$PROJECT, run=$RUN_REGION, data=$DATA_REGION)"

api_url="$(service_url "vectorreel-api")"
web_url="$(service_url "vectorreel-web")"

if [[ -n "$api_url" ]]; then
  pass "Resolve vectorreel-api URL" "$api_url"
else
  fail "Resolve vectorreel-api URL" "not found in $RUN_REGION"
fi

if [[ -n "$web_url" ]]; then
  pass "Resolve vectorreel-web URL" "$web_url"
else
  fail "Resolve vectorreel-web URL" "not found in $RUN_REGION"
fi

if [[ -n "$api_url" ]]; then
  assert_http_200 "API /health HTTP 200" "$api_url/health"
  assert_db_roundtrip "$api_url"
  assert_auth_wired "$api_url"
else
  fail "API /health HTTP 200" "API URL unavailable"
  fail "API DB round-trip" "API URL unavailable"
  fail "Auth API mounted (login rejects bad creds)" "API URL unavailable"
fi

if [[ -n "$web_url" ]]; then
  assert_http_200 "Web run.app HTTP 200" "$web_url"
else
  fail "Web run.app HTTP 200" "web URL unavailable"
fi
assert_http_200 "Web mdreel.com HTTP 200" "https://mdreel.com"

# Umami analytics (M3, rule 10) — self-hosted, EU-only, min-instances=0 (may cold-start).
umami_url="$(service_url "mdreel-umami")"
if [[ -n "$umami_url" ]]; then
  pass "Resolve mdreel-umami URL" "$umami_url"
  assert_http_200 "Umami analytics HTTP 200" "$umami_url"
else
  fail "Resolve mdreel-umami URL" "not found in $RUN_REGION"
  fail "Umami analytics HTTP 200" "umami URL unavailable"
fi

if assert_cloudsql_tables; then
  pass "Cloud SQL required tables exist" "events, tenants, payments, usage_ledger"
else
  fail "Cloud SQL required tables exist" "missing table(s) or connection failed"
fi
cleanup_sql_proxy

for service in vectorreel-web vectorreel-api vectorreel-worker mdreel-umami; do
  assert_run_service_ready "$service"
done

assert_cloudsql
assert_bucket "raw-videos-eu"
assert_bucket "outputs-eu"

printf '\nSummary: %s passed / %s failed\n' "$passed" "$failed"
if [[ "$failed" -ne 0 ]]; then
  exit 1
fi
