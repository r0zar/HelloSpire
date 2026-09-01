using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Deal Amount to a random enemy, discard a card, bank the seal. Judge: deal 6 to ALL.
/// The Tithe seal: the discard fires faces and feeds every discard payoff on the way in.
/// </summary>
public sealed class SealOfThePenitent() : SealCard(1, CardRarity.Common, 2m)
{
    protected override async Task Arm(PlayerChoiceContext ctx, decimal amount)
    {
        var enemy = PaladinEffects.RandomEnemy(Owner);
        if (enemy != null)
            await CreatureCmd.Damage(ctx, [enemy], amount, ValueProp.Unpowered, Owner.Creature);
        await PaladinEffects.DiscardChosen(ctx, Owner, 1, this);
        await Seals.Grant<SealOfThePenitentPower>(ctx, Owner, amount, this);
    }
}
