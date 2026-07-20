# TESTING.md — the whole-solution verify loop

> **Living doc.** Sole authority for *how the solution is verified*: the test tiers, the exact
> commands, and the measured runtimes. Strategy philosophy stays in DEVELOPMENT.md §4; the
> definition of done cites this file (CLAUDE.md rule 4). Keep runtimes honest — re-measure when
> the suite changes shape, don't let them rot.

The design goal: **any session (usually an AI agent) verifies the entire solution in minutes,
without re-deriving how to run anything.** Everything below is deterministic — zero Vertex spend,
zero Stripe calls (FakePaymentGateway), nothing leaves the machine.

## 1. Tiers

| Tier | Command | What it proves | Measured (2026-07-17, warm) |
|---|---|---|---|
| **smoke** | `scripts/e2e.sh up && scripts/e2e.sh smoke` | Stack boots; health + the full API job funnel (upload → real ffmpeg Stage A → output contracts → Postgres ledger → erasure) | up 10 s + smoke 17 s |
| **full** (default) | `scripts/e2e.sh full` | Everything below | ~3.5 min total |
| ├─ .NET | `dotnet test --filter Category!=Live` | 135 unit+integration tests, incl. the Postgres-backed store tests (`Category=RequiresDocker` — needs compose postgres) and the replay-harness fidelity tests | 40 s (2026-07-20 warm) |
| ├─ web unit | `cd web && npm test` | 33 tests: contracts, corpus, attribution, output document | 26 s |
| └─ E2E | `scripts/e2e.sh test` | 7 Playwright tests: browser funnel, API funnel + frozen contracts, payments + webhook + attribution | 30 s |
| **live** | `dotnet test tests/Live` | Real-Vertex Stage B+C smoke on a CC-BY YouTube `fileData.fileUri` (`Category=Live`; ADC + real spend). Run before deploys / when prompts or schemas change (CLAUDE.md rule 5). Never part of the default loop. | ~20 s |

First-ever run adds one-time costs: docker image builds (~4–6 min for api+web),
`npm install` + chromium download in `tests/E2E` (auto-run by `scripts/e2e.sh`).

## 2. The stack

`docker compose --profile e2e up -d --build` brings up everything the suites run against:

| Service | Host address | Notes |
|---|---|---|
| api | http://localhost:8080 | real ffmpeg Stage A; Stage B/C run the deterministic offline stand-in (`PipelineModel__Mode=fake`); Stage D persists to fake-gcs; Postgres stores |
| web | http://localhost:3000 | production Next.js build, `NEXT_PUBLIC_API_BASE` baked to :8080 |
| postgres | localhost:5432 (dev/dev, db `vectorreel`) | the METRICS.md §6.2 source of truth |
| fake-gcs-server | http://localhost:4443 | GCS emulation; Stage D writes `output.md`/`output.json` here in E2E (`-external-url` is the in-network hostname so resumable uploads resolve from the api container) |
| aspire-dashboard | **http://localhost:19888** | logs + traces + metrics UI; OTLP in-network on `aspire-dashboard:18889` |

Default profile (`docker compose up -d`, no `--profile`) still starts only postgres + fake-gcs
for host-side `dotnet run` development — unchanged.

## 3. Agent runbook — red test to root cause

```bash
scripts/e2e.sh up                # bring up / rebuild the stack
scripts/e2e.sh smoke             # fastest signal
scripts/e2e.sh test              # full E2E; add any playwright args, e.g. specs/funnel.spec.ts
scripts/e2e.sh full              # the whole definition-of-done suite
scripts/e2e.sh logs api          # follow one service's logs (api | web | postgres)
scripts/e2e.sh db "select name, session_id, tenant_id from events order by created_at desc limit 20"
scripts/e2e.sh down              # stop (volumes kept)
```

**Failure artifacts (Playwright):** on failure, `tests/E2E/test-results/<test>/` contains
`error-context.md` (the failure + page snapshot — read this first), `test-failed-1.png`, `video.webm`,
and `trace.zip` (`npx playwright show-trace <path>` for the full step-by-step timeline).

