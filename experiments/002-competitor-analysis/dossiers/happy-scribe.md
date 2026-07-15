# Happy Scribe — ring: adjacent · verified: 2026-07-15 · confidence: high

## 1. Vitals
- **HQ / jurisdiction:** Barcelona, Spain (operating entity historically **Happy Scribe Limited, Dublin, Ireland** — founded as an Irish startup, now Barcelona-based). EU jurisdiction throughout. [Q, T-Crunchbase/Catalonia Hub]
- **Founded:** 2017, by André Bastié (CEO) and Marc Assens Reina (CTO). [T-Crunchbase, Slator]
- **Funding:** **~€2M seed** (only known priced round), led by **K Fund**, with **Y Combinator** (YC alum), TinyVC, and business angels incl. Juan Roig / Angels Capital. Otherwise **largely bootstrapped** — founder publicly framed it as bootstrapped to €4M+ ARR. No later rounds sourced. [T-web search/Slator/SaaS Club podcast]
- **Headcount:** LeadIQ lists **~228** (May 2025); this likely blends staff with a **freelance human-transcriber pool** and is high for a bootstrapped transcription SaaS — treat as an upper bound. Core team plausibly far smaller. [T-LeadIQ, E]
- **Status: growing but maturing.** ~2M monthly visits (below), consistent product expansion (2026 added meeting recording + multi-LLM AI assistant). Recent MoM traffic dip (-9.2%) suggests a plateauing, competitive category rather than hypergrowth. [E, T-Similarweb]

## 2. Business model
Model type: **seat-gated subscription + metered AI minutes** (monthly AI-minute allowance per plan; per-minute overage; separate à-la-carte human transcription).

Pricing ladder (vendor USD, annual-billing rate; USD→EUR ≈ 0.90 applied and stated):

| Plan | Price (annual) | Included | Effective €/video-h | Overage |
|---|---|---|---|---|
| Free | $0 | 10-min AI trial; unlimited meeting recording (45 min/rec) | N/A (trial) | — |
| Basic | $8.50/mo → **€7.65** | 120 min AI/mo (2h), 1 seat | **€3.83/h** | $0.20/min → **€10.80/h** |
| Pro (most popular) | $19/mo → **€17.10** | 600 min AI/mo (10h), 3 seats | **€1.71/h** | **€10.80/h** |
| Business | $59/mo → **€53.10** | 6,000 min AI/mo (100h), 5 seats | **€0.53/h** | **€10.80/h** |
| Enterprise | Custom / contact sales | SSO/SAML, dedicated AM | unknown | unknown |

- **Entry price point:** €7.65/mo (Basic). Effective per-hour price **falls steeply with tier** (€3.83 → €0.53/h), i.e. volume discount by design.
- **Free tier shape:** 10-min AI trial (no recurring free AI); the "free" plan is really a meeting-recorder loss-leader.
- **Enterprise motion:** primarily **self-serve** up to Business; **sales-led** only at Enterprise.
- **Meters mdreel doesn't:** **seats** (1/3/5 per tier), **AI minutes as a monthly allowance** (hard-ish, overage-billed), and separate **human-service** billing. mdreel's Pro/Business are hour-capped but seat-agnostic.
- **Note vs mdreel:** at the Business tier Happy Scribe's **€0.53/h is below mdreel's €0.65/h COGS** — but it buys a transcript, not structured video→Markdown (see §3/§7).

