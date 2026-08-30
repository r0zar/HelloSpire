using MegaCrit.Sts2.Core.Entities.Relics;

namespace HelloSpire.HelloSpireCode.Alchemist.Relics;

/// <summary>
/// The Alchemist's starter relic. STUB: the multiplayer branch references this from
/// Alchemist.StartingRelics but never committed it. Keeps the character loadable; the real
/// effect belongs to the Alchemist's author. See design/alchemist.md.
/// </summary>
public sealed class PortableAlembic : Characters.AlchemistRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
}
