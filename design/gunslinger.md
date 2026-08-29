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
- Layered defense: Block + Armor + short-lived Dodge.
- Excellent tactical control over single large enemy hits.
- Strong "burst turn" potential through Deadeye and multi-shot cards.

### Core Weaknesses
- Ammo setup costs cards and Energy.
- Multi-shot cards are poor when the Cylinder is empty.
- Dodge expires quickly and cannot be banked.
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

### Load
**Load [Round]** places the Round into the first empty chamber clockwise, beginning at the hammer.

If every chamber is full, Loading remains legal:
- Replace chambers beginning at the hammer and moving clockwise.
- Replaced ammunition is discarded.
- Loading does not move the hammer.

This rule prevents ammo cards from becoming unplayable while still making over-loading a meaningful sequencing cost.

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
**Deadeye X:** The next Round you successfully Fire deals **X additional Attack damage**, then all Deadeye is removed.

Rules:
- A Click does not consume Deadeye.
- Deadeye applies to only the first successful Round in a multi-shot Fire action.
- Deadeye is additive, similar in practical damage ordering to Vigor, but only works on Rounds.
- This prevents Deadeye from scaling every hit of Fire 6 while still rewarding chamber planning.

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

## 3.4 Dodge
**Dodge X:** Prevent all damage from the next X individual enemy Attack hits this turn.

At the start of your next turn, remove all remaining Dodge.

Important:
- Dodge does **not** persist between turns.
- Dodge stops one hit, not an entire multi-hit intent.
- It does not prevent non-Attack HP loss.
- This makes it a tactical "read the intent" defense rather than permanent Intangible/Buffer.

Balance rule:
- No unconditional Common card grants Dodge.
- Repeatable Dodge should require rare engines, special ammo, or significant Energy.

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
- Is not reduced by Block, Armor, or Dodge unless future testing explicitly changes that rule.

This creates a clean Russian Roulette ruleset.

---

# 4. Ammunition

| Round | Printed effect when Fired at an enemy | Intended niche |
|---|---|---|
| **Lead Round** | Deal 6 damage. | Baseline ammunition. |
| **Heavy Round** | Deal 10 damage. | Pure damage / Deadeye target. |
| **Crippling Round** | Deal 4 damage. Apply 1 Weak. | Control. |
| **Piercing Round** | Deal 7 damage. This Round ignores Block. | Anti-Block. |
| **Guard Round** | Deal 4 damage. Gain 4 Block. | Hybrid defense. |
| **Smoke Round** | Deal 2 damage. Gain 1 Dodge. | Premium tactical defense. |
| **Rending Round** | Deal 5 damage. Apply 1 Debilitate. | Rare debuff ammunition. |
| **Black Powder Round** | Deal 16 damage. After firing it at an enemy, lose 3 HP. | Rare risk damage. |
| **Dead Man's Round** | Deal 24 damage. | Russian Roulette payload. |

### Ammo Philosophy
Lead, Heavy, and Crippling appear early.  
Piercing, Guard, and Smoke appear primarily at Uncommon.  
Rending, Black Powder, and Dead Man's Rounds are Rare/special.

This keeps Act 1 readable and stops the Cylinder UI from presenting nine mechanics immediately.

---

# 5. Starting Loadout

## Starting Deck — 10 cards
- Strike ×4
- Defend ×4
- **Reload** ×1
- **Quick Draw** ×1

### Reload
**1 Energy — Skill**  
Load 2 Lead Rounds.

**Reload+**  
Load 3 Lead Rounds.

### Quick Draw
**0 Energy — Attack**  
Fire 1.

**Quick Draw+**  
Gain 2 Deadeye. Fire 1.

The starter deck teaches:
1. Ammo exists.
2. Firing consumes it.
3. A zero-cost Fire card is only good if you prepared ammunition.
4. Ordinary Strikes remain available when the gun is empty.

---

# 6. Starter Relic and Ancient Upgrade

## Old Iron — Starter Relic
At the start of each combat, **Load 3 Lead Rounds**.

Purpose:
- Guarantees Quick Draw is functional in the opening hand.
- Lets the player experience the gun immediately.
- Does not remove the need to draft or play Loading cards.

