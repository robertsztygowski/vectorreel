# METHODOLOGY — reusable competitor deep-dive template

> 🧊 Point-in-time memo written 2026-07-20 after the **first** competitor deep-dive (004,
> TwelveLabs). Never authoritative; it is a *checklist and folder pattern*, not a strategy doc.
> Future competitor folders (`005-…`, `006-…`) should follow this so each is a fill-in exercise.
> Supersedes nothing. Update this file after each run with lessons learned (§8).

## 0. When to open a new competitor folder

One competitor = one numbered experiment folder: `experiments/NNN-<competitor-slug>-deep-dive/`.
Never mix two competitors in one folder. The shallow multi-competitor matrix
(`experiments/002-competitor-analysis/`) stays the index; each deep-dive is a successor linked from
002's row for that competitor (add a one-line pointer at the top of the shallow dossier — do not
otherwise edit frozen memos).

## 1. Folder structure (copy verbatim)

```
experiments/NNN-<slug>-deep-dive/
  STATUS.md            # status banner, progress table, NEEDS-FOUNDER, contradictions, account/budget
  METHODOLOGY.md       # optional per-run notes; the canonical template is THIS file (004)
  dossier/
    01-company.md              # funding, team, HQ/entity, residency story, customers, partners
    02-products-models.md      # model families, versions, capabilities, modalities
    03-api-reference-notes.md  # every API surface, auth, limits, SDKs, webhooks, changelog
    04-playground-ux.md        # hands-on UI walkthrough (screenshots)
    05-pricing-limits.md       # documented pricing + OBSERVED limits/latency from hands-on
    06-ecosystem-gtm.md        # distribution motion, partners, MCP/marketplace, content engine
    07-synthesis-vs-mdreel.md  # feature matrix, overlap/gap, pricing (cite METRICS names),
                               #   EU/residency attack surface, copy/avoid/crush, threat level,
                               #   living-doc contradictions (report only)
  assets/              # screenshots: <surface>-<YYYY-MM-DD>.png
  api-captures/        # raw (redacted) request/response JSON per endpoint exercised
  code-samples/        # runnable curl + Python, with actual outputs committed (README.md)
  .gitignore           # ignores .env.local, tmp/, *.mp4/*.mov/*.webm
  .env.local           # API key etc. — NEVER committed (gitignored)
```

## 2. Milestone sequence (dependency order)

| M | Name | Owner | Depends on |
|---|---|---|---|
| **M0** | Scaffold + secrets guardrails | coordinator | — |
| **M1** | Company intelligence | cheap research agent | M0 |
| **M2** | Docs corpus deep-dive | cheap research agent | M0 |
| **M3** | Playground hands-on (browser) | **coordinator only** | M0 |
| **M4** | API hands-on (budget-guarded) | coordinator | M3 (needs API key) |
| **M5** | Synthesis vs mdreel | coordinator | M1–M4 |
| **M6** | Methodology + wrap-up | coordinator | M1–M5 |

M1 and M2 are parallel-safe and delegate-safe. M3/M4/M5 stay with the coordinator (browser is one
sequential instance; API spend and synthesis need judgment). One or two commits per milestone,
direct to main, `docs:` prefix.

## 3. 🔒 Secrets rules (hard — non-negotiable, CLAUDE.md rule 1)

1. Credentials (login email, password, API keys) live **only** in `.env.local`, which is gitignored
   in M0 **before** any key is generated. Verify the ignore works: `git status` must never show it.
2. **Pre-commit secret scan of the staged diff** — a hard stop on any hit:
   ```powershell
   git --no-pager diff --cached | Select-String -Pattern '<pw-fragment>','<email-fragment>','<key-prefix>0','%40'
   ```
   - Include the **URL-encoded email** (`%40`) — the plain scan misses it in network captures.
   - Use a *fragment* of the password/email, never the literal, so the scan command itself is safe
     to commit (a scan pattern containing the full secret would itself leak it — this bit us in 004).
3. Redact every captured request/response and code sample: replace the real key with
   `<key-prefix>***REDACTED***`. A bare key *prefix* alone (e.g. `tlk_`) is a safe placeholder.
4. **Never screenshot a page showing a full API key.** Snapshot the DOM first, confirm the key is
   not rendered (or extract it via `browser_evaluate` into `.env.local`), only then screenshot.
5. Generate **exactly one** API key. Never enter payment details, upgrade, or click through checkout.

## 4. Model routing (cost discipline)

- **Delegate reading/searching** (M1 company, M2 docs) to cheap models via `task` with
  `agent_type: "research"`/`"explore"`.
