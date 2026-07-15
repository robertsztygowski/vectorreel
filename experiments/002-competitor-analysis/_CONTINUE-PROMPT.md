# Continuation prompt — paste into a fresh Claude Code session (after spend resets)

---

Continue the mdreel competitor analysis in `experiments/002-competitor-analysis/`. A prior session
did most of it, then paused on a spend limit. **Do NOT restart from scratch and do NOT edit any
living doc** — this stays research-only; decisions get written into BUSINESS_MODEL/METRICS only
after the founder reads the results.

**Read first, in this order:** `experiments/002-competitor-analysis/STATUS.md` (ground truth for
what's on disk and what's left), then the living docs it references — CLAUDE.md, BUSINESS_MODEL.md
(§4 positioning, §5 ICP, §6 pricing, §8 risks), METRICS.md §1.2 (our COGS N6=€0.65/video-hour) +
§1.5–1.6, DISTRIBUTION.md — then skim `RESULTS.md`, `primary-source-notes.md`, and 2–3 existing
files in `dossiers/` to match their style and the §1–§8 template + Q/T/E evidence grading.

**⚠️ Persistence gotcha (bit the last session):** the Bash tool runs in an ephemeral sandbox that
discards files written into the repo tree on reset. Create any new directory with a
**sandbox-disabled** Bash `mkdir` first; write files with the Write tool into a dir that already
exists on real disk; then **verify with a `dangerouslyDisableSandbox` Bash `ls -la`/`wc -c`** before
trusting anything. The Playwright MCP and Write tool persist only if the parent dir is on real disk.

## Tasks, in priority order

1. **Finish the 3 dossiers that failed on the spend limit:** `zoom-ai-companion` and `google-meet`
   (substitute ring), `azure-ai-video-indexer` (infra). Same template + evidence grades. Cheapest
   path: the dossier workflow is resumable — the 17 done agents replay from cache for free:
   `Workflow({ scriptPath: "experiments/002-competitor-analysis/_dossier-workflow.js", resumeFromRunId: "wf_d2ff7bfb-764" })`
   (only the 3 failed agents will actually spend). Then extract their markdown to
   `dossiers/<slug>.md` (parse the workflow journal, as STATUS.md describes).

2. **Add dossiers for the 3 discovered direct-ring startups** (details in STATUS.md): **VideoDB**
   (public pricing → full dossier), **Coactive AI** (enterprise-only ~$75k/yr → lighter),
   **Flowstate** (no public pricing → lighter). Append them to the `COMPETITORS` array in
   `_dossier-workflow.js`, then resume the workflow (cached agents stay free).

3. **Build `MATRIX.md`** — the cross-competitor table the brief asked for. Rows: pricing entry
   point · effective €/video-hour at entry and at scale · on-screen-text capture · structured/
   Markdown output · API/webhooks/MCP · EU hosting · DPA · no-training term · free-trial shape ·
   primary distribution channel. Columns: every competitor (all 20 dossiers). Mark unknowns
   explicitly. **State the USD→EUR rate you use** (dossiers used 0.90) and keep it consistent.

4. **Reconcile `RESULTS.md` against the 20 dossiers.** It was written before the dossiers existed;
   substance holds, but cross-check every §2 pricing row and update any drift. Keep the
   "point-in-time, never authoritative" banner and the a–f structure (entry vs per-hour · EU-premium
   framing · on-screen-text wedge · DPO gaps · direct-competitor distribution · 2–3 two-plan pricing
   structures with survival math [contribution → accounts to cover N2 → CAC ceiling 3×N0] · top-3
   threats ranked).

5. **Fill missing pricing screenshots** into `assets/<slug>-pricing-2026-07-XX.png` (Playwright,
   full-page): tl;dv (their `/pricing` 404s — find the live URL), zoom, google workspace/meet, azure
   video indexer, google video intelligence, whisper/openai, gemini/vertex. Non-blocking.

## Guardrails (unchanged from the original brief)
- One dossier file per competitor; every claim graded **Q** (quoted from vendor's own page) / **T**
  (named third party) / **E** (estimate, state derivation); write "unknown" rather than guess.
- Comparable unit = effective €/video-hour = plan price ÷ included hours (overage separately).
- Screenshot pricing pages (they change silently).
- **Never say "transcription"** in positioning; anchor mdreel on structured-video / agent-citable
  documents. Don't edit living docs — hand the founder the memo and reconvene on pricing to re-pin
  N0/N1a/N23 in METRICS.

## The three findings already established (verify, don't rediscover)
1. On-screen text is **not** a unique wedge — Cloudglue, Twelve Labs, and Mixpeek all capture it.
   The wedge is the portable, spoken-vs-shown-separated, no-lock-in, EU-processed **Markdown
   artifact**.
2. **No one charges an EU premium** — infra APIs give EU residency free; EU-hosted SaaS price at/
   below US peers. EU is a DPO deal-unblocker, not a markup (caution on A1).
3. The default competitor is the **bundled meeting recap** (M365 Copilot). Anchor on asset video
   (demos/trainings/talks), not meetings.
