// Relics, read from HelloSpireCode/**/Relics.
//
// A relic's tunable surface is much smaller than a card's. Cards keep their
// numbers in DynamicVars, which the game itself reads back for the card text;
// a relic's numbers are ordinary arguments inside its behaviour (`Revolver
// .Load(ctx, Gun, Rounds.Lead, 3)`), with nothing marking which of them is the
// balance knob. So this parser claims only what it can identify unambiguously:
// the rarity. Everything else a designer changes about a relic — its name,
// description, flavour and art — lives outside the C# anyway.

import { classToId, classToSlug } from "./ids.ts";
import type { Edit, Span } from "./edits.ts";

export interface ParsedRelic {
  className: string;
  id: string;
  slug: string;
  file: string;
  base: string;
  character: string;
  summary: string;
  line: number;
  rarity: string;
  raritySpan: Span | null;
}

export const RELIC_RARITIES = [
  "Starter",
  "Common",
  "Uncommon",
  "Rare",
  "Boss",
  "Shop",
  "Event",
] as const;

const DECL = /(?:public|internal)\s+sealed\s+class\s+(\w+)\s*:\s*(?:[\w.]*\.)?(\w*Relic)\b([^{]*)/g;
const RARITY = /RelicRarity\.(\w+)/;

const CHARACTERS = ["Gunslinger", "Alchemist", "Paladin"] as const;

export function parseRelics(file: string, source: string): ParsedRelic[] {
  const decls = [...source.matchAll(DECL)];
  const relics: ParsedRelic[] = [];

  for (const [i, m] of decls.entries()) {
    const [full, className, base] = m;
    if (!className || !base) continue;
    const declStart = m.index;
    const bodyStart = declStart + full.length;
    const bodyEnd = i + 1 < decls.length ? decls[i + 1]!.index : source.length;
    const body = source.slice(bodyStart, bodyEnd);

    const rm = RARITY.exec(body);
    const rarityStart = rm?.[1] ? bodyStart + rm.index + rm[0].indexOf(rm[1]) : null;

    relics.push({
      className,
      id: classToId(className),
      slug: classToSlug(className),
      file,
      base,
      character: CHARACTERS.find((c) => base.startsWith(c)) ?? "Unknown",
      summary: summaryAbove(source, declStart),
      line: source.slice(0, declStart).split("\n").length,
      rarity: rm?.[1] ?? "Unknown",
      raritySpan:
        rarityStart === null ? null : { start: rarityStart, end: rarityStart + rm![1]!.length },
    });
  }
  return relics;
}

export interface RelicPatch {
  rarity?: string;
}

export function relicEdits(relic: ParsedRelic, patch: RelicPatch): Edit[] {
  if (patch.rarity === undefined || patch.rarity === relic.rarity) return [];
  if (!RELIC_RARITIES.includes(patch.rarity as (typeof RELIC_RARITIES)[number])) {
    throw new Error(`rarity must be one of ${RELIC_RARITIES.join(", ")}, got ${patch.rarity}`);
  }
  if (!relic.raritySpan) {
    throw new Error(`${relic.className} does not declare a RelicRarity to change`);
  }
  return [{ ...relic.raritySpan, text: patch.rarity }];
}

/** Plain text of the /// <summary> block immediately above `declStart`. */
function summaryAbove(source: string, declStart: number): string {
  const before = source.slice(0, declStart).split("\n");
  const doc: string[] = [];
  for (let i = before.length - 2; i >= 0; i--) {
    const line = before[i]!.trim();
    if (line.startsWith("///")) doc.unshift(line.replace(/^\/{3}\s?/, ""));
    else if (line === "") continue;
    else break;
  }
  const text = doc.join(" ");
  const inner = /<summary>([\s\S]*?)<\/summary>/.exec(text);
  return (inner?.[1] ?? text)
    .replace(/<[^>]+>/g, "")
    .replace(/\s+/g, " ")
    .trim();
}
