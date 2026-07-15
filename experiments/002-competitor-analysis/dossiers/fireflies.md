# Fireflies — ring: adjacent · verified: 2026-07-15 · confidence: high

## 1. Vitals
- **HQ/jurisdiction:** San Francisco, CA / Miami, FL, USA (US-incorporated; Crunchbase lists Miami, FL). No EU legal entity found.
- **Founded:** 2016. Founders Krish Ramineni (CEO) and Sam Udotong.
- **Funding:** ~$19M total across 5 rounds (Crunchbase). Seed $5M (2019-10-28); Series A ~$14M (2021-05-24). Lead/notable investors: Khosla Ventures, Canaan Partners (T: Crunchbase). No disclosed raise since 2021 — the company positions itself as profitable/bootstrapped-in-spirit; GetLatka estimates ~$10.9M ARR (2024, T, low confidence). PitchBook page exists but valuation not sourced here (a "$1B valuation" claim appears on GetLatka but is **unverified** — do not rely on it).
- **Headcount:** ~100–118 (LeadIQ ~118 Apr 2026; PitchBook ~106; Latka ~100). LinkedIn ~11.5k followers. Call it **~100–120**.
- **Status:** **Growing.** ~4.6M monthly site visits (Similarweb, Apr 2026), 746 G2 reviews at 4.7, claims 1M+ companies. Category leader in the AI-notetaker space with active product velocity (recently shipped MCP server). Not a zombie.

## 2. Business model
- **Model type:** Per-seat SaaS subscription (annual/monthly), metered by transcription **minutes/seat**, not €/hour. Unlimited transcription from Business up.
- **Pricing ladder** (USD/seat/mo, billed annual; USD→EUR ≈ 0.90 applied; €/video-h **N/A** — per-seat, not per-hour, so shown as per-seat €):

| Plan | Price (USD/seat/mo) | Included | Effective €/video-h | Overage |
|---|---|---|---|---|
| Free | $0 | 400 min storage/team, limited credits | N/A (≈ €0/seat) | — |
| Pro | $10 annual (≈ **€9/seat/mo**) | 8,000 min/seat storage, unlimited transcription | N/A — per-seat | none stated |
| Business (MOST POPULAR) | $19 annual (≈ **€17/seat/mo**) | Unlimited storage & transcription | N/A — per-seat | none |
| Enterprise | $39 annual (≈ **€35/seat/mo**) | Unlimited + admin/security | N/A — per-seat | custom |

- **Entry price point:** Free tier (real, generous — 800 min/user transcription historically, capped storage). Effective floor ≈ €9/seat/mo.
- **Enterprise motion:** Self-serve up to Business; **sales-led** for Enterprise (HIPAA-BAA, Private Storage, SSO gated here).
- **Meters mdreel doesn't:** **seats** (per-user pricing), **storage minutes** (Free/Pro caps), and AI-summary "credits" on lower tiers. mdreel meters video-hours, not seats — a structural pricing-axis difference.

## 3. Product & features
- [x] transcript (spoken audio; 95% accuracy claimed)
- [ ] verbatim ON-SCREEN text (slides/code/UI) — **no.** Fireflies transcribes spoken audio only; it captures video_url but does not OCR/extract on-screen text. **This is mdreel's core wedge.**
- [ ] visual descriptions — no (records video but does not describe/structure visual content)
- [x] timestamps — **sentence-level** (per-`sentence` objects); word-level not clearly exposed
- [~] structured/Markdown output — structured JSON via API + summaries/notes in-app; **not** portable Markdown files as a primary deliverable. Mark [?].
- [x] JSON/API — GraphQL API, JSON responses
- [x] webhooks — meeting-completion + super-admin team webhooks
- [x] MCP server — **yes**, remote OAuth server at `api.fireflies.ai/mcp` (transcripts, metadata, speakers, summaries, action items)
- [x] connectors — Salesforce, HubSpot, Slack, Asana, Trello, Notion, Lever/Greenhouse/BambooHR (ATS), Aircall, RingCentral, Zapier; MCP to Claude/ChatGPT/Cursor/Devin
- [x] speaker ID — speaker recognition + talk-time analytics
- [x] languages — **100+** (auto-detect); UI in EN/ES/DE/FR/PT-BR
- [~] max video length — upload limits: audio ≤200MB, **video ≤1.5GB (Pro/Enterprise)**; no explicit duration cap
- [x] processing-speed claims — real-time/live captions for ongoing meetings; async notes post-meeting
- [x] retention/erasure controls — 0-day retention (no training use), delete-anytime, storage caps by tier
- [ ] self-host — no (Enterprise "Private Storage" = choose data location, not self-hosting)

