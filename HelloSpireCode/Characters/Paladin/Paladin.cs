using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using HelloSpire.HelloSpireCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

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

    // PROTOTYPE DECK. Not the designed starter (4 Strike / 4 Defend / 1 Mend / 1 Aura of
    // Protection). This is built to exercise every Faith path in one fight: printed Faith
    // (Smite, Hold the Line), Oath-triggered Faith across all three verbs, a deity-neutral
    // heal, and a threshold payoff (The Scales). Replace once the mechanic is verified.
    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<StrikeIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<DefendIronclad>(),
        ModelDb.Card<Smite>(),
        ModelDb.Card<Smite>(),
        ModelDb.Card<HoldTheLine>(),
        ModelDb.Card<HoldTheLine>(),
        ModelDb.Card<Mend>(),
        ModelDb.Card<Mend>(),
        ModelDb.Card<OathOfVengeanceCard>(),
        ModelDb.Card<OathOfTheCrownCard>(),
        ModelDb.Card<OathOfRedemptionCard>(),
        ModelDb.Card<TheScales>(),
    ];

    // TODO(Phase 2): replace with a starting relic that encodes this character's fantasy.
    public override IReadOnlyList<RelicModel> StartingRelics => [ModelDb.Relic<BurningBlood>()];

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

    public override string CustomIconTexturePath => "character_icon.png".CharacterUiPath(AssetFolder);
    public override string CustomCharacterSelectIconPath => "char_select.png".CharacterUiPath(AssetFolder);
    public override string CustomCharacterSelectLockedIconPath => "char_select_locked.png".CharacterUiPath(AssetFolder);
    public override string CustomMapMarkerPath => "map_marker.png".CharacterUiPath(AssetFolder);
}
