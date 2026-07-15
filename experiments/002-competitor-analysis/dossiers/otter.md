# Otter (Otter.ai / AISense Inc.) — ring: adjacent · verified: 2026-07-15 · confidence: high

## 1. Vitals
- **HQ/jurisdiction:** Mountain View, California, USA (legal entity AISense, Inc.). US jurisdiction — data hosted in the US (AWS). [T: Crunchbase/Tracxn; hosting Q below]
- **Founded:** 2016 (seed round Sept 2016). Co-founders Sam Liang (CEO) and Yun Fu. [T: Crunchbase]
- **Funding:** ~$73M total disclosed. Seed $3M (Sept 2016, Draper Associates/Draper Dragon); Series A $10M (Nov 2017, Horizons Ventures); strategic $10M (Jan 2020, NTT DOCOMO Ventures); **Series B $50M (Feb 2021, led by Spectrum Equity**, with Horizons Ventures, Draper, GGV Capital; incl. $10M convertible note). No new priced round found since 2021. [T: otter.ai blog + Spectrum Equity + Crunchbase/Tracxn]
- **Headcount:** ~200 employees (2025), largest function Engineering (~34). [T: getLatka, Unify — LinkedIn-derived est.]
- **Status: growing, at scale.** Vendor claims **$100M ARR reached March 2025** and **35M+ registered users**, with <200 staff (~$500k rev/employee). Heavy 2025 push into enterprise + "AI Meeting Agents." A late-stage, self-funded-since-2021 scale-up, not a zombie. [T: otter.ai blog / BusinessWire, Dec 2025 — vendor-attributed, unaudited]

## 2. Business model
Per-**seat** SaaS subscription (meeting-notetaker), metered in **transcription/recording minutes per user**, not video-hours or batch jobs. This is a poor unit-match for mdreel; €/h below is a forced comparison (a live meeting minute ≠ a processed video-hour).

USD→EUR ≈ 0.90 applied throughout. €/h derived from included minutes ÷ 60, at the **annual** price.

| Plan | Price (USD/user/mo) | Included | Effective €/video-h | Overage |
|---|---|---|---|---|
| Basic | Free | 300 min/mo (5 h); 3 lifetime imports | €0 (5 h cap) | none — hard cap |
| Pro | $16.99 monthly / **$8.33 annual** | 1,200 rec-min/user (20 h); 10 imports/mo; 90-min meeting cap | ~**€0.375/h** (€7.50 ÷ 20 h, annual) | none stated; hard minute cap |
| Business | $30 monthly / **$19.99 annual** | **Unlimited** meetings/recordings; 4-h meeting cap; 3 concurrent | **N/A** (unlimited; €18/seat/mo) | N/A |
| Enterprise | Custom (sales) | Unlimited + SSO/SCIM, API/webhooks, HIPAA add-on, video replay | N/A | N/A |

- **Entry price point:** Free (Basic, 300 min/mo), then **$8.33/user/mo annual** (Pro).
- **Free/trial shape:** genuine free tier, not a time trial — 300 min/mo forever + 3 lifetime imports. Generous funnel.
- **Enterprise motion:** self-serve up to Business (credit-card, in-product upgrade); **sales-led** only at Enterprise (demo request, SSO/SCIM/API gated there).
- **Meters mdreel doesn't:** priced **per seat** (mdreel is per-account hours); meeting-**minutes** and **concurrent-meeting** limits; **file-import** counts (10/mo on Pro); meeting-length caps (90 min Pro / 4 h Business); API/webhooks gated to Enterprise.

## 3. Product & features — checklist
- [x] transcript — core product (spoken audio, real-time)
- [~] verbatim ON-SCREEN text — **NO true OCR.** Business+ "Automated slide capture" inserts slide **images** into the transcript, not structured verbatim slide/code/UI text. This is mdreel's core gap. [Q per task brief]
- [ ] visual descriptions — no
- [x] timestamps — yes; sentence/utterance-level granularity in the transcript
- [~] structured/Markdown output — produces summaries, action items, "meeting notes"; **exports to TXT/DOCX/PDF/SRT/clipboard**, not clean portable Markdown-for-RAG. Not designed as a KB artifact.
- [x] JSON/API — **Enterprise only** (API/webhooks gated)
- [x] webhooks — Enterprise only
- [?] MCP server — no evidence found (unknown; likely no)
- [x] connectors — Zoom, Google Meet, MS Teams (auto-join bot); Salesforce & HubSpot (Pro+); Slack; calendar
- [x] speaker ID — yes (all tiers incl. free)
- [x] languages — English, Spanish, French (multi-language on Basic+); limited set, not broad. [T/Q pricing page]
- [x] max video/meeting length — 90 min (Pro), 4 h (Business); import-based
- [?] processing-speed claims — real-time/live transcription is the model; no batch throughput SLO published
- [x] retention/erasure — user delete; trash auto-purged after 30 days; admin controls
- [ ] self-host option — no; SaaS-only, US cloud

