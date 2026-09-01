using BaseLib.Abstracts;
using HelloSpire.HelloSpireCode.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The one-turn Strength loss the Humility and Martyr judges apply -- the Dark Shackles pattern:
/// TemporaryStrengthPower handles the apply/restore bookkeeping and borrows the vanilla
/// Temporary Strength Down text, so this class only names its origin. (Successor to the retired
/// Seal of Justice shackles; keeps its icon.)
/// </summary>
public sealed class HumblingShacklesPower : TemporaryStrengthPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Cards.SealOfHumility>();

    protected override bool IsPositive => false;

    public string CustomPackedIconPath => "seal_of_justice_shackles_power.png".PowerImagePath();
    public string CustomBigIconPath => "seal_of_justice_shackles_power.png".BigPowerImagePath();
}
