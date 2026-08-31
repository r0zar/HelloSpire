using BaseLib.Abstracts;
using HelloSpire.HelloSpireCode.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The one-turn Strength loss Seal of Justice's passive applies -- the Dark Shackles pattern:
/// TemporaryStrengthPower handles the apply/restore bookkeeping and borrows the origin card's
/// title and the vanilla Temporary Strength Down text, so this class only names its origin.
/// </summary>
public sealed class SealOfJusticeShacklesPower : TemporaryStrengthPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Cards.SealOfJustice>();

    protected override bool IsPositive => false;

    // ICustomPower, not HelloSpirePower: the mechanics need the TemporaryStrengthPower base, and
    // BaseLib's icon patch keys on the interface, so this is how a vanilla-based power gets a
    // custom icon (a red-shifted variant of the seal's own medallion).
    public string CustomPackedIconPath => "seal_of_justice_shackles_power.png".PowerImagePath();
    public string CustomBigIconPath => "seal_of_justice_shackles_power.png".BigPowerImagePath();
}
