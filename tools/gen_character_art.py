#!/usr/bin/env python3
"""
Generate placeholder character UI art for a HelloSpire character.

This is scaffolding art, not final art. Its job is to make each character
visually distinct so the asset pipeline can be verified end to end, and to
produce correctly-sized files that real art can later drop straight into.

Everything is drawn at 4x and downsampled, which is what gives clean edges.

Usage:
    python tools/gen_character_art.py paladin --motif shield --color e8c46a
    python tools/gen_character_art.py alchemist --motif flask --color 6ad48a
    python tools/gen_character_art.py gunslinger --motif star --color d4703c

Sizes are dictated by the game and must not change:
    character_icon        128x128
    map_marker            128x128
    char_select           132x195
    char_select_locked    132x195
    big_energy             74x74
    text_energy            24x24
"""
import argparse
import os
from PIL import Image, ImageDraw

SS = 4  # supersampling factor


def hex_rgb(h):
    h = h.lstrip("#")
    return tuple(int(h[i:i + 2], 16) for i in (0, 2, 4))


def shade(rgb, f):
    """f < 1 darkens, f > 1 lightens."""
    return tuple(max(0, min(255, int(c * f))) for c in rgb)


def new(w, h):
    return Image.new("RGBA", (w * SS, h * SS), (0, 0, 0, 0))


def finish(img, w, h):
    return img.resize((w, h), Image.LANCZOS)


def shield_pts(w, h):
    """Heater-shield outline scaled into a w*h box.

    Built as an explicit right edge swept from the top corner down to the
    bottom point, then mirrored about x=0.5 to come back up. Mirroring the
    parametric curve (rather than hand-listing points) is what keeps the
    two sides symmetric.
    """
    def p(x, y):
        return (x * w, y * h)

    N = 28
    pts = [p(0.10, 0.08), p(0.90, 0.08)]
    for i in range(N + 1):                      # right edge, top -> point
        t = i / N
        pts.append(p(0.90 - 0.40 * (t ** 2.2), 0.08 + 0.88 * t))
    for i in range(N, -1, -1):                  # left edge, point -> top
        t = i / N
        pts.append(p(0.10 + 0.40 * (t ** 2.2), 0.08 + 0.88 * t))
    return pts


def flask_pts(w, h):
    """Erlenmeyer flask outline."""
    def p(x, y):
        return (x * w, y * h)
    return [
        p(0.38, 0.08), p(0.62, 0.08), p(0.62, 0.38),
        p(0.88, 0.86), p(0.80, 0.94), p(0.20, 0.94),
        p(0.12, 0.86), p(0.38, 0.38),
    ]


def star_pts(w, h, points=6):
    """Sheriff-style star."""
    import math
    cx, cy = w / 2, h / 2
    r_out, r_in = min(w, h) * 0.46, min(w, h) * 0.20
    pts = []
    for i in range(points * 2):
        a = math.pi * i / points - math.pi / 2
        r = r_out if i % 2 == 0 else r_in
        pts.append((cx + r * math.cos(a), cy + r * math.sin(a)))
    return pts


MOTIFS = {"shield": shield_pts, "flask": flask_pts, "star": star_pts}


def draw_motif(dr, motif, w, h, base, dark, light, cross=False):
    if motif == "shield":
        pts = shield_pts(w, h)
    elif motif == "flask":
        pts = flask_pts(w, h)
    else:
        pts = star_pts(w, h)
    dr.polygon(pts, fill=base, outline=dark)
    # thicken the outline by redrawing the perimeter
    dr.line(pts + [pts[0]], fill=dark, width=max(2, int(w * 0.02)))

    if motif == "shield" and cross:
        cw = w * 0.10
        dr.rectangle([w * 0.5 - cw / 2, h * 0.20, w * 0.5 + cw / 2, h * 0.72], fill=light)
        dr.rectangle([w * 0.26, h * 0.34 - cw / 2, w * 0.74, h * 0.34 + cw / 2], fill=light)
    elif motif == "flask":
        # liquid line
        dr.polygon([(w * 0.20, h * 0.72), (w * 0.80, h * 0.72),
                    (w * 0.86, h * 0.86), (w * 0.80, h * 0.94),
                    (w * 0.20, h * 0.94), (w * 0.14, h * 0.86)], fill=light)
    else:
        r = min(w, h) * 0.10
        dr.ellipse([w / 2 - r, h / 2 - r, w / 2 + r, h / 2 + r], fill=light)


def orb(w, h, base, dark, light):
    img = new(w, h)
    dr = ImageDraw.Draw(img)
    W, H = w * SS, h * SS
    m = W * 0.06
    dr.ellipse([m, m, W - m, H - m], fill=base, outline=dark, width=max(2, int(W * 0.05)))
    # highlight
    dr.ellipse([W * 0.26, H * 0.20, W * 0.52, H * 0.44], fill=light)
    return finish(img, w, h)


def icon(w, h, motif, base, dark, light, locked=False):
    img = new(w, h)
    dr = ImageDraw.Draw(img)
    W, H = w * SS, h * SS
    if locked:
        base, light = shade(base, 0.32), shade(light, 0.38)
        dark = shade(dark, 0.5)
    draw_motif(dr, motif, W, H, base + (255,), dark + (255,), light + (255,), cross=True)
    return finish(img, w, h)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("character", help="asset folder name, e.g. paladin")
    ap.add_argument("--motif", choices=sorted(MOTIFS), default="shield")
    ap.add_argument("--color", default="e8c46a", help="hex, no #")
    ap.add_argument("--out", default=None)
    a = ap.parse_args()

    base = hex_rgb(a.color)
    dark, light = shade(base, 0.42), shade(base, 1.35)

    out = a.out or os.path.join(
        os.path.dirname(os.path.dirname(os.path.abspath(__file__))),
        "HelloSpire", "images", "charui", a.character)
    os.makedirs(out, exist_ok=True)

    files = {
        "character_icon.png":     icon(128, 128, a.motif, base, dark, light),
        "map_marker.png":         icon(128, 128, a.motif, base, dark, light),
        "char_select.png":        icon(132, 195, a.motif, base, dark, light),
        "char_select_locked.png": icon(132, 195, a.motif, base, dark, light, locked=True),
        "big_energy.png":         orb(74, 74, base, dark, light),
        "text_energy.png":        orb(24, 24, base, dark, light),
    }
    for name, img in files.items():
        p = os.path.join(out, name)
        img.save(p)
        print(f"  {name:24} {img.size[0]}x{img.size[1]}  -> {p}")
    print(f"\n{len(files)} files written for '{a.character}' ({a.motif}, #{a.color})")


if __name__ == "__main__":
    main()
