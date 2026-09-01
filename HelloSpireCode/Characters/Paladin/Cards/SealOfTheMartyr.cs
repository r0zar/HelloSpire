using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Gain Amount Thorns now, bank the seal. Judge: ALL enemies lose 5 Strength this turn.
/// The Prot emergency: bleed while you hold, save the team when you cash.
/// </summary>
public sealed class SealOfTheMartyr() : SealCard(1, CardRarity.Rare, 3m)
{
    protected override async Task Arm(PlayerChoiceContext ctx, decimal amount)
    {
        await PowerCmd.Apply<ThornsPower>(ctx, Owner.Creature, amount, Owner.Creature, this);
        await Seals.Grant<SealOfTheMartyrPower>(ctx, Owner, amount, this);
    }
}
