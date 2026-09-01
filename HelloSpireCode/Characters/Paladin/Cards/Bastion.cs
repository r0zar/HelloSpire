using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Gain Block equal to 4 + your Plating. Your armor made wall -- the defensive twin of
/// Shield Bash (Plating into damage there, into Block here).
/// </summary>
public sealed class Bastion() : PaladinCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override bool GainsBlock => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var amount = 4m + Owner.Creature.GetPowerAmount<PlatingPower>();
        await CreatureCmd.GainBlock(Owner.Creature, amount, ValueProp.Move, cardPlay);
    }

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
