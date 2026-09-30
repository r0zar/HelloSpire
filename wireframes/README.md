# Wireframes

Structural prototypes for each character's stance and movement — self-contained HTML, open
any file directly in a browser. Same spirit as [`../concept-art/`](../concept-art/README.md):
a design reference, not a game asset.

| File | Character |
|---|---|
| `paladin.html` | grounded, wide-split, almost-locked-knee stance, shield held out front; slow heavy breathing sway with a periodic shield-arm "readiness" flex |
| `gunslinger.html` | contrapposto, weight on the back foot, gun hand (the back hand) cocked at the hip over the holster, about to draw; body nearly still except a fast tremor in the gun hand and a slow coat-tail sway |
| `alchemist.html` | hunched forward, bent knees, oversized head; constant small motion — vial hand rotating, head tilting, satchel bouncing |

Each page renders an animated SVG stick figure — an 18-bone humanoid skeleton (spine chain,
both arms, both legs, plus stub bones for a back-mounted item and a belt) built from a small
per-character data block (`RIG` in the page's own `<script>`) that a shared, hand-rolled 2D
forward-kinematics solver walks every frame. Three things are defined per character:

- **Stance** — each bone's rest-pose rotation. This *is* the posture: how wide the feet are
  planted, how bent the knees are, where the hands sit. Click "Pause (view stance)" on any
  page to freeze the animation and inspect it exactly (or open it with `#stance` on the URL).
  Every character **faces right**: in combat the player's side stands on the left of the
  screen facing the enemies, so each rig is posed in profile toward the right edge.
- **Movement** — a sine-wave oscillation layered on top of the stance per bone (amplitude,
  frequency, phase), so each character idles with a gait that's actually theirs rather than
  a shared default loop. Movement is meant to read as an extension of the character's
  established identity (see each page's "Movement" note and the linked `design/*.md`), not
  as a separate animation decision.
- **Component map** — ten labeled attachment points (head, chest, back, belt, both
  shoulders, both hands, both feet), each following the bone chain during animation and each
  annotated with what art belongs there and what it should look like. This is the "grand
  vision" layer: a written target for whoever illustrates or rigs the real part next.

## Art attachment

Every character is skinned with painted, transparent cut-out parts — a full, detailed figure
at any frame, with the stick figure hidden underneath (**"Hide art overlay"** brings it back).
The parts live in `assets/<character>/` and come from
`../image_gen_pipeline/generate_wireframe_parts.py`, which paints each piece with the OpenAI
Images API, steered by that character's reference art (`art_reference/<character>/`, or the
character-select art for the Gunslinger), and trims it to its silhouette:

| Piece | How it's drawn | How the page places it |
|---|---|---|
| `thigh`, `shin`, `upper_arm`, `forearm`, `hand` | upright, joint end at the top | stretched along its bone (`fit:'bone'`, `start:'top'`) |
| `chest`, `abdomen` | upright, joint end at the bottom | stretched along `chest` / `spine` (`start:'bottom'`) |
| `head`, `foot`, `back`, `belt`, props | side view facing right | pinned to a bone point, upright or `align`ed |

Near and far limbs share one image; the far side is shaded darker. Re-painting a piece is
`python3 generate_wireframe_parts.py <character> --only <part> --force` (masters are cached
in `wireframe_masters/`, so re-running without `--force` costs nothing).

There are two kinds of placement. **Segments** — the torso and limbs, listed in a rig's
`skin` array — stretch along their bone:

```js
{ bone:'r_thigh', src:'assets/gunslinger/thigh.png', fit:'bone', start:'top', pad:[0.2, 0.2], scale:0.8 }
```

The image's joint end sits on the bone's start; `pad:[before, after]` extends it past both
joints (fractions of the bone's length) so neighbouring segments overlap and no gap opens at an
elbow or knee; the width follows the image's own proportions, times `scale`.

Overlap alone leaves each part's inked cut end showing, so joints are finished with draw order
and feathering: the part that belongs on top is drawn on top (boot over trouser leg, coat over
the thighs), and `fadeTop:[from,to]` / `fadeBottom:[from,to]` (fractions of the image's height)
melt the end that tucks underneath. Fade only an end that some opaque neighbour covers — a fade
over bare background reads as a murky patch. Where a generated shin already carries a whole boot
(Gunslinger, Alchemist) it *is* the foot; the separate foot image is dropped rather than doubled,
and the Alchemist's bent knees get a second trouser piece on the shin bone so the leg follows the
bend down into the boot.

**Pinned pieces** — the component-map slots' `art` — sit at a point on a bone:

```js
art: { src:'assets/gunslinger/head.png', bone:'neck', t:0.15, w:74, anchor:{x:0.56,y:0.9} }
```

`w` sets the size (height follows the image), `anchor` is the point of the image (0–1 fractions)
that sits on the bone at `t`. The page solves the rest pose once at load and cancels each bone's
rest-world-angle, so a pinned piece stands exactly upright at stance and only turns by however
much the rig moves from there. Optional fields:

- `tilt` — a fixed extra rotation in degrees (the Alchemist's head is tipped down 12°).
- `align:true` — lay the art's +x axis along the bone instead of standing it upright, for
  weapons that should point where the hand points (the revolver, the mace).
- `mirror:true` — flip the art horizontally, for a painted part that came out facing left.
- `behind:true` — tuck the piece under everything else in its layer (boots under shins).
- `shapes:[...]` instead of `src` — a procedural vector icon (`tag` + `attrs` SVG descriptors,
  with `'color'`/`'accent'`/`'base'` resolving against the rig's palette), for sketching a slot
  before it has painted art.

A slot can also carry a `moveArt` block that replaces `art` (or appears on its own) while Attack
or Self buff plays — the Gunslinger's revolver sits in the holster at idle, and during a move
the holster shows empty and the revolver appears in his hand.

**Depth.** Parts are drawn in four layers, back to front: `back` (the cape/coat/satchel, hung
from a short stub at the top of the back), `far` (the limbs on the side away from the viewer,
shaded darker), `body` (torso, head, belt), `near`. Bones and art default to their layer by name
(`l_*` far, `r_*` near, everything else body); a `layer` field overrides it — the Gunslinger's
draw arm is on the near layer so his cocked hand reads over the hip.

**"Show attachment markers"** overlays the component-map points and a frame around every part,
for calibrating placement.

## Movement: Idle, Attack, Self buff

Beyond the continuous idle gait, each character now has two triggered, one-shot moves —
**Attack** and **Self buff** — playable from buttons above the rig, so you can actually watch
how each character's stance and material read carry into their other animations, not just
standing still. A move is a short list of keyframe poses (bone → angle delta from stance) at
fractional times through its duration, interpolated with smoothstep easing:

```js
attack: {
  duration: 0.9,
  keyframes: [
    { at:0.0,  pose:{} },
    { at:0.25, pose:{ r_upper_arm:-35, r_forearm:-25, chest:-6, spine:-4, l_forearm:10 } },
    { at:0.5,  pose:{ r_upper_arm:55,  r_forearm:15,  chest:10, spine:6,  l_forearm:-5 } },
    { at:1.0,  pose:{} },
  ],
  effect: { bone:'r_hand', t:1, type:'flash', peakAt:0.5, width:0.12, color:'#f3d68a' },
}
```

Only the bones a move actually poses stop taking their idle sine sway while it plays — the legs
keep breathing through the Paladin's mace swing, for instance — and every move auto-returns to
idle when it finishes (or immediately, via the **Idle / gait** button, which also cancels a move
early). An optional `effect` drives a small glow/flash ring at a bone, for a bit of combat
feedback (a muzzle flash, a sigil pulse) without drawing a full particle system.

The three characters' Attack/Buff read distinctly on purpose, extending the same traits their
stance and idle gait already establish — see each page's "Movement" section for the specifics
(mace swing vs. a half-second gunshot vs. a loose, off-balance flask toss; a presented shield vs.
a showman's cylinder spin vs. a held-up, fizzing vial).

**What this does and doesn't fix.** All of this answers "can art be mapped onto the wireframe and
previewed through multiple movements" — yes, for all 30 slots and all three motion states now.
It does **not** touch the re-integration gap described above: the bone names, coordinate scheme,
and this attachment/move format are still local to this HTML/SVG tool, not Spine's slot/
attachment model, `edits.json`'s schema, or its animation format. Everything validated here still
has to be manually re-authored inside the real pipeline once someone has access to it.

## Why HTML and not a Godot scene or a Spine skeleton

This mod's actual combat rigs are Spine skeletons, and [`../spine/README.md`](../spine/README.md)
is explicit that `spine/<character>/` is **generated, do-not-hand-edit** build output — the
only source of truth for stance, posture, proportions, and hidden parts is
`skeletons/<character>/edits.json` inside the external
[`sts2-reskin-pipeline`](https://github.com/r0zar/sts2-reskin-pipeline) workbench, which isn't
checked into this repo. There's also no in-repo Godot scene using `Skeleton2D`/`Bone2D` today —
the game's own animation is loaded programmatically at runtime (see
`HelloSpireCode/Characters/CharacterSkeletons.cs`).

So a wireframe that's both hand-editable *in this repo* and immediately viewable without the
base game, the Spine editor, or the external workbench has to be its own thing. HTML+SVG was
the natural fit — it's the format `concept-art/` already uses for exactly this kind of
pre-production reference, it needs no build step or network access (no webfonts, unlike
`concept-art/*.html` — this is meant to be poked at and iterated on, not just read), and the
"component map" slots are just labeled points on a bone chain, which is a small step from
being literal attachment points once a rig is authored for real.

## Where this goes next

Nothing here writes to `spine/` or touches the mod's runtime rendering — it's a planning
layer that sits upstream of the workbench. The intended handoff, once real character art
exists: the stance numbers and slot list here become the starting values for that
character's `edits.json`, and the slot vision notes become the brief for `art_in/<character>/`.
Until then, these pages are the place to iterate on proportions, posture, and gait before
spending Spine-editor time on any of it.
