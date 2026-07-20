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
| M0 | Scaffold + secrets guardrails | ✅ done |
| M1 | Company intelligence → `dossier/01-company.md` | ✅ done |
| M2 | Docs corpus deep-dive → `02`, `03`, `05` | ✅ done |
| M3 | Playground hands-on → `04` + `assets/` | ✅ done |
| M4 | API hands-on → `api-captures/`, `code-samples/` | ✅ done |
| M5 | Synthesis vs mdreel → `06`, `07` | ✅ done |
| M6 | Methodology template + wrap-up → `METHODOLOGY.md` | ⬜ pending |

## Account / budget state
- Account: founder's real TwelveLabs account (free tier). Logged in ONCE 2026-07-20, session kept.
- Plan: **Free Plan** — Jockey Free ($0/mo, 5 GB knowledge store) + Marengo & Pegasus PAYG gated by
  free caps: **10 video-hours** (Index + Analyze/Segment shared), **100 videos/index**, **10 hr
  max/index**, index expires **90 days** after creation. No payment method registered.
- Free-tier "credit" = the 10 video-hour allowance. **Consumed: 0 hr / 10 hr** as of M3 end.
- API keys generated this run: **1** (`mdreel-research-2026-07`, 3-mo expiry) → stored in `.env.local`.
- Free-tier consumed / remaining: **240 s used / 36,000 s (10 h) — ~0.7%**; 1 video / 100;
  9.93 h remaining. Well under the contract §3 50% stop threshold. One M4 index retained as
  evidence (`mdreel-m4-1784579548`); orphaned empty index from a failed first attempt was deleted.

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

- **No strategy-overturning contradictions.** The deep-dive corroborates the EU-residency wedge,
  GDPR-as-feature, and the "not a corpus-search platform" non-goal (dossier 07 §7).
- **One nuance (not a contradiction):** TwelveLabs' free tier (10 video-hours) is far more generous
  than mdreel's trial credit (METRICS.md N33), creating real pressure on A2 / willingness-to-pay.
  The founder may want to revisit N33's size against competitive free tiers. Reported only.
