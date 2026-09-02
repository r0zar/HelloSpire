# The Gunslinger — Slay the Spire 2 Character Design Plan
**Version:** 0.1  
**Design target:** A sixth-character-sized fan concept built to the current Slay the Spire 2 roster structure.  
**Theme:** A weathered, Dark-Tower-adjacent gunslinger: calm, dangerous, defensive, and willing to gamble with fate.

> **Design note:** This is intentionally an original mechanical interpretation rather than a direct adaptation of a copyrighted character. The tone is "mythic western gunslinger on an endless road."

---

## 1. Current STS2 Template Being Mirrored

As checked on **August 29, 2026**, the five current Slay the Spire 2 characters each use the following normal class-package shape:

- 80 normal class cards:
  - 20 Common
  - 35 Uncommon
  - 25 Rare
- A starter relic which can be replaced by an upgraded Ancient version.
- 7 additional class relics:
  - 1 Common
  - 2 Uncommon
  - 3 Rare
  - 1 Shop
- 3 class potions.
- 5 additional multiplayer-specific cards.

This plan mirrors that structure.

---

## 2. Character Identity

### The Gunslinger
**Max HP:** 72  
**Ascension 2+ target HP:** 58  
**Energy:** 3  
**Draw:** 5  

**One-line identity:**  
A sequencing character who loads a visible six-shooter, manipulates its cylinder, and chooses between reliable defense and high-risk burst damage.

### Intended Skill Profile
- Complexity: Medium-high.
- Easier than Necrobinder's layered companion/resource management.
- More sequencing-heavy than Ironclad.
- More deterministic than a pure random/gambling class.
- Strong player expression comes from planning the next 2–4 chambers.

### Core Strengths
- Flexible front-loaded damage once the Cylinder is prepared.
- Strong Weak access.
- Layered defense: Block + Armor, with Intangible as the rare panic button.
- Gadgets: a whole second axis that works with the gun empty.
- Excellent tactical control over single large enemy hits.
- Strong "burst turn" potential through Deadeye and multi-shot cards.

### Core Weaknesses
- Ammo setup costs cards and Energy.
- Multi-shot cards are poor when the Cylinder is empty.
- Intangible is scarce: two sources in the whole pack, both Rare.
- Armor erodes under repeated hits.
- Weak/Debilitate package is defensive and lacks native Vulnerable.
- Several premium cards become weaker if draw order prevents proper Cylinder setup.

---

# 3. Core Combat System

## 3.1 The Cylinder
The Gunslinger has a **six-chamber Cylinder** visible beside the hand.

Each chamber is either:
- Empty, or
- Loaded with one Round.

One chamber is always under the **hammer**.

The Cylinder state is fully visible to the player unless a card is resolving an immediate gamble such as Russian Roulette.

### The cylinder on screen
A brass ring with six chambers in it sits beside the Energy orb: a big circle with six small ones
around the inside, a fixed sight at twelve o'clock, and the loaded count in the middle.

- Each chamber is coloured by what is in it — one colour per kind of ammunition, near-black when
  empty — so the next three chambers can be read at a glance rather than counted in a tooltip.
- The chamber under the hammer is called out by an open reticle drawn over it, not by a colour.
  Colour is already spoken for: it is how a chamber says what is loaded in it, and the chamber
  under the hammer is the one whose contents matter most. An outline says "this one is next"
  without overwriting what the chamber is telling you.
- The ring turns one sixth of a revolution each time the hammer moves, in the direction a real
  cylinder would turn, so a Fire and a Cycle look like what they are.
- A Spin adds a whole extra revolution on top of wherever it landed. Without that, a Spin that
  happened to land on the chamber it started from would not read as having spun at all.

Implemented as `CylinderDisplay`, built from stock Godot nodes and driven by a change event on
`CylinderPower` — no per-frame polling, one Tween per change. It hangs off whatever node in the
combat UI names itself for Energy; if the game renames that node the search fails and the widget
falls back to the bottom-left corner, which looks wrong but never crashes and is never invisible.

### Load
**Load [Round]** places the Round into the first empty chamber clockwise, beginning at the hammer.

If every chamber is full, Loading remains legal:
- Replace chambers beginning at the hammer and moving clockwise.
- Replaced ammunition is discarded.
- Loading does not move the hammer.

This rule prevents ammo cards from becoming unplayable while still making over-loading a meaningful sequencing cost.

### The Cylinder between fights
The Cylinder is emptied at the start of every combat: no Rounds, hammer on chamber 1, every
counter at zero. Whatever is in the gun on turn one was put there by *this* fight — Old Iron and
the other opening-load effects run afterwards, at the top of the player's first turn.

This is worth stating as a rule because it is not free in the implementation: power models in
StS2 outlive the combat they were applied in, so a Cylinder left alone carries the last fight's
ammunition into the next one. `CylinderPower` clears itself both when combat ends and the first
time it is touched in a combat it has not seen before.

### Fire X
To **Fire 1**:
1. Resolve the chamber under the hammer.
2. If it contains a Round, deal that Round's damage and resolve its effect.
3. Empty that chamber.
4. Advance the hammer one chamber clockwise.

If the chamber is empty, it **Clicks**:
- No Round damage/effect occurs.
- The hammer still advances.

**Fire X** repeats this process X times.

Unless a card says otherwise, a Round fired by an Attack deals Attack damage and therefore interacts normally with Strength, Weak, Vulnerable, and other Attack modifiers.

### Cycle X
Advance the hammer clockwise X chambers **without firing**.

Cycle is the Gunslinger's main deterministic setup tool.

### Spin
Move the hammer to a **random chamber**.

The result is revealed after the Spin resolves.  
If the same card immediately performs another action (for example, Russian Roulette), the player cannot interrupt between the Spin and that action.

Spin should be used as:
- A gamble.
- A cheap way to escape a bad chamber.
- An engine trigger for certain Powers/relics.
- A setup tool that becomes more controllable with rare support.

### Click
A Click is simply a Fire action resolving an empty chamber.

Clicks are normally a failure state, but a small number of cards turn them into tempo or defense.

---

## 3.2 Deadeye
**Deadeye X:** Every Round you Fire this turn deals **X additional Attack damage**. All Deadeye is removed at the end of your turn.

Rules:
- A Click gets nothing, and costs nothing — Deadeye is not spent by a shot.
- Deadeye applies to every Round in a multi-shot Fire action, not just the first.
- Deadeye is additive, similar in practical damage ordering to Vigor, but only works on Rounds.
- It does not carry across turns. Banking it for "the right chamber next turn" is not a plan.

### Why this changed (2026-08-30)
The original rule spent Deadeye on the first Round that landed, which made the keyword strictly
worse than printing flat damage on the same card: "gain 5 Deadeye" was 5 damage once, on a card
that also had to survive to a turn where the gun was loaded. Worse, it inverted the character —
stacking Deadeye and then playing Fire 6 was actively wrong, when Fire 6 is the exact turn the
whole deck is built towards.

