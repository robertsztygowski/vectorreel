# Twelve Labs — ring: direct · verified: 2026-07-15 · confidence: high

## 1. Vitals
- **HQ/jurisdiction:** San Francisco, California, USA (660 4th Street). US company; US legal jurisdiction. Korean roots (founders ex-Korea; NAVER/Korea Investment Partners on cap table).
- **Founded:** 2021.
- **Funding:** **$100M Series B announced 2026-07-01**, co-led by **NEA and NAVER Ventures**, with **Amazon, Radical Ventures, Korea Investment Partners, Index Ventures, Quadrille Capital, and Red Bull Ventures** participating (T: GlobeNewswire/Bloomberg). Follows a **$50M Series A (2024)** co-led by NEA and NVIDIA's NVentures. Cumulative funding ≈ **$150M+** (T: Dealroom/Bloomberg; one aggregator claims >$200M — unverified, treat as low-grade). As part of the round, signed a **multi-year AWS deal** making AWS its primary cloud (T: TechFundingNews).
- **Headcount:** ~**178–192** (mid-2026; T: LeadIQ ~178 Jun 2026, Tracxn 192 May 2026). LinkedIn's public band still shows 11–50 (stale). getLatka cites 117 people / $4.2M revenue for 2023 (T, dated).
- **Status:** **Growing / hot.** Just closed a large B two weeks ago, strategic cloud + chip backing (Amazon/AWS Trainium, NVIDIA), shipping fast (Marengo 3.0 Dec 2025, MCP server 2026). Well-capitalized and clearly in expansion. Not a zombie.

## 2. Business model
**Model type: usage-based / PAYG API credits** (metered per-minute indexing + per-minute analysis + per-query search), plus custom committed-use enterprise. **Not a per-seat or per-hour subscription** — so a clean "plan price ÷ included hours" €/video-h does not exist; the figure below is the effective **API processing cost** to make one video-hour LLM-ready.

| Plan | Price | Included | Effective €/video-h | Overage |
|---|---|---|---|---|
| Free / trial | $0, no card | **600 min (10h)** cumulative indexing, 90-day index access | €0 (trial only) | none — hard cap |
| Developer PAYG | usage-based | pay per minute | **≈ €3.9/video-h** to index+analyze (see below) | linear, no cap |
| Enterprise | custom (committed-use) | negotiated | unknown | custom |

**Effective €/video-h derivation** (USD→EUR ≈ 0.90 applied): Marengo video indexing $0.042/min = **$2.52/h** (one-time) + Infrastructure $0.0015/min = $0.09/h + Pegasus Analyze input $0.0292/min = **$1.75/h** → **≈ $4.36/h ≈ €3.9/video-h** to index + analyze one hour. **Excludes**: Search ($4/1,000 queries), Pegasus output text ($0.0075/1k tokens), and re-indexing after 90-day retention lapses. So the "all-in to get structured output" number is higher in practice.
- **Entry price point:** free 10h trial, then pure metered usage (no monthly minimum on Developer tier).
- **Enterprise motion:** sales-led ("Talk to sales" / committed-use custom).
- **Meters mdreel doesn't:** **search queries** ($4/1k), **Pegasus output tokens**, **index storage/retention** (90-day free access then you lose it / re-pay), and **infrastructure per-minute**. mdreel bills flat included-hours; Twelve Labs bills every retrieval action forever.

## 3. Product & features — checklist
- [x] transcript (ASR is part of multimodal indexing; audio+speech)
- [?] **verbatim ON-SCREEN text** — **partial and NOT portable.** Old dedicated `/text-in-video` OCR endpoint **DEPRECATED** (v1.1, doc 404s). v1.3/Marengo 3.0 folds OCR ("text-in-visual", 89.2% TextCaps) into the visual embedding, surfaced as a **search modality**, not extracted as a verbatim document. You can search for on-screen text; you cannot export a clean slide/code transcript.
- [x] visual descriptions (Pegasus generates scene/segment descriptions)
- [x] timestamps — **segment/time-coded metadata**; Pegasus 1.5+ produces time-based metadata (chapters/segments with start–end). Granularity = segment-level, not word-level.
- [?] structured/Markdown output — **JSON, yes; Markdown, no.** `/analyze` returns structured JSON to a user-defined schema (summaries, chapters, highlights, custom fields). No portable Markdown-file product; you build the doc yourself.
- [x] JSON/API (REST API, Python + JS SDKs)
- [?] webhooks (async indexing status callbacks exist; not a rich event system) 
- [x] **MCP server** (official; `mcp-alpic.twelvelabs.io`; works with Claude, Cursor, VS Code, Gemini CLI, Goose)
- [?] connectors — no CMS/Drive connectors marketed; **AWS Bedrock** availability (Marengo/Pegasus as Bedrock models) is the main "connector."
- [?] speaker ID — not clearly marketed as diarization; unknown/weak.
- [x] languages — **37** (query in 36 + English), Marengo 3.0.
- [x] max video length — **up to 4 hours / 6 GB per file** (Marengo 3.0, 2× prior).
- [?] processing-speed claims — not a headline metric; unknown SLA.
- [?] retention/erasure controls — 90-day free index access on PAYG; enterprise custom; SOC 2 covers handling. No self-serve residency erasure marketed.
- [ ] self-host option — no (managed API only; nearest is running the models via AWS Bedrock in your AWS account/region).

