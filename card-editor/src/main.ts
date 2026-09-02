// HelloSpire card & relic editor.
//
// Layout: controls bar, a wall of faces, and a side panel for whatever is
// selected. Saving is explicit — nothing writes to the repo until you press
// save, because every write lands in a tracked source file.

import {
  fetchCards,
  fetchRelics,
  fetchSchema,
  fetchWarnings,
  saveCard,
  saveRelic,
  type CardRecord,
  type RelicRecord,
  type Schema,
  type Warning,
} from "./api.ts";
import { cardFace, el, relicFace } from "./card-face.ts";
import { imageFileFromDrop, openCropModal, type ArtKind } from "./art-drop.ts";
import {
  emptyFilters,
  facets,
  filterCards,
  filterRelics,
  toggle,
  type FilterState,
  type SortKey,
} from "./filters.ts";
import { danglingVars } from "./text.ts";
import "./styles.css";

type Tab = "cards" | "relics" | "warnings";

interface State {
  tab: Tab;
  cards: CardRecord[];
  relics: RelicRecord[];
  warnings: Warning[];
  schema: Schema | null;
  filters: FilterState;
  /** className of the selected card/relic. */
  selected: string | null;
  /** Preview every card at its upgraded values. */
  upgraded: boolean;
}

const state: State = {
  tab: "cards",
  cards: [],
  relics: [],
  warnings: [],
  schema: null,
  filters: emptyFilters(),
  selected: null,
  upgraded: false,
};

const $ = <T extends HTMLElement>(sel: string): T => document.querySelector<T>(sel)!;

const tabsEl = $("#tabs");
const controlsEl = $("#controls");
const gridEl = $("#grid");
const panelEl = $("#panel");
const statusEl = $("#status");

function setStatus(text: string, kind: "info" | "ok" | "error" = "info"): void {
  statusEl.textContent = text;
  statusEl.className = `status ${kind}`;
}

// ── Loading ─────────────────────────────────────────────────────────────────

async function loadAll(): Promise<void> {
  setStatus("loading…");
  try {
    const [cards, relics, warnings, schema] = await Promise.all([
      fetchCards(),
      fetchRelics(),
      fetchWarnings(),
      fetchSchema(),
    ]);
    state.cards = cards;
    state.relics = relics;
    state.warnings = warnings;
    state.schema = schema;
    setStatus(`${cards.length} cards · ${relics.length} relics · ${warnings.length} warnings`, "ok");
    renderAll();
  } catch (err) {
    setStatus(`${(err as Error).message} — is the API running? (npm run dev)`, "error");
  }
}

// ── Chrome ──────────────────────────────────────────────────────────────────

function renderTabs(): void {
  tabsEl.replaceChildren();
  const counts: Record<Tab, number> = {
    cards: state.cards.length,
    relics: state.relics.length,
    warnings: state.warnings.length,
  };
  for (const tab of ["cards", "relics", "warnings"] as Tab[]) {
    const b = el("button", state.tab === tab ? "active" : "", `${tab} (${counts[tab]})`);
    b.addEventListener("click", () => {
      state.tab = tab;
      state.selected = null;
      renderAll();
    });
    tabsEl.appendChild(b);
  }
}

function chip(label: string, on: boolean, onClick: () => void): HTMLButtonElement {
  const b = el("button", `chip${on ? " on" : ""}`, label);
  b.addEventListener("click", () => {
    onClick();
    renderAll();
  });
  return b;
}

function group(label: string, children: HTMLElement[]): HTMLDivElement {
  const g = el("div", "chip-group");
  g.appendChild(el("span", "chip-label", label));
  for (const c of children) g.appendChild(c);
  return g;
}

