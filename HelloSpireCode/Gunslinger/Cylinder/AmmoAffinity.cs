using MegaCrit.Sts2.Core.Entities.Players;

namespace HelloSpire.HelloSpireCode.Gunslinger.Cylinder;

/// <summary>
/// Which Round a given class hands you when you ask to borrow something.
///
/// Hand Me That is the Gunslinger's party card, and a party card should read differently depending
/// on who is at the table. Handing the gun to an Ironclad gets you something heavy back; handing it
/// to a Defect gets you something that goes through armour. Overlaps are fine and deliberate — the
/// table has more classes than the character has interesting ammunition.
///
/// Matched on the character model's type name rather than on its type, because the base game's
/// character classes are not referenced anywhere else in this mod and a modded fourth party member
/// should still get an answer. Anything unrecognised falls through to Lead, which is never wrong.
/// </summary>
public static class AmmoAffinity
{
    /// <summary>Substring of the character class name, and what that class lends you.</summary>
    private static readonly (string Name, Func<Round> Ammo)[] Table =
    [
        ("ironclad",    Rounds.Heavy),      // the biggest thing in the bandolier
        ("silent",      Rounds.Crippling),  // poison logic, expressed as Weak
        ("defect",      Rounds.Piercing),   // straight through the plating
        ("regent",      Rounds.Guard),      // pays for itself in Block
        ("necrobinder", Rounds.Rending),    // Debilitate, the rarest ammunition there is
        ("paladin",     Rounds.Guard),
        ("alchemist",   Rounds.Smoke),
        ("gunslinger",  Rounds.Lead),       // your own kit, and solo play
    ];

    /// <summary>The Round this player's class supplies. Lead for anyone the table does not know.</summary>
    public static Func<Round> For(Player? player)
    {
        var name = player?.Character?.GetType().Name;
        if (string.IsNullOrEmpty(name)) return Rounds.Lead;

        foreach (var (key, ammo) in Table)
            if (name.Contains(key, StringComparison.OrdinalIgnoreCase))
                return ammo;

        return Rounds.Lead;
    }
}
