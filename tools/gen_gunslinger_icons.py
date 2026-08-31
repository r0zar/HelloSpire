#!/usr/bin/env python3
"""
Generate the Gunslinger's power and relic icons.

Unlike tools/gen_card_art.py, this is not scaffolding: it is the art. Every icon is a
handful of flat vector shapes, so the whole set is one file that can be re-rendered at
any size and adjusted by editing a shape instead of repainting a bitmap.

Two families, following what the rest of the pack already does:

  * Keyword powers -- Cylinder, Deadeye, Armor, Dodge -- are flat glyphs on transparent,
    the way the base game draws Strength and Dexterity and the way the Paladin's Spirit
    icon does. These four read as stats the character has, not as buffs it was granted.
  * Engine powers get the medallion disc the Alchemist's fifteen use: brown disc, brass
    ring, pale glyph. They are things a card gave you, and the disc says so.

Relics are flat objects on transparent, matching the Paladin's Holy Book and the
Necrobinder-style Chained Gauntlet. Their _outline companions are generated from the
rendered alpha, so a silhouette can never drift from the art it belongs to.

Requires rsvg-convert (brew install librsvg) and Pillow.

Usage:
    python tools/gen_gunslinger_icons.py            # everything
    python tools/gen_gunslinger_icons.py deadeye    # one icon, by key
    python tools/gen_gunslinger_icons.py --sheet    # also write a contact sheet
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
RELICS_DIR = os.path.join(ROOT, "HelloSpire", "images", "relics")

# Sizes the game asks for. Powers: 128 in the status bar, 256 in the tooltip.
# Relics: 128 in the tray, 256 in the tooltip, 128 for the silhouette.
POWER_SMALL, POWER_BIG = 128, 256
RELIC_SMALL, RELIC_BIG = 128, 256

BOX = 256  # every glyph is authored in a 256x256 viewBox
C = BOX / 2

# ------------------------------------------------------------------ palette

INK       = "#241809"  # the outline every shape carries
BRASS     = "#d9a05b"  # the Gunslinger's colour, from Gunslinger.Color
BRASS_LT  = "#f2d9a8"
BRASS_DK  = "#8a6430"
LEAD      = "#9aa7b4"
LEAD_DK   = "#5d6a77"
STEEL     = "#b9c4ce"
LEATHER   = "#7a4a28"
LEATHER_DK= "#4a2c17"
SMOKE     = "#ddd6ca"
SMOKE_DK  = "#8d8478"
RUST      = "#b4552f"
BONE      = "#efe3cd"
DISC_DARK = "#3a2612"

# ------------------------------------------------------------------ svg helpers


def svg(body, defs=""):
    return (
        f'<svg xmlns="http://www.w3.org/2000/svg" width="{BOX}" height="{BOX}" '
        f'viewBox="0 0 {BOX} {BOX}">{defs}{body}</svg>'
    )


def circle(cx, cy, r, fill="none", stroke=INK, sw=0):
    return (f'<circle cx="{cx:.2f}" cy="{cy:.2f}" r="{r:.2f}" fill="{fill}" '
            f'stroke="{stroke}" stroke-width="{sw}"/>')


def rect(x, y, w, h, rx=0, fill="none", stroke=INK, sw=0, rot=None, cx=None, cy=None):
    transform = ""
    if rot is not None:
        transform = f' transform="rotate({rot} {cx if cx is not None else x + w / 2:.2f} {cy if cy is not None else y + h / 2:.2f})"'
    return (f'<rect x="{x:.2f}" y="{y:.2f}" width="{w:.2f}" height="{h:.2f}" rx="{rx}" '
            f'fill="{fill}" stroke="{stroke}" stroke-width="{sw}"{transform}/>')


def path(d, fill="none", stroke=INK, sw=0, cap="round", join="round"):
    return (f'<path d="{d}" fill="{fill}" stroke="{stroke}" stroke-width="{sw}" '
            f'stroke-linecap="{cap}" stroke-linejoin="{join}"/>')


def group(content, transform=""):
    t = f' transform="{transform}"' if transform else ""
    return f"<g{t}>{content}</g>"


def polygon(points, fill="none", stroke=INK, sw=0):
    pts = " ".join(f"{x:.2f},{y:.2f}" for x, y in points)
    return (f'<polygon points="{pts}" fill="{fill}" stroke="{stroke}" '
            f'stroke-width="{sw}" stroke-linejoin="round"/>')


def star(cx, cy, points, outer, inner, fill, stroke=INK, sw=6, phase=-math.pi / 2):
    pts = []
    for i in range(points * 2):
        r = outer if i % 2 == 0 else inner
        a = phase + i * math.pi / points
        pts.append((cx + math.cos(a) * r, cy + math.sin(a) * r))
    return polygon(pts, fill, stroke, sw)


def ring_of(n, cx, cy, orbit, make, phase=-math.pi / 2):
    """`make(x, y, index)` once per position around a circle."""
    out = []
    for i in range(n):
        a = phase + i * math.tau / n
        out.append(make(cx + math.cos(a) * orbit, cy + math.sin(a) * orbit, i))
    return "".join(out)


def cartridge(cx, cy, length=86, width=30, rot=0, case=BRASS, tip=LEAD):
    """A round: brass case, rim at the base, lead tip at the top. Points up at rot=0."""
    half_w = width / 2
    tip_h = length * 0.30
    body_top = cy - length / 2 + tip_h
    body_bot = cy + length / 2
    body = (
        f"M{cx - half_w:.2f},{body_top:.2f} L{cx + half_w:.2f},{body_top:.2f} "
        f"L{cx + half_w:.2f},{body_bot - 6:.2f} Q{cx + half_w:.2f},{body_bot:.2f} "
        f"{cx + half_w - 6:.2f},{body_bot:.2f} L{cx - half_w + 6:.2f},{body_bot:.2f} "
        f"Q{cx - half_w:.2f},{body_bot:.2f} {cx - half_w:.2f},{body_bot - 6:.2f} Z"
    )
    nose = (
        f"M{cx - half_w:.2f},{body_top:.2f} "
        f"Q{cx - half_w:.2f},{cy - length / 2:.2f} {cx:.2f},{cy - length / 2:.2f} "
        f"Q{cx + half_w:.2f},{cy - length / 2:.2f} {cx + half_w:.2f},{body_top:.2f} Z"
    )
    rim = rect(cx - half_w - 4, body_bot - 16, width + 8, 14, 3, case, INK, 5)
    content = path(body, case, INK, 6) + path(nose, tip, INK, 6) + rim
    return group(content, f"rotate({rot} {cx:.2f} {cy:.2f})")


def shield(cx, cy, w=110, h=126, fill=STEEL, stroke=INK, sw=7):
    """A heater shield: square shoulders, straight sides, a point at the bottom."""
    hw, hh = w / 2, h / 2
    d = (f"M{cx - hw:.2f},{cy - hh + 14:.2f} "
         f"Q{cx - hw:.2f},{cy - hh:.2f} {cx - hw + 14:.2f},{cy - hh:.2f} "
         f"L{cx + hw - 14:.2f},{cy - hh:.2f} "
         f"Q{cx + hw:.2f},{cy - hh:.2f} {cx + hw:.2f},{cy - hh + 14:.2f} "
         f"L{cx + hw:.2f},{cy + hh * 0.10:.2f} "
         f"Q{cx + hw:.2f},{cy + hh * 0.62:.2f} {cx:.2f},{cy + hh:.2f} "
         f"Q{cx - hw:.2f},{cy + hh * 0.62:.2f} {cx - hw:.2f},{cy + hh * 0.10:.2f} Z")
    return path(d, fill, stroke, sw)


def medallion():
    """The disc every engine power sits on."""
    defs = (
        '<radialGradient id="disc" cx="38%" cy="32%" r="78%">'
        f'<stop offset="0%" stop-color="#7d5626"/>'
        f'<stop offset="100%" stop-color="{DISC_DARK}"/>'
        "</radialGradient>"
    )
    body = (circle(C, C, 118, "url(#disc)", INK, 9)
            + circle(C, C, 100, "none", BRASS, 7))
    return defs, body


# ------------------------------------------------------------------ keyword powers
# Flat, transparent, no disc: these four are stats the character has.


def glyph_cylinder():
    holes = ring_of(6, C, C, 54, lambda x, y, i: circle(x, y, 21, BRASS_DK if i else BRASS, INK, 6))
    return circle(C, C, 96, "#2f2417", BRASS, 12) + holes + circle(C, C, 12, BRASS_LT, INK, 5)


def glyph_deadeye():
    ticks = "".join(
        rect(C - 6, C - 108, 12, 30, 6, BRASS, INK, 5, rot=a, cx=C, cy=C)
        for a in (0, 90, 180, 270)
    )
    return (circle(C, C, 74, "none", BRASS, 14) + circle(C, C, 74, "none", INK, 4)
            + ticks + circle(C, C, 16, RUST, INK, 5))


def glyph_armor():
    band = rect(C - 62, C - 18, 124, 30, 8, LEAD, INK, 6)
    rivets = "".join(circle(C + dx, C - 52, 10, BRASS, INK, 5) for dx in (-38, 0, 38))
    return shield(C, C - 4, 138, 168, STEEL, INK, 9) + band + rivets


def glyph_dodge():
    # Three tapered slipstream strokes and the shape that got out of the way.
    lines = "".join(
        path(f"M{C - 96 + i * 6:.2f},{C - 46 + i * 46:.2f} L{C - 6 + i * 10:.2f},{C - 62 + i * 46:.2f}",
             "none", SMOKE_DK, 20)
        + path(f"M{C - 92 + i * 6:.2f},{C - 46 + i * 46:.2f} L{C - 10 + i * 10:.2f},{C - 61 + i * 46:.2f}",
               "none", SMOKE, 12)
        for i in range(3)
    )
    ghost = circle(C + 62, C, 40, "none", SMOKE_DK, 10)
    solid = circle(C + 62, C, 26, SMOKE, INK, 6)
    return lines + ghost + solid


# ------------------------------------------------------------------ engine powers
# Medallion glyphs. Everything is drawn inside a radius of about 76.


def glyph_stacked_chamber():
    return (circle(C, C + 44, 34, "#2f2417", BRASS, 8)
            + cartridge(C, C - 26, 88, 32, rot=180)
            + path(f"M{C - 20:.2f},{C + 16:.2f} L{C:.2f},{C + 34:.2f} L{C + 20:.2f},{C + 16:.2f}",
                   "none", BRASS_LT, 8))


def glyph_block_next_turn():
    arrow = path(f"M{C - 34:.2f},{C + 4:.2f} A34,34 0 1 1 {C + 12:.2f},{C + 32:.2f}",
                 "none", BRASS_LT, 11)
    head = polygon([(C + 4, C + 14), (C + 26, C + 40), (C - 4, C + 42)], BRASS_LT, INK, 4)
    return shield(C, C - 6, 96, 112, LEAD, INK, 7) + arrow + head


def glyph_bottomless_bandolier():
    strap = rect(C - 92, C + 26, 184, 34, 8, LEATHER, INK, 7, rot=-12, cx=C, cy=C + 40)
    rounds = "".join(cartridge(C - 54 + i * 54, C - 8 + i * -11, 78, 28, rot=-12)
                     for i in range(3))
    return strap + rounds


def glyph_debilitating_presence():
    broken = (path(f"M{C - 62:.2f},{C - 26:.2f} A62,62 0 0 1 {C + 62:.2f},{C - 26:.2f}",
                   "none", SMOKE_DK, 13)
              + path(f"M{C - 54:.2f},{C + 34:.2f} A62,62 0 0 0 {C - 14:.2f},{C + 60:.2f}",
                     "none", SMOKE_DK, 13)
              + path(f"M{C + 54:.2f},{C + 34:.2f} A62,62 0 0 1 {C + 14:.2f},{C + 60:.2f}",
                     "none", SMOKE_DK, 13))
    arrow = (path(f"M{C:.2f},{C - 48:.2f} L{C:.2f},{C + 22:.2f}", "none", RUST, 16)
             + polygon([(C - 30, C + 12), (C + 30, C + 12), (C, C + 52)], RUST, INK, 5))
    return broken + arrow


def glyph_dry_fire():
    hammer = polygon([(C - 26, C - 92), (C + 26, C - 92), (C + 16, C - 26),
                      (C, C - 8), (C - 16, C - 26)], LEAD, INK, 7)
    chamber = circle(C, C + 36, 38, "#2f2417", BRASS, 9)
    sparks = "".join(
        path(f"M{C + dx:.2f},{C + 2:.2f} L{C + dx * 1.9:.2f},{C - 22:.2f}", "none", BRASS_LT, 9)
        for dx in (-30, 30)
    ) + path(f"M{C:.2f},{C - 2:.2f} L{C:.2f},{C - 28:.2f}", "none", BRASS_LT, 9)
    return chamber + sparks + hammer


def glyph_gunfighters_rhythm():
    dashes = ring_of(6, C, C, 62,
                     lambda x, y, i: rect(x - 8, y - 22, 16, 44, 8,
                                          BRASS_LT if i == 5 else BRASS_DK, INK, 5,
                                          rot=math.degrees(math.atan2(y - C, x - C)) + 90,
                                          cx=x, cy=y))
    return circle(C, C, 62, "none", BRASS_DK, 5) + dashes + circle(C, C, 16, BRASS, INK, 5)


def glyph_hard_leather():
    patch = rect(C - 66, C - 60, 132, 120, 26, LEATHER, INK, 8)
    stitches = "".join(
        rect(C - 48 + i * 24, C - 44, 6, 16, 3, BRASS_LT, "none", 0) +
        rect(C - 48 + i * 24, C + 30, 6, 16, 3, BRASS_LT, "none", 0)
        for i in range(5)
    )
    grain = path(f"M{C - 40:.2f},{C:.2f} Q{C:.2f},{C - 20:.2f} {C + 40:.2f},{C:.2f}",
                 "none", LEATHER_DK, 8)
    return patch + stitches + grain


def glyph_iron_will():
    top = rect(C - 78, C - 46, 156, 34, 8, LEAD, INK, 7)
    horn = polygon([(C - 78, C - 46), (C - 108, C - 30), (C - 78, C - 12)], LEAD, INK, 7)
    waist = polygon([(C - 34, C - 12), (C + 34, C - 12), (C + 22, C + 26), (C - 22, C + 26)],
                    LEAD_DK, INK, 7)
    base = rect(C - 58, C + 26, 116, 30, 8, LEAD, INK, 7)
    return horn + top + waist + base


def glyph_loaded_dice():
    die = rect(C - 62, C - 62, 124, 124, 20, BONE, INK, 8, rot=-14, cx=C, cy=C)
    pips = "".join(circle(C + dx, C + dy, 11, INK) for dx, dy in
                   [(-34, -34), (34, -34), (-34, 0), (34, 0), (-34, 34), (34, 34)])
    return die + group(pips, f"rotate(-14 {C} {C})")


def glyph_never_still():
    shank = rect(C - 14, C - 96, 28, 78, 12, LEAD, INK, 6)
    rowel = star(C, C + 26, 8, 74, 34, BRASS, INK, 7)
    return shank + rowel + circle(C, C + 26, 15, DISC_DARK, INK, 5)


def glyph_quickdraw_legend():
    bolt = polygon([(C + 26, C - 92), (C - 44, C + 12), (C - 4, C + 12),
                    (C - 26, C + 92), (C + 46, C - 16), (C + 6, C - 16)],
                   BRASS_LT, INK, 7)
    return bolt


def glyph_ride_together():
    return (cartridge(C, C, 156, 34, rot=-34) + cartridge(C, C, 156, 34, rot=34))


def glyph_sixth_shot():
    holes = ring_of(6, C, C, 56, lambda x, y, i: circle(x, y, 17, BRASS_DK, INK, 5))
    burst = star(C, C - 56, 8, 44, 17, RUST, INK, 5)
    return circle(C, C, 84, "none", BRASS, 9) + holes + burst


def glyph_smoke_and_lead():
    trail = (path(f"M{C - 62:.2f},{C + 62:.2f} Q{C - 10:.2f},{C + 34:.2f} {C - 30:.2f},{C - 4:.2f} "
                  f"Q{C - 48:.2f},{C - 42:.2f} {C + 4:.2f},{C - 56:.2f}", "none", SMOKE, 18))
    return trail + cartridge(C + 40, C - 34, 92, 34, rot=32)


def glyph_sure_hand():
    bar = rect(C - 88, C - 4, 176, 24, 10, LEAD, INK, 7)
    bubble = circle(C, C + 8, 15, BRASS_LT, INK, 5)
    posts = "".join(rect(C + dx - 8, C + 20, 16, 40, 6, LEAD_DK, INK, 5) for dx in (-58, 58))
    return posts + bar + bubble + cartridge(C, C - 54, 72, 26)


def glyph_untouchable():
    spark = star(C + 52, C - 52, 4, 46, 13, BRASS_LT, INK, 5)
    return shield(C - 10, C + 6, 112, 130, LEAD, INK, 7) + spark


# ------------------------------------------------------------------ relics
# Flat objects on transparent, the way the Paladin's Holy Book is drawn. The _outline
# companion is derived from the alpha of these, further down.


def grip_path(fill=LEATHER, sw=8, tilt=-10):
    """A single-action grip: flat where it meets the frame, swelling to a rounded butt."""
    d = ("M100,36 L168,36 Q196,110 176,178 Q158,220 120,214 "
         "Q96,196 94,150 Q88,90 100,36 Z")
    return group(path(d, fill, INK, sw), f"rotate({tilt} {C} {C})")


def revolver(barrel=STEEL, frame=LEAD, grip=LEATHER, accent=BRASS, tilt=-6):
    """A side-on six-shooter, muzzle left, filling the frame."""
    barrel_body = rect(12, 92, 116, 52, 14, barrel, INK, 7)
    muzzle = circle(24, 118, 13, DISC_DARK, INK, 5)
    sight = polygon([(38, 92), (52, 70), (66, 92)], barrel, INK, 5)

    frame_block = rect(112, 86, 118, 64, 14, frame, INK, 7)
    cyl = circle(154, 118, 42, frame, INK, 8)
    flutes = ring_of(6, 154, 118, 25, lambda x, y, i: circle(x, y, 9, DISC_DARK, accent, 3))

    hammer = polygon([(206, 86), (244, 58), (256, 84), (222, 104)], accent, INK, 5)
    guard = path("M182,150 Q190,204 232,196", "none", frame, 12)
    trigger = path("M196,158 Q200,180 214,182", "none", accent, 8)
    grip_shape = path("M190,140 L248,140 Q258,210 200,250 Q162,200 190,140 Z", grip, INK, 7)

    content = (barrel_body + sight + muzzle + grip_shape + frame_block + cyl + flutes
               + guard + trigger + hammer)
    return group(content, f"rotate({tilt} {C} {C})")


def glyph_old_iron():
    return revolver(STEEL, LEAD, LEATHER, BRASS)


def glyph_true_iron():
    return (revolver(BRASS, BRASS_LT, LEATHER_DK, BRASS_LT)
            + star(56, 60, 5, 34, 15, BRASS_LT, INK, 5))


def glyph_oiled_rag():
    cloth = path("M22,140 Q50,72 104,98 Q140,116 166,78 Q212,88 224,132 "
                 "Q234,186 176,196 Q122,206 90,178 Q48,198 22,140 Z", BONE, INK, 8)
    folds = (path("M58,136 Q90,160 126,148", "none", SMOKE_DK, 8)
             + path("M100,176 Q140,188 176,174", "none", SMOKE_DK, 8))
    stain = path("M106,128 Q150,112 184,140 Q148,166 106,128 Z", BRASS_DK, "none", 0)
    return cloth + stain + folds + cartridge(150, 128, 96, 34, rot=64)


def glyph_tin_badge():
    return (star(C, C, 5, 104, 46, LEAD, INK, 8)
            + circle(C, C, 34, STEEL, INK, 6)
            + star(C, C, 5, 24, 10, LEAD_DK, "none", 0))


def glyph_spare_speedloader():
    plate = circle(C, C + 8, 88, LEAD, INK, 8)
    rounds = ring_of(6, C, C + 8, 52, lambda x, y, i: circle(x, y, 20, BRASS, INK, 5))
    knob = rect(C - 16, C - 100, 32, 44, 10, STEEL, INK, 6)
    return knob + plate + rounds + circle(C, C + 8, 16, STEEL, INK, 5)


def glyph_longcoat_plates():
    coat = path(f"M{C - 74:.2f},{C - 88:.2f} L{C + 74:.2f},{C - 88:.2f} "
                f"L{C + 58:.2f},{C + 92:.2f} L{C - 58:.2f},{C + 92:.2f} Z", LEATHER, INK, 8)
    plates = "".join(rect(C - 44, C - 60 + i * 50, 88, 38, 8, LEAD, INK, 6) for i in range(3))
    return coat + plates


def glyph_lucky_coin():
    return (circle(C, C, 96, BRASS, INK, 9)
            + circle(C, C, 76, "none", BRASS_DK, 6)
            + circle(C + 18, C - 12, 26, DISC_DARK, INK, 6)
            + path(f"M{C - 44:.2f},{C + 44:.2f} L{C - 8:.2f},{C + 10:.2f}", "none", BRASS_DK, 8))


def glyph_engraved_hammer():
    head = path("M64,34 L192,34 Q206,44 200,72 L172,120 "
                "Q158,140 128,140 Q98,140 84,120 L56,72 Q50,44 64,34 Z", STEEL, INK, 8)
    spur = rect(58, 18, 140, 28, 10, LEAD, INK, 6)
    checker = "".join(path(f"M{78 + i * 20},22 L{78 + i * 20},42", "none", LEAD_DK, 4)
                      for i in range(6))
    scroll = (path("M92,66 Q128,44 164,66", "none", BRASS, 7)
              + path("M104,92 Q128,110 152,92", "none", BRASS, 7)
              + circle(128, 78, 11, BRASS, INK, 4))
    rays = "".join(
        path(f"M{128 + dx},{158} L{128 + dx * 1.7},{212}", "none", BRASS_LT, 10)
        for dx in (-46, 0, 46))
    return rays + head + spur + checker + scroll


def glyph_ivory_handle():
    tang = group(rect(96, 12, 76, 34, 8, LEAD, INK, 6), f"rotate(-10 {C} {C})")
    detail = group(
        path("M122,80 Q158,124 140,182", "none", BRASS_DK, 7)
        + path("M156,92 Q126,136 150,192", "none", BRASS_DK, 7)
        + circle(136, 122, 15, BRASS, INK, 5)
        + path("M128,122 L144,122", "none", INK, 4),
        f"rotate(-10 {C} {C})")
    return tang + grip_path(BONE, 9) + detail


# ------------------------------------------------------------------ the set

KEYWORD_POWERS = {
    "cylinder": glyph_cylinder,
    "deadeye": glyph_deadeye,
    "armor": glyph_armor,
    "dodge": glyph_dodge,
}

ENGINE_POWERS = {
    "stacked_chamber": glyph_stacked_chamber,
    "block_next_turn": glyph_block_next_turn,
    "bottomless_bandolier": glyph_bottomless_bandolier,
    "debilitating_presence": glyph_debilitating_presence,
    "dry_fire": glyph_dry_fire,
    "gunfighters_rhythm": glyph_gunfighters_rhythm,
    "hard_leather": glyph_hard_leather,
    "iron_will": glyph_iron_will,
    "loaded_dice": glyph_loaded_dice,
    "never_still": glyph_never_still,
    "quickdraw_legend": glyph_quickdraw_legend,
    "ride_together": glyph_ride_together,
    "sixth_shot": glyph_sixth_shot,
    "smoke_and_lead": glyph_smoke_and_lead,
    "sure_hand": glyph_sure_hand,
    "untouchable": glyph_untouchable,
}

RELICS = {
    "old_iron": glyph_old_iron,
    "true_iron": glyph_true_iron,
    "oiled_rag": glyph_oiled_rag,
    "tin_badge": glyph_tin_badge,
    "spare_speedloader": glyph_spare_speedloader,
    "longcoat_plates": glyph_longcoat_plates,
    "lucky_coin": glyph_lucky_coin,
    "engraved_hammer": glyph_engraved_hammer,
    "ivory_handle": glyph_ivory_handle,
}


# ------------------------------------------------------------------ rendering


def render(markup, size):
    """SVG string -> RGBA Image, via rsvg-convert."""
    with tempfile.NamedTemporaryFile("w", suffix=".svg", delete=False) as handle:
        handle.write(markup)
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
    """White where the art is, transparent where it is not. The relic _outline files."""
    alpha = image.getchannel("A").point(lambda v: 255 if v > 96 else 0)
    out = Image.new("RGBA", image.size, (255, 255, 255, 0))
    out.putalpha(alpha)
    white = Image.new("RGBA", image.size, (255, 255, 255, 255))
    white.putalpha(alpha)
    return white


def power_svg(key):
    if key in KEYWORD_POWERS:
        return svg(KEYWORD_POWERS[key]())
    defs, disc = medallion()
    return svg(disc + ENGINE_POWERS[key](), defs)


def write(image, path_out):
    os.makedirs(os.path.dirname(path_out), exist_ok=True)
    image.save(path_out)


def build(only=None):
    made = []

    powers = {**KEYWORD_POWERS, **ENGINE_POWERS}
    for key in powers:
        if only and key not in only:
            continue
        markup = power_svg(key)
        name = f"{key}_power.png"
        write(render(markup, POWER_BIG), os.path.join(POWERS_DIR, "big", name))
        write(render(markup, POWER_SMALL), os.path.join(POWERS_DIR, name))
        made.append(("power", key))

    for key, glyph in RELICS.items():
        if only and key not in only:
            continue
        markup = svg(glyph())
        big = render(markup, RELIC_BIG)
        small = render(markup, RELIC_SMALL)
        write(big, os.path.join(RELICS_DIR, "big", f"{key}.png"))
        write(small, os.path.join(RELICS_DIR, f"{key}.png"))
        write(silhouette(small), os.path.join(RELICS_DIR, f"{key}_outline.png"))
        made.append(("relic", key))

    return made


def contact_sheet(path_out, columns=8, cell=128):
    """Every icon in one image, for eyeballing the set as a set."""
    tiles = []
    for key in {**KEYWORD_POWERS, **ENGINE_POWERS}:
        tiles.append(render(power_svg(key), cell))
    for key, glyph in RELICS.items():
        tiles.append(render(svg(glyph()), cell))

    rows = (len(tiles) + columns - 1) // columns
    sheet = Image.new("RGBA", (columns * cell, rows * cell), (32, 26, 20, 255))
    for i, tile in enumerate(tiles):
        sheet.alpha_composite(tile, ((i % columns) * cell, (i // columns) * cell))
    sheet.convert("RGB").save(path_out)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("keys", nargs="*", help="icon keys to rebuild (default: all)")
    parser.add_argument("--sheet", default=None, help="also write a contact sheet here")
    args = parser.parse_args()

    if subprocess.run(["which", "rsvg-convert"], capture_output=True).returncode != 0:
        sys.exit("rsvg-convert not found; brew install librsvg")

    made = build(set(args.keys) or None)
    for kind, key in made:
        print(f"{kind:6} {key}")

    if args.sheet:
        contact_sheet(args.sheet)
        print(f"sheet  {args.sheet}")


if __name__ == "__main__":
    main()
