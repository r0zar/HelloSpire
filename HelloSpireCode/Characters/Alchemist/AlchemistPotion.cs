using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using HelloSpire.HelloSpireCode.Extensions;

namespace HelloSpire.HelloSpireCode.Characters;

/// <summary>Base class for Alchemist potions. [Pool] routes subclasses into the Alchemist's potion pool.</summary>
[Pool(typeof(AlchemistPotionPool))]
public abstract class AlchemistPotion : CustomPotionModel
{
    public override string? CustomPackedImagePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
    public override string? CustomPackedOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionOutlineImagePath();
}
