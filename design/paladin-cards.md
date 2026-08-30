# The Paladin — cards in code

The live list. If a card is not here, it is not in the mod. Numbers are exactly what the code
says; when a card changes, this file changes in the same commit.

The archived 91-card Spirit set is in [`paladin-faith-cards-archive.md`](paladin-faith-cards-archive.md).

## Basic (4)

| Card | Cost | Type | Effect | Upgrade |
|---|---|---|---|---|
| Strike | 1 | Attack | Deal 6 damage. | +3 damage |
| Defend | 1 | Skill | Gain 5 Block. | +3 Block |
| Mend | 1 | Skill | Heal 5 HP plus your Spirit. Exhaust. | Heal 7 |
| Judgment | 1 | Attack | Deal 10 damage. Trigger the effects of all your Seals. | costs 0 |

Starter deck: 4 Strike, 4 Defend, 1 Mend, 1 Judgment.

**MP only** = `MultiplayerOnly`: never appears in solo rewards/shops. Rules in `paladin.md`:
heals that target any player are MP cards (self-heals are unrestricted), auras are MP Powers,
blessings are MP Skills. The caster's Spirit boosts every heal, whoever receives it.

## Common (15)

| Card | Cost | Type | Effect | Upgrade |
|---|---|---|---|---|
| Crusader Strike | 1 | Attack | Deal 10 damage. Apply 1 Vulnerable. | +3 damage |
| Divine Storm | 1 | Attack | Deal 5 damage to ALL enemies. | +3 damage |
| Blade of Justice | 2 | Attack | Deal 12 damage. | +4 damage |
| Holy Light | 1 | Skill | Heal a player 5 HP plus your Spirit. Draw 1 card. Exhaust. **MP only** | Heal 8 |
| Flash of Light | 0 | Skill | Heal a player 2 HP plus your Spirit. Exhaust. **MP only** | Heal 4 |
| Devotion | 1 | Skill | Gain 2 Spirit. | 3 Spirit |
| Divine Smite | 1 | Attack | Deal 3 damage, plus 3 per Seal. | +3 base |
| Absolve | 1 | Attack | Deal 5 damage. Apply 1 Weak. | +2 / +1 |
| Exorcism | 2 | Attack | Deal 8 damage. Trigger the effects of all your Seals. | +4 damage |
| Comfort | 1 | Skill | Gain 1 Spirit. Heal 3 HP plus your Spirit. Exhaust. | Heal 6 |
| Penance | 0 | Skill | Lose 4 HP. Gain 2 Energy. | 3 Energy |
| Templar's Verdict | 1 | Attack | Deal 4 damage twice. | 4x2 -> 6x2 |
| Word of Glory | 1 | Skill | Heal a player 4 HP plus your Spirit. Exhaust. **MP only** | Heal 7 |
| Bind the Wounds | 1 | Skill | Heal a player 3 plus Spirit; they gain 4 Block. Exhaust. **MP only** | Heal 5 |
| Blessing of Might | 1 | Skill | A player gains 2 Strength. **MP only** | 3 Strength |

## Uncommon (17)

| Card | Cost | Type | Effect | Upgrade |
|---|---|---|---|---|
| Hammer of Wrath | 2 | Attack | Deal 9 damage. Double vs enemies at or below half HP. | +3 damage |
| Consecration | 1 | Power | At the start of your turn, deal 3 damage to ALL enemies. | 5 damage |
| Avenger's Shield | 2 | Attack | Deal 8 damage. Gain 8 Block. | +3/+3 |
| Seal of Righteousness | 1 | Skill | Gain Seal of Righteousness: Attacks deal +2. Judgment: deal 5. | Attacks +3 |
| Seal of Command | 1 | Skill | Gain Seal of Command: first Attack/turn applies 1 Vulnerable. Judgment: 2 Vulnerable. | Judgment: 3 |
| Seal of Justice | 1 | Skill | Gain Seal of Justice: first Attack/turn applies 1 Weak. Judgment: 2 Weak. | Judgment: 3 |
| Seal of Wisdom | 1 | Skill | Gain Seal of Wisdom: first Attack/turn draws. Judgment: hand-size damage, draw 1. | draw 2 |
| Divine Purpose | 1 | Skill | Trigger the effects of all your Seals. Draw 1 card. | costs 0 |
| Shield of the Righteous | 1 | Skill | Gain 5 Block. Trigger the effects of all your Seals. | +3 Block |
| Divine Favor | 0 | Skill | Gain 2 Energy. Exhaust. | 3 Energy |
| Vigil | 1 | Skill | Gain 1 Spirit. Next turn: gain 2 Energy. | 3 Energy |
| Rebuke | 1 | Skill | An enemy loses 2 Strength. | 3 Strength |
| Righteous Fury | 2 | Power | At the start of your turn, gain 1 Strength. | costs 1 |
| Circle of Healing | 2 | Skill | Heal ALL players 4 plus your Spirit. Exhaust. **MP only** | Heal 6 |
| Blessing of Sacrifice | 1 | Skill | Lose 3 HP. A player gains 8 Block. **MP only** | +3 Block |
| Aura of Protection | 1 | Power | Start of turn: ALL players gain 2 Block. **MP only** | 3 Block |
| Aura of Mercy | 2 | Power | Start of turn: heal ALL players 2. **MP only** | Heal 3 |

