using BaseLib.Patches.UI;
using Godot;
using HelloSpire.HelloSpireCode.Gunslinger.Powers;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace HelloSpire.HelloSpireCode.Gunslinger.Cylinder;

/// <summary>
/// The gun, on screen: a brass ring with six chambers in it, sitting beside the Energy orb, turning
/// one sixth of a revolution every time the hammer moves and a whole extra turn every time you Spin.
///
/// The Cylinder power's tooltip already reports "4 of 6 chambers loaded", but a number in a tooltip
/// is not something a player can plan two chambers ahead against. This is the same state drawn the
/// way the character actually thinks about it: which chamber is under the hammer, and what colour
/// the next three are.
///
/// It is built from stock Godot nodes and driven by <see cref="CylinderPower.AnyChanged"/> — no
/// custom Node subclass, no per-frame polling, and one Tween per change. That keeps the whole
/// widget on Godot API this mod has already shipped working code against (see the Paladin's
/// FaithDisplay), which matters for a piece of UI that cannot be unit tested.
/// </summary>
public sealed class CylinderDisplay : ICustomResourceVisualsHandler
{
    private const float Size = 104f;
    private const float Centre = Size / 2f;
    private const float ChamberOrbit = 30f;
    private const float ChamberSize = 22f;
    private const double SpinSeconds = 0.32;

    /// <summary>Offset from the Energy orb. Up and to the left of it, clear of the hand.</summary>
    private static readonly Vector2 EnergyOffset = new(-124f, -18f);

    /// <summary>Where the widget lands if no Energy node can be found to hang it off.</summary>
    private static readonly Vector2 FallbackOffset = new(40f, -200f);

    private static readonly Color Brass = new("d9a05b");
    private static readonly Color Empty = new("2a2119");
    private static readonly Color Body = new("1a1410");

    /// <summary>One colour per kind of ammunition, keyed by <see cref="Round.Key"/>.</summary>
    private static readonly Dictionary<string, Color> RoundColors = new()
    {
        ["LEAD_ROUND"]        = new Color("d9a05b"),
        ["HEAVY_ROUND"]       = new Color("9aa7b4"),
        ["CRIPPLING_ROUND"]   = new Color("7fb36a"),
        ["PIERCING_ROUND"]    = new Color("6fc3d9"),
        ["GUARD_ROUND"]       = new Color("7f8fd9"),
        ["SMOKE_ROUND"]       = new Color("b0b0b0"),
        ["RENDING_ROUND"]     = new Color("b06fd9"),
        ["BLACK_POWDER_ROUND"]= new Color("d9603c"),
        ["DEAD_MANS_ROUND"]   = new Color("f0e6d2"),
    };

    private Creature? _creature;
    private Control? _root;
    private Control? _ring;
    private Label? _count;
    private readonly Panel[] _chambers = new Panel[CylinderPower.ChamberCount];

    /// <summary>Ring rotation in chamber-widths, unwrapped so 5 -> 0 turns forwards, not back 300°.</summary>
    private double _turnUnits;
    private int _lastHammer;
    private int _lastSpin;
    private Tween? _tween;

    public void AddDisplay(NCombatUi nCombatUi, PlayerCombatState playerCombatState)
    {
        // PlayerCombatState has no back-reference to its Player; resolve the local player the same
        // way BaseLib does when it invokes this hook.
        var me = CombatManager.Instance?.DebugOnlyGetState() is { } state ? LocalContext.GetMe(state) : null;
        if (me == null || me.PlayerCombatState != playerCombatState || me.Creature == null) return;

        _creature = me.Creature;
        _root = Build();

        Attach(nCombatUi, _root);

        // Hidden until this player actually has a gun: the resource is registered mod-wide, so a
        // Paladin's combat reaches this code too and should see nothing.
        _root.Visible = false;

        CylinderPower.AnyChanged += OnCylinderChanged;
        _root.TreeExited += Detach;

        if (_creature.GetPower<CylinderPower>() is { } existing) OnCylinderChanged(existing);
    }

    private void Detach()
    {
        CylinderPower.AnyChanged -= OnCylinderChanged;
        _creature = null;
        _root = null;
        _ring = null;
        _count = null;
        _tween = null;
    }

    // ------------------------------------------------------------------ state

    private void OnCylinderChanged(CylinderPower cylinder)
    {
        if (cylinder.Owner != _creature) return;
        if (_root == null || !GodotObject.IsInstanceValid(_root)) return;

        _root.Visible = true;

        for (var i = 0; i < CylinderPower.ChamberCount; i++)
        {
            var round = cylinder.Chambers[i];
            Paint(_chambers[i], round == null ? Empty : ColorFor(round));
        }

        if (_count != null) _count.Text = cylinder.LoadedCount.ToString();

        Turn(cylinder.Hammer, cylinder.SpinCount);
    }

