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
/// Invest costs are reduced by 2 Gold (minimum 1); Merchant prices are 10% higher. The discount
/// rides IInvestDiscount; the markup rides the game's own ModifyMerchantPrice hook.
/// </summary>
public sealed class GildedLedger : Characters.AlchemistRelic, IInvestDiscount
{
    public override RelicRarity Rarity => RelicRarity.Shop;

    public int DiscountInvest(LabContext lab) => 2;

    public override decimal ModifyMerchantPrice(Player player, MerchantEntry entry, decimal cost) =>
        player == Owner ? cost * 1.10m : cost;
}
