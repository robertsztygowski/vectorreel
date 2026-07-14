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
# Cumulative cap over every phase this ledger has carried: Phase 0 (EUR 5) + Phase 0.1
# (EUR 1) + Phase 0.2 (EUR 3) = EUR 9. Per-phase caps are set with spike_budget().
BUDGET_USD = 10.5

# Vertex tokenizes video at a fixed rate, independent of content (Phase 0.1, out/YOUTUBE.md).
# This is what makes a video's TRUE duration free: it falls out of the bill.
VIDEO_TOKENS_PER_S = {None: 258, "MEDIA_RESOLUTION_LOW": 66, "MEDIA_RESOLUTION_MEDIUM": 258}


def fetched_duration_s(usage: dict, media_resolution: str | None = None) -> float:
    """How much video Vertex ACTUALLY fetched, in seconds.

    🚨 The coverage guard must divide by this, never by the requested window. Vertex clamps
    an endOffset to the end of the video, so on any video shorter than the window the guard
    sees false under-generation, retries, and double-bills — observed on a 59 s video in
    Phase 0.1 (out/YOUTUBE.md, pathology 1). On the YouTube path the true length is not
    known up front, so it must be read back off the bill."""
    return modality_tokens(usage)["VIDEO"] / VIDEO_TOKENS_PER_S[media_resolution]

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


_spike = {"name": None, "start": 0.0, "cap": 0.0}


def spike_budget(name: str, cap_usd: float) -> None:
    """Cap the spend of one PHASE on top of whatever the ledger already holds.

    The anchor is persisted, so the cap is cumulative across every process in the phase.
    Phase 0.1 got away with an in-memory anchor because it ran in one shot; Phase 0.2 runs
    in three passes, and a per-process anchor would silently hand each pass a fresh full
    budget — a cap that resets is not a cap."""
    anchor = OUT / f"spike_{name}.json"
    if anchor.exists():
        start = json.loads(anchor.read_text(encoding="utf-8"))["start"]
    else:
        start = ledger_total()
        anchor.write_text(json.dumps({"start": start, "cap": cap_usd}), encoding="utf-8")
    _spike.update(name=name, start=start, cap=cap_usd)
    print(f"[{name}] anchored at ${start:.4f}; phase has spent ${spike_spent():.4f} "
          f"of ${cap_usd:.2f}")


def spike_spent() -> float:
    return ledger_total() - _spike["start"] if _spike["name"] else 0.0


def gemini_call(body: dict, label: str, config: str = "", model: str = MODEL,
                region: str = REGION, api_version: str = "v1",
                max_retries: int = 5) -> dict:
    """generateContent with backoff + ledger. Returns the full response JSON."""
    total = ledger_total()
    if total > BUDGET_USD:
        raise RuntimeError(
            f"Ledger at ${total:.2f} > budget ${BUDGET_USD} — stop and ask the founder.")
    if _spike["name"] and spike_spent() > _spike["cap"]:
        raise RuntimeError(
            f"[{_spike['name']}] spent ${spike_spent():.4f} > cap ${_spike['cap']:.2f} "
            f"— stop and ask the founder.")
    url = (f"https://{region}-aiplatform.googleapis.com/{api_version}/projects/{PROJECT}"
           f"/locations/{region}/publishers/google/models/{model}:generateContent")
    for attempt in range(max_retries):
        t0 = time.time()
        try:
            resp = requests.post(url, json=body, timeout=1800,
                                 headers={"Authorization": f"Bearer {gcloud_token()}"})
        except requests.exceptions.RequestException as e:
            # A dropped connection is the most transient failure there is, and it used to be
            # the ONLY one not retried: the backoff below only ever looked at status codes,
            # so a mid-run TCP reset killed a whole multi-segment render. Long video calls
            # hold a connection open for minutes — this will happen again.
            if attempt >= max_retries - 1:
                raise
            wait = 15 * 2 ** attempt
            print(f"  {label}: connection error, retry in {wait}s — {str(e)[:120]}")
            time.sleep(wait)
            continue
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


