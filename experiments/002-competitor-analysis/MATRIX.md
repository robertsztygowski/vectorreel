# MATRIX — cross-competitor comparison (mdreel 002 competitor analysis)

> **⚠️ Point-in-time snapshot, NEVER authoritative.** Compiled 2026-07-15 from the 23 dossiers in
> `dossiers/`. Pricing pages change silently; every cell is a claim graded in its source dossier
> (Q = quoted from vendor · T = named third party · E = estimate/inference). See each dossier's
> §8 Evidence log for the receipt. This file restates **no** METRICS.md number — mdreel's own
> figures are cited by N-code only.

**FX:** USD→EUR = **0.90** throughout.
**Comparable unit:** effective **€/video-hour** = plan price ÷ included hours; overage noted
separately; **N/A** where pricing is per-seat/bundled (no consumption meter); **unknown** where no
public price exists.
**Legend:** ✅ yes · ❌ no · 〜 partial / caveated · ？ unknown / unverified.

---

## Table 1 — Direct ring (video → LLM-ready structured output)

| Field | Cloudglue | Twelve Labs | Mixpeek | VideoDB | Coactive | Flowstate |
|---|---|---|---|---|---|---|
| **Entry price** | $15 / 2h credit | Free 10h, then usage | $25/mo | Free $0 / Pro $20/mo | Enterprise only (~$75k/yr) | Enterprise only (no public price) |
| **Effective €/video-h (entry→scale)** | €6.75 → €4.32 | ~€3.9 (post-free) | ~€2.70 (video meter; OCR +$0.10/min) | €1.11 → €2.13 | unknown | unknown |
| **On-screen text capture** | ✅ | 〜 (not portable) | ✅ OCR | 〜 VLM, not verbatim | 〜 "on-screen context", no OCR endpoint | ✅ OCR |
| **Structured / Markdown output** | ❌ (JSON) | ❌ | ❌ (queryable index) | ❌ (SDK objects/JSON) | ❌ (index) | ❌ |
| **API / webhooks / MCP** | API ✅ · MCP ✅ | API ✅ · MCP ✅ | API ✅ · MCP ✅ | API ✅ · MCP ✅ | API ✅ · MCP ✅ | API ✅ · MCP ❌ |
| **EU hosting** | ❌ | ❌ | 〜 BYO-cloud | ❌ | ❌ (US transfer disclosed) | ❌ (no residency) |
| **DPA** | ？ | ？ | ？ | ❌ (none public) | ❌ (none public) | ❌ (none public) |
| **No-training term** | ？ | ？ | ？ | ？ | ？ | ✅ (SOC2 + pledge) |
| **Free-trial shape** | Prepaid credit ladder | Free 10h tier | Free tier | Free tier (no card) | Demo request only | Demo request only |
| **Primary distribution** | Dev SEO / API docs | Dev / API | Dev / API | MCP + AI-IDE ecosystem, dev SEO | Sales-led + AWS Marketplace | Sales-led, LinkedIn/founder |

## Table 2 — Adjacent ring (transcription SaaS, incl. EU-hosted)

| Field | Happy Scribe | Amberscript | Sonix | Rev | Otter | Descript | tl;dv | Fireflies |
|---|---|---|---|---|---|---|---|---|
| **Entry price** | €7.65/mo | €19/mo | $10/h or $25/mo | usage | Free / $8.33 seat | Free / seat | Free / €16 seat | Free / €9 seat |
| **Effective €/video-h (entry→scale)** | €3.83 → €0.53 | ~ audio meter | ~ $10/h | ~ audio | per-seat N/A | per-seat N/A | per-seat N/A | per-seat N/A |
| **On-screen text capture** | ❌ (audio) | ❌ | ❌ | ❌ | 〜 slide images, no OCR | ❌ | 〜 slide capture, no OCR | ❌ |
| **Structured / Markdown output** | 〜 partial | ❌ | ❌ | ❌ | ❌ | ✅ MD export | ？ | ？ |
| **API / webhooks / MCP** | API ✅ · MCP ❌ | API ✅ · MCP ❌ | API ✅ · MCP ❌ | API ✅ · MCP ❌ | limited · MCP ❌ | API ✅ · MCP ✅ | API ✅ · MCP ？ | API ✅ · MCP ？ |
| **EU hosting** | ✅ EU-hosted | 〜 EU-hosted (leaks) | ❌ (US AWS) | ❌ (US) | ❌ | ❌ | ✅ EU standard | 〜 EU = Enterprise |
| **DPA** | ✅ | 〜 on request | 〜 contractual | ✅ | ？ | 〜 on request | ✅ | ✅ |
| **No-training term** | ？ | ？ | ？ | ？ | ？ | ？ | ✅ | ？ |
| **Free-trial shape** | Short free | Free trial | 30 min free | usage | Free tier | Free tier | Free tier | Free tier |
| **Primary distribution** | SEO transcription | SEO / EU compliance | SEO | SEO / marketplace | Freemium / virality | Creator / editor | Freemium / sales-notetaker | Freemium / CRM |

## Table 3 — Infra ring (what our ICP's own devs would DIY)

