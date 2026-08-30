using BaseLib.Patches.UI;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Faith;

/// <summary>
/// Draws the three Faith tracks as a row of orb-like tiles floating above the Paladin's head,
/// where Defect's orbs sit.
///
/// The game positions orbs via NCreature.SetOrbManagerPosition, which reads
/// Visuals.OrbPosition scaled by the creature's visual scale. Rather than re-derive that, the
/// row is parented to the same NCreature and placed at OrbManager's position, nudged upward so
/// Faith stacks above any orbs the player might also hold.
///
/// BaseLib calls AddDisplay once per registered resource, but Faith should read as one
/// portfolio, so the first call builds the whole row and later calls only bind their deity's
/// tile. AddDisplay runs during NCombatUi.Activate, by which point NCombatRoom and the player's
/// NCreature already exist. Rows are keyed per NCreature so each combat gets a fresh one.
///
/// Code-built Control, no .tscn. Orb-styled art replaces the plain tiles once the numbers are
/// verified in play.
/// </summary>
public sealed class FaithDisplay(FaithResource resource) : ICustomResourceVisualsHandler
{
    private const float TileSize = 52f;
    private const float Gap = 6f;
    private const float RiseAboveOrbs = 70f;

    private static readonly Dictionary<NCreature, HBoxContainer> _rows = new();

    private static readonly Dictionary<Deity, Color> _colors = new()
    {
        [Deity.Torm]    = new Color("e8c46a"),
        [Deity.Ilmater] = new Color("6ad48a"),
        [Deity.Tyr]     = new Color("d4703c"),
        [Deity.Bane]    = new Color("7a3fb0"),
    };

    public void AddDisplay(NCombatUi nCombatUi, PlayerCombatState playerCombatState)
    {
        // PlayerCombatState has no back-reference to its Player; resolve the local player the
        // same way BaseLib does when it invokes this hook.
        var me = CombatManager.Instance?.DebugOnlyGetState() is { } state ? LocalContext.GetMe(state) : null;
        if (me == null || me.PlayerCombatState != playerCombatState) return;

        var creatureNode = NCombatRoom.Instance?.GetCreatureNode(me.Creature);
        if (creatureNode == null) return;

        if (!_rows.TryGetValue(creatureNode, out var row))
        {
            row = BuildRow();
            _rows[creatureNode] = row;
            creatureNode.AddChild(row);
            creatureNode.TreeExited += () => _rows.Remove(creatureNode);
            PositionRow(creatureNode, row);
        }

        var live = FaithTracks.Get(playerCombatState, resource.Deity);
        var tile = row.GetNode<Control>(resource.Deity.ToString());
        var label = tile.GetNode<Label>("Stack/Count");

        void Show(int amount)
        {
            label.Text = amount.ToString();
            // Invisible at zero, appears on the first gain. Faith is a mod-wide resource, so any
            // character that picks up a Paladin card and gains Faith sees it -- but nobody sees
            // three empty tiles before they have any.
            tile.Visible = amount > 0;
        }

        Show(live.Amount);
        live.AmountChanged += (_, now) => Show(now);
    }

    /// <summary>Sit where the orbs would, then rise above them. Centred on the creature.</summary>
    private static void PositionRow(NCreature creatureNode, HBoxContainer row)
    {
        var anchor = creatureNode.OrbManager?.Position ?? Vector2.Zero;
        var width = 3 * TileSize + 2 * Gap;
        row.Position = new Vector2(anchor.X - width / 2f, anchor.Y - RiseAboveOrbs);
    }

    private static HBoxContainer BuildRow()
    {
        var row = new HBoxContainer { Name = "FaithRow" };
        row.AddThemeConstantOverride("separation", (int)Gap);
        foreach (var deity in Enum.GetValues<Deity>())
            row.AddChild(BuildTile(deity));
        return row;
    }

    private static Control BuildTile(Deity deity)
    {
        var tile = new PanelContainer
        {
            Name = deity.ToString(),
            CustomMinimumSize = new Vector2(TileSize, TileSize),
            TooltipText = $"Faith in {deity}",
        };

        var style = new StyleBoxFlat
        {
            BgColor = _colors[deity].Darkened(0.55f),
            BorderColor = _colors[deity],
            CornerRadiusTopLeft = 26, CornerRadiusTopRight = 26,
            CornerRadiusBottomLeft = 26, CornerRadiusBottomRight = 26,
        };
        style.SetBorderWidthAll(3);
        tile.AddThemeStyleboxOverride("panel", style);

        var stack = new VBoxContainer { Name = "Stack", Alignment = BoxContainer.AlignmentMode.Center };
        stack.AddThemeConstantOverride("separation", -4);

        var initial = new Label
        {
            Name = "Initial",
            Text = deity.ToString()[..1],
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        initial.AddThemeFontSizeOverride("font_size", 12);
        initial.AddThemeColorOverride("font_color", _colors[deity]);

        var count = new Label
        {
            Name = "Count",
            Text = "0",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        count.AddThemeFontSizeOverride("font_size", 20);
        count.AddThemeColorOverride("font_color", Colors.White);

        stack.AddChild(initial);
        stack.AddChild(count);
        tile.AddChild(stack);
        return tile;
    }
}
