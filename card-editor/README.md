# Card & relic editor

A local web app for balancing HelloSpire's cards and relics: edit costs, values
and upgrade deltas, write the card text, and crop art into every size the mod
needs — without hand-editing 200 C# files and a 372-key string table.

```bash
cd card-editor
npm install
npm run dev        # UI on http://localhost:5180, API on 127.0.0.1:2580
```

`npm test` runs the parser suite; `npm run typecheck` type-checks everything.

## What it edits, and how

The mod has no content database — a card *is* a C# class, and its behaviour is
real code:

```csharp
/// <summary>Fire 1. Draw 1 card.</summary>
public sealed class SnapShot() : GunslingerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new DynamicVar("Deadeye", 0m)];
    ...
    protected override void OnUpgrade() => DynamicVars["Deadeye"].UpgradeValueBy(2m);
}
```

So the editor does **not** generate C#. It parses each class, records the byte
range of every number and enum it can identify, and splices replacements into
those ranges on save. Everything it did not claim — `OnPlay` bodies, hover tips,
comments, formatting — comes through a save byte-for-byte. A save touches three
literals and one JSON line, and reads as a balance change in review.

| Surface | Where it lives | Editable |
| --- | --- | --- |
| Cost, type, rarity, target | the four constructor arguments | yes, unless inherited |
| Var base values | `CanonicalVars` | yes |
| Upgrade deltas | `UpgradeValueBy(…)` in `OnUpgrade` | yes, where one exists |
| Title, description, upgrade text | `localization/eng/cards.json` | yes |
| Relic rarity | `RelicRarity` override | yes |
| Relic title, description, flavor | `localization/eng/relics.json` | yes |
| Portrait and icon art | `images/card_portraits`, `images/relics` | yes, drag & drop |
| Everything else | `OnPlay`, hooks, powers | edit the source |

A var whose value is a named constant (`new DynamicVar("Threshold", ArmorThreshold)`)
is still editable — the edit rewrites the `private const` line, which the panel
labels, since other code in the class may read it too.

Most cards name a character base and pass all four arguments. A few sit on an
intermediate base instead — the Paladin's eight Seals extend
`SealCard(int cost, CardRarity rarity, decimal amount)`, which bakes in
`CardType.Skill` and `TargetType.Self` and shares one `OnUpgrade` between them.
The editor reads those through the base and shows the inherited values, but
locks the controls: the number is real, it just is not that card's to change.

Two things the editor deliberately will not do. It cannot add an upgrade to a
var that has none, because that means writing a statement into `OnUpgrade` —
a code change, not a number. And a relic's numbers sit inside its behaviour
with nothing marking which argument is the balance knob, so relics expose
rarity, text and art only.

## Identity

One rule ties everything together, and the editor derives rather than stores it:

```
class SnapShot → HELLOSPIRE-SNAP_SHOT   (string-table key)
               → snap_shot.png          (art filename)
```

That is why a duplicate class name is a real bug rather than a style question —
two classes with one name share a portrait and a title. The **warnings** tab
lists those, plus every class with no localized title and every class with no
art.

The rule runs both ways, and the test suite checks the other direction too: a
string-table key with no class behind it is a card that was retired without its
text being cleared, which nothing else in the toolchain ever notices.

## Art

Drop an image on any card or relic (in the grid or the side panel), pan and zoom
to frame it, and save. One crop exports every size the mod loads:

- cards → `card_portraits/big/<slug>.png` (1000×760) and `card_portraits/<slug>.png` (250×190)
- relics → `relics/big/<slug>.png` (256×256), `relics/<slug>.png` (128×128), and
  `relics/<slug>_outline.png`, the white silhouette the game tints

**New art files need one Godot pass.** Godot writes the sibling `.png.import`
(and its `uid://`) when it next scans the project, and the game cannot load the
texture until it exists. The editor says so after writing a file that has none.
Replacing existing art needs no rescan.

## Safety

The API is a development tool with no authentication that rewrites tracked
source files. It refuses to start under `NODE_ENV=production`, binds `127.0.0.1`
as a literal, and allows only the local dev origin. Don't loosen those — write a
different program instead.

Work on a branch and read `git diff` before committing, the same as any other
source change. The parser's regression suite asserts that re-saving all 263
cards unchanged produces zero edits, and that every recorded span still points
at the text it claims.