As a turn-long aura it does the job the character was designed around: prepare the cylinder,
sharpen the turn, then spend the whole gun into it. The cost is that it no longer banks, so a
Deadeye card played on a turn you cannot Fire is wasted — which is the right tension for a
character whose problem is that setting the gun up and using it compete for the same Energy.

**Balance note.** This is a large buff and the numbers have not been retuned to match. Deadeye
Focus (12) into a Fire 6 turn moved from +12 damage to +72. If it plays as strong as it reads,
the fix is on the grants rather than the rule: roughly halving Steady Hand (5), Hammerfall (8),
Deadeye Focus (12), High Noon (3) and Sightline Tonic (10) restores the old ceiling.

Expiry is implemented on the *enemy* side's turn start rather than the player's. Bottomless
Bandolier and Ride Together both grant Deadeye from the player's turn-start sweep, and the order
powers are visited in is not fixed — clearing on the same edge that grants would have eaten a
fresh stack about half the time.

---

## 3.3 Armor
**Armor X:** Whenever unblocked damage from an enemy Attack would cause HP loss, reduce that damage instance by X, then lose 1 Armor.

Examples:
- 2 Armor vs a 12-damage hit after Block: take 10, then Armor becomes 1.
- 3 Armor vs three separate 5-damage hits with no Block: take 2, then 3, then 4; Armor becomes 0.

Design purpose:
- Armor is stronger than Block against repeated future turns.
- It is weaker against long multi-hit sequences because it erodes.
- It is intentionally distinct from STS2 **Plating**, which produces Block at end of turn and then decays.

---

## 3.4 Intangible

The Gunslinger's premium defence is the base game's **Intangible**, not a keyword of its own.

### Why Dodge is gone (2026-09-02)
Dodge read "prevent all damage from the next X individual enemy Attack hits this turn", cleared at
the start of the owner's turn, and cost the mod a custom power, a branch in the damage patch and a
pending-flag handshake so a stack was only spent when a hit really landed — all of it to make
"hits" countable, which is the one thing the base game's damage pipeline does not hand you
cleanly.

Intangible is the game's own word for the same job, needs no keyword, no per-hit bookkeeping and
no patch, and every player already knows what it does. The mod's damage patch is now Armor and
nothing else.

### What it costs
Intangible answers an entire enemy turn rather than one hit, so it is worth several times what a
Dodge stack was, and it is priced accordingly. Exactly two sources exist:

| Source | Rarity | Grants |
|---|---|---|
| **Ghost Step** | Rare card, 1 Energy, Exhaust | 1 Intangible |
| **Ghost Smoke** | Rare potion | 1 Intangible |

Balance rule:
- No card below Rare grants Intangible, and no engine, relic or Round ever does.
- One stack at a time. A card that grants 2 is a card that skips a boss turn twice.

Everything that used to hand out cheap Dodge — Duck and Weave, Never Still, Dead Man's Bluff, the
Smoke Round — now pays in Block and Armor instead, and most of them became **Gadgets** (§3.7).

---

## 3.5 Weak and Debilitate
The Gunslinger has strong access to **Weak**.

The Gunslinger has **limited** access to **Debilitate**.

In current STS2, Debilitate doubles the effectiveness of Weak and Vulnerable on that target for its duration. The Gunslinger intentionally has almost no native Vulnerable, so Debilitate is mainly:
- A defensive multiplier on Weak in solo play.
- A team-enabling debuff in multiplayer.

This preserves Ironclad's stronger Vulnerable identity.

---

## 3.6 Self-Fire
A few risk cards can **Self-Fire**.

To Self-Fire 1:
- Resolve the current chamber.
- If empty: Click.
- If loaded: lose HP equal to the Round's **printed damage**.
- Ignore the Round's other effects.
- Empty the chamber.
- Advance the hammer.

Self-Fire damage:
- Ignores Strength, Weak, Vulnerable, and Deadeye.
- Is HP loss for balance purposes, not normal enemy Attack damage.
- Is not reduced by Block or Armor unless future testing explicitly changes that rule.

The last line is enforced by the damage patch, which now skips anything flagged `Unpowered` —
the flag every self-inflicted cost in this character carries. Before that, a defensive power on
the Gunslinger made Russian Roulette's Self-Fire, Grit Teeth's HP cost and the Black Powder
Round's recoil all free, which quietly removed the risk from every risk card in the set.

This creates a clean Russian Roulette ruleset.

---

## 3.7 Gadgets

A **Gadget** is a card that uses no ammunition: it never Loads, Fires, Cycles or Spins.

### Why they exist (2026-09-02)
The character shipped with one axis. Put Rounds in, take Rounds out — every card in the set sat
somewhere on that line, so a hand with no ammunition card *and* no Fire card did nothing at all,
and a draft that missed the cartridge commons had no second plan. The alternative archetypes the
plan lists (Weak/Debilitate, Armor) were never really alternatives: they were riders on cards that
still wanted the gun.

Gadgets are the second plan. They pay in debuffs, Block and Armor, and they are worth exactly as
much with the cylinder empty as with it full.

Rules:
- A Gadget touches no chamber. Reading state the cylinder owns is fine — Bear Trap asks how much
  Armor you hold — but a Gadget that Loads is a cartridge card with the wrong word on it, and the
  archetype stops meaning anything the moment one ships.
- Gadget-ness is a property of the card, not a cost or a keyword the player pays for. Payoff cards
  count Gadgets played; nothing consumes them.
- The word appears on the card so the hover tip is reachable.

### The package

| Rarity | Card | Type | Cost | Effect |
|---|---|---|---:|---|
| Common | **Pistol Whip** | Attack | 1 | Deal 9 damage. *(retagged)* |
| Common | **Shoulder Shot** | Attack | 1 | Deal 7 damage. Gain 4 Block. *(retagged)* |
| Common | **Gut Shot** | Attack | 1 | Deal 8 damage; 4 more if the enemy is Weak. *(retagged)* |
| Common | **Warning Shot** | Attack | 0 | Deal 3 damage. Apply 1 Weak. Exhaust. *(retagged)* |
| Common | **Pocket Sand** | Skill | 1 | Apply 2 Weak. *(retagged)* |
| Common | **Blinding Powder** | Skill | 1 | Apply 1 Weak to ALL enemies. Gain 3 Block. |
| Common | **Bear Trap** | Attack | 1 | Deal 6 damage and apply 1 Weak. If you have Armor, deal 4 more. |
| Common | **Tripwire** | Skill | 0 | Apply 1 Weak. Gain 1 Armor. Exhaust. |
| Uncommon | **Cold Read** | Skill | 1 | Apply 1 Weak and 1 Debilitate. Exhaust. *(retagged)* |
| Uncommon | **Under the Duster** | Skill | 1 | Gain 3 Armor. *(retagged)* |
| Uncommon | **Grit Teeth** | Skill | 1 | Lose 2 HP. Gain 10 Block and 2 Armor. *(retagged)* |
| Uncommon | **Duck and Weave** | Skill | 2 | Gain 3 Armor, then Block equal to twice your Armor. Exhaust. *(rebuilt)* |
| Uncommon | **Smoke Bomb** | Skill | 1 | Gain 6 Block. Apply 1 Weak to ALL enemies. |
| Uncommon | **Field Kit** | Skill | 1 | Gain 2 Armor. Draw 1 card. |
| Uncommon | **Scattergun Shell** | Attack | 2 | Deal 8 damage to ALL enemies. Apply 1 Weak to ALL enemies. |
| Uncommon | **Tinker's Kit** | Power | 1 | The first Gadget you play each turn, draw 1 card. *(not itself a Gadget)* |
| Rare | **Never Still** | Skill | 1 | Gain 8 Block. Next turn, gain 1 Energy and draw 1. Exhaust. *(rebuilt)* |
| Rare | **Gadgeteer** | Power | 2 | Whenever you play a Gadget, gain 1 Armor. *(not itself a Gadget)* |

