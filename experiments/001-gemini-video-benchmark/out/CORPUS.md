# RESULTS — Phase 0.2: public benchmark & demo corpus

Date: 2026-07-14 · Model: `gemini-2.5-flash` (Vertex AI) · Region: `europe-central2` ·
`mediaResolution: LOW` · Spend: **$3.57 (~€3.07)** against a €3 envelope + a founder-approved
€0.20 top-up (all calls in `ledger.csv`; labels `c_*`, `hero_*`, `fuse_*`, `*_probe*`).

> ⚠️ **Point-in-time memo, never authoritative** (CLAUDE.md). Graduated numbers live in METRICS.md
> (N4b, **N4d**, N7, **N7b**, **N30–N32**) and ARCHITECTURE.md §3/§8. **If this memo and a living
> doc disagree, the living doc wins.**

---

## What Phase 0.2 was for

One corpus that does three jobs at once: closes the **A4 category gap**, produces the first
**publishable** artifacts vectorreel has ever made, and seeds **committable test fixtures**. The
internal videos can do none of these — they are confidential and 293 MB.

## The corpus — 5 videos, every one licence-verified

**Nothing entered on a guess.** `08_corpus.py` calls the **YouTube Data API** (`videos.list`,
`part=status`) and admits a video only if `status.license == "creativeCommon"`, public, and
embeddable. The YouTube CC *search filter* is a discovery hint; the API is the evidence, and
`out/corpus.json` is the audit trail behind every published artifact.

34 candidates were verified; 5 were selected (EU conferences and technical content preferred —
these outputs *are* the Phase 0.3 demo material).

| Category | Video | Channel |
|---|---|---|
| slide talk | Exploring the CPython JIT (29:42) | EuroPython |
| slide talk | **How (Not) To Containerise Securely** (29:26) — *the hero* | FOSDEM |
| talking head | Dave Baszucki on Roblox (57:56) | Conversations with Tyler |
| talking head | In Conversation with Timnit Gebru & Melissa Chan (30:04) | Access Now |
| screencast | Crust of Rust: Lifetime Annotations (93:23) | Jon Gjengset |

🚧 **The gate proved its worth on day one:** all **7** URLs used in the Phase 0.1 spike came back
`license: "youtube"` — **none was CC, none was ever publishable.** 0.1's warning was right, and
0.2 correctly started from zero.

---

## Deliverable 1 — the A4 verdict, per category

### How it was graded (this is the part that matters)

Grading a model's OCR against its own summary is **circular**, and asking it "were you right?"
measures agreement, not accuracy. So: **independent re-probe.** For a sample of blocks, take the
block's *own claimed timestamp* and issue a second, cheap, differently-prompted call windowed to
exactly that moment, told nothing about the block — it just reports what it sees and hears.

One trick grounds all three questions: a **wrong timestamp** lands the probe on different content;
**invented OCR** cannot be reproduced verbatim by an independent look; a block claiming on-screen
text where the probe sees a bare face is a **hallucination, caught**.

Two looks can agree and both be wrong — this is evidence, not proof. But they cannot agree on an
*invented verbatim string* by chance, and that is the failure mode that threatens A4.

### The verdict → **METRICS.md N30–N32**

| Category | Coverage | Timestamp anchor | OCR verbatim | Invented text | Verdict |
|---|---|---|---|---|---|
| **slide talk** | 95% | **99%** | **99%** | **0/7** | ✅ **Strong** |
| **talking head** | 95% | **97%** | 77% | **0/8** | ✅ **Strong** |
| **screencast** | 95% | 77% | 68% | 0/4 | ⚠️ **Weakest** |

**On-screen text is real, not guessed.** Independent looks reproduced the same rare strings —
`cargo new --lib strsplit`, ``Created library `strsplit` package``, `#[test]`, and the speaker's
own name plate read off a title card (*"Andrew Martin … FOSDEM"*).

**The inverse edge case passes.** On bare studio footage the model claimed on-screen text in only
**4% of blocks**, and the independent probe agreed there was none. **It abstains rather than
invents** — the mirror image of Phase 0's silent-video result.

> ### 🚨 The finding that matters most is *where* the pass is thinnest
> The two categories added to close the gap — slide talk, talking head — came out **strong**. The
> category that **the paying product actually ingests** (screen recordings of internal demos) is the
> one that degrades: it under-segments badly, **7 blocks per 10 minutes (~86 s each)** versus ~25 s
> on slide talks, so citations land on vague, coarse spans. This has now appeared in **all three
> phases** — it is reproducible, not noise (**N7b**). **A4-accuracy is not settled until the
> screen-recording path is fixed.**