## Rare (6)

| Card | Cost | Type | Effect | Upgrade |
|---|---|---|---|---|
| Hammer of Justice | 3 | Attack | Deal 4 damage. Stun the enemy. | costs 2 |
| Seal of the Martyr | 1 | Skill | Gain Seal of the Martyr: enemies that hit you take 3. Judgment: 3 to ALL. | 4 / 4 |
| Avenging Wrath | 2 | Power | Whenever you trigger your Seals, they trigger twice. | costs 1 |
| Zeal | 0 | Skill | Gain 1 Energy for each of your Seals. Exhaust. | 2 per Seal |
| Lay on Hands | 1 | Skill | Heal a player 12 HP plus your Spirit. Exhaust. **MP only** | Heal 16 |
| Beacon of Light | 1 | Power | A player becomes the Beacon: your heals also heal them 3. **MP only** | Heal 4 |

## Powers

| Power | Effect |
|---|---|
| Spirit | Increases the healing of your cards. The Paladin's Strength-for-heals; granted by Holy Symbol. |
| Seal of Righteousness | Attacks deal +Amount. Judged: deal 5. |
| Seal of Light | First Attack/turn heals Amount HP. Judged: gain 1 Spirit. |
| Seal of Command | First Attack/turn: 1 Vulnerable. Judged: Amount Vulnerable. |
| Seal of Justice | First Attack/turn: 1 Weak. Judged: Amount Weak. |
| Seal of Wisdom | First Attack/turn: draw 1. Judged: deal damage equal to hand size, draw Amount. |
| Seal of the Martyr | Enemies that hit you take Amount. Judged: Amount to ALL. |
| Avenging Wrath | Judgments trigger all Seals twice. |
| Consecration | Start of turn: Amount damage to ALL enemies. |
| Righteous Fury | Start of turn: gain Amount Strength. |
| Aura of Protection | Start of turn: ALL players gain Amount Block. (MP) |
| Aura of Mercy | Start of turn: heal ALL players Amount. (MP) |
| Beacon of Light | Bearer heals Amount whenever the Paladin heals a player. (MP) |

## Relics

| Relic | Rarity | Effect |
|---|---|---|
| Holy Book | Starter | At the start of each combat, gain Seal of Light. |
| Holy Fervor | Common | Whenever you play a card that heals, gain 1 Strength. Once per turn. |
| Holy Symbol | Common | At the start of each combat, gain 1 Spirit. |

## Adding a card

1. Write the class in `HelloSpireCode/Characters/Paladin/Cards/`, extending `PaladinCard`. The
   `[Pool]` attribute on the base puts it in the Paladin's pool; do not re-annotate.
2. Add `HELLOSPIRE-<CLASS_NAME>.title` and `.description` to `localization/eng/cards.json`. The
   build fails until both exist, and tells you the exact keys.
3. Art resolves by class name in snake_case: `card_portraits/<class_name>.png` (250×190) and
   `card_portraits/big/<class_name>.png` (1000×760). Most names already have art in the repo.
4. `dotnet publish`, launch, play it. Then update this table.

The card editor in `card-editor/` (Dan's) edits costs, values, upgrade deltas and text for
existing cards without touching the C# by hand.
