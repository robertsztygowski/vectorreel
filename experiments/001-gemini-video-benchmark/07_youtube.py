"""Phase 0.1 — YouTube-ingestion spike (PLAN.md).

Three questions, answered against the founder's 7-URL corpus:
  Q1  Does europe-central2 accept a YouTube fileData.fileUri at all? (fallback europe-west3)
  Q2  Does videoMetadata.startOffset/endOffset clip a YouTube URL server-side?
  Q3  What does the un-blended (no Stage A, no static lever) path actually cost?

We never download bytes — Google fetches the video (CLAUDE.md rule 8). countTokens is free,
so Q1 and Q2 are answered without spending anything; only Q3 costs money.

Usage:  python 07_youtube.py regions | offsets | cost | all
"""

import csv
import json
import sys

from common import (LEDGER, OUT, build_stage_b_body, count_tokens, gemini_call, hhmmss,
                    ledger_total, response_text, spike_budget, spike_spent)

# EUR 1 spike (PLAN.md Phase 0.1); EUR/USD = 0.86 as in Phase 0.
SPIKE_CAP_USD = 1.16

VIDEOS = [
    {"id": "demo",      "vid": "samaSr6cmLU", "category": "product demo"},
    {"id": "stream",    "vid": "Bbt8cEyzsTk", "category": "streamed video"},
    {"id": "conf1",     "vid": "WkBPX-oDMnA", "category": "conference presentation"},
    {"id": "conf2",     "vid": "xUnRQ9vLXxo", "category": "conference presentation"},
    {"id": "tutorial",  "vid": "4biXYSNkn9Y", "category": "tutorial"},
    {"id": "podcast",   "vid": "kwSVtQ7dziU", "category": "podcast"},
    {"id": "teamscall", "vid": "rOqgRiNMVqg", "category": "corporate teams call"},
]
for v in VIDEOS:
    v["url"] = f"https://www.youtube.com/watch?v={v['vid']}"

REGIONS = ["europe-central2", "europe-west3"]
API_VERSIONS = ["v1", "v1beta1"]

WINDOW_S = 600  # 10-min Stage B window, comparable to Phase 0's 12-min segments


def _body(url, start=None, end=None, media_resolution=None):
    return build_stage_b_body(url, hhmmss(start or 0), start_offset_s=start,
                              end_offset_s=end, media_resolution=media_resolution)


# --- Q1: does an EU region accept a YouTube fileUri? (free) ---

def probe_regions() -> dict:
    print("=== Q1: region x api_version acceptance (countTokens, free) ===")
    matrix = {}
    for v in VIDEOS:
        for region in REGIONS:
            for api in API_VERSIONS:
                key = f"{v['id']}|{region}|{api}"
                r = count_tokens(_body(v["url"]), region=region, api_version=api)
                matrix[key] = r
                if r["ok"]:
                    print(f"  OK   {key}: total={r['total']} video={r['video']} "
                          f"audio={r['audio']}")
                else:
                    print(f"  FAIL {key}: HTTP {r['status']} {r['error'][:120]}")
    (OUT / "yt_regions.json").write_text(
        json.dumps(matrix, indent=2, ensure_ascii=False), encoding="utf-8")
    return matrix


# --- Q1/Q2: acceptance + offset clipping (paid, but bounded) ---
#
# countTokens turned out to be useless here: it does not tokenize fileData media parts
# at all (returns the text-only count, video=0, for every URL and every region), so it
# neither proves acceptance nor measures a window. Only generateContent does.
#
# That makes the ORDER a safety property. If offsets are silently ignored, a call on a
# 3-hour podcast processes the whole video and eats the entire spike budget in one shot.
# So the first probe is deliberately the cheapest thing that can still answer the
# question: a 60-second window, low media resolution, 256 output tokens, no thinking.
# Its ledger row tells us what the server actually fetched.

