using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HelloSpire.HelloSpireCode.Alchemist.Potions;
using HelloSpire.HelloSpireCode.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace HelloSpire.HelloSpireCode.Alchemist;

/// <summary>
/// The real ILabBridge: every operation the quarantine declared, implemented against the game's
/// actual APIs (PotionCmd/PotionFactory for the belt, PlayerCmd for Gold, CreatureCmd.LoseMaxHp
/// for Render, CardSelectCmd for choices, CardCmd/CardPileCmd for the piles).
///
/// Two deliberate deviations from the stub's ideal, both documented rather than hidden:
/// - Invest and Render auto-Pay when affordable instead of prompting. Playing a card whose text
///   says "Invest 3" is the consent, exactly as playing Bloodletting consents to the HP loss;
///   a bespoke Pay-or-Decline popup is a later nicety, not a correctness requirement.
/// - ChoosePotion takes the first held Potion rather than asking. The game has no potion-select
///   UI to borrow; a real picker is the remaining polish item.
/// </summary>
public sealed class WiredLabBridge : ILabBridge
{
    // -------------------------------------------------------------------------- Gold
    public int Gold(Player player) => player.Gold;

    public Task GainGold(Player player, int amount) => PlayerCmd.GainGold(amount, player);

    public async Task<bool> OfferInvest(PlayerChoiceContext ctx, Player player, int cost)
    {
        cost = System.Math.Max(1, cost - AlchemistHooks.InvestDiscount(LabContext.From(player)));
        if (player.Gold < cost) return false;
        await PlayerCmd.LoseGold(cost, player);
        return true;
    }

    // -------------------------------------------------------------------------- Max HP
    public async Task<bool> OfferRender(PlayerChoiceContext ctx, Player player, int cost)
    {
        if (player.Creature == null || player.Creature.MaxHp - cost <= 0) return false;
        await CreatureCmd.LoseMaxHp(ctx, player.Creature, cost, isFromCard: true);
        return true;
    }

    // -------------------------------------------------------------------------- the belt
    public IReadOnlyList<PotionModel> Held(Player player) => player.Potions.ToList();

    public int SlotCount(Player player) => player.PotionSlots.Count;

    public async Task<PotionModel?> Brew(PlayerChoiceContext ctx, Player player, PotionModel potion)
    {
        if (!player.HasOpenPotionSlots) return null;   // documented Brew-failure: the Potion is lost
        var result = await PotionCmd.TryToProcure(potion, player);
        return result.success ? result.potion : null;
    }

    public Task Discard(PlayerChoiceContext ctx, Player player, PotionModel potion) =>
        PotionCmd.Discard(potion);

    public PotionModel? RandomCombatPotion(Player player, PotionRarity? rarity = null)
    {
        // Curated per design/alchemist.md: nothing whose value outlives the fight. The Stone is
        // non-Brewable by rule; Aurum Tincture makes real Gold.
        var options = PotionFactory.GetPotionOptions(player, [])
            .Where(p => p is not PhilosophersStone and not AurumTincture)
            .Where(p => rarity == null || p.Rarity == rarity)
            .ToList();
        if (options.Count == 0) return null;
        return player.RunState.Rng.CombatPotionGeneration.NextItem(options).ToMutable();
    }

    public PotionModel? NamedPotion(BasePotion which) => which switch
    {
        BasePotion.Block => ModelDb.Potion<BlockPotion>().ToMutable(),
        BasePotion.ExplosiveAmpoule => ModelDb.Potion<ExplosiveAmpoule>().ToMutable(),
        BasePotion.Energy => ModelDb.Potion<EnergyPotion>().ToMutable(),
        _ => null,
    };

    // -------------------------------------------------------------------------- choices
    public Task<PotionModel?> ChoosePotion(PlayerChoiceContext ctx, Player player, IReadOnlyList<PotionModel> from) =>
        Task.FromResult<PotionModel?>(from.Count > 0 ? from[0] : null);

    public Task<CardModel?> ChooseCard(PlayerChoiceContext ctx, Player player, IReadOnlyList<CardModel> from) =>
        CardSelectCmd.FromChooseACardScreen(ctx, from, player);

    // -------------------------------------------------------------------------- hand and piles
    public IReadOnlyList<CardModel> Hand(Player player) =>
        PileType.Hand.GetPile(player).Cards.ToList();

    public Task Exhaust(PlayerChoiceContext ctx, Player player, CardModel card) =>
        CardCmd.Exhaust(ctx, card);

    public Task DiscardCard(PlayerChoiceContext ctx, Player player, CardModel card) =>
        CardCmd.Discard(ctx, card);

    public async Task BottomOfDraw(PlayerChoiceContext ctx, Player player, CardModel card) =>
        await CardPileCmd.Add(card, PileType.Draw, CardPilePosition.Bottom);

    public CardModel? RandomCard(Player player, CardRarity? rarity = null, CardType? type = null)
    {
        var options = ModelDb.CardPool<AlchemistCardPool>()
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .Where(c => c.Rarity is not CardRarity.Basic and not CardRarity.Status and not CardRarity.Curse)
            .Where(c => rarity == null || c.Rarity == rarity)
            .Where(c => type == null || c.Type == type)
            .ToList();
        if (options.Count == 0) return null;
        var model = player.RunState.Rng.CombatTargets.NextItem(options);
        return player.RunState.CreateCard(model, player);
    }

    public async Task CreateInHand(PlayerChoiceContext ctx, Player player, CardModel card, bool costsZeroThisTurn)
    {
        await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, player);
        if (costsZeroThisTurn) card.EnergyCost.SetThisTurnOrUntilPlayed(0);
    }

    public Task UpgradeForCombat(PlayerChoiceContext ctx, Player player, CardModel card)
    {
        if (card.IsUpgradable) CardCmd.Upgrade(card, CardPreviewStyle.None);
        return Task.CompletedTask;
    }

    public Task UpgradePermanently(Player player, CardModel card)
    {
        // Combat cards are clones; the deck original is what survives the fight.
        if (card.IsUpgradable) CardCmd.Upgrade(card, CardPreviewStyle.None);
        if (card.CloneOf is { IsUpgradable: true } original) CardCmd.Upgrade(original, CardPreviewStyle.None);
        return Task.CompletedTask;
    }
}
