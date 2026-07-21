# mdreel ~8h autonomous build run — coordinator launch prompt (first real collection)

> **How to use:** open a fresh agent session in the repo root, enable `/autopilot`, `/keep-alive`,
> raise `/limits`, then paste everything below the line as the first message.
>
> **Provenance:** the settled decisions in this prompt come from the 2026-07-21 founder strategy
> session. The reasoning behind each one — including the ideas that were *rejected* and why — is in
> [`DECISIONS.md`](./DECISIONS.md) next to this file. Read it if a constraint here looks arbitrary;
> do not relitigate a decision without reading why it was made.

---

Fleet deployed: you are the **marathon coordinator** for an ~8-hour autonomous build run on mdreel
(repo `robertsztygowski/vectorreel`, branch `main`, starting at `481d3fd`). Work fully
autonomously — the founder is away and does NOT want to be involved. Before starting, read
PLAN.md STATUS (especially the 2026-07-20/21 pivot-run report and NEEDS-FOUNDER), ARCHITECTURE.md
**§4b repository contract** + §4 + §5, DISTRIBUTION.md ("First public collections", "GitHub
distribution", "The weekly publishing system"), INFRA.md, `experiments/005-pivot-assumption-tests/TESTS.md`,
METRICS.md (cite numbers **by name**, never restate), TESTING.md and CLAUDE.md.

**This run is the one that finally spends real inference money.** The last four runs shipped
product surface and narrative; every deployed service still runs `PipelineModel__Mode=fake`. The
top risk is **A5 — distribution** (METRICS.md §2, read via §4 "A5 — Traffic"), and the single artifact that moves it is a
**real, browsable AI Engineering collection**. Docs and scaffolding do not move A5. Producing the
corpus is the point of this run; everything else is downstream of it.

## Your role: coordinator, not coder

- You sequence milestones, launch worker sub-agents (complete context per prompt), verify their
  output, commit, deploy, and move on. Prefer delegating implementation; keep your own context for
  orchestration, verification and recovery. ⚠️ **Last run a delegated sub-agent returned empty
  after a long run** — set a checkpoint expectation in every worker prompt (files written +
  tests run), and if a worker returns nothing usable, re-implement directly rather than re-delegating.
- Track everything in todos (one per milestone task, kebab-case ids, explicit dependencies).
  `in_progress` before starting, `done`/`blocked` at completion.
- Each milestone ends in 1–3 commits direct to main — **git history IS the checkpoint.** If you
  crash and restart, `git log` + todos + PLAN.md STATUS tell you exactly where you were.
- Update the PLAN.md STATUS block at every milestone boundary, in the same commit.

## Authorization contract (founder-approved for THIS RUN only)

1. **Rule 5 override**: you ARE authorized to deploy to Cloud Run from local for this run (via
   `scripts/deploy.sh`), after the DoD gate passes. Record the override in INFRA.md. Never enable
   any CI auto-deploy.
2. **🚨 Vertex spend is AUTHORIZED this run — bounded.** The founder approved producing
   **20–25 AI Engineering sessions** through the real pipeline. Conditions, all mandatory:
   - Every LLM call **and** every compute step recorded in the ledger (CLAUDE.md rule 6). A
     session that cannot be reconciled in the ledger does not count as produced.
   - **Calibrate before you commit the batch:** produce the first **2** sessions, reconcile actual
     €/video-hour against **METRICS.md N4c**, and extrapolate the full batch. If the projection
     exceeds the founder-approved budget guardrail recorded in INFRA.md, **shrink the batch**
     (quality gates never shrink — DISTRIBUTION.md weekly-runbook cut rule) and record the cut.
   - **Hard stop** if any single session's cost approaches **METRICS.md N4d** — that is the naive
     over-segmentation failure mode, not a price. Fix the segmentation (rule 9: `maxOutputTokens`,
     bounded `thinkingBudget`, wall-clock timeout; segment dense content shorter *up front*) or
     drop that video. Do not pay for overflow retries in bulk.
   - Production runs use the low-media-resolution default (**METRICS.md N4b/N4c** — this is the
     internal gallery-production path). Deployed prod services stay `PipelineModel__Mode=fake`
     unless a milestone explicitly needs otherwise.
3. **Rule 2, hard**: EU only. Cloud Run `europe-west1`; buckets/SQL/secrets `europe-central2`,
   Vertex `europe-central2` with the `europe-west3` fallback already configured. Any non-EU
   resource = hard stop.
4. **Rule 1**: no secrets in git, no GCP JSON keys. ADC locally, runtime SA on Cloud Run, all
   credentials via Secret Manager. Scan every diff before commit. **The GitHub token/`gh` auth
   already on the machine is the only Git credential — never write it into a file.**
5. **Rule 8, hard**: **never download YouTube bytes.** Ingestion is Vertex `fileData.fileUri` only.
   No `yt-dlp`, no scraping, no "just for the thumbnail". A ToS violation inside a company selling
   compliance is existential. **CC-licensed sources only, verified and attributed per video before
   processing** (DISTRIBUTION.md inclusion criteria #1) — an unverifiable licence means the video
   is dropped, never processed "pending check".
6. **Rule 10**: no US analytics/tracking, ever. Umami only.
7. **Stripe test mode only** — a `sk_live_…` key anywhere is a hard stop.
8. **New continuously-billing resources = STOP + NEEDS-FOUNDER.** Pre-approved standing surface
   only: existing Cloud Run services (caps + `min-instances=0`), shared Cloud SQL `mdreel-db`,
   Cloud Tasks `webhook-deliveries` queue, the two GCS buckets, Secret Manager. Vertex inference is
   authorized per §2 above and is usage-based, not a new fixed base.
9. **Hard stops** (halt, write the report, wait): non-EU resource required; live payment keys; a
   source video that is not verified **CC-BY** (`NC`/`ND`/`SA`/standard-YouTube/unknown all
   disqualify); **making any GitHub repo public**; destructive operation on
   prod data; git history rewrite; Vertex projection over the guardrail after a batch cut.

## 🚧 The public-artifact boundary (founder decision, 2026-07-21)

**Nothing becomes publicly visible this run.** The founder wants to review licensing and
attribution before anything carries mdreel's name in public.

- GitHub org **`github.com/mdreel` exists**. Collection repos are pushed **PRIVATE**. Flipping
  visibility is the founder's call → NEEDS-FOUNDER.
- Collection pages on mdreel.com are built and may be deployed, but **must be gated behind a
  flag that is OFF in prod** (route returns 404 / is unlinked when off). Local, CI and E2E run
  with the flag ON so the pages are fully tested. The existing public `/gallery` copy is
  unchanged — do not point it at unreviewed content.
- **Do NOT set T0** in METRICS.md §2.2 and do not publish a post, an ad, or a release
  announcement. Phase 5 artifacts are *drafted for founder review only*.

## NEEDS-FOUNDER protocol

Carried over and still open: Polish lawyer review of the legal pack · Brevo API key ·
`Admin__Emails` on the prod api · confirm the first Cloud Monitoring alert email arrives · the
GDPR consent decision for first-party UTM attribution (PLAN.md Phase 5 pre-flight). New this run
will be at minimum: **licence/attribution review + repo visibility flip**.

**Add one question to the queued Polish-lawyer review** (append it to that NEEDS-FOUNDER item, do
not create a competing one). It is the single highest-leverage legal question the company has,
because it sets the ceiling on how much of any topic can ever be published:

> Under EU law (incl. the DSM Directive Art. 3/4 TDM exceptions and their opt-out) and YouTube's
> ToS, which of these may we publish from a video we do not own and that is **not** CC-licensed:
> (1) a generated summary; (2) extracted structural metadata (topics, timestamps, speaker,
> chapters); (3) transformed knowledge — an answer synthesized across many talks, citing each;
> (4) a near-complete timestamped transcript incl. verbatim on-screen text (this is what a §4b
> `full` session document is). We currently publish (4) for CC-BY only and (2) for anything
> public. Is (1) or (3) safely available to us, and does the answer change between the **public
> collections** and a **customer's private repository** of their own footage? **And separately
> from all four: may we *process* a non-CC public video for internal analysis — publishing no
> derived text from it, only a cross-source synthesis that cites it — or does processing itself
> require a licence?** (This last one decides whether cross-source synthesis can ever scale beyond
> CC-BY material, so answer it even if the others are ambiguous.)

Note in PLAN.md that a relaxation here **widens every future collection**, and that until it is
answered the two-tier rule above stands.

**Nothing blocks on these.** When work reaches a point needing one: build to "one
credential/decision from done", append the exact action (exact command, exact record, exact
question) to the NEEDS-FOUNDER section in PLAN.md STATUS, and move to the next unblocked todo.
Never idle, never guess or fabricate credentials.

## Settled decisions (do not relitigate)

- **🧭 The unit of a collection is a TOPIC, not a conference or an event** (founder decision,
  2026-07-21). Nobody wakes up needing "KubeCon 2026"; they need "how are people building agents".
  A collection is assembled **across sources** — many events, many speakers, many years — and that
  cross-source synthesis is the thing no single conference channel can offer. Conference names are
  *provenance metadata on a session*, never the top-level object. This is a curation/naming
  decision only: ARCHITECTURE §4b already carries `topics/`, `speakers/` and `timeline/`, and the
  pipeline is unchanged.
- **A collection must satisfy all five selection properties** (fold these into DISTRIBUTION.md's
  inclusion criteria in M1): (1) **licence-feasible** — enough CC-BY material exists to cover the
  topic honestly; (2) **impossible to consume manually** — if a person could watch it in an
  afternoon, it is not a collection; (3) **evergreen** — a subject, not a schedule ("Swift
  Concurrency", never "WWDC Day 2"); (4) **naturally expandable** — next month's batch has an
  obvious home in it; (5) **cross-source** — a minimum of **three distinct events/channels**
  represented, or it is a conference index wearing a topic's name.
- **🚨 Two publication tiers per collection — this is what lets a topic be covered honestly**
  (founder decision, 2026-07-21). ARCHITECTURE §4b gains an **additive v1.1** field: every session
  in `metadata/manifest.json` carries `inclusion: "full" | "reference"`.
  - **`full`** — the complete §4b session document (structured timestamped Markdown, verbatim
    on-screen text, original embedded). This is a near-complete derivative of the talk. **CC-BY
    only, no exceptions.**
  - **`reference`** — an index entry only: title, speaker, event, year, topic tags, and deep-link
    timestamps into the original. **No derived text, no transcript, no summary of the content, no
    processing.** This is the §4b rule "indexes cite, never restate" applied to material we may not
    render. Eligible for any publicly available video, and it **costs zero inference budget**.
  - Renderers, validator and site pages must make the tier visible to a reader — a collection that
    blurs the two is worse than one that publishes less. Reference entries never appear as
    session pages; they appear inside topic/timeline/speaker indexes.
  - This is a **contract + curation** change only. **Do not build a reference-ingestion pipeline
    this run**; reference entries are hand-curated metadata. Design the field so a later run does
    not need a v2.
- **🏭 TWO FACTORIES — the scope guard for this run and the next six months** (founder,
  2026-07-21). Factory A: `video → session document` — deterministic, contract-frozen (§4/§4b),
  cost measured (METRICS.md **N4c**), quality gate exists. Factory B:
  `many session documents → knowledge artifact` — editorial, cost is **founder-hours** (the
  scarcest input in the company, and the one nothing currently meters), quality gate does not
  exist, legality open. **Factory A is the product. Factory B is an experiment.** Land that exact
  sentence in the PLAN.md STATUS block this run — it is the cheapest available defence against
  scope drift. Note also that Factory B's raw material *is* Factory A's output, so the ordering is
  enforced by dependency, not only by discipline.
- **A third tier — cross-source editorial synthesis (Factory B) — is the likely future product
  line, and this run does NOT build it as a system.** A synthesis that answers a question across many talks and
  cites each one is legally the most permissive layer (category 3 in the lawyer question below)
  and is what a reader actually shares. It is **not** built this run because it has two unpaid
  costs: (a) synthesizing across non-CC talks requires *processing* them, which is exactly the open
  legal question — until answered, a synthesis may only draw on `full`-tier CC-BY material;
  (b) a synthesis asserts claims in mdreel's name while **A4 (output trust) is open** and the
  weakest measured category is METRICS.md **N32** — so it needs a far heavier gate than the
  3-timestamp spot-verify that gates a session document. **Exactly one** synthesis artifact is
  hand-authored this run, as the M7 launch draft, and it is not published. Its performance later
  decides whether this becomes a tier.
- **🚨 Licence feasibility is the FIRST selection filter, not the final check** — for `full` tier.
  **CC-BY only.**
  `NC` variants are disqualified (our use is commercial), `ND` is disqualified (we publish a
  derivative), and `SA` is disqualified for v1 because share-alike conflicts with the CC-BY-4.0 we
  publish derived content under (DISTRIBUTION.md "GitHub distribution"). Standard-YouTube-licence
  material — vendor keynotes, most product channels, most paid conferences — **is not eligible for
  `full` at any volume, however good it is**, and is `reference` tier at best. Do not argue fair
  use; do not process "pending a check". If a topic has too little CC-BY material for a credible
  `full` core, **pick a different topic** and record why — a collection that is 90% reference
  entries is a link list, not a demo of the manufacturing process.
- **The CC-BY-only rule for `full` is a conservative default under review, not a settled truth.**
  The founder has asked whether the four distinct legal situations (generated summaries · extracted
  metadata · transformed knowledge · near-complete transcripts) relax it. **That question goes to
  the lawyer already queued in NEEDS-FOUNDER — not to this run, and not to a worker sub-agent.**
  Until it is answered, CC-BY-only stands and is not to be relitigated mid-run.
- **Collection #1 = a topic-first cut of AI Engineering** (production RAG / agents / evals /
  LLM-ops), assembled cross-source from CC-BY corpora — FOSDEM, CNCF/Linux Foundation events,
  and comparable CC-BY conference archives. Verify every licence at the source; the named
  examples are leads, not permission. `.NET` and `Kubernetes/CNCF` get *scaffolded* repos only
  this run — no inference budget for them.
- **The public/private asymmetry is the product philosophy — treat it as settled** (founder,
  2026-07-21): *the public collection shows the shape, and the shape is only complete on video you
  own.* Public artifacts **demonstrate**; private repositories **monetize**. mdreel is a
  **manufacturing system for AI-ready knowledge artifacts**: the SaaS is the factory, the
  customer's own video is the raw material, the subscription buys access to the factory — **never
  access to the published collections.** Publishing is a *distribution* model, not a revenue model;
  it must never drift into a media business. This is compatible with BUSINESS_MODEL §7 as written
  — it sharpens it, it does not replace it. If a **single sentence** in BUSINESS_MODEL §4 or §7
  states it better, land it; **do not open a positioning rewrite** (three reframings in six days
  is already one too many, and nothing has shipped between them). The promise is **checkable**
  answers — every claim carries a timestamp into footage the reader can open. Never promise
  *correct* answers: that is A4, it is open, and METRICS.md **N32** says where it is weakest.
  Keep the two sentences separate and concrete: **the repository is the artifact; checkable
  answers are what it gives you.** Rejected 2026-07-21 and not to be re-proposed:
  *"auditable knowledge artifacts"* — abstract vendor register with no reader in it, and
  "auditable" implies a formal audit trail we do not provide, which is a near-miss claim sitting
  next to a real compliance claim.
- **🌐 mdreel.com is the destination; GitHub is an export target.** The site is where the funnel is
  instrumented — **you cannot measure a GitHub repo**: no Umami, no `collection_convert_click`, no
  funnel, and distribution is the top risk. GitHub earns its place as *portability proof* (clone
  it, open it in Cursor/Claude Code, it is yours, no walled garden), which matters to a
  docs-as-code ICP. Consequence for this run: the repos are private and therefore carry **zero
  distribution value**, so **M5 (site pages) outranks M4 (repo push)** — if the run runs short,
  M4 slips, not M5.
- **Naming: do NOT rename anything in code, contract or events.** `collection` stays the noun in
  the manifest (`visibility: "public-collection"`), the routes, and the Umami events
  (`collection_session_view`, `collection_convert_click` — continuity was deliberately preserved
  last run and a rename destroys the series). The *published artifact* may carry a different
  marketing noun in its title and copy; that word is a founder decision to be settled on real
  traffic via the existing A/B instrument, not chosen mid-run. **"Dossier" is ruled out** — in
  European usage it means a file kept on a *person*, which is precisely the wrong word in front of
  a DPO.
- **Compounding is upside, never a plan.** Content brands (Stripe, Cloudflare, Fowler) compounded
  over years, each with a salary or a team underneath. This company has a **calendar kill deadline
  (METRICS.md §2.2)** instead of a burn clock. "It will compound" must never be used to justify
  shipping slower or to soften the A5 kill criterion — it is unfalsifiable on the timescale the
  decision is made.
- **Deferred, deliberately: how hard to lean on continuous publishing as the distribution motion.**
  Not "is mdreel a publisher" — the docs already answer that (it is not; it publishes to earn
  trust, like Stripe's docs or HashiCorp's modules). The open question is **intensity and cadence**,
  and it needs evidence a shipped collection has not yet produced (A5 is the top risk, N15 is still
  zero). Do not reframe any living doc toward it during this run. Note it in the final report as a
  founder session, not a milestone.
- **The repository contract is ARCHITECTURE.md §4b, v1, frozen.** Generate to it; do not redesign
  it. The canonical example `tests/fixtures/repository/` and `web/lib/repository.test.ts` (22
  tests) are the acceptance oracle — extend, never weaken.
- **Licence audit trail = the `corpus.json` pattern** already used in `experiments/001-*`. One
  record per video: source URL, licence, licence-verification evidence, attribution line, date.
- **Speakers are curation metadata, not inference** (§4b) — do not have the model guess identities.
- **Indexes cite, never restate** (§4b citation grammar). Topic/speaker/timeline pages link into
  session documents with timestamps; they do not duplicate prose.
- **Repo conventions are already written** (DISTRIBUTION.md "GitHub distribution"): README shape,
  CHANGELOG as freshness proof, weekly `v<yyyy.ww>` releases, CC-BY-4.0 for derived content,
  the two issue forms in `templates/collection-repo/`. Use them as-is.
- **Site pages are derived from the §4b contract**, not hand-authored per collection, and not from
  the per-file specimen renderer the current gallery uses.
- **No new infra, no Terraform, no queue changes.** This run is corpus + generation + pages.
- Numbers live in METRICS.md; cite by name. `scripts/check-docs.sh` enforces it.

## Milestones (dependency order)

### M0 — Guardrails (do FIRST; nothing else starts until committed)
- `scripts/preflight.sh` green (expect 21/21); confirm the GCP budget alert exists and Cloud Run
  caps + `min-instances=0` hold on web/api/worker/umami; confirm `gh auth status` can reach the
  `mdreel` org; confirm ADC can reach Vertex in `europe-central2`.
- Record the rule-5 override **and the bounded Vertex authorization** for this run in INFRA.md.
- Create the M1–M8 todo graph. Commit `infra: …`.

### M1 — Topic-first source list + licence audit trail (depends on M0)
- **Work the funnel in this order: licence → topic fit → ICP recognition.** Start from CC-BY
  archives and find the topic they can honestly cover; do not start from a wish-list of talks and
  hope the licences work out. Expect to discard a lot — record the discards, they are the evidence
  behind the topic choice.
- 20–25 **CC-BY** sessions at `full` tier for collection #1, spanning **≥3 distinct
  events/channels**, meeting all five selection properties above. Verify each licence at the source
  and record the audit trail (`corpus.json` pattern) with per-video attribution lines. **No bytes
  downloaded — `fileUri` only (rule 8).**
- **Plus `reference`-tier entries** for the rest of the topic's significant public talks — the
  ones that make the collection cover its subject rather than its licences. Curated metadata only
  (title, speaker, event, year, topic tags, deep-link timestamps); **no processing, no derived
  text, no inference spend.** Aim for a `full` core that is credible on its own, not a link list.
- **ARCHITECTURE §4b → v1.1 (additive)**: add `inclusion: "full" | "reference"` to the manifest,
  update `tests/fixtures/contracts/repository-manifest.schema.json` and the canonical fixture
  `tests/fixtures/repository/` to carry at least one entry of each tier. §4 file contract and §5
  API stay frozen.
- Tests: extend `web/lib/repository.test.ts` (currently 22) so a `reference` entry carrying any
  derived text **fails**, and a `full` entry without licence + verification evidence + attribution
  fails, before either can enter production.
- Doc: rewrite DISTRIBUTION.md's inclusion criteria to the five selection properties and the
  CC-BY-only rule; state the topic-first unit explicitly (the three launch collections keep their
  rationale but are re-described as topics assembled cross-source). Record the produced source
  list by reference, not by copying criteria.
- Commit `feat: topic-first collection source list + licence audit trail` (+ `docs:` for the
  criteria rewrite if it lands as a separate commit).

### M2 — Repeatable collection-production path (depends on M0; parallel-safe with M1)
- One command that takes the source list and produces §4 output documents through the **real**
  pipeline: ledgered per call (rule 6), rule-9 guards on every Stage B call, low media resolution,
  **short segments up front** for dense slide content (the N4d lesson), resumable (re-running must
  not re-pay for already-produced sessions).
- Tests: unit/integration on the batch driver with the replay fixtures (no live calls in the
  default suite — `Category!=Live` must stay hermetic and offline).
- Commit `feat: collection production batch driver`.

### M3 — 🎯 Produce the real batch (depends on M1, M2) — **the milestone that matters**
- Calibrate on 2 sessions → reconcile against **METRICS.md N4c** → extrapolate → then run the rest.
- Per-session quality gate (DISTRIBUTION.md weekly-publishing threshold): licence gate,
  **3-timestamp spot-verify against the actual video**, §4b contract validation. Failures are
  dropped, not patched into the batch.
- Record measured reality: cost per video-hour actual vs N4c, overflow/runaway rate (**N7**) per
  content category, Stage-A forced-cue effectiveness (**N7c** — this batch is the first real read
  on whether Stage B *obeys* the boundaries; that verdict has been open since Phase 1).
- Doc: a point-in-time memo under `experiments/` for the measurements (never authoritative), and
  update METRICS.md **only** where a number it already owns is now measured rather than assumed.
- Commit `feat: first real AI Engineering collection corpus` + `docs: batch measurements`.

### M4 — §4b repository generation + PRIVATE push (depends on M3)
- Generate the AI Engineering repo to the §4b layout (`README.md`, `sessions/`, `topics/`,
  `speakers/`, `timeline/`, `metadata/manifest.json`) with README/CHANGELOG/release conventions
  and the `templates/collection-repo/` issue forms. Scaffold `.NET` and `K8s-CNCF` as
  contract-valid but empty-corpus repos.
- Validate against `repository-manifest.schema.json` + the 22-test validator (extend for anything
  the real corpus exposes that the fixture didn't).
- **Push all three PRIVATE to `github.com/mdreel`.** Never `--public`, never flip visibility.
- NEEDS-FOUNDER: licence/attribution review → make public → pin on the org profile.
- Commit `feat: collection repository generator + private mdreel org push`.

### M5 — Collection pages on mdreel.com from the §4b contract (depends on M1's v1.1 contract change; parallel-safe with M3/M4)
- Session / topic / speaker / timeline pages derived from a §4b repository (the canonical fixture
  is enough to build and test against — do not wait on M3). **Index pages must render both tiers
  and visibly distinguish them**: `full` entries link to a session page, `reference` entries link
  out to the original with their timestamps and never to an mdreel session page. Behind a prod-OFF
  flag per the
  public-artifact boundary above. Reuse the shipped `ConvertCta` and the
  `collection_session_view` / `collection_convert_click` events — no new instrumentation, no
  third-party pixels.
- Tests: web unit (extend `repository.test.ts` usage) + a Playwright spec with the flag ON.
- Commit `feat: contract-derived collection pages (flag-gated)`.

### M6 — Weekly batch rehearsal (depends on M3, M4)
- Run one real weekly batch end-to-end through the DISTRIBUTION.md M6 runbook: 1–3 sessions →
  regenerated indexes → changelog + `v<yyyy.ww>` release (on the **private** repo) → the
  distribution touch **drafted, not posted**.
- Correct the runbook against measured reality (actual founder-minutes per step, what broke).
- Commit `docs: weekly publishing runbook corrected against a real batch`.

### M7 — Phase 5 prep, drafted only (depends on M6)
- **The artifact post is REDEFINED** (founder, 2026-07-21). PLAN.md Phase 5 item 4 currently
  specifies a side-by-side *plain transcript vs mdreel Markdown* demo. Replace it with an
  **answer-shaped artifact**: *"The State of &lt;topic&gt; &lt;year&gt; — based on N hours of talks
  from M public events"* — key themes, timeline, where speakers disagree, how positions evolved,
  **every claim carrying a timestamp link into the footage**, and the repository offered as the
  downloadable, portable evidence behind it. People wake up wanting answers, not repositories; the
  repository is what makes the answer checkable. The format demo becomes a section inside it, not
  the headline. Update PLAN.md Phase 5 item 4 in the same commit.
- **Authorship is a three-stage editorial workflow, not "the model writes it" and not "the founder
  types it"**: (1) the model extracts candidate observations from the corpus, each with evidence
  and timestamps; (2) claims are **verified**; (3) the model turns *verified observations only*
  into prose. Constraints on each stage:
  - **Split candidates by verification technology, not by confidence score.**
    **Quantitative claims** (counts, coverage, first-appearance, timeline shifts — *"mentioned in
    14 sessions, up from 3"*) are verified **programmatically against the §4b corpus** at ~zero
    cost and full coverage; they are also the legally safest layer (facts, not expression) and are
    only possible because the corpus is structured — **lead with these.**
    **Interpretive claims** (themes, disagreements, shifts in position) cost real founder minutes
    and assert an opinion in mdreel's name; keep them few and heavily checked.
  - 🚨 **Do not verify only the interesting claims.** Surprising and fabricated correlate — a
    hallucinated trend is more interesting than a real one. Verify every claim intended for
    publication **plus a random sample of the discarded ones**, and report the observed error rate.
    That rate is the number that decides whether Factory B is viable at all; without it the run
    produces a nice draft and zero knowledge.
  - **Stage 3 invariant: the prose may not contain a claim that is not in the verified list.**
    State this in the draft's own notes so a later run can check it.
- **Drawing only on `full`-tier material this run** (see the tier-3 note above). An unverifiable
  claim is cut, not softened.
- Also draft the Google Ads campaign (keywords, negatives, structure) for founder review.
  **Publish nothing. Spend nothing. Do not set T0.**
- Restate the open founder decisions in NEEDS-FOUNDER, including the GDPR consent decision.
- Commit `docs: phase 5 launch drafts for founder review`.

### M8 — Stretch, only if time remains
- `/app` library table on the real `/api/v1/jobs` list (carried backlog — needs a real-jobs UI
  model), or the per-tenant usage rollup on `/app/admin`.

## Definition of done — EVERY milestone, before its commit (CLAUDE.md rule 4)

build clean (warnings-as-errors) → `dotnet test --filter Category!=Live` green (compose up) →
`cd web && npm test` green → `scripts/e2e.sh up && scripts/e2e.sh full` green → `dotnet format`
(format only your files) → no secrets in diff → impacted living doc updated in the same commit →
`scripts/check-docs.sh` passes → commit to main with `feat:`/`fix:`/`infra:`/`docs:` prefix and
trailer `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` → push → deploy
affected services via `scripts/deploy.sh` → `scripts/smoke-remote.sh` stays green (currently 25/0).
`tests/Live/` runs before any deploy that touches the pipeline or prompts.

Machine quirks: run repo bash scripts via `& 'C:\Program Files\Git\bin\bash.exe' scripts/x.sh`
(plain `bash` is a broken WSL relay). Outbound TCP 5432 is blocked — DB checks go through the
Cloud SQL Auth Proxy pattern already in `smoke-remote.sh`.

## Final report (end of run, or at any hard stop)

Write to PLAN.md STATUS and reply with: milestones completed with commits + deployed revisions;
**sessions actually produced and the reconciled Vertex spend** (cite METRICS.md names — N4c/N4d/N7/N7c —
never restate figures); what failed and why; the complete NEEDS-FOUNDER checklist with exact
commands (licence review + visibility flip first); the monthly cost delta; and the ranked next-run
backlog. State plainly if the batch was cut and why.

Include three specific items:
- **The licence funnel**: candidate sessions considered → survived to `reference` → survived to
  `full` (CC-BY), and which topics had to be abandoned for lack of an eligible `full` core. This is
  the real constraint on the whole collections strategy and nobody has measured it yet. Report the
  `full : reference` ratio the collection actually shipped at.
- **The intent funnel, honestly scoped.** The chain that matters is: legally publishable → worth
  clicking → worth sharing → *"I wish I had this for my company."* Only the **last** step is
  instrumented today (`collection_convert_click` → signup, shipped last run) and it is the one that
  matters — it is the bridge from content to product. **Do not invent metrics for the middle
  steps**: with no public artifact and no traffic this run, clicks and shares are unmeasurable, and
  share counts risk landing on METRICS.md §7's anti-metric list. State what is instrumented, what
  will only be readable after launch, and change nothing else.
- **A one-paragraph input to the deferred founder session** on publishing cadence/intensity — what
  manufacturing one real collection taught you about what a weekly batch actually costs in founder
  time and euros. Observations only, no doc rewrites, no recommendation dressed as a decision.