function renderControls(): void {
  controlsEl.replaceChildren();
  if (state.tab === "warnings") return;

  const f = state.filters;
  const fx = facets(state.cards, state.relics);

  const search = el("input", "search");
  search.type = "search";
  search.placeholder = "search name, text or class…";
  search.value = f.search;
  search.addEventListener("input", () => {
    f.search = search.value;
    renderGrid();
  });
  controlsEl.appendChild(search);

  controlsEl.appendChild(
    group(
      "character",
      fx.characters.map((c) => chip(c, f.characters.has(c), () => toggle(f.characters, c))),
    ),
  );

  if (state.tab === "cards") {
    controlsEl.appendChild(
      group(
        "type",
        fx.types.map((t) => chip(t, f.types.has(t), () => toggle(f.types, t))),
      ),
    );
    controlsEl.appendChild(
      group(
        "rarity",
        fx.cardRarities.map((r) => chip(r, f.rarities.has(r), () => toggle(f.rarities, r))),
      ),
    );
    controlsEl.appendChild(
      group(
        "cost",
        fx.costs.map((c) =>
          chip(c < 0 ? "X" : String(c), f.costs.has(c), () => toggle(f.costs, c)),
        ),
      ),
    );
    controlsEl.appendChild(
      group("mode", [
        chip("singleplayer", f.modes.has("singleplayer"), () => toggle(f.modes, "singleplayer")),
        chip("multiplayer only", f.modes.has("multiplayer"), () => toggle(f.modes, "multiplayer")),
      ]),
    );
  } else {
    controlsEl.appendChild(
      group(
        "rarity",
        fx.relicRarities.map((r) => chip(r, f.rarities.has(r), () => toggle(f.rarities, r))),
      ),
    );
  }

  controlsEl.appendChild(
    group("missing", [
      chip("no art", f.missingArt, () => (f.missingArt = !f.missingArt)),
      chip("no text", f.missingText, () => (f.missingText = !f.missingText)),
    ]),
  );

  if (state.tab === "cards") {
    const sort = el("select", "sort");
    const options: [SortKey, string][] = [
      ["name", "name"],
      ["cost", "cost"],
      ["damage", "damage ↓"],
      ["block", "block ↓"],
      ["perEnergy", "per energy ↓"],
      ["rarity", "rarity"],
      ["file", "source order"],
    ];
    for (const [value, label] of options) {
      const o = el("option", "", label);
      o.value = value;
      if (f.sort === value) o.selected = true;
      sort.appendChild(o);
    }
    sort.addEventListener("change", () => {
      f.sort = sort.value as SortKey;
      renderGrid();
    });
    controlsEl.appendChild(group("sort", [sort]));

    controlsEl.appendChild(
      group("preview", [
        chip("upgraded", state.upgraded, () => (state.upgraded = !state.upgraded)),
      ]),
    );
  }

  const reset = el("button", "chip", "reset");
  reset.addEventListener("click", () => {
    state.filters = emptyFilters();
    renderAll();
  });
  controlsEl.appendChild(reset);
}

// ── Grid ────────────────────────────────────────────────────────────────────

function renderGrid(): void {
  gridEl.replaceChildren();

  if (state.tab === "warnings") {
    gridEl.className = "warnings";
    if (state.warnings.length === 0) {
      gridEl.appendChild(el("p", "empty", "no warnings — every class has art and strings."));
      return;
    }
    for (const w of state.warnings) {
      const row = el("div", `warn warn--${w.kind}`);
      row.appendChild(el("span", "warn-kind", w.kind));
      const name = el("button", "warn-name", w.className);
      name.addEventListener("click", () => {
        const isCard = state.cards.some((c) => c.className === w.className);
        state.tab = isCard ? "cards" : "relics";
        state.filters = emptyFilters();
        state.selected = w.className;
        renderAll();
      });
      row.appendChild(name);
      row.appendChild(el("span", "warn-msg", w.message));
      gridEl.appendChild(row);
    }
    return;
  }

  gridEl.className = "grid";
  if (state.tab === "cards") {
    const cards = filterCards(state.cards, state.filters);
    if (cards.length === 0) gridEl.appendChild(el("p", "empty", "nothing matches those filters."));
    for (const c of cards) {
      gridEl.appendChild(
        cardFace(c, {
          size: "grid",
          upgraded: state.upgraded,
          selected: state.selected === c.className,
          onClick: () => select(c.className),
        }),
      );
    }
  } else {
    const relics = filterRelics(state.relics, state.filters);
    if (relics.length === 0) gridEl.appendChild(el("p", "empty", "nothing matches those filters."));
    for (const r of relics) {
      gridEl.appendChild(
        relicFace(r, {
          size: "grid",
          upgraded: false,
          selected: state.selected === r.className,
          onClick: () => select(r.className),
        }),
      );
    }
  }
}

function select(className: string): void {
  state.selected = state.selected === className ? null : className;
  renderAll();
}

// ── Side panel ──────────────────────────────────────────────────────────────

function field(label: string, control: HTMLElement, hint?: string): HTMLDivElement {
  const wrap = el("div", "field");
  wrap.appendChild(el("label", "", label));
  wrap.appendChild(control);
  if (hint) wrap.appendChild(el("span", "field-hint", hint));
  return wrap;
}

