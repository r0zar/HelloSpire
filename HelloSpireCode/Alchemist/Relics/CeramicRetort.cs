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

/// <summary>The first Brew each turn: gain 4 Block. Setting up stops being a defensive gap.</summary>
public sealed class CeramicRetort : Characters.AlchemistRelic, IBrewListener
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    private bool _usedThisTurn;

    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner) _usedThisTurn = false;
        return Task.CompletedTask;
    }

    public async Task OnBrewed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        if (_usedThisTurn) return;
        _usedThisTurn = true;
        Flash();
        await AlchemistEffects.GainBlock(lab, 4m);
    }
}
