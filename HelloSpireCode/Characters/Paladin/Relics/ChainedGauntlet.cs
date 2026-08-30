using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Relics;

/// <summary>
/// +1 Energy every turn; your Seals no longer trigger passively (Judgment still works).
/// The passive gate lives in SealPower.PassivesDisabled, checked by every seal passive.
/// </summary>
public sealed class ChainedGauntlet : PaladinRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override async Task AfterEnergyReset(Player player)
    {
        if (player != Owner) return;
        Flash();
        await PlayerCmd.GainEnergy(1m, player);
    }
}
