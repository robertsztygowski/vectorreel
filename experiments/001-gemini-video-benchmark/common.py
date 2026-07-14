"""Shared plumbing for the Phase 0 benchmark: auth, Vertex REST calls, pricing, ledger.

Throwaway experiment code (see experiments/README.md). Auth = local gcloud user creds,
no ADC / key files. Every generateContent call MUST go through gemini_call() so it lands
in the ledger.
"""

import csv
import json
import subprocess
import time
from pathlib import Path

import requests

PROJECT = "tensile-runway-442915-j6"
REGION = "europe-central2"
FALLBACK_REGION = "europe-west3"
MODEL = "gemini-2.5-flash"
BUCKET = "tensile-runway-442915-j6-vectorreel-dev"

ROOT = Path(__file__).parent
OUT = ROOT / "out"
WORK = ROOT / "work"
OUT.mkdir(exist_ok=True)
WORK.mkdir(exist_ok=True)

LEDGER = OUT / "ledger.csv"
LEDGER_COLS = [
    "label", "model", "region", "config", "latency_s",
    "text_in", "video_in", "audio_in", "candidates_out", "thoughts",
    "cost_usd", "cumulative_usd",
]

# USD per 1M tokens, Vertex Gemini 2.5 Flash (verified 2026-07-14, standard tier)
PRICE = {"text_in": 0.30, "video_in": 0.30, "audio_in": 1.00, "out": 2.50}
BUDGET_USD = 5.5  # ~ EUR 5

_token_cache = {"token": None, "ts": 0.0}


def gcloud_token() -> str:
    if _token_cache["token"] and time.time() - _token_cache["ts"] < 45 * 60:
        return _token_cache["token"]
    r = subprocess.run("gcloud auth print-access-token", shell=True,
                       capture_output=True, text=True, check=True)
    lines = [ln.strip() for ln in r.stdout.splitlines()
             if ln.strip() and "WARNING" not in ln]
    _token_cache.update(token=lines[-1], ts=time.time())
    return _token_cache["token"]


def modality_tokens(usage: dict) -> dict:
    counts = {"TEXT": 0, "VIDEO": 0, "AUDIO": 0, "IMAGE": 0, "DOCUMENT": 0}
    for d in usage.get("promptTokensDetails", []):
        counts[d.get("modality", "TEXT")] = d.get("tokenCount", 0)
    return counts


def cost_usd(usage: dict) -> float:
    m = modality_tokens(usage)
    out_tokens = usage.get("candidatesTokenCount", 0) + usage.get("thoughtsTokenCount", 0)
    return (
        (m["TEXT"] + m["IMAGE"] + m["DOCUMENT"]) * PRICE["text_in"]
        + m["VIDEO"] * PRICE["video_in"]
        + m["AUDIO"] * PRICE["audio_in"]
        + out_tokens * PRICE["out"]
    ) / 1e6


def ledger_total() -> float:
    if not LEDGER.exists():
        return 0.0
    with LEDGER.open(newline="", encoding="utf-8") as f:
        rows = list(csv.DictReader(f))
    return float(rows[-1]["cumulative_usd"]) if rows else 0.0


def ledger_append(label: str, model: str, region: str, config: str,
                  latency_s: float, usage: dict) -> float:
    m = modality_tokens(usage)
    cost = cost_usd(usage)
    cumulative = ledger_total() + cost
    new = not LEDGER.exists()
    with LEDGER.open("a", newline="", encoding="utf-8") as f:
        w = csv.writer(f)
        if new:
            w.writerow(LEDGER_COLS)
        w.writerow([
            label, model, region, config, f"{latency_s:.1f}",
            m["TEXT"], m["VIDEO"], m["AUDIO"],
            usage.get("candidatesTokenCount", 0), usage.get("thoughtsTokenCount", 0),
            f"{cost:.4f}", f"{cumulative:.4f}",
        ])
    print(f"  ledger: {label} cost=${cost:.4f}  cumulative=${cumulative:.4f}")
    return cumulative


def gemini_call(body: dict, label: str, config: str = "", model: str = MODEL,
                region: str = REGION, api_version: str = "v1",
                max_retries: int = 5) -> dict:
    """generateContent with backoff + ledger. Returns the full response JSON."""
    total = ledger_total()
    if total > BUDGET_USD:
        raise RuntimeError(
            f"Ledger at ${total:.2f} > budget ${BUDGET_USD} — stop and ask the founder.")
    url = (f"https://{region}-aiplatform.googleapis.com/{api_version}/projects/{PROJECT}"
           f"/locations/{region}/publishers/google/models/{model}:generateContent")
    for attempt in range(max_retries):
        t0 = time.time()
        resp = requests.post(url, json=body, timeout=1800,
                             headers={"Authorization": f"Bearer {gcloud_token()}"})
        latency = time.time() - t0
        if resp.status_code == 200:
            data = resp.json()
            ledger_append(label, model, region, config, latency,
                          data.get("usageMetadata", {}))
            return data
        if resp.status_code == 401 and attempt < max_retries - 1:
            print(f"  {label}: HTTP 401, refreshing token and retrying")
            _token_cache["token"] = None
            time.sleep(2)
            continue
        if resp.status_code in (429, 500, 503) and attempt < max_retries - 1:
            wait = 15 * 2 ** attempt
            print(f"  {label}: HTTP {resp.status_code}, retry in {wait}s — {resp.text[:200]}")
            time.sleep(wait)
            continue
        raise RuntimeError(f"{label}: HTTP {resp.status_code}: {resp.text[:2000]}")


