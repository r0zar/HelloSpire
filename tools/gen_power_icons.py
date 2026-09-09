#!/usr/bin/env python3
"""
Generate the pack's missing power icons -- the Alchemist's ten engine powers, the Paladin's
four one-turn bookkeeping powers, and Renew, whose file existed but was a labelled placeholder
tile from gen_card_art.py rather than art.

Like tools/gen_gunslinger_icons.py and tools/gen_potion_icons.py, and unlike
tools/gen_card_art.py, this is not scaffolding: it is the art.

It matches the medallion the Alchemist's seven finished icons and the Paladin's seals
already use, which is a slightly different disc from the Gunslinger's: a **coloured disc
under a gold rim ring**, rather than the Gunslinger's brown disc with a brass ring set in
from the edge. Measured off `big/eternal_crucible_power.png` and
`big/seal_of_righteousness_power.png` so a new icon sits beside a hand-made one without
looking generated -- dark rim stroke, ~5px gold band, a radial disc that is lighter at the
upper left, and a cream glyph with a thin ink outline and one accent colour.

The disc colour carries the meaning the glyph cannot: poison green, brew amber, distill
teal, defence steel-blue, volatility violet, fury crimson.

Two of the pack's remaining "missing" icons are deliberately not here:

  * `BottledFuryStrengthPower` is a plain vanilla `TemporaryStrengthPower` that does NOT
    implement ICustomPower, so it already shows the base game's own temporary-Strength
    icon. That is correct, and giving it mod art would make the same buff read as two
    different things depending on who granted it.
  * Nine more power icons are labelled placeholder tiles -- divine_allegiance, judged, warded,
    sentinel, the_broken_god, blessing_of_sacrifice and the three oaths -- but every one of them
    is orphaned art for a class that no longer exists (Judged/Warded and the Oaths were cut; see
    design/paladin-faith-archive.md). Nothing loads them. Renew was the only live one.

Requires rsvg-convert (brew install librsvg) and Pillow.

Usage:
    python tools/gen_power_icons.py                      # all 15
    python tools/gen_power_icons.py toxic_culture        # one, by key (no _power suffix)
    python tools/gen_power_icons.py --sheet /tmp/x.png   # contact sheet, to judge the set
"""
import argparse
import math
import os
import subprocess
import sys
import tempfile

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
POWERS_DIR = os.path.join(ROOT, "HelloSpire", "images", "powers")

# The sizes the game asks for: 128 in the status bar, 256 in the tooltip.
SMALL, BIG = 128, 256

BOX = 256  # every glyph is authored in a 256x256 viewBox
C = BOX / 2

# ------------------------------------------------------------------ palette

INK      = "#26211c"  # the rim and glyph outline, measured off the hand-made icons
CREAM    = "#f4ecd6"  # every glyph's body
CREAM_DK = "#cdbf9e"
GOLD     = "#e8c46a"  # the rim ring, and the Paladin's colour
GOLD_LT  = "#f8d170"
GOLD_DK  = "#a8823a"
STEEL    = "#c3d0da"
STEEL_DK = "#7d8b98"
VENOM    = "#6ad48a"
VENOM_DK = "#2f7a4c"
ACID     = "#b6e34a"
EMBER    = "#e8703a"
EMBER_LT = "#f6b45c"
BLOOD    = "#b03a30"
VIOLET   = "#a877e0"
IRON     = "#8e97a4"

#: Disc base colours. Mid tone -- the gradient lightens and darkens around it.
DISCS = {
    "poison":   "#2e7a47",
    "spore":    "#1f6b40",
    "brew":     "#7f5a2e",
    "distill":  "#23613d",
    "defence":  "#47516b",
    "energy":   "#3d4d84",
    "draw":     "#2b6a78",
    "press":    "#7f4a28",
    "volatile": "#61327f",
    "fury":     "#8d2e29",
    # the Paladin's four, matched to the seal each one belongs to
    "righteous": "#8d2e29",
    "crusader":  "#a8792a",
    "martyr":    "#5b296f",
    "shackles":  "#3a3a46",
    "renew":     "#2e6f5a",
}


def shade(colour, factor):
    r, g, b = (int(colour[i:i + 2], 16) for i in (1, 3, 5))
    return "#" + "".join(f"{min(255, int(v * factor)):02x}" for v in (r, g, b))


