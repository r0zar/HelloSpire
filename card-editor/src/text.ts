// Rendering card text the way the game does.
//
// A localized description is a template over the card's DynamicVars:
//
//   "[gold]Fire[/gold] 1.\nDraw {Cards} card."
//
// {Name} interpolates that var's current value, and the `:diff()` suffix asks
// the game to show base-vs-upgraded. [gold]…[/gold] is the game's inline
// markup. Substituting here is what makes the grid a balance view rather than
// a list of templates: you read the number you are about to change in the
// sentence the player actually sees.

import type { CardVar } from "./cs-cards.ts";

export interface RenderOpts {
  /** Show each var at its upgraded value (base + upgrade delta). */
  upgraded: boolean;
}

/** Value of `name` at the requested level, or null when the card lacks it. */
function valueOf(vars: readonly CardVar[], name: string, upgraded: boolean): number | null {
  const v = vars.find((x) => x.name === name);
  if (!v) return null;
  return upgraded ? v.value + (v.upgrade ?? 0) : v.value;
}

/**
 * Substitute vars into a description and strip the colour markup, leaving the
 * words a player reads. Unknown placeholders are left verbatim — that is the
 * signal that a description references a var the class no longer declares.
 */
export function renderText(text: string, vars: readonly CardVar[], opts: RenderOpts): string {
  return text
    .replace(/\{(\w+)(?::[^}]*)?\}/g, (whole, name: string) => {
      const v = valueOf(vars, name, opts.upgraded);
      return v === null ? whole : String(v);
    })
    .replace(/\[\/?\w+\]/g, "");
}

/** Placeholders in `text` that no var satisfies. */
export function danglingVars(text: string, vars: readonly CardVar[]): string[] {
  const names = new Set(vars.map((v) => v.name));
  const out = new Set<string>();
  for (const m of text.matchAll(/\{(\w+)(?::[^}]*)?\}/g)) {
    if (m[1] && !names.has(m[1])) out.add(m[1]);
  }
  return [...out];
}
