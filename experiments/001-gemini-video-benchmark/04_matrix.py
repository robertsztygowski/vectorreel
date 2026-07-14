"""Q2: sampling matrix. Configs A/B/C on segments 1-2, extrapolate EUR/video-hour.

A = 720p, default sampling (1 fps, default media resolution)
B = 720p, fps 0.25
C = 1080p, default sampling
"""

import csv
import json
import time

from common import BUCKET, LEDGER, OUT
from importlib import import_module

stage_b = import_module("03_stage_b")

SEG_DUR_S = 770  # segments 1 and 2 are both 770 s long
SEGMENTS = {1: 0, 2: 750}  # index -> global start second

CONFIGS = {
    "A": {"suffix": "720p", "fps": None},
    "B": {"suffix": "720p", "fps": 0.25},
    "C": {"suffix": "1080p", "fps": None},
    # fps=0.25 proved unreliable (timestamp drift, under-coverage) — low media
    # resolution at default 1 fps is the alternative cost lever
    "D": {"suffix": "720p", "fps": None, "media_resolution": "MEDIA_RESOLUTION_LOW"},
}

EUR_PER_USD = 0.86  # 2026-07 approx; RESULTS.md states the rate used


def main():
    for seg, start in SEGMENTS.items():
        for name, cfg in CONFIGS.items():
            label = f"seg{seg}_config{name}"
            if (OUT / f"{label}.json").exists():
                print(f"{label}: output exists, skipping")
                continue
            uri = f"gs://{BUCKET}/segments/seg{seg}_{cfg['suffix']}.mp4"
            stage_b.run_stage_b(uri, start, label, config=name, fps=cfg["fps"],
                                media_resolution=cfg.get("media_resolution"),
                                clip_dur_s=SEG_DUR_S)
            time.sleep(20)  # ease off the regional TPM quota

    # summarize from the ledger; retries append rows, keep only the LAST row per label
    with LEDGER.open(newline="", encoding="utf-8") as f:
        by_label = {r["label"]: r for r in csv.DictReader(f)
                    if r["label"].startswith("seg") and r["config"] in CONFIGS}
    rows = list(by_label.values())
    summary = {}
    for name in CONFIGS:
        cfg_rows = [r for r in rows if r["config"] == name]
        if not cfg_rows:
            continue
        cost = sum(float(r["cost_usd"]) for r in cfg_rows)
        hours = len(cfg_rows) * SEG_DUR_S / 3600
        summary[name] = {
            "calls": len(cfg_rows),
            "avg_latency_s": round(sum(float(r["latency_s"]) for r in cfg_rows) / len(cfg_rows), 1),
            "video_tokens_per_call": round(sum(int(r["video_in"]) for r in cfg_rows) / len(cfg_rows)),
            "audio_tokens_per_call": round(sum(int(r["audio_in"]) for r in cfg_rows) / len(cfg_rows)),
            "out_tokens_per_call": round(sum(
                int(r["candidates_out"]) + int(r["thoughts"]) for r in cfg_rows) / len(cfg_rows)),
            "usd_per_call": round(cost / len(cfg_rows), 4),
            "usd_per_video_hour": round(cost / hours, 3),
            "eur_per_video_hour": round(cost / hours * EUR_PER_USD, 3),
        }
    (OUT / "matrix_summary.json").write_text(json.dumps(summary, indent=2), encoding="utf-8")
    print(json.dumps(summary, indent=2))
    for name, s in summary.items():
        ok = "OK" if s["eur_per_video_hour"] < 1.50 else "OVER"
        print(f"config {name}: EUR {s['eur_per_video_hour']}/video-hour -> guardrail {ok}")


if __name__ == "__main__":
    main()
