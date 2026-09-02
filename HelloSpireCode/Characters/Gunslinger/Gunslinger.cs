using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using HelloSpire.HelloSpireCode.Extensions;
using HelloSpire.HelloSpireCode.Gunslinger.Cards;
using HelloSpire.HelloSpireCode.Gunslinger.Relics;
using GunslingerCard = HelloSpire.HelloSpireCode.Gunslinger.Cards.GunslingerCard;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;

namespace HelloSpire.HelloSpireCode.Characters;

/// <summary>
/// The Gunslinger: a sequencing character built around a visible six-chamber revolver.
///
/// The gun is the whole character. Ammunition has to be loaded before it can be spent, the order of
/// the chambers is knowable and manipulable, and almost every card either fills the cylinder, spends
/// it, or rearranges what is coming next. Everything else — Deadeye, Armor, Weak, Gadgets — exists to
/// give the player something to do with the two or three chambers they can see ahead.
/// </summary>
public class Gunslinger : PlaceholderCharacterModel
{
    public const string CharacterId = "Gunslinger";

    /// <summary>Asset subfolder under images/charui/ for this character's UI.</summary>
    public const string AssetFolder = "gunslinger";

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

    public override CardPoolModel CardPool => ModelDb.CardPool<GunslingerCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<GunslingerRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<GunslingerPotionPool>();

    /// <summary>
    /// The Silent's animator, verbatim: the Gunslinger rides the Silent rig now (spine/gunslinger/
    /// holds the silent skeleton; CharacterSkins repaints it), and the animator must map triggers
    /// onto animation names that skeleton actually has -- including "Shiv", which Revolver fires
    /// on every shot. The inherited Ironclad-shaped animator referenced attack_heavy instead.
    /// </summary>
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        var idle = new AnimState("idle_loop", isLooping: true);
        var cast = new AnimState("cast");
        var attack = new AnimState("attack");
        var hurt = new AnimState("hurt");
        var die = new AnimState("die");
        var shiv = new AnimState("shiv");
        var relaxed = new AnimState("relaxed_loop", isLooping: true);
        cast.NextState = idle;
        attack.NextState = idle;
        hurt.NextState = idle;
        shiv.NextState = idle;
        relaxed.AddBranch("Idle", idle);

        var animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Idle", idle);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("Hit", hurt);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Cast", cast);
        animator.AddAnyState("Shiv", shiv);
        animator.AddAnyState("Relaxed", relaxed);
        animator.AddAnyState("PowerUp", cast);
        return animator;
    }

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

    /// <summary>
    /// The character-select backsplash: a Control scene the select screen instantiates. Same shape
    /// as the Paladin's -- a TextureRect over the base game's 2560x1200 canvas.
    /// </summary>
    public override string CustomCharacterSelectBg => "res://HelloSpire/scenes/char_select_bg_gunslinger.tscn";

    // In-combat body: the inherited Ironclad rig, repainted by the CharacterSkins shader patch.

    public override string CustomIconTexturePath => "character_icon.png".CharacterUiPath(AssetFolder);
    public override string CustomCharacterSelectIconPath => "char_select.png".CharacterUiPath(AssetFolder);
    public override string CustomCharacterSelectLockedIconPath => "char_select_locked.png".CharacterUiPath(AssetFolder);
    public override string CustomMapMarkerPath => "map_marker.png".CharacterUiPath(AssetFolder);
}
