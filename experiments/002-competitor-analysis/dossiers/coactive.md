# Coactive AI — ring: direct · verified: 2026-07-15 · confidence: med

## 1. Vitals

| Field | Detail | Grade |
|---|---|---|
| Legal entity | Coactive Systems Inc. | Q — privacy policy, coactive.ai/privacy-policy, 2026-07-15 |
| HQ / jurisdiction | 60 South Market Street, Suite 760, San Jose, CA 95113 — US | Q — privacy policy footer |
| Founded | 2021 | Q — Series B blog: "We founded Coactive in 2021", coactive.ai/blog/series-b |
| Funding — Seed / Series A | ~$14 M; investors: Andreessen Horowitz, Bessemer Venture Partners, Exceptional Capital; approximate date Sep 2022 | E — seed/A amount widely cited in press but direct source access failed during this research; investors inferred from Series B blog ("previous investors Andreessen Horowitz, Bessemer Venture Partners, and Exceptional Capital, all participating in this raise") |
| Funding — Series B | $30 M; co-led by Cherryrock Capital and Emerson Collective; significant participation from Greycroft; all prior investors also participated; date approximately Jan 2024 | Q for amount and investors (coactive.ai/blog/series-b, fetched 2026-07-15); E for date (not stated in blog; inferred from context and release-note chronology) |
| Total funding disclosed | ~$44 M | E — sum of two rounds above |
| Headcount | unknown — LinkedIn unreachable during this research; estimated 50–100 from funding stage and enterprise customer count | E |
| Status | Private, active, US-incorporated enterprise AI platform company. Post-Series B growth stage. | E — inferred from funding recency, active docs/product, recent release notes |

**Series B quote (verbatim):** *"Today we're excited to announce we are accelerating the path to a no metadata future with $30 million in Series B funding, cementing our position as the leading platform for analyzing images and videos. Cherryrock Capital, in its first investment, co-led this funding round with Emerson Collective. They were joined with significant participation from Greycroft, and previous investors Andreessen Horowitz, Bessemer Venture Partners, and Exceptional Capital, all participating in this raise."* — coactive.ai/blog/series-b [Q]

---

## 2. Business model

| Dimension | Detail | Grade |
|---|---|---|
| Model type | Enterprise SaaS — platform (Multimodal Application Platform / MAP). Sold via direct sales + AWS Marketplace. No self-serve tier visible. | Q — docs/site require "Request a demo" / "contact us" everywhere |
| Published pricing | None. All pages route to `sales@coactive.ai` or contact form. | Q — coactive.ai contact page, 2026-07-15 |
| AWS Marketplace ~$75 k/yr figure | Reported by third parties as approximate starting ACV via AWS Marketplace channel; this researcher could not load any AWS Marketplace listing for Coactive during this research session and cannot confirm or deny the figure directly. | T — cited in prompt as sourced from AWS Marketplace/third parties; unverified in this research |
| €/video-hour equivalent | **unknown** — enterprise contract pricing with no published unit economics; cannot derive €/video-hour from the ~$75 k/yr ACV figure even if confirmed. | — |
| Entry price | unknown (enterprise contract, minimum unknown) | — |
| Free / trial shape | "Trial version" of image and video search service mentioned in privacy policy (account requires email + password); no public sign-up link found on main site as of 2026-07-15. | Q — privacy-policy text, coactive.ai/privacy-policy |
| Enterprise motion | 100% sales-led: demo request → account executive → contract. AWS Marketplace as secondary procurement channel. | Q — every CTA on the site, 2026-07-15 |
| What they meter that mdreel doesn't | Assets/images ingested at scale (millions), video hours processed, number of datasets, number of encoders/models used, context studio packages, ad impression forecasts. No meter is published; enterprise contracts likely meter by asset volume or MAU. | E — inferred from architecture and docs |

---

## 3. Product & features — checklist

