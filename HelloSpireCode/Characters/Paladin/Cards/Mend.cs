using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// Heal 5 HP plus your Faith. Exhaust. With the Holy Symbol starting 3 Faith that is 8
/// (upgraded: 7 base, 10 with starter Faith). Exhaust is the anti-stall guarantee; Faith is why
/// building the stat makes the one heal matter.
/// </summary>
public sealed class Mend() : PaladinCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new HealVar(5m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        await Faith.Heal(Owner, DynamicVars.Heal.BaseValue);
    }

    protected override void OnUpgrade() => DynamicVars.Heal.UpgradeValueBy(2m);
}
