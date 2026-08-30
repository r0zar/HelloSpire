#!/usr/bin/env python3
"""
Generate the Paladin's card set and its supporting powers from one spec.

    python tools/gen_paladin.py

Emits, idempotently:
  HelloSpireCode/Characters/Paladin/Cards/<Name>.cs        one file per card
  HelloSpireCode/Characters/Paladin/Powers/<Name>Power.cs   one file per generated power
  HelloSpire/localization/eng/cards.json, powers.json       merged; keys this script owns are overwritten
  HelloSpire/images/card_portraits/**, powers/**            labelled placeholder tiles where no art exists

Hand-written cards (Strike, Defend, Mend, Hammer of Justice, the three Oath cards) and hand-written
powers (the Oaths, the marker powers, the two bases) are never touched.

The spec is the source of truth for card text and numbers. design/paladin-cards.md is the design
intent; where the two differ the DEVIATIONS table at the bottom says why.
"""
import json, os, re, collections, io
from PIL import Image, ImageDraw, ImageFont

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
CARDS_DIR = os.path.join(ROOT, "HelloSpireCode/Characters/Paladin/Cards")
POWERS_DIR = os.path.join(ROOT, "HelloSpireCode/Characters/Paladin/Powers")
LOC = os.path.join(ROOT, "HelloSpire/localization/eng")
IMG = os.path.join(ROOT, "HelloSpire/images")

def snake(n): return re.sub(r"(?<!^)(?=[A-Z])", "_", n).lower()
def key(n):   return "HELLOSPIRE-" + snake(n).upper()

# --------------------------------------------------------------------------- C# snippets

ATTACK_FX = '"vfx/vfx_attack_slash"'
def DMG(hits=None, fx=ATTACK_FX):
    h = f".WithHitCount({hits})" if hits else ""
    return ("ArgumentNullException.ThrowIfNull(cardPlay.Target);\n"
            f"await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target){h}.WithHitFx({fx}).Execute(choiceContext);")
def DMG_EXPR(expr, fx=ATTACK_FX):
    return ("ArgumentNullException.ThrowIfNull(cardPlay.Target);\n"
            f"await DamageCmd.Attack({expr}).FromCard(this).Targeting(cardPlay.Target).WithHitFx({fx}).Execute(choiceContext);")
def DMG_ALL(expr="DynamicVars.Damage.BaseValue", fx=ATTACK_FX):
    return f"await DamageCmd.Attack({expr}).FromCard(this).TargetingAllOpponents(Owner.Creature.CombatState).WithHitFx({fx}).Execute(choiceContext);"
BLK        = "await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);"
BLK_TARGET = "await CreatureCmd.GainBlock(cardPlay.Target!, DynamicVars.Block, cardPlay);"
def BLK_EXPR(expr): return f"await CreatureCmd.GainBlock(Owner.Creature, {expr}, ValueProp.Move, cardPlay);"
CAST       = 'await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);'
POWERUP    = 'await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);'
HEAL       = CAST + "\nawait CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);"
HEAL_TARGET= CAST + "\nawait CreatureCmd.Heal(cardPlay.Target!, DynamicVars.Heal.BaseValue);"
def HEAL_ALL(expr): return CAST + f"\nforeach (var ally in Allies()) await CreatureCmd.Heal(ally, {expr});"
DRAW       = "await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner, false);"
ENERGY     = "await PlayerCmd.GainEnergy(DynamicVars.Energy.BaseValue, Owner);"
def APPLY(T, target="Owner.Creature", amount=None):
    amount = amount or f'DynamicVars["{T}"].BaseValue'
    return f"await PowerCmd.Apply<{T}>(choiceContext, {target}, {amount}, Owner.Creature, this);"
def FAITH(d, n): return f"FaithTracks.Gain(Owner, Deity.{d}, {n});"
LOSE_HP    = "await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered, this);"

def V(kind, *a):
    if kind == "dmg":    return ("Damage", f"new DamageVar({a[0]}m, ValueProp.Move)")
    if kind == "blk":    return ("Block",  f"new BlockVar({a[0]}m, ValueProp.Move)")
    if kind == "heal":   return ("Heal",   f"new HealVar({a[0]}m)")
    if kind == "cards":  return ("Cards",  f"new CardsVar({a[0]})")
    if kind == "energy": return ("Energy", f"new EnergyVar({a[0]})")
    if kind == "hploss": return ("HpLoss", f"new HpLossVar({a[0]}m)")
    if kind == "power":  return (a[0],     f"new PowerVar<{a[0]}>({a[1]}m)")
    if kind == "var":    return (a[0],     f'new DynamicVar("{a[0]}", {a[1]}m)')
    raise ValueError(kind)
def UP(var, n): return f"DynamicVars[\"{var}\"].UpgradeValueBy({n}m);"

# --------------------------------------------------------------------------- the cards
# name, title, cost, type, rarity, target, vars, body, up, desc, [mp], [req=(D,n)], [kw], [tips], [gb]
G="[gold]"; g="[/gold]"
def card(name, title, cost, type_, rarity, target, vars_, body, up, desc, mp=False, req=None, kw=(), tips=(), gb=False):
    return dict(name=name, title=title, cost=cost, type=type_, rarity=rarity, target=target, vars=vars_, body=body, up=up, desc=desc, mp=mp, req=req, kw=list(kw), tips=list(tips), gb=gb)

