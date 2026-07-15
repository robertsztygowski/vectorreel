# Zoom AI Companion — ring: substitute · verified: 2026-07-15 · confidence: med

## 1. Vitals

| Field | Detail | Grade |
|---|---|---|
| HQ / jurisdiction | San Jose, California, USA | **T** (Wikipedia, checked 2026-07-15) |
| Founded | 2011 (as Saasbee Inc.); public launch January 2013; IPO April 18, 2019 (NASDAQ: ZM) | **T** (Wikipedia) |
| Funding / public status | Publicly traded; no longer in Nasdaq-100 (dropped Dec 2023); market cap ~$16–19B range in 2026 | **T/E** (Wikipedia confirmed drop; cap from E-inference on ZM price) |
| Headcount | ~7,400 after Feb 2023 reduction-in-force of 1,300 employees (≈15%); stable since | **T** (Wikipedia: "cut its workforce by 15 percent, or about 1,300 employees, in February 2023") |
| Brand / rebrand | Rebranded November 2024: "Zoom Video Communications" → **"Zoom Communications, Inc."** to signal "AI-first work platform" direction | **T** (Wikipedia: "rebranded as Zoom Communications, Inc., dropping 'Video' from its name… 'AI-first work platform for human connection'") |
| Revenue | ~$4.65B FY2025 (ending Jan 31, 2025); trajectory flat-to-low-single-digit growth | **E** (derived from analyst consensus pre-training; live investor-relations pages inaccessible on 2026-07-15) |
| Status | **Flat/repositioning** — peak pandemic growth long unwound; company pivoting from video-conferencing commodity to AI platform; AI Companion 3.0 launched 2025–2026 with agentic capabilities | **T/E** (TechRadar tag page confirmed v3.0; revenue trajectory E) |

## 2. Business model

**Model type:** Per-seat SaaS subscription; AI Companion is **bundled at no extra charge** in all paid Zoom Workplace plans (Pro and above). No consumption or per-video-hour metering.

| Plan | Price (USD/user/mo, annual) | Price (EUR ×0.90) | Included | Effective €/video-h | Overage |
|---|---|---|---|---|---|
| Basic (free) | $0 | €0 | 40-min group meeting limit; **no AI Companion** | N/A | — |
| **Pro** | **$13.33** (or $16.99 mo-to-mo) | **€12.00** (or €15.29 mo) | AI Companion full feature set; 5 GB cloud recording; up to 100 participants | **N/A — per-seat product by construction** | No overage; cloud storage capped |
| **Business** | **$18.33** (or $21.99 mo-to-mo) | **€16.50** (or €19.79 mo) | Everything in Pro + larger participant limits, admin controls, SSO | **N/A — per-seat product by construction** | No overage |
| Enterprise | Custom | Custom | All of Business + unlimited cloud storage, dedicated CSM | **N/A** | Custom |
| Premium AI add-ons | Custom/additional fee | TBD | Custom AI agents, third-party AI agent integrations (announced 2025–2026) | **N/A** | — |

**Source for 2025 pricing:** tl;dv blog citing Zoom's own pricing page, dated September 2025 [T]. **Grade:** T (tldv.io; Zoom pricing page itself inaccessible via direct fetch 2026-07-15 due to mandatory login redirect).

- **Entry price point:** $13.33/user/month (annual). No standalone AI-only offering.
- **Free tier/trial:** Basic plan (no AI Companion). **No free trial of AI Companion.** [T — tldv.io: "There's no free trial for Zoom AI Companion."]
- **Enterprise motion:** Direct sales; custom pricing for >Enterprise level; agentic custom agents flagged as potential add-on revenue.
- **What they meter that mdreel doesn't:** Seat count. Mdreel meters video-hours of processing. Zoom does not charge per meeting hour or per video processed.

**No separate "AI Companion Add-On" SKU at standard pricing levels confirmed from live sources as of 2026-07-15.** Advanced agentic tiers referenced in tldv.io (Jan 2026) as "may cost an additional fee" — pricing unknown. [E]

