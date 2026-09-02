// The controls bar, and the filter/sort it drives.
//
// 263 cards is too many to scroll, and the questions a designer actually asks
// are narrow: "show me the Gunslinger commons", "which cards have no art yet",
// "sort the attacks by damage per energy". Each control here answers one.

import type { CardRecord, RelicRecord } from "./api.ts";

export type SortKey = "name" | "cost" | "damage" | "block" | "perEnergy" | "rarity" | "file";

export interface FilterState {
  search: string;
  characters: Set<string>;
  types: Set<string>;
  rarities: Set<string>;
  costs: Set<number>;
  /** multiplayer | singleplayer | any. */
  modes: Set<string>;
  /** Only content with no portrait/icon. */
  missingArt: boolean;
  /** Only content with no localized title. */
  missingText: boolean;
  sort: SortKey;
}

export function emptyFilters(): FilterState {
  return {
    search: "",
    characters: new Set(),
    types: new Set(),
    rarities: new Set(),
    costs: new Set(),
    modes: new Set(),
    missingArt: false,
    missingText: false,
    sort: "name",
  };
}

const RARITY_ORDER = ["Starter", "Common", "Uncommon", "Rare", "Special", "Shop", "Boss", "Event"];

function matchesSearch(text: string, record: CardRecord | RelicRecord): boolean {
  if (text === "") return true;
  const q = text.toLowerCase();
  return (
    record.className.toLowerCase().includes(q) ||
    record.slug.includes(q) ||
    (record.loc["title"] ?? "").toLowerCase().includes(q) ||
    (record.loc["description"] ?? "").toLowerCase().includes(q) ||
    record.summary.toLowerCase().includes(q)
  );
}

/**
 * Mode chips are pools, not raw constraint values: "singleplayer" is the card
 * pool a solo run can see (everything not MultiplayerOnly); "multiplayer only"
 * is just the co-op-gated cards. Both/neither selected shows everything.
 */
function matchesMode(f: FilterState, c: CardRecord): boolean {
  if (f.modes.size === 0) return true;
  if (f.modes.has("singleplayer") && c.mode !== "multiplayer") return true;
  if (f.modes.has("multiplayer") && c.mode === "multiplayer") return true;
  return false;
}

/** Shared predicate: everything relics and cards both have. */
function common(f: FilterState, r: CardRecord | RelicRecord): boolean {
  if (!matchesSearch(f.search, r)) return false;
  if (f.characters.size > 0 && !f.characters.has(r.character)) return false;
  if (f.missingArt && r.art.small !== null) return false;
  if (f.missingText && r.loc["title"] !== undefined) return false;
  return true;
}

export function filterCards(cards: CardRecord[], f: FilterState): CardRecord[] {
  const out = cards.filter(
    (c) =>
      common(f, c) &&
      (f.types.size === 0 || f.types.has(c.type)) &&
      (f.rarities.size === 0 || f.rarities.has(c.rarity)) &&
      (f.costs.size === 0 || f.costs.has(c.cost)) &&
      matchesMode(f, c),
  );
  return sortCards(out, f.sort);
}

export function filterRelics(relics: RelicRecord[], f: FilterState): RelicRecord[] {
  const out = relics.filter(
    (r) => common(f, r) && (f.rarities.size === 0 || f.rarities.has(r.rarity)),
  );
  return out.sort((a, b) => {
    const byRarity = RARITY_ORDER.indexOf(a.rarity) - RARITY_ORDER.indexOf(b.rarity);
    return byRarity !== 0 ? byRarity : a.className.localeCompare(b.className);
  });
}

function sortCards(cards: CardRecord[], key: SortKey): CardRecord[] {
  const name = (c: CardRecord): string => c.loc["title"] ?? c.className;
  const byName = (a: CardRecord, b: CardRecord): number => name(a).localeCompare(name(b));

  // Descending for the balance numbers — the outliers you are hunting sort to
  // the top. Ascending for cost and name, where the natural reading is upward.
  const cmp: Record<SortKey, (a: CardRecord, b: CardRecord) => number> = {
    name: byName,
    cost: (a, b) => a.cost - b.cost || byName(a, b),
    damage: (a, b) => b.balance.damage - a.balance.damage || byName(a, b),
    block: (a, b) => b.balance.block - a.balance.block || byName(a, b),
    perEnergy: (a, b) =>
      (b.balance.damagePerEnergy ?? b.balance.blockPerEnergy ?? -1) -
        (a.balance.damagePerEnergy ?? a.balance.blockPerEnergy ?? -1) || byName(a, b),
    rarity: (a, b) =>
      RARITY_ORDER.indexOf(a.rarity) - RARITY_ORDER.indexOf(b.rarity) || byName(a, b),
    file: (a, b) => a.file.localeCompare(b.file) || a.line - b.line,
  };
  return [...cards].sort(cmp[key]);
}

/** Distinct values present in the data, so the chips only offer real options. */
export function facets(cards: CardRecord[], relics: RelicRecord[]) {
  const uniq = <T>(xs: T[]): T[] => [...new Set(xs)];
  return {
    characters: uniq([...cards, ...relics].map((r) => r.character)).sort(),
    types: uniq(cards.map((c) => c.type)).sort(),
    cardRarities: uniq(cards.map((c) => c.rarity)).sort(
      (a, b) => RARITY_ORDER.indexOf(a) - RARITY_ORDER.indexOf(b),
    ),
    relicRarities: uniq(relics.map((r) => r.rarity)).sort(
      (a, b) => RARITY_ORDER.indexOf(a) - RARITY_ORDER.indexOf(b),
    ),
    costs: uniq(cards.map((c) => c.cost)).sort((a, b) => a - b),
  };
}

/** Toggle membership, for chip clicks. */
export function toggle<T>(set: Set<T>, value: T): void {
  if (set.has(value)) set.delete(value);
  else set.add(value);
}