def _probe_body(url, start, end, media_resolution=None):
    part = build_stage_b_body(url, "00:00:00", start_offset_s=start, end_offset_s=end,
                              media_resolution=media_resolution)["contents"][0]["parts"][0]
    cfg = {"temperature": 0.0, "maxOutputTokens": 256,
           "thinkingConfig": {"thinkingBudget": 0}}
    if media_resolution:
        cfg["mediaResolution"] = media_resolution
    return {
        "contents": [{"role": "user", "parts": [
            part,
            {"text": "In one sentence: what is shown and said in this clip? "
                     "Then state the first and last thing you can hear."},
        ]}],
        "generationConfig": cfg,
    }


def _last_ledger_row() -> dict:
    with LEDGER.open(newline="", encoding="utf-8") as f:
        return list(csv.DictReader(f))[-1]


def probe_acceptance() -> dict:
    """Q1 + first read on Q2, at minimum blast radius."""
    print("\n=== Q1: does europe-central2 accept a YouTube fileUri in generateContent? ===")
    v = VIDEOS[0]
    out = {}
    for region in REGIONS:
        label = f"yt_probe60_{region}"
        try:
            data = gemini_call(_probe_body(v["url"], 0, 60, "MEDIA_RESOLUTION_LOW"),
                               label=label, config="probe60_low", region=region)
        except RuntimeError as e:
            print(f"  FAIL {region}: {e}")
            out[region] = {"ok": False, "error": str(e)[:400]}
            continue
        row = _last_ledger_row()
        out[region] = {"ok": True, "video_tokens": int(row["video_in"]),
                       "audio_tokens": int(row["audio_in"]),
                       "cost_usd": float(row["cost_usd"]),
                       "text": response_text(data)[:400]}
        print(f"  OK   {region}: video={row['video_in']} audio={row['audio_in']} "
              f"cost=${row['cost_usd']}")
        print(f"       -> {out[region]['text'][:160]}")
    (OUT / "yt_acceptance.json").write_text(
        json.dumps(out, indent=2, ensure_ascii=False), encoding="utf-8")
    return out


def probe_offsets(video_id: str = "podcast") -> dict:
    """Q2. Two windows of EQUAL LENGTH at DIFFERENT POSITIONS on the same video.
    If offsets clip server-side: equal token counts, different content.
    If offsets are ignored: both return the full-video token count and identical content.
    A third, longer window checks that tokens scale with window length.

    Needs a LONG video: the first run used VIDEOS[0], which turned out to be a ~60 s ad —
    the mid window fell past its end and Vertex returned 400 INVALID_ARGUMENT. That is
    itself evidence the offsets are applied server-side, but it cannot show a shift."""
    print("\n=== Q2: are videoMetadata offsets honoured? ===")
    v = next(x for x in VIDEOS if x["id"] == video_id)
    print(f"  video: {v['id']} ({v['category']}) {v['url']}")
    windows = [("early", 0, 120), ("mid", 1800, 1920), ("long", 1800, 2400)]
    out = {}
    for name, start, end in windows:
        label = f"yt_offset_{video_id}_{name}"
        try:
            data = gemini_call(_probe_body(v["url"], start, end), label=label,
                               config=f"offset_{name}")
        except RuntimeError as e:
            print(f"  FAIL {name}: {e}")
            out[name] = {"ok": False, "error": str(e)[:400]}
            continue
        row = _last_ledger_row()
        out[name] = {"ok": True, "window_s": end - start,
                     "video_tokens": int(row["video_in"]),
                     "audio_tokens": int(row["audio_in"]),
                     "cost_usd": float(row["cost_usd"]),
                     "text": response_text(data)[:500]}
        print(f"  {name:>6} [{start:>4}-{end:>4}s, {end - start:>3}s]: "
              f"video={row['video_in']:>7} audio={row['audio_in']:>6} "
              f"cost=${row['cost_usd']}")
        print(f"         -> {out[name]['text'][:150]}")
        print(f"  spike spend: ${spike_spent():.4f} / ${SPIKE_CAP_USD}")
    (OUT / "yt_offsets.json").write_text(
        json.dumps(out, indent=2, ensure_ascii=False), encoding="utf-8")
    return out