# ------------------------------------------------------------------ svg helpers


def svg(body, defs=""):
    return (f'<svg xmlns="http://www.w3.org/2000/svg" width="{BOX}" height="{BOX}" '
            f'viewBox="0 0 {BOX} {BOX}">{defs}{body}</svg>')


def circle(cx, cy, r, fill="none", stroke=INK, sw=0, opacity=None):
    op = f' opacity="{opacity}"' if opacity is not None else ""
    return (f'<circle cx="{cx:.2f}" cy="{cy:.2f}" r="{r:.2f}" fill="{fill}" '
            f'stroke="{stroke}" stroke-width="{sw}"{op}/>')


def ellipse(cx, cy, rx, ry, fill="none", stroke=INK, sw=0, rot=None):
    tr = f' transform="rotate({rot} {cx:.2f} {cy:.2f})"' if rot is not None else ""
    return (f'<ellipse cx="{cx:.2f}" cy="{cy:.2f}" rx="{rx:.2f}" ry="{ry:.2f}" fill="{fill}" '
            f'stroke="{stroke}" stroke-width="{sw}"{tr}/>')


def rect(x, y, w, h, rx=0, fill="none", stroke=INK, sw=0, rot=None):
    tr = f' transform="rotate({rot} {x + w / 2:.2f} {y + h / 2:.2f})"' if rot is not None else ""
    return (f'<rect x="{x:.2f}" y="{y:.2f}" width="{w:.2f}" height="{h:.2f}" rx="{rx}" '
            f'fill="{fill}" stroke="{stroke}" stroke-width="{sw}"{tr}/>')


def path(d, fill="none", stroke=INK, sw=0, cap="round", join="round"):
    return (f'<path d="{d}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}" '
            f'stroke-linecap="{cap}" stroke-linejoin="{join}"/>')


def polygon(points, fill="none", stroke=INK, sw=0):
    pts = " ".join(f"{x:.2f},{y:.2f}" for x, y in points)
    return (f'<polygon points="{pts}" fill="{fill}" stroke="{stroke}" '
            f'stroke-width="{sw}" stroke-linejoin="round"/>')


def group(content, transform=""):
    t = f' transform="{transform}"' if transform else ""
    return f"<g{t}>{content}</g>"


def star(cx, cy, points, outer, inner, fill, stroke=INK, sw=5, phase=-math.pi / 2):
    pts = []
    for i in range(points * 2):
        r = outer if i % 2 == 0 else inner
        a = phase + i * math.pi / points
        pts.append((cx + math.cos(a) * r, cy + math.sin(a) * r))
    return polygon(pts, fill, stroke, sw)


def droplet(cx, cy, w, h, fill, stroke=INK, sw=5):
    """A teardrop, point up."""
    d = (f"M{cx:.2f},{cy - h / 2:.2f} "
         f"Q{cx + w / 2:.2f},{cy:.2f} {cx + w / 2:.2f},{cy + h / 6:.2f} "
         f"A{w / 2:.2f},{w / 2:.2f} 0 1 1 {cx - w / 2:.2f},{cy + h / 6:.2f} "
         f"Q{cx - w / 2:.2f},{cy:.2f} {cx:.2f},{cy - h / 2:.2f} Z")
    return path(d, fill, stroke, sw)


def shield(cx, cy, w, h, fill=CREAM, stroke=INK, sw=6):
    d = (f"M{cx - w / 2},{cy - h / 2} L{cx + w / 2},{cy - h / 2} L{cx + w / 2},{cy + h * 0.08} "
         f"Q{cx + w / 2},{cy + h / 2} {cx},{cy + h / 2} "
         f"Q{cx - w / 2},{cy + h / 2} {cx - w / 2},{cy + h * 0.08} Z")
    return path(d, fill, stroke, sw)


def arrow_down(cx, cy, w, h, fill=GOLD, stroke=INK, sw=5):
    return polygon([(cx - w * 0.26, cy - h / 2), (cx + w * 0.26, cy - h / 2),
                    (cx + w * 0.26, cy + h * 0.08), (cx + w / 2, cy + h * 0.08),
                    (cx, cy + h / 2), (cx - w / 2, cy + h * 0.08),
                    (cx - w * 0.26, cy + h * 0.08)], fill, stroke, sw)


