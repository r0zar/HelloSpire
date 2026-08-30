# The Paladin — full card set

**91 cards**: 4 basic, 19 common, 38 uncommon, 29 rare. Base-game parity is 4 / 20 / 36 / 26 (87); this draft runs three rares over, which is a fine place to be before playtesting cuts anything.

Adapted from WoW and D&D 5e paladin kits; sources noted where a card is a direct lift.
**All numbers are placeholders** until the damage-per-energy benchmark exists.

House rules every card obeys:

- **Flat, never percentage.** `deal 5 additional`, not `+50%`.
- **Countable triggers.** `whenever you play an Attack`, never `whenever you deal damage`.
- **Generation is flat.** No card generates Faith proportionally to anything.
- **Multipliers are capped at two**, both Rare: `Zealotry`, `Avenging Wrath`.
- **Deities do not gate card types.** Every deity attacks, heals, and blocks.

Notation: `[T]` `[I]` `[Y]` `[B]` = Torm / Ilmater / Tyr / Bane. `[T+Y]` = hybrid. `` = multiplayer-only.

## Multiplayer-only cards

The game has a first-class flag, `CardMultiplayerConstraint.MultiplayerOnly`, enforced at the
source: `CardPoolModel.GetUnlockedCards` filters it out, so a flagged card **never appears as a
reward in a solo run**. `TargetType.AnyAlly` explicitly excludes the owner, and every shipped
`AnyAlly` card is flagged.

`TargetType.AnyPlayer` is the other half: it accepts **any living player including you**. Solo,
such a card has exactly one legal target and just works; in co-op it is a real choice. So:

- **"a player"** cards are `AnyPlayer`, work solo, and are stronger with a party. Most of the
  Paladin's heals and grants are written this way.
- **`[MP]`** is reserved for the six cards that are meaningless alone -- redirects, a chosen
  ally, a revive. That matches the base game's 4-5 per character.
- **"all allies"** cards include you and are not flagged.

## Basic (4)

Every deck starts with these.

```
Strike               1E  [-]  Deal 6 damage.
Defend               1E  [-]  Gain 5 Block.
Mend                 1E  [-]  Heal 6.
Hammer of Justice    2E  [-]  Deal 10 damage. Stun the enemy.
```

**No starter names a deity, and none generates Faith.** The starter must not push a player
toward any god — the first Faith decision is the first card reward, not turn one. It also
means Faith comes only from cards the player chose, so the mechanic never accumulates
passively off cards you cannot avoid drawing. No starter has Exhaust.

`Mend` is a true Defend mirror: same cost, same number, HP instead of Block.

`Hammer of Justice` is the signature starter: a 2-cost tempo tool that buys a turn, which is
how a defensive character with no Faith yet survives Act 1. (Aura of Protection was the
original pick, but it is ally-targeting, and the game treats every ally-targeting card as
multiplayer-only content. A starter must work solo.)

Starter deck: 4 Strike, 4 Defend, 1 Mend, 1 Hammer of Justice — ten cards.

---

## Common (20)

The backbone. Playable, unexciting, seen constantly. **No common generates Faith.** Faith is
scarce by design: commons are pure gameplay, a handful of uncommons grant 1, and the real
engines are the Oaths and rares. A deck accumulates Faith because the player took specific
reward cards, never because the cards they see every fight happen to carry it.

### Torm (7)

```
Hold the Line      1E  [T]    Gain 8 Block.
Shield Bash        1E  [T+Y]  Deal 5 damage. Gain 3 Block.
Brace              1E  [T]    Gain 6 Warded.
Interpose          1E  [T]    Gain 4 Block. A player gains 4 Block.
Aura of Protection 1E  [T]    Power. At the start of your turn, all allies gain 2 Block.
Steadfast          0E  [T]    Gain 3 Block. Draw 1 card.
Shield Wall        2E  [T]    Gain 11 Warded.
```

### Ilmater (6)

```
Soothe             1E  [I]    Heal a player 6.
Flash of Light     1E  [I]    Heal a player 5. Blessed.                              [WoW]
Holy Shock         1E  [I+Y]  Deal 5 damage OR heal 5.                     [WoW]
Salve              0E  [I]    Heal yourself 3. Blessed.
Renew              1E  [I]    Power. At the start of your turn, heal 2.
Comfort            1E  [I]    Heal a player 4. Draw 1 card. Blessed.
```

