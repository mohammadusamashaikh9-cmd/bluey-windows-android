# Design and animation fidelity

Authority is the pinned Swift source, not a redrawn mockup. Everything in the
source table is source-observed, not a runtime comparison. See [QA](QA.md) for
the planned comparison procedure and [source map](PORTING-MAP.md) for owners.

## Assets and color

| Authority | Exact content / rule |
| --- | --- |
| [Palette.swift](../Shared/Palette.swift) | sRGB berry1 `#A9BCFF`, berry2 `#6C86F5`, berry3 `#4254D6`, berry4 `#2B2F8F`; stops `[0, 0.34, 0.68, 1]` |
| Same palette | nose `#1C1F66`, ink `#17151F`, inkSoft `#B9B2CC`, panel `#1E1B29` |
| [BlobShape.swift](../iOS/Sources/BlobShape.swift) | Gradient endpoints `(0.25w,0.067h)` -> `(0.75w,0.933h)`; do not substitute a renderer's angle convention |
| [FaceView.swift](../iOS/Sources/FaceView.swift) | Cheeks `#F3A6D8`, tongue `#E58BC4`; preserve opacity, clipping and layered highlights |
| [Fredoka](../Shared/Fonts/Fredoka.ttf) | Rounded display face; preserve [OFL notice](../Shared/Fonts/OFL-Fredoka.txt) |
| [Plex Sans](../Shared/Fonts/IBMPlexSans.ttf), [Mono regular](../Shared/Fonts/IBMPlexMono-Regular.ttf), [Mono medium](../Shared/Fonts/IBMPlexMono-Medium.ttf) | Preserve [Plex OFL notice](../Shared/Fonts/OFL-IBMPlex.txt); verify family/weight resolution on each target |
| [Fonts.swift](../Mac/Sources/GooglyMac/Fonts.swift) and BlobShape font helpers | Apple rounded/system fallbacks are source behavior, not portable font equivalents |
| [Face reference](../docs/previews/expressions.png), [listening](../docs/previews/listening.png), [happy](../docs/previews/happy.png), [sleepy](../docs/previews/sleepy.png), [pairing](../docs/previews/pairing.png) | Existing upstream previews, not new target captures |
| [Cursor](../docs/previews/cursor.png), [flight](../docs/previews/cursor-flying.png), [comet](../docs/previews/pointer-comet.gif), [string](../docs/previews/pointer-string.gif) | Existing visual references; a GIF alone does not specify timing/physics |

No new binary assets are needed in F0. Do not replace vectors with screenshots,
emoji, generic blobs or independently guessed SVGs. Keep font redistribution
notices even though repository code is under [Unlicense](../LICENSE).

## Face geometry and integration

`FaceView.draw` uses an 844 x 390 design canvas, black background, bottom alignment
and **max** scale (cover/crop, not fit). Body rect is `(12,44+bounce,820,700)`;
`bounce = -talk*16 + breathe*3 - hop`. Rotation pivot is body bottom center;
stretch is `1 + clamp(hop/900,-0.03,0.03)`, with x scale `2-stretch`.
Eyes center at body offsets `(280,157)` and `(540,157)`, whites 190 across,
base pupil radius 46, reach 50. Preserve gaze-vector normalization and eyelid
clipping, catchlights, crown, brows and mouth ordering from the drawing function.

Blob corner radii TL/TR/BR/BL are `(0.52w,0.58h)`, `(0.48w,0.56h)`,
`(0.46w,0.44h)`, `(0.54w,0.42h)`; quarter-ellipse cubic constant is `0.5523`.
Crown source points in a 60 x 40 box are `(30,4),(36,16),(52,12),(40,24),
(46,36),(30,28),(14,36),(20,24),(8,12),(24,16)`, closed. Translate path
operations and transforms literally before optimizing.

Spring integration is semi-implicit Euler: `c=2*sqrt(k)*damping`,
`velocity += (k*(target-value)-c*velocity)*dt`, then `value += velocity*dt`.
Phone `dt=min(elapsed,1/20)`; initial dt is zero. These damping numbers are ratios,
not directly interchangeable with framework spring parameters.

| Channel | Stiffness | Damping |
| --- | --- | --- |
| gaze x/y | 260 | 0.82 |
| pupil | 160 | 0.5 |
| brow lift / tilt | 180 / 150 | 0.55 / 0.6 |
| lean / hop | 60 / 260 | 0.8 / 0.35 |
| blush / squint | 40 / 150 | 1 / 0.7 |

