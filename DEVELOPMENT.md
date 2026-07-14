# Development Guidelines — vectorreel

> How the software gets built. Companion to ARCHITECTURE.md (what to build) and
> BUSINESS_MODEL.md (why). Hard rules are mirrored in CLAUDE.md.
> Decisions recorded 2026-07-14; update this doc in the same commit as any decision that changes it.

## 1. Principles

1. **Velocity with a floor.** Solo pre-MVP: minimum ceremony, but the floor is non-negotiable —
   strict compiler settings, tested pipeline core, no secrets in git. Cheap now, ruinous to retrofit.
2. **The pipeline is the product.** Stages A–D and the output contract get the highest testing bar.
   CRUD, UI, and glue code get a lighter one.
3. **Abstraction only at the seams.** Provider interfaces (`IVideoAnalyzer`, `ITextFuser`, storage,
   queue) exist because migration (Mistral, EU-owned infra, self-hosted) is on the roadmap.
   Everywhere else: plain classes, no speculative layers, no MediatR, built-in DI only.
4. **Integration over unit.** Confidence comes from exercising real flows against real-ish
   dependencies, not from mocking everything. Unit tests where logic is pure and dense
   (segment math, rendering, cost calc); integration/e2e for everything that talks to something.
5. **EU + cost awareness are code concerns.** Every resource pinned to an EU region explicitly.
   Every LLM call recorded in the per-job cost ledger. These are product features, not ops chores.

## 2. Repository layout (single monorepo)

```
vectorreel/
  VectorReel.sln
  Directory.Build.props        # strictness settings, applies to all projects
  .editorconfig
  src/
    Api/                       # ASP.NET Core minimal API — Cloud Run service
      Features/                # vertical slices: Jobs/, Uploads/, Usage/, Webhooks/, ApiKeys/
    Worker/                    # queue-driven worker — Cloud Run service
      Stages/                  # stage handlers (thin: dequeue → call Core → persist)
    Core/                      # domain + pipeline (the IP lives here)
      Pipeline/                # StageA/ StageB/ StageC/ StageD/
      Providers/               # IVideoAnalyzer, ITextFuser, IObjectStorage, ITaskQueue + impls
      Output/                  # output-contract renderer (markdown + json)
      Domain/                  # entities, job state machine, cost ledger
  tests/
    Unit/                      # pure logic only
    Integration/               # against docker compose; LLM = replayed fixtures
    Live/                      # real Vertex AI; run on demand (see §5)
    fixtures/                  # recorded LLM responses, sample videos, golden outputs
  web/                         # Next.js 15 app (currently: static landing page)
  infra/                       # Terraform (EU region pinning as code)
  experiments/                 # ANY language, ZERO standards — see §9
  docs/                        # (optional) diagrams, DPA template, etc.
  docker-compose.yml           # local dev/test dependencies
  .env.example                 # every variable the app reads, with placeholder values
```

Rules:
- `Core` has no ASP.NET or Cloud Run dependencies — it must run in a future self-hosted edition.
- `Api` and `Worker` depend on `Core`; never on each other.
- New API capability = new folder under `Features/` (endpoint + request/response types +
  handler together). No `Controllers/`, `Services/`, `Repositories/` layer folders.

## 3. .NET conventions

- **Strict from day one** — `Directory.Build.props` at repo root:
  ```xml
  <Project>
    <PropertyGroup>
      <Nullable>enable</Nullable>
      <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
      <AnalysisLevel>latest-recommended</AnalysisLevel>
      <ImplicitUsings>enable</ImplicitUsings>
    </PropertyGroup>
  </Project>
  ```
- `.editorconfig` committed; run `dotnet format` before every commit.
- Minimal APIs, `record` types for DTOs, `IOptions<T>` for config binding.
- Async end-to-end; no `.Result`/`.Wait()`.
- Errors returned as RFC 7807 `problem+json` (per ARCHITECTURE.md §5).
- Structured logging (`ILogger` message templates) with `jobId`/`tenantId` scopes on every
  pipeline log line; OpenTelemetry traces across API → queue → worker.
- External calls (Vertex, GCS) get retry with exponential backoff + jitter; idempotency key
  `jobId+segmentIndex` so a failed segment retries alone (never restart a job from zero).

## 4. Testing strategy

Shape: **integration-first, TDD where it pays.**

| Layer | What | When to write |
|---|---|---|
| **Golden/snapshot** | Stage A segmentation + static detection on sample videos; output-contract renderer (same input → byte-identical markdown) | With the feature — these define correctness of the IP |
| **Integration** | API endpoints + worker stages against docker compose (Postgres, fake GCS, in-process task dispatch); LLM calls replayed from fixtures | The default test type — most tests live here |
| **e2e** | Full pipeline upload → markdown on a short reference video, real Vertex | Before deploy; when pipeline or prompts change |
| **Unit** | Pure, dense logic: timestamp math, overlap merging, sampling config, cost calculation | TDD style — write the test first for anything algorithmic |

