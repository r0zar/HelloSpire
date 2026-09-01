using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Deal damage equal to your Plating + Block. Body Slam, Paladin dialect -- the Prot payoff
/// attack. Ardent Defender, Fortitude, and the starter relic all feed it.
/// </summary>
public sealed class ShieldBash() : PaladinCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        var creature = Owner.Creature;
        var amount = creature.GetPowerAmount<PlatingPower>() + creature.Block;
        if (amount <= 0) return;
        await DamageCmd.Attack(amount).FromCard(this).Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_heavy_blunt")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