/** Disable a control whose value is not written in this class. */
function lock<T extends HTMLInputElement | HTMLSelectElement>(control: T, span: unknown): T {
  if (span === null) {
    control.disabled = true;
    control.title = "inherited from the card's base class — edit it there";
  }
  return control;
}

function numberInput(value: number, onChange: (n: number) => void): HTMLInputElement {
  const i = el("input", "num");
  i.type = "number";
  i.step = "1";
  i.value = String(value);
  i.addEventListener("change", () => {
    const n = Number(i.value);
    if (Number.isFinite(n)) onChange(n);
  });
  return i;
}

function selectInput(
  value: string,
  options: string[],
  onChange: (s: string) => void,
): HTMLSelectElement {
  const s = el("select");
  // A value the schema does not list still has to appear, or opening the panel
  // would silently rewrite it to the first option on the next save.
  for (const opt of options.includes(value) ? options : [value, ...options]) {
    const o = el("option", "", opt);
    o.value = opt;
    if (opt === value) o.selected = true;
    s.appendChild(o);
  }
  s.addEventListener("change", () => onChange(s.value));
  return s;
}

function textArea(value: string, rows: number, onChange: (s: string) => void): HTMLTextAreaElement {
  const t = el("textarea");
  t.rows = rows;
  t.value = value;
  t.addEventListener("change", () => onChange(t.value));
  return t;
}

function renderPanel(): void {
  panelEl.replaceChildren();
  if (state.selected === null) {
    panelEl.className = "panel is-empty";
    return;
  }
  panelEl.className = "panel";

  const card = state.cards.find((c) => c.className === state.selected);
  const relic = state.relics.find((r) => r.className === state.selected);
  if (card) renderCardPanel(card);
  else if (relic) renderRelicPanel(relic);
  else panelEl.className = "panel is-empty";
}

function panelHead(className: string, id: string, file: string, line: number): HTMLDivElement {
  const head = el("div", "panel-head");
  head.appendChild(el("h2", "", className));
  head.appendChild(el("code", "panel-id", id));
  head.appendChild(el("code", "panel-file", `${file}:${line}`));
  const close = el("button", "panel-close", "×");
  close.addEventListener("click", () => {
    state.selected = null;
    renderAll();
  });
  head.appendChild(close);
  return head;
}

