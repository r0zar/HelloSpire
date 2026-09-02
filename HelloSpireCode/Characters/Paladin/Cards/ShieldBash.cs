using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Deal damage equal to your Plating. Apply 1 Weak. Free -- the tank's strike: the wall
/// itself swings, and the attacker comes away sapped. Upgrade: 2 Weak.
/// </summary>
public sealed class ShieldBash() : PaladinCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Weak", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var amount = Owner.Creature.GetPowerAmount<PlatingPower>();
        if (amount > 0)
            await DamageCmd.Attack(amount).FromCard(this).Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_heavy_blunt")
                .Execute(choiceContext);
        if (!cardPlay.Target.IsDead)
            await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target,
                DynamicVars["Weak"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Weak"].UpgradeValueBy(1m);
}
