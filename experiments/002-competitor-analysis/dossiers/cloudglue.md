# Cloudglue — ring: direct · verified: 2026-07-15 · confidence: med

## 1. Vitals
- **HQ/jurisdiction:** San Francisco, CA, USA. Operator: **Aviary Inc.** (aviaryhq.com). US jurisdiction. [Q/T]
- **Founded:** 2024. [T: YC]
- **Founders:** **Amy Xiao** (Co-founder & CEO — ex-Snapchat, AWS, Arize AI ML/infra eng); co-founders reported as **Matt Pua** and **Kevin Dela Rosa** (per Tracxn). [T]
- **Funding:** **Y Combinator S24 batch** (standard YC deal ≈ $500k; not separately disclosed). No priced round, amount, or additional investors publicly disclosed. YC primary partner listed: Gustaf Alström. **Total raised: unknown.** [T: YC; E on YC standard deal]
- **Headcount:** **~4** (YC company page; Tracxn concurs "4 employees"). [T]
- **Status:** **Early-stage / growing** — YC-backed, active docs + changelog, MCP server shipped ~2 months ago, ProductHunt launch, at least one named customer (11x). Tiny team, pre-scale. Not a zombie; not yet proven at scale. [E from activity signals]

## 2. Business model
**Model type:** Usage-based **prepaid credit packs** (self-serve). Subscriptions gated behind "contact us." Credits are consumed per operation: transcribe/describe/extract/scene-segmentation = **4 credits/min each**; chat = 1 credit/req; search = 1 credit/search.

Prices are USD; €/h converted at **USD→EUR 0.90** and derived as pack price ÷ included video-hours (assuming credits spent on a single 4-credit/min operation).

| Plan | Price | Included | Effective €/video-h | Overage |
|---|---|---|---|---|
| Free | $0 | 200 cr / **25 min** | N/A (free) | none / upgrade |
| Mini | $15 | 1,000 cr / **2 h** | **€6.75** | buy next pack |
| Starter (Popular) | $45 | 3,000 cr / **6 h** | **€6.75** | buy next pack |
| Builder (−22%) | $350 | 30,000 cr / **63 h** | **€5.00** | buy next pack |
| Scale (−33%) | $600 | 60,000 cr / **125 h** | **€4.32** | buy next pack |
| Enterprise | contact | unknown | unknown | unknown |

- **Entry price point:** $15 (Mini) self-serve; **free 25-min tier** for trial (200 credits). [Q]
- **Free tier/trial shape:** Free forever pack (200 cr / 25 min video), no card implied. [Q]
- **Enterprise motion:** Self-serve credit packs up to Scale; **subscriptions & Enterprise are sales-led** ("contact us"). [Q]
- **Meters mdreel doesn't:** per-operation credits (a single video costs 4× if you run transcribe+describe+extract+segment), **chat queries** (1 cr) and **searches** (1 cr) metered separately, and **data lands in Cloudglue Collections + their retrieval stack** (index retention = lock-in). mdreel is a flat hourly cap with portable Markdown out. [Q/E]

## 3. Product & features — checklist
- [x] transcript (Describe) [Q]
- [x] **verbatim ON-SCREEN text** — "extract text visible on screen" / Scene Text Recognition documented [Q]
- [x] visual descriptions (+ audio descriptions, sound events) [Q]
- [x] timestamps — **granularity: unknown** (scene/shot + chapter segmentation; second-level implied but not stated) [E]
- [x] structured output — **JSON / custom schema**, not Markdown (Markdown not offered) [Q]
- [x] JSON/API — REST, Python + JS SDKs, Responses API, Playground, Schema Builder [Q]
- [x] webhooks [Q]
- [x] MCP server — official `@cloudglue/cloudglue-mcp-server` (@aviaryhq); tools: describe_video, extract_video_entities, segment_video_chapters, search_video_moments; MCP clients Claude/Cursor/Windsurf [Q]
- [x] connectors — **YouTube, TikTok, Loom URLs** (direct ingest); no S3/Dropbox/Drive documented [Q]
- [x] speaker ID — **diarization** documented [Q]; also face detection/matching
- [?] languages — **unknown** (not stated in docs) 
- [?] max video length — **unknown** (no documented cap)
- [x] processing-speed claims — "**2 hours of video in ~3 minutes**" (marketing/case-study) [T]
- [ ] retention/erasure controls — no product-level retention/deletion controls documented (generic EEA rights only) [E]
- [ ] self-host option — none; hosted API only [E]