| Feature | Status | Note | Grade |
|---|---|---|---|
| Transcript (speech-to-text) | [x] | Full speech-to-text: semantic search AND exact-match search on transcripts; timestamps in ms returned. `POST /api/v1/search/text-to-transcript/exact-match` returns `speech_to_text_transcription`, `start_time_ms`, `end_time_ms`. | Q — API schema, docs.coactive.ai transcript-exact-match.md |
| Verbatim ON-SCREEN text (OCR) | [~] | Context Studio docs state the platform analyzes "visual scenes, spoken dialogue, and on-screen context together"; no discrete OCR endpoint found in the full API index; no `on_screen_text` field in any schema reviewed. Partial/unconfirmed. | Q for "on-screen context" language (context-studio/overview.md); E for absence of confirmed OCR feature |
| Visual descriptions / captions | [x] | Keyframe captioning endpoint (`POST /api/v0/video-summarization/datasets/{id}/videos/{id}/caption-keyframes`) used upstream of Narrative Metadata classification; visual concepts and dynamic tags generate semantic scene descriptions. | Q — narrative-metadata.md |
| Timestamps | [x] | Timestamps at keyframe, shot, scene, and audio-segment level; millisecond precision in transcript API. | Q — transcript API schema |
| Structured / Markdown output | [ ] | **No.** Output is platform-native: embeddings stored in Coactive's vector DB, queryable via SQL, exportable as JSON/CSV via Query Export API or S3 bundle (Context Studio). No Markdown output. Lock-in by design. | Q — query-engine.md, context-studio/overview.md |
| JSON / API | [x] | Full REST API at `api.coactive.ai`; versioned endpoints (v0, v1, v3 for tags); OpenAPI spec available. | Q — llms.txt index, 2026-07-15 |
| Webhooks | [?] | Not found in API index or docs. Unknown. | — |
| MCP server | [x] | MCP server documented at `https://docs.coactive.ai/_mcp/server`; also at `https://docs.coactive.ai/latest/_mcp/server`. Explicitly listed for Claude Code / Cursor integration. | Q — llms.txt header, docs.coactive.ai/latest/llms.txt |
| Connectors | [x] | S3 bucket Connections API (Beta as of Jun 2025); supports creating, testing, updating, deleting cloud storage connections; future support for other providers planned. SSO federated via Okta, Microsoft Entra ID, Google Workspace. | Q — release-notes.md, sso.md |
| Speaker ID / celebrity detection | [x] | Dedicated Celebrity Detection API: enroll persons, get persons in an image, identify celebrities in external images; not standard speaker-diarization but person-level face recognition. | Q — llms.txt API index |
| Languages | [~] | Agentic Search: English-only as of 2026-07-15 ("Keep queries in English — Agentic Search is English-only today"). Transcript search and visual search language support beyond English: unknown. | Q — agentic-search.md |
| Max video length | [x] | 5 hours maximum; files longer than 5 hours require special handling ("contact us"). Minimum 5 seconds. | Q — accepted-media-formats.md |
| Processing-speed claims | [x] | "Find clips in less than a second" stated in Series B blog; Agentic Search "returns in under 30 seconds". | Q — blog/series-b; release-notes.md |
| Retention / erasure controls | [~] | Account deletion by emailing support@coactive.ai; primary data expunged but aggregate data may persist on production servers; backup retention not time-bounded. No enterprise data-lifecycle SLA found publicly. | Q — privacy-policy |
| Self-host option | [x] | Fandom case study explicitly states: *"Enterprise-grade deployment: Coactive runs in a self-hosted environment, meeting Fandom's security and scale requirements."* | Q — coactive.ai/case-study/fandom |

**What the output actually looks like:** Coactive ingests video/image assets from S3 or public URLs and indexes them into a proprietary vector database with multimodal embeddings. Outputs are surfaced through (a) a web UI with semantic search results returning thumbnail keyframes, shot timecodes, and relevance scores; (b) a REST API returning JSON objects with asset IDs, timestamps, similarity scores, and metadata fields; and (c) a SQL query engine where users write `SELECT` statements against tables like `coactive_table_adv`, `dt_[tagname]_visual`, and `dt_[tagname]_transcript` to pull structured numeric scores at the keyframe/shot/video level, exportable as CSV/Parquet to S3. There is no human-readable narrative summary document, no portable Markdown file, and no output that survives removal from the Coactive platform.

