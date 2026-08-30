using System.Collections.Generic;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Alchemist.Lab;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Rooms;

namespace HelloSpire.HelloSpireCode.Alchemist.Relics;

/// <summary>Every in-combat Gold gain yields 3 more. The Transmutation deck compounds.</summary>
public sealed class GoldenCrucible : Characters.AlchemistRelic, IGoldModifier
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public int ModifyGoldGain(LabContext lab, int amount)
    {
        Flash();
        return 3;
    }
}
