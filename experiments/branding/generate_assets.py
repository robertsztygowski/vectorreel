#!/usr/bin/env python3
"""
vectorreel — brand asset generation via Vertex AI (Gemini image models).

Auth:     Application Default Credentials only. No key files, ever.
              gcloud auth application-default login
Run:      python generate_assets.py            # all assets
          python generate_assets.py logo_mark  # just one
          python generate_assets.py --list

See README.md for the region/model story (short version: europe-west3 has NO image
models; europe-west4 is the closest EU-compliant region that does).
"""

from __future__ import annotations

import argparse
import io
import os
import sys
import time
from dataclasses import dataclass, field

from google import genai
from google.genai import types
from PIL import Image

# --------------------------------------------------------------------------------------
# Config
# --------------------------------------------------------------------------------------

OUT_DIR = os.path.join(os.path.dirname(os.path.abspath(__file__)), "out")

# The task spec named VERTEXREEL_*; the product is "vectorreel". Accept both so neither
# spelling silently falls back to the default and looks like it was ignored.
def _env(*names: str, default: str) -> str:
    for n in names:
        v = os.environ.get(n)
        if v:
            return v
    return default


PROJECT = _env("VECTORREEL_PROJECT", "VERTEXREEL_PROJECT", default="tensile-runway-442915-j6")

# europe-west4 (Netherlands), NOT europe-west3: europe-west3 serves no image model at all.
# Verified by probing every EU region -- see README.
LOCATION = _env("VECTORREEL_LOCATION", "VERTEXREEL_LOCATION", default="europe-west4")

# EU-resident default. Override with VECTORREEL_MODEL=gemini-3-pro-image-preview, which is
# stronger at rendering legible text but is `global`-routed (NON-EU) -- an explicit choice.
MODEL = _env("VECTORREEL_MODEL", default="gemini-2.5-flash-image")

# Models that only serve from the multi-region `global` endpoint (i.e. not EU-resident).
GLOBAL_ONLY_MODELS = {"gemini-3-pro-image-preview"}

# --------------------------------------------------------------------------------------
# Cost metering (rule 6: every call records cost)
#
# Token counts printed below are REAL -- read from response.usage_metadata.
# The $/1M rates are a local rate card and are the only estimated part. Verify against
# https://cloud.google.com/vertex-ai/generative-ai/pricing before quoting these anywhere.
# --------------------------------------------------------------------------------------

USD_PER_1M = {
    # model: (input_text, output_incl_image)
    "gemini-2.5-flash-image": (0.30, 30.00),
    "gemini-3-pro-image-preview": (2.00, 120.00),
}


@dataclass
class Spend:
    calls: int = 0
    in_tok: int = 0
    out_tok: int = 0
    usd: float = 0.0

    def add(self, model: str, usage) -> tuple[int, int, float]:
        i = getattr(usage, "prompt_token_count", 0) or 0
        o = (getattr(usage, "candidates_token_count", 0) or 0) + (
            getattr(usage, "thoughts_token_count", 0) or 0
        )
        r_in, r_out = USD_PER_1M.get(model, (0.0, 0.0))
        usd = (i / 1e6) * r_in + (o / 1e6) * r_out
        self.calls += 1
        self.in_tok += i
        self.out_tok += o
        self.usd += usd
        return i, o, usd


LEDGER = Spend()

# --------------------------------------------------------------------------------------
# Brand
# --------------------------------------------------------------------------------------

# Proposed palette -- trustworthy EU-tech, Linear/Vercel/Stripe register.
INK = "#0B1020"     # near-black indigo. Primary surface / mark on light.
SIGNAL = "#4F6BFF"  # indigo-blue. Primary brand. Reads as trust/infra, not playful.
VECTOR = "#2DD4BF"  # restrained teal. Accent only -- precision, "structured output".

PALETTE = (
    f"Strict 3-colour palette ONLY: deep near-black indigo {INK}, "
    f"primary indigo-blue {SIGNAL}, and a restrained teal accent {VECTOR} used sparingly. "
    "No other hues. No gradients other than subtle flat tonal steps."
)

STYLE = (
    "Flat vector, geometric, minimal, high precision. Crisp straight edges and exact "
    "right angles. Generous negative space. Developer-tool brand aesthetic in the register "
    "of Linear, Vercel and Stripe: restrained, technical, trustworthy, grown-up."
)