- **Preferred cheap model: `claude-haiku-4.5`.** In 004, `gemini-3.5-flash` produced **0 useful
  turns** on a file-writing company-research task and had to be re-run on haiku. `gemini-3.5-flash`
  *did* succeed on the docs task — so it is usable but unreliable for "write the file" jobs.
  **Rule: if a cheap agent returns 0 useful turns or an empty file, do not re-launch the same
  config — fall back to `claude-haiku-4.5` or do it yourself.** Fallbacks: `gpt-5-mini`.
- **Coordinator (expensive model) keeps:** all browser/Playwright work, all live API calls, all
  synthesis, all quality gates, all commits.
- Don't spawn an agent for a lookup you can do in ≤5 direct tool calls.

## 5. Evidence discipline (every claim is sourced)

- Every claim in a dossier cites **either** a URL + date **or** an asset/capture filename.
- Grades: **Q** = primary/official source, **T** = third-party, **O** = observed hands-on.
- Each dossier ends in an **Evidence log** table: `# | Claim | Source | Checked (date) | Grade`.
- Speculation is explicitly labeled **[SPECULATION]** and never entered into an evidence log as fact.
- Numbers that belong to mdreel's own strategy are **cited by METRICS.md name (N-id), never
  restated** — `scripts/check-docs.sh` only guards living docs, but keep the habit in experiments.
- If findings contradict a living doc: **report only** in `07 §7` + STATUS.md; never rewrite
  strategy docs autonomously (contract §5).

## 6. Hands-on rules (M3/M4)

- **Browser:** log in ONCE, keep the session. Screenshots save to repo root, then `Move-Item` to
  `assets/`. Name `<surface>-<YYYY-MM-DD>.png`. Capture `browser_network_requests` to infer the
  internal API shape.
- **API budget:** free-tier credits only. Read the account's quota endpoint first; index ≤2 short
  videos (≤5 min); **stop at 50% of any credit balance** and record the remainder.
- **Test video:** a short CC-licensed clip (Big Buck Bunny cut) to a gitignored `tmp/`, never
  committed. Watch provider minimums (TwelveLabs rejected <360×360 — re-encode with ffmpeg).
- **Respect ToS:** documented API usage and normal browsing only; no scraping at scale, no
  rate-limit probing. mdreel sells compliance — behave like it (CLAUDE.md rule 8 spirit).
- Capture full **redacted** request/response pairs to `api-captures/`; **time every operation**
  (indexing/search/analyze latency is competitively important).

## 7. Definition of done (per milestone, docs-only run)

Every claim sourced → screenshots/captures exist at referenced paths → **staged-diff secret scan
passes** → `& 'C:\Program Files\Git\bin\bash.exe' scripts/check-docs.sh` passes → STATUS.md updated
in the same commit → commit to main with `docs:` prefix and the
`Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>` trailer → push. Stage only
the experiment folder's paths (`git add experiments/NNN-…`) — never sweep unrelated working-tree
changes into a docs commit.

## 8. Lessons from run 004 (feed these into the next run)

- **`gemini-3.5-flash` is unreliable for file-writing sub-agent tasks** — default to
  `claude-haiku-4.5` for "research and write the dossier file" jobs (§4).
- **The secret-scan command itself must not contain full literals** — use fragments, or the scan
  line leaks the secret it's meant to catch (caught and fixed in 004 M0).
- **URL-encoded email (`%40`) leaks into network-capture text** — add it to the scan pattern.
- **Provider input constraints bite** — TwelveLabs needs ≥360×360 video, `x-api-key` (not Bearer),
  multipart for `/search` + `/embed` but JSON for `/analyze`, and `stream:false` to avoid NDJSON.
  Budget an hour for "make the first call actually succeed."
- **Quota is metered per engine-family per operation**, not one wall-clock pool — read the usage
  endpoint before *and* after to report true consumption.
- **Next-run improvement:** pre-write the `code-samples/run_pipeline.py` skeleton in M0 so M4 is
  purely filling in provider specifics; and add a `05` "observed vs documented" table stub in M0.

## 9. Recommended next competitors (folders 005+)

Prioritize by overlap with mdreel's *actual* wedge (EU residency + video→document artifact +
non-developer buyer), not by raw fame:
1. **Direct video→text/transcript tools with an EU angle** (e.g. transcript/summary SaaS that
   markets to compliance) — closest to mdreel's buyer; highest A2 pressure.
2. **RAG-over-video / knowledge-base platforms** that could add a document exporter.
3. **General meeting/video summarizers** (Fireflies/Otter-class) — adjacent buyer, watch for
   knowledge-base expansion.
Pick #1 first: it stress-tests the residency wedge against a same-buyer competitor.