## 3. Product & features — checklist
- [x] transcript (AI + human tiers)
- [ ] **verbatim ON-SCREEN text (slides/code/UI)** — none; audio-only ASR. **mdreel's core wedge.**
- [ ] visual descriptions — no
- [x] timestamps (word/segment-level; subtitle-grade timing for SRT/VTT)
- [x] structured/Markdown output — **partial:** exports TXT/DOCX/PDF/SRT/VTT/**JSON**; not knowledge-base Markdown with spoken-vs-shown separation
- [x] JSON/API — public REST API
- [?] webhooks — likely via API/Zapier; not confirmed on-page
- [ ] MCP server — none found
- [x] connectors — YouTube, Vimeo, Google Drive, Dropbox, **Zapier**, API
- [x] speaker ID (diarization)
- [x] languages — **150+ transcription / 80+ translation** [Q pricing page]
- [?] max video length — unlimited recording duration on Pro+; upload length cap not stated
- [x] processing-speed claims — AI "ready within minutes," ~85% accuracy; human ~99% in 4h+
- [x] retention/erasure controls — DPA + AI-training opt-out; recording history limited on lower tiers
- [ ] self-host — no
- **New (2026):** built-in **meeting recorder** and an **AI assistant offering multiple models (Claude, ChatGPT, Mistral, GLM)** to chat over transcripts — a move up-stack toward "understanding," but still audio-derived.

Output reality: the deliverable is a **timestamped, speaker-labelled transcript** editable in a browser workspace and exportable as SRT/VTT/DOCX/TXT/PDF/JSON. It captures **what was said**, not **what was shown** — slides, on-screen code, or UI text are lost. There is no structured knowledge-base schema separating spoken audio from on-screen text.

## 4. Size & customer base
- **Case studies/logos:** unknown (no prominent enterprise-logo wall sourced; skews SMB/creator/media).
- **Reviews:** **Capterra 4.7/5, 38 reviews** (dated 2026); **G2 ~4.8/5** (exact count unsourced); Trustpilot present. Small review volume relative to traffic → self-serve, low-touch, long-tail user base. [T-Capterra, T-G2]
- **Web traffic:** **~2M visits/mo, -9.2% MoM** (Similarweb, June 2025); **68.7% organic search**, 26.5% direct — an SEO-driven acquisition engine. [T-Similarweb]
- **GitHub/community:** N/A (not open-source / not dev-community-led).
- **Hiring signals:** ~228 headcount figure implies ongoing operation at scale; no specific hiring surge sourced. [T-LeadIQ]

## 5. GTM & distribution
- **Dominant channel: SEO** — ~69% organic; ranks for "transcription," "subtitles," language- and format-specific long-tail ("transcribe [language]," "convert audio to text," "SRT generator"). Heavy programmatic content/blog footprint (incl. their own "Happy Scribe review 2026" post).
- **Free tool / loss-leader:** free meeting recorder + 10-min AI trial as top-of-funnel.
- **Connectors/integrations** (YouTube/Vimeo/Drive/Zapier) as distribution surface.
- **Ads/partners/gallery:** not prominent; no curated public gallery like mdreel's.
- **Positioning (verbatim, near-verbatim from site):** *"Transcribe & subtitle your audio and video files in 150+ languages"* — creator/media/localization framing, not knowledge-base framing.
- **Who the pricing page talks to:** the **individual practitioner / small team** (marketers, podcasters, media, educators, researchers) — a per-seat, self-serve buyer. **Not** a developer, and **not** a DPO (GDPR sits on a separate /security page, not front-and-center in pricing).

## 6. EU/GDPR posture
- **Hosting:** EU Tier-IV data center marketed; **but security page also lists AWS + Heroku** (Heroku default region US-East) — a **material EU-residency caveat** mdreel can exploit. [Q-security page, per verified facts]
- **DPA:** available. [Q]
- **Subprocessor list:** exists but **not a short, EU-only list** (AWS/Heroku implied) — length/detail not fully published. [E]
- **No-training terms:** **AI-training opt-out** offered. [Q]
- **Certifications:** **ISO 27001, SOC 2 Type II, PCI DSS, AES-256.** [Q]
- **Residency premium:** **does NOT appear to charge or market a distinct EU-residency SKU** — GDPR is a trust/compliance page, not a priced tier or headline. Strong on certifications, **soft on strict data-locality** (US subprocessors).

## 7. Threat assessment
- **ICP overlap: LOW–MED.** Same *macro* space (video/audio → text, EU-hosted, GDPR-marketed) but different buyer and different output. Happy Scribe sells **transcripts/subtitles to creators, media, localization, L&D**; mdreel sells **structured video→Markdown for AI knowledge bases to 50–500-person EU software/IT teams**. Overlap is real only where an L&D/consultancy buyer conflates "transcript" with "knowledge asset."
- **What they'd need to do to kill mdreel:** add **verbatim on-screen text/slide/code/UI extraction + spoken-vs-shown separation + knowledge-base Markdown schema**, plus reposition to internal-KB/RAG buyers. **Likelihood: low–moderate.** They have the ASR pipeline, EU footprint, LLM plumbing (already multi-model in 2026) and SEO muscle — but it's a category and buyer pivot away from a healthy creator/media business; and their US subprocessor stack undercuts a strict-residency pitch. The AI-assistant move shows appetite, though.
- **What mdreel does that they structurally can't (quickly):** treat the **video frame as a first-class source** (verbatim on-screen text, spoken-vs-shown), emit **portable no-lock-in Markdown for BYO-RAG**, and offer **strict EU-only residency (Vertex europe-*, no US subprocessors)** — the opposite of their AWS/Heroku posture.
- **What mdreel should steal:** (1) their **SEO-first, organic-dominant** acquisition engine (~69% organic → mdreel's A5 distribution answer); (2) the **certification stack** (ISO 27001 / SOC 2 Type II) as table-stakes trust signals; (3) the **AI-training opt-out** as a standard published term; (4) tiered **volume pricing** that rewards heavier use.

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | Free 10-min trial; Basic $8.50/120min; Pro $19/600min; Business $59/6000min; +$0.20/min overage | https://www.happyscribe.com/pricing | 2026-07-15 | Q |
| 2 | 150+ transcription / 80+ translation languages; integrations YouTube/Vimeo/Drive/Dropbox/Zapier/API | https://www.happyscribe.com/pricing | 2026-07-15 | Q |
| 3 | 2026 plans add meeting recording + AI assistant (Claude/ChatGPT/Mistral/GLM), seats 1/3/5 | https://www.happyscribe.com/pricing | 2026-07-15 | Q |
| 4 | EU Tier-IV DC, ISO 27001, SOC 2 Type II, PCI DSS, AES-256, DPA, AI-training opt-out; AWS + Heroku listed | https://www.happyscribe.com/security | 2026-07-15 | Q |
| 5 | Founded 2017; co-founders André Bastié (CEO), Marc Assens (CTO); Barcelona/Dublin | https://www.crunchbase.com/organization/happy-scribe | 2026-07-15 | T-Crunchbase |
| 6 | ~€2M seed led by K Fund; YC-backed; TinyVC + angels; largely bootstrapped | https://slator.com/andre-bastie-scaling-happy-scribe-after-finding-instant-product-market-fit/ | 2026-07-15 | T-Slator/web |
| 7 | Headcount ~228 (May 2025) — likely incl. freelance transcriber pool | https://leadiq.com/c/happy-scribe/5ed9f40e2fd07506cb22fbfc/employee-directory | 2026-07-15 | T-LeadIQ |
| 8 | Capterra 4.7/5, 38 reviews; SMB-skewed (89% small companies) | https://www.capterra.com/p/192368/Happy-Scribe/ | 2026-07-15 | T-Capterra |
| 9 | G2 ~4.8/5 (exact review count not sourced) | https://www.g2.com/products/happyscribe/reviews | 2026-07-15 | T-G2 |
| 10 | ~2M visits/mo, -9.2% MoM, 68.7% organic / 26.5% direct (June 2025) | https://www.similarweb.com/website/happyscribe.com/ | 2026-07-15 | T-Similarweb |
| 11 | Effective €/h (€3.83 / €1.71 / €0.53) and overage €10.80/h via USD→EUR 0.90 | derived from row 1 | 2026-07-15 | E |
| 12 | Positioning "Transcribe & subtitle your audio and video files in 150+ languages" | https://www.happyscribe.com | 2026-07-15 | Q |