# Appended to every prompt. Image models cannot take a true negative-prompt parameter on
# generateContent, so this is phrased as positive constraints, which they follow better.
NEGATIVE = (
    "Hard constraints: absolutely NO lettering, NO words, NO numbers, NO glyphs and NO "
    "UI text of any kind anywhere in the image (garbled pseudo-text is the single worst "
    "failure mode here) unless the prompt explicitly asks for a word. "
    "NO photorealism, NO 3D render, NO drop shadows, NO bevels, NO glossy highlights, "
    "NO gradients meshes, NO lens flare. "
    "NO cartoon characters, NO mascots, NO hand-drawn or sketchy lines, NO clutter, "
    "NO busy background patterns, NO stock-illustration people. "
    "Clean, sparse, deliberate."
)


@dataclass
class Asset:
    key: str
    prompt: str
    aspect: str = "1:1"
    # Post-processing
    knockout: bool = False              # cut near-white bg to transparent alpha
    exact_size: tuple[int, int] | None = None  # center-crop + resize to exact px
    notes: str = ""
    model: str | None = None            # per-asset model override
    extra: dict = field(default_factory=dict)


ASSETS: dict[str, Asset] = {
    # ---------------------------------------------------------------------------------
    "logo_mark": Asset(
        key="logo_mark",
        aspect="1:1",
        knockout=True,
        notes="Primary mark. Favicon-safe, monochrome-friendly.",
        prompt=(
            "A single abstract geometric logo mark for a developer infrastructure company. "
            "The mark fuses two ideas into ONE compact glyph: a film/video frame and a "
            "stack of structured text lines. Concretely: a bold rounded-square bracket or "
            "aperture form, and inside it three horizontal bars of decreasing length that "
            "read simultaneously as lines of text and as a bar-code/waveform. "
            "Perfectly symmetrical, centred, built on a strict geometric grid, thick "
            "confident strokes of uniform weight. "
            "It must survive being scaled down to a 16x16 favicon: no thin lines, no fine "
            "detail, at most 4 shapes, and it must still read when filled in one flat "
            "colour. "
            f"{STYLE} {PALETTE} The mark itself is {INK} and {SIGNAL} with one small {VECTOR} "
            "accent bar. "
            "Pure flat WHITE background, no shadow, mark floating centred with wide margin. "
            f"{NEGATIVE}"
        ),
    ),
    # ---------------------------------------------------------------------------------
    "logo_horizontal": Asset(
        key="logo_horizontal",
        aspect="21:9",
        knockout=True,
        notes='Horizontal lockup with wordmark "vectorreel".',
        prompt=(
            "A horizontal logo lockup for a software company, on a pure flat WHITE "
            "background. "
            "LEFT: a compact abstract geometric mark -- a rounded-square aperture "
            "containing three horizontal bars of decreasing length that read as both text "
            "lines and a waveform. "
            "RIGHT of it, vertically centred, the single lowercase wordmark spelled exactly: "
            "vectorreel "
            "Spelled v-e-c-t-o-r-r-e-e-l, one word, all lowercase, no space, no tagline, no "
            "other text anywhere. "
            "Set in a clean modern geometric sans-serif (in the register of Inter or "
            "GT America), medium weight, slightly tightened letter-spacing, perfectly "
            "level baseline, crisp and legible. "
            "The mark and the wordmark are optically balanced with clear space between them. "
            f"{STYLE} {PALETTE} Wordmark in {INK}; mark in {SIGNAL} with one {VECTOR} accent. "
            "Flat white background, no shadow, no container, no border. "
            "NO photorealism, NO 3D, NO drop shadows, NO clutter, NO mascots. "
            "The ONLY text permitted in the image is the exact word 'vectorreel'."
        ),
    ),
    # ---------------------------------------------------------------------------------
    "hero_background": Asset(
        key="hero_background",
        aspect="16:9",
        notes="Website hero bg. Dark. Video frames resolving into structured text.",
        prompt=(
            "An abstract wide hero background graphic for a technical SaaS landing page. "
            "Dark canvas. "
            "Composition, left to right: a sequence of empty rounded-rectangle video frames "
            "on a horizontal timeline rail, which progressively dissolve and re-form into "
            "neat stacked horizontal bars representing structured lines of text. "
            "Beneath the rail, small evenly-spaced tick marks like a timeline scrubber. "
            "Fine thin connector lines link the frames to the text bars, implying extraction "
            "and mapping. "
            "Sparse scattered small dots suggesting a vector embedding space in the far "
            "background, very subtle. "
            "Strong horizontal rhythm, lots of empty space, content weighted to the lower "
            "two-thirds so a headline can sit on top. "
            f"{STYLE} {PALETTE} Background is deep {INK}. Shapes in {SIGNAL} with sparing "
            f"{VECTOR} accents on a few highlighted elements. Low contrast, calm, no visual "
            "shouting -- this sits BEHIND text. "
            f"{NEGATIVE}"
        ),
    ),
    # ---------------------------------------------------------------------------------
    "feature_upload": Asset(
        key="feature_upload",
        aspect="1:1",
        notes="Spot 1 — upload / ingest video.",
        prompt=(
            "A small square abstract spot illustration, flat geometric icon-illustration, "
            "for the concept 'upload a video'. "
            "A single rounded-rectangle video frame with a play triangle, sitting above a "
            "horizontal base rail, with an upward arrow or upward-flowing chevrons moving "
            "the frame into a rounded container. "
            "One clear idea, at most 5 shapes, centred, generous margin. "
            f"{STYLE} {PALETTE} "
            "Light off-white background. Shapes in "
            f"{INK} and {SIGNAL}, one {VECTOR} accent. "
            f"{NEGATIVE}"
        ),
    ),
    "feature_markdown": Asset(
        key="feature_markdown",
        aspect="1:1",
        notes="Spot 2 — timestamped Markdown output.",
        prompt=(
            "A small square abstract spot illustration, flat geometric icon-illustration, "
            "for the concept 'timestamped structured document'. "
            "A rounded-rectangle document card containing several horizontal bars of varying "
            "length representing lines of text, arranged as a heading bar plus body bars. "
            "Down the LEFT edge of the card, a vertical column of small uniform pill-shaped "
            "chips representing timestamps, each aligned to a line. "
            "The chips are blank -- shapes only, no digits. "
            "One clear idea, centred, generous margin. "
            f"{STYLE} {PALETTE} "
            "Light off-white background. Card and bars in "
            f"{INK} and {SIGNAL}; the timestamp chips in {VECTOR}. "
            f"{NEGATIVE}"
        ),
    ),
    "feature_knowledge": Asset(
        key="feature_knowledge",
        aspect="1:1",
        notes="Spot 3 — plug into knowledge base / vector store.",
        prompt=(
            "A small square abstract spot illustration, flat geometric icon-illustration, "
            "for the concept 'feed a document into a vector knowledge base'. "
            "On one side a small document card of stacked horizontal bars; thin connector "
            "lines flow from it into a structured lattice or grid of evenly-spaced nodes and "
            "edges representing a vector index / embedding space. "
            "The lattice is orderly and geometric, not organic, not a messy web. "
            "One clear idea, centred, generous margin. "
            f"{STYLE} {PALETTE} "
            "Light off-white background. Document in "
            f"{INK}; lattice nodes in {SIGNAL} with a few {VECTOR} highlighted nodes. "
            f"{NEGATIVE}"
        ),
    ),
    # ---------------------------------------------------------------------------------
    "og_share": Asset(
        key="og_share",
        aspect="16:9",
        exact_size=(1200, 630),
        notes="Social/OG card, cropped to exactly 1200x630.",
        prompt=(
            "A wide abstract social share card background for a developer tool, dark theme. "
            "Left two-thirds deliberately EMPTY dark space reserved for a headline to be "
            "overlaid later. "
            "Right third: an abstract motif of video frames on a timeline rail transforming "
            "into stacked horizontal bars of structured text, with fine connector lines and "
            "small timeline tick marks. "
            "Balanced, calm, premium, lots of negative space. "
            f"{STYLE} {PALETTE} Deep {INK} background, motif in {SIGNAL} with sparing "
            f"{VECTOR} accents. "
            f"{NEGATIVE}"
        ),
    ),
}

