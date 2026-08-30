using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Deal 3, plus 3 for each of your Seals. The D&D smite: divinity channelled into the blow.
/// Replaces the old Bash-copy (Crusader Strike owns that slot now) with a new paladin shape --
/// seal-count as a damage axis.
/// </summary>
public sealed class DivineSmite() : PaladinCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    public const decimal PerSeal = 3m;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var seals = Owner.Creature.GetPowerInstances<SealPower>().Count();
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + PerSeal * seals).FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3m);
}
