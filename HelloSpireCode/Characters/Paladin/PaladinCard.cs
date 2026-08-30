using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Characters.PaladinContent;
using HelloSpire.HelloSpireCode.Characters.PaladinContent.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using HelloSpire.HelloSpireCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace HelloSpire.HelloSpireCode.Characters;

/// <summary>
/// Base class for Paladin cards. The [Pool] attribute puts every subclass into the
/// Paladin's card pool automatically, so individual cards never declare it.
/// Card art resolves by class name, which is unique mod-wide, so all characters
/// share the images/card_portraits tree.
/// </summary>
[Pool(typeof(PaladinCardPool))]
public abstract class PaladinCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    //Normal art: 1000x760   Full art: 606x852
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();

    //Small variants: normal 250x190, fullart 250x350
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    /// <summary>Blessed: pulse every Aura the owner has. Snapshot first, since a pulse may add or remove powers.</summary>
    protected async Task PulseAuras(PlayerChoiceContext choiceContext)
    {
        var auras = Owner.Creature.Powers.OfType<IAura>().ToList();
        foreach (var aura in auras) await aura.Pulse(choiceContext);
    }

    protected static IHoverTip Tip(MegaCrit.Sts2.Core.HoverTips.StaticHoverTip tip) => HoverTipFactory.Static(tip);
}
