# vectorreel — brand asset generation

Design-asset spike. Generates logo/site graphics with **Vertex AI (Gemini image models)** via ADC.

⚠️ `experiments/**` is never authoritative (CLAUDE.md). This is a point-in-time exploration,
not a brand spec. Nothing here is deployed, and no asset here is a chosen brand — the images in
`assets/` are committed only so the candidates survive without re-paying for Vertex calls.

---

## Region & model: what's actually available

The repo default `europe-west3` **serves no image model at all**. Probed every EU region plus
`us-central1` against Imagen and the Gemini image models:

| Model | europe-west3 | europe-west4 | us-central1 | global |
|---|---|---|---|---|
| `imagen-*` (3.0, 4.0, 4.0-fast) | 404 | 404 | **404** | — |
| `gemini-2.5-flash-image` | 404 | ✅ | ✅ | ✅ |
| `gemini-3-pro-image-preview` | 404 | 404 | 404 | ✅ |

Two things worth knowing:

- **Imagen 404s in `us-central1` too**, so this is *not* an EU restriction — the project simply
  has no Imagen access. Switching regions would not have fixed it.
- **`gemini-2.5-flash-image` runs in `europe-west4` (Netherlands)** — EU-resident, so the default
  config honours the EU-only rule (CLAUDE.md rule 2) with no exception needed.

**Default: `gemini-2.5-flash-image` @ `europe-west4`.**

`gemini-3-pro-image-preview` is noticeably better at rendering legible text, but is **`global`-routed
only — i.e. NOT EU-resident**. It is opt-in, never the default, and the script prints a `!! NON-EU`
warning when you select it:

```bash
python generate_assets.py logo_horizontal --model gemini-3-pro-image-preview --suffix _pro
```

---

## Auth

ADC only. No key files, ever (CLAUDE.md rule 1).

```bash
gcloud auth application-default login
gcloud auth application-default set-quota-project tensile-runway-442915-j6
```

## Run

```bash
python -m venv .venv
.venv/Scripts/python -m pip install google-genai pillow    # Windows
# .venv/bin/pip install google-genai pillow                # POSIX

python generate_assets.py            # all assets
python generate_assets.py --list     # asset keys
python generate_assets.py logo_mark  # regenerate ONE asset
```

Config via env (both spellings accepted):

| Var | Default |
|---|---|
| `VECTORREEL_PROJECT` / `VERTEXREEL_PROJECT` | `tensile-runway-442915-j6` |
| `VECTORREEL_LOCATION` / `VERTEXREEL_LOCATION` | `europe-west4` |
| `VECTORREEL_MODEL` | `gemini-2.5-flash-image` |

Useful flags:

- `--suffix _v2` — write `logo_mark_v2.png` instead of overwriting, to compare variants.
- `--rework` — re-run *post-processing only* on existing PNGs. **No API call, no cost.** Use this
  to fix an alpha cut without re-rolling an image you like (re-rolling the wordmark risks losing a
  correctly-spelled `vectorreel`).

## Cost metering (rule 6)

Every call prints real token counts from `response.usage_metadata`, plus a run-total ledger.
**Token counts are real; the $/1M rate card in `USD_PER_1M` is a local estimate** — verify against
Google's pricing page before quoting it anywhere. Observed: a flat ~1290 output tokens per image.

## Palette

| | Hex | Role |
|---|---|---|
| Ink | `#0B1020` | near-black indigo; dark surfaces, wordmark |
| Signal | `#4F6BFF` | primary indigo-blue; trust/infra, not playful |
| Vector | `#2DD4BF` | restrained teal; accent only — precision, structured output |

---

## Two things the image model cannot do

**1. No alpha channel.** Gemini image models cannot emit transparency. Logos are generated on flat
white and cut to alpha here, in `knockout_white()`.

That function flood-fills the background inward **from the image border** rather than thresholding
on luminance globally. The distinction is load-bearing — the first version thresholded globally and
did two things wrong: it punched a hole clean through the mark's white *interior*, and it left an
alpha-15 residue everywhere the model's "white" background was really 253–254 rather than a true
255. Only light pixels reachable from the edge are background; enclosed light pixels are artwork.

