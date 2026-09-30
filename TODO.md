# From Empty Shell to a Finished Character

A working checklist for taking a HelloSpire character from "a tile appears on character select" to one that feels like it shipped with the game.

**Run this once per character.** The Paladin, the Alchemist and the Gunslinger each need their own pass through Phases 0–8; Phases 9–11 are pack-wide and done once. Where a phase says "the character", substitute whichever you're working on.

Ordered by dependency, not by effort. Phases 0–3 are load-bearing: everything later assumes they are settled. Phases 4–7 are the bulk of the work. Phase 8 is the one people skip and should not. Phase 9 matters the moment anyone plays co-op.

Every API name here was verified against **game v0.107.1** (`data_sts2_windows_x86_64\sts2.xml` and `sts2.dll`) and **BaseLib 3.4.5**. Early Access moves; re-verify after breaking updates.

## Status (2026-09-28)

All three characters are content-complete in code and playable end to end: **280 cards, 26 relics,
31 potions, 75 powers.** Every card has a working upgrade; the only card without an `OnUpgrade` is
`VolatileResidue`, a Status card, which is correct.

| | Cards | Relics | Potions | Powers | Card art |
|---|---:|---:|---:|---:|---|
| Paladin | 86 | 8 | 3 | 35 | 86/86 unique |
| Alchemist | 90 | 9 | 25 | 19 | 90/90 unique |
| Gunslinger | 104 | 9 | 3 | 21 | 104/104 unique |

The 2026-08-30 "Reset" note this replaced is long superseded — the Paladin was rebuilt past it into
the 86-card Plating/Seals/Spirit set. The Faith system, and Judged / Warded / Blessed, stayed cut;
`design/paladin-faith-archive.md` keeps that design, and `design/paladin-rework-2026-08-31.md`
records what replaced it.

**How to read this file.** Every checkbox below was audited against the code on 2026-09-28.
~~Struck-through~~ items are done, with the evidence after the dash. Items marked ⚠️ are
outstanding. A "Decide X only if…" item counts as done when the answer was "not needed".

### ⚠️ Outstanding

The open boxes from every phase below, grouped by kind.

