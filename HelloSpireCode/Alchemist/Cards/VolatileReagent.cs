using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace HelloSpire.HelloSpireCode.Alchemist.Cards;

/// <summary>
/// A free Colorless card born from an unstable Brew. Gain 1 Energy, no strings attached -- Pooled
/// as Colorless rather than as an Alchemist card, since gaining Energy has nothing to do with the
/// Belt.
/// </summary>
[Pool(typeof(ColorlessCardPool))]
public sealed class VolatileReagent() : AlchemistCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await AlchemistEffects.GainEnergy(Lab, 1m);
}