---

## 4. Size & customer base

| Dimension | Detail | Grade |
|---|---|---|
| Named logos / case studies | **Fandom** (350 M unique visitors/mo, 250k+ wikis): 2.4 M images/month moderated; 88% of manual labeling automated; 500+ contractor hours/week saved. Self-hosted Coactive deployment. | Q — coactive.ai/case-study/fandom |
| Other customers | "Fortune 500 retailers, media & entertainment companies, and community platforms" — unnamed in Series B blog. | Q — blog/series-b |
| G2 reviews | unknown — G2 returned 403 during this research | — |
| Web traffic | unknown — SimilarWeb returned no content | — |
| Community | No public Slack/Discord/forum found; support via email/contact form. | E — absence of evidence |
| Hiring | unknown — LinkedIn unreachable; no careers page successfully loaded | — |

---

## 5. GTM & distribution

| Channel | Detail |
|---|---|
| Primary | Direct enterprise sales — all website CTAs route to demo request or `sales@coactive.ai` |
| Secondary | AWS Marketplace (procurement/billing channel for enterprise buyers) |
| Docs / MCP | Self-serve docs + MCP server for technical evaluation by data/ML practitioners once under contract |

**Positioning sentence (verbatim, from Series B blog):** *"Coactive unlocks the untapped potential of images and videos for applications ranging from intelligent search to video analytics – no metadata or tags required, creating an enterprise-grade operating system for visual content."* [Q — coactive.ai/blog/series-b]

**Who the site talks to:** Data scientists, machine learning engineers, and data practitioners at Fortune 500 and large tech-enabled enterprises — specifically teams drowning in unstructured visual-media archives (media libraries, user-generated image platforms, e-commerce catalogs). The messaging is always about *scale* ("millions of uploads", "massive volumes"), never about individual video knowledge capture or mid-market knowledge-management.

---

## 6. EU/GDPR posture

| Dimension | Detail | Grade |
|---|---|---|
| Hosting regions | US — S3 bucket visible at `coactive-public.s3.us-west-1.amazonaws.com` in docs video assets. Privacy policy: "Our Site hosted in the United States". API domain `api.coactive.ai` — US region unconfirmed by policy but implied. | Q — privacy-policy; Q — docs asset URL |
| DPA (Data Processing Agreement) | Not publicly available; no DPA link found on site as of 2026-07-15. Enterprise contracts may include custom DPAs — unknown. | E — absence of public DPA |
| EU data residency / Standard Contractual Clauses | Privacy policy explicitly states transfers to the US: *"If you choose to use our Site from the European Union or other regions of the world with laws governing data collection and use that may differ from U.S. law, then please note that you are transferring your personal information outside of those regions to the United States for storage and processing."* No EU residency option or SCCs mentioned publicly. | Q — privacy-policy |
| No-training guarantee | Not stated publicly. | — |
| Certifications | Not found publicly (SOC 2, ISO 27001 etc. — unknown; enterprise contracts likely address this). | — |
| Residency premium | unknown (enterprise contract) | — |
| Summary | Coactive is a US-jurisdiction company with US-first hosting. No public EU data residency option, no public DPA, and the privacy policy affirmatively discloses cross-border transfer to the US. EU customers would need bespoke contractual arrangements. | — |

---

## 7. Threat assessment

**ICP overlap with mdreel: LOW.**

Coactive targets large US-headquartered enterprises (Fortune 500, media groups, community platforms with hundreds of millions of MAUs) at implied ACV of ~$75 k+/yr, selling to data science / ML teams who manage million-asset media libraries. mdreel targets EU software/IT/L&D teams of 50–500 employees who need structured, human-readable knowledge capture from internal training and meeting videos — a completely different buyer, job-to-be-done, price point, and regulatory context.

