using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace HelloSpire.HelloSpireCode.Alchemist;

/// <summary>
/// A pick-one-of-N Potion chooser: one popup showing every option with its icon and name;
/// clicking an option resolves the pick.
///
/// Built from stock Godot nodes (the CylinderDisplay precedent -- no custom Node subclass) and
/// parented onto NModalContainer WITHOUT entering its OpenModal slot. The slot is cleared by
/// NVerticalPopup's button signals in an order that raced the old sequential Brew/Pass popups
/// (one Potion shown, a different one brewed); a single popup with its own backstop has no
/// second act to race.
///
/// The pick is mandatory by design -- the Gold is already spent -- so there is no cancel.
/// </summary>
public static class PotionPickerPopup
{
    /// <summary>Show the chooser. Null when there is no UI host (TestMode, headless).</summary>
    public static Task<int>? TryShow(IReadOnlyList<PotionModel> options)
    {
        var host = NModalContainer.Instance;
        if (host == null) return null;

        var tcs = new TaskCompletionSource<int>();

        var root = new Control { MouseFilter = Control.MouseFilterEnum.Stop };
        root.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        var dim = new ColorRect { Color = new Color(0f, 0f, 0f, 0.6f), MouseFilter = Control.MouseFilterEnum.Stop };
        dim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(dim);

        var center = new CenterContainer();
        center.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(center);

        var panel = new PanelContainer();
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color("1a1410"),
            BorderColor = new Color("d9a05b"),
            BorderWidthTop = 2, BorderWidthBottom = 2, BorderWidthLeft = 2, BorderWidthRight = 2,
            CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10,
            CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10,
            ContentMarginTop = 26, ContentMarginBottom = 30,
            ContentMarginLeft = 34, ContentMarginRight = 34,
        });
        center.AddChild(panel);

        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 18);
        panel.AddChild(vbox);

        var header = new Label
        {
            Text = new LocString("cards", "HELLOSPIRE-ALCHEMIST_BREW_CHOICE.header").GetFormattedText(),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        header.AddThemeFontSizeOverride("font_size", 30);
        vbox.AddChild(header);

        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 20);
        vbox.AddChild(row);

        for (var i = 0; i < options.Count; i++)
        {
            var index = i;
            var potion = options[i];

            var button = new Button { CustomMinimumSize = new Vector2(190, 230) };

            var inner = new VBoxContainer
            {
                MouseFilter = Control.MouseFilterEnum.Ignore,
                Alignment = BoxContainer.AlignmentMode.Center,
            };
            inner.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            inner.AddThemeConstantOverride("separation", 10);

            inner.AddChild(new TextureRect
            {
                Texture = potion.Image,
                CustomMinimumSize = new Vector2(120, 120),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            });

            inner.AddChild(new Label
            {
                Text = potion.Title.GetFormattedText(),
                HorizontalAlignment = HorizontalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                CustomMinimumSize = new Vector2(170, 0),
                MouseFilter = Control.MouseFilterEnum.Ignore,
            });

            button.AddChild(inner);
            button.Pressed += () =>
            {
                root.QueueFree();
                tcs.TrySetResult(index);
            };
            row.AddChild(button);
            if (index == 0) button.CallDeferred("grab_focus");
        }

        host.AddChild(root);
        return tcs.Task;
    }
}