**Find a trace by jobId:** open http://localhost:19888 → Traces → filter. Every pipeline run is a
`pipeline.job` root span with `mdreel.job_id`, `mdreel.upload_id`, `mdreel.cost_cents` attributes
and one child span per stage. Structured logs from the api land in
the same UI (and in `scripts/e2e.sh logs api` — stage transitions log the jobId).
Metrics from the API and worker land in the same local Aspire dashboard: job/stage duration,
processed video minutes, Stage B runaway guard activations (METRICS.md N7), LLM tokens, runtime
instrumentation, and webhook delivery failures. The cost ledger remains the source of truth for
per-job product metering (METRICS.md N9).
Local OTel is env-gated: no `OTEL_EXPORTER_OTLP_ENDPOINT` ⇒ no local exporter; production export is
separately gated by the Cloud Run switch documented in INFRA.md.

**Queryable checks** (what the E2E tests assert, runnable by hand):

```bash
scripts/e2e.sh db "select step, quantity, unit, cents from usage_ledger where job_id = '<jobId>'"
scripts/e2e.sh db "select plan, first_utm_source, amount_cents from payments order by created_at desc limit 5"
scripts/e2e.sh db "select email, plan, trial_credit_hours, archive_hours from tenants order by created_at desc limit 5"
```

## 4. What the E2E suite covers (tests/E2E)

- `api-funnel.spec.ts` — the frozen ARCHITECTURE §5 contract funnel against the real container:
  upload → PUT the committed CC clip → job → poll (every body validated against
  `tests/fixtures/contracts/*.schema.json`) → output.md/.json → `usage_ledger` rows → DELETE
  erasure → problem+json; plus simulated failure and 401 paths.
- `funnel.spec.ts` — real browser: landing (UTM) → signup with the N20 volume fields → tenant +
  first-touch attribution in Postgres → upload → progress → done badge → download .md → the five
  funnel events as Postgres rows keyed by the browser's session_id.
- `payments.spec.ts` — signup → checkout (fake gateway) → Stripe webhook (test signature; forged
  signature rejected) → payment row with first-touch attribution + plan flip; browser handoff to
  the checkout URL (route-intercepted); dark starter plan stays 404.

## 5. Honest gaps — TODO tiers, not fake-green tests

- **Browser upload path exercises the web app's mock `/api/jobs` routes** — that is what the
  product wires today (Phase 2R). The identical journey against the real API is covered
  request-level in `api-funnel.spec.ts`. When Phase 3 points the panel at the real API, the
  browser spec starts covering it with no changes.
- **`tests/Live/` and `tests/fixtures/llm/` now exist.** Stage B/C run behind a
  `PipelineModel:Mode` switch (`fake` | `live` | `replay` | `record`). The default suite and E2E
  use `fake` — a deterministic offline stand-in that synthesizes a valid document from the real
  Stage A segments, so there is zero Vertex spend and no network. `replay` reads committed fixtures
  under `tests/fixtures/llm/` through the *real* Stage B guards + fuser + renderer (the
  `PipelineReplayHarnessTests` fidelity tests). `record` wraps real Vertex and refreshes those
  fixtures; `live` is the real path used by prod, the gallery worker, and `tests/Live/`. Refresh
  fixtures with `PipelineModel__Mode=Record` + `PipelineModel__FixturesDirectory=tests/fixtures/llm`.
- **Settings "erase" button is UI-only** (no API call yet) — deliberately untested until wired.

## 6. Harness invariants

- Determinism is a hard requirement: no Vertex, no Stripe, no network beyond localhost.
  `STRIPE_SECRET_KEY` unset ⇒ FakePaymentGateway (deterministic URLs, `test-signature` webhooks).
- The Postgres-backed .NET tests create a throwaway database per run and drop it; the E2E suite
  writes to the dev `vectorreel` database (unique emails/sessions per run, no cleanup needed).
- `CORS_ALLOWED_ORIGINS` on the api enables the browser → api calls (`AllowCredentials` because
  `sendBeacon` always sends credentials mode include). Unset ⇒ no CORS headers.
- Port conflicts: anything squatting on 3000/8080 (e.g. a leftover `next dev`) shadows the
  containers and produces confusing failures — `scripts/e2e.sh up` output tells you the ports.