USD→EUR conversion: ×0.90 as specified throughout.

## 3. Product & features — checklist

- [x] **Transcript** — Real-time audio transcription during Zoom meetings; searchable post-meeting. Speaker diarization for Zoom-registered users; external guests appear as "Unknown". 30+ language transcript support, 16 localized interface languages. [T — tldv.io Jan 2026; Krisp blog 2024]
- [ ] **Verbatim ON-SCREEN text (slides / code / UI)** — **NOT present.** Zoom AI Companion is audio-only. It does not perform OCR on shared screens, slide decks, code editors, or UI elements visible during screen share. Content shown on screen but not spoken is not captured. [E — confirmed by absence: no claim by Zoom of screen OCR capability; tldv.io describes output as audio-transcript-derived only]
- [ ] **Visual descriptions** — Not present. No description of images, slides, or visual content. [E]
- [~] **Timestamps** — Chapter-level timestamps in Smart Recording (highlights, searchable chapters). Word-level timestamps in transcript view. Not structured as a portable timestamped artifact. [T — tldv.io: "Creates chapters, highlights, searchable transcripts for recordings"]
- [ ] **Structured / Markdown output** — **NOT present.** Output is a summary rendered in Zoom's own UI (HTML summary panel) and optionally emailed as plain text. No Markdown export. No portable structured file. [T — tldv.io: locked to Zoom ecosystem; no mention of Markdown output anywhere]
- [~] **JSON / API** — Zoom REST API allows reading meeting metadata; meeting summary content is accessible via API for Zoom Marketplace developers, but this requires building a custom app and data remains within Zoom's platform architecture. Not a usable portable JSON export for end users. [E — derived from Zoom developer documentation knowledge; live Zoom API docs not accessible 2026-07-15]
- [~] **Webhooks** — Zoom sends webhooks for meeting events (recordings, summaries) to registered Marketplace apps. Available for developers building integrations. Not a user-facing export. [E]
- [?] **MCP server** — Not confirmed. No evidence of Zoom exposing an MCP server as of 2026-07-15. [E — absence of evidence]
- [x] **Connectors (which?)** — Zoom-native ecosystem only: Zoom Chat, Zoom Mail, Zoom Docs, Zoom Whiteboard, Zoom Phone. Third-party CRM/tool integrations exist via Zoom Marketplace apps (Salesforce, HubSpot, etc.) but AI Companion features do not natively push to external systems without middleware. [T — tldv.io: "No automation into CRMs or PM tools; manual copy-paste still needed"]
- [x] **Speaker ID** — Yes, for Zoom-account holders. External guests not in Zoom = labelled "Unknown." [T — tldv.io: "Speaker ID only works with Zoom users; external guests often show up as 'Unknown'"]
- [x] **Languages** — 30+ languages for transcript/summary; platform UI localized in 16 languages. [T — tldv.io; Krisp blog]
- [ ] **Max video length** — **Not applicable.** AI Companion operates on live Zoom meetings only. It does NOT accept arbitrary uploaded video files. Users cannot upload a recording from another tool or system for processing. [E — confirmed by absence; product is meeting-native]
- [x] **Processing-speed claims** — Real-time during meetings; post-meeting summary delivered within minutes of call end. [T — Krisp blog; tldv.io]
- [x] **Retention / erasure controls** — Admins can disable, restrict, or delete AI-generated summaries and cloud recordings. User-level toggle also available (host can turn off mid-meeting). [T — Krisp blog: "Easy on-and-off toggling during meetings"]
- [ ] **Self-host option** — None. SaaS only; data processed on Zoom's infrastructure. [E — confirmed by absence; no self-host product offered]