function renderCardPanel(card: CardRecord): void {
  const patch: Record<string, unknown> = {};
  const values: Record<string, number> = {};
  const upgrades: Record<string, number> = {};
  const loc: Record<string, string> = {};

  panelEl.appendChild(panelHead(card.className, card.id, card.file, card.line));

  const preview = el("div", "panel-preview");
  preview.appendChild(cardFace(card, { size: "large", upgraded: state.upgraded }));
  panelEl.appendChild(preview);

  const stats = el("div", "panel-section");
  stats.appendChild(el("h3", "", "stats"));

  // A card on an intermediate base (the Paladin's Seals extend SealCard) does
  // not write all four of these itself: the base bakes some in and shares them
  // with every subclass. A null span is how the parser says so, and the control
  // is locked rather than hidden — the value is still what the card has, it just
  // is not this card's to change.
  const inherited = `set by ${card.via ?? "its base"}`;
  stats.appendChild(
    field(
      "cost",
      lock(numberInput(card.cost, (n) => (patch["cost"] = n)), card.costSpan),
      card.costSpan ? "-1 is X" : inherited,
    ),
  );
  stats.appendChild(
    field(
      "type",
      lock(
        selectInput(card.type, state.schema?.cardTypes ?? [], (s) => (patch["type"] = s)),
        card.typeSpan,
      ),
      card.typeSpan ? undefined : inherited,
    ),
  );
  stats.appendChild(
    field(
      "rarity",
      lock(
        selectInput(card.rarity, state.schema?.cardRarities ?? [], (s) => (patch["rarity"] = s)),
        card.raritySpan,
      ),
      card.raritySpan ? undefined : inherited,
    ),
  );
  stats.appendChild(
    field(
      "target",
      lock(
        selectInput(
          card.target,
          ["Self", "AnyEnemy", "AllEnemies", "RandomEnemy", "None", "AnyPlayer", "AllPlayers"],
          (s) => (patch["target"] = s),
        ),
        card.targetSpan,
      ),
      card.targetSpan ? undefined : inherited,
    ),
  );
  panelEl.appendChild(stats);

  if (card.vars.length > 0) {
    const varsBox = el("div", "panel-section");
    varsBox.appendChild(el("h3", "", "vars"));
    const table = el("table", "vars");
    const head = el("tr");
    for (const h of ["var", "base", "upgrade", "kind"]) head.appendChild(el("th", "", h));
    table.appendChild(head);

    for (const v of card.vars) {
      const row = el("tr");
      row.appendChild(el("td", "var-name", v.name));

      const baseCell = el("td");
      baseCell.appendChild(numberInput(v.value, (n) => (values[v.name] = n)));
      row.appendChild(baseCell);

      const upCell = el("td");
      if (v.upgrade === null) {
        // No UpgradeValueBy call exists to retarget, and inventing one means
        // writing a statement into OnUpgrade — a code change, not a number.
        upCell.appendChild(el("span", "no-upgrade", "—"));
      } else {
        // An upgrade with no span is one the card inherits: the UpgradeValueBy
        // lives in the base class and every card on that base shares it, so it
        // shows its value but cannot be edited from here.
        upCell.appendChild(
          lock(
            numberInput(v.upgrade, (n) => (upgrades[v.name] = n)),
            v.upgradeSpan,
          ),
        );
      }
      row.appendChild(upCell);

      // A const-backed var edits the `private const` line, which other code in
      // the class may also read — worth saying so before someone changes it.
      const kindCell = el("td", "var-kind", v.kind);
      if (v.constName !== null) {
        kindCell.appendChild(el("span", "var-const", ` const ${v.constName}`));
      }
      row.appendChild(kindCell);
      table.appendChild(row);
    }
    varsBox.appendChild(table);
    panelEl.appendChild(varsBox);
  }

  const text = el("div", "panel-section");
  text.appendChild(el("h3", "", "text"));
  const title = card.loc["title"] ?? "";
  const desc = card.loc["description"] ?? "";
  const upDesc = card.loc["upgrade.description"] ?? "";
  const titleInput = el("input");
  titleInput.value = title;
  titleInput.addEventListener("change", () => (loc["title"] = titleInput.value));
  text.appendChild(field("title", titleInput));
  text.appendChild(
    field("description", textArea(desc, 3, (s) => (loc["description"] = s)), "{Var} · [gold]…[/gold]"),
  );
  text.appendChild(
    field(
      "upgrade description",
      textArea(upDesc, 3, (s) => (loc["upgrade.description"] = s)),
      "blank falls back to the base text",
    ),
  );
  const dangling = danglingVars(desc + upDesc, card.vars);
  if (dangling.length > 0) {
    text.appendChild(el("p", "warn-inline", `text references undeclared vars: ${dangling.join(", ")}`));
  }
  panelEl.appendChild(text);

  panelEl.appendChild(artSection("card", card.slug, card.art.big !== null));

  panelEl.appendChild(
    saveBar(async () => {
      if (Object.keys(values).length > 0) patch["values"] = values;
      if (Object.keys(upgrades).length > 0) patch["upgrades"] = upgrades;
      const body = {
        ...(Object.keys(patch).length > 0 ? { patch } : {}),
        ...(Object.keys(loc).length > 0 ? { loc } : {}),
      };
      if (Object.keys(body).length === 0) {
        setStatus("nothing changed", "info");
        return;
      }
      const updated = await saveCard(card.className, body);
      state.cards = state.cards.map((c) => (c.className === updated.className ? updated : c));
      setStatus(`saved ${updated.className} → ${updated.file}`, "ok");
      await refreshWarnings();
      renderAll();
    }),
  );
}

