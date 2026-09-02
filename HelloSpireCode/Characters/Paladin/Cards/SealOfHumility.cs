using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Gain Amount Block now, bank the seal. Judge: enemy loses 3 Strength this turn.
/// The humble seal: quiet defense in, defensive valve out. Never a heal -- no loops.
/// </summary>
public sealed class SealOfHumility() : SealCard(1, CardRarity.Common, 3m)
{
    public override bool GainsBlock => true;

    protected override async Task Arm(PlayerChoiceContext ctx, decimal amount)
    {
        await CreatureCmd.GainBlock(Owner.Creature, amount, ValueProp.Move, null);
        await Seals.Grant<SealOfHumilityPower>(ctx, Owner, 1m, this);
    }
}