CARDS = [
# ---------------- Common: Torm
card("HoldTheLine","Hold the Line",1,"Skill","Common","Self",[V("blk",8)],[BLK],[UP("Block",3)],f"Gain {{Block:diff()}} {G}Block{g}.",gb=True),
card("ShieldBash","Shield Bash",1,"Attack","Common","AnyEnemy",[V("dmg",5),V("blk",3)],[DMG(),BLK],[UP("Damage",2),UP("Block",2)],f"Deal {{Damage:diff()}} damage.\nGain {{Block:diff()}} {G}Block{g}.",gb=True),
card("Brace","Brace",1,"Skill","Common","Self",[V("blk",8)],[BLK],[UP("Block",3)],f"Gain {{Block:diff()}} {G}Block{g}.",gb=True),
card("Interpose","Interpose",1,"Skill","Common","AnyPlayer",[V("blk",4)],[BLK,BLK_TARGET],[UP("Block",2)],f"Gain {{Block:diff()}} {G}Block{g}.\nA player gains {{Block:diff()}} {G}Block{g}.",gb=True),
card("Steadfast","Steadfast",0,"Skill","Common","Self",[V("blk",3),V("cards",1)],[BLK,DRAW],[UP("Block",2)],f"Gain {{Block:diff()}} {G}Block{g}.\nDraw {{Cards:diff()}} {{Cards:plural:card|cards}}.",gb=True),
card("ShieldWall","Shield Wall",2,"Skill","Common","Self",[V("blk",12)],[BLK],[UP("Block",4)],f"Gain {{Block:diff()}} {G}Block{g}.",gb=True),
card("AuraOfProtection","Aura of Protection",1,"Power","Common","Self",[V("power","AuraOfProtectionPower",2)],[POWERUP,APPLY("AuraOfProtectionPower")],[UP("AuraOfProtectionPower",1)],f"At the start of your turn, all allies gain {{AuraOfProtectionPower:diff()}} {G}Block{g}.",tips=["AuraOfProtectionPower"]),
# ---------------- Common: Ilmater
card("Soothe","Soothe",1,"Skill","Common","AnyPlayer",[V("heal",6)],[HEAL_TARGET],[UP("Heal",3)],"Heal a player {Heal:diff()} HP."),
card("FlashOfLight","Flash of Light",1,"Skill","Common","AnyPlayer",[V("heal",5)],[HEAL_TARGET],[UP("Heal",3)],"Heal a player {Heal:diff()} HP."),
card("HolyShock","Holy Shock",1,"Attack","Common","AnyEnemy",[V("dmg",5),V("heal",3)],[DMG(),CAST,"await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);"],[UP("Damage",2),UP("Heal",2)],"Deal {Damage:diff()} damage.\nHeal {Heal:diff()} HP."),
card("Salve","Salve",0,"Skill","Common","Self",[V("heal",3)],[HEAL],[UP("Heal",2)],"Heal {Heal:diff()} HP."),
card("Renew","Renew",1,"Power","Common","Self",[V("power","RenewPower",2)],[POWERUP,APPLY("RenewPower")],[UP("RenewPower",1)],"At the start of your turn, heal {RenewPower:diff()} HP.",tips=["RenewPower"]),
card("Comfort","Comfort",1,"Skill","Common","AnyPlayer",[V("heal",4),V("cards",1)],[HEAL_TARGET,DRAW],[UP("Heal",2)],"Heal a player {Heal:diff()} HP.\nDraw {Cards:diff()} {Cards:plural:card|cards}."),
# ---------------- Common: Tyr
card("Smite","Smite",1,"Attack","Common","AnyEnemy",[V("dmg",9)],[DMG()],[UP("Damage",3)],"Deal {Damage:diff()} damage."),
card("CrusaderStrike","Crusader Strike",1,"Attack","Common","AnyEnemy",[V("dmg",6)],[DMG(hits=2)],[UP("Damage",2)],"Deal {Damage:diff()} damage twice."),
card("Cleave","Cleave",1,"Attack","Common","AllEnemies",[V("dmg",5)],[DMG_ALL()],[UP("Damage",3)],"Deal {Damage:diff()} damage to ALL enemies."),
card("RighteousBlow","Righteous Blow",2,"Attack","Common","AnyEnemy",[V("dmg",14)],[DMG(fx='"vfx/vfx_attack_blunt"')],[UP("Damage",4)],"Deal {Damage:diff()} damage."),
card("Rebuke","Rebuke",1,"Attack","Common","AnyEnemy",[V("dmg",4),V("power","WeakPower",1)],[DMG(),APPLY("WeakPower","cardPlay.Target")],[UP("Damage",2),UP("WeakPower",1)],f"Deal {{Damage:diff()}} damage.\nApply {{WeakPower:diff()}} {G}Weak{g}.",tips=["WeakPower"]),
card("VengefulMending","Vengeful Mending",1,"Attack","Common","AnyEnemy",[V("dmg",7),V("heal",3)],[DMG(),CAST,"await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);"],[UP("Damage",2),UP("Heal",2)],"Deal {Damage:diff()} damage.\nHeal {Heal:diff()} HP."),
# ---------------- Common: neutral
card("Prayer","Prayer",1,"Skill","Common","Self",[V("cards",2)],[DRAW],[UP("Cards",1)],"Draw {Cards:diff()} cards."),

# ---------------- Uncommon: Torm
card("Bulwark","Bulwark",1,"Skill","Uncommon","Self",[],[BLK_EXPR("FaithTracks.Effective(Owner, Deity.Torm)")],["EnergyCost.UpgradeBy(-1);"],f"Gain {G}Block{g} equal to your {G}Faith{g} in {G}Torm{g}.",gb=True),
card("HolyShield","Holy Shield",1,"Skill","Uncommon","Self",[V("blk",5),V("power","HolyShieldPower",3)],[BLK,APPLY("HolyShieldPower")],[UP("Block",3),UP("HolyShieldPower",2)],f"Gain {{Block:diff()}} {G}Block{g}.\nUntil end of turn, whenever you are attacked, deal {{HolyShieldPower:diff()}} damage back.",tips=["HolyShieldPower"],gb=True),
card("GuardiansMercy","Guardian's Mercy",1,"Skill","Uncommon","AnyPlayer",[V("blk",4),V("heal",3)],[BLK,HEAL_TARGET],[UP("Block",2),UP("Heal",2)],f"Gain {{Block:diff()}} {G}Block{g}.\nHeal a player {{Heal:diff()}} HP.",gb=True),
card("ConsecratedGround","Consecrated Ground",2,"Power","Uncommon","Self",[V("power","ConsecratedGroundPower",1)],[POWERUP,APPLY("ConsecratedGroundPower")],[UP("ConsecratedGroundPower",1)],f"Whenever you play a card that gains {G}Block{g}, all allies gain {{ConsecratedGroundPower:diff()}} {G}Block{g}.",tips=["ConsecratedGroundPower"]),
card("Bastion","Bastion",2,"Skill","Uncommon","Self",[V("blk",10),V("power","BastionPower",1)],[BLK,APPLY("BastionPower")],[UP("Block",4)],f"Gain {{Block:diff()}} {G}Block{g}.\n{G}Block{g} is not removed at the start of your next turn.",tips=["BastionPower"],gb=True),
card("Retribution","Retribution",1,"Attack","Uncommon","AllEnemies",[],[DMG_ALL("Owner.Creature.Block")],["EnergyCost.UpgradeBy(-1);"],f"Deal damage equal to your {G}Block{g} to ALL enemies."),
card("ShieldSlam","Shield Slam",1,"Attack","Uncommon","AnyEnemy",[],[DMG_EXPR("Owner.Creature.Block")],["EnergyCost.UpgradeBy(-1);"],f"Deal damage equal to your {G}Block{g}."),
card("Sentinel","Sentinel",1,"Power","Uncommon","Self",[V("power","SentinelPower",2)],[POWERUP,APPLY("SentinelPower")],[UP("SentinelPower",1)],f"Whenever an ally is attacked, gain {{SentinelPower:diff()}} {G}Block{g}.",mp=True,tips=["SentinelPower"]),
card("ImmovableObject","Immovable Object",2,"Skill","Uncommon","Self",[V("blk",15)],[BLK],[UP("Block",5)],f"Requires 3 {G}Faith{g} in {G}Torm{g}.\nGain {{Block:diff()}} {G}Block{g}.",req=("Torm",3),gb=True),
# ---------------- Uncommon: Ilmater
card("BlessingOfSacrifice","Blessing of Sacrifice",1,"Power","Uncommon","Self",[V("power","BlessingOfSacrificePower",1)],[POWERUP,APPLY("BlessingOfSacrificePower")],["EnergyCost.UpgradeBy(-1);"],"Damage an ally would take is dealt to you instead.",mp=True,tips=["BlessingOfSacrificePower"]),
card("AuraOfMercy","Aura of Mercy",2,"Power","Uncommon","Self",[V("power","AuraOfMercyPower",1)],[POWERUP,APPLY("AuraOfMercyPower")],[UP("AuraOfMercyPower",1)],"Whenever an ally is healed, every other ally heals {AuraOfMercyPower:diff()} HP.",mp=True,tips=["AuraOfMercyPower"]),
card("Absolve","Absolve",1,"Skill","Uncommon","AnyPlayer",[V("heal",5)],[HEAL_TARGET,"var debuff = cardPlay.Target!.Powers.OfType<PowerModel>().FirstOrDefault(p => p.Type == PowerType.Debuff);","if (debuff != null) await PowerCmd.Remove(debuff);"],[UP("Heal",3)],"Heal a player {Heal:diff()} HP.\nRemove one debuff from them."),
card("HolyLight","Holy Light",2,"Skill","Uncommon","AnyPlayer",[V("heal",12)],[HEAL_TARGET],[UP("Heal",4)],"Heal a player {Heal:diff()} HP."),
card("BindTheWounds","Bind the Wounds",1,"Skill","Uncommon","AnyPlayer",[V("heal",4)],[HEAL_TARGET,FAITH("Ilmater",1)],[UP("Heal",3)],f"Heal a player {{Heal:diff()}} HP.\nGain 1 {G}Faith{g} in {G}Ilmater{g}."),
card("PrayerOfMending","Prayer of Mending",1,"Skill","Uncommon","AnyPlayer",[V("heal",4),V("power","PrayerOfMendingPower",4)],[HEAL_TARGET,APPLY("PrayerOfMendingPower","cardPlay.Target")],[UP("Heal",2),UP("PrayerOfMendingPower",2)],"Heal a player {Heal:diff()} HP.\nAt the start of their next turn, heal them {PrayerOfMendingPower:diff()} HP.",tips=["PrayerOfMendingPower"]),
card("Martyr","Martyr",1,"Skill","Uncommon","AnyPlayer",[V("hploss",4),V("heal",8)],[LOSE_HP,HEAL_TARGET],[UP("Heal",4)],"Lose {HpLoss:diff()} HP.\nHeal a player {Heal:diff()} HP."),
card("CircleOfHealing","Circle of Healing",2,"Skill","Uncommon","Self",[V("heal",4)],[HEAL_ALL("DynamicVars.Heal.BaseValue")],[UP("Heal",2)],"Heal all allies {Heal:diff()} HP."),
card("FaithfulServant","Faithful Servant",2,"Skill","Uncommon","AnyPlayer",[],[CAST,"await CreatureCmd.Heal(cardPlay.Target!, FaithTracks.Effective(Owner, Deity.Ilmater));"],["EnergyCost.UpgradeBy(-1);"],f"Requires 3 {G}Faith{g} in {G}Ilmater{g}.\nHeal a player HP equal to your {G}Faith{g} in {G}Ilmater{g}.",req=("Ilmater",3)),
card("Sanctuary","Sanctuary",1,"Skill","Uncommon","AnyPlayer",[V("heal",3),V("blk",5)],[HEAL_TARGET,BLK_TARGET],[UP("Heal",2),UP("Block",3)],f"Heal a player {{Heal:diff()}} HP.\nThey gain {{Block:diff()}} {G}Block{g}.",gb=True),
# ---------------- Uncommon: Tyr
card("HolySmite","Holy Smite",1,"Attack","Uncommon","AnyEnemy",[V("dmg",6),V("heal",3)],[DMG(),CAST,"await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);"],[UP("Damage",3),UP("Heal",2)],"Deal {Damage:diff()} damage.\nHeal {Heal:diff()} HP."),
card("BladeOfJustice","Blade of Justice",1,"Attack","Uncommon","AnyEnemy",[V("dmg",9)],[DMG(),FAITH("Tyr",1)],[UP("Damage",3)],f"Deal {{Damage:diff()}} damage.\nGain 1 {G}Faith{g} in {G}Tyr{g}."),
card("Judgment","Judgment",1,"Attack","Uncommon","AnyEnemy",[V("dmg",5),V("power","JudgedPower",4)],[DMG(),APPLY("JudgedPower","cardPlay.Target")],[UP("Damage",2),UP("JudgedPower",2)],"Deal {Damage:diff()} damage.\nThe enemy takes {JudgedPower:diff()} additional damage from all sources this turn.",tips=["JudgedPower"]),
card("Exorcism","Exorcism",1,"Attack","Uncommon","AnyEnemy",[V("dmg",12)],[DMG()],[UP("Damage",5)],f"Deal {{Damage:diff()}} damage.\n{G}Exhaust{g}.",kw=["Exhaust"]),
card("WakeOfAshes","Wake of Ashes",2,"Attack","Uncommon","AllEnemies",[V("dmg",7)],[DMG_ALL(),FAITH("Tyr",1)],[UP("Damage",3)],f"Deal {{Damage:diff()}} damage to ALL enemies.\nGain 1 {G}Faith{g} in {G}Tyr{g}."),
card("Zeal","Zeal",1,"Attack","Uncommon","AnyEnemy",[V("dmg",4)],[DMG(hits=2)],[UP("Damage",2)],"Deal {Damage:diff()} damage twice."),
card("EqualMeasure","Equal Measure",1,"Attack","Uncommon","AnyEnemy",[],[DMG_EXPR("FaithTracks.Effective(Owner, Deity.Tyr)")],["EnergyCost.UpgradeBy(-1);"],f"Deal damage equal to your {G}Faith{g} in {G}Tyr{g}."),
card("Consecration","Consecration",2,"Power","Uncommon","Self",[V("power","ConsecrationPower",3)],[POWERUP,APPLY("ConsecrationPower")],[UP("ConsecrationPower",2)],"At the start of your turn, deal {ConsecrationPower:diff()} damage to ALL enemies.",tips=["ConsecrationPower"]),
card("CrusaderAura","Crusader Aura",2,"Power","Uncommon","Self",[V("power","CrusaderAuraPower",1)],[POWERUP,APPLY("CrusaderAuraPower")],[UP("CrusaderAuraPower",1)],"All allies' Attacks deal {CrusaderAuraPower:diff()} additional damage.",tips=["CrusaderAuraPower"]),
card("DivinePurpose","Divine Purpose",1,"Attack","Uncommon","AnyEnemy",[V("dmg",10),V("energy",1)],[DMG(),ENERGY],[UP("Damage",4)],f"Requires 3 {G}Faith{g} in {G}Tyr{g}.\nDeal {{Damage:diff()}} damage.\nGain {{Energy:energyIcons()}}.",req=("Tyr",3)),
# ---------------- Uncommon: cross-deity
card("Sacrament","Sacrament",1,"Skill","Uncommon","Self",[],[CAST,"FaithTracks.Consolidate(Owner);"],["EnergyCost.UpgradeBy(-1);"],f"Move all your {G}Faith{g} into your highest deity."),
card("Kneel","Kneel",0,"Skill","Uncommon","Self",[],[CAST,"FaithTracks.Gain(Owner, FaithTracks.Lowest(Owner), 1);"],["EnergyCost.UpgradeBy(0);"],f"Gain 1 {G}Faith{g} in your lowest deity.\n{G}Exhaust{g}.",kw=["Exhaust"]),
card("Devotion","Devotion",1,"Skill","Uncommon","Self",[],[CAST,"FaithTracks.Gain(Owner, FaithTracks.Highest(Owner).deity, 1);"],["EnergyCost.UpgradeBy(-1);"],f"Gain 1 {G}Faith{g} in your highest deity."),
card("Tithe","Tithe",1,"Skill","Uncommon","Self",[V("energy",2)],[CAST,"var (top, _) = FaithTracks.Highest(Owner);","if (FaithTracks.Spend(Owner, top, 3) > 0) " + ENERGY],["EnergyCost.UpgradeBy(-1);"],f"Spend 3 {G}Faith{g} in your highest deity.\nGain {{Energy:energyIcons()}}."),
card("Litany","Litany",1,"Skill","Uncommon","Self",[],[CAST,"var n = FaithTracks.Highest(Owner).amount / 4;","if (n > 0) await CardPileCmd.Draw(choiceContext, n, Owner, false);"],["EnergyCost.UpgradeBy(-1);"],f"Draw 1 card for every 4 {G}Faith{g} in your highest deity."),
card("SealOfCommand","Seal of Command",1,"Skill","Uncommon","Self",[V("power","SealOfCommandPower",3)],[POWERUP,APPLY("SealOfCommandPower")],[UP("SealOfCommandPower",2)],"For {SealOfCommandPower:diff()} turns, your Attacks deal 2 additional damage.",tips=["SealOfCommandPower"]),
card("SealOfLight","Seal of Light",1,"Skill","Uncommon","Self",[V("power","SealOfLightPower",3)],[POWERUP,APPLY("SealOfLightPower")],[UP("SealOfLightPower",2)],"For {SealOfLightPower:diff()} turns, whenever you play an Attack, heal 2 HP.",tips=["SealOfLightPower"]),
card("HallowedGround","Hallowed Ground",2,"Power","Uncommon","Self",[V("power","HallowedGroundPower",1)],[POWERUP,APPLY("HallowedGroundPower")],[UP("HallowedGroundPower",1)],f"Whenever you gain {G}Faith{g} in your highest deity, gain {{HallowedGroundPower:diff()}} {G}Block{g}.",tips=["HallowedGroundPower"]),

# ---------------- Rare: Torm
card("ShieldOfTheRighteous","Shield of the Righteous",1,"Skill","Rare","Self",[],[BLK_EXPR("2 * FaithTracks.Effective(Owner, Deity.Torm)")],["EnergyCost.UpgradeBy(-1);"],f"Gain {G}Block{g} equal to twice your {G}Faith{g} in {G}Torm{g}.",gb=True),
card("ArdentDefender","Ardent Defender",2,"Power","Rare","Self",[V("power","ArdentDefenderPower",1)],[POWERUP,APPLY("ArdentDefenderPower")],["EnergyCost.UpgradeBy(-1);"],"The first time you would die this combat, instead heal to a third of your Max HP.",tips=["ArdentDefenderPower"]),
card("DivineAllegiance","Divine Allegiance",1,"Power","Rare","Self",[V("power","DivineAllegiancePower",1)],[POWERUP,APPLY("DivineAllegiancePower")],["EnergyCost.UpgradeBy(-1);"],"Damage an ally would take is dealt to you instead.",mp=True,tips=["DivineAllegiancePower"]),
card("AuraOfDevotion","Aura of Devotion",2,"Power","Rare","Self",[V("power","AuraOfDevotionPower",1)],[POWERUP,APPLY("AuraOfDevotionPower")],[UP("AuraOfDevotionPower",1)],f"All allies take {{AuraOfDevotionPower:diff()}} less damage from attacks for every 5 {G}Faith{g} in {G}Torm{g}.",tips=["AuraOfDevotionPower"]),
card("AvengersShield","Avenger's Shield",1,"Attack","Rare","AllEnemies",[V("dmg",8),V("blk",6)],["foreach (var enemy in Enemies().Take(3))","    await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(enemy).WithHitFx(\"vfx/vfx_attack_blunt\").Execute(choiceContext);",BLK],[UP("Damage",3),UP("Block",3)],f"Deal {{Damage:diff()}} damage to up to 3 enemies.\nGain {{Block:diff()}} {G}Block{g}.",gb=True),
card("GuardianOfAncientKings","Guardian of Ancient Kings",3,"Skill","Rare","Self",[V("power","GuardianOfAncientKingsPower",3)],[POWERUP,APPLY("GuardianOfAncientKingsPower")],["EnergyCost.UpgradeBy(-1);"],f"Requires 5 {G}Faith{g} in {G}Torm{g}.\nFor {{GuardianOfAncientKingsPower:diff()}} turns, all damage you take is halved.",req=("Torm",5),tips=["GuardianOfAncientKingsPower"]),
card("DivineShield","Divine Shield",2,"Skill","Rare","Self",[V("power","IntangiblePower",1)],[POWERUP,APPLY("IntangiblePower")],["EnergyCost.UpgradeBy(-1);"],f"Requires 4 {G}Faith{g} in {G}Torm{g}.\nGain {{IntangiblePower:diff()}} {G}Intangible{g}.\n{G}Exhaust{g}.",req=("Torm",4),kw=["Exhaust"],tips=["IntangiblePower"]),
# ---------------- Rare: Ilmater
card("LayOnHands","Lay on Hands",2,"Skill","Rare","AnyPlayer",[],[CAST,"await CreatureCmd.Heal(cardPlay.Target!, cardPlay.Target!.MaxHp);"],["EnergyCost.UpgradeBy(-1);"],f"Heal a player to full.\n{G}Exhaust{g}.",kw=["Exhaust"]),
card("BeaconOfLight","Beacon of Light",2,"Power","Rare","AnyAlly",[V("power","BeaconOfLightPower",1)],[POWERUP,APPLY("BeaconOfLightPower"),"Owner.Creature.GetPower<BeaconOfLightPower>()!.Beneficiary = cardPlay.Target;"],["EnergyCost.UpgradeBy(-1);"],"Choose an ally. Whenever you play a card that heals, they also heal half that much.",mp=True,tips=["BeaconOfLightPower"]),
card("WordOfGlory","Word of Glory",1,"Skill","Rare","AnyPlayer",[V("heal",12)],["if (FaithTracks.Spend(Owner, Deity.Ilmater, 3) > 0) { " + HEAL_TARGET.replace("\n"," ") + " }"],[UP("Heal",4)],f"Requires 3 {G}Faith{g} in {G}Ilmater{g}.\nSpend 3 {G}Faith{g} in {G}Ilmater{g}. Heal a player {{Heal:diff()}} HP.",req=("Ilmater",3)),
card("AuraOfVitality","Aura of Vitality",2,"Power","Rare","Self",[V("power","AuraOfVitalityPower",3)],[POWERUP,APPLY("AuraOfVitalityPower")],[UP("AuraOfVitalityPower",1)],"At the start of your turn, all allies heal {AuraOfVitalityPower:diff()} HP.",tips=["AuraOfVitalityPower"]),
card("LightOfDawn","Light of Dawn",2,"Skill","Rare","Self",[],[HEAL_ALL("FaithTracks.Effective(Owner, Deity.Ilmater) / 2")],["EnergyCost.UpgradeBy(-1);"],f"Heal all allies HP equal to half your {G}Faith{g} in {G}Ilmater{g}."),
card("Redemption","Redemption",3,"Skill","Rare","Self",[],[CAST,"foreach (var fallen in Owner.Creature.CombatState.PlayerCreatures.OfType<Creature>().Where(c => c.IsDead)) await CreatureCmd.Heal(fallen, 1m);"],["EnergyCost.UpgradeBy(-1);"],f"Requires 5 {G}Faith{g} in {G}Ilmater{g}.\nRevive all downed allies at 1 HP.\n{G}Exhaust{g}.",mp=True,req=("Ilmater",5),kw=["Exhaust"]),
card("TheBrokenGod","The Broken God",2,"Power","Rare","Self",[V("power","TheBrokenGodPower",1)],[POWERUP,APPLY("TheBrokenGodPower")],["EnergyCost.UpgradeBy(-1);"],f"At the start of your turn, all allies heal HP equal to half your {G}Faith{g} in {G}Ilmater{g}.",tips=["TheBrokenGodPower"]),
# ---------------- Rare: Tyr
card("DivineSmite","Divine Smite",1,"Attack","Rare","AnyEnemy",[V("dmg",8),V("var","Bonus",8)],["var bonus = FaithTracks.HasAny(Owner, 3) ? DynamicVars[\"Bonus\"].BaseValue : 0m;",DMG_EXPR("DynamicVars.Damage.BaseValue + bonus",fx='"vfx/vfx_attack_blunt"')],[UP("Damage",3),UP("Bonus",3)],f"Deal {{Damage:diff()}} damage.\nIf you have 3 or more {G}Faith{g} in any deity, deal {{Bonus:diff()}} additional damage."),
card("HammerOfWrath","Hammer of Wrath",1,"Attack","Rare","AnyEnemy",[V("dmg",10),V("var","Bonus",10)],["ArgumentNullException.ThrowIfNull(cardPlay.Target);","var low = cardPlay.Target.CurrentHp * 2 < cardPlay.Target.MaxHp;","var bonus = low ? DynamicVars[\"Bonus\"].BaseValue : 0m;","await DamageCmd.Attack(DynamicVars.Damage.BaseValue + bonus).FromCard(this).Targeting(cardPlay.Target).WithHitFx(\"vfx/vfx_attack_blunt\").Execute(choiceContext);","if (low) " + FAITH("Tyr",2)],[UP("Damage",3),UP("Bonus",3)],f"Deal {{Damage:diff()}} damage.\nIf the enemy is below half HP, deal {{Bonus:diff()}} additional damage and gain 2 {G}Faith{g} in {G}Tyr{g}."),
card("VowOfEnmity","Vow of Enmity",1,"Power","Rare","AnyEnemy",[V("power","VowOfEnmityPower",4)],[POWERUP,APPLY("VowOfEnmityPower"),"Owner.Creature.GetPower<VowOfEnmityPower>()!.Foe = cardPlay.Target;"],[UP("VowOfEnmityPower",2)],"Choose an enemy. Your Attacks against it deal {VowOfEnmityPower:diff()} additional damage.",tips=["VowOfEnmityPower"]),
card("EyeForAnEye","Eye for an Eye",1,"Power","Rare","Self",[V("power","EyeForAnEyePower",1)],[POWERUP,APPLY("EyeForAnEyePower")],["EnergyCost.UpgradeBy(-1);"],"Whenever an enemy attacks you, deal that much damage back to it.",tips=["EyeForAnEyePower"]),
card("DivineStorm","Divine Storm",2,"Attack","Rare","AllEnemies",[V("dmg",10),V("heal",3)],["var hit = Enemies().Count();",DMG_ALL(),CAST,"await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue * hit);"],[UP("Damage",3),UP("Heal",1)],"Deal {Damage:diff()} damage to ALL enemies.\nHeal {Heal:diff()} HP for each enemy hit."),
card("FinalReckoning","Final Reckoning",3,"Attack","Rare","AllEnemies",[],[DMG_ALL("FaithTracks.Total(Owner)",fx='"vfx/vfx_attack_blunt"')],["EnergyCost.UpgradeBy(-1);"],f"Deal damage to ALL enemies equal to your total {G}Faith{g} across every deity."),
card("TheScales","The Scales",2,"Attack","Rare","AnyEnemy",[],[DMG_EXPR("2 * FaithTracks.Effective(Owner, Deity.Tyr)",fx='"vfx/vfx_attack_blunt"')],["EnergyCost.UpgradeBy(-1);"],f"Deal damage equal to twice your {G}Faith{g} in {G}Tyr{g}."),
# ---------------- Rare: cross-deity and Bane
card("Heresy","Heresy",3,"Power","Rare","Self",[V("power","HeresyPower",1)],[POWERUP,APPLY("HeresyPower")],["EnergyCost.UpgradeBy(-1);"],f"Your {G}Faith{g} in every deity counts as your highest.",tips=["HeresyPower"]),
card("Zealotry","Zealotry",3,"Power","Rare","Self",[V("power","ZealotryPower",1)],[POWERUP,APPLY("ZealotryPower")],["EnergyCost.UpgradeBy(-1);"],f"Whenever you gain {G}Faith{g}, gain 1 additional.",tips=["ZealotryPower"]),
card("AvengingWrath","Avenging Wrath",2,"Skill","Rare","Self",[V("power","AvengingWrathPower",3)],[POWERUP,APPLY("AvengingWrathPower")],[UP("AvengingWrathPower",2)],f"Requires 4 {G}Faith{g} in {G}Tyr{g}.\nFor {{AvengingWrathPower:diff()}} turns, your Attacks deal 5 additional damage.",req=("Tyr",4),tips=["AvengingWrathPower"]),
card("TimeOfTroubles","Time of Troubles",3,"Attack","Rare","AnyEnemy",[],["ArgumentNullException.ThrowIfNull(cardPlay.Target);","var torm = FaithTracks.Raw(Owner, Deity.Torm);","FaithTracks.Get(Owner.PlayerCombatState, Deity.Torm).ModifyAmount(-torm);","FaithTracks.Get(Owner.PlayerCombatState, Deity.Bane).ModifyAmount(torm);","await DamageCmd.Attack(2 * torm).FromCard(this).Targeting(cardPlay.Target).WithHitFx(\"vfx/vfx_attack_blunt\").Execute(choiceContext);"],["EnergyCost.UpgradeBy(-1);"],f"Spend ALL your {G}Faith{g} in {G}Torm{g}.\nDeal double that damage. Gain that much {G}Faith{g} in {G}Bane{g}."),
card("Tyranny","Tyranny",2,"Power","Rare","Self",[V("power","BaneTyrannyPower",1)],[POWERUP,APPLY("BaneTyrannyPower")],["EnergyCost.UpgradeBy(-1);"],f"Requires 5 {G}Faith{g} in {G}Bane{g}.\nYour {G}Faith{g} in {G}Bane{g} counts as {G}Faith{g} in every deity.",req=("Bane",5),tips=["BaneTyrannyPower"]),
]

