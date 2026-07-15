export const meta = {
  name: 'mdreel-competitor-dossiers',
  description: 'Research one evidence-graded dossier per competitor across four rings for mdreel',
  phases: [
    { title: 'Discover', detail: 'find extra direct-ring video-RAG startups' },
    { title: 'Dossiers', detail: 'one research agent per competitor → filled template' },
  ],
}

// ---- shared context every agent gets ----
const CONTEXT = `
mdreel = EU-hosted SaaS that turns internal company video (demos, trainings, meetings) into
structured, timestamped Markdown for AI knowledge bases: spoken audio + VERBATIM on-screen text
(slides/code/UI), spoken-vs-shown separated, processed entirely in the EU (GCP europe-* + Vertex
Gemini). Output = portable Markdown files, bring-your-own-RAG, no lock-in. ICP: EU software/IT teams
(50-500) building internal AI assistants; L&D teams; AI consultancies. Our measured all-in COGS is
€0.65/video-hour. Our price hypotheses: Pro €149/mo for 25h (hard cap), Business €690/mo for 150h
(+€6/h overage), 1h one-time trial credit, no free tier.

The FOUR RINGS: direct (video → LLM-ready structured output), adjacent (transcription SaaS incl
EU-hosted), infra (what a dev team would DIY with: STT/vision APIs & cloud primitives), substitute
(bundled meeting-recap incumbents that win by default: Teams Copilot, Zoom, Meet).
`

const RULES = `
EVIDENCE RULES (non-negotiable):
- Every factual claim gets a row in the §8 Evidence log: | # | Claim | Source URL | Checked (YYYY-MM-DD) | Grade |
- Grade each: Q = quoted verbatim from the VENDOR'S OWN page · T = third-party (NAME it: G2,
  Crunchbase, press, review site) · E = estimate/inference (state the derivation).
- Write "unknown" rather than guess. Do NOT invent funding, headcount, review counts, or prices.
- Comparable unit = effective €/video-hour = plan price ÷ included hours; overage separately.
  Use USD→EUR ≈ 0.90 and SAY you used it. If a tool is per-seat/unlimited, say so and give the
  per-seat price instead, noting €/h is N/A.
- Today is 2026-07-15. Prefer the vendor's live pricing/docs pages. Note when a figure is stale.
- Use WebSearch + WebFetch. Fetch the actual pricing page, docs, and a funding/company source.
`

const TEMPLATE = `
Return ONLY the filled dossier as GitHub-flavored Markdown, starting exactly with the header line.
Use THIS structure exactly:

# <Name> — ring: <direct|adjacent|infra|substitute> · verified: 2026-07-15 · confidence: <high|med|low>

## 1. Vitals
HQ/jurisdiction · founded · funding (rounds, amounts, investors, dates) · headcount (LinkedIn est) ·
status (growing/flat/zombie — say why). Use "unknown" where you cannot source it.

## 2. Business model
Model type (subscription/usage/credits/seat). Full pricing ladder as a table:
| Plan | Price | Included | Effective €/video-h | Overage |
Entry price point · free tier/trial shape · enterprise motion (sales-led vs self-serve) ·
anything they meter that mdreel doesn't (seats, storage, queries, index retention).

## 3. Product & features — checklist (mark [x]/[ ]/[?])
[ ] transcript  [ ] verbatim ON-SCREEN text (slides/code/UI)  [ ] visual descriptions
[ ] timestamps (state granularity)  [ ] structured/Markdown output  [ ] JSON/API  [ ] webhooks
[ ] MCP server  [ ] connectors (which?)  [ ] speaker ID  [ ] languages  [ ] max video length
[ ] processing-speed claims  [ ] retention/erasure controls  [ ] self-host option
Then 3 sentences on what the output ACTUALLY looks like (find a real sample/schema if possible).

## 4. Size & customer base — evidence, not vibes
Case studies/logos (who?) · review counts + ratings (G2/Capterra, dated) · web-traffic estimate
(name the source) · GitHub stars/community size if dev-facing · hiring signals. "unknown" if unsourced.

## 5. GTM & distribution (feeds mdreel's A5 distribution risk)
Observed channels: SEO (what ranks?), free tool, gallery/demo, community, ads, partners ·
their positioning sentence VERBATIM · who the pricing page talks to (developer? team lead? DPO?).

## 6. EU/GDPR posture (feeds mdreel's A1 "is EU a purchase driver")
Hosting regions · DPA available? · subprocessor list (short/long?) · no-training terms ·
certifications (SOC2/ISO) · do they CHARGE or MARKET a residency premium?

## 7. Threat assessment
Overlap with mdreel's ICP (high/med/low + why) · what they'd have to do to kill mdreel (+ how likely) ·
what mdreel does that they structurally can't · what they do that mdreel should steal.

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | ... | https://... | 2026-07-15 | Q |
`

