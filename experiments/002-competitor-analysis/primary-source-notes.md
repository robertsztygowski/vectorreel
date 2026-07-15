# Primary-source notes — captured directly via Playwright/WebFetch, 2026-07-15

> Raw verified data from official pages (not search snippets). Screenshots saved alongside.
> Feeds RESULTS.md. Point-in-time — prices as displayed on 2026-07-15.

## Cloudglue (Aviary Inc.) — Ring 1 ANCHOR
Source: https://cloudglue.dev/pricing · https://cloudglue.dev/privacy · https://docs.cloudglue.dev — checked 2026-07-15

### Pricing ladder (credit packs, one-time / top-up; "subscriptions" only via Contact Us)
| Pack | Price | Credits | Hours Indexed | Effective $/video-hour |
|---|---|---|---|---|
| Free | $0 | 200 | 25 min | — (free trial) |
| Mini | $15 | 1,000 | 2 h | $7.50/h |
| Starter (MOST POPULAR) | $45 | 3,000 | 6 h | $7.50/h |
| Builder (−22%) | $350 | 30,000 | 63 h | $5.56/h |
| Scale (−33%) | $600 | 60,000 | 125 h | $4.80/h |
| Enterprise | Contact | custom | custom | custom |

- **Credit model:** credits consumed per API request. Transcribe / Describe / Extract / Scene Segmentation / Collection indexing each = **4 credits per minute of footage**. Chat Completion = 1/request. Search = 1/search. Upload = included. (A full index pass ≈ 480 credits/video-hour — transcription + scene segmentation — which reconciles the tiers; the $/hour figures are derived from the page's own "hours indexed", so correct.)
- Effective **$7.50/h entry → $4.80/h at scale.** Entry point: **$0 free (25 min) then $15**.

### Output (what it actually is)
- "Turn any video and audio into **structured data, ready for LLMs**." Video *context engine/layer* for agents.
- **Describe** API returns moment-by-moment: transcript, diarization, visual descriptions, audio descriptions, sound, **on-screen text**.
- **Extract** (prompt or custom schema → JSON), **Segment** (shot/chapter), **Search** (semantic), **Chat Completion** (across a corpus, playable citations), **Responses API**.
- 🔴 **Scene Text Recognition — "extract text visible on screen (captions, presentations, etc.)"** is a documented, configurable output. ⚠️ **KEY:** Cloudglue DOES capture on-screen text. Output lands in **Collections** + their retrieval/chat/search stack — a platform, not a portable file. Our wedge is NOT "on-screen text exists"; it's EU residency + portable Markdown + no retrieval lock-in + explicit spoken-vs-shown separation for BYO-RAG.

### API / webhooks / MCP
- REST API, Python + JS SDKs, **Webhooks (yes)**, **official MCP Server (yes** — Claude Desktop, Cursor, Windsurf; npm `@cloudglue/cloudglue-mcp-server` + `@aviaryhq/…`; tools describe_video, extract_video_entities, segment_video_chapters, search_video_moments…), **llms.txt**, Playground, Schema Builder, Changelog. Docs on Mintlify — high quality.

### EU / GDPR posture (the wedge)
- Legal entity **Aviary Inc. (US)** (aviaryhq.com). Privacy policy effective **2025-04-12**.
- Subprocessors named: **PostHog** (analytics), **Stripe** (payments), **Google** (auth). No video-processing subprocessor / hosting region disclosed.
- **No stated data-residency option, no EU region, no explicit DPA offering, no contractual no-training term.** Generic GDPR-rights section only. Retention "as long as necessary" (vague). Homepage has zero EU/GDPR/SOC2 positioning (verified twice + null searches).

### Target buyer & distribution
- Buyer: **developers** building AI agents/apps.
- Distribution: **Y Combinator-backed**; free **Playground** (interactive demo, no code); high-quality docs + llms.txt + MCP; **Discord**; **Blog**; **Newsletter**; X + LinkedIn. Developer-led / PLG.

---

## Twelve Labs — Ring 1
Source: https://www.twelvelabs.io/pricing · https://www.twelvelabs.io/pricing-calculator — checked 2026-07-15

- **US-based**; announced **$100M Series B** (banner on pricing page).
- **Free:** 600 min (10 h) one-time cumulative, 90-day index access, no card.
- **Developer (pay-as-you-go):**
  - Marengo (retrieval) video indexing: **$0.042/min = $2.52/h** (one-time) + Embedding Infra $0.0015/min monthly + Search $4/1,000 queries.
  - Pegasus (video-language) Analyze: input video **$0.0292/min = $1.75/h** + output text **$0.0075/1k tokens**.
  - Effective: index+analyze combined ~**$4.3/h**; analyze-only ~**$1.75/h + output**.
- **Enterprise:** committed-use, custom; fine-tuning available.
- Output = **embeddings / search / analyze** via two foundation models (Marengo, Pegasus). Not a portable structured-Markdown deliverable.
- On-screen text: old dedicated `/text-in-video` endpoint **DEPRECATED** (v1.1; doc 404s; score/confidence removed). In current **v1.3 / Marengo 3.0** OCR ("text-in-visual", 89.2% TextCaps) is **folded into the visual model** and surfaced as a search modality — real capability, but not a portable document.
- **EU / GDPR:** no EU residency; DPA (twelvelabs.io/legal/dpa) hosts GCP/AWS, **EEA→US transfer under SCCs**. Not marketed. Contrast: **ElevenLabs** (unrelated audio co.) DOES market EU residency.
- Distribution: research-forward brand, Playground, big funding PR, developer docs.

---

## Ring 3 — Infrastructure / DIY (verified via WebFetch, 2026-07-15)
- **AssemblyAI** (assemblyai.com/pricing): Universal-3.5 Pro **$0.21/h** pre-recorded, Universal-2 **$0.15/h**; streaming from $0.15/h. **$50 free credits**, no card. Audio-only, no visual. 🔑 **"EU region is the same price as US" with GDPR-compliant data residency** — EU residency at NO premium.
- **Deepgram** (deepgram.com/pricing): Nova-3 monolingual pre-recorded **$0.0077/min ≈ $0.46/h** (PAYG); streaming $0.0048/min. **$200 free credit**, no expiry. 🔑 **EU Data Residency via `api.eu.deepgram.com`**; GDPR/SOC2/HIPAA; on-prem via sales — EU residency at NO premium.
- **Gemini/Vertex direct** (our own basis): Stage B ≈ €0.28–0.45/video-hour (METRICS.md N4/N4b). The true DIY floor + the "just send it to Gemini" risk (BUSINESS_MODEL §8 / A2).
- **Azure AI Video Indexer / Google Video Intelligence**: usage-priced (~$0.10–0.15/min, third-party — verify before quoting); region-selectable; primitives, not a finished document.

## Ring 4 — Substitutes (bundled incumbents, verified 2026-07-15)
- **Microsoft 365 Copilot** (microsoft.com/.../microsoft-365-copilot/pricing): Copilot Business add-on **from $18/user/mo yearly ($25.20 monthly)**; bundled M365 Business Standard+Copilot $23.50/user/mo, Business Premium+Copilot $32/user/mo. Historically standalone add-on was $30/user/mo. Meeting *intelligent recap* is a **Teams Premium (~$10/user/mo, third-party est.)** / Copilot feature.
- **Zoom AI Companion**: limited-free basic tier (≈3 meetings/mo) bundled with paid Zoom; a **paid add-on** with monthly AI credits exists (exact price did not retrieve cleanly — mark approximate; historically bundled at no extra cost; add-on ≈ $12/user/mo third-party est.).
- **Google Meet / Gemini**: meeting notes bundled into Google Workspace Gemini (~$20/user/mo add-on, largely folded into Workspace tiers).
- ⚠️ All Ring 4 produce **meeting summaries / recaps / action items** — NOT structured timestamped docs of on-screen content, NOT an API over arbitrary video files. They win the *meeting-notes* job by default because the ICP already pays for them.

## Ring 2 — transcription price points (verified 2026-07-15)
- **Happy Scribe** (happyscribe.com/pricing + /security): Basic $8.50/mo (120min ≈ $4.25/h), Pro $19/mo (600min=10h ≈ $1.90/h), Business $59/mo (6,000min=100h ≈ $0.59/h) + $0.20/min overage; 10-min free trial. **EU data center (Tier IV, ISO 27001, PCI DSS), SOC 2 Type II, AES-256, DPA available, AI-training opt-out, Barcelona HQ.** Heavily GDPR-marketed — but priced at/below US peers (no EU premium). Caveat: security page also lists AWS + Heroku (Heroku default US-East) — mild residency tension.
- **Amberscript** (amberscript.com/en/pricing): Starter €19/mo (5h, €3.8/h), Pro €29/mo (10h, €2.9/h), Power €49/mo (25h, €1.96/h). GDPR/ISO 27001+9001, GCP Frankfurt storage, DPA/NDA on request. 🚩 **Verified refutation:** privacy policy concedes "some partners are located outside the EEA" (human transcription partners receive files) and the **Summary feature sends transcripts to a third-party LLM outside the EU**; the "no-training" line is a marketing-FAQ statement, NOT a contractual T&C/DPA clause. → EU-hosted ≠ fully-EU-processed; a real residency gap mdreel (pure Vertex, no human-in-loop) beats.
- **Otter** (otter.ai/pricing): Pro $8.33/user/mo (1,200min=20h), Business $19.99/user/mo unlimited; **has "Automated slide capture" (Business+)** — slide *images* into transcript, not structured verbatim OCR text. US-hosted.
- **Fireflies** (fireflies.ai/pricing): Free / Pro $10 / Business $19 / Enterprise $39 per seat; unlimited transcription. GDPR/SOC2/HIPAA but **no EU data-residency option stated**. Meeting notetaker.
- **tl;dv** (tldv.io): markets "slide capture" in the record/summarize workflow — **no structured on-screen-text extraction disclosed** (verified: one sentence, no technical detail). EU-founded (Germany), GDPR-forward.
- **Sonix / Rev / Descript** (US): ~$5–22/h or seat-based (Sonix ~$10/h PAYG, third-party); US-hosted; GDPR DPAs but no EU-residency lead; transcript/editor output.

## Amberscript no-training / residency — verified nuance
- EU storage in GCP Frankfurt CONFIRMED, but the "never leaves Europe / no-training contractual guarantee" is REFUTED by their own privacy policy (non-EEA transcription partners; Summary → US LLM). The no-training statement is marketing FAQ, not a T&C/DPA clause.

## Cross-cutting takeaways (folded into RESULTS.md)
1. **Q(b) correction:** verbatim on-screen text is already a Cloudglue (Scene Text Recognition) + Twelve Labs (visual-model OCR) capability — NOT a clean open wedge. The wedge is packaging (portable Markdown, no retrieval lock-in) + EU residency + spoken/shown separation + the citable-document artifact.
2. **Q(a):** entry points are LOW ($0 free → $15 Cloudglue; €19 Amberscript; $8.50 Happy Scribe). Per-hour: infra €0.28–0.46; EU transcript €0.59–4.25; structured+visual $4.80–7.50. **No one charges an EU premium** — AssemblyAI/Deepgram give EU residency free; EU SaaS prices at/below US. EU is a DPO deal-unblocker, not a markup.
3. **Q(c):** Cloudglue gap for DPO buyers = no residency region, no DPA offering, no no-training term, US entity/CLOUD Act, vague retention (confirmed from their privacy page). Even EU-hosted incumbents leak (Amberscript).
4. **Q(d) (Cloudglue distribution):** YC + free Playground + docs/llms.txt/MCP + Discord + blog + newsletter. Developer PLG, not sales-led. Their on-ramp is a $15 impulse buy vs our 1h-credit→€149 canyon.
