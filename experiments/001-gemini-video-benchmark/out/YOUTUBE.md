# RESULTS — Phase 0.1 YouTube-ingestion spike

Date: 2026-07-14 · Model: `gemini-2.5-flash` (Vertex AI) · Regions: `europe-central2`,
`europe-west3` · API: `v1` · Spend: **$0.69 (~€0.60)** of the ≤ €1 budget (all calls in
`ledger.csv`, labels `yt_*`). EUR/USD = 0.86.

Corpus: the 7 URLs supplied by the founder (product demo, streamed video, 2 × conference
presentation, tutorial, podcast, corporate Teams call). **No bytes were downloaded** — every call
passed a `youtube.com/watch?v=…` URL as `fileData.fileUri` and Google fetched it (CLAUDE.md rule 8).

> ⚠️ **Point-in-time memo, never authoritative** (CLAUDE.md). Graduated numbers live in METRICS.md.

---

## Q1 — Does `europe-central2` accept a YouTube `fileUri`? ✅ **YES**

Both EU regions accept a YouTube watch URL in `generateContent` and genuinely fetch the video —
the 60 s probe correctly described a charity-app advert and quoted its first and last audible words.

| Region | API `v1` | Result |
|---|---|---|
| `europe-central2` | ✅ | video=3,894 tok, audio=1,475 tok, $0.0028 |
| `europe-west3` | ✅ | video=3,894 tok, audio=1,475 tok, $0.0027 |

Identical token counts in both regions. **No US region is required. Phases 0.2/0.3 proceed as
planned.**

**`mimeType` is mandatory even for YouTube URLs.** Omitting it returns
`400 INVALID_ARGUMENT — "empty mimeType parameter in fileData"`. It is a formality (Google decides
the real format), but the request is rejected without it.

### 🚧 Methodological trap: `countTokens` is useless as a probe here

`countTokens` returned **`totalTokens: 421`, `video: 0`** for *every* URL in *every* region — the
prompt text alone. It does **not** tokenize `fileData` media parts; it silently ignores them and
returns `200 OK`. **It therefore cannot prove acceptance and cannot measure a window.** The original
plan was to answer Q1 and Q2 for free this way. It does not work. Only `generateContent` bills, and
only `generateContent` tells the truth.

Consequence for the spike's safety: since a call on a long video with ignored offsets could have
eaten the entire budget at once, the first paid probe was deliberately the smallest thing that could
still answer the question — 60 s window, `MEDIA_RESOLUTION_LOW`, `maxOutputTokens: 256`,
`thinkingBudget: 0` → **$0.0028.**

---

## Q2 — Do `videoMetadata.startOffset/endOffset` clip a YouTube URL? ✅ **YES, server-side**

Three windows on the same podcast (`kwSVtQ7dziU`), default media resolution:

| Window | Length | Video tok | Audio tok | Cost | First words heard |
|---|---|---|---|---|---|
| `0–120 s` | 120 s | 30,960 | 3,000 | $0.0128 | *"Code's not even the right verb anymore…"* |
| `1800–1920 s` | 120 s | **30,960** | **3,000** | $0.0125 | *"…we should be able to see more speciation."* |
| `1800–2400 s` | 600 s | **154,800** (5×) | **15,000** (5×) | $0.0617 | *"…we should be able to see more speciation."* |

**Equal length ⇒ identical token count regardless of position. 5× the length ⇒ exactly 5× the
tokens.** And the two independent calls that both seek to 1800 s report the **same first audible
line** — the server really is seeking, not re-reading from the top.

Corroborating evidence from the first attempt (run against a 59 s advert before we knew it was
short): a window at `600–720 s` on that video returned **`400 INVALID_ARGUMENT`** — Vertex refuses to
seek past the end. It is applying the offsets, not ignoring them.

> ### ⇒ The consequence that matters
> **Long videos can be segmented without ever holding the bytes.** The public path needs no Stage A,
> no ffmpeg, no GCS round-trip — just N calls with different offsets. It is a genuinely simpler
> pipeline than the private path, exactly as ARCHITECTURE §1 hoped.

### 🎁 Bonus: **the true duration comes back free in every response**

Tokenization is a fixed rate — **video 258 tok/s, audio 25 tok/s** at default media resolution
(and video ≈ 66 tok/s at `MEDIA_RESOLUTION_LOW`; audio is unaffected by media resolution).

So `video_tokens ÷ 258` recovers **exactly what Google actually fetched**:

| Call | Requested window | Video tok | ⇒ real duration |
|---|---|---|---|
| `yt_demo` | 0–600 s | 15,222 | **59 s** — it is a 59 s advert |
| `yt_tutorial` | 0–600 s | 137,514 | **533 s** — the video is under 9 min |
| everything else | 0–600 s | 154,800 | 600 s |

**Vertex clamps the window to the video's end and bills only what it fetched.** No YouTube Data API
call, no metadata scrape, no `yt-dlp` is needed to learn a video's length — **ask for more than you
expect and read the duration off the bill.**

---

## Q3 — Cost of the un-blended path: ✅ **measured, and cheaper than feared**

Full ARCHITECTURE §3 Stage B (structured JSON, `thinkingBudget: 4096`) on the first 10 minutes of
each video — which is *exactly what the production free tool will do* (PLAN.md Phase 3 caps at the
first ~10–15 min), so this measures the real thing.

€/video-hour is computed from the **actual fetched duration** (see above), not the requested window.