def arrow_up(cx, cy, w, h, fill=GOLD, stroke=INK, sw=5):
    return group(arrow_down(cx, cy, w, h, fill, stroke, sw), f"rotate(180 {cx} {cy})")


def bolt(cx, cy, r, fill=GOLD_LT, stroke=INK, sw=5):
    d = (f"M{cx + r * 0.22},{cy - r} L{cx - r * 0.62},{cy + r * 0.12} L{cx - r * 0.04},{cy + r * 0.12} "
         f"L{cx - r * 0.22},{cy + r} L{cx + r * 0.62},{cy - r * 0.16} L{cx + r * 0.02},{cy - r * 0.16} Z")
    return path(d, fill, stroke, sw)


def flame(cx, cy, w, h, fill=EMBER, stroke=INK, sw=5):
    d = (f"M{cx},{cy - h / 2} Q{cx + w * 0.20},{cy - h * 0.14} {cx + w / 2},{cy - h * 0.06} "
         f"Q{cx + w * 0.46},{cy + h * 0.36} {cx},{cy + h / 2} "
         f"Q{cx - w * 0.46},{cy + h * 0.36} {cx - w / 2},{cy - h * 0.06} "
         f"Q{cx - w * 0.20},{cy - h * 0.14} {cx},{cy - h / 2} Z")
    return path(d, fill, stroke, sw)


def card(cx, cy, w, h, fill=CREAM, tilt=0, band=True):
    body = rect(cx - w / 2, cy - h / 2, w, h, 7, fill, INK, 5)
    inner = rect(cx - w * 0.30, cy - h * 0.30, w * 0.60, h * 0.34, 4, CREAM_DK, "none", 0) if band else ""
    return group(body + inner, f"rotate({tilt} {cx} {cy})")


def flask(cx, cy, r, liquid, neck_h=44):
    """The round-bottom flask the Alchemist's own icons already use."""
    neck = rect(cx - 15, cy - r - neck_h + 8, 30, neck_h, 5, CREAM, INK, 5)
    lip = rect(cx - 22, cy - r - neck_h + 2, 44, 14, 6, CREAM, INK, 5)
    bowl = circle(cx, cy, r, CREAM, INK, 6)
    fill = path(f"M{cx - r * 0.94},{cy + r * 0.24} A{r},{r} 0 0 0 {cx + r * 0.94},{cy + r * 0.24} Z",
                liquid, "none", 0)
    return neck + bowl + fill + circle(cx, cy, r, "none", INK, 6) + lip


def hourglass_badge(cx=176, cy=176, r=38):
    """
    The badge every one-turn power carries: this effect is the seal's, but only until the
    turn ends. One shared mark, so the three temp buffs and the shackles read as a family.
    """
    disc = circle(cx, cy, r, "#241f22", GOLD, 6)
    frame = (rect(cx - 17, cy - 20, 34, 7, 3, GOLD, INK, 3) +
             rect(cx - 17, cy + 13, 34, 7, 3, GOLD, INK, 3))
    sand = polygon([(cx - 13, cy - 13), (cx + 13, cy - 13), (cx, cy),
                    (cx + 13, cy + 13), (cx - 13, cy + 13), (cx, cy)], CREAM, INK, 3)
    return disc + sand + frame


def medallion(base):
    """The disc every icon here sits on: gold rim ring, radial disc, lighter to the upper left."""
    defs = ('<defs>'
            '<linearGradient id="ring" x1="0" y1="0" x2="0" y2="1">'
            f'<stop offset="0%" stop-color="{GOLD_LT}"/>'
            f'<stop offset="100%" stop-color="{shade(GOLD, 0.86)}"/>'
            '</linearGradient>'
            '<radialGradient id="disc" cx="38%" cy="32%" r="78%">'
            f'<stop offset="0%" stop-color="{shade(base, 1.20)}"/>'
            f'<stop offset="100%" stop-color="{shade(base, 0.86)}"/>'
            '</radialGradient>'
            '</defs>')
    body = circle(C, C, 116, "url(#ring)", INK, 6) + circle(C, C, 109, "url(#disc)", "none", 0)
    return defs, body


# ------------------------------------------------------------------ the Alchemist's engines
# Ten powers, each one glyph: what the engine turns into what.


def glyph_residual_toxins():
    """Poison begets poison: one drop lands, two more follow."""
    return (droplet(C, 118, 84, 112, CREAM) +
            circle(C, 132, 16, VENOM_DK, "none", 0) +
            droplet(C - 52, 196, 42, 56, VENOM) +
            droplet(C + 52, 196, 42, 56, VENOM))


