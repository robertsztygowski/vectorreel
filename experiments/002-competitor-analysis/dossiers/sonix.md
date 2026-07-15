# Sonix — ring: adjacent · verified: 2026-07-15 · confidence: high

## 1. Vitals
- **HQ/jurisdiction:** San Francisco, California, USA (US jurisdiction; data stored on AWS US) [T: Crunchbase/PitchBook; Q: Sonix security page].
- **Founded:** 2017 [T: search-aggregated Crunchbase/getlatka].
- **Founders:** Jamie Sutherland, Stephen Hopkins, David Dat Nguyen [T: Crunchbase-sourced search].
- **Funding:** Bootstrapped — **$0 disclosed external funding** [T: Crunchbase / getlatka]. No rounds/investors found. Treat as unverified-but-consistent across sources.
- **Headcount:** ~6 (getlatka, 2025) to 11–50 (Crunchbase band) [T]. Small team.
- **Revenue signal:** getlatka reports ~$660K revenue with a 6-person team (2025) [T: getlatka — self-reported/estimated, low reliability].
- **Status:** **Flat-to-slowly-growing lifestyle/bootstrap business.** Why: tiny team, no funding, modest review counts, but SOC 2 Type II + active content SEO machine and current 2026 pricing = maintained and operating, not zombie. Not a venture threat scaling aggressively.

## 2. Business model
**Model type:** Subscription + usage (metered transcription hours) with a pure pay-as-you-go option and seat add-ons. Prices below are vendor USD; € column uses **USD→EUR ≈ 0.90**.

| Plan | Price (USD) | Included | Effective €/video-h | Overage |
|---|---|---|---|---|
| Pay As You Go | $10/hr | per-use, 5 GB storage, 1 user | ~€9.00/h | n/a (is the unit price) |
| Core | $25/mo ($275/yr) | 5 hrs transcription+translation/mo, 5 hrs AI workspace, 25 GB, 1 user | ~€4.50/h | $10/hr → ~€9/h |
| Advanced (Most Popular) | $50/mo ($550/yr) | 20 hrs/mo, 25 hrs AI workspace, 50 GB, 1 user | ~€2.25/h | $10/hr → ~€9/h |
| Pro | $80/mo ($880/yr) | 40 hrs/mo, 100 hrs AI workspace, 100 GB, 1 user | ~€1.80/h | $10/hr → ~€9/h |
| Enterprise | Custom | 1 TB storage, unlimited users, custom hours, SOC2/HIPAA/GDPR | unknown | custom |

- **Entry price point:** $10/hr pay-as-you-go (no subscription) or $25/mo Core (~€22.50).
- **Free tier/trial shape:** **30 minutes free, no credit card** [Q]. No permanent free tier.
- **Enterprise motion:** Hybrid — fully self-serve up to Pro; **sales-led** only at Enterprise (custom hours, dedicated AM).
- **Meters mdreel doesn't:** **per-seat/user** (extra users +$25/mo each), **storage** (5 GB → 1 TB tiers), and a **separate "AI workspace hours"** meter distinct from transcription hours. mdreel meters only video-hours; Sonix layers seats + storage + a second AI-usage meter on top.

## 3. Product & features — checklist
- [x] transcript — "99% accuracy," word-level
- [ ] verbatim ON-SCREEN text (slides/code/UI) — **no OCR / screen-text extraction found**; audio-first. This is mdreel's core wedge, absent here.
- [ ] visual descriptions — none found
- [x] timestamps — **word-level** granularity [Q-ish, features page]
- [?] structured/Markdown output — exports PDF, DOCX, SRT/VTT, plain transcript; **no Markdown export confirmed**. JSON via API only.
- [x] JSON/API — RESTful API on subscription plans
- [x] webhooks — documented as available
- [ ] MCP server — none found
- [x] connectors — Zoom, Microsoft Teams, Google Meet, Webex, Dropbox, Google Drive, Zapier, Adobe Premiere
- [x] speaker ID — supported (diarization)
- [x] languages — **54+ transcription / 55+ translation** [Q]
- [?] max video length — not published (unknown)
- [x] processing-speed claims — "less than ~4 min per hour of audio" [T: features page]
- [x] retention/erasure controls — RBAC, deletion, 2FA; DPA/SCC on request
- [ ] self-host option — none (SaaS, AWS US only)

