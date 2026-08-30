// Reading (and writing back) the card set that lives in HelloSpireCode/**/Cards.
//
// A HelloSpire card is a C# class, not a data file: its behaviour is real code
// in OnPlay. What a designer balances, though, is a small and very regular
// surface — the four constructor arguments, the base value of each declared
// DynamicVar, and the per-var upgrade deltas in OnUpgrade. All 263 cards in
// the mod are written in the same shape:
//
//   /// <summary>Fire 1. Draw 1 card.</summary>
//   public sealed class SnapShot() : GunslingerCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
//   {
//       protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(1), new DynamicVar("Deadeye", 0m)];
//       ...
//       protected override void OnUpgrade() => DynamicVars["Deadeye"].UpgradeValueBy(2m);
//   }
//
// This module locates exactly those literals and records their byte ranges.
// Writing replaces the ranges and nothing else, so OnPlay bodies, hover tips,
// comments and formatting survive a save untouched.

import { classToId, classToSlug } from "./ids.ts";
import type { Edit, Span } from "./edits.ts";

export interface CardVar {
  /** How the card indexes it: DynamicVars["Bonus"] → "Bonus". */
  name: string;
  /** Declared constructor, e.g. "DamageVar", "PowerVar<WeakPower>". */
  kind: string;
  /** Base value at level 0. */
  value: number;
  valueSpan: Span;
  /** Set when the value came from a named constant rather than a literal —
   *  the span then points at the constant's own initializer. */
  constName: string | null;
  /** True when the source wrote a decimal literal (6m) rather than an int (6). */
  decimal: boolean;
  /** Amount OnUpgrade adds, or null when the var does not upgrade. */
  upgrade: number | null;
  upgradeSpan: Span | null;
  upgradeDecimal: boolean;
}

export interface ParsedCard {
  className: string;
  /** HELLOSPIRE-SNAP_SHOT */
  id: string;
  /** snap_shot — the art filename stem. */
  slug: string;
  /** Repo-relative source file. */
  file: string;
  /** Base class as written, e.g. GunslingerCard. */
  base: string;
  /** Gunslinger | Alchemist | Paladin, from the base class. */
  character: string;
  /** First line of the /// <summary> above the class, plain text. */
  summary: string;
  line: number;

  cost: number;
  costSpan: Span;
  type: string;
  typeSpan: Span;
  rarity: string;
  raritySpan: Span;
  target: string;
  targetSpan: Span;

  vars: CardVar[];
}

const DECL =
  /(?:public|internal)\s+sealed\s+class\s+(\w+)\(\)\s*:\s*(?:[\w.]*\.)?(\w*Card)\(([^)]*)\)/g;

const CHARACTERS = ["Gunslinger", "Alchemist", "Paladin"] as const;

/** Numeric literal, with the C# decimal suffix when present. */
const NUMBER = /(-?\d+(?:\.\d+)?)(m?)/;

/**
 * Parse every card class in one source file.
 *
 * `file` is only carried through onto the results — parsing reads `source`.
 */
export function parseCards(file: string, source: string): ParsedCard[] {
  const decls = [...source.matchAll(DECL)];
  const cards: ParsedCard[] = [];

  for (const [i, m] of decls.entries()) {
    const [full, className, base, argText] = m;
    if (!className || !base || argText === undefined) continue;
    const declStart = m.index;
    const argsStart = declStart + full.lastIndexOf(argText);

    const ctor = parseCtorArgs(argText, argsStart);
    if (!ctor) continue;

    // The class body runs to the next declaration, or to end of file.
    const bodyEnd = i + 1 < decls.length ? decls[i + 1]!.index : source.length;
    const body = source.slice(declStart, bodyEnd);

    cards.push({
      className,
      id: classToId(className),
      slug: classToSlug(className),
      file,
      base,
      character: CHARACTERS.find((c) => base.startsWith(c)) ?? "Unknown",
      summary: summaryAbove(source, declStart),
      line: source.slice(0, declStart).split("\n").length,
      ...ctor,
      vars: parseVars(body, declStart),
    });
  }
  return cards;
}

/** The four positional constructor arguments, with their spans. */
function parseCtorArgs(
  argText: string,
  base: number,
):
  | Pick<
      ParsedCard,
      "cost" | "costSpan" | "type" | "typeSpan" | "rarity" | "raritySpan" | "target" | "targetSpan"
    >
  | undefined {
  const cost = spanOf(argText, /(-?\d+)/, base);
  const type = spanOf(argText, /CardType\.(\w+)/, base);
  const rarity = spanOf(argText, /CardRarity\.(\w+)/, base);
  const target = spanOf(argText, /TargetType\.(\w+)/, base);
  if (!cost || !type || !rarity || !target) return undefined;

  return {
    cost: Number(cost.text),
    costSpan: cost.span,
    type: type.text,
    typeSpan: type.span,
    rarity: rarity.text,
    raritySpan: rarity.span,
    target: target.text,
    targetSpan: target.span,
  };
}

