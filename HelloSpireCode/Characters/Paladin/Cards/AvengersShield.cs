using System;
using System.Collections.Generic;
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
/// Deal 8. Gain 2 Plating. Judge. The whack that armors -- the Prot rhythm card: swing, the
/// wall regrows behind you, and the shield procs the verdict (Prot's drafted judge now that
/// Judgment left the starter).
/// </summary>
public sealed class AvengersShield() : PaladinCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(8m, ValueProp.Move), new DynamicVar("Plating", 2m)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Judge)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).WithAttackerAnim(HeavyAttackAnim, HeavyAttackDelay).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature,
            DynamicVars["Plating"].BaseValue, Owner.Creature, this);
        await Seals.Judge(choiceContext, Owner, cardPlay.Target);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["Plating"].UpgradeValueBy(1m);
    }
}
