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

## M2 — Per-candidate evidence (5 Haiku evidence passes; URLs in sources.md)

Each candidate got one Haiku fetch/extract pass (demand / competition / buyer-channel / pipeline-delta).
Load-bearing claims carried to M3 for adversarial verification are tagged **[→M3]**.

### C1 — Regulatory-training audit-trail docs
- **Demand:** Real regulatory pull — GDPR training records, EU AI Act (Aug-2026 high-risk logging), OSHA/EEOC/HHS inspectors increasingly ask "show the actual content, per-person per-date." Compliance-training market ~$5.63B, 8.6% CAGR. **[→M3: "regulators/buyers actually demand a *verbatim-content* audit record"]**
- **Competition:** LMS incumbents (Docebo ~$25k/yr, Absorb, iSpring $2.29/user/mo, TalentLMS $69/mo, SkyPrep $319/mo) all log **who/when/score** via xAPI/SCORM — **none produce a verbatim spoken+on-screen content record.** Genuine feature gap.
- **Buyer & channel:** ⛔ **Procurement-gated.** 60–120-day RFP cycle, weighted scorecards, multi-stakeholder, vendor-consolidation dynamic. **No self-serve inbound behaviour found.** **[→M3: "compliance officer reachable self-serve, no outreach"]**
- **Pipeline delta:** A credible *audit* product likely needs tamper-evidence / signed records / retention export — **net-new build** not in the current pipeline.
- **Verdict-hint (worker):** strong regulatory wallet, but incumbents own the buyer via procurement and **the verbatim-content gap is one no regulator is actually asking for.**

### C2 — Conference/webinar → agent-citable knowledge base ⭐ contender
- **Demand:** ✅ **Strongest of the five.** Active, current RAG-for-video deployment (Fortune-500 shift to unified video knowledge; Qdrant/AWS/Vimeo "video transcript → RAG" tutorials; arXiv 2602.15859 "From Transcripts to AI Agents"). GraphRAG indexing cost collapsed ~1000× (2024→2025), removing the cost barrier. **[→M3: "buyers will PAY for this vs build it themselves"]**
- **Competition:** ScreenApp ($19/mo) exports Markdown but **no schema, no slide-vs-spoken separation**; Otter/Fireflies trap output in-platform, US-hosted (Otter zero EU residency). Twelve Labs = JSON API, no portable doc. **None ship schema-consistent, portable, slide-separated Markdown for RAG citation** — the wedge is unoccupied. Google **Open Knowledge Format** (Jun-2026: dir of `.md` + YAML frontmatter, no lock-in) legitimizes exactly mdreel's output shape.
- **Buyer & channel:** ✅ Inbound/community fit (DEV.to tutorials, GitHub-repo proof, KM conferences, SEO). **mdreel's already-planned curated CC gallery of processed conference talks is a near-perfect channel match** (DISTRIBUTION.md). **[→M3: "buyer reachable self-serve/inbound"]**
- **Pipeline delta:** ✅ **Essentially zero** — matches current output. Cross-video search index + cross-talk linking are *optional* net-new upsells, not required for launch.
- **Verdict-hint (worker):** unoccupied portable-Markdown wedge + real tailwind; risk is **bottom-up DIY / unclear willingness-to-pay.**

### C3 — Accessibility (EAA/WCAG) docs for internal video
- **Demand/regulatory:** 🚨 **Scope kill:** the EAA (effective 28-Jun-2025) covers **external-facing** audiovisual media tied to covered products/services — **internal employee training video is NOT in scope.** The regulatory pull that made this candidate attractive does not reach the target content.
- **Competition:** Rev ($1.99/min human ≈ $119/h; $0.25/min AI ≈ $15/h), 3Play (quote-based), Verbit. Captioning is perceived as a **high-touch premium OR commodity** service anchored to the transcription shelf — mdreel's richer output doesn't obviously command more.
- **Buyer & channel:** procurement/vendor-list driven; limited self-serve signal.
- **Pipeline delta:** 🚨 **Net-new build:** WCAG captions need **WebVTT/SRT/TTML**, not a Markdown document — a caption-format renderer mdreel does not have. Dents the SB gate.
- **Verdict-hint (worker):** regulatory hook doesn't reach internal video + output-format mismatch. **Killed on evidence.**

