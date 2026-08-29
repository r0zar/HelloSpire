using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using HelloSpire.HelloSpireCode.Extensions;

namespace HelloSpire.HelloSpireCode.Characters;

/// <summary>Base class for Paladin potions. [Pool] routes subclasses into the Paladin's potion pool.</summary>
[Pool(typeof(PaladinPotionPool))]
public abstract class PaladinPotion : CustomPotionModel
{
    public override string? CustomPackedImagePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
    public override string? CustomPackedOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionOutlineImagePath();
}
