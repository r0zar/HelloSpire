using BaseLib.Abstracts;
using HelloSpire.HelloSpireCode.Characters.PaladinContent.Powers;
using MegaCrit.Sts2.Core.Entities.Players;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Faith;

/// <summary>
/// Deity-keyed access to the Faith resources. Each is a separate CustomResources&lt;T&gt;
/// registration; BaseLib wires each one's AmountChanged into the CombatManager state tracker
/// on first access, which is what makes the value multiplayer-safe.
///
/// Two readings of Faith exist and cards must pick the right one:
///   Raw       -- what the counter says. Used for gaining, spending and display.
///   Effective -- raw, plus the rule-changers. Heresy makes every deity count as your highest;
///                Tyranny makes every deity count as at least your Bane. Used by thresholds
///                and "damage equal to your Faith" payoffs.
/// </summary>
public static class FaithTracks
{
    // ---- resource access (by combat state; the display needs this form) ----

    public static FaithResource Get(PlayerCombatState state, Deity deity) => deity switch
    {
        Deity.Torm    => CustomResources<TormFaith>.Get(state),
        Deity.Ilmater => CustomResources<IlmaterFaith>.Get(state),
        Deity.Tyr     => CustomResources<TyrFaith>.Get(state),
        Deity.Bane    => CustomResources<BaneFaith>.Get(state),
        _ => throw new ArgumentOutOfRangeException(nameof(deity), deity, null),
    };

    // ---- raw ----

    public static int Raw(Player p, Deity d) => Get(p.PlayerCombatState, d).Amount;

    public static int Total(Player p) => Enum.GetValues<Deity>().Sum(d => Raw(p, d));

    /// <summary>Highest Triad deity by raw Faith. All-zero resolves to Torm, by design.</summary>
    public static (Deity deity, int amount) Highest(Player p)
    {
        var best = (Deity.Torm, Raw(p, Deity.Torm));
        foreach (var d in DeityExtensions.Triad)
        {
            var a = Raw(p, d);
            if (a > best.Item2) best = (d, a);
        }
        return best;
    }

    /// <summary>Lowest Triad deity by raw Faith. Ties resolve to Torm.</summary>
    public static Deity Lowest(Player p)
    {
        var best = (Deity.Torm, Raw(p, Deity.Torm));
        foreach (var d in DeityExtensions.Triad)
        {
            var a = Raw(p, d);
            if (a < best.Item2) best = (d, a);
        }
        return best.Item1;
    }

    // ---- effective ----

    public static int Effective(Player p, Deity d)
    {
        var v = Raw(p, d);
        var c = p.Creature;
        if (d.IsTriad() && c.GetPower<HeresyPower>() != null)  v = Math.Max(v, Highest(p).amount);
        if (d.IsTriad() && c.GetPower<TyrannyPower>() != null) v = Math.Max(v, Raw(p, Deity.Bane));
        return v;
    }

    public static bool Has(Player p, Deity d, int n) => Effective(p, d) >= n;

    public static bool HasAny(Player p, int n) => DeityExtensions.Triad.Any(d => Effective(p, d) >= n);

    // ---- gaining and spending ----

    /// <summary>Flat gain. Zealotry adds one -- the only multiplier on generation in the set.</summary>
    public static void Gain(Player p, Deity d, int amount)
    {
        if (amount <= 0) return;
        if (p.Creature.GetPower<ZealotryPower>() != null) amount += 1;
        Get(p.PlayerCombatState, d).ModifyAmount(amount);
    }

    /// <summary>
    /// Spend Triad Faith. This is the fall: Bane accrues half of what was spent, rounded up.
    /// Returns what was actually spent (capped at what was held).
    /// </summary>
    public static int Spend(Player p, Deity d, int amount)
    {
        var have = Raw(p, d);
        var spent = Math.Min(amount, have);
        if (spent <= 0) return 0;
        Get(p.PlayerCombatState, d).ModifyAmount(-spent);
        if (d.IsTriad()) Get(p.PlayerCombatState, Deity.Bane).ModifyAmount((spent + 1) / 2);
        return spent;
    }

    /// <summary>Move every other Triad deity's Faith into the highest one. Not a spend; no Bane.</summary>
    public static void Consolidate(Player p)
    {
        var (top, _) = Highest(p);
        foreach (var d in DeityExtensions.Triad)
        {
            if (d == top) continue;
            var a = Raw(p, d);
            if (a <= 0) continue;
            Get(p.PlayerCombatState, d).ModifyAmount(-a);
            Get(p.PlayerCombatState, top).ModifyAmount(a);
        }
    }
}
