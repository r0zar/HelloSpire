using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// X-cost: deal 5 damage X times, Judge ×X. Strength multiplies per hit, seals multiply per
/// judge -- the Ret finisher. Upgrade: Retain, carry the wrath until the turn that deserves it.
/// </summary>
public sealed class HolyWrath() : PaladinCard(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override bool HasEnergyCostX => true;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Judge)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var times = ResolveEnergyXValue();
        if (times <= 0) return;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithAttackerAnim(HeavyAttackAnim, HeavyAttackDelay).Targeting(cardPlay.Target)
            .WithHitCount(times)
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(choiceContext);
        await Seals.Judge(choiceContext, Owner, cardPlay.Target, times);
    }

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
