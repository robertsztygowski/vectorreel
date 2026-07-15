# Amberscript — ring: adjacent · verified: 2026-07-15 · confidence: med

## 1. Vitals
- **HQ/jurisdiction:** Amsterdam, Noord-Holland, Netherlands (EU). [T: Crunchbase]
- **Founded:** 2017. [T: Crunchbase]
- **Funding:** ~$11.1M total. Seed round + a $10M Series A on 2021-11-04 led by **Endeit Capital**; other investors named across sources: Reach Incubator, StartCapital Partners, INSEAD Endowment. No round reported since 2021 (funding looks stale — 4.5 yrs since last raise). [T: Crunchbase; PitchBook]
- **Headcount:** LinkedIn/Crunchbase band **51–200**; PitchBook lists ~100. [T: Crunchbase, PitchBook]
- **Status:** **Flat-to-growing, mature niche incumbent.** Established brand with large logo book and ~300–370K monthly web visits, but no fresh capital since 2021 and a commoditizing STT market — reads as a stable, self-sustaining transcription business rather than a fast-scaling one. [E: derived from funding gap + traffic + headcount band]

## 2. Business model
Subscription (monthly hour buckets, machine STT) **plus** usage credits (one-time hour packs) and a human >99%-accuracy per-file tier. Self-serve at the low end; sales-assisted "Business/Enterprise" motion above.

| Plan | Price | Included | Effective €/video-h | Overage |
|---|---|---|---|---|
| Starter | €19/mo | 5h machine | €3.80 | buy more via credits |
| Pro | €29/mo | 10h machine | €2.90 | credits |
| Power | €49/mo | 25h machine | €1.96 | credits |
| One-time credits | pay-as-you-go | N hrs | ≈ per-hour pack | N/A |
| Human transcription | quote/per-file | >99% accuracy | N/A (per-file) | N/A |
| Business/Enterprise | contact sales | bulk/API/SSO | unknown | unknown |

- **Entry price point:** €19/mo (or one-time credits — no true recurring commitment required for occasional use). [Q: pricing page, pre-verified]
- **Free tier/trial:** free-trial minutes on signup (small); no permanent free tier. [T: review sites; specifics unknown]
- **Enterprise motion:** self-serve for the €19–49 machine plans; **sales-led** for API/volume/human/Business. [E]
- **Meters mdreel doesn't:** included **hours are per-account monthly buckets that don't roll cleanly** (reviewers cite "subscription term clarity" complaints); human-accuracy is priced per file/turnaround-time; API is a separate commercial track. No seat/storage/query metering observed.

## 3. Product & features — checklist
- [x] transcript (core product)
- [ ] verbatim ON-SCREEN text (slides/code/UI) — **not offered; audio-only STT**
- [ ] visual descriptions — no
- [x] timestamps — word/segment-level; SRT/VTT caption timing (`maxCharsPerRow` 30–45, 1–2 rows) [Q: API docs]
- [~] structured/Markdown output — **exports TEXT/JSON/SRT/VTT/STL(dep.); no Markdown** [Q: API docs]
- [x] JSON/API — REST API, `/jobs/upload-media`, `/jobs/status`, `/jobs/export-*`, glossary/translated-subtitles [Q: API docs]
- [x] webhooks — `callbackUrl` POST on completion, hourly retry, max 10 attempts [Q: API docs]
- [ ] MCP server — none found
- [?] connectors — no documented SaaS connectors; API/RapidAPI only [T: RapidAPI listing]
- [x] speaker ID — diarization via `numberOfSpeakers` (0–5); x-vector / 2-channel [Q: API docs; T: helpdesk]
- [x] languages — 80+ machine languages; ~11 core API langs (EN, DE, NL, FR, ES, IT, PT, DA, SV, FI, NO); 18+ human [Q: API docs; T]
- [x] max video length — file size cap **6GB direct / 16GB URL** (duration effectively long) [Q: API docs]
- [x] processing-speed claims — machine "direct" job ~**1 hour turnaround**; human = longer per `turnaroundTime` [Q: API docs]
- [x] retention/erasure controls — GDPR data-subject controls; deletion on request (marketed) [Q: vendor security page, pre-verified]
- [ ] self-host — no; SaaS only (GCP Frankfurt storage)

