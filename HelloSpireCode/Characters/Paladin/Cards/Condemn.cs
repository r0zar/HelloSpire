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
/// Deal 6. Apply 1 Vulnerable. Tithe: gain 5 Block. Replaces Absolve -- condemned to judgment,
/// or pitched as a shield.
/// </summary>
public sealed class Condemn() : PaladinCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);
        if (!cardPlay.Target.IsDead)
            await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, 1m, Owner.Creature, this);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx) =>
        await CreatureCmd.GainBlock(Owner.Creature, 5m, ValueProp.Unpowered, null);

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