- TDD is the expected workflow for pipeline logic (write the failing test, then implement).
  For glue/CRUD, tests may follow the code — but they exist before the commit.
- Assertions on LLM-derived content check **structure and invariants** (valid JSON schema,
  timestamps monotonic, no empty sections), never exact prose.
- Reference videos live in `tests/fixtures/videos/` (small, seconds-long clips committed;
  larger benchmark videos in a GCS dev bucket, fetched by script).

## 5. Local environment (hybrid: Docker deps + real GCP AI)

```
docker compose up:
  postgres:16              # app DB
  fake-gcs-server          # GCS emulation for buckets
# Cloud Tasks: no emulator — local runs use an in-process dispatcher behind ITaskQueue

Real GCP from the local machine (via gcloud ADC):
  Vertex AI Gemini — dev project, EU region (europe-central2 / europe-west3)
```

- `ITaskQueue` has two implementations: `CloudTasksQueue` (deployed) and
  `InProcessQueue` (local/tests). Selected by config.
- GCP auth is **Application Default Credentials** (`gcloud auth application-default login`).
  Never download service-account JSON key files.
- One command to a running system: `docker compose up -d && dotnet run --project src/Api`
  (plus Worker). Keep it that simple; if setup grows, script it in `scripts/`.

### LLM record/replay

- Integration tests replay recorded Vertex responses from `tests/fixtures/llm/` —
  fast, free, deterministic, offline-capable.
- `tests/Live/` hits real Vertex. Run it: **before every deploy**, **whenever a prompt or
  response schema changes**, and periodically to catch model drift.
- Refreshing fixtures is one command (a `Live` run with a `--record` flag rewrites fixtures);
  refreshed fixtures are committed so the diff shows behavior drift.

## 6. Secrets & configuration

- `.env` (gitignored) for local values; **`.env.example` committed** and kept complete —
  every variable the app reads appears there with a placeholder. A new variable without its
  `.env.example` line is a bug.
- No GCP key files ever — ADC locally, service-account identity on Cloud Run.
- Production secrets (Stripe keys, webhook secrets) go to **Secret Manager**, injected into
  Cloud Run as env vars at deploy.
- **Before every commit: scan the diff for secrets** (keys, tokens, connection strings,
  real customer data in fixtures). This is part of the definition of done.
- Test fixtures must contain no real personal data — sample videos are ours or synthetic.

## 7. Git workflow

- **Direct to main** while solo. Revisit (PRs + protection) when a second contributor joins.
- Main must always satisfy the definition of done — small, complete commits; don't push
  mid-refactor broken states.
- Commit messages: imperative summary line, body explains *why* when non-obvious.
  Conventional-commit prefixes (`feat:`, `fix:`, `chore:`, `docs:`, `infra:`) — lightweight, aids scanning.
- A decision that changes ARCHITECTURE.md / BUSINESS_MODEL.md / INFRA.md / DEVELOPMENT.md is
  committed **together with the doc update**. The living docs are the source of truth; no ADRs.

## 8. Definition of done & deploy

A change may be committed to main when:

1. `dotnet build` — zero warnings (enforced by TreatWarningsAsErrors) — and `dotnet format` clean.
2. Integration tests green against docker compose.
3. The affected flow was exercised **end-to-end locally** — real Vertex if the pipeline,
   prompts, or schema were touched.
4. Diff contains no secrets, no real personal data, no stray key files.
5. Any impacted living doc updated in the same commit.

**Deploy is a separate, deliberate step** (no CI/CD yet — from the local machine):

1. Run `tests/Live/` (real Vertex smoke).
2. `gcloud run deploy … --region europe-central2 --project <project>` per service
   (commands recorded in INFRA.md).
3. Verify the deployed service on a real request; note the revision in INFRA.md if noteworthy.

CI/CD (GitHub Actions) comes later; nothing in this workflow should make that harder —
everything already runs headless from a shell.

## 9. experiments/ — the escape hatch

Prompt tuning, sampling-config benchmarks, and cost measurements need iteration speed,
not build systems.

- Any language (Python notebooks welcome). No style, test, or strictness rules apply.
- Never referenced by product code; nothing under `src/` may import from it.
- Same secrets rules apply (no keys, `.env` gitignored) — that rule has no escape hatch.
- When an experiment produces a decision (a prompt, a sampling config, a cost number),
  the result graduates into `src/` + fixtures + the relevant doc; the notebook can stay as a record.

## 10. Cost & EU guardrails (recap as code rules)

- Every LLM call writes tokens + cost to the per-job ledger — no exceptions, it's a product feature.
- Every GCP resource (bucket, queue, service, Vertex endpoint) declares an EU region explicitly
  in Terraform/config — never rely on defaults.
- New third-party dependency that processes customer data = update the subprocessor list
  and /gdpr page in the same change (currently: Google Cloud EU, Stripe, email provider).