**Output shape:** A transcript is a JSON object (`id, title, date, duration, participants, sentences[], speakers[], summary, action_items, audio_url, video_url`). `sentences` carry speaker + timestamp; `summary` gives AI overview/action items. It is a **meeting-notes record for consumption inside Fireflies or via API**, not a portable timestamped Markdown artifact with spoken-vs-shown separation — the entire on-screen/verbatim-visual layer is absent.

## 4. Size & customer base
- **Logos/case studies:** Claims 1M+ companies; markets to Fortune 500 CIOs, sales/HR/eng teams. Specific named enterprise case-study logos not sourced here — **partly unknown**.
- **Reviews:** G2 **4.7 / 746 reviews** (T: G2, 2026); Capterra ~**4.9** (T: Capterra, count unknown). Strong.
- **Web traffic:** **~4.6M visits/mo** (T: Similarweb, Apr 2026), 54% direct — indicates strong brand/repeat, not purely SEO-dependent.
- **Community:** Not primarily dev-facing; no notable GitHub star base. LinkedIn ~11.5k followers.
- **Hiring:** ~100–120 headcount, up modestly YoY (Latka 96→~100+) — steady, not hypergrowth-hiring.

## 5. GTM & distribution
- **Positioning (verbatim):** *"The #1 AI Assistant For Your Meetings."*
- **Channels:** (1) Strong **brand/direct** (54% direct traffic); (2) **product-led free tier** as the funnel — the notetaker auto-joins calls, seeds virality via meeting participants; (3) huge **integrations/marketplace** footprint (CRM/ATS/Slack/Zapier) as distribution; (4) **SEO** on "AI meeting notes / notetaker / transcription" comparison terms; (5) **MCP + AI-tool ecosystem** (Claude/ChatGPT) as a new 2026 distribution surface; (6) affiliate/partner and app-marketplace listings.
- **Who the pricing page talks to:** the **team lead / ops buyer and end-user** (per-seat, "unlimited transcription," productivity framing), escalating to **CIO/security buyer** at Enterprise. Not developer-first, not DPO-first.

## 6. EU/GDPR posture
- **Hosting regions:** Not EU-specific by default. **"Private Storage" to store data "at your preferred location" is Enterprise-only** — i.e., EU residency is effectively a **paywalled enterprise feature**, not a standard offering, and specific EU regions are not published.
- **DPA:** Claims GDPR compliance ("in line with European regulations"); explicit self-serve DPA availability not confirmed on the security page (likely available on request — unverified).
- **Subprocessor list:** Not published on the security page; directed to trust.fireflies.ai.
- **No-training terms:** **Yes** — "0-day retention… never used for AI model training." Strong and marketed.
- **Certifications:** **SOC 2 Type II, GDPR (self-asserted), HIPAA-BAA (Enterprise only).** No ISO 27001 stated here.
- **Residency premium:** **Yes, implicitly** — EU/preferred-location data storage requires Enterprise ($39/seat + sales). They do **not** market EU residency as a headline; it is buried in enterprise security, not a purchase driver they lean on.

