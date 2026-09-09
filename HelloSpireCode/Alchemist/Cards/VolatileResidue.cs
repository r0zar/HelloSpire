using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

/// <summary>
/// Junk: what an Attack knocks loose from a Potion it just Brewed mid-swing (or what a card like
/// Spare Flask leaves behind on purpose). Unplayable, does nothing on its own, and since the
/// Paladin rework cut Geas it is the pack's only Status card (<see cref="VolatileReagent"/> used
/// to share this shape too, before it became a real, playable Colorless card). Left in the
/// discard pile automatically by <see cref="Lab.Belt.Brew"/> whenever the Brewing card is an
/// Attack, or directly via <see cref="Lab.Alchemy.CreateVolatileResidue"/>.
///
/// No OnUpgrade, deliberately: Status cards are never offered to the campfire or to an upgrade
/// effect, so an upgrade path here would be dead code. It is the one card in the pack without
/// one -- everything else, Colorless tokens included, upgrades into something.
/// </summary>
public sealed class VolatileResidue() : AlchemistCard(-1, CardType.Status, CardRarity.Status, TargetType.None)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable, CardKeyword.Ethereal];

    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => Task.CompletedTask;
}