Tinker's Kit and Gadgeteer are deliberately not Gadgets themselves: a Power that counted its own
play would make the very first trigger an ordering question nobody should have to reason about.

### Balance watchlist
- Tripwire is free and Gadgeteer pays per Gadget, so a hand of zero-cost Gadgets is the loop to
  watch. Tripwire Exhausts for exactly that reason.
- Duck and Weave reads Armor *after* its own Armor lands, so its floor is the printed value
  doubled and its ceiling is whatever the deck has stacked. Watch it alongside Iron Will.
- The archetype's damage ceiling is meant to be low. If a pure Gadget deck starts closing fights
  faster than a gun deck, cut Scattergun Shell rather than the defensive cards.

---

# 4. Ammunition

| Round | Printed effect when Fired at an enemy | Intended niche |
|---|---|---|
| **Lead Round** | Deal 7 damage. | Baseline ammunition. |
| **Heavy Round** | Deal 12 damage. | Pure damage / Deadeye target. |
| **Crippling Round** | Deal 5 damage. Apply 1 Weak. | Control. |
| **Piercing Round** | Deal 8 damage. This Round ignores Block. | Anti-Block. |
| **Guard Round** | Deal 5 damage. Gain 5 Block. | Hybrid defense. |
| **Smoke Round** | Deal 3 damage. Gain 4 Block and 1 Armor. | Layered defensive ammunition. |
| **Rending Round** | Deal 6 damage. Apply 1 Debilitate. | Rare debuff ammunition. |
| **Black Powder Round** | Deal 16 damage. After firing it at an enemy, lose 3 HP. | Rare risk damage. |
| **Dead Man's Round** | Deal 24 damage. | Russian Roulette payload. |

### Ammo Philosophy
Lead, Heavy, and Crippling appear early.  
Piercing, Guard, and Smoke appear primarily at Uncommon.  
Rending, Black Powder, and Dead Man's Rounds are Rare/special.

This keeps Act 1 readable and stops the Cylinder UI from presenting nine mechanics immediately.

### Why these numbers moved (balance pass, 2026-08-29)
The first draft priced a Lead Round at 6. In play that is the wrong number, because a Round is
never free: it costs a card to Load *and* a card to Fire. Two cards and two Energy to put 12
damage on a target is worse than two Strikes, which is exactly the "the Gunslinger feels weak"
complaint. Lead moved to 7 and every specialist Round moved with it, so the gap that makes
special ammunition worth drafting is preserved rather than squeezed.

The other half of that fix is on the Fire side rather than the ammunition side — see the starter
deck below.

### Randomness
The character leans slightly random about *what* is in the gun and *how much* of it, and never
about what happens when you pull the trigger. Firing, Cycling and the chamber order are fully
deterministic and fully visible; Loading is where the dice are.

| Effect | What is rolled |
|---|---|
| **Reload** | 2-4 Lead Rounds (3-5 upgraded) |
| **Quick Load** | 1-2 more of the last Round type you Loaded (2-3 upgraded) |
| **Fresh Cartridges** | 1 Round from the seven ordinary kinds (2 upgraded) |
| **Old Iron** | the fourth opening Round, from Heavy / Crippling / Piercing / Guard |
| **Oiled Rag** | the spare Round, from the seven ordinary kinds |
| **Spare Speedloader** | 3-5 Lead Rounds |
| **Speedloader Flask** | the fourth Round, from Heavy / Crippling / Piercing / Guard |
| **Bottomless Bandolier** | the per-turn Round, from Heavy / Crippling / Piercing / Guard |

Two rules hold this together. Every roll has a floor at least as good as the fixed value it
replaced, so a bad roll is never worse than the old card was. And Black Powder and Dead Man's
Rounds are never in a random pool: both carry a drawback the card that chambers them is priced
around, and neither should ever arrive unannounced.

All of it runs through `Revolver.Roll`, on the seeded combat RNG stream, so a run stays
reproducible.

---

# 5. Starting Loadout

## Starting Deck — 10 cards
- Strike ×4
- Defend ×4
- **Reload** ×1
- **Quick Draw** ×1

### Reload
**1 Energy — Skill**  
Load 2-4 Lead Rounds. Gain 3 Block.

**Reload+**  
Load 3-5 Lead Rounds. Gain 5 Block.

### Quick Draw
**0 Energy — Attack**  
Fire 2.

**Quick Draw+**  
Fire 2-3.

The upgrade used to grant 2 Deadeye, which was the weakest upgrade in the pack: two damage, once,
on the card whose whole identity is that it is free and spends two chambers. Upgrading it along
the axis the character actually cares about — how much of the cylinder one card can spend — makes
it a real choice at a campfire, and it is the same "roughly what you asked for, occasionally more"
roll Reload already teaches on turn one.

The starter deck teaches:
1. Ammo exists, and how much of it you get is the gun's business.
2. Firing consumes it.
3. A zero-cost Fire card is only good if you prepared ammunition.
4. Ordinary Strikes remain available when the gun is empty.

**Why Quick Draw fires twice.** One Round per free card is the rate that made the character feel
weak, and no amount of raising Round damage fixes it — a card that spends one chamber is priced
against a Strike, and the Gunslinger already paid a card to put that chamber there. Two chambers
per Fire card is the rate the rest of the set is written against, and the starter is where the
player learns to expect it. It also makes the empty gun hurt more, which is the correct tension:
Quick Draw into a dry cylinder is now two Clicks, not one.

---

# 6. Starter Relic and Ancient Upgrade

## Old Iron — Starter Relic
At the start of each combat, **Load 3 Lead Rounds and 1 random special Round**
(Heavy, Crippling, Piercing or Guard).

Purpose:
- Guarantees Quick Draw is functional in the opening hand — four chambers now covers both its shots.
- Lets the player experience the gun, and the ammunition menu, immediately.
- Does not remove the need to draft or play Loading cards.
- Makes the first turn of every fight a slightly different puzzle, which is the temperament the
  rest of the set is written for.

## True Iron — Ancient Starter Upgrade
Replaces Old Iron.

At the start of each combat, **fill all 6 chambers with Lead Rounds**.  
**Lead Rounds deal 2 additional damage.**

Balance rationale:
- Comparable in spirit to current STS2 starter upgrades that significantly amplify the character's native resource engine.
- Six prepared Rounds are powerful, but still require Fire cards to convert into damage.
- The +1 Lead damage is intentionally narrow so special ammo remains desirable.

