using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// The rare capstone of the Regen family: ALL players gain Regen and Block, once. Was an unbounded
/// per-turn aura; now it stacks with Aura of Vitality/Mercy into one decaying pool -- playing the
/// family together is the mechanic.
/// </summary>
public sealed class TyrsDeliverance() : PaladinCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override bool GainsBlock => true;
    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DynamicVar("Regen", 4m), new BlockVar(4m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        foreach (var player in CombatState.PlayerCreatures.Where(c => c.IsAlive))
        {
            await PowerCmd.Apply<RegenPower>(choiceContext, player,
                DynamicVars["Regen"].BaseValue, Owner.Creature, this);
            await CreatureCmd.GainBlock(player, DynamicVars.Block, cardPlay);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Regen"].UpgradeValueBy(2m);
        DynamicVars.Block.UpgradeValueBy(2m);
    }
}
