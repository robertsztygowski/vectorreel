# Mixpeek — ring: direct · verified: 2026-07-15 · confidence: med

## 1. Vitals
- **HQ/jurisdiction:** New York, NY, USA (US company — no EU entity found). [T: Crunchbase]
- **Founded:** 2022 (per Crunchbase-derived search; not vendor-confirmed). [T, low confidence]
- **Funding:** Pre-seed round, amount **obfuscated/undisclosed** on Crunchbase. Investors named: **Essence VC** and **Work-Bench** (3 investors total per Crunchbase). No Seed/Series A found. [T: Crunchbase; Essence VC careers page hosts Mixpeek jobs]
- **Headcount:** unknown (LinkedIn est. not sourced; Essence VC job board actively lists Mixpeek AI Engineer roles → small, hiring). [T]
- **Status:** **Growing but early.** Signals: active hiring via investor job boards, live per-unit pricing refreshed "as of July 2026," multiple maintained GitHub repos (SDK, benchmarks, use-cases), a Supabase-authored integration guide. Pre-seed stage with undisclosed raise → still proving GTM, not scaled.

## 2. Business model
**Model type:** Usage-based (per-minute/per-unit metering) with monthly plan minimums that unlock capacity ceilings. Not seat-based; not hour-bundled like mdreel.

| Plan | Price | Included | Effective €/video-h | Overage |
|---|---|---|---|---|
| Build | $25/mo min (~€22.50) | 25 collections, 5 namespaces, up to 100K objects/mo, processing pool | See usage rate below | metered per unit |
| Scale | $250/mo min (~€225) | 200 collections, 25 namespaces, up to 1M objects/mo, SSO, priority support | See usage rate below | metered per unit |
| Enterprise | Custom | Unlimited collections/namespaces, dedicated single-tenant, forward-deployed engineer, audit logs, custom extractors, fine-tuning | N/A | negotiated |

