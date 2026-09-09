#!/usr/bin/env python3
"""
Generate the pack's potion icons -- the fill art and its white `outline/` companion.

Like tools/gen_gunslinger_icons.py and unlike tools/gen_card_art.py, this is not
scaffolding: it is the art. Every icon is a handful of flat vector shapes, so the whole
set is one file that can be re-rendered at any size and adjusted by editing a shape
instead of repainting a bitmap.

The grammar is one sentence long: **a glass vessel, a coloured liquid, and one emblem
that says what drinking it does.** The vessel shape is the character's -- the Paladin
carries reliquary ampullae and altar vials, the Gunslinger carries a hip flask and field
tonics, the Alchemist carries bench glassware -- and the liquid carries the character's
colour, so a potion reads as *whose* before it reads as *what*. Ink outlines and flat
fills match the medallion icons the Gunslinger's powers already use.

The 15 Volatile Common Potions are deliberately absent: they point at the base game's own
potion sprites (see VolatileCommonPotions.cs), so they are not missing art and must not be
given any. Volatile Poison Potion and Volatile Poison Ampoule share `poison_potion.png`
with the real Poison Potion, which this script does draw.

Outlines are derived from the alpha of the fill art, so a silhouette can never drift from
the art it belongs to. Do not hand-edit them.

Requires rsvg-convert (brew install librsvg) and Pillow.

Usage:
    python tools/gen_potion_icons.py                     # all 14
    python tools/gen_potion_icons.py anointing_oil       # one icon, by key
    python tools/gen_potion_icons.py --sheet /tmp/x.png  # contact sheet, to judge the set
"""
import argparse
import math
import os
import subprocess
import sys
import tempfile

from PIL import Image

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
POTIONS_DIR = os.path.join(ROOT, "HelloSpire", "images", "potions")

# The size the generic potion.png ships at, and what the potion bar and tooltip both scale from.
SIZE = 256

BOX = 256  # every icon is authored in a 256x256 viewBox
C = BOX / 2

# ------------------------------------------------------------------ palette

INK        = "#241809"  # the outline every shape carries, shared with the Gunslinger's icons
GLASS      = "#dbe7ec"  # empty glass, above the liquid line
GLASS_DK   = "#a9bcc6"
SHINE      = "#ffffff"
CORK       = "#a9702f"
CORK_LT    = "#c9924d"
WAX        = "#8d2f2f"
STEEL      = "#b9c4ce"
STEEL_DK   = "#5d6a77"

GOLD       = "#e8c46a"  # Paladin.Color
GOLD_LT    = "#f9e7b4"
GOLD_DK    = "#a8823a"

BRASS      = "#d9a05b"  # Gunslinger
RUST       = "#b4552f"
LEAD       = "#9aa7b4"
SMOKE      = "#d8d2c6"
SMOKE_DK   = "#8d8478"

VENOM      = "#6ad48a"  # Alchemist
VENOM_LT   = "#b7f0c8"
VENOM_DK   = "#2f7a4c"
ACID       = "#b6e34a"
SOLVENT    = "#7fd6d0"
MURK       = "#7b6a44"
MURK_DK    = "#4a3f28"
CRIMSON    = "#c8412f"
CRIMSON_LT = "#f08b6a"
VIOLET     = "#8f6bd0"

# ------------------------------------------------------------------ svg helpers


def svg(body, defs=""):
    return (f'<svg xmlns="http://www.w3.org/2000/svg" width="{BOX}" height="{BOX}" '
            f'viewBox="0 0 {BOX} {BOX}">{defs}{body}</svg>')


def circle(cx, cy, r, fill="none", stroke=INK, sw=0, opacity=None):
    op = f' opacity="{opacity}"' if opacity is not None else ""
    return (f'<circle cx="{cx:.2f}" cy="{cy:.2f}" r="{r:.2f}" fill="{fill}" '
            f'stroke="{stroke}" stroke-width="{sw}"{op}/>')


def ellipse(cx, cy, rx, ry, fill="none", stroke=INK, sw=0, opacity=None, rot=None):
    op = f' opacity="{opacity}"' if opacity is not None else ""
    tr = f' transform="rotate({rot} {cx:.2f} {cy:.2f})"' if rot is not None else ""
    return (f'<ellipse cx="{cx:.2f}" cy="{cy:.2f}" rx="{rx:.2f}" ry="{ry:.2f}" fill="{fill}" '
            f'stroke="{stroke}" stroke-width="{sw}"{op}{tr}/>')


