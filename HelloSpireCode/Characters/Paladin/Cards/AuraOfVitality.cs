using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>
/// All players gain Regen. Was a per-turn aura power; that healed without bound and was the one
/// hole in the anti-stall rule, so it became Renew's party-wide cousin: Regen is self-limiting
/// (ticks down each turn) and the card Exhausts, same as every repeatable heal.
/// </summary>
public sealed class AuraOfVitality() : PaladinCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Regen", 4m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        foreach (var creature in Owner.Creature.CombatState!.PlayerCreatures.Where(c => c.IsAlive))
            await PowerCmd.Apply<RegenPower>(choiceContext, creature,
                DynamicVars["Regen"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Regen"].UpgradeValueBy(2m);
}
