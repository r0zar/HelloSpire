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
| Starter relic | **Holy Book** — *At the start of each combat, gain Seal of Light.* |

Holy Fervor (heal a card, gain 1 Strength, once per turn) moves to the relic pool: now that
heals cost Spirit, it pays you for spending it.

### Starter deck — ten cards

```
4x Strike             1E  Deal 6 damage.
4x Defend             1E  Gain 5 Block.
1x Mend               1E  Heal 5 HP plus your Spirit. Exhaust.
1x Judgment           1E  Trigger your Seal's effect. Unplayable without a Seal.
```

The deck reads as *durable but slow to kill*, which is the D&D and WoW read of the class. Mend
is a Defend mirror in HP. Judgment is a blank card that the active Seal fills in -- it teaches the Seal loop from turn
one. Hammer of Justice moved to the Rare pool: a bought turn is too strong to see every run.

No starter card carries any mechanic beyond the base game's. That is deliberate: the starter
must be understood on sight.

### Multiplayer identity

The Paladin is the co-op class: the target is **as many MultiplayerOnly cards as solo cards**,
so in team games roughly half of every reward roll is party support (rewards pick uniformly
within the rolled rarity, so pool share IS reward share; solo runs filter MP cards out
entirely and never see a dead card).

The category rules:

- **Heals that target `AnyPlayer` are MultiplayerOnly.** Self-heals are unrestricted -- Mend
  and Seal of Light are self-heals and appear everywhere. Ally-targeting heals (Holy Light,
  Flash of Light, and the co-op batch to come) only exist where allies do.
- **Auras are MultiplayerOnly Powers** -- ongoing party-wide effects (none built yet).
- **Blessings are MultiplayerOnly Skills** -- single-shot ally buffs (none built yet).
- All heals route through `Spirit.Heal(healer, target, amount)`: the *caster's* Spirit boosts
  the heal regardless of who receives it, so the party healer scales the whole party.

### Seals and Judgment

Seals are ordinary buff powers with one extra face: a **Judgment effect**. Stack as many as you
draft -- there is no slot limit and no consumption -- and **Judgment triggers the effects of
ALL your Seals**. Seal count is the scaling axis; Judgment is the payoff button.

The balance rule: **each Seal is deliberately small**, priced so that having several is normal
rather than degenerate. A one-seal Judgment is weak; a four-seal Judgment is a strong turn you
built toward. Replaying a Seal stacks its Amount.

| Seal | Rarity | Passive | Judgment |
|---|---|---|---|
| Light (starter) | -- | Attacks heal 1 | heal 3 + Spirit |
| Righteousness | Uncommon | Attacks +2 | deal 5 |
| Command | Uncommon | first Attack/turn: 1 Vulnerable | 2 Vulnerable |
| Justice | Uncommon | first Attack/turn: 1 Weak | 2 Weak |
| Wisdom | Uncommon | first Attack/turn: draw 1 | draw 1 |
| Martyr | Rare | enemies that hit you take 3 | 3 to ALL enemies |

Triggers: Judgment (1E, unplayable without a Seal; upgrade 0E), Exorcism (attack), Divine
Purpose (cantrip), Shield of the Righteous (block). Avenging Wrath (Rare power) makes every
trigger fire twice. All of it funnels through `Seals.Judge`, so new Seals and new trigger
cards compose automatically.

### Spirit, the third stat (was: Faith)

Strength raises attacks, Dexterity raises Block, **Spirit raises healing** -- one icon in the
power bar next to Strength, reset per combat like the others. The **Holy Symbol** (starter
Fed by **Holy Symbol** (Common relic, 1 Spirit per combat) and **Devotion** (Common skill,
flat 2 Spirit -- a per-turn engine version compounded too hard). Draft in and heals scale.

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

Eight Commons, two Uncommons and a Rare, added as a playtest batch (2026-08-30): enough of a pool that Act 1
drafts are real. See `paladin-cards.md` for the live table. Uncommons and further Rares wait on
how these play.

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
