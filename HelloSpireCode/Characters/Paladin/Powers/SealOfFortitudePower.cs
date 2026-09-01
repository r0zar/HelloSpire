using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The Prot seal. While held: gain Amount Plating at the start of your turn.
/// Judge: gain 10 Block -- the armor engine with an emergency wall attached.
/// </summary>
public sealed class SealOfFortitudePower : SealPower
{
    public const decimal JudgeBlock = 10m;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (PassivesDisabled || player.Creature != Owner) return;
        Flash();
        await PowerCmd.Apply<PlatingPower>(choiceContext, Owner, Amount, Owner, null);
    }

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target) =>
        await CreatureCmd.GainBlock(Owner, JudgeBlock, ValueProp.Unpowered, null);
}
