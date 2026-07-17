#!/usr/bin/env bash
# e2e.sh — the whole-solution verify loop (TESTING.md is the authority for tiers & runbook).
#
#   scripts/e2e.sh up          bring up the full stack (docker compose --profile e2e, builds images)
#   scripts/e2e.sh smoke       @smoke Playwright tests only (~30 s warm)
#   scripts/e2e.sh test        full Playwright E2E suite against the running stack
#   scripts/e2e.sh full        everything: dotnet suite (incl. Postgres-backed) + web unit + E2E
#   scripts/e2e.sh logs [svc]  follow container logs (all services or one of: api web postgres)
#   scripts/e2e.sh db "<sql>"  run SQL against the compose Postgres (events, usage_ledger, payments…)
#   scripts/e2e.sh down        stop the stack (keeps volumes; add -v yourself to wipe data)
#
# Deterministic by design: Stages B–D are stubs, payments use FakePaymentGateway — zero Vertex
# spend, zero Stripe calls. tests/Live (real Vertex) is a separate explicit tier — see TESTING.md.

set -euo pipefail
cd "$(dirname "$0")/.."

cmd="${1:-}"
shift || true

compose() { docker compose --profile e2e "$@"; }

ensure_e2e_deps() {
  if [[ ! -d tests/E2E/node_modules ]]; then
    (cd tests/E2E && npm install && npx playwright install chromium)
  fi
}

case "$cmd" in
  up)
    compose up -d --build
    echo "stack up — web http://localhost:3000, api http://localhost:8080, traces http://localhost:19888"
    ;;
  smoke)
    ensure_e2e_deps
    (cd tests/E2E && npx playwright test --grep @smoke)
    ;;
  test)
    ensure_e2e_deps
    (cd tests/E2E && npx playwright test "$@")
    ;;
  full)
    dotnet test --filter Category!=Live
    (cd web && npm test)
    ensure_e2e_deps
    (cd tests/E2E && npx playwright test)
    ;;
  logs)
    compose logs -f --tail 100 "$@"
    ;;
  db)
    docker compose exec postgres psql -U dev -d vectorreel -c "$*"
    ;;
  down)
    compose down
    ;;
  *)
    sed -n '2,15p' "$0"
    exit 2
    ;;
esac
