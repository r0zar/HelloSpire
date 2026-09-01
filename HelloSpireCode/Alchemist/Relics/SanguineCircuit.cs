using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Alchemist.Lab;
using HelloSpire.HelloSpireCode.Alchemist.Potions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Alchemist.Relics;

/// <summary>Whenever you use Unstable Concoction, draw 2 cards.</summary>
public sealed class SanguineCircuit : Characters.AlchemistRelic, IPotionUseListener
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public async Task OnPotionUsed(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        if (potion is not UnstableConcoction) return;

        Flash();
        await AlchemistEffects.Draw(ctx, lab, 2);
    }
}