**Output shape:** JSON job export with segments, speaker labels, and timing plus caption formats (SRT/VTT). It is a **timestamped speech transcript / subtitle file** — spoken words only, with no separation of "spoken vs. shown," no on-screen/slide/code capture, and no Markdown/knowledge-base structure. To feed a RAG index a customer would post-process the JSON themselves.

## 4. Size & customer base
- **Logos (from vendor customers/home page):** BBC, Disney+, ZDF, TF1, National Geographic, Warner Brothers, Microsoft, Philips, Puma, Sodexo, Amsterdam UMC, University of Barcelona, Utrecht University, University of Amsterdam, Humboldt-Universität, Gemeente Amsterdam, République Française. Claims "all Dutch universities" and ~1/3 of Dutch municipalities. [Q: vendor home/customers page; T: FeaturedCustomers]
- **Reviews:** G2 **4.4/5, 72 reviews** (58% 5★); Capterra **4.3/5, 43 reviews**; also Trustpilot/TrustRadius/SoftwareAdvice presences. [T: G2, Capterra — 2026-07-15]
- **Web traffic:** Similarweb ~**371K visits (Sep 2024)**, ~**308K (Dec 2024)**. [T: Similarweb via reviewbolt] (stale to late-2024)
- **GitHub/community:** `amberscript/api-docs` + `amberscript/api` public repos, low-star (dev-facing but not a community play). [T: GitHub]
- **Hiring signals:** unknown (no current req count sourced).

## 5. GTM & distribution
- **Positioning (verbatim):** *"Europe's trusted partner for secure, high-quality transcription and subtitles."* [Q: amberscript.com/en]
- **SEO:** heavy content/helpdesk/blog engine ("best speech-to-text tools," academic transcription, subtitle guides) — ranks on transcription/subtitle keywords; large logo wall for social proof. [T: SERP + vendor blog]
- **Channels:** self-serve web signup, RapidAPI marketplace listing, academy/case-study content, vertical landing pages (media, education, legal, government, healthcare). No free-tool/gallery growth loop and no MCP; no obvious paid-ads dominance. [E]
- **Who the pricing/site talks to:** **buyers in media, academia, government, legal, healthcare** — the researcher, subtitler, compliance-minded org admin. **Not** the developer building an internal AI KB and **not** a DPO as primary persona (security is a trust badge, not the pitch). [E]

## 6. EU/GDPR posture
- **Hosting:** GCP **Frankfurt (EU)** storage; "data stored in Europe." [Q: vendor security page, pre-verified]
- **DPA:** **on request** (DPA/NDA), not self-serve/public. [Q: pre-verified]
- **Subprocessor list:** effectively **short/soft** — privacy policy concedes some **partners outside the EEA** (human-transcription partners receive files) and the **Summary feature ships transcripts to a third-party LLM outside the EU**. The "no store / no train" assurance is a marketing-FAQ line, **not a contractual T&C/DPA clause**. → **EU-storage ≠ fully-EU-processing.** [Q: privacy/security pages, pre-verified — key differentiator for mdreel]
- **No-training terms:** asserted in FAQ, not contractually bound. [Q, caveat above]
- **Certifications:** **ISO 27001 + ISO 9001**, GDPR, TPN content-security badge. No SOC 2 mentioned. [Q]
- **Residency premium:** **markets** EU trust as core positioning but does **not appear to charge** a separate residency SKU — security is table-stakes framing, baked into all plans. [E]