def glyph_concentrate():
    """Infusion narrows to a spark: everything poured in comes out as Energy."""
    funnel = polygon([(58, 62), (198, 62), (146, 130), (110, 130)], CREAM, INK, 6)
    stem = rect(C - 17, 128, 34, 26, 4, CREAM, INK, 5)
    return funnel + stem + bolt(C, 196, 46)


def glyph_thermal_buffer():
    """Waste heat, banked as a wall."""
    waves = "".join(
        path(f"M{C - 62},{y} q16,-20 32,0 q16,20 32,0 q16,-20 32,0", "none", EMBER_LT, 9)
        for y in (62, 88))
    return waves + shield(C, 164, 108, 118, CREAM) + flame(C, 164, 52, 70, EMBER)


def glyph_reagent_press():
    """Two plates and what comes out between them. The chevrons are why the plates are heavy."""
    posts = (rect(58, 86, 13, 100, 4, CREAM_DK, INK, 4) + rect(185, 86, 13, 100, 4, CREAM_DK, INK, 4))
    top = rect(44, 70, 168, 34, 7, CREAM, INK, 6)
    bottom = rect(44, 172, 168, 34, 7, CREAM, INK, 6)
    press = (path(f"M{C - 30},32 L{C},46 L{C + 30},32", "none", EMBER, 9) +
             path(f"M{C - 30},48 L{C},62 L{C + 30},48", "none", EMBER, 9))
    return press + posts + droplet(C, 140, 58, 70, ACID) + top + bottom


def glyph_efficient_distillation():
    """The drip off the still, caught as Block."""
    drip = droplet(C, 62, 40, 54, STEEL)
    ticks = (path(f"M{C - 40},74 L{C - 26},86", "none", STEEL_DK, 7) +
             path(f"M{C + 40},74 L{C + 26},86", "none", STEEL_DK, 7))
    return drip + ticks + shield(C, 162, 108, 118, CREAM) + droplet(C, 158, 40, 52, VENOM)


def glyph_toxic_culture():
    """A dish seen from above, and what has grown in it."""
    dish = circle(C, C + 6, 74, CREAM, INK, 6)
    ring = circle(C, C + 6, 56, "none", CREAM_DK, 6)
    spores = "".join(circle(C + dx, C + 6 + dy, r, VENOM_DK, "none", 0)
                     for dx, dy, r in ((-24, -14, 15), (20, -20, 11), (6, 22, 17), (-14, 26, 9)))
    return dish + ring + spores


def glyph_volatile_laboratory():
    """Junk goes in, a bang comes out."""
    return (star(C, C, 10, 100, 52, VIOLET, INK, 6) +
            circle(C, C, 40, CREAM, INK, 5) +
            bolt(C, C, 26, VIOLET))


def glyph_accumulation():
    """Infusion, drawn back out as cards."""
    return (card(C - 46, 156, 62, 86, CREAM_DK, -20) +
            card(C + 46, 156, 62, 86, CREAM_DK, 20) +
            card(C, 148, 66, 92, CREAM, 0) +
            arrow_up(C, 62, 58, 60))


def glyph_brewing_engine():
    """Every brew hands a card back."""
    return card(C + 40, 74, 58, 80, CREAM, 16) + flask(C - 14, 156, 62, GOLD)


def glyph_bottled_fury():
    """Fury, bottled: the potion is the fuse, the Strength is the burn."""
    bottle = rect(C - 46, 96, 92, 116, 22, CREAM, INK, 6)
    neck = rect(C - 18, 62, 36, 42, 6, CREAM, INK, 5)
    lip = rect(C - 26, 54, 52, 16, 6, CREAM, INK, 5)
    return bottle + flame(C, 154, 66, 88, EMBER) + flame(C, 162, 32, 48, EMBER_LT) + neck + lip


# ------------------------------------------------------------------ the Paladin's one-turn powers
# Each is its seal's own glyph plus the hourglass badge: same effect, this turn only.


