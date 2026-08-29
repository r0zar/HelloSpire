# From Empty Shell to a Finished Character

A working checklist for taking a HelloSpire character from "a tile appears on character select" to one that feels like it shipped with the game.

**Run this once per character.** The Paladin, the Alchemist and the Gunslinger each need their own pass through Phases 0–8; Phases 9–11 are pack-wide and done once. Where a phase says "the character", substitute whichever you're working on.

Ordered by dependency, not by effort. Phases 0–3 are load-bearing: everything later assumes they are settled. Phases 4–7 are the bulk of the work. Phase 8 is the one people skip and should not. Phase 9 matters the moment anyone plays co-op.

Every API name here was verified against **game v0.107.1** (`data_sts2_windows_x86_64\sts2.xml` and `sts2.dll`) and **BaseLib 3.4.5**. Early Access moves; re-verify after breaking updates.

---

## Phase 0 — Decide what the character *is*

Do this before writing code. Every later decision resolves faster when there's a one-sentence answer to "what does this character do that no other one does?"

- [ ] **Write the fantasy in one sentence.** "The Gunslinger spends ammunition it cannot easily replace." Not "a burst character with good scaling."
- [ ] **Name the core tension.** Every good StS character has a cost to its power. Ironclad trades HP. Silent trades tempo for setup. Defect trades slots. What does this one give up?
- [ ] **Check it against the other two.** Three characters in one pack should not overlap. If the Paladin and the Alchemist both want to stall and scale, one of them needs to change.
- [ ] **Pick the win condition shape.** Scaling powers? Burst combo? Attrition? Deck-thinning? This determines your rare cards.
- [ ] **Decide whether you need a new mechanic at all.** A character built from existing primitives (damage, block, powers, statuses) is *far* cheaper to build and balance. Add a resource only if the fantasy genuinely can't be expressed without it.
- [ ] **Write 5 fake card names + effects on paper.** If they're boring, the fantasy is wrong. Iterate here, it's free.
- [ ] **Pick a color.** It propagates to `NameColor`, `DialogueColor`, `SpeechBubbleColor`, `DeckEntryCardColor`, `MapDrawingColor`, and the card-back HSV in the character's `CardPool`. Choose once, reuse.

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

- [ ] Set `StartingHp`. 66–80 is the shipped range. Go low only if the kit has real defensive or evasive tools; go high only if the kit spends HP.
- [ ] Set `StartingGold` explicitly (all base characters use 99 — deviating is a real balance lever, not a flavor one).
- [ ] Override `MaxEnergy` only if the character is genuinely built around it. Default is fine for almost everything.
- [ ] Override `BaseOrbSlotCount` only if you're doing an orb character (see Phase 3).
- [ ] Set `Gender` (`CharacterGender.Neutral` / `Feminine` / `Masculine`) — drives grammar in generated text, and must agree with the pronoun loc keys.

### Identity and color

- [ ] `NameColor` — statistics screen
- [ ] `DialogueColor` — Ancient dialogue speech bubble
- [ ] `SpeechBubbleColor` — general speech (returns `VfxColor`, not `Color`)
- [ ] `MapDrawingColor`
- [ ] `RemoteTargetingLineColor` / `RemoteTargetingLineOutline` — multiplayer targeting
- [ ] `EnergyLabelOutlineColor`
- [ ] Card-back HSV in `HelloSpireCardPool` (`H`/`S`/`V`), or supply a `CustomFrame` texture instead

### Localization

All keys are **flat dotted strings** under `HelloSpire/localization/eng/`. The `STS001` analyzer fails the build on any missing key, so it will tell you exactly what's outstanding — treat build errors as your checklist.

- [ ] `characters.json` — `title`, `titleObject`, `description`, four pronoun keys, `goldMonologue`, `eventDeathPrevention`, `aromaPrinciple`, `cardsModifierTitle`, `cardsModifierDescription`, `banter.alive.endTurnPing`, `banter.dead.endTurnPing` *(done for placeholder text — rewrite when the fantasy is final)*
- [ ] `ancients.json` — Architect dialogue *(placeholder written)*
- [ ] Rewrite all placeholder strings once Phase 0 is locked
- [ ] `CharacterSelectDesc` — the pitch a player reads before committing 45 minutes

