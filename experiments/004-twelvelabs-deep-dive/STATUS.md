# 004 — TwelveLabs deep-dive — STATUS / handoff

> 🧊 **Point-in-time memo, started 2026-07-20. Never authoritative** (`experiments/**` rule,
> CLAUDE.md). Supersedes nothing. If anything here conflicts with a living doc, the living doc
> wins. This is the ground truth for *what exists on disk* and *what's left* in THIS folder only.

This is the first of a series of per-competitor deep-dives; its structure is generalized into
`METHODOLOGY.md` (M6) so folders `005+` are a fill-in exercise. The shallow predecessor is
`experiments/002-competitor-analysis/dossiers/twelve-labs.md` — this folder goes 10× deeper.

## Progress

| Milestone | What | State |
|---|---|---|
| M0 | Scaffold + secrets guardrails | 🟡 in progress |
| M1 | Company intelligence → `dossier/01-company.md` | ⬜ pending |
| M2 | Docs corpus deep-dive → `02`, `03`, `05` | ⬜ pending |
| M3 | Playground hands-on → `04` + `assets/` | ⬜ pending |
| M4 | API hands-on → `api-captures/`, `code-samples/` | ⬜ pending |
| M5 | Synthesis vs mdreel → `06`, `07` | ⬜ pending |
| M6 | Methodology template + wrap-up → `METHODOLOGY.md` | ⬜ pending |

## Account / budget state
- Account: founder's real TwelveLabs account (free tier). Log in ONCE, keep the session.
- Plan / credit balance: _to be recorded in M3_.
- API keys generated this run: _0 so far_ (max 1, generated in M3).
- Free-tier credits consumed / remaining: _to be recorded in M4_.

## Secrets discipline (CLAUDE.md rule 1)
- Email, password, and API key appear in **no committed file** (not even inside a documented
  scan command — the literal secrets are never written to a tracked file). Key lives only in
  `.env.local` (gitignored). All captures/samples redact the key as `tlk_` + `***REDACTED***`.
- Pre-commit scan (run on every staged diff): pipe `git --no-pager diff --cached` into
  `Select-String` for the founder's password fragment, the founder's gmail address, and the
  TwelveLabs key prefix (the three literal patterns are kept only in the launch prompt in the
  session folder, never here) — any hit is a hard stop.

## NEEDS-FOUNDER
_(email verification codes, 2FA/CAPTCHA, payment-walled features, sales-gated tiers — nothing
blocks on these; document from public sources and list the exact required founder action here.)_

- _none yet._

## Living-doc contradictions found
_(report only — strategy docs are not rewritten autonomously, contract §5.)_

- _none yet._
