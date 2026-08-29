using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using HelloSpire.HelloSpireCode.Extensions;
using HelloSpire.HelloSpireCode.Gunslinger.Cards;
using HelloSpire.HelloSpireCode.Gunslinger.Relics;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace HelloSpire.HelloSpireCode.Character;

/// <summary>
/// The Gunslinger: a sequencing character built around a visible six-chamber revolver.
///
/// The gun is the whole character. Ammunition has to be loaded before it can be spent, the order of
/// the chambers is knowable and manipulable, and almost every card either fills the cylinder, spends
/// it, or rearranges what is coming next. Everything else — Deadeye, Armor, Dodge, Weak — exists to
/// give the player something to do with the two or three chambers they can see ahead.
/// </summary>
public class TheGunslinger : PlaceholderCharacterModel
{
    public const string CharacterId = "TheGunslinger";

    /// <summary>Weathered brass and sun-bleached leather.</summary>
    public static readonly Color Color = new("d9a05b");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;

    /// <summary>Middling HP: the Gunslinger defends in layers rather than by having a big pool.</summary>
    public override int StartingHp => 72;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeGunslinger>(),
        ModelDb.Card<StrikeGunslinger>(),
        ModelDb.Card<StrikeGunslinger>(),
        ModelDb.Card<StrikeGunslinger>(),
        ModelDb.Card<DefendGunslinger>(),
        ModelDb.Card<DefendGunslinger>(),
        ModelDb.Card<DefendGunslinger>(),
        ModelDb.Card<DefendGunslinger>(),
        ModelDb.Card<Reload>(),
        ModelDb.Card<QuickDraw>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<OldIron>()];

    public override CardPoolModel CardPool => ModelDb.CardPool<HelloSpireCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<HelloSpireRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<HelloSpirePotionPool>();

    /*  PlaceholderCharacterModel falls back to base-game assets for anything not overridden here.
        Art lives in HelloSpire/images/charui/ — swap these files rather than these paths. */
    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath => "character_icon_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_char_name.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_char_name_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_char_name.png".CharacterUiPath();
}
