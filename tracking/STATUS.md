# Current verified state

2026-10-20 Asia/Karachi: F4-A implemented on `port/ai-voice-foundation`, based on
verified `origin/main` / `32a38c045e6de87e9d79d44d1114a4f0c76d2b8d`.
Both supplied applications and the evidence below use **`e0dcda03ca2168ada223bb9f01d7fb231a506202`**.
Subsequent closeout edits are canonical documentation/tracking only.
[PR #5](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/5) is merged at `7930efd1bca650ed0358bfc56201950083ea530c` (GitHub verified).
Developer reports F4-A functional physical acceptance; no measured performance or
advanced compatibility data supplied. F4-B is active on `port/live-ai-voice` from
that verified main. D011 records new live consent/credential/cost boundaries.
All F4-B provider tests are offline; no paid call or real key is authorized.

## Accepted F3 gate

GitHub confirms PR #4 squash-merged at the accepted main baseline. Developer reports
physical mTLS pairing, gaze/moods, Android touch precedence, Wi-Fi recovery, revocation,
full Windows exit/reopen, explicit host start and saved authenticated reconnect without
new QR or identity replacement passed. This supersedes R3's pending-retest status.
This is developer-reported functional acceptance, not agent-operated physical QA.
Measured latency/FPS/CPU/memory, multiple-device and advanced compatibility QA remain
UNVERIFIED; F1/F2 detailed visual/performance QA stays separate.

## F4-A implementation and source fidelity

D010: pure monotonic Kotlin six-state controller with injected mock events/IDs;
300 ms hold and double-tap arbitration; generation cancellation on sleep/pause/connection
changes. Touch > local voice > fresh authenticated remote > offline demo. Existing
FaceAnimator receives a data override only; spring math and remote receive/stale path
are retained. C# Bluey.Core and all original Mac/iOS/Shared/previews remain unchanged.

ModeIndicator preserves Swift colors, 0/6/10/16/8/8 border widths, pulse rates/equation,
250 ms explicit cubic ease-in-out and label layout/Fredoka. Android public per-corner
radii/cutouts replace the private iOS lookup; adjustable 55 dp fallback. Native vector
icons preserve intent rather than reproducing SF Symbols. Public circular physical
corners differ from Apple's continuous-corner primitive; glass alignment needs device QA.
Deterministic 600 ms wake and 700/1000/1300 ms deltas/1600 ms completion are mock timing.
Windows reuses existing bubble geometry/pop/fade and source reading time
`clamp(1.2 + words*0.32, 2.8, 12) + 3` seconds. Offline controls require no host/API key.

Only `presentation` is added after authenticated ready, with exact bounded fields,
32-entry per-session queue, 2048-unit caption bound and no offline replay. Existing
mTLS, exact pin, protected trust, approvals, revocation, framing/rate/sequence checks
remain. One active presentation owner per host; multiple-phone presentation is not
accepted QA. Mic/cloud/screen planned states are separately OFF and ungrantable.
No audio recorder, provider client, token endpoint, API key, capture or tool exists.
Optional protected master-key UI is deferred to F4-B; future GA client-secret boundary
is documentation only, with current auth/consent/rate/expiry/revocation requirements.
Official generated OpenAI GA SDK source was inspected; direct web-guide fetches failed.
No live API was called.

## Actual automated evidence

| Target | Observed result at app commit | Downloads |
| --- | --- | --- |
| [Windows CI 39335639475](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39335639475) | Windows-2025/.NET10.0.402; 27 core + 35 link tests passed, zero failures/skips; native WPF build zero warnings/errors; self-contained publish; 16 native checks and 13 offscreen WPF frames | [Windows x64 + full SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39335639475/artifacts/12225683285), [TRX/native/frame evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39335639475/artifacts/12225593494) |
| [Android CI 39335639487](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39335639487) | JDK17/Gradle8.13/SDK36; 70 tests passed, zero failures/errors/skips; lint zero errors/17 warnings; apksigner verified; debug APK; 41 frames (17 retained + 24 border/composite) | [APK + SHA256SUMS + SIGNING-CERT](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39335639487/artifacts/12226463377), [JUnit/lint/frame evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39335639487/artifacts/12226781591) |

Tests cover six states/300 ms boundary/short and double taps/canceled holds, mock
streaming and expiry, stale callbacks after sleep/pause/connection change, flood bounds,
remote target restoration and 2.5 s stale cutoff, exact border math and interrupted
transitions, landscape/corner clamping, actual merged manifest's LAN-only permissions.
Shared 27 F4-A fixtures supplement unchanged 40 F3 fixtures. Actual .NET software-key
TLS tests prove pre-ready presentation denial, trusted ready/delta/face delivery and
revocation cleanup; caption tests cover illegal transitions, bounds, source reading,
late cancellation and owner release. These do not establish physical Keystore/Schannel
or Compose gesture/fidelity acceptance. All existing protocol/animation tests remain.

Local `.NET` core27/link35 tests passed (zero failed/skipped). Initial red presentation
test failed on unknown command before implementation. First Android CI
[39334928523](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39334928523) failed three NewApi lint checks; all cutout
accesses were moved inside the API28 guard. Intermediate
[39335422436](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39335422436) failed Kotlin's unsupported predicate-split
word counter; replaced with an explicit Unicode-whitespace run counter. Final CI passes.
Superseded runs were canceled, not counted as passes. Android emits existing deprecated
NSD-host warnings and path-library strip warning; 17 lint warnings comprise 15 existing
plus two optional Canvas KTX suggestions. Pinned trust-manager lint remains an explicit
D009 policy; no validation was disabled to pass CI.

Downloaded Android APK checksum, embedded public APK-v2 certificate hash, JUnit/lint
XML and 41 PNGs were inspected. Listening/speaking composites were compared visually
with the original listening preview and Swift parameters: vector face retained and
border/label overlaid; different poses/time phases are not pixel-equivalence evidence.
Windows download redirects to `productionresultssa16.blob.core.windows.net`, blocked by
this cloud's policy; archive/TRX/frame local inspection was not possible. Actual CI
logs report the tests/native checks/hashes and upload success. No proxy bypass attempted.

## Published SHA-256 and install continuity

All Windows values below are CI-reported; Android APK/manifest/certificate were also
locally rehashed. Artifact ZIP digests are GitHub metadata and differ from file hashes.

| Item | SHA-256 |
| --- | --- |
| Windows Bluey.Windows.exe | `01aa3f55718116ca61228e21e1f0b7f39ff3808722a835a55a05f87bd671f474` |
| Windows Bluey.Windows.dll | `dfa9f1837413ad31182e4c4ae482e922b5ad652148e6971b9b041f67826153fe` |
| Windows SHA256SUMS.txt | `7b984ee1dc763878ec90b395838ac409dba22f0ddd4e60f0dcac6e533b57b3e9` |
| Windows application ZIP (GitHub) | `815d4430a49bd7fab78069e69d86c957e64ea05b1eba88c83f013fdf42f3d410` |
| Android app-debug.apk | `c0e06477b5562f74fe0e78af452cbfdc5269e145e3097c1663058bd47648e2f1` |
| Android SHA256SUMS.txt | `05200e73d57a74352b39c0ae505e07c404bd9c0b02696ece800dfa759abe4fc6` |
| Android application ZIP (GitHub) | `06fecc14aaca2928ffbc441d2341bd1f19f3afdad9cea92a10da3cb1901c23ce` |
| Android debug public signer | `054f151e74b6035f0a73d2c778250e50197c2057bcd6de327147191b0622f3f4` |

APK signer differs from delivered R3's `2e78aada585a01b285af3dc4baed41a0764da841ab7c468a0d7e8ca74dbd52c1`.
An in-place update over that signer will fail. Uninstall/install intentionally loses
Android data and Keystore identity; explicit re-enrollment is then required. Revoke
obsolete peer entry separately. No code silently deletes/replaces a phone identity.
If the installed signer matches, normal update retains data. Do not mix later PR-merge
artifacts/debug signers with these app-commit downloads. Windows folder is unsigned,
untrimmed self-contained win-x64; no runtime/SDK install. Preserve Windows DPAPI data
and existing executable path/firewall rule. Artifacts expire after14 days.

## Physical QA and one next action

F4-A real gestures, screen corners/cutouts/safe insets, animations, Windows bubble,
sleep/pause/disconnect behavior, FPS/resources/latency remain UNVERIFIED. Public API
and offscreen tests do not prove glass alignment or physical click-through/focus/DPI.

**One next action:** run the [F4-A physical offline demonstration](../project/QA.md#f4-a-physical-offline-demonstration-pending)
with the two matching app-commit downloads: verify manifests/install continuity,
explicitly start host and connect, double-tap wake, hold300 ms/release, observe thinking
→ streamed mock caption/pink speaking → Windows bubble → timed cleanup, then interrupt
with sleep/pause/disconnect and confirm no late wake. Record device/OS/build/time and
observed results; no microphone, provider or capture access is authorized.

## Exact changed files

- [.github/workflows/android.yml](../.github/workflows/android.yml)
- [.github/workflows/windows.yml](../.github/workflows/windows.yml)
- [android/app/build.gradle.kts](../android/app/build.gradle.kts)
- [android/app/src/main/java/dev/bluey/port/MainActivity.kt](../android/app/src/main/java/dev/bluey/port/MainActivity.kt)
- [android/app/src/main/java/dev/bluey/port/animation/FaceAnimator.kt](../android/app/src/main/java/dev/bluey/port/animation/FaceAnimator.kt)
- [android/app/src/main/java/dev/bluey/port/link/DeviceLink.kt](../android/app/src/main/java/dev/bluey/port/link/DeviceLink.kt)
- [android/app/src/main/java/dev/bluey/port/link/PresentationCodec.kt](../android/app/src/main/java/dev/bluey/port/link/PresentationCodec.kt)
- [android/app/src/main/java/dev/bluey/port/link/Wire.kt](../android/app/src/main/java/dev/bluey/port/link/Wire.kt)
- [android/app/src/main/java/dev/bluey/port/voice/ModeIndicator.kt](../android/app/src/main/java/dev/bluey/port/voice/ModeIndicator.kt)
- [android/app/src/main/java/dev/bluey/port/voice/ModeIndicatorRenderer.kt](../android/app/src/main/java/dev/bluey/port/voice/ModeIndicatorRenderer.kt)
- [android/app/src/main/java/dev/bluey/port/voice/VoiceController.kt](../android/app/src/main/java/dev/bluey/port/voice/VoiceController.kt)
- [android/app/src/main/java/dev/bluey/port/voice/VoiceFace.kt](../android/app/src/main/java/dev/bluey/port/voice/VoiceFace.kt)
- [android/app/src/main/java/dev/bluey/port/voice/VoicePreview.kt](../android/app/src/main/java/dev/bluey/port/voice/VoicePreview.kt)
- [android/app/src/test/java/dev/bluey/port/link/ProtocolTest.kt](../android/app/src/test/java/dev/bluey/port/link/ProtocolTest.kt)
- [android/app/src/test/java/dev/bluey/port/voice/IndicatorRenderTest.kt](../android/app/src/test/java/dev/bluey/port/voice/IndicatorRenderTest.kt)
- [android/app/src/test/java/dev/bluey/port/voice/VoiceControllerTest.kt](../android/app/src/test/java/dev/bluey/port/voice/VoiceControllerTest.kt)
- [project/APP-FLOWS.md](../project/APP-FLOWS.md)
- [project/ARCHITECTURE.md](../project/ARCHITECTURE.md)
- [project/DESIGN-FIDELITY.md](../project/DESIGN-FIDELITY.md)
- [project/IMPLEMENTATION.md](../project/IMPLEMENTATION.md)
- [project/PORTING-MAP.md](../project/PORTING-MAP.md)
- [project/PRD.md](../project/PRD.md)
- [project/PROTOCOL-DATA-SCHEMA.md](../project/PROTOCOL-DATA-SCHEMA.md)
- [project/QA.md](../project/QA.md)
- [project/SECURITY.md](../project/SECURITY.md)
- [project/TRD.md](../project/TRD.md)
- [protocol/README.md](../protocol/README.md)
- [protocol/f4a-cases.jsonl](../protocol/f4a-cases.jsonl)
- [tracking/CHECKPOINTS.md](../tracking/CHECKPOINTS.md)
- [tracking/DECISIONS.md](../tracking/DECISIONS.md)
- [tracking/HANDOFF.md](../tracking/HANDOFF.md)
- [tracking/SESSION-LOG.md](../tracking/SESSION-LOG.md)
- [tracking/STATUS.md](../tracking/STATUS.md)
- [windows/Bluey.Link.Tests/Bluey.Link.Tests.csproj](../windows/Bluey.Link.Tests/Bluey.Link.Tests.csproj)
- [windows/Bluey.Link.Tests/PresentationTests.cs](../windows/Bluey.Link.Tests/PresentationTests.cs)
- [windows/Bluey.Link.Tests/ProtocolTests.cs](../windows/Bluey.Link.Tests/ProtocolTests.cs)
- [windows/Bluey.Link.Tests/TlsTests.cs](../windows/Bluey.Link.Tests/TlsTests.cs)
- [windows/Bluey.Link/LinkServer.cs](../windows/Bluey.Link/LinkServer.cs)
- [windows/Bluey.Link/Presentation.cs](../windows/Bluey.Link/Presentation.cs)
- [windows/Bluey.Link/Wire.cs](../windows/Bluey.Link/Wire.cs)
- [windows/Bluey.Windows/ControlsWindow.cs](../windows/Bluey.Windows/ControlsWindow.cs)
- [windows/Bluey.Windows/NativeSmoke.cs](../windows/Bluey.Windows/NativeSmoke.cs)
- [windows/Bluey.Windows/OverlayArt.cs](../windows/Bluey.Windows/OverlayArt.cs)
- [windows/Bluey.Windows/Program.cs](../windows/Bluey.Windows/Program.cs)
- [windows/Bluey.Windows/VoicePresentation.cs](../windows/Bluey.Windows/VoicePresentation.cs)
- [windows/README.md](../windows/README.md)