**What the output actually looks like:** Pegasus `/analyze` returns **JSON** — you pass a schema (e.g. `summary`, `chapters[]`, `key_scenes[]`, custom fields) and it populates timestamped segments plus generated text (titles, topics, hashtags, summaries, highlights). Marengo produces **embeddings + a searchable index**, not a document. The core deliverable is **queryable video intelligence inside their index**, not a portable transcript file — the customer assembles any Markdown/KB artifact themselves from the JSON.

## 4. Size & customer base — evidence
- **Case studies/logos:** NFL Media, MLSE, Sejong City (Korea gov), Source Digital, MindsDB, Voxel51, Mindprober (Q: homepage/case-studies + testimonials). Heavy **media & entertainment, sports, broadcast, government/public-sector** slant — NOT internal-KB / L&D.
- **Reviews:** **No G2/Capterra product review corpus found** (G2 lists it as "Image Recognition Software" with alternatives, but no aggregate rating; PeerSpot/Slashdot have 0 user reviews). Glassdoor: ~10 employee reviews (mixed; one "sinking ship," others positive) — T, not customer signal. **Effective customer review count: ~0 public.**
- **Web traffic:** one SEO aggregator (Inpages) estimates **~7,000 monthly organic visits** for twelvelabs.io (T, low-grade); Similarweb page exists but figure not captured. Treat as **low-to-modest, developer-niche traffic**.
- **GitHub/community:** official SDKs `twelvelabs-python` / `twelvelabs-js` and a public Discord; **star counts unknown** (not captured). Dev-facing but not a large OSS community.
- **Hiring signals:** headcount roughly **doubled 2023→2026** (~117 → ~180+); fresh $100M B implies aggressive hiring. Growing.

## 5. GTM & distribution
- **Positioning (VERBATIM homepage):** **"See the unseen. Know the unknowable."** Sub: *"Your video contains every insight, every event, every decision that mattered. Extracting it has been impossible. Until now."*
- **Channels:** developer-led (Developer Hub, SDKs, MCP server, Discord, blog with model research); **AWS co-marketing / Bedrock marketplace listing** (major distribution lever post-deal); PR-heavy (funding, model launches on PRWeb/GlobeNewswire); design-forward brand (Pentagram rebrand). SEO is modest; no free-tool lead magnet beyond the 10h trial; no public curated gallery in mdreel's sense.
- **Who the pricing page talks to:** **developers/ML engineers** — per-minute API rates, tokens, search-query pricing, SDKs. Not DPOs, not L&D buyers, not "team leads." Enterprise motion is a separate sales-led track (media/sports/gov).

## 6. EU/GDPR posture
- **Hosting regions:** **US-centric, NO EU residency.** Runs on GCP/AWS; the DPA (twelvelabs.io/legal/dpa) explicitly **authorises EEA→US transfer under SCCs**. AWS is now primary cloud — region not EU-committed for customers.
- **DPA available?** Yes (public legal page).
- **Subprocessor list:** exists via DPA/Trust Center; includes hyperscalers (GCP/AWS). Not marketed as short/curated.
- **No-training terms:** not prominently marketed on public pages (unknown/enterprise-negotiated).
- **Certifications:** **SOC 2 Type II** (Q: homepage "SOC 2 Type II certified. Encrypted data handling."). **No ISO 27001** advertised. Trust Center exists.
- **Residency premium:** **Neither charges nor markets EU residency** — it's simply absent. EU data-residency is a non-feature for them. This is mdreel's cleanest structural wedge (feeds A1).

