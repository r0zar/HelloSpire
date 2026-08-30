# The Paladin — design

Status: **rebuilding from the starter deck, one card at a time.** Reset on 2026-08-30.

This is the live design. The earlier full build — Spirit across the Triad, Bane, Judged /
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
| Starter relic | **Libram of Righteousness** — *At the start of each combat, gain Seal of Righteousness.* |

Holy Fervor (heal a card, gain 1 Strength, once per turn) moves to the relic pool: now that
heals cost Spirit, it pays you for spending it.

### Starter deck — ten cards

```
4x Strike             1E  Deal 6 damage.
4x Defend             1E  Gain 5 Block.
1x Mend               1E  Heal 5 HP plus your Spirit. Exhaust.
1x Judgment           1E  Deal 6 damage. Consume your Seal, triggering its effect.
```

The deck reads as *durable but slow to kill*, which is the D&D and WoW read of the class. Mend
is a Defend mirror in HP. Judgment is a Strike when no Seal is up and the evoke button when one is -- it teaches the
Seal loop from turn one. Hammer of Justice moved to the Rare pool: a bought turn is too strong
to see every run.

No starter card carries any mechanic beyond the base game's. That is deliberate: the starter
must be understood on sight.

### Seals and Judgment

The Defect's orbs, with a one-slot rule. A **Seal** is a passive buff while active; **Judgment**
consumes it and triggers its effect -- channel and evoke, except you only ever have one, so a
new Seal replaces the old. Which Seal is up, and whether to cash it in, is the decision.

**Seal of Righteousness** (starter, from the Libram): Attacks deal +2 damage; Judged, it deals
10 damage. Cracked Core translated: passive trickle, real evoke.

Implementation: a Seal is a `Single`-stack power (`SealPower`); `Seals.Grant` enforces the
one-slot rule, `Seals.Judge` fires `OnJudged` and removes it. New Seals are one subclass, one
loc entry, one icon each.

### Spirit, the third stat (was: Faith)

Strength raises attacks, Dexterity raises Block, **Spirit raises healing** -- one icon in the
power bar next to Strength, reset per combat like the others. The **Holy Symbol** (starter
**Currently sourceless**: the Holy Symbol (its granter) was replaced by the Libram, so Mend
reads +0 until a Spirit-granting card or relic lands. The stat stays; deliberately unfed.

**Mend Exhausts.** That is the anti-stall guarantee now -- one heal per copy per fight, no
economy to police. Spirit is why the one heal is worth building toward: Mend heals 5 + Spirit
(8 on turn one; upgraded 7 + Spirit).

Engine note: the game has no combat-heal modify hook (rest-site heals have one, combat heals do
not), so Spirit cannot intercept heals centrally the way Strength intercepts damage. Paladin heal
cards add it at heal time through one helper (`Spirit.Heal`), which behaves identically for
everything we ship.

The earlier spend-economy design (a Faith pool; Mend cost 1) and the signed holy/unholy
extension are archived thinking; the stat model replaces the pool. Unholy, if it returns, would
be negative Spirit on the same icon.

### Card pool

Empty. Every card is added one at a time, deliberately, after the previous one is playtested.
Everything the Paladin sees today is colorless or shop.

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
other character does?* — is open again. The Spirit answer is archived, not rejected; it may come
back in part. The next few cards should be chosen to find out what feels good in play before
committing to any system.

## Candidate next cards

Not a plan. A shortlist to pick one from, each chosen to test one thing:

| Card | Tests |
|---|---|
| **Smite** — 1E, deal 9, gain 1 Spirit (built once, pulled pending playtest) | does a plain better Strike make Act 1 feel right, or is it just filler? |
| **Hold the Line** — 1E, gain 8 Block | same question for defence |
| **Vengeful Mending** — 1E, deal 7, heal 3 | the heal-on-attack pattern, and Holy Fervor firing off an Attack |
| **Renew** — 1E Power, heal 2 at turn start | healing over time as a Power; whether a Power belongs this early |
| **Brace** — 1E, Block that survives the turn | whether persistent Block is worth a mechanic at all |

The art for all of these already exists in the repo — Dan painted the full set before the
reset — so adding one is a class, a localization entry and a build.
