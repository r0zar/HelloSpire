using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Deal 9. Apply 1 Weak. Gain 1 Plating. WoW Prot's signature swing: the hit that saps the
/// attacker and thickens the wall. Takes the uncommon slot the old Blessing of Protection
/// held -- and chips at the uncommon-Attack curve deficit.
/// </summary>
public sealed class HammerOfTheRighteous() : PaladinCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar(9m, ValueProp.Move), new DynamicVar("Weak", 1m), new DynamicVar("Plating", 1m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
        if (!cardPlay.Target.IsDead)
            await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target,
                DynamicVars["Weak"].BaseValue, Owner.Creature, this);
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner.Creature,
            DynamicVars["Plating"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
        DynamicVars["Plating"].UpgradeValueBy(1m);
    }
}
