using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Heal a player 12 + Spirit. Exhaust. Tithe: deal 4 to ALL enemies and apply 1 Weak.
/// The big single heal below Lay on Hands. The face was a flat 8 to a random enemy -- Holy
/// Shock's identity in a trench coat; now it's the sunburst: pitched light scorches and
/// dazzles the whole room.
/// </summary>
public sealed class HolyLight() : PaladinCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyPlayer), IHealingCard
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new SpiritHealVar(12m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var target = TargetOrOwner(cardPlay);
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Spirit.Heal(Owner, target, DynamicVars.Heal.BaseValue);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx)
    {
        if (Owner.Creature.CombatState is not { } state) return;
        var enemies = state.HittableEnemies.ToList();
        if (enemies.Count == 0) return;
        await CreatureCmd.Damage(ctx, enemies, 4m, ValueProp.Unpowered, Owner.Creature);
        foreach (var enemy in enemies)
            if (!enemy.IsDead)
                await PowerCmd.Apply<WeakPower>(ctx, enemy, 1m, Owner.Creature, null);
    }

    protected override void OnUpgrade() => DynamicVars.Heal.UpgradeValueBy(4m);
}
