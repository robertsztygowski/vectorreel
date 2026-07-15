# Rev — ring: adjacent · verified: 2026-07-15 · confidence: med

Note on identity: "Rev" ships two related products under one company — **Rev.com** (self-serve human + AI transcription/captioning app, seat-priced) and **Rev AI / rev.ai** (developer speech-to-text API, usage-priced). Both are covered below because the API is the part that actually overlaps mdreel's "video → structured output" job.

## 1. Vitals
- **HQ/jurisdiction:** Austin, Texas, USA (also SF presence). US company, **US-only data processing** — see §6. [Q/T]
- **Founded:** 2010, by Jason Chicola (CEO) + Dan Kokotov, David Abrameto, Josh Breinlinger, Mark Chen, Paul Huck (ex-oDesk people). [T: Wikipedia, Crunchbase]
- **Funding:** Reported **~$32M total** across ~3 rounds; the only cleanly-sourced round is a **$4.5M Series A (March 2013)** led by early oDesk employees/angels (TechCrunch). Later round detail is not cleanly sourced; treat the $32M total as a third-party aggregate, not verified. **unknown** on investor names beyond the Series A. Note: Rev has long positioned itself as largely bootstrapped/profitable, so "funding raised" understates scale. [T: Crunchbase/Tracxn aggregate; TechCrunch for Series A]
- **Headcount:** ~**60–200** depending on source (LinkedIn/ZoomInfo band 51–200; one estimate ~155). Plus a large *external* marketplace of freelance human transcriptionists (not employees). [T: ZoomInfo/LinkedIn]
- **Revenue/valuation:** getLatka lists **$10M ARR / $33M valuation (2025)** — this looks implausibly low for a company of Rev's headcount and 15-year history and conflicts with its market presence; **treat as low-confidence third-party estimate, likely stale or mis-attributed.** [T: getLatka — flagged unreliable]
- **Status:** **Growing / repositioning.** Classic human-transcription marketplace that has pivoted hard into AI (launched the Reverb ASR model, seat-based AI plans, per-minute API). Actively iterating pricing and adding AI features (multi-file analysis, translation beta) — not a zombie. [E: derived from live 2026 pricing/feature set]

## 2. Business model
Two motions: **(a) per-seat subscription** (rev.com app) and **(b) usage/pay-as-you-go per audio-minute** (rev.ai API), plus **à-la-carte human transcription** at ~$1.99/min. Self-serve for both; enterprise = sales-led ("Unlimited" custom + Rev AI volume).

USD→EUR ≈ **0.90** applied throughout. **Important caveat on €/video-hour:** Rev prices by **audio-minutes of AI transcription per seat**, not by video-hours, and there is no on-screen-text/visual pass. So €/video-h below is a **rough audio-time equivalent** (min ÷ 60), not a like-for-like of mdreel's video-hour unit.

**rev.com app (per-seat/month, annual billing shown):**

| Plan | Price (annual) | Included | Effective €/video-h (audio-equiv) | Overage |
|---|---|---|---|---|
| Free | $0 → **€0** | 45 AI min/mo (English) | €0 (but 0.75 h/mo cap) | none; buy human à la carte |
| Essentials | $25.49/seat/mo → **€22.9** | 5,000 AI min/mo (~83 h) | **≈ €0.28/h** | human svc at 10% off |
| Pro | $47.99/seat/mo → **€43.2** | 10,000 verbatim AI min/mo (~167 h), 37+ langs | **≈ €0.26/h** | human svc at 15% off |
| Unlimited | Custom | Unlimited AI min, HIPAA/CJIS, DAM | N/A (unlimited) | custom |

**rev.ai developer API (pay-as-you-go, per audio-hour unless noted):**

| Product | Price | Included | €/video-h (audio-equiv) | Notes |
|---|---|---|---|---|
| Reverb ASR (Eng) | $0.20/hr → **€0.18/h** | free credits ≈ 5 h Reverb | **€0.18/h** | per-word timestamps free |
| Reverb Turbo | $0.10/hr → **€0.09/h** | " | **€0.09/h** | fastest tier |
| Foreign Language | $0.30/hr → **€0.27/h** | 56+ langs | **€0.27/h** | |
| Whisper (Eng) | $0.005/min = $0.30/hr → **€0.27/h** | | **€0.27/h** | |
| Human transcription | $1.99/min → **€107/h** | 99%+ accuracy | **€107/h** | marketplace humans |

**Entry price point:** $0 free tier (45 min/mo) or **€0.09–0.18/video-h** on the API. **Meters mdreel doesn't:** per-**seat** licensing, per-audio-**minute**, **multi-file-analysis file caps** (5/10/50/500 by plan), and analytics add-ons (sentiment, topic extraction, language ID, translation) billed separately.