## True Iron — Ancient Starter Upgrade
Replaces Old Iron.

At the start of each combat, **fill all 6 chambers with Lead Rounds**.  
**Lead Rounds deal 1 additional damage.**

Balance rationale:
- Comparable in spirit to current STS2 starter upgrades that significantly amplify the character's native resource engine.
- Six prepared Rounds are powerful, but still require Fire cards to convert into damage.
- The +1 Lead damage is intentionally narrow so special ammo remains desirable.

---

# 7. Complete 80-Card Pool

## Commons — 20
| # | Card | Type | Cost | Base | Upgrade | Role |
|---:|---|---|---:|---|---|---|
| 1 | **Snap Shot** | Attack | 1 | Fire 1. Draw 1 card. | Gain 2 Deadeye, then Fire 1. Draw 1 card. | Fire / draw |
| 2 | **Fan the Hammer** | Attack | 2 | Fire 3. | Fire 4. | Multi-shot |
| 3 | **Last Round** | Attack | 1 | Fire 1. If the Cylinder is empty afterward, deal 6 additional damage. | Additional damage becomes 9. | Empty-cylinder |
| 4 | **Suppressing Fire** | Attack | 1 | Fire 1. If a Round hits, apply 1 Weak. | Apply 2 Weak instead. | Weak / Fire |
| 5 | **Ricochet** | Attack | 1 | Fire 1. If a Round hits, deal 4 damage to ALL other enemies. | Splash damage becomes 6. | AoE / Fire |
| 6 | **Pistol Whip** | Attack | 1 | Deal 9 damage. | Deal 12 damage. | Non-Fire fallback |
| 7 | **Shoulder Shot** | Attack | 1 | Deal 7 damage. Gain 4 Block. | Deal 9 damage. Gain 5 Block. | Hybrid |
| 8 | **Gut Shot** | Attack | 1 | Deal 8 damage. If the enemy is Weak, deal 4 additional damage. | Deal 10 damage; bonus becomes 6. | Weak payoff |
| 9 | **Warning Shot** | Attack | 0 | Deal 3 damage. Apply 1 Weak. Exhaust. | Deal 5 damage. | Cheap Weak |
| 10 | **Point Blank** | Attack | 1 | Deal 10 damage. If all 6 chambers are loaded, deal 4 additional damage. | Deal 13 damage; bonus becomes 5. | Full-cylinder |
| 11 | **Fresh Cartridges** | Skill | 1 | Load 2 Lead Rounds. | Load 3 Lead Rounds. | Ammo |
| 12 | **Quick Load** | Skill | 0 | Load 1 Lead Round. Exhaust. | Load 2 Lead Rounds. Exhaust. | Ammo tempo |
| 13 | **Heavy Cartridge** | Skill | 1 | Load 1 Heavy Round. Gain 3 Block. | Gain 5 Block. | Heavy ammo |
| 14 | **Crippling Cartridge** | Skill | 1 | Load 1 Crippling Round. Gain 3 Block. | Gain 5 Block. | Weak ammo |
| 15 | **Take Cover** | Skill | 1 | Gain 7 Block. | Gain 10 Block. | Block |
| 16 | **Duster Up** | Skill | 1 | Gain 5 Block. Gain 1 Armor. | Gain 7 Block. Gain 1 Armor. | Armor |
| 17 | **Roll Aside** | Skill | 1 | Gain 5 Block. Cycle 1. If the new chamber is empty, gain 3 more Block. | Gain 7 Block; conditional Block becomes 4. | Cycle / Block |
| 18 | **Steady Hand** | Skill | 1 | Gain 5 Deadeye. | Gain 8 Deadeye. | Shot empowerment |
| 19 | **Spin Cylinder** | Skill | 0 | Spin. Draw 1 card. Exhaust. | After Spinning, gain 3 Deadeye. Draw 1 card. Exhaust. | Spin |
| 20 | **Pocket Sand** | Skill | 1 | Apply 2 Weak. | Apply 3 Weak. | Weak |

