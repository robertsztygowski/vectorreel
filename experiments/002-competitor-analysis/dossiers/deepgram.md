# Deepgram — ring: infra · verified: 2026-07-15 · confidence: high

## 1. Vitals
- **HQ/jurisdiction:** San Francisco, California, USA (US jurisdiction — the EU-residency story is a routing/endpoint feature, not a corporate domicile).
- **Founded:** 2015 (Y Combinator W16); founders Scott Stephenson and Noah Shutty. [E — widely reported; not re-verified this session; treat founding year as E]
- **Funding:** Total **~$229M over 8 rounds** (Tracxn, T). Most recent: **Series C $130M at a $1.3B valuation, announced Jan 2026**, led by **AVP**; participation from Alkeon, In-Q-Tel, Madrona, Tiger, Wing, Y Combinator, BlackRock funds, plus new strategics Twilio, ServiceNow Ventures, SAP, Citi Ventures, Alumni Ventures, Princeville Capital, and university endowments (Michigan, Columbia, Stanford). [Q vendor press release + T Tracxn]
- **Headcount:** **~326** (Tracxn, dated May 2026). [T]
- **Status: growing.** Fresh $130M Series C at a unicorn valuation (Jan 2026), a roster of strategic corporate investors (SAP, ServiceNow, Twilio, Citi), and In-Q-Tel (US intelligence-community VC) backing. This is a well-capitalized, scaling infra vendor, not a zombie.

## 2. Business model
**Model type: usage-based (per-minute API metering) + prepaid credit**, with a sales-led enterprise tier on top. This is developer infrastructure, not a per-seat SaaS.

| Plan | Price | Included | Effective €/video-h | Overage |
|---|---|---|---|---|
| Pay As You Go (Nova-3, pre-recorded, monolingual) | $0.0077/min (~$0.46/h) | Metered; **$200 free credit, no expiry** | **~€0.42/audio-h** (USD→EUR 0.90) — see caveat | same rate, linear |
| Pay As You Go (streaming/live) | $0.0048/min (~$0.29/h) | Metered | **~€0.26/audio-h** | same rate, linear |
| Growth / volume commit | unknown (historically an annual spend commitment for a volume discount) | unknown | unknown | unknown |
| Enterprise / on-prem | Custom (sales-led) | Custom | unknown | Custom |

- **⚠️ €/video-hour caveat:** Deepgram is **audio-only STT**. The €/h figures are per **audio** hour; it does not process the *video* frame (no on-screen text, no visuals). So the "comparable unit" is not truly comparable to mdreel's video-hour — Deepgram would be one *input* to a DIY video pipeline, not a substitute output.
- **Entry price point:** effectively free to start — **$200 credit with no expiry**, then linear per-minute billing. No credit card wall to test.
- **Free tier/trial shape:** $200 non-expiring credit (≈430 hours of pre-recorded Nova-3, or ≈700 hours streaming). Extremely generous vs. mdreel's 1h trial credit — but that's an infra-primitive economics difference, not a like-for-like.
- **Enterprise motion:** self-serve PAYG for developers; sales-led for volume commits, on-prem, HIPAA/BAA, and custom DPAs.
- **What they meter that mdreel doesn't:** nothing structural beyond audio minutes; add-on *features* (diarization, language detection, audio-intelligence like sentiment/topics/summarization) can carry incremental per-minute cost. No seats, no storage, no index retention.

## 3. Product & features — checklist
- [x] transcript (core product; Nova-3 STT)
- [ ] **verbatim ON-SCREEN text (slides/code/UI)** — no; audio-only, cannot read the screen
- [ ] visual descriptions — no
- [x] timestamps — **word-level** timestamps (per-word start/end), utterances, paragraphs
- [ ] structured/Markdown output — outputs **JSON** (and text/SRT/VTT), not knowledge-base Markdown
- [x] JSON/API — REST + WebSocket; JSON response schema
- [x] webhooks — async callback on pre-recorded jobs
- [?] **MCP server** — an **async MCP server exists but appears community/third-party** (GitHub `ctaylor86`, `reddheeraj`), not confirmed as an official first-party Deepgram product. Grade [?].
- [?] connectors — SDKs (Python/JS/Go/.NET) rather than app connectors (Notion/Drive/etc.); no productized SaaS connectors observed
- [x] speaker ID — diarization across all Nova batch models, no cap on speaker count, next-gen model trained on 100k+ voices
- [x] languages — "over a dozen" languages + automatic language detection (multilingual on Nova-3)
- [?] max video/audio length — long-file async supported (hour+ files); explicit hard cap unknown
- [x] processing-speed claims — markets very low latency / real-time; third-party reviews cite "~20× faster" and near-instant streaming
- [x] retention/erasure controls — configurable; no-training available via DPA (see §6)
- [x] self-host option — on-prem/self-hosted via sales/Enterprise
- **Output shape:** Deepgram returns a **JSON document** — an array of channels/alternatives containing the full transcript plus a per-word list with `word`, `start`, `end`, `confidence`, and (if enabled) `speaker` labels; optional blocks for paragraphs, utterances, summaries, topics, and detected entities. It is a machine transcript payload, **not** a reader-facing Markdown artifact, and it contains **zero on-screen/visual content**. A developer must post-process it into anything knowledge-base-ready.