**What Coactive would need to do to threaten mdreel directly:**
1. Build EU data residency + a public DPA (non-trivial legal/infra investment for a US company)
2. Add verbatim on-screen text (slide OCR, screen-recording text) — not in current product
3. Produce portable, human-readable Markdown output that customers can own outside the platform (antithetical to their platform/lock-in model)
4. Build self-serve pricing at €100–500/mo range (their entire GTM is sales-led enterprise)
5. Pivot messaging from "million-asset media intelligence" to "individual meeting/training knowledge capture"

**Likelihood: very low.** Coactive is doubling down on enterprise media intelligence (Context Studio for ad targeting, celebrity detection, brand safety at scale). Moving downmarket to mid-market EU knowledge management would contradict every product and GTM investment they've made.

**What mdreel structurally does that Coactive cannot currently match:**
- EU-processed, GDPR-native with visible data residency — mdreel's core promise
- Verbatim OCR of on-screen text (slides, code, dashboards) separated from spoken audio — not confirmed in Coactive
- Portable, ownable Markdown output — zero lock-in; Coactive's entire value is the proprietary platform
- Self-serve, instant activation with mid-market pricing — Coactive requires a sales cycle

**What mdreel should steal from Coactive:**
- **Agentic/conversational search interface** — Coactive's natural-language "search my library" UX is excellent; mdreel's Markdown-as-input to LLMs achieves similar results but a dedicated chat UI over processed transcripts could be a roadmap item
- **Concept training / custom classifiers** — lightweight active-learning loop where users label a few examples to teach the system a new category; powerful for L&D tagging (e.g., tag all videos where "compliance topic X" appears)
- **SQL analytics layer** — SQL queries over structured metadata (which keyframe has which tag; how many videos cover a topic) is genuinely useful at scale; could inspire a structured export schema for mdreel Markdown archives

---

## 8. Evidence log