/** Match `re` in `text` and return capture 1 plus its absolute span. */
function spanOf(
  text: string,
  re: RegExp,
  base: number,
): { text: string; span: Span } | undefined {
  const m = re.exec(text);
  if (!m || m[1] === undefined) return undefined;
  const start = base + m.index + m[0].indexOf(m[1]);
  return { text: m[1], span: { start, end: start + m[1].length } };
}

/**
 * The vars a card declares, joined to the upgrade deltas that mention them.
 * `body` is the class body; `base` is its absolute offset in the file.
 */
function parseVars(body: string, base: number): CardVar[] {
  const vars = parseCanonicalVars(body, base);
  const upgrades = parseUpgrades(body, base);
  for (const v of vars) {
    const up = upgrades.get(v.name);
    if (!up) continue;
    v.upgrade = up.value;
    v.upgradeSpan = up.span;
    v.upgradeDecimal = up.decimal;
  }
  return vars;
}

const NEW_VAR = /new\s+(\w+Var(?:<\s*\w+\s*>)?)\s*\(/g;

function parseCanonicalVars(body: string, base: number): CardVar[] {
  const at = body.indexOf("CanonicalVars");
  if (at === -1) return [];
  const open = body.indexOf("[", at);
  if (open === -1) return [];
  const close = matchBracket(body, open, "[", "]");
  if (close === -1) return [];

  const region = body.slice(open, close);
  const out: CardVar[] = [];

  for (const m of region.matchAll(NEW_VAR)) {
    const kind = m[1]!;
    const argsOpen = m.index + m[0].length - 1;
    const argsClose = matchBracket(region, argsOpen, "(", ")");
    if (argsClose === -1) continue;
    const args = region.slice(argsOpen + 1, argsClose);

    // Usually the value is a literal in the argument list. One card writes it
    // as a named constant instead (`new DynamicVar("Threshold", ArmorThreshold)`);
    // resolving that keeps the var visible and editable rather than dropping it
    // silently, which would also make its {Threshold} placeholder look dangling.
    const num = NUMBER.exec(args);
    const literal = num
      ? {
          value: Number(num[1]),
          span: {
            start: base + open + argsOpen + 1 + num.index,
            end: base + open + argsOpen + 1 + num.index + num[0].length,
          },
          decimal: num[2] === "m",
          constName: null as string | null,
        }
      : resolveConst(args, body, base);
    if (!literal) continue;

    const named = /"([^"]+)"/.exec(args);
    out.push({
      name: named?.[1] ?? defaultVarName(kind),
      kind,
      value: literal.value,
      valueSpan: literal.span,
      constName: literal.constName,
      decimal: literal.decimal,
      upgrade: null,
      upgradeSpan: null,
      upgradeDecimal: true,
    });
  }
  return out;
}

/**
 * An argument that names a `const` declared in the same class: return the
 * constant's value and the span of ITS initializer, so an edit rewrites the
 * constant — the one place the number actually lives.
 */
function resolveConst(
  args: string,
  body: string,
  base: number,
): { value: number; span: Span; decimal: boolean; constName: string } | null {
  for (const m of args.matchAll(/(?:^|,)\s*([A-Za-z_]\w*)\s*(?=,|$)/g)) {
    const ident = m[1]!;
    const decl = new RegExp(`const\\s+\\w+\\s+${ident}\\s*=\\s*(-?\\d+(?:\\.\\d+)?)(m?)`).exec(body);
    if (!decl?.[1]) continue;
    const start = base + decl.index + decl[0].lastIndexOf(decl[1]);
    return {
      value: Number(decl[1]),
      span: { start, end: start + decl[1].length + decl[2]!.length },
      decimal: decl[2] === "m",
      constName: ident,
    };
  }
  return null;
}

/** `DamageVar` → "Damage"; `PowerVar<WeakPower>` → "WeakPower". */
function defaultVarName(kind: string): string {
  const generic = /<\s*(\w+)\s*>/.exec(kind);
  if (generic?.[1]) return generic[1];
  return kind.replace(/Var$/, "");
}

const UPGRADE =
  /DynamicVars(?:\.(\w+)|\[\s*"([^"]+)"\s*\])\s*\.\s*UpgradeValueBy\(\s*(-?\d+(?:\.\d+)?)(m?)/g;

