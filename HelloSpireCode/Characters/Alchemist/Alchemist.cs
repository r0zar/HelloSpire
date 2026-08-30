using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using HelloSpire.HelloSpireCode.Alchemist.Cards;
using HelloSpire.HelloSpireCode.Alchemist.Relics;
using HelloSpire.HelloSpireCode.Extensions;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace HelloSpire.HelloSpireCode.Characters;

/// <summary>
/// The Alchemist: a conversion character built on three pools and one verb.
///
/// Potions, Gold and Max HP all hold value, and Transforming is the pump that moves value between
/// them. The character is not the one with the most of any pool — it is the one that keeps the
/// least value sitting still. A good turn Exhausts a dead card into Gold, Invests that Gold into a
/// bigger attack, and Distills a spare Potion for the Energy to play it.
///
/// The load-bearing asymmetry: Gold comes back and Max HP does not. See design/alchemist.md.
/// </summary>
public class Alchemist : PlaceholderCharacterModel
{
    public const string CharacterId = "Alchemist";

    /// <summary>Asset subfolder under images/charui/ for this character's UI.</summary>
    public const string AssetFolder = "alchemist";

    /// <summary>Verdigris and cheap glass.</summary>
    public static readonly Color Color = new("6ad48a");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Neutral;

    /// <summary>
    /// Near the roster's floor. The belt is an exceptionally flexible defensive tool, Potions cost
    /// no Energy to use, and Gold buys emergency Block — so the HP pool is where that flexibility
    /// is paid for.
    /// </summary>
    public override int StartingHp => 68;

    /// <summary>
    /// Four Strikes, four Defends, and the two Formulas.
    ///
    /// Gold is deliberately absent. Turn one teaches exactly one system — the belt has space, cards
    /// make Potions, Potions cost no Energy, and a Brewed Potion vanishes if you do not spend it.
    /// The Gold half of the character begins at the first card reward.
    /// </summary>
    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeAlchemist>(),
        ModelDb.Card<StrikeAlchemist>(),
        ModelDb.Card<StrikeAlchemist>(),
        ModelDb.Card<StrikeAlchemist>(),
        ModelDb.Card<DefendAlchemist>(),
        ModelDb.Card<DefendAlchemist>(),
        ModelDb.Card<DefendAlchemist>(),
        ModelDb.Card<DefendAlchemist>(),
        ModelDb.Card<PyricFormula>(),
        ModelDb.Card<AegisFormula>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<PortableAlembic>()];

    public override CardPoolModel CardPool => ModelDb.CardPool<AlchemistCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<AlchemistRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<AlchemistPotionPool>();

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

    public override string CustomIconTexturePath => "character_icon.png".CharacterUiPath(AssetFolder);
    public override string CustomCharacterSelectIconPath => "char_select.png".CharacterUiPath(AssetFolder);
    public override string CustomCharacterSelectLockedIconPath => "char_select_locked.png".CharacterUiPath(AssetFolder);
    public override string CustomMapMarkerPath => "map_marker.png".CharacterUiPath(AssetFolder);
}
