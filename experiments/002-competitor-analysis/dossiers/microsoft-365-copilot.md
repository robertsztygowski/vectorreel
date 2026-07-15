# Microsoft 365 Copilot (Teams Premium) — ring: substitute · verified: 2026-07-15 · confidence: high

## 1. Vitals
**HQ/jurisdiction:** Microsoft Corp., Redmond, WA, USA (US public company, NASDAQ: MSFT). EU processing via the Microsoft EU Data Boundary; EU legal entity Microsoft Ireland Operations Ltd. for many EU contracts. **Founded:** 1975 (Copilot product line launched 2023; Teams Premium GA Feb 2023). **Funding:** N/A — mega-cap public company, not venture-funded. Market cap in the trillions; irrelevant as a "runway" signal. **Headcount:** ~228,000 company-wide (Microsoft, well-known public figure); Copilot/Teams product org is a large but unsplit fraction — unknown at product granularity. **Status:** Growing, aggressively. Microsoft 365 Copilot passed **20M paid enterprise seats by 2026-04-29**, +5M in the prior quarter (fastest 3-month growth since launch), reaching >90% of the Fortune 500 [T: Windows News/earnings]. This is the "do nothing / it's already in the tenant" incumbent — the single most dangerous substitute for mdreel's ICP.

## 2. Business model
**Model type:** Per-seat subscription (per-user/month, annual commitment cheaper than monthly). No usage/€-per-hour metering — recap/transcription are seat entitlements, not volume-priced. €/video-hour is **N/A** for this competitor by construction; the comparable unit is the per-seat price.

| Plan | Price (USD list) | Price (EUR ≈ ×0.90) | Included | Effective €/video-h | Overage |
|---|---|---|---|---|---|
| Teams Premium (add-on) | ~$10/user/mo | ~€9/user/mo | Intelligent recap, transcript, live translation (40+ langs), timeline/chapters | N/A (per-seat, unlimited meetings) | none |
| M365 Copilot Business (add-on) | $18/user/mo annual ($25.20 monthly) | ~€16.20 / ~€22.70 | Copilot across M365 + intelligent recap + custom recap templates + audio recap | N/A | none |
| M365 Business Standard + Copilot | $23.50/user/mo | ~€21.15 | Bundle: Office/Teams + Copilot | N/A | none |
| M365 Business Premium + Copilot | $32.00/user/mo | ~€28.80 | Bundle + security/device mgmt | N/A | none |

USD→EUR 0.90 applied throughout. **Entry price point:** effectively ~$10/user/mo (Teams Premium) for meeting recap alone; $18/user/mo/annual for full Copilot. **Free tier/trial:** No standing free tier for these paid capabilities; Microsoft runs time-boxed enterprise trials/promotions via sales, not self-serve. **Enterprise motion:** Sales-led (Enterprise Agreements, 15–30% negotiated discounts, CSP/partner channel) with self-serve available for SMB via the web store. **Metered that mdreel doesn't:** seats (the whole model). No per-hour, per-video, storage, or index-retention meter — this removes the volume-cost anxiety mdreel's €/h pricing creates, but also means it never produces a portable per-video artifact.

**Critical licensing nuance (verified):** Standard **intelligent recap requires either Teams Premium OR a Copilot license** [T: Microsoft Learn / UIowa ITS]. But **custom summary templates** and **audio recap** specifically require the **M365 Copilot** license, not Teams Premium alone. So "recap" is real at the ~$10 tier; the richer AI-note features push buyers up to the $18+ Copilot tier.

## 3. Product & features — checklist
- [x] transcript (live + saved, per-speaker)
- [ ] **verbatim ON-SCREEN text (slides/code/UI)** — no OCR of shared-screen content into structured text; recap works from audio/transcript, not pixels. **This is mdreel's core wedge.**
- [~] visual descriptions — limited; "topics"/chapters and AI notes summarize discussion, not on-screen visuals; screen content is not transcribed
- [x] timestamps — meeting timeline with speaker segments, chapter/topic markers, "when people joined/left," name-mention jump points (segment/topic granularity, not word-level export)
- [~] structured/Markdown output — AI notes / action items are structured **inside Teams/Loop**, not exported as portable Markdown files
- [~] JSON/API — no first-class "meeting recap → JSON" export; transcripts/artifacts reachable via Microsoft Graph API, but there's no clean bring-your-own-RAG Markdown export path
- [x] webhooks — via Graph change notifications / Power Automate (indirect)
- [?] MCP server — Microsoft is building Copilot/agent extensibility (Copilot Studio, agents), MCP support emerging across the ecosystem; no clean "recap-as-MCP" for third-party RAG — treat as unknown/not-for-this-purpose
- [x] connectors — deep, but inward-facing: SharePoint, OneDrive, Outlook, Teams, Graph connectors to third-party sources (the point is to feed *Copilot*, not to emit files to *your* RAG)
- [x] speaker ID (named attribution in transcript/recap)
- [x] languages — transcription/translation 40+ languages
- [x] max video length — bounded by meeting length; no user-facing hard cap (not positioned as a video-file processor)
- [ ] processing-speed claims — N/A (recap is post-meeting/near-real-time, not marketed as an ingestion throughput number)
- [x] retention/erasure controls — enterprise retention/compliance (Purview, retention policies, recap-without-saved-transcript option arriving mid-2026)
- [ ] self-host option — no; SaaS in Microsoft cloud only