/** var name → the delta OnUpgrade adds to it. */
function parseUpgrades(
  body: string,
  base: number,
): Map<string, { value: number; span: Span; decimal: boolean }> {
  const out = new Map<string, { value: number; span: Span; decimal: boolean }>();
  const at = body.indexOf("OnUpgrade");
  if (at === -1) return out;

  // Everything after OnUpgrade in the class body. Nothing else in these classes
  // calls UpgradeValueBy, so the region does not need a tighter end.
  const region = body.slice(at);
  for (const m of region.matchAll(UPGRADE)) {
    const name = m[1] ?? m[2];
    const digits = m[3]!;
    if (!name) continue;
    const start = base + at + m.index + m[0].lastIndexOf(digits);
    out.set(name, {
      value: Number(digits),
      span: { start, end: start + digits.length + m[4]!.length },
      decimal: m[4] === "m",
    });
  }
  return out;
}

/** Index of the bracket closing the one at `open`, or -1. */
function matchBracket(text: string, open: number, o: string, c: string): number {
  let depth = 0;
  for (let i = open; i < text.length; i++) {
    const ch = text[i];
    if (ch === '"') {
      i = text.indexOf('"', i + 1);
      if (i === -1) return -1;
      continue;
    }
    if (ch === o) depth++;
    else if (ch === c && --depth === 0) return i;
  }
  return -1;
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

// ── Writing ─────────────────────────────────────────────────────────────────

/** The editable surface of a card, as the UI sends it back. */
export interface CardPatch {
  cost?: number;
  type?: string;
  rarity?: string;
  target?: string;
  /** var name → new base value. */
  values?: Record<string, number>;
  /** var name → new upgrade delta. Only vars that already upgrade. */
  upgrades?: Record<string, number>;
}

export const CARD_TYPES = ["Attack", "Skill", "Power", "Status", "Curse"] as const;
export const CARD_RARITIES = ["Starter", "Common", "Uncommon", "Rare", "Special"] as const;

/**
 * Turn a patch into source edits against the file `card` was parsed from.
 * Fields absent from the patch produce no edit, so a save only ever touches
 * what the designer actually changed.
 */
export function cardEdits(card: ParsedCard, patch: CardPatch): Edit[] {
  const edits: Edit[] = [];

  if (patch.cost !== undefined && patch.cost !== card.cost) {
    if (!Number.isInteger(patch.cost) || patch.cost < -1) {
      throw new Error(`cost must be an integer >= -1, got ${patch.cost}`);
    }
    edits.push({ ...card.costSpan, text: String(patch.cost) });
  }
  if (patch.type !== undefined && patch.type !== card.type) {
    requireEnum("type", patch.type, CARD_TYPES);
    edits.push({ ...card.typeSpan, text: patch.type });
  }
  if (patch.rarity !== undefined && patch.rarity !== card.rarity) {
    requireEnum("rarity", patch.rarity, CARD_RARITIES);
    edits.push({ ...card.raritySpan, text: patch.rarity });
  }
  if (patch.target !== undefined && patch.target !== card.target) {
    if (!/^\w+$/.test(patch.target)) throw new Error(`bad target: ${patch.target}`);
    edits.push({ ...card.targetSpan, text: patch.target });
  }

  for (const [name, value] of Object.entries(patch.values ?? {})) {
    const v = card.vars.find((x) => x.name === name);
    if (!v) throw new Error(`${card.className} has no var "${name}"`);
    if (value === v.value) continue;
    edits.push({ ...v.valueSpan, text: numberLiteral(value, v.decimal) });
  }

  for (const [name, value] of Object.entries(patch.upgrades ?? {})) {
    const v = card.vars.find((x) => x.name === name);
    if (!v) throw new Error(`${card.className} has no var "${name}"`);
    if (!v.upgradeSpan) {
      throw new Error(`${card.className}.${name} has no upgrade to change`);
    }
    if (value === v.upgrade) continue;
    edits.push({ ...v.upgradeSpan, text: numberLiteral(value, v.upgradeDecimal) });
  }

  return edits;
}

function requireEnum(field: string, value: string, allowed: readonly string[]): void {
  if (!allowed.includes(value)) {
    throw new Error(`${field} must be one of ${allowed.join(", ")}, got ${value}`);
  }
}

/** Re-emit a number in the same style the source used for that literal. */
function numberLiteral(value: number, decimal: boolean): string {
  if (!Number.isFinite(value)) throw new Error(`not a finite number: ${value}`);
  const s = Number.isInteger(value) ? String(value) : String(Number(value.toFixed(4)));
  return decimal ? `${s}m` : s;
}
