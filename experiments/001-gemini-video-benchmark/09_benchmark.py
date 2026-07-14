"""Phase 0.2 — the A4 benchmark on the licensed public corpus (PLAN.md).

A4 asks: **is the output citable?** Three sub-questions, one per deliverable:
  Q1  Is `on_screen_text` verbatim — or is it paraphrased/invented?
  Q2  Are the timestamps accurate — does the claim at t actually happen at t?
  Q3  Does the model hallucinate on-screen text on talking-head footage, where there is
      almost none? (The inverse of Phase 0's silent-video edge case: there, the temptation
      was to invent speech; here, it is to invent text.)

## How Q1-Q3 are grounded without a human watching six videos

Grading a model's OCR against its own summary is circular, and asking it "were you right?"
measures agreement, not accuracy. So the instrument is an **independent re-probe**:

  For a sample of blocks, take the block's own claimed timestamp t and issue a SECOND,
  cheap, differently-prompted call windowed to [t, t+PROBE_S]. That call is told nothing
  about the block. It just reports what it sees and hears.

That single trick grounds all three questions at once:
  - if the block's timestamp is wrong, the probe lands on different content and its text
    and speech will NOT match the block            -> Q2
  - the probe reads on-screen text back verbatim from the same moment, independently, so
    agreement on rare strings (names, code, headings) is evidence of real OCR, not of a
    plausible guess                                -> Q1
  - if a talking-head block claims on_screen_text and the probe sees a bare face and
    nothing else, that is a hallucination, caught  -> Q3

Two independent looks can still agree and both be wrong, so this is evidence, not proof —
but they cannot agree on an *invented* verbatim string by chance, which is the failure
mode that actually threatens A4.

Usage:  python 09_benchmark.py run     # Stage B over the corpus (paid)
        python 09_benchmark.py probe   # the re-probe grading pass (paid, cheap)
        python 09_benchmark.py report  # verdicts -> out/CORPUS.md (free)
"""

import csv
import json
import random
import re
import sys
from importlib import import_module

from common import (LEDGER, OUT, gemini_call, hhmmss, ledger_total, response_text,
                    spike_budget, spike_spent)

stage_b = import_module("03_stage_b")

# EUR 3 spike (PLAN.md Phase 0.2); EUR/USD = 0.86 as in Phase 0/0.1.
SPIKE_CAP_USD = 3.72  # EUR 3.20: the EUR 3 envelope plus a founder-approved top-up to
                      # finish the last two renders after adaptive halving overran it

# The public path runs at LOW media resolution by default: 3.9x cheaper with quality intact
# (Phase 0.1, out/YOUTUBE.md). Benchmarking any other config would measure a product we do
# not ship.
RESOLUTION = "MEDIA_RESOLUTION_LOW"

# 🚨 ONE STAGE B SEGMENT, not "the window the free tool shows".
#
# The first run of this benchmark used 900 s, reading PLAN.md Phase 3's "cap at the first
# ~10-15 min" as a single call. BOTH slide talks then died on maxOutputTokens: 15 minutes of
# slide-heavy conference footage simply does not fit in 65,535 output tokens, because dense
# on-screen text is exactly what makes Stage B verbose. The 10-minute segments of Phase 0
# and 0.1 never hit it.
#
# So the free tool's minute-cap must be enforced by SEGMENTING (server-side offsets — Phase
# 0.1 proved they work), never by widening the window. 600 s is the segment size that has
# now survived three phases.
WINDOW_S = 600

PROBE_S = 30          # re-probe window length
PROBES_PER_VIDEO = 4  # sampled blocks per video

PROBE_PROMPT = """\
Report ONLY what is directly observable in this clip. Do not summarize, do not infer, do
not speculate about the wider video.

Return JSON:
- "on_screen_text": every piece of text visible on screen, VERBATIM, exactly as written
  (slide headings, body text, code, UI labels, speaker name plates, captions). Preserve
  spelling and case. If there is NO text visible on screen at all, return an empty string.
  Never describe the text — quote it.
- "spoken": what is actually said, verbatim. Empty string if nothing is said.
- "scene": one sentence describing what is shown.
"""

