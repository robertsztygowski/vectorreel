# Google Meet (Gemini notes) — ring: substitute · verified: 2026-07-15 · confidence: med

## 1. Vitals

| Field | Detail | Grade |
|---|---|---|
| HQ / jurisdiction | Mountain View, CA, USA · US law, Alphabet Inc. | Q |
| Founded | Google Meet GA 2017; "Workspace" brand 2020; "Take notes for me" GA January 2025 (bundled) | Q |
| Funding / public status | Alphabet Inc. (NASDAQ: GOOGL); ~$2 T market cap as of mid-2025 | Q |
| Headcount | Alphabet ~180,000 globally (2024 10-K); Workspace is a product unit, not standalone | T |
| Status | Live and shipping — "Take notes for me" rolled out to all Business & Enterprise Workspace plans from 15 Jan 2025; previously behind a Gemini Business/Enterprise add-on ($20 + $30/user/month respectively) that was discontinued 31 Jan 2025 | Q |

Google is a wholly-owned subsidiary of Alphabet Inc., a US public company. Google Meet is a cloud product delivered from Google's global infrastructure; it is not an independent company. The "take notes for me" feature is the relevant "meeting recap" substitute for mdreel.

---

## 2. Business model

**Model type:** Per-seat bundled SaaS. "Take notes for me" is a feature included in Google Workspace subscriptions — it is not independently priced, metered per hour, or sold separately. Effective €/video-hour is **N/A by construction** (bundle, not consumption pricing).

| Plan | Price (USD/user/mo) | Price (EUR ×0.90) | Includes "Take Notes for Me"? | EU Data Residency (storage)? | Effective €/video-h |
|---|---|---|---|---|---|
| Business Starter | $7 | €6.30 | ✅ (since Jan 2025) | ❌ (no data region control) | N/A — bundled |
| Business Standard | $14 | €12.60 | ✅ | ✅ EU or US storage | N/A — bundled |
| Business Plus | $22 | €19.80 | ✅ | ✅ EU or US storage | N/A — bundled |
| Enterprise Standard | Contact sales | Contact sales | ✅ | ✅ EU or US storage | N/A — bundled |
| Enterprise Plus | Contact sales | Contact sales | ✅ | ✅ EU or US storage + AI processing in EU (Assured Controls) | N/A — bundled |

**Pricing source:** TechRadar Google Workspace review (checked 2026-07-15). [T — techradar.com/reviews/google-workspace] Google's own pricing.html page was live but rendered only footnote disclaimers in fetched content; the actual pricing table is JavaScript-rendered and inaccessible via text fetch. Prices reflect the January 2025 price increase that accompanied Gemini bundling. [Q — workspaceupdates.googleblog.com/2025/01/]

**Entry price:** $7/user/month (Business Starter) for smallest commitment; free tier exists for Google Meet video calls (100 participants, 60 min), but **"take notes for me" requires a paid Workspace plan**. [Q — support.google.com/meet/answer/14754931, checked 2026-07-15]

**Free / trial shape:** Google Workspace offers a 14-day free trial. "Take notes for me" may be accessible during trial on eligible editions. No free-forever tier for meeting notes. [Q — workspace.google.com pricing page footnotes]

**Enterprise motion:** Direct enterprise sales team for Enterprise editions; also reseller channel. Enterprise Plus adds Assured Controls for EU AI processing. New "AI Expanded Access" add-on (launched Feb 2026) covers higher usage of advanced image/video generation — NOT relevant to notes. [Q — workspaceupdates.googleblog.com/2026/02/google-workspace-ai-expanded-access.html]

**What Google meters that mdreel doesn't:** Nothing granular for notes — usage limits exist but Google describes them as "generous limits designed to support the everyday needs of most teams." [Q — workspaceupdates.googleblog.com/2026/02/] Google meters participants (max 100/150/500/1000 by tier), storage, and higher AI features (image/video gen). Notes themselves are not per-call charged.

