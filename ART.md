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
  card_portraits/        shared;  <card_class>.png       and  big/<card_class>.png
  relics/                shared;  <relic_class>.png, <relic_class>_outline.png, big/<relic_class>.png
  potions/               shared;  <potion_class>.png     and  outline/<potion_class>.png
  powers/                shared;  <power_class>.png      and  big/<power_class>.png

spine/<character>/       plain .atlas + .skel + page .png; present = that character's combat
                         rig is replaced, absent = it keeps the inherited one
```

Card, relic and potion art resolves by **class name** (`Id.Entry`, lowercased), which is
already unique mod-wide — `PaladinStrike` and `GunslingerStrike` cannot collide — so those
trees stay shared. Character UI uses fixed filenames per character, so it is the one tree
namespaced by character folder.

**`Id.Entry` is `SCREAMING_SNAKE_CASE`, so the lowercased filename keeps the underscores.**
`HandMeThat` → `hand_me_that.png`. `handmethat.png` is a file the game never asks for, and the
only symptom is a `Could not find card image path` line in `godot.log` while the card shows the
generic back. Single-word names hide the mistake, which is how it went unnoticed once already.

Missing art degrades gracefully: the helpers in `StringExtensions.cs` fall back to the
generic placeholder and log `Could not find ... image path`, rather than crashing.

## Required sizes

| Asset | Size |
|---|---|
| `character_icon.png`, `map_marker.png` | 128×128 |
| `char_select.png`, `char_select_locked.png` | 132×195 |
| `big_energy.png` | 74×74 |
| `text_energy.png` | 24×24 |
| Potion icon and its `outline/` companion | 256×256 |
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
python tools/gen_gunslinger_icons.py                    # all 21 powers and 9 relics
python tools/gen_gunslinger_icons.py deadeye old_iron   # just these
python tools/gen_gunslinger_icons.py --sheet /tmp/x.png # contact sheet, to judge the set as a set
```

Two families, following what the pack already does:

- **Keyword powers** — Cylinder, Deadeye, Armor — are flat glyphs on transparent, the way the base
  game draws Strength and Dexterity and the way the Paladin's Spirit icon does. These three read as
  stats the character has, not as buffs it was granted.
- **Engine powers** (18 of them) get a medallion disc: brown disc, brass ring, pale glyph. They are
  things a card gave you, and the disc says so. This style was modelled on the Paladin's `HolyBook`
  and `ChainedGauntlet` and on the Alchemist's power icons — note that the Alchemist's set is only
  7 of 18 done, so it is a target to match, not a finished set to copy from.

Relic `_outline` files are derived from the alpha of the relic art itself, so a silhouette can
never drift from the art it belongs to. Do not hand-edit them.

## Generating the potion icons

`tools/gen_potion_icons.py` is the same kind of thing: not scaffolding, but the art. It draws all
14 potion icons the pack actually needs — fill plus the white `outline/` companion — as flat vector
shapes through `rsvg-convert`.

```
python tools/gen_potion_icons.py                     # all 14
python tools/gen_potion_icons.py anointing_oil       # one, by key
python tools/gen_potion_icons.py --sheet /tmp/x.png  # contact sheet, to judge the set as a set
```

The grammar is one sentence: **a glass vessel, a coloured liquid, and one emblem that says what
drinking it does.** The vessel is the character's (Paladin reliquary ampullae, Gunslinger hip flask
and field tonics, Alchemist bench glassware) and the liquid carries the character's colour, so a
potion reads as *whose* before it reads as *what*. Six vessel shapes cover the whole set, which is
what makes it read as a set.

14, not 31: the 15 Volatile Common Potions point at the base game's own potion sprites (see the
comment on `VolatileCommonPotion`), so they are not missing art and must not be given any. Volatile
Poison Potion and Volatile Poison Ampoule share `poison_potion.png` with the real Poison Potion,
which this script does draw.

The `outline/` files are derived from the alpha of the fill art, exactly as relic `_outline` files
are. Do not hand-edit them.

## Generating the missing power icons

`tools/gen_power_icons.py` fills the pack's power-icon gap: the Alchemist's ten engine powers, the
Paladin's four one-turn bookkeeping powers, and Renew, whose file existed but was a labelled
placeholder tile rather than art.

```
python tools/gen_power_icons.py                      # all 15
python tools/gen_power_icons.py toxic_culture        # one, by key (no _power suffix)
python tools/gen_power_icons.py --sheet /tmp/x.png   # contact sheet, to judge the set as a set
```

It matches the medallion the Alchemist's seven finished icons and the Paladin's seals already use,
which is a **different disc from the Gunslinger's**: a coloured disc under a gold rim ring, rather
than a brown disc with a brass ring set in from the edge. The geometry and palette were measured
off `big/eternal_crucible_power.png` and `big/seal_of_righteousness_power.png` so a generated icon
sits beside a hand-made one without looking generated. The disc colour carries the meaning the
glyph cannot — poison green, brew amber, distill teal, defence steel-blue, volatility violet, fury
crimson.

The Paladin's four all carry one shared mark: an **hourglass badge** in the corner, over the parent
seal's own glyph. Those powers sit in the power bar right next to the seal that granted them, so
they cannot use the seal's icon unchanged — the badge is what says *this turn only*.

Two things it deliberately does not draw:

- `BottledFuryStrengthPower` is a plain vanilla `TemporaryStrengthPower` with no `ICustomPower`, so
  it already shows the base game's own temporary-Strength icon. Mod art there would make one buff
  read as two different things depending on who granted it.
- Nine power icons in the tree are labelled placeholder tiles — `divine_allegiance`, `judged`,
  `warded`, `sentinel`, `the_broken_god`, `blessing_of_sacrifice` and the three oaths — but every
  one is orphaned art for a class that no longer exists. Nothing loads them. Renew was the only
  live one in that set.

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
you get working animation for free and can defer this entirely. All three characters still derive
from it.

On top of that, `Characters/CharacterSkeletons.cs` swaps in a mod-local rig for any character with
a `spine/<character>/` folder — plain `.atlas`/`.skel`/page `.png`, no `.spskel` wrapper or pck
import needed — and `Characters/CharacterSkins.cs` repaints whatever rig is in use through a
per-character palette shader. Today the Paladin ships a reskinned Ironclad rig, the Gunslinger
ships an unrepainted copy of the Silent rig (its rust comes entirely from the shader), and the
Alchemist ships no rig at all and rides the inherited one. A missing folder is not an error: it
degrades to the shader repaint alone.

## Tools

| Tool | Install | For |
|---|---|---|
| Krita | `winget install KDE.Krita` | painting card portraits and icons |
| Godot RE Tools | `winget install GDRETools.gdsdecomp` | extracting reference assets |
| Godot 4.5.1 mono | — | already required to build; doubles as the animation tool |
| Inkscape | `winget install Inkscape.Inkscape` | optional, vector UI icons |
| Spine | [paid](https://en.esotericsoftware.com/spine-purchase) | optional, only to match base-game rigging exactly |