def rect(x, y, w, h, rx=0, fill="none", stroke=INK, sw=0, rot=None, opacity=None):
    tr = ""
    if rot is not None:
        tr = f' transform="rotate({rot} {x + w / 2:.2f} {y + h / 2:.2f})"'
    op = f' opacity="{opacity}"' if opacity is not None else ""
    return (f'<rect x="{x:.2f}" y="{y:.2f}" width="{w:.2f}" height="{h:.2f}" rx="{rx}" '
            f'fill="{fill}" stroke="{stroke}" stroke-width="{sw}"{tr}{op}/>')


def path(d, fill="none", stroke=INK, sw=0, cap="round", join="round", opacity=None):
    op = f' opacity="{opacity}"' if opacity is not None else ""
    return (f'<path d="{d}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}" '
            f'stroke-linecap="{cap}" stroke-linejoin="{join}"{op}/>')


def polygon(points, fill="none", stroke=INK, sw=0, opacity=None):
    pts = " ".join(f"{x:.2f},{y:.2f}" for x, y in points)
    op = f' opacity="{opacity}"' if opacity is not None else ""
    return (f'<polygon points="{pts}" fill="{fill}" stroke="{stroke}" '
            f'stroke-width="{sw}" stroke-linejoin="round"{op}/>')


def group(content, transform="", opacity=None):
    t = f' transform="{transform}"' if transform else ""
    op = f' opacity="{opacity}"' if opacity is not None else ""
    return f"<g{t}{op}>{content}</g>"


def star(cx, cy, points, outer, inner, fill, stroke=INK, sw=5, phase=-math.pi / 2):
    pts = []
    for i in range(points * 2):
        r = outer if i % 2 == 0 else inner
        a = phase + i * math.pi / points
        pts.append((cx + math.cos(a) * r, cy + math.sin(a) * r))
    return polygon(pts, fill, stroke, sw)


def droplet(cx, cy, w, h, fill, stroke=INK, sw=4):
    """A teardrop, point up."""
    d = (f"M{cx:.2f},{cy - h / 2:.2f} "
         f"Q{cx + w / 2:.2f},{cy:.2f} {cx + w / 2:.2f},{cy + h / 6:.2f} "
         f"A{w / 2:.2f},{w / 2:.2f} 0 1 1 {cx - w / 2:.2f},{cy + h / 6:.2f} "
         f"Q{cx - w / 2:.2f},{cy:.2f} {cx:.2f},{cy - h / 2:.2f} Z")
    return path(d, fill, stroke, sw)


# ------------------------------------------------------------------ vessels
# A vessel is three things: a body (drawn twice -- once filled, once as the ink rim over the
# liquid, and once more as the clip for anything poured into it), a neck, and a mouth the
# stopper sits on. Every potion in the set is one of these six with a different liquid and
# emblem, which is what makes the set read as a set.


class Vessel:
    def __init__(self, body, neck, mouth_y, mouth_w, level, emblem, emblem_r):
        #: body(fill, stroke, sw) -> one svg element. Called three times per icon.
        self.body = body
        #: the neck svg, drawn under the body so the join disappears behind the glass.
        self.neck = neck
        self.mouth_y = mouth_y      # top of the neck; the stopper sits here
        self.mouth_w = mouth_w      # neck width, so collars and corks size themselves
        self.level = level          # y of the liquid surface
        self.emblem = emblem        # (cx, cy) the emblem centres on
        self.emblem_r = emblem_r    # roughly how much room it has


def round_flask():
    """The classic boiling flask: a sphere with a short neck. The workhorse."""
    cx, cy, r = C, 158, 74
    return Vessel(
        body=lambda fill, stroke, sw: circle(cx, cy, r, fill, stroke, sw),
        neck=rect(C - 25, 54, 50, 66, 8, GLASS, INK, 7),
        mouth_y=54, mouth_w=50, level=124, emblem=(cx, cy + 6), emblem_r=44)


def conical_flask():
    """An Erlenmeyer: the bench shape. Wide base, sloping shoulders."""
    d = ("M104,96 L152,96 L208,206 Q214,222 196,224 L60,224 Q42,222 48,206 Z")
    return Vessel(
        body=lambda fill, stroke, sw: path(d, fill, stroke, sw),
        neck=rect(C - 24, 46, 48, 60, 6, GLASS, INK, 7),
        mouth_y=46, mouth_w=48, level=140, emblem=(C, 178), emblem_r=42)


