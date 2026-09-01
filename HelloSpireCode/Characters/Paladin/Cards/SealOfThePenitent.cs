using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Gain Amount Block. Discard a card. Bank the seal. Judge: return a random card from your
/// discard pile to your hand. The confession loop: penance now, absolution at judgment --
/// tithe a heal for its face, then judge it back for the true cast.
/// </summary>
public sealed class SealOfThePenitent() : SealCard(1, CardRarity.Common, 3m)
{
    public override bool GainsBlock => true;

    protected override async Task Arm(PlayerChoiceContext ctx, decimal amount)
    {
        await CreatureCmd.GainBlock(Owner.Creature, amount, ValueProp.Move, null);
        await PaladinEffects.DiscardChosen(ctx, Owner, 1, this);
        await Seals.Grant<SealOfThePenitentPower>(ctx, Owner, 1m, this);
    }
}