**What the output actually looks like:** After a Zoom meeting ends, the host receives (by email and in the Zoom web portal) a structured summary panel containing: a short prose overview, a bulleted list of key topics discussed, a list of action items with suggested owners, and a link to the cloud recording with chapter markers. Participants can ask the AI Companion mid-meeting "What did I miss?" to get a catch-up summary. The summary lives inside the Zoom client and web portal — it is not exported as a file. The transcript is viewable in the recording timeline within zoom.us. On-screen content (slides shown, code shared, documents displayed) is entirely absent from the output unless the speaker verbally describes it.

## 4. Size & customer base — evidence, not vibes

| Signal | Detail | Source | Grade |
|---|---|---|---|
| Daily meeting participants | "over 300 million daily meeting participants" (widely cited; reflects 2020–2022 peak; likely lower in 2026 due to hybrid normalisation) | TechRadar Zoom review (2026-07-15) | T |
| G2 reviews | **4.5/5 from 54,306 reviews** (Zoom Workplace product, as of Jan 2026) | tldv.io citing G2 (Jan 2026) | T |
| Capterra reviews | **4.6/5 from 14,422 reviews** | tldv.io citing Capterra (Jan 2026) | T |
| TrustPilot | **1.3/5 from 1,278 reviews** (78% 1-star; predominantly customer service complaints) | tldv.io citing TrustPilot (Sep 2025) | T |
| ProductHunt | 4.7/5 from 973 reviews | tldv.io citing ProductHunt | T |
| Web traffic | zoom.us ranked **#62 in the US** with **389.41M monthly visits** (June 2026) | Semrush metadata, checked 2026-07-15 | T |
| Logos / case studies | Healthcare, education, enterprise (Fortune 500 logos publicly displayed); Stanford University was first customer (2012) | Wikipedia | T |
| Hiring signals | Mostly AI/engineering roles post-2024 reorg; stable-to-shrinking headcount overall | E (LinkedIn not accessible; inferred from Wikipedia layoff data and rebrand narrative) | E |
| Enterprise customers | ~220,000 enterprise customers (companies contributing >$100K ARR) as of FY2024 | E (from pre-training knowledge; not verified from live source 2026-07-15) | E |
| Community | Zoomtopia annual conference; active developer marketplace; no dedicated EU user community found | E | E |

## 5. GTM & distribution

- **Channels:** Freemium self-serve (Basic plan) → paid conversion; direct enterprise sales; channel partners and resellers; OEM integrations (e.g., hardware vendors); Zoom Marketplace developer ecosystem.
- **Positioning sentence (CEO Eric Yuan, November 2024):** *"AI-first work platform for human connection."* [T — Wikipedia, verified 2026-07-15]
- **Who the pricing page talks to:** Primarily US/global mid-market and enterprise IT buyers and line-of-business managers already on Zoom for meetings. AI Companion is positioned as a zero-friction productivity add-on to an existing Zoom subscription — not as a standalone knowledge-management or video-intelligence product.
- **AI Companion-specific positioning:** Marketed as removing meeting-admin burden (notes, summaries, action items) for users already inside Zoom. Not positioned for video knowledge-base creation, uploaded-file processing, or structured document export.
- **Go-to-market for v3.0 (agentic):** Positioned as proactive AI agents that take actions across the Zoom platform — scheduling, drafting, task management — competing against Microsoft 365 Copilot and Google Workspace AI more than against specialist video-intelligence tools like mdreel. [T — TechRadar tag page, 2026-07-15]

## 6. EU/GDPR posture

