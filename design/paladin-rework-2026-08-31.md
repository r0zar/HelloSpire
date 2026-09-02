# Paladin rework — aligned design (2026-08-31)

Product of a 20-question alignment session. This supersedes the seal/starter sections of
`paladin.md`; merge when ready. Numbers are placeholders — the editor tunes them later.

## The fantasy

**Discovering a build from cards that weren't supposed to work together.** Both kinds:
planted pairs (Barricade+Body-Slam-style designed surprises) and systemic emergence
(broad mechanics overlapping in unplanned ways). Everything below serves this.

**The anti-goal it replaces:** seals as no-brainer goodstuff ("take every one, swing every
turn"). The old model — unlimited simultaneous seals, Judgment fires all of them — was a
choice-free accumulation. Diagnosis: a *choice* problem, not power or variety.

## Always true, any build

**Paladins are tanky.** Starter relic (Holy Book retired): **start each combat with ~4
Plating** (real STS2 mechanic: Block equal to stacks at end of turn, before end-of-turn
damage, decaying 1/turn). Front-loads survival through the buff-early turns without
scaling into the late game. Both signature starter cards (Mend, Judgment) are also
retired; replacements TBD — the starter kit teaches tanky+buff-early, NOT seals.

## Seals & Judgment (opt-in package)

Classic-WoW model — the held seal is a *strategy signpost*:

- Seals are **Skills** (recastable), not Powers. **One held at a time**; casting a new
  seal (or a second copy) replaces/re-arms — **no stacking**.
- Judge cards have a real **base effect** (never dead without a seal) plus **Judge ×N**:
  fire the held seal's judge payoff N times, then the seal is **consumed**.
- No free seal from any relic. Seal-less Paladin decks must be fully viable.
- Build expression = deck ratio of **seals : judge cards : fishers** (draw/tutor cards
  that assemble the right play order). More judge commons than today; bigger payoffs.

Per-lane relationship with the same verb:
- **Ret**: small passives, big judge payouts, judges often (seal as ammo).
- **Prot**: one strong passive seal; its judge is an emergency button (e.g. mass
  Strength-down for a turn to save the team). May never judge by choice.
- **Holy**: engine seal that powers heals; possibly never judged. Innate-on-upgrade
  supports one-seal identity decks.

## Protection

Plating is the lane's scaler (drafted Plating growth) with two payoff dialects:
- **Brace/Blur persistence** — Block that survives into your turn, making Block-payoff
  cards honest despite Plating's end-of-turn timing.
- **Enemy-turn / on-being-attacked triggers** — retaliation, temp buffs/debuffs when hit.
  The clean fallback spine if mixing the two reads messy; uniquely Paladin and rides
  Plating's rhythm (its Block exists precisely during the enemy's turn).

## Holy

**Spirit stays** — the Dex-parallel that scales healing. The engine is **dual-mode
heals** (needs its own keyword — liturgical, not "Sly"; e.g. Offer/Tithe):

- Every heal card has a **discard face**: a small repeatable utility (Flash of Light:
  discard → Weak 2; faces can point at any lane — defense, smite, draw, Spirit tick).
  The card cycles back through reshuffles.
- The **true cast** is the once-per-copy, Spirit-scaled heal, which Exhausts.
- Finite heals = the lane's built-in clock: every real cast is a candle spent; when the
  last is gone the fight must be closing. No durdle-forever, no external enrage needed.
- Overheal-conversion and resurrection-from-exhaust are demoted to individual payoff /
  build-around cards, not the engine.
- Needs **discard enablers** as a support family (outlets beyond the cards' own faces).

## Cross-pollination rule (the discovery engine)

**Discard, seals, and Plating are languages, not lane property.** Every archetype's pool
contains some cards that meaningfully use the other lanes' mechanics as splash effects.
This is where discovered builds live.

## Validated against real A10 data (decompiled, `sts2-reskin-pipeline/docs/decompiled/game/`)

Ascension levels: 1 SwarmingElites … 8 ToughEnemies (+HP), 9 DeadlyEnemies (+dmg),
**10 DoubleBoss**. Final act (Glory, index 2) boss pool — at A10 you fight two:

| Boss | HP (A8) | Signature |
|---|---|---|
| Queen (+Torch Head Amalgam) | 419 | Perma Weak/Frail/Vuln **99**, Chains of Binding (card afflictions); after Amalgam dies: 4×5 multi → Execution 18 → +2 Str loop |
| Test Subject | 111→212→313 | Respawns twice; Bite 22, claws 11×3 (growing), Pounce 45, Burns into deck, ramping Str |
| Aeonglass | 535 | Artifact 3; Withering Presence 6 (first 6 plays each add a Wither; Withers +3 dmg every 3rd turn, forever); Ebb 32 + 33 self-block; lasers 12×2 |

Why the kit fits: judge damage and heal-conversion are **not Attacks** (Queen's Weak 99
doesn't shrink them); seals/Spirit persist across Test Subject's respawns; heals ignore
Artifact; dual-mode heals turn Aeonglass's guaranteed chip into engine fuel while its
Withering tax punishes exactly the card-spam the new design no longer needs.

## Open items

- Starter card replacements (two slots, teach tanky/buff-early).
- Keyword name for the heal discard-face mechanic.
- Migration: existing 6 seals redesigned to lane philosophy; Judgment card family
  redesigned to base-effect + Judge ×N; Holy Book replaced by the Plating relic.
- Tuning knobs flagged: judge payoff sizes, Plating relic amount, discard-face budgets,
  Spirit gain rates.
