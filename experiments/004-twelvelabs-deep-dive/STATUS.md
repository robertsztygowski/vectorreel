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
| M6 | Methodology template + wrap-up → `METHODOLOGY.md` | ✅ done |

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

---

## FINAL REPORT — TwelveLabs deep-dive (run 004), 2026-07-20

### Milestones completed (all committed to main, pushed)
| M | Commit | Deliverable |
|---|---|---|
| M0 | `c6021b8` | Scaffold + secrets guardrails |
| M1+M2 | `2cdee42` | Company intel (`01`) + docs corpus (`02`,`03`,`05`) |
| M3 | `3b6b362` | Playground walkthrough (`04` + 8 screenshots) |
| M4 | `d1b3055` | API hands-on: `api-captures/*.json`, `code-samples/` |
| M5 | `d2901a3` | Synthesis + threat assessment (`06`,`07`) |
| M6 | _(this commit)_ | `METHODOLOGY.md` template + 002 pointer + final report |

### Top 10 strategically important findings
1. **TwelveLabs is US-HQ (SF) with no EU data residency** — its own DPA authorizes EEA→US SCC
   transfer; London is GTM-only. This is mdreel's single clearest attack surface (07 §4).
2. **They ship Google Analytics (`G-END0TB2RFD`) + HubSpot on their own playground** — the exact
   US-tracker pattern CLAUDE.md rule 10 prohibits; a screenshot-able differentiator (M3, 07 §4).
3. **Different buyer:** their ICP is media/sports/gov/security (corpus-scale search), not mdreel's
   EU-internal-KB/DPO/L&D buyer — not a direct competitor today (01, 07 §6).
4. **Different deliverable:** they sell search/embeddings *infrastructure*; mdreel sells a finished
   timestamped-Markdown *artifact*. No rendered-document product on their side (07 §1–§2).
5. **Realistic loss path is A2 build-vs-buy**, not displacement: their generous free tier (10
   video-hours) + $0.042/min API lets a technical stakeholder say "I could build this myself"
   (METRICS.md N21) (07 §3, §6).
6. **Free tier is far more generous than mdreel's trial credit (METRICS.md N33)** — real pressure
   on A2; founder may want to revisit N33 sizing (reported, contract §5).
7. **MCP-first onboarding** (Jockey MCP at `mcp.twelvelabs.io/jockey/mcp`) is a low-CAC distribution
   channel mdreel currently ignores — the main GTM lesson for A5 (06 §5).
8. **NVIDIA/AWS/Databricks/Oracle-backed**, model-velocity advantage — mdreel must never compete on
   model horsepower; compete on residency/artifact/buyer-fit/price (07 §5).
9. **Live API facts:** `x-api-key` auth; `/search`+`/embed` are multipart-only; `/analyze` is JSON +
   streams NDJSON unless `stream:false`; Marengo 3.0 512-dim; **Marengo 2.7 sunset confirmed live**
   (03, 05 §4).
10. **Latency benchmarks (60 s clip, free tier):** index ~45 s (~0.75× real-time), search ~0.7 s,
    analyze ~12.8 s, embed ~0.6 s — competitive reference points (05 §4).

### NEEDS-FOUNDER checklist
- **None.** No email codes, 2FA/CAPTCHA, payment walls, or sales-gated blockers were hit. Enterprise
  pricing is "contact us" (documented from public page, not pursued — contract §2).

### TwelveLabs credit / quota consumed vs remaining
- **240 s of 36,000 s (10 h) hard cap consumed (~0.7%)**; 1 video of 100; **~9.93 h remaining**.
  Well under the contract §3 50% stop threshold — no 2nd video needed. One evidence index retained;
  one orphaned empty index deleted. **1 API key generated** (3-mo expiry, in `.env.local` only).

### Living-doc contradictions
- **None overturning strategy** — deep-dive corroborates the EU-residency wedge, GDPR-as-feature,
  and the non-corpus-search non-goal. One nuance flagged (N33 free-tier sizing pressure), reported
  only.

### Recommended next competitors (folders 005+) — see METHODOLOGY.md §9
1. **Direct EU-angle video→transcript/summary SaaS** (same buyer) — highest A2 pressure; do first.
2. **RAG-over-video / knowledge-base platforms** that could add a document exporter.
3. **Meeting/video summarizers** (Fireflies/Otter-class) — adjacent buyer, watch for KB expansion.

### METHODOLOGY improvements after run 1
- Pre-write `code-samples/run_pipeline.py` skeleton + `05` observed-vs-documented stub in M0.
- Keep secret-scan patterns as *fragments* (never literals) and always include `%40`.
- Default file-writing sub-agents to `claude-haiku-4.5`; treat `gemini-3.5-flash` as unreliable.
- Budget ~1h for "first live API call succeeds" (provider input/auth/content-type quirks).
