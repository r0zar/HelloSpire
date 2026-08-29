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

## The deities

The Triad — three canonically allied lawful good deities of the Forgotten Realms — plus the
god you fall to.

### Torm the True — Protection
God of duty, loyalty, obedience, truth, and **of paladins specifically**.

Faith gained by **holding the line**: gaining Block, guarding allies, ending turns unbroken.
Payoffs are Block retention, damage reduction, and party-wide guarding.

```
Hold the Line     1 energy.  Gain 6 Block. Gain 2 Faith in Torm.
Bulwark           1 energy.  Gain Block equal to your Faith in Torm.
Unyielding        2 energy.  Requires 8 Faith in Torm.
                             Block is not removed at the start of your turn.
Aegis             1 energy.  Grant an ally Block equal to half your Faith in Torm.
```

### Ilmater the Broken God — Holy
God of suffering, endurance, and compassion. He bears others' pain.

Faith gained by **suffering**: losing HP, taking hits meant for allies. Payoffs are healing,
healing-over-time, and damage redirection. Bound and bloodied hands are his symbol.

```
Bear the Weight   1 energy.  Redirect the next attack on an ally to yourself.
                             Gain 3 Faith in Ilmater.
Endure            1 energy.  Power — whenever you lose HP, gain 1 Faith in Ilmater.
Broken Hands      2 energy.  Aura — at the start of your turn, all allies heal
                             equal to half your Faith in Ilmater.
Martyr's Grace    1 energy.  Requires 6 Faith in Ilmater.
                             Heal an ally to full. You take the HP they were missing.
```

Note the loop this creates with Torm: **taking damage feeds Ilmater**, so the tank plan and
the healer plan are the same plan. That is the mix-and-match working as intended, and it is
why healing is the character's core rather than a footnote. Delayed and over-time healing
also sidesteps the usual "heals are dead at full HP" problem entirely.

### Tyr the Maimed God — Retribution
God of justice. Blind, and missing the hand Kezef took.

Faith gained by **being wronged**: taking damage, allies taking damage. Payoffs are
proportional punishment — damage measured against what was done to you.

```
Blind Justice     1 energy.  Deal damage equal to the damage you took last turn.
                             Gain 2 Faith in Tyr.
Equal Measure     1 energy.  Deal damage to a random enemy equal to your Faith in Tyr.
The Scales        2 energy.  Requires 8 Faith in Tyr.
                             Deal damage equal to twice your Faith in Tyr.
Maimed            0 energy.  Lose 3 HP. Gain 4 Faith in Tyr.
```

### Bane — the fall
Torm's arch-enemy. During the Time of Troubles they killed each other; Ao resurrected Torm
and Tyr raised him to greater power.

**Bane Faith cannot be gained directly.** It only accrues when you *spend* Faith in a Triad
deity. Falling is the only way in.

```
Falter            0 energy.  Spend 5 Faith in any deity. Gain 3 Faith in Bane.
                             Draw 2 cards. Gain 1 energy.
The Black Hand    2 energy.  Requires 6 Faith in Bane.
                             Deal heavy damage to ALL enemies.
Time of Troubles  3 energy.  Spend ALL your Faith in Torm.
                             Deal double that as damage to a single enemy.
                             Gain that much Faith in Bane.
Tyranny           1 energy.  Requires 10 Faith in Bane.
                             Your Faith in Bane counts as Faith in every deity.
```

Bane Faith should **suppress the Triad** while held — the Triad turns away from you — so
falling is a genuine commitment within the fight, not free value. `Tyranny` is the payoff
for going all the way: at the bottom, everything unlocks again.

Because Faith resets each combat, falling is a **tactical, per-fight decision**, not a run
sentence. "I'm ten into Torm but this fight needs damage right now" is exactly the choice
this is for.

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
- **Is Faith visible per-deity in the UI, and how?** Four tracks is a lot of screen. May
  need only the highest one or two shown prominently.
- **Solo viability.** Ilmater and Torm both lean on allies existing. Ally-targeted effects
  need a defined self-target fallback for single-player.

## Multiplayer note

Faith is custom state and must sync across clients. `Bear the Weight`-style redirection and
party-wide Auras are the highest-risk pieces. Test with two players early, per TODO.md
Phase 9 — a desync here is much harder to debug once there are 80 cards.