def tall_vial():
    """A test-tube vial: narrow, upright, rounded foot. Reads as a single measured dose."""
    d = ("M84,86 L172,86 L172,190 Q172,228 128,228 Q84,228 84,190 Z")
    return Vessel(
        body=lambda fill, stroke, sw: path(d, fill, stroke, sw),
        neck=rect(C - 27, 44, 54, 56, 6, GLASS, INK, 7),
        mouth_y=44, mouth_w=54, level=112, emblem=(C, 158), emblem_r=36)


def ampoule():
    """An onion-bellied ampulla drawn out to a sealed tip -- the reliquary shape."""
    d = ("M128,64 Q150,104 168,124 Q206,164 186,204 Q166,236 128,236 "
         "Q90,236 70,204 Q50,164 88,124 Q106,104 128,64 Z")
    return Vessel(
        body=lambda fill, stroke, sw: path(d, fill, stroke, sw),
        neck="",
        mouth_y=64, mouth_w=34, level=140, emblem=(C, 182), emblem_r=42)


def hip_flask():
    """A shouldered pocket flask. The Gunslinger's own glassware."""
    d = ("M72,104 Q72,88 92,88 L164,88 Q184,88 184,104 L192,206 "
         "Q194,226 172,226 L84,226 Q62,226 64,206 Z")
    return Vessel(
        body=lambda fill, stroke, sw: path(d, fill, stroke, sw),
        neck=rect(C - 22, 50, 44, 52, 6, GLASS, INK, 7),
        mouth_y=50, mouth_w=44, level=126, emblem=(C, 166), emblem_r=42)


def wide_jar():
    """A squat apothecary jar: a big mouth, for things that come out in a cloud."""
    d = ("M62,118 Q62,104 80,104 L176,104 Q194,104 194,118 L194,202 "
         "Q194,224 172,224 L84,224 Q62,224 62,202 Z")
    return Vessel(
        body=lambda fill, stroke, sw: path(d, fill, stroke, sw),
        neck=rect(C - 40, 66, 80, 52, 6, GLASS, INK, 7),
        mouth_y=66, mouth_w=80, level=140, emblem=(C, 172), emblem_r=44)


# ------------------------------------------------------------------ stoppers


def cork(v, tilt=0):
    w = v.mouth_w * 0.72
    body = rect(C - w / 2, v.mouth_y - 30, w, 40, 7, CORK, INK, 7)
    grain = rect(C - w / 2 + 7, v.mouth_y - 24, w - 14, 8, 4, CORK_LT, "none", 0)
    lip = rect(C - v.mouth_w / 2 - 7, v.mouth_y - 2, v.mouth_w + 14, 18, 6, GLASS_DK, INK, 7)
    return group(body + grain + lip, f"rotate({tilt} {C} {v.mouth_y})")


def metal_cap(v, colour=BRASS, accent=GOLD_LT):
    w = v.mouth_w * 0.86
    cap = rect(C - w / 2, v.mouth_y - 34, w, 44, 8, colour, INK, 7)
    band = rect(C - w / 2, v.mouth_y - 16, w, 9, 3, accent, "none", 0)
    lip = rect(C - v.mouth_w / 2 - 8, v.mouth_y - 2, v.mouth_w + 16, 18, 6, colour, INK, 7)
    return cap + band + lip


def wax_seal(v):
    lip = rect(C - v.mouth_w / 2 - 8, v.mouth_y - 4, v.mouth_w + 16, 20, 6, GLASS_DK, INK, 7)
    blob = path(f"M{C - 30},{v.mouth_y - 6} Q{C},{v.mouth_y - 44} {C + 30},{v.mouth_y - 6} "
                f"Q{C},{v.mouth_y + 10} {C - 30},{v.mouth_y - 6} Z", WAX, INK, 6)
    return lip + blob


def open_mouth(v, colour=GLASS_DK):
    """No stopper: a flared rim. For the two that are meant to be venting."""
    return (rect(C - v.mouth_w / 2 - 10, v.mouth_y - 4, v.mouth_w + 20, 20, 6, colour, INK, 7) +
            ellipse(C, v.mouth_y + 2, v.mouth_w / 2 + 4, 8, GLASS, INK, 5))


def sealed_tip(v, colour=GOLD):
    """The ampoule's drawn-out neck, pinched shut and capped."""
    neck = path("M118,74 Q128,52 138,74", "none", INK, 9)
    band = rect(C - 15, 44, 30, 20, 6, colour, INK, 6)
    ring = circle(C, 34, 12, "none", INK, 7)
    return neck + band + ring


# ------------------------------------------------------------------ contents


