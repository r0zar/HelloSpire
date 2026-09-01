using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Gain Amount Plating now, and bank the seal. Judge: gain 10 Block. The per-turn tick paid
/// up front -- fast Judgment cycling never gave it a second turn.
/// </summary>
public sealed class SealOfFortitude() : SealCard(1, CardRarity.Common, 2m)
{
    protected override async Task Arm(PlayerChoiceContext ctx, decimal amount)
    {
        await PowerCmd.Apply<PlatingPower>(ctx, Owner.Creature, amount, Owner.Creature, this);
        await Seals.Grant<SealOfFortitudePower>(ctx, Owner, amount, this);
    }
}
