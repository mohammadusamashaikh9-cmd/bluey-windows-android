# Decision log

Date basis: Asia/Karachi. Accepted planning decisions do not imply implemented
behavior. Add dated entries for material changes; preserve earlier decisions.

| ID / date | State | Decision, rationale, alternatives and consequence |
| --- | --- | --- |
| D001 / 2026-10-18 | Accepted | Preserve pinned Apple source and existing assets; source-based unofficial port with Riley attribution. Alternative: rewrite original directories or reverse-engineer binaries. Separate target code will require later authorization; no binary analysis needed. |
| D002 / 2026-10-18 | Accepted | No required hosted backend/cloud database in V1. Local settings/sessions/trust plus LAN protocol and direct OpenAI boundary match source ownership. Alternative: hosted relay/accounts/database adds operational and privacy scope without demonstrated need. Revisit only for a specific future requirement. |
| D003 / 2026-10-18 | Proposed | C#/.NET Windows and Kotlin/Compose Android; WPF/WinUI/renderer, SDK/OS/CPU support remain undecided. Alternatives include other native rendering stacks. F1/F2 spikes must prove fidelity, transparency/lifecycle and maintenance costs before acceptance. No packages/toolchains selected in F0. |
| D004 / 2026-10-18 | Accepted requirement; profile proposed | Authenticated encrypted device pairing before sensitive traffic, no plaintext downgrade; new version/service boundary distinct from legacy v0. Alternative: carry raw `_googly._tcp` trust forward. That exposes token/capture/action authority. Exact TLS/bootstrap/envelope and discovery identity await F3 review. |
| D005 / 2026-10-18 | Accepted | V1 is F1–F4; F5 control/notes optional, web research deferred. Separate mic/cloud/screen/recording/control opt-ins; computer control off by default, app-enforced consequential confirmation and latched stop if F5 selected. Alternative: reproduce upstream defaults/prompt-only rules; rejected for port trust boundary. |
| D006 / 2026-10-18 | Accepted | One canonical root/project/tracking tree and exactly four `.agents/skills` workflows. F0 is documentation/fixture validation only. Alternative: parallel planning kit tree/tool installations. No archive was present; future imported references must merge into this tree. |

## Open decisions for implementation

F1: Android minimum/target SDK, devices and renderer/font proof. F2: Windows
framework/renderer, supported OS/CPU and packaging. F3: TLS/bootstrap profile,
service/envelope version, limits/media strategy and trust recovery. F4: current
API/account/model/cost/retention and capture-consent UX; explicit storage migration
version/units. F5: whether either optional feature is selected and retention budget.
These are bounded gate inputs, not blockers to the documentation foundation.

## D007 / 2026-10-18 — F1 Android implementation choices

Accepted for the user-authorized F1 slice: Kotlin + Compose Canvas under `android/`,
pure Kotlin animator and Android Canvas drawing inside Compose. Use AGP 8.13.2,
Gradle 8.13, Kotlin/Compose compiler 2.2.21, Compose BOM 2025.12.01, JDK 17,
compile/target SDK 36 and minimum SDK 26. Stable pinned versions rather than a
preview/current-major migration minimize setup risk. AGP's [compatibility table](https://developer.android.com/build/releases/agp-8-13-0-release-notes)
specifies Gradle 8.13/JDK 17 and supports API 36; [Compose BOM](https://developer.android.com/develop/ui/compose/bom)
and [Kotlin Gradle compatibility](https://kotlinlang.org/docs/gradle-configure-project.html)
were checked before selection. Exact dependency resolution still needs an actual build.

No local Java/Gradle/Android SDK was found on PATH. CI installs SDK/build tooling;
the laptop needs neither Studio nor emulator. CI uses setup-gradle's pinned
distribution rather than adding a wrapper JAR/bootstrap script. Optional local
builds must use the documented Gradle version. Robolectric 4.16.1 is test-only
for offscreen Android-native bitmap comparisons on SDK 35/JDK 17, not an emulator
or a substitute for physical-device acceptance. Runtime libraries are limited to
Compose/activity/lifecycle. Reuse `Shared/Fonts` as assets with original notices.

Preserve spring/state math literally. A monotonic active-time demo clock pauses
in background; reject backwards/nonfinite input at the pure animator boundary.
The standalone controls feed in-memory FaceState (no networking). Cached software
blur masks let hardware Canvas draw the source glow/cheeks without relying on
unsupported GPU mask filters; blur kernel/antialiasing/text metrics need visual
calibration. Android is now selected for F1; Windows choices remain proposed.
Allowed changes: `android/`, `.github/workflows/android.yml`, additive README and
relevant canonical project/tracking docs. Original Apple source/assets stay read-only.

## D008 / 2026-10-18 — F2 native Windows overlay

Accepted by the F2 request: C#/.NET 10, WPF and Win32 on Windows 11 x64,
under `windows/`, based on main merge `15194dec8356385b15dc6712630d45e4761cd1de`.
Pin SDK 10.0.402 (official .NET 10 release metadata). A pure `Bluey.Core` library
owns timing, flight, state, effects and vector/layout geometry; `Bluey.Windows`
uses WPF retained drawings and a transparent HWND. No second UI framework,
renderer package or application dependency is needed. Win32 provides overlay
styles, monitor/DPI, cursor reads and hotkeys. A regular WPF control window is
always reachable independently of the hidden/click-through overlay.

WPF is the smallest maintainable starting point: built-in paths/gradients/text,
transparent topmost windows and direct HWND interoperability. WinUI would add
packaging/composition complexity before an observed WPF blocker. Layered-window
GPU behavior, blur kernels, antialiasing, font metrics, fullscreen interaction,
click-through and 60 Hz performance are target-unverified until laptop testing.
Primary monitor only: engine units are DIPs, y down; Win32 physical screen points
are explicitly mapped through monitor origin and per-monitor DPI. No mixed-DPI
multi-monitor support claim. Manifest requests PerMonitorV2.

CI uses Windows and .NET 10; publish a self-contained, untrimmed win-x64 folder
with SHA-256 sums (no installer/signing/release). The laptop only needs to extract
and run the development EXE; no SDK, IDE, Docker or WSL. Retained assets are linked
from original Shared fonts with OFL notices. Pure tests execute in Linux cloud
and Windows CI; native window checks in CI are distinct from physical desktop QA.

Deliberate adapter differences: y-down WPF needs no AppKit layer y flip; Ctrl+Alt
shortcuts replace Control+Option, with conflict feedback. Demo settings are local
nonsecret versioned JSON. Effects remain visual only, with no input injection.
Source redirect carry is capped at 0.6*distance; exact speed continuity therefore
holds only below that cap, while direction is retained above it. No networking,
audio, APIs, capture, OCR or computer actions enter F2. Original Apple and Android
files remain unchanged. Allowed changes: `windows/`, `.github/workflows/windows.yml`,
additive README and relevant canonical project/tracking guidance.