# --------------------------------------------------------------------------- the powers
# name (without "Power"), title, type, stack, base, desc, smart, body
def power(name, title, ptype, stack, desc, smart, body, base="PaladinPower"):
    return dict(name=name, title=title, ptype=ptype, stack=stack, desc=desc, smart=smart, body=body, base=base)

ALLIES = "CombatState.PlayerCreatures.OfType<Creature>().Where(c => c.IsAlive)"
ENEMIES = "CombatState.Creatures.OfType<Creature>().Where(c => !c.IsPlayer && c.IsHittable)"

POWERS = [
power("AuraOfProtection","Aura of Protection","Buff","Counter",
  "At the start of your turn, all allies gain [blue]2[/blue] [gold]Block[/gold].","At the start of your turn, all allies gain [blue]{Amount}[/blue] [gold]Block[/gold].",
  f"""public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {{
        if (!participants.Contains(Owner)) return;
        Flash();
        foreach (var ally in {ALLIES}) await CreatureCmd.GainBlock(ally, Amount, ValueProp.Unpowered, null);
    }}"""),
power("Renew","Renew","Buff","Counter",
  "At the start of your turn, heal [blue]2[/blue] HP.","At the start of your turn, heal [blue]{Amount}[/blue] HP.",
  """public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        Flash();
        await CreatureCmd.Heal(Owner, Amount);
    }"""),
power("HolyShield","Holy Shield","Buff","Counter",
  "Until end of turn, whenever you are attacked, deal [blue]3[/blue] damage back.","Until end of turn, whenever you are attacked, deal [blue]{Amount}[/blue] damage back.",
  """public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || dealer == null || dealer.IsPlayer) return;
        Flash();
        await CreatureCmd.Damage(choiceContext, dealer, Amount, ValueProp.Unpowered | ValueProp.SkipHurtAnim, Owner, null);
    }
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == Owner.Side) await PowerCmd.Remove(this);
    }"""),
power("ConsecratedGround","Consecrated Ground","Buff","Counter",
  "Whenever you play a card that gains [gold]Block[/gold], all allies gain [blue]1[/blue] [gold]Block[/gold].","Whenever you play a card that gains [gold]Block[/gold], all allies gain [blue]{Amount}[/blue] [gold]Block[/gold].",
  f"""public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {{
        if (cardPlay.Card.Owner?.Creature != Owner || !cardPlay.Card.GainsBlock) return;
        Flash();
        foreach (var ally in {ALLIES}) await CreatureCmd.GainBlock(ally, Amount, ValueProp.Unpowered, null);
    }}"""),
power("Bastion","Bastion","Buff","Single",
  "[gold]Block[/gold] is not removed at the start of your next turn.","[gold]Block[/gold] is not removed at the start of your next turn.",
  """public override bool ShouldClearBlock(Creature creature) => creature != Owner;
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature == Owner) await PowerCmd.Remove(this);
    }"""),
power("Sentinel","Sentinel","Buff","Counter",
  "Whenever an ally is attacked, gain [blue]2[/blue] [gold]Block[/gold].","Whenever an ally is attacked, gain [blue]{Amount}[/blue] [gold]Block[/gold].",
  """public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!target.IsPlayer || target == Owner || dealer == null || dealer.IsPlayer) return;
        Flash();
        await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null);
    }"""),
power("BlessingOfSacrifice","Blessing of Sacrifice","Buff","Single",
  "Damage an ally would take is dealt to you instead.","Damage an ally would take is dealt to you instead.",
  """public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
        => target.IsPlayer && target != Owner && Owner.IsAlive ? Owner : target;"""),
power("AuraOfMercy","Aura of Mercy","Buff","Counter",
  "Whenever an ally is healed, every other ally heals [blue]1[/blue] HP.","Whenever an ally is healed, every other ally heals [blue]{Amount}[/blue] HP.",
  f"""private bool _busy;
    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {{
        if (_busy || delta <= 0m || !creature.IsPlayer || creature == Owner) return;
        _busy = true;
        try {{ foreach (var ally in {ALLIES}.Where(c => c != creature)) await CreatureCmd.Heal(ally, Amount); }}
        finally {{ _busy = false; }}
    }}"""),
power("PrayerOfMending","Prayer of Mending","Buff","Counter",
  "At the start of your next turn, heal [blue]4[/blue] HP.","At the start of your next turn, heal [blue]{Amount}[/blue] HP.",
  """public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player.Creature != Owner) return;
        Flash();
        await CreatureCmd.Heal(Owner, Amount);
        await PowerCmd.Remove(this);
    }"""),
power("Judged","Judged","Debuff","Counter",
  "Takes [blue]4[/blue] additional damage from all sources this turn.","Takes [blue]{Amount}[/blue] additional damage from all sources this turn.",
  """public override decimal ModifyDamageAdditive(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        => target == Owner && amount > 0m ? amount + Amount : amount;
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Owner.Side) await PowerCmd.Remove(this);
    }"""),
power("Consecration","Consecration","Buff","Counter",
  "At the start of your turn, deal [blue]3[/blue] damage to ALL enemies.","At the start of your turn, deal [blue]{Amount}[/blue] damage to ALL enemies.",
  f"""public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {{
        if (player.Creature != Owner) return;
        Flash();
        await CreatureCmd.Damage(choiceContext, {ENEMIES}.ToList(), (decimal)Amount, ValueProp.Unpowered, Owner);
    }}"""),
power("CrusaderAura","Crusader Aura","Buff","Counter",
  "All allies' Attacks deal [blue]1[/blue] additional damage.","All allies' Attacks deal [blue]{Amount}[/blue] additional damage.",
  """public override decimal ModifyDamageAdditive(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        => dealer != null && dealer.IsPlayer && cardSource != null && !target.IsPlayer ? amount + Amount : amount;"""),
power("SealOfCommand","Seal of Command","Buff","Counter",
  "For [blue]3[/blue] turns, your Attacks deal [blue]2[/blue] additional damage.","For [blue]{Amount}[/blue] turns, your Attacks deal [blue]2[/blue] additional damage.",
  """public override decimal ModifyDamageAdditive(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        => dealer == Owner && cardSource != null ? amount + 2m : amount;""", base="TimedPaladinPower"),
power("SealOfLight","Seal of Light","Buff","Counter",
  "For [blue]3[/blue] turns, whenever you play an Attack, heal [blue]2[/blue] HP.","For [blue]{Amount}[/blue] turns, whenever you play an Attack, heal [blue]2[/blue] HP.",
  """public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner?.Creature != Owner || cardPlay.Card.Type != CardType.Attack) return;
        await CreatureCmd.Heal(Owner, 2m);
    }""", base="TimedPaladinPower"),
power("HallowedGround","Hallowed Ground","Buff","Counter",
  "Whenever you gain [gold]Faith[/gold] in your highest deity, gain [blue]1[/blue] [gold]Block[/gold].","Whenever you gain [gold]Faith[/gold] in your highest deity, gain [blue]{Amount}[/blue] [gold]Block[/gold].",
  """private int _last = -1;
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Owner.Player == null) return;
        var now = FaithTracks.Highest(Owner.Player).amount;
        if (_last >= 0 && now > _last) { Flash(); await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Unpowered, null); }
        _last = now;
    }"""),
power("ArdentDefender","Ardent Defender","Buff","Single",
  "The first time you would die this combat, instead heal to a third of your Max HP.","The first time you would die this combat, instead heal to a third of your Max HP.",
  """private bool _used;
    public override bool ShouldDie(Creature creature)
    {
        if (creature != Owner || _used) return true;
        _used = true;
        Flash();
        _ = CreatureCmd.SetCurrentHp(Owner, Math.Max(1, Owner.MaxHp / 3));
        _ = PowerCmd.Remove(this);
        return false;
    }"""),
power("DivineAllegiance","Divine Allegiance","Buff","Single",
  "Damage an ally would take is dealt to you instead.","Damage an ally would take is dealt to you instead.",
  """public override Creature ModifyUnblockedDamageTarget(Creature target, decimal amount, ValueProp props, Creature? dealer)
        => target.IsPlayer && target != Owner && Owner.IsAlive ? Owner : target;"""),
power("AuraOfDevotion","Aura of Devotion","Buff","Counter",
  "All allies take [blue]1[/blue] less damage from attacks for every 5 [gold]Faith[/gold] in [gold]Torm[/gold].","All allies take [blue]{Amount}[/blue] less damage from attacks for every 5 [gold]Faith[/gold] in [gold]Torm[/gold].",
  """public override decimal ModifyDamageAdditive(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!target.IsPlayer || dealer == null || dealer.IsPlayer || Owner.Player == null) return amount;
        var reduce = Amount * (FaithTracks.Effective(Owner.Player, Deity.Torm) / 5);
        return Math.Max(0m, amount - reduce);
    }"""),
power("GuardianOfAncientKings","Guardian of Ancient Kings","Buff","Counter",
  "For [blue]3[/blue] turns, all damage you take is halved.","For [blue]{Amount}[/blue] turns, all damage you take is halved.",
  """public override decimal ModifyDamageMultiplicative(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        => target == Owner ? amount * 0.5m : amount;""", base="TimedPaladinPower"),
power("BeaconOfLight","Beacon of Light","Buff","Single",
  "Whenever you play a card that heals, the chosen ally also heals half that much.","Whenever you play a card that heals, the chosen ally also heals half that much.",
  """public Creature? Beneficiary { get; set; }
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (Beneficiary == null || !Beneficiary.IsAlive || cardPlay.Card.Owner?.Creature != Owner) return;
        if (!cardPlay.Card.DynamicVars.Keys.Cast<string>().Contains("Heal")) return;
        Flash();
        await CreatureCmd.Heal(Beneficiary, cardPlay.Card.DynamicVars.Heal.BaseValue / 2);
    }"""),
power("AuraOfVitality","Aura of Vitality","Buff","Counter",
  "At the start of your turn, all allies heal [blue]3[/blue] HP.","At the start of your turn, all allies heal [blue]{Amount}[/blue] HP.",
  f"""public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {{
        if (player.Creature != Owner) return;
        Flash();
        foreach (var ally in {ALLIES}) await CreatureCmd.Heal(ally, Amount);
    }}"""),
power("TheBrokenGod","The Broken God","Buff","Single",
  "At the start of your turn, all allies heal HP equal to half your [gold]Faith[/gold] in [gold]Ilmater[/gold].","At the start of your turn, all allies heal HP equal to half your [gold]Faith[/gold] in [gold]Ilmater[/gold].",
  f"""public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {{
        if (player.Creature != Owner) return;
        var amount = FaithTracks.Effective(player, Deity.Ilmater) / 2;
        if (amount <= 0) return;
        Flash();
        foreach (var ally in {ALLIES}) await CreatureCmd.Heal(ally, amount);
    }}"""),
power("VowOfEnmity","Vow of Enmity","Buff","Counter",
  "Your Attacks against the sworn enemy deal [blue]4[/blue] additional damage.","Your Attacks against the sworn enemy deal [blue]{Amount}[/blue] additional damage.",
  """public Creature? Foe { get; set; }
    public override decimal ModifyDamageAdditive(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        => dealer == Owner && cardSource != null && Foe != null && target == Foe ? amount + Amount : amount;"""),
power("EyeForAnEye","Eye for an Eye","Buff","Single",
  "Whenever an enemy attacks you, deal that much damage back to it.","Whenever an enemy attacks you, deal that much damage back to it.",
  """public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != Owner || dealer == null || dealer.IsPlayer || amount <= 0m) return;
        Flash();
        await CreatureCmd.Damage(choiceContext, dealer, amount, ValueProp.Unpowered | ValueProp.SkipHurtAnim, Owner, null);
    }"""),
power("AvengingWrath","Avenging Wrath","Buff","Counter",
  "For [blue]3[/blue] turns, your Attacks deal [blue]5[/blue] additional damage.","For [blue]{Amount}[/blue] turns, your Attacks deal [blue]5[/blue] additional damage.",
  """public override decimal ModifyDamageAdditive(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        => dealer == Owner && cardSource != null ? amount + 5m : amount;""", base="TimedPaladinPower"),
]