function prompt(c) {
  return `You are a competitive-intelligence analyst producing ONE evidence-graded dossier.
${CONTEXT}
TARGET: **${c.name}** — ring: **${c.ring}**. Official URL(s): ${c.urls}
${c.seed ? `\nVERIFIED PRIMARY-SOURCE FACTS (already checked 2026-07-15 from the vendor's own pages — grade these Q with the given URLs; do NOT contradict them; spend your research budget on the GAPS: §1 vitals/funding/headcount, §4 size/reviews/traffic, §5 GTM, and anything below marked unknown):\n${c.seed}\n` : ''}
${RULES}
${TEMPLATE}`
}

// ---- competitor registry ----
const COMPETITORS = [
  { name: 'Cloudglue', ring: 'direct', urls: 'https://cloudglue.dev/pricing, https://docs.cloudglue.dev, https://cloudglue.dev/privacy', effort: 'high', seed:
`- Operator: Aviary Inc. (US), aviaryhq.com. Y Combinator-backed. Privacy policy effective 2025-04-12.
- Pricing (credit packs, "subscriptions? contact us"): Free $0/200cr/25min; Mini $15/1,000cr/2h; Starter $45/3,000cr/6h (Popular); Builder $350/30,000cr/63h (-22%); Scale $600/60,000cr/125h (-33%); Enterprise contact. Effective $7.50/h (Mini/Starter) → $4.80/h (Scale). Credits: transcribe/describe/extract/scene-segmentation each 4 credits/min; chat 1/req; search 1/search.
- Output: structured LLM-ready data. Describe (transcript, diarization, visual descriptions, audio descriptions, sound, ON-SCREEN TEXT), Extract (prompt/custom schema→JSON), Segment (shots/chapters), Search, Chat Completion (playable citations), Responses API. Scene Text Recognition = documented ("extract text visible on screen"). Lands in Collections + their retrieval stack (lock-in).
- API/webhooks/MCP: REST, Python+JS SDKs, Webhooks YES, official MCP server YES (npm @cloudglue/cloudglue-mcp-server + @aviaryhq; tools describe_video, extract_video_entities, segment_video_chapters, search_video_moments), llms.txt YES, Playground, Schema Builder, Changelog. Docs on Mintlify.
- EU/GDPR: NO stated hosting region, NO data-residency option, NO explicit DPA, NO contractual no-training term. Subprocessors PostHog/Stripe/Google. Generic EEA rights only. US developer-infra play.` },

  { name: 'Twelve Labs', ring: 'direct', urls: 'https://www.twelvelabs.io/pricing, https://www.twelvelabs.io/pricing-calculator, https://docs.twelvelabs.io', effort: 'high', seed:
`- US company; $100M Series B (banner on pricing page). Models: Marengo (retrieval/embeddings), Pegasus (video-language generation).
- Pricing Developer PAYG: Marengo video indexing $0.042/min = $2.52/h (one-time) + Infrastructure $0.0015/min + Search $4/1,000 queries; Pegasus Analyze input $0.0292/min = $1.75/h + output text $0.0075/1k tokens. Free 600 min (10h) cumulative, 90-day index access, no card. Enterprise committed-use custom.
- On-screen text: old dedicated /text-in-video endpoint DEPRECATED (v1.1, doc 404s); current v1.3/Marengo 3.0 folds OCR ("text-in-visual", 89.2% TextCaps) into the visual model, surfaced as a search modality — not a portable document.
- EU/GDPR: NO EU residency. DPA (twelvelabs.io/legal/dpa) hosts GCP/AWS, authorises EEA→US transfer under SCCs. Not marketed.` },

  { name: 'Mixpeek', ring: 'direct', urls: 'https://mixpeek.com, https://mixpeek.com/pricing, https://mixpeek.com/comparisons/mixpeek-vs-twelvelabs', effort: 'med' },

  { name: 'Happy Scribe', ring: 'adjacent', urls: 'https://www.happyscribe.com/pricing, https://www.happyscribe.com/security', effort: 'med', seed:
`- Pricing (USD, automatic transcription): Free 10-min trial; Basic $8.50/mo (annual) 120 min (~$4.25/h); Pro $19/mo 600 min=10h (~$1.90/h); Business $59/mo 6,000 min=100h (~$0.59/h); +$0.20/min overage.
- Transcript/subtitles only (+ human tier). HQ Barcelona.
- EU/GDPR (strongest EU marketing): EU Tier-IV data center, ISO 27001, PCI DSS, SOC 2 Type II, AES-256, DPA available, AI-training opt-out. Caveat: security page also lists AWS + Heroku (Heroku default US-East).` },

  { name: 'Amberscript', ring: 'adjacent', urls: 'https://www.amberscript.com/en/pricing/, https://www.amberscript.com/en/blog/data-security-and-privacy/, https://www.amberscript.com/en/privacy-policy/', effort: 'med', seed:
`- Pricing (EUR, machine): Starter €19/mo 5h (€3.8/h); Pro €29/mo 10h (€2.9/h); Power €49/mo 25h (€1.96/h). Also human >99% tier + one-time credits.
- Transcript/subtitles only. NL-based. ISO 27001 & 9001, GDPR, GCP Frankfurt storage, DPA/NDA on request.
- VERIFIED CAVEAT: privacy policy concedes some partners outside the EEA (human transcription partners receive files), and the Summary feature sends transcripts to a third-party LLM OUTSIDE the EU. The "third parties do not store/train" line is a marketing-FAQ statement, NOT a contractual T&C/DPA clause. So EU-storage ≠ fully-EU-processed.` },

  { name: 'Sonix', ring: 'adjacent', urls: 'https://sonix.ai/pricing', effort: 'med' },
  { name: 'Rev', ring: 'adjacent', urls: 'https://www.rev.com/pricing', effort: 'med' },

  { name: 'Otter', ring: 'adjacent', urls: 'https://otter.ai/pricing', effort: 'med', seed:
`- Pricing (USD/user/mo): Basic free (300 min/mo); Pro $8.33 annual (1,200 min/user); Business $19.99 annual unlimited; Enterprise custom. Per-seat meeting notetaker; €/h N/A.
- Has "Automated slide capture" at Business+ — but slide IMAGES into the transcript, not structured verbatim OCR text. US-hosted, SOC 2. No EU residency.` },

  { name: 'Descript', ring: 'adjacent', urls: 'https://www.descript.com/pricing', effort: 'med' },
  { name: 'tldv', ring: 'adjacent', urls: 'https://tldv.io/pricing/, https://tldv.io/features/meeting-recordings-transcriptions/', effort: 'med', seed:
`- EU-founded (Germany), GDPR-forward meeting notetaker (Zoom/Meet/Teams). Markets "slide capture" in the record/summarize workflow but discloses NO structured on-screen-text extraction (verified: one sentence, no technical detail). Per-seat pricing.` },

  { name: 'Fireflies', ring: 'adjacent', urls: 'https://fireflies.ai/pricing', effort: 'med', seed:
`- Pricing (USD/seat/mo): Free (400 min storage/team); Pro $10 annual (8,000 min/seat); Business $19 annual unlimited (MOST POPULAR); Enterprise $39. Unlimited transcription. Per-seat; €/h N/A.
- GDPR/SOC 2 Type II/HIPAA but NO EU data-residency option stated. Meeting notetaker.` },

  { name: 'AssemblyAI', ring: 'infra', urls: 'https://www.assemblyai.com/pricing', effort: 'med', seed:
`- Pricing (USD): Universal-3.5 Pro $0.21/h pre-recorded; Universal-2 $0.15/h; streaming from $0.15/h. $50 free credits, no card. Audio-only STT, no visual understanding. Add-ons: speaker diarization +$0.02-0.12/h, etc.
- EU/GDPR: "EU region is the same price as US" with GDPR-compliant data residency — EU residency at NO premium.` },

  { name: 'Deepgram', ring: 'infra', urls: 'https://deepgram.com/pricing', effort: 'med', seed:
`- Pricing (USD, PAYG): Nova-3 monolingual pre-recorded $0.0077/min (~$0.46/h); streaming $0.0048/min. $200 free credit, no expiry. Audio-only STT.
- EU/GDPR: EU Data Residency via dedicated endpoint api.eu.deepgram.com; GDPR/SOC 2/HIPAA/CCPA; on-prem via sales. EU residency at NO premium.` },

  { name: 'Whisper (OpenAI)', ring: 'infra', urls: 'https://openai.com/api/pricing/, https://github.com/openai/whisper', effort: 'med' },
  { name: 'Vertex Gemini (Google direct)', ring: 'infra', urls: 'https://ai.google.dev/gemini-api/docs/pricing, https://cloud.google.com/vertex-ai/generative-ai/pricing', effort: 'med', seed:
`- This is the model layer mdreel itself is built on (Gemini 2.5 Flash @ europe-central2). It IS the DIY floor / the A2 "just send it to Gemini yourself" risk. Report current Gemini video/token pricing and EU region availability. mdreel's own Stage B cost is ~€0.28-0.45/video-hour at this layer.` },

  { name: 'Azure AI Video Indexer', ring: 'infra', urls: 'https://azure.microsoft.com/en-us/pricing/details/video-indexer/', effort: 'med' },
  { name: 'Google Video Intelligence', ring: 'infra', urls: 'https://cloud.google.com/video-intelligence/pricing', effort: 'med' },

  { name: 'Microsoft 365 Copilot (Teams Premium)', ring: 'substitute', urls: 'https://www.microsoft.com/en-us/microsoft-365-copilot/pricing, https://www.microsoft.com/en-us/microsoft-teams/premium', effort: 'med', seed:
`- The "do nothing" incumbent already inside the ICP's Microsoft tenant. Copilot Business add-on from $18/user/mo (paid yearly; $25.20 monthly); bundles M365 Business Standard+Copilot $23.50/user/mo, Business Premium+Copilot $32/user/mo. Teams Premium ~$10/user/mo (third-party). Delivers meeting intelligent recap, transcript, summary, action items. Per-seat; €/h N/A. Verify Teams Premium price + whether recap needs Copilot vs Teams Premium.` },

  { name: 'Zoom AI Companion', ring: 'substitute', urls: 'https://www.zoom.com/en/products/ai-assistant/, https://www.zoom.com/en/pricing/', effort: 'med' },
  { name: 'Google Meet (Gemini)', ring: 'substitute', urls: 'https://workspace.google.com/pricing, https://support.google.com/meet/answer/14100714', effort: 'med' },
]

function slug(name) {
  return name.toLowerCase()
    .replace(/\(.*?\)/g, '')
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '')
}

