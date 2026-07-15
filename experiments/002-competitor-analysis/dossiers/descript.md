# Descript — ring: adjacent · verified: 2026-07-15 · confidence: high

## 1. Vitals
- **HQ / jurisdiction:** San Francisco, California, USA (US company, US legal terms). [#12]
- **Founded:** 2017, by Andrew Mason (Groupon co-founder). [#13]
- **Funding:** ~$100M total across 4 rounds. Latest disclosed: **Series C, $50M, announced Nov 2022, led by OpenAI Startup Fund**, with a16z, Redpoint Ventures, Spark Capital, and Daniel Gross participating; reported post-money valuation **~$550M**. Prior valuation ~$260M (Jan 2021). No public raise since. [#9][#10][#11]
- **Headcount:** ~185 employees (third-party trackers, May 2026). [#14]
- **Status:** **Growing but slowing / mature-scaleup.** No new funding in ~3.5 years despite an active AI-editing market; pivoted hard into AI ("Underlord" agent), API and MCP in 2024-2025 to defend a crowded creator-tools category. Not a zombie — shipping actively (changelog current) — but no fresh capital and a fixed valuation make it a settled incumbent rather than a rocket. Grade E (inference from funding gap + shipping cadence).

## 2. Business model
**Subscription, per-seat, with metered "media hours" and a separate AI-credit meter.** This is a video/podcast **editing** product, not a structured-extraction pipeline; the €/h figures below compare only the transcription-input allowance, not equivalent output.

| Plan | Price (billed monthly) | Included | Effective €/video-h | Overage |
|---|---|---|---|---|
| Free | $0 | 60 min/mo media, 100 one-time AI credits, 1 seat, 5GB, 720p export | N/A (trial-shaped) | none — hard cap |
| Hobbyist | $16/mo (≈ €14.40) | 10 h/mo media, 400 AI credits/mo, 1 seat, 100GB, 1080p | **€1.44/h** | media-hour + AI-credit top-ups |
| Creator ("most popular") | $24/mo (≈ €21.60) | 30 h/mo media, 800 credits/mo, 1–3 seats (billed separately), 1TB, 4K | **€0.72/h** | top-ups |
| Business | $50/mo (≈ €45.00) | 40 h/mo media, 1,500 credits/mo, up to 5 seats (billed separately), 2TB, 4K, SLA, 30+ lang translation | **€1.13/h** | +media-hour / +AI-credit top-ups |
| Enterprise | Custom (sales-led) | Custom media + credits, **unlimited seats**, SSO/SCIM, custom legal terms | unknown | custom |

- **USD→EUR 0.90 applied**; prices are the vendor's monthly-billed tier. Annual billing is marketed as ~35% cheaper (exact annual per-month figures from the page were internally inconsistent — treat as uncertain). [#1]
- **Entry price:** $16/mo (Hobbyist) paid; $0 Free for the 60-min trial shape. **No 1h-style "trial credit"; instead a permanent capped Free tier.**
- **Enterprise motion:** self-serve up to Business; **sales-led** for Enterprise (SSO/SCIM/custom terms).
- **Meters mdreel doesn't:** **seats** (per-seat billing above 1–5), **AI credits** (a second consumable meter distinct from media hours), **storage tiers** (5GB→2TB), and export resolution gating. mdreel meters only video-hours. [#1]

## 3. Product & features — checklist
- [x] transcript (core — editable "doc" transcript, word-level)
- [ ] verbatim ON-SCREEN text (slides/code/UI) — **no OCR / screen-text extraction** in product or API [#3]
- [?] visual descriptions — has generative/AI video and eye-contact/scene tools, but no evidence of descriptive visual analysis of source footage for KB use [#1]
- [x] timestamps — **word-level** timecodes; exportable with markers [#3]
- [x] structured/Markdown output — transcript export in **Markdown**, plus TXT/HTML/RTF/DOCX/SRT [#3]
- [x] JSON/API — **REST API** (import, agent-edit, publish, export-transcript, job mgmt) [#2][#3]
- [x] webhooks — `callback_url` POSTs job status on completion/fail [#4]
- [x] MCP server — **official Descript MCP**, positioned for Claude/Claude Code integration [#5][#6]
- [?] connectors — Zapier/n8n via API; no native enterprise connectors (SharePoint/Drive/Confluence) documented [#6]
- [x] speaker ID — speaker labels in transcript + export [#3]
- [x] languages — **30+ languages** (translation w/ proofread on Business) [#1]
- [?] max video length — not documented in API/pricing [#3]
- [?] processing-speed claims — not surfaced on pricing/API pages
- [x] retention/erasure controls — GDPR/CCPA-aligned; transcript-sharing for model improvement is **opt-in and off by default** [#7]
- [ ] self-host option — SaaS only; **data on AWS S3 / Google Cloud**, no self-host [#7]

**Output shape:** The native artifact is an **editable transcript "document"** — words tied to the audio/video timeline, speaker-labelled, editable by editing text. Via API you export that transcript to Markdown/SRT/DOCX with optional speaker labels, timecodes and markers. It captures **spoken audio only** — there is no verbatim on-screen-text layer and no spoken-vs-shown separation, so a slide bullet or on-screen code that is never spoken does not appear in the output. [#3]

## 4. Size & customer base
- **Logos / case studies:** unknown (specific named-account case studies not surfaced in this pass). Broadly used by podcasters, YouTubers, marketing/video teams.
- **Reviews:** G2 and Capterra list Descript with large review volumes and ~4.3–4.6★ ratings historically; **exact current counts not verified in this pass — "unknown" pending a dated pull.** (Grade would be T once pulled.)
- **Web traffic:** unknown (no source pulled this pass).
- **Dev community:** third-party MCP servers exist on GitHub (e.g. `josephtandle/descript-complete`); star counts unknown. Descript ships an **official** API + MCP. [#5][#6]
- **Hiring signals:** unknown this pass.

## 5. GTM & distribution
- **Positioning (verbatim, from API page title):** "**Video Editing API | Automate Transcription, Clips & Captions**." Core brand positioning is the all-in-one AI video/audio **editor**. [#2]
- **Channels:** strong content/SEO (blog, help center, changelog), product-led free tier, template/creator gallery, YouTube-creator word-of-mouth, Zapier/n8n integration marketplaces, and a developer track (API docs + MCP). Notable blog stance: "**Don't ship your API as an MCP**" — a developer-audience thought-leadership play. [#6]
- **Who the pricing page talks to:** primarily **creators / small teams** (podcasters, video marketers) at self-serve tiers; **Business/Enterprise** shifts to team leads and IT (SLA, SSO/SCIM, custom legal). It does **not** speak to a DPO or address EU data residency. [#1]

## 6. EU/GDPR posture
- **Hosting regions:** **US-centric** — data stored on "Amazon S3 or Google Cloud"; **no EU region / EU data-residency offering stated.** This is the key gap vs mdreel. [#7]
- **DPA:** not published on the security page; a **DPO contact (dpo@descript.com)** exists, implying a DPA is available on request — not verified. [#7]
- **Subprocessors:** a list is disclosed and is **long/broad** — includes Google Cloud, AWS, Stripe, Zendesk, **Rev, OpenAI Whisper, Hedra**, Braze, Mandrill, Sparkpost, Segment, Amplitude, etc. Several are US AI/data subprocessors. [#7]
- **No-training terms:** AI features are **opt-in**; sharing transcripts to improve the service is **off by default** and user-toggleable. [#7]
- **Certifications:** **SOC 2 Type II** (CPA-attested). **No ISO 27001 stated.** GDPR/CCPA "aligned." [#7]
- **Residency premium:** **Does not charge or market EU residency** — it has none to sell. EU compliance is framed as "aligned," not as a residency guarantee. [#7]

## 7. Threat assessment
- **ICP overlap: LOW-MED.** Descript targets creators, podcasters and video-marketing teams — not EU software/IT teams building internal AI knowledge bases. But the surface narrative overlaps dangerously: it has **transcription → Markdown export, an API, an official MCP, webhooks and speaker labels**, so a buyer skimming feature lists could see it as "video → LLM-ready." The reality gap (no on-screen text, no EU residency, editor-not-pipeline) is real but not obvious at first glance.
- **What they'd need to do to kill mdreel:** (a) add **verbatim on-screen-text / slide-code OCR with spoken-vs-shown separation**, and (b) stand up **EU data residency + a marketed DPA/subprocessor posture**. Likelihood: **low.** (a) is off their creator-editing roadmap; (b) means re-architecting US-anchored infra and a long subprocessor chain (Whisper, Rev, Hedra) — expensive and off-strategy for a company with no fresh capital since 2022. A cheap partial move (better Markdown/API polish) is plausible but doesn't close the KB gap.
- **What mdreel does that they structurally can't (near-term):** EU-only processing (Vertex in europe-*) sold to DPOs; **verbatim on-screen text** (slides/code/UI) as a first-class layer; spoken-vs-shown separation; a lean EU subprocessor story; portable no-lock-in Markdown as the *product* rather than an export afterthought.
- **What mdreel should steal:** (1) the **official MCP + REST API + `callback_url` webhook** developer surface — table stakes for the ICP; (2) **Markdown/SRT/DOCX export with optional speaker labels + timecodes** as selectable flags; (3) the credit/metering UX clarity; (4) the blog-as-distribution engine (e.g. their "don't ship your API as an MCP" developer content). Avoid their per-seat + dual-meter (credits) pricing complexity — mdreel's single video-hour meter is a positioning advantage for procurement.

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | Full pricing ladder, tiers, included hours, seats, storage, overage top-ups, 30+ languages | https://www.descript.com/pricing | 2026-07-15 | Q |
| 2 | API positioning "Automate Transcription, Clips & Captions"; import/agent-edit/publish/export endpoints | https://www.descript.com/api | 2026-07-15 | Q |
| 3 | Transcript export formats (Markdown/TXT/HTML/RTF/DOCX/SRT), speaker labels, timecodes, markers; no OCR/on-screen-text | https://docs.descriptapi.com/ | 2026-07-15 | Q |
| 4 | Webhook via `callback_url` POSTs job status on completion/failure | https://help.descript.com/hc/en-us/articles/43370311322509-Descript-API | 2026-07-15 | T (Descript help via search) |
| 5 | Official Descript MCP server; third-party MCP on GitHub | https://help.descript.com/hc/en-us/articles/46056322186509-Descript-MCP-overview | 2026-07-15 | Q |
| 6 | MCP positioned for Claude Code; "Don't ship your API as an MCP" blog | https://www.descript.com/blog/article/dont-ship-your-api-as-an-mcp | 2026-07-15 | Q |
| 7 | SOC 2 Type II; AWS S3/Google Cloud hosting, no EU residency stated; opt-in AI, off-by-default training; long subprocessor list; dpo@descript.com | https://www.descript.com/security | 2026-07-15 | Q |
| 9 | Series C $50M led by OpenAI Startup Fund, Nov 2022; a16z/Redpoint/Spark/Daniel Gross | https://www.goodwinlaw.com/en/news-and-events/news/2022/11/11_30-descript-raises-50-million-series-c | 2026-07-15 | T (Goodwin / press) |
| 10 | ~$550M Series C valuation; prior ~$260M (Jan 2021); ~$100M total raised | https://techcrunch.com/2022/11/15/ai-powered-media-editing-app-descript-lands-fresh-cash-from-openai/ | 2026-07-15 | T (TechCrunch) |
| 11 | Total funding ~$101M over 4 rounds; investors incl. Redpoint, a16z, Spark; Naval Ravikant angel | https://www.crunchbase.com/organization/descript | 2026-07-15 | T (Crunchbase) |
| 12 | HQ San Francisco, USA | https://www.crunchbase.com/organization/descript | 2026-07-15 | T (Crunchbase) |
| 13 | Founded 2017 by Andrew Mason (Groupon founder) | https://techcrunch.com/2022/11/15/ai-powered-media-editing-app-descript-lands-fresh-cash-from-openai/ | 2026-07-15 | T (TechCrunch) |
| 14 | ~185 employees (May 2026) | https://tracxn.com/d/companies/descript/__vF948CDG-Kh3N00CfMczYLzLCnpIqW3JvPsaCVEZfPU | 2026-07-15 | T (Tracxn) |
| 15 | Effective €/video-h = plan price × 0.90 ÷ included hours (Hobbyist €1.44, Creator €0.72, Business €1.13) | derived from #1 | 2026-07-15 | E |

**Caveats:** Annual per-month prices from the pricing fetch were internally inconsistent and are omitted (only monthly-billed figures used). G2/Capterra review counts, web-traffic, and video-length limits were not sourced this pass and are marked "unknown" rather than guessed. DPA availability is inferred from the published DPO contact, not confirmed.