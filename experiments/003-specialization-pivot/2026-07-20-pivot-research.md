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
*(exact questions only the founder can answer; nothing in this run blocked on them)*

1. **Adopt the C2 launch-positioning recommendation?** Should the Phase 5 gallery curation + one
   landing headline variant bias toward *"turn your talk & webinar library into an agent-citable
   knowledge base"* (§1, §3a)? This is a positioning choice, not a pivot — it reuses the wedge and
   adds no build. **ASSUMPTION taken in this memo:** worth testing, because it costs only curation
   hours and rides the already-funded T-LAUNCH read.
2. **Is the .NET/architecture LinkedIn audience actually near any of these buyers?** The C2/C5
   reachability verdicts assumed it is *not* the KM/compliance/consultancy buyer (a friendly crowd,
   not the ICP). Only the founder knows the true composition of that following. **ASSUMPTION:** it is
   a dev crowd, not a KM/compliance-budget crowd (DISTRIBUTION.md open question #3).
3. **Willingness to abandon the current build for a pivot** — moot given the keep-the-wedge verdict,
   but recorded: no candidate cleared the bar that would justify abandoning the launch-ready product.
4. **Denmark/Poland beachhead network reality (BUSINESS_MODEL §5)** — not load-bearing for this
   verdict (all winning candidates were killed on self-serve reachability, which the network wouldn't
   fix without outreach). Flagged only so it isn't assumed away.

---

## Progress / run checkpoints
- **M0** — frame + rubric — ✅
- **M1** — candidate long-list (14) + shortlist (5) — ✅ (see §4 + §M1 below)
- **M2** — 5 Haiku evidence passes — ✅
- **M3** — 5 Sonnet refutations (all refuted) — ✅
- **M4** — scored ranking (baseline wins) — ✅
- **M5** — synthesis + verdict: **KEEP THE WEDGE** — ✅
- **Cost contract:** 7 Haiku + 5 Sonnet + coordinator-only synthesis; no Opus/Fable worker for
  search/fetch/extract (run-log.md).

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

**KEEP THE CURRENT WEDGE. Do not pivot. Launch as planned.** Of five shortlisted adjacent
specialized tools, **none beats the baseline** on mdreel's own decision rules, and every one was
refuted on the claim that would have justified it (M3). The one with the strongest demand tailwind —
**C2, webinar/talk → agent-citable knowledge base** — is (a) barely a pivot at all (it is the current
wedge with a narrower content emphasis) and (b) had *both* its load-bearing pillars refuted (buyers
DIY rather than buy — A2; and its buyer isn't reachable through mdreel's channels — A5).

**The trigger rule for "keep the wedge" is A5 (METRICS.md A5 — the top risk, which gates the other
four).** No candidate makes distribution *easier* than the already-planned gallery-led launch:
- the only candidate with a *better* channel (C4, DevRel/MCP) dies on A2/DIY (refuted **high**);
- the candidates with a real buyer (C1 compliance, C5 consultancy) can only be reached through
  **procurement or partnership — both are outreach, which the founder has ruled out** (A5);
- C2's channel is the *same gallery mdreel is already building* — so "pivot to C2" collapses into
  "curate the existing launch toward talks/webinars," which is a positioning choice, not a pivot.

**The secondary confirming rule is SB (METRICS.md §2.2 — ship-by 2026-09-15).** Three of five
candidates need net-new build (C1 tamper-evidence, C3 caption renderer, C5 white-label portal) that
would blow the gate. Keeping the wedge fits SB by construction.

**What this run does change (a positioning recommendation, not a pivot — founder's call):** the C2
evidence is the one genuinely useful finding. Active RAG-for-video demand is real and rising, the
Google Open Knowledge Format (Jun 2026) legitimizes mdreel's exact output shape, and **the pipeline's
measured-strongest content type (slide talks, N30) is exactly what a talk/webinar library is.** So the
recommendation is to **bias the Phase 5 launch — gallery curation and one landing/positioning
variant — toward "turn your talk & webinar library into an agent-citable knowledge base,"** and let
T-LAUNCH measure whether the buy-demand M3 doubted actually exists. That reuses the wedge, adds zero
build, and rides the already-funded launch experiment (METRICS.md N27). See §5.

## 2. The ranked table (M4 — scored against the §0 rubric)

Scores are the coordinator's, cited to the named rule. **Every candidate is ranked against the
baseline "keep the current wedge."** A ⛔ marks the column that kills the candidate.

| Rule (cited) | **Baseline: keep the wedge** | C2 webinar-KB | C4 DevRel docs | C1 compliance | C5 white-label | C3 accessibility |
|---|---|---|---|---|---|---|
| **A5** distribution (top risk) | Med (gallery-led launch, planned) | Med-Low *(reachability refuted)* | High *(MCP/llms.txt/blog)* | ⛔ Low *(procurement)* | ⛔ Low *(no inbound; needs outreach)* | Low |
| **A1** EU a purchase driver | Med | Low-Med | ⛔ Low *(public content, residency moot)* | High | High | Med *(but scope-killed)* |
| **A2** buy vs DIY | Med | ⛔ Low *(OKF commoditizes; DIY)* | ⛔ Low *(refuted high)* | Med | Med | Med |
| **A3** flow vs backfill | open (instrumented) | Med | ⛔ Low *(bursty backfill)* | Med | High | Med |
| **A4** citable output | High *(N30/N31 strong)* | High *(slide talks, N30)* | Med *(some weak path)* | High | Med | Med |
| **N1a** ~2–3 cover infra | Med (Pro/Business §6) | Med | Low-Med | Med-High | High | Low |
| **N15** traffic-to-survival | Med *(good-post-sized)* | Med | Med | Low | ⛔ Low | Low |
| **N23** CAC ceiling / on-ramp | Med (€398 Pro) | Med | Med | Med | High | Low *(commodity shelf)* |
| **N16–N19** bootstrappable TAM | Med (€65M TAM) | Med | Med | Med | Med | Low |
| **SB** fits before go-to-market | ✅ High *(no new build)* | High *(current output)* | High *(small delta)* | ⛔ Low *(tamper-evidence/signing)* | ⛔ Low *(white-label portal)* | ⛔ Low *(caption renderer)* |
| **Competitor test (002)** | ✅ clears (portable artifact wedge) | clears | clears | clears | clears | ⛔ files on transcription shelf |
| **Net verdict** | **BASELINE — the one to beat** | **least-weak pivot, but no verified edge** | killed A1/A2/A3 | killed A5/SB | killed A5/N15/SB | killed scope/SB |

### The ranking, least-weak pivot first — **and none beats the baseline**

1. **Baseline — keep the current wedge.** ✅ Only option that clears every column without a ⛔.
2. **C2 webinar/talk → agent-citable KB** — strongest demand tailwind and best A4/SB, but its two
   pillars (**A2 buy-vs-DIY, A5 reachability**) were both refuted, and it is barely a *pivot* at all
   — it is the current wedge with a narrower content emphasis.
3. **C4 DevRel docs** — best channel fit (A5) but killed on the sharpest A2/DIY exposure (refuted
   high) + A1 moot + A3 backfill.
4. **C1 compliance audit-trail** — real regulatory wallet, but ⛔ A5 procurement (fails no-outreach)
   and ⛔ SB (audit-grade needs net-new tamper-evidence).
5. **C5 white-label consultancy** — real EU/resale gap, but ⛔ A5 (zero inbound; needs a ruled-out
   partnership motion) and ⛔ SB.
6. **C3 accessibility** — ⛔ scope (EAA excludes internal video) and ⛔ SB (needs a caption renderer).

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

**C2 — Conference/webinar → agent-citable knowledge base** *(rank 2; least-weak pivot)*
- *Job:* turn an org's recorded talks/webinars into one schema-consistent, timestamped, portable
  Markdown KB an agent can cite. *Buyer:* KM/research lead at event-heavy orgs, analyst/consulting
  firms, associations.
- *Demand:* real and rising (RAG-for-video tutorials, Fortune-500 shift, arXiv 2602.15859, OKF).
- *Competition:* unoccupied for *schema-consistent slide-separated portable Markdown* — but OKF now
  lets anyone emit that shape.
- *Pipeline delta:* ≈ zero (this is the current output). *Scores:* A4 High, SB High; A2 Low, A5 Med-Low.
- *Single biggest risk:* **it is not a pivot** — and its buy-demand (A2) and reachability (A5) were
  both refuted. Adopt it as launch *positioning*, not a new product.

**C4 — Product/SDK docs from demo & talk videos (DevRel)** *(rank 3; killed)*
- *Job:* devtool demo/talk videos → agent-citable product-docs Markdown. *Buyer:* DevRel/technical PM.
- *Why killed:* A1 moot (public content → EU residency irrelevant), A2/DIY refuted **high** (this
  buyer wires Gemini+MCP in ~1 h), A3 backfill (bursty launch-cycle corpus). Best channel fit (A5),
  wasted on a buyer who won't pay.
- *Single biggest risk:* the audience most able to build it themselves — the sharpest A2 exposure.

**C1 — Regulatory-training audit-trail docs** *(rank 4; killed)*
- *Job:* verbatim timestamped audit records from compliance-training video. *Buyer:* compliance/L&D
  in regulated EU industries.
- *Why killed:* A5 procurement (60–120-day RFP; fails no-outreach — refuted med-high); SB (audit-grade
  needs net-new tamper-evidence/signing). Real feature gap (LMS logs who/when, not content) that
  **no regulator is actually asking for.**
- *Single biggest risk:* a wedge nobody requested, behind a procurement wall.

**C5 — White-label RAG-ingestion for AI consultancies** *(rank 5; killed)*
- *Job:* per-client video→RAG ingestion consultancies resell. *Buyer:* AI consultancy/software house.
- *Why killed:* A5 (zero self-serve demand; consultancies build in-house; needs a ruled-out
  partnership motion); N15 (no inbound); SB (white-label portal net-new).
- *Single biggest risk:* the reseller motion is outreach, which is off the table.

**C3 — Accessibility (EAA/WCAG) docs** *(rank 6; killed)*
- *Job:* WCAG transcripts/captions for internal video. *Why killed:* the EAA **excludes internal
  employee training video** (scope), and shipping captions needs a **net-new WebVTT/SRT renderer**
  (SB); generic captioning files on the €2–4/h transcription shelf (competitor test).
- *Single biggest risk:* the regulatory hook doesn't reach the target content, and the output format
  is wrong.

## 3a. What would change this verdict — the cheapest confirming experiment

Framed like a METRICS.md §1.6 gated experiment (buys evidence, not customers):

**The single test:** at the Phase 5 launch, run a **positioning variant** — curate a slice of the
already-planned gallery around **well-known conference talks the ICP knows**, and A/B one landing
headline: *"Turn your talk & webinar library into an agent-citable knowledge base"* against the
current wedge headline. Measure **signup-rate by referrer** (the METRICS.md §6.5 proxy, subject to
the §2.1 floor). 
- **Cost:** founder gallery-curation hours + the already-funded T-LAUNCH tranche (METRICS.md N27).
  **Zero net-new build, zero extra ad spend, no pipeline change.**
- **Confirms C2** if the webinar-KB framing lifts signup-rate on cold traffic above the wedge
  baseline (the buy-demand M3 doubted would be real after all).
- **Kills C2** if it doesn't — in which case the refutations stand and the general wedge is correct.
- **Why it's safe:** it rides the launch mdreel is doing anyway; it cannot blow SB; and it resolves
  the one open question (A2 buy-vs-DIY for this framing) that no amount of desk research could settle
  — exactly the A1/N12-style read T-LAUNCH exists to buy.

**Do NOT** pre-build any candidate's net-new features (tamper-evidence, caption renderer, white-label
portal) before this read — that would be building product to avoid the finding (DISTRIBUTION.md).

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

**See §3a** — the launch positioning A/B for the C2 (webinar-KB) framing, riding the already-funded
T-LAUNCH tranche (METRICS.md N27) at zero net-new build. That is the one read that would flip the
verdict from "keep the general wedge" to "specialize toward talk/webinar libraries."
