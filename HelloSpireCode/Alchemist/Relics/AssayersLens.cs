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

/// <summary>The first Gold gained each combat: gain 1 Energy. Money moves fast here.</summary>
public sealed class AssayersLens : Characters.AlchemistRelic, IGoldListener
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    private bool _usedThisCombat;

    public async Task OnGoldGained(PlayerChoiceContext ctx, LabContext lab, int amount)
    {
        if (_usedThisCombat) return;
        _usedThisCombat = true;
        Flash();
        await AlchemistEffects.GainEnergy(lab, 1m);
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _usedThisCombat = false;
        return Task.CompletedTask;
    }
}