# --- Q3: cost of the un-blended path (paid) ---

def run_cost() -> None:
    """One 10-min Stage B window per video. The window is the FIRST 10 minutes, which is
    exactly what the production free tool will do (PLAN.md Phase 3: cap at first ~10-15
    min) — so this measures the real thing and needs no duration lookup."""
    from importlib import import_module
    stage_b = import_module("03_stage_b")

    print(f"\n=== Q3: Stage B on the first {WINDOW_S // 60} min of each video (paid) ===")
    results = {}
    for v in VIDEOS:
        label = f"yt_{v['id']}"
        print(f"\n{label}: {v['url']} window 00:00:00-{hhmmss(WINDOW_S)}")
        try:
            parsed = stage_b.run_stage_b(
                v["url"], 0, label, config="youtube_default",
                clip_dur_s=WINDOW_S, youtube_window=(0, WINDOW_S))
            row = _last_ledger_row()
            results[v["id"]] = {
                "category": v["category"], "blocks": len(parsed.get("blocks", [])),
                "video_tokens": int(row["video_in"]), "audio_tokens": int(row["audio_in"]),
                "out_tokens": int(row["candidates_out"]), "cost_usd": float(row["cost_usd"]),
                "latency_s": float(row["latency_s"]),
            }
        except Exception as e:  # a failure here is a finding, not a crash
            print(f"  {label}: FAILED — {e}")
            results[v["id"]] = {"category": v["category"], "error": str(e)[:300]}
        print(f"  spike spend so far: ${spike_spent():.4f} / ${SPIKE_CAP_USD}")

    # Bonus: Stage A's PER-SEGMENT static routing is unavailable on this path (no local
    # bytes), but the BLANKET mediaResolution knob may not be. If quality holds, it
    # changes the abuse-cap math (METRICS.md N10).
    v = next(x for x in VIDEOS if x["id"] == "conf1")
    print("\nyt_conf1_lowres: same window, MEDIA_RESOLUTION_LOW")
    try:
        parsed = stage_b.run_stage_b(v["url"], 0, "yt_conf1_lowres",
                                     config="youtube_lowres",
                                     media_resolution="MEDIA_RESOLUTION_LOW",
                                     clip_dur_s=WINDOW_S, youtube_window=(0, WINDOW_S))
        row = _last_ledger_row()
        results["conf1_lowres"] = {
            "category": "conference presentation (low res)",
            "blocks": len(parsed.get("blocks", [])),
            "video_tokens": int(row["video_in"]), "audio_tokens": int(row["audio_in"]),
            "out_tokens": int(row["candidates_out"]), "cost_usd": float(row["cost_usd"]),
            "latency_s": float(row["latency_s"]),
        }
    except Exception as e:
        print(f"  yt_conf1_lowres: FAILED — {e}")

    (OUT / "yt_cost.json").write_text(
        json.dumps(results, indent=2, ensure_ascii=False), encoding="utf-8")

    print(f"\n--- cost per video-hour (un-blended, {WINDOW_S}s windows) ---")
    for k, r in results.items():
        if "cost_usd" in r:
            per_hour_eur = r["cost_usd"] * (3600 / WINDOW_S) * 0.86
            print(f"  {k:>14}: ${r['cost_usd']:.4f}/call -> EUR {per_hour_eur:.2f}"
                  f"/video-hour  ({r['blocks']} blocks, {r['latency_s']:.0f}s)")
    print(f"\nspike spend: ${spike_spent():.4f}   ledger total: ${ledger_total():.4f}")


if __name__ == "__main__":
    what = sys.argv[1] if len(sys.argv) > 1 else "all"
    spike_budget("phase-0.1-youtube", SPIKE_CAP_USD)

    if what in ("regions", "all"):
        probe_regions()
    if what in ("acceptance", "all"):
        probe_acceptance()
    if what in ("offsets", "all"):
        probe_offsets()
    if what in ("cost", "all"):
        run_cost()