    /// <summary>
    /// Rotates the ring so the chamber under the hammer sits at twelve o'clock.
    ///
    /// The target is accumulated rather than recomputed so the gun always turns the way a real
    /// cylinder would: advancing from chamber 5 to chamber 0 is one more sixth forward, not five
    /// sixths back. A Spin adds a whole extra revolution on top, which is the only way a Spin that
    /// happens to land where it started would read as having spun at all.
    /// </summary>
    private void Turn(int hammer, int spinCount)
    {
        var step = ((hammer - _lastHammer) % CylinderPower.ChamberCount + CylinderPower.ChamberCount)
                   % CylinderPower.ChamberCount;

        _turnUnits -= step;
        if (spinCount > _lastSpin) _turnUnits -= CylinderPower.ChamberCount * (spinCount - _lastSpin);

        _lastHammer = hammer;
        _lastSpin = spinCount;

        if (_ring == null || !GodotObject.IsInstanceValid(_ring)) return;

        var radians = (float)(_turnUnits * Mathf.Tau / CylinderPower.ChamberCount);

        if (_tween != null && GodotObject.IsInstanceValid(_tween)) _tween.Kill();

        if (!_ring.IsInsideTree())
        {
            _ring.Rotation = radians;
            return;
        }

        _tween = _ring.CreateTween();
        _tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        _tween.TweenProperty(_ring, "rotation", radians, SpinSeconds);
    }

    private static Color ColorFor(Round round) =>
        RoundColors.TryGetValue(round.Key, out var color) ? color : Brass;

    // ------------------------------------------------------------------ nodes

    /// <summary>
    /// Hangs the widget off the Energy orb.
    ///
    /// The combat UI's node names are not part of any API this mod can compile against, so rather
    /// than guess a member it walks the tree for a Control whose name mentions energy. If the game
    /// renames it the search fails and the widget lands in the bottom-left corner instead, which is
    /// wrong-looking but never a crash and never invisible.
    /// </summary>
    private static void Attach(NCombatUi nCombatUi, Control root)
    {
        if (FindEnergy(nCombatUi) is { } energy)
        {
            energy.AddChild(root);
            root.Position = EnergyOffset;
            return;
        }

        nCombatUi.AddChild(root);
        root.SetAnchorsPreset(Control.LayoutPreset.BottomLeft);
        root.Position = FallbackOffset;
    }

    private static Control? FindEnergy(Node node)
    {
        foreach (var child in node.GetChildren())
        {
            if (child is Control control &&
                control.Name.ToString().Contains("energy", StringComparison.OrdinalIgnoreCase))
            {
                return control;
            }

            if (FindEnergy(child) is { } found) return found;
        }

        return null;
    }

    private Control Build()
    {
        var root = new Control
        {
            Name = "GunslingerCylinder",
            CustomMinimumSize = new Vector2(Size, Size),
            Size = new Vector2(Size, Size),
            MouseFilter = Control.MouseFilterEnum.Pass,
            TooltipText = "The Cylinder",
        };

        root.AddChild(Circle("Body", Size, Vector2.Zero, Body, Brass.Darkened(0.3f), 3));

        _ring = new Control
        {
            Name = "Ring",
            Size = new Vector2(Size, Size),
            PivotOffset = new Vector2(Centre, Centre),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        root.AddChild(_ring);

        for (var i = 0; i < CylinderPower.ChamberCount; i++)
        {
            // Chamber 0 at twelve o'clock, the rest clockwise from it.
            var angle = i * Mathf.Tau / CylinderPower.ChamberCount;
            var offset = new Vector2(Mathf.Sin(angle), -Mathf.Cos(angle)) * ChamberOrbit;
            var topLeft = new Vector2(Centre, Centre) + offset - new Vector2(ChamberSize / 2f, ChamberSize / 2f);

            _chambers[i] = Circle($"Chamber{i}", ChamberSize, topLeft, Empty, Brass.Darkened(0.5f), 2);
            _ring.AddChild(_chambers[i]);
        }

        // The hammer does not turn; the cylinder turns under it.
        var hammer = Circle("Hammer", 12f, new Vector2(Centre - 6f, -4f), Brass, Brass.Lightened(0.4f), 2);
        root.AddChild(hammer);

        _count = new Label
        {
            Name = "Loaded",
            Text = "0",
            Position = new Vector2(Centre - 20f, Centre - 12f),
            Size = new Vector2(40f, 24f),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        _count.AddThemeFontSizeOverride("font_size", 20);
        _count.AddThemeColorOverride("font_color", Brass.Lightened(0.5f));
        root.AddChild(_count);

        return root;
    }

    private static Panel Circle(string name, float size, Vector2 position, Color fill, Color border, int width)
    {
        var panel = new Panel
        {
            Name = name,
            Position = position,
            Size = new Vector2(size, size),
            CustomMinimumSize = new Vector2(size, size),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };

        panel.AddThemeStyleboxOverride("panel", Pill(fill, border, width, size));
        return panel;
    }

    private static void Paint(Panel panel, Color fill)
    {
        if (!GodotObject.IsInstanceValid(panel)) return;
        panel.AddThemeStyleboxOverride("panel", Pill(fill, fill.Lightened(0.35f), 2, ChamberSize));
    }

    /// <summary>A fully rounded box: at radius = size/2 a square panel draws as a circle.</summary>
    private static StyleBoxFlat Pill(Color fill, Color border, int width, float size)
    {
        var radius = Mathf.RoundToInt(size / 2f);
        var style = new StyleBoxFlat
        {
            BgColor = fill,
            BorderColor = border,
            CornerRadiusTopLeft = radius,
            CornerRadiusTopRight = radius,
            CornerRadiusBottomLeft = radius,
            CornerRadiusBottomRight = radius,
        };
        style.SetBorderWidthAll(width);
        return style;
    }
}
