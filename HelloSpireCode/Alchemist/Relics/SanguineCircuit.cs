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
/// Whenever you Render, draw 2 cards. Pays in cards, never in HP -- a hard rule (Override 1):
/// Max HP is one-way, so no relic may ever hand it back.
/// </summary>
public sealed class SanguineCircuit : Characters.AlchemistRelic, IRenderListener
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public async Task OnRendered(PlayerChoiceContext ctx, LabContext lab, int cost)
    {
        Flash();
        await AlchemistEffects.Draw(ctx, lab, 2);
    }
}