**What the output actually looks like:** An in-Teams "Recap" tab per meeting — a video recording with an AI-generated timeline (chapters/topics, speaker segments, name-mention and screen-share markers), a bulleted AI summary, suggested action items/owners, and a searchable transcript. It lives inside Teams/Loop as an interactive surface, **not** as a portable `.md`/`.json` file you hand to your own vector DB. There is no verbatim rendering of slide/code/UI text shown on screen — the intelligence is derived from spoken audio + transcript.

## 4. Size & customer base — evidence, not vibes
**Seats/scale:** >20M paid M365 Copilot seats (2026-04-29), 41% of M365 enterprise customers adopting, >90% of Fortune 500 [T: Windows News summarizing MSFT FY2026 earnings]. **Named logos/case studies:** Accenture, Bayer, Johnson & Johnson, EY; Publicis Groupe cited at 95,000+ seats [T: earnings-call reporting / analyst blogs — treat exact seat figures as third-party]. **Reviews:** G2 and Capterra both host active Microsoft 365 Copilot review pages (2026), plus Gartner Peer Insights and TrustRadius; **exact star rating and review count unknown** (G2 page returned HTTP 403; do not fabricate). Gartner Peer Insights and TrustRadius list it in the generative-AI-apps category. **Web traffic:** unknown (not separately measured; microsoft.com is top-10 global but not product-attributable). **GitHub/community:** N/A for recap; broader Copilot developer community via Copilot Studio/Graph, not relevant here. **Hiring signals:** Continuous large-scale Copilot/AI hiring across Microsoft; not product-granular — unknown.

## 5. GTM & distribution
**Positioning (verbatim, pricing page):** *"Flexible Copilot plans for every organization."* Teams Premium is marketed as adding *"AI-powered meeting recaps, live translation … custom branding, and watermarking."* **Channels:** (1) The tenant itself — default distribution: it's already deployed where the ICP works, zero new vendor onboarding, no new DPA; (2) Enterprise field sales + EA renewals; (3) CSP/partner/reseller ecosystem; (4) massive SEO/owned-media footprint (Microsoft Learn, support docs dominate "Teams recap / intelligent recap license" SERPs); (5) in-product upsell (recap prompts nudging Teams Premium/Copilot trials). No standalone free tool or public gallery — it doesn't need one. **Who the pricing page talks to:** business leaders / org decision-makers and IT admins — procurement and productivity framing (seats, security, EA), **not** developers and **not** DPOs primarily. Compliance is a reassurance layer, not the headline.

## 6. EU/GDPR posture
**Hosting regions:** EU Data Boundary — for EU-provisioned tenants, Copilot prompts/responses and Graph-accessed data are processed and stored within the EU/EEA; Copilot added to Microsoft's data-residency commitments in Product Terms on 2024-03-01 [T: Microsoft Learn privacy doc]. **Multi-Geo** optional add-on for per-user placement. **DPA:** Yes — Microsoft Products & Services Data Protection Addendum (industry-standard, publicly available). **Subprocessor list:** Long, public, enterprise-grade. **No-training terms:** Yes, explicit — customer prompts/responses/Graph data are not used to train foundation models [Q-adjacent: Microsoft Learn]. **Certifications:** Extensive — ISO 27001/27017/27018/27701, SOC 1/2/3, GDPR, EU Data Boundary, etc. **Residency premium:** Base EU Data Boundary is included (not upcharged); Multi-Geo is a paid enterprise add-on.

**⚠️ The exploitable crack (feeds mdreel's A1):** As of 2026, **Anthropic models used as a Copilot subprocessor are excluded from the EU Data Boundary**, with the app-level setting **on by default for EU/EFTA/UK tenants created after 2026-03-25** — processing occurs **outside** the EU boundary. Additionally, **Flex Routing** can push Copilot LLM inferencing outside the EU boundary during peak demand (admin-disable-able) [T: Microsoft Learn + changepilot/hubsite365 reporting]. So the "it's all EU" story now has documented asterisks a DPO can be shown. mdreel's "every step in EU, Vertex Gemini, no US transfer, ever" is a cleaner claim than Copilot can currently make.

## 7. Threat assessment
**ICP overlap:** **High.** Every EU software/IT team of 50–500 that mdreel targets is overwhelmingly a Microsoft 365 tenant with Teams. Copilot/Teams Premium is the default "we already have something for meetings" objection — the substitute that wins by inertia and zero procurement friction.

**What they'd have to do to kill mdreel:** Add (a) verbatim OCR of on-screen slide/code/UI text, spoken-vs-shown separated, and (b) a portable Markdown/JSON export for bring-your-own-RAG. **Likelihood: low-to-medium and slow.** Both cut against Microsoft's strategic grain: Copilot's entire thesis is *keep the data and the RAG inside the Microsoft graph*, not emit portable files that feed a competitor's vector DB. They also process meeting **streams**, not arbitrary uploaded demo/training video files — mdreel's ingestion surface (any internal video → file) is outside Teams' native scope. A generic "summarize any video" feature is plausible eventually, but verbatim on-screen-text fidelity + no-lock-in export is not where the roadmap points.

**What mdreel does that they structurally can't (or won't):** (1) Verbatim on-screen text extraction (slides/code/UI) as first-class output; (2) portable Markdown, no lock-in, bring-your-own-RAG — the anti-graph; (3) a **cleaner, asterisk-free EU-residency claim** (no Anthropic-subprocessor / Flex-Routing carve-outs); (4) ingest *any* internal video file, not just live Teams meetings; (5) transparent per-hour COGS/pricing vs. per-seat lock-in.

