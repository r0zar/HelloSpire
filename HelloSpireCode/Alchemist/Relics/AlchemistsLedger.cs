using System.Collections.Generic;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;

namespace HelloSpire.HelloSpireCode.Alchemist.Relics;

/// <summary>
/// Whenever you Transform, gain 1 Block. No per-turn limit -- this relic is the reason Transform
/// has a hard definition (design/alchemist.md, Override 3).
/// </summary>
public sealed class AlchemistsLedger : Characters.AlchemistRelic, ITransformListener
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public async Task OnTransformed(PlayerChoiceContext ctx, LabContext lab, TransformVector vector)
    {
        Flash();
        await AlchemistEffects.GainBlock(lab, 1m);
    }
}
