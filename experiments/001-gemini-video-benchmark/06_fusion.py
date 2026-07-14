"""Bonus: Stage C fusion. Ordered segment JSONs -> one output.md per the §4 contract.

Runs segments 3-4 at the winning config first if their outputs are missing
(segments 1-2 are reused from the matrix), then a single text-only fusion call.
"""

import json
import sys

from common import BUCKET, OUT, gemini_call, response_text
from importlib import import_module

stage_b = import_module("03_stage_b")

WINNING = "A"  # set from matrix results before running
# seg2's continuous-demo footage collapsed to <=3 blocks on every config-A run;
# config D (low media res) gave 14 well-spread blocks there — use it for fusion
SEG_SOURCES = {1: "A", 2: "D", 3: "A", 4: "A"}
SEG_STARTS = {1: 0, 2: 750, 3: 1500, 4: 2250}
VIDEO_META = {
    "source_filename": "Isolation Component V2 demo.mp4",
    "duration": "00:50:40",
    "processed_at": "2026-07-14T00:00:00Z",  # stamped properly at run time below
    "generator": "vectorreel-experiments/001@phase0",
}

FUSION_PROMPT = """\
You are fusing per-segment analyses of one video into a single Markdown document for an
AI knowledge base. The segments overlap by ~20 seconds; blocks appearing in two segments
are duplicates — keep one. Block timestamps are already global to the full video.

Produce EXACTLY this structure (output raw Markdown, no code fences):

---
title: "<concise descriptive title>"
source_filename: "{source_filename}"
duration: "{duration}"
language: "<dominant language code>"
processed_at: "{processed_at}"
generator: "{generator}"
summary: "<one-paragraph abstract of the video>"
tags: [<3-6 lowercase tags>]
---

# <same title>

Then one `## [hh:mm:ss] <topic heading>` section per TOPIC (merge adjacent blocks that
belong to the same topic; topic boundaries, not segment boundaries). Within each section:
- `**Spoken:**` cleaned transcript of the topic (omit the line if nothing spoken)
- `**On screen:**` followed by a `>` blockquote with the important verbatim on-screen text
  (omit if none)
- `**Visual:**` brief description of what is shown (omit if redundant)

Rules: every section anchored with its [hh:mm:ss] start time in ascending order; on-screen
text stays verbatim; never invent speaker names — unknown speakers stay "Speaker N";
do not invent content that is not in the input JSON. Topic sections should be 2-6 minutes
each — a ~50 minute video yields roughly 10-18 sections; never collapse more than ~6 minutes
of distinct material into one section, and preserve ALL spoken and on-screen content from
the input (redistribute it, don't drop it).

Segment analyses (ordered):
{segments_json}
"""


def main():
    import datetime
    for seg in (3, 4):
        label = f"seg{seg}_config{SEG_SOURCES[seg]}"
        if not (OUT / f"{label}.json").exists():
            uri = f"gs://{BUCKET}/segments/seg{seg}_720p.mp4"
            stage_b.run_stage_b(uri, SEG_STARTS[seg], label, config=SEG_SOURCES[seg],
                                clip_dur_s=790 if seg == 4 else 770)

    segments = [
        json.loads((OUT / f"seg{i}_config{SEG_SOURCES[i]}.norm.json").read_text(encoding="utf-8"))
        for i in (1, 2, 3, 4)
    ]
    meta = dict(VIDEO_META)
    meta["processed_at"] = datetime.datetime.now(datetime.UTC).strftime("%Y-%m-%dT%H:%M:%SZ")
    prompt = FUSION_PROMPT.format(
        segments_json=json.dumps(segments, ensure_ascii=False), **meta)

    body = {
        "contents": [{"role": "user", "parts": [{"text": prompt}]}],
        "generationConfig": {"temperature": 0.2, "maxOutputTokens": 65535},
    }
    data = gemini_call(body, label="fusion", config="C-fusion")
    md = response_text(data).strip()
    if md.startswith("```"):
        md = md.split("\n", 1)[1].rsplit("```", 1)[0].strip()
    (OUT / "output.md").write_text(md + "\n", encoding="utf-8")
    print(f"wrote out/output.md ({len(md)} chars)")


if __name__ == "__main__":
    if len(sys.argv) > 1:
        WINNING = sys.argv[1]
    main()
