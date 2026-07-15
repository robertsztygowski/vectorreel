# AssemblyAI — ring: infra · verified: 2026-07-15 · confidence: high

## 1. Vitals
- **HQ/jurisdiction:** San Francisco, California, USA (US company; no EU legal entity found — EU is a data-residency region, not a jurisdiction). [T: Crunchbase, Latka]
- **Founded:** 2017; founder/CEO Dylan Fox. [T: Crunchbase, Contrary Research]
- **Funding:** $115M total across seed→Series C per the vendor's own Series C post; third-party trackers cite $158.1M across 5 rounds (discrepancy likely from convertible notes). Series C = **$50M, Dec 2023, led by Accel**, with Insight Partners, Keith Block/Smith Point Capital, Daniel Gross & Nat Friedman, Y Combinator. Valuation ~$300M (2023). [Q: vendor blog "$50M Series C" / T: Crunchbase, PitchBook, Latka]
- **Headcount:** ~101–110 (LinkedIn/Tracxn/Datanyze, May 2026). [T: Tracxn, Datanyze]
- **Status: growing.** Revenue reportedly $4.9M (2023) → $10.4M+ (2024), 200k+ developer signups, 4,000+ paying brands (incl. NASA, Spotify, WSJ, NBCUniversal), named to Fast Company Most Innovative 2025, active model releases (Universal-3.5) and product expansion (Voice Agents). [T: Latka, Contrary Research, vendor]

## 2. Business model
**Usage-based (pay-as-you-go, per audio-hour + token metering).** Pure consumption API — no seats, no monthly plan floor, no included-hour buckets. This makes €/video-hour a *direct meter rate*, not a plan average.

| Plan | Price | Included | Effective €/video-h | Overage |
|---|---|---|---|---|
| Universal-2 (async STT) | $0.15/h | usage-metered | **~€0.14/h** | linear, no cap |
| Universal-3.5 Pro (async STT) | $0.21/h | usage-metered | **~€0.19/h** | linear, no cap |
| Streaming (Universal-Streaming) | from $0.15/h | usage-metered | ~€0.14/h | linear |
| U-3.5 Pro Realtime | $0.45/h | usage-metered | ~€0.41/h | linear |
| Voice Agent API | $4.50/h ($0.075/min) | STT+LLM+TTS bundled | N/A (not transcription) | linear |
| Enterprise | custom | volume discount, higher concurrency | unknown | contract |

USD→EUR 0.90 applied throughout and stated as such. **Caveat: these are audio-only STT rates. AssemblyAI produces no on-screen/visual text, so its €0.14–0.19/h is NOT comparable to mdreel's €0.65/h COGS for spoken+shown extraction — it prices a strictly narrower job.**
- **Entry point:** $0 — **$50 free credits, no credit card**. [Q: vendor pricing]
- **Free/trial shape:** credit-based (~238h of Universal-2 on $50), self-serve, instant.
- **Enterprise motion:** self-serve first (card-in, PAYG), sales-led only for volume/concurrency/BAA. Rate limits: 5 parallel jobs free → 200 paid. [Q: docs]
- **Meters mdreel doesn't:** stacking per-hour **add-ons** (diarization +$0.02–0.12, translation +$0.06, PII redaction +$0.05–0.08, topic detection +$0.15, etc.) and **LLM Gateway tokens** (GPT/Claude/Gemini pass-through per-MTok). A "full-featured" transcript can cost 2–4× the base rate.

## 3. Product & features — checklist
- [x] transcript
- [ ] **verbatim ON-SCREEN text (slides/code/UI)** — none; audio-only STT, no vision. **This is the structural gap.**
- [ ] visual descriptions — none
- [x] timestamps — **word-level, millisecond granularity** (`start`/`end` per word) [Q: docs]
- [~] structured/Markdown output — structured JSON yes; **Markdown not a native output** (JSON → user renders)
- [x] JSON/API — REST + official Python/other SDKs
- [x] webhooks — job-completion callbacks [Q: docs]
- [x] **MCP server** — official AssemblyAI MCP server (transcribe, search transcripts, LLM Gateway from Claude Code etc.) [T: vendor changelog, Zapier]
- [x] connectors — n8n, Zapier, Microsoft Power Platform, Vercel AI SDK [T: n8n, Zapier, MS Learn]
- [x] speaker ID — diarization (`speaker_labels`, A/B/…), paid add-on
- [x] languages — **99 languages** (Universal-2); Universal-3.5 Pro higher accuracy on ~6, code-switching across 18 [T: vendor blog/docs]
- [x] max video length — **up to 10 hours / 5 GB per request**; min 160 ms [Q: docs]
- [x] processing-speed claims — word timestamps add "a few seconds per hour"; batch-scale transcription marketed [Q: vendor]
- [x] retention/erasure controls — `DELETE` transcript endpoint; no-training terms + compliance certs (see §6)
- [ ] self-host — no; cloud API only (US or EU region)