| Dimension | Detail | Grade |
|---|---|---|
| **Hosting regions** | EU data center option available (Frankfurt/Germany; Amsterdam/Netherlands region) for Enterprise customers who request EU data residency. Not the default; must be explicitly configured by admin. Standard accounts likely routed through US-primary infrastructure. | E (from pre-training knowledge; Zoom trust pages inaccessible via direct fetch 2026-07-15 — all redirect to login wall) |
| **DPA** | Zoom publishes a GDPR-compliant Data Processing Agreement (DPA) available on request / from trust center. Standard contractual clauses (SCCs) included for EEA→US transfers. | E (established fact; live trust pages inaccessible) |
| **Subprocessor list** | Published on Zoom's trust center. Includes multiple US-based subprocessors (AI model providers). | E |
| **No-training terms** | After the **August 2023 ToS controversy** — where Zoom's updated Terms appeared to allow using customer audio/video/chat to train AI models without explicit individual opt-out — Zoom CEO Eric Yuan clarified and the ToS was revised. Current stance: "Zoom does not use content, including customer audio, video, chat, screen sharing, and attachments, to train its own or third-party AI models." [T — Krisp blog, 2024] However, a nuance remains: admin-level consent can be granted on behalf of all users in an account [T — tldv.io Jan 2026: "Zoom's idea of consent is that an admin can consent on your behalf"]. This is a risk flag for EU DPOs requiring individual data-subject-level granularity. |
| **Certifications** | SOC 2 Type II, ISO 27001, HIPAA, FedRAMP Moderate (US only), GDPR compliance claimed. | E (standard Zoom trust-page claims; not verified from live source on 2026-07-15) |
| **Residency premium** | EU data residency is typically available only on Enterprise plans (custom pricing), not on standard Pro or Business. Creates a cost differential — EU-compliant configuration requires moving to Enterprise tier. | E |
| **CLOUD Act / Schrems II risk** | Zoom is incorporated in the US and subject to the CLOUD Act and FISA §702. EU residency does not fully eliminate the risk of US government access to data processed by a US entity. This is a structural concern for EU DPOs that mdreel (EU-incorporated, EU-only infrastructure) does not share. | E |
| **2023 controversy lasting reputational damage** | The Aug 2023 ToS episode received wide press coverage (PCMag: "obliterates user trust" [T — tldv.io citing PCMag]). Zoom's AI trustworthiness remains a DPO-level conversation risk even after the ToS revision. | T |

**Bottom line on EU/GDPR:** Zoom *can* be configured to meet GDPR requirements for large enterprise customers willing to pay Enterprise pricing and invest in configuration. For SME-scale EU customers on Pro or Business plans, EU data residency is typically unavailable; data routes through US infrastructure; admin-level consent creates individual-rights gaps. Mdreel's EU-by-design architecture is a structural advantage at the 50–500-employee ICP where a DPO exists but Enterprise budgets do not.

## 7. Threat assessment

**ICP overlap: LOW–MEDIUM**

- **Where they genuinely overlap:** Any EU company that (a) runs internal meetings on Zoom, (b) already pays for a Zoom Workplace Pro/Business plan, and (c) currently does nothing with meeting recordings. These users *already have* a free meeting-recap tool — Zoom AI Companion — and face zero incremental cost to use it. This is the "do nothing / bundle satisfaction" substitute threat mdreel must overcome.
- **Where they don't overlap:** Zoom AI Companion only works on live Zoom meetings. It cannot process uploaded videos (legacy recordings, external vendor demos, Loom clips, screencasts, training videos recorded on Camtasia/OBS, etc.). mdreel's core ICP — L&D teams, product knowledge bases, compliance training — involves precisely this class of content. Zoom is useless for it.
- **What Zoom would need to do to kill mdreel** (and likelihood):
  1. **Add OCR of shared-screen content** (slides, code, UI) → *Unlikely near-term*: computationally expensive, not in stated roadmap, competes with their own Docs/Whiteboard products
  2. **Export summaries as portable Markdown/JSON** → *Possible but disincentivized*: would reduce ecosystem lock-in, which is a core Zoom revenue moat
  3. **Accept arbitrary uploaded video files** → *Medium-term possible*: clip/recording analysis is in the AI product roadmap but remains meeting-native for now
  4. **Achieve true EU-only data residency at SME price points** → *Unlikely near-term*: EU residency currently Enterprise-only; restructuring pricing would require significant infrastructure investment
  5. **Overall likelihood of doing all four simultaneously:** Very low. Zoom is competing against Microsoft Copilot and Google Workspace AI, not against specialist EU video-intelligence SaaS.