| Field | AssemblyAI | Deepgram | Whisper | Vertex/Gemini | Google Video Intelligence | Azure AI Video Indexer |
|---|---|---|---|---|---|---|
| **Entry price** | usage (free credit) | usage (free credit) | OSS / API usage | usage (free credit) | usage (from $0) | usage (free 10h/2500min) |
| **Effective €/video-h (entry→scale)** | €0.14 → €0.19 | €0.26 → €0.42 | €0.16 → €0.32 (API) | €0.11 → €0.33 (floor) | from €0 | €3.11 → €10.26 |
| **On-screen text capture** | ❌ (audio) | ❌ (audio) | ❌ (audio) | 〜 capable, not guaranteed | 〜 OCR raw fragments | ✅ OCR (all video presets) |
| **Structured / Markdown output** | ❌ | ❌ | ❌ | 〜 prompt-only | ❌ | ❌ (JSON insights) |
| **API / webhooks / MCP** | API ✅ · MCP ✅ | API ✅ · MCP (community) | API ✅ · MCP ❌ | API ✅ · MCP ？ | API ✅ · MCP ❌ | API ✅ · MCP ❌ |
| **EU hosting** | ✅ free-parity | ✅ EU endpoint free | 〜 self-host only | ✅ region-pin | 〜 same price, US parent | ✅ EU regions, no premium |
| **DPA** | ✅ | ✅ (no-train opt-out) | 〜 platform | ✅ | ✅ | ✅ (Azure DPA) |
| **No-training term** | ✅ opt | ✅ opt-out | n/a (self-host) | ✅ | ✅ | ✅ |
| **Free-trial shape** | Free credit | Free credit | OSS free | Free credit | Free tier | Free trial |
| **Primary distribution** | Dev API / SEO | Dev API | OSS / community | GCP platform | GCP platform | Azure platform |

## Table 4 — Substitute ring ("do nothing" incumbents inside our ICP)

| Field | Microsoft 365 Copilot | Google Meet ("take notes for me") | Zoom AI Companion |
|---|---|---|---|
| **Entry price** | $18/user/mo add-on ($360/yr) | Bundled from $7/user/mo (Workspace) | Bundled free with paid Zoom (Pro ~$13.33/user/mo) |
| **Effective €/video-h** | N/A (per-seat) | N/A (bundled) | N/A (bundled) |
| **On-screen text capture** | ❌ | ❌ (alpha screenshots, not OCR) | ❌ (audio-only) |
| **Structured / Markdown output** | ❌ | ❌ (Google Docs) | ❌ |
| **API / webhooks / MCP** | Graph API 〜 · MCP ？ | ❌ (no notes API/MCP) | limited · MCP ❌ |
| **EU hosting** | 〜 EU Data Boundary (Anthropic carve-out) | 〜 EU storage (Business+); EU AI-processing only Enterprise Plus | 〜 Enterprise-only on request |
| **DPA** | ✅ | ✅ (Google Cloud DPA) | ✅ (on request) |
| **No-training term** | ✅ | ✅ | ✅ (post-2023 ToS revision) |
| **Free-trial shape** | No free (paid add-on) | Bundled (no separate trial) | Freemium (Basic) |
| **Primary distribution** | MS bundle / enterprise sales | Workspace bundle | Freemium / bundle / sales |

---

## Cross-ring readouts

1. **On-screen text is NOT a unique wedge.** Verbatim OCR is already shipped by Cloudglue, Mixpeek,
   Flowstate, Azure AI Video Indexer (all presets), and — as raw fragments — Google Video
   Intelligence. Twelve Labs, VideoDB, Coactive capture it 〜partially (VLM/scene, not verbatim).
   The wedge is not *capturing* on-screen text; it is the **portable, spoken-vs-shown-separated,
   no-lock-in, EU-processed Markdown artifact** — which **no** competitor produces.

2. **Nobody produces a portable Markdown deliverable except Descript** (a creator/editor, audio-only,
   US-hosted, no EU residency). Every direct/infra player outputs a **queryable platform index or
   proprietary JSON** — lock-in, not a document you own.

3. **No EU premium anywhere.** EU hosting, where offered, is priced at parity (AssemblyAI, Deepgram,
   Azure, Happy Scribe). The gap DPO buyers hit is **availability, not price**: EU *AI-processing*
   control is Enterprise-Plus-only at Google Meet, Enterprise-only at Zoom, and absent entirely
   across the US direct ring. EU-native residency is a **deal-unblocker**, not a markup.

4. **The real default competitor is the bundled recap** (Substitute ring): Copilot, Google Meet,
   Zoom AI Companion are already in the ICP's invoice at ~zero incremental cost — but all three are
   audio-only, no OCR, no Markdown, and only partially EU-compliant. That is the "do nothing" mdreel
   must beat on the **portable artifact + true EU processing** axes, not on price.

5. **Entry-price bands:** infra floor €0.11–0.42/video-h (audio, DIY, no artifact) · direct ring
   €1.11–6.75/video-h · Azure video OCR €3.11–10.26/video-h · substitutes per-seat (N/A). mdreel's
   COGS floor and price hypotheses are in METRICS.md (N6) and BUSINESS_MODEL.md §6 — not restated here.