## Uncommons — 35
| # | Card | Type | Cost | Base | Upgrade | Role |
|---:|---|---|---:|---|---|---|
| 1 | **Called Shot** | Attack | 1 | Choose a loaded chamber. Move it under the hammer. Fire 1. | Gain 4 Deadeye before firing. | Precision |
| 2 | **Quickdraw** | Attack | 0 | Fire 1. If it Clicks, draw 2 cards. Exhaust. | Draw 3 cards on a Click. | Click payoff |
| 3 | **Double Action** | Attack | 1 | Fire 2. | Gain 3 Deadeye, then Fire 2. | Multi-shot |
| 4 | **Through the Coat** | Attack | 1 | Fire 1. This shot ignores Block regardless of Round type. | Gain 4 Deadeye before firing. | Piercing |
| 5 | **Kneecapper** | Attack | 1 | Deal 8 damage. If the enemy has no Block, apply 2 Weak. | Deal 11 damage and apply 3 Weak. | Weak |
| 6 | **Pinning Shot** | Attack | 1 | Fire 1. If the target is Weak, apply 1 Debilitate. | Apply 2 Debilitate instead. | Weak / Debilitate |
| 7 | **Crossfire** | Attack | 1 | Fire 1. If a Round hits, deal 5 damage to ALL other enemies. | Splash damage becomes 8. | AoE |
| 8 | **Trick Shot** | Attack | 1 | Spin. Fire 2 at random enemies. Rounds fired by this card deal +2 damage. | Bonus becomes +4. | Spin / Multi-shot |
| 9 | **Run the Cylinder** | Attack | 2 | Fire until a chamber Clicks, up to 6 times. | Rounds fired by this card deal +2 damage. | Loaded-chain |
| 10 | **Empty the Cylinder** | Attack | 3 | Fire 6. Exhaust. | Costs 2. | Full salvo |
| 11 | **Covering Fire** | Attack | 2 | Fire 2. Gain 4 Block for each Round that hits. | Gain 5 Block per hit. | Fire / Block |
| 12 | **Lead Storm** | Attack | 2 | Load 2 Lead Rounds, then Fire 2. | Load 3 Lead Rounds, then Fire 3. | Self-contained Fire |
| 13 | **Hammerfall** | Attack | 2 | Gain 8 Deadeye. Fire 2. | Gain 12 Deadeye. Fire 2. | Burst |
| 14 | **Showdown** | Attack | 1 | Deal 6 damage. If the enemy intends to Attack, Fire 1. | Deal 9 damage before the conditional Fire. | Intent payoff |
| 15 | **Reversal** | Attack | 1 | If you gained Armor this turn, Fire 2. Otherwise, Fire 1. | Gain 3 Deadeye before firing. | Armor crossover |
| 16 | **Bandolier** | Skill | 1 | Load 1 Lead Round and 1 Crippling Round. | Also Load 1 additional Lead Round. | Mixed ammo |
| 17 | **Speedloader** | Skill | 2 | Fill all empty chambers with Lead Rounds. Exhaust. | Costs 1. | Reload |
| 18 | **Custom Load** | Skill | 1 | Choose Heavy, Crippling, or Guard. Load 1 of that Round and 1 Lead Round. | Load 2 of the chosen Round and 1 Lead Round. | Ammo choice |
| 19 | **Piercing Cartridge** | Skill | 1 | Load 2 Piercing Rounds. | Load 3 Piercing Rounds. | Piercing ammo |
| 20 | **Guard Cartridge** | Skill | 1 | Load 2 Guard Rounds. | Load 3 Guard Rounds. | Guard ammo |
| 21 | **Smoke Cartridge** | Skill | 1 | Load 1 Smoke Round. Gain 4 Block. | Gain 6 Block. | Dodge ammo |
| 22 | **Re-Cock** | Skill | 0 | Cycle 1. Gain 2 Deadeye. | Gain 4 Deadeye. | Cycle |
| 23 | **Check the Cylinder** | Skill | 0 | Cycle up to 2. If the current chamber is loaded, draw 1 card. Exhaust. | Cycle up to 3. | Selection |
| 24 | **Stacked Chamber** | Skill | 1 | The next Round you Load is placed under the hammer. Gain 5 Deadeye. | Gain 8 Deadeye. | Setup |
| 25 | **Under the Duster** | Skill | 1 | Gain 3 Armor. | Gain 4 Armor. | Armor |
| 26 | **Hunker Down** | Skill | 1 | Gain 8 Block. If you have not Fired this turn, gain 4 more Block. | Gain 10 Block; conditional Block becomes 5. | Block |
| 27 | **Duck and Weave** | Skill | 2 | Gain 1 Dodge. Exhaust. | Also gain 6 Block. | Dodge |
| 28 | **Dive for Cover** | Skill | 1 | If any enemy intends to Attack, gain 9 Block. If total incoming Attack damage is 20 or more, gain 1 Armor. | Gain 12 Block; gain 2 Armor at the threshold. | Intent defense |
| 29 | **Grit Teeth** | Skill | 1 | Lose 2 HP. Gain 10 Block and 2 Armor. | Gain 13 Block and 2 Armor. | Risk defense |
| 30 | **Dead Man's Bluff** | Skill | 1 | Spin. If the current chamber is empty, gain 1 Dodge; otherwise gain 9 Block. Exhaust. | Loaded result gives 12 Block. | Spin defense |
| 31 | **Cold Read** | Skill | 1 | Apply 1 Weak and 1 Debilitate. Exhaust. | Apply 2 Weak and 1 Debilitate. | Debuff control |
| 32 | **Gunfighter's Rhythm** | Power | 1 | Every 6th Round you Fire, draw 1 card. | Draw 2 cards instead. | Cylinder cadence |
| 33 | **Hard Leather** | Power | 1 | The first time each turn Armor prevents damage, gain 3 Block next turn. | Gain 5 Block next turn. | Armor engine |
| 34 | **Smoke and Lead** | Power | 1 | The first time each turn you Fire a Round, gain 3 Block. | Gain 4 Block. | Fire defense |
| 35 | **Sure Hand** | Power | 1 | The first time each turn you Spin, gain 4 Deadeye. | Gain 6 Deadeye. | Spin engine |

