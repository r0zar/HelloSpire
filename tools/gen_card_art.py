#!/usr/bin/env python3
"""
Generate labelled placeholder card art.

This is scaffolding, not final art. Its only job is to make cards tellable
apart in hand during testing instead of every one of them falling back to the
single generic card.png, and to produce correctly-sized files that real art can
later drop straight into.

Filenames must be the card's class name, lowercased with nothing between the
words -- that is what CustomCardModel.PortraitPath resolves to.

Usage:
    python tools/gen_card_art.py "Hand Me That" "Softened Up"
    python tools/gen_card_art.py --color 8f5aa8 "Shared Flask"

Sizes are dictated by the game and must not change:
    big/<name>.png    1000x760
    <name>.png         250x190
"""
import argparse
import os
import re

from PIL import Image, ImageDraw, ImageFont

BIG = (1000, 760)
SMALL = (250, 190)

# Sampled from the existing Gunslinger placeholders so new cards match the set.
FILL = (150, 75, 45)
BORDER = (60, 30, 18)
TEXT = (245, 234, 216)

FONTS = [
    "/System/Library/Fonts/Supplemental/Arial Bold.ttf",
    "/System/Library/Fonts/Supplemental/DejaVuSans-Bold.ttf",
    "/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf",
    "/Library/Fonts/Arial Bold.ttf",
]


def hex_rgb(h):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


def font(size):
    for path in FONTS:
        if os.path.exists(path):
            return ImageFont.truetype(path, size)
    raise SystemExit("no bold TrueType font found; add one to FONTS")


def filename(title):
    """'Hand Me That' -> 'handmethat', matching Id.Entry lowercased."""
    return re.sub(r"[^a-z0-9]", "", title.lower())


def wrap(draw, title, fnt, max_width):
    """Greedy wrap; long card names need two lines at 1000px wide."""
    words, lines, line = title.split(), [], ""
    for word in words:
        candidate = f"{line} {word}".strip()
        if draw.textlength(candidate, font=fnt) <= max_width or not line:
            line = candidate
        else:
            lines.append(line)
            line = word
    if line:
        lines.append(line)
    return lines


def tile(title, fill, border):
    """The 1000x760 master. The small variant is a downsample of this."""
    w, h = BIG
    img = Image.new("RGB", BIG, fill)
    draw = ImageDraw.Draw(img)

    draw.rectangle([0, 0, w - 1, h - 1], outline=border, width=4)
    draw.rectangle([4, 4, w - 5, h - 5], outline=border, width=6)

    fnt = font(64)
    lines = wrap(draw, title, fnt, w - 160)
    spacing = 78
    top = h / 2 - (len(lines) * spacing) / 2

    for i, line in enumerate(lines):
        draw.text((w / 2, top + i * spacing + spacing / 2), line,
                  font=fnt, fill=TEXT, anchor="mm")

    return img


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("titles", nargs="+", help="card display names")
    parser.add_argument("--color", default=None, help="tile fill, hex (default: Gunslinger rust)")
    parser.add_argument("--out", default="HelloSpire/images/card_portraits")
    args = parser.parse_args()

    fill = hex_rgb(args.color) if args.color else FILL
    os.makedirs(os.path.join(args.out, "big"), exist_ok=True)

    for title in args.titles:
        name = f"{filename(title)}.png"
        img = tile(title, fill, BORDER)
        img.save(os.path.join(args.out, "big", name))
        img.resize(SMALL, Image.LANCZOS).save(os.path.join(args.out, name))
        print(f"{title} -> {name}")


if __name__ == "__main__":
    main()