def sword(cx, cy, h, blade=CREAM, hilt=GOLD):
    half = h / 2
    blade_shape = polygon([(cx, cy - half), (cx + 17, cy - half + 26), (cx + 17, cy + half * 0.20),
                           (cx, cy + half * 0.36), (cx - 17, cy + half * 0.20),
                           (cx - 17, cy - half + 26)], blade, INK, 5)
    guard = rect(cx - 46, cy + half * 0.20, 92, 20, 6, hilt, INK, 5)
    grip = rect(cx - 10, cy + half * 0.36, 20, half * 0.44, 5, "#6b4526", INK, 5)
    pommel = circle(cx, cy + half * 0.86, 14, hilt, INK, 5)
    return blade_shape + guard + grip + pommel


def cross_flory(cx, cy, r, fill=CREAM, accent=BLOOD):
    arm = r * 0.30
    d = (f"M{cx - arm},{cy - r} L{cx + arm},{cy - r} L{cx + arm},{cy - arm} "
         f"L{cx + r},{cy - arm} L{cx + r},{cy + arm} L{cx + arm},{cy + arm} "
         f"L{cx + arm},{cy + r} L{cx - arm},{cy + r} L{cx - arm},{cy + arm} "
         f"L{cx - r},{cy + arm} L{cx - r},{cy - arm} L{cx - arm},{cy - arm} Z")
    return path(d, fill, INK, 6) + circle(cx, cy, r * 0.22, accent, INK, 4)


def glyph_seal_of_righteousness_strength():
    return sword(C - 20, 114, 164) + hourglass_badge(184, 182, 38)


def glyph_seal_of_the_crusader_strength():
    return cross_flory(C - 18, 112, 70, CREAM, BLOOD) + hourglass_badge(184, 182, 38)


def barb(cx, cy, angle, r_in, r_out, spread=0.16, curl=0.30, fill=CREAM):
    """One hooked thorn off a ring. Curved and swept, which is what separates a bramble
    from a sunburst at 32 pixels."""
    def polar(a, r):
        return cx + math.cos(a) * r, cy + math.sin(a) * r

    p1, p2 = polar(angle - spread, r_in), polar(angle + spread, r_in)
    tip = polar(angle + curl, r_out)
    ctrl = polar(angle + curl * 0.35, (r_in + r_out) / 2 + 10)
    d = (f"M{p1[0]:.2f},{p1[1]:.2f} Q{ctrl[0]:.2f},{ctrl[1]:.2f} {tip[0]:.2f},{tip[1]:.2f} "
         f"L{p2[0]:.2f},{p2[1]:.2f} Z")
    return path(d, fill, INK, 5)


def glyph_seal_of_the_martyr_thorns():
    """A crown of thorns: what the Martyr's judge leaves on anything that hits you."""
    cx, cy, r = C - 18, 112, 46
    spikes = "".join(
        barb(cx, cy, i * math.tau / 7 + math.tau / 14, r - 2, r + (40 if i % 2 else 28))
        for i in range(7))
    band = (circle(cx, cy, r, "none", CREAM, 13) + circle(cx, cy, r + 6.5, "none", INK, 4) +
            circle(cx, cy, r - 6.5, "none", INK, 4))
    return spikes + band + hourglass_badge(184, 182, 38)


def glyph_humbling_shackles():
    """A cuff and a broken link, and the arrow that says which way the Strength went."""
    cx, cy = C - 26, 112
    cuff = (circle(cx, cy, 50, "none", CREAM, 22) + circle(cx, cy, 61, "none", INK, 5) +
            circle(cx, cy, 39, "none", INK, 5))
    hinge = rect(cx - 15, cy - 74, 30, 26, 6, CREAM, INK, 5)
    chain = "".join(
        ellipse(cx + 46 + i * 30, cy + 52 + i * 24, 11, 19, "none", CREAM, 11, rot=-34) +
        ellipse(cx + 46 + i * 30, cy + 52 + i * 24, 16, 24, "none", INK, 4, rot=-34)
        for i in range(2))
    return cuff + hinge + chain + arrow_down(C - 26, 214, 56, 46, CREAM) + hourglass_badge(186, 62, 34)