---

## Phase 2 — The starter kit

The single highest-leverage balance decision in the whole character. A player sees the starting deck 40+ times per run.

- [ ] **Write your own Strike and Defend.** Currently inherited from Ironclad (`StrikeIronclad`, `DefendIronclad`) — placeholder, must be replaced.
- [ ] Decide the starter deck ratio. 5 Strike / 5 Defend is the default; deviating is a strong statement (Necrobinder and Defect both do).
- [ ] **Add 1–2 signature starter cards** that teach the mechanic on turn one. This is how a character introduces itself. If your mechanic isn't visible in the opening hand, players won't find it.
- [ ] **Design the starting relic.** `RelicRarity.Starter`. It should encode the fantasy, not just give stats. Burning Blood (heal on combat end) *is* the Ironclad's attrition identity in one relic.
- [ ] Replace `BurningBlood` in `StartingRelics`.
- [ ] `StartingPotions` — usually empty; override only for a deliberate reason.

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

- [ ] Decide: new resource, orbs, custom pile, or none
- [ ] Implement the resource type and its visuals handler
- [ ] Wire `ShouldAlwaysShowStarCounter` (or the custom equivalent) so the counter is visible when relevant
- [ ] Implement generation *and* sinks — a resource with no sink is a scoreboard, not a mechanic
- [ ] **Decide the cap and the overflow rule.** Uncapped resources break in long fights.
- [ ] **Decide what happens at combat end.** Carrying over between fights is a huge power spike; verify it's intended.
- [ ] Test the mechanic against a Time Eater-style long fight and a burst fight — resources tend to break at one extreme or the other
- [ ] Test in multiplayer if you care about it: custom resources need state sync, which is why BaseLib is a hard dependency

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

- [ ] **Minimum viable:** ~35–45 cards. Below this the pool exhausts and runs feel repetitive by Act 2. This is a *prototype* threshold, not a shipping one.
- [ ] **Comfortable:** 60–75 cards.
- [ ] **Parity with base game:** ~88 cards.
- [ ] **Rarity split.** Base-game convention is roughly Common > Uncommon > Rare. Commons are the backbone — they appear most, so they must be *playable but unexciting*. Rares are allowed to be build-defining.
- [ ] **Type split.** Attacks / Skills / Powers. Power-heavy characters need more early defense to survive the setup turns.

`CardRarity`: `Basic`, `Common`, `Uncommon`, `Rare`, `Ancient`, `Event`, `Token`, `Status`, `Curse`, `Quest`
`CardType`: `Attack`, `Skill`, `Power`, `Status`, `Curse`, `Quest`
`TargetType`: `Self`, `AnyEnemy`, `AllEnemies`, `RandomEnemy`, `AnyPlayer`, `AnyAlly`, `AllAllies`, `TargetedNoCreature`, `Osty`
`CardKeyword`: `Exhaust`, `Ethereal`, `Innate`, `Unplayable`, `Retain`, `Sly`, `Eternal`

### Per-card checklist

For every card:

- [ ] Class extends `<Name>Card`, constructor passes `(cost, type, rarity, target)`
- [ ] `[Pool(typeof(<Name>CardPool))]` is inherited from the base — don't re-annotate
- [ ] Upgrade defined (what `+` does). Prefer "meaningfully better" over "+2 damage" on at least a third of the set.
- [ ] Localization entry: `HELLOSPIRE-CARD_NAME.title` and `.description`
- [ ] Description uses the game's formatting variables (`{Damage:diff()}`, `{Block:diff()}`) so upgrades and Strength show correctly — **hardcoded numbers in descriptions are a bug**, they won't reflect buffs
- [ ] Art at `card_portraits/card_name.png` (1000×760 normal, 606×852 full-art; 250×190 / 250×350 small variants)
- [ ] Keywords set where relevant
- [ ] Plays correctly with zero energy, at max hand size, and when the target dies mid-effect

### Card design coverage

Make sure the pool answers each of these, or the character has a structural hole:

