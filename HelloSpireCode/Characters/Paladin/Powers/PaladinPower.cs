using BaseLib.Abstracts;
using BaseLib.Extensions;
using HelloSpire.HelloSpireCode.Extensions;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Powers;

public abstract class PaladinPower : CustomPowerModel
{
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
}