---

# 7. Complete 87-Card Pool

The template in section 1 is 80 class cards, 20/35/25. This pool is 87, and the extra seven are
deliberate: five of them (Thumb the Gate, Take Stock, Clear the Chamber, Powder Burn, Dry Fire)
are the discard / draw / exhaust axis the first pass had almost nothing on. The Gunslinger spends
cards faster than any other resource — ammunition is paid for in cards, and a stalled hand is the
character's real failure state — so a set with no way to convert cards into anything was missing
a whole verb. If the pool needs to come back to 80, the cut list is the low-impact filler in each
rarity, not these.

## Commons — 23
| # | Card | Type | Cost | Base | Upgrade | Role |
|---:|---|---|---:|---|---|---|
| 1 | **Snap Shot** | Attack | 1 | Fire 1. Draw 1 card. | Gain 2 Deadeye, then Fire 1. Draw 1 card. | Fire / draw |
| 2 | **Fan the Hammer** | Attack | 2 | Fire 3. | Fire 4. | Multi-shot |
| 3 | **Lead Storm** | Attack | 2 | Load 2 Lead Rounds, then Fire 2. | Load 3 Lead Rounds, then Fire 3. | Self-contained Fire |
| 4 | **Last Round** | Attack | 1 | Fire 1. If the Cylinder is empty afterward, return a Reload to your hand from anywhere. | Costs 0. | Empty-cylinder |
| 5 | **Suppressing Fire** | Attack | 1 | Fire 1. If a Round hits, apply 1 Weak. | Apply 2 Weak instead. | Weak / Fire |
| 6 | **Ricochet** | Attack | 1 | Fire 1. If a Round hits, deal 4 damage to ALL other enemies. | Splash damage becomes 6. | AoE / Fire |
| 7 | **Pistol Whip** | Attack | 1 | Deal 9 damage. | Deal 12 damage. | Non-Fire fallback |
| 8 | **Shoulder Shot** | Attack | 1 | Deal 7 damage. Gain 4 Block. | Deal 9 damage. Gain 5 Block. | Hybrid |
| 9 | **Gut Shot** | Attack | 1 | Deal 8 damage. If the enemy is Weak, deal 4 additional damage. | Deal 10 damage; bonus becomes 6. | Weak payoff |
| 10 | **Warning Shot** | Attack | 0 | Deal 3 damage. Apply 1 Weak. Exhaust. | Deal 5 damage. | Cheap Weak |
| 11 | **Point Blank** | Attack | 1 | Deal 10 damage. If all 6 chambers are loaded, deal 4 additional damage. | Deal 13 damage; bonus becomes 5. | Full-cylinder |
| 12 | **Fresh Cartridges** | Skill | 1 | Load 2 Lead Rounds and 1 random Round. Gain 2 Block. | Load 2 random Rounds. | Ammo / wildcard |
| 13 | **Quick Load** | Skill | 0 | Load 1-2 more of the last Round you Loaded. Gain 2 Block. Exhaust. | Load 2-3. Exhaust. | Ammo tempo |
| 14 | **Heavy Cartridge** | Skill | 1 | Load 1 Heavy Round. Gain 3 Block. | Gain 5 Block. | Heavy ammo |
| 15 | **Crippling Cartridge** | Skill | 1 | Load 1 Crippling Round. Gain 3 Block. | Gain 5 Block. | Weak ammo |
| 16 | **Take Cover** | Skill | 1 | Gain 7 Block. If the chamber under the hammer is empty, Load 1 Lead Round. | Gain 10 Block. | Block / failsafe ammo |
| 17 | **Duster Up** | Skill | 1 | Gain 3 Block. Gain 1 Armor. Load 1 Guard Round. | Gain 5 Block. Gain 1 Armor. Load 1 Guard Round. | Armor / defensive ammo |
| 18 | **Roll Aside** | Skill | 1 | Gain 5 Block. Cycle 1. If the new chamber is empty, gain 3 more Block and Load 1 Lead Round. | Gain 7 Block; conditional Block becomes 4. | Cycle / Block |
| 19 | **Steady Hand** | Skill | 1 | Gain 5 Deadeye. | Gain 8 Deadeye. | Shot empowerment |
| 20 | **Spin Cylinder** | Skill | 0 | Spin. Draw 1 card. Exhaust. | After Spinning, gain 3 Deadeye. Draw 1 card. Exhaust. | Spin |
| 21 | **Pocket Sand** | Skill | 1 | Apply 2 Weak. | Costs 0. | Weak |
| 22 | **Thumb the Gate** | Skill | 0 | Discard a card. Load 2 Lead Rounds. Exhaust. | Load 3 Lead Rounds. | Discard → ammo |
| 23 | **Take Stock** | Skill | 1 | Draw 2 cards. If the chamber under the hammer is empty, Cycle 1. | Draw 3 cards. | Draw |

