// Identity rules for mod content. A card or relic class name is the single
// source of truth: the game derives the localization key AND the art filename
// from it, so the editor derives them the same way rather than storing them.
//
//   class SnapShot
//     → id entry   HELLOSPIRE-SNAP_SHOT   (localization key prefix)
//     → art slug   snap_shot              (images/card_portraits/snap_shot.png)
//
// Verified against HelloSpire/localization/eng/cards.json: every one of the
// 179 localized titles is reachable from a class name by this rule, with no
// leftovers on either side.

export const ID_PREFIX = "HELLOSPIRE-";

/** PascalCase class name → SCREAMING_SNAKE entry (no prefix). */
export function classToEntry(className: string): string {
  return className.replace(/(?<!^)(?=[A-Z])/g, "_").toUpperCase();
}

/** PascalCase class name → full localization id, e.g. HELLOSPIRE-SNAP_SHOT. */
export function classToId(className: string): string {
  return ID_PREFIX + classToEntry(className);
}

/** PascalCase class name → art filename stem, e.g. snap_shot. */
export function classToSlug(className: string): string {
  return classToEntry(className).toLowerCase();
}

/** Human label for a class name, e.g. SnapShot → "Snap Shot". Only a fallback
 *  for content with no localized title yet. */
export function classToLabel(className: string): string {
  return className.replace(/(?<!^)(?=[A-Z])/g, " ");
}
