// Drag an image onto a face → pan/zoom crop → write both sizes.
//
// The mod stores each portrait twice (250×190 in hand, 1000×760 inspected) and
// each relic icon twice more plus a silhouette. Cropping once and exporting
// every size from the same frame is the whole point of doing this in the
// editor: hand-cropping four files is where mismatched art comes from.

import { uploadArt, type ArtUploadResult } from "./api.ts";

export type ArtKind = "card" | "relic";

interface Size {
  w: number;
  h: number;
}

/** Target sizes per kind, largest first — the crop canvas uses the large one. */
export const ART_SIZES: Record<ArtKind, { big: Size; small: Size }> = {
  card: { big: { w: 1000, h: 760 }, small: { w: 250, h: 190 } },
  relic: { big: { w: 256, h: 256 }, small: { w: 128, h: 128 } },
};

/** First image file in a drop, if any. */
export function imageFileFromDrop(ev: DragEvent): File | null {
  for (const f of ev.dataTransfer?.files ?? []) {
    if (f.type.startsWith("image/")) return f;
  }
  return null;
}

interface CropState {
  img: HTMLImageElement;
  frame: Size;
  /** Multiplier over cover-fit; 1 means the image exactly covers the frame. */
  zoom: number;
  ox: number;
  oy: number;
}

const coverScale = (img: HTMLImageElement, frame: Size): number =>
  Math.max(frame.w / img.naturalWidth, frame.h / img.naturalHeight);

function clamp(state: CropState): void {
  const s = coverScale(state.img, state.frame) * state.zoom;
  state.ox = Math.min(0, Math.max(state.frame.w - state.img.naturalWidth * s, state.ox));
  state.oy = Math.min(0, Math.max(state.frame.h - state.img.naturalHeight * s, state.oy));
}

function draw(canvas: HTMLCanvasElement, state: CropState): void {
  const ctx = canvas.getContext("2d")!;
  const s = coverScale(state.img, state.frame) * state.zoom;
  ctx.clearRect(0, 0, state.frame.w, state.frame.h);
  ctx.drawImage(
    state.img,
    state.ox,
    state.oy,
    state.img.naturalWidth * s,
    state.img.naturalHeight * s,
  );
}

/** Downscale `source` into a fresh canvas of `size`. */
function resample(source: HTMLCanvasElement, size: Size): HTMLCanvasElement {
  const c = document.createElement("canvas");
  c.width = size.w;
  c.height = size.h;
  const ctx = c.getContext("2d")!;
  ctx.imageSmoothingQuality = "high";
  ctx.drawImage(source, 0, 0, size.w, size.h);
  return c;
}

/**
 * The relic silhouette: every pixel the icon actually paints, turned white,
 * transparency preserved. That is what the existing *_outline.png files in the
 * tree are, and the game tints the result per relic rarity.
 */
function silhouette(source: HTMLCanvasElement): HTMLCanvasElement {
  const c = document.createElement("canvas");
  c.width = source.width;
  c.height = source.height;
  const ctx = c.getContext("2d")!;
  ctx.drawImage(source, 0, 0);
  const data = ctx.getImageData(0, 0, c.width, c.height);
  for (let i = 0; i < data.data.length; i += 4) {
    data.data[i] = 255;
    data.data[i + 1] = 255;
    data.data[i + 2] = 255;
  }
  ctx.putImageData(data, 0, 0);
  return c;
}

export interface CropCallbacks {
  onSaved: (result: ArtUploadResult) => void;
  onStatus: (text: string, kind?: "info" | "ok" | "error") => void;
}

/** Open the crop dialog for `file`, targeting `slug`. */
export function openCropModal(
  kind: ArtKind,
  slug: string,
  file: File,
  cb: CropCallbacks,
): void {
  const sizes = ART_SIZES[kind];
  const img = new Image();
  img.onerror = () => cb.onStatus(`could not read ${file.name}`, "error");
  img.onload = () => {
    const state: CropState = { img, frame: sizes.big, zoom: 1, ox: 0, oy: 0 };

    const overlay = document.createElement("div");
    overlay.className = "modal-overlay";
    const box = document.createElement("div");
    box.className = "modal";
    overlay.appendChild(box);

    const title = document.createElement("h2");
    title.textContent = `${slug} — ${sizes.big.w}×${sizes.big.h}`;
    box.appendChild(title);

    const canvas = document.createElement("canvas");
    canvas.width = sizes.big.w;
    canvas.height = sizes.big.h;
    canvas.className = "crop-canvas";
    box.appendChild(canvas);

    const hint = document.createElement("p");
    hint.className = "modal-hint";
    hint.textContent =
      kind === "relic"
        ? "drag to pan · scroll to zoom · saves icon, big icon and outline"
        : "drag to pan · scroll to zoom · saves both portrait sizes";
    box.appendChild(hint);

    const row = document.createElement("div");
    row.className = "modal-actions";
    const cancel = document.createElement("button");
    cancel.textContent = "cancel";
    const save = document.createElement("button");
    save.textContent = "save art";
    save.className = "primary";
    row.append(cancel, save);
    box.appendChild(row);

    const redraw = (): void => {
      clamp(state);
      draw(canvas, state);
    };
    redraw();

    let dragging = false;
    let lastX = 0;
    let lastY = 0;
    canvas.addEventListener("pointerdown", (e) => {
      dragging = true;
      lastX = e.clientX;
      lastY = e.clientY;
      canvas.setPointerCapture(e.pointerId);
    });
    canvas.addEventListener("pointermove", (e) => {
      if (!dragging) return;
      // Pointer pixels are CSS pixels; the canvas is drawn at native size.
      const scale = state.frame.w / canvas.getBoundingClientRect().width;
      state.ox += (e.clientX - lastX) * scale;
      state.oy += (e.clientY - lastY) * scale;
      lastX = e.clientX;
      lastY = e.clientY;
      redraw();
    });
    canvas.addEventListener("pointerup", () => {
      dragging = false;
    });
    canvas.addEventListener(
      "wheel",
      (e) => {
        e.preventDefault();
        state.zoom = Math.min(8, Math.max(1, state.zoom * (e.deltaY < 0 ? 1.1 : 1 / 1.1)));
        redraw();
      },
      { passive: false },
    );

    const close = (): void => overlay.remove();
    cancel.addEventListener("click", close);
    overlay.addEventListener("click", (e) => {
      if (e.target === overlay) close();
    });

    save.addEventListener("click", async () => {
      save.disabled = true;
      cb.onStatus("saving art…");
      try {
        const small = resample(canvas, sizes.small);
        const payload: { small: string; big: string; outline?: string } = {
          big: canvas.toDataURL("image/png"),
          small: small.toDataURL("image/png"),
        };
        if (kind === "relic") payload.outline = silhouette(small).toDataURL("image/png");
        const result = await uploadArt(kind, slug, payload);
        close();
        cb.onSaved(result);
      } catch (err) {
        save.disabled = false;
        cb.onStatus((err as Error).message, "error");
      }
    });

    document.body.appendChild(overlay);
  };
  img.src = URL.createObjectURL(file);
}