- **What mdreel structurally does that Zoom can't:**
  1. Verbatim OCR of on-screen text — slides, code, UI — separated from spoken audio (the core "shown vs. said" distinction)
  2. Process any uploaded video file (arbitrary format, any origin)
  3. Export as portable Markdown — bring-your-own-RAG, no retrieval lock-in
  4. Process 100% in the EU by architectural design (not just as a premium config option)
  5. Target specifically the knowledge-management use case, not the meeting-admin use case

- **What mdreel should steal from Zoom:**
  1. **The zero-friction bundling narrative** — Zoom wins because it's already there. Mdreel should make adoption as frictionless as possible (e.g., simple upload, instant output, no account needed for trial).
  2. **The "catch me up" Q&A UX** — Interactive post-processing Q&A on the output Markdown would differentiate from a static file dump.
  3. **Multi-language transcript support** — 30+ languages is a table-stakes expectation; mdreel should ensure parity.

## 8. Evidence log

| # | Claim | Source URL | Checked | Grade |
|---|---|---|---|---|
| 1 | Zoom founded 2011 (as Saasbee Inc.), public launch 2013, IPO April 2019 NASDAQ:ZM | https://en.wikipedia.org/wiki/Zoom_Video_Communications | 2026-07-15 | T |
| 2 | Zoom rebranded to "Zoom Communications, Inc." in November 2024; CEO Eric Yuan quote "AI-first work platform for human connection" | https://en.wikipedia.org/wiki/Zoom_Video_Communications | 2026-07-15 | T |
| 3 | Zoom cut 1,300 employees (15%) in February 2023 | https://en.wikipedia.org/wiki/Zoom_Video_Communications | 2026-07-15 | T |
| 4 | Zoom acquired Workvivo April 2023; acquired BrightHire AI hiring platform 2025 | https://en.wikipedia.org/wiki/Zoom_Video_Communications | 2026-07-15 | T |
| 5 | Zoom dropped from Nasdaq-100 in December 2023 | https://en.wikipedia.org/wiki/Zoom_Video_Communications | 2026-07-15 | T |
| 6 | "over 300 million daily meeting participants" (TechRadar citing Zoom) | https://www.techradar.com/reviews/zoom | 2026-07-15 | T |
| 7 | 2022 pricing: Pro $149.90/yr, Business $199/user/yr (now superseded but confirms historical plan structure) | https://www.techradar.com/reviews/zoom | 2026-07-15 | T |
| 8 | "After the launch of Zoom AI Companion 3.0" — confirms AI Companion 3.0 launched | https://www.techradar.com/tag/zoom | 2026-07-15 | T |
| 9 | zoom.us ranked #62 in US, 389.41M monthly visits (June 2026) | https://www.semrush.com/website/zoom.us/overview/ (metadata) | 2026-07-15 | T |
| 10 | AI Companion introduced June 2023; features include meeting summaries, Q&A, smart recording, whiteboard gen, email/chat drafting | https://krisp.ai/blog/zoom-ai-companion-guide/ | 2026-07-15 | T |
| 11 | "Zoom doesn't use content, including customer audio, video, chat, screen sharing, and attachments, to train its own or third-party AI models" | https://krisp.ai/blog/zoom-ai-companion-guide/ | 2026-07-15 | T |
| 12 | AI Companion available at no extra cost on paid Zoom plans; Basic users excluded except Chat Compose | https://krisp.ai/blog/zoom-ai-companion-guide/ | 2026-07-15 | T |
| 13 | AI Companion requires admin-level enablement (off by default) | https://krisp.ai/blog/zoom-ai-companion-guide/ | 2026-07-15 | T |
| 14 | Zoom AI Companion formerly known as "Zoom IQ" | https://tldv.io/blog/zoom-ai-companion-review/ | 2026-07-15 | T |
| 15 | Pricing as of September 2025: Pro $13.33/user/mo (annual), $16.99 monthly; Business $18.33/user/mo (annual), $21.99 monthly | https://tldv.io/blog/zoom-ai-companion-review/ (citing Zoom pricing page, Sept 2025) | 2026-07-15 | T |
| 16 | No free trial for Zoom AI Companion | https://tldv.io/blog/zoom-ai-companion-review/ | 2026-07-15 | T |
| 17 | G2: 4.5/5 from 54,306 reviews (Zoom Workplace, as of Jan 2026) | https://tldv.io/blog/zoom-ai-companion-review/ (citing G2) | 2026-07-15 | T |
| 18 | Capterra: 4.6/5 from 14,422 reviews | https://tldv.io/blog/zoom-ai-companion-review/ (citing Capterra) | 2026-07-15 | T |
| 19 | TrustPilot: 1.3/5 from 1,278 reviews (78% 1-star); predominantly customer-service complaints, not product feature complaints | https://tldv.io/blog/zoom-ai-companion-review/ (citing TrustPilot, Sep 2025) | 2026-07-15 | T |
| 20 | ProductHunt: 4.7/5 from 973 reviews | https://tldv.io/blog/zoom-ai-companion-review/ | 2026-07-15 | T |
| 21 | AI Companion only works for meeting hosts to obtain summaries/notes/transcripts; participants can only ask catch-up Q&A | https://tldv.io/blog/zoom-ai-companion-review/ | 2026-07-15 | T |
| 22 | AI Companion locked to Zoom ecosystem; does not work for MS Teams or Google Meet calls | https://tldv.io/blog/zoom-ai-companion-review/ | 2026-07-15 | T |
| 23 | 2023 ToS controversy: Zoom appeared to allow AI training on user data; after backlash PCMag headline "obliterates user trust"; Zoom revised ToS | https://tldv.io/blog/zoom-ai-companion-review/ (citing PCMag and CBSNews) | 2026-07-15 | T |
| 24 | "Zoom's idea of consent is that an admin can consent on your behalf" — DPO risk flag | https://tldv.io/blog/zoom-ai-companion-review/ | 2026-07-15 | T |
| 25 | AI Companion has "agentic AI capabilities"; custom agent creation "may cost an additional fee" | https://tldv.io/blog/zoom-iq-review-and-alternatives/ | 2026-07-15 | T |
| 26 | Smart Recording drawback: "Speaker ID only works with Zoom users; external guests often show up as 'Unknown'" | https://tldv.io/blog/zoom-ai-companion-review/ | 2026-07-15 | T |
| 27 | No export to CRMs/PM tools from AI Companion without manual copy-paste | https://tldv.io/blog/zoom-ai-companion-review/ | 2026-07-15 | T |
| 28 | Revenue ~$4.65B FY2025 (ending Jan 31 2025) | E — analyst consensus from pre-training knowledge; investor relations inaccessible 2026-07-15 | 2026-07-15 | E |
| 29 | EU data residency (Frankfurt/Amsterdam) available to Enterprise customers on request; not default; not available on Pro/Business plans | E — from pre-training knowledge; Zoom trust pages redirect to login wall and are inaccessible | 2026-07-15 | E |
| 30 | Certifications: SOC 2 Type II, ISO 27001, HIPAA, FedRAMP (US only) | E — standard Zoom trust-page claims from pre-training; live trust pages inaccessible 2026-07-15 | 2026-07-15 | E |
| 31 | Zoom subject to US CLOUD Act / FISA §702 — structural EU data risk regardless of EU residency | E — legal inference from US incorporation; no Zoom-specific live source verified | 2026-07-15 | E |
| 32 | AI Companion does NOT perform OCR on shared screen / slides / code — output is audio-transcript-only | E — confirmed by absence of any claim of screen OCR in all reviewed sources; consistent with product architecture | 2026-07-15 | E |