### Tyr (6)

```
Smite              1E  [Y]    Deal 9 damage.
Crusader Strike    1E  [Y]    Deal 5 damage 2 times. Apply 1 Judged.       [WoW]
Cleave             1E  [Y]    Deal 5 damage to ALL enemies.
Righteous Blow     2E  [Y]    Deal 14 damage.
Rebuke             1E  [Y]    Deal 4 damage. Apply 1 Weak.
Vengeful Mending   1E  [Y+I]  Deal 7 damage. Heal yourself 3.
```

### Neutral (1)

```
Prayer             1E  [-]    Draw 2 cards. Blessed.
```

---

## Uncommon (38)

The largest tier, and where the archetypes actually take shape. Hybrids concentrate here —
this is the tier that decides tall vs wide.

### Torm (10)

```
Bulwark                1E  [T]    Gain Block equal to your Faith in Torm.
Holy Shield            1E  [T+Y]  Gain 5 Block. Until end of turn, whenever you
                                  are attacked, deal 3 damage to the attacker.
Guardian's Mercy       1E  [T+I]  Gain 4 Block. Heal a player 3.
Blessing of Protection 1E  [T]    A player gains 8 Block.                        [WoW]
Consecrated Ground     2E  [T]    Power. Whenever you play a card that gains
                                  Block, all allies gain 1 Block.
Bastion                2E  [T]    Gain 8 Block and 8 Warded.
Retribution            1E  [T+Y]  Deal damage equal to the Block you gained
                                  this turn.
Shield Slam            1E  [T+Y]  Deal damage equal to your Block.              [WoW]
Sentinel               1E  [T]    Power. Whenever an ally is attacked, gain  [MP]
                                  2 Block.
Immovable Object       2E  [T]    Requires 3 Faith in Torm. Gain 15 Warded.
```

### Ilmater (10)

```
Blessing of Sacrifice  1E  [I]    Power. Damage an ally would take is dealt   [WoW]  [MP]
                                  to you instead, halved.
Aura of Mercy          2E  [I]    Power. Aura — whenever you heal an ally,  [MP]
                                  every other ally heals 1.
Absolve                1E  [I]    Heal a player 5. Remove one debuff from them.
Holy Light             2E  [I]    Heal a player 12.                            [WoW]
Bind the Wounds        1E  [I]    Heal a player 4. Gain 1 Faith in Ilmater. Blessed.
Prayer of Mending      1E  [I]    Heal a player 4. At the start of your next
                                  turn, heal them 4 again.
Martyr                 1E  [I]    Lose 4 HP. Heal a player 8.
Circle of Healing      2E  [I]    Heal all allies 4.
Faithful Servant       2E  [I]    Requires 3 Faith in Ilmater. Heal a player
                                  equal to your Faith in Ilmater.
Sanctuary              1E  [I+T]  Heal a player 3. Grant them 5 Block. Blessed.
```

### Tyr (10)

```
Holy Smite             1E  [Y+I]  Deal 6 damage. Heal a player 3. Blessed.
Blade of Justice       1E  [Y]    Deal 9 damage. Apply 1 Judged.               [WoW]
                                  Gain 1 Faith in Tyr.
Judgment               1E  [Y]    Deal 5 damage. Apply 2 Judged.               [WoW]
Exorcism               1E  [Y]    Deal 12 damage. Exhaust.                     [WoW]
Wake of Ashes          2E  [Y]    Deal 7 damage to ALL enemies.                [WoW]
                                  Gain 1 Faith in Tyr.
Zeal                   1E  [Y]    Deal 4 damage 2 times.
Equal Measure          1E  [Y]    Deal damage equal to your Faith in Tyr.
Consecration           2E  [Y]    Power. At the start of your turn, deal 3    [WoW, adapted]
                                  damage to ALL enemies.
Crusader Aura          2E  [Y]    Power. Aura — all allies' Attacks deal      [WoW]
                                  1 additional damage.
Divine Purpose         1E  [Y]    Requires 3 Faith in Tyr. Deal 10 damage.    [WoW]
                                  Gain 1 Energy.
```

### Cross-deity and utility (8)

