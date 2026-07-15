# 002 — Competitor Analysis: pricing, positioning & feature landscape for mdreel

> 🧊 **STATUS: POINT-IN-TIME MEMO — NEVER AUTHORITATIVE.** Written **2026-07-15**. A reasoning
> trail, not instructions. Prices and competitor facts were true on the date each was checked and
> **go stale fast** (Twelve Labs shipped a major API generation between 2023 and now; Cloudglue's
> changelog moves weekly). **If this memo ever contradicts a living doc, the living doc wins**
> (CLAUDE.md). No decision here is binding until the founder reads it and writes it into
> BUSINESS_MODEL.md / METRICS.md. **This memo does not edit any living doc.**
>
> **Every load-bearing number cites a source URL + the date it was checked.** Quoted = read off the
> vendor's own page. *Estimate* / *third-party* = flagged inline. Our own figures (N-codes) are
> cited to METRICS.md.
>
> **🔄 Reconciled 2026-07-15** against the full 23-dossier set (6 added after this memo's first
> draft: `videodb`, `coactive`, `flowstate`, `azure-ai-video-indexer`, `google-meet`,
> `zoom-ai-companion`). See `MATRIX.md` for the cross-competitor table. The memo's substance holds;
> the reconciliation **hardened** three previously-hedged rows (Azure video pricing, Zoom/Meet
> substitute pricing) and **strengthened** the §7 on-screen-text finding with three more OCR-capable
> competitors. No conclusion reversed.

---

## 0. How to read this / methodology

- **Primary sources** were fetched directly (Playwright + WebFetch) on **2026-07-15**: Cloudglue
  pricing/privacy/docs, Twelve Labs pricing, AssemblyAI, Deepgram, Happy Scribe, Amberscript,
  Otter, Fireflies, M365 Copilot. Anchor screenshots saved alongside
  (`cloudglue-pricing-2026-07-15.png`, `twelvelabs-pricing-2026-07-15.png`). Raw extracts:
  `primary-source-notes.md`.
- A **deep-research fan-out** (107 sub-agents, 111 claims extracted, 25 adversarially
  verified 2/3-vote, 3 killed) cross-checked the anchors and swept the rest. Verified-claim log:
  `workflow-verified-claims.txt`.
- **Currency:** native currency is kept as-is. For cross-tool band comparison, treat **1 USD ≈
  €0.90** (*approximate, mid-2026*) — the rings are far enough apart that FX changes no conclusion.
- **Our anchors** (cited to METRICS.md): break-even COGS **N6 ≈ €0.65/video-hour**;
  contribution/account **N0 ≈ €131/mo**; fixed base **N2 ≈ €300/mo**; survival **N1a ≈ 2–3
  accounts**; CAC ceiling **N23 ≈ €390**; current price hypotheses **BUSINESS_MODEL §6** (Pro
  €149/25 h hard-cap; Business €690/150 h + €6/h overage; N33 1 h trial credit).

---

## 1. The four rings — at a glance

**Effective price per video-hour, cheapest → dearest (all checked 2026-07-15):**

| Band | Players | Effective €/$ per video-hour | What you actually get |
|---|---|---|---|
| **Infra / DIY** | Gemini/Vertex direct, Deepgram, AssemblyAI | **€0.28–0.46/h** (Vertex direct is our N4/N4b) | Raw STT or raw model output — you build the pipeline |
| **EU transcription SaaS** | Happy Scribe, Amberscript, Sonix | **€0.59–4.25/h** | Cleaned transcript / subtitles. **Transcript only.** |
| **Meeting notetakers** | Otter, Fireflies, tl;dv | **$8–39 / user / mo, "unlimited"** | Auto meeting notes + summary; some slide *images* |
| **Bundled substitutes** | Teams/Copilot, Zoom, Meet | **$10–32 / user / mo** (already paid) | Meeting recap inside a suite the ICP owns |
| **Structured video → LLM (DIRECT)** | **Cloudglue**, Twelve Labs, Mixpeek | **$4.80–7.50/h** (Cloudglue) · **~$1.75–4.3/h** (Twelve Labs components) | Structured, multimodal, LLM-ready data **+ their own retrieval/search stack** |
| **mdreel (hypothesis)** | — | **€4.60–5.96/h** (BUSINESS_MODEL §6) | Structured **portable Markdown**, spoken+shown separated, EU-resident, no lock-in |