## 7. Threat assessment
- **ICP overlap: LOW–MEDIUM.** Same *technical* space (video → structured/timestamped output, MCP-native, both target AI-agent/RAG builders). But Twelve Labs sells **searchable video intelligence to media/sports/gov + ML developers**, priced and documented **for engineers building on an API**. mdreel sells a **finished portable Markdown artifact + EU residency to EU software/L&D/DPO buyers (50–500)**. Different buyer, different deliverable, no EU story. The overlap that matters: a technical EU team could DIY on Twelve Labs' API.
- **What they'd have to do to kill mdreel:** (a) stand up genuine **EU-region hosting + market it to DPOs** — *unlikely near-term*: just re-committed to AWS as primary cloud, US-HQ, zero EU-residency messaging, and their whole DPA is built around SCC US transfer; (b) ship a **portable verbatim on-screen-text + Markdown document product** for internal-KB teams — *possible but off-strategy*: they're moving OCR *into* the search index (opposite of "export a clean doc"), and their brand/GTM points at media/sports superintelligence, not L&D KBs. Combined probability of both: **low**.
- **What mdreel does that they structurally can't (soon):** EU-only processing with a DPO-legible residency guarantee and a short subprocessor story; **verbatim on-screen text as a portable, spoken-vs-shown-separated Markdown file** (they deprecated the dedicated OCR endpoint and now hide OCR inside embeddings); flat, capped, predictable pricing vs. their meter-everything (index + search-query + tokens + retention) model that punishes RAG re-querying.
- **What mdreel should steal:** (1) the **schema-driven `/analyze`** pattern — let users pass a JSON schema for custom timestamped fields, not just a fixed Markdown template; (2) **first-class MCP server** as a distribution wedge into Claude/Cursor/Copilot workflows (they treat it as a core surface); (3) **4h / 6GB single-file** ceiling and 37-language support as table-stakes bars to match; (4) their crisp research-blog + PR cadence as a low-cost credibility/SEO engine (feeds A5).

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | $100M Series B, 2026-07-01, co-led NEA + NAVER Ventures; Amazon, Radical, Korea Investment Partners, Index, Quadrille, Red Bull participating | https://www.globenewswire.com/news-release/2026/07/01/3320545/0/en/twelvelabs-raises-100-million-in-series-b-funding-to-build-video-superintelligence.html | 2026-07-15 | Q |
| 2 | Round raises $100M from Amazon, NEA, NAVER etc.; NVIDIA-backed; AWS primary-cloud deal | https://www.bloomberg.com/news/articles/2026-07-01/video-search-startup-raises-100-million-from-amazon-vc-funds | 2026-07-15 | T (Bloomberg) |
| 3 | Prior $50M Series A (2024) co-led NEA + NVIDIA NVentures; ~$150M cumulative | https://app.dealroom.co/news/note/twelve-labs-raises-100m-series-b-from-amazon-and-nea-for-video-ai | 2026-07-15 | T (Dealroom) |
| 4 | Multi-year AWS deal, AWS as primary cloud provider | https://techfundingnews.com/twelvelabs-raises-100m-as-amazon-secures-aws-as-its-preferred-cloud/ | 2026-07-15 | T (TechFundingNews) |
| 5 | HQ San Francisco 660 4th St; founded 2021 | https://www.zoominfo.com/c/twelve-labs/558050535 | 2026-07-15 | T (ZoomInfo) |
| 6 | Headcount ~178 (Jun 2026) / 192 (May 2026, Tracxn) | https://leadiq.com/c/twelvelabs/60ee65fab27cefd863e226c5 | 2026-07-15 | T (LeadIQ/Tracxn) |
| 7 | 2023: ~117 people, $4.2M revenue | https://getlatka.com/companies/twelvelabs.io | 2026-07-15 | T (getLatka, dated) |
| 8 | PAYG rates: Marengo indexing $0.042/min, infra $0.0015/min, Search $4/1k; Pegasus analyze $0.0292/min in, $0.0075/1k tokens out; free 600 min/90-day/no card | https://www.twelvelabs.io/pricing | 2026-07-15 | Q |
| 9 | Positioning "See the unseen. Know the unknowable."; SOC 2 Type II; customers NFL, MLSE, Sejong City, Source Digital, MindsDB, Voxel51, Mindprober | https://www.twelvelabs.io/ | 2026-07-15 | Q |
| 10 | `/analyze` returns structured JSON (summaries, chapters, highlights, custom schema); time-based metadata | https://docs.twelvelabs.io/docs/guides/analyze-videos | 2026-07-15 | Q |
| 11 | Marengo 3.0: 37 languages, up to 4h / 6GB per file | https://docs.twelvelabs.io/docs/concepts/models/marengo | 2026-07-15 | Q |
| 12 | Official MCP server (Claude/Cursor/VS Code/Gemini CLI/Goose); URL migrated to mcp-alpic.twelvelabs.io, old URL retired 2026-07-15 | https://www.twelvelabs.io/blog/twelve-labs-mcp-server | 2026-07-15 | Q |
| 13 | DPA authorises EEA→US transfer under SCCs; no EU residency | https://www.twelvelabs.io/legal/dpa | 2026-07-15 | Q (per verified task facts) |
| 14 | On-screen-text: `/text-in-video` deprecated; OCR folded into Marengo visual search (89.2% TextCaps) | https://docs.twelvelabs.io/docs/concepts/models/marengo | 2026-07-15 | Q (per verified task facts) |
| 15 | No aggregate G2/Capterra customer review score found; listed only among Image-Recognition alternatives | https://www.g2.com/products/twelve-labs/competitors/alternatives | 2026-07-15 | T (G2) |
| 16 | ~7,000 monthly organic visits estimate | https://inpages.ai/insight/competitors/elevenlabs.io-vs-twelvelabs.io | 2026-07-15 | T (Inpages, low-grade) |
| 17 | Case-study customers (NFL Media, Source Digital etc.); media/sports/gov slant | https://www.twelvelabs.io/case-studies | 2026-07-15 | Q |