# --------------------------------------------------------------------------------------
# Post-processing
# --------------------------------------------------------------------------------------


def knockout_white(img: Image.Image, thresh: int = 238) -> Image.Image:
    """Cut a near-white background to transparent alpha.

    Gemini image models cannot emit an alpha channel, so a 'transparent' logo has to be
    made here. Feathered on luminance so edges stay smooth rather than jagged.
    """
    img = img.convert("RGBA")
    px = img.load()
    w, h = img.size
    for y in range(h):
        for x in range(w):
            r, g, b, _ = px[x, y]
            lum = (r * 299 + g * 587 + b * 114) // 1000
            if lum >= 255:
                px[x, y] = (r, g, b, 0)
            elif lum > thresh:
                # feather: ramp alpha across the near-white band
                a = int(255 * (255 - lum) / max(1, 255 - thresh))
                px[x, y] = (r, g, b, a)
    return img


def crop_to(img: Image.Image, size: tuple[int, int]) -> Image.Image:
    """Center-crop to the target aspect, then resize to exact pixels."""
    tw, th = size
    target = tw / th
    w, h = img.size
    cur = w / h
    if cur > target:  # too wide -> trim sides
        nw = int(h * target)
        img = img.crop(((w - nw) // 2, 0, (w - nw) // 2 + nw, h))
    elif cur < target:  # too tall -> trim top/bottom
        nh = int(w / target)
        img = img.crop((0, (h - nh) // 2, w, (h - nh) // 2 + nh))
    return img.resize((tw, th), Image.LANCZOS)


# --------------------------------------------------------------------------------------
# Generation
# --------------------------------------------------------------------------------------


def make_client(model: str) -> tuple[genai.Client, str]:
    """Vertex client via ADC. Returns (client, effective_location)."""
    loc = "global" if model in GLOBAL_ONLY_MODELS else LOCATION
    client = genai.Client(vertexai=True, project=PROJECT, location=loc)
    return client, loc


def generate(asset: Asset, retries: int = 2) -> str | None:
    model = asset.model or MODEL
    client, loc = make_client(model)

    cfg = types.GenerateContentConfig(
        response_modalities=["IMAGE"],
        image_config=types.ImageConfig(aspect_ratio=asset.aspect),
    )

    eu = "EU" if loc != "global" else "NON-EU"
    print(f"\n[{asset.key}] {model} @ {loc} ({eu})  aspect={asset.aspect}")

    for attempt in range(1, retries + 2):
        try:
            t0 = time.time()
            resp = client.models.generate_content(
                model=model, contents=asset.prompt, config=cfg
            )
            dt = time.time() - t0

            data = None
            for part in resp.candidates[0].content.parts:
                if getattr(part, "inline_data", None) and part.inline_data.data:
                    data = part.inline_data.data
                    break
            if not data:
                raise RuntimeError("no image part in response")

            i, o, usd = LEDGER.add(model, resp.usage_metadata)

            img = Image.open(io.BytesIO(data))
            if asset.knockout:
                img = knockout_white(img)
            if asset.exact_size:
                img = crop_to(img, asset.exact_size)

            path = os.path.join(OUT_DIR, f"{asset.key}.png")
            img.save(path)

            print(
                f"  ok  {img.size[0]}x{img.size[1]}  {dt:4.1f}s  "
                f"in={i} out={o} tok  ~${usd:.4f}"
                + ("  [alpha]" if asset.knockout else "")
            )
            return path

        except Exception as e:  # noqa: BLE001
            msg = str(e).replace("\n", " ")[:180]
            if attempt <= retries:
                print(f"  retry {attempt}/{retries}: {msg}")
                time.sleep(2 * attempt)
            else:
                print(f"  FAILED: {msg}")
                return None
    return None


def main() -> int:
    ap = argparse.ArgumentParser(description="Generate vectorreel brand assets.")
    ap.add_argument("keys", nargs="*", help="asset keys to generate (default: all)")
    ap.add_argument("--list", action="store_true", help="list asset keys and exit")
    ap.add_argument("--model", help="override model for this run")
    ap.add_argument("--suffix", default="", help="append to output filename (for variants)")
    args = ap.parse_args()

    if args.list:
        for k, a in ASSETS.items():
            print(f"{k:20} {a.aspect:6} {a.notes}")
        return 0

    global MODEL
    if args.model:
        MODEL = args.model

    keys = args.keys or list(ASSETS)
    unknown = [k for k in keys if k not in ASSETS]
    if unknown:
        print(f"unknown asset(s): {', '.join(unknown)}\ntry --list", file=sys.stderr)
        return 2

    os.makedirs(OUT_DIR, exist_ok=True)

    loc = "global" if MODEL in GLOBAL_ONLY_MODELS else LOCATION
    print(f"project={PROJECT}  location={loc}  model={MODEL}")
    if loc == "global":
        print("  !! NON-EU: `global` is a multi-region endpoint. EU default is "
              "europe-west4 + gemini-2.5-flash-image.")

    made = []
    for k in keys:
        a = ASSETS[k]
        if args.suffix:
            a = Asset(**{**a.__dict__, "key": f"{a.key}{args.suffix}"})
        p = generate(a)
        if p:
            made.append(p)

    print(
        f"\n{'-' * 70}\n"
        f"ledger: {LEDGER.calls} call(s)  "
        f"in={LEDGER.in_tok} out={LEDGER.out_tok} tok  "
        f"~${LEDGER.usd:.4f} total  (rate card is an estimate; token counts are real)\n"
        f"wrote {len(made)}/{len(keys)} -> {OUT_DIR}"
    )
    return 0 if len(made) == len(keys) else 1


if __name__ == "__main__":
    raise SystemExit(main())
