# CLAUDE.md — vectorreel

EU SaaS: video → structured, timestamped Markdown for AI knowledge bases.
**Read the living docs before non-trivial work:** ARCHITECTURE.md (what/how),
BUSINESS_MODEL.md (why), INFRA.md (GCP state), PLAN.md (phase-by-phase execution plan),
**DEVELOPMENT.md (full guidelines — this file is the summary).**

## Hard rules

1. **No secrets in git.** `.env` gitignored, `.env.example` committed & complete. No GCP JSON
   key files ever (ADC locally, service accounts on Cloud Run). Scan every diff before commit.
2. **EU regions only**, declared explicitly on every resource (`europe-central2` / `europe-west3`).
3. **Strict build:** `TreatWarningsAsErrors` + `Nullable` on; `dotnet format` before commit.
4. **Definition of done** (before any commit to main): build clean → integration tests green
   (docker compose) → affected flow exercised e2e locally (real Vertex if pipeline/prompts touched)
   → no secrets in diff → impacted living doc updated in the same commit.
5. **Deploy is deliberate**, from local, after `tests/Live/` passes. Never auto-deploy.
6. Every LLM call **and every compute step** records cost in the per-job ledger — product feature,
   not optional. (ffmpeg/Cloud Run transcode is ~30% of true COGS; metering only LLM calls
   undercounts by a third.)
7. Git: direct to main, small complete commits, `feat:`/`fix:`/`chore:`/`docs:`/`infra:` prefixes.
8. **YouTube: never download bytes.** Ingestion is Vertex `fileData.fileUri` only — Google fetches,
   we don't. **No `yt-dlp`, no scraping, ever**, however awkward `fileUri` gets. A YouTube-ToS
   violation inside a company that sells compliance to DPOs is existential, not a nit. The public
   gallery is **curated, CC-licensed, attributed** — never a scaled transcript farm.
   **YouTube is free distribution only, never a paid feature** (BUSINESS_MODEL §10).
9. **Stage B calls always set `maxOutputTokens`, a bounded `thinkingBudget`, and a wall-clock
   timeout.** ~8% of benchmark calls degenerated (61k output / 63k thinking tokens). Unguarded
   calls break the SLO and the margin.

## Layout

- Monorepo: `src/Api` (minimal API, vertical slices in `Features/`), `src/Worker`,
  `src/Core` (pipeline Stages A–D, providers, output renderer — no ASP.NET deps),
  `tests/{Unit,Integration,Live,fixtures}`, `web/` (Next.js), `infra/` (Terraform),
  `experiments/` (any language, no standards, never imported by `src/`).
- Abstraction only at seams: `IVideoAnalyzer`, `ITextFuser`, `IObjectStorage`, `ITaskQueue`.
  No MediatR, no layer ceremony, built-in DI only.

## Testing

- Integration-first: docker compose (postgres, fake-gcs-server) + `InProcessQueue`;
  LLM calls replayed from `tests/fixtures/llm/`. TDD for pipeline/algorithmic logic.
- LLM assertions check structure/invariants, never exact prose.
- `tests/Live/` (real Vertex): before deploys and whenever prompts/schemas change;
  `--record` refreshes committed fixtures.

## Commands

```bash
docker compose up -d                      # local deps
dotnet run --project src/Api              # + src/Worker
dotnet test --filter Category!=Live       # default suite
dotnet test tests/Live                    # real Vertex smoke (pre-deploy)
dotnet format                             # before commit
# deploy (see INFRA.md for full commands):
gcloud run deploy <svc> --region europe-central2 --project tensile-runway-442915-j6
```
