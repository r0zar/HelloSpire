# The Alchemist — design

**Status:** design settled, implementation in progress.

**Sources.** Issue #2 (*The Alchemist Design Doc*, v0.2) is the primary plan. Issue #4 (*The
Alchemist — Core Systems Doc*, v1.1) overrides it wherever the two disagree — #4 is the later,
tighter statement of the mechanics. This file is the reconciled result plus the decisions neither
issue made, and it is what the code is built against. Where this file and an issue disagree, this
file wins.

Everything here that differs from issue #2 is listed in [Overrides](#overrides). Read that section
first if you know issue #2.

---

## One sentence

**Nothing is useless. Some things simply have not been converted yet.**

Three pools hold value — **Potions**, **Gold**, **Max HP** — and one verb, **Transform**, moves
value between them and the player's hand. The Alchemist is not the character with the most of any
pool. It is the character who keeps the least value sitting still.

**Stats:** 68 Max HP · 99 Gold · 3 Energy · 5 draw · 3 Potion Slots. Colour `#6ad48a`.

**The tension:** Gold spent in combat is real Gold. Saving 8 Gold may cost you 9 HP now; spending
it may win the fight and make the next Merchant worse.

---

## Overrides

The decisions this file makes that issues #2 and #4 do not.

### 1. Max HP is one-way. There is no way to gain it back.