# --------------------------------------------------------------------------- emit

CARD_USINGS = """using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Characters.PaladinContent.Faith;
using HelloSpire.HelloSpireCode.Characters.PaladinContent.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;
"""

POWER_USINGS = """using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Characters.PaladinContent.Faith;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Powers;
"""

def emit_card(c):
    plain = re.sub(r"\[/?\w+\]", "", c["desc"]).replace("\n", " ")
    plain = re.sub(r"\{(\w+):[^}]*\}", r"{\1}", plain)
    lines = [f"// <auto-generated> tools/gen_paladin.py -- edit the spec, not this file. </auto-generated>", CARD_USINGS,
             f"/// <summary>{plain}</summary>",
             f'public sealed class {c["name"]}() : PaladinCard({c["cost"]}, CardType.{c["type"]}, CardRarity.{c["rarity"]}, TargetType.{c["target"]})', "{"]
    if c["gb"]: lines.append("    public override bool GainsBlock => true;")
    if c["mp"]: lines.append("    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;")
    if c["kw"]: lines.append("    public override IEnumerable<CardKeyword> CanonicalKeywords => [" + ", ".join(f"CardKeyword.{k}" for k in c["kw"]) + "];")
    if c["req"]:
        d, n = c["req"]
        lines.append(f"    // Requires {n} Faith in {d}. Outside combat the card reads as playable, like the base class.")
        lines.append(f"    protected override bool IsPlayable => Owner?.PlayerCombatState == null || FaithTracks.Has(Owner, Deity.{d}, {n});")
    vars_ = ", ".join(v[1] for v in c["vars"])
    lines.append(f"    protected override IEnumerable<DynamicVar> CanonicalVars => [{vars_}];")
    if c["tips"]: lines.append("    protected override IEnumerable<IHoverTip> ExtraHoverTips => [" + ", ".join(f"HoverTipFactory.FromPower<{t}>()" for t in c["tips"]) + "];")
    lines.append("")
    lines.append("    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)")
    lines.append("    {")
    for b in c["body"]:
        for l in b.split("\n"): lines.append("        " + l)
    lines.append("    }")
    lines.append("")
    up = " ".join(c["up"]) if c["up"] else ""
    lines.append(f"    protected override void OnUpgrade() {{ {up} }}")
    lines.append("")
    lines.append("    private IEnumerable<Creature> Allies()  => Owner.Creature.CombatState.PlayerCreatures.OfType<Creature>().Where(c => c.IsAlive);")
    lines.append("    private IEnumerable<Creature> Enemies() => Owner.Creature.CombatState.Creatures.OfType<Creature>().Where(c => !c.IsPlayer && c.IsHittable);")
    lines.append("}")
    return "\n".join(lines) + "\n"