**Output shape (3 sentences):** Sonix produces an interactive, in-browser transcript of the **spoken audio** with word-level timestamps, speaker labels, and inline editing/comments, plus AI-analysis layers (sentiment, topics, summaries) in its "AI workspace." Export is human-document / caption oriented — DOCX, PDF, SRT/VTT, and API/JSON — rather than a portable knowledge-base artifact. Crucially there is **no representation of on-screen/shown content** (slides, code, UI text); the transcript is a record of what was *said*, not what was *displayed*.

## 4. Size & customer base
- **Case studies/logos:** Marketed to journalism, legal, HR, medical, podcasting verticals; no strong named enterprise-logo wall surfaced. Specific logos: unknown.
- **Reviews:** **G2 ~4.7/5 (~26 reviews)** [T: G2, 2026]; Capterra profile active with positive reviews but a scraped overall count/rating wasn't cleanly confirmed (unknown exact count); Trustpilot and GetApp profiles exist. Review volume is modest — a niche, not a mass base.
- **Web-traffic estimate:** unknown (no reliable source pulled). Strong long-tail **SEO content footprint** (dozens of `/resources/` comparison + "GDPR-compliant transcription for [vertical]" pages) suggests meaningful organic traffic.
- **GitHub/community:** Not dev-community-facing; no notable stars. API exists but no OSS presence.
- **Hiring signals:** unknown / minimal (team ~6–50).

## 5. GTM & distribution
- **SEO (primary channel):** Massive programmatic content engine — "[Tool] vs Sonix" comparison pages (Verbit, Fireflies, Descript, Otter) and vertical "Best GDPR/SOC2-compliant transcription for [HR/hospitality/NGO/technology]" landing pages. This is the dominant distribution motion and directly relevant to mdreel's A5 — Sonix demonstrates that vertical + compliance-keyword SEO ranks in this category.
- **Free tool / trial:** 30-min free trial as the top-of-funnel hook, no gallery/public demo corpus.
- **Community/ads/partners:** Zapier + Adobe Premiere + cloud-storage integrations act as partner/marketplace distribution; no obvious paid-ads or developer-community play observed.
- **Positioning (verbatim):** "Convert audio to text with 99% accuracy" (features) / markets as GDPR- and SOC 2-compliant transcription & translation software.
- **Who the pricing page talks to:** The **individual professional / small team buyer** (journalists, researchers, legal, HR) and the ops/team-lead tier — priced per-user with self-serve tiers. Not developer-first, not DPO-first (compliance is a reassurance layer, not the headline).

## 6. EU/GDPR posture
- **Hosting regions:** **AWS, USA only** [Q: security page — "All data is stored at AWS in the USA"]. **No EU data residency option.**
- **DPA available?** Yes — DPA **and SCCs** available inside the account / on request [Q].
- **Subprocessor list:** Not published on the security page (short/undisclosed) — a gap for DPO buyers.
- **No-training terms:** Yes, explicit — "None of your data processed through Sonix is used for training purposes" [Q].
- **Certifications:** **SOC 2 Type II** (annual, Drata-monitored), **HIPAA** (via Medical Sonix), self-declared **GDPR-compliant**. **No ISO 27001 confirmed on the security page** (a third-party summary claimed ISO 27001 — treat as unverified; vendor page does not list it).
- **Residency premium:** They **market GDPR compliance heavily** (dozens of SEO pages) but do **not** offer or charge for true EU data residency — compliance is contractual (SCCs for US transfer), not architectural. This is precisely the A1 seam: Sonix sells "GDPR-compliant" while storing all data in the US.