## Rares — 25
| # | Card | Type | Cost | Base | Upgrade | Role |
|---:|---|---|---:|---|---|---|
| 1 | **High Noon** | Attack | 3 | Gain 3 Deadeye. Fire 6. Exhaust. | Costs 2. | Signature salvo |
| 2 | **One Bullet Left** | Attack | 1 | Fire 1. If it was the only loaded Round before firing, its damage is doubled. | Its damage is tripled instead. | Single-shot burst |
| 3 | **Executioner's Calm** | Attack | 2 | Fire 2. If the target is both Weak and Debilitated, Rounds fired by this card deal 50% more damage. | Bonus becomes 75%. | Debuff finisher |
| 4 | **Long Shot** | Attack | 2 | Gain 3 Deadeye for each empty chamber, then Fire 1. | Gain 4 Deadeye per empty chamber. | Empty-cylinder burst |
| 5 | **Black Powder** | Attack | 1 | Replace the current chamber with a Black Powder Round, then Fire 1. | The Black Powder Round deals 20 instead of 16 damage. | Risk damage |
| 6 | **Last Word** | Attack | 2 | Fire 1. If the target is Weak, Fire 1 again. If the target is Debilitated, Fire 1 again. | Rounds fired by this card deal +2 damage. | Debuff salvo |
| 7 | **No Witnesses** | Attack | 3 | Fire the current loaded Round at ALL enemies, then empty that chamber. Its non-damage effect triggers only once. | Costs 2. | AoE ammo duplication |
| 8 | **Double-Tap** | Attack | 1 | Fire 1. If it hits, repeat that Round's damage once; do not repeat its other effect. | Repeat its damage twice instead. | Ammo damage duplication |
| 9 | **Final Chamber** | Attack | 1 | Fire 1. If the Cylinder becomes empty, gain 2 Energy. Exhaust. | Also draw 2 cards if the Cylinder becomes empty. | Empty-cylinder tempo |
| 10 | **Russian Roulette** | Skill | 0 | Load 1 Dead Man's Round into a random empty chamber. Spin, then Self-Fire 1. If it Clicks, gain 1 Energy and draw 2 cards. Exhaust. | The Dead Man's Round deals 30 instead of 24 damage. | Signature gamble |
| 11 | **Stack the Cylinder** | Skill | 1 | Rearrange all chambers in any order and choose the hammer position. Exhaust. | Costs 0. | Perfect control |
| 12 | **Perfect Reload** | Skill | 2 | Choose Lead, Heavy, Crippling, Guard, or Piercing. Fill all empty chambers with that Round. Exhaust. | Costs 1. | Ammo capstone |
| 13 | **Ghost Step** | Skill | 2 | Gain 2 Dodge. Exhaust. | Costs 1. | Dodge capstone |
| 14 | **Armored Longcoat** | Skill | 2 | Gain 5 Armor. Exhaust. | Gain 7 Armor. | Armor capstone |
| 15 | **Never Still** | Skill | 1 | Gain 1 Dodge. Next turn, gain 1 Energy and draw 1 card. Exhaust. | Draw 2 cards next turn. | Tempo defense |
| 16 | **Deadeye Focus** | Skill | 1 | Gain 12 Deadeye. Exhaust. | Gain 16 Deadeye. | Shot capstone |
| 17 | **Sixth Sense** | Skill | 1 | Choose a chamber. If loaded, move it under the hammer and draw 2 cards. If empty, gain 1 Dodge. Exhaust. | Draw 3 if loaded; if empty, also gain 5 Block. | Precision defense |
| 18 | **Rending Cartridge** | Skill | 1 | Load 2 Rending Rounds. Exhaust. | Load 3 Rending Rounds. | Debilitate ammo |
| 19 | **Quickdraw Legend** | Power | 2 | The first card you play each turn that Fires costs 1 less. | Costs 1. | Fire tempo |
| 20 | **Bottomless Bandolier** | Power | 2 | At the start of your turn, if there is an empty chamber, Load 1 random special Round: Heavy, Crippling, Piercing, or Guard. | Also gain 2 Deadeye after loading. | Ammo engine |
| 21 | **Loaded Dice** | Power | 1 | After you Spin, you may Cycle 1. | You may Cycle up to 2 instead. | Spin control |
| 22 | **Iron Will** | Power | 2 | The first time each turn Armor would decrease, it does not. | Costs 1. | Armor engine |
| 23 | **Untouchable** | Power | 2 | Whenever you gain Dodge, gain 6 Block. | Gain 8 Block instead. | Dodge engine |
| 24 | **Debilitating Presence** | Power | 2 | The first time each turn you apply Weak, also apply 1 Debilitate. | Costs 1. | Debuff engine |
| 25 | **Sixth Shot** | Power | 3 | Every 6th Round you Fire deals +15 damage and grants 1 Energy. | Bonus damage becomes +20. | Cylinder capstone |