Mood targets and special drawing branches stay in `FaceAnimator.step` and
`FaceView.draw`: listening, resting, thinking, talking, pointing, happy, sleepy.
Local mood overrides remote; live means last packet <2.5 s. Mood changes blink
and, except sleepy/resting, add hop velocity 260 and pupil velocity 3. Talk
smoothing uses `min(1,dt*25)`; lean targets `-gazeX*0.045`. Normal blink is
`sin(pi*p)` over 0.15 s; sleepy over 0.7 s; double blink probability is 0.25 with
second phase 1.3–2.3. Normal blink interval is 2–5 s, sleepy 1.6–3 s.
Idle wander/jitter/hops use randomness; future parity tests must supply identical
draws and clocks rather than recording two uncontrolled runs.

## Desktop cursor

[CursorOverlay.swift](../Mac/Sources/GooglyMac/CursorOverlay.swift) owns
`TimingCurve`, `Flight`, `CursorEngine`, `Teardrop`, layers, effects and captions.
Engine coordinates are top-left/y-down points; layer coordinates are y-up.
Translate this conversion explicitly into Windows logical/physical coordinates.

The teardrop is a square with three radius `size/2` corners and a small tip
radius `6*(size/96)`. Default setting is 72 points (menu range 48–120).
The clipped white stroke is `8*(size/96)`, leaving half visible; shine is radial
at `(0.3s,0.24s)` with radius `0.38s`, white alpha 0.55 to zero. Preserve glow
and nonglow shadow branches, cursor eyes, comet/string/none trails and clipping.

Flight duration is `clamp(0.4+0.12*log2(1+distance/24),0.45,1.2)` seconds;
drag multiplies by 1.45 and clamps to 0.6–1.6. Cubic timing curves are launch
`(0.42,0,0.22,1)`, redirect `(0.25,0.25,0.22,1)`, steady `(0.42,0,0.38,1)`.
Solve the timing curve's x parameter; using its raw y polynomial is incorrect.
Preserve velocity carry on redirect, curved control points, arrival-once callback,
press squish (0.42 s), hover, fade and caption layout. Desktop dt clamps to
1/480–1/20 after an initial 1/60 step. Follow/dock/pin are distinct states.

## Planned parity evidence

Capture identical fixed states and transition traces at declared resolution,
DPI, font hashes, clock and RNG sequence. Compare geometry masks, gradient stops,
occlusion, cropped edges, keyframe timing and interrupted trajectories. Record
renderer/OS differences separately; propose numeric tolerances before acceptance.
Use the existing previews as references and obtain a runnable Apple reference
recording when available. No Apple runtime or target screenshots were produced
in F0. Device refresh rate, reduced motion and thermal/battery behavior remain
unverified until physical-device measurements.

## Android F1 translation

[FaceAnimator.kt](../android/app/src/main/java/dev/bluey/port/animation/FaceAnimator.kt)
translates Spring and FaceAnimator, preserving update order, every mood target,
random scheduling and precedence. Clock and unit-random inputs are injectable;
the demo uses monotonic active time and pauses in background. Invalid/backwards
time is rejected rather than integrated. No desktop-idle mood policy is invented.

[FaceGeometry.kt](../android/app/src/main/java/dev/bluey/port/animation/FaceGeometry.kt)
owns palette, crown points and cover transform.
[FaceRenderer.kt](../android/app/src/main/java/dev/bluey/port/render/FaceRenderer.kt)
translates BlobShape and FaceView.draw through Compose Canvas's native canvas.
All artwork is drawn from paths and math; only the static vector-derived blur
masks are cached (about 4.4 MB) to avoid unsupported GPU mask filters. No external
art, screenshots, GIFs or animation libraries are used as the character.
Bundled original Fredoka draws the z glyphs; all original font/OFL files are assets.

Android versus Swift blur kernel, antialiasing and text metrics are calibration
items, not asserted pixel identity. The upstream PNGs lack deterministic capture
inputs; compare composition/geometry qualitatively, and compare the original path
and animator mathematics structurally. CI renders fixed frames and a happy-mood
transition with the same renderer, without an emulator. GPU pacing, actual font
appearance, touch/lifecycle/fullscreen and visual fidelity remain physical-device
checks. Hide the secondary demo panel before recording comparisons.