def contents(v, key, liquid, inner=""):
    """clipPath defs + the clipped group. Kept separate so defs can go in <defs>."""
    defs = f'<defs><clipPath id="cut_{key}">{v.body("#fff", "none", 0)}</clipPath></defs>'
    body = (rect(0, v.level, BOX, BOX - v.level, 0, liquid, "none", 0) + inner)
    return defs, f'<g clip-path="url(#cut_{key})">{body}</g>'


def surface(v, colour, inset=0):
    """The meniscus: a paler line where the liquid meets the glass."""
    return rect(0, v.level, BOX, 7, 0, colour, "none", 0, opacity=0.85)


def bubbles(spec):
    """spec: (cx, cy, r, colour) tuples. Suspended, unstroked -- they read as *in* the liquid."""
    return "".join(circle(cx, cy, r, col, "none", 0, opacity=0.75) for cx, cy, r, col in spec)


def shine(v, x, y, h, w=15, tilt=-12):
    """The glass highlight. One streak, always upper-left, always the same angle."""
    return rect(x, y, w, h, w / 2, SHINE, "none", 0, rot=tilt, opacity=0.5)


# ------------------------------------------------------------------ emblems
# One emblem per potion, sized to the vessel's emblem_r and drawn over the liquid. This is
# the half of the icon that says what the potion DOES; the vessel and colour say whose it is.


def cross_flory(cx, cy, r, fill=GOLD_LT):
    arm = r * 0.30
    d = (f"M{cx - arm},{cy - r} L{cx + arm},{cy - r} L{cx + arm},{cy - arm} "
         f"L{cx + r},{cy - arm} L{cx + r},{cy + arm} L{cx + arm},{cy + arm} "
         f"L{cx + arm},{cy + r} L{cx - arm},{cy + r} L{cx - arm},{cy + arm} "
         f"L{cx - r},{cy + arm} L{cx - r},{cy - arm} L{cx - arm},{cy - arm} Z")
    return path(d, fill, INK, 5)


def shield(cx, cy, w, h, fill, stroke=INK, sw=5):
    d = (f"M{cx - w / 2},{cy - h / 2} L{cx + w / 2},{cy - h / 2} L{cx + w / 2},{cy + h * 0.08} "
         f"Q{cx + w / 2},{cy + h / 2} {cx},{cy + h / 2} "
         f"Q{cx - w / 2},{cy + h / 2} {cx - w / 2},{cy + h * 0.08} Z")
    return path(d, fill, stroke, sw)


def hammer(cx, cy, r, head=GOLD_LT, haft=CORK):
    shaft = rect(cx - 5, cy - r * 0.42, 10, r * 1.42, 5, haft, INK, 4)
    block = rect(cx - r * 0.62, cy - r * 0.86, r * 1.24, r * 0.52, 6, head, INK, 5)
    cheek = rect(cx - r * 0.62, cy - r * 0.74, r * 0.22, r * 0.28, 3, GLASS_DK, "none", 0)
    return group(shaft + block + cheek, f"rotate(-22 {cx} {cy})")


def skull(cx, cy, r, fill=VENOM_LT):
    dome = circle(cx, cy - r * 0.12, r * 0.72, fill, INK, 5)
    jaw = rect(cx - r * 0.36, cy + r * 0.42, r * 0.72, r * 0.42, 7, fill, INK, 5)
    eyes = (ellipse(cx - r * 0.3, cy - r * 0.14, r * 0.19, r * 0.23, INK, "none", 0) +
            ellipse(cx + r * 0.3, cy - r * 0.14, r * 0.19, r * 0.23, INK, "none", 0))
    nose = polygon([(cx, cy + r * 0.12), (cx - r * 0.12, cy + r * 0.34),
                    (cx + r * 0.12, cy + r * 0.34)], INK, "none", 0)
    return dome + jaw + eyes + nose


def cartridge(cx, cy, h, case=BRASS, tip=LEAD, tilt=0):
    body = rect(cx - 11, cy - h / 2 + h * 0.28, 22, h * 0.72, 4, case, INK, 5)
    nose = polygon([(cx - 11, cy - h / 2 + h * 0.30), (cx, cy - h / 2 - h * 0.02),
                    (cx + 11, cy - h / 2 + h * 0.30)], tip, INK, 5)
    rim = rect(cx - 13, cy + h / 2 - h * 0.12, 26, 8, 3, case, INK, 4)
    return group(body + nose + rim, f"rotate({tilt} {cx} {cy})")