**Output shape:** A JSON transcript object — `id`, `status`, `text`, `language_code`, `audio_duration`, overall `confidence`, an `utterances[]` array of speaker-segmented chunks, and a `words[]` array where each word carries `text`, `start`/`end` (ms), `confidence`, and `speaker`. Everything is spoken-audio-derived; there is no field for on-screen/slide/code text or spoken-vs-shown separation. Markdown, timestamped headings, and knowledge-base structuring are left entirely to the developer to build on top.

## 4. Size & customer base
- **Logos:** NASA, Spotify, Wall Street Journal, NBCUniversal (named in press/profiles; not confirmed on homepage this fetch). [T: Contrary Research, Latka]
- **Reviews:** **G2 4.6★ from 114 reviews.** Capterra listed, no sourced star count this check. [T: G2, Capterra]
- **Web traffic:** unknown (not sourced; did not verify SimilarWeb).
- **Community/scale:** 200k+ developer signups, 4,000+ paying brands, ~25M daily API calls / 10TB/day (late-2023 vendor figures — stale). [T: Contrary Research]
- **GitHub:** official Python SDK public; exact star count unknown this check.
- **Hiring signals:** ~101–110 headcount, active (voice-AI expansion, model cadence). [T: Tracxn]

## 5. GTM & distribution
- **Positioning (verbatim, homepage):** *"Voice AI infrastructure for developers building products that transcribe, understand, and act on speech."*
- **Who the pricing/site talks to:** the **developer/engineering team** — "developer-friendly APIs," per-hour rates, rate limits, SDKs. Not a DPO, not an L&D lead, not a non-technical buyer. Product-led growth via free credits.
- **Channels:**
  - **SEO/content:** very strong — deep docs, "best speech-to-text API" comparison content, engineering blog (model benchmarks, "30% fewer hallucinations than Whisper"), changelog. Ranks on STT-API and voice-agent queries.
  - **Free tool:** $50 free credits = the funnel (no gated demo needed).
  - **Ecosystem/partners:** MCP server, n8n, Zapier, Microsoft Power Platform, Vercel AI SDK — distribution via integration marketplaces.
  - **Ads/gallery/community:** developer community + docs-led; no consumer gallery.
- **Motion:** bottom-up, developer self-serve → usage expansion → enterprise contract.

## 6. EU/GDPR posture
- **Hosting regions:** US and **EU** available; **"EU region is the same price as US"** — residency at **no premium**. [Q: vendor pricing]
- **Certifications:** **SOC 2 Type 2, ISO 27001, PCI-DSS, GDPR-compliant, HIPAA BAA (no premium).** [Q: vendor pricing]
- **DPA:** available (standard for GDPR/SOC2 posture); full subprocessor list length unknown this check.
- **No-training terms:** compliance page asserts data-handling controls; explicit "we do not train on your data" wording not quoted this check — mark unknown/verify.
- **Residency premium: NO** — they neither charge nor heavily market EU residency; it's a checkbox parity feature, not a wedge. This is evidence *against* mdreel's A1 "EU is a purchase driver" for the infra buyer: a US vendor already offers EU residency at list price with SOC2/ISO. mdreel's EU angle must lean on *processing entirely in EU + Vertex-in-EU + DPO-facing narrative*, not residency alone.

