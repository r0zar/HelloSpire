using BaseLib.Abstracts;
using BaseLib.Patches.UI;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Faith;

/// <summary>
/// Draws the three Faith tracks as a row of orb-like tiles next to the energy counter.
///
/// BaseLib calls AddDisplay once per registered resource, but Faith should read as one
/// portfolio rather than three unrelated widgets, so the first call builds the whole row
/// and later calls only bind their deity's tile to that deity's AmountChanged event.
///
/// This is a code-built Control, not a scene, so it ships with no .tscn. Orb-styled art
/// replaces the plain tiles once the numbers are verified.
/// </summary>
public sealed class FaithDisplay(FaithResource resource) : ICustomResourceVisualsHandler
{
    private const float TileSize = 56f;
    private const float Gap = 8f;

    // One row per combat UI instance. NCombatUi is rebuilt per combat, so keying on it
    // gives a fresh row each fight without any explicit cleanup.
    private static readonly Dictionary<NCombatUi, HBoxContainer> _rows = new();

    private static readonly Dictionary<Deity, Color> _colors = new()
    {
        [Deity.Torm]    = new Color("e8c46a"),
        [Deity.Ilmater] = new Color("6ad48a"),
        [Deity.Tyr]     = new Color("d4703c"),
    };

    public void AddDisplay(NCombatUi nCombatUi, PlayerCombatState playerCombatState)
    {
        if (!_rows.TryGetValue(nCombatUi, out var row))
        {
            row = BuildRow();
            _rows[nCombatUi] = row;
            nCombatUi.EnergyCounterContainer.AddChild(row);
            nCombatUi.TreeExited += () => _rows.Remove(nCombatUi);
        }

        // Bind this deity's tile to the live resource for this player.
        var live = FaithTracks.Get(playerCombatState, resource.Deity);
        var label = row.GetNode<Label>($"{resource.Deity}/Count");
        label.Text = live.Amount.ToString();
        live.AmountChanged += (_, now) => label.Text = now.ToString();
    }

    private static HBoxContainer BuildRow()
    {
        var row = new HBoxContainer
        {
            Name = "FaithRow",
            Position = new Vector2(0f, -(TileSize + Gap * 2)),   // sit just above the energy orb
        };
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
            CornerRadiusTopLeft = 28, CornerRadiusTopRight = 28,
            CornerRadiusBottomLeft = 28, CornerRadiusBottomRight = 28,
        };
        style.SetBorderWidthAll(3);
        tile.AddThemeStyleboxOverride("panel", style);

        var stack = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        stack.AddThemeConstantOverride("separation", -4);

        var initial = new Label
        {
            Name = "Initial",
            Text = deity.ToString()[..1],
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        initial.AddThemeFontSizeOverride("font_size", 13);
        initial.AddThemeColorOverride("font_color", _colors[deity]);

        var count = new Label
        {
            Name = "Count",
            Text = "0",
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        count.AddThemeFontSizeOverride("font_size", 22);
        count.AddThemeColorOverride("font_color", Colors.White);

        stack.AddChild(initial);
        stack.AddChild(count);
        tile.AddChild(stack);
        return tile;
    }
}