**What the output looks like:** Extract returns **schema-conformant JSON** — e.g. a defined schema yielding `speakers[]` and `topics[]` lists — driven by a natural-language prompt or Schema Builder. Describe returns a structured object combining transcript, diarization, visual descriptions, audio/sound descriptions, and on-screen text; Chat Completions return answers with **playable citation timestamps** back into the source video. Output is designed to **land in Cloudglue Collections** for their native retrieval/chat, not exported as portable files.

## 4. Size & customer base — evidence, not vibes
- **Case studies/logos:** **11x** (AI SDR/"Alice" — video added to knowledge base from sales calls) is the one named reference found. Other marketing mentions "sales meetings / product demos" generically. [T]
- **Reviews:** No G2/Capterra profile with counted ratings found. Aggregator stubs exist (SaaSworthy, SourceForge, saasreviews.org) with **no substantive dated review counts**. **Effective rating: unknown.** [E]
- **Web traffic:** **unknown** (no Similarweb/Semrush figure obtainable). [unknown]
- **GitHub/community:** Dev-facing but **small** — MCP server repo ~**6 stars**; llms.txt, changelog, ProductHunt launch present. [T]
- **Hiring signals:** ~4 employees, no visible large hiring push found. [T/E]

## 5. GTM & distribution
- **Channels:** Developer-led / **PLG via free credit tier + self-serve packs**; **YC network**; **MCP/agent ecosystem distribution** (listed on Glama, mcp.so, mcpservers.org, lobehub, aibase — appears in "awesome MCP" directories); **llms.txt + Mintlify docs** for LLM/SEO discoverability; ProductHunt launch; YouTube channel. No visible paid-ad or free-standalone-tool play; no public gallery. [Q/T]
- **Positioning (verbatim):** **"The video context layer for AI."** Also: **"Cloudglue APIs make it easy to transform video and audio into structured data for LLM/RAG/AI applications"** and **"the only video context system designed to work natively with AI agents."** [Q]
- **Who the pricing page talks to:** the **developer/builder** — credits, SDKs, playground, "Builder"/"Scale" plan names, MCP tooling. Not team leads, not L&D, **not DPOs**. Pure US developer-infra buyer. [E]

## 6. EU/GDPR posture
- **Hosting regions:** **none stated** — no EU/data-residency region offered. [Q: absence]
- **DPA:** **no explicit DPA** advertised. [Q: absence]
- **Subprocessors:** short list — **PostHog, Stripe, Google** (note: PostHog = US-tied product analytics; Google = compute). [Q]
- **No-training term:** **none stated** contractually. [Q: absence]
- **Certifications:** none published (no SOC2/ISO found). [unknown]
- **Residency premium:** **does not market or charge** for EU residency — generic EEA data-subject rights only. Privacy policy effective 2025-04-12. [Q]

