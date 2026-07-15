# 002 Competitor Analysis — STATUS / handoff (paused 2026-07-15)

> 🧊 Point-in-time, never authoritative. Research **paused mid-run** because the Claude account hit
> its monthly spend limit. This file is the ground truth for *what exists on disk* and *what's left*.
> No living doc was edited (per the brief).

## What happened
- Two workflows ran. (1) A deep-research fan-out (107 agents) → verified claim log. (2) A
  per-competitor **dossier** workflow (20 dossier agents + 1 discovery). **17 dossiers completed;
  3 failed on the spend limit**; discovery returned 3 extra direct-ring startups (no dossiers yet).
- ⚠️ **Persistence gotcha (fixed):** the Bash tool runs in an ephemeral sandbox — files created via
  a *sandboxed* `mkdir`/write are discarded on reset. Everything here was (re)written to **real
  disk** (Write tool into a real-disk dir, or Bash with the sandbox disabled) and verified with
  byte counts. If you add files, create parent dirs with a **sandbox-disabled** Bash `mkdir` first.

## On disk now — `experiments/002-competitor-analysis/`
| File | What it is | State |
|---|---|---|
| `RESULTS.md` | One-page synthesized deliverable (answers a–f: entry vs per-hour, EU premium, on-screen-text wedge, DPO gaps, distribution, 3 pricing structures + survival math, top-3 threats) | ✅ complete (built from primary sources + deep-research, **not** yet reconciled against the 17 new dossiers) |
| `primary-source-notes.md` | Raw verified extracts I pulled directly (Playwright/WebFetch) | ✅ |
| `workflow-verified-claims.txt` | 70 adversarially-verified claim rows from the deep-research run | ✅ |
| `dossiers/*.md` | **17 evidence-graded competitor dossiers** (template §1–§8, Q/T/E grades) | ✅ 17 of 20 |
| `assets/*-pricing-2026-07-15.png` | Pricing-page screenshots (13) | ⚠️ partial (see gaps) |
| `_dossier-workflow.js` | The dossier workflow script — **resumable** (see below) | ✅ kept on purpose |

### The 17 dossiers present
direct: **cloudglue, twelve-labs, mixpeek** · adjacent: **happy-scribe, amberscript, sonix, rev,
otter, descript, tl-dv, fireflies** · infra: **assemblyai, deepgram, whisper, vertex-gemini,
google-video-intelligence** · substitute: **microsoft-365-copilot**

### Screenshots present (13) in `assets/`
cloudglue, twelve-labs, happy-scribe, amberscript, assemblyai, deepgram, otter, fireflies, sonix,
rev, descript, mixpeek, microsoft-365-copilot (viewport only — page too long for full-page).

## ❌ Gaps — what's NOT done
1. **3 dossiers failed on the spend limit** (no file): `zoom-ai-companion` (substitute),
   `google-meet` (substitute), `azure-ai-video-indexer` (infra).
2. **3 discovered direct-ring startups have NO dossier yet** (discovery output below):
   **VideoDB** (public pricing — do a full dossier), **Coactive AI** (enterprise-only, ~$75k/yr
   reported via AWS Marketplace/3rd-party — light dossier), **Flowstate** (no public pricing — light).
3. **Missing pricing screenshots:** tl;dv (their `/pricing` 404s — find the live URL),
   zoom, google workspace/meet, azure video indexer, google video intelligence, whisper/openai,
   gemini/vertex. (Non-blocking; dossiers carry the numbers.)
4. **MATRIX.md not built yet** — the cross-competitor comparison table the brief asked for.
5. **RESULTS.md not reconciled** against the 17 dossiers (it predates them; substance still holds,
   but cross-check the dossier §2 pricing rows before pinning anything).

## Discovery result (verbatim, from the workflow) — direct-ring startups beyond Cloudglue/TwelveLabs/Mixpeek
- **VideoDB** — https://www.videodb.io — "Database/infrastructure layer that indexes video (spoken +
  visual) and serves it as searchable, LLM-ready context for RAG and agents via SDK." **Public
  pricing: yes.**
- **Coactive AI** — https://www.coactive.ai — "Multimodal AI platform that ingests video/images and
  outputs structured, searchable metadata and embeddings." **Public pricing: no** (enterprise
  contract; ~$75k/yr reported via AWS Marketplace/third parties).
- **Flowstate** — https://www.flowstatehq.com — "Video intelligence platform doing frame-level,
  schema-driven extraction of objects/scenes/speakers/topics into structured, queryable, LLM-ready
  data with semantic search and NL Q&A." **Public pricing: no.**
- Rejected as poor fits: **Vidrovr** (acquired by CesiumAstro Feb 2026, pivoted to defense/smart-city),
  **Reducto** (document-parsing, video not core).

## 🔑 Headline findings so far (detail in RESULTS.md; each backed by a dossier + evidence log)
1. **On-screen text is NOT a clean wedge.** Cloudglue ("Scene Text Recognition"), Twelve Labs (OCR
   in the visual model), **and Mixpeek** ("On-screen text (video OCR) +$0.10/min", verified live)
   all do it. The wedge is the **deliverable** — one portable, timestamped Markdown file, spoken-vs-
   shown separated, schema-consistent, no retrieval lock-in, clean EU processing — not the OCR.
2. **Nobody charges an EU premium.** AssemblyAI ("EU region is the same price as US") and Deepgram
   (EU endpoint `api.eu.deepgram.com`) give EU residency **free**; EU-hosted SaaS (Happy Scribe,
   Amberscript) price at/below US peers. ⇒ EU residency is a **DPO deal-unblocker, not a markup**
   (caution on A1). Amberscript's "EU-only / no-training" claim **leaks** (non-EEA transcribers +
   Summary→US LLM; no-training is marketing-FAQ, not a contract term) — verified.
3. **The real default competitor is the bundled recap** (M365 Copilot, from $18/user/mo, already in
   the tenant, already DPO-approved). mdreel must anchor on **asset video** (demos/trainings/talks),
   never meetings.
4. **Effective-price bands (all verified, per video-hour):** infra €0.28–0.46 · EU transcription
   €0.59–4.25 · structured video (Cloudglue) **$4.80–7.50** ($/h; dossiers convert at USD→EUR 0.90,
   so €4.32–6.75). mdreel's hypothesised €4.60–5.96/h sits **in Cloudglue's band** — defensible only
   if the buyer files us as structured-video, not transcription.

## ⚠️ Data-quality notes for whoever continues
- Dossiers use **USD→EUR 0.90**; `RESULTS.md` mostly keeps native USD. Pick one convention when you
  build MATRIX.md and state it.
- Cloudglue dossier §1 adds detail not independently re-verified by me: founders (Amy Xiao CEO;
  Matt Pua, Kevin Dela Rosa per Tracxn), **YC S24**, **~4 employees**, MCP repo ~6 stars, one named
  customer (**11x**). All graded T/E in the dossier — treat as third-party, not gospel.
- The em-dash in some dossier headers renders as `?`/`�` in the Git Bash console but the files are
  valid UTF-8.

## ▶️ Cheapest way to finish (once spend resets)
The dossier workflow is **resumable** — the 17 done agents replay from cache for free; only the 3
failed ones (+ any you add) actually spend:
```
Workflow({ scriptPath: "experiments/002-competitor-analysis/_dossier-workflow.js",
           resumeFromRunId: "wf_d2ff7bfb-764" })
```
To add the 3 discovered startups, append them to the `COMPETITORS` array in that script first, then
resume. See `_CONTINUE-PROMPT.md` for the full next-session brief.
