# From Vanilla Greeter to a Finished Character

A working checklist for taking `HelloSpire` from "a tile appears on character select" to a character that feels like it shipped with the game.

Ordered by dependency, not by effort. Phases 0–3 are load-bearing: everything later assumes they're settled. Phases 4–6 are the bulk of the work. Phase 8 is the one people skip and shouldn't.

Every API name here was verified against **game v0.107.1** (`data_sts2_windows_x86_64\sts2.xml` and `sts2.dll`) and **BaseLib 3.4.5**. Early Access moves; re-verify after breaking updates.

---

## Phase 0 — Decide what the character *is*

Do this before writing code. Every later decision resolves faster when there's a one-sentence answer to "what does this character do that no other one does?"

- [ ] **Write the fantasy in one sentence.** "The Greeter turns enemy attention into resources." Not "a control character with good scaling."
- [ ] **Name the core tension.** Every good StS character has a cost to its power. Ironclad trades HP. Silent trades tempo for setup. Defect trades slots. What does The Greeter give up?
- [ ] **Pick the win condition shape.** Scaling powers? Burst combo? Attrition? Deck-thinning? This determines your rare cards.
- [ ] **Decide whether you need a new mechanic at all.** A character built from existing primitives (damage, block, powers, statuses) is *far* cheaper to build and balance. Add a resource only if the fantasy genuinely can't be expressed without it.
- [ ] **Write 5 fake card names + effects on paper.** If they're boring, the fantasy is wrong. Iterate here, it's free.
- [ ] **Pick a color.** It propagates to `NameColor`, `DialogueColor`, `SpeechBubbleColor`, `DeckEntryCardColor`, `MapDrawingColor`, and the card-back HSV in `HelloSpireCardPool`. Choose once, reuse.

---

## Phase 1 — The character shell

All of this lives in `HelloSpireCode/Character/HelloSpire.cs`.

### Stats

Base-game values, read directly out of `sts2.dll` for reference:

| Character | Starting HP | Starting Gold | Notes |
|---|---:|---:|---|
| Ironclad | 80 | 99 | highest HP, HP-as-resource design |
| Regent | 75 | 99 | uses Stars |
| Defect | 75 | 99 | 3 orb slots (`BaseOrbSlotCount`) |
| Silent | 70 | 99 | lowest of the classic three |
| Necrobinder | 66 | 99 | lowest HP, summon-based |
| **HelloSpire** | **70** | *(inherited)* | currently Silent-equivalent |

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

Base game ships **578 cards** across all pools. A single character's share is a fraction of that, but the shape matters more than the count.

### Targets

- [ ] **Minimum viable:** ~35–45 cards. Below this, runs feel repetitive by Act 2 because the pool exhausts.
- [ ] **Comfortable:** 60–75 cards.
- [ ] **Rarity split.** Base-game convention is roughly Common > Uncommon > Rare. Commons are the backbone — they appear most, so they must be *playable but unexciting*. Rares are allowed to be build-defining.
- [ ] **Type split.** Attacks / Skills / Powers. Power-heavy characters need more early defense to survive the setup turns.

`CardRarity`: `Basic`, `Common`, `Uncommon`, `Rare`, `Ancient`, `Event`, `Token`, `Status`, `Curse`, `Quest`
`CardType`: `Attack`, `Skill`, `Power`, `Status`, `Curse`, `Quest`
`TargetType`: `Self`, `AnyEnemy`, `AllEnemies`, `RandomEnemy`, `AnyPlayer`, `AnyAlly`, `AllAllies`, `TargetedNoCreature`, `Osty`
`CardKeyword`: `Exhaust`, `Ethereal`, `Innate`, `Unplayable`, `Retain`, `Sly`, `Eternal`

### Per-card checklist

For every card:

- [ ] Class extends `HelloSpireCard`, constructor passes `(cost, type, rarity, target)`
- [ ] `[Pool(typeof(HelloSpireCardPool)))]` is inherited from the base — don't re-annotate
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

Base game ships **298 relics**.

- [ ] Starting relic *(Phase 2)*
- [ ] 8–15 character-specific relics
- [ ] Rarity spread: `RelicRarity` is `Starter`, `Common`, `Uncommon`, `Rare`, `Shop`, `Event`, `Ancient`
- [ ] At least 2 that interact with your custom mechanic specifically
- [ ] Each has: `PackedIconPath`, `PackedIconOutlinePath`, `BigIconPath`, and loc entries
- [ ] **Avoid strictly-better-than-basegame relics.** They warp every run they appear in.
- [ ] Check each against the Act 1 boss relic pool — a relic that trivializes an early boss is a problem

---

## Phase 6 — Potions

Base game ships **64 potions**. `PotionRarity`: `Common`, `Uncommon`, `Rare`, `Event`, `Token`.

- [ ] 3–6 character potions
- [ ] Extend `HelloSpirePotion`, images + outlines, loc entries
- [ ] Potions are emergency buttons — they should solve a problem, not add incremental value

---

## Phase 7 — Art and audio

The template ships placeholders that will absolutely ship if you let them.

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

## Phase 9 — Meta and polish

- [ ] Unlocks — `UnlocksAfterRunAs` if the character should be gated
- [ ] `GetUnlockText` — what the locked tile says
- [ ] `RunWonAchievement`
- [ ] Ancient dialogue for every Ancient, not just the Architect *(currently one stub)*
- [ ] Character-specific events (`CustomEventModel`)
- [ ] Character-specific encounters (`CustomEncounterModel`, `CustomMonsterModel`)
- [ ] Badges (`CustomBadge`) — end-of-run flavor
- [ ] `ShouldReceiveCombatHooks` — set correctly or passives silently won't fire
- [ ] Multiplayer: verify custom state syncs; `affects_gameplay: true` is already set in the manifest

---

## Phase 10 — Release and maintenance

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