## 7. Threat assessment
- **ICP overlap: MED-HIGH.** Same core capability as mdreel's **direct ring** — video → structured, LLM-ready output *including verbatim on-screen text and diarization*. But their buyer is the **US developer building an agent**, not the **EU 50–500-person software/IT/L&D team with a DPO**. Feature overlap is high; buyer/geography overlap is where mdreel differentiates.
- **What they'd need to do to kill mdreel:** stand up **EU-region hosting + DPA + no-training contractual term + Markdown/portable export**, and go to market to EU compliance buyers. **Likelihood: low-to-medium near-term** — a 4-person US YC company optimizing for developer PLG is unlikely to prioritize EU data-residency plumbing and DPO-oriented sales; it's off their current axis. If they raise and expand, medium.
- **What mdreel structurally does that they can't (easily):** **EU-only processing (GCP europe-* + Vertex) with residency as the product**, **portable Markdown with no retrieval lock-in** (Cloudglue's value compounds when data stays in Collections — deliberate stickiness they won't undermine), **contractual no-training + DPA aimed at DPOs**, and **spoken-vs-shown separation** as an explicit output contract.
- **What mdreel should steal:** (1) **MCP server + llms.txt + directory listings** — cheap agent-ecosystem distribution mdreel should match immediately (A5). (2) **Schema Builder / prompt→JSON Extract** as a self-serve dev on-ramp. (3) **Playable citation timestamps** back into source video — strong UX proof for a knowledge-base buyer. (4) **Named case study (11x-style)** — a single concrete logo with a "learns from past calls" narrative. (5) **"2h in 3min" speed claim** framing (mdreel should publish its own metered speed).

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | Operator Aviary Inc. (US), aviaryhq.com; YC-backed; privacy policy eff. 2025-04-12; subprocessors PostHog/Stripe/Google; no EU region/DPA/no-training term | https://cloudglue.dev/privacy | 2026-07-15 | Q |
| 2 | Credit-pack pricing: Free 200cr/25min, Mini $15/1000cr/2h, Starter $45/3000cr/6h, Builder $350/30000cr/63h, Scale $600/60000cr/125h, Enterprise contact; ops at 4cr/min, chat 1/req, search 1/search | https://cloudglue.dev/pricing | 2026-07-15 | Q |
| 3 | Output: Describe (transcript, diarization, visual/audio descriptions, sound, on-screen text), Extract (schema→JSON), Segment, Search, Chat w/ playable citations, Responses API; Collections retrieval | https://docs.cloudglue.dev | 2026-07-15 | Q |
| 4 | Scene Text Recognition = "extract text visible on screen"; diarization; face detection/matching; YouTube/TikTok/Loom URL ingest; MCP clients Claude/Cursor/Windsurf; webhooks | https://docs.cloudglue.dev/introduction | 2026-07-15 | Q |
| 5 | Official MCP server `@cloudglue/cloudglue-mcp-server` (@aviaryhq); tools describe_video, extract_video_entities, segment_video_chapters, search_video_moments; REST + Python/JS SDKs; llms.txt; Playground; Schema Builder; Changelog; Mintlify docs | https://docs.cloudglue.dev | 2026-07-15 | Q |
| 6 | YC batch Summer 2024; founded 2024; Amy Xiao Co-founder & CEO (ex-Snapchat/AWS/Arize); team size 4; HQ San Francisco; status active; tagline "The video context layer for AI."; YC partner Gustaf Alström | https://www.ycombinator.com/companies/cloudglue | 2026-07-15 | T (Y Combinator) |
| 7 | Co-founders also listed as Matt Pua & Kevin Dela Rosa; ~4 employees; founded 2024; SF | https://tracxn.com/d/companies/cloudglue/ | 2026-07-15 | T (Tracxn) |
| 8 | Customer 11x uses Cloudglue to add video to knowledge base so AI SDR "Alice" learns from past sales calls; "2 hours of video in just 3 minutes" | https://www.aitoolscafe.com/tool/cloudglue | 2026-07-15 | T (AI Tools Cafe) |
| 9 | MCP server repo ~6 GitHub stars, TypeScript, MIT, authored by aviaryhq, ~2 months old | https://github.com/aviaryhq/cloudglue-mcp-server | 2026-07-15 | T (GitHub) |
| 10 | Effective €/h: Mini/Starter €6.75, Builder €5.00, Scale €4.32 (pack price × 0.90 USD→EUR ÷ included hours) | (derived from #2) | 2026-07-15 | E |
| 11 | Total funding / round amounts / additional investors: not disclosed | https://www.ycombinator.com/companies/cloudglue | 2026-07-15 | E (absence) |
| 12 | Web traffic, counted G2/Capterra reviews, supported languages, max video length, timestamp granularity: unknown/unsourced | — | 2026-07-15 | unknown |