### C4 — Product/SDK docs from demo & talk videos (DevRel) ⭐ contender
- **Demand:** Real (62% of devs prefer long-form video; DevRel teams seek video→docs efficiency; "YouTube tutorial → knowledge base" a stated 2026 pattern). **But the observed demand is for human-facing illustrated how-to (Docsie/Vidocu/Castify), not proven demand for *agent-citable* output.** **[→M3: "buyers value agent-citable Markdown over human how-to guides"]**
- **Competition:** Docsie, Vidocu, Castify, ScreenApp = human-facing illustrated guides; Mintlify = text-first, video-supplementary. **None produce agent-citable spoken-vs-shown Markdown** — wedge is real but narrow. None mention EU residency.
- **Buyer & channel:** ✅ Excellent inbound fit — MCP + llms.txt + technical blog + HN (mdreel already ships MCP/llms.txt). **But** 🚨 **DIY threat is HIGH:** Gemini natively does video→timestamped chapters; a devtool team can wire Gemini/Claude + MCP → Markdown in ~1 hour at ~$0. This buyer is the one *most* able to build it. **[→M3: "the DIY threat does not kill willingness to pay (A2)"]**
- **Pipeline delta:** small (code-language detection, agent-native `llms-full.txt`/SKILL.md emit are optional polish).
- **Verdict-hint (worker):** unoccupied agent-citable wedge + perfect channel, but **the sharpest A2/DIY exposure of any candidate** and no proven preference for agent-ready output.

### C5 — White-label RAG-ingestion for AI consultancies
- **Demand:** 🚨 **Thin/none found.** Consultancies (Keyhole, RaftLabs, N-iX…) **build** ingestion in-house, bundled into delivery; no public "we'd buy a video-ingestion API" signal. Enterprise-RAG tailwind exists but ingestion isn't shopped separately.
- **Competition:** Twelve Labs (no EU clarity), Mixpeek (single-tenant + regional residency — already 2/3 of the EU/no-lock-in gap), Cloudglue (MCP). **None advertise resale/white-label terms** — a real gap, but an unproven-demand one.
- **Buyer & channel:** 🚨 **No self-serve demand; a reseller/partnership motion is required — and outreach is ruled out (DISTRIBUTION.md).** Founder's audience exists but shows no inbound sourcing of white-label video APIs.
- **Pipeline delta:** net-new white-label portal + wholesale licensing + multi-tenant sub-accounts.
- **Verdict-hint (worker):** real EU/resale gap, but **zero inbound demand + needs a ruled-out sales motion.** Killed on A5.

## M3 — Adversarial verification (5 Sonnet skeptics, prompted to refute)

Methodology: a claim that **survives** a genuine refutation is *verified*; one that does **not** is
downgraded to *speculative*. Skeptics defaulted to `refuted=true` on thin evidence. C3 and C5 got no
Sonnet pass (deliberate cost cut, logged in run-log) — their killing facts are already documented.

| Candidate | Load-bearing claim tested | Verdict | Conf. | The refutation that stuck |
|---|---|---|---|---|
| **C1** | Compliance buyer reachable self-serve, no outreach | **REFUTED** | med-high | Regulated-industry ICP sits in the RFP/procurement tier; self-serve pricing exists only for full-suite LMS, not point-tool add-ons. **Fails the no-outreach constraint outright.** |
| **C2** | Buyers will **pay** vs DIY | **REFUTED** | med | Evidence is DIY "how-to-build" tutorials, not buy-demand; Google OKF (free, vendor-neutral) **commoditizes** the schema-Markdown output mdreel would charge for. |
| **C2** | Buyer reachable inbound/self-serve | **REFUTED** | med | Founder's .NET/architecture audience ≠ KM buyers; KM tooling skews committee/procurement (SSO/RBAC/residency gates); gallery ranking on processed transcripts is unproven (derivative-content risk). |
| **C4** | Buyers prefer **agent-citable** over human how-to | **REFUTED** | med | Funded competitors (Docsie/Vidocu/Castify) all target human-facing guides — the revealed preference; agent-citable demand is only indirect. |
| **C4** | DIY threat survivable (A2 holds) | **REFUTED** | **high** | For public DevRel content EU residency is moot → pitch collapses to DIY-able chunking; bursty launch-cycle corpus = A3 **backfill**, not subscription. |

**Result: every tested load-bearing claim was refuted.** No candidate has a *verified* demand-AND-
reachability story. C1/C4 are killed (A5 procurement / A2-DIY). C2 survives only as the least-weak
option, with both pillars **speculative**. C3/C5 killed on documented facts.

## 3. Per-candidate one-pagers
*(consolidated at M5 from M2 evidence + M3 verdicts + M4 scores)*

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
