# Flowstate — ring: direct · verified: 2026-07-15 · confidence: med

> **Source note:** flowstatehq.com resolves (via 308 redirect) to www.flowstatehq.com and returns a live, clearly video-intelligence product site — this is **not** the music app, creator-economy Flowstate, or any unrelated namesake. All sections below profile FlowState AI, Inc. The LinkedIn handle `/company/flowstate-ai` is confirmed as theirs. The handle `/company/flowstatehq` on LinkedIn is a *different*, unrelated creator-economy product and was excluded. Funding, headcount, and founding date are not publicly disclosed; those fields are marked unknown with derivation where possible.

---

## 1. Vitals

| Field | Value | Grade |
|---|---|---|
| Legal name | FlowState AI, Inc. | Q |
| HQ / jurisdiction | 2261 Market Street, Suite 5510, San Francisco, CA 94114 — California, US (virtual-office address; no physical HQ disclosed) | Q |
| Founded | Unknown — earliest dated content on site is Feb 13 2026 (research paper); Terms of Service last updated Feb 12 2026; oldest case study dated Mar 6 2026 — inference: incorporated late 2024 or very early 2025, publicly active from early 2026 | E |
| Funding | Unknown — no Crunchbase profile found, no press releases or investor disclosures found; Crunchbase returned 403. Virtual-office address typical of pre-seed / seed-stage US startups | E |
| Headcount | Unknown — no team page, no LinkedIn employee count accessible. Inference: <10 FTE based on single named author (Sahil Shah, Founder & CEO) across all published content and stage indicators | E |
| Status | Active and commercially live — demo-led enterprise sales, multiple published case studies with named customers, SOC 2 Type II certification obtained | Q |
| CEO background | Sahil Shah — "nearly a decade at Waymo and Apple building large-scale video AI and computer vision systems … over 15 years of experience bringing frontier video technologies from research into production environments" | Q |

**Why:** All public documentation (7+ pages fetched from flowstatehq.com, ToS, privacy policy, LinkedIn) points to a single-founder-led pre-seed/seed-stage US startup with deep ML pedigree (Waymo + Apple CV). No co-founders named. No investor names disclosed. No headcount range published. Stage is inferred from virtual-office registration, demo-only commercial motion, and absence of any funding announcement.

---

## 2. Business model

**Model type:** Enterprise SaaS / usage-based, fully sales-led. No self-serve signup, no free tier, no public pricing page (confirmed 404 on /pricing).

**Pricing — public table:**

| Tier | Price | Notes |
|---|---|---|
| Entry / free trial | None disclosed | Demo/pilot process only — "Teams typically start with a guided demo or pilot to validate use cases, performance, and integration before moving to production" |
| Standard | Not published | — |
| Enterprise | Not published | — |
| Effective €/video-hour | **Unknown** — no public pricing; no unit economics disclosed | — |

**What Flowstate says about pricing (verbatim):** *"Flowstate pricing is usage-based and tailored to enterprise needs, factoring in video volume, processing type (archive or live), and deployment requirements."* [Q]

**Entry price:** Unknown. Sales-led pilot process. [Q]

**Free/trial shape:** No free tier. Pilot engagement with guided demo required. [Q]

**Enterprise motion:** Demo → pilot → production. Emphasis on custom integration, APIs/SDKs, human-in-the-loop review interfaces, and flexible deployment options. [Q]

**What they meter that mdreel doesn't:** Volume of video processed (archive vs live stream), live-analysis compute (real-time stream ingestion priced separately from batch archive), and likely per-schema-update reprocessing jobs across large archives. [E — inferred from FAQ wording]

---

## 3. Product & features