| Video | Category | Blocks | Coverage | Video tok | Out tok | $/call | **€/video-hour** |
|---|---|---|---|---|---|---|---|
| `stream` | streamed video | 32 | 97% | 154,800 | 8,948 | 0.0942 | **0.49** |
| `conf1` | conference presentation | 30 | 98% | 154,800 | 7,228 | 0.0824 | **0.43** |
| `conf2` | conference presentation | 14 | 97% | 154,800 | 4,274 | 0.0825 | **0.43** |
| `tutorial` | tutorial | 24 | 82% | 137,514 | 9,156 | 0.0879 | **0.51** |
| `podcast` | podcast | 29 | 94% | 154,800 | 6,008 | 0.0798 | **0.41** |
| `teamscall` | corporate Teams call | 14 | 90% | 154,800 | 4,276 | 0.0825 | **0.43** |
| | | | | | | | **mean ≈ 0.45** |
| `conf1_lowres` | ⭐ same talk, `MEDIA_RESOLUTION_LOW` | 31 | 98% | 39,600 | 6,588 | 0.0537 | **0.28** |

Stage B only — Stage C fusion is on top (~€0.10/video-hour, Phase 0 RESULTS).

**Cost is remarkably flat across content categories** (€0.41–0.51). It is dominated by duration, not
by what is *in* the video: video tokens are a fixed 258/s regardless of content. The only variance
comes from output tokens.

### ⭐ The finding that changes the abuse math: **a blanket low media resolution IS available**

PLAN.md assumed the cost lever was simply gone on this path. That is **half right**. Stage A's
*per-segment* static-content routing is indeed impossible (no local bytes → no frame-difference
pass). But `mediaResolution` is a **request-level** knob that needs no local analysis at all —
and it can just be set to `LOW` for the whole public path.

It cuts video tokens **3.9×** (258 → 66 tok/s) for **€0.28 vs €0.43** on the same talk, and
**quality held**: 31 blocks vs 30, coverage 98% vs 97%, and the OCR stayed verbatim — it read the
sponsor wall, *"Geoffrey Litt · Design Engineer at Notion"*, and the slide body text. It even
caught one sponsor logo the default-resolution run missed.

> **Recommendation: the public path runs at `MEDIA_RESOLUTION_LOW` by default.** It is free money on
> a path whose entire purpose is to be given away, and it is the single biggest input to the abuse
> caps (METRICS.md N10).

**Audio becomes the floor.** At low media resolution, audio ($1.00/M — 3.3× video's $0.30/M) and
*output* tokens dominate: for `conf1_lowres`, video is $0.012 of a $0.054 call, while output is
$0.027. Cutting video resolution further would buy almost nothing.

---

## Pathologies & product-side guards

1. 🚨 **The Phase-0 coverage guard false-fires on this path — and it costs double.** `yt_demo`
   "failed" twice and was billed twice. It did nothing wrong: it is a 59 s advert and the model
   covered it perfectly (13 blocks, 00:00:00 → 00:00:53). The guard divided by the *requested*
   600 s window (53/600 = 8.8% coverage) and declared under-generation.
   **⇒ On the YouTube path the clip length is not known up front, so the coverage denominator must
   be the fetched duration (`video_tokens ÷ 258`), not the requested window.** Without this, every
   video shorter than the window is a guaranteed double-billed false failure. Phase 2/3 requirement.
2. **`thinkingBudget: 4096` is binding, not slack.** 5 of 8 Stage B calls hit the cap exactly
   (4,092–4,094 thought tokens). Output was fine in every case (`finishReason: STOP`, good
   coverage), so a cap-hit here is **not** a failure signal — contra ARCHITECTURE §3's
   "treat a cap-hit as a segment failure". Worth revisiting that rule with this evidence.
3. **No runaway generations** in this batch (0 of 8 hit `maxOutputTokens`) — the guards work.
4. **Regional 429s are common** under sequential load (up to 3 backoffs on one call). Exponential
   backoff sufficed. The free tool needs a queue, not just retries.
5. **Coverage on `tutorial` was 82%** — the weakest of the six. Continuous screen-recorded footage
   again (the same pathology as Phase 0's seg2), not a YouTube-specific problem.

---

## Notes for Phase 0.2 — read before selecting the corpus

- ⚠️ **None of these 7 videos is confirmed CC-licensed, so none of this output is publishable.**
  This spike only sent URLs to Vertex; it published nothing. **Phase 0.2 still requires a deliberate
  Creative-Commons selection** (CLAUDE.md rule 8) before anything reaches the gallery or a post.
- ⚠️ **The link labelled "product demo" (`samaSr6cmLU`) is a 59-second advert**, not a product demo.
  It is not usable as the long-screen-recording category.
- The A4 content-category gap is *provisionally* looking good — conference talks, podcast and Teams
  call all produced 90–98% coverage with populated `on_screen_text`. That is encouraging but it is
  **not** the A4 verdict; Phase 0.2 owns that, on a licensed corpus, with per-category quality checks.

## Graduated to the living docs (same commit)

- **METRICS.md §1.2** — public-path cost rows (default vs low media resolution), now `measured`;
  §1.3 N10 sizing.
- **ARCHITECTURE.md §1/§3/§8** — offsets confirmed; `mimeType` required; blanket low media
  resolution **is** available; coverage-guard denominator fix.
- **PLAN.md** — Phase 0.1 done → Phase 0.2 next.