**The one-sentence read:** mdreel's hypothesised per-hour price sits **squarely inside the
structured-video band** with Cloudglue — *if* the buyer files us there. If the buyer files us with
transcription (€0.59–4.25/h) or with the meeting recap they already own, €5.96/h looks expensive.
**Pricing power is entirely a function of which shelf the buyer puts us on** — which is exactly the
"never say transcription" rule (CLAUDE.md) and the A1 question, now with numbers behind it.

---

## 2. Ring 1 — DIRECT: video → LLM-ready structured output

### 2.1 Cloudglue (the anchor) — go deep

Operated by **Aviary Inc. (US)** (aviaryhq.com), **Y Combinator-backed**. Positioning verbatim:
*"Video context engine for AI … The API to structure, search, and reason over video context for
developers."* Source: https://cloudglue.dev, https://docs.cloudglue.dev/introduction — checked 2026-07-15.

**Pricing ladder** (credit packs; *"Looking for subscriptions? Contact us"* — the ladder itself is
prepaid, not subscription). Source: https://cloudglue.dev/pricing — checked 2026-07-15:

| Pack | Price (USD) | Credits | Hours indexed | **Effective $/video-hour** |
|---|---|---|---|---|
| Free | $0 | 200 | 25 min | — (trial) |
| Mini | **$15** | 1,000 | 2 h | **$7.50/h** |
| Starter *(MOST POPULAR)* | **$45** | 3,000 | 6 h | **$7.50/h** |
| Builder *(−22%)* | **$350** | 30,000 | 63 h | **$5.56/h** |
| Scale *(−33%)* | **$600** | 60,000 | 125 h | **$4.80/h** |
| Enterprise | Contact | custom | custom | custom |

- **Metering:** credits per successful API request. Transcribe / Describe / Extract / Scene
  Segmentation / Collection indexing each = **4 credits per minute of footage**; Chat = 1/request;
  Search = 1/search. *(A full index pass runs ~480 credits/video-hour — transcription + scene
  segmentation — which reconciles the tiers exactly; the effective $/hour figures above are derived
  from the page's own stated "hours indexed," so they are correct.)*
- **Entry point: $0 free (25 min) → $15 impulse buy.** Per-hour **$7.50 → $4.80** with volume.
- **Output** = structured, multimodal, LLM-ready data via composable APIs: **Describe** (moment-by-moment:
  transcript, diarization, visual descriptions, audio descriptions, sound, **on-screen text**),
  **Extract** (prompt or **custom schema** → JSON), **Segment** (shots/chapters), **Search**
  (semantic), **Chat Completion** (across a corpus, *with playable citations*), **Responses API**.
  🔴 **Crucially it also has "Scene Text Recognition — extract text visible on screen (captions,
  presentations, etc.)"** as a documented, configurable output. Output lands in **Collections** +
  their retrieval/chat/search stack — **a platform, with gravity**, not a portable file.
- **API / webhooks / MCP:** REST, Python + JS SDKs, **Webhooks**, **official MCP server** (npm
  `@cloudglue/cloudglue-mcp-server`, also `@aviaryhq/…`; tools `describe_video`,
  `extract_video_entities`, `segment_video_chapters`, `search_video_moments`, …), **llms.txt**,
  Playground, Schema Builder, Changelog. Docs on Mintlify — high quality. Sources:
  https://github.com/cloudglue/cloudglue-mcp-server, https://docs.cloudglue.dev — checked 2026-07-15.
- **EU / GDPR posture — the wedge.** Privacy policy (https://cloudglue.dev/privacy, effective
  2025-04-12): subprocessors **PostHog / Stripe / Google**; **no stated hosting region, no data
  residency option, no explicit DPA offering, no contractual no-training term**; generic EEA
  data-subject rights (access/erasure/portability) only; retention "as long as necessary" (vague).
  Two independent WebFetches of the homepage + null web searches confirm **EU/GDPR is nowhere in
  their positioning.** A US developer-infra play.
- **Buyer:** developers building AI agents/apps ("without building your own video-understanding stack").
- **Distribution:** YC brand; free **Playground** (zero-code demo); **docs + llms.txt + MCP**
  (be-what-the-agent-recommends); **Discord**; **Blog**; **Newsletter**; X + LinkedIn. Classic
  developer PLG — no sales gate, no free-tool abuse surface (their free tier is account-gated credits).

### 2.2 Twelve Labs

