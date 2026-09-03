using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace HelloSpire.HelloSpireCode.Alchemist;

/// <summary>
/// A pick-one-of-N Potion chooser: every option's own art floats directly on a dimmed full-screen
/// backdrop, spread in a row with room to breathe and a slight grow-on-hover, the same visual
/// language the base game's own card-selection screens use (CardSelectCmd.FromChooseACardScreen,
/// decompiled from sts2.dll) -- there is no potion equivalent of that screen to delegate to
/// (confirmed: MegaCrit.Sts2.Core.Commands has exactly one Potion-related command, PotionCmd, and
/// nothing named *PotionSelect*), so this reproduces the feel by hand instead of inheriting it.
///
/// Built from stock Godot nodes (the CylinderDisplay precedent -- no custom Node subclass) and
/// parented onto NModalContainer WITHOUT entering its OpenModal slot. The slot is cleared by
/// NVerticalPopup's button signals in an order that raced the old sequential Brew/Pass popups
/// (one Potion shown, a different one brewed); a single popup with its own backstop has no
/// second act to race.
///
/// The pick is mandatory by design in most callers -- the card has already committed to Brewing
/// or Distilling one -- so there is no cancel by default. Grand Combustion is the one caller that
/// genuinely offers "however many you like": passing allowStop adds a "Done" button that resolves
/// the pick as null instead.
/// </summary>
public static class PotionPickerPopup
{
    private const float IconSize = 220f;
    private const float ButtonWidth = 240f;
    private const float ButtonHeight = 320f;
    private const float HoverScale = 1.08f;
    private const float HoverSeconds = 0.12f;

    /// <summary>
    /// Show the chooser. Null when there is no UI host (TestMode, headless).
    /// </summary>
    /// <param name="options">The Potions to offer.</param>
    /// <param name="header">
    /// The popup's title. Defaults to "Choose a Potion to Brew" (Buy Ingredients and friends);
    /// callers choosing among ALREADY-HELD Potions for something other than Brewing (Distill,
    /// Stabilize, Pressure Burst) must pass their own, or the popup lies about what it does.
    /// </param>
    /// <param name="allowStop">Add a "Done" button that resolves the pick as null.</param>
    public static Task<int?>? TryShow(IReadOnlyList<PotionModel> options, LocString? header = null, bool allowStop = false)
    {
        var host = NModalContainer.Instance;
        if (host == null) return null;

        var tcs = new TaskCompletionSource<int?>();

        var root = new Control { MouseFilter = Control.MouseFilterEnum.Stop };
        root.SetAnchorsPreset(Control.LayoutPreset.FullRect);

        // Full-screen dim, same as the card-selection screens -- the options float directly on
        // this, nothing else framing them.
        var dim = new ColorRect { Color = new Color(0f, 0f, 0f, 0.75f), MouseFilter = Control.MouseFilterEnum.Stop };
        dim.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        root.AddChild(dim);

        var vbox = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 48);
        root.AddChild(vbox);

        var headerLabel = new Label
        {
            Text = (header ?? new LocString("cards", "HELLOSPIRE-ALCHEMIST_BREW_CHOICE.header")).GetFormattedText(),
            HorizontalAlignment = HorizontalAlignment.Center,
        };
        headerLabel.AddThemeFontSizeOverride("font_size", 36);
        headerLabel.AddThemeColorOverride("font_color", new Color("d9a05b"));
        headerLabel.AddThemeColorOverride("font_shadow_color", new Color(0f, 0f, 0f, 0.8f));
        headerLabel.AddThemeConstantOverride("shadow_offset_x", 2);
        headerLabel.AddThemeConstantOverride("shadow_offset_y", 2);
        vbox.AddChild(headerLabel);

        var row = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        row.AddThemeConstantOverride("separation", 36);
        row.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        vbox.AddChild(row);

        for (var i = 0; i < options.Count; i++)
        {
            var index = i;
            var potion = options[i];

            // No panel, no border -- the art and its name are the whole button, floating on the
            // dim like every other option beside it.
            var button = new Button
            {
                CustomMinimumSize = new Vector2(ButtonWidth, ButtonHeight),
                Flat = true,
                PivotOffset = new Vector2(ButtonWidth / 2f, ButtonHeight / 2f),
            };

            var inner = new VBoxContainer
            {
                MouseFilter = Control.MouseFilterEnum.Ignore,
                Alignment = BoxContainer.AlignmentMode.Center,
            };
            inner.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            inner.AddThemeConstantOverride("separation", 14);

            inner.AddChild(new TextureRect
            {
                Texture = potion.Image,
                CustomMinimumSize = new Vector2(IconSize, IconSize),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            });

            inner.AddChild(new Label
            {
                Text = potion.Title.GetFormattedText(),
                HorizontalAlignment = HorizontalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                CustomMinimumSize = new Vector2(ButtonWidth - 20, 0),
                MouseFilter = Control.MouseFilterEnum.Ignore,
            });

            button.AddChild(inner);

            // A light grow-on-hover, the same "this one's in play" cue the card screens give you
            // when you drag over an option.
            button.MouseEntered += () => ScaleTo(button, HoverScale);
            button.MouseExited += () => ScaleTo(button, 1f);

            button.Pressed += () =>
            {
                root.QueueFree();
                tcs.TrySetResult(index);
            };
            row.AddChild(button);
            if (index == 0) button.CallDeferred("grab_focus");
        }

        if (allowStop)
        {
            var doneButton = new Button
            {
                Text = new LocString("cards", "HELLOSPIRE-ALCHEMIST_POTION_PICKER.done").GetFormattedText(),
                CustomMinimumSize = new Vector2(0, 44),
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
            };
            doneButton.Pressed += () =>
            {
                root.QueueFree();
                tcs.TrySetResult(null);
            };
            vbox.AddChild(doneButton);
        }

        host.AddChild(root);
        return tcs.Task;
    }

    private static void ScaleTo(Control control, float scale)
    {
        var tween = control.CreateTween();
        tween.TweenProperty(control, "scale", new Vector2(scale, scale), HoverSeconds)
            .SetTrans(Tween.TransitionType.Cubic)
            .SetEase(Tween.EaseType.Out);
    }
}
