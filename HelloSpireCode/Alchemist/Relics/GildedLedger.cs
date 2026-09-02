using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Alchemist.Relics;

/// <summary>
/// Whenever you Distill a Potion, gain 1 Block; Merchant prices are 10% higher. A bargain on one
/// side of the ledger, a markup on the other.
/// </summary>
public sealed class GildedLedger : Characters.AlchemistRelic, IDistillListener
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public async Task OnDistilled(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        Flash();
        await AlchemistEffects.GainBlock(lab, 1m);
    }

    public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal cost) =>
        player == Owner ? cost * 1.10m : cost;
}