def emit_power(p):
    plain = re.sub(r"\[/?\w+\]", "", p["desc"])
    own_type = p["base"] == "PaladinPower"
    lines = ["// <auto-generated> tools/gen_paladin.py -- edit the spec, not this file. </auto-generated>", POWER_USINGS,
             f"/// <summary>{plain}</summary>",
             f'public sealed class {p["name"]}Power : {p["base"]}', "{"]
    if own_type:
        lines.append(f'    public override PowerType Type => PowerType.{p["ptype"]};')
        lines.append(f'    public override PowerStackType StackType => PowerStackType.{p["stack"]};')
    lines.append("")
    lines.append("    " + p["body"])
    lines.append("}")
    return "\n".join(lines) + "\n"

def write(path, text):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    io.open(path, "w", encoding="utf-8", newline="\n").write(text)

def merge_loc(fname, entries):
    p = os.path.join(LOC, fname)
    d = json.load(io.open(p, encoding="utf-8-sig"), object_pairs_hook=collections.OrderedDict)
    d.update(entries)
    io.open(p, "w", encoding="utf-8").write(json.dumps(d, indent=2, ensure_ascii=False) + "\n")
    return len(d)

GOLD = (232, 196, 106); DARK = (90, 72, 30)
def tile(w, h, text, size, color=GOLD):
    im = Image.new("RGBA", (w, h), color + (255,)); d = ImageDraw.Draw(im)
    d.rectangle([4, 4, w - 5, h - 5], outline=DARK + (255,), width=6)
    f = ImageFont.truetype("C:/Windows/Fonts/arialbd.ttf", size)
    bb = d.multiline_textbbox((0, 0), text, font=f, align="center")
    d.multiline_text(((w - (bb[2] - bb[0])) / 2, (h - (bb[3] - bb[1])) / 2 - bb[1]), text, fill=(255, 255, 255, 255), font=f, align="center")
    return im