US, **$100M Series B** (banner on pricing page). Source: https://www.twelvelabs.io/pricing,
https://www.twelvelabs.io/pricing-calculator — checked 2026-07-15.

- **Free:** 600 min (10 h) cumulative, 90-day index access, no card. **Developer (PAYG):** Marengo
  video indexing **$0.042/min = $2.52/h** (one-time) + Infrastructure $0.0015/min + Search $4/1,000
  queries; **Pegasus Analyze** input video **$0.0292/min = $1.75/h** + output text **$0.0075/1k
  tokens**. Effective: index+analyze combined **~$4.3/h**; analyze-only ~$1.75/h + tokens.
  **Enterprise:** committed-use, custom; fine-tuning available.
- **Output** = a two-model **video-foundation-model platform** (Marengo = retrieval/embeddings,
  Pegasus = video-language generation). On-screen text **OCR exists** but — verified — the old
  dedicated `/text-in-video` endpoint is **deprecated** (v1.1); in current **v1.3 / Marengo 3.0**,
  OCR ("text-in-visual", 89.2% TextCaps) is **folded into the visual model** and surfaced as a
  **search modality**, not a portable structured document. Sources:
  https://docs.twelvelabs.io/docs/guides/search, https://www.twelvelabs.io/blog/marengo-3-0.
