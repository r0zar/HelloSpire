using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The one-turn Strength loss Seal of Justice's passive applies -- the Dark Shackles pattern:
/// TemporaryStrengthPower handles the apply/restore bookkeeping and borrows the origin card's
/// title and the vanilla Temporary Strength Down text, so this class only names its origin.
/// </summary>
public sealed class SealOfJusticeShacklesPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Cards.SealOfJustice>();

    protected override bool IsPositive => false;
}