---

# 8. Character Relics

The normal STS2 class template uses 7 non-starter class relics.

| Rarity | Relic | Effect | Purpose |
|---|---|---|---|
| Common | **Oiled Rag** | The first time each combat you play a card that Loads, also Load 1 Lead Round. | Early ammo smoothing. |
| Uncommon | **Tin Badge** | The first time each turn you apply Weak, gain 3 Block. | Weak/defense bridge. |
| Uncommon | **Spare Speedloader** | The first time each combat the Cylinder becomes completely empty, Load 3 Lead Rounds. | Prevents ammo collapse. |
| Rare | **Longcoat Plates** | Start each combat with 3 Armor. | Durable defensive identity. |
| Rare | **Lucky Coin** | The first time each turn you Spin: if the current chamber is loaded, draw 1 card; if empty, gain 4 Block. | Makes Spin productive without removing randomness. |
| Rare | **Engraved Hammer** | The first successful Round you Fire each turn deals +4 damage. | Consistent precision scaling. |
| Shop | **Ivory Handle** | Non-Lead Rounds deal +3 damage. | Premium special-ammo payoff. |

---

# 9. Character Potions

| Rarity | Potion | Effect |
|---|---|---|
| Common | **Speedloader Flask** | Load 3 Lead Rounds into empty chambers. |
| Uncommon | **Sightline Tonic** | Gain 10 Deadeye. |
| Rare | **Ghost Smoke** | Gain 2 Dodge this turn. |

