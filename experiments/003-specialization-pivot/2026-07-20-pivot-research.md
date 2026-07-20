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
- **M0** — frame + rubric — ✅
- **M1** — candidate long-list (14) + shortlist (5) — ✅ (see §4 + §M1 below)

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

## M1 — Candidate generation (2 Haiku sweeps → long-list → shortlist)

Two Haiku discovery sweeps ran (pipeline-outward; demand-inward with URLs). Combined long-list = **14**
candidates. A useful finding up front: **the F1–F4 constraint filter passes almost everything** — nearly
every candidate reuses the pipeline, is EU-hostable, needs no YouTube bytes, and isn't meetings. So the
real cuts are **strategic**, made against the competitor test (002) and the weak-path fact (screen
recordings are the pipeline's weakest category — METRICS.md N32/N7b). Kills are in §4.

### The shortlist (5) — carried to M2 evidence

| # | Candidate | The one specialized job | Buyer | Main content type | Nearest competitor |
|---|---|---|---|---|---|
| **C1** | **Regulatory-training audit-trail docs** | Timestamped, speaker-attributed, verbatim-on-screen proof-of-content records from recorded compliance/regulatory training, for audit | Compliance officer / L&D in regulated EU industries (finance, health, pharma, insurance) | slide talk, talking head (**strong path**) | LMS + manual transcript; no purpose-built product found |
| **C2** | **Conference/webinar → agent-citable knowledge base** | Turn an org's recorded talks/webinars into one schema-consistent, timestamped Markdown KB an agent can cite | Knowledge/research manager at event-heavy orgs, analyst/consulting firms, associations | slide talk (**pipeline's strongest, N30**) | ScreenApp, DistillNote, DigestAI (summarizers) |
| **C3** | **Accessibility (EAA/WCAG) docs for internal video libraries** | WCAG-compliant transcript + captions + on-screen-text records for internal video, meeting the EU Accessibility Act | Accessibility/compliance coordinator, HR/IT, public sector | mixed / any | 3Play Media, Rev, Amberscript (transcription shelf) |
| **C4** | **Product/SDK docs from demo & talk videos (DevRel)** | Convert devtool demo/talk recordings into agent-citable product-docs + code-sample Markdown | DevRel / technical PM at devtool & SaaS companies | demo + slide (**mixed; some weak-path**) | Docsie, Vidocu, Castify (human-facing how-to) |
| **C5** | **White-label RAG-ingestion for AI consultancies** | Sell the pipeline as a per-client video→RAG-corpus ingestion step consultancies resell inside delivery | AI consultancy / software house building client RAG (ICP #3, sharpened) | mixed | Cloudglue/Twelve Labs (platform, not white-label) |

**Baseline to beat:** *"keep the current wedge"* (video→Markdown for any EU AI KB, launch as planned).
A candidate only wins if it beats that baseline on the rubric (§0). The null result is valid (M4).

## 4. Candidates killed at M1 (long-list → shortlist)

Killed on strategy/rubric (all technically pass F1–F4 unless noted):

| Killed candidate | Why killed | Rule it fails |
|---|---|---|
| **Screencast → illustrated how-to guides** (Docsie/Vidocu/Castify/ScreenApp cluster) | Output is a **human-facing** how-to with screenshots — abandons the agent-citable-Markdown wedge — and rides the **weakest content type** (screen recording, N32). Crowded with funded US tools. | Competitor test (wedge); A4 (weak path) |
| **Customer-support FAQ from demos** | Same cluster; human-facing help articles, weak path, US-tool-dense. | Competitor test; A4 |
| **Internal process runbooks from screencasts** | Same how-to-guide shape; screen-recording path (N32); no EU driver. | A4; A1 |
| **User-research / ResOps session indexing** | Entrenched US incumbents (Dovetail, UserTesting); screen-recording heavy (weak); EU not a purchase driver for this buyer. | A1; A4; A2 |
| **Sales-pitch / enablement documentation** | Drifts toward call-recording (Gong) territory adjacent to the meetings no-go; sales-ops buys platforms not documents (A2). | A2; competitor-test proximity to threat #1 |
| **Patent-disclosure timestamping** | Buyers are consultant-/lawyer-mediated, not self-serve inbound; niche too thin for N15. | **A5**; N15 |
| **Video accessibility as generic transcription** (merged into C3, kept only in its regulated/EAA framing) | Generic captioning files us on the €2–4/h transcription shelf (002 §3) where we lose on price; only the **regulatory (EAA)** framing survives. | Competitor test (shelf); N23 |
| **"Internal video KB / employee-onboarding handbooks" (generic)** | Too close to the **current wedge** to count as a specialization — folded into the baseline and into C2/C5. | Not a pivot (scope) |

## 5. What would change this verdict — the cheapest confirming experiment
*(filled at M5; framed like METRICS.md §1.6 gated experiments)*