def response_text(data: dict) -> str:
    parts = data["candidates"][0]["content"]["parts"]
    return "".join(p.get("text", "") for p in parts)


def hhmmss(seconds: float) -> str:
    s = int(round(seconds))
    return f"{s // 3600:02d}:{s % 3600 // 60:02d}:{s % 60:02d}"


def parse_hhmmss(t: str) -> int:
    parts = [int(p) for p in t.strip().split(":")]
    while len(parts) < 3:
        parts.insert(0, 0)
    return parts[0] * 3600 + parts[1] * 60 + parts[2]


def normalize_times(ts: list[str], clip_dur_s: int | None) -> list[int]:
    """Model timestamps drift in format (observed at fps=0.25: mm:ss:centiseconds).
    Decide the format once per response: strict hh:mm:ss if the whole sequence is
    monotonic, fits the clip, and no minute/second field exceeds 59; otherwise try
    reading the first two fields as mm:ss."""
    def fields(t):
        return [int(p) for p in t.strip().split(":")]

    def ok(seq):
        monotonic = all(a <= b for a, b in zip(seq, seq[1:]))
        fits = not clip_dur_s or not seq or seq[-1] <= clip_dur_s * 1.05
        return monotonic and fits

    strict = [parse_hhmmss(t) for t in ts]
    if ok(strict) and all(all(x <= 59 for x in fields(t)[1:]) for t in ts):
        return strict
    alt = [fields(t)[0] * 60 + fields(t)[1] if len(fields(t)) == 3 else parse_hhmmss(t)
           for t in ts]
    if ok(alt):
        return alt
    return strict  # unfixable — caller's coverage check flags it


# --- Stage B (ARCHITECTURE.md §3) ---

STAGE_B_SCHEMA = {
    "type": "OBJECT",
    "properties": {
        "segment_start": {"type": "STRING"},
        "language": {"type": "STRING"},
        "blocks": {
            "type": "ARRAY",
            "items": {
                "type": "OBJECT",
                "properties": {
                    "t": {"type": "STRING"},
                    "spoken": {"type": "STRING", "nullable": True},
                    "speaker": {"type": "STRING", "nullable": True},
                    "on_screen_text": {"type": "STRING", "nullable": True},
                    "visual": {"type": "STRING"},
                    "kind": {"type": "STRING", "enum": [
                        "slide", "demo", "talking_head", "screen_share",
                        "whiteboard", "other"]},
                },
                "required": ["t", "spoken", "speaker", "on_screen_text", "visual", "kind"],
            },
        },
        "segment_summary": {"type": "STRING"},
    },
    "required": ["segment_start", "language", "blocks", "segment_summary"],
}

STAGE_B_PROMPT = """\
You are analyzing one segment of a longer video for a searchable knowledge base.
This segment starts at {segment_start} in the full video.

Break the segment into blocks: start a new block whenever the content meaningfully changes
(new slide, new screen, new topic, new demonstrated action). Typical block length 20-90 s.

Rules:
- "t": timestamp of the block start WITHIN THIS CLIP, as hh:mm:ss with two-digit fields,
  e.g. "00:03:45" (00:00:00 = clip start). For clips under one hour the hours field stays
  "00". Never use frames or fractions of a second.
- "spoken": cleaned transcript of what is actually said during the block. If nothing is
  spoken, use an empty string. NEVER invent or paraphrase speech that is not audible.
- "speaker": the speaker's name only if stated or visible; otherwise "Speaker 1",
  "Speaker 2", ... consistently; null if there is no speech.
- "on_screen_text": the important text visible on screen, VERBATIM (UI labels, headings,
  code, slide text). Do not correct spelling. Empty string if none.
- "visual": one or two sentences describing what is shown or demonstrated.
- "kind": one of slide | demo | talking_head | screen_share | whiteboard | other.
- "segment_start": echo the value "{segment_start}".
- "segment_summary": 2-4 sentences summarizing this segment.
- COVERAGE IS MANDATORY: the blocks together must cover the ENTIRE clip from 00:00:00 to
  the very end, with no gaps. Do not stop after the first block. A 10-15 minute clip
  typically yields 10-25 blocks.
"""


def build_stage_b_body(gcs_uri: str, segment_start: str, fps: float | None = None,
                       media_resolution: str | None = None) -> dict:
    video_part = {"fileData": {"mimeType": "video/mp4", "fileUri": gcs_uri}}
    if fps is not None:
        video_part["videoMetadata"] = {"fps": fps}
    gen_config = {
        "temperature": 0.2,
        "maxOutputTokens": 65535,
        # unbounded dynamic thinking ran away on one call (63k thought tokens, truncated
        # answer, 3x cost) — bound it; extraction needs little deliberation
        "thinkingConfig": {"thinkingBudget": 4096},
        "responseMimeType": "application/json",
        "responseSchema": STAGE_B_SCHEMA,
    }
    if media_resolution is not None:
        gen_config["mediaResolution"] = media_resolution  # e.g. MEDIA_RESOLUTION_LOW
    return {
        "contents": [{
            "role": "user",
            "parts": [
                video_part,
                {"text": STAGE_B_PROMPT.format(segment_start=segment_start)},
            ],
        }],
        "generationConfig": gen_config,
    }
