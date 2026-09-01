using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Alchemist.Relics;

/// <summary>Whenever you Distill a Potion, Infuse 3 Damage into Unstable Concoction.</summary>
public sealed class GoldenCrucible : Characters.AlchemistRelic, IDistillListener
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public Task OnDistilled(PlayerChoiceContext ctx, LabContext lab, PotionModel potion)
    {
        Flash();
        Belt.Infuse(lab, damage: 3m);
        return Task.CompletedTask;
    }
}
