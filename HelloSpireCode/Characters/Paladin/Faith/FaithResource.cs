using BaseLib.Abstracts;
using BaseLib.Patches.UI;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Faith;

/// <summary>
/// Base for one deity's Faith. Faith is a standing value: gained, checked, scaled off, normally
/// never spent. It resets each combat via BasicCustomResource.PrepForCombat.
///
/// BaseLib's CustomResources&lt;T&gt; is a static registry keyed by *type*, so per-deity Faith
/// means one concrete resource type per deity rather than one type parameterised by deity.
/// That gives each deity its own counter, its own visuals and its own synced SpireField.
///
/// The second ctor argument to BasicCustomResource is <c>setEachTurn</c>: if non-negative the
/// Amount is reset to it at the start of every turn. Faith must persist across a fight, so it
/// is left at the default -1. Passing 0 here would silently wipe Faith every turn.
/// </summary>
public abstract class FaithResource(Deity deity) : BasicCustomResource($"HelloSpire.Faith.{deity}")
{
    public Deity Deity { get; } = deity;

    // BasicCustomResource's default handler draws nothing; FaithDisplay is what puts Faith on screen.
    public override ICustomResourceVisualsHandler ResourceVisualsHandler() => new FaithDisplay(this);
}

public sealed class TormFaith()    : FaithResource(Deity.Torm);
public sealed class IlmaterFaith() : FaithResource(Deity.Ilmater);
public sealed class TyrFaith()     : FaithResource(Deity.Tyr);
