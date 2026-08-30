namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Faith;

/// <summary>
/// The deities Faith can be held in. Torm, Ilmater and Tyr are the Triad -- the gods you build
/// toward. Bane is the fall: his Faith cannot be gained directly and accrues only when Triad
/// Faith is spent. See design/paladin.md.
/// </summary>
public enum Deity { Torm, Ilmater, Tyr, Bane }

public static class DeityExtensions
{
    public static readonly Deity[] Triad = [Deity.Torm, Deity.Ilmater, Deity.Tyr];
    public static bool IsTriad(this Deity d) => d != Deity.Bane;
}
