# Whisper (OpenAI) — ring: infra · verified: 2026-07-15 · confidence: high

## 1. Vitals
- **HQ/jurisdiction:** OpenAI, San Francisco, California, USA (US jurisdiction — relevant to mdreel's A1 EU-residency thesis).
- **Founded:** Whisper (the open-source model) publicly released September 2022; hosted `whisper-1` API added March 2023. OpenAI the company founded 2015.
- **Funding:** Whisper is a feature/model, not a standalone entity — no separate funding. Parent OpenAI is one of the most heavily funded private AI companies in the world (multiple multi-billion-dollar rounds led by Microsoft and others). Exact latest round not re-verified here → treat parent funding as "very large, unknown exact figure as of 2026-07-15."
- **Headcount:** N/A for Whisper specifically (it is not a team/product org). OpenAI overall headcount unknown/not sourced here.
- **Status:** **Growing / actively maintained but bifurcating.** The open-source repo (MIT) is a long-term reference implementation (105k+ GitHub stars). OpenAI is steering the *hosted* transcription business toward newer `gpt-4o-transcribe` / `gpt-4o-mini-transcribe` / `gpt-4o-transcribe-diarize` models; legacy `whisper-1` is quietly de-emphasized (no longer on the headline pricing page). So: the brand is thriving, the specific `whisper-1` endpoint is in slow legacy mode.

## 2. Business model
Whisper is **not a subscription product** — it is (a) a free MIT-licensed open-source model you self-host, and (b) a **usage-metered API** billed per minute of audio. No seats, no plans, no free tier beyond OpenAI's general trial credits.

| Plan / model | Price | Included | Effective €/video-h | Overage |
|---|---|---|---|---|
| Self-host open-source Whisper (MIT) | €0 license + your own GPU/CPU compute | Unlimited (you pay infra) | Compute-only (varies; not a per-hour price) | N/A |
| `whisper-1` API (legacy) | $0.006 / min = $0.36/h | pay-as-you-go | **≈ €0.32/h** (USD→EUR 0.90) | pure usage, no cap |
| `gpt-4o-transcribe` API | $0.006 / min = $0.36/h | pay-as-you-go | **≈ €0.32/h** | pure usage |
| `gpt-4o-mini-transcribe` API | $0.003 / min = $0.18/h | pay-as-you-go | **≈ €0.16/h** | pure usage |
| `gpt-realtime-whisper` (live) | $0.017 / min = $1.02/h | pay-as-you-go | ≈ €0.92/h | pure usage |

- **Entry price point:** effectively $0 (self-host) or ~$0.003/min hosted. **This is the floor of the entire market** and sits *below* mdreel's €0.65/h all-in COGS — but that comparison is apples-to-oranges (see §7): Whisper produces audio transcript only, mdreel's COGS includes vision/on-screen extraction and fusion.
- **Free tier/trial shape:** open-source = free forever; hosted API = general OpenAI trial credits, no Whisper-specific free tier.
- **Enterprise motion:** **self-serve developer**, API-key driven. No sales-led motion for transcription; enterprise contracts exist at the OpenAI-platform level, not Whisper-specific.
- **Meters mdreel doesn't:** per-minute audio only. No storage, no index retention, no query metering, no seats. Simpler meter than mdreel — but also a narrower deliverable.

## 3. Product & features — checklist
- [x] transcript (core function)
- [ ] verbatim ON-SCREEN text (slides/code/UI) — **no vision at all; audio-only**
- [ ] visual descriptions — none
- [x] timestamps — **segment-level and word-level, but word/segment `timestamp_granularities[]` is "only available for whisper-1"** (the gpt-4o transcribe models drop granular timestamp control)
- [~] structured/Markdown output — JSON/SRT/VTT/verbose_json/text; **no Markdown**, and no document structure (headings/sections)
- [x] JSON/API — REST API, json + verbose_json
- [ ] webhooks — none native to the transcription endpoint
- [ ] MCP server — none for Whisper specifically
- [ ] connectors — none (raw file upload only)
- [x] speaker ID — via separate `gpt-4o-transcribe-diarize` model (speaker labels + start/end + optional known-speaker reference clips 2–10s); **not** in `whisper-1`
- [x] languages — ~99 languages (multilingual + translation-to-English)
- [~] max video length — **not video; audio only.** Hosted API hard-limits uploads to **25 MB** (must chunk longer files); self-host has no hard limit but 30s sliding window internally
- [x] processing-speed claims — `turbo` open-source model ~8× faster than `large`
- [~] retention/erasure controls — governed by OpenAI platform policy, not a Whisper feature; self-host = you control 100%
- [x] self-host option — **yes, MIT license, weights + code fully open** (this is Whisper's defining trait)

**What the output actually looks like:** A flat transcript. `verbose_json` returns an array of segments each with `start`, `end`, `text`, plus optional word-level objects (`whisper-1` only); `srt`/`vtt` are subtitle files. There is **no document structure, no on-screen text, no spoken-vs-shown separation, no Markdown** — it is a linear time-coded string of what was *said*. To get mdreel's deliverable you would bolt Whisper's audio track onto a separate vision pipeline and a fusion/formatting layer yourself.

## 4. Size & customer base — evidence, not vibes
- **Case studies/logos:** none Whisper-specific; it is embedded silently inside thousands of products.
- **Review counts/ratings:** N/A — it's a model/API, not a listed SaaS on G2/Capterra (no dedicated review page found).
- **Web-traffic estimate:** unknown (traffic accrues to openai.com / platform docs, not separable).
- **GitHub stars/community:** **105,000+ stars, ~12.8k forks, 13 releases** on `openai/whisper` — one of the most-starred ML repos in existence. Huge downstream ecosystem (whisper.cpp, faster-whisper, WhisperX, etc.).
- **Hiring signals:** N/A at Whisper level.

## 5. GTM & distribution (feeds A5)
- **Channels:** dominant **developer/OSS distribution** — GitHub repo + platform docs + API. No landing-page funnel, no free web tool, no demo gallery for Whisper itself. Distribution is "every ML tutorial and every STT wrapper on the internet uses Whisper as the default."
- **SEO:** ranks #1 for "open source speech to text," "whisper api pricing," "transcription model" — an enormous long-tail of third-party pricing/comparison pages (diyai.io, costgoat, costbench, invertedstone, etc.) that mdreel will never out-rank on the generic term.
- **Free tool / community:** the repo *is* the free tool; community is the moat.
- **Positioning sentence (verbatim):** *"Whisper is a general-purpose speech recognition model."* (openai/whisper README)
- **Who the page talks to:** **developers.** Docs are code-first (API params, `timestamp_granularities[]`, `chunking_strategy`). No language aimed at team leads, L&D, or DPOs.

## 6. EU/GDPR posture (feeds A1)
- **Hosting regions:** hosted API runs on **OpenAI/US infrastructure** — **no EU-residency guarantee for the `whisper-1`/gpt-4o-transcribe endpoints** (regional data residency exists for some OpenAI enterprise offerings but is not a property of the Whisper transcription API). **Self-hosting is the only way to keep data in the EU — and then it's on your own GCP/EU boxes, not OpenAI's.**
- **DPA available?:** OpenAI offers a platform-level DPA for API customers; not Whisper-specific.
- **Subprocessor list:** OpenAI platform list (US-centric).
- **No-training terms:** OpenAI API data is not used for training by default (platform policy) — but the *processing* still occurs in the US for the hosted path.
- **Certifications:** OpenAI holds SOC 2; ISO posture at platform level (not re-verified here).
- **Residency premium:** **not marketed or charged** — EU residency is simply **not on offer** for the hosted transcription API. This is precisely the gap mdreel's A1 thesis targets: a DPO cannot buy "EU-processed" from OpenAI's Whisper endpoint, only self-host it themselves.

## 7. Threat assessment
- **ICP overlap: LOW-to-MEDIUM.** Whisper is *infra*, not a competitor product. An EU team of 50–500 building an internal AI assistant will almost certainly touch Whisper — but as a **component**, not a finished deliverable. Overlap is at the "build vs buy" boundary: Whisper is the strongest argument for a customer to DIY the audio half.
- **What they'd have to do to kill mdreel:** OpenAI would have to (a) add **vision / verbatim on-screen-text extraction**, (b) add **spoken-vs-shown separation and structured Markdown output**, and (c) offer **EU-only processing** — none of which is on the transcription roadmap; their direction is diarization and realtime, still audio-only, still US-hosted. **Likelihood: low.** More realistic indirect threat: a third party stitches Whisper + GPT-4o-vision + a formatter into a mdreel-shaped product cheaply — but that party then owns the fusion/EU-residency problem mdreel already solved.
- **What mdreel does that they structurally can't (as `whisper-1`):** (1) **on-screen VERBATIM text** (slides/code/UI) — Whisper is deaf to pixels; (2) **spoken-vs-shown separation**; (3) **portable structured Markdown** built for RAG; (4) **guaranteed EU processing without the customer self-hosting**; (5) a **video** deliverable, not a 25 MB audio-chunk workflow.
- **What mdreel should steal:** (1) **radically simple usage meter** — per-minute, no seats, no storage games; mdreel's per-hour hard cap is fine but keep the pricing page this legible. (2) **`verbose_json` schema shape** (segments with start/end/text/words) is the de-facto interchange format — mdreel's Markdown should map cleanly to/from it so customers can drop mdreel in where they already pipe Whisper. (3) **Word-level timestamp granularity** as a first-class option. (4) **Developer-first docs tone** — code samples over marketing copy. (5) Publish an honest **cost-per-hour** number the way the ecosystem does for Whisper (€0.16–0.32/h); mdreel's premium over that must be justified by the vision+fusion+EU story, made explicit.

## 8. Evidence log
| # | Claim | Source URL | Checked | Grade |
| 1 | "Whisper is a general-purpose speech recognition model" | https://github.com/openai/whisper | 2026-07-15 | Q (via cached README fetch) |
| 2 | MIT license, weights + code open; model sizes tiny/base/small/medium/large/turbo; turbo ~8× faster than large | https://github.com/openai/whisper | 2026-07-15 | Q |
| 3 | 105,000+ GitHub stars, ~12.8k forks, 13 releases | https://github.com/openai/whisper | 2026-07-15 | Q |
| 4 | Whisper is audio-only; no on-screen text / vision; multilingual + translation, 30s sliding window | https://github.com/openai/whisper | 2026-07-15 | Q |
| 5 | Hosted models: whisper-1, gpt-4o-transcribe, gpt-4o-mini-transcribe, gpt-4o-transcribe-diarize | https://developers.openai.com/api/docs/guides/speech-to-text | 2026-07-15 | Q |
| 6 | Word + segment timestamps via `timestamp_granularities[]`, "only available for whisper-1" | https://developers.openai.com/api/docs/guides/speech-to-text | 2026-07-15 | Q |
| 7 | Upload limit 25 MB; ~99 languages; diarization requires `chunking_strategy` >30s | https://developers.openai.com/api/docs/guides/speech-to-text | 2026-07-15 | Q |
| 8 | Output formats: whisper-1 = json/text/srt/vtt/verbose_json; gpt-4o = json/text; diarize = json/text/diarized_json; no Markdown | https://developers.openai.com/api/docs/guides/speech-to-text | 2026-07-15 | Q |
| 9 | gpt-4o-transcribe $0.006/min; gpt-4o-mini-transcribe $0.003/min; gpt-realtime-whisper $0.017/min | https://developers.openai.com/api/docs/pricing | 2026-07-15 | Q |
| 10 | whisper-1 $0.006/min ($0.36/hr) legacy standard rate | https://costgoat.com/pricing/openai-transcription | 2026-07-15 | T (costgoat) |
| 11 | Per-hour cost examples: $0.18 mini, $0.36 whisper-1/4o, $1.02 realtime | https://diyai.io/ai-tools/speech-to-text/openai-whisper-api-pricing-2026/ | 2026-07-15 | T (diyai.io) |
| 12 | Effective €/h ≈ €0.16 (mini), €0.32 (whisper-1/4o), €0.92 (realtime) | derived from #9–#11 | 2026-07-15 | E (USD/hr × 0.90 USD→EUR) |
| 13 | Hosted transcription API has no EU-residency guarantee; self-host is only EU-keeping path; no residency premium marketed | https://developers.openai.com/api/docs/guides/speech-to-text | 2026-07-15 | E (inference: no residency option present in docs/pricing; US HQ) |
| 14 | OpenAI HQ San Francisco, USA; Whisper OSS released Sep 2022, whisper-1 API Mar 2023 | https://github.com/openai/whisper | 2026-07-15 | T/E (widely documented; dates from release history) |

Note: `openai/whisper` GitHub page returned HTTP 403 on direct fetch; repo facts (#1–4) taken from a cached fetch of the same URL earlier in this session. Parent-company funding and headcount deliberately left "unknown" rather than guessed.