---

## Deliverable 2 — publishable artifacts

`out/corpus_md/` — one Markdown document per video, each carrying its CC BY attribution **inside
the file**, so it cannot be separated from the content downstream.

- **`JvbBFwlqxeI_full.md`** — the FOSDEM talk **end-to-end** (29:26, 9 topic sections): the hero
  artifact for the Phase 0.3 post. Verbatim slide text, real timestamps, speaker identified.
- Four × `*_excerpt_10min.md` — first 10 minutes, enough to show the shape per category.

## Deliverable 3 — committable fixtures ⚠️ **scope corrected**

**PLAN.md asked for CC clips from YouTube. That is not possible** — it means downloading bytes, and
**CLAUDE.md rule 8 forbids it.** A CC licence on the *content* does not override YouTube's ToS on
the *delivery*; they are separate instruments, and "the licence said I could" is not a defence a
company selling compliance to DPOs can afford to test.

So the fixtures come from sites that publish the media file itself under a redistributable licence:
**FOSDEM** (CC BY 2.0 BE), **NASA** (public domain), **Wikimedia Commons** (CC BY-SA own-work).
Three 90-second clips, **5.4 MB total**, in `tests/fixtures/videos/` with full provenance.

*EmacsConf was rejected despite being a natural fit: its only licence statement covers "material on
the EmacsConf wiki", which does not clearly extend to the speakers' recordings.*

---

## Pathologies — every one of these cost money to find

1. 🚨 **A 15-minute window blows the output cap on slide-heavy talks.** Both slide talks failed
   `MAX_TOKENS`, twice each — **~€0.70 for zero output**. Phase 0.1 never saw it because it used
   10-minute windows. The corpus window was cut to 600 s and all five then passed.
2. 🚨 **…and 10 minutes is not safe either.** The dense middle of the FOSDEM talk overflowed even at
   600 s and had to be split to **~75 s**. **Segment length does not bound Stage B's output —
   on-screen text density does, and that is not knowable before the call.** ⇒ the pipeline must
   **react** to an overflow (halve and re-run), not merely pick a good window. → ARCHITECTURE §3.
3. 🚨 **Naive halving is a cost amplifier.** You pay for the failed parent *and* both halves: the
   full FOSDEM talk took **22 Stage B calls** and came to **≈ €3.80/video-hour — ~13× N4c**
   (**METRICS.md N4d**). It also falsifies 0.1's "cost is flat across content categories": flat is
   true of the *input* only. **Phase 2 must segment dense content shorter up front.**
4. 🚨 **The coverage guard was half a guard.** A 15-min segment came back with blocks running to
   `04:48:10` — **1921% coverage** — and was **accepted**, because nothing ever checked *upward*.
   Cause: the model mixes `mm:ss:centiseconds` (`00:14:87` — 87 seconds) *with* real `hh:mm:ss`
   **inside one response**, which a whole-response format detector cannot represent. Fixed by
   normalizing **per timestamp** and guarding coverage on **both** sides.
5. **`MAX_TOKENS` must not be retried unchanged** — the overflow is deterministic, so the retry
   overflows identically and bills twice. It needs a *smaller segment*, not another attempt.
6. **A dropped connection was the one failure never retried** (the backoff only inspected HTTP
   status codes), and it killed a whole multi-segment render mid-flight. Long video calls hold a
   connection open for minutes; this will recur.
7. **A budget cap that resets is not a cap.** `spike_budget` anchored in memory, so each of the
   phase's three passes would have been handed a fresh full €3. Anchored to disk. **It then did its
   job — it halted the run mid-render at $3.4947/$3.49, which is why this memo has a top-up line.**

## Graduated to the living docs (same commit)

- **METRICS.md** — **N4d** (dense-content cost), N4a/N4b/N4c qualified, **N7** (per-category),
  **N7b** (under-segmentation), **N30–N32** (the A4 category verdicts), A4 evidence upgraded.
- **ARCHITECTURE.md §3/§8** — split-on-overflow; two-sided coverage guard; per-timestamp
  normalization; screen-recording weakness + the Stage A fix available on the private path.
- **PLAN.md** — Phase 0.2 done → **0.3 next**; new Phase 1/2 requirements.