| Feature | Status | Note | Grade |
|---|---|---|---|
| **Transcript** | [x] | Audio/speech transcribed at segment level; multi-language supported (Attention Economics case study covers Eastern European language + Latin/Cyrillic on-screen text) | Q |
| **Verbatim ON-SCREEN text (OCR)** | [x] | Explicitly listed in LinkedIn architectural post: "actions, scenes, dialogue, **OCR**, objects, people, brands, timestamps, confidence, and provenance"; confirmed in editorial-bias case study (on-screen graphics in Cyrillic + Latin extracted) | Q |
| **Visual descriptions** | [x] | Frame-level visual analysis — objects, scenes, motion, brand logos, sponsor logos tracked frame-by-frame (motorsports case study: 60 TB archive, logo detection at frame level) | Q |
| **Timestamps** | [x] | Every output is time-coded; timestamp references returned with NL Q&A results; "timestamp references so analysts could quickly verify findings" | Q |
| **Structured / Markdown output** | [~] | Output is structured JSON-like queryable metadata (not Markdown). LinkedIn post explicitly frames output as "portable … human-readable … agent-readable" deep-caption index — but no evidence of Markdown file export; output is platform-resident or API-served | Q/E |
| **JSON / API** | [x] | "Flowstate integrates via APIs and SDKs … export break markers as time-coded objects that map into playout tooling" | Q |
| **Webhooks** | [?] | Not mentioned on site; plausible for event-driven live-analysis use cases, but unconfirmed | — |
| **MCP server** | [ ] | No mention; no evidence | — |
| **Connectors** | [x] | Integrates with DAM/MAM/CMS systems, existing cloud storage, enterprise tools; "connect to existing storage, CMS, DAM/MAM systems … without data duplication" | Q |
| **Speaker ID** | [x] | Named political actors identified (Attention Economics); driver/person tagging by reference image in motorsports case study; "speakers" named in structured-extraction feature description | Q |
| **Languages** | [~] | Multi-language confirmed (Eastern European language + mixed script case study); no list of supported languages published | Q |
| **Max video length** | [?] | Not stated. Processes "10+ hours of daily programming" in live mode; 60 TB archives in batch. Practical limit not disclosed | Q/E |
| **Processing-speed claims** | [x] | "Reduced processing time by 90%" (iFit); "processing time from months to days" for large archive batch; real-time live stream ingestion | Q |
| **Retention / erasure controls** | [~] | "No model training on your data"; retention referenced in privacy policy (data kept as long as necessary); no explicit video-file deletion SLA or customer-controlled erasure UI mentioned | Q |
| **Self-host option** | [~] | "Flexible deployment options — Deployments can run on approved cloud infrastructure or customer-controlled environments" — implies private-cloud or on-prem deployment is available (enterprise sales conversation) | Q |

**What the output actually looks like:** Flowstate produces a time-aligned, structured semantic index ("deep-caption index" per LinkedIn post) composed of segment-level records containing speech transcripts, OCR'd on-screen text, visual descriptions, named entities, sentiment labels, confidence scores, and frame-accurate timestamps — all queryable via natural language in a web UI or via API/SDK. The output format is a platform-resident knowledge graph, not an export file; results are surfaced as search hits with timestamp + thumbnail, or as structured JSON delivered over API. There is no evidence of a Markdown or plain-text file export mode.

---

## 4. Size & customer base

| Signal | Detail | Grade |
|---|---|---|
| **Named logos** | iFit (connected fitness, US); South Park Commons (VC/founder community, SF); Attention Economics (media advisory firm, Eastern European market); unnamed North American motorsports team (30+ sponsors, 4M social followers) | Q |
| **Reviews** | None found on G2, Capterra, or Trustpilot as of 2026-07-15 | T |
| **Web traffic** | Unknown — no SimilarWeb or public traffic data found | — |
| **Community** | No Discord, Slack, or user community found. LinkedIn company page present; post cadence unknown | E |
| **Hiring** | No jobs page found on site (404 on /company); no active job postings found on LinkedIn or Wellfound (403 on Wellfound) | E |
| **Review sites** | Absent from major B2B review aggregators | E |