def label(title):
    w = title.split(); return "\n".join([" ".join(w[:2]), " ".join(w[2:])]).strip().upper()

def main():
    cards_loc, powers_loc = {}, {}
    n_cards = n_powers = n_art = 0
    for c in CARDS:
        write(os.path.join(CARDS_DIR, c["name"] + ".cs"), emit_card(c)); n_cards += 1
        cards_loc[key(c["name"]) + ".title"] = c["title"]; cards_loc[key(c["name"]) + ".description"] = c["desc"]
        s = snake(c["name"])
        for sub, w, h, sz in (("", 250, 190, 22), ("big", 1000, 760, 60)):
            p = os.path.join(IMG, "card_portraits", sub, s + ".png")
            if not os.path.exists(p): tile(w, h, label(c["title"]), sz).save(p); n_art += 1
    for p_ in POWERS:
        write(os.path.join(POWERS_DIR, p_["name"] + "Power.cs"), emit_power(p_)); n_powers += 1
        k = key(p_["name"] + "Power")
        powers_loc[k + ".title"] = p_["title"]; powers_loc[k + ".description"] = p_["desc"]; powers_loc[k + ".smartDescription"] = p_["smart"]
        s = snake(p_["name"] + "Power")
        for sub, w, h, sz in (("", 128, 128, 40), ("big", 256, 256, 64)):
            p = os.path.join(IMG, "powers", sub, s + ".png")
            if not os.path.exists(p): tile(w, h, label(p_["title"])[:12], sz).save(p); n_art += 1
    # IntangiblePower is base-game; its loc and art ship with the game.
    tc = merge_loc("cards.json", cards_loc); tp = merge_loc("powers.json", powers_loc)
    print(f"cards: {n_cards} files | powers: {n_powers} files | loc: cards.json={tc} powers.json={tp} | new art tiles: {n_art}")

if __name__ == "__main__":
    main()