**Cut: `Regenerative Tincture`** (issue #2 §37, issue #4 §5) — *Invest 60 Gold: gain 5 Max HP and
heal to full.*

Heal-to-full on a 1-Energy card is too strong on its own, and the Max HP gain is the deeper
problem: a buy-back valve turns Render from a sacrifice into a purchase. If the body can be bought
back, spending it is a cash-flow decision, and cash-flow decisions are not heavy.

**The ruling:** Render only ever goes down. The Alchemist regains Max HP exactly the way every
other character does — Rest Sites, events, run rewards — and by no class-specific means at all.

Consequences, all intended:

- **Every Render is permanent for the run.** Four Max HP spent in Act 1 is four Max HP you do not
  have at the Act 3 boss. This is the "very heavy decision" the mechanic is for.
- **Render stays Rare-only and stays rare within Rare.** Four cards in eighty, listed below. A
  fifth would make it a template rather than an event.
- **Render payoffs must be worth a permanent cost.** Not "deal more damage" — a Render card has to
  do something no amount of Gold can buy. Both surviving Render cards create objects.
- **`Sanguine Circuit`** (issue #4's Render relic) pays in cards, never in HP. It was already
  written that way and that is now a hard rule, not a preference.
- **`Alkahest Core`** survives as a *cost*, not a refund: reforming the Philosopher's Stone charges
  the same 6 Max HP it cost to make. It is cut from the relic list for space (see
  [Relics](#relics)), not on principle.

**The four Render cards, and nothing else in the kit touches Max HP:**

| Card | Cost | Effect |
|---|---|---|
| `Homunculus Pact` | 2E Rare Skill | Render 4 Max HP → copy a card in your Hand; it costs 0 this turn. |
| `The Great Work` | 3E Rare Skill, Exhaust | Render 6 Max HP → Brew a **Philosopher's Stone**. Not Volatile. |
| `Equivalent Exchange` | 1E Rare Skill, Exhaust | Render 3 Max HP → gain 2 Energy and draw 2. *(replaces `Regenerative Tincture`'s slot)* |
| `Transmute Flesh` | 2E Rare Skill, Exhaust | Render 5 Max HP → permanently Upgrade a card in your Hand. *(the Gold-free `Masterwork`)* |

`Equivalent Exchange` and `Transmute Flesh` are new here. They exist because cutting the buy-back
valve left the Render side of the kit with two cards, which is not enough to be a mechanic. Both
are deliberately things Gold cannot buy at any price: `Transmute Flesh` is `Masterwork` for a
player who has already spent their Gold, and pricing a permanent Upgrade in permanent HP is the
cleanest statement the mechanic can make.

### 2. The card count is held at 80.

Issue #2 §42 left this open. **Resolved: hold 20 / 35 / 25.** Base-game parity is a deliberate
target, not an accident, and "accept 88" is how a class ends up with eight cards nobody can
remember. Sections 37–40 add 7 cards after `Regenerative Tincture` is cut, so 7 come out:

| Cut | Rarity | Why |
|---|---|---|
| `Empty Vial` | Common | Fourth card in the empty-belt cluster, and the name collides with `Extra Vial`. |
| `Gold Leaf` | Uncommon | 7+7 Block with a condition. `Gilded Guard` is the same card with a decision in it. |
| `Emergency Reserve` | Uncommon | Invest 5 → draw 2. `Cost of Knowledge` is that card plus an Upgrade. |
| `Gilded Revision` | Rare | `Field Upgrade` with a bigger Invest bolted on. |
| `Grand Commission` | Rare | `Commission` at a higher rarity. Two cards, one idea. |
| `Refill the Retort` | Rare | `Reconstitute` for two copies. Same. |
| `Emergency Apothecary` | Rare | Fixed Brew of two known Potions, next to `Magnum Opus`. Dull at Rare. |

Added in their place: `Extra Vial` (Common); `Concentrate`, `Bandolier` (Uncommon);
`Homunculus Pact`, `The Great Work`, `Equivalent Exchange`, `Transmute Flesh` (Rare).

Note the Rare cuts and adds balance at 4 each only because `Widen the Belt` and
`Distillation Mastery` also need Rare slots — see the [full pool](#the-80) for the final table.

### 3. Transform is a real keyword, with issue #4's boundary.

Issue #2 describes transformation loosely. Issue #4 §4 gives it a hard definition because a relic
needs a trigger boundary. That definition is adopted verbatim:

> **An effect Transforms** when it consumes a card (via Exhaust), a Potion (via Distill), or Gold
> or Max HP (via Invest or Render) *specifically in order to produce a different kind of value*.
> Spending Energy to play a card is never, by itself, a Transform.

### 4. Naming collisions resolved.

- Issue #4's **`Assayer's Scale`** and issue #2's **`Assayer's Lens`** are the same relic. Kept as
  **`Assayer's Lens`**.
- Issue #4's **`Buy Ingredients`** (0E) and issue #2's (0E, Invest 6) are the same card. Issue #2's
  wording kept.
- Issue #2's multiplayer **`Shared Flask`** keeps its name and loses its effect — see
  `design/multiplayer-cards.md`.

---

## The four systems

Condensed from issue #4, which is authoritative on all of this.

### Potions — the fast pool

**Brew [Potion]** creates a Potion in the first empty Potion Slot. A Brewed Potion is **Volatile**
unless stated otherwise: it occupies a real Slot, is used normally, costs no Energy, triggers
everything that cares about using a Potion, and is **removed at the end of combat**. It never
enters the post-combat inventory.

If no Slot is empty when a Brew resolves, the card still resolves and the Potion is lost. That is
a real cost, not an edge case.

**Distill a Potion** discards one of your Potions without resolving it, in exchange for the card's
own listed reward. Distill is *not* "using" a Potion — "whenever you use a Potion" does not fire;
"whenever a Slot becomes empty" does.

**Potency** is a persistent stat: *whenever you use a Volatile Potion, its damage and Block values
increase by your Potency.* **Never** applies to a found, bought, or procured Potion. This
restriction is load-bearing — vanilla keeps Potions outside stat scaling, and this class only gets
an exception for Potions it manufactured itself.

**Random Brew** draws from a curated Combat Potion pool that excludes anything whose value
survives the fight: no Max HP, no healing that persists, no procuring permanent Potions, no run
scaling. Curation lives at the point of the random pick and nowhere else.

### Gold — the slow pool

**Invest X Gold** is an optional clause: Pay or Decline, Pay removes X Gold immediately, Decline
still resolves the card's base effect in full, and being short of X only disables Pay. **A card
built around Invest must be worth playing for its base effect alone.**

| Tier | Gold | For |
|---|---:|---|
| Cheap | 2–4 | a minor bump |
| Medium | 5–8 | a real tempo swing |
| Large | 10–15 | Rare-tier burst |
| Permanent | 30–75 | run-level effects only |

Guardrails: cards that create Gold outright are rare, usually Exhaust, and capped. **No engine may
reward stalling a fight to farm Gold** — every repeatable Gold trigger carries a hard per-combat
cap. A player who never Invests and a player who always Invests should both be able to win.

### Max HP — the heaviest pool

**Render X Max HP**, structured exactly like Invest: Pay or Decline, never hidden, never mandatory,
Pay disabled if it would take Max HP to 0 or below. Rare only. Four cards. **One-way** — see
[Override 1](#1-max-hp-is-one-way-there-is-no-way-to-gain-it-back).

### Transform — the pump

Not a fourth pool: the verb that moves value between the other three and the hand. Six vectors,
and a good turn chains two or three:

| Consume | Produce | Example |
|---|---|---|
| A card (Exhaust) | Gold | `Transmute` |
| A card (Exhaust) | A Potion | `Pocket Formula` |
| A Potion (Distill) | Energy / cards | `Distillation Column` |
| Gold (Invest) | A permanent Upgrade | `Masterwork` |
| Gold (Invest) | A Potion | `Buy Ingredients` |
| A card | A different card | `Field Upgrade` |
| **Max HP (Render)** | **a card, an object, a permanent Upgrade** | `Homunculus Pact`, `The Great Work`, `Transmute Flesh` |

Transform cards undershoot on raw stats on purpose — the conversion is the second half of the
payment. Transform effects lean on Exhaust so that every conversion draws down the current deck to
pay for the new value.

---

## Starting kit

**Deck (10):** Strike ×4, Defend ×4, `Pyric Formula`, `Aegis Formula`.

```
Pyric Formula   1E  Basic Skill, Exhaust.  Brew an Explosive Ampoule.   (+: costs 0)
Aegis Formula   1E  Basic Skill, Exhaust.  Brew a Block Potion.         (+: costs 0)
```

Turn one teaches the belt has space, cards make Potions, Potions cost no Energy, and a Brewed
Potion is a tool that vanishes. Gold is deliberately **not** in the starting deck — one system at
a time.

**Starting relic: Portable Alembic** — *at the start of each combat, Brew a random Common Combat
Potion.* Ancient upgrade **Greater Alembic** — *Brew 2 instead.* Neither touches Slot count, so
neither compounds with belt-size relics.

---

## The 80

20 Common / 35 Uncommon / 25 Rare, after the cuts and adds in
[Override 2](#2-the-card-count-is-held-at-80). Numbers are placeholders until the
damage-per-energy benchmark in `TODO.md` Phase 8 exists.

### Common — 20

| Card | Type | Cost | Effect | Upgrade |
|---|---|---:|---|---|
| **Flask Toss** | Attack | 1 | Deal 8. If you used a Potion this turn, deal 4 more. | 10; bonus 5. |
| **Glass Shard** | Attack | 1 | Deal 9. If you have an empty Slot, deal 3 more. | 12. |
| **Scatter Flask** | Attack | 1 | Deal 5 to ALL. If you used a Potion this turn, 3 more to ALL. | 7 / 3. |
| **Gilded Scalpel** | Attack | 1 | Deal 8. If you gained Gold this turn, deal 3 more. | 11. |
| **Pyric Burst** | Attack | 2 | Deal 16. Invest 3 → deal 6 more. | 20. |
| **Quick Silver** | Attack | 0 | Deal 4. If a Slot became empty this turn, deal 4 more. | 6. |
| **Crucible Blow** | Attack | 1 | Deal 8. Exhaust another card in Hand → deal 6 more. | 10; bonus 7. |
| **Copper Shot** | Attack | 1 | Deal 7. Invest 2 → apply 1 Weak. | 10. |
| **Transmute** | Skill | 1 | Exhaust another card. Gain 5 Gold. Exhaust. | 7 Gold. |
| **Salvage Reagents** | Skill | 0 | Exhaust another card. Gain 4 Block, 2 Gold. Exhaust. | 6 / 3. |
| **Pocket Formula** | Skill | 1 | Brew a random Common Combat Potion. Exhaust. | Costs 0. |
| **Glass Apron** | Skill | 1 | Gain 7 Block, +1 per empty Slot. | 10. |
| **Steady Pour** | Skill | 1 | Gain 8 Block. If you used a Potion this turn, 3 more. | 11. |
| **Dilute** | Skill | 0 | Distill a Potion. Draw 2. Exhaust. | Draw 3. |
| **Recycle Glass** | Skill | 1 | Gain 5 Block. Exhaust another card → 5 more. | 7; bonus 6. |
| **Coin Purse** | Skill | 0 | Invest 4 → gain 1 Energy. Exhaust. | Invest 3. |
| **Bitter Solvent** | Skill | 1 | Apply 2 Weak. If you used a Potion this turn, gain 4 Block. | 3 Weak. |
| **Market Sense** | Skill | 1 | Draw 2, discard 1. If you spent Gold this turn, gain 3 Block. | Draw 3. |
| **Extra Vial** | Skill | 1 | Gain 1 Potion Slot for this combat. Volatile-only. Exhaust. | Costs 0. |
| **Residual Heat** | Power | 1 | First Potion used each turn deals 3 to a random enemy. | 5. |

### Uncommon — 35

| Card | Type | Cost | Effect | Upgrade |
|---|---|---:|---|---|
| **Bottle Barrage** | Attack | 1 | Deal 3 per occupied Slot. | 4 per Slot. |
| **Shatterstock** | Attack | 1 | Deal 9. Distill a Potion → deal 9 again. | Both 12. |
| **Pressure Burst** | Attack | 1 | Deal 8. If the belt is full, deal 8 more. | 11; bonus 9. |
| **Empty Bottle** | Attack | 0 | Deal 4. If you hold no Potions, deal 4 more and draw 1. | 6. |
| **Cinnabar Edge** | Attack | 1 | Deal 9. If you Exhausted a card this turn, apply 1 Vulnerable. | 12. |
| **Black Market Blade** | Attack | 1 | Deal 8. Invest up to 5 → +2 damage per Gold. | 11 base. |
| **Volatile Compound** | Attack | 2 | Deal 18. Costs 1 if you used a Potion this turn. | 22. |
| **Flash Powder** | Attack | 1 | Deal 6 to ALL. Invest 4 → 5 more to ALL. | 8; bonus 6. |
| **Auric Needle** | Attack | 1 | Deal 7, +1 per Gold gained this combat, max +10. | max +15. |
| **Corkscrew** | Attack | 1 | Deal 5 twice; three times if you Distilled this turn. | 7. |
| **Reactive Slash** | Attack | 1 | Deal 10. If you created a card this turn, deal 5 more. | 13; bonus 6. |
| **Mercury Lance** | Attack | 2 | Deal 20. Exhaust a random other card in Hand. | 25. |
| **Distillation Column** | Skill | 1 | Distill a Potion. Gain 2 Energy. Exhaust. | Also draw 1. |
| **Reconstitute** | Skill | 1 | Brew a Volatile copy of a Potion you used this combat. Exhaust. | Costs 0. |
| **Buy Ingredients** | Skill | 0 | Invest 6 → choose 1 of 3 Common Combat Potions and Brew it. Exhaust. | Invest 4. |
| **Commission** | Skill | 1 | Invest 8 → create a random Alchemist card in Hand, costing 0 this turn. Exhaust. | Invest 6. |
| **Field Upgrade** | Skill | 1 | Upgrade a card in Hand for this combat. Invest 4 → Upgrade ALL instead. | Costs 0. |
| **Gilded Guard** | Skill | 1 | Gain 7 Block. Invest up to 5 → +2 Block per Gold. | 10 base. |
| **Liquidate** | Skill | 1 | Exhaust another card. Gain 3 Gold +2 per Energy it cost, max 9. Exhaust. | 4 base, max 11. |
| **Smelt the Weak** | Skill | 0 | Exhaust a Status or non-Eternal Curse from Hand. Gain 1 Energy. Exhaust. | Also draw 1. |
| **Spare Flask** | Skill | 1 | Gain 5 Block. If a Slot is empty, Brew a random Common Combat Potion. Exhaust. | 8 Block. |
| **Stabilize** | Skill | 1 | Invest 40 → remove Volatile from a Potion. Exhaust. | Invest 30. |
| **Safety Goggles** | Skill | 1 | Gain 3 Block per empty Slot. | 4 per Slot. |
| **Cost of Knowledge** | Skill | 0 | Invest 3 → draw 1 and Upgrade it for this combat. Exhaust. | Invest 2. |
| **Catalytic Wash** | Skill | 1 | Exhaust up to 2 cards in Hand. Draw 1 per card Exhausted. | Draw 1 more. |
| **False Bottom** | Skill | 1 | Bottom a card. Draw 2. If you used a Potion this turn, gain 4 Block. | Costs 0. |
| **Tincture Trade** | Skill | 1 | Distill a Potion. Gain 8 Block and draw 1. | 11 Block. |
| **Bandolier** | Skill | 1 | Gain 2 Potion Slots for this combat. Volatile-only. Exhaust. | Costs 0. |
| **Concentrate** | Power | 1 | Gain 2 Potency. | 3 Potency. |
| **Heat Bath** | Power | 1 | First Brew each turn: gain 4 Block. | 6. |
| **Coin Press** | Power | 1 | First Exhaust each turn: gain 1 Gold. Max 3 times per combat. | Max 4. |
| **Merchant's Instinct** | Power | 1 | Whenever you Invest, gain 3 Block. | 5. |
| **Reactive Mixture** | Power | 1 | First Potion used each turn: draw 1. | Also gain 2 Block. |
| **Closed System** | Power | 1 | First Slot emptied each turn: gain 4 Block. | 6. |
| **Refiner's Eye** | Power | 2 | Cards you create in combat are created Upgraded. | Costs 1. |

### Rare — 25

| Card | Type | Cost | Effect | Upgrade |
|---|---|---:|---|---|
| **Philosopher's Flame** | Attack | 2 | Deal 20. Invest 10 → deal 20 more. | Both 24. |
| **Chain Reaction** | Attack | 2 | Deal 10 to ALL, +4 to ALL per Potion used this turn, max 3. | 12; bonus 5. |
| **Midas Needle** | Attack | 1 | Deal 5 +1 per Gold spent this combat, max +30. | 8 base; max +40. |
| **Matter Annihilation** | Attack | 2 | Exhaust another card. Deal 8 +7 per Energy it cost. | 10; +8 each. |
| **Homunculus Assault** | Attack | 2 | Deal 14. Invest 8 → create 2 random Attacks in Hand, costing 0 this turn. | 18. |
| **Gilded Execution** | Attack | 2 | Deal 18. If Fatal, gain 15 Gold. | 23; 20 Gold. |
| **Grand Combustion** | Attack | 3 | Deal 18 to ALL. Distill any number of Potions; +6 to ALL each. Exhaust. | 22; bonus 7. |
| **Alchemize** | Skill | 1 | Procure a random Potion. Exhaust. *(the base-game card, adopted)* | Costs 0. |
| **Heavy Transmute** | Skill | 1 | Exhaust every other card in Hand. Gain 4 Gold each. Exhaust. | 5 Gold each. |
| **Magnum Opus** | Skill | 3 | Fill all empty Slots with random Volatile Combat Potions. Exhaust. | Costs 2. |
| **Masterwork** | Skill | 2 | Invest 50 → permanently Upgrade another card in Hand. Exhaust. | Invest 40. |
| **Essence Distillation** | Skill | 1 | Distill a Potion; payout scales with its rarity. Exhaust. | Draw 1 more in each case. |
| **Bottled Time** | Skill | 1 | The next Potion you use this turn is not consumed. Exhaust. | Costs 0. |
| **Gold Standard** | Skill | 0 | Invest 10 → gain 2 Energy and draw 2. Exhaust. | Invest 7. |
| **Perfect Solvent** | Skill | 1 | Exhaust up to 3 other cards. Gain 5 Block and draw 1 each. Exhaust. | 6 Block each. |
| **Widen the Belt** | Skill | 2 | Invest 75 → permanently gain 1 Potion Slot. Exhaust. | Invest 60. |
| **Homunculus Pact** | Skill | 2 | **Render 4** → copy a card in Hand; it costs 0 this turn. | Render 3. |
| **The Great Work** | Skill | 3 | **Render 6** → Brew a Philosopher's Stone. Not Volatile. Exhaust. | Render 5. |
| **Equivalent Exchange** | Skill | 1 | **Render 3** → gain 2 Energy and draw 2. Exhaust. | Render 2. |
| **Transmute Flesh** | Skill | 2 | **Render 5** → permanently Upgrade a card in Hand. Exhaust. | Render 4. |
| **Compound Interest** | Power | 1 | At combat end, regain 25% of Gold Invested this combat. | 33%. |
| **Eternal Crucible** | Power | 2 | The first Potion each combat resolves twice, consumed once. | Costs 1. |
| **Golden Engine** | Power | 2 | First Gold gained each turn: draw 1 and gain 2 Block. | 4 Block. |
| **Conservation of Matter** | Power | 2 | First Exhaust each turn: draw 1. | Costs 1. |
| **Distillation Mastery** | Power | 2 | Whenever you Distill, gain 1 Potency this combat. | 2 Potency. |

Cut for space against the 25: `Laboratory Network` (a 25% chance is not a Rare) and
`Philosopher's Method` (Strength/Dexterity-per-Potion is the strongest thing in the draft and
wants its own playtest before it takes a slot). Both are the first candidates back in if a Render
card proves weak.

---

## Relics — 8

Base-game parity is exactly 8 per character: 1 starter and 7 more.

| Rarity | Relic | Effect |
|---|---|---|
| Starter | **Portable Alembic** | Start of each combat, Brew a random Common Combat Potion. |
| Common | **Cork Stopper** | First Potion used each combat: draw 1. |
| Uncommon | **Ceramic Retort** | First Brew each turn: gain 4 Block. |
| Uncommon | **Assayer's Lens** | First Gold gained in combat each combat: gain 1 Energy. |
| Rare | **Golden Crucible** | Whenever a card grants you Gold in combat, gain 3 additional Gold. |
| Rare | **Alchemist's Ledger** | Whenever you **Transform**, gain 1 Block. No per-turn limit. |
| Rare | **Sanguine Circuit** | Whenever you **Render**, draw 2 cards. Never HP — see Override 1. |
| Shop | **Gilded Ledger** | Invest costs are reduced by 2 Gold, minimum 1. Merchant prices are 10% higher. |

`Alchemist's Ledger` is the reason Transform needed a hard definition. Cut for space and named
here as the alternates: `Utility Sash`, `Bottomless Stopper`, `Glass Homunculus`, `Alkahest Core`.

## Potions — 3

| Rarity | Potion | Effect |
|---|---|---|
| Common | **Solvent Flask** | Exhaust a card in your Hand. Draw 2. |
| Uncommon | **Aurum Tincture** | Gain 15 Gold and 1 Energy. *(excluded from random Brew — it makes real Gold)* |
| Rare | **Panacea of Plenty** | Fill all empty Slots with random Volatile Common Combat Potions. |

Plus **Philosopher's Stone**, a unique non-Brewable Potion with exactly one source
(`The Great Work`): *deal 20 to ALL, gain 20 Block, gain 2 Energy, draw 3, gain 2 Strength and 2
Dexterity this combat.* Not Volatile, so it survives combat and can be hoarded for a boss.

---

## Archetypes

Seven, from issue #2 §17, unchanged: **Brewer** (Volatile throughput + Potency), **Transmutation**
(Exhaust → Gold), **Investor** (Gold as a combat resource), **Distillation** (Potions as fuel),
**Full Belt** (payoffs for holding), **Empty Belt** (payoffs for spending), **Gilded Scholar**
(card creation and Upgrades).

Full Belt and Empty Belt being opposite payoffs off the same board state is the class's cleanest
internal tension; both must remain legitimate.

---

## Balance watchlist

1. **Gold farming.** Any repeatable Gold trigger without a per-combat cap is a bug. `Coin Press` is
   capped; verify nothing else stacks past it.
2. **`Heavy Transmute` + a dead hand.** Exhausting six Statuses for 24 Gold is either the best
   card in the class or fine. It is the first thing to measure.
3. **Belt size stacking.** `Widen the Belt` + `Utility Sash` + a found `Potion Belt` is uncapped by
   design, as in vanilla. Watch it at high Ascension.
4. **Potency + `Magnum Opus`.** Six Volatile Potions all scaled by Potency in one fight is the
   Brewer's ceiling. Check it against a Time Eater-shaped fight.
5. **Render's real cost.** The metric is whether players ever *decline* a Render. If Render is
   always paid, the cost is too cheap and the numbers go up — never the payoffs down.
6. **Sozu and Ectoplasm.** Both boss relics are near-unpickable for this class. That is intended
   and stays; it is a real draft decision, not a bug.

## Open

- **Every number.** Placeholders until the `TODO.md` Phase 8 benchmark exists.
- **Alchemize suppression.** Adopting the base-game Colorless card into the class pool needs the
  Colorless copy suppressed while playing the Alchemist, or two cards share a name.
- **`Bottled Time` + `Philosopher's Stone`.** Not consuming the Stone is correct by the rules as
  written, and is also the strongest line in the class. Verify it is not degenerate before shipping.
- **Volatile-only Slots.** `Extra Vial`, `Bandolier` and `Utility Sash` all create Slots a
  non-Volatile Potion may not occupy. That "skips restricted Slots" rule needs the game's Slot API
  to support per-Slot restriction at all.
