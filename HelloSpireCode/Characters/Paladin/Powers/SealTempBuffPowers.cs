using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using HelloSpire.HelloSpireCode.Extensions;
using HelloSpire.HelloSpireCode.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The seals' one-turn cast buffs. No seal leaves anything permanent behind except its banked
/// judge charge: Strength seals surge for the turn (the Dark Shackles pattern, positive sign),
/// and the Martyr's Thorns flare and fade the same way. Recurring permanent stats per deck
/// cycle made act 1 trivial.
/// </summary>
public sealed class SealOfRighteousnessStrengthPower : TemporaryStrengthPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Cards.SealOfRighteousness>();
    public string CustomPackedIconPath => "seal_of_righteousness_power.png".PowerImagePath();
    public string CustomBigIconPath => "seal_of_righteousness_power.png".BigPowerImagePath();
}

public sealed class SealOfTheCrusaderStrengthPower : TemporaryStrengthPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Cards.SealOfTheCrusader>();
    public string CustomPackedIconPath => "seal_of_the_crusader_power.png".PowerImagePath();
    public string CustomBigIconPath => "seal_of_the_crusader_power.png".BigPowerImagePath();
}

/// <summary>
/// Temporary Thorns, hand-rolled on the TemporaryStrengthPower bookkeeping (the engine has no
/// Thorns analog): applying it silently applies Thorns; at end of the side turn it removes
/// itself and takes the Thorns with it.
/// </summary>
public sealed class SealOfTheMartyrThornsPower : HelloSpirePower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier,
        CardModel? cardSource) =>
        await PowerCmd.Apply<ThornsPower>(new ThrowingPlayerChoiceContext(), target, amount,
            applier, cardSource, silent: true);

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext,
        PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this && amount != Amount)
            await PowerCmd.Apply<ThornsPower>(choiceContext, Owner, amount, applier, cardSource, silent: true);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner)) return;
        Flash();
        await PowerCmd.Remove(this);
        await PowerCmd.Apply<ThornsPower>(choiceContext, Owner, -Amount, Owner, null);
    }
}
