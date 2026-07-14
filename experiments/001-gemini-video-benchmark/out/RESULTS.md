# RESULTS — Phase 0 Gemini video benchmark

Date: 2026-07-14 · Model: `gemini-2.5-flash` (Vertex AI) · Region: `europe-central2` ·
Total Vertex spend: **$2.22 (~€1.91)** of the ≤ €5 budget (all calls in `ledger.csv`).
Test video: 50:40 demo screen-recording (Teams meeting, slides + live IDE demo, 1080p@16fps);
edge case: 1:52 silent UI screencast (HabiCen). EUR/USD = 0.86.

## Q1 — EU availability: ✅ YES

Probed via free `countTokens` against regional endpoints (project `tensile-runway-442915-j6`):

| Region | gemini-2.5-flash | flash-lite | 2.5-pro | Gemini 3.x |
|---|---|---|---|---|
| `europe-central2` | ✅ | ✅ | ✅ | ❌ (any variant) |
| `europe-west3` | ✅ | ❌ | ❌ | ❌ |

Native video from a GCS URI in `europe-central2` works (all Stage B calls below used it).
**Decision: `gemini-2.5-flash` @ `europe-central2`; `europe-west3` is a Flash-only fallback.**

## Q2 — Real cost per video-hour: ✅ €0.21–0.40/h, guardrail holds 4–7×

Matrix on 2 × 12.8-min segments (720s+50s overlap each), per-modality tokens from
`usageMetadata`, bounded `thinkingBudget: 4096`:

| Config | Sampling | Video tok/call | $/call | **€/video-hour** | Latency | Quality notes |
|---|---|---|---|---|---|---|
| A | 720p, 1 fps, default res | 198,660 | 0.095 | **0.38** | 94 s | best OCR reliability; granularity collapse on seg2 (see Q3) |
| B | 720p, fps 0.25 | 49,794 | 0.050 | **0.20** | 70 s | ❌ rejected: timestamp format drift + under-coverage |
| C | 1080p source, 1 fps | 198,660 | 0.100 | **0.40** | 150 s | same tokens as A; marginally better OCR on densest slide; option for a "high" tier |
| D | 720p, 1 fps, `MEDIA_RESOLUTION_LOW` | 50,820 | 0.053 | **0.21** | 61 s | OCR still near-verbatim; best granularity on hard seg2 |

- Audio is priced 3.3× video per token ($1.00/M vs $0.30/M) and is constant across configs
  (~19k tok/call) — at low media resolution **audio becomes ~40% of input cost**.
- Retry overhead observed in practice ≈ 1.3× (under-generation retries, one thinking blowup).
- Fusion pass (Stage C, text-only): ~$0.10 per 50-min video.
- **Blended estimate** with Stage A routing static runs (~67%, Q4) to config D:
  ≈ **€0.25–0.30/video-hour** → 4–7× inside the €1.50/h COGS guardrail (BUSINESS_MODEL §9).
- Guardrail check per config: A 0.38 ✅ · B 0.20 ✅ · C 0.40 ✅ · D 0.21 ✅ (all < €1.50).

## Q3 — Stage B quality: ✅ verbatim OCR & timestamps; ⚠ reliability needs product-side guards

Spot-check: 10 known moments vs frames extracted at the claimed timestamps
(founder to confirm — see table below):

- 9/10 match. Slide text, UI labels, Danish email bodies, and **C# code verbatim**
  (`public class TemplateManagementDbContext : BusinessIsolationDbContext` read correctly at
  720p). Timestamps accurate within ~±5 s. Speakers correctly kept as "Speaker 1".
- Errors found: one word on the densest diagram at 720p ("acting" vs "editing"; 1080p read it
  correctly); one row misattributed in a 9-row ticket table (requester/type swapped);
  one fast-changing demo moment where block text aggregates over the block, not the exact frame.
- **Silent video (HabiCen): PASS.** 20 blocks, `on_screen_text` populated in all, `spoken`
  empty everywhere, speaker null — no hallucinated transcript despite the present-but-silent
  audio track.

