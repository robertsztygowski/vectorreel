# 07 — Synthesis: TwelveLabs vs mdreel

> 🧊 Point-in-time (2026-07-20), never authoritative. Synthesizes M1–M4. Numbers cite METRICS.md
> by name (never restated); target design cites ARCHITECTURE.md. Speculation labeled
> **[SPECULATION]**.

## 1. Feature matrix (TwelveLabs vs mdreel target design — ARCHITECTURE.md)

| Dimension | TwelveLabs | mdreel (target — ARCHITECTURE.md) |
|---|---|---|
| **Core output** | Vector embeddings + ranked video *clips* (start/end timestamps) + generated text | **Structured timestamped Markdown document** for AI knowledge bases (ARCHITECTURE §4, frozen output contract) |
| **Primary use case** | Semantic video *search* & retrieval over a large corpus; RAG over video | Turn one long video into a citable, human-readable doc/RAG source |
| **Models** | Marengo 3.0 (embed/search), Pegasus 1.2 (analyze/generate) — proprietary, in-house trained | Vertex Gemini native-video (Stage B) + text fusion (Stage C); no in-house model |
| **Search** | ✅ First-class multimodal search API (visual+audio, ranked clips) | ❌ Not a product surface — output feeds *someone else's* vector DB / RAG |
| **Embeddings** | ✅ 512-dim video/text/image/audio embeddings API | ❌ Not exposed; mdreel emits Markdown, not vectors |
| **Analyze/summarize** | ✅ Pegasus: summary, chapters, open-ended prompts, streaming | ✅ The whole product — but as a *rendered Markdown artifact*, not an API answer |
| **Ingestion** | Upload + cloud URL; documented YouTube/URL paths | Upload + **YouTube via Vertex `fileUri` only, never downloads** (CLAUDE.md rule 8) |
| **Deployment / residency** | US-HQ (SF); **no EU data-residency** (DPA authorizes EEA→US SCC transfer); London = GTM only | **EU-only** (`europe-central2`/`europe-west3`, CLAUDE.md rule 2); GDPR is a product feature (ARCHITECTURE §7) |
| **Analytics on own property** | **Ships Google Analytics + HubSpot** on playground (observed M3) | **Prohibited** — Umami, EU, cookieless (CLAUDE.md rule 10) |
| **Onboarding** | MCP-first (Jockey MCP), developer/API-led | Magic-link signup, trial credit (METRICS.md N33), gallery-led funnel |
| **Cost model** | Metered PAYG per indexing-minute + per-token + per-search | Metered per-video contribution (METRICS.md N0); every LLM+compute+ad euro in the ledger (rule 6) |
| **Pricing floor** | $0.042/min indexing + infra + search + analyze tokens | Break-even at all-in COGS (METRICS.md N6) |

## 2. Overlap & gap analysis

**The framing still holds, and the deep-dive sharpens it.** TwelveLabs is a **video-native
retrieval infrastructure platform** — it makes a corpus of video *searchable and queryable* by
machines. mdreel is a **video→document transformation** — it makes *one video* readable and citable
by humans and by any downstream RAG the customer already owns.

- **Overlap (narrow):** both call a video model to produce text (Pegasus analyze ≈ mdreel Stage B/C),
  and both attach timestamps. If a customer's only goal is "let me ask questions about my video,"
  TwelveLabs Analyze and mdreel's Markdown+RAG both answer it.
- **Gap (wide):** TwelveLabs sells **search/embeddings over many videos** as a developer platform;
  it has **no rendered-document deliverable** and no opinion about a knowledge-base of Markdown.
  mdreel sells a **finished artifact per video** to non-developers (DPO/L&D/internal-KB ICP,
  BUSINESS_MODEL) with **EU residency and GDPR** as the wedge. TwelveLabs' ICP is media/sports/gov
  (dossier 01) — a different buyer.
- **Convergence risk [SPECULATION]:** Pegasus already generates chapters+summaries; a "export
  timestamped transcript/doc" feature is a small step for them. Their moat is the model+search;
  a Markdown exporter is a weekend feature. mdreel's durable moat is **not** the transformation —
  it is **EU residency + GDPR posture + the non-developer artifact + price** (see §5).

## 3. Pricing ladder comparison (cite METRICS.md names — figures never restated)

- TwelveLabs free tier: **10 video-hours, 100 videos/index, 90-day retention** (dossier 05).
  mdreel's trial is the one-time processing credit **METRICS.md N33** — a *much* smaller, single-shot
  allowance. TwelveLabs' free tier is generous enough to be a real competitor to a paid mdreel plan
  for a low-volume user, which pressures **A2 / willingness-to-pay** (METRICS.md §4 A2).
- TwelveLabs meters **per indexing-minute + per-token + per-search**; mdreel meters **per-video
  contribution** (METRICS.md N0) against the break-even floor (METRICS.md N6). Because TwelveLabs
  charges for *search and embeddings as ongoing usage*, a heavy retrieval workload costs more there
  over time; mdreel's cost is front-loaded at processing and then the artifact is free to reuse.