## 7. Threat assessment
- **ICP overlap: medium.** Both touch "video/meeting → structured text for downstream use," and Fireflies' MCP server now aims at the same "feed my AI knowledge base" job. **But** Fireflies targets *live meetings* and *per-seat productivity buyers*; mdreel targets *asset video libraries* (demos, trainings, recorded UI/code) with *verbatim on-screen extraction* and *per-hour batch* pricing. Different capture surface, different buyer, overlapping destination.
- **What they'd need to do to kill mdreel:** (a) add **on-screen/verbatim visual OCR + spoken-vs-shown separation** to process arbitrary uploaded video, (b) ship **portable Markdown export** as a first-class artifact, and (c) offer **standard EU data residency** (not enterprise-gated). Each is plausible but off their core roadmap (they optimize meeting-notes UX, not video-asset structuring). **Likelihood: low-to-medium** — they could bolt on OCR, but the whole meter/positioning is seat-based meeting-centric; pivoting to per-hour EU-resident video processing is a strategy change, not a feature.
- **What mdreel does that they structurally can't (easily):** verbatim on-screen text (slides/code/UI) extraction with spoken-vs-shown separation; per-video-hour economics (€0.65 COGS) decoupled from seats; **default EU-only residency as the product, not an enterprise upsell**; portable no-lock-in Markdown / bring-your-own-RAG. Their per-seat, US-hosted, meeting-notes model fights all four.
- **What mdreel should steal:** (1) the **MCP server** as a distribution surface into Claude/ChatGPT — table stakes now; (2) the **free-tier-as-funnel + integrations marketplace** PLG motion; (3) the **"0-day retention, never trained on"** privacy messaging, which mdreel can go further on with genuine EU residency; (4) their crisp category headline discipline.

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
|---|---|---|---|---|
| 1 | Pricing: Free / Pro $10 / Business $19 / Enterprise $39 annual per seat; unlimited transcription | https://fireflies.ai/pricing | 2026-07-15 | Q |
| 2 | Total funding ~$19M over 5 rounds; Khosla, Canaan investors | https://www.crunchbase.com/organization/fireflies | 2026-07-15 | T (Crunchbase) |
| 3 | Seed $5M 2019-10-28; Series A ~$14M 2021-05-24 | https://www.crunchbase.com/organization/fireflies | 2026-07-15 | T (Crunchbase) |
| 4 | HQ Miami, FL / US-incorporated | https://www.crunchbase.com/organization/fireflies | 2026-07-15 | T (Crunchbase) |
| 5 | Headcount ~100–118 | https://leadiq.com/c/firefliesai/5a1d886c24000024006220c7 | 2026-07-15 | T (LeadIQ/PitchBook/Latka) |
| 6 | ~ARR $10.9M (2024) | https://getlatka.com/companies/firefliesai | 2026-07-15 | T (GetLatka, low conf) |
| 7 | G2 4.7 / 746 reviews | https://www.g2.com/products/fireflies-ai/reviews | 2026-07-15 | T (G2) |
| 8 | Capterra ~4.9 | https://www.capterra.com/p/197037/Fireflies/reviews/ | 2026-07-15 | T (Capterra) |
| 9 | ~4.6M monthly visits, 54% direct | https://www.similarweb.com/website/fireflies.ai/ | 2026-07-15 | T (Similarweb) |
| 10 | Tagline "The #1 AI Assistant For Your Meetings"; 100+ languages; speaker recognition; connectors | https://fireflies.ai/ | 2026-07-15 | Q |
| 11 | MCP server at api.fireflies.ai/mcp; OAuth; transcripts/summaries/action items | https://docs.fireflies.ai/getting-started/mcp-configuration | 2026-07-15 | Q |
| 12 | MCP works with Claude/OpenAI/Cursor/Devin | https://fireflies.ai/blog/fireflies-mcp-server/ | 2026-07-15 | Q |
| 13 | Transcript JSON fields (sentences, speakers, summary, audio_url/video_url); sentence-level timestamps; upload audio ≤200MB / video ≤1.5GB; GraphQL; webhooks | https://docs.fireflies.ai/llms-full.txt | 2026-07-15 | Q |
| 14 | SOC 2 Type II, GDPR, HIPAA-BAA (Enterprise only); 0-day retention / no training; Private Storage = Enterprise | https://fireflies.ai/security | 2026-07-15 | Q |
| 15 | No EU data-residency option stated as standard; residency via enterprise Private Storage only | https://fireflies.ai/security | 2026-07-15 | Q |
| 16 | Webhooks configured in Developer settings; Make integration | https://www.make.com/en/integrations/fireflies-ai/gateway | 2026-07-15 | T (Make) |