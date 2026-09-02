using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

/// <summary>
/// Junk: what an Attack knocks loose from a Potion it just Brewed mid-swing. Unplayable, does
/// nothing on its own -- same shape as the mod's one other Status card, the Paladin's Geas
/// (<see cref="VolatileReagent"/> used to share this shape too, before it became a real,
/// playable Colorless card). Left in the discard pile automatically by
/// <see cref="Lab.Belt.Brew"/> whenever the Brewing card is an Attack; no card ever creates this
/// one directly.
/// </summary>
public sealed class VolatileResidue() : AlchemistCard(-1, CardType.Status, CardRarity.Status, TargetType.None)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => Task.CompletedTask;
}
