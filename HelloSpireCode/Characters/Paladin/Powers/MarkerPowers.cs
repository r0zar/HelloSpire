using MegaCrit.Sts2.Core.Entities.Powers;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Powers;

// The three Faith rule-changers. None of them hold logic: FaithTracks checks for their presence
// on the owner. Keeping the rules in one place (FaithTracks) rather than in three powers is what
// keeps "Effective Faith" a single definition every threshold card agrees on.

/// <summary>Your Faith in every deity counts as your highest. The wide build's payoff.</summary>
public sealed class HeresyPower : PaladinPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}

/// <summary>Whenever you gain Faith, gain 1 additional. The set's one generation multiplier.</summary>
public sealed class ZealotryPower : PaladinPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}

/// <summary>Your Faith in Bane counts as Faith in every deity. The bottom of the fall.</summary>
public sealed class BaneTyrannyPower : PaladinPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
}
