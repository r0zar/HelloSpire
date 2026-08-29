# The Paladin — rare tier

Target: **26 rares** (measured base-game parity). This is 25 drafted.

Adapted from WoW and D&D 5e paladin kits. Source noted per card. **All numbers are
placeholders** — they are unsettable until the damage-per-energy benchmark from TODO.md
Phase 8 exists.

House rules these obey:

- **Flat, never percentage.** `deal 5 additional damage`, not `+50% damage`. Percentages
  compound and are much harder to balance.
- **Countable triggers.** `whenever you play an Attack`, not `whenever you deal damage`.
- **Multipliers capped.** Only `Zealotry` and `Avenging Wrath` scale another value. That is
  the whole allowance.
- **Deities do not gate card types.** Torm cards attack, Tyr cards heal, Ilmater cards smite.

---

## Torm — Protection (7)

```
Shield of the Righteous   1E  Gain Block equal to your Faith in Torm.            [WoW]

Ardent Defender           2E  Power. The first time you would die this combat,   [WoW]
                              instead heal to a third of your max HP.

Divine Allegiance         1E  Power. Whenever an ally would take damage,         [D&D Crown]
                              you may take it instead.

Aura of Protection        2E  Power. Aura — all allies take 1 less damage from   [D&D core]
                              attacks for every 5 Faith in Torm.

Avenger's Shield          1E  Deal 8 damage to up to 3 enemies. Gain 6 Block.    [WoW]

Guardian of Ancient Kings 3E  Requires 10 Faith in Torm.                         [WoW]
                              For 3 turns, all damage you take is halved.

Divine Shield             2E  Requires 8 Faith in Torm.                          [WoW]
                              You take no damage next turn. Exhaust.
```

`Avenger's Shield` is the type-freedom example — an Attack that is unmistakably a Torm card.

## Ilmater — Holy (7)

```
Lay on Hands              2E  Heal an ally to full. Exhaust.                     [WoW + D&D]

Beacon of Light           2E  Power. Choose an ally. Whenever you heal,          [WoW]
                              they also heal half that much.

Holy Shock                1E  Deal 5 damage OR heal 5. Gain 1 Faith in Ilmater.  [WoW]

Word of Glory             1E  Spend 3 Faith in Ilmater. Heal an ally 12.         [WoW]

Aura of Vitality          2E  Power. Aura — at the start of your turn,           [D&D]
                              all allies heal 3.

Light of Dawn             2E  Heal all allies equal to half your Faith           [WoW]
                              in Ilmater.

Redemption                3E  Requires 12 Faith in Ilmater.                      [D&D Revivify]
                              Revive a downed ally at 1 HP. Exhaust.
```

`Redemption` is the strongest argument for the whole character existing in co-op. Nothing in
the base game does it.

`Word of Glory` is the clean example of **spending as a choice** — it costs you Faith and
therefore progress toward your thresholds, and by the falling rule it accrues Bane.

## Tyr — Retribution (7)

```
Divine Smite              1E  Deal 8 damage. If you have 5 or more Faith         [D&D, iconic]
                              in any deity, deal 8 additional damage.

Hammer of Wrath           1E  Deal 10 damage. If the enemy is below half HP,     [WoW]
                              deal 10 additional and gain 2 Faith in Tyr.

Vow of Enmity             1E  Power. Choose an enemy. Your Attacks against it    [D&D Vengeance]
                              deal 4 additional damage.

Eye for an Eye            1E  Power. Whenever you take damage, deal that much    [WoW]
                              to the attacker.

Judgment                  1E  Deal 5 damage. The enemy takes 4 additional        [WoW + D&D]
                              damage from all sources this turn.

Divine Storm              2E  Deal 10 damage to ALL enemies.                     [WoW]
                              Heal 3 for each enemy hit.

Final Reckoning           3E  Deal damage to ALL enemies equal to your total     [WoW]
                              Faith across every deity.
```

`Divine Storm` is a Tyr card that heals — it triggers Redemption as well as Vengeance.
`Final Reckoning` is the one card that explicitly rewards going **wide**, counting Faith
across all deities rather than one.

## Bane and cross-deity (4)

```
Heresy                    3E  Power. Your Faith in every deity counts as
                              your highest Faith.

Zealotry                  3E  Power. Whenever you gain Faith, gain 1 additional.
                              <- multiplier 1 of 2

Avenging Wrath            2E  Requires 8 Faith in Tyr. Power. For 3 turns,       [WoW]
                              your Attacks deal 5 additional damage and your
                              heals restore 5 additional HP.
                              <- multiplier 2 of 2

Time of Troubles          3E  Spend ALL Faith in Torm. Deal double that damage
                              to one enemy. Gain that much Faith in Bane.
```

Further Bane rares to draft when the fall mechanic is prototyped — `Dreadful Aspect` and
`Aura of Hate` from the 5e Oathbreaker are the obvious next two, along with `Seraphim` from
WoW as a temporary all-deity Faith spike.

---

## Deliberately not adapted

Recorded so they are not re-proposed:

| Ability | Why not |
|---|---|
| Consecration | ground-targeted AoE; StS2 has no positional layer |
| Hammer of Justice | stun exists as a base-game keyword; nothing new |
| Divine Steed, Divine Sense | no mechanical hook in a deckbuilder |
| Blessing of Freedom, Cleanse | debuff removal is fine, but is common-tier, not rare |
| Holy Power | this is Faith. Already the core mechanic. |
| Crusader Strike / Templar's Verdict | builder/spender is Faith generation and payoff, already covered by the common and Oath tiers |

## Open

- **Numbers.** Every value above is a placeholder.
- **Does `Word of Glory`-style spending accrue Bane?** By the falling rule it should. That
  makes some Holy cards push you toward Bane, which is thematically odd but mechanically
  interesting. Worth playtesting before deciding it is a bug.
- **One rare short of parity.** Room for whatever the prototype suggests is missing.
