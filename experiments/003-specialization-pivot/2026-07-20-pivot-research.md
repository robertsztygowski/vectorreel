# 003 — Specialization Pivot Research: which adjacent specialized tool, or keep the wedge?

> 🧊 **STATUS: POINT-IN-TIME MEMO — NEVER AUTHORITATIVE.** Written **2026-07-20**. A reasoning
> trail, not instructions. Competitor facts, demand signals and search-volume reads were true on the
> date each was checked and **go stale fast**. **If this memo ever contradicts a living doc, the
> living doc wins** (CLAUDE.md). No decision here is binding until the founder reads it and writes it
> into BUSINESS_MODEL.md / METRICS.md / PLAN.md. **This memo edits no living doc, no `src/`, no
> `web/`, no `infra/`, no `scripts/`.**
>
> **Scope (founder decision 2026-07-20):** *adjacent specialized tool* — reuse the existing
> video→timestamped-Markdown pipeline (ARCHITECTURE Stages A–D, Vertex, EU residency) unchanged;
> reshape only the **product/job** into something more specialized than "video→Markdown for any EU AI
> knowledge base." A candidate needing a different pipeline, non-EU processing, or YouTube-byte
> download (CLAUDE.md rule 8) is out of scope by construction.
>
> **Every load-bearing number cites a source URL + date, or is a METRICS.md / BUSINESS_MODEL N-code
> cited by name — never restated. Uncited demand claims are labelled ASSUMPTION or speculative.**

---

## NEEDS-FOUNDER
*(exact questions only the founder can answer; nothing in this run blocks on them — provisional
answers are marked ASSUMPTION inline)*

- *(accumulates through the run)*

---

## Progress / run checkpoints
- **M0** — frame + rubric — ⏳ in progress

---

## 0. The rubric — mdreel's own decision rules as a scoring table

**The rubric is mdreel's own decision rules, cited by name. No new success metrics are invented.**
Each candidate is scored **High / Med / Low** (or the stated verdict) against every column. The
number/rule behind each column lives in the cited doc — never restated here.

### 0.1 The constraint filter (pass/fail gate — applied BEFORE scoring)

A candidate must pass **all four** or it is killed with a one-line reason and never scored:

| Filter | Question | Kill if |
|---|---|---|
| **F1 — Pipeline reuse** | Does it reuse Stages A–D (ARCHITECTURE §3) essentially unchanged? | Needs a materially different pipeline |
| **F2 — EU-only** | Does delivery stay EU-resident, no US-hosted processing/tracking (CLAUDE.md r2/r10)? | Depends on US processing |
| **F3 — No YouTube bytes** | Ingestion via upload or `fileData.fileUri` only (CLAUDE.md r8)? | Needs byte download / yt-dlp / scraping |
| **F4 — Not meetings** | Anchored on asset video, never the meeting-recap job (BUSINESS_MODEL §8 threat #1; experiments/002 §5)? | Drifts toward meetings/Teams/Zoom |

### 0.2 The scoring columns (each = one named decision rule)

| Col | Rule (cited by name) | What **High** means | What **Low** means |
|---|---|---|---|
| **A5** | Distribution — **top risk** (METRICS.md A5; DISTRIBUTION.md) | Buyer reachable **self-serve/inbound, no outreach**; a clear cheap channel exists | Requires outreach / a channel we don't have |
| **A1** | EU residency a **purchase driver** for *this* buyer (METRICS.md A1) | Residency is a real deal-unblocker for this niche's buyer | EU is irrelevant to the buyer |
| **A2** | Buyers **buy** vs DIY (METRICS.md A2) | Buyer can't/won't build it themselves | Buyer is the exact audience able to DIY |
| **A3** | Recurring **flow** vs one-time **backfill** (METRICS.md A3) | Naturally recurring ingest → subscription | One-time archive conversion → credit-pack only |
| **A4** | Output trusted/**citable** (METRICS.md A4) | Pipeline's measured-strong categories (N30/N31) fit this job | Relies on the weak screen-recording path (N32/N7b) |
| **N1a** | ~2–3 retained accounts cover infra (METRICS.md N1a/N0) | Price point + buyer supports N0-scale contribution | Buyer only bears an impulse price → many accounts to survive |
| **N15** | Traffic-to-survival reachable for this niche's channels (METRICS.md N15) | Niche has a good-post-sized reachable audience | Audience too thin or diffuse to reach N15 |
| **N23** | CAC ceiling / on-ramp canyon (METRICS.md N23; BUSINESS_MODEL §6) | Contribution supports a workable CAC ceiling + sane on-ramp | Collapses the CAC ceiling / worsens the canyon |
| **N16–N19** | Bottom-up TAM — bootstrappable, not a VC swing (METRICS.md N16–N19) | TAM big enough to bootstrap, small enough to own | Too small to survive, or needs VC scale |
| **SB** | Ship-by gate — fits **before** go-to-market (METRICS.md §2.2 SB = 2026-09-15) | Zero/near-zero net-new build; positioning-only | Needs new build that blows the deadline |

### 0.3 The competitor test (experiments/002 — every candidate must clear it)

From experiments/002-competitor-analysis (point-in-time, 2026-07-15):
- **OCR is table-stakes** — never the wedge (002 §7).
- **EU residency is unpriceable but a deal-unblocker** — not a markup (002 §6/§8).
- **The bundled meeting-recap owns meetings** — never pivot toward meetings (002 §5, threat #1).
- **The durable moat is no-lock-in / portable artifact** (002 §7, BUSINESS_MODEL §4).

A pivot that walks into **threat #1** (meetings) is dead on arrival — say so explicitly.

---

## 1. Executive verdict
*(filled at M5)*

## 2. The ranked table
*(filled at M4/M5)*

## 3. Per-candidate one-pagers
*(filled at M2–M5)*

## 4. Candidates killed by the constraint filter
*(filled at M1)*

## 5. What would change this verdict — the cheapest confirming experiment
*(filled at M5; framed like METRICS.md §1.6 gated experiments)*