---

# 10. Multiplayer Cards — 5

These are outside the 80 normal-card pool, matching the current STS2 pattern.

| Card | Type | Cost | Effect | Upgrade |
|---|---|---:|---|---|
| **Covering Partner** | Skill | 1 | ALL players gain 5 Block. Cycle 1. | ALL players gain 7 Block. |
| **Suppressive Volley** | Attack | 2 | Fire 2. Apply 1 Weak to ALL enemies. | Apply 2 Weak. |
| **Hand Me That** | Skill | 1 | Another player draws 2 cards. Load 2 Lead Rounds. | They draw 3 cards. |
| **Softened Up** | Skill | 1 | Apply 1 Debilitate to ALL enemies. Exhaust. | Apply 2 Debilitate. |
| **Stand Together** | Skill | 2 | ALL players gain 8 Block. You gain 1 Dodge. | ALL players gain 11 Block. |

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

Play pattern:
Layer Block over a small Armor reserve and let Armor shave off damage that leaks through.

Failure mode:
Fast multi-hit enemies erode Armor quickly.

---

## E. Smoke / Evasion
**Core:** Smoke Rounds + Dodge + intent-reading.

Key cards:
- Smoke Cartridge
- Duck and Weave
- Dead Man's Bluff
- Ghost Step
- Never Still
- Untouchable

Play pattern:
Use Dodge on the exact turn it matters; do not try to bank it.

Failure mode:
Dodge is inefficient against many small hits and useless against non-Attack HP loss.

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

## 13.4 Dodge
Dodge is intentionally premium.

Guardrails:
- No repeatable unconditional Common Dodge.
- 1 Dodge at Uncommon generally costs 2 Energy, Exhausts, or is conditional.
- 2 Dodge belongs at Rare and usually Exhausts.
- Dodge expires at the start of the Gunslinger's turn, so it cannot be stockpiled.

## 13.5 Weak / Debilitate
Weak is a major class strength.

Guardrails:
- The Gunslinger should not also have broad native Vulnerable.
- Repeatable Debilitate should be rare.
- Applying Weak + Debilitate in one card should normally Exhaust or be a Rare engine.
- Multiplayer Debilitate needs separate testing because allies may bring Vulnerable.

## 13.6 Deadeye
Deadeye only buffs one successful Round.

Guardrails:
- Common Deadeye: 5–8 for 1 Energy.
- Rare Deadeye: 12–16 with Exhaust.
- Deadeye should not apply to every Round in a multi-shot action unless a card explicitly says so.

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
5. Dodge/Smoke.
6. Empty-cylinder.
7. Spin/gamble.

Look for:
- "Parasitic" cards that require too many other cards to function.
- Infinite loops involving 0-cost Spin/Cycle/draw.
- Fire 6 turns gaining too much from Strength.
- Smoke Round loops producing multiple Dodge too cheaply.

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
- Dodge is the precise answer to a specific hit.
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
- Dodge invalidating single-hit elites.
- Perfect Reload + High Noon becoming a one-card two-turn kill package.
- Sixth Shot creating Energy-positive loops.
- Quickdraw Legend making 3-cost salvo cards effectively free too often.

---

# 16. Tuning Knobs

If the class is too strong:
1. Lead 6 → 5.
2. Old Iron starts with 2 Lead instead of 3.
3. Smoke Round grants 6 Block instead of Dodge.
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
6. Quick Draw+ gains 3 Deadeye instead of 2.

---

# 17. Design Rules for Future Cards

When adding/replacing cards, preserve these rules:

1. **The Cylinder must matter.**  
   Avoid creating too many generic attacks that ignore the gun.

2. **The gun must not be mandatory every hand.**  
   Keep enough direct attacks and ordinary Block that bad draw order is playable.

3. **Randomness needs correction tools.**  
   Spin is fun because Cycle, Called Shot, Stack the Cylinder, and Loaded Dice exist.

4. **Dodge must remain tactical.**  
   Do not turn it into bankable Intangible.

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