## 7. Threat assessment
- **ICP overlap: LOW–MED.** Amberscript's buyers are media/academic/government subtitlers and researchers; mdreel targets **EU software/IT teams (50–500) building internal AI assistants**. Overlap exists only where an L&D/IT team wants training-video text — but Amberscript delivers **audio-only transcripts, no on-screen text, no Markdown/KB structure, no MCP**, so it's a weak substitute for the RAG-ingest job. [E]
- **What they'd need to do to kill mdreel:** add (a) **verbatim on-screen/slide/code/UI capture with spoken-vs-shown separation**, (b) **Markdown/structured KB output + MCP/connectors**, and (c) **contractually-bound fully-EU processing** (close the non-EEA summary-LLM gap). That's a product-category pivot from "subtitles" to "video→knowledge," against a 4.5-yr-unfunded roadmap. **Likelihood: low** near-term; medium that they bolt on a shallow "AI summary" (already outsourced to a non-EU LLM — a liability, not a moat).
- **What mdreel structurally does that they can't (easily):** dual-track **spoken + verbatim on-screen text**, **portable Markdown / bring-your-own-RAG, no lock-in**, native **MCP**, and a **genuinely all-in-EU pipeline** (Vertex Gemini in europe-*) that Amberscript's own privacy policy can't currently claim. Their metered COGS floor (~€1.96–3.80/h list) is also far above mdreel's €0.65/h all-in.
- **What mdreel should steal:** (1) the **logo-wall trust play** and vertical landing pages; (2) **ISO 27001/9001 + TPN-style badges** as concrete proof, not adjectives; (3) **webhook/callback + clean REST job API** ergonomics (6GB direct / 16GB URL, glossary, translated subtitles); (4) their **"Europe's trusted partner" one-liner** — mdreel can go one better by making "fully processed in the EU, contractually" the headline Amberscript legally can't match.

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | Founded 2017, HQ Amsterdam NL | https://www.crunchbase.com/organization/amberscript | 2026-07-15 | T (Crunchbase) |
| 2 | ~$11.1M total; $10M Series A 2021-11-04 led by Endeit Capital | https://www.crunchbase.com/organization/amberscript | 2026-07-15 | T (Crunchbase) |
| 3 | Headcount 51–200 (~100) | https://pitchbook.com/profiles/company/399681-19 | 2026-07-15 | T (PitchBook/Crunchbase) |
| 4 | Machine pricing €19/5h, €29/10h, €49/25h | https://www.amberscript.com/en/pricing/ | 2026-07-15 | Q |
| 5 | Effective €/h 3.80/2.90/1.96 | https://www.amberscript.com/en/pricing/ | 2026-07-15 | E (price÷hours) |
| 6 | Output formats JSON/SRT/VTT/TEXT/STL(dep.); no Markdown | https://amberscript.github.io/api-docs/ | 2026-07-15 | Q |
| 7 | Webhook callbackUrl, hourly retry ×10 | https://amberscript.github.io/api-docs/ | 2026-07-15 | Q |
| 8 | Diarization numberOfSpeakers 0–5 | https://amberscript.github.io/api-docs/ | 2026-07-15 | Q |
| 9 | File cap 6GB direct / 16GB URL; direct job ~1h turnaround | https://amberscript.github.io/api-docs/ | 2026-07-15 | Q |
| 10 | 80+ machine langs; ~11 core API langs | https://amberscript.github.io/api-docs/ | 2026-07-15 | Q/T |
| 11 | G2 4.4/5, 72 reviews | https://www.g2.com/products/amberscript/reviews | 2026-07-15 | T (G2) |
| 12 | Capterra 4.3/5, 43 reviews | https://www.capterra.com/p/186740/AmberScript/reviews/ | 2026-07-15 | T (Capterra) |
| 13 | Web traffic ~371K (Sep24)/308K (Dec24) | https://reviewbolt.com/r/amberscript.com | 2026-07-15 | T (Similarweb) |
| 14 | Logos: BBC, Disney+, Microsoft, Puma, univs, govt | https://www.amberscript.com/en/ | 2026-07-15 | Q |
| 15 | Positioning "Europe's trusted partner…" | https://www.amberscript.com/en/ | 2026-07-15 | Q |
| 16 | ISO 27001/9001, GDPR, Frankfurt storage, DPA on request | https://www.amberscript.com/en/blog/data-security-and-privacy/ | 2026-07-15 | Q |
| 17 | Non-EEA human partners + Summary LLM outside EU; no-train is FAQ not T&C | https://www.amberscript.com/en/privacy-policy/ | 2026-07-15 | Q |
| 18 | Public API repos (dev-facing, low community) | https://github.com/amberscript/api-docs | 2026-07-15 | T (GitHub) |