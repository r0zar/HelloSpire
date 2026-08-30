using BaseLib.Abstracts;
using BaseLib.Patches.UI;
using MegaCrit.Sts2.Core.Entities.Players;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// Faith, holy-only for now: a small non-negative pool, reset each combat, granted by the Holy
/// Symbol at the start of the fight. Heals cost it; some cards earn it. That is the whole heal
/// economy: you can only heal as much as your Faith allows, and earning more means playing
/// proactive cards -- so dragging a fight out to heal does not work.
///
/// The signed holy/unholy design (negative Faith as the dark side) is agreed and parked; see
/// design/paladin.md. Everything here is written so that extension is additive.
/// </summary>
public sealed class FaithResource() : BasicCustomResource("HelloSpire.Faith")
{
    public override ICustomResourceVisualsHandler ResourceVisualsHandler() => new FaithDisplay(this);
}

public static class Faith
{
    public static int Of(Player p) => CustomResources<FaithResource>.Get(p.PlayerCombatState).Amount;

    public static bool Has(Player p, int n) => Of(p) >= n;

    public static void Gain(Player p, int n)
    {
        if (n > 0) CustomResources<FaithResource>.Get(p.PlayerCombatState).ModifyAmount(n);
    }

    /// <summary>Pay a Faith cost. Callers gate on Has, so this never goes below zero.</summary>
    public static void Spend(Player p, int n) => CustomResources<FaithResource>.Get(p.PlayerCombatState).ModifyAmount(-n);
}
