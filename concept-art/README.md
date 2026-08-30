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

## `cards/` — full card and relic galleries

One gallery page per character (`cards/alchemist.html`, `cards/paladin.html`,
`cards/gunslinger.html`), covering every card, relic, and potion in each character's
full design doc — 94-99 pieces per character, ~290 total.

These are **not** hand-illustrated per card. Each page embeds the character's full card
data (name, cost, type, rarity, rules text, and for the Paladin, deity) and a small
rendering engine that computes a rarity-tinted frame and an icon from the card's actual
type and keywords — Brew → flask, Fire → bullet, Faith in Tyr → sun-blade, and so on.
That's the same spirit as `tools/gen_character_art.py`: correct, distinct, systematic
placeholders so real art has something concrete to replace, not a substitute for it.

Sourced directly from each character's authoritative card list:
- Alchemist — [GitHub issue #2](https://github.com/r0zar/HelloSpire/issues/2) (82 cards, 9 relics, 3 potions, 5 multiplayer cards)
- Paladin — [`../design/paladin-cards.md`](../design/paladin-cards.md) (90 cards — the doc's own header says 91; its per-tier breakdown sums to 90, a pre-existing typo, not something changed here — plus 4 relics)
- Gunslinger — [`../design/gunslinger.md`](../design/gunslinger.md) (82 cards, 9 relics, 3 potions, 5 multiplayer cards)

Two things worth knowing if you're picking this up:
- The icon-selection rules are a first pass (see the `pickIcon` function in each file) —
  they read each card's rules text with a handful of regexes, not real understanding, so
  an occasional icon will be a reasonable-but-imperfect guess.
- The Paladin page omits `Falter` and `The Black Hand`, which appear in the older
  `design/paladin.md` draft but were superseded by `Time of Troubles` and `Tyranny` in
  the authoritative `design/paladin-cards.md` Bane list — including them would have
  presented stale draft content as current design.
