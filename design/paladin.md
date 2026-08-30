# The Paladin — design

Status: **rebuilding from the starter deck, one card at a time.** Reset on 2026-08-30.

This is the live design. The earlier full build — Faith across the Triad, Bane, Judged /
Warded / Blessed, 91 cards — is archived in [`paladin-faith-archive.md`](paladin-faith-archive.md)
and [`paladin-faith-cards-archive.md`](paladin-faith-cards-archive.md), with its code at git
`aaec1e4`. It was built end to end and compiled, and none of it was ever played. That is the
lesson this document is organised around.

## The rule

**One card at a time. Each one is a deliberate decision. Playtest before the next.**

Nothing enters the pool because a design doc listed it. A card is added because a run made
its absence felt, or because a specific idea is worth testing in isolation. The card set grows
at the speed of play, not the speed of writing.

## What exists

### Character

| | |
|---|---|
| Starting HP | 75 — matches Regent and Defect; the kit is inherently sturdy, so no more than that |
| Colour | gold `#e8c46a`, on the card frame via `ShaderColor` |
| Starter relic | **Holy Fervor** — *Whenever you play a card that heals, gain 1 Strength. Once per turn.* |

Holy Fervor is the one piece of the old build that survived, because it does not depend on
anything else. It turns the character's least-valued verb (healing) into its most-valued stat
(Strength), which makes every heal card a scaling decision. The once-per-turn cap is what keeps
Mend + a second heal from being a turn-one Demon Form.

### Starter deck — ten cards

```
4x Strike             1E  Deal 6 damage.
4x Defend             1E  Gain 5 Block.
1x Mend               1E  Heal 6 HP.
1x Hammer of Justice  2E  Deal 10 damage. Stun the enemy.
```

The deck reads as *durable but slow to kill*, which is the D&D and WoW read of the class. Mend
is a Defend mirror in HP. Hammer of Justice is a tempo tool — buying a turn is how a defensive
character survives Act 1 — and it is the card that says on turn one what the Paladin is.

No starter card carries any mechanic beyond the base game's. That is deliberate: the starter
must be understood on sight.

### Card pool

Empty. Every reward the Paladin sees today is colorless or shop.

## What is settled

These survived the reset because they are conclusions from evidence, not design preferences:

- **Ally-targeting is co-op content.** The game's `AnyAlly` excludes the owner and every shipped
  `AnyAlly` card is `MultiplayerOnly`. `AnyPlayer` includes the owner and works solo. Write
  "a player", not "an ally", unless the card is meaningless alone.
- **Healing is not a dead verb.** Healing over time and delayed heals sidestep the full-HP
  problem, and Holy Fervor makes every heal a Strength gain regardless.
- **Base-game shape** is 87–88 cards per character at roughly 4 / 20 / 36 / 26 by rarity, and
  8 character relics. That is the eventual target, not the plan.
- **The build pipeline is proven**: cards resolve art by class name in snake_case, localization
  keys are `HELLOSPIRE-<CLASS>` and the STS001 analyzer fails the build on any missing one, and
  `dotnet publish` puts the result in the game.

## What is open

Everything else. In particular, the identity question — *what does the Paladin do that no
other character does?* — is open again. The Faith answer is archived, not rejected; it may come
back in part. The next few cards should be chosen to find out what feels good in play before
committing to any system.

## Candidate next cards

Not a plan. A shortlist to pick one from, each chosen to test one thing:

| Card | Tests |
|---|---|
| **Smite** — 1E, deal 9 | does a plain better Strike make Act 1 feel right, or is it just filler? |
| **Hold the Line** — 1E, gain 8 Block | same question for defence |
| **Vengeful Mending** — 1E, deal 7, heal 3 | the heal-on-attack pattern, and Holy Fervor firing off an Attack |
| **Renew** — 1E Power, heal 2 at turn start | healing over time as a Power; whether a Power belongs this early |
| **Brace** — 1E, Block that survives the turn | whether persistent Block is worth a mechanic at all |

The art for all of these already exists in the repo — Dan painted the full set before the
reset — so adding one is a class, a localization entry and a build.
