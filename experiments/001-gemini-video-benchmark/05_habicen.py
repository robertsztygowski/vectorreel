"""Q3 edge case: HabiCen video has an audio track but nothing spoken.

The pipeline must output empty `spoken` fields, not a hallucinated transcript.
"""

import json

from common import BUCKET, OUT
from importlib import import_module

stage_b = import_module("03_stage_b")


def main():
    label = "habicen_configA"
    if (OUT / f"{label}.json").exists():
        parsed = json.loads((OUT / f"{label}.json").read_text(encoding="utf-8"))
        print(f"{label}: output exists, re-checking assertions only")
    else:
        parsed = stage_b.run_stage_b(f"gs://{BUCKET}/habicen.mp4", 0, label, config="A")

    hallucinated = [b for b in parsed["blocks"] if (b.get("spoken") or "").strip()]
    named_speakers = [b for b in parsed["blocks"] if b.get("speaker")]
    with_ocr = [b for b in parsed["blocks"] if (b.get("on_screen_text") or "").strip()]

    print(f"blocks: {len(parsed['blocks'])}, with on_screen_text: {len(with_ocr)}")
    if hallucinated:
        print(f"FAIL: {len(hallucinated)} blocks contain 'spoken' content on a silent video:")
        for b in hallucinated:
            print(f"  [{b['t']}] {b['spoken'][:120]!r}")
    else:
        print("PASS: no hallucinated speech (all 'spoken' fields empty).")
    if named_speakers:
        print(f"note: {len(named_speakers)} blocks carry a non-null speaker despite silence:")
        for b in named_speakers[:5]:
            print(f"  [{b['t']}] speaker={b['speaker']!r}")
    if not with_ocr:
        print("WARN: no on_screen_text extracted at all — check quality.")


if __name__ == "__main__":
    main()