## Uncommons — 37
| # | Card | Type | Cost | Base | Upgrade | Role |
|---:|---|---|---:|---|---|---|
| 1 | **Called Shot** | Attack | 1 | Choose a loaded chamber. Move it under the hammer. Fire 1. | Gain 4 Deadeye before firing. | Precision |
| 2 | **Quickdraw** | Attack | 0 | Fire 1. If it Clicks, draw 2 cards. Exhaust. | Draw 3 cards on a Click. | Click payoff |
| 3 | **Double Action** | Attack | 1 | Fire 2. | Gain 3 Deadeye, then Fire 2. | Multi-shot |
| 4 | **Reckless Fire** | Attack | 2 | Fire 2-5. | Fire 3-6. | Multi-shot gamble |
| 5 | **Through the Coat** | Attack | 1 | Fire 1. This shot ignores Block regardless of Round type. | Gain 4 Deadeye before firing. | Piercing |
| 6 | **Kneecapper** | Attack | 1 | Deal 8 damage. If the enemy has no Block, apply 2 Weak. | Deal 11 damage and apply 3 Weak. | Weak |
| 7 | **Pinning Shot** | Attack | 1 | Fire 1. If the target is Weak, apply 1 Debilitate. | Apply 2 Debilitate instead. | Weak / Debilitate |
| 8 | **Crossfire** | Attack | 1 | Fire 1. If a Round hits, deal 5 damage to ALL other enemies. | Splash damage becomes 8. | AoE |
| 9 | **Trick Shot** | Attack | 1 | Spin. Fire 2 at random enemies. Rounds fired by this card deal +2 damage. | Bonus becomes +4. | Spin / Multi-shot |
| 10 | **Run the Cylinder** | Attack | 2 | Fire until a chamber Clicks, up to 6 times. | Rounds fired by this card deal +2 damage. | Loaded-chain |
| 11 | **Empty the Cylinder** | Attack | 3 | Fire 6. Exhaust. | Costs 2. | Full salvo |
| 12 | **Covering Fire** | Attack | 2 | Fire 2. Gain 4 Block for each Round that hits. | Gain 5 Block per hit. | Fire / Block |
| 13 | **Hammerfall** | Attack | 2 | Gain 8 Deadeye. Fire 2. | Gain 12 Deadeye. Fire 2. | Burst |
| 14 | **Showdown** | Attack | 1 | Deal 6 damage. If the enemy intends to Attack, Fire 1. | Deal 9 damage before the conditional Fire. | Intent payoff |
| 15 | **Reversal** | Attack | 1 | If you gained Armor this turn, Fire 2. Otherwise, Fire 1. | Gain 3 Deadeye before firing. | Armor crossover |
| 16 | **Bandolier** | Skill | 1 | Load 1 Lead Round and 1 Crippling Round. Gain 3 Block. | Also Load 1 additional Lead Round. | Mixed ammo |
| 17 | **Speedloader** | Skill | 2 | Fill all empty chambers with Lead Rounds. Gain 5 Block. Exhaust. | Costs 1. | Reload |
| 18 | **Custom Load** | Skill | 1 | Choose Heavy, Crippling, or Guard. Load 1 of that Round and 1 Lead Round. Gain 3 Block. | Load 2 of the chosen Round and 1 Lead Round. | Ammo choice |
| 19 | **Piercing Cartridge** | Skill | 1 | Load 2 Piercing Rounds. Gain 2 Block. | Load 3 Piercing Rounds. | Piercing ammo |
| 20 | **Guard Cartridge** | Skill | 1 | Load 2 Guard Rounds. Gain 3 Block. | Load 3 Guard Rounds. | Guard ammo |
| 21 | **Smoke Cartridge** | Skill | 1 | Load 1 Smoke Round. Gain 4 Block. | Gain 6 Block. | Defensive ammo |
| 22 | **Re-Cock** | Skill | 0 | Cycle 1. Gain 2 Deadeye. | Gain 4 Deadeye. | Cycle |
| 23 | **Check the Cylinder** | Skill | 0 | Cycle up to 2. If the current chamber is loaded, draw 1 card. Exhaust. | Cycle up to 3. | Selection |
| 24 | **Stacked Chamber** | Skill | 1 | The next Round you Load is placed under the hammer. Gain 5 Deadeye. | Gain 8 Deadeye. | Setup |
| 25 | **Under the Duster** | Skill | 1 | Gain 3 Armor. | Gain 4 Armor. | Armor |
| 26 | **Hunker Down** | Skill | 1 | Gain 8 Block. If you have not Fired this turn, gain 4 more Block and Load 1 Lead Round. | Gain 10 Block; conditional Block becomes 5. | Block |
| 27 | **Duck and Weave** | Skill | 2 | Gain 3 Armor, then gain Block equal to twice your Armor. Exhaust. *Gadget.* | Gain 4 Armor. | Armor payoff |
| 28 | **Dive for Cover** | Skill | 1 | If any enemy intends to Attack, gain 9 Block. If total incoming Attack damage is 20 or more, gain 1 Armor. If no enemy intends to Attack, Load 2 Lead Rounds instead. | Gain 12 Block; gain 2 Armor at the threshold. | Intent defense |
| 29 | **Grit Teeth** | Skill | 1 | Lose 2 HP. Gain 10 Block and 2 Armor. | Gain 13 Block and 2 Armor. | Risk defense |
| 30 | **Dead Man's Bluff** | Skill | 1 | Spin. If the current chamber is empty, gain 2 Armor and Load 1 Lead Round; otherwise gain 9 Block. Exhaust. | Loaded result gives 12 Block. | Spin defense |
| 31 | **Cold Read** | Skill | 1 | Apply 1 Weak and 1 Debilitate. Exhaust. | Apply 2 Weak and 1 Debilitate. | Debuff control |
| 32 | **Gunfighter's Rhythm** | Power | 1 | Every 6th Round you Fire, draw 1 card. | Draw 2 cards instead. | Cylinder cadence |
| 33 | **Hard Leather** | Power | 1 | The first time each turn Armor prevents damage, gain 3 Block next turn. | Gain 5 Block next turn. | Armor engine |
| 34 | **Smoke and Lead** | Power | 1 | The first time each turn you Fire a Round, gain 3 Block. | Gain 4 Block. | Fire defense |
| 35 | **Sure Hand** | Power | 1 | The first time each turn you Spin, gain 4 Deadeye. | Gain 6 Deadeye. | Spin engine |
| 36 | **Clear the Chamber** | Attack | 1 | Discard a card. Fire 2. Draw 1 card. | Draw 2 cards. | Discard → filter |
| 37 | **Powder Burn** | Skill | 1 | Exhaust your hand. Load 1 random Round for each card Exhausted. Draw 2 cards. Exhaust. | Draw 3 cards. | Exhaust → ammo |