**Overage:** None published for notes. Higher AI features (image/video gen) require AI Expanded Access add-on ($unknown, available for Business Standard+ and Enterprise Standard+). [Q — workspaceupdates.googleblog.com/2026/02/google-workspace-ai-expanded-access.html]

---

## 3. Product & features — checklist

- [x] **transcript** — YES, but via a *separate* Google Meet Transcription feature (verbatim speech-to-text saved as Google Doc), distinct from "take notes for me." The notes feature itself produces AI summary, not verbatim transcript. Both require Business Standard or higher. [Q — support.google.com/meet/answer/14754931, checked 2026-07-15]

- [ ] **verbatim ON-SCREEN text (slides/code/UI)** — NO. Standard "take notes for me" captures spoken audio only; produces an AI summary of speech. There is an Alpha-only "Include screenshots of presented content" option described as capturing "resources that resemble presentations like slides and related documents" — but this takes image *screenshots* (not OCR/text extraction) and is explicitly marked "only available to Alpha users" with no GA date published. Verbatim on-screen text is not captured at any tier. [Q — support.google.com/meet/answer/14754931, checked 2026-07-15]

- [ ] **visual descriptions** — NO. Even the Alpha screenshots feature attaches images to the notes doc without structured textual description of on-screen content. [Q — support.google.com/meet/answer/14754931]

- [~] **timestamps** — PARTIAL. The notes document is timestamped (meeting time, calendar event). Individual sentence-level or section-level in-document timestamps are not published in the standard output description. No seekable per-sentence timestamps confirmed. [Q — support.google.com/meet/answer/14754931; granularity: E]

- [ ] **structured/Markdown output** — NO. Output is a Google Docs file — Google proprietary format. Sections are Summary, Decisions (English only), Next Steps, Details. Not Markdown. Not portable. No `.md` export of meeting notes. [Q — support.google.com/meet/answer/14754931]

- [ ] **JSON/API** — NO public API for "take notes for me" note content. Google Workspace APIs exist (Drive, Docs) so an admin could read the generated Doc via API, but there is no dedicated notes-content API. [E — no source found for a meeting-notes-specific API; Drive/Docs API documented separately]

- [ ] **webhooks** — NO published webhooks for notes events (note created, note ready). [E]

- [ ] **MCP server** — NO. Google has not published an MCP server for Workspace notes. [E]

