#!/usr/bin/env python3
"""
Build the horizontal vectorreel lockup as pure SVG geometry.

The wordmark is converted to OUTLINES (path data), not an SVG <text> element. A <text>
logo silently re-renders in a fallback font on any machine without Inter installed --
which is how wordmarks break in the wild. Outlined, it is geometry and renders identically
everywhere, with no webfont dependency.

    python make_lockup.py
"""

from __future__ import annotations

import os

from fontTools.pens.svgPathPen import SVGPathPen
from fontTools.ttLib import TTFont

HERE = os.path.dirname(os.path.abspath(__file__))
FONT = os.path.join(HERE, ".fonts", "Inter-Medium.ttf")
SVG_DIR = os.path.join(HERE, "svg")

INK = "#0B1020"
SIGNAL = "#4F6BFF"
VECTOR = "#2DD4BF"

WORD = "vectorreel"
TRACKING = -0.012  # em. Slight negative tracking; geometric sans lockups want it tight.


def outline(word: str) -> tuple[str, float, float, float]:
    """Return (path_d_in_font_units, advance_width, units_per_em, x_height)."""
    font = TTFont(FONT)
    upem = font["head"].unitsPerEm
    xheight = getattr(font["OS/2"], "sxHeight", None) or upem * 0.52
    cmap = font.getBestCmap()
    glyphs = font.getGlyphSet()
    hmtx = font["hmtx"]

    d: list[str] = []
    x = 0.0
    track = TRACKING * upem
    for ch in word:
        name = cmap[ord(ch)]
        pen = SVGPathPen(glyphs)
        glyphs[name].draw(pen)
        p = pen.getCommands()
        if p:
            d.append(f'<path transform="translate({x:.1f} 0)" d="{p}"/>')
        x += hmtx[name][0] + track
    return "\n    ".join(d), x - track, upem, xheight


def build(mono: bool) -> str:
    d, adv, upem, xheight = outline(WORD)

    # Mark occupies a 64x64 box. Scale the word so its x-height is ~22/64 of that box --
    # optically matched to the mark's internal bar stack rather than to its outer frame.
    mark = 64.0
    target_xheight = 22.0
    s = target_xheight / xheight  # font units -> px
    word_w = adv * s
    gap = 18.0

    # Baseline sits so the x-height band is vertically centred on the mark's centre.
    baseline_y = mark / 2 + target_xheight / 2

    w = mark + gap + word_w
    h = mark
    tx = mark + gap

    ink = "currentColor" if mono else INK
    sig = "currentColor" if mono else SIGNAL
    vec = "currentColor" if mono else VECTOR
    root_fill = ' fill="currentColor"' if mono else ""
    label = "monochrome" if mono else "colour"

    return f"""<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 {w:.1f} {h:.1f}" width="{w:.1f}" height="{h:.1f}"{root_fill} role="img" aria-label="vectorreel">
  <title>vectorreel logo ({label})</title>
  <!-- mark -->
  <rect x="3.5" y="8.5" width="57" height="47" rx="9.5" fill="none" stroke="{ink}" stroke-width="5"/>
  <path d="M16 21.5 L16 42.5 L31 32 Z" fill="{ink}"/>
  <rect x="36" y="21.5" width="17" height="5" rx="2.5" fill="{sig}"/>
  <rect x="36" y="29.5" width="17" height="5" rx="2.5" fill="{sig}"/>
  <rect x="36" y="37.5" width="10" height="5" rx="2.5" fill="{vec}"/>
  <!-- wordmark: Inter Medium, outlined to paths (no webfont dependency) -->
  <g fill="{ink}" transform="translate({tx:.2f} {baseline_y:.2f}) scale({s:.6f} -{s:.6f})">
    {d}
  </g>
</svg>
"""


def main() -> None:
    os.makedirs(SVG_DIR, exist_ok=True)
    for mono in (False, True):
        name = "logo-horizontal-mono.svg" if mono else "logo-horizontal.svg"
        path = os.path.join(SVG_DIR, name)
        with open(path, "w", encoding="utf-8") as f:
            f.write(build(mono))
        print(f"wrote {path}")


if __name__ == "__main__":
    main()