## Rares — 27
| # | Card | Type | Cost | Base | Upgrade | Role |
|---:|---|---|---:|---|---|---|
| 1 | **High Noon** | Attack | 3 | Gain 3 Deadeye. Fire 6. Exhaust. | Costs 2. | Signature salvo |
| 2 | **One Bullet Left** | Attack | 1 | Fire 1. If it was the only loaded Round before firing, its damage is doubled. | Its damage is tripled instead. | Single-shot burst |
| 3 | **Executioner's Calm** | Attack | 2 | Fire 2. If the target is both Weak and Debilitated, Rounds fired by this card deal 50% more damage. | Bonus becomes 75%. | Debuff finisher |
| 4 | **Long Shot** | Attack | 2 | Gain 3 Deadeye for each empty chamber, then Fire 1. | Gain 4 Deadeye per empty chamber. | Empty-cylinder burst |
| 5 | **Black Powder** | Attack | 1 | Replace the current chamber with a Black Powder Round, then Fire 1. | The Black Powder Round deals 20 instead of 16 damage. | Risk damage |
| 6 | **Last Word** | Attack | 2 | Fire 1. If the target is Weak, Fire 1 again. If the target is Debilitated, Fire 1 again. | Rounds fired by this card deal +2 damage. | Debuff salvo |
| 7 | **No Witnesses** | Attack | 3 | Fire the current loaded Round at ALL enemies, then empty that chamber. Its non-damage effect triggers only once. | Costs 2. | AoE ammo duplication |
| 8 | **Double-Tap** | Attack | 1 | Fire 2. Each Round that hits repeats its damage once; do not repeat its other effect. | Repeat each Round's damage twice instead. | Ammo damage duplication |
| 9 | **Final Chamber** | Attack | 1 | Fire 1. If the Cylinder becomes empty, gain 2 Energy. Exhaust. | Also draw 2 cards if the Cylinder becomes empty. | Empty-cylinder tempo |
| 10 | **Russian Roulette** | Skill | 0 | Load 1 Dead Man's Round into a random empty chamber. Spin, then Self-Fire 1. If it Clicks, gain 1 Energy and draw 2 cards. Exhaust. | The Dead Man's Round deals 30 instead of 24 damage. | Signature gamble |
| 11 | **Stack the Cylinder** | Skill | 1 | Rearrange all chambers in any order and choose the hammer position. Exhaust. | Costs 0. | Perfect control |
| 12 | **Perfect Reload** | Skill | 2 | Choose Lead, Heavy, Crippling, Guard, or Piercing. Fill all empty chambers with that Round. Gain 5 Block. Exhaust. | Costs 1. | Ammo capstone |
| 13 | **Ghost Step** | Skill | 1 | Gain 1 Intangible. Exhaust. | Costs 0. | Defensive capstone |
| 14 | **Armored Longcoat** | Skill | 2 | Gain 5 Armor. Exhaust. | Gain 7 Armor. | Armor capstone |
| 15 | **Never Still** | Skill | 1 | Gain 8 Block. Next turn, gain 1 Energy and draw 1 card. Exhaust. *Gadget.* | Draw 2 cards next turn. | Tempo defense |
| 16 | **Deadeye Focus** | Skill | 1 | Gain 12 Deadeye. Exhaust. | Gain 16 Deadeye. | Shot capstone |
| 17 | **Sixth Sense** | Skill | 1 | Choose a chamber. If loaded, move it under the hammer and draw 2 cards. If empty, gain 1 Armor. Exhaust. | Draw 3 if loaded; if empty, also gain 5 Block. | Precision defense |
| 18 | **Rending Cartridge** | Skill | 1 | Load 2 Rending Rounds. Exhaust. | Load 3 Rending Rounds. | Debilitate ammo |
| 19 | **Lucky Shot** | Skill | 0 | Load 1 random Round. | Load 2 random Rounds. | Free ammo gamble |
| 20 | **Quickdraw Legend** | Power | 2 | The first card you play each turn that Fires costs 1 less. | Costs 1. | Fire tempo |
| 21 | **Bottomless Bandolier** | Power | 2 | At the start of your turn, if there is an empty chamber, Load 1 random special Round: Heavy, Crippling, Piercing, or Guard. | Also gain 2 Deadeye after loading. | Ammo engine |
| 22 | **Loaded Dice** | Power | 1 | After you Spin, you may Cycle 1. | You may Cycle up to 2 instead. | Spin control |
| 23 | **Iron Will** | Power | 2 | The first time each turn Armor would decrease, it does not. | Costs 1. | Armor engine |
| 24 | **Untouchable** | Power | 2 | Whenever you gain Armor, gain 1 Block per stack. | Gain 2 Block per stack instead. | Armor engine |
| 25 | **Debilitating Presence** | Power | 2 | The first time each turn you apply Weak, also apply 1 Debilitate. | Costs 1. | Debuff engine |
| 26 | **Sixth Shot** | Power | 3 | Every 6th Round you Fire deals +15 damage and grants 1 Energy. | Bonus damage becomes +20. | Cylinder capstone |
| 27 | **Dry Fire** | Power | 2 | Whenever a chamber Clicks, Load 1 Lead Round and draw 1 card. | Costs 1. | Empty-cylinder engine |

---

# 8. Character Relics

The normal STS2 class template uses 7 non-starter class relics.

| Rarity | Relic | Effect | Purpose |
|---|---|---|---|
| Common | **Oiled Rag** | The first time each combat you play a card that Loads, also Load 1 random Round. | Early ammo smoothing, and an early taste of the ammunition menu. |
| Uncommon | **Tin Badge** | The first time each turn you apply Weak, gain 3 Block. | Weak/defense bridge. |
| Uncommon | **Spare Speedloader** | The first time each combat the Cylinder becomes completely empty, Load 3-5 Lead Rounds. | Prevents ammo collapse. |
| Rare | **Longcoat Plates** | Start each combat with 3 Armor. | Durable defensive identity. |
| Rare | **Lucky Coin** | The first time each turn you Spin: if the current chamber is loaded, draw 1 card; if empty, gain 4 Block. | Makes Spin productive without removing randomness. |
| Rare | **Engraved Hammer** | The first successful Round you Fire each turn deals +4 damage. | Consistent precision scaling. |
| Shop | **Ivory Handle** | Non-Lead Rounds deal +3 damage. | Premium special-ammo payoff. |

Old Iron, Oiled Rag and Spare Speedloader all roll what they hand you. See **Randomness** in
section 4 for the full list and the two rules that keep it fair.

---

# 9. Character Potions

| Rarity | Potion | Effect |
|---|---|---|
| Common | **Speedloader Flask** | Load 3 Lead Rounds into empty chambers. |
| Uncommon | **Sightline Tonic** | Gain 10 Deadeye. |
| Rare | **Ghost Smoke** | Gain 1 Intangible. |

---

# 10. Multiplayer Cards — 5

These are outside the 82 normal-card pool, matching the current STS2 pattern.

| Card | Type | Cost | Effect | Upgrade |
|---|---|---:|---|---|
| **Covering Partner** | Skill | 1 | ALL players gain 5 Block. Cycle 1. | ALL players gain 7 Block. |
| **Suppressive Volley** | Attack | 2 | Fire 3. Apply 1 Weak to ALL enemies. | Apply 2 Weak. |
| **Hand Me That** | Skill | 1 | Another player draws 2 cards. Load 2 Lead Rounds. | They draw 3 cards. |
| **Softened Up** | Skill | 1 | Apply 1 Debilitate to ALL enemies. Exhaust. | Apply 2 Debilitate. |
| **Ride Together** | Power | 2 | The first time each turn each other player plays an Attack, Load 1 Lead Round. | Also gain 2 Deadeye. |

Multiplayer note:
Debilitate is particularly potent in co-op because teammates can exploit doubled Weak/Vulnerable effects, so Softened Up should be watched closely in four-player balance tests.

---

# 11. Draft Archetypes

## A. High-Caliber / Deadeye
**Core:** Heavy Rounds + Deadeye + precise single shots.

Key cards:
- Heavy Cartridge
- Steady Hand
- Hammerfall
- Called Shot
- Long Shot
- One Bullet Left
- Deadeye Focus

Play pattern:
Prepare one premium chamber, line it up, stack Deadeye, and make one shot matter.

Failure mode:
Too many setup cards and not enough actual Fire cards.

---

## B. Cylinder Burst
**Core:** Keep the gun loaded and cash it out with multi-shot attacks.

Key cards:
- Fan the Hammer
- Double Action
- Speedloader
- Run the Cylinder
- Empty the Cylinder
- High Noon
- Sixth Shot

Play pattern:
Spend one turn loading; spend the next turning ammunition into a burst.

Failure mode:
Drawing salvos into an empty Cylinder.

---

## C. Suppression / Debilitate
**Core:** Weak enemies, then amplify Weak with Debilitate.

Key cards:
- Suppressing Fire
- Crippling Cartridge
- Pocket Sand
- Pinning Shot
- Cold Read
- Rending Cartridge
- Debilitating Presence
- Executioner's Calm
- Last Word

Play pattern:
Turn incoming damage down, then use the debuffed target as a damage-condition payoff.

Failure mode:
Against enemies that do not attack often, the deck can lack raw damage if it over-drafts control.

---

## D. Armored Drifter
**Core:** Block for the current turn, Armor for future hits, and Guard Rounds for hybrid turns.

