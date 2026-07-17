#!/usr/bin/env bash
# preflight.sh — fail-fast checks before provisioning/deploying mdreel Cloud Run + Cloud SQL.
#
#   scripts/preflight.sh
#
# Verifies local tooling, GCP authentication, required APIs, Docker, and EU buckets. Missing
# buckets are created in DATA_REGION. Secret Manager is enabled if absent; other missing APIs fail.

set -euo pipefail
cd "$(dirname "$0")/.."

PROJECT="${PROJECT:-tensile-runway-442915-j6}"
RUN_REGION="${RUN_REGION:-europe-west1}"
DATA_REGION="${DATA_REGION:-europe-central2}"
SQL_INSTANCE="${SQL_INSTANCE:-mdreel-db}"
SQL_DATABASE="${SQL_DATABASE:-vectorreel}"
SQL_USER="${SQL_USER:-mdreel_app}"
SECRET_NAME="${SECRET_NAME:-mdreel-postgres-connection}"

failures=0
rows=()

record() {
  local status="$1"
  local name="$2"
  local details="${3:-}"
  rows+=("$status|$name|$details")
  printf '%-48s %-4s %s\n' "$name" "$status" "$details"
  if [[ "$status" == "FAIL" ]]; then
    failures=$((failures + 1))
  fi
}

pass() { record "PASS" "$1" "${2:-}"; }
fail() { record "FAIL" "$1" "${2:-}"; }

upper_region() {
  printf '%s' "$1" | tr '[:lower:]' '[:upper:]'
}

check_api() {
  local api="$1"
  local allow_enable="${2:-0}"
  local enabled

  enabled="$(gcloud services list --enabled \
    --project "$PROJECT" \
    --filter="config.name:$api" \
    --format='value(config.name)' 2>/dev/null || true)"
  if grep -Fxq "$api" <<<"$enabled"; then
    pass "API $api" "enabled"
    return
  fi

  if [[ "$allow_enable" == "1" ]]; then
    if gcloud services enable "$api" --project "$PROJECT" --quiet >/dev/null 2>&1; then
      enabled="$(gcloud services list --enabled \
        --project "$PROJECT" \
        --filter="config.name:$api" \
        --format='value(config.name)' 2>/dev/null || true)"
      if grep -Fxq "$api" <<<"$enabled"; then
        pass "API $api" "enabled now"
      else
        fail "API $api" "enable returned but API is not listed enabled"
      fi
    else
      fail "API $api" "missing and enable failed"
    fi
    return
  fi

  fail "API $api" "missing or disabled"
}

ensure_bucket() {
  local bucket="$1"
  local expected
  local location

  expected="$(upper_region "$DATA_REGION")"
  if location="$(gcloud storage buckets describe "gs://$bucket" --project "$PROJECT" --format='value(location)' 2>/dev/null)"; then
    if [[ "$location" == "$expected" ]]; then
      pass "Bucket gs://$bucket" "location $location"
    else
      fail "Bucket gs://$bucket" "wrong location $location (expected $expected)"
    fi
    return
  fi

  if gcloud storage buckets create "gs://$bucket" \
      --location="$DATA_REGION" \
      --project "$PROJECT" \
      --uniform-bucket-level-access \
      --quiet >/dev/null 2>&1; then
    pass "Bucket gs://$bucket" "created in $expected"
  else
    fail "Bucket gs://$bucket" "missing and create failed"
  fi
}

echo "mdreel preflight (project=$PROJECT, run=$RUN_REGION, data=$DATA_REGION)"
printf '%-48s %-4s %s\n' "Check" "Result" "Details"
printf '%-48s %-4s %s\n' "-----" "------" "-------"

if command -v gcloud >/dev/null 2>&1; then
  pass "gcloud CLI" "$(command -v gcloud)"
else
  fail "gcloud CLI" "not found"
fi

active_account="$(gcloud auth list --project "$PROJECT" --filter=status:ACTIVE --format='value(account)' 2>/dev/null || true)"
if [[ -n "$active_account" ]]; then
  pass "gcloud active account" "$active_account"
else
  fail "gcloud active account" "none"
fi

if gcloud auth application-default print-access-token --project "$PROJECT" >/dev/null 2>&1; then
  pass "Application Default Credentials" "token acquired"
else
  fail "Application Default Credentials" "not available"
fi

if gcloud projects describe "$PROJECT" --project "$PROJECT" >/dev/null 2>&1; then
  pass "Project reachable" "$PROJECT"
else
  fail "Project reachable" "$PROJECT not reachable"
fi

check_api "run.googleapis.com"
check_api "sqladmin.googleapis.com"
check_api "storage.googleapis.com"
check_api "artifactregistry.googleapis.com"
check_api "secretmanager.googleapis.com" "1"
check_api "cloudbuild.googleapis.com"

if command -v docker >/dev/null 2>&1; then
  pass "docker CLI" "$(command -v docker)"
else
  fail "docker CLI" "not found"
fi

if docker info >/dev/null 2>&1; then
  pass "docker daemon" "responding"
else
  fail "docker daemon" "not responding"
fi

ensure_bucket "raw-videos-eu"
ensure_bucket "outputs-eu"

printf '\nSummary\n'
printf '%-48s %-4s %s\n' "Check" "Result" "Details"
printf '%-48s %-4s %s\n' "-----" "------" "-------"
for row in "${rows[@]}"; do
  IFS='|' read -r status name details <<<"$row"
  printf '%-48s %-4s %s\n' "$name" "$status" "$details"
done

if [[ "$failures" -ne 0 ]]; then
  printf '\n✗ preflight failed (%s failure(s))\n' "$failures"
  exit 1
fi

printf '\n✓ preflight passed (%s checks)\n' "${#rows[@]}"