**2. Exact pixel sizes.** Only fixed aspect ratios are supported, so the OG card is generated 16:9
and center-cropped to exactly 1200×630 in `crop_to()`.

## Known limitations of the generated marks — read before using these

- **The mark is NOT monochrome-safe.** Its play triangle and text bars are formed by the *white
  interior* of the frame, not by the silhouette. Fill it as one flat colour and it collapses into a
  solid rounded-square blob (see `assets/_favicon_check.png`, top-right). The spec asked for
  monochrome-friendly; **this does not meet it.**
- **16px is marginal.** It reads from ~32px up. `assets/favicon.ico` is generated anyway, multi-size.
- **A raster logo is not a production logo.** Real logos are vector. Treat everything in `assets/` as
  *design direction to hand off or trace into SVG*, not as a shippable asset. The 429 quota on
  `europe-west4` is also tight — expect `RESOURCE_EXHAUSTED` retries on a full run.

## Vectors (`svg/`)

Raster PNGs are not shippable logos. `svg/` holds the vector set, produced two different ways
**on purpose**:

### Hand-authored — the logo (`make_lockup.py`, plus the marks written directly)

The mark is four primitives (rounded rect, triangle, three bars), so it is authored as exact
geometry rather than traced. Tracing a logo bakes the raster's wobbly anti-aliased edges into
hundreds of bézier points: a vector-shaped copy of an imprecise drawing. **Don't trace logos.**

This also **fixes the monochrome failure** described above, by construction. The frame is a
*stroked* rect with no fill, so the interior is a true hole and the counters are formed by
geometry, not by white paint. Fill every shape one flat colour and it still reads — verified in
`assets/_svg_check.png` (ink-on-white and white-on-dark rows).

| File | Notes |
|---|---|
| `logo-mark.svg` | Primary mark, brand colours |
| `logo-mark-mono.svg` | `fill="currentColor"` — inherits CSS `color`, one colour, works anywhere |
| `logo-horizontal.svg` | Mark + wordmark lockup |
| `logo-horizontal-mono.svg` | Same, `currentColor` |
| `favicon.svg` | **Redrawn, not scaled down** — two bars, fatter strokes, tighter margins. Three bars mush at 16px. Inverts on dark browser chrome via `prefers-color-scheme`. |

The wordmark is **outlined to paths**, not an SVG `<text>` element. A `<text>` logo silently
re-renders in a fallback font on any machine without Inter — which is how wordmarks break in
production. Outlined, it's geometry: identical everywhere, no webfont dependency.

`make_lockup.py` downloads Inter Medium to `.fonts/` (gitignored) and outlines it via fontTools.
Change the typeface or tracking there and re-run.

### Auto-traced — the illustrations (`trace_illustrations.py`)

Hero, OG and the three spots are dozens of flat colour regions with no precision requirement, so
`vtracer` is the right tool. Output is 2–8 KB each (vs 150–720 KB PNG) and scales losslessly.

```bash
python trace_illustrations.py
```

Tuned for flat art: `mode="polygon"` (straight edges, not splines) and `filter_speckle=8`. Note
that speckle filtering **drops the hero's faint scattered background dots** — arguably cleaner,
but it is a real difference from the PNG. Lower it to keep them.

### Rendering / exporting

`resvg` (pure-Python wheel; `cairosvg` needs native Cairo, which isn't available here) renders SVG
back to PNG at any size — that's how `assets/logo-*@*.png` and the multi-size `assets/favicon.ico` are
produced, crisply, straight from the vector.

⚠️ **Unverified:** resvg ignores `@media (prefers-color-scheme)`, so the favicon's dark-mode
inversion could not be confirmed locally. It is a standard, well-supported browser feature —
but check it in a real browser before trusting it.

## Regenerating one asset

```bash
python generate_assets.py hero_background            # re-roll it
python generate_assets.py logo_mark --suffix _v3     # keep the old one, add a variant
python generate_assets.py logo_horizontal --rework   # fix alpha only, free
```

Prompts live in the `ASSETS` dict in `generate_assets.py` — one entry per asset, each carrying its
style, palette, background and negative guidance. Edit the prompt, re-run that one key.