## 4. Size & customer base — evidence, not vibes
- **Case studies/logos:** 16 case studies / customer success stories referenced (FeaturedCustomers, T); strategic investors (Twilio, SAP, ServiceNow, Citi) signal enterprise embed relationships. Specific named logos not enumerated this session — [partly unknown].
- **Reviews:** **G2 — named "High Performer," ranked #2 in Voice Recognition Software**; review count reported inconsistently (42 for the core product page; ~446 across all Deepgram seller products). **Exact aggregate star rating: unknown** (G2 page returned HTTP 403). **PeerSpot: 8.4/10** (T). FeaturedCustomers: 58 customer references. [T]
- **Web traffic:** **~932.8K monthly visits (Similarweb, dated Jan 2024 — STALE, ~2.5 years old)**; current figure unknown. [T, stale]
- **GitHub/community:** developer-facing with public SDKs and active GitHub org/discussions (GDPR threads #604, #1085); exact star counts unknown this session.
- **Hiring signals:** ~326 staff and a fresh $130M round imply active hiring; specific req counts unknown.

## 5. GTM & distribution (feeds A5)
- **SEO:** strong developer-content engine — a large `deepgram.com/learn` blog (diarization, timestamps, model releases) that ranks for STT/voice-AI technical queries; docs at `developers.deepgram.com` rank for API/how-to terms.
- **Free tool:** the **$200 no-expiry credit + instant API key** is the free-tool motion — try before contact.
- **Gallery/demo:** interactive API playground / live transcription demo.
- **Community:** GitHub org, SDKs, developer discussions, MCP-ecosystem presence (community MCP servers extend reach).
- **Ads/partners:** heavy **strategic-partner distribution** — SAP, ServiceNow, Twilio, Citi as investors/embed channels; In-Q-Tel opens US-gov/regulated doors.
- **Positioning sentence (verbatim homepage):** **"The Voice AI Economy is Powered by Deepgram."** Sub-positioning verbatim: **"For developers and product teams ready to move fast with flexible APIs."**
- **Who the pricing page talks to:** the **developer / product engineer** who thinks in $/minute and API calls — not a team lead, not a DPO. Compliance is a separate trust/security surface, not the pricing hero.

## 6. EU/GDPR posture (feeds A1)
- **Hosting regions:** EU Data Residency via dedicated endpoint **`api.eu.deepgram.com`**; default is US. [Q — from verified facts]
- **DPA available?** Yes — standard DPA including **SCCs**, executed by contacting security@deepgram.com. [T]
- **Subprocessor list:** published at `deepgram.com/privacy/subprocessors`; full/specific list surfaced via DPA on request — **short/gated**, not a long public roster. [T]
- **No-training terms:** **opt-OUT, not default** — customers must **execute a DPA to limit processing to core delivery** and exclude their data from model improvement/benchmarking. This is a real gap vs. a "we never train, full stop" default. [T]
- **Certifications:** **SOC 2 Type 1 & Type 2, GDPR-ready, HIPAA, CCPA**; on-prem via sales. [Q/T]
- **Do they charge/market a residency premium?** **No premium** — EU residency is offered at no extra charge, and they do **not** lead marketing with it. EU residency is a checkbox on a trust page, not a headline. **This is the key contrast with mdreel:** Deepgram treats EU as a feature-flag; mdreel treats EU-native processing + DPO-facing messaging as the whole brand.

## 7. Threat assessment
- **Overlap with mdreel's ICP: LOW-to-MEDIUM.** Deepgram sells an **audio STT primitive to developers**; mdreel sells a **finished video→structured-Markdown artifact to EU software/L&D teams**. They intersect only inside the "infra ring" — i.e., an EU dev team could DIY with Deepgram (audio) + a vision/OCR API (on-screen text) + a fuser + an EU-residency setup. Deepgram alone covers maybe a third of the job and none of the visual/on-screen layer.
- **What they'd have to do to kill mdreel:** add (a) native **video ingestion with verbatim on-screen text + slide/code/UI OCR**, (b) **spoken-vs-shown separation**, (c) opinionated **knowledge-base Markdown output**, and (d) **EU-default, no-train-by-default residency marketed to DPOs**. That is a product-category pivot away from "voice AI infrastructure for developers" — **low likelihood**; it fights their entire positioning and their audio-only model line.
- **What mdreel does that they structurally can't (easily):** read the **screen** (verbatim on-screen text, code, slides) and separate spoken from shown; ship a **portable, no-lock-in Markdown artifact** aimed at a non-developer buyer; and make **EU-native, no-training-by-default the default and the pitch** rather than a gated DPA opt-out.
- **What mdreel should steal:** (1) the **$200 no-expiry, no-card free credit** as a frictionless "try the real thing" motion (calibrated to mdreel's COGS — mdreel's 1h trial is much stingier); (2) the **developer-content SEO engine** (`/learn` + docs ranking for how-to queries); (3) **word-level timestamp fidelity** and a clean JSON schema as the machine-readable companion to the Markdown; (4) **diarization quality** as a table-stakes bar to match on the audio side.

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | Nova-3 pre-recorded $0.0077/min; streaming $0.0048/min; $200 free credit no expiry; audio-only STT | https://deepgram.com/pricing | 2026-07-15 | Q |
| 2 | EU residency via api.eu.deepgram.com; GDPR/SOC2/HIPAA/CCPA; on-prem via sales; no residency premium | https://deepgram.com/pricing | 2026-07-15 | Q |
| 3 | Series C $130M at $1.3B valuation, led by AVP, Jan 2026; strategic investors incl. SAP, ServiceNow, Twilio, Citi | https://deepgram.com/learn/press-release-deepgram-raises-series-c | 2026-07-15 | Q |
| 4 | Total funding ~$229M over 8 rounds | https://tracxn.com/d/companies/deepgram/__ThWYvD43I1HgSdguaRgcoZ1dLjXMEMfCv1tzktOKL2k/funding-and-investors | 2026-07-15 | T (Tracxn) |
| 5 | Headcount ~326 (May 2026) | https://tracxn.com/d/companies/deepgram/__ThWYvD43I1HgSdguaRgcoZ1dLjXMEMfCv1tzktOKL2k | 2026-07-15 | T (Tracxn) |
| 6 | G2: High Performer, #2 Voice Recognition; PeerSpot 8.4/10; 16 case studies | https://www.g2.com/products/deepgram/reviews · https://www.featuredcustomers.com/vendor/deepgram | 2026-07-15 | T (G2/PeerSpot/FeaturedCustomers) |
| 7 | Exact G2 aggregate star rating and definitive review count | https://www.g2.com/products/deepgram/reviews | 2026-07-15 | unknown (G2 returned 403) |
| 8 | Web traffic ~932.8K monthly visits, Jan 2024 (stale) | https://www.similarweb.com/website/deepgram.com/ | 2026-07-15 | T (Similarweb, stale) |
| 9 | Diarization across Nova models, no speaker cap; "over a dozen" languages + language detection | https://developers.deepgram.com/docs/diarization · https://deepgram.com/learn/nextgen-speaker-diarization-and-language-detection-models | 2026-07-15 | Q |
| 10 | Homepage positioning "The Voice AI Economy is Powered by Deepgram"; "For developers and product teams…flexible APIs"; products STT/TTS/Voice Agent/Audio Intelligence | https://deepgram.com/ | 2026-07-15 | Q |
| 11 | DPA with SCCs available; no-training is opt-out via DPA (security@deepgram.com); SOC2 Type 1&2 | https://developers.deepgram.com/trust-security/data-privacy-compliance · https://deepgram.com/data-security | 2026-07-15 | T |
| 12 | Subprocessor list published/gated | https://deepgram.com/privacy/subprocessors | 2026-07-15 | Q |
| 13 | MCP servers for Deepgram exist but appear community/third-party, not confirmed first-party | https://glama.ai/mcp/servers/@ctaylor86/deepgram-mcp-server · https://github.com/reddheeraj/Deepgram-MCP | 2026-07-15 | T |
| 14 | Founded 2015, YC W16, founders Stephenson & Shutty | (not re-verified this session) | 2026-07-15 | E |