## 7. Threat assessment
- **ICP overlap: medium.** Sonix targets professionals/teams needing accurate transcripts (incl. some EU teams via its GDPR SEO), overlapping mdreel's "video → text" job. But mdreel's ICP (EU IT teams 50–500 building internal AI knowledge bases from demos/trainings, needing on-screen text) is a different job — Sonix serves interviews/media/meetings, not slide/code/UI capture for RAG.
- **What they'd have to do to kill mdreel:** (a) stand up **true EU-region hosting** (not just SCCs) — *plausible but not their current priority; a US bootstrap with all-AWS-US infra rebuilding for EU residency is a real lift, low-medium likelihood*; (b) add **verbatim on-screen/slide/code OCR + spoken-vs-shown separation** — *not on their roadmap surface; medium effort, low likelihood*; (c) ship **Markdown/MCP knowledge-base output** — *easy, they already have API/JSON; medium likelihood but insufficient alone without EU + OCR*. Killing mdreel requires all three; combined likelihood **low**.
- **What mdreel does that Sonix structurally can't (cheaply):** genuine **EU-only processing** (Vertex europe-*) as an architectural fact, not an SCC promise; **verbatim on-screen text + spoken-vs-shown separation** for slides/code/UI (Sonix is audio-transcription DNA); **portable Markdown, bring-your-own-RAG, no lock-in** vs Sonix's in-browser workspace + seat/storage lock-in.
- **What mdreel should steal:** their **programmatic vertical/compliance SEO** ("GDPR-compliant [X] for [vertical]" and "vs [competitor]" pages) — cheap, ranks, and feeds A5; their **broad connector list** (Teams/Meet/Zoom/Drive) as ingestion paths; the **30-min no-CC trial** framing as a low-friction on-ramp analogous to mdreel's 1h trial credit.

**mdreel pricing vs Sonix (context):** Sonix effective ~€1.80–€9/video-h across tiers vs mdreel's COGS €0.65/h and Pro €149/25h (~€5.96/h) / Business €690/150h (~€4.60/h). Sonix's high tiers undercut mdreel on raw €/h, but Sonix bills a *transcript* while mdreel bills *transcript + verbatim on-screen text + EU residency* — different unit, not directly substitutable.

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | Full pricing ladder (PAYG $10/hr; Core $25; Advanced $50; Pro $80; overage $10/hr; Enterprise custom) | https://sonix.ai/pricing | 2026-07-15 | Q |
| 2 | 30-min free trial, no credit card | https://sonix.ai/pricing | 2026-07-15 | Q |
| 3 | 54+ transcription / 55+ translation languages | https://sonix.ai/pricing | 2026-07-15 | Q |
| 4 | Word-level timestamps, speaker ID, ~<4 min/hr processing | https://sonix.ai/features | 2026-07-15 | T |
| 5 | Connectors: Zoom, Teams, Meet, Webex, Dropbox, Drive, Zapier, Adobe Premiere; RESTful API + webhooks | https://sonix.ai/features | 2026-07-15 | T |
| 6 | All data stored on AWS in the USA; no EU residency | https://sonix.ai/security | 2026-07-15 | Q |
| 7 | SOC 2 Type II, HIPAA (Medical Sonix), GDPR-compliant; DPA + SCC on request; no-training terms | https://sonix.ai/security | 2026-07-15 | Q |
| 8 | Founded 2017, San Francisco; founders Sutherland/Hopkins/Nguyen; bootstrapped $0 funding | https://www.crunchbase.com/organization/sonix-sonix-ai | 2026-07-15 | T |
| 9 | Headcount ~6 (2025); ~$660K revenue | https://getlatka.com/companies/sonix.ai | 2026-07-15 | T |
| 10 | Headcount band 11–50 | https://www.crunchbase.com/organization/sonix-sonix-ai | 2026-07-15 | T |
| 11 | G2 rating ~4.7/5, ~26 reviews | https://www.g2.com/products/sonix-sonix-ai/reviews | 2026-07-15 | T (G2) |
| 12 | GDPR-compliant transcription vertical SEO pages (HR, hospitality, NGO, technology) | https://sonix.ai/resources/best-gdpr-compliant-transcription-software-hr-recruiting/ | 2026-07-15 | Q |
| 13 | USD→EUR 0.90 applied to all € figures | (stated methodology) | 2026-07-15 | E |
| 14 | Effective €/h derived as plan price ÷ included transcription hrs × 0.90 (e.g. Advanced $50/20h=$2.50→€2.25) | (derivation) | 2026-07-15 | E |
| 15 | No OCR/on-screen-text, no Markdown-native export, no MCP, no self-host (absence noted on features/pricing/security pages) | https://sonix.ai/features | 2026-07-15 | E |