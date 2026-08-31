# HelloSpire

A three-character pack for [Slay the Spire 2](https://store.steampowered.com/app/2868840/), built to be played together in co-op.

Built against **game v0.107.1** and **BaseLib 3.4.5**. Slay the Spire 2 is in Early Access, so expect this to need a rebuild after breaking updates.

## The characters

| Character | HP | Colour | Status |
|---|---:|---|---|
| **The Paladin** | 75 | gold | starter deck only, being rebuilt one card at a time |
| **The Alchemist** | 68 | green | shell only — borrowed starter kit |
| **The Gunslinger** | 72 | rust | full card set, relics, potions and cylinder UI in code; first balance pass done |

The Paladin's 91-card set, Faith system and starter relic are all in code, generated from one spec (`tools/gen_paladin.py`); its numbers are untuned placeholders and most card art is a labelled tile. The Gunslinger's full set is in and compiling, with real power and relic icons generated from `tools/gen_gunslinger_icons.py`. The Alchemist is scaffolded but excluded from the build until its card text lands.

- [**TODO.md**](TODO.md) — phased roadmap for building each character out, with real base-game baselines
- [**ART.md**](ART.md) — the art pipeline: where assets live, required sizes, and how to extract the base game's art for reference

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

```
HelloSpireCode/
  MainFile.cs                     single [ModInitializer] for the whole pack; Harmony instance
  Extensions/StringExtensions.cs  asset path helpers
  Powers/HelloSpirePower.cs       shared — powers are mod-wide, not per-character
  Characters/
    Paladin/     Paladin.cs, PaladinCardPool/RelicPool/PotionPool.cs,
                 PaladinCard.cs, PaladinRelic.cs, PaladinPotion.cs
    Alchemist/   ... same seven files
    Gunslinger/  ... same seven files

HelloSpire/
  images/
    charui/paladin|alchemist|gunslinger/   per-character UI art
    card_portraits/ relics/ potions/ powers/   shared trees
  localization/eng/*.json                 all display text
```

### Why only `charui` is namespaced per character

Card, relic and potion art resolves by **class name** (`Id.Entry`), which is already unique mod-wide — `PaladinStrike` and `GunslingerStrike` cannot collide. So those trees stay shared. Character UI art (icon, select portrait, map marker, energy orb) is the one asset class with fixed filenames per character, so it is the only one split by folder.

### Adding a fourth character

Copy any `Characters/<Name>/` folder, rename the seven classes, then:

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
