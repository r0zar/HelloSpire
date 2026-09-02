using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Draw Amount cards now, bank the seal. Judge: draw 2, then discard 1. The cantrip seal.</summary>
public sealed class SealOfWisdom() : SealCard(1, CardRarity.Uncommon, 1m)
{
    protected override async Task Arm(PlayerChoiceContext ctx, decimal amount)
    {
        await CardPileCmd.Draw(ctx, amount, Owner);
        await Seals.Grant<SealOfWisdomPower>(ctx, Owner, 1m, this);
    }
}
