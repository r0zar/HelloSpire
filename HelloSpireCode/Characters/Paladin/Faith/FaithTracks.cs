using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Players;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Faith;

/// <summary>
/// Deity-keyed access to the three Faith resources. Each is a separate CustomResources&lt;T&gt;
/// registration; BaseLib wires each one's AmountChanged into the CombatManager state tracker
/// on first access, which is what makes the value multiplayer-safe.
/// </summary>
public static class FaithTracks
{
    public static FaithResource Get(PlayerCombatState state, Deity deity) => deity switch
    {
        Deity.Torm    => CustomResources<TormFaith>.Get(state),
        Deity.Ilmater => CustomResources<IlmaterFaith>.Get(state),
        Deity.Tyr     => CustomResources<TyrFaith>.Get(state),
        _ => throw new ArgumentOutOfRangeException(nameof(deity), deity, null),
    };

    public static int Amount(PlayerCombatState state, Deity deity) => Get(state, deity).Amount;

    public static void Gain(PlayerCombatState state, Deity deity, int amount)
    {
        if (amount <= 0) return;
        Get(state, deity).ModifyAmount(amount);
    }

    /// <summary>Highest single-deity Faith, for "your highest deity" card text.</summary>
    public static (Deity deity, int amount) Highest(PlayerCombatState state)
    {
        var best = (Deity.Torm, -1);
        foreach (var d in Enum.GetValues<Deity>())
        {
            var a = Amount(state, d);
            if (a > best.Item2) best = (d, a);
        }
        return best;
    }
}