def glyph_renew():
    """A drop of light inside a turning ring: the heal that arrives next turn, then fades."""
    cx, cy, r = C, C + 2, 66
    ring = (circle(cx, cy, r, "none", GOLD_LT, 17) +
            circle(cx, cy, r + 8.5, "none", INK, 4) +
            circle(cx, cy, r - 8.5, "none", INK, 4))
    a = math.radians(-52)
    hx, hy = cx + math.cos(a) * r, cy + math.sin(a) * r
    head = group(polygon([(hx - 27, hy + 10), (hx + 27, hy + 10), (hx, hy - 34)], GOLD_LT, INK, 5),
                 f"rotate({math.degrees(a) + 180:.1f} {hx:.2f} {hy:.2f})")
    return ring + head + droplet(cx, cy + 4, 58, 78, CREAM) + circle(cx, cy + 16, 15, BLOOD, INK, 4)


#: key -> (disc colour, glyph). The key IS the filename, minus `_power.png`.
POWERS = {
    # the Alchemist's engines
    "residual_toxins": ("poison", glyph_residual_toxins),
    "concentrate": ("energy", glyph_concentrate),
    "thermal_buffer": ("defence", glyph_thermal_buffer),
    "reagent_press": ("press", glyph_reagent_press),
    "efficient_distillation": ("distill", glyph_efficient_distillation),
    "toxic_culture": ("spore", glyph_toxic_culture),
    "volatile_laboratory": ("volatile", glyph_volatile_laboratory),
    "accumulation": ("draw", glyph_accumulation),
    "brewing_engine": ("brew", glyph_brewing_engine),
    "bottled_fury": ("fury", glyph_bottled_fury),
    # the Paladin's one-turn powers
    "seal_of_righteousness_strength": ("righteous", glyph_seal_of_righteousness_strength),
    "seal_of_the_crusader_strength": ("crusader", glyph_seal_of_the_crusader_strength),
    "seal_of_the_martyr_thorns": ("martyr", glyph_seal_of_the_martyr_thorns),
    "humbling_shackles": ("shackles", glyph_humbling_shackles),
    # the Paladin's heal-over-time, whose file was a labelled placeholder tile
    "renew": ("renew", glyph_renew),
}


# ------------------------------------------------------------------ rendering


def markup(key):
    disc_key, glyph = POWERS[key]
    defs, disc = medallion(DISCS[disc_key])
    return svg(disc + glyph(), defs)


def render(svg_markup, size):
    """SVG string -> RGBA Image, via rsvg-convert."""
    with tempfile.NamedTemporaryFile("w", suffix=".svg", delete=False) as handle:
        handle.write(svg_markup)
        svg_path = handle.name
    png_path = svg_path.replace(".svg", ".png")
    try:
        subprocess.run(
            ["rsvg-convert", "-w", str(size), "-h", str(size), "-o", png_path, svg_path],
            check=True, capture_output=True)
        return Image.open(png_path).convert("RGBA").copy()
    finally:
        for leftover in (svg_path, png_path):
            if os.path.exists(leftover):
                os.remove(leftover)


def write(image, path_out):
    os.makedirs(os.path.dirname(path_out), exist_ok=True)
    image.save(path_out)


def build(only=None):
    made = []
    for key in POWERS:
        if only and key not in only:
            continue
        art = markup(key)
        name = f"{key}_power.png"
        write(render(art, BIG), os.path.join(POWERS_DIR, "big", name))
        write(render(art, SMALL), os.path.join(POWERS_DIR, name))
        made.append(key)
    return made


def contact_sheet(path_out, columns=5, cell=128):
    """Every icon in one image, for eyeballing the set as a set."""
    keys = list(POWERS)
    rows = (len(keys) + columns - 1) // columns
    sheet = Image.new("RGBA", (columns * cell, rows * cell), (32, 26, 20, 255))
    for i, key in enumerate(keys):
        sheet.alpha_composite(render(markup(key), cell), ((i % columns) * cell, (i // columns) * cell))
    sheet.convert("RGB").save(path_out)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("keys", nargs="*", help="icon keys to rebuild (default: all)")
    parser.add_argument("--sheet", default=None, help="also write a contact sheet here")
    args = parser.parse_args()

    if subprocess.run(["which", "rsvg-convert"], capture_output=True).returncode != 0:
        sys.exit("rsvg-convert not found; brew install librsvg")

    unknown = set(args.keys) - set(POWERS)
    if unknown:
        sys.exit(f"unknown power key(s): {', '.join(sorted(unknown))}")

    for key in build(set(args.keys) or None):
        print(f"power  {key}")

    if args.sheet:
        contact_sheet(args.sheet)
        print(f"sheet  {args.sheet}")


if __name__ == "__main__":
    main()
