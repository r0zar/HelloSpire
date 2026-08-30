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
| Judgment | 1 | Attack | Trigger your Seal's effect. Unplayable without a Seal. | costs 0 |

Starter deck: 4 Strike, 4 Defend, 1 Mend, 1 Judgment.

## Common (8)

| Card | Cost | Type | Effect | Upgrade |
|---|---|---|---|---|
| Crusader Strike | 1 | Attack | Deal 8 damage. | +3 damage |
| Divine Storm | 1 | Attack | Deal 5 damage to ALL enemies. | +3 damage |
| Blade of Justice | 2 | Attack | Deal 12 damage. | +4 damage |
| Holy Light | 1 | Skill | Heal 5 HP plus your Spirit. Draw 1 card. Exhaust. | Heal 8 |
| Flash of Light | 0 | Skill | Heal 2 HP plus your Spirit. Exhaust. | Heal 4 |
| Seal of Light | 1 | Skill | Gain Seal of Light: Attacks heal 1 HP. Judgment: heal 4 plus Spirit. | Attacks heal 2 |
| Devotion | 1 | Skill | Gain 2 Spirit. | 3 Spirit |
| Consecration | 2 | Power | At the start of your turn, deal 3 damage to ALL enemies. | 5 damage |

## Uncommon (2)

| Card | Cost | Type | Effect | Upgrade |
|---|---|---|---|---|
| Hammer of Wrath | 2 | Attack | Deal 9 damage. Double vs enemies at or below half HP. | +3 damage |
| Avenger's Shield | 2 | Attack | Deal 8 damage. Gain 8 Block. | +3/+3 |

## Rare (1)

| Card | Cost | Type | Effect | Upgrade |
|---|---|---|---|---|
| Hammer of Justice | 3 | Attack | Deal 4 damage. Stun the enemy. | costs 2 |

## Powers

| Power | Effect |
|---|---|
| Spirit | Increases the healing of your cards. The Paladin's Strength-for-heals; granted by Holy Symbol. |
| Seal of Righteousness | Attacks deal +2 damage. Judged: deal 8 damage. One Seal at a time; Judging does not consume it. |
| Seal of Light | Attacks heal Amount HP. Judged: heal 4 plus Spirit. |
| Consecration | Start of turn: Amount damage to ALL enemies. |

## Relics

| Relic | Rarity | Effect |
|---|---|---|
| Holy Book | Starter | At the start of each combat, gain Seal of Righteousness. |
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