| # | Moment | Verdict (Claude) | Founder |
|---|---|---|---|
| 1 | 02:11 Agenda slide | ✅ verbatim | ☐ |
| 2 | 04:26 multitenancy diagram | ✅ verbatim | ☐ |
| 3 | 07:24 dbo.people table | ✅ verbatim | ☐ |
| 4 | 09:44 architecture diagram | ✅ (1 word off @720p, correct @1080p) | ☐ |
| 5 | 11:29 business vs platform slide | ✅ verbatim | ☐ |
| 6 | 27:12 DbContext code in IDE | ✅ verbatim code | ☐ |
| 7 | 32:18 HasData demo | ⚠ aggregate-over-block, frame shows terminal | ☐ |
| 8 | HabiCen 00:04 ticket queue | ✅ (1 table row misattributed) | ☐ |
| 9 | HabiCen 00:12 internal note flow | ✅ verbatim Danish | ☐ |
| 10 | HabiCen 00:30 search results | ✅ verbatim | ☐ |

**Reliability pathologies observed (each needs a deterministic product-side guard, Phase 3):**

1. **Under-generation / block clustering**: on continuous demo footage (seg2: 12 min of
   uninterrupted IDE work) config A returned ≤ 3 blocks clustered at clip start in 4/4 runs,
   while B/C/D spread 7–14 blocks fine. Guard: coverage check (last block start / clip length)
   + retry; consider prompt/chunking work.
2. **Thinking runaway**: unbounded (default) thinking hit 63k thought tokens on one call —
   truncated answer, 3× cost. Guard: `thinkingBudget: 4096` (used for all committed outputs
   except habicen/first seg1 runs).
3. **Timestamp format drift**: at non-default sampling the model emitted `mm:ss:centiseconds`
   (`02:11:80`) instead of `hh:mm:ss`. Guard: sequence-level normalization (monotonicity +
   clip-fit + field>59 heuristics) — implemented in `common.normalize_times`.
4. **Absolute-vs-relative anchoring**: one run anchored block times at the segment's global
   start (given in the prompt) instead of clip time. Guard: detect first-block ≈ segment_start
   and shift — implemented in `03_stage_b._clip_times`.
5. **MAX_TOKENS blowup**: one seg4 call filled 65k output tokens with runaway JSON. Guard:
   finishReason check + retry.
6. **Regional 429s**: ~1 per 4–5 calls under sequential load with user quota; exponential
   backoff sufficed; one stale-token 401 after long backoff (guard: token refresh on 401).

## Q4 — Static-content lever: ✅ ~67% of a real demo recording

Frame-difference pass (1 fps, 160×90 gray, mean-abs-diff): 60–79% of the 50-min video is
static depending on threshold; at threshold 2.0/255, **66.7% sits in static runs ≥ 10 s**
(46 runs). `static_timeline_*.json` has the full timeline.
Given config D is ~45% cheaper than A and quality held, Stage A routing static runs to
low media resolution is worth it; fps-reduction (config B) is **not** the mechanism —
it destabilizes timestamps/coverage.

## Bonus — end-to-end `output.md`

`out/output.md`: full 50-min video → §4-contract Markdown (frontmatter, timestamped `##`
topics, verbatim `On screen` blockquotes). Sources: segments 1/3/4 config A + segment 2
config D (A collapsed on that segment, see pathology 1). Fusion topic granularity is itself
nondeterministic — runs without explicit guidance gave 11 then 6 sections; with a
sections-per-duration instruction in the prompt the final run gave 13 well-placed sections
(00:00:00 → 00:43:15, monotonic). Seed for the future golden test (Phase 2/3).

## Graduated decisions (same commit)

- BUSINESS_MODEL.md §6 — measured COGS replaces the provisional $0.3–2/h range.
- ARCHITECTURE.md §2 — confirmed region/model matrix; §8 — levers reordered
  (media-resolution over fps), measured numbers, Stage B normalization/guard requirements.

## Open items for later phases

- Benchmark missing content categories: slide-deck talk, talking-head meeting (Phase 7 rerun).
- Segment length: 12.8 min worked; blocks cluster on continuous footage — try shorter
  segments or explicit per-N-seconds block instruction in Phase 3 prompt work.
- `gemini-2.5-flash-lite` (also in europe-central2) untested — potential further 3× cost cut
  for Stage B if quality holds.
- Diarization: single-speaker video only exercised "Speaker 1" labeling; multi-speaker
  accuracy untested.
