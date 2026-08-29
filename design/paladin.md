# The Paladin — design

Status: **design agreed, not implemented.** No cards exist yet.

## Core mechanic: Faith

Faith is a **standing value, not a currency**. It is gained, checked, and scaled off. It is
normally never spent — but it *can* be, and that is the whole oathbreaker theme.

Faith is held **per deity**, not in one pool. This is the part nothing else in the game does:

| | Shape |
|---|---|
| Regent's Stars | one pool, spent as a cost |
| Defect's Orbs | an ordered queue of objects |
| Necrobinder's Osty | a second body |
| **Paladin's Faith** | **a portfolio split across deities** |

Thresholds are **per deity**, so spreading your investment reaches nothing. The pressure to
concentrate is structural, before any explicit penalty exists.

**Faith resets every combat.** A run-long accumulating number would get out of hand and
trivialize Act 3. Each fight is its own arc of devotion.

### Why this shape works

- **The ramp is the fight's arc.** Cheap low-threshold cards early, an 8-Faith payoff late.
  Tension curve for free.
- **Card rewards get hard.** Twelve Faith into Torm and you're offered a great Tyr card.
  Taking it costs something real.
- **Splitting is a build, not just a mistake.** Wide decks pay a tax for breadth. That's the
  Silent-style mix-and-match: concentration is the default good play, breadth is the clever
  alternative for a deck built to support it.

## The three verbs

Faith generation is deliberately orthogonal. Each deity keys off one action, and every deck
already does all three to some degree — so any deck generates some Faith without trying.
Concentration means choosing which verb you do *most*.

| Deity | Trigger | Verb | Archetype |
|---|---|---|---|
| **Torm the True** | gain Block | defend | Protection |
| **Ilmater the Broken God** | heal | mend | Holy |
| **Tyr the Maimed God** | deal damage | attack | Retribution |

All three are **actions you take**, not things done to you. An earlier draft had Ilmater key
off losing HP; that was worse, because it was enemy-driven and it overlapped with Tyr.

Two consequences of Ilmater keying off healing, both load-bearing:

- **Heals are never dead cards.** Healing at full HP still generates Faith, so the card always
  did something. No special overheal rule is needed — the mechanic handles it natively.
- **Healing an ally generates Faith.** The co-op hook is the engine rather than a bolt-on. A
  Paladin in a party has more healing targets, so more Faith, with no "in multiplayer..."
  clause anywhere in the card text.

## Deities are flavor, not card types

**A deity does not restrict what a card can do.** All three have attacks, heals and Block.
Tyr can heal through an attack. Ilmater can smite. Torm can carry thorns. The deity is a
flavor lens and a Faith source, nothing more.

This is deliberate, and it creates the central deckbuilding axis for free:

- **Single-verb cards** generate one deity's Faith efficiently — they build **tall**.
- **Hybrid cards** trigger two Oaths at once, at a worse rate per deity — they build **wide**,
  and wide decks are the ones that want `Heresy`.

```
Holy Smite        1E  Deal 6 damage. Heal an ally 3.
                      -> Vengeance AND Redemption
Holy Shield       1E  Gain 5 Block. Until end of turn, when you are attacked,
                      deal 3 damage back.
                      -> Crown; thorns are Tyr flavor on a Torm card
Guardian's Mercy  1E  Gain 4 Block. Heal an ally 3.
                      -> Crown AND Redemption
Vengeful Mending  1E  Deal 8 damage. Heal yourself 4.
                      -> Vengeance AND Redemption
Retribution       1E  Deal damage equal to the Block you gained this turn.
                      -> a Tyr payoff that only pays off in a Torm deck
```

Balance lever: hybrids are slightly under-rate on each half. You pay for flexibility.

### Two sources of Faith

Hybrids make the distinction matter:

- **Printed** — the card text says `Gain 1 Faith in Tyr`.
- **Oath-triggered** — a Power fires based on what the card *did*.

`Holy Smite` printed with Tyr Faith, played under Oath of Redemption, yields Tyr 1 from the
print and Ilmater 1 from the Oath.

## Generation rules

These are hard constraints, not guidelines. They exist so a Faith number means the same thing
in every deck.

1. **Generation is always flat.** `Gain 2 Faith.` Never proportional, never scaled off another
   value. Proportional generation is much harder to balance and compounds with everything.
2. **Triggers must be countable.** `Whenever you play an Attack`, not `whenever you deal
   damage` — a multi-hit or AoE card would fire the latter five times off one play. Same for
   Torm: key off playing a card that gains Block, not off each Block instance.
3. **Multipliers are Rare only, additive, and capped.** `Gain 1 additional`, never `gain
   double`. No card multiplies another multiplier. Two or three in the entire set.

Scaling belongs on the **payoff** side, where it is visible and bounded by how much Faith you
actually managed to accumulate.

## Cards

### Core generation — flat, countable, boring on purpose

```
Hold the Line   1E  Gain 6 Block.    Gain 1 Faith in Torm.
Mend            1E  Heal an ally 4.  Gain 1 Faith in Ilmater.
Smite           1E  Deal 7 damage.   Gain 1 Faith in Tyr.
Kneel           0E  Name a deity. Gain 2 Faith in it.
```

### Oaths — make a verb passive. Rare.

```
Oath of the Crown     2E  Power. Whenever you play a card that gains Block,
                          gain 1 Faith in Torm.
Oath of Redemption    1E  Power. Whenever you play a card that heals,
                          gain 1 Faith in Ilmater.
Oath of Vengeance     2E  Power. Whenever you play an Attack,
                          gain 1 Faith in Tyr.
```