- [~] **connectors** — PARTIAL. Notes auto-connect to: Google Calendar (attached to event), Google Drive (stored in organizer's Drive), Gmail (email notification to invitees). No out-of-the-box connector to Slack, Notion, Confluence, or generic RAG pipelines. [Q — support.google.com/meet/answer/14754931]

- [x] **speaker ID** — YES. Notes attribute spoken content to named participants. [Q — support.google.com/meet/answer/14754931]

- [x] **languages** — 8 supported: English, French, German, Italian, Japanese, Korean, Portuguese, Spanish. "This feature supports one language at a time. Multiple languages spoken in the same meeting aren't currently supported." [Q — support.google.com/meet/answer/14754931, checked 2026-07-15]

- [x] **max video length** — 8 hours recommended maximum for "take notes for me"; 15 minutes minimum recommended. Google Meet itself supports longer meetings but note quality may degrade. [Q — support.google.com/meet/answer/14754931]

- [ ] **processing-speed claims** — NO specific latency SLA published. Notes available "shortly after the meeting ends." [Q — support.google.com/meet/answer/14754931]

- [x] **retention/erasure controls** — YES. Notes documents follow the Meet retention policy configured by the admin in Google Vault. Admins can configure retention periods and deletion. [Q — support.google.com/meet/answer/14754931]

- [ ] **self-host option** — NO. Google Workspace is fully cloud-hosted by Google. [E]

**What the output actually looks like (3 sentences):** "Take notes for me" produces a Google Docs file with four AI-generated sections — a brief Summary paragraph, a Decisions section (tracking whether items were "Aligned / Needs Further Discussion / Disagreed / Shelved," English-only), a bullet-point Next Steps list with assignments, and a Details section with more granular discussion points. The document is stored in the meeting organizer's Google Drive folder, attached to the Google Calendar event for easy discovery, and emailed to invited participants based on the host's sharing settings (all invitees, internal-only, or hosts/co-hosts only). The output contains zero verbatim on-screen text from slides, code, or shared UI — all content is derived exclusively from spoken audio, and even the Alpha-stage screenshot attachment captures images rather than extracting machine-readable text from shared screens.

---

## 4. Size & customer base

| Metric | Value | Grade |
|---|---|---|
| Businesses on Workspace | "More than 10 million businesses of all sizes — from innovative startups to global enterprises" | Q — workspace.google.com/blog/product-announcements/empowering-businesses-with-AI, checked 2026-07-15 |
| Generative AI adopters | "More than 100,000 customers embrace generative AI" across Workspace | Q — same source |
| Named enterprise logos | Nielsen, Colgate, Airbus, Whirlpool (mentioned in Workspace blog case studies) | Q — workspaceupdates.googleblog.com |
| G2 rating | Unknown — G2 product page returned 403 during verification | — |
| Web traffic | workspace.google.com and meet.google.com are top-20 global SaaS properties; exact MAU not disclosed | E |
| EU customer count | Not publicly disclosed by Google | unknown |
| Hiring signals | Google has an established EU workforce (Dublin HQ for EMEA); no Workspace-specific EU hiring surge detected in this research | E |

---

## 5. GTM & distribution

**Channels:** (1) Self-serve online signup at workspace.google.com — credit card, any size; (2) Reseller/partner channel — Google Workspace Partners and Managed Service Providers; (3) Enterprise direct sales — for Enterprise Standard/Plus, large seats, custom contracts.

**Positioning sentence (verbatim):** *"We believe AI is foundational to the future of work and its transformative power should be accessible to every business and every employee, at an affordable price. That's why today we've decided to include the best of Google AI in Workspace Business and Enterprise plans, bringing the latest generative AI capabilities to our business customers without the need to purchase any add-ons."* [Q — workspace.google.com/blog/product-announcements/empowering-businesses-with-AI, checked 2026-07-15]

**Who the pricing page talks to:** IT admins and business owners at SMBs to large enterprises — primarily those already in the Google ecosystem (Gmail, Drive, Calendar). The page leads with "up to 300 users" for Business editions and "unlimited" for Enterprise, signalling the mid-market to enterprise boundary is fluid. There is no dedicated vertical messaging for EU/GDPR buyers.

---

## 6. EU/GDPR posture

| Element | Detail | Grade |
|---|---|---|
| Hosting regions | Google data centers globally, including EU (Dublin, Frankfurt, Warsaw, others). Exact Meet note processing region not published as a separate endpoint. | Q — cloud.google.com/about/locations referenced in DPA |
| DPA | Yes — Google Cloud Data Processing Addendum, incorporated into Workspace agreements; covers Customer Data definition, security measures, subprocessors, SCCs for international transfer. | Q — cloud.google.com/terms/data-processing-addendum/, checked 2026-07-15 |
| Subprocessors | Disclosed publicly at workspace.google.com/intl/en/terms/subprocessors/ (page loads but JS-rendered; referenced in DPA). | Q — cloud.google.com/terms/data-processing-addendum/ |
| No-training terms | "Your data is your data: We don't use your data, prompts, or generated responses to train Gemini models outside of your domain without permission. We don't sell your data or use it for ads targeting." | Q — workspaceupdates.googleblog.com/2025/01/, checked 2026-07-15 |
| EU data residency (storage) | Configurable data region (US or EU/Europe) available on Business Standard, Business Plus, Enterprise Standard, Enterprise Plus. **NOT available on Business Starter.** | Q — knowledge.workspace.google.com/admin/compliance/choose-a-geographic-location-for-your-data, checked 2026-07-15 |
| EU AI/Gemini PROCESSING in EU | Restricted: only Enterprise Plus with Assured Controls add-on (premium, contact sales) can restrict data processing location to EU. Standard Business plans (including Business Standard/Plus with EU storage) do NOT restrict where Gemini processes prompts and generates notes. | Q — knowledge.workspace.google.com; "Enterprise Plus and Frontline Plus only" for processing data region settings |
| Certifications | ISO 27001, ISO 27017, ISO 27018, ISO 42001 (AI-specific), SOC 1, SOC 2, SOC 3. | Q — workspaceupdates.googleblog.com/2025/01/; cloud.google.com/privacy/gdpr |
| HIPAA compliance | Available for Workspace (BAA required); relevant for healthcare verticals. | Q — workspaceupdates.googleblog.com/2025/01/ |
| Residency premium? | YES — significant. EU storage control starts at Business Standard ($14/user/month); EU **AI processing** control requires Enterprise Plus (custom pricing). A 200-seat EU IT company on Business Standard has EU file storage but Gemini note-taking may process audio/text outside EU. This is a material gap for EU DPOs. | Q (structure), E (DPO assessment) |
| Access Transparency | Google staff access logs via Access Transparency — Enterprise only. | Q — cloud.google.com/privacy/gdpr |
| SCCs | Standard Contractual Clauses included in DPA for EU/EEA transfers. | Q — cloud.google.com/terms/data-processing-addendum/ |

---

## 7. Threat assessment

**ICP overlap + why:** Very high overlap with mdreel's ICP (EU software/IT/L&D teams of 50–500 with a DPO). Any company already running Google Workspace — and that's the dominant choice for many EU tech SMBs — automatically received "take notes for me" bundled into their existing subscription at zero incremental cost from January 2025. The DPO at a 200-seat EU software firm paying €14/seat for Business Standard already has a meeting-notes-for-free argument to deploy internally. The "do nothing" option for meeting notes is literally included in the invoice they're already paying.

**What they'd need to kill mdreel + likelihood:** Three things simultaneously — (1) verbatim on-screen text OCR (exit Alpha screenshots and add actual text extraction), (2) portable Markdown/JSON export (breaking Drive lock-in, which Google has no business incentive to do), and (3) EU-guaranteed AI processing without requiring Enterprise Plus tax. Likelihood: **LOW for full capability parity within 18 months** — OCR of screenshots could advance, but Markdown export contradicts Google's lock-in strategy, and EU AI processing for sub-Enterprise customers is structurally constrained by Google's infrastructure and pricing logic. However, even a partial improvement (e.g., Alpha screenshots going GA) would reduce the "demo notes" use case gap.

**What mdreel structurally does that Meet can't:**
1. **Verbatim on-screen OCR** — captures slides, code, terminal output, UI text verbatim; Google's notes are audio-only summaries.
2. **Ingest arbitrary uploaded video** — pre-recorded demos, training videos, onboarding recordings, old call recordings; Meet notes only work for live Meet sessions.
3. **Portable Markdown output** — truly portable, importable into any RAG pipeline, vector store, or Obsidian vault; Google's output is locked in Google Docs/Drive.
4. **Spoken vs. shown separation** — mdreel explicitly separates what presenter said from what was on screen; Google conflates both into a single AI summary.
5. **EU-only AI processing at SMB price** — mdreel can commit to EU residency without an Enterprise Plus surcharge.

**What mdreel should steal from Google:**
- **Auto-enable for recurring meetings** — admins can set "take notes for me" as default for recurring Calendar events; mdreel should offer a similar "always-capture" trigger for calendar-linked recordings.
- **"Summary so far" real-time catch-up** — in-session running summary for latecomers; mdreel equivalent would be a URL-shareable partial transcript for async viewers.
- **Explicit consent notification UX** — Google's "Gemini may improve your notes with screenshots" banner is a best-practice consent pattern; mdreel should replicate for GDPR consent flows in recording contexts.
- **Calendar attachment** — attaching the output file directly to the originating calendar event is elegant; mdreel should consider a webhook/integration that mimics this for EU calendars.

---

## 8. Evidence log

| # | Claim | Source URL | Checked | Grade |
|---|---|---|---|---|
| 1 | "Take notes for me" feature description, sections (Summary/Decisions/Next Steps/Details), languages, max length 8h, screenshots Alpha only | https://support.google.com/meet/answer/14754931 | 2026-07-15 | Q |
| 2 | "Take notes for me" included in Business Starter/Standard/Plus + Enterprise Starter/Standard/Plus from January 15, 2025; Gemini add-ons discontinued; pricing adjustments effective January 16, 2025 for new customers | https://workspaceupdates.googleblog.com/2025/01/ | 2026-07-15 | Q |
| 3 | No-training terms: "We don't use your data, prompts, or generated responses to train Gemini models outside of your domain without permission" | https://workspaceupdates.googleblog.com/2025/01/ | 2026-07-15 | Q |
| 4 | Certifications: SOC 1/2/3, ISO 27001/17/18, ISO 42001, HIPAA | https://workspaceupdates.googleblog.com/2025/01/ | 2026-07-15 | Q |
| 5 | "More than 10 million businesses" use Workspace; "more than 100,000 customers embrace generative AI" | https://workspace.google.com/blog/product-announcements/empowering-businesses-with-AI | 2026-07-15 | Q |
| 6 | Pricing: Business Starter $7, Standard $14, Plus $22 per user/month post-January 2025 bundling | https://www.techradar.com/reviews/google-workspace | 2026-07-15 | T |
| 7 | EU data residency (storage) available on Business Standard/Plus, Enterprise Standard/Plus; NOT on Starter. EU data processing in EU requires Enterprise Plus + Assured Controls only | https://knowledge.workspace.google.com/admin/compliance/choose-a-geographic-location-for-your-data | 2026-07-15 | Q |
| 8 | Google Cloud/Workspace DPA exists; ISO 27017/27018 compliance for Workspace; subprocessors disclosed | https://cloud.google.com/terms/data-processing-addendum/ | 2026-07-15 | Q |
| 9 | GDPR security overview: encryption at rest/transit, Access Transparency (Enterprise), SCCs, Data Regions feature | https://cloud.google.com/privacy/gdpr | 2026-07-15 | Q |
| 10 | "Take notes for me in Meet... are available in our standard plans with generous limits designed to support the everyday needs of most teams" (confirmed not usage-metered for standard notes) | https://workspaceupdates.googleblog.com/2026/02/google-workspace-ai-expanded-access.html | 2026-07-15 | Q |
| 11 | AI Expanded Access add-on (Feb 2026) for higher limits on image/video gen; available for Business Standard+ and Enterprise Standard+; not related to notes | https://workspaceupdates.googleblog.com/2026/02/google-workspace-ai-expanded-access.html | 2026-07-15 | Q |
| 12 | Workspace plans capped at 300 users (Starter/Standard/Plus); no cap for Enterprise; footnote text from Google's own pricing page | https://workspace.google.com/pricing.html | 2026-07-15 | Q |
| 13 | Notes stored in organizer's Drive folder; follow Meet retention policy (Vault); attached to Calendar event; shared via email to invitees | https://support.google.com/meet/answer/14754931 | 2026-07-15 | Q |
| 14 | "Screenshots of presented content" is Alpha-only; even then captures images not OCR text; "only available to Alpha users" explicit qualifier | https://support.google.com/meet/answer/14754931 | 2026-07-15 | Q |
| 15 | Note sections customization and screenshots of presented content — "only available to Alpha users" | https://support.google.com/meet/answer/14754931 | 2026-07-15 | Q |
| 16 | Google Workspace "empowering businesses with AI" blog (primary announcement of bundling + no-training commitment) — "Updated April 28, 2025: Pricing for very small business customers will be updated starting July 7, 2025" | https://workspace.google.com/blog/product-announcements/empowering-businesses-with-AI | 2026-07-15 | Q |
| 17 | Speech translation in Meet GA January 27, 2026 for select Workspace plans | https://workspaceupdates.googleblog.com/2026/02/speech-translation-meet-ga.html | 2026-07-15 | Q |
| 18 | Google's "take notes for me" blog snippet confirmed: "notes document is stored in the meeting owner's drive folder and will follow the Meet retention policy" | https://workspaceupdates.googleblog.com/search?q=take+notes | 2026-07-15 | Q |
