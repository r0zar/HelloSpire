# HelloSpire

A three-character pack for [Slay the Spire 2](https://store.steampowered.com/app/2868840/), built to be played together in co-op.

Built against **game v0.107.0** (the manifest's declared floor) and **BaseLib 3.4.5**. Slay the Spire 2 is in Early Access, so expect this to need a rebuild after breaking updates.

## The characters

| Character | HP | Colour | Status |
|---|---:|---|---|
| **The Paladin** | 75 | gold | full 87-card set in code; numbers are placeholders, most card art is a labelled tile |
| **The Alchemist** | 68 | green | full ~85-card set and Lab/Belt economy in code; newest of the three, under active balance iteration |
| **The Gunslinger** | 72 | rust | full ~104-card set, relics, potions, Cylinder UI and a Gadgets sub-archetype in code; balance pass done |

All three characters compile and are playable end to end. The Paladin runs three lanes — Protection, Retribution, Holy — around **Plating** (decaying end-of-turn Block), with **Seals**/**Judge** and Spirit-scaled healing as opt-in sub-mechanics drafted rather than in the starter kit; its card art is still a labelled tile from `tools/gen_card_art.py`. The Gunslinger loads a six-chamber **Cylinder** and chooses to Fire, Cycle or Spin it, cashing out through **Deadeye**, with a parallel **Gadgets** archetype that skips the gun entirely; its power and relic icons are real art from `tools/gen_gunslinger_icons.py`, not placeholders. The Alchemist converts Potions, Gold and Max HP into each other via **Transform** — **Brew**, **Distill**, **Invest**, **Render** — banking Volatile Potions into a **Belt**; it's the most recently reworked of the three and where most current commits land.

- [**TODO.md**](TODO.md) — phased roadmap for building each character out, with real base-game baselines
- [**ART.md**](ART.md) — the art pipeline: where assets live, required sizes, and how to extract the base game's art for reference

## Known gaps

Coverage counts below are verified against what's actually on disk and what the code actually
requests, not against ART.md's own description of itself (which is out of date in a couple of
places, noted inline).

### Art

| Asset | Paladin | Gunslinger | Alchemist |
|---|---|---|---|
| Character UI (`charui/`) | complete | complete | complete |
| Combat body / spine rig | custom reskinned rig (`spine/paladin/`) | no mod-local rig — reuses the base game's Silent skeleton as-is, repainted by shader | no mod-local rig — also reuses the Silent skeleton, repainted; visually just a palette-shifted Silent in combat |
| Card portraits | **86/86** — hand-painted | **85/104** finished; 9 are still placeholder tiles (`Gadgets.cs` group); 10 more exist on disk under the *wrong* filename from an underscore-stripping bug in `tools/gen_card_art.py` (rename, not new art, fixes those) | **50/85** (59%) have art; the other 35 fall back to the generic card back entirely |
| Card upgraded/beta art | none for any character — `BetaPortraitPath` always falls back to base art; no `beta/` folder exists yet (~275 cards affected pack-wide) |||
| Relic icon (small + outline) | 6/6 | 9/9 | **8/9** — the starter relic `AlchemicalSatchel` has no icon at all, falls back to generic |
| Relic detail art (`relics/big/`) | **3/6** (`ChainedGauntlet`, `HolyBook`, `LibramOfWrath` missing) | 9/9 | **0/9** |
| Potion icons | n/a — the Paladin has no potions in the kit yet | **0/3** | **0/25** |
| Power icons | 31/35 (4 unmatched — may be minor aux powers that intentionally reuse a base-game icon, worth a manual check before treating as a real gap) | 21/21 | **4/15** — 11 missing, the largest icon gap in the pack |
| Ancient dialogue | only the Architect has lines (4 lines × 3 characters); every other Ancient is unstubbed |||

`tools/gen_gunslinger_icons.py` output is real, shipped art for the Gunslinger's powers and relics,
not scaffolding — and per its own comment it modeled that style on the Paladin's `HolyBook`/
`ChainedGauntlet` icons and an assumed complete Alchemist power-icon set. That assumption is wrong:
the Alchemist only has 4/15 power icons today, so `ART.md`'s claim that engine powers get "the
medallion disc the Alchemist's fifteen use" is aspirational, not current. `tools/gen_card_art.py`
output (card portraits) is scaffolding, meant to be replaced by hand art.

### Non-art

- **Multiplayer cards aren't gated.** Only the Gunslinger's five (`HelloSpireCode/Gunslinger/Cards/Multiplayer.cs`) exist in code; the Paladin's five and the Alchemist's five are design text only (`design/multiplayer-cards.md`). There's also no mechanism yet to keep a multiplayer-only card out of solo rewards/shops — the Gunslinger's five currently leak into the normal solo pool.
- **Localization still has placeholder "for now" copy.** `HelloSpire/localization/eng/characters.json`'s `cardsModifierDescription` for the Paladin and Alchemist literally reads "...is still borrowed steel/apparatus. For now." — written before the Paladin rework and never revisited. `ancients.json` is likewise still placeholder text (see the Ancient dialogue row above).
- **Cut-content cleanup.** ~30 orphaned `.cs.uid` files (and matching orphaned art) under `Characters/Paladin/Cards/` are leftovers from the pre-rework "Reset" commit; harmless but worth deleting in a pass. `HelloSpireCode/Characters/Paladin/Ui/PaladinSkin.cs.uid` is a similar orphan from a since-generalized patch.
- `TODO.md`'s Phase 0–11 checklist (129 items) has never actually been checked off for any character — progress is tracked narratively in its "Status" note instead. Don't read the unchecked boxes as "nothing done."

## Why one mod instead of three

Because of how the game gates multiplayer. On joining a lobby the game exchanges an `InitialGameInfoMessage` carrying the game version, an `idDatabaseHash` fingerprint of the whole model database, and **two separate mod lists** — `gameplayAffectingMods` and `otherMods`. A mismatch in the first list is a first-class rejection: `ConnectionFailureReason.ModMismatch`.

Which list a mod lands in is decided by one manifest field:

| `affects_gameplay` | Consequence |
|---|---|
| `true` | every player must have it, at a matching version |
| `false` | free to differ — cosmetic and UI mods |

A character mod is necessarily `true`. Shipping three separate character mods would mean three things every player has to install at matching versions; shipping one pack means one. Same reason the version matters as much as the name — a teammate on v0.1 against your v0.2 has a different model set and a different hash.

## Prerequisites

| Requirement | Notes |
|---|---|
| Slay the Spire 2 | v0.107.1 or compatible |
| [BaseLib](https://github.com/Alchyr/BaseLib-StS2/releases) | v3.4.5, in your `mods/` folder or via Steam Workshop |
| .NET SDK | 9.0 or higher |
| MegaDot, or Godot **4.5.1** .NET | Must be 4.5.1 — the game refuses `.pck` files exported by a newer Godot |

## Building

1. Copy `Directory.Build.props.example` to `Directory.Build.props` and point `<GodotPath>` at your MegaDot/Godot 4.5.1 mono executable. If the game isn't at the default Steam location, set `<Sts2Path>` too. (`Directory.Build.props` is gitignored — it holds machine-specific paths.)

2. ```
   dotnet build      # code only, ~2s — produces the .dll
   dotnet publish    # full — produces the .pck and deploys .dll/.pck/.json to mods/
   ```

   **Close the game first.** A running Slay the Spire 2 holds `HelloSpire.dll` open and the copy step fails.

3. Launch with **"Play with Mods"**, accept the untrusted-code warning, restart, enable HelloSpire in the Mods menu, restart again. All three characters then appear on character select.

## Layout

Not uniform per character — Paladin keeps everything under `Characters/Paladin/`; Gunslinger and
Alchemist keep only their base/pool glue there and hold the real card/relic/power content in
sibling top-level trees. An artifact of how each character was built, not a rule to follow.

```
HelloSpireCode/
  MainFile.cs                     single [ModInitializer] for the whole pack; Harmony instance;
                                   wires the Alchemist's LabBridge before patching
  Extensions/StringExtensions.cs  asset path helpers
  Powers/HelloSpirePower.cs       shared — powers are mod-wide, not per-character

  Characters/
    Paladin/     Paladin.cs, PaladinCard.cs, PaladinCardPool.cs, PaladinRelic.cs, PaladinRelicPool.cs,
                 PaladinPotion.cs, PaladinPotionPool.cs, PaladinEffects.cs, PaladinTips.cs
                 Cards/ Relics/ Powers/ Ui/    — the Paladin's actual content, one class per file
    Gunslinger/  Gunslinger.cs + the seven base/pool glue classes only
    Alchemist/   Alchemist.cs + the seven base/pool glue classes only

  Gunslinger/    Cards/ Cylinder/ Powers/ Relics/ Potions/   the Gunslinger's actual content
  Alchemist/     Cards/ Lab/ Powers/ Relics/ Potions/        the Alchemist's actual content

HelloSpire/
  images/
    charui/paladin|alchemist|gunslinger/   per-character UI art
    card_portraits/ relics/ potions/ powers/   shared trees
  localization/eng/*.json                 all display text
```

### Why only `charui` is namespaced per character

Card, relic and potion art resolves by **class name** (`Id.Entry`), which is already unique mod-wide — `PaladinStrike` and `GunslingerStrike` cannot collide. So those trees stay shared. Character UI art (icon, select portrait, map marker, energy orb) is the one asset class with fixed filenames per character, so it is the only one split by folder.

### Adding a fourth character

Copy `Characters/Gunslinger/` for the seven base/pool glue classes, then either keep its real
content in a sibling top-level `HelloSpireCode/<Name>/` tree or follow the Paladin's pattern and
put everything under `Characters/<Name>/` — the game doesn't care which. Rename the seven classes,
then:

1. Give the character class a `CharacterId`, an `AssetFolder`, and a `Color`
2. Create `images/charui/<assetfolder>/` with the six UI images
3. Add its localization keys to `characters.json` and `ancients.json`

The `[Pool(typeof(...))]` attribute on the three content base classes does the registration — individual cards and relics never declare a pool.

## Localization

Keys are **flat dotted strings**, namespaced by mod id, with the model slug in `SCREAMING_SNAKE_CASE`:

```json
"HELLOSPIRE-PALADIN.title": "The Paladin",
"HELLOSPIRE-PALADIN.pronounSubject": "they"
```

Files must live at `res://HelloSpire/localization/<lang>/`. A file at `res://localization/...` — without the mod id segment — is silently ignored.

The game ships a Roslyn analyzer (`STS001`) that **fails the build** if a model references a key you haven't written, and lists exactly which ones are missing. Treat its errors as your checklist rather than an obstacle.

## Card & relic editor

`card-editor/` is a local web app for balancing the sets: cost, var values and
upgrade deltas, card and relic text, and drag-and-drop art cropped into every
size the mod loads.

```bash
cd card-editor && npm install && npm run dev    # http://localhost:5180
```

It edits the C# in place rather than generating it — it records the byte range
of each number it can identify and splices replacements in, so `OnPlay` bodies
and formatting survive a save untouched. Its warnings tab lists the mistakes the
compiler cannot catch: classes with no localized title, classes with no art, and
duplicate class names (which silently share a portrait and a string-table key).
See `card-editor/README.md`.

## Where the real documentation is

The game ships its own API docs — `data_sts2_windows_x86_64/sts2.xml`, ~5 MB covering roughly 19,600 members with real summaries. Alongside it sit `0Harmony.dll` and `MonoMod.*`, so patching is first-class. Start there before guessing.

## Credits

- [Alchyr](https://github.com/Alchyr/ModTemplate-StS2) — the mod template this began as, and BaseLib
- [fresh-milkshake](https://fresh-milkshake.github.io/Modding-Tutorial/) — the modding handbook
- Mega Crit — for shipping `sts2.xml`

## License

MIT
