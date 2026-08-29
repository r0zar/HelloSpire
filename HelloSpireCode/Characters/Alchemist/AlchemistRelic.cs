using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using HelloSpire.HelloSpireCode.Extensions;

namespace HelloSpire.HelloSpireCode.Characters;

/// <summary>Base class for Alchemist relics. [Pool] routes subclasses into the Alchemist's relic pool.</summary>
[Pool(typeof(AlchemistRelicPool))]
public abstract class AlchemistRelic : CustomRelicModel
{
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();
    protected override string PackedIconOutlinePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();
    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
}
