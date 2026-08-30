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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace HelloSpire.HelloSpireCode.Alchemist.Relics;

/// <summary>The first Potion you use each combat: draw 1. The tempo tax refund.</summary>
public sealed class CorkStopper : Characters.AlchemistRelic, IPotionUseListener
{
    public override RelicRarity Rarity => RelicRarity.Common;

    private bool _usedThisCombat;

    public async Task OnPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        if (_usedThisCombat) return;
        _usedThisCombat = true;
        Flash();
        await AlchemistEffects.Draw(ctx, lab, 1);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _usedThisCombat = false;
        return Task.CompletedTask;
    }
}