**Art**
- ~~**65 card portraits.**~~ — done 2026-09-28: 62 generated with `../image_gen_pipeline` (Alchemist 40, Gunslinger 19, one of each Paladin pair); the README's "Card art" names the few worth a second pass.
- No `beta/` art exists for any of the 280 cards. `BetaPortraitPath` falls back to the normal portrait, so nothing looks broken; this is only needed if the game's beta-art option should show something different.
- ~~Relic art: the `AlchemicalSatchel` icon, 9 Alchemist relic detail images, and 3 Paladin relic detail images.~~ — done 2026-09-28; see ART.md's "Paladin relic icons" and "Alchemist relic icons".
- ~~Gunslinger card-back tint is still the identity.~~ — `GunslingerCardPool` now sets `ShaderColor => Gunslinger.Color`, like the other two pools. ⚠️ Not yet compiled or seen in game.
- ~~Rest-site bodies are still shader repaints of the placeholder's scene.~~ — done: all three characters have a real `spine/<character>/restsite/` rig (`012c5e6`, "Rest site shows each character's own recoloured body"), loaded the same way as the combat rig via `CharacterSkeletons.cs` and swapped in by `RoomSkins.RestSite.Reskin`. Both the rest-site swap and the shop swap (`RoomSkins.Merchant.Reskin`) are code-complete but ⚠️ neither has been confirmed in a running game yet.
- Nothing character-specific for SFX, arm (rock/paper/scissors/pointing) textures, card trail or character-select transition; all are inherited from Ironclad.
- ⚠️ `pistol_whip.png` (small and `big/`) is a gunshot muzzle-flash painting, not a pistol-whip — confirmed by looking at it (issue #17). The card's mechanics were reworked to match its name (6 damage + 1 Vulnerable, blunt VFX), but the art itself still needs a real illustration; no generation tool available covers this.
- ⚠️ 8 of 9 Alchemist relic images (small and/or `big/`) have a flat-opaque background instead of real transparency — confirmed by inspecting alpha channels (issue #15). `AlchemicalSatchel` is the one that's already correct, proving clean transparency is achievable in this art style; an automated background-matting attempt strips the ink outlines and shading along with the background and isn't good enough to ship. Needs either the original generation pipeline re-run with a transparent background, or real matting tooling (e.g. `rembg`).

**Bugs**
- Stray code or tags reported at the bottom of some cards, mostly the Gunslinger's. Quickdraw Legend's was found and fixed; the rest need naming in game (the files themselves check clean).
- ~~35 card descriptions contain a literal number where the rules call for a `{Var:diff()}`.~~ — audited 2026-09-28; two were real bugs and are fixed (see Phase 4). The other 33 are correct as written.

**Character shell**
- `StartingGold`, `DialogueColor`, `SpeechBubbleColor`, `MapDrawingColor`, `RemoteTargetingLineColor`/`Outline`, `EnergyLabelOutlineColor` and the anim delays are all inherited from the placeholder rather than set per character.

**Content**
- ~~Paladin has 6 relics against the base game's 8, and no Uncommon or Shop relic.~~ — Judge's Gavel (Uncommon) and Tithing Box (Shop) added 2026-09-28, with art. ⚠️ Not yet compiled or playtested.

**Verification and testing**
- The eight inferred Ancient IDs (`gen_ancient_dialogue.py check --base …`).
- Every playtest, balance and multiplayer check in Phases 3, 4, 8 and 9 — none is recorded as done.

**Release**
- Version is still `v0.0.0`, no git tags, nothing published. The tooling now exists: `PUBLISHING.md` covers the GitHub release, the one-line playtest installer and the Steam Workshop upload.

See the README's "Known gaps" for per-file detail.

---

## Phase 0 — Decide what the character *is*

Do this before writing code. Every later decision resolves faster when there's a one-sentence answer to "what does this character do that no other one does?"

All of Phase 0 is done for all three: `design/paladin.md`, `design/gunslinger.md` and `design/alchemist.md` settle it, and the shipped sets are built on them.

- [x] ~~**Write the fantasy in one sentence.** "The Gunslinger spends ammunition it cannot easily replace." Not "a burst character with good scaling."~~
- [x] ~~**Name the core tension.** Every good StS character has a cost to its power. Ironclad trades HP. Silent trades tempo for setup. Defect trades slots. What does this one give up?~~
- [x] ~~**Check it against the other two.** Three characters in one pack should not overlap. If the Paladin and the Alchemist both want to stall and scale, one of them needs to change.~~
- [x] ~~**Pick the win condition shape.** Scaling powers? Burst combo? Attrition? Deck-thinning? This determines your rare cards.~~
- [x] ~~**Decide whether you need a new mechanic at all.** A character built from existing primitives (damage, block, powers, statuses) is *far* cheaper to build and balance. Add a resource only if the fantasy genuinely can't be expressed without it.~~ — Cylinder, Belt, Plating/Seals
- [x] ~~**Write 5 fake card names + effects on paper.** If they're boring, the fantasy is wrong. Iterate here, it's free.~~ — superseded by 280 real ones
- [x] ~~**Pick a color.**~~ — each character has a `Color` constant, used by `NameColor` and `DeckEntryCardColor`. It propagates to `NameColor`, `DialogueColor`, `SpeechBubbleColor`, `DeckEntryCardColor`, `MapDrawingColor`, and the card-back HSV in the character's `CardPool`. Choose once, reuse. *(Wiring it into the other properties is still open; see Phase 1.)*

---

## Phase 1 — The character shell

All of this lives in `HelloSpireCode/Characters/<Name>/<Name>.cs`.

### Stats

Base-game values, read directly out of `sts2.dll` for reference:

| Character | Starting HP | Starting Gold | Notes |
|---|---:|---:|---|
| Ironclad | 80 | 99 | highest HP, HP-as-resource design |
| Regent | 75 | 99 | uses Stars |
| Defect | 75 | 99 | 3 orb slots (`BaseOrbSlotCount`) |
| Silent | 70 | 99 | lowest of the classic three |
| Necrobinder | 66 | 99 | lowest HP, summon-based |
| **Paladin** | **75** | *(inherited)* | matches Regent/Defect |
| **Gunslinger** | **70** | *(inherited)* | matches Silent |
| **Alchemist** | **68** | *(inherited)* | near Necrobinder's floor |

- [x] ~~Set `StartingHp`. 66–80 is the shipped range. Go low only if the kit has real defensive or evasive tools; go high only if the kit spends HP.~~ — 75 / 72 / 68
- [ ] ⚠️ Set `StartingGold` explicitly (all base characters use 99 — deviating is a real balance lever, not a flavor one). *No character overrides it yet.*
- [x] ~~Override `MaxEnergy` only if the character is genuinely built around it. Default is fine for almost everything.~~ — not needed
- [x] ~~Override `BaseOrbSlotCount` only if you're doing an orb character (see Phase 3).~~ — not needed
- [x] ~~Set `Gender` (`CharacterGender.Neutral` / `Feminine` / `Masculine`) — drives grammar in generated text, and must agree with the pronoun loc keys.~~

### Identity and color

- [x] ~~`NameColor` — statistics screen~~
- [ ] ⚠️ `DialogueColor` — Ancient dialogue speech bubble
- [ ] ⚠️ `SpeechBubbleColor` — general speech (returns `VfxColor`, not `Color`)
- [ ] ⚠️ `MapDrawingColor`
- [ ] ⚠️ `RemoteTargetingLineColor` / `RemoteTargetingLineOutline` — multiplayer targeting
- [ ] ⚠️ `EnergyLabelOutlineColor`
- [x] ~~Card-back HSV in `HelloSpireCardPool` (`H`/`S`/`V`), or supply a `CustomFrame` texture instead.~~ — all three via `ShaderColor` (the Gunslinger's added 2026-09-28; ⚠️ not yet seen in game).

### Localization

All keys are **flat dotted strings** under `HelloSpire/localization/eng/`. The `STS001` analyzer fails the build on any missing key, so it will tell you exactly what's outstanding — treat build errors as your checklist.

- [x] ~~`characters.json`~~ — `title`, `titleObject`, `description`, four pronoun keys, `goldMonologue`, `eventDeathPrevention`, `aromaPrinciple`, `cardsModifierTitle`, `cardsModifierDescription`, `banter.alive.endTurnPing`, `banter.dead.endTurnPing` — written for all three; the "For now" placeholder `cardsModifierDescription` copy is gone
- [x] ~~`ancients.json`~~ — all nine Ancients × three characters × four beats, 108 lines. The eight past the Architect are keyed from wiki display names; a wrong key is silently ignored, so run `tools/gen_ancient_dialogue.py check --base <base game's ancients.json>` once to verify the IDs, and `rename` to fix any that are off. `design/ancient-dialogue.md` has the shape and the voices.
- [x] ~~Rewrite all placeholder strings once Phase 0 is locked~~
- [x] ~~`CharacterSelectDesc` — the pitch a player reads before committing 45 minutes~~ — each character's `description` in `characters.json`

---

## Phase 2 — The starter kit

The single highest-leverage balance decision in the whole character. A player sees the starting deck 40+ times per run.

- [x] ~~**Write your own Strike and Defend.**~~ — Done for all three — `StrikePaladin`/`DefendPaladin`, and the pairs in each character's `Cards/Starter.cs`. Nothing is inherited from Ironclad any more.
- [x] ~~Decide the starter deck ratio. 5 Strike / 5 Defend is the default; deviating is a strong statement (Necrobinder and Defect both do).~~ — 4 Strike / 4 Defend + 2 signature cards for all three
- [x] ~~**Add 1–2 signature starter cards** that teach the mechanic on turn one.~~ — Paladin `Prayer`/`Smite`, Gunslinger `Reload`/`QuickDraw`, Alchemist `Infusion`/`AegisFormula`. This is how a character introduces itself. If your mechanic isn't visible in the opening hand, players won't find it.
- [x] ~~**Design the starting relic.** `RelicRarity.Starter`. It should encode the fantasy, not just give stats. Burning Blood (heal on combat end) *is* the Ironclad's attrition identity in one relic.~~
- [x] ~~Replace `BurningBlood` in `StartingRelics`.~~ Paladin `ConsecratedPlate`, Gunslinger `OldIron`, Alchemist `AlchemicalSatchel`, all with icon art.
- [x] ~~`StartingPotions` — usually empty; override only for a deliberate reason.~~ — left empty

**Sanity check:** play 10 Act 1 openings with only the starter deck. If you can't reliably clear the first three fights, it's too weak. If you never take damage, it's too strong.

---

## Phase 3 — Custom mechanics (optional, but this is the interesting part)

**Yes, you can add Stars/Orbs/Focus-style mechanics.** BaseLib 3.4.5 exposes a full custom-resource system. Verified types:

| Need | BaseLib type |
|---|---|
| A new spendable resource (Stars-like) | `CustomResource`, `CustomResources<T>`, `BasicCustomResource` |
| Cards that cost that resource | `CustomResourceCost<T>`, `ICustomResourceCost` |
| React to spending | `IAfterSpendResource<T>` |
| Change cost contextually | `IModifyResourceCostInCombat<T>` |
| Orbs (Defect-style) | `CustomOrbModel` — channel/evoke/passive SFX, custom sprite |
| A new card pile beyond Draw/Hand/Discard/Exhaust | `CustomPile`, `CustomPiles` |
| Stance-like temporary state | `CustomTemporaryPowerModel` |
| New enum values (target types, reward types, keywords) | `CustomEnumAttribute`, `CustomEnums` |
| New card keywords | `CustomKeywords` |
| Summons / pets | `CustomPetModel` |
| Resource UI | `CustomEnergyCounter`, `ICustomEnergyIconPool`, `ICustomResourceVisualsHandler` |

`ICustomResourceCost` alone covers scoping that most mods get wrong: `SetThisTurn`, `SetThisCombat`, `SetUntilPlayed`, `SetThisTurnOrUntilPlayed`, plus `UpgradeCostBy` / `ResetForDowngrade` and `ResolveXValue` for X-cost cards. Use these rather than hand-rolling cleanup.

Note: **Focus is not a special mechanic** — it's a Power. Anything Focus-shaped is a `CustomPowerModel`, no resource plumbing needed.

- [x] ~~Decide: new resource, orbs, custom pile, or none~~ — Gunslinger Cylinder, Alchemist Lab/Belt, Paladin Plating/Seals/Spirit
- [x] ~~Implement the resource type and its visuals handler~~ — `Gunslinger/Cylinder/` with its UI; the Belt rides the potion slots (`Alchemist/Lab/`)
- [x] ~~Wire `ShouldAlwaysShowStarCounter` (or the custom equivalent) so the counter is visible when relevant~~ — not needed; nothing is star-based
- [x] ~~Implement generation *and* sinks — a resource with no sink is a scoreboard, not a mechanic~~
- [x] ~~**Decide the cap and the overflow rule.** Uncapped resources break in long fights.~~ — six chambers; the Belt is capped by potion slots
- [ ] ⚠️ **Decide what happens at combat end.** Carrying over between fights is a huge power spike; verify it's intended.
- [ ] ⚠️ Test the mechanic against a Time Eater-style long fight and a burst fight — resources tend to break at one extreme or the other
- [ ] ⚠️ Test in multiplayer if you care about it: custom resources need state sync, which is why BaseLib is a hard dependency

---

## Phase 4 — The card set

### How big is a real character's card set?

Pool membership is declared **by the pool**, not the card — `CardPoolModel.GenerateAllCards()` is where the list lives. Decoding that method for every shipped pool gives the true counts:

| Pool | Cards |
|---|---:|
| DefectCardPool | 88 |
| NecrobinderCardPool | 88 |
| RegentCardPool | 88 |
| SilentCardPool | 88 |
| IroncladCardPool | 87 |
| ColorlessCardPool | 64 |
| EventCardPool | 27 |
| CurseCardPool | 18 |
| TokenCardPool | 14 |
| StatusCardPool | 12 |
| QuestCardPool | 3 |

**Every shipped character has 87–88 cards.** That consistency is a deliberate design target, not an accident — treat it as the real bar.

### Targets

- [x] ~~**Minimum viable:** ~35–45 cards. Below this the pool exhausts and runs feel repetitive by Act 2. This is a *prototype* threshold, not a shipping one.~~
- [x] ~~**Comfortable:** 60–75 cards.~~
- [x] ~~**Parity with base game:** ~88 cards.~~ — Paladin 86, Alchemist 90, Gunslinger 104
- [x] ~~**Rarity split.**~~ — Basic/Common/Uncommon/Rare: Paladin 4/20/36/26 (exactly on template), Gunslinger 4/26/46/28, Alchemist 4/18/45/22 (+1 Status). Measured from every shipped pool, the template is near-identical across all five characters:

| | Basic | Common | Uncommon | Rare | Total |
|---|---:|---:|---:|---:|---:|
| per character | 3-4 | 20 | **36** | **26** | 87-88 |

  **Uncommon is the largest tier, and there are more Rares than Commons.** That is the
  opposite of the usual assumption. Plan the bulk of the work as uncommons. Commons are the
  backbone you see most, so they must be *playable but unexciting*; rares are allowed to be
  build-defining, and at 26 apiece they carry a lot of the character's identity.
- [x] ~~**Type split.** Attacks / Skills / Powers. Power-heavy characters need more early defense to survive the setup turns.~~ — Attack/Skill/Power: Paladin 24/34+8 Seals/20, Gunslinger 41/47/16, Alchemist 22/50/17

`CardRarity`: `Basic`, `Common`, `Uncommon`, `Rare`, `Ancient`, `Event`, `Token`, `Status`, `Curse`, `Quest`
`CardType`: `Attack`, `Skill`, `Power`, `Status`, `Curse`, `Quest`
`TargetType`: `Self`, `AnyEnemy`, `AllEnemies`, `RandomEnemy`, `AnyPlayer`, `AnyAlly`, `AllAllies`, `TargetedNoCreature`, `Osty`
`CardKeyword`: `Exhaust`, `Ethereal`, `Innate`, `Unplayable`, `Retain`, `Sly`, `Eternal`

### Per-card checklist

For every card:

- [x] ~~Class extends `<Name>Card`, constructor passes `(cost, type, rarity, target)`~~ — all 280
- [x] ~~`[Pool(typeof(<Name>CardPool))]` is inherited from the base — don't re-annotate~~
- [x] ~~Upgrade defined (what `+` does). Prefer "meaningfully better" over "+2 damage" on at least a third of the set.~~ — 279/280; `VolatileResidue` is a Status card
- [x] ~~Localization entry: `HELLOSPIRE-CARD_NAME.title` and `.description`~~ — all 280; every `{Var}` in them resolves to a var the card defines
- [x] ~~Description uses the game's formatting variables (`{Damage:diff()}`, `{Block:diff()}`) so upgrades and Strength show correctly — **hardcoded numbers in descriptions are a bug**, they won't reflect buffs.~~ — audited 2026-09-28. The 35 descriptions with a literal number were checked against their code. Two were real bugs and are fixed: `DIVINE_INTERVENTION`'s "10 Block" is Dexterity-affected (`ValueProp.Move`), so it is now a `BlockVar` shown as `{Block:diff()}`; `VENGEFUL_MENDING` used `{Heal}` where every other heal card uses `{Heal:diff()}`. The other 33 are correct as written: every literal is a fixed rider that no upgrade changes and no buff touches (Tithe Block and damage are `Unpowered`, Seal payoffs are `Unpowered` constants, and the rest are draw, discard, Energy, Weak and Spirit counts).
- [x] ~~Art at `card_portraits/card_name.png` (1000×760 normal, 606×852 full-art; 250×190 / 250×350 small variants).~~ — every card has unique art as of 2026-09-28.
- [x] ~~Keywords set where relevant~~
- [ ] ⚠️ Plays correctly with zero energy, at max hand size, and when the target dies mid-effect. *Not recorded as tested.*

### Card design coverage

Make sure the pool answers each of these, or the character has a structural hole:

Covered by all three unless noted:

- [x] ~~Single-target damage at 1 cost~~
- [x] ~~AoE damage~~
- [x] ~~Block that scales~~ — Plating, Armor, Aegis
- [x] ~~Draw~~
- [x] ~~Energy generation or cost reduction~~
- [x] ~~Something that answers a big incoming hit (block burst, weak, intangible-like)~~
- [x] ~~Deck manipulation or thinning~~
- [x] ~~At least 3 rares that suggest *different* builds~~ — the three Paladin lanes, Gunslinger gun vs. Gadgets, Alchemist Brew/Distill/Invest/Render
- [ ] ⚠️ At least one card that's a trap in most decks but excellent in one — this is what makes archetypes feel discovered. *A playtest judgement; not yet assessed.*

---

## Phase 5 — Relics

Base game ships **298 relics**, but the split is the surprising part:

| Pool | Relics |
|---|---:|
| EventRelicPool | 140 |
| SharedRelicPool | 118 |
| IroncladRelicPool | 8 |
| SilentRelicPool | 8 |
| DefectRelicPool | 8 |
| NecrobinderRelicPool | 8 |
| RegentRelicPool | 8 |

**Every character gets exactly 8 character-specific relics.** The overwhelming majority of relics are shared or event relics that any character can find. This is a much smaller scope than it first appears — don't over-build here.

- [x] ~~Starting relic *(Phase 2)*~~
- [x] ~~**8 character-specific relics** to match base-game parity.~~ — Gunslinger 9, Alchemist 9, Paladin 8 (Judge's Gavel and Tithing Box added 2026-09-28; ⚠️ not yet compiled or playtested).
- [x] ~~Rarity spread: `RelicRarity` is `Starter`, `Common`, `Uncommon`, `Rare`, `Shop`, `Event`, `Ancient`.~~ — all three span Starter through Shop.
- [x] ~~At least 2 that interact with your custom mechanic specifically~~
- [x] ~~Each has: `PackedIconPath`, `PackedIconOutlinePath`, `BigIconPath`, and loc entries.~~ — all 26, as of 2026-09-28.
- [ ] ⚠️ **Avoid strictly-better-than-basegame relics.** They warp every run they appear in. *Not audited.*
- [ ] ⚠️ Check each against the Act 1 boss relic pool — a relic that trivializes an early boss is a problem

---

## Phase 6 — Potions

Base game ships **64 potions**. `PotionRarity`: `Common`, `Uncommon`, `Rare`, `Event`, `Token`.

- [x] ~~3–6 character potions~~ — 3 each for Paladin and Gunslinger, 25 for the Alchemist
- [x] ~~Extend `<Name>Potion`, images + outlines, loc entries~~
- [x] ~~Potions are emergency buttons~~ — they should solve a problem, not add incremental value

---

## Phase 7 — Art and audio

The template ships placeholders that will absolutely ship if you let them.

### Getting reference assets out of the game

You cannot match the game's look without seeing how it does things. The game's art lives inside `SlayTheSpire2.pck`.

Done once for the pack — `art_reference/` and the base-game rigs copied into `spine/` came out of it:

- [x] ~~Install [**GDRE Tools**](https://github.com/GDRETools/gdsdecomp/releases) (Godot RE Tools — also available via `winget install GDRETools.gdsdecomp`)~~
- [x] ~~Run it → **Recover Project** → open `Slay the Spire 2\SlayTheSpire2.pck`~~
- [x] ~~Extract the whole thing to a scratch folder for browsing. You get `localization/` (every base-game string, invaluable for matching description phrasing and keyword grammar), the full art tree, and decompiled code under `src/Core`~~
- [x] ~~Study 5–10 base card portraits before drawing anything: palette, value range, how much of the frame the subject fills, how silhouettes read at small size~~

### Exact dimensions

From the template's own base classes — these are not suggestions, wrong sizes get scaled and look soft:

| Asset | Size |
|---|---|
| Card art, normal | 1000×760 (500×380 also works, it scales) |
| Card art, full-art | 606×852 (2:3) |
| Card art small variant, normal | 250×190 |
| Card art small variant, full-art | 250×350 |

Ship both the large and small variants. The small ones are a performance measure, not an optional extra.

### Making the art

There is no single community pipeline; the practical options:

- **Draw or paint it.** Highest ceiling, slowest. The base game's style is painterly with strong silhouettes and a limited palette per character.
- **Generative tools, then heavy manual cleanup.** Common in practice for card art at 88-cards scale. Raw output rarely matches the game's palette or framing — expect to repaint edges, unify lighting, and crop to the game's composition conventions.
- **Commission it.** The realistic answer for a character mod you intend people to actually play. 88 card portraits is a genuine art budget.
- **Ship deliberate placeholder art and iterate.** Legitimate for an early release, as long as you say so on the mod page.

Existing art-replacement mods worth studying for conventions: [Card Art Editor](https://www.nexusmods.com/slaythespire2/mods/293), [Custom Card Texture Loader](https://www.nexusmods.com/slaythespire2/mods/471), and the various full-art packs on Nexus.

- [x] ~~**Budget the art before designing 88 cards.** Art is almost always the reason character mods stall. Decide the pipeline first, then size the card set to what that pipeline can actually produce.~~ — moot now; the sets are built and the remaining art is listed in the README

### Asset checklist

- [x] ~~`character_icon_char_name.png`~~ — `charui/<character>/`, all three
- [x] ~~`char_select_char_name.png` and `_locked` variant~~
- [x] ~~`map_marker_char_name.png`~~
- [x] ~~`mod_image.png` (mod list / Workshop thumbnail)~~
- [x] ~~`charui/big_energy.png` and `charui/text_energy.png`~~
- [x] ~~Card frame or HSV tint.~~ — all three via `ShaderColor`.
- [x] ~~Card art for every card *(the long pole — budget for it early)*.~~ — 280/280 unique.
- [x] ~~Relic and potion icons.~~ — all potions and relics, tray and tooltip sizes.
- [ ] ⚠️ `CharacterSelectBg`, `CharacterSelectTransitionPath`. *All three have a background; none has a transition of its own.*
- [ ] ⚠️ `RestSiteAnimPath`, `MerchantAnimPath` — the character appears at rest sites and shops. *Both the shop rig swap and the rest-site rig swap (`RoomSkins`, `012c5e6`) are code-complete and use each character's own rig, not the placeholder's; neither has been confirmed in a running game yet.*
- [ ] ⚠️ SFX: `AttackSfx`, `CastSfx`, `PowerUpSfx`, `DeathSfx`, `CharacterSelectSfx`, `CharacterTransitionSfx`. *All inherited from Ironclad.*
- [ ] ⚠️ Animation timing: `AttackAnimDelay`, `CastAnimDelay`, `PowerUpAnimDelay` — these are abstract, you must set them, and wrong values make every attack feel off. *Inherited from BaseLib's placeholder; not tuned to the Silent or Necrobinder rigs.*
- [ ] ⚠️ `ArmRockTexture` / `ArmPaperTexture` / `ArmScissorsTexture` / `ArmPointingTexture`. *Inherited from Ironclad (Silent for the Alchemist).*
- [ ] ⚠️ `TrailPath`. *Inherited.*

---

## Phase 8 — Balance

The part that separates a mod people try from one people keep installed.

Nothing in this phase is recorded as done. The Gunslinger has had one balance pass (see the README), but none of the checks below was logged.

### Derive benchmarks instead of guessing

Do not eyeball this. The base game is right there and you have the tooling to read it — the scratch project at `C:\Users\Ross\Tools\EnumDump` already decodes property IL out of `sts2.dll`.

- [ ] ⚠️ **Build a damage-per-energy table from base-game commons.** Decompile a dozen `Models.Cards` commons across characters, record cost vs. damage vs. block vs. rider effects. That table is your ruler.
- [ ] ⚠️ Do the same for uncommons and rares to learn how much the game lets rarity buy.
- [ ] ⚠️ Compare every card you've written against the band for its cost and rarity. Anything outside it needs a reason you can say out loud.

### Structural checks

- [ ] ⚠️ **No infinite loops** unless deliberate and gated. Check: cards that draw + reduce cost + return themselves.
- [ ] ⚠️ **Scaling has a ceiling** or a real cost. Unbounded scaling trivializes Act 3+.
- [ ] ⚠️ **The character can lose.** If you never die in testing, the kit is overtuned — or you're only testing the good draws.
- [ ] ⚠️ **Test at Ascension 0 and Ascension 20.** Characters that are fine at A0 often collapse at A20 (or vice versa if they scale).
- [ ] ⚠️ **Test the bad draw.** Shuffle to worst-case openings deliberately.
- [ ] ⚠️ Check interaction with basegame **colorless** and **shop** cards.
- [ ] ⚠️ Check interaction with the strongest basegame relics — anything that doubles or duplicates is where combos break.
- [ ] ⚠️ Verify against each Act boss individually. Bosses are the real balance test, not normal fights.

### Playtest discipline

- [ ] ⚠️ **20+ complete runs** before publishing. Not 20 Act 1s.
- [ ] ⚠️ Log every run: seed, final deck, where it died, what felt bad. Patterns emerge around run 12.
- [ ] ⚠️ **Track win rate.** Wildly above or below the basegame characters' is the signal.
- [ ] ⚠️ **Watch someone else play it.** You know your own mechanic too well to see what's unclear.
- [ ] ⚠️ Ask specifically: "at what point did you understand what the character does?" If it's after Act 1, the starter kit isn't teaching.
- [ ] ⚠️ Rebalance the *outliers* first — the one card that's in every winning deck, and the ones never picked.

### Common failure modes

- **Everything is good.** If no card is ever a skip, there are no decisions. Some cards should be bad in most decks.
- **The mechanic is a tax.** If the resource is something you manage rather than something you exploit, it isn't fun.
- **Rares that are just bigger commons.** Rares should change how you play, not how hard you hit.
- **Solved starting relic.** If one relic makes every run identical, it's doing too much.
- **Only one viable build.** Aim for at least three that can win.

---

## Phase 9 — Multiplayer

**Short answer: everyone needs the same *gameplay-affecting* mods. Cosmetic mods can differ freely.**

The game enforces this at the lobby handshake. `InitialGameInfoMessage` is exchanged on join and carries:

```
string                          version                 game version
uint32                          idDatabaseHash          fingerprint of the whole model database
List<string>                    gameplayAffectingMods   must match
List<string>                    otherMods               informational
GameMode                        gameMode
RunSessionState                 sessionState
ConnectionFailureReason?        connectionFailureReason
```

And `ConnectionFailureReason` is exactly:

```
None · LobbyFull · NotInSaveGame · RunInProgress · VersionMismatch · ModMismatch
```

So `ModMismatch` is a first-class, designed-for rejection — not a crash or a desync you discover in Act 2.

### What decides which list you land in

The `affects_gameplay` field in your manifest. That's it.

| `affects_gameplay` | Goes into | Consequence |
|---|---|---|
| `true` | `gameplayAffectingMods` | **every player must have it**, matching |
| `false` | `otherMods` | free to differ — cosmetic/UI/art mods |

`HelloSpire.json` sets `"affects_gameplay": true`, which is correct for a character mod — it adds cards and a character to the model database, so any client without it cannot deserialize the run.

The `idDatabaseHash` is the deeper check. You can watch it in your own log:

```
ModelIdSerializationCache initialized. Categories: 20 Entries: 1622 Epochs: 57 Hash: 3954186980
```

Different content → different entry count → different hash. This is why version-matched mods matter, not just same-named mods: a teammate running HelloSpire v0.1 against your v0.2 has a different model set.

### Checklist

- [x] ~~Keep `affects_gameplay: true` (correct for any character mod)~~
- [x] ~~Set `affects_gameplay: false` **only** for genuinely cosmetic mods — mislabeling causes desyncs rather than a clean rejection~~ — not applicable; this is a gameplay mod
- [x] ~~BaseLib is a **hard requirement** for multiplayer custom content — it handles custom state sync and registers custom message wrappers (your log shows it claiming message IDs 128 and 129)~~ — declared in `HelloSpire.json`
- [ ] ⚠️ Custom resources (Phase 3) must serialize — verify a resource's value survives a host/client sync, not just a save/load
- [ ] ⚠️ Test an actual 2-player run, not just a lobby join. Desyncs surface during card resolution, not at connect. *None of the 19 multiplayer cards has been through one.*
- [ ] ⚠️ Test the rejection path: have someone join without the pack and confirm a clean `ModMismatch`, not a hang
- [ ] ⚠️ Version your releases properly — a mod ID match with a content mismatch is the nastiest failure mode. *Still `v0.0.0`.*
- [ ] ⚠️ `RemoteTargetingLineColor` / `RemoteTargetingLineOutline` on the character are multiplayer-only visuals; set them or your character looks unfinished in co-op
- [x] ~~**Gate multiplayer-only cards out of solo runs.**~~ `CardModel.MultiplayerConstraint` returning `CardMultiplayerConstraint.MultiplayerOnly` is the mechanism — pools filter on `RunState.CardMultiplayerConstraint` when asked for unlocked cards, so the `[Pool]` attribute stays as-is. The Paladin's nine party cards carry it per-card; the Gunslinger's five carry it once on `GunslingerMultiplayerCard`, the Alchemist's five on `AlchemistMultiplayerCard`.
- [ ] ⚠️ Every multiplayer card still needs a defined single-player behaviour — the gate keeps them out of the solo *offer*, but save continuation or a lobby that empties out can still put one in a solo deck. *Not audited.*

---

## Phase 10 — Meta and polish

None of the optional extras (unlocks, achievements, events, encounters, badges) exists yet. Each is a decision as much as a task; "not wanted" is a fine answer, but record it here.

- [ ] ⚠️ Unlocks — `UnlocksAfterRunAs` if the character should be gated
- [ ] ⚠️ `GetUnlockText` — what the locked tile says
- [ ] ⚠️ `RunWonAchievement`
- [x] ~~Ancient dialogue for every Ancient, not just the Architect~~ — 108 lines, `design/ancient-dialogue.md` records the roster, the four-beat shape and the three voices
- [ ] ⚠️ Verify the eight inferred Ancient IDs against the base game's own (`gen_ancient_dialogue.py check --base ...`); a wrong ID makes that Ancient silently mute
- [ ] ⚠️ Character-specific events (`CustomEventModel`)
- [ ] ⚠️ Character-specific encounters (`CustomEncounterModel`, `CustomMonsterModel`)
- [ ] ⚠️ Badges (`CustomBadge`) — end-of-run flavor
- [ ] ⚠️ `ShouldReceiveCombatHooks` — set correctly or passives silently won't fire. *Not overridden by any character; confirm the inherited value is right.*

---

## Phase 11 — Release and maintenance

- [ ] ⚠️ Bump `version` in `HelloSpire.json` off `v0.0.0`
- [x] ~~Pin `Alchyr.Sts2.BaseLib` to an explicit version in `HelloSpire.csproj`~~ — pinned to `[3.4.5]`
- [ ] ⚠️ Verify `min_game_version` matches what you actually tested. *The manifest says 0.107.0; this file's APIs were checked against 0.107.1.*
- [ ] ⚠️ Screenshots and a real description
- [ ] ⚠️ Publish to Steam Workshop and/or Nexus
- [ ] ⚠️ Tag the release in git. *No tags yet.*
- [ ] ⚠️ **Set up a re-verification pass for each game update.** Early Access breaks mods. The failure mode looks exactly like MoreAscensions in this repo's history: the mod loads and reports success while individual Harmony patches silently no-op. Read `%APPDATA%\SlayTheSpire2\logs\godot.log` after every game update and grep for `Skipping patch`.
- [x] ~~Adopt the modern manifest conventions from day one: object-form `dependencies` with `min_version`, and an explicit `min_game_version`.~~

---

## Reference

**Where the real docs are:** `data_sts2_windows_x86_64\sts2.xml` — ~5 MB, ~19,600 members, with genuine summaries. Read it before guessing.

**Key API surface:**

| Purpose | Type |
|---|---|
| Mod entry point | `MegaCrit.Sts2.Core.Modding.ModInitializerAttribute` |
| Register content into a pool | `ModHelper.AddModelToPool<,>` |
| Inject / remove models | `ModelDb.Inject`, `ModelDb.Remove` (documented as mods-and-tests only) |
| Run/combat hook subscriptions | `ModHelper.SubscribeForRunStateHooks`, `SubscribeForCombatStateHooks` |
| Character definition | `MegaCrit.Sts2.Core.Models.CharacterModel` |
| Reward odds (balance-relevant) | `MegaCrit.Sts2.Core.Odds.CardRarityOdds`, `PotionRewardOdds` |
| Upgrade-odds hook | `Hook.ModifyCardRewardUpgradeOdds` |

**Abstract members you must implement on `CharacterModel`:** `StartingHp`, `StartingGold`, `StartingDeck`, `StartingRelics`, `CardPool`, `RelicPool`, `PotionPool`, `Gender`, `NameColor`, `AttackAnimDelay`, `CastAnimDelay`.