- **Comparable unit:** €/video-hour is **not plan-bundled** — video is metered at **$0.05/min = $3.00/video-hour ≈ €2.70/video-hour** (USD→EUR ×0.90). This is a *processing/extraction* rate, not an all-in structured-output price, and excludes storage/query. [Q, from pricing page + vs-TwelveLabs page]
- **Other meters (mdreel doesn't charge these):** audio $0.01/min; images/docs $1.50/1K; text $2/M tokens; **vector storage $0.33/GB/mo**; **queries $2/M** beyond pool. "Searches and retrievals are always free" on Build. [Q]
- **Entry price point:** $25/mo (~€22.50). **No free tier stated; no explicit trial shape found.**
- **Enterprise motion:** Self-serve on Build/Scale ("no long-term commitments"); **sales-led** for Enterprise (custom, forward-deployed engineer, Slack support).

## 3. Product & features — checklist
- [x] transcript (Whisper)
- [x] verbatim ON-SCREEN text — **OCR extraction listed**, but framed as a searchable feature, not verbatim slide/code fidelity output [Q, unverified fidelity]
- [x] visual descriptions (scene descriptions via Gemini)
- [x] timestamps — timestamped results / scene-level; **exact granularity unknown**
- [ ] structured/**Markdown** output — **No.** Output is embeddings + searchable indexed features (JSON/vectors), **not portable Markdown docs**
- [x] JSON/API (video search API; sub-100ms p95 retrieval)
- [x] webhooks (Build plan)
- [x] MCP server (MCP integration support mentioned)
- [x] connectors: **S3, GCS, R2, Azure/S3-compatible, Mux, LangChain**
- [x] speaker ID (speaker diarization); plus face recognition/identity matching
- [?] languages — unknown (Whisper implies multilingual, not stated)
- [?] max video length — unknown
- [x] processing-speed claims — retrieval "well under 100ms p95" (retrieval, not ingest)
- [?] retention/erasure — indexes in-place; "nothing leaves your cloud," but explicit erasure controls unknown
- [x] self-host option — **BYO-Cloud / single-tenant "in your chosen cloud and region"** (Enterprise)

**What the output actually looks like:** Mixpeek's output is **not a document** — it is an *index*. Per docs, video ingestion produces transcript text (Whisper), transcript embeddings (E5-Large 1024D), visual embeddings (Vertex AI 1408D), face embeddings (ArcFace 512D), scene descriptions (Gemini), and keyframes, written into searchable "collections." You query it (hybrid dense/sparse/BM25) and get back timestamped scenes — you do **not** get a portable Markdown transcript file to drop into your own RAG. This is the core structural divergence from mdreel.

## 4. Size & customer base
- **Case studies/logos:** unknown (target verticals named — creative/media, e-commerce, education, brand-safety — but no named logos sourced).
- **Reviews:** **No G2/Capterra Mixpeek review pages found** (search returned unrelated products). Review count effectively **unknown/near-zero**.
- **Web traffic:** unknown (no source).
- **GitHub/community:** Active org (`github.com/mixpeek`) — Python SDK, `video-embedding-benchmark`, `use-cases`, `awesome-multimodal-search`; third-party **Supabase docs** include a Mixpeek video-search example (external validation). **Star counts not sourced.**
- **Hiring:** AI Engineer roles live on Essence VC careers board → actively hiring. [T]

## 5. GTM & distribution
- **Positioning (verbatim):** *"Find any scene in your video library."* Also (vs TwelveLabs, verbatim): *"Twelve Labs is the best pure video-understanding API… Mixpeek is the better fit when video is part of a mixed corpus… or when you want transparent per-unit pricing."*
- **Channels:** Developer SEO + **comparison pages** (`/comparisons/mixpeek-vs-twelvelabs` — classic bottom-funnel SEO play), open-source GitHub repos & benchmarks as top-funnel, third-party integration docs (Supabase, LangChain), investor job-board presence. No free tool or public gallery observed; no ads observed.
- **Who the pricing page talks to:** **Developers and solutions/infra engineers** — language is collections, namespaces, objects, embeddings, per-unit metering. Not L&D, not DPO, not a team lead.

## 6. EU/GDPR posture
- **Hosting regions:** US-headquartered. **Data-residency story is BYO-Cloud, not EU-managed** — "single-tenant deploys in your chosen cloud and region; data stays in-region," "indexes it in place… nothing leaves your cloud." So EU residency is achievable **only if the customer runs it in an EU bucket/region themselves** (Enterprise tier). No mixpeek-operated EU region advertised.
- **DPA:** unknown (not surfaced).
- **Subprocessor list:** unknown, but architecture implies Whisper/Vertex AI/Gemini in the pipeline (Google + likely OpenAI-lineage models).
- **No-training terms:** unknown.
- **Certifications:** **"SOC 2-ready," "HIPAA-ready"** (i.e., *ready*, not certified). [Q]
- **Residency premium:** In-region isolation is an **Enterprise-tier** capability (single-tenant), effectively gated behind custom pricing — they market deployment flexibility, not an EU-hosted managed service.

## 7. Threat assessment
- **ICP overlap: LOW–MED.** Mixpeek sells **multimodal search/retrieval infrastructure** to developers building search over large media libraries (media, e-comm, brand-safety). mdreel sells **portable structured Markdown** to EU IT/L&D teams building internal AI knowledge bases. Overlap exists only where a customer wants "video → feed my RAG," but Mixpeek's answer is "query our index," not "here's a Markdown file you own." Different buyer (infra dev vs. knowledge-base owner), different artifact.
- **What they'd need to do to kill mdreel:** ship an EU-managed (not BYO) hosting option **and** pivot output from embeddings-index to portable, verbatim-fidelity Markdown docs with no lock-in, **and** re-target IT/L&D buyers. **Unlikely** — it contradicts their whole thesis (search-as-a-service, lock-in-by-index is the moat; BYO-cloud is their residency answer). They'd be abandoning their differentiation.
- **What mdreel does that they structurally can't (cheaply):** (1) **no lock-in** — mdreel hands over Markdown you keep; Mixpeek's value *is* the persistent index; (2) **EU-managed residency as default** for buyers who won't/can't run their own single-tenant infra; (3) **verbatim on-screen text (slides/code/UI) as the deliverable**, spoken-vs-shown separated, vs OCR-as-a-search-signal; (4) simple bundled €/hour pricing vs a 6-meter usage model developers must forecast.
- **What mdreel should steal:** (1) the **comparison-page SEO** play (`/comparisons/mdreel-vs-X`) — cheap distribution against a named incumbent, directly relevant to A5; (2) **BYO-storage / index-in-place** as an enterprise objection-killer for data-sensitive EU accounts ("your video never leaves your bucket"); (3) **transparent per-unit pricing table** framing as a trust signal.

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | Positioning "Find any scene in your video library"; SOC2-ready, HIPAA-ready, BYO-Cloud | https://mixpeek.com | 2026-07-15 | Q |
| 2 | Build $25/mo (25 collections/5 namespaces/100K objects); Scale $250/mo (200/25/1M, SSO); Enterprise custom single-tenant; searches free | https://mixpeek.com/pricing | 2026-07-15 | Q |
| 3 | Usage rates: video $0.05/min, audio $0.01/min, images $1.50/1K, text $2/M tok, storage $0.33/GB/mo, queries $2/M | https://mixpeek.com/pricing | 2026-07-15 | Q |
| 4 | €2.70/video-h derived from $0.05/min ×60 ×0.90 USD→EUR | https://mixpeek.com/pricing | 2026-07-15 | E |
| 5 | Single-tenant in chosen cloud/region; indexes in place, nothing leaves your cloud; per-unit vs TwelveLabs multi-meter | https://mixpeek.com/comparisons/mixpeek-vs-twelvelabs | 2026-07-15 | Q |
| 6 | Output: Whisper transcripts, E5-Large 1024D transcript embeddings, Vertex AI 1408D visual embeddings, ArcFace 512D face embeddings, Gemini scene descriptions, keyframes | https://mixpeek.com/docs | 2026-07-15 | Q |
| 7 | HQ New York; investors Essence VC + Work-Bench; pre-seed, amount obfuscated; 3 investors | https://www.crunchbase.com/organization/mixpeek | 2026-07-15 | T (Crunchbase) |
| 8 | Founded 2022 | https://www.crunchbase.com/organization/mixpeek | 2026-07-15 | T (low conf) |
| 9 | Active GitHub org (SDK, video-embedding-benchmark, use-cases); Supabase third-party integration guide | https://github.com/mixpeek | 2026-07-15 | T (GitHub/Supabase) |
| 10 | Actively hiring (AI Engineer) via Essence VC careers board | https://careers.essencevc.fund/companies/mixpeek-2 | 2026-07-15 | T |
| 11 | Connectors S3/GCS/R2/Azure/Mux/LangChain; hybrid dense/sparse/BM25 <100ms p95; MCP support | https://mixpeek.com | 2026-07-15 | Q |
| 12 | No G2/Capterra review pages located; review count unknown | (search, no result) | 2026-07-15 | T (absence) |

**Caveats:** Founded-year, headcount, funding amount, DPA, languages, max length, and timestamp granularity are **unsourced/unknown** — not guessed. Crunchbase page returned 403 on direct fetch; company facts are from search-surfaced Crunchbase snippets (grade T, treat founding year as low-confidence).