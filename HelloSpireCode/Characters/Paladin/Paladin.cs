using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using HelloSpire.HelloSpireCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;
using HelloSpire.HelloSpireCode.Characters.PaladinContent.Relics;

namespace HelloSpire.HelloSpireCode.Characters;

public class Paladin : PlaceholderCharacterModel
{
    public const string CharacterId = "Paladin";

    /// <summary>Asset subfolder under images/charui/ for this character's UI.</summary>
    public const string AssetFolder = "paladin";

    public static readonly Color Color = new("e8c46a");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 75;

    // The starter: 4 Strike / 4 Defend / 1 Mend / 1 Judgment. The Paladin is being built
    // one card at a time from here; every addition is a deliberate decision, not a batch.
    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikePaladin>(),
        ModelDb.Card<StrikePaladin>(),
        ModelDb.Card<StrikePaladin>(),
        ModelDb.Card<StrikePaladin>(),
        ModelDb.Card<DefendPaladin>(),
        ModelDb.Card<DefendPaladin>(),
        ModelDb.Card<DefendPaladin>(),
        ModelDb.Card<DefendPaladin>(),
        ModelDb.Card<Mend>(),
        ModelDb.Card<Judgment>(),
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<LibramOfRighteousness>()];

    public override CardPoolModel CardPool => ModelDb.CardPool<PaladinCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<PaladinRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<PaladinPotionPool>();

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    /// <summary>
    /// The character-select backsplash. The game instantiates this as a Control scene into the
    /// select screen's background container; BaseLib routes it through CharacterSelectBg, which
    /// also puts it on the preload list. A TextureRect over the 2560x1200 canvas the base game uses.
    /// </summary>
    public override string CustomCharacterSelectBg => "res://HelloSpire/scenes/char_select_bg_paladin.tscn";

    public override string CustomIconTexturePath => "character_icon.png".CharacterUiPath(AssetFolder);
    public override string CustomCharacterSelectIconPath => "char_select.png".CharacterUiPath(AssetFolder);
    public override string CustomCharacterSelectLockedIconPath => "char_select_locked.png".CharacterUiPath(AssetFolder);
    public override string CustomMapMarkerPath => "map_marker.png".CharacterUiPath(AssetFolder);
}
