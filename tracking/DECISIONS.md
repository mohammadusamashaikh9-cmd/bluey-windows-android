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