**What the output actually looks like:** A conversation view — timestamped, speaker-labelled transcript with an auto-generated summary ("Otter AI Chat" / takeaways / action items) alongside it, optimized for reading and search inside Otter's app, not as a portable file. On-screen content, when captured (Business+), appears as embedded slide **snapshots** in the timeline, so anything shown on a slide/screen (code, UI, verbatim text) is a picture, not extractable text. Exports are TXT/DOCX/PDF/SRT — the on-screen text never becomes machine-readable structured output.

## 4. Size & customer base
- **Users/logos:** vendor claims **35M+ registered users**; enterprise expansion messaging but few named public logos in sources reviewed (unknown specific enterprise logos). [T: otter.ai blog]
- **Reviews:** **G2 ~4.3/5 across ~300+ reviews** (one aggregate cites 462 reviews / 4.4); **Capterra ~4.4/5, ~98 reviews.** Common complaint: accuracy drops on accents/noise; limited integrations. [T: G2, Capterra, 2026]
- **Web traffic:** Similarweb (Sept 2025) — global rank ~#6,250; category rank ~#79; ~72% direct, ~17% organic search; top country USA. Absolute monthly-visit figure not captured (Similarweb reports it in the millions/mo range but exact number unknown here). [T: Similarweb]
- **Revenue:** ~$100M ARR (2025, vendor). [T: otter.ai blog]
- **GitHub/community:** not dev-facing; no public repo/stars. Community = large consumer/prosumer user base, not developers.
- **Hiring:** actively hiring (careers page live; Great Place To Work certified) — consistent with growth.

## 5. GTM & distribution
- **Positioning (verbatim):** "**The world's smartest AI Notetaker**" (pricing/home headline). Secondary framing: "AI Meeting Agents" / corporate knowledge base.
- **Channels:**
  - **Product-led / freemium** is the dominant engine — free 300-min tier drives the funnel; ~72% direct traffic signals strong brand + word-of-mouth.
  - **SEO:** ranks for "AI meeting notes," "transcription," "meeting summary," comparison/alternative pages; strong blog content operation.
  - **Integrations as distribution:** Zoom/Meet/Teams marketplace presence; the auto-join bot spreads the brand into every meeting it attends (viral surface).
  - **Enterprise sales-led** motion layered on top in 2025 (SSO/SCIM/API, demo requests, PR around ARR/agents).
  - No public free micro-tool or curated gallery in the mdreel sense; no dev-community play.
- **Who the pricing page talks to:** the **individual professional and team lead / small-team buyer** ("individuals and small teams," "medium-sized teams"). Not developers, not DPOs — no residency/compliance language on the pricing page at all.

## 6. EU/GDPR posture (feeds mdreel's A1)
- **Hosting regions:** **US-hosted** — AWS (S3, SSE encryption). Privacy page does **not disclose any EU region or EU data-residency option**; third parties confirm US processing. **No EU residency offered or charged.** [Q otter.ai/privacy-security; T: Sally, openli]
- **DPA:** **Yes** — available, incorporates **EU SCCs (Module 2, Controller→Processor)**; 30-day subprocessor-objection window. [T: DPA/subprocessor pages]
- **Subprocessor list:** published at otter.ai/subprocessors (includes AWS + AI service providers); appears **moderate/short** — not the long enterprise list of a residency-first EU vendor.
- **No-training terms:** **Partial and a liability for them.** Customer data is **not** used to train the third-party AI providers' models, BUT **Otter trains its OWN proprietary models on "de-identified" customer data.** This is *not* a clean no-training commitment — a real objection for EU DPOs. [Q privacy-security]
- **Certifications:** SOC 2 Type II; HIPAA (Enterprise add-on); GDPR/CCPA/VPAT; policies built on the ISO 27001/2 framework (framework-aligned, ISO 27001 certification not clearly claimed).
- **Residency premium:** none — they neither market nor charge for EU residency; it simply isn't offered.

