# HelloSpire — The Gunslinger

A **playable character mod** for [Slay the Spire 2](https://store.steampowered.com/app/2868840/): a
weathered shooter built around a visible six-chamber revolver.

Built against **game v0.107.1**. Slay the Spire 2 is in Early Access, so expect this to need a
rebuild after breaking updates.

## What it adds

**The Gunslinger** — 72 HP, 3 Energy, 5 cards. The gun is the character: ammunition has to be
Loaded before it can be spent, the order of the chambers is knowable and manipulable, and almost
every card either fills the cylinder, spends it, or rearranges what is coming next.

| | |
|---|---|
| Cards | 84 — 4 starter, 20 Common, 35 Uncommon, 25 Rare |
| Relics | 9 — starter + Ancient upgrade, 1 Common, 2 Uncommon, 3 Rare, 1 Shop |
| Potions | 3 |
| New mechanics | Cylinder (Load / Fire / Cycle / Spin / Click / Self-Fire), Deadeye, Armor, Dodge, 9 Round types |

The full design lives in [`the_gunslinger_sts2_design_plan_v0_1.md`](the_gunslinger_sts2_design_plan_v0_1.md).

### The verbs

- **Load [Round]** — place a Round into the first empty chamber clockwise from the hammer. A full
  cylinder overwrites from the hammer forwards, so ammo cards are never dead draws.
- **Fire X** — resolve the chamber under the hammer and advance it, X times. A loaded chamber deals
  its Round's damage as Attack damage from the card, so Strength / Weak / Vulnerable apply.
- **Cycle X** — advance the hammer without firing. The deterministic setup tool.
- **Spin** — hammer to a random chamber. The gamble.
- **Click** — a Fire that resolves an empty chamber. Usually a failure; a few cards pay for it.
- **Self-Fire** — point it at yourself for the Round's printed damage as HP loss, ignoring
  everything. Russian Roulette.

## Where the code lives

```
HelloSpireCode/Gunslinger/
  GunContext.cs            who is working the gun — a card, a relic, or a potion
  GunslingerEffects.cs     the only file that calls the base game's command APIs
  GunslingerHooks.cs       mod-internal listeners: Fire / Load / Spin / Weak / Dodge / Armor
  GunslingerIntents.cs     reading enemy intents (see "Needs verifying" below)
  GunslingerTips.cs        hover tips for Load, Fire, Cycle, Spin, Click, Self-Fire
  Cylinder/Round.cs        the 9 ammunition types
  Cylinder/Revolver.cs     the rules of the gun — every operation goes through here
  Powers/CylinderPower.cs  the six chambers and the hammer; all per-combat state
  Powers/CorePowers.cs     Deadeye, Armor, Dodge and the small delayed-effect powers
  Powers/EnginePowers.cs   the 11 powers behind the Power cards
  Powers/GunslingerDamagePatch.cs   the ONLY place this mod touches the damage pipeline
  Cards/                   84 cards, grouped by rarity
  Relics/  Potions/        9 relics, 3 potions
```

Two rules keep this navigable. **Every base-game command call goes through `GunslingerEffects`**, so
a signature change in the game breaks one file rather than eighty. **Every cylinder operation goes
through `Revolver`**, so the six-chamber rules are stated once and cards stay short enough to read
like their own card text.

## Needs verifying against a real build

The game was not installed on the machine this was written on, so nothing here has been through a
compiler. Most of it is written against APIs taken from working mods, but four things are inferred
and are the first places to look if something misbehaves:

1. **`Powers/GunslingerDamagePatch.cs`** — Armor and Dodge reduce incoming damage by patching
   `Hook.ModifyDamage`. The parameter names are the ones BaseLib itself binds to; the decimal return
   type is inferred. The patch refuses to apply rather than crash if the shape is wrong, which
   leaves Armor and Dodge inert but the run playable. It also runs *before* Block is subtracted, so
   it recomputes the split itself — if Dodge or Armor drains faster than expected, check whether the
   hook is also being called to render forecasts.
2. **`GunslingerIntents.cs`** — matched against working intent-reading code from another mod, but
   the damage total is wrapped anyway: a failed read degrades to "no enemy is attacking", so
   Showdown and Dive for Cover read weaker rather than throwing.
3. **Power turn hooks** — `BeforeSideTurnStart(ctx, side, state)` is verified on relics but assumed
   to have the same shape on powers. If Dodge stops expiring, this is why.
4. **`PowerCmd.ModifyAmount<T>(target, delta)`** — used only in the damage patch.

## Deliberate deviations from the design doc

Three cards ask the player to pick a chamber or an ammunition type. There is no base-game selection
screen for either, and a custom cylinder UI is its own project, so each resolves to the choice a
player would almost always make. All three are marked in the code and in the card text:

- **Called Shot / Sixth Sense** move the highest-damage loaded chamber under the hammer.
- **Stack the Cylinder** packs every Round into one run from the hammer, heaviest first.
- **Custom Load / Perfect Reload** read the board: Piercing into Block, Guard into an incoming
  Attack, Crippling when nothing is Weak, Heavy otherwise.

**Quickdraw Legend** refunds an Energy on the first Fire of the turn rather than discounting the
card, since the game's cost hooks cannot see that a card is going to Fire before it is played. The
two differ only when the Energy was not there to spend.

Relics that "start each combat" instead act at the top of the player's first turn, which is
indistinguishable in play and keeps every relic on hook signatures with working precedent.

## Not built yet

- **The 5 multiplayer-specific cards** the design calls for. The base game's multiplayer card
  constraints are the one content type with no working example to copy from.
- **Ascension 2+ HP (58)**. The game applies its own ascension HP reduction; if a per-character
  override exists, it was not found.
- **A cylinder UI.** The six chambers and the hammer currently live in the Cylinder power's tooltip
  (`{Loaded} of 6 chambers loaded, hammer on chamber {Chamber}`) rather than as art beside the hand.
  This is what forces the three "player picks a chamber" cards into automatic choices above.

## Art

Card portraits, relic icons, power icons and character UI all fall back to placeholders until real
files land in `HelloSpire/images/`. File names are derived from the class name, lowercased —
`SnapShot` → `snap_shot.png`. Sizes are in the comments on each base class in
`HelloSpireCode/Cards/`, `Relics/`, `Powers/`.

## Prerequisites

| Requirement | Notes |
|---|---|
| Slay the Spire 2 | v0.107.1 or compatible |
| [BaseLib](https://github.com/Alchyr/BaseLib-StS2/releases) | v3.4.5, in your `mods/` folder or via Steam Workshop |
| .NET SDK | 9.0 or higher |
| MegaDot, or Godot **4.5.1** .NET | Must be 4.5.1 — the game refuses `.pck` files exported by a newer Godot |
| Alchyr's templates | `dotnet new install Alchyr.Sts2.Templates` |

## Building

1. Copy `Directory.Build.props.example` to `Directory.Build.props` and point `<GodotPath>` at your
   MegaDot/Godot 4.5.1 mono executable. If the game isn't at the default Steam location, uncomment and
   set `<Sts2Path>` too. (`Directory.Build.props` is gitignored — it holds machine-specific paths.)

2. Compile only (fast, code changes only — produces `.dll`):
   ```
   dotnet build
   ```

3. Full publish (produces `.pck` with assets and copies `.dll` + `.pck` + `.json` into your `mods/` folder):
   ```
   dotnet publish
   ```

4. Launch the game with **"Play with Mods"**, accept the untrusted-code warning, restart, enable HelloSpire in the Mods menu, restart again.

## How it fits together

```
HelloSpire.json                  mod manifest — id, version, BaseLib dependency, min game version
Directory.Build.props            your machine's Godot / StS2 paths (the only file you must edit)
project.godot                    Godot project; drives .pck export
export_presets.cfg               the "BasicExport" preset that dotnet publish invokes

HelloSpireCode/
  MainFile.cs                    [ModInitializer] entry point; creates the Harmony instance
  Character/TheGunslinger.cs     the CharacterModel — HP, starting deck, relics, pools, icons
  Character/*Pool.cs             card / relic / potion pools for the character
  Cards/, Relics/, Potions/, Powers/   abstract bases for each content type
  Gunslinger/                    the character itself (see above)

HelloSpire/
  images/                        art, referenced by path from the model classes
  localization/eng/*.json        all display text (see below)
```

### Localization is compile-time checked

The game ships a Roslyn analyzer (`STS001`) that fails your build if a model references a localization key you haven't written. That's a feature — it catches typos before the game does.

Keys are **flat, dotted strings**, namespaced by mod id, and the slug is the class name in `SCREAMING_SNAKE_CASE`:

```json
{
  "HELLOSPIRE-THE_GUNSLINGER.title": "The Gunslinger",
  "HELLOSPIRE-SNAP_SHOT.title": "Snap Shot"
}
```

Files must live at `res://<manifest_id>/localization/<lang>/<file>.json`. A file at `res://localization/...` — without the mod id segment — is silently ignored.

### The manifest

```json
{
  "id": "HelloSpire",
  "min_game_version": "0.107.0",
  "has_pck": true,
  "has_dll": true,
  "dependencies": [
    { "id": "BaseLib", "min_version": "3.3.0" }
  ],
  "affects_gameplay": true
}
```

Use the **object form** for dependencies with `min_version`. The older bare-string form (`"dependencies": ["BaseLib"]`) still loads but logs an error and is slated for removal. Declaring `min_game_version` also silences a load warning.

## Where the real documentation is

The game ships its own API docs — `data_sts2_windows_x86_64/sts2.xml`, ~5 MB covering roughly 19,600 members, with real summaries. Alongside it sit `0Harmony.dll` and `MonoMod.*`, so patching is first-class rather than bolted on. Start there before guessing.

Useful entry points:

- `MegaCrit.Sts2.Core.Modding.ModInitializerAttribute` — marks your entry point
- `MegaCrit.Sts2.Core.Modding.ModHelper.AddModelToPool<,>` — registers content into pools
- `MegaCrit.Sts2.Core.Models.CharacterModel` — `Title`, `NameColor`, `Gender`, `AssetPaths`, `IsPlayable`
- `MegaCrit.Sts2.Core.Models.ModelDb.Inject` / `.Remove` — explicitly documented as "should only be used in tests and mods"

## Credits

- [Alchyr](https://github.com/Alchyr/ModTemplate-StS2) — the mod template this is generated from, and BaseLib
- [fresh-milkshake](https://fresh-milkshake.github.io/Modding-Tutorial/) — the modding handbook
- Mega Crit — for shipping `sts2.xml`

## License

MIT
