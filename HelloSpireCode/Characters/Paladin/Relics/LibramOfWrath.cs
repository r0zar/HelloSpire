using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Relics;

/// <summary>
/// +1 Energy every turn; you can no longer gain Spirit. Fuel bought with the heal identity --
/// the Spirit gate lives in the Spirit.Gain funnel.
/// </summary>
public sealed class LibramOfWrath : PaladinRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner) return;
        Flash();
        await PlayerCmd.GainEnergy(1m, player);
    }
}