## 7. Threat assessment
- **ICP overlap: LOW–MEDIUM.** Otter targets individuals/teams capturing **live meetings** for personal recall and CRM notes. mdreel targets EU IT/L&D teams turning **existing demo/training/meeting video** into structured KB Markdown. They overlap on "meetings → text," but diverge sharply on: (a) EU residency, (b) verbatim on-screen text/OCR, (c) portable file output for BYO-RAG, (d) batch video ingestion vs live notetaking. An enterprise could see Otter as "good enough" for meeting recall — that's the substitution risk — but Otter does not serve the on-screen-text KB use case.
- **What they'd need to do to kill mdreel:** (1) add **EU data residency** (europe region + residency-tier marketing) — *plausible but not on their roadmap; US-centric, no signal*; (2) add **real OCR of on-screen slide/code/UI text as structured output** rather than slide images — *possible technically, low priority given their live-meeting focus*; (3) ship **portable Markdown export for RAG** + open API on lower tiers — *possible but conflicts with their in-app-lock-in, seat-based model*. Combined likelihood of all three (the actual mdreel wedge): **low near-term.** They are racing toward "enterprise meeting agents," not EU compliance + KB artifacts.
- **What mdreel does that Otter structurally can't (without repricing/rehosting):** EU-only processing with residency as a sold feature; verbatim on-screen text separated from spoken audio; per-hour batch pricing for a firehose of pre-recorded video (Otter's per-seat/minute model punishes bulk async ingestion); no-lock-in Markdown files; a clean no-training story (Otter trains on de-identified customer data).
- **What mdreel should steal:** the **freemium funnel** (a real free tier / 1-h trial is directionally right, but Otter's generous free 300-min is a proven acquisition engine); **integration-as-distribution** (auto-join/connector presence spreading the brand); the **"corporate knowledge base" enterprise narrative** (Otter validates the KB positioning mdreel is aiming at — mdreel can claim the EU + structured-output version of it); and their crisp one-line positioning discipline.

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | Pricing: Basic free 300 min/mo; Pro $16.99 mo/$8.33 annual, 1,200 min; Business $30/$19.99 annual unlimited; Enterprise custom | https://otter.ai/pricing | 2026-07-15 | Q |
| 2 | Headline "The world's smartest AI Notetaker" | https://otter.ai/pricing | 2026-07-15 | Q |
| 3 | Slide capture = images at Business+, not verbatim OCR; US-hosted, SOC2, no EU residency | (task brief, vendor-verified) | 2026-07-15 | Q |
| 4 | Series B $50M Feb 2021 led by Spectrum Equity; ~$73M total | https://otter.ai/blog/otter-raises-50-million | 2026-07-15 | Q |
| 5 | Seed $3M 2016, Series A $10M 2017 (Horizons), NTT DOCOMO $10M 2020; investors | https://www.crunchbase.com/organization/aisense-inc | 2026-07-15 | T (Crunchbase) |
| 6 | ~200 employees, Engineering largest function | https://www.unifygtm.com/insights-headcount/otter-ai | 2026-07-15 | T (Unify/getLatka, LinkedIn est) |
| 7 | $100M ARR (Mar 2025), 35M+ users, <200 staff, ~$500k rev/employee | https://otter.ai/blog/otter-ai-breaks-100m-arr-barrier-and-transforms-business-meetings-launching-industry-first-ai-meeting-agent-suite | 2026-07-15 | Q (vendor, unaudited) |
| 8 | G2 ~4.3/5 (~300+ reviews); Capterra ~4.4/5 (~98) | https://www.g2.com/products/otter-ai/reviews | 2026-07-15 | T (G2/Capterra) |
| 9 | Traffic: global rank ~#6,250, ~72% direct / ~17% organic, Sept 2025, top country US | https://www.similarweb.com/website/otter.ai/ | 2026-07-15 | T (Similarweb) |
| 10 | DPA with EU SCCs (Module 2), 30-day subprocessor objection | https://otter.ai/subprocessors | 2026-07-15 | Q |
| 11 | US/AWS hosting, SOC2 Type II, ISO 27001/2 framework, 30-day trash auto-delete, no EU residency option | https://otter.ai/privacy-security | 2026-07-15 | Q |
| 12 | Otter trains its OWN models on de-identified customer data; third-party providers not trained on customer data | https://otter.ai/privacy-security | 2026-07-15 | Q |
| 13 | Connectors: Zoom/Meet/Teams auto-join, Salesforce/HubSpot (Pro+); API/webhooks Enterprise-only | https://otter.ai/pricing | 2026-07-15 | Q |
| 14 | Effective Pro €/h ≈ €0.375 (1,200 min=20h; $8.33×0.90=€7.50÷20); USD→EUR 0.90 applied | (derivation from #1) | 2026-07-15 | E |