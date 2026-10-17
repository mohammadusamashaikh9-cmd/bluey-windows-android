---
name: bluey-animation-fidelity
description: Translate or review Bluey face and cursor geometry, physics and visual parity using the pinned Swift paths and reproducible frame or recording comparisons.
---

# Animation fidelity workflow

Read [DESIGN-FIDELITY](../../../project/DESIGN-FIDELITY.md), then the relevant
`FaceAnimator`/`FaceView.draw`/`BlobShape` or `TimingCurve`/`CursorEngine`/`Teardrop`
symbols linked there. The source and existing previews have different evidence
roles: source defines math; previews illustrate appearance; neither proves a
target renderer or device is correct.

Extract the selected path operations, transforms, clipping/layer order, sRGB stops,
font/weight, dt clamp and state transitions before translating. Preserve the face's
844 x 390 bottom-aligned cover crop. Preserve desktop y-down logical coordinates
and explicitly map them to platform layers/DPI. Retain crown points, eyelids,
highlights and clipped rim instead of replacing them with generic shapes.

Build future deterministic traces with injected clock and controlled random draws.
Check spring velocity-before-position integration, mood precedence, stale remote
input, blink/hop scheduling and talk smoothing. For flights, invert cubic timing x,
compare derivatives and redirect velocity, and verify one arrival event. A native
spring/easing preset is an approximation until measured against those traces.

For visual review, record source/target commits, device/OS, resolution/aspect/DPI,
fonts/hashes and seed/time trace. Compare fixed-frame geometry masks, color/clip
differences and keyframe recordings for all relevant transitions. Agree numerical
tolerances before accepting differences; inspect timing/interruption as well as
static endpoints. Use [QA](../../../project/QA.md) for device/performance evidence.

Report source match, deliberate differences and target-unverified behavior
separately. If runnable Apple reference or physical target hardware is unavailable,
retain the limitation rather than declaring animation parity. Add no regenerated
artwork or new binary dependencies unless the actual task authorizes them.