| # | Claim | Source URL | Checked | Grade |
|---|---|---|---|---|
| 1 | Legal entity: Coactive Systems Inc. | https://www.coactive.ai/privacy-policy | 2026-07-15 | Q |
| 2 | HQ: 60 South Market Street, Suite 760, San Jose, CA 95113 | https://www.coactive.ai/privacy-policy | 2026-07-15 | Q |
| 3 | Founded 2021 | https://www.coactive.ai/blog/series-b | 2026-07-15 | Q |
| 4 | Series B: $30 M raised | https://www.coactive.ai/blog/series-b | 2026-07-15 | Q |
| 5 | Series B co-led by Cherryrock Capital and Emerson Collective; Greycroft + prior investors participating | https://www.coactive.ai/blog/series-b | 2026-07-15 | Q |
| 6 | Prior investors A16Z, Bessemer, Exceptional Capital (inferred from Series B "previous investors" language) | https://www.coactive.ai/blog/series-b | 2026-07-15 | E |
| 7 | Series A ~$14 M — amount widely cited; direct source inaccessible during this research | Multiple press references (inaccessible); inferred from context | 2026-07-15 | E |
| 8 | Total funding ~$44 M (sum of rounds) | Derived | 2026-07-15 | E |
| 9 | Product description: "Multimodal Application Platform for data practitioners…intelligent search, metadata generation and analytical queries" | https://docs.coactive.ai/docs-guides/introduction/overview.md | 2026-07-15 | Q |
| 10 | Max video length: 5 hours; min 5 seconds | https://docs.coactive.ai/docs-guides/getting-started/accepted-media-formats.md | 2026-07-15 | Q |
| 11 | Speech-to-text transcript search (semantic and exact match) with timestamps in ms | https://docs.coactive.ai/api-reference/api-reference/search/search/transcript-exact-match.md | 2026-07-15 | Q |
| 12 | "On-screen context" analyzed (Context Studio) — OCR not confirmed as discrete feature | https://docs.coactive.ai/docs-guides/core-features/context-studio/overview.md | 2026-07-15 | Q |
| 13 | No Markdown output; output is JSON API + SQL tables + S3 CSV/Parquet export | https://docs.coactive.ai/docs-guides/core-features/query-engine.md | 2026-07-15 | Q |
| 14 | MCP server at docs.coactive.ai/_mcp/server | https://docs.coactive.ai/latest/llms.txt | 2026-07-15 | Q |
| 15 | S3 Connections API (Beta, Jun 2025) | https://docs.coactive.ai/docs-guides/introduction/release-notes.md | 2026-07-15 | Q |
| 16 | SSO via SAML 2.0 / OIDC; Okta, Entra ID, Google Workspace | https://docs.coactive.ai/docs-guides/administration/single-sign-on-sso.md | 2026-07-15 | Q |
| 17 | Celebrity detection API | https://docs.coactive.ai/latest/llms.txt | 2026-07-15 | Q |
| 18 | Agentic Search English-only | https://docs.coactive.ai/docs-guides/core-features/agentic-search.md | 2026-07-15 | Q |
| 19 | "Find clips in less than a second" | https://www.coactive.ai/blog/series-b | 2026-07-15 | Q |
| 20 | Agentic Search results "under 30 seconds" | https://docs.coactive.ai/docs-guides/introduction/release-notes.md (March 6, 2026 entry) | 2026-07-15 | Q |
| 21 | Context Studio launched March 6, 2026 | https://docs.coactive.ai/docs-guides/introduction/release-notes.md | 2026-07-15 | Q |
| 22 | Fandom case study — 2.4 M images/month, 88% labeling automated, self-hosted Coactive | https://www.coactive.ai/case-study/fandom | 2026-07-15 | Q |
| 23 | Fandom VP quote: "Partnering with Coactive has transformed moderation from a bottleneck into a competitive advantage." | https://www.coactive.ai/case-study/fandom | 2026-07-15 | Q |
| 24 | Self-host option: "Coactive runs in a self-hosted environment, meeting Fandom's security and scale requirements" | https://www.coactive.ai/case-study/fandom | 2026-07-15 | Q |
| 25 | "Fortune 500 retailers, media & entertainment companies, and community platforms" as customer segments | https://www.coactive.ai/blog/series-b | 2026-07-15 | Q |
| 26 | US hosting: "Our Site hosted in the United States and intended for visitors located within the United States" | https://www.coactive.ai/privacy-policy | 2026-07-15 | Q |
| 27 | EU data transfer disclosed: "please note that you are transferring your personal information outside of those regions to the United States" | https://www.coactive.ai/privacy-policy | 2026-07-15 | Q |
| 28 | S3 asset region: us-west-1 | https://coactive-public.s3.us-west-1.amazonaws.com (visible in docs video src) | 2026-07-15 | Q |
| 29 | No public DPA found | Site search, 2026-07-15 | 2026-07-15 | E (absence) |
| 30 | Positioning sentence: "enterprise-grade operating system for visual content" | https://www.coactive.ai/blog/series-b | 2026-07-15 | Q |
| 31 | Narrative Metadata (mood, subject, genre, format) — experimental feature | https://docs.coactive.ai/docs-guides/experimental/narrative-metadata/narrative-metadata.md | 2026-07-15 | Q |
| 32 | Dynamic Tags — visual and transcript classifiers with relevance scores, SQL-queryable | https://docs.coactive.ai/docs-guides/core-features/dynamic-tags/overview.md | 2026-07-15 | Q |
| 33 | AWS Marketplace ~$75 k/yr ACV — reported by third parties; not directly verified in this research | Prompt citation (third-party source); AWS Marketplace page inaccessible | 2026-07-15 | T (unverified) |
| 34 | Trial version exists (privacy policy reference); no public sign-up link found | https://www.coactive.ai/privacy-policy | 2026-07-15 | Q |
| 35 | Headcount unknown — LinkedIn inaccessible | LinkedIn.com/company/coactive-ai (404) | 2026-07-15 | — |