**Inference:** Customer base is very small — four case-study customers named, all in media/sports/content verticals. No enterprise-tier logos in tech, IT services, or L&D. Likely <20 total customers. [E]

---

## 5. GTM & distribution

**Channels:**
- Demo-led enterprise sales (primary): "Book a Demo" CTA on every page; pilot → production motion [Q]
- Content marketing: case studies (4 named), use-case pages (5), blog (1 post), research (1 paper) — early content build-out, all dated Feb–Jul 2026 [Q]
- Try-before-buy demo: public demo at try.flowstatehq.com — 35 pre-indexed short advertising/trailer clips demonstrating semantic search UI [Q]
- No product-led growth; no self-serve signup; no App Store or marketplace listing found [E]

**Positioning sentence (verbatim from site):** *"Flowstate transforms hours of unstructured footage into searchable, actionable, intelligent content."* [Q]

**Homepage meta-description (verbatim):** *"Flowstate is an AI video intelligence platform that transforms unstructured video into searchable, structured data — enabling multimodal search, automated metadata extraction, and real-time video analysis at scale."* [Q]

**Who the site talks to:** Enterprise media, sports, and broadcast teams dealing with large video archives. Named verticals: FAST channel programmers, sports organizations with multi-decade archives and sponsor tracking obligations, media advisory firms doing compliance/governance work, digital-first brands repurposing long-form content. **Not** L&D, IT, software engineering, or enterprise knowledge-management buyers. No EU-specific messaging. [Q]

---

## 6. EU / GDPR posture

| Signal | Detail | Grade |
|---|---|---|
| **Hosting regions** | Not disclosed — privacy policy states data "processed at the Company's operating offices and in any other places where the parties involved in the processing are located"; no EU region specified | Q |
| **DPA (Data Processing Agreement)** | Not mentioned on site; no DPA template published | E |
| **Subprocessors list** | Not published; privacy policy references retention.com and RB2B as third-party tracking/advertising partners | Q |
| **No-training pledge** | Explicit: *"Your videos, metadata, and outputs are never used to train AI models by Flowstate or third-party providers"* | Q |
| **Certifications** | SOC 2 Type II (explicitly stated on homepage: *"Flowstate is SOC 2 Type II certified, validating that our security controls are independently audited"*); no ISO 27001, no EU AI Act compliance claimed | Q |
| **EU data residency** | Unknown — no mention of EU-region processing, no EU server location advertised; California law governs per ToS | Q |
| **Residency premium** | Unknown — "flexible deployment options … customer-controlled environments" implies this may be negotiable at enterprise tier | E |
| **GDPR posture** | Privacy policy references GDPR opt-out (links to rb2b.com/rb2b-gdpr-opt-out) but this applies to marketing cookies only, not video data processing; no GDPR-specific DPA, adequacy transfer mechanism, or EU-resident-specific controls visible | Q |
| **AI Act** | No mention; not positioned for EU AI Act compliance | E |

**Summary:** Flowstate has a strong no-training pledge and SOC 2 Type II, but zero visible EU data-residency guarantees, no published DPA, and California/US legal jurisdiction. For any EU customer with a DPO, this would require a bespoke enterprise negotiation — not table-stakes-ready.

---

## 7. Threat assessment

**ICP overlap with mdreel:**
Partial and indirect. Flowstate targets large-volume media, sports, and broadcast enterprises with multi-decade archives; mdreel targets EU software/IT/L&D teams of 50–500 with a DPO. The overlap zone is narrow: both products extract structured, timestamped, AI-readable intelligence from video including OCR. If a mid-market EU tech company wanted to turn internal video (all-hands, demos, training recordings) into searchable knowledge, Flowstate's demo-led enterprise motion, US jurisdiction, and media/sports ICP framing would make it a poor fit — but the underlying capability is comparable.

