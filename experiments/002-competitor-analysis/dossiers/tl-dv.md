# tl;dv — ring: adjacent · verified: 2026-07-15 · confidence: high

## 1. Vitals
- **HQ/jurisdiction:** Aachen, Germany (EU). German company — founder Raphael Allstadt states "tl;dv is a German company" publicly [T: LinkedIn]. Legal/marketing entity operates globally (remote workforce "across 6 continents").
- **Founded:** 2020, by Raphael Allstadt, Carlo Thissen, and Allan Bettarel [T: Tracxn].
- **Funding:** ~$5.5M total disclosed across ~3 rounds [T: Crunchbase]. Seed of ~$4.5M closed 2022-06-16; a Bridge round of ~$0.93M closed 2023-11-08 [T: Crunchbase]. Investors named: **Seedcamp, Shilling VC, K Fund** (6 investors total) [T: Crunchbase]. No round disclosed after Nov 2023 — funding looks stale/bootstrapped-to-revenue since.
- **Headcount:** ~50–61 employees; getLatka cites "50 person team" at $4.5M revenue (2024); Tracxn/other trackers report ~60 (Jan 2026) [T: getLatka, Tracxn]. LinkedIn ~13,450 followers [T: LinkedIn]. (PitchBook's "30" is an outlier.)
- **Status: Growing.** Rationale: high review velocity (1,400+ G2 reviews at 4.7), ~$4.5M revenue on a ~50-person team (2024), active promo engine, and continued product shipping (MCP server, AI coaching, EU/US AI hosting toggle). No fresh raise since 2023 suggests capital-efficient rather than blitz-scaling.

## 2. Business model
**Model type: per-seat subscription** (each person who records needs a license). Not usage/credit-metered for recording volume; instead gated by weekly recording caps and AI-feature tiers.

| Plan | Price (per seat) | Included | Effective €/video-h | Overage |
|---|---|---|---|---|
| Free | $0 | 10 AI notes lifetime; 40 recordings/week; 3-month auto-delete; Slack/email/calendar only | N/A (per-seat) | none |
| Pro | $29/mo monthly · **$18/mo annual** (≈**€16.20/mo** @0.90) | Unlimited AI notes, permanent storage, Zapier CRM sync, searchable library, unlimited downloads | N/A (per-seat) | none stated |
| Business | $98/mo monthly · **$59/mo annual** (≈**€53.10/mo** @0.90) | Native Salesforce/HubSpot, AI coaching, sales playbooks, Claude AI summaries, 120 recordings/week | N/A (per-seat) | none stated |
| Enterprise | Custom (sales-led) | Private/dedicated AI hosting, custom terms, dedicated support | N/A | custom |

- **USD→EUR:** converted at 0.90 as instructed. 40%-off annual promo has run since 2025 and is live in 2026 [T: Claap].
- **€/video-hour is N/A** — tl;dv is per-seat with (near-)unlimited recording, not hour-metered. The relevant comparable is **~€16/seat/mo (Pro) to ~€53/seat/mo (Business)**.
- **Entry price point:** Free tier (genuine, no time limit) → Pro at ~€16/seat/mo annual.
- **Free/trial shape:** Free forever but crippled (10 lifetime AI summaries, 90-day deletion). Land-and-expand via seats.
- **Enterprise motion:** Sales-led (contact sales; private AI hosting is the enterprise hook).
- **Meters mdreel doesn't:** **seats** (mdreel is hour-based), weekly **recording count** caps, **retention window** (3-month deletion on free), and **AI-note count** on free.

## 3. Product & features — checklist
- [x] transcript (real-time; claims ~96% accuracy clear English)
- [?] verbatim ON-SCREEN text — markets "slide capture" but **no disclosed structured/verbatim on-screen text (code/UI) extraction**; appears to be attach-the-slide, not OCR'd verbatim text [Q: features page mentions "slide capture" only]
- [ ] visual descriptions (no disclosure)
- [x] timestamps (moment-level; clip/highlight timestamps into meeting library)
- [~] structured/Markdown output — AI notes/summaries exported to Notion/Docs/Confluence; not positioned as portable structured-Markdown-for-RAG
- [x] JSON/API (Public API; webhook payloads "match the format used in tl;dv's Public API")
- [x] webhooks (Settings > Webhooks; JSON payload on meeting/transcript ready)
- [x] MCP server (documented in Integrations)
- [x] connectors — HubSpot, Salesforce, Pipedrive, Close, Attio, Copper; Slack, Teams, Notion, Confluence, Google Docs; Zapier/Make/n8n ("5000+")
- [x] speaker ID (audio-based auto-recognition; degrades in large/noisy calls)
- [x] languages (**30+**; EN/ES/PT/FR/DE/JA/KO/ZH/HI etc.)
- [x] max video length (**3 hours** per recorded/uploaded meeting)
- [x] processing-speed claims (real-time transcription during the live meeting)
- [x] retention/erasure controls (permanent storage on paid; 3-month auto-delete on free; GDPR erasure)
- [?] self-host (not offered; "private AI hosting" on Enterprise is managed, not customer self-host)

**Output reality:** The artifact is a **recorded meeting object** — video + timestamped transcript + AI-generated summary/notes/action-items, browsable in tl;dv's library and pushed into CRM/Notion/Docs. It is meeting-recap prose keyed to speaker turns and clip timestamps, optimized for sales/CRM workflows, **not** a portable verbatim spoken-vs-shown Markdown file for a RAG index. On-screen content is captured as attached slides, not extracted as verbatim on-screen text.

## 4. Size & customer base
- **Case studies/logos:** Positioned for revenue/sales teams; specific named enterprise logos unknown from primary source.
- **Reviews:** **G2 ~4.7/5 with 1,400+ reviews** [T: G2] — strong volume. Capterra listed [T: Capterra], exact count unknown. Product Hunt reviews present [T: Product Hunt].
- **Web traffic:** Estimates vary widely by source — HypeStat/Similarweb-derived cite ~120k–220k monthly visits; one source cites 1.33M (May) [T: HypeStat/Similarweb]. Treat as **high six figures monthly**, top source = direct (~63%), then organic search. Geo skew toward Brazil/India/US (i.e., a global freemium audience, not narrowly EU-B2B).
- **GitHub/community:** N/A (closed-source SaaS).
- **Hiring signals:** ~50→60 headcount trend suggests modest continued hiring; no fresh raise since 2023.

## 5. GTM & distribution
- **SEO is the dominant engine:** massive programmatic content farm — tl;dv's blog ranks for competitor/pricing terms far outside its own product (e.g., "ChatGPT pricing," "Claude Enterprise pricing," "Otter.ai pricing," "free AI note-taking"). This drives the large organic + direct traffic. **This is the single most transferable tactic** for mdreel's A5.
- **Free tool / freemium:** the Free forever tier is the top-of-funnel; land individual users, expand to seats/teams.
- **Integrations-led:** listed in Zapier/HubSpot/Salesforce marketplaces (partner-channel distribution).
- **Ads/gallery:** no public curated gallery; paid search likely on branded competitor terms (inferred from comparison content).
- **Positioning sentence (verbatim):** "**AI Meeting Notetaker for Zoom, Google Meet & Teams**" [Q: tldv.io]. Also "Welcome to the new meeting culture."
- **Who the pricing page talks to:** **team leads and revenue/sales ops** (CRM sync, sales coaching, playbooks) and individual knowledge workers — not developers, not DPOs. Developer surface (API/MCP/webhooks) exists but is not the pricing-page narrative.

## 6. EU/GDPR posture
- **Hosting:** "All our data centers are located in Europe" [Q: security page]. Infra spans **GCP, AWS, and Hetzner** [T]. Notable: customer can **choose AI hosting region — Europe or US** [Q: security page].
- **DPA:** Available (published; GDPR-compliant) [T: multiple; security page GDPR claims].
- **Subprocessors:** Published; **Anthropic (Claude)** named as an AI subprocessor, with anonymization + chunking/randomization safeguards [Q: security page].
- **No-training:** "No customer data is used to train the AI" [Q: security page].
- **Certifications:** **SOC 2 Type II**, GDPR, **EU AI Act compliant**; data centers ISO 27001 / PCI DSS L1 / SOC 1&2 [Q: security page].
- **Residency premium:** **EU hosting appears standard, not upcharged.** US AI hosting is offered as a compliance *option*, not an EU premium. **Private/dedicated AI hosting is gated to Enterprise** — that is where residency-control monetization sits. So they market GDPR heavily but do **not** charge a line-item EU premium below Enterprise.

## 7. Threat assessment
- **ICP overlap: MEDIUM.** Same buyer geography (EU) and same raw material (company video), and it is EU-founded + GDPR-forward, which directly pressures mdreel's A1 "EU is a purchase driver" wedge — a DPO who wants an EU notetaker already has a credible German option. **But** tl;dv's product is meeting-recap for **sales/CRM/L&D live calls**, per-seat, output = recap prose in a library/CRM. mdreel's ICP (IT teams turning demos/trainings into **structured Markdown for a BYO RAG index**, hour-based, no lock-in) is a different job. Overlap is on "EU + video + transcript," not on the structured-output/RAG use case.
- **What they'd need to do to kill mdreel:** ship (a) **verbatim on-screen text/code/UI extraction** (they only do "slide capture"), (b) **portable structured-Markdown export designed for RAG** with no lock-in, and (c) an **hour-based, non-per-seat** pricing model for batch video-library processing. Likelihood: **low–medium** — they *have* the API/MCP/EU/Anthropic plumbing to do it technically, but it cuts against their per-seat sales-recap business model and their programmatic-SEO growth machine. More likely they add shallow "export to Markdown" as a checkbox than re-target the RAG/knowledge-base ICP.
- **What mdreel does that they structurally can't (easily):** verbatim **spoken-vs-shown separation** and on-screen code/UI extraction; **hour-metered COGS-transparent pricing (€0.65/h COGS)** vs seat-locked; **no-lock-in portable Markdown** as the product (their moat is the CRM-synced library — lock-in is the point); processing **uploaded/library video at scale** rather than only live-call capture.
- **What mdreel should steal:** (1) the **programmatic-SEO content engine** ranking for adjacent high-intent terms — cheapest A5 lever available; (2) the **customer-chosen AI hosting region (EU/US) toggle** as a trust feature; (3) surfacing **SOC 2 Type II + EU AI Act + named subprocessor (Anthropic) + anonymization** prominently — this is exactly the DPO-facing proof mdreel sells on; (4) **MCP server** as a first-class integration (they already ship one).

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | EU-founded (Germany), GDPR-forward notetaker for Zoom/Meet/Teams; "slide capture" only, no structured on-screen-text extraction | https://tldv.io/features/meeting-recordings-transcriptions/ | 2026-07-15 | Q |
| 2 | HQ Aachen Germany; founded 2020 by Allstadt, Thissen, Bettarel | https://tracxn.com/d/companies/tldv/__PM-rlqNoAjpqedTJS9tA7onDXsfzmuI_4toKLP4l0Ns | 2026-07-15 | T (Tracxn) |
| 3 | Founder states "tl;dv is a German company" | https://www.linkedin.com/posts/allstadt_tldv-is-a-german-company-yet-almost-activity-7179061726468812800-8dXz | 2026-07-15 | T (LinkedIn) |
| 4 | ~$5.5M total; Seed ~$4.5M 2022-06-16; Bridge ~$0.93M 2023-11-08; Seedcamp/Shilling VC/K Fund | https://www.crunchbase.com/organization/tl-dv | 2026-07-15 | T (Crunchbase) |
| 5 | ~50-person team at $4.5M revenue (2024); ~60 employees | https://getlatka.com/companies/tldv.io | 2026-07-15 | T (getLatka) |
| 6 | LinkedIn ~13,450 followers | https://www.linkedin.com/company/tl-dv | 2026-07-15 | T (LinkedIn) |
| 7 | Per-seat pricing: Free $0; Pro $29 mo / $18 annual; Business $98 mo / $59 annual; Enterprise custom | https://www.claap.io/blog/tl-dv-pricing | 2026-07-15 | T (Claap) |
| 8 | Free = 10 AI notes lifetime, 40 rec/week, 90-day delete; Business = 120 rec/week, native SF/HubSpot, Claude summaries; 3h max per session | https://www.claap.io/blog/tl-dv-pricing | 2026-07-15 | T (Claap) |
| 9 | 40%-off annual promo live in 2026 | https://tldv.io/app/pricing/ | 2026-07-15 | Q |
| 10 | API, webhooks, MCP server; JSON payloads match Public API | https://intercom.help/tldv/en/articles/11583137-api-and-webhooks | 2026-07-15 | T (tl;dv Help Center) |
| 11 | Connectors: HubSpot/Salesforce/Pipedrive/Close/Attio/Copper; Slack/Teams/Notion/Confluence/Google Docs; Zapier/Make/n8n | https://tldv.io/integrations/ | 2026-07-15 | T (tl;dv integrations) |
| 12 | 30+ languages; auto speaker recognition; ~96% claimed accuracy | https://tldv.io/features/languages/ | 2026-07-15 | T (tl;dv features via search) |
| 13 | Data centers all in Europe; customer chooses AI hosting EU or US | https://tldv.io/features/security-commitment/ | 2026-07-15 | Q |
| 14 | No customer data trains AI; anonymization + chunking/randomization; Anthropic partner | https://tldv.io/features/security-commitment/ | 2026-07-15 | Q |
| 15 | SOC 2 Type II, GDPR, EU AI Act; data centers ISO 27001/PCI DSS L1/SOC 1&2 | https://tldv.io/features/security-commitment/ | 2026-07-15 | Q |
| 16 | Hosted on GCP, AWS, Hetzner; DPA + published subprocessors | https://tldv.io/blog/gdpr-compliant-meeting-assistants-you-can-actually-trust/ | 2026-07-15 | T (tl;dv blog) |
| 17 | G2 ~4.7/5, 1,400+ reviews | https://www.g2.com/products/tl-dv/reviews | 2026-07-15 | T (G2) |
| 18 | Monthly web traffic ~120k–220k (some sources higher); direct-led, then organic search | https://hypestat.com/info/tldv.io | 2026-07-15 | T (HypeStat/Similarweb) |
| 19 | Programmatic SEO on competitor/pricing terms (ChatGPT/Claude/Otter pricing) | https://tldv.io/blog/claude-enterprise-pricing/ | 2026-07-15 | Q |
| 20 | Positioning: "AI Meeting Notetaker for Zoom, Google Meet & Teams" | https://tldv.io/ | 2026-07-15 | Q |