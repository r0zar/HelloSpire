# The Paladin — cards in code

The live list. If a card is not here, it is not in the mod. Numbers are exactly what the code
says; when a card changes, this file changes in the same commit.

The archived 91-card Faith set is in [`paladin-faith-cards-archive.md`](paladin-faith-cards-archive.md).

## Basic (4)

| Card | Cost | Type | Effect | Upgrade |
|---|---|---|---|---|
| Strike | 1 | Attack | Deal 6 damage. | +3 damage |
| Defend | 1 | Skill | Gain 5 Block. | +3 Block |
| Mend | 1 | Skill | Heal 5 HP plus your Faith. Exhaust. | Heal 7 |
| Hammer of Justice | 3 | Attack | Deal 4 damage. Stun the enemy. | costs 2 |

Starter deck: 4 Strike, 4 Defend, 1 Mend, 1 Hammer of Justice.

## Common (1)

| Card | Cost | Type | Effect | Upgrade |
|---|---|---|---|---|
| Smite | 1 | Attack | Deal 9 damage. Gain 1 Faith. | +3 damage |

## Uncommon (0)

## Rare (0)

## Powers

| Power | Effect |
|---|---|
| Faith | Increases the healing of your cards. The Paladin's Strength-for-heals; granted by Holy Symbol (3/combat) and Smite. |

## Relics

| Relic | Rarity | Effect |
|---|---|---|
| Holy Symbol | Starter | At the start of each combat, gain 3 Faith. |
| Holy Fervor | Common | Whenever you play a card that heals, gain 1 Strength. Once per turn. |

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
