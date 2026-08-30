using BaseLib.Abstracts;
using BaseLib.Patches.UI;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Draws Faith as a single gold counter beside the energy orb -- the side-panel spot where the
/// Regent's Star counter lives, which is where players already look for a secondary resource.
/// Hidden at zero. Code-built Control, no .tscn; BaseLib calls AddDisplay during
/// NCombatUi.Activate.
/// </summary>
public sealed class FaithDisplay(FaithResource resource) : ICustomResourceVisualsHandler
{
    private const float Size = 64f;
    private static readonly Color Gold = new("e8c46a");

    public void AddDisplay(NCombatUi nCombatUi, PlayerCombatState playerCombatState)
    {
        // PlayerCombatState has no back-reference to its Player; resolve the local player the
        // same way BaseLib does when it invokes this hook.
        var me = CombatManager.Instance?.DebugOnlyGetState() is { } state ? LocalContext.GetMe(state) : null;
        if (me == null || me.PlayerCombatState != playerCombatState) return;

        var tile = BuildTile();
        tile.Position = new Vector2(-(Size + 12f), -Size - 8f);   // above-left of the energy orb
        nCombatUi.EnergyCounterContainer.AddChild(tile);

        var label = tile.GetNode<Label>("Count");
        void Show(int amount)
        {
            tile.Visible = amount > 0;
            label.Text = amount.ToString();
        }

        var live = CustomResources<FaithResource>.Get(playerCombatState);
        Show(live.Amount);
        live.AmountChanged += (_, now) => Show(now);
    }

    private static PanelContainer BuildTile()
    {
        var tile = new PanelContainer
        {
            Name = "FaithCounter",
            CustomMinimumSize = new Vector2(Size, Size),
            TooltipText = "Faith. Heals and blessings cost it; acts of devotion earn it.",
        };
        var style = new StyleBoxFlat
        {
            BgColor = Gold.Darkened(0.55f),
            BorderColor = Gold,
            CornerRadiusTopLeft = 32, CornerRadiusTopRight = 32,
            CornerRadiusBottomLeft = 32, CornerRadiusBottomRight = 32,
        };
        style.SetBorderWidthAll(4);
        tile.AddThemeStyleboxOverride("panel", style);

        var count = new Label
        {
            Name = "Count",
            Text = "0",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        };
        count.AddThemeFontSizeOverride("font_size", 26);
        count.AddThemeColorOverride("font_color", Colors.White);
        tile.AddChild(count);
        return tile;
    }
}
