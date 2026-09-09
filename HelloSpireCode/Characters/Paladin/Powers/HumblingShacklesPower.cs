using BaseLib.Abstracts;
using HelloSpire.HelloSpireCode.Extensions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The one-turn Strength loss the Humility and Martyr judges apply -- the Dark Shackles pattern:
/// TemporaryStrengthPower handles the apply/restore bookkeeping and borrows the vanilla
/// Temporary Strength Down text, so this class only names its origin. (Successor to the retired
/// Seal of Justice shackles; it now carries its own icon rather than the retired seal's, which
/// was named for a card that no longer exists.)
/// </summary>
public sealed class HumblingShacklesPower : TemporaryStrengthPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<Cards.SealOfHumility>();

    protected override bool IsPositive => false;

    public string CustomPackedIconPath => "humbling_shackles_power.png".PowerImagePath();
    public string CustomBigIconPath => "humbling_shackles_power.png".BigPowerImagePath();
}
