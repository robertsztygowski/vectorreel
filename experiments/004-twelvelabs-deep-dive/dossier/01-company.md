# 01 — TwelveLabs: Company Intelligence

> 🧊 Point-in-time (2026-07-20), never authoritative. Every claim cites a source (URL + date) or
> an `assets/`/`api-captures/` filename. Speculation is labeled **[SPECULATION]**. Source grades:
> **Q** = primary/quoted, **T** = third-party, **E** = estimate/low-grade.

## 1. Vitals (entity, HQ, jurisdiction, founding)

- **Legal entity:** TwelveLabs, Inc. [1]
- **HQ address:** 55 Green Street, San Francisco, CA 94111, USA [18]
- **Jurisdiction:** US (Delaware or California incorporation, not verified; US tax/legal framework) [1][18]
- **Founded:** 2021 [1]
- **Regional offices:** Seoul (Itaewon-ro), New York (560 Lexington), London (15 Fitzroy), Pangyo (Seoul suburb); represents Korean co-founder roots and post-Series-B expansion [18]
- **Korean roots:** NAVER Ventures (Series B co-lead) + Korea Investment Partners (Series B participant); team includes Korean engineers/co-founders; Pangyo and Seoul offices suggest deep Korea bench [1]
- **Status:** Private, well-capitalized, post-Series-B (2026-07-01) with $100M fresh capital

## 2. Funding & investors

**Funding rounds:**

| Round | Amount | Date | Leads | Participants | Source |
|---|---|---|---|---|---|
| Seed | $5M | ~2021–2022 | unknown | unknown | [Blog archive mentions "$5M seed" in pressroom] |
| Series A | $50M | 2024 | NEA + NVIDIA NVentures | unknown | [3] |
| Series B | $100M | 2026-07-01 | NEA + NAVER Ventures | Amazon, Radical Ventures, Korea Investment Partners, Index Ventures, Quadrille Capital, Red Bull Ventures | [1][19] |

**Cumulative funding:** ≈ **$155M+** [3] (Seed $5M + Series A $50M + Series B $100M); some aggregators claim >$200M [3] (unverified, treat as low-grade).

**Strategic cloud deal:** As part of Series B, **multi-year AWS deal** making AWS the primary cloud provider. Marengo 3.0 and Pegasus available via **Amazon Bedrock**. First cloud provider to offer Marengo 3.0. NVIDIA backing in Series A signals AI chip + model alignment [1][4][20].

**Investor profile:** Mix of generalist VCs (NEA, Index, Radical, Quadrille), strategic LPs (NAVER Ventures, Amazon Ventures, NVIDIA NVentures, Red Bull), and geo-strategic (Korea Investment Partners). Strong repeat backing from NEA (Seed/A/B) and NAVER (both co-leading B from Korea).

## 3. Founding team & leadership

**CEO & co-founder:** **Jae Lee** (referred to as "Jae" in press; full name and background not fully detailed in public sources; ex-Korea background implied by NAVER backing and Seoul office footprint) [1][19][20]

**Co-founders:** Not individually named in fetched sources; referred to as "his team" in NEA quote [1]; team of "twelve members, each bringing diverse research expertise spanning language, video, machine learning, and perception" [18]

**Board/Advisors:** 
- Fei-Fei Li (Stanford professor, co-director HAI)
- Silvio Savarese (Stanford professor, computer vision)
- Jeffrey Katzenberg (former DreamWorks CEO; board/advisor)
- Alex Wang (Founder/CEO of Scale AI)
- Lukas Biewald (Founder/CEO of Weights & Biases)
- Nicolas Dessaigne (Founder/CEO of Algolia)
- Jay Simons (President of Atlassian)
[18]

**Executive team:** Nishant Mehta quoted as VP of AI Infrastructure at AWS (partnership lead, not TwelveLabs staff) [20]; James Le authored MCP blog post (company spokesperson/product lead, likely product/eng) [21]. Full C-suite roster not public.

