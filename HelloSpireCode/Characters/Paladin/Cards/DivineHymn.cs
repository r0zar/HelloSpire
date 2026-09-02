using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Cards;

/// <summary>Heal ALL players 10 + Spirit. Exhaust. Tithe: ALL players gain 2 Block. The great chorus, once per copy.</summary>
public sealed class DivineHymn() : PaladinCard(3, CardType.Skill, CardRarity.Rare, TargetType.Self), IHealingCard
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [new SpiritHealVar(10m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(PaladinTips.Tithe)];

    public override bool HasTithe => true;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
        foreach (var player in CombatState.PlayerCreatures.Where(c => c.IsAlive))
            await Spirit.Heal(Owner, player, DynamicVars.Heal.BaseValue);
    }

    protected override async Task OnTithe(PlayerChoiceContext ctx)
    {
        foreach (var player in Owner.Creature.CombatState.PlayerCreatures.Where(c => c.IsAlive))
            await CreatureCmd.GainBlock(player, 2m, ValueProp.Unpowered, null);
    }

    protected override void OnUpgrade() => DynamicVars.Heal.UpgradeValueBy(4m);
}
