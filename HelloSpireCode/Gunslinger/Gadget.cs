namespace HelloSpire.HelloSpireCode.Gunslinger;

/// <summary>
/// A Gadget: a card that needs no ammunition.
///
/// The Gunslinger's problem has always been that the gun is the whole character. Loading costs a
/// card, Firing costs another, and a hand with neither is a hand that does nothing — so every
/// build in the set was some arrangement of the same two verbs. Gadgets are the other half: cards
/// that touch none of Load, Fire, Cycle or Spin, pay out in debuffs, Block and Armor, and are
/// worth exactly as much with the cylinder empty as with it full.
///
/// The interface is a marker and carries no members. It exists so payoff powers can ask
/// <c>cardPlay.Card is IGadget</c> from the base game's <c>AfterCardPlayed</c> hook, which is
/// already how <see cref="Powers.RideTogetherPower"/> watches what allies play. That is the whole
/// mechanism — no dispatch, no listener registry, nothing for a card to remember to announce.
///
/// The rule a card has to honour to wear it is the one the player reads off the card: no cylinder
/// verbs. Reading the cylinder is still allowed — a Gadget may ask how much Armor you are holding
/// — but a Gadget that Loads is a cartridge card wearing the wrong word, and the archetype stops
/// meaning anything the moment one ships.
/// </summary>
public interface IGadget;