- [ ] Single-target damage at 1 cost
- [ ] AoE damage
- [ ] Block that scales
- [ ] Draw
- [ ] Energy generation or cost reduction
- [ ] Something that answers a big incoming hit (block burst, weak, intangible-like)
- [ ] Deck manipulation or thinning
- [ ] At least 3 rares that suggest *different* builds
- [ ] At least one card that's a trap in most decks but excellent in one — this is what makes archetypes feel discovered

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

- [ ] Starting relic *(Phase 2)*
- [ ] **8 character-specific relics** to match base-game parity
- [ ] Rarity spread: `RelicRarity` is `Starter`, `Common`, `Uncommon`, `Rare`, `Shop`, `Event`, `Ancient`
- [ ] At least 2 that interact with your custom mechanic specifically
- [ ] Each has: `PackedIconPath`, `PackedIconOutlinePath`, `BigIconPath`, and loc entries
- [ ] **Avoid strictly-better-than-basegame relics.** They warp every run they appear in.
- [ ] Check each against the Act 1 boss relic pool — a relic that trivializes an early boss is a problem

---

## Phase 6 — Potions

Base game ships **64 potions**. `PotionRarity`: `Common`, `Uncommon`, `Rare`, `Event`, `Token`.

- [ ] 3–6 character potions
- [ ] Extend `<Name>Potion`, images + outlines, loc entries
- [ ] Potions are emergency buttons — they should solve a problem, not add incremental value

---

## Phase 7 — Art and audio

The template ships placeholders that will absolutely ship if you let them.

### Getting reference assets out of the game

You cannot match the game's look without seeing how it does things. The game's art lives inside `SlayTheSpire2.pck`.