PROBE_SCHEMA = {
    "type": "OBJECT",
    "properties": {
        "on_screen_text": {"type": "STRING"},
        "spoken": {"type": "STRING"},
        "scene": {"type": "STRING"},
    },
    "required": ["on_screen_text", "spoken", "scene"],
}


def corpus() -> list[dict]:
    data = json.loads((OUT / "corpus.json").read_text(encoding="utf-8"))
    if not data["accepted"]:
        sys.exit("out/corpus.json has no accepted videos — run 08_corpus.py first.")
    return data["accepted"]


def _last_ledger_row() -> dict:
    with LEDGER.open(newline="", encoding="utf-8") as f:
        return list(csv.DictReader(f))[-1]


# --- pass 1: Stage B over the corpus (the thing we actually ship) ---

def run() -> None:
    print(f"=== Stage B on the first {WINDOW_S // 60} min of each corpus video "
          f"({RESOLUTION}) ===")
    results = {}
    for v in corpus():
        label = f"c_{v['category']}_{v['video_id']}"
        window = min(WINDOW_S, v["duration_s"])
        print(f"\n{label}: {v['title'][:60]} [{v['duration_s']}s, window 0-{window}s]")
        try:
            parsed = stage_b.run_stage_b(
                v["url"], 0, label, config="corpus_lowres",
                media_resolution=RESOLUTION,
                clip_dur_s=window, youtube_window=(0, window),
                derive_clip_dur=True)  # denominator = fetched, not requested (0.1 fix)
            row = _last_ledger_row()
            results[label] = {
                **{k: v[k] for k in ("video_id", "category", "title", "channel", "url",
                                     "duration_s", "attribution")},
                "window_s": window,
                "blocks": len(parsed["blocks"]),
                "video_tokens": int(row["video_in"]),
                "audio_tokens": int(row["audio_in"]),
                "out_tokens": int(row["candidates_out"]),
                "cost_usd": float(row["cost_usd"]),
                "latency_s": float(row["latency_s"]),
            }
        except Exception as e:  # a failure is a finding, not a crash
            print(f"  FAILED — {e}")
            results[label] = {**v, "error": str(e)[:300]}
        print(f"  spike spend: ${spike_spent():.4f} / ${SPIKE_CAP_USD}")

    (OUT / "corpus_stage_b.json").write_text(
        json.dumps(results, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"\nspike spend: ${spike_spent():.4f}   ledger total: ${ledger_total():.4f}")


# --- pass 2: the independent re-probe (the grading instrument) ---

def _probe(url: str, start_s: int, label: str) -> dict:
    body = {
        "contents": [{"role": "user", "parts": [
            {"fileData": {"mimeType": "video/mp4", "fileUri": url},
             "videoMetadata": {"startOffset": f"{start_s}s",
                               "endOffset": f"{start_s + PROBE_S}s"}},
            {"text": PROBE_PROMPT},
        ]}],
        "generationConfig": {
            "temperature": 0.0,
            "maxOutputTokens": 2048,
            "thinkingConfig": {"thinkingBudget": 0},  # transcription, not deliberation
            "mediaResolution": RESOLUTION,
            "responseMimeType": "application/json",
            "responseSchema": PROBE_SCHEMA,
        },
    }
    data = gemini_call(body, label=label, config="probe30")
    return json.loads(response_text(data))


def _words(s: str) -> set[str]:
    return {w for w in re.findall(r"[a-z0-9]{4,}", (s or "").lower())}


def _overlap(a: str, b: str) -> float:
    """Fraction of the block's distinctive words the independent probe also saw.
    Short/common words are dropped: agreement on 'the' is not evidence of anything."""
    wa, wb = _words(a), _words(b)
    return len(wa & wb) / len(wa) if wa else 0.0


def probe() -> None:
    runs = json.loads((OUT / "corpus_stage_b.json").read_text(encoding="utf-8"))
    rng = random.Random(20260714)  # reproducible sample
    out = {}

    for label, r in runs.items():
        if "error" in r:
            continue
        blocks = json.loads((OUT / f"{label}.norm.json").read_text(
            encoding="utf-8"))["blocks"]
        # sample blocks that START far enough from the window end to be probeable
        probeable = [b for b in blocks
                     if _t(b["t"]) + PROBE_S <= min(r["window_s"], r["duration_s"])]
        picked = rng.sample(probeable, min(PROBES_PER_VIDEO, len(probeable)))
        picked.sort(key=lambda b: _t(b["t"]))

        print(f"\n{label}: probing {len(picked)} of {len(blocks)} blocks")
        rows = []
        for i, b in enumerate(picked):
            t = _t(b["t"])
            try:
                p = _probe(r["url"], t, f"{label}_probe{i}")
            except Exception as e:
                print(f"  probe@{b['t']} FAILED — {e}")
                continue
            row = {
                "t": b["t"],
                "block_on_screen_text": b.get("on_screen_text") or "",
                "probe_on_screen_text": p["on_screen_text"],
                "block_spoken": b.get("spoken") or "",
                "probe_spoken": p["spoken"],
                "probe_scene": p["scene"],
                # Q2: does the moment the block claims actually contain what it says?
                "speech_overlap": _overlap(b.get("spoken") or "", p["spoken"]),
                # Q1: was the OCR real, or a plausible-looking guess?
                "ost_overlap": _overlap(b.get("on_screen_text") or "", p["on_screen_text"]),
                # Q3: block claims text; independent look sees a bare frame
                "ost_hallucination_suspect": bool(
                    (b.get("on_screen_text") or "").strip()
                    and not p["on_screen_text"].strip()),
            }
            rows.append(row)
            print(f"  @{b['t']}  speech={row['speech_overlap']:.0%}  "
                  f"ost={row['ost_overlap']:.0%}"
                  f"{'  ⚠ OST HALLUCINATION SUSPECT' if row['ost_hallucination_suspect'] else ''}")
        out[label] = {"category": r["category"], "title": r["title"], "probes": rows}
        print(f"  spike spend: ${spike_spent():.4f} / ${SPIKE_CAP_USD}")

    (OUT / "corpus_probes.json").write_text(
        json.dumps(out, indent=2, ensure_ascii=False), encoding="utf-8")
    print(f"\nspike spend: ${spike_spent():.4f}   ledger total: ${ledger_total():.4f}")


def _t(hms: str) -> int:
    p = [int(x) for x in hms.split(":")]
    return p[0] * 3600 + p[1] * 60 + p[2]


# --- pass 3: verdicts (free) ---

def _anchor(block_spoken: str, probe_spoken: str, n: int = 12) -> float:
    """Q2, done honestly. Does the probe at the block's claimed time hear the block's
    OPENING words?

    The naive metric — what fraction of the block's speech the probe reproduced — punishes
    a correct block: blocks run 20-90 s but the probe window is 30 s, so the block's later
    words are simply not in the probe's clip. That is a window-length mismatch, not a
    timestamp error, and it was dragging honest videos to 47%.

    What actually tests the timestamp is the ANCHOR: seek to t, and the first thing you
    hear should be the first thing the block says."""
    head = [w for w in re.findall(r"[a-z0-9]{4,}", (block_spoken or "").lower())][:n]
    if not head:
        return None
    pw = _words(probe_spoken)
    return sum(1 for w in head if w in pw) / len(head)


def report() -> None:
    runs = json.loads((OUT / "corpus_stage_b.json").read_text(encoding="utf-8"))
    probes = json.loads((OUT / "corpus_probes.json").read_text(encoding="utf-8"))
    lines = []
    by_cat: dict[str, list] = {}

    for label, r in runs.items():
        if "error" in r:
            continue
        blocks = json.loads((OUT / f"{label}.norm.json").read_text(
            encoding="utf-8"))["blocks"]
        pr = probes.get(label, {}).get("probes", [])
        real = r["video_tokens"] / 66
        n_ost = sum(1 for b in blocks if (b.get("on_screen_text") or "").strip())
        # OCR fidelity is only defined where the block claimed text AND the probe saw text.
        # Averaging over blocks with no text at all scored a PERFECT abstention as 0% —
        # the talking-head video, whose whole job is to have no on-screen text, came out
        # looking like a total OCR failure. An empty sample is n/a, not zero.
        gradeable = [p for p in pr if p["block_on_screen_text"].strip()
                     and p["probe_on_screen_text"].strip()]
        stat = {
            "label": label,
            "category": r["category"],
            "title": r["title"],
            "channel": r["channel"],
            "blocks": len(blocks),
            "coverage": _t(blocks[-1]["t"]) / real if blocks else 0,
            "ost_blocks_pct": n_ost / len(blocks) if blocks else 0,
            "speech_anchor": _mean([a for p in pr
                                    if (a := _anchor(p["block_spoken"],
                                                     p["probe_spoken"])) is not None]),
            "ost_overlap": _mean([p["ost_overlap"] for p in gradeable]) if gradeable else None,
            "ost_gradeable": len(gradeable),
            # block claims on-screen text, an independent look sees none -> INVENTED
            "hallucinated": sum(1 for p in pr if p["ost_hallucination_suspect"]),
            # block claims none, an independent look sees text -> MISSED
            "missed_ost": sum(1 for p in pr if not p["block_on_screen_text"].strip()
                              and p["probe_on_screen_text"].strip()),
            "probes": len(pr),
            "cost_per_hour_eur": r["cost_usd"] * (3600 / max(real, 1)) * 0.86,
        }
        by_cat.setdefault(r["category"], []).append(stat)
        lines.append(stat)

    print(f"{'video':<34} {'blk':>4} {'cov':>5} {'ost%':>5} {'anchor':>7} {'ocr':>6} "
          f"{'halu':>5} {'miss':>5} {'EUR/h':>6}")
    for s in lines:
        ocr = f"{s['ost_overlap']:.0%}" if s["ost_overlap"] is not None else "n/a"
        print(f"{s['title'][:33]:<34} {s['blocks']:>4} {s['coverage']:>4.0%} "
              f"{s['ost_blocks_pct']:>4.0%} {s['speech_anchor']:>6.0%} {ocr:>6} "
              f"{s['hallucinated']:>2}/{s['probes']} {s['missed_ost']:>3}/{s['probes']} "
              f"{s['cost_per_hour_eur']:>6.2f}")

    (OUT / "corpus_verdict.json").write_text(
        json.dumps({"per_video": lines, "by_category": {
            c: {
                "videos": len(v),
                "coverage": _mean([s["coverage"] for s in v]),
                "speech_anchor": _mean([s["speech_anchor"] for s in v]),
                "ost_overlap": _mean([s["ost_overlap"] for s in v
                                      if s["ost_overlap"] is not None]) or None,
                "ost_blocks_pct": _mean([s["ost_blocks_pct"] for s in v]),
                "hallucinated": sum(s["hallucinated"] for s in v),
                "missed_ost": sum(s["missed_ost"] for s in v),
                "probes": sum(s["probes"] for s in v),
                "cost_per_hour_eur": _mean([s["cost_per_hour_eur"] for s in v]),
            } for c, v in by_cat.items()}}, indent=2), encoding="utf-8")
    print(f"\nwrote {OUT / 'corpus_verdict.json'}")


def _mean(xs: list[float]) -> float:
    xs = [x for x in xs if x is not None]
    return sum(xs) / len(xs) if xs else 0.0


if __name__ == "__main__":
    what = sys.argv[1] if len(sys.argv) > 1 else "report"
    if what in ("run", "probe"):
        spike_budget("phase-0.2-corpus", SPIKE_CAP_USD)
    {"run": run, "probe": probe, "report": report}[what]()