```
Sacrament              1E  [-]    Convert all Faith in one deity to another.
Kneel                  0E  [-]    Name a deity. Gain 1 Faith in it. Exhaust.
Devotion               1E  [-]    Gain 1 Faith in your highest deity.
Tithe                  1E  [B]    Spend 3 Faith. Gain 2 Energy.
Litany                 1E  [-]    Draw 1 card for every 4 Faith in your Blessed.
                                  highest deity.
Seal of Command        1E  [-]    Seal — for 3 turns, your Attacks deal       [WoW]
                                  2 additional damage.
Seal of Light          1E  [-]    Seal — for 3 turns, whenever you play an    [WoW]
                                  Attack, heal 2.
Hallowed Ground        2E  [-]    Power. Whenever you gain Faith in your
                                  highest deity, gain 1 Block.
```

---

## Rare (29)

The tier that carries the character's identity. Adapted per-card; see sources.

### Torm (7)

```
Shield of the Righteous   1E  [T]    Gain Block equal to twice your Faith       [WoW]
                                     in Torm.
Ardent Defender           2E  [T]    Power. The first time you would die this   [WoW]
                                     combat, instead heal to a third of max HP.
Divine Allegiance         1E  [T]    Power. Whenever an ally would take damage, [D&D Crown]  [MP]
                                     you may take it instead.
Aura of Devotion          2E  [T]    Power. Aura — all allies take 1 less       [D&D core]
                                     damage from attacks for every 5 Faith
                                     in Torm.
Avenger's Shield          1E  [T+Y]  Deal 8 damage to up to 3 enemies.          [WoW]
                                     Gain 6 Block.
Guardian of Ancient Kings 3E  [T]    Requires 5 Faith in Torm. For 3 turns,    [WoW]
                                     all damage you take is halved.
Divine Shield             2E  [T]    Requires 4 Faith in Torm. You take no      [WoW]
                                     damage next turn. Exhaust.
```

### Ilmater (7)

```
Lay on Hands              2E  [I]    Heal a player to full. Exhaust.             [WoW + D&D]
Beacon of Light           2E  [I]    Power. Choose an ally. Whenever you heal,  [WoW]  [MP]
                                     they also heal half that much.
Word of Glory             1E  [I]    Spend 3 Faith in Ilmater. Heal a player 12. Blessed. [WoW]
Aura of Vitality          2E  [I]    Power. Aura — at the start of your turn,   [D&D]
                                     all allies heal 3.
Light of Dawn             2E  [I]    Heal all allies equal to half your Faith Blessed.   [WoW]
                                     in Ilmater.
Redemption                3E  [I]    Requires 5 Faith in Ilmater. Revive a     [D&D Revivify]  [MP]
                                     downed ally at 1 HP. Exhaust.
The Broken God            2E  [I]    Power. At the start of your turn, all
                                     allies heal equal to half your Faith
                                     in Ilmater.
```

### Tyr (7)

```
Divine Smite              1E  [Y]    Deal 8 damage. If you have 3 or more       [D&D, iconic]
                                     Faith in any deity, deal 8 additional.
Hammer of Wrath           1E  [Y]    Deal 10 damage. If the enemy is below      [WoW]
                                     half HP, deal 10 additional and gain
                                     2 Faith in Tyr.
Vow of Enmity             1E  [Y]    Power. Choose an enemy. Your Attacks       [D&D Vengeance]
                                     against it deal 4 additional damage.
Eye for an Eye            1E  [Y]    Power. Whenever you take damage, deal      [WoW]
                                     that much to the attacker.
Divine Storm              2E  [Y+I]  Deal 10 damage to ALL enemies. Heal 3     [WoW]
                                     for each enemy hit.
Final Reckoning           3E  [Y]    Deal damage to ALL enemies equal to your   [WoW]
                                     total Faith across every deity.
The Scales                2E  [Y]    Deal damage equal to twice your Faith
                                     in Tyr.
```

### Oaths (3)

Rare because they make a verb passive — the strongest possible engine.

```
Oath of the Crown         2E  [T]    Power. Whenever you play a card that
                                     gains Block, gain 1 Faith in Torm.
Oath of Redemption        1E  [I]    Power. Whenever you play a card that
                                     heals, gain 1 Faith in Ilmater.
Oath of Vengeance         2E  [Y]    Power. Whenever you play an Attack,
                                     gain 1 Faith in Tyr.
```

