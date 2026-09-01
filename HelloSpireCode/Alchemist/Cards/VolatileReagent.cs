using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

/// <summary>
/// Junk: unstable byproduct of a Brew gone slightly wrong. Unplayable, does nothing on its own --
/// its entire purpose is to be Exhausted (Smelt the Weak, Solvent Strike, Reagent Recovery,
/// Alchemy.ExhaustJunk/ExhaustJunkFromDiscard already treat any CardType.Status as junk with no
/// changes needed) or simply clog a draw. Same shape as the mod's one other Status card,
/// the Paladin's Geas.
/// </summary>
public sealed class VolatileReagent() : AlchemistCard(-1, CardType.Status, CardRarity.Status, TargetType.None)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];

    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => Task.CompletedTask;
}