- **EU / GDPR:** **no EU residency.** DPA (https://www.twelvelabs.io/legal/dpa) hosts on GCP/AWS with
  no EU region and authorises **EEA → US transfer under SCCs**. GDPR baseline (Trust Center + DPA),
  but not marketed. *(Verified contrast: **ElevenLabs** — an unrelated audio company — DOES market
  EU data residency with a dedicated page + enterprise tier. So an EU-residency premium play exists
  in the broader AI-audio market; no video-understanding player has taken it.)*
- **Buyer:** developers/enterprises building video search & analytics. **Distribution:** research
  brand, big funding PR, Playground, developer docs.

### 2.3 Mixpeek and the rest of the field

- **Mixpeek** — multimodal (video/image/audio/text) extraction + retrieval pipeline; positions
  directly against Twelve Labs (https://mixpeek.com/comparisons/mixpeek-vs-twelvelabs). Developer
  infra, usage-priced, **no EU-residency positioning found.** Same category as Cloudglue/Twelve Labs:
  a retrieval platform, not a portable-document product.
- **VideoDB** 🇺🇸 (*added 2026-07-15, own dossier*) — "video database" / video-RAG SDK. Free tier +
  **Pro $20/mo** ($20 rolling credit); transcription $0.01/min, so **effective ~€1.11/video-h
  transcript-only → ~€2.13/h with scene indexing** (E, derived from unit rates). On-screen text is
  captured only via **VLM scene prompts — interpretive, not verbatim OCR.** Output is **SDK
  objects / REST JSON**, no Markdown export. **MCP server yes** (targets Claude Code/Cursor/Codex).
  **No EU residency, no public DPA.** Distributes through the **AI-IDE / MCP ecosystem** — the
  cheapest direct-ring on-ramp found, but a queryable index, not a document you own.
- **Coactive AI** 🇺🇸 (*added, own dossier*) — enterprise "Multimodal Application Platform" (~$75k/yr
  ACV, *third-party est.*). **100% sales-led** (demo → AE → contract) + AWS Marketplace; MCP server
  yes. On-screen text is "context", **no discrete OCR endpoint** found. **US-hosted, privacy policy
  affirmatively discloses EU→US transfer, no public DPA.** Targets Fortune-500 media/ML teams — a
  different buyer, price point, and regulatory context from mdreel's ICP.
- **Flowstate** 🇺🇸 (*added, own dossier*) — video-intelligence platform, **no public pricing**
  (enterprise-only; €/h **unknown**). Does ship **verbatim OCR** (listed in its own architecture
  post) and has a **strong no-training pledge + SOC 2 Type II** — but **zero EU data-residency
  guarantee, no public DPA, California jurisdiction, no MCP.** Output is a platform index, not
  portable Markdown.
- The **video-RAG / video-ingestion** startup field is small and developer-infra-shaped. **None
  found positions on EU residency, and none ships a portable-Markdown-document product** — they all
  sell an embeddings/collections/search API you build *on top of*. That gap is mdreel's actual
  white space (see §7).

---

## 3. Ring 2 — ADJACENT: transcription SaaS (incl. EU-hosted)

All checked 2026-07-15. **Transcript / subtitle output only — none produces structured,
visual-aware, spoken-vs-shown Markdown.**

| Player | Entry / ladder | Effective per-hour | EU / GDPR posture | Output |
|---|---|---|---|---|
| **Amberscript** 🇳🇱 | €19 / €29 / €49 mo (5/10/25 h) | **€1.96–3.8/h** | GDPR, ISO 27001/9001, GCP Frankfurt **storage**, DPA/NDA on request. ⚠️ **But**: privacy policy concedes **non-EEA transcription partners** get files, and the **Summary feature ships transcripts to a US LLM**; "no-training" is a marketing-FAQ line, **not a contractual term**. | Transcript/subtitles |
| **Happy Scribe** 🇪🇸 | $8.50 / $19 / $59 mo (120/600/6,000 min) + $0.20/min | **$0.59–4.25/h** | **Strongest EU marketing:** EU Tier-IV data center, ISO 27001, PCI DSS, SOC 2 Type II, AES-256, DPA, **AI-training opt-out**, Barcelona HQ. ⚠️ Mild tension: page also lists AWS + Heroku (Heroku default US-East). | Transcript/subtitles + human tier |
| **Otter** 🇺🇸 | Free / $8.33 / $19.99 user/mo | "unlimited" at Business | US-hosted; SOC 2. No EU residency. | Meeting notes; **slide *image* capture** (Business+), not structured OCR text |
| **Fireflies** 🇺🇸 | Free / $10 / $19 / $39 seat | "unlimited" | GDPR/SOC2/HIPAA, **no EU-residency option** | Meeting notes |
| **tl;dv** 🇩🇪 | Free + paid tiers | per-seat | EU-founded, GDPR-forward | Meeting notes; "slide capture" in the record/summarise flow — **no structured on-screen-text extraction disclosed** |
| **Sonix / Rev / Descript** 🇺🇸 | ~$5–10/h or seat-based | ~$5–22/h (Sonix ~$10/h PAYG, *third-party*) | US-hosted; GDPR DPAs but no EU-residency lead | Transcript / editor |

**Read:** EU-hosted transcription (Amberscript, Happy Scribe) prices **at or below** US peers and
uses GDPR as **trust/qualification, not a price lever.** And the EU-residency claims **leak** on
inspection (Amberscript's own policy). mdreel's edge here is *both* (a) fully-EU processing with
**no human-in-the-loop and no non-EEA transfer** — a genuinely cleaner residency story than the
EU-hosted incumbents — and (b) an output that isn't a transcript at all.

---

## 4. Ring 3 — INFRASTRUCTURE / DIY (what our ICP's own devs would use)

All checked 2026-07-15. **This ring is the A2 risk made concrete** (BUSINESS_MODEL §8 — "Gemini
makes DIY trivial"): our ICP #1/#3 are dev teams who *can* wire these up.

| Tool | Price | EU residency | Note |
|---|---|---|---|
| **Gemini / Vertex direct** | Stage B ≈ **€0.28–0.45/video-hour** (our N4/N4b) | ✅ EU regions (we use them) | **The true DIY floor.** Everything mdreel adds is pipeline, schema, compliance packaging (the "boring hard parts"). |
| **AssemblyAI** | **$0.21/h** (Universal-3.5 Pro) / $0.15/h (Universal-2) pre-recorded; **$50 free** | ✅ **"EU region is the same price as US"** | Audio only. **EU residency at NO premium.** |
| **Deepgram** | Nova-3 pre-recorded **$0.0077/min ≈ $0.46/h**; **$200 free** | ✅ **EU endpoint `api.eu.deepgram.com`** | Audio only. On-prem via sales. **EU residency at NO premium.** |
| **Azure AI Video Indexer** | combined audio+video **≈ €3.11–10.26/video-h** (own dossier; USD→EUR 0.90) | ✅ EU regions, **no premium** | Cloud video AI. **Verbatim OCR in all video presets** (Azure AI Vision, 50+ langs, bounding boxes). But output is **proprietary JSON insights — no Markdown**, OCR and transcript are separate arrays with no spoken-vs-shown layer. Azure DPA. No MCP. |
| **Google Video Intelligence** | usage-priced (label/OCR/transcript primitives; from ~$0) | ✅ region-selectable (US parent) | OCR as **raw text fragments**, not a finished document. |

**Read:** the DIY layer is **cheap and already EU-capable at no premium.** A dev team can transcribe
in-EU for pennies. What they *cannot* trivially buy is the assembled pipeline: boundary-preserving
chunking of long video, the runaway-generation guards, a **consistent schema across hundreds of
files**, and the compliance packaging. **That assembly — not the model access — is the product**
(sell the boring hard parts, DISTRIBUTION.md §3). The competitive fact that EU access is free here
is a double-edged sword: it removes "EU hosting" as a thing anyone will pay a premium *for*, and
throws all the weight onto the **output artifact** and the **saved engineering**.

---

## 5. Ring 4 — SUBSTITUTES: the "do nothing" incumbents (take them seriously)

All checked 2026-07-15. **For the ICP, the realistic competitor is often not another
video-to-Markdown tool — it's the meeting recap Microsoft already bundles.**

| Substitute | Price | What it does | Why it wins by default |
|---|---|---|---|
| **Microsoft 365 Copilot / Teams Premium** | Copilot add-on **from $18/user/mo yearly ($25.20 monthly)**; bundles $23.50–32/user/mo; Teams Premium **~$10/user/mo** (*third-party est.*) | Meeting **intelligent recap**, transcript, summary, action items | Already in the ICP's Microsoft tenant; a DPO already cleared it; zero new procurement. |
| **Zoom AI Companion** | bundled **free** with paid Zoom (Pro ~$13.33/user/mo annual); no separate meter | Meeting summary, recap, action items, notes | Bundled; no new vendor. **Audio-only, no OCR, no Markdown.** EU residency **Enterprise-only on request**; US-incorporated (CLOUD Act / FISA §702 exposure). |
| **Google Meet / Gemini** | bundled from **$7/user/mo** (Workspace); "take notes for me" included Business Standard+ | Meet notes, "take notes for me" (Google Docs output) | Bundled in Workspace. **No OCR** (alpha "screenshots" only). ⚠️ **EU *storage* from Business Standard, but EU *AI-processing* control requires Enterprise Plus + Assured Controls** — a material A1 gap for a DPO on a standard plan. |

**Read — this is Threat #1 (see §9).** The bundled suites own the **meeting-notes** job completely
and at near-zero marginal cost to the buyer. **mdreel must not fight there.** Its defensible ground
is the video these tools *don't* touch: **product demos, training libraries, onboarding, recorded
walkthroughs, conference talks** — asset video, not live meetings — where the deliverable is a
**citable knowledge-base document with verbatim on-screen text**, consumed by an *agent* via API,
not a recap email a human skims. The moment the landing page says "meeting notes," it has picked a
fight with a $18/seat incumbent already inside the account and lost.

---

## 6. (a) Where the market prices entry vs per-hour — and the EU-premium question

**Entry point:** the market's first paid step is **low and impulse-shaped** — $0 free tiers
everywhere, then $8.50 (Happy Scribe), $10 (Fireflies), **$15 (Cloudglue Mini)**, $19 (Amberscript).
Even the direct anchor lets a developer seriously test for **$15**.

**Per-hour:** stratified by *what the output is*, not by geography — Infra €0.28–0.46 · EU
transcription €0.59–4.25 · **structured video $4.80–7.50 (Cloudglue)**.

**Is anyone charging an EU/GDPR premium? — No. This is the single most important pricing finding.**
- Infra (AssemblyAI, Deepgram) offers **EU residency at the same price as US.**
- EU-hosted SaaS (Happy Scribe, Amberscript) prices **at or below** US peers and uses GDPR as
  **trust/qualification, not a price lever** — and their residency claims leak under inspection.
- The only player *marketing* an EU-residency tier at all is **ElevenLabs** (audio, adjacent
  market) — and even there it's an enterprise gate, not a per-unit premium.

⇒ **The market treats EU residency as a hygiene checkbox available for free, not a feature you can
mark up.** For mdreel this is a **direct caution on A1** (METRICS.md): do **not** price a premium
for "EU" as a standalone. The premium can only be earned by the **combination nobody sells** —
EU-resident **+** structured timestamped Markdown **+** verbatim on-screen text **+** no lock-in —
and it must be priced as a *structured-video* product (Cloudglue's shelf), not an *EU-transcription*
product (where the buyer anchors to €2–4/h and balks). **The per-hour price is defensible; the word
"EU" is not the thing defending it.**

## 7. (b) 🎯 Is verbatim on-screen text capture an open wedge? — **No. Correct the hypothesis.**

This was framed as *the* wedge. **The evidence says it is not a clean open wedge — the direct
competitors already do it:**
- **Cloudglue** documents **"Scene Text Recognition — extract text visible on screen (captions,
  presentations, etc.)"** as a first-class, configurable output modality (verified verbatim, docs).
- **Twelve Labs** does OCR ("text-in-visual", 89.2% TextCaps) inside its visual model, surfaced as a
  search modality (verified; the old dedicated OCR endpoint is deprecated but the capability persists).
- **Azure AI Video Indexer** ships **verbatim OCR in every video preset** (Azure AI Vision, 50+
  languages, bounding boxes) — **EU-hosted, at no premium** (own dossier). **Flowstate** lists OCR
  in its architecture; **Google Video Intelligence** returns on-screen text as raw fragments. So the
  OCR capability is table-stakes across the direct + infra rings, including an EU-hosted option.
- **Mixpeek** meters "video OCR +$0.10/min" as a first-class capability. **VideoDB / Coactive**
  capture on-screen content only interpretively (VLM/scene), not as verbatim OCR.
- Meeting tools (Otter, tl;dv) do **slide capture**, but as *images / visual context*, **not**
  structured verbatim OCR text.

**So do not market "we read on-screen text" as the differentiator — a technical buyer answers "so
does Cloudglue."** What is genuinely unoccupied is the **combination and the artifact**:

> **The open wedge is not the OCR capability — it's the deliverable.** Nobody ships a **single,
> portable, timestamped Markdown document** that (a) cleanly **separates "spoken" from "shown on
> screen"** so a RAG can weight them, (b) is **schema-consistent across hundreds of files**, (c)
> carries **no proprietary retrieval stack / zero lock-in**, and (d) is **processed entirely in the
> EU with no human-in-the-loop and no non-EEA transfer.** Cloudglue and Twelve Labs capture the same
> raw signals but **trap them inside a collections/search platform**; the transcription tools produce
> a transcript; the meeting tools produce a recap. **The portable-document-for-agents shape is the
> white space.** Sell the artifact, not the OCR.

## 8. (c) What Cloudglue-style products DON'T do that DPO-sensitive EU buyers need

Verified against Cloudglue's + Twelve Labs' own pages (2026-07-15):

| DPO need | Cloudglue | Twelve Labs | mdreel's opening |
|---|---|---|---|
| **Data residency (EU region)** | ❌ none stated | ❌ EEA→US under SCCs | ✅ EU regions only |
| **Explicit DPA offering** | ❌ generic policy only | ⚠️ DPA exists, US-transfer | ✅ EU-focused DPA (roadmap) |
| **Contractual no-training term** | ❌ none | ⚠️ DPA baseline | ✅ contractual, no training ever |
| **Short/transparent subprocessor list** | ⚠️ PostHog/Stripe/Google, no processing region | ⚠️ GCP/AWS, no region | ✅ short, EU, published |
| **Source-video retention / erasure** | ⚠️ "as long as necessary", vague | ⚠️ index-based | ✅ **source deleted after processing by default**, `DELETE` endpoint |
| **US analytics off the property** | ❌ PostHog | — | ✅ Umami self-hosted EU (CLAUDE.md r10) |

**This is a real, verifiable gap** — and it survives the §6 caveat: mdreel isn't charging a premium
*for EU*; it's clearing a **compliance bar the direct competitors fail**, so that a DPO can approve
the purchase at all. **EU residency is not the reason to pay more — it's the reason a DPO says yes
instead of no.** That reframing (residency as *deal-unblocker*, not *price-justifier*) is the honest
one and matches the market evidence.

## 9. (d) How the direct competitors get distribution

- **Cloudglue:** developer PLG — **YC** credibility, free **Playground** (zero-code demo, no abuse
  surface because it's account-gated), **excellent docs + llms.txt + official MCP server** (so an AI
  assistant recommends them), **Discord** community, **blog + newsletter**, fast changelog. **No
  sales motion, no ads visible.** Their funnel is: land on docs → try Playground → $15 impulse buy →
  grow into credit packs.
- **Twelve Labs:** research-brand + **funding PR** ($100M Series B), Playground, developer docs,
  academic/benchmark content. Top-of-funnel is press and research reputation.
- **Mixpeek:** comparison-SEO ("Mixpeek vs Twelve Labs"), developer docs.

**Lesson for A5 / DISTRIBUTION.md:** the direct field wins developers with **docs, a free zero-code
demo, and MCP** — exactly the mdreel plan (gallery-as-demo, llms.txt, MCP server, technical blog).
Two divergences worth noting: (1) **Cloudglue's demo is a live Playground; mdreel's is a pre-rendered
gallery** (deliberate — no compute/abuse surface, METRICS.md N10). The gallery must therefore work
*harder* as proof, since a skeptic can't paste their own URL. (2) **Cloudglue's on-ramp is a $15
impulse buy; mdreel's is a 1 h free credit → €149** (see §10 — the on-ramp canyon).

---

## 10. (e) Recommended pricing — three two-plan structures, with survival math

**Constraints (from the brief + BUSINESS_MODEL §6):** two plans only — a **small hard-cap** plan +
a **larger metered-overage** plan — **no free tier**, plus the **N33 one-time 1 h trial credit.**
All margins use **N6 = €0.65/video-hour** and cover **N2 = €300/mo**; contribution = N0-style.

### The competitive envelope this must respect
- Per-hour must read as **structured-video** ($4.80–7.50/h ≈ €4.3–6.8/h), **not** transcription
  (€2–4/h). Our hypotheses (€4.60–5.96/h) already sit in-band. ✅
- **No EU premium is bankable** (§6) — the price is justified by the *output*, not the region.
- **The on-ramp is our weakest point vs the field:** every competitor's first paid step is
  $8.50–19; ours is **€149**. Our only sub-€149 on-ramp is the free 1 h credit. **There is a canyon
  between "1 h free" and "€149/mo" that Cloudglue fills with $15 and $45 stepping-stones.**

### Option A — **Hold the line** (= current BUSINESS_MODEL §6) ⭐ recommended launch default
| Plan | Price | Included | Effective | COGS | **Contribution** | Margin |
|---|---|---|---|---|---|---|
| Pro *(hard cap)* | €149/mo | 25 h | €5.96/h | €16.25 | **€132.75** | 89% |
| Business *(overage €6/h)* | €690/mo | 150 h | €4.60/h | €97.50 | **€592.50** | 86% |

- **Accounts to cover N2:** **~3 Pro** (300 ÷ 132.75) **or ~1 Business** → **N1a = 2–3.** ✅
- **CAC ceiling (3 × contribution):** **€398 on Pro**, €1,777 on Business.
- **Why default:** per-hour validated in-band; **highest N0 → highest CAC ceiling**, which matters
  because paid search is the A1/N12 *instrument* (METRICS.md §1.6–1.7) — a higher ceiling buys more
  keyword room. Fewest accounts to survive. **Risk:** the €149 on-ramp canyon may throttle
  trial→paid (A2).

### Option B — **Lower the on-ramp** (fallback if the canyon bites)
| Plan | Price | Included | Effective | COGS | **Contribution** | Margin |
|---|---|---|---|---|---|---|
| Starter *(hard cap)* | €99/mo | 15 h | €6.60/h | €9.75 | **€89.25** | 90% |
| Business *(overage €5.50/h)* | €499/mo | 100 h | €4.99/h | €65.00 | **€434** | 87% |

- **Accounts to cover N2:** ~4 Starter (300 ÷ 89.25) or ~1 Business → **N1a = 1–4.**
- **CAC ceiling:** **€268 on Starter**, €1,302 on Business.
- **Why:** €99 is a familiar SaaS entry a team lead can expense; 15 h suits one L&D/dev team's
  monthly cadence; still 90% margin; halves the trial→paid canyon. **Cost:** lower N0 → **lower CAC
  ceiling** (€268 vs €398) → less paid-search room; more accounts to survive; more support surface.

### Option C — **Not recommended: €59 entry.** Modeled for completeness
€59/mo hard-cap (8 h) contributes only €53.80 → **CAC ceiling collapses to €161** and survival needs
~6 accounts. For a **DPO-approved B2B** product this is the wrong direction: it invites bill-shopping
against €2/h transcription, multiplies support load, and starves the paid-search instrument. **A
DPO-gated purchase is not a $15 impulse; do not price it like one.**

### 🔀 The A3 fork — and the market backs it
**If cohort hour-decay says backfill (METRICS.md A3), all of the above become prepaid credit packs**
(*"€200 for 20 h" = €10/h*). **The competitive evidence de-risks this pivot:** Cloudglue's entire
ladder **already is** prepaid credit packs (Mini/Starter/Builder/Scale — *"subscriptions? contact
us"*), and Amberscript/Happy Scribe both sell top-up credits. **A credit pack is the market-normal
shape for this product** — so the A3=backfill branch is not a downgrade, it's converging on what the
category already does. Keep the packs pre-designed.

### Recommendation
**Ship Option A at launch; pre-build Option B's €99 Starter as a one-switch fallback.** Trigger the
switch if T-LAUNCH shows the canyon biting: `checkout_clicked` < 5% of trials (A2 floor) **or** N21
"too expensive" dominant. Hold the €690 Business either way; its 86% margin and huge CAC ceiling make
it the account that actually reaches N1a fastest.

---

## 🧭 One-page decision summary — for the founder

**Recommended pricing (launch):** **Option A** — Pro **€149/mo / 25 h hard-cap** (€5.96/h) + Business
**€690/mo / 150 h, €6/h overage** (€4.60/h) + **1 h trial credit** (N33). **Survival:** 2–3 accounts
cover N2 (✅ N1a). **CAC ceiling €398** (Pro) / €1,777 (Business), inside N23. **Pre-build the €99
Starter fallback** and flip to it only if T-LAUNCH shows the trial→€149 canyon throttling checkout.
Keep prepaid packs ("€200/20 h") designed but dark — deploy only if A3 = backfill (the category's
normal shape anyway).

**Positioning line vs each ring:**
- **vs Cloudglue/Twelve Labs (direct):** *"The same structured video understanding — but as a
  portable Markdown file your agent owns, processed entirely in the EU, with no retrieval stack to
  lock you in."* (We match their capability; we beat their **lock-in** and their **DPO story**.)
- **vs EU transcription (Amberscript/Happy Scribe):** *"Not a transcript — a knowledge-base document
  that also captures what's on screen, and a residency story that doesn't leak to a US LLM or an
  offshore transcriber."* (Never compete on €/hour here.)
- **vs infra/DIY (Gemini/Deepgram/AssemblyAI):** *"You could wire this yourself; here's the pipeline
  you'd rebuild badly — boundary-safe chunking, runaway-generation guards, one consistent schema
  across every file, compliance packaged."* (Sell the boring hard parts.)
- **vs bundled substitutes (Copilot/Zoom/Meet):** *"Those summarize your meetings. This turns your
  demos, trainings and walkthroughs — the asset video they never touch — into something your AI
  assistant can cite."* (Change the job; never say "meeting notes.")

**The three biggest threats found:**
1. **🥇 The bundled substitute owns the meeting-notes job.** Teams/Copilot ($18–32/seat, already
   bought, already DPO-approved) is the true default. If mdreel's framing drifts toward "meetings,"
   it loses to an incumbent inside the account. **Mitigation:** anchor on **asset video** (demos,
   trainings, talks) and the **agent-citable document**, not meetings — enforce it in the copy.
2. **🥈 On-screen text is not a unique wedge, and EU residency isn't a priced premium.** Cloudglue's
   Scene Text Recognition and Twelve Labs' OCR already read on-screen text; AssemblyAI/Deepgram give
   EU residency free. Marketing either as *the* differentiator invites "so does X." **Mitigation:**
   sell the **combination + the portable artifact** (spoken/shown separated, schema-consistent, no
   lock-in, clean EU processing) and treat residency as a **deal-unblocker for the DPO, not a
   markup.**
3. **🥉 Cloudglue is one `europe-west3` deploy from erasing the residency edge — and out-executes on
   distribution.** YC-backed, fast changelog, MCP + Playground + $15 on-ramp vs our 1 h-credit→€149
   canyon. If they add an EU region, differentiator #1 in the direct ring thins to "no lock-in"
   alone. **Mitigation:** lead with the **no-lock-in portable-file model they structurally won't copy**
   (it undermines their platform), ship the EU-owned-infra roadmap (BUSINESS_MODEL §4), and close the
   on-ramp gap (the €99 fallback).

**And the finding that should change a slide today:** *the wedge is the deliverable, not the OCR.*
Everything the demo shows should be the **one portable Markdown file, spoken-vs-shown separated, that
an agent cites** — not "look, we can read a slide."

---

*Sources are inline with the date checked (2026-07-15). Primary extracts: `primary-source-notes.md`.
Verified-claim log (adversarially checked, 3 killed): `workflow-verified-claims.txt`. Anchor
screenshots: `cloudglue-pricing-2026-07-15.png`, `twelvelabs-pricing-2026-07-15.png`. Third-party /
estimated figures are flagged inline; all others are read from the vendor's own live page.*
