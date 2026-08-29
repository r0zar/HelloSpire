using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using HelloSpire.HelloSpireCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace HelloSpire.HelloSpireCode.Characters;

public class Gunslinger : PlaceholderCharacterModel
{
    public const string CharacterId = "Gunslinger";

    /// <summary>Asset subfolder under images/charui/ for this character's UI.</summary>
    public const string AssetFolder = "gunslinger";

    public static readonly Color Color = new("d4703c");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;
    public override int StartingHp => 70;

    // TODO(Phase 2): replace with Gunslinger-specific Strike/Defend and a signature starter card.
    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>()
    ];

    // TODO(Phase 2): replace with a starting relic that encodes this character's fantasy.
    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<BurningBlood>()];

    public override CardPoolModel CardPool => ModelDb.CardPool<GunslingerCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<GunslingerRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<GunslingerPotionPool>();

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomIconTexturePath => "character_icon.png".CharacterUiPath(AssetFolder);
    public override string CustomCharacterSelectIconPath => "char_select.png".CharacterUiPath(AssetFolder);
    public override string CustomCharacterSelectLockedIconPath => "char_select_locked.png".CharacterUiPath(AssetFolder);
    public override string CustomMapMarkerPath => "map_marker.png".CharacterUiPath(AssetFolder);
}