**What mdreel should steal:** (1) The timeline/chapter + name-mention jump UX for navigating long recordings; (2) speaker-attributed structure; (3) "recap without saving the raw transcript" as a compliance-forward option (mirror as an erasure/retention control); (4) the framing that the artifact should be *searchable and structured*, then go further by making it *portable*.

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
|---|---|---|---|---|
| 1 | Copilot Business add-on $18/user/mo annual ($25.20 monthly); bundles $23.50 / $32 | https://www.microsoft.com/en-us/microsoft-365-copilot/pricing | 2026-07-15 | Q |
| 2 | Pricing-page tagline "Flexible Copilot plans for every organization"; addressed to business leaders/IT | https://www.microsoft.com/en-us/microsoft-365-copilot/pricing | 2026-07-15 | Q |
| 3 | Teams Premium list ~$10/user/mo, adds AI recap, 40+ lang translation, branding/watermark | https://www.microsoft.com/en-us/microsoft-teams/premium | 2026-07-15 | Q |
| 4 | Teams Premium ~$10/user/mo; annual billing, ~5% monthly uplift; EA discounts 15–30% | https://thenegotiationexperts.com/blog/microsoft-teams-licensing-premium/ | 2026-07-15 | T (Negotiation Experts) |
| 5 | Intelligent recap requires Teams Premium OR Copilot license | https://learn.microsoft.com/en-us/microsoftteams/intelligent-recap-calls-meetings | 2026-07-15 | Q |
| 6 | Custom recap templates + audio recap specifically require Copilot (not Teams Premium alone) | https://its.uiowa.edu/news/2026/01/teams-premium-vs-microsoft-365-copilot-teams-meetings | 2026-07-15 | T (Univ. of Iowa ITS) |
| 7 | Copilot in EU Data Boundary; added to residency commitments 2024-03-01; no training on customer data | https://learn.microsoft.com/en-us/microsoft-365/copilot/microsoft-365-copilot-privacy | 2026-07-15 | Q |
| 8 | Anthropic subprocessor models excluded from EU Data Boundary; on by default for EU/UK tenants after 2026-03-25 | https://changepilot.cloud/blog/microsoft-365-copilot-flex-routing-eu-data-boundary-mc1269223 | 2026-07-15 | T (ChangePilot / MC1269223) |
| 9 | Flex Routing can process outside EU Data Boundary at peak; admin-disable-able | https://www.hubsite365.com/en-ww/crm-pages/european-union-vs-microsoft-copilot-gdpr-eu-data-boundary-anthropic-and-flexible-routing.htm | 2026-07-15 | T (HubSite365) |
| 10 | >20M paid Copilot seats by 2026-04-29; +5M prior quarter; >90% Fortune 500; 41% of M365 enterprise | https://windowsnews.ai/article/microsoft-365-copilot-hits-20m-paid-seats-enterprise-ai-adoption-governance-roi.415952 | 2026-07-15 | T (Windows News, citing MSFT earnings) |
| 11 | Named adopters Accenture, Bayer, J&J, EY; Publicis 95,000+ seats | https://valueaddvc.com/blog/microsoft-copilot-enterprise-adoption-what-the-data-shows-about-real-usage-vs-hype | 2026-07-15 | T (ValueAdd VC, citing earnings) |
| 12 | G2/Capterra/Gartner/TrustRadius host active Copilot review pages (exact count/rating unknown — G2 403) | https://www.g2.com/products/microsoft-microsoft-365-copilot/reviews | 2026-07-15 | T (G2, page inaccessible) |
| 13 | Microsoft headcount ~228,000 (company-wide, product-level unknown) | https://www.microsoft.com/en-us/microsoft-365-copilot/pricing | 2026-07-15 | E (well-known public figure; not product-granular) |
| 14 | €/video-hour N/A — per-seat model, no volume metering (derivation: all plans priced per user/mo) | https://www.microsoft.com/en-us/microsoft-365-copilot/pricing | 2026-07-15 | E |