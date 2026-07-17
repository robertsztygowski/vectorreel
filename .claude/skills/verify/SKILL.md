---
name: verify
description: >
  Verify the whole mdreel solution end-to-end. Use when asked to "verify the solution",
  "run the tests", "check everything works", before any commit to main (definition of done,
  CLAUDE.md rule 4), or after changes to src/, web/, or docker-compose.yml.
---

# Whole-solution verify loop

**TESTING.md is the authority** — tiers, measured runtimes, the full agent runbook. Read it if
anything below surprises you. Everything is deterministic: zero Vertex spend, no Stripe, no
network beyond localhost.

## The loop

```bash
scripts/e2e.sh up      # full stack in docker (api, web, postgres, fake-gcs, aspire) — ~10 s warm
scripts/e2e.sh smoke   # fastest signal (~17 s): health + full API job funnel with frozen contracts
scripts/e2e.sh full    # definition of done: dotnet suite + web unit + Playwright E2E (~3.5 min)
```

Targeted runs: `scripts/e2e.sh test specs/funnel.spec.ts`,
`dotnet test --filter Category!=Live`, `cd web && npm test`.

## When a test fails

1. Playwright: read `tests/E2E/test-results/<test>/error-context.md` first — it contains the
   error plus a page snapshot. Screenshots/video/trace.zip sit next to it.
2. Logs: `scripts/e2e.sh logs api` (stage transitions log the jobId).
3. Traces: http://localhost:19888 → filter spans by `mdreel.job_id`.
4. Database: `scripts/e2e.sh db "select ... from events / usage_ledger / payments / tenants"`.

## Gotchas

- Postgres-backed .NET tests (`Category=RequiresDocker`) need the compose stack up.
- A leftover dev server on port 3000/8080 shadows the containers — kill it.
- `dotnet test tests/Live` does not exist yet (Phase 3) — never fake it.
- After changing src/Api or web/, rebuild: `scripts/e2e.sh up` (it passes `--build`).
