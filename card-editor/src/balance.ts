// Balance readouts, derived from the vars a card declares.
//
// The numbers are deliberately shallow — damage and block per energy, and what
// an upgrade buys. They are not a verdict on a card; a Gunslinger card that
// Loads the cylinder does its work through Rounds that never appear as a
// DamageVar. Treat a blank readout as "this card's value is in its text", not
// as "this card does nothing".

import type { ParsedCard } from "./cs-cards.ts";

export interface BalanceStats {
  damage: number;
  block: number;
  /** null when the card costs X or 0 — dividing by it would say nothing. */
  damagePerEnergy: number | null;
  blockPerEnergy: number | null;
  /** Total value an upgrade adds across every var. */
  upgradeGain: number;
  /** True when no var maps to damage or block. */
  utility: boolean;
}

const DAMAGE_KINDS = new Set(["DamageVar"]);
const BLOCK_KINDS = new Set(["BlockVar"]);

export function balanceStats(card: ParsedCard): BalanceStats {
  let damage = 0;
  let block = 0;
  let upgradeGain = 0;

  for (const v of card.vars) {
    if (DAMAGE_KINDS.has(v.kind)) damage += v.value;
    else if (BLOCK_KINDS.has(v.kind)) block += v.value;
    upgradeGain += v.upgrade ?? 0;
  }

  const per = (n: number): number | null =>
    card.cost > 0 && n > 0 ? Math.round((n / card.cost) * 10) / 10 : null;

  return {
    damage,
    block,
    damagePerEnergy: per(damage),
    blockPerEnergy: per(block),
    upgradeGain,
    utility: damage === 0 && block === 0,
  };
}