## 3. Product & features — checklist
- [x] transcript (AI + human marketplace)
- [ ] **verbatim ON-SCREEN text (slides/code/UI)** — **no.** Rev is audio-only speech-to-text; it does not read slides/code/UI frames. This is mdreel's core wedge.
- [ ] visual descriptions — no (audio only)
- [x] timestamps — **per-word timestamps** (free on API); word-level granularity
- [~] structured/Markdown output — structured **JSON** yes; **Markdown not a first-class output** (JSON/Word/PDF/txt for transcripts; 12 caption formats incl SRT/VTT)
- [x] JSON/API — yes, RESTful (rev.ai); JSON transcripts
- [x] webhooks — yes (async API "requires webhooks for production")
- [ ] MCP server — **not found / unknown** (no evidence of an MCP server)
- [~] connectors — Zoom/mobile recording, SSO for enterprise; no evidence of Slack/Notion/RAG connectors
- [x] speaker ID — **diarization up to 8 speakers by default, ~95% accuracy, no extra cost** (strong)
- [x] languages — **37+ (app Pro) / 56–58+ (API) / 75+ (human, marketed); streaming in 9**
- [?] max video length — unknown (per-minute billing, large-file async)
- [x] processing-speed claims — Reverb Turbo marketed as fastest tier; async + streaming (9 langs)
- [~] retention/erasure controls — **custom retention policies** on enterprise; not exposed on lower tiers
- [ ] self-host option — **no** (SaaS/US cloud only)

**What the output looks like:** Rev AI returns a **word-level JSON** transcript — an array of monologues/elements with `type`, `value`, `ts` (start), `end`, and `speaker` fields — plus optional caption exports (SRT/VTT + 10 other formats) and Word/PDF/txt for the app. It is a **speech transcript object keyed on timecodes and speakers**, not a document that fuses spoken audio with on-screen text. There is no field anywhere for "text shown on screen" — everything Rev emits comes from the audio track.

## 4. Size & customer base
- **Logos/case studies:** Rev markets heavily to media/podcast/legal/education and enterprise (HIPAA/CJIS tiers imply healthcare + gov/law-enforcement customers). Named logos **unknown** (not extracted from a case-study page this pass).
- **Reviews:** Widely reviewed on G2/Capterra/Trustpilot and covered by PCMag ("Editor's Choice" 2018; top transcription 2019), but exact **counts/ratings unknown** (not pulled this pass — do not invent).
- **Web traffic:** unknown (no source named).
- **Dev community/GitHub:** SDKs for Python, Node.js, Java (no Go/.NET); public API docs at `rev.ai`. GitHub stars **unknown**.
- **Hiring signals:** Active AI product development (Reverb model, new AI seat plans, translation beta) implies ongoing eng hiring; specifics **unknown**.

## 5. GTM & distribution
- **Channels:** Strong **SEO** on "transcription service / captions / subtitles" head terms (15-year domain authority, ranks for transcription intent); **free tier** (45 min/mo) as a funnel; **developer API** with free credits (~5 h) as a bottoms-up dev motion; **à-la-carte human marketplace** as an upsell; enterprise sales-led for HIPAA/CJIS/Unlimited.
- **Positioning (verbatim, Rev AI):** transcription API described as *"A RESTful API to access Rev's workforce of fast, high quality transcriptionists and captioners."* Security page: *"Our services are designed to help you meet your GDPR obligations effortlessly."*
- **Who the pricing page talks to:** the **app** page speaks to *individual professionals / small teams* (per-seat, "no cancellation penalties," legal templates, mobile dictation); the **rev.ai** page speaks to *developers* (per-minute, free credits, models, webhooks). Neither speaks to a DPO — GDPR is a reassurance line, not the headline.

## 6. EU/GDPR posture
- **Hosting regions:** **US-only.** Third-party/help-center sourcing states Rev *"ensure[s] that all customer data is processed and stored within the United States."* Data stored on AWS S3 with SSE encryption; **no EU region offered.** This is the opposite of mdreel's pitch. [T: Rev Help Center]
- **DPA:** **Yes** — published at rev.com/legal/data-processing-addendum. [Q]
- **Subprocessor list:** referenced via the DPA; length/detail **unknown** (not enumerated this pass).
- **No-training terms:** **Strong and explicit** — *"We do not sell customer data or allow for customer data to be used to train external LLMs… we'll never train external LLMs on your data."* [Q, rev.com/security]
- **Certifications:** **SOC 2 Type II** (audited) + **SOC 3** report available publicly; **HIPAA & CJIS** on Unlimited tier. **No ISO 27001 mentioned; no EU data residency.** [Q]
- **Residency premium:** They **do not sell EU residency** — they market the opposite (US data sovereignty) as the feature. GDPR is framed as "we help you comply," not "your data stays in the EU." **This is the single biggest structural gap mdreel can exploit (A1).**