- The competitively important point for mdreel's ad math: TwelveLabs' developer-PAYG per-minute
  price sets an anchor a technical buyer will compare against mdreel's per-video price. If a DPO
  benchmarks mdreel against "$0.042/min + build-it-myself," that is the **A2 kill signal**
  (METRICS.md N21, "I could build this myself") arriving through a competitor's price page.

## 4. EU / data-residency attack surface

**This is mdreel's single clearest attack surface and the deep-dive confirmed it in three
independent places:**
1. TwelveLabs is **US-incorporated, SF-HQ**; its own DPA authorizes **EEA→US transfer under SCCs**
   (dossier 01) — i.e. data *leaves the EU*. There is no documented EU processing region.
2. The London office is **GTM/sales only**, not a data region (dossier 01).
3. TwelveLabs' **own playground ships Google Analytics + HubSpot** (observed M3, assets/) — US
   trackers on the property they show to prospects. mdreel's CLAUDE.md rule 10 prohibits exactly
   this; it is a live, screenshot-able differentiator when selling to a DPO.

For mdreel's ICP (EU orgs with a compliance function), "your video never leaves `europe-central2`"
is a **binary qualifier** TwelveLabs cannot currently match without a new region. This directly
supports the GDPR-as-feature stance (ARCHITECTURE §7) and is the highest-leverage message for A1
(METRICS.md §4 A1) against this competitor.

## 5. What to copy / what to avoid / where they'd crush us

**Copy:**
- **Clip-level timestamped results with a copyable start/end** — their search UX makes "jump to the
  moment" trivial; mdreel's Markdown anchors (ARCHITECTURE §4) should be as effortless to deep-link.
- **Streaming analyze** for perceived latency on long generations (they stream NDJSON by default).
- **Transparent, self-serve API-key + usage dashboard** with a hard free-tier cap (`is_hard_limit`)
  — good guardrail hygiene that matches mdreel's ledger discipline (rule 6).
- **MCP as a distribution channel** (see 06) — low-cost developer reach mdreel currently ignores.

**Avoid:**
- **US analytics/HubSpot on the property** — for mdreel this is disqualifying (rule 10); it is also
  a credibility gap *they* have that mdreel should never copy.
- **Developer-platform positioning** — mdreel's buyer is not a developer; matching their API-first
  onboarding would abandon the DPO/L&D wedge.
- **In-house model training** — mdreel deliberately rides Vertex (ARCHITECTURE §2); competing on
  model R&D against NVIDIA-funded TwelveLabs is unwinnable and unnecessary.

**Where they'd crush us:**
- **Corpus-scale semantic search** — anything needing "search 10,000 videos" is their home turf;
  mdreel has no answer and should not pretend to.
- **Multimodal embeddings as infrastructure** — if a customer wants raw vectors, TwelveLabs wins.
- **Funding & model velocity** — NVIDIA-backed; they will out-ship on model capability. mdreel must
  compete on **residency, artifact, buyer-fit and price**, never on model horsepower.

## 6. Threat level & reasoning

**Threat level: MODERATE, and mostly indirect.**
- **Not a direct competitor for mdreel's ICP today:** different buyer (media/sports/gov vs
  EU-internal-KB/DPO), different deliverable (search API vs Markdown artifact), and a
  residency posture that *disqualifies* them for the compliance-driven buyer mdreel targets.
- **Indirect threat via A2 / build-vs-buy:** a technical stakeholder inside mdreel's prospect can
  point at TwelveLabs' generous free tier and per-minute API and say "we'll just use this"
  (METRICS.md N21). That is the realistic loss path, not head-to-head displacement.
- **Latent threat via convergence [SPECULATION]:** if TwelveLabs ships a doc/transcript exporter
  and an EU region, the overlap widens sharply. Watch their changelog for both.
- **Reasoning:** mdreel wins where it is chosen — EU residency, GDPR-as-feature, non-developer
  artifact, per-video price. It loses where the buyer is technical and residency-indifferent. The
  strategic response is to **lean into the residency/artifact/buyer wedge harder**, not to add
  search/embeddings.

## 7. Living-doc contradictions found (report only)

- **None that overturn strategy.** The deep-dive *corroborates* BUSINESS_MODEL/DISTRIBUTION/METRICS
  rather than contradicting them: the EU-residency wedge, the GDPR-as-feature stance, and the
  "not a corpus-search platform" non-goal all hold up against the strongest incumbent in the space.
- **One nuance worth flagging (not a contradiction):** TwelveLabs' free tier (10 video-hours) is
  materially more generous than mdreel's trial credit (METRICS.md N33). This does not contradict any
  doc, but it is a real pressure on A2 and on the perceived value of mdreel's paid entry — the
  founder may want to revisit N33's size against competitive free tiers. **Reported only; no doc
  edited autonomously (contract §5).**

Mirrored into STATUS.md → Living-doc-contradictions section.

## Evidence log

| # | Claim | Source URL / dossier | Checked | Grade |
|---|---|---|---|---|