**What Flowstate would need to kill mdreel:**
1. EU data residency + published DPA + GDPR-ready subprocessor controls (currently absent)
2. Pivot ICP messaging toward IT/L&D/knowledge-management buyers (currently all messaging is media/sports)
3. Portable, file-export output (Markdown or equivalent) for AI knowledge-base ingestion workflows (currently platform-resident queryable index only)
4. Scalable, transparent SMB pricing that removes the demo-and-pilot barrier (currently enterprise-only motion)

**Likelihood of this happening in 12–18 months:** Low-to-medium. Sahil Shah's Waymo/Apple CV pedigree means the technology can scale in any direction, but GTM pivots take time and their content/brand investment is clearly media/sports-vertical. EU compliance infrastructure is a non-trivial legal and operational investment for a sub-10-person US startup.

**What mdreel structurally does that Flowstate cannot currently:**
- EU processing with explicit data-residency guarantees — day-one DPO-ready
- Portable, lock-in-free Markdown output (files you own and export, not platform-resident indexes)
- Explicit spoken-vs-shown text separation (transcript vs. verbatim on-screen Markdown blocks) — mdreel's core differentiator for software/IT content where slides and code on screen ≠ what was spoken
- SMB-accessible transparent pricing; no forced sales demo to start
- Framing entirely around internal video / knowledge base / L&D — not media archive monetization

**What mdreel should steal from Flowstate:**
- The "deep captions first, embeddings second" architectural narrative — Flowstate articulates this compellingly on LinkedIn and it directly validates mdreel's portable-Markdown approach; mdreel should publish a comparable explainer
- Frame-accurate sponsor/logo/visual-element detection as a potential premium feature for enterprise L&D (track which software tools appear on screen in demos)
- Human-in-the-loop review interface — Flowstate's quality-assurance layer for AI-generated metadata is a trust-building mechanism mdreel could adopt for verbatim OCR accuracy
- Schema-driven structured extraction terminology — mdreel could borrow this framing to explain how its Markdown headings/sections map to re-usable structure

---

## 8. Evidence log