The 5e oath names — Devotion, Vengeance, the Crown, Redemption, Glory, the Ancients, the
Watchers, Conquest — are the naming pool for these.

### Payoffs — scale off Faith, never touch generation

```
Immovable       2E  Power. At the start of your turn, gain Block
                    equal to half your Faith in Torm.
The Broken God  2E  Power. At the start of your turn, all allies heal
                    equal to half your Faith in Ilmater.
The Scales      2E  Deal damage equal to twice your Faith in Tyr.
```

### Supporting — the whole quarantine, and it is short

```
Heresy          3E  Power. Your Faith in every deity counts as your highest.
Zealotry        3E  Power. Whenever you gain Faith, gain 1 additional.   <- the multiplier
Tithe           1E  Spend 3 Faith. Gain 2 Energy.
Sacrament       1E  Convert all Faith in one deity to another.
```

`Heresy` is the most important card in the set: it turns "splitting is a mistake" into
"splitting is a deck", delivering archetype-mixing through one card rather than a subsystem.
It is a rule-change, not a multiplier, which is why it can be strong without compounding.

`Zealotry` is the only multiplier in the set.

### Bane — the fall
Torm's arch-enemy. During the Time of Troubles they killed each other; Ao resurrected Torm
and Tyr raised him to greater power.

**Bane Faith cannot be gained directly.** It only accrues when you *spend* Faith in a Triad
deity. Falling is the only way in.

```
Falter            0E  Spend 5 Faith in any deity. Gain 3 Faith in Bane.
                      Draw 2 cards. Gain 1 Energy.
The Black Hand    2E  Requires 6 Faith in Bane.
                      Deal heavy damage to ALL enemies.
Time of Troubles  3E  Spend ALL Faith in Torm. Deal double that damage
                      to one enemy. Gain that much Faith in Bane.
Tyranny           2E  Requires 10 Faith in Bane.
                      Your Faith in Bane counts as Faith in every deity.
```

Bane Faith should **suppress the Triad** while held — the Triad turns away from you — so
falling is a genuine commitment within the fight, not free value. `Tyranny` is the payoff
for going all the way: at the bottom, everything unlocks again.

Because Faith resets each combat, falling is a **tactical, per-fight decision**, not a run
sentence. "I'm ten into Torm but this fight needs damage right now" is exactly the choice
this is for.

## Starting relic: Holy Symbol

**Holy Symbol** — *While held, your first Faith gain each combat is doubled. When your total
Faith in one deity across the run reaches N, the Holy Symbol is consumed and you receive that
deity's relic.*

This follows three base-game patterns at once: **Touch of Orobas** (replaces your starter
relic with an upgraded one), **Sword of Stone** (transforms after a milestone), and
**Pael's Wing** (sacrifice to earn a relic). All three ship, so the shape is proven.

Why this and not "name a deity at combat start":

- **You commit by playing, not by picking.** The relic watches what you actually do and
  rewards it — which is how a paladin's god works. No per-fight choosing.
- **Devotion is a run-long arc without Faith persisting.** Faith still resets each combat;
  only the *milestone* accumulates. Every reward screen tempts you off the path before you've
  earned the relic, which is the pressure we want.
- **Splitting costs something real.** A wide deck never triggers the transform and keeps the
  weaker starter relic all run — until `Heresy`, which counts everything as your highest and
  pops it immediately. That is the wide build's payoff line.

Rules:

- One milestone number, the same for every deity. No god is easier to earn.
- `Sacrament` counts toward the **destination** deity, so "pulled away, then repented" is a
  playable story.
- N is a placeholder until playtesting; it should land somewhere around the end of Act 1 for
  a focused deck.

### The three deity relics

Placeholders. Each should be a stronger, deity-flavoured version of what the Holy Symbol did,
not an unrelated effect.

```
Torm      Gauntlet of the True      Block is not fully removed at the start of your turn;
                                    keep half.
Ilmater   Bound Hands               At the start of each combat, all allies heal 4.
Tyr       Scales of Judgment        At the start of each combat, gain 3 Faith in Tyr.
                                    Whenever you take damage, gain 1 Faith in Tyr.
```

## Supporting card categories

These are card *types*, not mechanics competing with Faith:

| Category | Behaviour |
|---|---|
| **Auras** | Powers, party-wide, usually scaling off Faith |
| **Seals** | self-buffs that expire after N turns; high thresholds gate the strong ones |
| **Blessings** | ally-targeted buffs, usually Ilmater or Torm |
| **Oaths** | rare, high-threshold payoffs. The 5e oath names (Devotion, Vengeance, the Crown, Redemption, Glory, the Ancients, the Watchers, Conquest) are the natural naming pool. |

## Open questions

- **Faith gain rate and threshold values.** Needs the damage-per-energy benchmark from
  TODO.md Phase 8 before any numbers here are meaningful.
- **Does Bane suppress the Triad entirely, or scale it down?** Hard lockout is cleaner to
  reason about; scaling is more forgiving.
- **Faith UI.** Three tracks plus Bane. Should fit; verify once the counter exists.
- **Solo viability.** Ilmater and Torm both lean on allies existing. Ally-targeted effects
  need a defined self-target fallback for single-player.

## Multiplayer note

Faith is custom state and must sync across clients. `Bear the Weight`-style redirection and
party-wide Auras are the highest-risk pieces. Test with two players early, per TODO.md
Phase 9 — a desync here is much harder to debug once there are 80 cards.