Key cards:
- Duster Up
- Guard Cartridge
- Under the Duster
- Grit Teeth
- Hard Leather
- Armored Longcoat
- Iron Will
- Untouchable

Play pattern:
Layer Block over a small Armor reserve and let Armor shave off damage that leaks through.

Failure mode:
Fast multi-hit enemies erode Armor quickly.

---

## E. Gadgets
**Core:** Debuffs, Block and Armor from cards that never touch the gun.

Key cards:
- Pocket Sand, Blinding Powder, Tripwire, Bear Trap
- Smoke Bomb, Field Kit, Scattergun Shell
- Under the Duster, Grit Teeth, Duck and Weave
- Tinker's Kit, Gadgeteer

Play pattern:
Weak the room, stack Armor, and let Gadgeteer turn a hand of cheap debuffs into a wall. The gun is
a damage option, not a requirement — the deck functions on an empty cylinder.

Failure mode:
Low ceiling on damage. Without Scattergun Shell or a few Rounds to Fire, the deck out-defends
every fight and then cannot close one.

---

## F. Empty-Chamber Tempo
**Core:** Intentionally run the gun dry, then profit.

Key cards:
- Last Round
- Quickdraw
- Long Shot
- Final Chamber
- Spare Speedloader
- Speedloader
- Lucky Shot

Play pattern:
Empty the Cylinder on purpose, collect damage/Energy/draw payoffs, then reload quickly.

Failure mode:
If the reload half of the deck does not appear, payoff cards become stranded.

---

## G. Gambler / Spin
**Core:** Spin for cheap value, then progressively turn randomness into control.

Key cards:
- Spin Cylinder
- Trick Shot
- Dead Man's Bluff
- Sure Hand
- Loaded Dice
- Lucky Coin
- Russian Roulette

Play pattern:
Early-game Spin is volatile; late-game Spin becomes a trigger engine with correction tools.

Failure mode:
Should never become "Spin until you win" through zero-cost infinite loops.

---

# 12. Signature Card: Russian Roulette

## Russian Roulette
**0 Energy — Rare Skill — Exhaust**

1. Load 1 **Dead Man's Round** into a random empty chamber.
2. Spin.
3. Self-Fire 1.
4. If it Clicks, gain 1 Energy and draw 2 cards.

**Dead Man's Round:** 24 damage.  
**Upgraded Russian Roulette:** Dead Man's Round deals 30.

### Why this version works
With an otherwise empty Cylinder:
- 1 chamber is lethal ammunition.
- 5 chambers are empty.
- The player has a 1-in-6 chance to lose 24 HP.
- On a Click, the Dead Man's Round remains somewhere in the Cylinder and can later be fired at an enemy.

This creates three decisions:
1. Do I empty my gun before gambling?
2. Can I survive the bad result?
3. If I survive the gamble, can I line the Dead Man's Round up efficiently afterward?

Importantly, other loaded Rounds make the gamble worse: Self-Fire can hit those Rounds too, and only a Click grants the reward.

---

# 13. Balance Guardrails

## 13.1 Damage
Target baselines for initial testing:
- Generic 1-Energy direct Attack: ~8–10 damage.
- Attack with meaningful utility: ~6–8 damage plus effect.
- Lead-based Fire should be efficient only after accounting for the card/energy spent Loading.
- Heavy ammunition is allowed to look "over-rate" because it requires setup and a Fire action.
- Fire 6 effects should generally cost 3, Exhaust, or require substantial setup.

## 13.2 Block
Initial targets:
- Common 1-Energy pure Block: 7–10.
- Hybrid block/ammo cards: ~3–6 Block plus setup value.
- Armor should not replace Block; most decks still need ordinary Block cards.

### Block/ammo exchange rate (balance pass, 2026-08-30)
The character had a clean split it did not want: reload cards did nothing for the turn they were
played, and Block cards did nothing for the gun. That is two dead halves of a deck, and it is the
same "the Gunslinger feels weak" complaint from the other side — the turn you reload is the turn
you get hit, and the turn you Block is the turn the cylinder runs dry.

Both sides now cross over, priced off the Cartridge commons that already did it (Heavy Cartridge
is 1 Energy for 1 Heavy Round and 3 Block):

| Rider | Costs |
|---|---|
| Load 1 Lead Round on a Block card | 2 Block |
| Load 1 specialist Round on a Block card | 3 Block |
| A Load on a branch that is already conditional | free |
| Block on a card that only Loads | 2–3 at 1 Energy, 5 at 2 Energy |

Two rules keep this from flattening the deck:

**A rider never replaces the card's job.** Take Cover keeps the full 7 Block and only Loads when
the chamber under the hammer is already empty; Guard Cartridge still Loads 2 Guard Rounds and the
3 Block does not buy a Round back. The Block on a reload card is deliberately under a Defend, so
these are reloads that cover you, not Defends that also load.

**Conditional Loads are free because they are self-limiting.** Roll Aside, Hunker Down, Dead Man's
Bluff and Take Cover all Load on a branch that requires an empty chamber or a held trigger, which
means the Load can never overwrite ammunition the player was about to Fire. Dive for Cover is the
clearest case: its Load lives in the branch where the card used to do nothing at all, so it raises
the floor without touching the ceiling.

Left alone deliberately: **Grit Teeth** (already the densest defensive uncommon, and paying for a
Round in HP changes what the card is), **Ghost Step** (Intangible is premium and should not
accumulate riders), and **Rending Cartridge** / **Lucky Shot** (a Rare debuff engine and a 0-cost
gamble, both already at their ceiling).

## 13.3 Armor
Treat 1 Armor as approximately:
- Weak in a single-hit hallway fight.
- Strong in a 2–4 turn fight.
- Highly variable against multi-hit patterns.

Watch for:
- Iron Will + large Armor stacks becoming effectively permanent.
- Armor making Frail irrelevant.
- Armor exceeding 6–8 too easily without Rare cards.

If Armor is too strong, adjust card values before changing the keyword.

**Implementation note — why Armor used to evaporate.** Armor reduces damage from a Harmony
postfix on `Hook.ModifyDamage`, which is the only hook that sees an incoming hit before it
lands. That hook answers the question "how big would this hit be", and the game asks it more than
once per hit — the intent forecast above each enemy asks it too. The first implementation spent a
stack of Armor every time it was asked, so a stack drained to nothing before anything had swung,
which read in play as "Armor wears off immediately".

The reduction and the spend are now separate. The patch only ever reduces, which makes it
idempotent — the property the hook actually requires — and raises a pending flag on the power.
`ArmorPower.BeforeDamageReceived` spends that flag; that hook runs once, for damage that is
really being dealt. `Hard Leather` is announced from the same
place, so it can no longer be triggered by a forecast redraw either.

## 13.4 Intangible
Intangible is intentionally scarce — see §3.4 for the full rule.

Guardrails:
- Rare only, one stack, and never from an engine, relic or Round.
- Both sources Exhaust or are consumed, so it can never be looped.
- If it proves too strong, the fix is removing a source, not shaving the stack.

## 13.5 Weak / Debilitate
Weak is a major class strength.

