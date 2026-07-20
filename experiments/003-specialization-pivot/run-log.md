# 003 Specialization Pivot — run log

> Milestone/commit checkpoints + the cost contract this run is held to. Point-in-time; never
> authoritative.

## Cost contract (fixed — the point of this run)

| Work | Who | Model |
|---|---|---|
| Sequencing, judgment, scoring, synthesis, ranking | Coordinator (this session) | Fable 5 / Opus |
| Candidate discovery, web search, fetch, first-pass extraction | Sub-agents | **Haiku 4.5** (`claude-haiku-4-5-20251001`) |
| Adversarial claim verification, per-candidate scoring | Sub-agents | **Sonnet 5** (`claude-sonnet-5`) |

**Rules enforced:**
- Never spawn an Opus/Fable sub-agent for search/fetch/extraction — the exact cost bug this run avoids.
- Fan-out caps: discovery ≤2 Haiku sweeps; per candidate ≤3 Haiku fetch/extract + ≤2 Sonnet verifiers.
- Every sub-agent prompt carries complete context (they share no memory) and asks for raw structured data.
- Deliberately-dropped coverage is logged here and in the memo — no silent truncation.

## Model-call tally (proof the contract held)
| Milestone | Haiku calls | Sonnet calls | Coordinator (Fable) |
|---|---|---|---|
| M1 discovery | 2 | 0 | dedupe+shortlist |
| M2 evidence | | | |
| M3 verification | | | |
| **total** | | | |

## Checkpoints
- **M0** — frame + rubric — folder + memo skeleton + this log created. ✅
- **M1** — 2 Haiku sweeps (pipeline-out + demand-in), 14 long-list → 5 shortlist (C1–C5), 8 killed. ✅
