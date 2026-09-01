using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Alchemist.Relics;

/// <summary>Whenever you use any Potion, gain 1 Block. No per-turn limit -- every drink counts.</summary>
public sealed class AlchemistsLedger : Characters.AlchemistRelic, IPotionUseListener
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public async Task OnPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        Flash();
        await AlchemistEffects.GainBlock(lab, 1m);
    }
}