def crosshair(cx, cy, r, colour=GOLD_LT):
    ring = circle(cx, cy, r, "none", colour, 7)
    ticks = "".join(
        path(f"M{cx + math.cos(a) * (r - 4):.2f},{cy + math.sin(a) * (r - 4):.2f} "
             f"L{cx + math.cos(a) * (r + 15):.2f},{cy + math.sin(a) * (r + 15):.2f}",
             "none", colour, 7)
        for a in (0, math.pi / 2, math.pi, 3 * math.pi / 2))
    dot = circle(cx, cy, 5, colour, "none", 0)
    return ring + ticks + dot


def ghost(cx, cy, w, h, fill=SMOKE):
    """A dome with a scalloped hem. The one shape that reads as \"ghost\" at 32 pixels."""
    d = (f"M{cx - w / 2},{cy + h * 0.40} L{cx - w / 2},{cy - h * 0.04} "
         f"A{w / 2},{h * 0.5} 0 0 1 {cx + w / 2},{cy - h * 0.04} L{cx + w / 2},{cy + h * 0.40} "
         f"q{-w / 6},{h * 0.20} {-w / 3},0 q{-w / 6},{-h * 0.20} {-w / 3},0 "
         f"q{-w / 6},{h * 0.20} {-w / 3},0 Z")
    eyes = (ellipse(cx - w * 0.19, cy - h * 0.10, w * 0.10, h * 0.13, INK, "none", 0) +
            ellipse(cx + w * 0.19, cy - h * 0.10, w * 0.10, h * 0.13, INK, "none", 0))
    return path(d, fill, INK, 5) + eyes


def wisp(cx, cy, r, colour=SMOKE, sw=9, opacity=0.9):
    """A curl of smoke, for the trail leaving an open mouth."""
    d = (f"M{cx - r},{cy + r * 0.5} Q{cx - r * 0.2},{cy + r * 0.1} {cx - r * 0.5},{cy - r * 0.4} "
         f"Q{cx - r * 0.7},{cy - r} {cx + r * 0.1},{cy - r * 0.85} "
         f"Q{cx + r},{cy - r * 0.7} {cx + r * 0.6},{cy}")
    return path(d, "none", colour, sw, opacity=opacity)


def card_glyph(cx, cy, w, h, fill=GLASS):
    """A card with its bottom edge eaten away. Solvent Flask's whole sentence."""
    d = (f"M{cx - w / 2},{cy - h / 2} L{cx + w / 2},{cy - h / 2} L{cx + w / 2},{cy + h * 0.14} "
         f"q{-w * 0.25},{h * 0.26} {-w * 0.5},{h * 0.02} "
         f"q{-w * 0.24},{-h * 0.22} {-w * 0.5},{h * 0.06} Z")
    face = rect(cx - w * 0.28, cy - h * 0.34, w * 0.56, h * 0.3, 4, GLASS_DK, "none", 0)
    return path(d, fill, INK, 5) + face


def gem(cx, cy, r, fill=CRIMSON, facet=CRIMSON_LT):
    top = r * 0.42
    outline = polygon([(cx - r * 0.62, cy - top), (cx + r * 0.62, cy - top),
                       (cx + r, cy - top * 0.1), (cx, cy + r), (cx - r, cy - top * 0.1)],
                      fill, INK, 5)
    cut = polygon([(cx - r * 0.62, cy - top), (cx + r * 0.62, cy - top),
                   (cx + r * 0.3, cy + r * 0.05), (cx - r * 0.3, cy + r * 0.05)], facet, "none", 0)
    ridge = path(f"M{cx - r},{cy - top * 0.1} L{cx + r},{cy - top * 0.1}", "none", INK, 4)
    return outline + cut + ridge


def bolt(cx, cy, r, fill=ACID):
    d = (f"M{cx + r * 0.2},{cy - r} L{cx - r * 0.6},{cy + r * 0.12} L{cx - r * 0.04},{cy + r * 0.12} "
         f"L{cx - r * 0.24},{cy + r} L{cx + r * 0.6},{cy - r * 0.18} L{cx + r * 0.02},{cy - r * 0.18} Z")
    return path(d, fill, INK, 5)


def mini_bottle(cx, cy, h, liquid, tilt=0):
    """A potion inside a potion. Panacea of Plenty is a belt in a bottle."""
    w = h * 0.62
    body = rect(cx - w / 2, cy - h * 0.18, w, h * 0.66, w * 0.28, GLASS, INK, 5)
    fill = rect(cx - w / 2 + 3, cy + h * 0.10, w - 6, h * 0.36, 5, liquid, "none", 0)
    neck = rect(cx - w * 0.17, cy - h * 0.44, w * 0.34, h * 0.3, 3, GLASS, INK, 5)
    cap = rect(cx - w * 0.24, cy - h * 0.56, w * 0.48, h * 0.16, 3, CORK, INK, 4)
    return group(body + fill + neck + cap, f"rotate({tilt} {cx} {cy})")


