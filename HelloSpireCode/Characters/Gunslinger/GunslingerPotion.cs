using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using HelloSpire.HelloSpireCode.Extensions;

namespace HelloSpire.HelloSpireCode.Characters;

/// <summary>Base class for Gunslinger potions. [Pool] routes subclasses into the Gunslinger's potion pool.</summary>
[Pool(typeof(GunslingerPotionPool))]
public abstract class GunslingerPotion : CustomPotionModel
{
    public override string? CustomPackedImagePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();
    public override string? CustomPackedOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionOutlineImagePath();
}