- [ ] Install [**GDRE Tools**](https://github.com/GDRETools/gdsdecomp/releases) (Godot RE Tools — also available via `winget install GDRETools.gdsdecomp`)
- [ ] Run it → **Recover Project** → open `Slay the Spire 2\SlayTheSpire2.pck`
- [ ] Extract the whole thing to a scratch folder for browsing. You get `localization/` (every base-game string, invaluable for matching description phrasing and keyword grammar), the full art tree, and decompiled code under `src/Core`
- [ ] Study 5–10 base card portraits before drawing anything: palette, value range, how much of the frame the subject fills, how silhouettes read at small size

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

- [ ] **Budget the art before designing 88 cards.** Art is almost always the reason character mods stall. Decide the pipeline first, then size the card set to what that pipeline can actually produce.

### Asset checklist

- [ ] `character_icon_char_name.png`
- [ ] `char_select_char_name.png` and `_locked` variant
- [ ] `map_marker_char_name.png`
- [ ] `mod_image.png` (mod list / Workshop thumbnail)
- [ ] `charui/big_energy.png` and `charui/text_energy.png`
- [ ] Card frame or HSV tint
- [ ] Card art for every card *(the long pole — budget for it early)*
- [ ] Relic and potion icons
- [ ] `CharacterSelectBg`, `CharacterSelectTransitionPath`
- [ ] `RestSiteAnimPath`, `MerchantAnimPath` — the character appears at rest sites and shops
- [ ] SFX: `AttackSfx`, `CastSfx`, `PowerUpSfx`, `DeathSfx`, `CharacterSelectSfx`, `CharacterTransitionSfx`
- [ ] Animation timing: `AttackAnimDelay`, `CastAnimDelay`, `PowerUpAnimDelay` — these are abstract, you must set them, and wrong values make every attack feel off
- [ ] `ArmRockTexture` / `ArmPaperTexture` / `ArmScissorsTexture` / `ArmPointingTexture`
- [ ] `TrailPath`

---

## Phase 8 — Balance

The part that separates a mod people try from one people keep installed.

### Derive benchmarks instead of guessing

Do not eyeball this. The base game is right there and you have the tooling to read it — the scratch project at `C:\Users\Ross\Tools\EnumDump` already decodes property IL out of `sts2.dll`.

- [ ] **Build a damage-per-energy table from base-game commons.** Decompile a dozen `Models.Cards` commons across characters, record cost vs. damage vs. block vs. rider effects. That table is your ruler.
- [ ] Do the same for uncommons and rares to learn how much the game lets rarity buy.
- [ ] Compare every card you've written against the band for its cost and rarity. Anything outside it needs a reason you can say out loud.

### Structural checks

- [ ] **No infinite loops** unless deliberate and gated. Check: cards that draw + reduce cost + return themselves.
- [ ] **Scaling has a ceiling** or a real cost. Unbounded scaling trivializes Act 3+.
- [ ] **The character can lose.** If you never die in testing, the kit is overtuned — or you're only testing the good draws.
- [ ] **Test at Ascension 0 and Ascension 20.** Characters that are fine at A0 often collapse at A20 (or vice versa if they scale).
- [ ] **Test the bad draw.** Shuffle to worst-case openings deliberately.
- [ ] Check interaction with basegame **colorless** and **shop** cards.
- [ ] Check interaction with the strongest basegame relics — anything that doubles or duplicates is where combos break.
- [ ] Verify against each Act boss individually. Bosses are the real balance test, not normal fights.

### Playtest discipline

- [ ] **20+ complete runs** before publishing. Not 20 Act 1s.
- [ ] Log every run: seed, final deck, where it died, what felt bad. Patterns emerge around run 12.
- [ ] **Track win rate.** Wildly above or below the basegame characters' is the signal.
- [ ] **Watch someone else play it.** You know your own mechanic too well to see what's unclear.
- [ ] Ask specifically: "at what point did you understand what the character does?" If it's after Act 1, the starter kit isn't teaching.
- [ ] Rebalance the *outliers* first — the one card that's in every winning deck, and the ones never picked.

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

- [ ] Keep `affects_gameplay: true` (correct for any character mod)
- [ ] Set `affects_gameplay: false` **only** for genuinely cosmetic mods — mislabeling causes desyncs rather than a clean rejection
- [ ] BaseLib is a **hard requirement** for multiplayer custom content — it handles custom state sync and registers custom message wrappers (your log shows it claiming message IDs 128 and 129)
- [ ] Custom resources (Phase 3) must serialize — verify a resource's value survives a host/client sync, not just a save/load
- [ ] Test an actual 2-player run, not just a lobby join. Desyncs surface during card resolution, not at connect.
- [ ] Test the rejection path: have someone join without the pack and confirm a clean `ModMismatch`, not a hang
- [ ] Version your releases properly — a mod ID match with a content mismatch is the nastiest failure mode
- [ ] `RemoteTargetingLineColor` / `RemoteTargetingLineOutline` on the character are multiplayer-only visuals; set them or your character looks unfinished in co-op

---

## Phase 10 — Meta and polish

- [ ] Unlocks — `UnlocksAfterRunAs` if the character should be gated
- [ ] `GetUnlockText` — what the locked tile says
- [ ] `RunWonAchievement`
- [ ] Ancient dialogue for every Ancient, not just the Architect *(currently one stub)*
- [ ] Character-specific events (`CustomEventModel`)
- [ ] Character-specific encounters (`CustomEncounterModel`, `CustomMonsterModel`)
- [ ] Badges (`CustomBadge`) — end-of-run flavor
- [ ] `ShouldReceiveCombatHooks` — set correctly or passives silently won't fire

---

## Phase 11 — Release and maintenance

- [ ] Bump `version` in `HelloSpire.json` off `v0.0.0`
- [ ] Pin `Alchyr.Sts2.BaseLib` to an explicit version in `HelloSpire.csproj` — it's `Version="*"` today, so the manifest's `min_version` moves on its own
- [ ] Verify `min_game_version` matches what you actually tested
- [ ] Screenshots and a real description
- [ ] Publish to Steam Workshop and/or Nexus
- [ ] Tag the release in git
- [ ] **Set up a re-verification pass for each game update.** Early Access breaks mods. The failure mode looks exactly like MoreAscensions in this repo's history: the mod loads and reports success while individual Harmony patches silently no-op. Read `%APPDATA%\SlayTheSpire2\logs\godot.log` after every game update and grep for `Skipping patch`.
- [ ] Adopt the modern manifest conventions from day one: object-form `dependencies` with `min_version`, and an explicit `min_game_version`.

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