# ------------------------------------------------------------------ the potions
# Each entry returns (defs, body). Read them as: vessel, liquid, emblem, stopper.


# --- Paladin: gold liquid, reliquary glassware -------------------------------

def anointing_oil():
    v = ampoule()
    inner = (surface(v, GOLD_LT) +
             bubbles([(104, 176, 7, GOLD_LT), (152, 196, 6, GOLD_LT), (126, 212, 5, GOLD_LT)]) +
             cross_flory(*v.emblem, 30, GLASS) +
             "".join(path(f"M{v.emblem[0] + math.cos(a) * 36:.2f},{v.emblem[1] + math.sin(a) * 36:.2f} "
                          f"L{v.emblem[0] + math.cos(a) * 48:.2f},{v.emblem[1] + math.sin(a) * 48:.2f}",
                          "none", GLASS, 7, opacity=0.85)
                     for a in (math.pi / 4, 3 * math.pi / 4, 5 * math.pi / 4, 7 * math.pi / 4)))
    defs, poured_in = contents(v, "anointing_oil", GOLD, inner)
    return defs, (v.body(GLASS, INK, 7) + poured_in + v.body("none", INK, 7) +
                  shine(v, 84, 150, 46) + sealed_tip(v))


def vial_of_verdict():
    v = tall_vial()
    inner = (surface(v, GOLD_LT) +
             bubbles([(102, 178, 6, GOLD_LT), (154, 198, 5, GOLD_LT)]) +
             hammer(v.emblem[0] - 2, v.emblem[1] + 6, 40, GLASS, MURK_DK) +
             star(v.emblem[0] + 26, v.emblem[1] - 28, 4, 21, 5, GLASS, INK, 3))
    defs, poured_in = contents(v, "vial_of_verdict", GOLD, inner)
    return defs, (v.neck + v.body(GLASS, INK, 7) + poured_in + v.body("none", INK, 7) +
                  shine(v, 96, 118, 62) + metal_cap(v, GOLD, GOLD_LT))


def sanctified_draught():
    v = round_flask()
    inner = (surface(v, GOLD_LT) +
             bubbles([(96, 176, 7, GOLD_LT), (160, 190, 6, GOLD_LT), (120, 204, 5, GOLD_LT)]) +
             shield(v.emblem[0], v.emblem[1] + 2, 64, 74, GLASS, INK, 5) +
             cross_flory(v.emblem[0], v.emblem[1] + 2, 19, GOLD_DK))
    defs, poured_in = contents(v, "sanctified_draught", GOLD, inner)
    return defs, (v.neck + v.body(GLASS, INK, 7) + poured_in + v.body("none", INK, 7) +
                  shine(v, 84, 132, 52) + wax_seal(v))


# --- Gunslinger: brass and rust, field glassware -----------------------------

def speedloader_flask():
    v = hip_flask()
    inner = (surface(v, BRASS) +
             cartridge(v.emblem[0] - 34, v.emblem[1] + 6, 74, BRASS, LEAD, -10) +
             cartridge(v.emblem[0], v.emblem[1] - 2, 78, BRASS, LEAD, 0) +
             cartridge(v.emblem[0] + 34, v.emblem[1] + 6, 74, BRASS, LEAD, 10))
    defs, poured_in = contents(v, "speedloader_flask", RUST, inner)
    return defs, (v.neck + v.body(GLASS, INK, 7) + poured_in + v.body("none", INK, 7) +
                  shine(v, 84, 122, 58) + metal_cap(v, STEEL, GLASS))


def sightline_tonic():
    v = tall_vial()
    inner = (surface(v, GOLD_LT) +
             bubbles([(100, 196, 6, BRASS), (156, 178, 5, BRASS)]) +
             crosshair(v.emblem[0], v.emblem[1], 29, "#f7ecd8"))
    defs, poured_in = contents(v, "sightline_tonic", RUST, inner)
    return defs, (v.neck + v.body(GLASS, INK, 7) + poured_in + v.body("none", INK, 7) +
                  shine(v, 96, 118, 62) + metal_cap(v, RUST, BRASS))


def ghost_smoke():
    v = round_flask()
    inner = (surface(v, SMOKE) +
             ghost(v.emblem[0], v.emblem[1] + 6, 66, 80))
    defs, poured_in = contents(v, "ghost_smoke", SMOKE_DK, inner)
    escaping = (wisp(C - 4, 46, 26, SMOKE, 9, 0.8) + wisp(C + 26, 24, 16, SMOKE, 7, 0.55))
    return defs, (v.neck + v.body(GLASS, INK, 7) + poured_in + v.body("none", INK, 7) +
                  shine(v, 84, 132, 52) + open_mouth(v, LEAD) + escaping)


