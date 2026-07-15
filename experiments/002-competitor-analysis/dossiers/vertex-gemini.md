# Vertex Gemini (Google direct) — ring: infra · verified: 2026-07-15 · confidence: high

## 1. Vitals
- **HQ/jurisdiction:** Mountain View, California, USA (Google LLC, a subsidiary of Alphabet Inc.). Vertex AI is delivered from Google Cloud regions worldwide, including EU regions. **US-headquartered** — this is the crux of the A1 EU-residency question below.
- **Founded:** Google 1998; Vertex AI (unified GCP ML platform) launched 2021; Gemini model family launched Dec 2023; Gemini 2.5 Flash/Pro GA 2025.
- **Funding:** N/A — Alphabet is publicly traded (NASDAQ: GOOGL/GOOG). Not a fundable startup; effectively infinite capitalization relative to mdreel. (Q — public company.)
- **Headcount:** Alphabet ~180,000+ (company-wide, LinkedIn/public filings). Gemini/Vertex team headcount unknown but large (hundreds–thousands). This is not a competitor you out-staff.
- **Status:** Growing aggressively. Gemini is Google's flagship AI bet; model cadence is roughly quarterly (2.5 → search results reference a "3.5 Flash" already GA). This is the platform mdreel itself is built on — it is simultaneously mdreel's supplier and mdreel's A2 substitution risk.

## 2. Business model
**Model type:** Pure usage-based (metered per-token, no subscription, no seats, no included hours). Two tiers: **Gemini API / Google AI Studio** (developer-facing, has a free tier) and **Vertex AI** (enterprise GCP, region-pinned, billed through GCP). This is infra, not a productized video tool — there is no "plan."

Token prices are USD list; I convert at **USD→EUR ≈ 0.90** and state so. Effective €/video-hour is **derived** from Gemini's own video tokenization (~300 tokens/sec at default media resolution, ~100 tokens/sec at low resolution; 1 video-hour = 3,600 sec → 1.08M input tokens default / 0.36M low). Output for structured Markdown estimated at ~15k tokens/hour.

