---
name: marathon-launch-prompt
description: >
  Author a launch prompt for a long (multi-hour) autonomous "marathon coordinator" run on
  mdreel. Use when asked to "create a launch prompt", "set up an 8h run", "prepare a marathon
  session", "make a coordinator prompt", or to turn a backlog of milestones into an autonomous
  overnight build run.
---

# Marathon launch-prompt author

Produces a single self-contained prompt that a fresh coordinator session executes unattended
for hours. The prompt is a **contract**, not a wish list: every guardrail explicit, every
decision pre-settled, every missing input pre-absorbed so nothing blocks and nothing idles.

`template.md` (next to this file) is the proven skeleton — fill it, don't reinvent it.
The 2026-07-17 run prompt it derives from delivered guardrails→auth→Umami→Stripe→jobs→website.

## Process

1. **Gather before writing.** Read PLAN.md STATUS, INFRA.md, METRICS.md (names only),
   ARCHITECTURE.md and CLAUDE.md. The prompt must cite current reality (deployed revisions,
   live costs, enabled APIs) — stale facts send the run down dead ends.
2. **Interview the founder** (ask_user, one question at a time) for anything the run could
   need and cannot invent: spend limits, credentials/API keys, DNS records, scope cuts,
   go/no-go on deploys. **Every input the founder cannot provide today becomes a
   NEEDS-FOUNDER item, never a blocker** — design each affected milestone to build to
   "one credential/DNS record from done".
3. **Settle tech decisions in the prompt, not in the run.** Anything debatable (library,
   provider, pattern) gets decided now and listed under "do not relitigate". A marathon that
   stops to weigh options burns its budget on deliberation.
4. **Write the milestone DAG.** Guardrails/spend caps are always M0 and gate everything.
   Order by dependency; mark parallel-safe milestones. Per milestone: concrete file-level
   scope, tests to add, commit prefix, which living doc to update.
5. **Emit the prompt** as a standalone markdown file the founder reviews and pastes into a
   fresh session (`/autopilot`, `/keep-alive`, raised `/limits`).

## Non-negotiable sections (see template.md)

- **Coordinator role** — sequences and verifies, delegates implementation to sub-agents,
  tracks SQL todos, commits per milestone (git history = crash-proof checkpoint), updates
  PLAN.md STATUS at every boundary.
- **Authorization contract** — explicit rule 5 override wording (this run only, recorded in
  INFRA.md), EU-only hard stop (rule 2), secrets discipline (rule 1), founder-approved spend
  ceiling, pre-approved billing surface, Stripe test-only, hard-stop list.
- **NEEDS-FOUNDER protocol** — exact command/DNS record appended to PLAN.md STATUS, then
  skip to next unblocked todo. Never idle, never guess credentials.
- **Settled decisions** — the "do not relitigate" list.
- **Definition of done per milestone** — CLAUDE.md rule 4 verbatim, incl. `scripts/e2e.sh
  full`, `scripts/check-docs.sh`, secret scan, living-doc update in the same commit, commit
  trailer `Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>`.
- **Machine quirks** — repo bash scripts via `& 'C:\Program Files\Git\bin\bash.exe' scripts/x.sh`
  (plain `bash` is a broken WSL relay); outbound TCP 5432 blocked (use the Cloud SQL Auth
  Proxy pattern from smoke-remote.sh).
- **Final report spec** — milestones/commits/revisions, failures + why, complete
  NEEDS-FOUNDER checklist, cost delta (METRICS.md names only), next-run backlog.

## Quality bar

- One fact, one home: the prompt cites METRICS.md numbers **by name**, never restates them.
- Every milestone must be independently committable and leave main green — worst case the
  run ends early with fewer milestones done, nothing half-broken.
- Riskiest milestones flagged honestly to the founder before launch.
- Deliver the prompt file path + a short summary; the founder fires it, not you.