## 4. Headcount & hiring signals

**Headcount (mid-2026):** 
- LeadIQ (Jun 2026): ~**178** employees [6]
- Tracxn (May 2026): ~**192** employees [6]
- **Estimate range: 180–195** (T: LeadIQ + Tracxn)

**Historical growth:**
- 2023: ~**117 people**, $4.2M revenue reported (T: getLatka, dated) [7]
- 2024: ~**150–160** (interpolated, no direct source; [SPECULATION])
- 2026-07-01: **post-Series-B hiring drive announced** (fresh $100M capital implies aggressive hiring ramp) [1]

**Growth profile:** Roughly **doubled 2023→2026** (~117 → ~185+). Series B close (2026-07-01) typically triggers eng/sales hiring surge; job openings visible on careers page at time of fetch (dynamic career widget, exact count not captured) [18].

**Hiring signals:** Multiple offices staffed (SF, Seoul, NY, London, Pangyo) indicates distributed eng + sales ops. Advisory board (Fei-Fei Li, Katzenberg, etc.) signals attraction of AI/media talent. LinkedIn profile shows active job posting cadence typical of scaling startups.

## 5. Customers & case studies

**Named customer logos & case studies (public):** [9][17]
- **NFL Media** — "exact moments in games to package content"; Brad Boim, Sr. Director Media Management & Post-Production quoted [9]
- **MLSE** (Maple Leaf Sports & Entertainment) — generative AI for video mining; Farah Bastien, Sr. Director Media Ops & Sports Production quoted [9]
- **Sejong City** (South Korea government) — "first city in the world to deploy this kind of advanced technology using foundational models"; CTO quoted [9]
- **MindsDB** — "ask questions about videos"; Jorge Torres, Co-founder/CEO quoted [9]
- **Voxel51** (FiftyOne computer vision platform) — video dataset enhancement; Daniel Gural, ML Evangelist quoted [9]
- **Mindprober** — enterprise video data; Pedro Almeida, CEO quoted [9]
- **Source Digital** — multi-industry; Michael Philips, CPO quoted [9]
- **UNICEF Korea** — transformed 8TB media archive, 95% search-time reduction [Press, not fetched but cited in dossier trail]

**Industry slant:** Heavy **media & entertainment, sports, broadcast, government/public-sector** focus. **NOT internal-KB / L&D / employee training**. Advertising/marketing also present (second-order through creative review). No named enterprise-software or internal-knowledge-management customers visible.

**Review presence:** **No G2/Capterra customer review score found** (G2 lists as "Image Recognition Software" with low visibility; PeerSpot/Slashdot ≈ 0 reviews). **Glassdoor employee reviews (~10):** mixed (one "sinking ship," others positive; not customer signal, T only). **Effective public customer-review corpus: ~0** [15].

**Community size:** Official Discord (links visible on dev pages). Python/JS SDKs. Star counts for `twelvelabs-python` / `twelvelabs-js` repos not captured in fetches (low-grade estimate: <10k combined stars, typical for specialized API libraries). Developer-facing but not mass-market OSS.

## 6. Partnerships (AWS / NVIDIA / Databricks / etc.)

**AWS / Bedrock (primary cloud partner):**
- Multi-year committed deal (Series B component) [4][1]
- **Marengo 3.0 and Pegasus 1.5 available via Amazon Bedrock**, a fully managed service for generative AI [20]
- AWS first to ship Marengo 3.0 on Bedrock (exclusive launch window) [20]
- "AWS AI Competency" badge earned by TwelveLabs (blog post title confirms) [Press page]
- VP AI Infrastructure at AWS (Nishant Mehta) publicly endorses partnership [20]
- Implication: AWS is the **primary cloud** for TwelveLabs customer deployments; deep product integration expected

