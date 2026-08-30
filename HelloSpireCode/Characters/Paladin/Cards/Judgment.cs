using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Deal 10, then trigger the effects of all your Seals. Unplayable without a Seal. The class
/// signature: a real hit whose rider scales with every Seal you have collected. Upgrade: 0E.
/// </summary>
public sealed class Judgment() : PaladinCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10m, ValueProp.Move)];

    // Outside combat the card reads as playable, like the base class.
    protected override bool IsPlayable =>
        Owner?.PlayerCombatState == null || Seals.Active(Owner.Creature) != null;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        await Seals.Judge(choiceContext, Owner, cardPlay.Target);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