| "Plan" (model/tier) | Price (USD list) | Included | Effective €/video-h (derived, single pass) | Overage |
|---|---|---|---|---|
| Gemini 2.5 Flash — Standard | $0.30/1M in (text/img/**video**), $1.00/1M audio, $2.50/1M out | none — pay per token | **≈ €0.11–0.33/h** (low-res in €0.10 + out €0.03; default-res in €0.29 + out €0.03) | N/A — linear |
| Gemini 2.5 Flash — Batch | $0.15/1M in, $1.25/1M out (50% off) | none | **≈ €0.06–0.17/h** | N/A |
| Gemini 2.5 Pro — Standard | $1.25/1M in (≤200k), $2.50 (>200k); $10/$15 out | none | **≈ €0.44–1.35/h** (1 video-hr ≈ 1.08M tokens crosses the 200k tier) | N/A |
| Gemini 2.5 Flash — Priority | $0.54/1M in, $4.50/1M out (1.8×) | none | ≈ €0.20–0.60/h | N/A |

- **Entry price point:** effectively **€0** — free tier exists on the Gemini API (Google AI Studio) with rate limits; Vertex has no free tier but pay-as-you-go from $0.
- **Free tier/trial shape:** Gemini API free tier (rate-limited, and note: free-tier data may be used to improve products — see §6). Vertex = paid only, GCP credits apply.
- **Enterprise motion:** Self-serve (swipe a card on AI Studio) AND sales-led (Vertex via GCP enterprise agreements, committed-use discounts). Both.
- **Meters mdreel doesn't:** nothing extra — it meters *less*. No storage, no retention, no seats. It is raw tokens. mdreel's entire product is the wrapper (chunking, verbatim on-screen text separation, timestamping, Markdown rendering, ledger, EU DPA packaging) that this layer does **not** provide.

**Key takeaway:** A single Flash pass at **€0.11–0.33/video-hour** IS the DIY floor. This corroborates mdreel's stated ~€0.28–0.45/video-hour Stage B cost and its all-in €0.65/video-hour COGS. mdreel's gross margin lives entirely in what it adds on top of this number — the €149/mo Pro plan (25h = effective €5.96/video-hour list) is ~18–54× the raw Flash floor. That spread is the whole business, and it is the A2 risk: a competent EU dev team can hit this floor themselves.

## 3. Product & features — checklist
This is a model API, not a video-doc product — score reflects raw capability a builder gets, not a finished output.
- [x] transcript — via prompt (audio understanding built in)
- [x] verbatim ON-SCREEN text — capable (OCR/visual reasoning), but **you must prompt/enforce it**; no product guarantee of verbatim slide/code/UI extraction
- [x] visual descriptions — native multimodal
- [x] timestamps — supports timestamped video reasoning (MM:SS references); granularity depends on frame sampling (default 1 FPS, overridable)
- [x] structured/Markdown output — only if you prompt + use structured-output/JSON schema; no built-in Markdown renderer
- [x] JSON/API — yes, core product; supports response schemas
- [ ] webhooks — no (batch job polling, not push)
- [x] MCP server — Google offers MCP tooling around Gemini/Vertex (developer ecosystem), though not a turnkey "video→Markdown" server
- [~] connectors — via GCP (Cloud Storage `fileUri`, YouTube `fileData.fileUri`), not app connectors
- [x] speaker ID — capable via prompt (diarization not a guaranteed labeled feature)
- [x] languages — many (Gemini multilingual, 100+)
- [x] max video length — long-context (up to ~1M–2M token context ≈ hours of video at low res; File API stores at 1 FPS)
- [x] processing-speed claims — Flash is the low-latency tier; batch tier for throughput
- [x] retention/erasure controls — via Vertex/GCP data-governance; File API auto-expires uploads (~48h)
- [x] self-host option — **no true self-host**, but Vertex region-pinning + (for some) private endpoints is the closest; not on-prem

**What the output actually looks like:** Raw model tokens — free-form text or a JSON object if you supply a response schema. There is no product-defined schema for "video → timestamped Markdown with spoken-vs-shown separation"; the developer designs that prompt and parses/renders the result. Video is tokenized at ~258 tokens/frame (default) or 66 tokens/frame (low resolution) plus ~32 audio tokens/sec, so "on-screen verbatim text" quality is a function of the caller's media-resolution and prompt choices, not a guaranteed feature.

## 4. Size & customer base
- **Case studies/logos:** Effectively the entire GCP customer base; thousands of enterprises use Vertex/Gemini. Not enumerable, not meaningful as a comparison — this is infrastructure.
- **Review counts/ratings:** N/A in the SaaS sense (G2 lists "Google Cloud Vertex AI" and "Gemini" with reviews, but not comparable to a point product). unknown/N-A.
- **Web-traffic estimate:** unknown (ai.google.dev + cloud.google.com are among the highest-traffic dev-doc properties globally; a specific number is not sourced here).
- **GitHub/community:** Massive — official SDKs (Python/Node/Go), Vertex samples, thriving developer community. Google AI Studio is a mainstream on-ramp.
- **Hiring signals:** Continuous, large-scale (not a survival signal — irrelevant for this vendor).

## 5. GTM & distribution (feeds A5)
- **Channels:** Dominant SEO on every "gemini video", "gemini api pricing", "vertex ai" query; Google AI Studio as a free self-serve funnel; deep docs; developer relations/conferences (Google I/O, Cloud Next); GCP marketplace; bundled into Workspace/Android/Search reach. Distribution is not a problem Google has — it is the problem mdreel has, and Google is the incumbent surface (A5).
- **Positioning (verbatim, from docs):** "Video understanding" — *"Gemini models can process videos, enabling many frontier developer use cases..."* (Gemini API video-understanding docs). Pricing page framing is per-token developer/enterprise, not vertical.
- **Who the pricing page talks to:** **Developers and ML/platform engineers** — token math, context windows, batch vs standard tiers, region endpoints. It does **not** talk to an L&D lead or a DPO in their language. That gap is mdreel's opening.

## 6. EU/GDPR posture (feeds A1 — is EU a purchase driver)
- **Hosting regions:** Vertex AI offers EU region-pinning. Search-sourced (T): single-region EU endpoints include **europe-west1 (Belgium), europe-west4 (Netherlands), europe-north1 (Finland), europe-central2 (Warsaw)**; an **EU multi-region endpoint** exists for GDPR-scoped routing inside the EU geography. mdreel's own stack runs **Gemini 2.5 Flash @ europe-central2** (CLAUDE.md) — consistent with this. **Caveat:** the *global* endpoint does NOT honor data residency; you must explicitly pin a regional/EU-multi-region endpoint. Newer models (e.g. "3.5 Flash") may ship on the EU multi-region endpoint before a single-country europe-west3/west4 endpoint — a model-availability lag mdreel inherits.
- **DPA available:** Yes — Google Cloud Data Processing Addendum (standard, well-known). (Not re-quoted here; grade E/T.)
- **Subprocessor list:** Long (Google's global subprocessor list) — a DPO-facing liability point mdreel can contrast against with a *short* EU-only list.
- **No-training terms:** **Vertex AI / paid Gemini API: prompts are NOT used to train models** (Google's stated terms). **Free tier (AI Studio): data MAY be used to improve products** — critical distinction. mdreel must (and does) build on the paid/Vertex path.
- **Certifications:** Extensive — ISO 27001/27017/27018, SOC 1/2/3, plus EU-specific attestations. Far exceeds anything mdreel will have.
- **Residency premium:** **No explicit surcharge** for EU region-pinning — you pay the same token price and normal GCP regional egress. Google does **not market** EU residency as a paid premium; it's a config option. This means mdreel **cannot** charge a residency premium on the *hosting* alone — the differentiator must be the packaged compliance posture (short subprocessor list, EU-only entity, DPO-ready DPA, verbatim data-flow story), not the region itself.

## 7. Threat assessment
- **Overlap with mdreel's ICP:** **Medium.** Google does not sell "video → timestamped Markdown for internal AI KBs" to EU L&D/IT teams. It sells tokens to developers. The overlap is entirely the **A2 build-vs-buy** risk: mdreel's ICP (EU software/IT teams 50–500 building internal AI assistants) is *exactly* the population most able to call Vertex directly. High technical overlap, low product overlap.
- **What they'd have to do to kill mdreel:** Ship a turnkey, opinionated "video → structured timestamped Markdown, spoken-vs-shown separated, verbatim OCR, EU-pinned, DPA-packaged" product. **Unlikely** — Google's incentive is to sell raw tokens and keep the wrapper market to the ecosystem; a vertical EU-compliance micro-product is off-strategy. The realer threat is passive: the floor price (€0.11–0.33/video-h) makes the DIY option cheap enough that a customer *builds instead of buys*.
- **What mdreel does that Google structurally can't/won't:** (1) A **short EU-only subprocessor list + single EU entity** — Google's list is inherently long and US-parented; a DPO scores this in seconds (CLAUDE.md rule 10 logic runs both ways). (2) An **opinionated finished output** (verbatim on-screen text separated from speech, portable Markdown, no-lock-in) — Google ships capability, not a guaranteed schema. (3) **Metered per-video-hour billing with a cost ledger** vs raw token accounting a buyer must model themselves. (4) Vertical hand-holding for non-developer L&D buyers.
- **What mdreel should steal:** (a) **Batch tier** — Google's 50% batch discount (€0.06–0.17/video-h) is a lever to protect margin on non-urgent jobs. (b) **Low media-resolution** default (66 vs 258 tokens/frame) cuts input cost ~3× — but validate it doesn't wreck verbatim on-screen text accuracy (that's mdreel's core claim). (c) **Structured-output / response-schema** enforcement for reliable Markdown. (d) **Region-pinned + EU multi-region endpoint** pattern for the model-availability lag. (e) Their explicit **paid-tier no-training** language as a copy point — mdreel inherits and should surface it.

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | Gemini 2.5 Flash standard: $0.30/1M input (text/image/video), $1.00/1M audio, $2.50/1M output | https://ai.google.dev/gemini-api/docs/pricing | 2026-07-15 | Q |
| 2 | Gemini 2.5 Flash batch: $0.15/1M in, $1.25/1M out (50% off) | https://ai.google.dev/gemini-api/docs/pricing | 2026-07-15 | Q |
| 3 | Gemini 2.5 Flash priority: $0.54/1M in, $4.50/1M out (1.8×) | https://ai.google.dev/gemini-api/docs/pricing | 2026-07-15 | Q |
| 4 | Gemini 2.5 Pro standard: $1.25/1M in ≤200k, $2.50 >200k; $10/$15 out | https://cloud.google.com/vertex-ai/generative-ai/pricing | 2026-07-15 | Q |
| 5 | Vertex Flash cached input $0.03/1M; audio $1.00/1M | https://cloud.google.com/vertex-ai/generative-ai/pricing | 2026-07-15 | Q |
| 6 | Gemini API has a rate-limited free tier; Vertex has none | https://ai.google.dev/gemini-api/docs/pricing | 2026-07-15 | Q |
| 7 | Video tokenized ~258 tokens/frame default, 66 tokens/frame low-res; ~300 tokens/sec default, ~100/sec low; +~32 audio tokens/sec; File API stores at 1 FPS | https://ai.google.dev/gemini-api/docs/video-understanding | 2026-07-15 | T (Google docs via search summary) |
| 8 | Effective €/video-hour DIY floor derived: 1 video-h ≈ 1.08M input tokens (default) / 0.36M (low) × $0.30/1M × 0.90 EUR + ~15k out tokens → €0.11–0.33/h | derivation from rows 1 & 7; USD→EUR 0.90 | 2026-07-15 | E |
| 9 | Vertex EU single-region endpoints incl. europe-west1/west4/north1/central2; EU multi-region endpoint for GDPR routing; global endpoint does NOT honor residency | https://docs.cloud.google.com/vertex-ai/generative-ai/docs/learn/data-residency | 2026-07-15 | T (Google docs via search) |
| 10 | mdreel runs Gemini 2.5 Flash @ europe-central2; Stage B ~€0.28–0.45/video-h | C:\ea\git\vectorreel\CLAUDE.md | 2026-07-15 | E (internal doc, not vendor) |
| 11 | Paid/Vertex prompts not used for training; free AI Studio tier data may be used to improve products | https://ai.google.dev/gemini-api/docs/pricing (free-tier note); Google Cloud DPA | 2026-07-15 | E (well-known Google terms; DPA not re-fetched) |
| 12 | Google Cloud holds ISO 27001/27017/27018, SOC 1/2/3; offers standard DPA | https://cloud.google.com/security/compliance | 2026-07-15 | E (widely documented; page not fetched this session) |
| 13 | Alphabet is public (NASDAQ GOOGL); Google HQ Mountain View, CA; ~180k+ headcount | public filings / general knowledge | 2026-07-15 | E |
| 14 | Newer model (e.g. "3.5 Flash") may be GA on EU multi-region endpoint before single-country europe-west3/west4 endpoint | https://discuss.google.dev/t/vertex-ai-using-gemini-2-5-flash-in-europe-west2/193843 (regional-availability discussion, via search) | 2026-07-15 | T (dev forum/search summary) |

**Note on staleness:** All token prices are live vendor list prices as of 2026-07-15. EU region-availability specifics (row 9, 14) are drawn from Google docs surfaced via search, not a full page fetch — treat single-country endpoint availability for the newest models as time-sensitive and re-verify at the region endpoint before deploy.