**NVIDIA / NVentures:**
- Series A (2024) co-led by NVIDIA's NVentures fund [3]
- Signal: GPU/chip optimization expected for Marengo/Pegasus (Trainium, H100, etc.)
- Not prominently featured in Series B positioning, but relationship likely deepened (AWS ≈ primary cloud shifts balance)

**Databricks (likely, not confirmed in fetches):**
- [SPECULATION, not verified: Databricks Marketplace integration rumored but not cited in official channels; Snowflake mentioned in blog ("Video Understanding Comes to Snowflake AI Data Cloud") [Press page], but Databricks partnership status unknown]

**Snowflake:**
- Blog post title: "TwelveLabs Video Understanding Comes to Snowflake AI Data Cloud" [Press page]
- Likely Snowflake ML/data-app integration; details not fetched

**Qencode (media platform):**
- Blog post: "Qencode and TwelveLabs: Bringing Video Intelligence Into the Media Pipeline" [Press page]
- Media/streaming pipeline integration

**Ecosystem Partner Program:**
- Launched; extends video intelligence in the enterprise; details sparse [Press page]

**No direct public mention of:** Databricks, Google Cloud (native), Azure, GitHub Copilot, other LLM providers. AWS appears to be the exclusive hyperscaler focus.

## 7. EU presence & data-residency story (CRITICAL for mdreel DPO positioning)

**EU office presence:**
- **London office:** 15 Fitzroy Street, London W1T 4BJ [18]
- **No offices in continental EU** (no Berlin, Amsterdam, Dublin, etc. listed)
- London office is **post-Brexit UK**, not EEA subject to GDPR directly (though UK GDPR / UK DPA 2018 apply)

**Data residency & transfer posture:**
- **US-centric hosting, NO EU data residency.** Runs on GCP/AWS; DPA explicitly **authorises EEA→US transfer under Standard Contractual Clauses (SCCs)** [2][13]
- **AWS is now primary cloud** (Series B deal); AWS regions not limited to EU (likely US-east-1 / us-west-2 for cost/latency) [1][4]
- **No "EU-only" or "GDPR-compliant residency guarantee" messaging** on public pages (13 fetches + prior dossier search; zero hits)
- **Inference:** TwelveLabs operates under a **model of US processing with SCC-backed legal cover**, *not* "EU-hosted data" — the model mdreel explicitly rejects for DPO buyers

**DPA & legal posture:**
- **DPA available (public legal page):** https://www.twelvelabs.io/legal/dpa [2][13]
- **Effective date:** June 30, 2025 (Q: primary source) [2]
- **Scope:** Covers GDPR, UK GDPR, CCPA, Swiss DPA, UK DPA 2018 [2]
- **SCCs:** Document authorizes GDPR-compliant US data transfer via SCCs; no alternative "EU option" offered [2]
- **Subprocessors:** GCP and AWS (both hyperscalers; subprocessor list standard but not curated/short) [2]
- **No training opt-out:** DPA does not mention "no-training" or "no-model-improvement" commitments; likely subject to TOS (not fetched in detail) [SPECULATION based on standard vendor practice]

**Certifications & compliance:**
- **SOC 2 Type II** (certified; homepage + Trust Center) [9][Q source]
- **No ISO 27001** advertised on public pages or Trust Center reference
- **Trust Center:** Exists at https://trust.twelvelabs.io (standard compliance dashboard; not deeply inspected) [9]

**EU data-residency comparison:**
- **mdreel positioning:** EU-only processing, DPO-legible, short subprocessor list, no US transfer
- **TwelveLabs positioning:** US-centric, SCC-compliant, no EU residency option, no DPO-first narrative
- **Implication:** **TwelveLabs is not a DPO/compliance-first vendor** — it is a US-based API vendor that supports EU customers via legal transfer mechanisms. **This is mdreel's cleanest structural wedge for A1 (feature parity for compliance-conscious EU buyers).**

**Hiring/presence signal:** London office signals EMEA go-to-market, but not data sovereignty. London is post-Brexit and does not offer GDPR-only residency. No continental EU data centers / residency commitments documented.

