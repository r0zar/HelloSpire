# Art pipeline

How art gets from a file on disk into the game. Verified end to end on the Paladin.

## The loop

```
edit/generate PNG  →  dotnet publish  →  relaunch game
```

`dotnet publish` runs Godot headless, which reimports any changed image, packs it into
`HelloSpire.pck`, and copies the result into the game's `mods/` folder. **Close the game
first** — a running instance holds `HelloSpire.dll` open and the copy step fails with
`MSB3021`.

## Where art lives

```
HelloSpire/images/
  charui/<character>/    per-character UI — the only tree split by character
  card_portraits/        shared;  <cardclass>.png  and  big/<cardclass>.png
  relics/                shared;  <relicclass>.png and <relicclass>_outline.png
  potions/               shared
  powers/                shared
```

Card, relic and potion art resolves by **class name** (`Id.Entry`, lowercased), which is
already unique mod-wide — `PaladinStrike` and `GunslingerStrike` cannot collide — so those
trees stay shared. Character UI uses fixed filenames per character, so it is the one tree
namespaced by character folder.

Missing art degrades gracefully: the helpers in `StringExtensions.cs` fall back to the
generic placeholder and log `Could not find ... image path`, rather than crashing.

## Required sizes

| Asset | Size |
|---|---|
| `character_icon.png`, `map_marker.png` | 128×128 |
| `char_select.png`, `char_select_locked.png` | 132×195 |
| `big_energy.png` | 74×74 |
| `text_energy.png` | 24×24 |
| Card art, normal | 1000×760 (500×380 also scales) |
| Card art, full-art | 606×852 |
| Card art small variants | 250×190 normal, 250×350 full-art |

Ship both large and small card variants — the small ones are a performance measure.

## Generating placeholder character art

`tools/gen_character_art.py` draws all six `charui` images at correct sizes, supersampled
4× for clean edges. Requires Pillow.

```
python tools/gen_character_art.py paladin    --motif shield --color e8c46a
python tools/gen_character_art.py alchemist  --motif flask  --color 6ad48a
python tools/gen_character_art.py gunslinger --motif star   --color d4703c
```

This is scaffolding, not final art. Its value is correct sizes and visual distinctness so
real art can drop straight in.

## Generating the Gunslinger's power and relic icons

`tools/gen_gunslinger_icons.py` is not scaffolding — it *is* the art. Every icon is a handful of
flat vector shapes rendered through `rsvg-convert`, so the set can be re-rendered at any size and
adjusted by editing a shape rather than repainting a bitmap. Requires `rsvg-convert`
(`brew install librsvg`) and Pillow.

```
python tools/gen_gunslinger_icons.py                    # all 20 powers and 9 relics
python tools/gen_gunslinger_icons.py deadeye old_iron   # just these
python tools/gen_gunslinger_icons.py --sheet /tmp/x.png # contact sheet, to judge the set as a set
```

Two families, following what the pack already does:

- **Keyword powers** — Cylinder, Deadeye, Armor, Dodge — are flat glyphs on transparent, the way
  the base game draws Strength and Dexterity and the way the Paladin's Spirit icon does. These
  four read as stats the character has, not as buffs it was granted.
- **Engine powers** get the medallion disc the Alchemist's fifteen use: brown disc, brass ring,
  pale glyph. They are things a card gave you, and the disc says so.

Relic `_outline` files are derived from the alpha of the relic art itself, so a silhouette can
never drift from the art it belongs to. Do not hand-edit them.

## Reference: extracting the base game's assets

Install [Godot RE Tools](https://github.com/GDRETools/gdsdecomp) (`winget install
GDRETools.gdsdecomp`), then:

```
gdre_tools.exe --headless --recover="<game>/SlayTheSpire2.pck" --output=<dir> \
  --include='res://images/**' --include='res://localization/**' \
  --include='res://.godot/**' --skip-checksum-check
```

**`res://.godot/**` is not optional.** The actual `.ctex` texture data lives there; without
it the recovery emits only `.import`/`.tres` metadata and reports
`Imported resources for export session: 0`. With it you get ~3,500 PNGs and all 645
localization JSONs.

Two things to expect:

- **Card art ships in texture atlases**, not one PNG per card. `images/atlases/` holds 169
  `.atlas`/`.spatlas` sheets; individual portraits are regions within them.
- `localization/eng/cards.json` is the best available reference for card description
  phrasing and formatting-variable syntax.

## What the game actually uses for animation

The game ships `libspine_godot` and its API has ~76 Spine references, so base-game
characters are [Spine](https://esotericsoftware.com/) 2D skeletal rigs. There is no 3D
pipeline — `res://models/` contains two files.

**You do not need Spine.** BaseLib exposes `CustomAnimation` with `UseAnimationTree`,
`UseAnimationPlayer` and `UseAnimatedSprite2D`, and `MerchantCharacterAnimPatch` has a
`SkipInitialAnimIfNotSpine` path — it explicitly supports custom characters that aren't
Spine rigs. Godot's own animation tools are enough.

Simplest option of all: `PlaceholderCharacterModel` borrows base-game character assets, so
you get working animation for free and can defer this entirely. All three characters run
that way today.

## Tools

| Tool | Install | For |
|---|---|---|
| Krita | `winget install KDE.Krita` | painting card portraits and icons |
| Godot RE Tools | `winget install GDRETools.gdsdecomp` | extracting reference assets |
| Godot 4.5.1 mono | — | already required to build; doubles as the animation tool |
| Inkscape | `winget install Inkscape.Inkscape` | optional, vector UI icons |
| Spine | [paid](https://en.esotericsoftware.com/spine-purchase) | optional, only to match base-game rigging exactly |
