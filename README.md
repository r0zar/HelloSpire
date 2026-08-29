# HelloSpire

A minimal, working **"hello world" character mod** for [Slay the Spire 2](https://store.steampowered.com/app/2868840/) — the smallest thing that adds a new selectable character to the character select screen and actually starts a run.

Built against **game v0.107.1**. Slay the Spire 2 is in Early Access, so expect this to need a rebuild after breaking updates.

## What it adds

**The Greeter** — a playable character with 70 starting HP, the Ironclad's starter deck and Burning Blood, and its own (empty) card / relic / potion pools. It is deliberately boring: the point is that every piece of wiring a real character needs is present and minimal, so you can see the whole skeleton at once.

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
  Character/HelloSpire.cs        the CharacterModel — HP, starting deck, relics, pools, icons
  Character/*Pool.cs             card / relic / potion pools for the character
  Cards/, Relics/, Potions/, Powers/   one stub of each content type

HelloSpire/
  images/                        art, referenced by path from the model classes
  localization/eng/*.json        all display text (see below)
```

### Localization is compile-time checked

The game ships a Roslyn analyzer (`STS001`) that fails your build if a model references a localization key you haven't written. That's a feature — it catches typos before the game does.

Keys are **flat, dotted strings**, namespaced by mod id, and the slug is the class name in `SCREAMING_SNAKE_CASE`:

```json
{
  "HELLOSPIRE-HELLO_SPIRE.title": "The Greeter",
  "HELLOSPIRE-HELLO_SPIRE.pronounSubject": "they"
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