### Bane and cross-deity (5)

```
Heresy                    3E  [-]    Power. Your Faith in every deity counts
                                     as your highest.
Zealotry                  3E  [-]    Power. Whenever you gain Faith, gain
                                     1 additional.                 <- multiplier 1/2
Avenging Wrath            2E  [Y]    Requires 4 Faith in Tyr. Power. For 3     [WoW]
                                     turns, Attacks deal 5 additional and
                                     heals restore 5 additional.    <- multiplier 2/2
Time of Troubles          3E  [B]    Spend ALL Faith in Torm. Deal double that
                                     damage to one enemy. Gain that much Faith
                                     in Bane.
Tyranny                   2E  [B]    Requires 5 Faith in Bane. Your Faith in
                                     Bane counts as Faith in every deity.
```

---

## Tallies

| Tier | Torm | Ilmater | Tyr | Cross / Bane | Total |
|---|---:|---:|---:|---:|---:|
| Basic | 0 | 0 | 0 | 4 | 4 |
| Common | 6 | 6 | 6 | 1 | 19 |
| Uncommon | 10 | 10 | 10 | 8 | 38 |
| Rare | 7 | 7 | 7 | 8 | 29 |
| **Total** | **23** | **23** | **23** | **22** | **91** |

Rare cross/Bane breaks down as 3 Oaths + 5 Bane/cross-deity. Three rares over base-game
parity (26); candidates to cut after playtesting are `The Scales` (plain scaler) and whichever
Oath proves weakest.

Hybrids: 12 across the set, concentrated at uncommon. Faith-threshold cards: 8. Faith-spend
cards: 3 (`Word of Glory`, `Tithe`, `Time of Troubles`) — the entire fall surface.

## Deliberately not adapted

| Ability | Why not |
|---|---|
| Consecration (ground AoE) | no positional layer; adapted as a start-of-turn AoE Power instead |
| Hammer of Justice | stun is a base-game keyword; nothing new |
| Divine Steed, Divine Sense | no hook in a deckbuilder |
| Holy Power | this *is* Faith |
| Crusader Strike / Templar's Verdict as builder/spender | that is the generation/payoff split, already the whole design |

## Open

- **Every number.** Placeholders until the benchmark exists.
- **Does Holy spending accrue Bane?** By the falling rule `Word of Glory` should. Thematically
  odd, mechanically interesting. Playtest before calling it a bug.
- **Ally-target fallback in solo.** ~20 cards target an ally. They need a defined self-target
  behaviour for single-player or the character is unplayable alone.
- **Revive.** `Redemption` assumes a downed-ally state exists in multiplayer. Verify before
  building around it.

## Deviations in code

`tools/gen_paladin.py` is the source of truth for what shipped. Where it differs from the text
above, it is because the game has no clean primitive for the original:

| Card | Text says | Code does | Why |
|---|---|---|---|
| Holy Shock | Deal 5 OR heal 5 | Deal 5 and heal 3 | no play-time choice UI |
| Retribution | damage = Block gained this turn | damage = your Block, to ALL enemies | no per-turn Block tracking; Shield Slam already covers single-target |
| Sacrament | convert one deity to another | consolidate everything into your highest | no deity-choice UI |
| Kneel | name a deity | +1 to your lowest deity | no deity-choice UI; feeding the lowest is the wide build's tool |
| Tithe | spend 3 Faith | spend 3 from your highest deity | no deity-choice UI |
| Redemption | revive a downed ally | revive all downed allies, self-targeted | dead creatures cannot be targeted |
| Divine Allegiance / Blessing of Sacrifice | you may take it / halved | always redirect, full amount | no opt-in prompt; halving needs a second hook |
| Divine Shield | take no damage | Intangible 1 (damage reduced to 1) | closest shipped power |
| Avenging Wrath | +5 attacks and +5 heals | +5 attacks only | no heal-modification hook |
| Eye for an Eye | that much damage | the pre-Block amount | hook fires before Block |
| Holy Smite | heal an ally 3 | heal yourself 3 | one target per card |
| spending Faith | accrues Bane (unspecified) | Bane gains half of what was spent, rounded up | first concrete rule; tune in play |
