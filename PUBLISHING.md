# Publishing HelloSpire

How a build gets from this repo to other people. There are two channels, and a release
normally goes out on both:

| Channel | Who it's for | What the player does |
|---|---|---|
| **GitHub release + install script** | playtesters, friends, anyone before the Workshop page is public | runs one PowerShell line |
| **Steam Workshop** | everyone, eventually; also private/unlisted playtests | clicks Subscribe |

Both ship the same thing: the contents of `mods/HelloSpire/` after `dotnet publish`.

## What a build is

`dotnet publish` (see the README's "Building") writes the finished mod into the game's mods
folder:

```
mods/HelloSpire/
  HelloSpire.dll      code
  HelloSpire.pck      every image, scene and localisation file (Godot export)
  HelloSpire.json     the manifest: id, version, BaseLib dependency
  HelloSpire.pdb      debug symbols; kept, so a playtester's crash log has line numbers
  spine/<character>/  combat rigs, which the game reads from disk, not the .pck
```

That folder is the release. Nothing below re-derives it; the tools copy it as is.

**`dotnet build` is not enough.** It skips the Godot export, so the `.pck` would be stale:
new card art, relic icons and text changes would be missing from the build you hand out.

## Step 1: bump the version

`HelloSpire.json` → `"version"`. A co-op lobby compares gameplay-affecting mods **by version**
(see "Why one mod instead of three" in the README): two players on different HelloSpire builds
cannot join each other. So every build that leaves your machine needs its own number, and every
tester in a group must be on the same one.

Suggested scheme for Early Access: `v0.MINOR.PATCH`. Bump MINOR for new content or balance
passes, PATCH for bug fixes. Tag the commit with the same string.

```sh
git commit -am "v0.1.0"
git tag v0.1.0
git push && git push --tags
```

## Step 2: build and package

```sh
dotnet publish                              # close the game first
python tools/package-release.py             # -> dist/HelloSpire-v0.1.0.zip
```

`package-release.py` zips the deployed `mods/HelloSpire/` folder. It refuses to run if the
`.pck` is missing (you ran `build`, not `publish`) or if the deployed manifest's version doesn't
match `HelloSpire.json` (you bumped the version but didn't publish again). It finds the mods
folder the same way the build does; pass `--mods-dir` if yours is unusual.

The zip holds a single top-level `HelloSpire/` folder. Anyone can install it by hand by unzipping
it into the game's `mods/` folder.

## Step 3a: GitHub release (playtest channel)

```sh
python tools/package-release.py --github --notes "Paladin relics, all card art, Gunslinger frame tint"
```

This creates release `v0.1.0` on `r0zar/HelloSpire` with the zip attached. It needs the GitHub
CLI (`gh auth login` once). The repo is public, so testers need no GitHub account.

### What testers run

Send them this. It is the whole install, and running it again is how they update:

```powershell
irm https://raw.githubusercontent.com/r0zar/HelloSpire/main/tools/install-playtest.ps1 | iex
```

Paste it into PowerShell (Start menu → "PowerShell"). [`tools/install-playtest.ps1`](tools/install-playtest.ps1):

1. finds Slay the Spire 2 through Steam's registry key and every Steam library folder;
2. stops if the game is running (it locks the files);
3. downloads the newest release zip and **replaces** `mods/HelloSpire/`, so files deleted
   between builds don't linger;
4. installs BaseLib at the exact version `HelloSpire.json` declares, from BaseLib's own GitHub
   releases. If the tester already subscribes to BaseLib on the Workshop, it leaves BaseLib alone
   and warns if a second copy is sitting in `mods/`;
5. ends with **SUCCESS** or **FAILED: <reason>** and waits for Enter, so the window stays open
   long enough to read (or copy into a bug report).

To keep a whole group on one build even after you publish a newer one:

```powershell
& ([scriptblock]::Create((irm https://raw.githubusercontent.com/r0zar/HelloSpire/main/tools/install-playtest.ps1))) -Version v0.1.0
```

If auto-detection fails, add `-GameDir 'D:\SteamLibrary\steamapps\common\Slay the Spire 2'`.

The script is for Windows. On macOS or Linux, unzip the release into the mods folder by hand:
`…/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods/` on macOS, `…/Slay the Spire 2/mods/`
on Linux. Then install BaseLib the same way.

After installing: launch, choose **Play with Mods**, accept the warning, enable HelloSpire and
BaseLib in the Mods menu, and restart.

## Step 3b: Steam Workshop

Slay the Spire 2 has had Workshop support since v0.107.1. Uploads go through Mega Crit's
official tool, [sts2-mod-uploader](https://github.com/megacrit/sts2-mod-uploader). Download the
`ModUploader-<os>.zip` for your platform from its releases.

### The workspace

The uploader works on a folder called a *workspace*. This repo keeps one at [`workshop/`](workshop/):

```
workshop/
  workshop.json   title, description, visibility, tags, dependencies (tracked)
  image.png       the Workshop thumbnail, under 1 MB (tracked)
  mod_id.txt      written by the first upload: the Workshop item's ID (commit it!)
  content/        the build to upload (gitignored; filled by package-release.py)
```

`workshop.json` already declares BaseLib (Workshop ID `3737335127`) as a dependency, so Steam
subscribes players to it automatically. It starts at `"visibility": "unlisted"`: reachable by
link, not listed in search. That is the right setting for a playtest. Switch it to `"public"`
for launch. `"friends_only"` and `"private"` also work.

### First upload

```sh
python tools/package-release.py --workshop workshop     # copies the build into workshop/content/
ModUploader upload -w workshop                           # ModUploader.exe on Windows
```

Steam must be running and logged in to the account that will own the item. The first upload
creates the item and writes `workshop/mod_id.txt`. **Commit that file.** Without it, the next
upload creates a second, separate Workshop item instead of updating this one.

Then open the item's page on Steam, check the description renders, and accept the Workshop
legal agreement if Steam asks (items stay hidden until it is accepted).

### Updates

1. Bump the version and `dotnet publish` (steps 1–2).
2. Write a `changeNote` in `workshop/workshop.json`. Players see it in the item's change log.
3. Run the same two commands again. `mod_id.txt` points the upload at the existing item.

Subscribers update automatically the next time Steam syncs. That is the downside for co-op
playtests: a group can end up on different versions mid-session. If that matters, pin the
group to a GitHub release (step 3a) instead.

### Workshop and manual installs don't mix

Workshop mods live in `steamapps/workshop/content/2868840/<id>/`, not in the game's `mods/`
folder. If a tester has both a Workshop subscription and a manual copy in `mods/`, the game can
load two HelloSpires. Tell testers to pick one channel. `install-playtest.ps1` warns about the
BaseLib case.

### If a Workshop subscription alone doesn't load the mod

At least one tester has reported that subscribing on the Workshop page was not enough by
itself — the mod only loaded after they also manually created a `mods/HelloSpire/` folder and
copied the mod's files into it, on top of the Workshop subscription (see issue
[#19](https://github.com/r0zar/HelloSpire/issues/19)). That contradicts the "Workshop and manual
installs don't mix" guidance above, and the root cause hasn't been confirmed — it may be a
BaseLib or base-game mod-loader quirk with Workshop-only installs rather than anything in this
repo's control. Until it's root-caused, if **Play with Mods** doesn't show HelloSpire as
enabled after a Workshop subscribe, fall back to the manual install (step 3a) instead of
troubleshooting the Workshop path further.

## Release checklist

- [ ] Version bumped in `HelloSpire.json`, commit tagged with the same string
- [ ] `dotnet publish` run **after** the bump, game closed
- [ ] Launched once from the deployed build: character select shows all three, one combat each
- [ ] `python tools/package-release.py --github` → release exists with the zip attached
- [ ] Installer one-liner run on a clean machine (or after deleting `mods/HelloSpire`)
- [ ] Workshop: `--workshop workshop`, `changeNote` written, `ModUploader upload -w workshop`
- [ ] `workshop/mod_id.txt` committed (first upload only)
- [ ] Testers told the version number, so a co-op group can confirm they match
