// The card tile. Deliberately close to what the game renders — cost badge,
// portrait window, title, interpolated description — so that balancing here
// means reading the same sentence a player reads, with the number you are
// about to change already substituted in.

import { artUrl, type CardRecord, type RelicRecord } from "./api.ts";
import { classToLabel } from "./ids.ts";
import { danglingVars, renderText } from "./text.ts";

export const TYPE_COLOR: Record<string, string> = {
  Attack: "#ff7777",
  Skill: "#77bbff",
  Power: "#cc99ff",
  Status: "#8a8a9a",
  Curse: "#b06a9a",
};

export const RARITY_COLOR: Record<string, string> = {
  Starter: "#8a8a9a",
  Common: "#d9d9e6",
  Uncommon: "#66d9c4",
  Rare: "#ffd54f",
  Special: "#ff9f6e",
  Boss: "#ff6e6e",
  Shop: "#7ec8ff",
  Event: "#c6a4ff",
};

export interface FaceOpts {
  size: "grid" | "large";
  /** Render every var at its upgraded value. */
  upgraded: boolean;
  selected?: boolean;
  onClick?: () => void;
}

export function el<K extends keyof HTMLElementTagNameMap>(
  tag: K,
  className?: string,
  text?: string,
): HTMLElementTagNameMap[K] {
  const node = document.createElement(tag);
  if (className) node.className = className;
  if (text !== undefined) node.textContent = text;
  return node;
}

/** The portrait window, or a drop hint when the slug has no PNG yet. */
function artWindow(
  kind: "card" | "relic",
  slug: string,
  mtime: number | null,
  size: FaceOpts["size"],
): HTMLDivElement {
  const win = el("div", `cf-art cf-art--${kind}`);
  const url = artUrl(kind, slug, size === "large" ? "big" : "small", mtime);
  if (url) {
    const img = el("img");
    img.src = url;
    img.alt = slug;
    img.draggable = false;
    win.appendChild(img);
  } else {
    win.classList.add("cf-art--empty");
    win.appendChild(el("span", "cf-hint", "drop art"));
  }
  return win;
}

/** Damage/block per energy, and what an upgrade buys. */
function statsStrip(card: CardRecord): HTMLDivElement {
  const s = card.balance;
  const strip = el("div", "cf-stats");
  if (s.damage > 0) {
    strip.appendChild(el("span", "cf-stat cf-stat--dmg", `⚔ ${s.damage}`));
    if (s.damagePerEnergy !== null) {
      strip.appendChild(el("span", "cf-stat", `${s.damagePerEnergy}/⚡`));
    }
  }
  if (s.block > 0) {
    strip.appendChild(el("span", "cf-stat cf-stat--blk", `🛡 ${s.block}`));
    if (s.blockPerEnergy !== null) {
      strip.appendChild(el("span", "cf-stat", `${s.blockPerEnergy}/⚡`));
    }
  }
  if (s.utility) strip.appendChild(el("span", "cf-stat cf-stat--util", "utility"));
  if (s.upgradeGain > 0) strip.appendChild(el("span", "cf-stat cf-stat--up", `+${s.upgradeGain}⭡`));
  return strip;
}

export function cardFace(card: CardRecord, opts: FaceOpts): HTMLDivElement {
  const face = el(
    "div",
    `card-face card-face--${opts.size}${opts.selected ? " is-selected" : ""}`,
  );
  face.dataset.kind = "card";
  face.dataset.slug = card.slug;
  face.dataset.className = card.className;
  face.style.setProperty("--type-color", TYPE_COLOR[card.type] ?? "#d9d9e6");
  face.style.setProperty("--rarity-color", RARITY_COLOR[card.rarity] ?? "#d9d9e6");

  const head = el("div", "cf-head");
  const cost =
    opts.upgraded && card.costUpgrade != null
      ? Math.max(0, card.cost + card.costUpgrade)
      : card.cost;
  const costEl = el("span", "cf-cost", cost < 0 ? "X" : String(cost));
  if (opts.upgraded && card.costUpgrade != null && card.cost >= 0) costEl.style.color = "#7ee08a";
  head.appendChild(costEl);
  head.appendChild(
    el(
      "span",
      "cf-type",
      opts.size === "large" ? `${card.type} · ${card.rarity}` : card.type,
    ),
  );
  face.appendChild(head);

  face.appendChild(artWindow("card", card.slug, card.art.small, opts.size));

  const title = card.loc["title"];
  const name = el("div", "cf-name", title ?? classToLabel(card.className));
  if (title === undefined) name.classList.add("cf-name--untranslated");
  face.appendChild(name);

  const desc = card.loc["description"];
  if (desc !== undefined) {
    const body = el("div", "cf-text", renderText(desc, card.vars, { upgraded: opts.upgraded }));
    const dangling = danglingVars(desc, card.vars);
    if (dangling.length > 0) {
      body.classList.add("cf-text--dangling");
      body.title = `no such var: ${dangling.join(", ")}`;
    }
    face.appendChild(body);
  } else {
    face.appendChild(el("div", "cf-text cf-text--missing", card.summary || "no card text yet"));
  }

  face.appendChild(statsStrip(card));

  if (opts.onClick) {
    face.classList.add("is-clickable");
    face.addEventListener("click", opts.onClick);
  }
  return face;
}

export function relicFace(relic: RelicRecord, opts: FaceOpts): HTMLDivElement {
  const face = el(
    "div",
    `card-face relic-face card-face--${opts.size}${opts.selected ? " is-selected" : ""}`,
  );
  face.dataset.kind = "relic";
  face.dataset.slug = relic.slug;
  face.dataset.className = relic.className;
  face.style.setProperty("--type-color", "#ffd54f");
  face.style.setProperty("--rarity-color", RARITY_COLOR[relic.rarity] ?? "#d9d9e6");

  const head = el("div", "cf-head");
  head.appendChild(el("span", "cf-type", relic.rarity));
  head.appendChild(el("span", "cf-char", relic.character));
  face.appendChild(head);

  face.appendChild(artWindow("relic", relic.slug, relic.art.small, opts.size));

  const title = relic.loc["title"];
  const name = el("div", "cf-name", title ?? classToLabel(relic.className));
  if (title === undefined) name.classList.add("cf-name--untranslated");
  face.appendChild(name);

  const desc = relic.loc["description"];
  face.appendChild(
    desc !== undefined
      ? el("div", "cf-text", renderText(desc, [], { upgraded: false }))
      : el("div", "cf-text cf-text--missing", relic.summary || "no relic text yet"),
  );

  const flavor = relic.loc["flavor"];
  if (flavor !== undefined && opts.size === "large") {
    face.appendChild(el("div", "cf-flavor", flavor));
  }

  if (opts.onClick) {
    face.classList.add("is-clickable");
    face.addEventListener("click", opts.onClick);
  }
  return face;
}