function renderRelicPanel(relic: RelicRecord): void {
  const patch: Record<string, unknown> = {};
  const loc: Record<string, string> = {};

  panelEl.appendChild(panelHead(relic.className, relic.id, relic.file, relic.line));

  const preview = el("div", "panel-preview");
  preview.appendChild(relicFace(relic, { size: "large", upgraded: false }));
  panelEl.appendChild(preview);

  const stats = el("div", "panel-section");
  stats.appendChild(el("h3", "", "stats"));
  stats.appendChild(
    field(
      "rarity",
      selectInput(relic.rarity, state.schema?.relicRarities ?? [], (s) => (patch["rarity"] = s)),
    ),
  );
  stats.appendChild(
    el(
      "p",
      "field-note",
      "A relic's numbers live inside its behaviour, where nothing marks which " +
        "argument is the balance knob — edit those in " +
        relic.file +
        ".",
    ),
  );
  panelEl.appendChild(stats);

  const text = el("div", "panel-section");
  text.appendChild(el("h3", "", "text"));
  const titleInput = el("input");
  titleInput.value = relic.loc["title"] ?? "";
  titleInput.addEventListener("change", () => (loc["title"] = titleInput.value));
  text.appendChild(field("title", titleInput));
  text.appendChild(
    field(
      "description",
      textArea(relic.loc["description"] ?? "", 3, (s) => (loc["description"] = s)),
    ),
  );
  text.appendChild(
    field(
      "flavor",
      textArea(relic.loc["flavor"] ?? "", 2, (s) => (loc["flavor"] = s)),
    ),
  );
  panelEl.appendChild(text);

  panelEl.appendChild(artSection("relic", relic.slug, relic.art.big !== null));

  panelEl.appendChild(
    saveBar(async () => {
      const body = {
        ...(Object.keys(patch).length > 0 ? { patch } : {}),
        ...(Object.keys(loc).length > 0 ? { loc } : {}),
      };
      if (Object.keys(body).length === 0) {
        setStatus("nothing changed", "info");
        return;
      }
      const updated = await saveRelic(relic.className, body);
      state.relics = state.relics.map((r) => (r.className === updated.className ? updated : r));
      setStatus(`saved ${updated.className} → ${updated.file}`, "ok");
      await refreshWarnings();
      renderAll();
    }),
  );
}

function artSection(kind: ArtKind, slug: string, hasBig: boolean): HTMLDivElement {
  const box = el("div", "panel-section");
  box.appendChild(el("h3", "", "art"));
  box.appendChild(
    el(
      "p",
      "field-note",
      hasBig
        ? `drop an image on the preview above to replace ${slug}.png`
        : `no art yet — drop an image on the preview above to create ${slug}.png`,
    ),
  );
  const pick = el("button", "chip", "choose file…");
  const input = el("input");
  input.type = "file";
  input.accept = "image/*";
  input.style.display = "none";
  input.addEventListener("change", () => {
    const file = input.files?.[0];
    if (file) startCrop(kind, slug, file);
  });
  pick.addEventListener("click", () => input.click());
  box.append(pick, input);
  return box;
}

function saveBar(onSave: () => Promise<void>): HTMLDivElement {
  const bar = el("div", "panel-actions");
  const save = el("button", "primary", "save");
  save.addEventListener("click", async () => {
    save.disabled = true;
    try {
      await onSave();
    } catch (err) {
      setStatus((err as Error).message, "error");
    } finally {
      save.disabled = false;
    }
  });
  bar.appendChild(save);
  bar.appendChild(el("span", "hint", "writes the .cs source and the string table"));
  return bar;
}

// ── Art drop (delegated: covers both the grid and the panel preview) ────────

function startCrop(kind: ArtKind, slug: string, file: File): void {
  openCropModal(kind, slug, file, {
    onStatus: setStatus,
    onSaved: async (result) => {
      const note =
        result.needsImport.length > 0
          ? " — open the Godot project once so it writes the .import files"
          : "";
      setStatus(`wrote ${result.written.join(", ")}${note}`, "ok");
      await Promise.all([reloadContent(), refreshWarnings()]);
      renderAll();
    },
  });
}

function faceFromEvent(ev: Event): HTMLElement | null {
  const target = ev.target as HTMLElement | null;
  return target?.closest<HTMLElement>(".card-face") ?? null;
}

document.addEventListener("dragover", (ev) => {
  const face = faceFromEvent(ev);
  if (!face) return;
  ev.preventDefault();
  face.classList.add("is-drop-target");
});

document.addEventListener("dragleave", (ev) => {
  faceFromEvent(ev)?.classList.remove("is-drop-target");
});

document.addEventListener("drop", (ev) => {
  const face = faceFromEvent(ev);
  if (!face) return;
  ev.preventDefault();
  face.classList.remove("is-drop-target");
  const file = imageFileFromDrop(ev as DragEvent);
  const slug = face.dataset["slug"];
  const kind = face.dataset["kind"] as ArtKind | undefined;
  if (!file || !slug || !kind) return;
  startCrop(kind, slug, file);
});

async function reloadContent(): Promise<void> {
  const [cards, relics] = await Promise.all([fetchCards(), fetchRelics()]);
  state.cards = cards;
  state.relics = relics;
}

async function refreshWarnings(): Promise<void> {
  state.warnings = await fetchWarnings();
}

// ── Root ────────────────────────────────────────────────────────────────────

function renderAll(): void {
  renderTabs();
  renderControls();
  renderGrid();
  renderPanel();
}

void loadAll();