Guardrails:
- The Gunslinger should not also have broad native Vulnerable.
- Repeatable Debilitate should be rare.
- Applying Weak + Debilitate in one card should normally Exhaust or be a Rare engine.
- Multiplayer Debilitate needs separate testing because allies may bring Vulnerable.

## 13.6 Deadeye
Deadeye buffs every Round Fired this turn and expires at end of turn.

Guardrails:
- Common Deadeye: 5–8 for 1 Energy.
- Rare Deadeye: 12–16 with Exhaust.
- Deadeye cannot be banked across turns, so every grant has to be spendable the turn it is played.
- The numbers above pre-date the rule change and have not been retuned — see the balance note in
  3.2. Deadeye now multiplies with shot count, so a grant is worth roughly `X × shots this turn`.

---

# 14. Suggested Unlock / Epoch Structure

To feel like a native STS2 character, gate the more complex cards and relics behind progression.

### Epoch I — First Blood (Beat Act 1)
- Pinning Shot
- Under the Duster
- High Noon

### Epoch II — Old Tools (Beat Act 2)
- Oiled Rag
- Tin Badge
- Longcoat Plates

### Epoch III — Road Medicine (Beat Act 3)
- Speedloader Flask
- Sightline Tonic
- Ghost Smoke

### Epoch IV — The Face Remembered (Beat Ascension 1)
- Rending Cartridge
- Loaded Dice
- Long Shot

### Epoch V — No Safe Road (Kill 15 Elites)
- Russian Roulette
- Ghost Step
- Sixth Shot

### Epoch VI — The Last Horizon (Kill 15 Bosses)
- Spare Speedloader
- Lucky Coin
- Engraved Hammer

**Ivory Handle** is the always-available class Shop relic, matching the role of other class-specific Shop relics.

---

# 15. Playtest Plan

## Phase 1 — Starter Deck
Run 20–30 Act 1 starts.

Track:
- Damage taken in first 3 hallway fights.
- How often Quick Draw is dead on draw.
- How often Reload feels mandatory rather than interesting.
- Average loaded chambers at end of turn.
- Whether Old Iron should load 2, 3, or 4 Lead.

Success target:
The Gunslinger should not feel weaker than Silent/Regent in Act 1 merely because the player has not found ammo cards.

## Phase 2 — Card Economy
Test draft buckets separately:
1. Mostly Lead/Fire.
2. Heavy + Deadeye.
3. Weak/Debilitate.
4. Armor.
5. Gadgets (no-cylinder).
6. Empty-cylinder.
7. Spin/gamble.

Look for:
- "Parasitic" cards that require too many other cards to function.
- Infinite loops involving 0-cost Spin/Cycle/draw.
- Fire 6 turns gaining too much from Strength.
- Gadgeteer plus zero-cost Gadgets producing Armor faster than a fight can erode it.

## Phase 3 — Defense Stress Tests
Fight patterns:
- One giant hit.
- Many small hits.
- Two-enemy mixed intents.
- Non-Attack HP loss.
- Frail.
- Vulnerable.
- Artifact-heavy enemies.

Desired outcome:
- Block is the reliable baseline.
- Armor is a medium-term efficiency layer.
- Intangible is the rare, complete answer to one enemy turn.
- No one defensive mechanic should solve all three attack profiles.

## Phase 4 — Russian Roulette
Track at least 100 uses.

Questions:
- Is 24 self-damage enough to make the decision meaningful?
- Is 1 Energy + 2 draw too much or too little on a Click?
- Does the retained Dead Man's Round make successful gambles too rewarding?
- Does the card become trivial once Loaded Dice is in play?

Important rule:
**Loaded Dice should not be allowed to intervene during Russian Roulette's immediate Spin → Self-Fire resolution.**

## Phase 5 — High Ascension
Primary watchlist:
- Weak + Debilitate trivializing bosses.
- Armor invalidating chip damage.
- Intangible invalidating single-turn boss burst.
- Perfect Reload + High Noon becoming a one-card two-turn kill package.
- Sixth Shot creating Energy-positive loops.
- Quickdraw Legend making 3-cost salvo cards effectively free too often.

---

# 16. Tuning Knobs

If the class is too strong:
1. Lead 6 → 5.
2. Old Iron starts with 2 Lead instead of 3.
3. Gadgeteer grants Block instead of Armor.
4. Armor cards lose 1 stack across the board.
5. High Noon remains 3 Energy when upgraded; upgrade adds damage instead.
6. Dead Man's Round 24 → 20.
7. Debilitating Presence becomes once per combat rather than once per turn.

If the class is too weak:
1. Old Iron starts with 4 Lead.
2. Lead stays 6 but Fresh Cartridges loads 3 by default.
3. Deadeye persists through a Click (already planned).
4. Guard Round 4 Block → 5.
5. Armor cards gain +1 Block alongside Armor.
6. Quick Draw+ Fires 2-4 instead of 2-3.

---

# 17. Design Rules for Future Cards

When adding/replacing cards, preserve these rules:

1. **The Cylinder must matter.**  
   Avoid creating too many generic attacks that ignore the gun.

2. **The gun must not be mandatory every hand.**  
   Keep enough direct attacks and ordinary Block that bad draw order is playable.

3. **Randomness needs correction tools.**  
   Spin is fun because Cycle, Called Shot, Stack the Cylinder, and Loaded Dice exist.

4. **Intangible must stay Rare and singular.**  
   Two sources, one stack each. Nothing repeatable ever grants it.

5. **Armor is not Plating.**  
   Armor reduces leaking HP damage; Plating creates Block.

6. **Weak is a core identity; Vulnerable is not.**

7. **The strongest multi-shot cards need preparation.**

8. **Risk cards should create interesting board states even when they do not pay out.**  
   Russian Roulette leaving a Dead Man's Round in the gun after a Click is the model.

9. **Every archetype should share at least two cards with another archetype.**  
   This prevents narrow "ammo-color decks."

10. **Never make Spin the only correct choice.**  
    Deterministic chamber control should remain available at an Energy/card premium.

---

# 18. Recommended First Prototype Pool

Do not implement all 80 cards first.

Prototype these 24 cards to prove the character:

### Starter
- Reload
- Quick Draw

### Commons
- Snap Shot
- Fan the Hammer
- Suppressing Fire
- Pistol Whip
- Fresh Cartridges
- Heavy Cartridge
- Crippling Cartridge
- Duster Up
- Steady Hand
- Spin Cylinder

### Uncommons
- Called Shot
- Pinning Shot
- Speedloader
- Guard Cartridge
- Smoke Cartridge
- Under the Duster
- Duck and Weave
- Cold Read

### Rares
- High Noon
- Russian Roulette
- Ghost Step
- Loaded Dice

Test only:
- Lead
- Heavy
- Crippling
- Guard
- Smoke
- Dead Man's Round

Once that loop feels good, add Piercing, Rending, full Armor engines, and the complete Rare pool.

---

# 19. One-Sentence Pitch for the Character Select Screen

**"A road-worn gunslinger who loads fate six chambers at a time."**