## 7. Threat assessment
- **ICP overlap: LOW–MED.** Rev's buyers are media/legal/education/podcasters and developers needing an STT feed — **audio transcription**, not video→knowledge-base structuring. It overlaps mdreel only where an EU team might "just run the video's audio through an API." It does **not** touch the on-screen-text/slides/code use case at all, and it is **US-hosted**, which disqualifies it for mdreel's EU-DPO ICP.
- **What they'd need to do to kill mdreel:** (1) add a **visual/on-screen-text extraction pass** fused with the transcript, (2) emit **Markdown for RAG** as a first-class output, (3) stand up **EU data residency + a DPA that says data stays in the EU.** Each is plausible in isolation; **all three together is unlikely** — the US-residency reversal cuts against a 15-year US-sovereignty posture and their whole marketplace is US-based. **Likelihood: low.**
- **What mdreel does that Rev structurally can't (near-term):** EU-only processing as the *product*, **spoken-vs-shown separation**, verbatim on-screen slide/code/UI capture, portable no-lock-in Markdown for BYO-RAG. Rev is fundamentally an **audio** engine.
- **What mdreel should steal:** (1) **word-level timestamps + 8-speaker diarization included free by default** — a clean, generous default mdreel should match or beat; (2) the **rev.ai free-credit onboarding** (~5 h) as a low-friction dev trial shape (contrast mdreel's 1 h trial credit); (3) the **explicit "we never train external LLMs on your data"** one-liner — mdreel should say this *and* add "…and it never leaves the EU"; (4) their **usage API at €0.09–0.27/h** sets the anchor a technical buyer will compare mdreel's €149-for-25h (≈€6/h) against — mdreel must sell on the *visual/structuring* value, because on raw audio-transcription price Rev is ~20–60× cheaper.

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | App plans: Free 45 min; Essentials $25.49/$29.99; Pro $47.99/$59.99 (10,000 verbatim AI min, 37+ langs); Unlimited custom (HIPAA/CJIS) | https://www.rev.com/pricing | 2026-07-15 | Q |
| 2 | Multi-file-analysis file caps 5/10/50/500 by tier; human-service discounts 10%/15% | https://www.rev.com/pricing | 2026-07-15 | Q |
| 3 | API is RESTful STT/captioning; per-word timestamps free; min $1.99/file; verbatim $0.50/min; JSON/Word/PDF/txt + 12 caption formats | https://www.rev.com/api | 2026-07-15 | Q |
| 4 | rev.ai per-minute: Reverb $0.20/hr, Turbo $0.10/hr, Foreign $0.30/hr, Whisper $0.005/min, Human $1.99/min; 5h free credits; 15-sec min | https://www.rev.ai/pricing | 2026-07-15 | Q |
| 5 | Diarization up to 8 speakers, ~95% accuracy, default no extra cost; Reverb 58+ langs; streaming 9 langs; async requires webhooks; SDKs Python/Node/Java | https://brasstranscripts.com/blog/rev-ai-pricing-per-minute-2025-better-alternative | 2026-07-15 | T (BrassTranscripts) |
| 6 | Founded 2010; founders incl. Jason Chicola (CEO); HQ Austin TX + SF; marketplace + STT model | https://en.wikipedia.org/wiki/Rev_(company) | 2026-07-15 | T (Wikipedia) |
| 7 | $4.5M Series A, March 2013, ex-oDesk founders | https://techcrunch.com/2013/03/26/founded-by-early-odesk-employees-freelancer-marketplace-rev-com-raises-4-5-million-series-a/ | 2026-07-15 | T (TechCrunch) |
| 8 | ~$32M total funding aggregate; headcount band 51–200 (~155 est) | https://www.crunchbase.com/organization/rev | 2026-07-15 | T (Crunchbase/ZoomInfo) |
| 9 | $10M ARR / $33M valuation 2025 — flagged low-confidence/likely stale | https://getlatka.com/companies/rev | 2026-07-15 | T (getLatka, unreliable) |
| 10 | SOC 2 Type II audited; SOC 3 available; "never train external LLMs on your data"; "do not sell customer data"; GDPR "obligations effortlessly"; DPA published | https://www.rev.com/security | 2026-07-15 | Q |
| 11 | Data "processed and stored within the United States"; AWS S3 SSE; no EU region | https://support.rev.com/hc/en-us/articles/29708931771661-Privacy-And-Security | 2026-07-15 | T (Rev Help Center) |
| 12 | USD→EUR 0.90 conversion applied to all € figures | (analyst method) | 2026-07-15 | E |