## 7. Threat assessment
- **ICP overlap: LOW–MED.** AssemblyAI sells to *developers who build products*, not to EU L&D/IT teams wanting a finished Markdown knowledge-base artifact. An mdreel prospect with an engineering team is exactly who could DIY on AssemblyAI — so it's the **"build vs buy" denominator** (infra ring), not a head-to-head competitor. Overlap rises for the AI-consultancy segment of mdreel's ICP.
- **What they'd need to do to kill mdreel:** add (a) **visual/on-screen text + OCR/vision** understanding, (b) native **timestamped Markdown / knowledge-base output**, (c) an EU-first, DPO-facing packaged product rather than a raw API. **Likelihood: low–medium.** They have Gemini/LLM Gateway plumbing and could bolt on video vision, but their entire identity, pricing, and GTM is *voice/audio infrastructure for developers* — a packaged EU document product is off-strategy. More likely they stay audio-only and someone builds mdreel-like output *on top of* them.
- **What mdreel does that they structurally can't (short-term):** spoken **vs.** shown separation, verbatim slide/code/UI capture, finished portable Markdown for RAG, EU-processing narrative aimed at the compliance buyer rather than the developer. AssemblyAI is audio-only by design and by data pipeline.
- **What mdreel should steal:** (1) the **$50-credit, no-card, instant self-serve** funnel (mdreel's 1h trial credit is the analog — make it frictionless); (2) **word-level ms timestamps** as a quality bar; (3) **official MCP server + n8n/Zapier connectors** as distribution (feeds A5 — meet devs in integration marketplaces); (4) **docs-as-SEO** engine (benchmark posts, comparison pages); (5) transparent per-hour pricing page that a developer can self-qualify against.

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | Universal-3.5 Pro $0.21/h; Universal-2 $0.15/h; streaming from $0.15/h; add-ons stack per-hour | https://www.assemblyai.com/pricing | 2026-07-15 | Q |
| 2 | $50 free credits, no credit card | https://www.assemblyai.com/pricing | 2026-07-15 | Q |
| 3 | EU region same price as US; SOC2 Type2, ISO 27001, PCI-DSS, GDPR, HIPAA BAA no premium | https://www.assemblyai.com/pricing | 2026-07-15 | Q |
| 4 | Voice Agent API $4.50/h; LLM Gateway token pricing; in-region pricing +10% option | https://www.assemblyai.com/pricing | 2026-07-15 | Q |
| 5 | $50M Series C, Dec 2023, led by Accel; $115M total per vendor | https://www.assemblyai.com/blog/announcing-our-50m-series-c-to-build-superhuman-speech-ai-models | 2026-07-15 | Q |
| 6 | Total funding $158.1M / 5 rounds; ~$300M valuation | https://pitchbook.com/profiles/company/184620-25 | 2026-07-15 | T (PitchBook/Crunchbase) |
| 7 | Founded 2017, SF HQ, CEO Dylan Fox | https://www.crunchbase.com/organization/assemblyai | 2026-07-15 | T (Crunchbase) |
| 8 | Headcount ~101–110 | https://tracxn.com/d/companies/assemblyai/__Ka5MAyG7QWVq1ddyIfrErBH8kfGj0qmYFsILhdFtiIs | 2026-07-15 | T (Tracxn/Datanyze) |
| 9 | Revenue $4.9M(2023)→$10.4M(2024); 200k devs; 4,000 brands; NASA/Spotify/WSJ/NBCU | https://research.contrary.com/company/assemblyai | 2026-07-15 | T (Contrary/Latka) |
| 10 | G2 4.6★, 114 reviews | https://www.g2.com/sellers/assemblyai | 2026-07-15 | T (G2) |
| 11 | Word-level ms timestamps; utterances; per-word confidence & speaker in JSON | https://www.assemblyai.com/docs/speech-to-text/pre-recorded-audio | 2026-07-15 | Q |
| 12 | Max 5GB / 10 hours per request; 160ms min; 5 free / 200 paid parallel jobs | https://www.assemblyai.com/docs/speech-to-text/pre-recorded-audio | 2026-07-15 | Q |
| 13 | 99 languages (Universal-2); 18-language code-switching (U-3.5 Pro) | https://www.assemblyai.com/changelog | 2026-07-15 | T (vendor blog/changelog) |
| 14 | Official MCP server; n8n/Zapier/Microsoft/Vercel connectors; webhooks | https://zapier.com/mcp/assemblyai | 2026-07-15 | T (Zapier/n8n/MS Learn) |
| 15 | Homepage tagline "Voice AI infrastructure for developers…"; developer-targeted positioning | https://www.assemblyai.com/ | 2026-07-15 | Q |
| 16 | Effective €/h ~€0.14 (U-2) / ~€0.19 (U-3.5 Pro) at USD→EUR 0.90 | https://www.assemblyai.com/pricing | 2026-07-15 | E (0.90 FX applied to Q rates) |