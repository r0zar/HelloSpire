# Concept art

Visual-direction mockups for the three characters — self-contained HTML, open any file
directly in a browser. Each is a mood board (illustration, palette, typography, a short
gear breakdown), not a game asset. See [`../ART.md`](../ART.md) for the actual asset
pipeline and required PNG sizes.

| File | Inspirations | Design doc |
|---|---|---|
| `alchemist.html` | Talisman's Alchemist, Dota 2's Alchemist, Fullmetal Alchemist | [GitHub issue #2](https://github.com/r0zar/HelloSpire/issues/2), [#4](https://github.com/r0zar/HelloSpire/issues/4) |
| `paladin.html` | World of Warcraft Paladin, D&D Paladin | [`../design/paladin.md`](../design/paladin.md) |
| `gunslinger.html` | The Dark Tower (Roland Deschain / the wanderer archetype) | [`../design/gunslinger.md`](../design/gunslinger.md) |

The Paladin and Gunslinger pages predate a read of their own `design/` docs' specific
mechanics (Faith, the Cylinder) — they're pure visual direction, not an attempt to
illustrate those systems. Worth a pass to reconcile once real art direction is chosen;
until then these are reference only.

One placeholder-color mismatch worth flagging: `tools/gen_character_art.py` and the
`README.md` roster table currently list the Alchemist's placeholder color as green
(`6ad48a`), while both the concept art here and the Alchemist's actual design doc
(issues #2/#4) use a brass/ember/verdigris palette. The green was an arbitrary script
default from before any real design existed — worth updating once real art lands.
