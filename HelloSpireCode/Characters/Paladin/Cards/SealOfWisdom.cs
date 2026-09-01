using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Seal. While held: first Attack each turn draws Amount. Judge: draw 2, discard 1.</summary>
public sealed class SealOfWisdom() : SealCard(1, CardRarity.Uncommon, 1m)
{
    protected override Task Arm(PlayerChoiceContext ctx, decimal amount) =>
        Seals.Grant<SealOfWisdomPower>(ctx, Owner, amount, this);
}
