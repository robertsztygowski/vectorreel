# GATE — does Stage B obey Stage A's boundaries? (Phase 3, first job)

> **Status: PASS, 2026-07-17.** Point-in-time memo (experiments are never authoritative — the living
> docs win). Reproduce with `gate_boundary_check.py`; raw responses in `out/gate_*.raw.txt`.

## Question

PLAN.md Phase 3's first job. Phase 1 proved Stage A *emits* forced block boundaries (METRICS.md N7c);
only a Vertex call proves Stage B *obeys* them. If it ignores them, the fallback is to cut real
segment boundaries at the cues (more calls).

## Method

- Clip: `seg2_720p.mp4` — the N7b failing window (12.8 min, 94% static, a man talking over a frozen
  IDE) that returned ≤3 blocks in 4/4 unguided benchmark runs. Uploaded to
  `gs://…-vectorreel-dev/gate/seg2_720p.mp4`.
- Boundaries: the **real** Stage A output for this clip, dumped by running `StageARunner` (ffmpeg)
  end-to-end (`out/gate_seg2_boundaries.json`): 22 cues.
- Two live `gemini-2.5-flash` calls @ `europe-central2`, `MEDIA_RESOLUTION_LOW`, structured output:
  - **BASELINE** — shipped benchmark prompt, no boundaries.
  - **GUIDED** — same prompt + the 22 cues listed as mandatory block starts.
- Adherence = a model block start within ±6 s of a forced boundary.

## Result

| call | blocks | boundary adherence |
|---|---|---|
| baseline | 10 | 5 / 22 — arbitrary placement |
| **guided** | **22** | **21 / 22 — every match exact to the second** |

Guided block starts (s): `0, 45, 65, 112, 153, 175, 216, 265, 309, 350, 371, 391, 439, 486, 525,
569, 603, 634, 654, 674, 694, 733` — identical to the forced cues. The 22nd cue (753 s) was lost only
because the dense guided response overflowed the 12k output-token cap and the JSON truncated.

## Verdict

**Stage B obeys Stage A's boundaries.** The design holds; the fallback (cutting real segments at
cues) is not needed. The single side effect — a dense guided call overflowing `maxOutputTokens` — is
exactly the `MAX_TOKENS`→halve case ARCHITECTURE §3 and `StageBRunner` already handle. No prompt
redesign required for the pipeline build; the production prompt must list the segment's cues as
mandatory block starts.

## Cost

Two calls, ≈ $0.12 total (ledger `out/ledger.csv`, label `gate_*`).