| # | Claim | Source URL | Checked | Grade |
|---|---|---|---|---|
| 1 | Site resolves and is video-intelligence product | https://www.flowstatehq.com/ | 2026-07-15 | Q |
| 2 | Legal entity name FlowState AI, Inc. | https://www.flowstatehq.com/legal/privacy-policy | 2026-07-15 | Q |
| 3 | HQ address: 2261 Market Street, Suite 5510, San Francisco, CA 94114 | https://www.flowstatehq.com/legal/privacy-policy | 2026-07-15 | Q |
| 4 | Governing law: California, United States | https://www.flowstatehq.com/legal/terms-of-service | 2026-07-15 | Q |
| 5 | CEO: Sahil Shah; background at Waymo and Apple; 15+ years video AI | https://www.flowstatehq.com/case-studies/ifit | 2026-07-15 | Q |
| 6 | No pricing page (404) | https://www.flowstatehq.com/pricing | 2026-07-15 | Q |
| 7 | FAQ: "Flowstate pricing is usage-based and tailored to enterprise needs…" | https://www.flowstatehq.com/ | 2026-07-15 | Q |
| 8 | FAQ: "Teams typically start with a guided demo or pilot…" | https://www.flowstatehq.com/ | 2026-07-15 | Q |
| 9 | Schema-driven, frame-level extraction of objects, scenes, speakers, topics | https://www.flowstatehq.com/ | 2026-07-15 | Q |
| 10 | Live Watcher for real-time stream ingestion | https://www.flowstatehq.com/case-studies/attention-economics-editorial-bias-ai | 2026-07-15 | Q |
| 11 | NL Q&A over video archives with timestamp references | https://www.flowstatehq.com/case-studies/attention-economics-editorial-bias-ai | 2026-07-15 | Q |
| 12 | OCR explicitly included in index ("dialogue, OCR, objects, people, brands") | https://www.linkedin.com/company/flowstate-ai | 2026-07-15 | Q |
| 13 | "Deep-caption index … portable … human-readable … agent-readable" | https://www.linkedin.com/company/flowstate-ai | 2026-07-15 | Q |
| 14 | Output is queryable platform / API, not Markdown export | https://www.flowstatehq.com/ + case studies | 2026-07-15 | E |
| 15 | APIs and SDKs; integrates with DAM/MAM/CMS | https://www.flowstatehq.com/ | 2026-07-15 | Q |
| 16 | iFit customer: 95%+ accuracy, 90% processing time reduction, 5,000+ videos | https://www.flowstatehq.com/case-studies/ifit | 2026-07-15 | Q |
| 17 | Attention Economics: 10+ hours daily news; 70-80% manual effort reduction | https://www.flowstatehq.com/case-studies/attention-economics-editorial-bias-ai | 2026-07-15 | Q |
| 18 | South Park Commons: 6+ hours/week saved; short-form content scaling | https://www.flowstatehq.com/case-studies/south-park-commons | 2026-07-15 | Q |
| 19 | Motorsports: 60 TB archive; editorial time cut 95%; sponsor frame-level tracking | https://www.flowstatehq.com/case-studies/motorsports-editorial | 2026-07-15 | Q |
| 20 | SOC 2 Type II certified (explicitly stated on homepage) | https://www.flowstatehq.com/ | 2026-07-15 | Q |
| 21 | "No model training on your data" (verbatim) | https://www.flowstatehq.com/ | 2026-07-15 | Q |
| 22 | Flexible deployment: "approved cloud infrastructure or customer-controlled environments" | https://www.flowstatehq.com/ | 2026-07-15 | Q |
| 23 | Enterprise SSO/SAML and admin access controls | https://www.flowstatehq.com/ | 2026-07-15 | Q |
| 24 | No EU data residency, no DPA template, California law governs | https://www.flowstatehq.com/legal/terms-of-service | 2026-07-15 | Q |
| 25 | Third-party trackers: retention.com and RB2B (marketing/retargeting) | https://www.flowstatehq.com/legal/privacy-policy | 2026-07-15 | Q |
| 26 | GDPR opt-out link (rb2b.com) in privacy policy (marketing only, not video data) | https://www.flowstatehq.com/legal/privacy-policy | 2026-07-15 | Q |
| 27 | Demo search UI at try.flowstatehq.com; 35 indexed videos (ads/trailers) | https://try.flowstatehq.com/ | 2026-07-15 | Q |
| 28 | Terms last updated Feb 12 2026; earliest blog Feb 17 2026 | https://www.flowstatehq.com/legal/terms-of-service; /blog | 2026-07-15 | Q |
| 29 | No company about page, no team page (404) | https://www.flowstatehq.com/company | 2026-07-15 | Q |
| 30 | Crunchbase profile not found (403/no profile) | https://www.crunchbase.com/organization/flowstate-hq | 2026-07-15 | T |
| 31 | ICP: media, sports, security, enterprise knowledge (FAQ verbatim) | https://www.flowstatehq.com/ | 2026-07-15 | Q |
| 32 | Use cases: FAST channel programming, compliance tagging, archive monetization, short-form content, live sports highlights | https://www.flowstatehq.com/ (use-cases nav) | 2026-07-15 | Q |
| 33 | Research on IP violations in video diffusion models (Wan2.1, 671 videos) | https://www.flowstatehq.com/research/ip-violations-in-video-diffusion-models | 2026-07-15 | Q |
| 34 | LinkedIn creator-economy "Flowstate" excluded as different product | https://www.linkedin.com/company/flowstatehq | 2026-07-15 | T |
| 35 | Audit trails available on request for compliance/governance | https://www.flowstatehq.com/ | 2026-07-15 | Q |

**Screenshot:** `assets/flowstate-2026-07-15.png` (full-page homepage, captured via Playwright 2026-07-15).
