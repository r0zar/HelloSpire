using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Gain Amount Spirit now, and arm the seal. Judge: draw 3, discard 2. The per-turn tick died
/// in playtest -- small early decks cycle Judgment so fast the seal never got a second turn,
/// so the Spirit pays up front.
/// </summary>
public sealed class SealOfLight() : SealCard(1, CardRarity.Uncommon, 1m)
{
    protected override async Task Arm(PlayerChoiceContext ctx, decimal amount)
    {
        await Spirit.Gain(ctx, Owner, (int)amount, this);
        await Seals.Grant<SealOfLightPower>(ctx, Owner, amount, this);
    }
}