# --- Alchemist: bench glassware, venom green unless the potion says otherwise -

def solvent_flask():
    v = conical_flask()
    inner = (surface(v, "#c9f2ef") +
             bubbles([(96, 196, 7, "#c9f2ef"), (162, 184, 6, "#c9f2ef"), (128, 212, 5, "#c9f2ef")]) +
             card_glyph(v.emblem[0], v.emblem[1] - 8, 62, 78, GLASS))
    defs, poured_in = contents(v, "solvent_flask", SOLVENT, inner)
    return defs, (v.neck + v.body(GLASS, INK, 7) + poured_in + v.body("none", INK, 7) +
                  shine(v, 92, 150, 48) + cork(v))


def aurum_tincture():
    v = ampoule()
    inner = (surface(v, GOLD_LT) +
             bubbles([(100, 176, 7, GOLD_LT), (154, 198, 6, VENOM_LT), (124, 214, 5, GOLD_LT)]) +
             droplet(v.emblem[0], v.emblem[1] - 2, 44, 62, GOLD) +
             circle(v.emblem[0] - 8, v.emblem[1] + 8, 7, GOLD_LT, "none", 0))
    defs, poured_in = contents(v, "aurum_tincture", VENOM_DK, inner)
    return defs, (v.body(GLASS, INK, 7) + poured_in + v.body("none", INK, 7) +
                  shine(v, 84, 150, 46) + sealed_tip(v, GOLD))


def poison_potion():
    v = round_flask()
    inner = (surface(v, VENOM_LT) +
             bubbles([(94, 182, 7, VENOM_LT), (162, 196, 6, VENOM_LT)]) +
             skull(v.emblem[0], v.emblem[1] - 2, 44))
    defs, poured_in = contents(v, "poison_potion", VENOM, inner)
    return defs, (v.neck + v.body(GLASS, INK, 7) + poured_in + v.body("none", INK, 7) +
                  shine(v, 84, 132, 52) + cork(v))


def poison_ampoule():
    v = wide_jar()
    inner = (surface(v, VENOM_LT) +
             bubbles([(88, 190, 7, VENOM_LT), (168, 200, 6, VENOM_LT)]) +
             skull(v.emblem[0], v.emblem[1] + 2, 34))
    defs, poured_in = contents(v, "poison_ampoule", VENOM, inner)
    # The cloud: three plumes off the open mouth. This one goes wide, and the icon says so.
    cloud = "".join(droplet(C + dx, 44 + dy, 26, 34, VENOM_LT, INK, 4)
                    for dx, dy in ((-44, 8), (0, -8), (44, 8)))
    return defs, (v.neck + v.body(GLASS, INK, 7) + poured_in + v.body("none", INK, 7) +
                  shine(v, 82, 148, 50) + open_mouth(v, VENOM_DK) + cloud)


def residual_reagent():
    v = tall_vial()
    v.level = 186  # dregs: what is left when the useful part has already been used
    inner = (surface(v, MURK) +
             bubbles([(106, 206, 9, MURK), (150, 214, 7, MURK), (128, 222, 6, MURK_DK)]))
    defs, poured_in = contents(v, "residual_reagent", MURK, inner)
    crack = path("M110,98 L124,126 L108,140 L120,172", "none", INK, 7, opacity=0.8)
    return defs, (v.neck + v.body(GLASS, INK, 7) + poured_in + v.body("none", INK, 7) +
                  crack + shine(v, 96, 118, 44) + open_mouth(v, GLASS_DK))


def panacea_of_plenty():
    v = wide_jar()
    inner = (surface(v, GOLD_LT) +
             mini_bottle(v.emblem[0] - 40, v.emblem[1] + 4, 74, CRIMSON, -14) +
             mini_bottle(v.emblem[0] + 40, v.emblem[1] + 4, 74, SOLVENT, 14) +
             mini_bottle(v.emblem[0], v.emblem[1] - 8, 86, VENOM, 0))
    defs, poured_in = contents(v, "panacea_of_plenty", GOLD_DK, inner)
    sparks = (star(78, 92, 4, 17, 4, GOLD_LT, INK, 3) + star(182, 78, 4, 13, 3, GOLD_LT, INK, 3))
    return defs, (v.neck + v.body(GLASS, INK, 7) + poured_in + v.body("none", INK, 7) +
                  shine(v, 82, 148, 50) + open_mouth(v, GOLD) + sparks)


