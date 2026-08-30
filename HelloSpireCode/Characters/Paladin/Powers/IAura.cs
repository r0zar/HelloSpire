using System.Threading.Tasks;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;

namespace HelloSpire.HelloSpireCode.Characters.PaladinContent.Powers;

/// <summary>
/// A party-wide Power with a repeatable effect. Auras that act at the start of your turn route
/// that effect through Pulse; Blessed cards call Pulse on every Aura you have when played. That
/// is the engine: Auras are what you build, Blessed cards are what you feed them.
/// </summary>
public interface IAura
{
    Task Pulse(PlayerChoiceContext choiceContext);
}