def count_tokens(body: dict, model: str = MODEL, region: str = REGION,
                 api_version: str = "v1") -> dict:
    """countTokens — FREE, so no ledger row. Returns {"ok": bool, ...}.

    The workhorse of the Phase 0.1 spike: both "does this region accept a YouTube
    fileUri" and "are videoMetadata offsets honoured" are answerable from the
    per-modality token counts alone, without spending anything.
    """
    url = (f"https://{region}-aiplatform.googleapis.com/{api_version}/projects/{PROJECT}"
           f"/locations/{region}/publishers/google/models/{model}:countTokens")
    payload = {"contents": body["contents"]}
    resp = requests.post(url, json=payload, timeout=300,
                         headers={"Authorization": f"Bearer {gcloud_token()}"})
    if resp.status_code != 200:
        return {"ok": False, "status": resp.status_code, "error": resp.text[:500]}
    data = resp.json()
    m = modality_tokens(data)
    return {"ok": True, "total": data.get("totalTokens", 0),
            "video": m["VIDEO"], "audio": m["AUDIO"], "text": m["TEXT"]}


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
    """Model timestamps drift in format (mm:ss:centiseconds instead of hh:mm:ss).

    This used to decide the format ONCE for the whole response, which fails on the way the
    drift actually happens: the model MIXES the two formats within a single answer. Phase
    0.2 saw "00:14:87" (87 seconds — impossible) sitting beside genuine hh:mm:ss values, so
    neither whole-sequence reading was monotonic, the all-or-nothing detector fell back to
    the strict one, and a 15-minute clip came back with blocks at 04:48:10.

    So decide per timestamp, and let monotonicity choose: take the first reading that is
    (a) internally valid, (b) not before the previous block, and (c) inside the clip.
    A timestamp whose minute/second field exceeds 59 cannot be hh:mm:ss, whatever it fits."""
    def fields(t):
        return [int(p) for p in t.strip().split(":")]

    out: list[int] = []
    prev = -1
    limit = clip_dur_s * 1.05 if clip_dur_s else None
    for t in ts:
        f = fields(t)
        cands = []
        if all(x <= 59 for x in f[1:]):          # only then can it be hh:mm:ss
            cands.append(parse_hhmmss(t))
        if len(f) == 3:
            cands.append(f[0] * 60 + f[1])       # mm:ss:centiseconds -> drop the cs
        if not cands:
            cands = [parse_hhmmss(t)]
        pick = next((c for c in cands
                     if c >= prev and (limit is None or c <= limit)), None)
        if pick is None:  # unfixable — the caller's coverage guard flags it
            pick = min(cands, key=lambda c: abs(c - prev))
        out.append(pick)
        prev = pick
    return out


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


def build_stage_b_body(file_uri: str, segment_start: str, fps: float | None = None,
                       media_resolution: str | None = None,
                       start_offset_s: int | None = None,
                       end_offset_s: int | None = None) -> dict:
    """Stage B request. file_uri is a gs:// segment (private path) or a YouTube watch
    URL (public path — Google fetches it, we never hold the bytes; CLAUDE.md rule 8).
    On the YouTube path there is no ffmpeg, so segmentation is server-side via
    videoMetadata offsets instead of a cut file (ARCHITECTURE.md §1)."""
    # Vertex rejects fileData without a mimeType (400 INVALID_ARGUMENT) — including for
    # YouTube watch URLs, where it is a formality: Google fetches and decides the format.
    video_part = {"fileData": {"mimeType": "video/mp4", "fileUri": file_uri}}
    meta = {}
    if fps is not None:
        meta["fps"] = fps
    if start_offset_s is not None:
        meta["startOffset"] = f"{start_offset_s}s"
    if end_offset_s is not None:
        meta["endOffset"] = f"{end_offset_s}s"
    if meta:
        video_part["videoMetadata"] = meta
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