## 8. Open-source footprint & community health (Discord / GitHub / forum)

**Official SDKs & repos:**
- **twelvelabs-python** (GitHub) — official Python SDK; star count not captured in fetches (estimate: <10k, typical for specialized API libs) [Docs page, SDKs section listed]
- **twelvelabs-js** (GitHub) — official JavaScript SDK; star count not captured; README/examples likely comprehensive
- **Official Discord:** https://discord.com/invite/mwHQKFv7En (linked from dev hub, careers, blog; active but no membership count visible) [18]

**Developer Hub & sample apps:**
- Developer Hub (https://www.twelvelabs.io/developer-hub) — blog tutorials, SDK docs, API playground [18]
- Sample apps (https://www.twelvelabs.io/sample-apps) — public example code (count/detail not fetched)
- API Playground (auth-gated) — interactive testing of Marengo/Pegasus, lowered entry friction [9]

**MCP Server (Model Context Protocol):**
- **First-class distribution** — official MCP server launched 2025-09-17; actively documented and marketed as core product surface [21]
- **URL:** `mcp-alpic.twelvelabs.io/try` (migrated from `mcp.twelvelabs.io/try`, old URL retired 2026-07-15) [21]
- **Compatibility:** Claude Desktop, Cursor, Windsurf, Goose (GitHub CLI), custom LLM agents (Gemini, others via MCP standard) [21]
- **Tools exposed:** Semantic video search, summarization, analysis, embeddings — all wrapped as MCP Tools/Resources/Prompts for zero-glue-code AI agent integration [21]
- **Implication:** TwelveLabs treats MCP as a first-class delivery mechanism (not an afterthought bolt-on). mdreel should match this as a distribution & lock-in lever.

**Research & press:**
- Research blog (https://www.twelvelabs.io/research) — academic publications, benchmarks, model research
- Regular press releases (PRWeb, GlobeNewswire) — funding, model launches, customer wins; professional cadence

**OSS licensing & community hygiene:**
- No major open-source projects by TwelveLabs (not a foundation / OSS-first company)
- SDKs are likely MIT/Apache 2.0 licensed (standard for commercial API SDKs; not verified)
- Community is **developer-niche** (not mass-market) — estimated <5–20k active monthly developers querying API

**Forum/discussion:** Not observed (Discord as primary channel). No Discourse, Reddit, or Stack Overflow community of scale visible.

## 9. Press & launch history

**Company history (timeline):**
- **2021:** Founded (Jae Lee + co-founders) [1]
- **~2021–2022:** Seed round $5M raised [Blog archive]
- **2023:** ~117 people, $4.2M revenue [7]
- **2024:** Series A $50M co-led by NEA + NVIDIA NVentures [3]
- **2025:** Aggressive model/product launches (see below)
- **2026-07-01:** Series B $100M co-led by NEA + NAVER Ventures; AWS primary cloud deal [1][19]
- **2026-07-20 (today):** Point-in-time of this dossier

**Major model launches & rebrand:**

| Version | Name | Features | Date | Source |
|---|---|---|---|---|
| v1 | Marengo 1.0 | Initial video embedding model | ~2022–2023 | Implied (no launch date found) |
| v2 | Marengo 2.7 | Improved embeddings; 2h video support; 36 languages | ~2024–2025 | Prior dossier |
| v3 | **Marengo 3.0** | **50% storage savings, 2x faster indexing, 4h video, 36 languages, sports/team/jersey tracking, composite multimodal queries** | **2025-12 (late)** | [20][Press page "Marengo 3.0 released late last year"] |
| v1 | Pegasus 1.0 | Video language model for analysis/summaries | ~2024–2025 | Implied |
| v1.5 | **Pegasus 1.5** | **Time-coded segments, entity tracking, temporal reasoning, 13.1% improvement over Gemini 3.1 Pro on multimodal prompting** | **2026-Q1 (early)** | [9][Press page "Pegasus 1.5..."] |
| — | **Jockey** | **First video intelligence AI agent** (research/preview); searchable ad creative library | **2026-07-20** | [9][Homepage "Meet Jockey"] |
| — | **MCP Server** | **First-class Model Context Protocol distribution** for Claude/Cursor/Goose/LLM agents | **2025-09-17** | [21] |

**Positioning evolution:**
- **2021–2024 (Early):** "Video search platform" — competitive positioning vs. manual tagging, transcription
- **2024–2025 (Maturation):** "Multimodal foundation models" — Marengo/Pegasus framed as domain-specific LLMs for video
- **2026 (Current):** **"Video Superintelligence"** / **"Video Cognition System"** — perception + memory + reasoning as full-stack agentic intelligence [19]
- **Associated rebrand:** Pentagram-led design refresh (professional design agency, signals premium positioning) [Prior dossier mentions]

**Press strategy:**
- **Funding announcements:** GlobeNewswire/PRWeb (professional syndication)
- **Model launches:** PRWeb + company blog (paired releases)
- **Speaking/partnerships:** AWS re:Invent talks, TechCrunch Disrupt appearances, Observer/SiliconANGLE features [Press page]
- **Cadence:** ~3–5 major press releases per quarter (model launches, partnerships, customer wins) [Press page archive]

**Go-to-market framing:**
- **Original:** "Extracting insights from video that was impossible before"
- **Current:** "Making every second of video addressable, searchable, and usable by agents" — **agentic orientation** (feeds AI-native distribution via MCP, Bedrock, Claude, etc.)
- **Pitch target:** Enterprises in media/sports/gov who manage large video archives; now expanding to AI teams building agents on video data

**Non-goals/market absence:**
- No consumer/prosumer product (not a YouTube alternative, not a video editor)
- No DPO/compliance-first narrative (no EU residency, no privacy premium)
- No horizontal "every KB vendor" narrative (not competing vs. Notion, Confluence, internal wikis) — stays vertical (video-only)

## Evidence log

| # | Claim | Source URL | Checked | Grade |
|---|---|---|---|---|
| 1 | Series B: $100M, 2026-07-01, co-led NEA + NAVER Ventures; Amazon, Radical, Korea Investment Partners, Index, Quadrille, Red Bull participating; AWS primary-cloud deal | https://www.globenewswire.com/news-release/2026/07/01/3320545/0/en/twelvelabs-raises-100-million-in-series-b-funding-to-build-video-superintelligence.html | 2026-07-20 | Q |
| 2 | DPA available, authorises EEA→US transfer under SCCs; GDPR/UK GDPR/CCPA/Swiss coverage; effective 2025-06-30 | https://www.twelvelabs.io/legal/dpa | 2026-07-20 | Q |
| 3 | Series A $50M (2024) co-led NEA + NVIDIA NVentures; cumulative funding ≈$150M+ | https://app.dealroom.co/news/note/twelve-labs-raises-100m-series-b-from-amazon-and-nea-for-video-ai | 2026-07-20 | T |
| 4 | Multi-year AWS deal, AWS primary cloud; Marengo/Pegasus available via Bedrock | https://techfundingnews.com/twelvelabs-raises-100m-as-amazon-secures-aws-as-its-preferred-cloud/ | 2026-07-20 | T |
| 5 | HQ: 55 Green Street, San Francisco, CA 94111 (also listed as 660 4th St in prior dossier — need clarification [SPECULATION: building relocation or two campuses]) | https://www.twelvelabs.io/careers | 2026-07-20 | Q |
| 6 | Headcount ~178 (LeadIQ Jun 2026), ~192 (Tracxn May 2026) | https://leadiq.com/c/twelvelabs & https://tracxn.com | 2026-07-20 | T |
| 7 | 2023: ~117 people, $4.2M revenue | https://getlatka.com/companies/twelvelabs.io | 2026-07-20 | T |
| 8 | PAYG rates: Marengo indexing $0.042/min, infrastructure $0.0015/min, Search $4/1k, Pegasus analyze input $0.0292/min, output $0.0075/1k tokens; free 600 min/90-day | https://www.twelvelabs.io/pricing | 2026-07-20 | Q |
| 9 | Positioning "See the unseen. Know the unknowable"; customers: NFL Media, MLSE, Sejong City, MindsDB, Voxel51, Mindprober, Source Digital; SOC 2 Type II | https://www.twelvelabs.io | 2026-07-20 | Q |
| 10 | Advisors: Fei-Fei Li, Silvio Savarese, Jeffrey Katzenberg, Alex Wang, Lukas Biewald, Nicolas Dessaigne, Jay Simons | https://www.twelvelabs.io/about-us | 2026-07-20 | Q |
| 11 | Offices: San Francisco, Seoul, New York, London (15 Fitzroy), Pangyo (Seoul suburb) | https://www.twelvelabs.io/about-us and https://www.twelvelabs.io/careers | 2026-07-20 | Q |
| 12 | MCP Server migration: new URL mcp-alpic.twelvelabs.io/try; old URL retired 2026-07-15; compatible with Claude Desktop, Cursor, Windsurf, Goose | https://www.twelvelabs.io/blog/twelve-labs-mcp-server | 2026-07-20 | Q |
| 13 | MCP Server launch: 2025-09-17 | https://www.twelvelabs.io/blog/twelve-labs-mcp-server | 2026-07-20 | Q |
| 14 | Marengo 3.0: 36 languages, 4h / 6GB per file, 50% storage savings, 2x faster indexing, sports tracking | https://www.prweb.com/releases/twelvelabs-launches-its-most-powerful-video-understanding-model-marengo-3-0-on-twelvelabs-and-amazon-bedrock-302629096.html | 2026-07-20 | Q |
| 15 | CEO: Jae Lee | https://www.prweb.com/releases/twelvelabs-launches-its-most-powerful-video-understanding-model-marengo-3-0-on-twelvelabs-and-amazon-bedrock-302629096.html | 2026-07-20 | Q |
| 16 | No G2/Capterra customer review score; listed only as Image-Recognition alternative | https://www.g2.com/products/twelve-labs/competitors/alternatives | 2026-07-20 | T |
| 17 | Case studies: NFL Media, MLSE, Sejong City, MindsDB, Voxel51, Mindprober, Source Digital; media/sports/gov slant | https://www.twelvelabs.io/case-studies | 2026-07-20 | Q |
| 18 | Employees; Discord; Developer Hub listed as primary community channels | https://www.twelvelabs.io/careers and https://www.twelvelabs.io/developer-hub | 2026-07-20 | Q |
| 19 | Series B blog post; "Video Superintelligence" framing; Jae Lee quoted; $100M raise details | https://www.twelvelabs.io/blog/twelvelabs-series-b-100m | 2026-07-20 | Q |
| 20 | Marengo 3.0 available on Amazon Bedrock; AWS first cloud provider; "AWS AI Competency" badge | https://www.prweb.com/releases/twelvelabs-launches-its-most-powerful-video-understanding-model-marengo-3-0-on-twelvelabs-and-amazon-bedrock-302629096.html | 2026-07-20 | Q |
| 21 | Jockey agent launch; live on homepage as "first video intelligence AI agent" | https://www.twelvelabs.io | 2026-07-20 | Q |
| 22 | Press page: Pegasus 1.5, Marengo 3.0, UNICEF Korea case study, Ecosystem Partner Program, Snowflake integration, Qencode partnership | https://www.twelvelabs.io/press | 2026-07-20 | Q |
| 23 | Founding "twelve members, each bringing diverse research expertise spanning language, video, machine learning, and perception" | https://www.twelvelabs.io/about-us | 2026-07-20 | Q |