phase('Discover')
const DISCOVERY = await agent(
  `${CONTEXT}\nYou are scouting the DIRECT ring (video → LLM-ready structured output / video-RAG /
video-ingestion). Beyond Cloudglue, Twelve Labs and Mixpeek, find up to 3 more real, currently-active
startups or products that ingest video and output structured/LLM-ready data or a video knowledge/RAG
layer (candidates to check: VideoDB, Coactive AI, Marengo/other, Vidrovr, Anthropic/other, Reducto,
Aondo, Trieve-video, etc. — only include ones that genuinely exist and fit). For EACH, return: name,
official URL, one-line what-it-does, and whether it has PUBLIC pricing. Return a short JSON array
[{name,url,does,hasPublicPricing}]. Use WebSearch. Do not invent companies.`,
  { label: 'discover:direct-ring', phase: 'Discover' }
)
log(`Discovery returned ${String(DISCOVERY).slice(0, 300)}`)

phase('Dossiers')
const dossiers = await parallel(
  COMPETITORS.map((c) => () =>
    agent(prompt(c), { label: `dossier:${slug(c.name)}`, phase: 'Dossiers', effort: c.effort })
      .then((md) => ({ slug: slug(c.name), name: c.name, ring: c.ring, md }))
      .catch(() => ({ slug: slug(c.name), name: c.name, ring: c.ring, md: null }))
  )
)

return {
  discovery: DISCOVERY,
  dossiers: dossiers.map((d) => ({ slug: d.slug, name: d.name, ring: d.ring, ok: !!d.md, md: d.md })),
}