def philosophers_stone():
    v = round_flask()
    inner = (surface(v, CRIMSON_LT) +
             bubbles([(94, 190, 6, CRIMSON_LT), (164, 182, 5, CRIMSON_LT)]) +
             "".join(path(f"M{v.emblem[0] + math.cos(a) * 44:.2f},{v.emblem[1] + math.sin(a) * 44:.2f} "
                          f"L{v.emblem[0] + math.cos(a) * 55:.2f},{v.emblem[1] + math.sin(a) * 55:.2f}",
                          "none", GOLD_LT, 6, opacity=0.55)
                     for a in [i * math.tau / 8 + math.tau / 16 for i in range(8)]) +
             gem(v.emblem[0], v.emblem[1] - 2, 36))
    defs, poured_in = contents(v, "philosophers_stone", "#7d2233", inner)
    return defs, (v.neck + v.body(GLASS, INK, 7) + poured_in + v.body("none", INK, 7) +
                  shine(v, 84, 132, 52) + metal_cap(v, GOLD, GOLD_LT))


def unstable_concoction():
    v = conical_flask()
    v.level = 128
    # Three layers that never mixed -- the thing is built up a card at a time, and looks it.
    bands = (rect(0, 128, BOX, 30, 0, VIOLET, "none", 0) +
             rect(0, 158, BOX, 30, 0, CRIMSON, "none", 0) +
             rect(0, 188, BOX, 60, 0, ACID, "none", 0))
    inner = (bands + surface(v, "#c9a6f0") +
             bubbles([(92, 200, 7, ACID), (164, 194, 6, ACID), (128, 216, 5, "#f2f0c8")]) +
             bolt(v.emblem[0], v.emblem[1] - 6, 34, "#f2f0c8"))
    defs, poured_in = contents(v, "unstable_concoction", VIOLET, inner)
    fizz = (circle(C - 26, 34, 7, ACID, INK, 4) + circle(C + 6, 20, 10, ACID, INK, 4) +
            circle(C + 32, 38, 6, ACID, INK, 4))
    return defs, (v.neck + v.body(GLASS, INK, 7) + poured_in + v.body("none", INK, 7) +
                  shine(v, 92, 150, 48) + open_mouth(v, VIOLET) + fizz)


POTIONS = {
    # Paladin
    "anointing_oil": anointing_oil,
    "vial_of_verdict": vial_of_verdict,
    "sanctified_draught": sanctified_draught,
    # Gunslinger
    "speedloader_flask": speedloader_flask,
    "sightline_tonic": sightline_tonic,
    "ghost_smoke": ghost_smoke,
    # Alchemist
    "solvent_flask": solvent_flask,
    "aurum_tincture": aurum_tincture,
    "poison_potion": poison_potion,
    "poison_ampoule": poison_ampoule,
    "residual_reagent": residual_reagent,
    "panacea_of_plenty": panacea_of_plenty,
    "philosophers_stone": philosophers_stone,
    "unstable_concoction": unstable_concoction,
}


# ------------------------------------------------------------------ rendering


def markup(key):
    defs, body = POTIONS[key]()
    return svg(body, defs)


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


def silhouette(image):
    """White where the art is, transparent where it is not. The outline/ files."""
    alpha = image.getchannel("A").point(lambda v: 255 if v > 96 else 0)
    out = Image.new("RGBA", image.size, (255, 255, 255, 255))
    out.putalpha(alpha)
    return out


def write(image, path_out):
    os.makedirs(os.path.dirname(path_out), exist_ok=True)
    image.save(path_out)


def build(only=None):
    made = []
    for key in POTIONS:
        if only and key not in only:
            continue
        art = render(markup(key), SIZE)
        write(art, os.path.join(POTIONS_DIR, f"{key}.png"))
        write(silhouette(art), os.path.join(POTIONS_DIR, "outline", f"{key}.png"))
        made.append(key)
    return made


def contact_sheet(path_out, columns=5, cell=160):
    """Every icon in one image, for eyeballing the set as a set."""
    keys = list(POTIONS)
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

    unknown = set(args.keys) - set(POTIONS)
    if unknown:
        sys.exit(f"unknown potion key(s): {', '.join(sorted(unknown))}")

    for key in build(set(args.keys) or None):
        print(f"potion {key}")

    if args.sheet:
        contact_sheet(args.sheet)
        print(f"sheet  {args.sheet}")


if __name__ == "__main__":
    main()
