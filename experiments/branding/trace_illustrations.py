#!/usr/bin/env python3
"""
Auto-trace the generated raster illustrations to SVG with vtracer.

Tracing is the RIGHT tool for hero/spot illustrations: dozens of flat colour regions, no
precision requirement, and hand-authoring them would be absurd.

It is the WRONG tool for the logo mark -- tracing bakes the raster's anti-aliased, slightly
wobbly edges into hundreds of bezier points, giving you a vector-shaped copy of an imprecise
drawing. The mark is hand-authored instead (svg/logo-mark.svg). Don't trace logos.

    python trace_illustrations.py
"""

from __future__ import annotations

import os

import vtracer

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.join(HERE, "assets")
SVG_DIR = os.path.join(HERE, "svg")

# Flat vector art -> few colours, polygonal fitting, aggressive speckle filtering.
TARGETS = ["hero_background", "og_share", "feature_upload", "feature_markdown", "feature_knowledge"]


def main() -> None:
    os.makedirs(SVG_DIR, exist_ok=True)
    for key in TARGETS:
        src = os.path.join(OUT, f"{key}.png")
        if not os.path.exists(src):
            print(f"skip {key}: no {src}")
            continue
        dst = os.path.join(SVG_DIR, f"{key.replace('_', '-')}.svg")
        vtracer.convert_image_to_svg_py(
            src,
            dst,
            colormode="color",
            hierarchical="stacked",
            mode="polygon",      # flat geometric art: straight edges, not splines
            filter_speckle=8,    # drop dithering noise from the model's soft gradients
            color_precision=6,
            layer_difference=16,
            corner_threshold=60,
            path_precision=2,
        )
        kb = os.path.getsize(dst) / 1024
        print(f"traced {key:20} -> {os.path.basename(dst):28} {kb:7.1f} KB")


if __name__ == "__main__":
    main()
