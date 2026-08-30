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

    // Holy Symbol: the first Faith gain of a combat is multiplied. Keyed per combat state so
    // it resets naturally each fight, and consumed on the first gain regardless of deity.
    private static readonly Dictionary<PlayerCombatState, (int multiplier, Action onFire)> _firstGain = new();

    public static void ArmFirstGainMultiplier(PlayerCombatState state, int multiplier, Action onFire)
        => _firstGain[state] = (multiplier, onFire);

    public static void Gain(PlayerCombatState state, Deity deity, int amount)
    {
        if (amount <= 0) return;
        if (_firstGain.Remove(state, out var armed))
        {
            amount *= armed.multiplier;
            armed.onFire();
        }
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
