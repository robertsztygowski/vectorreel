# CLAUDE.md — mdreel

EU SaaS: video → structured, timestamped Markdown for AI knowledge bases.

## Which doc is authoritative for what

**Read the living docs before non-trivial work. They are the source of truth — always current,
rewritten in place.**

**🎯 The goal: survival — a handful of retained paying accounts covering infra** (METRICS.md **N1a**).
The founder needs **no salary** from this near-term, so the job-replacement figure (N1b) is the
*destination*, **not the plan**. **The top risk is A5 — distribution**, and because there is no burn
clock, its kill criterion is a **calendar deadline** (METRICS.md §2.2), not a cash threshold.
**Paid ads are gated experiments that buy evidence, not customers** (METRICS.md §1.6–1.7).
Read PLAN.md's STATUS block first.

| Doc | Sole authority for |
|---|---|
| **PLAN.md** | *What to do next, and in what order.* **Start here** — the STATUS block is the 60-second brief. |
| **METRICS.md** | 🔢 **Every load-bearing number, the five assumptions (A1–A5), and the decision rule attached to each.** |
| **ARCHITECTURE.md** | The *target design* — pipeline, stack, ingestion paths, API, data model. (Not the build order; that's PLAN.md.) |
| **BUSINESS_MODEL.md** | Problem, solution, positioning, ICP, pricing, non-goals. |
| **DISTRIBUTION.md** | How customers are reached. Owns **A5**. |
| **INFRA.md** | Current GCP state. |
| **DEVELOPMENT.md** | Full guidelines (this file is the summary). |

### 🚨 One fact, one home

**A number or decision rule lives in exactly ONE doc. Everywhere else cites it by name.**
*"Below break-even (METRICS.md N6)"* — never a second copy of the figure.

On 2026-07-14 a superseded memo contradicted four living docs because the same numbers had been
copied into all of them and only some were updated. **A number that lives in five files has five
chances to be wrong and one chance to be right.** `scripts/check-docs.sh` enforces this and is part
of the definition of done (rule 4).

⚠️ **`experiments/**/*.md` is NEVER authoritative.** Point-in-time memos and raw results: true on
the date written, frozen afterwards, routinely **superseded**. A reasoning trail, not instructions.
**If an experiment memo contradicts a living doc, the living doc wins — always.** Check the memo's
status banner before quoting it.

## Hard rules

1. **No secrets in git.** `.env` gitignored, `.env.example` committed & complete. No GCP JSON
   key files ever (ADC locally, service accounts on Cloud Run). Scan every diff before commit.
2. **EU regions only**, declared explicitly on every resource (`europe-central2` / `europe-west3`).
3. **Strict build:** `TreatWarningsAsErrors` + `Nullable` on; `dotnet format` before commit.
4. **Definition of done** (before any commit to main): build clean → integration tests green
   (docker compose) → affected flow exercised e2e locally (real Vertex if pipeline/prompts touched)
   → no secrets in diff → impacted living doc updated in the same commit →
   **`scripts/check-docs.sh` passes** (no number restated outside METRICS.md).
5. **Deploy is deliberate**, from local, after `tests/Live/` passes. Never auto-deploy.
6. Every LLM call, **every compute step, and every euro of ad spend** is recorded in the ledger —
   product feature, not optional. (ffmpeg/Cloud Run transcode is ~30% of true COGS; metering only
   LLM calls undercounts by a third. **Ad spend is the same bug: contribution per account is a
   fiction if acquisition cost isn't in it** — METRICS.md N29.)
7. Git: direct to main, small complete commits, `feat:`/`fix:`/`chore:`/`docs:`/`infra:` prefixes.
8. **YouTube: never download bytes.** Ingestion is Vertex `fileData.fileUri` only — Google fetches,
   we don't. **No `yt-dlp`, no scraping, ever**, however awkward `fileUri` gets. A YouTube-ToS
   violation inside a company that sells compliance to DPOs is existential, not a nit. The public
   gallery is **curated, CC-licensed, attributed** — never a scaled transcript farm.
   **YouTube is free distribution only, never a paid feature** (BUSINESS_MODEL §10).
9. **Stage B calls always set `maxOutputTokens`, a bounded `thinkingBudget`, and a wall-clock
   timeout.** ~8% of benchmark calls degenerated (61k output / 63k thinking tokens). Unguarded
   calls break the SLO and the margin.
10. **No US-based analytics or tracking on any mdreel property. Ever.** **Google Analytics is
    prohibited** (ruled unlawful by several EU DPAs over US transfers). Analytics is **Plausible,
    EU-hosted, cookieless — no consent banner.** The same test applies to *every* tool: heatmaps,
    session replay, chat widgets, A/B platforms, marketing pixels. **If it phones home to the US, it
    does not ship.** We sell EU residency to DPOs and our own copy tells them where to look — GA on
    this site is spotted in ten seconds. (Google **Ads** is fine — that is delivery. The Google
    **pixel** is not; conversions are measured first-party. METRICS.md §6.)

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
