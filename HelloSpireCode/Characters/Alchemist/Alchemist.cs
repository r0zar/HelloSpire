using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using HelloSpire.HelloSpireCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;

using Alchemist_ = HelloSpire.HelloSpireCode.Alchemist;

namespace HelloSpire.HelloSpireCode.Characters;

public class Alchemist : PlaceholderCharacterModel
{
    public const string CharacterId = "Alchemist";

    /// <summary>Asset subfolder under images/charui/ for this character's UI.</summary>
    public const string AssetFolder = "alchemist";

    public static readonly Color Color = new("b5824a");

    /// <summary>The Silent's rig fits the wiry alchemist far better than the Ironclad's bulk.
    /// Drives combat body, rest-site and shop scenes, trail, energy counter and sfx.</summary>
    public override string PlaceholderID => "silent";

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 68;

    // The teaching ten: Strikes, Defends, Aegis Formula (Brew) and Infusion (Infuse) -- the two
    // mechanics this class is built around, both taught from turn one.
    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<Alchemist_.Cards.StrikeAlchemist>(),
        ModelDb.Card<Alchemist_.Cards.StrikeAlchemist>(),
        ModelDb.Card<Alchemist_.Cards.StrikeAlchemist>(),
        ModelDb.Card<Alchemist_.Cards.StrikeAlchemist>(),
        ModelDb.Card<Alchemist_.Cards.DefendAlchemist>(),
        ModelDb.Card<Alchemist_.Cards.DefendAlchemist>(),
        ModelDb.Card<Alchemist_.Cards.DefendAlchemist>(),
        ModelDb.Card<Alchemist_.Cards.DefendAlchemist>(),
        ModelDb.Card<Alchemist_.Cards.AegisFormula>(),
        ModelDb.Card<Alchemist_.Cards.Infusion>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
        [ModelDb.Relic<Alchemist_.Relics.AlchemicalSatchel>()];

    public override CardPoolModel CardPool => ModelDb.CardPool<AlchemistCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<AlchemistRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<AlchemistPotionPool>();

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }


    // In-combat body: the inherited Ironclad rig, repainted by the CharacterSkins shader patch.

    /// <summary>The character-select backsplash: same shape as the Paladin's scene.</summary>
    public override string CustomCharacterSelectBg => "res://HelloSpire/scenes/char_select_bg_alchemist.tscn";

    public override string CustomIconTexturePath => "character_icon.png".CharacterUiPath(AssetFolder);
    public override string CustomIconOutlineTexturePath => "character_icon_outline.png".CharacterUiPath(AssetFolder);
    public override string CustomCharacterSelectIconPath => "char_select.png".CharacterUiPath(AssetFolder);
    public override string CustomCharacterSelectLockedIconPath => "char_select_locked.png".CharacterUiPath(AssetFolder);
    public override string CustomMapMarkerPath => "map_marker.png".CharacterUiPath(AssetFolder);
}
