using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent;

/// <summary>
/// The Prot seal, armed: no passive (the Plating was paid on cast).
/// Judge: gain 10 Block and apply 1 Weak to the judged enemy -- the wall goes up and the
/// attacker comes away sapped.
/// </summary>
public sealed class SealOfFortitudePower : SealPower
{
    public const decimal JudgeBlock = 10m;
    public const decimal JudgeWeak = 1m;

    public override async Task OnJudged(PlayerChoiceContext ctx, Creature target)
    {
        await CreatureCmd.GainBlock(Owner, JudgeBlock, ValueProp.Unpowered, null);
        await PowerCmd.Apply<MegaCrit.Sts2.Core.Models.Powers.WeakPower>(ctx, target, JudgeWeak, Owner, null);
    }
}
