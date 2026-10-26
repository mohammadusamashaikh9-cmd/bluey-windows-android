# Current F4-C Windows10 build19045 adaptation — PR #7

2026-10-26 Asia/Karachi. Clean start e5abe8a863bae5f100f9bb4ec6ff9e07a81dcc06,
port/screen-understanding; PR7 still targets port/live-ai-voice/PR6 at4b38029.
Both remain unmerged. Developer confirms actual OS: Windows10 Pro Education22H2,
10.0.19045. Earlier accepted63d7269 capture/preview/125%DPI pointing/Home/discard ran
on this laptop; prior Windows11 assumptions do not establish a second OS test.

Confirmed defect: sparse MSIX MinVersion22000 rejects19045. Set minimum19041 and
explicit SupportedOSPlatformVersion19041; keep SDK/TFM22000. Warnings-as-errors/
CA1416 audit direct managed/WinRT calls; Microsoft-documented native/API minima
fit19041. No WGC/JPEG/OCR engine/target/animation/F3/key changes. Portable fallback
and profile/identity fail-closed guards retained; no OS upgrade or migration.

Registration now preflights OS version and actual Register/ExternalLocation command
support. Tests distinguish19040 rejection and19041/19045/Win11 acceptance. Runtime
OCR metadata exposes actual OS, compiled minimum and required API presence. This
is diagnostic evidence, not simulated19045 runtime success. Server2025 CI cannot
prove exact Windows10 sparse activation, installed OCR language or word geometry.

Loose developer registration is a documented Windows10 API distinct from Win11-only
unsigned-MSIX installation; no AllowUnsigned flag used. Exact19045 activation remains
unverified. README includes a separately approved manual signed procedure if trust/
activation fails. No automatic certificate creation/trust/signing, package install,
Developer Mode/policy mutation, privilege fallback or new OCR library. .NET10's
maintained OS matrix excludes this Windows edition; target compatibility is not a
vendor lifecycle claim. Existing application already ran on the developer machine.

Manifest regression first failed2of3cases, exposing both the22000 minimum and
missing explicit managed floor. Local corrected contracts3pass and OCR WPF build
passes0warnings/errors. Full Windows/native/package and Android CI follow source
push. Physical19045 registration/native self-test/Notepad OCR remains UNVERIFIED.

One next action: review the new bundle and run its one explicit19045 registration,
public-text self-test and harmless Notepad OCR/word pointing/Home/Stop/expiry
sequence with cloud OFF. Stop on errors; no automatic signing/trust or provider call.

# Previous F4-C OCR developer identity — retained evidence

2026-10-26 Asia/Karachi. Started clean atff6115282a1d0505bf4c0455ce075a7a4e415980
on port/screen-understanding, stacked on unchanged port/live-ai-voice/PR6 at4b38029.
Both PRs stay unmerged. Developer accepts actual63d7269 Notepad WGC/JPEG/preview,
VisibleBounds125%DPI,500/500 visual pointing with stationary OS mouse, Home/discard
(three real captures). That gate is retained; no NativeCapture/JPEG repair reopened.

D014 selects an optional unsigned sparse/external identity developer build to
unlock existing Windows.Media.Ocr. Portable ZIP/app.manifest stay the fallback.
Only matching identity metadata, win32App/mediumIL,runFullTrust/unvirtualizedResources
are added. Startup refuses wrong/missing identity or redirected LocalAppData before
trust/key services. No protected data paths, pairing identities, keys, F3/provider,
geometry or animation code are changed. No data migration or installer.

Bundle includes self-contained app, unsigned identity-only MSIX/loose manifest,
public assets, checksums and explicit approval/WhatIf registration/removal helpers.
No certificate creation/import/trust, Developer Mode/policy modification, auto-
registration, all-users provisioning, app start or language download. Developer
must separately review/enable Developer Mode and approve per-user registration.
CI builds and checks these boundaries without registering any package. Exact
four-line real-engine self-test is available only after explicit local registration;
CI unregistered mode must report0 recognized words/lines, never fake success.

Manifest contracts first failed2cases before implementation and now pass.
Pending-capture Stop uses controlled completion and no timing poll; pending-OCR
Stop also rejects late targets/preview/sharing. Final actual evidence follows.

Tested application/bundle revision **6da41321de73c18b7fafe0889ab6ee05a0530d22**.
Later closeout changes tracking only; use the artifacts explicitly tied to6da4132.

| Evidence | Actual result |
| --- | --- |
| Local .NET10.0.402 |239 C# tests (Core27/Screen71/Link141),2 manifest contracts pass; both WPF profiles compile0warnings/errors |
| [Windows40231453791](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40231453791) |239unit,2Python identity contracts,47native checks,14built-package/approval checks pass; both self-contained variants publish |
| [Android40231454103](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40231454103) |111tests,0failure/error/skip; lint0errors18retained warnings; debug signature verified |
| Stacked PR checks |Windows40231458418 and Android40231458451 pass at6da4132 |
| Prior failure |Windows40230706897/41ec1c8 built both variants, then failed Windows PowerShell5.1 WhatIf/Get-FileHash;19bd1d3 corrected read-only .NET hashing and both19bd/final6da CI pass |
| Static |18 canonical/Windows Markdown files,220 local links/anchors and3 JSONL fixture collections pass; protected-path diff and git diff --check pass |

Artifacts retain14days:

- [OCR developer bundle: app, unsigned sparse MSIX/loose manifest, approval helpers, README and SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40231453791/artifacts/12612952571)
- [Portable Windows fallback + SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40231453791/artifacts/12613710856)
- [Windows TRX/native/PE-manifest/package evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40231453791/artifacts/12612842983)
- [Optional unchanged Android APK/manifest/signer](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40231454103/artifacts/12613217384) and [test/lint evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40231454103/artifacts/12612798287)

| File | CI SHA-256 |
| --- | --- |
| OCR bundle ZIP upload digest |`fa63ba8d31e4ca3f2832b4ebe210acdd67e8e3f2dda1712b7aef767c224c7ba6` |
| OCR app/Bluey.Windows.exe |`040ff33d162d41e4cda7f879bc274ebca9a777b2b4bd68bfbc234d22c0dba94a` |
| OCR app/Bluey.Windows.dll |`6542b06f7c57dcd24b27cf13899edf83a70f0ac6d9a562a7bac7a5c788a0c59d` |
| Bluey.Ocr.Dev.msix (unsigned identity-only) |`c8a3bc060b00f65accccd33e7ed7047e03176ea880d2e69f2d727d0fb0e00eeb` |
| OCR SHA256SUMS.txt |`7e0a7b456500b3496482250de76bc87255bd25e4a17d5f08f7ff29cac29bbd71` |
| Portable ZIP upload digest |`3bd77bc35f3c26604ea837ac43bcb11a4fbb8b31c5727e8e87079e782fea1946` |
| Portable Bluey.Windows.exe |`37a26bcc637adf9c8a227e7ee3bf0a5bf0b2af92eae12a0adb188f25d1ac3e06` |
| Portable Bluey.Windows.dll |`92d6716c0112744d4794af1d3b5235daa0c2d436d066701608437d4e533d96df` |
| Portable SHA256SUMS.txt |`9bba7718a4e4b1a55185b1237ba5cf4a04b04a27aa6047327587b3998a380138` |
| Optional app-debug.apk |`82e97b2a7115216c2d4b273226511f0469f6517cecfb1106173c1d2983a6a668` |

All bundle entries verified on the Windows runner. Native SDK mt.exe extracted
actual portable/OCR PE manifests and confirmed different matching profiles; MakeAppx
produced an unsigned identity/assets-only package. Approval-absent registration/
removal fail; Windows PowerShell5.1 WhatIf validates hashes and leaves package
count0. No package-signing certificate creation/import/trust or registration occurred. OCR executable
without identity exits3/UnavailableIdentity with0 lines/words. Thus registered
identity/native OCR has NOT been demonstrated in CI; no synthetic annotation pass.

The explicit --ocr-self-test, only after developer registration, uses real
NativeAnalysis on generated public four-line pixels and checks exact L1-L4/W1-W9,
word reading order/bounds, existing pure flight, Home, Stop, expiry and no unapproved
sharing. It was compiled, not run with identity here. Real Notepad OCR, installed
language, word positions/pointing and registered-profile data-path behavior remain
UNVERIFIED until the developer approves registration and tests on the laptop.
Visible Notepad chrome may precede the document's L IDs; never force/fabricate IDs.

Attempts to download final app/portable/evidence archives were policy-blocked at
productionresultssa14 (superseded19bd artifacts atsa13). Hashes/content evidence are
actual CI/API results, not independent local-byte rehashes. No proxy bypass or
network-policy change. CI retains the existing setup-android Node20/forced24
annotation and18lint warnings; no final failing checks. No provider request/key,
private capture/text, input injection, alternate OCR/NPU engine or new PR.

Android unchanged: keep the installed APK for this Windows-only offline test.
Optional fresh CI signer6b8ed14ac3dc2c722de58af0e52da5197546f481f128377c31cf82fbd0c92a2c
may differ from the installed app; reinstall would lose Android Keystore identity
and require explicit re-pair/revocation. Do not replace it unnecessarily. Existing
Windows DPAPI/Link/AI/settings locations, original Apple/Android source, accepted
WGC/JPEG, NativeAnalysis/Targets, F3/provider and animation files are unchanged.

One next action: review the OCR developer bundle and, only with developer approval,
follow its explicit registration and one harmless Notepad OCR/point/Home/Stop test.

Exact28 paths changed against ff6115282a1d0505bf4c0455ce075a7a4e415980:

```text
.github/workflows/windows.yml
project/ARCHITECTURE.md
project/IMPLEMENTATION.md
project/PORTING-MAP.md
project/QA.md
project/SECURITY.md
tracking/CHECKPOINTS.md
tracking/DECISIONS.md
tracking/HANDOFF.md
tracking/SESSION-LOG.md
tracking/STATUS.md
windows/Bluey.Screen.Tests/WorkflowTests.cs
windows/Bluey.Windows/Bluey.Windows.csproj
windows/Bluey.Windows/NativeSmoke.cs
windows/Bluey.Windows/Program.cs
windows/Bluey.Windows/Screen/OcrIdentity.cs
windows/Bluey.Windows/Screen/OcrSmoke.cs
windows/Bluey.Windows/Screen/ScreenCoordinator.cs
windows/Bluey.Windows/app.ocr.manifest
windows/README.md
windows/packaging/Build-OcrPackage.ps1
windows/packaging/OcrPackage.ps1
windows/packaging/README.md
windows/packaging/Register-OcrIdentity.ps1
windows/packaging/Test-OcrPackage.ps1
windows/packaging/Unregister-OcrIdentity.ps1
windows/packaging/identity/AppxManifest.xml
windows/packaging/test_package.py
```

# Accepted F4-C capture repair — retained physical evidence

2026-10-26 Asia/Karachi. Existing PR #7 remains open, unmerged and stacked on
`port/live-ai-voice`; PR #6 stays unchanged at4b380294e883e669d29000cd844962db88439e58.
Tested application/CI revision **63d7269fac56dd8a02ce7d6a6a1caf427a83ee02**.
JPEG correction7b35434; later63d7269 changes Windows CI SDK installation only.
Primary Android checkout remains untouched. Work continues in a managed detached
worktree at the published PR #7 revision, with no branch switch/reset/history rewrite.

**Actual physical root cause:** JpegEncode/0x88982F41 after successful WGC frame
acquisition and SoftwareBitmap copy. ImageQuality was incorrectly written as frame
metadata. Move existing Single0.72 into BitmapEncoder.CreateAsync options instead;
no capture backend, geometry/retry/consent/provider/animation change was required.
Independent native codec probe reproduced the old error and passed the correction.

**Corrected laptop evidence:** developer-triggered harmless Notepad captures
complete in687ms,265ms and281ms. All report578x557 content/buffer/pool after one
Recreate from594x565, final VisibleBounds, DPI1.25, HRnone. Developer explicitly
confirmed a genuine Notepad preview. Direct GUI reads confirm Preview ready,
578x557 Window,11 real UIA targets and mapping VisibleBounds. Labels include window
buttons and Notepad status text. Portable OCR correctly remains UnavailableIdentity;
this demonstrates accessible UIA labels, not OCR of the document body.

Developer explicitly confirmed500/500 center animation, stationary real mouse,
Return Home and Stop/discard clearing all passed. Subsequent direct GUI read shows
capture OFF, Snapshot:none and analysis/point/share actions disabled. Final status
retains prior local-analysis text, so the read alone does not distinguish Stop
from60s expiry; Stop clearing is developer-reported, not independently timed.
A prior capture also discarded after a selected-window geometry change.
No image/screenshot file, private text or provider data was retained in Git.
Only text diagnostics/UI accessibility were inspected by the agent; no screenshot
upload or Gemini request was performed. Cloud screen sharing stayed OFF.

| Fresh evidence | Result |
| --- | --- |
| [Windows40226759821](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40226759821) |238 unit tests (Core27/Screen70/Link141), zero failed/skipped;46 native checks including laptop-sized and scaled JPEG round trips; WPF0 warnings/errors; self-contained x64 |
| [PR Windows40226763230](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40226763230) |Also passes at63d7269 |
| [Android40226763197](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40226763197) |111 tests, zero failures/errors/skips; workflow passes; existing installed APK retained |
| Local artifact verification |ZIP digest matches GitHub; all413 manifest files plus EXE/DLL/manifest hashes match; matching TRX/native evidence downloaded and read |
| Local static |Exact seven-path scope, protected paths unchanged, historical tracking retained, git diff --check passes |

[Windows application artifact12611119362](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40226759821/artifacts/12611119362)
and [native/TRX evidence12611084399](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40226759821/artifacts/12611084399).
Local verified executable:
`D:\Development\bluey-windows-android\build\f4c-native-debug\ci-63d7269\app\Bluey.Windows.exe`.

| File | SHA-256 |
| --- | --- |
| Bluey.Windows.exe |5043e7adf10d9d1f683ef8506ddcb75de1196dce872e6fede5826338eda8efd4 |
| Bluey.Windows.dll |4b25e16d87d1a5ff423efc28b6094db2ba82a591c6ed49b19bfeb9e42dc435d9 |
| SHA256SUMS.txt |48e6d1e1c4ebf594e21c67d5351a9f5f0dc50f0070596c832569658af41b5588 |
| Application ZIP |2f8ad638ba570aeb92f4262b7213672b35500884fe80c4c055a56fab43f5a141 |
| Evidence ZIP |010ea63870bb65ef6a34c7a45f867dd68718596eea09c7653e13c8180c7d8fd3 |

Build prerequisite handling: no local SDK/Visual Studio installation. Initial CI
7b35434 failed before compilation because setup-dotnet followed latestPatch to an
unavailable10.0.403 archive (HTTP409). Installer is now pinned to the already-declared
10.0.402 baseline; global.json remains unchanged. setup-dotnet still warns about
its optional latest-LTS runtime download, but exact SDK setup and build pass.
GitHub CLI artifact download stalled at0bytes; normal GitHub connector download
succeeded, archive hash matched before extraction. No network-policy change.

Authorized commit/push used the GitHub public no-reply identity after GH007 rejected
publishing a protected private email. Rejected local commit was preserved in an
archived managed worktree; remote history was only fast-forwarded. No account
privacy setting changed. Required closeout documentation follows the tested app.

F4-C remains active for broader physical robustness: DPI/driver/performance,
active-capture cancellation timing and packaged OCR are not accepted by this test.
F4-B provider acceptance remains separate. No F5, autonomous input or merge.
**One next task:** on this same verified artifact and harmless Notepad window,
measure Stop during an active capture and confirm no late preview is published;
keep cloud OFF and both PRs unmerged.

Exact seven paths changed from reviewed91c9d3f:
`.github/workflows/windows.yml`, `windows/Bluey.Windows/Screen/NativeCapture.cs`,
`windows/Bluey.Windows/Screen/CaptureSmoke.cs`, and the four tracking files
`STATUS.md`, `CHECKPOINTS.md`, `HANDOFF.md`, `SESSION-LOG.md`.

# Earlier initial local diagnosis (before CI authorization)

2026-10-26 Asia/Karachi. PR #7 remains open/unmerged at reviewed head
`91c9d3f46d0d31fe0e39d7d1db2b932a391574c9`, targeting `port/live-ai-voice`.
Primary checkout was clean on `port/android-face` at `a9be517`; preserved in place.
Work is isolated at the reviewed PR #7 head in a managed detached worktree.
No commit, push, PR #6 change, provider request, tool installation or merge.

**Physical failure observed:** the running verified application reaches
`JpegEncode / NativeFailure / 0x88982F41`, not FirstFrame. Its safe diagnostic shows
non-null content578x557 in buffer594x565, then one successful pool recreation and
accepted content=buffer=pool578x557. DWM578x557/Win32594x565, DPI1.25, final
VisibleBounds, frame2 accepted391ms; JPEG failure516ms. Selection, WGC item, D3D
session, frame acquisition, recreation and SoftwareBitmap copy completed. No
Snapshot or WPF preview was published. This attempt supplies no evidence of window
movement, null frames or an unresolved frame-pool fault.

The existing complete Windows folder at `D:\Development\F4 C FirstFrame Capture Rejection`
was reused. All413 SHA256SUMS entries independently match, including EXE
`78d0eeb41540bcf36b616ee3e83a3d41058f10d879822ae7abd96e82d03cd483`, DLL
`5515303fa5317b14b7537675a3311ad98a8e1737797d5ce11f6661b924be536a` and manifest
`8cebb8aa5022011ce43ab58ef1ecdb41029f053d46b989a65dfd25e4ac599c0f`.
Computer Use read the existing Bluey diagnostic after the developer's GUI capture;
only the harmless Notepad window was requested, cloud sharing remained OFF.

Root cause: `ImageQuality` was sent to `BitmapProperties.SetPropertiesAsync`, a
metadata writer. It is an encoder option. The local installed Windows WinRT codec
independently reproduces `0x88982F41` (`WINCODEC_ERR_PROPERTYNOTSUPPORTED`) with the
old metadata call. Passing Single0.72 to `BitmapEncoder.CreateAsync` instead encodes
and decodes generated black/white images:578x557=>578x557/6916bytes and
1920x1080=>1100x619/12637bytes; decoded contrast passes. No desktop pixels, files,
network service or SDK installation are involved in that independent codec probe.
Probe scripts remain local under the primary checkout's ignored
`build/f4c-native-debug/`; they are not production application build evidence.

Minimal correction: move the existing quality option into encoder creation;
remove the invalid metadata write. Native smoke coverage now calls production
Encode/Decode for the exact laptop extent and existing scaled extent, checking
size and nonblank image. Capture policy/geometry/consent/provider/animation/F3 and
Apple/Android code are unchanged. No GDI fallback or broader capture is needed:
this physical attempt demonstrates WGC acquisition through bitmap copy.

Local build prerequisite missing: .NET SDK10.0.402 (`windows/global.json`). No
dotnet/MSBuild/cl compiler on PATH or dotnet in standard install locations;
legacy .NET Framework csc/Windows WinMetadata are present but cannot build this
net10 WPF application. No Visual Studio/SDK installed. Full application build,
unit/native smoke execution and corrected-binary physical preview remain UNRUN.

F4-C stays active: genuine corrected preview, visual geometry/point accuracy,
UIA accessible text, animated pointing with stationary OS mouse, and snapshot
clearing still need physical acceptance. Portable OCR package identity remains
unavailable by design. No Gemini API call or screenshot upload was performed.

Exact next task: approve committing/pushing this scoped repair to the existing
PR #7 branch for Windows CI, verify/download its self-contained artifact, then
repeat the harmless Notepad GUI capture and local analysis/point/Home/Stop checks.

# Current F4-C FirstFrame acquisition repair — PR #7

2026-10-25 Asia/Karachi. Started clean on2fb87600341ea4b429470dfbc16a03ec44b77fee,
port/screen-understanding; PR7 targets port/live-ai-voice. PR6 remains unchanged
at4b380294e883e669d29000cd844962db88439e58. Both remain unmerged.

Developer physical evidence: five Notepad attempts reach FirstFrame/Changed, latest
156ms, DWM578x557, WGC=Win32594x565, DPI1.25, WindowFrame. Prior ItemSize blocker is
resolved. No real preview yet. Old code commits to the first callback and rejects
null OR ContentSize != initial item.Size without a second frame. Those are confirmed
source defects; which condition the laptop encounters remains unknown.

Correction: FrameArrived only signals a capacity-one bounded channel. A single
consumer acquires <=6 frames and recreates the pool <=2 times, all within the
existing8s deadline. Every frame is disposed before Recreate. Copy requires exact
positive bounded content/buffer/pool agreement, never assumed aspect equivalence.
Final coordinates resolve from unchanged DWM/physical Win32 bounds, with explicit
PreviewOnly denial for unknown mappings. Movement, actual resize, blank/protected
frames, invalid bounds and stale/canceled publication remain rejected. No background
recording, second capture session or user-visible automatic retry.

Diagnostics add per-frame null/content/surface/pool/current-item extents, initial
item size, counts, recreations and elapsed ms to the existing8-attempt memory ring.
No titles/positions/handles, images, OCR/private text or keys. Event handlers never
copy/own frames; the consumer finishes before disposal. Stop cancels publication;
already-running native copy keeps its dependencies alive, then discards late pixels.
New capture stays blocked until cleanup completes. Consent/F3/provider/animation
code and original Apple/Android source remain untouched. No model request made.

Regression evidence: extracted legacy policy11 cases had9fail/2pass before repair.
The final screen suite70pass covers null then valid, transient/persistent mismatch,
clipped/undefined buffers, bounded Recreate, movement/resize, timeout/Stop, final
center mapping, preview-only denial and no unapproved sharing. Native Windows and
Android results follow. No final failing app tests.

Tested application revision **f3856770710d789cb7396e6080c13d7950c4c8b2**.
Later closeout changes tracking only; the binaries below contain the final repair.

| Evidence | Actual result |
| --- | --- |
| Local .NET10.0.402 |Core27 + Screen70 + Link141 =238pass; WPF0 warnings/errors |
| [Windows40222661491](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40222661491) |238 unit tests,0failed/skipped;45 native checks; WPF0 warnings/errors; self-contained unsigned Windows x64 |
| [Android40222661934](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40222661934) |111 tests,0failed/errors/skipped; lint0errors/18warnings; APK signature verified |
| Stacked PR checks |Windows40222665650 and Android40222665604 also pass at the same application revision |
| Static |17 canonical/Windows Markdown files,218 local links/anchors and3 JSONL fixture collections pass; protected paths unchanged; git diff --check |

Artifacts (14-day retention, matching application revision):

- [Windows x64 folder + SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40222661491/artifacts/12610415501)
- [Windows TRX/native/synthetic-image evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40222661491/artifacts/12609837506)
- [Optional unchanged Android APK + manifest/signer](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40222661934/artifacts/12610400668)
- [Android test/lint/bitmap evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40222661934/artifacts/12610575246)

| File | SHA-256 |
| --- | --- |
| Bluey.Windows.exe |`78d0eeb41540bcf36b616ee3e83a3d41058f10d879822ae7abd96e82d03cd483` |
| Bluey.Windows.dll |`5515303fa5317b14b7537675a3311ad98a8e1737797d5ce11f6661b924be536a` |
| Windows SHA256SUMS.txt |`8cebb8aa5022011ce43ab58ef1ecdb41029f053d46b989a65dfd25e4ac599c0f` |
| Windows ZIP upload digest |`da97a08d542b106db02b7511ba93e5259b20ca276a167f1e61c4c99c3cf9181a` |
| Optional app-debug.apk (CI hash) |`0a491e94132872aca857ae72cdc43c56b970f179c036a796571af20953363bb1` |

Downloaded Windows app and evidence; independently rehashed all413 manifest entries,
including EXE and DLL. Native JSON has45 checks, including actual supported
Recreate calls, but never StartCapture or WGC pixel acquisition. Native public
window dimensions are DWM=WGC306x203,Win32320x210,DPI1,VisibleBounds. Public generated
JPEG checks and pure frame-policy tests do not prove real laptop first-frame capture.
Physical preview, exact null-versus-size condition, DPI/point accuracy, exclusion,
driver/performance and Stop timing remain unverified. OCR package identity and
F4-B provider physical acceptance remain separate pending gates. No provider request.

Android source/protocol unchanged: retain the installed APK for this Windows test.
Optional new debug signer60191b84d5221a4195ee8153a7d900f9f24edca6bb061e55e8bdd06a2ab5b1bd
differs from earlier builds. If deliberately replacing an incompatible-signer APK,
manual reinstall loses Android Keystore identity and needs F3 re-pair/revoke old
phone trust. Do not silently delete identities or Windows protected data. Android
artifact download was blocked by restricted policy at productionresultssa10;
its hash/signature are verified CI evidence, not locally rehashed bytes. Windows
download succeeded normally; no proxy bypass or network-policy change. CI also
retains a setup-android Node20 deprecation annotation (forced Node24), unrelated
to app behavior; lint18warnings are retained, not new capture regressions.

Next: run one offline physical Notepad capture, inspect safe frame diagnostics,
and point500,500 only if the final map is verified; Return Home/Stop must clear it.

Exact15 changed paths against2fb8760:

```text
project/QA.md
tracking/CHECKPOINTS.md
tracking/DECISIONS.md
tracking/HANDOFF.md
tracking/SESSION-LOG.md
tracking/STATUS.md
windows/Bluey.Screen.Tests/CaptureDiagnosticsTests.cs
windows/Bluey.Screen.Tests/CaptureFramesTests.cs
windows/Bluey.Screen.Tests/WorkflowTests.cs
windows/Bluey.Screen/CaptureDiagnostics.cs
windows/Bluey.Screen/CaptureFrames.cs
windows/Bluey.Windows/Screen/CaptureSmoke.cs
windows/Bluey.Windows/Screen/NativeCapture.cs
windows/Bluey.Windows/Screen/ScreenWindow.cs
windows/README.md
```

# Earlier F4-C ItemSize geometry repair — retained evidence

2026-10-25 Asia/Karachi. Clean start cc122d02ec07a6844b2bedd3d8e5cd43968c276c on
port/screen-understanding. PR7 still targets port/live-ai-voice; PR6 remains open
at4b38029, unchanged. Neither PR merged. No new branch, provider request, OCR
packaging or F5 action. Original Apple/Android/link/animation/key code unchanged.

Developer physical evidence: Unicode picker titles are fixed (NOTEPAD.txt - Notepad).
Two real window attempts stop before frames at ItemSize/Changed/HRnone (203/110ms),
with no preview/snapshot and cloud OFF. Actual laptop dimensional values are unknown.
Source confirms the premature gate: WGC width/height must exactly equal cached DWM
visible bounds, with all differences classified as Changed before frame acquisition.
Microsoft distinguishes visible DWM bounds from DPI-virtualized Win32 rectangles
that may include invisible borders. The gate is invalid; whether this laptop's
specific mismatch is borders, DPI or an actual geometry change remains unverified.

Correction: positive WGC item dimensions (<=16384/axis,16Mi pixels, bounded aspect/
relative extents) size the frame pool. CaptureGeometry resolves an explicit image/
physical/DIP transform only for exact visible extents or a matching measured physical
Win32 rectangle containing the DWM selection, bounded per-edge padding<=32DIP and
primary containment. GetWindowRect scopes/restores per-monitor-v2 thread awareness.
Keep both rectangles for freshness; recheck before/after capture, frame content,
bitmap and encoded sizes. Never stretch DWM bounds or infer origins from DPI ratios.

Unknown but bounded capture geometries can show PREVIEW ONLY. Coordinate converters,
target extraction, analysis, point resolution and screenshot approval reject such
mappings; relevant UI controls are disabled. A preview is not coordinate authority.
Verified Surface aspect/bounds constraints remain and verified frame modes require
matching measured bounds. Stop/expiry/movement/resize/close/DPI/disconnect invalidate
pixels, targets and approval. No crop/warp, backend or input API was added.

Diagnostics keep8 memory-only records and now include DWM/WGC/Win32 width/height,
absolute deltas, DPI scale and mapping mode. No titles, origins/handles, screenshot
pixels, OCR/private text or credentials are logged. Existing screenshot review,
F3 one-question authorization, protected keys and cloud-OFF defaults are unchanged.

Final tested application **df1eb62d52ea325a16fda67d2e9624097c4e1303** (geometry source42ebf5c plus
Windows-CI-only safe report output). Later closeout changes tracking only.

| Evidence | Actual result / matching artifact |
| --- | --- |
| Local .NET10.0.402 |Screen53 pass; retained Core27/Link141 pass after geometry/guard integration; WPF0 warnings/errors |
| [Windows40133905687](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40133905687) |Core27/Screen53/Link141 (221 total), zero failed/skipped;44 native checks; WPF0 warnings/errors; self-contained unsigned x64 |
| Windows ZIP + SHA256SUMS |[artifact12575917352](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40133905687/artifacts/12575917352); no separate desktop runtime |
| Windows TRX/native/public fixture evidence |[artifact12576071997](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40133905687/artifacts/12576071997) |
| [Android40133905040](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40133905040) |111 tests, zero failed/errors/skips; lint0 errors18 warnings; signature verified, same application revision |
| Optional unchanged Android regression APK |[artifact12576037203](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40133905040/artifacts/12576037203); [test/lint/bitmap evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40133905040/artifacts/12575892345) |
| Stacked PR checks |Windows40133910135 and Android40133909958 also pass |

Downloaded final Windows app/evidence, rehashed all413 manifest entries and verified
EXE hash against actual CI output. Native JSON has44 checks. Earlier42ebf5c artifact
downloads were policy-blocked at sa14; those superseded bytes were not locally
rehashed. The final df1eb62 artifact downloaded normally. No proxy bypass/policy
change was used. Artifacts retain14 days from upload.

| File | SHA-256 |
| --- | --- |
| Bluey.Windows.exe |`d8c15c6c28390ca0e7526ce63af8339663497c5f86e831c7fcc0c8553914b966` |
| Bluey.Windows.dll |`01ff39f65d7acd00a7387972c53ba5ad111bac0f2cbac15065d09412604bcc2b` |
| Windows SHA256SUMS.txt |`adce7dc1940e3454ef4e4efae336087dc1be862545ee233353b36ab6e42cd648` |
| Windows ZIP upload digest |`4bdd3806885cc9a4d7d6dd4d81ffa648f08cb4955beb152167dc6dfe96b78bf4` |
| Optional app-debug.apk (CI hash) |`e6fd63d71819154abdac16bec7953d2cb6971e2621d1ac79eb431ccfbcb7f0ae` |

Native CI public window: DWM306x203, WGC306x203, Win32320x210, delta0x0,
dpiScale1, mapVisibleBounds. This is an exact-match native case, NOT the developer's
Notepad dimensions or a reproduced native WGC/DWM mismatch. Synthetic tests cover
measured border differences, unknown DPI/aspect, verified DIP/grid math, allocation
limits, moved/resized bounds, preview-only denial and stale/canceled cleanup.
Native generated1920x1080 image encodes/decodes1100x619, consistent with the transform.
CI NEVER starts WGC capture or reads desktop pixels. First-frame/real preview,
actual laptop dimension values, coordinate accuracy, driver/DPI/exclusion/performance
remain UNVERIFIED. Native OCR package identity and F4-B provider acceptance remain
separate pending checkpoints. No live Gemini/OpenAI request/key/personal image used.

Initial old-equality policy regression:13 focused cases,7 failed/6 passed. Added
verified-Surface proof test failed (unproven frame accepted), then passed after guard.
Final53 screen cases pass. No final CI failures. Only retained Android lint18 warnings.
24 repair paths below; preserved Mac/iOS/Shared/assets, Android app, Bluey.Core/
OverlayArt, Bluey.Link, API-key storage and original history checked unchanged.

Android app/protocol unchanged: retain installed APK for this Windows test. New
optional debug signer b7a970ec096261849730d320fe82c440666db3d1aac0bc119554585a7a932cd9
differs from prior builds; conscious reinstall loses Keystore identity and needs
re-pair/revoke. No silent identity deletion or Windows protected-data removal.

Exact24 changed paths against cc122d0:

```text
.github/workflows/windows.yml
project/QA.md
project/SECURITY.md
tracking/CHECKPOINTS.md
tracking/DECISIONS.md
tracking/HANDOFF.md
tracking/SESSION-LOG.md
tracking/STATUS.md
windows/Bluey.Screen.Tests/CaptureDiagnosticsTests.cs
windows/Bluey.Screen.Tests/CaptureGeometryTests.cs
windows/Bluey.Screen.Tests/WorkflowTests.cs
windows/Bluey.Screen/CaptureDiagnostics.cs
windows/Bluey.Screen/CaptureGeometry.cs
windows/Bluey.Screen/Geometry.cs
windows/Bluey.Screen/ScreenSession.cs
windows/Bluey.Screen/ScreenWorkflow.cs
windows/Bluey.Screen/Targets.cs
windows/Bluey.Windows/Screen/CaptureSmoke.cs
windows/Bluey.Windows/Screen/NativeAnalysis.cs
windows/Bluey.Windows/Screen/NativeCapture.cs
windows/Bluey.Windows/Screen/ScreenCoordinator.cs
windows/Bluey.Windows/Screen/ScreenSmoke.cs
windows/Bluey.Windows/Screen/ScreenWindow.cs
windows/README.md
```

One next action: developer performs the single local Notepad capture/pointing retest
in project/QA.md with cloud OFF; report only safe dimensions/mapping/stage/outcome
if preview or mapping fails. Never force pointing on PREVIEW ONLY.

# Earlier F4-C native capture repair (retained evidence)

2026-10-24 Asia/Karachi. Resumed from clean ad5f7aac4f3cbb006aac74d837fca7d1d805e6f6
on port/screen-understanding. Execution now works in the repository subdirectory;
/workspace itself is not a Git root. The prior interrupted attempt made no changes.
PR7 remains stacked against port/live-ai-voice; PR6 remains open/unmerged at4b38029.
Neither PR is merged. No new branch, F5, OCR packaging, provider request or key.

Developer physical evidence: picker labels were single characters and capture
returned capture-denied-or-unavailable without preview. No native local OCR/UIA/
pointing acceptance was reached. This repair does not turn that into a device pass.

Confirmed source defects and experiments:

- GetWindowTextW omitted CharSet.Unicode; native UTF16 output was marshalled as ANSI.
  Actual HWND regression uses both `*Untitled - Notepad` and a public Unicode title.
- IDXGIDevice UUID was wrong: requested54ec77fa-1377-44e6-8c32-388fd5f44c84 instead of
  Microsoft's54ec77fa-1377-44e6-8c32-88fd5f44c84c. QueryInterface cannot obtain the
  documented interface with that UUID; both capture kinds share this setup.
- Red native regression d32b1e4 / [Windows40006819441](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40006819441)
  failed Unicode title and D3D/pool setup E_NOINTERFACE(0x80004002). Window/primary
  capture-item creation passed. Minimal two-line correction16efb78 /
  [Windows40007491657](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40007491657)
  passes the same real native contracts. This isolates the shared setup defect;
  laptop first-frame/copy/encoding success remains a physical observation to obtain.

Final application revision **fdc9572ee9e305cc489d9784a50f865943355cf5** adds bounded, memory-only diagnostics:
last8 completed local attempt numbers, selected kind, stage, outcome, native HRESULT
and elapsed ms. No exception message/stack, title, HWND, image, OCR/private text,
credential or provider content is retained. The selectable read-only UI box and
short safe error guidance identify selection/support, activation/item projection,
D3D/DXGI/projection, pool/session/start, first frame, copy, JPEG and validation stages.
The free-threaded callback cannot be overwritten by an earlier wait-stage update.
Stop/cancel, preview, one-shot sharing, geometry/DPI/media limits and zeroing remain.
Self/hidden/cloaked/out-of-primary selection filtering remains; self-window filter
has a native regression. No legacy fallback, new dependency, input API or server.

| Evidence | Actual result / matching artifact |
| --- | --- |
| Local .NET10.0.402 | Core27/Screen35/Link141 pass; zero failures/skips; WPF0 warnings/errors |
| [Windows40008459044](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40008459044) |203 unit tests;42 native checks; WPF0 warnings/errors; self-contained unsigned win-x64 |
| Windows app/manifests | [artifact12525795255](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40008459044/artifacts/12525795255); no separate desktop runtime needed |
| Windows TRX/native/public fixture evidence | [artifact12524957786](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40008459044/artifacts/12524957786) |
| [Android40008459792](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40008459792) |111 tests, zero failures/errors/skips; lint0 errors18 warnings; APK signature verified; same application revision |
| Optional matching regression APK/manifests/signer | [artifact12524349807](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40008459792/artifacts/12524349807) |
| Android JUnit/lint/public bitmap evidence | [artifact12525606682](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40008459792/artifacts/12525606682) |
| Stacked PR checks |Windows40008467356 and Android40008467447 also pass |

Downloaded Windows app and evidence: rehashed all413 manifest entries, EXE hash
matches actual CI summary, inspected native42-check JSON. Android hash/signer evidence
is from CI, not a local APK rehash. Artifacts retain14 days from upload. Subsequent
tracking-only closeout does not alter the tested application/artifact bytes.

| File | SHA-256 |
| --- | --- |
| Bluey.Windows.exe | `a02e91fd2b151869f104703c515f20377d6195e3ca6cbf6571fef3ed8ff09fb6` |
| Bluey.Windows.dll | `6994418337883db756ebae9c2f22f1aff1b4d2c4f81882b7d349c5304a5fa1eb` |
| Windows SHA256SUMS.txt | `4083de9bb28f11f221b7eb7b51aa9ed023f7a58bbc3bb3681aa1ce3ef1ab3140` |
| Windows ZIP upload digest | `f65b7958d605e87b7c6e7791231a20f90db98e4a29cd70909188d00f2dc5a35c` |
| Optional app-debug.apk (CI) | `2ef6a495b6556f039376348202a39310d980444c7ceac0c19c4f98bb3d0e7bb0` |

Android application/protocol source is unchanged. Retain the installed F4-B APK for
this Windows-only repair; rebuilding debug APK changes signer. New public signer
`4b2742606a30f3ae978552272646f791d2a76214b382753aea35ab34af20b09b`
differs from older artifacts; deliberate reinstall loses Android Keystore identity
and requires F3 re-pairing/revocation. Never silently erase Windows protected data.

Evidence boundary: native CI exercises real Unicode Win32 titles, separate window/
primary capture-item metadata, D3D/DXGI/WinRT device/pool/session construction and an
invalid-window failure with safe stage/HRESULT. It NEVER calls StartCapture or reads
real desktop pixels. Existing JPEG/blank checks use generated public bitmaps only.
First-frame acquisition, actual laptop capture permission/driver/protected content,
geometry/DPI/preview and physical performance remain UNVERIFIED. Native OCR still
reports UnavailableIdentity in portable builds and is not a WGC prerequisite. F4-B
physical provider acceptance remains pending. No Gemini/OpenAI request or secret.

Exact 14 repair changed paths against ad5f7aa:

```text
project/QA.md
tracking/CHECKPOINTS.md
tracking/DECISIONS.md
tracking/HANDOFF.md
tracking/SESSION-LOG.md
tracking/STATUS.md
windows/Bluey.Screen.Tests/CaptureDiagnosticsTests.cs
windows/Bluey.Screen/CaptureDiagnostics.cs
windows/Bluey.Windows/Screen/CaptureSmoke.cs
windows/Bluey.Windows/Screen/NativeCapture.cs
windows/Bluey.Windows/Screen/ScreenCoordinator.cs
windows/Bluey.Windows/Screen/ScreenSmoke.cs
windows/Bluey.Windows/Screen/ScreenWindow.cs
windows/README.md
```

One next action: developer runs the local-only primary-display and Notepad capture
retest in project/QA.md, with cloud OFF, recording each result independently and
copying only the safe diagnostic row on failure. No OCR packaging or cloud test.

# Earlier F4-C implementation and delivery (retained evidence)

2026-10-22 Asia/Karachi. Explicitly authorized before final F4-B physical retest;
this is not F4-B acceptance. Base origin/port/live-ai-voice verified4b380294e883e669d29000cd844962db88439e58;
accepted main7930efd unchanged. Branch port/screen-understanding targets the F4-B
branch. Do not modify/merge PR6 or merge the F4-C PR.

Implemented pure Bluey.Screen maps/snapshots/targets/consent/workflow/choreography,
WGC/D3D11 one-snapshot capture with selectable primary/window and preview, read-only
UIA, supported WinRT OCR adapter/availability guard, local pointing/Home/Stop, and
optional same-request Gemini WAV+JPEG/structured target context. No Android app or
LAN schema change. No screenshot/audio/private text/key logging or persistence.

Exact native OCR deployment blocker: Microsoft requires package identity; current
portable unsigned build has none. UI reports UnavailableIdentity, not fabricated
recognition. No package install/registration or external OCR is added. Real capture,
UIA targets and point-at-spot remain independent; synthetic demo labels all declared
annotations. Google image/structured pages policy403; current official Google SDK
source verifies inline multipart/JSON schema shape; actual fixed-model multimodal
behavior and arbitrary account eligibility remain unverified. D013 records limits.

Final tested application revision: **fc832f8890d8dc0f8e565b74a4a79b4687343828**. Actual Windows
CI: Core27, Screen32, Link141 pass (200 total, zero failures/skips), WPF/net10
Windows22000 build0 warnings/errors. Windows native35 checks pass; Android111
tests pass with lint0 errors/18 retained warnings. Matching artifacts verified below.
No real provider call/key/personal screenshot or OS input action was used. F4-B's
five reported API attempts/two503s/no proven genuine visible answer stay pending.

One next action: developer runs the offline stages of the combined F4-B/
F4-C QA sequence, recording the OCR identity blocker separately from capture/UIA/
pointing; no cloud test occurs without separately current explicit authorization.

## F4-C delivery evidence

[PR #7](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/7)
is stacked against `port/live-ai-voice`. Both PRs remain open/unmerged; PR6 head
still4b380294e883e669d29000cd844962db88439e58. Documentation closeout after the
application revision above changes tracking only, not the tested binaries.

| Evidence | Actual result / download |
| --- | --- |
| Windows push CI | [39834969073](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39834969073): Core27/Screen32/Link141; native35; clean WPF; self-contained win-x64 |
| Windows app + SHA256SUMS | [artifact12450033743](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39834969073/artifacts/12450033743); no separately installed desktop runtime required |
| Windows TRX/native evidence | [artifact12449963853](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39834969073/artifacts/12449963853); 13 retained WPF frames +1 clearly public synthetic screen fixture; no real screenshot |
| Android dispatch CI | [39834970366](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39834970366):111 tests,zero failed/errors/skips; lint0 errors/18 warnings; verified debug signature |
| Matching regression APK + SHA256SUMS + signer | [artifact12450572867](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39834970366/artifacts/12450572867) |
| Android JUnit/lint/bitmap evidence | [artifact12450647703](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39834970366/artifacts/12450647703) |
| Additional stacked PR checks | Windows39834977612 and Android39834977562 also pass at this application revision |

Final Windows/Android uploads and hashes are observed in actual CI logs/artifact
metadata. Final archive downloads were blocked by this environment's network policy
at productionresultssa19/sa9.blob.core.windows.net, so final bytes were NOT locally
rehashed. Earlier6f471fa app/evidence downloads were successful: all413 Windows
manifest entries/APK were rehashed, TRX/native JSON and public ALPHA BETA/Save frame
inspected; that is earlier-revision evidence only, superseded by downloads above.
Artifacts retain14 days from2026-10-22; authenticated GitHub download may be required.

| File | SHA-256 |
| --- | --- |
| Windows artifact ZIP (GitHub upload digest) | `dc51d74dcff53a85f84751035fd975c6be6ba1233b6c632c78b4584ffbf3a76f` |
| Bluey.Windows.exe | `37bf5077ad8e8f12ca10e0101725953d3bda2c5ec5173c248d43ef79154031a9` |
| Bluey.Windows.dll | `79d939581a1b72e5ddd615205549f882408f21a99c75d259b867f64726a4f0fc` |
| Windows SHA256SUMS.txt | `68c5970cfc9c1d10ec4cc918b88ebc197a941ffb0c23e6a97e01cae0bfafd94f` |
| Android app-debug.apk | `0dd17e7c894e336f5b23d206bf634b1e6790b1d39f0a5e8b550d83fd16ccb0a4` |

New APK public signer SHA256:
`d2fdccd16b1fc08e1f339479184903badf0c222048dc620e0cd68270d1d1ebd3`,
different from the F4-B delivered b30fe5e0… signer. Android app/protocol source is
unchanged: retain the installed F4-B APK for the offline screen test.
If deliberately replacing it, compare signers first; a mismatch requires conscious
uninstall/reinstall, loses Android Keystore identity/trust and needs F3 re-pairing
plus revocation of the old phone. Never erase Windows protected keys/trust files.

Native evidence is precise: actual Windows11 runner JPEG encode/decode/blank-image
rejection and read-only UIA public-button/password-exclusion checks passed. The
unpackaged OCR guard reports `UnavailableIdentity`; **no OCR recognition pass**.
WGC desktop capture was deliberately NOT EXECUTED in CI. Real capture consent,
protected/HDR content, exclusion, DPI changes, actual pointing/focus/fidelity,
FPS/resource usage and optional fixed-model multimodal replies remain UNVERIFIED.
Microsoft-supported package identity deployment remains the native OCR blocker;
no install/registration or unsupported workaround was added. Multi-monitor targets,
automatic redaction and input automation are not implemented.

Fake HTTP/synthetic audio and pixels cover one combined WAV+JPEG request, strict
answer/target schema, separate screen consent/current F3 one-question authorization,
invalid target suppression, cancellation and no retry. Actual mutual TLS revocation
clears media and rejects a late fake reply. No new LAN message/frame size, credential
path, real provider request, key, paid call or personal capture was used. Source
comparison preserves1100/.72 JPEG, L400/W600/C140 and450ms/5000 UIA budgets,
0–1000/y-down geometry and existing1s flight allowance+2.2s hold/5s quiet-home;
primary DIP conversion and edge clamping are explicit platform adaptations.

Static closeout:17 canonical/Windows Markdown files,218 local links/anchors resolved
(including explicit legacy HTML anchors); protocol JSONL parses; git diff --check.
Original Mac/iOS/Shared/assets, Android app, Bluey.Core motion and OverlayArt unchanged
against4b38029. Initial local API/compiler errors corrected during implementation;
final focused local Screen32/WPF checks and all final CI tests pass. An initial link-check false positive on an existing
HTML anchor was resolved by recognizing explicit anchors, without changing that link.

Final source cleanup also binds queued local points to their current snapshot:
replacement, expiry or invalidation drops pending points and returns home once.
An authenticated model point replaces any local queue, preventing full-queue failure.
A deterministic stale-context regression passes; current capture status refreshes
during work and shows OFF after disposal, with historical results labeled separately.

Exact48 changed paths against the F4-B base:

```text
.github/workflows/android.yml
.github/workflows/windows.yml
project/APP-FLOWS.md
project/ARCHITECTURE.md
project/IMPLEMENTATION.md
project/PORTING-MAP.md
project/PRD.md
project/PROTOCOL-DATA-SCHEMA.md
project/QA.md
project/SECURITY.md
project/TRD.md
tracking/CHECKPOINTS.md
tracking/DECISIONS.md
tracking/HANDOFF.md
tracking/SESSION-LOG.md
tracking/STATUS.md
windows/Bluey.Link.Tests/GeminiScreenTests.cs
windows/Bluey.Link.Tests/ScreenAuthorityTests.cs
windows/Bluey.Link.Tests/TlsTests.cs
windows/Bluey.Link/AudioAuthority.cs
windows/Bluey.Link/Bluey.Link.csproj
windows/Bluey.Link/GeminiAnswers.cs
windows/Bluey.Link/LinkServer.cs
windows/Bluey.Link/LiveAuthority.cs
windows/Bluey.Link/ScreenAnswers.cs
windows/Bluey.Link/ScreenAuthority.cs
windows/Bluey.Screen.Tests/Bluey.Screen.Tests.csproj
windows/Bluey.Screen.Tests/ControlTreeTests.cs
windows/Bluey.Screen.Tests/ScreenTests.cs
windows/Bluey.Screen.Tests/WorkflowTests.cs
windows/Bluey.Screen/Bluey.Screen.csproj
windows/Bluey.Screen/ControlTree.cs
windows/Bluey.Screen/Geometry.cs
windows/Bluey.Screen/ScreenSession.cs
windows/Bluey.Screen/ScreenWorkflow.cs
windows/Bluey.Screen/Targets.cs
windows/Bluey.Windows/Bluey.Windows.csproj
windows/Bluey.Windows/Connection/WindowsLinkHost.cs
windows/Bluey.Windows/ControlsWindow.cs
windows/Bluey.Windows/NativeSmoke.cs
windows/Bluey.Windows/OverlayWindow.cs
windows/Bluey.Windows/Program.cs
windows/Bluey.Windows/Screen/NativeAnalysis.cs
windows/Bluey.Windows/Screen/NativeCapture.cs
windows/Bluey.Windows/Screen/ScreenCoordinator.cs
windows/Bluey.Windows/Screen/ScreenSmoke.cs
windows/Bluey.Windows/Screen/ScreenWindow.cs
windows/README.md
```

# Retained F4-B evidence (no new acceptance)

# Current F4-B response repair — PR #6

2026-10-22 Asia/Karachi. Continue port/live-ai-voice from reviewed7c846a3;
no new PR/merge/F4-C. Developer-reported physical requests reached Gemini but no
visible answer is proven: five requests,two503s,nonzero input/output usage. The
other three outcomes are unknown. F2 pointing demo text is not a Gemini reply.

Confirmed source/test defects:503/400 and empty MAX_TOKENS outcomes were collapsed;
request sent candidateCount with128 tokens/no low thinking. Seven local regression
cases reproduced the request/classification gaps; eight authority cases reproduced
loss of the specific failure on LAN. Baseline58278fe Android CI39671465812 ran103
tests with one intentional failure: coalesced reply/done plus disabled chirps skips
pink before a frame. This is not proof of physical provider text/response contents.

Repair: fixed model, no candidateCount, low thinking,512 combined thinking/output
tokens (up to4× prior allowance; still one question/5s/20s/256 display characters).
Eleven narrow liveStop outcomes validated in both languages/66 shared fixtures;
actionable status,8 safe local metadata records, no private text/body/audio/keys.
400ms minimum local replying interval with unchanged250ms border transition and
stale completion guards. Public synthetic trace spans real adapter fake HTTP,
authority/TLS, Android coordinator/controller and native WPF caption checks.
Offline preview now read-only OFF status plus an action opening existing opt-in
settings; opening it grants nothing. Actual permissions/Stop remain in LivePanel.

Local .NET10.0.402: Core27/Link124 pass,0 failed/skipped. Matching Windows/Android
CI and development uploads pass below; physical retest remains unverified.
Official new/thinking/error docs still policy403; supplied compatibility contract
and accessible official Google SDK source are separately attributed in D012.
No real Gemini/OpenAI call,credential,private audio or cloud test used by Codex.
Original Apple/Shared, animation engines/controllers and protected storage unchanged.

One next action after matching CI artifacts: developer performs the single physical
acceptance sequence in project/QA.md; any one-question cloud request requires the
developer's separate current Free Tier review and explicit authorization.

## Response-repair delivery evidence

Both downloads are built/tested at **36287bcdcb95496c3da8f61973a51fc1e337913f**. Subsequent closeout edits
change canonical documentation only. Existing [PR #6](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/6)
remains open and unmerged. Original/animation/key-store paths are unchanged.

| Target | Actual CI results | Matching artifacts |
| --- | --- | --- |
| [Windows 39674027367](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39674027367) | Windows2025/.NET10.0.402: Core27+Link124,0 failed/skipped; WPF0 warnings/errors;29 native checks/13 renders; self-contained unsigned win-x64 | [App + full SHA256SUMS.txt](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39674027367/artifacts/12381980060), [TRX/native evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39674027367/artifacts/12380199443) |
| [Android 39674027307](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39674027307) | JDK17/Gradle8.13/SDK36:111 tests,0 failures/errors/skips; native bitmap tests; lint0 errors18 warnings; APK signature verified | [APK + SHA256SUMS.txt + public signer](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39674027307/artifacts/12380439015), [JUnit/lint/render evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39674027307/artifacts/12380314227) |

Local .NET Core27/Link124 also pass. Final PR Windows39674033798 passes; earlier
PR39673123298 failed one retained cancellation test because its20ms sleep raced
async cleanup. The test now waits for provider entry and a terminal cleanup outcome,
then asserts every byte wiped/no stale reply, bounded5s. No assertion or production
cancellation behavior removed. Initial failing regression cases and intermediate
CI are retained in SESSION-LOG. Android compiler also reports three warnings in
unchanged test files: nullable alias string and two deprecated NSD host accesses.

| Item | Observed SHA-256 |
| --- | --- |
| Windows EXE (CI) | `f5c1f178640efbeb6cc9d2e2355ed93e9ea3a80dfa153a5f92f6941ff6a58e5f` |
| Windows DLL (CI) | `08e1207f1c4c9183805b10b11b1636bd17ee82eee854fab36d4f9c4fffd2a12c` |
| Full Windows SHA256SUMS.txt (CI) | `f23ac6fc0f3fc8159445d2322b0020d98471c38829e46e3c4eaccc0946018f0c` |
| Windows app ZIP (upload metadata) | `0cd20ec6e73aba43587180f9f845f3f6d926509ed12c74be5c210166f0a84b33` |
| Android APK (CI) | `73276555f3d04d50b6732c8d150c4a3230a0cf0c300f59734bced34c6b2b2e02` |
| Android app ZIP (upload metadata) | `e7bd9f0d5240267c7c2fb561a0f982af90c743ea4f99c630734b439ac0998e8d` |
| Android public debug signer | `b30fe5e09b7e180211497aaeb11ffdb056e68ccbf5e3b4cf09d9c7dfd9ce5909` |

The manifests and evidence are uploaded (14-day development artifacts); local
archive downloads were policy403 at productionresultssa2/sa10.blob.core.windows.net.
Hashes/test/native results above are actual CI/upload evidence, not local archive
rehashes. Windows needs no separate desktop runtime; this is not an installer/release.
Android is versionCode11/0.4.4-f4b-gemini-response. Its signer differs from the prior
published c48278f165f2c5df743db2ffa3075bfe777f4d9cc4c17c627f50ac9c08c4a2bb.
Compare installed signer before updating. A mismatch requires deliberate reinstall,
which loses Android Keystore/F3 trust: revoke the old Windows phone entry and pair
explicitly again. Preserve Windows DPAPI files; no automatic identity reset occurs.

Security evidence: no real Gemini/OpenAI request, user credential, hardware audio
capture or cloud test used by Codex/CI. Fake HTTP/synthetic PCM plus actual loopback
TLS and composed cross-language/native caption tests are distinct from physical
interoperability. Offline default, current per-phone cloud/Free Tier authorization,
one-question limit, pinned mTLS, DPAPI/Keystore, revocation, bounded transport and
no retry/fallback remain. Safe diagnostics retain8 code/time/size records, no private
content. The three non503 physical outcomes, revised actual provider behavior,
physical pink/caption/latency/audio/resource behavior and project eligibility remain
unverified. A public API contract and passing tests do not certify free account use.

One next action: the developer performs the [single physical acceptance sequence](../project/QA.md#f4-b-response-repair--one-physical-acceptance-sequence),
including separate current Free Tier/one-request authorization if choosing the cloud
step; record only safe Windows attempt/code/ms/counts and Android status. No automatic
cloud test, second request, merge or F4-C is authorized.

## Exact response-repair changed files

- [android/app/build.gradle.kts](../android/app/build.gradle.kts)
- [android/app/src/main/java/dev/bluey/port/MainActivity.kt](../android/app/src/main/java/dev/bluey/port/MainActivity.kt)
- [android/app/src/main/java/dev/bluey/port/live/LiveCoordinator.kt](../android/app/src/main/java/dev/bluey/port/live/LiveCoordinator.kt)
- [android/app/src/main/java/dev/bluey/port/live/LiveProtocol.kt](../android/app/src/main/java/dev/bluey/port/live/LiveProtocol.kt)
- [android/app/src/main/java/dev/bluey/port/voice/VoicePreview.kt](../android/app/src/main/java/dev/bluey/port/voice/VoicePreview.kt)
- [android/app/src/test/java/dev/bluey/port/link/ProtocolTest.kt](../android/app/src/test/java/dev/bluey/port/link/ProtocolTest.kt)
- [android/app/src/test/java/dev/bluey/port/live/AudioCoordinatorTest.kt](../android/app/src/test/java/dev/bluey/port/live/AudioCoordinatorTest.kt)
- [project/IMPLEMENTATION.md](../project/IMPLEMENTATION.md)
- [project/PROTOCOL-DATA-SCHEMA.md](../project/PROTOCOL-DATA-SCHEMA.md)
- [project/QA.md](../project/QA.md)
- [project/SECURITY.md](../project/SECURITY.md)
- [protocol/README.md](../protocol/README.md)
- [protocol/f4b-cases.jsonl](../protocol/f4b-cases.jsonl)
- [protocol/gemini-response-trace.json](../protocol/gemini-response-trace.json)
- [tracking/CHECKPOINTS.md](../tracking/CHECKPOINTS.md)
- [tracking/DECISIONS.md](../tracking/DECISIONS.md)
- [tracking/HANDOFF.md](../tracking/HANDOFF.md)
- [tracking/SESSION-LOG.md](../tracking/SESSION-LOG.md)
- [tracking/STATUS.md](../tracking/STATUS.md)
- [windows/Bluey.Link.Tests/AudioTests.cs](../windows/Bluey.Link.Tests/AudioTests.cs)
- [windows/Bluey.Link.Tests/Bluey.Link.Tests.csproj](../windows/Bluey.Link.Tests/Bluey.Link.Tests.csproj)
- [windows/Bluey.Link.Tests/GeminiTests.cs](../windows/Bluey.Link.Tests/GeminiTests.cs)
- [windows/Bluey.Link.Tests/ProtocolTests.cs](../windows/Bluey.Link.Tests/ProtocolTests.cs)
- [windows/Bluey.Link.Tests/TlsTests.cs](../windows/Bluey.Link.Tests/TlsTests.cs)
- [windows/Bluey.Link/AudioAuthority.cs](../windows/Bluey.Link/AudioAuthority.cs)
- [windows/Bluey.Link/AudioOutcome.cs](../windows/Bluey.Link/AudioOutcome.cs)
- [windows/Bluey.Link/GeminiAnswers.cs](../windows/Bluey.Link/GeminiAnswers.cs)
- [windows/Bluey.Link/LiveAuthority.cs](../windows/Bluey.Link/LiveAuthority.cs)
- [windows/Bluey.Link/LiveProtocol.cs](../windows/Bluey.Link/LiveProtocol.cs)
- [windows/Bluey.Windows/AI/AiWindow.cs](../windows/Bluey.Windows/AI/AiWindow.cs)
- [windows/Bluey.Windows/Bluey.Windows.csproj](../windows/Bluey.Windows/Bluey.Windows.csproj)
- [windows/Bluey.Windows/NativeSmoke.cs](../windows/Bluey.Windows/NativeSmoke.cs)
- [windows/Bluey.Windows/VoicePresentation.cs](../windows/Bluey.Windows/VoicePresentation.cs)
- [windows/README.md](../windows/README.md)

# Previous F4-B HTTP-completion evidence (superseded by response repair)

2026-10-21 Asia/Karachi — continuing existing port/live-ai-voice / PR #6 from
56bcbb573a50362e2abf88c8325978e171486836. No new PR or merge.

D012 now distinguishes developer-reported independent public model/audio/WAV/Free
Tier contract verification from still-unverified user account eligibility and real
provider/device behavior. All six official Google pages remain proxy403 locally;
agent inspected official REST cookbook and python-genai sources for HTTP shape.
No real Gemini/OpenAI request, credential, microphone hardware or billing was used.

Implemented GeminiAnswers through existing IAudioAnswers/CurrentUser DPAPI Gemini
store: fixed gemini-3.8-flash HTTPS v1beta generateContent, x-goog-api-key header,
short inline audio/wav from exact RIFF/WAVE PCM16 mono24k conversion, text-only
single candidate/128 output tokens,20s timeout,330000-byte request/16384-byte response
bounds. No redirects, retries, tools, grounding, speech or provider/model fallback.
Strict response JSON/UTF-8/duplicates, safety, empty/tool/media output and status
handling; sanitized bounded text only. Owned binary buffers cleared, no raw bodies/
audio/transcripts/credentials logged or persisted. Existing stop codes map quota to
limit (Android offers offline), safety/credentials to denied, timeout to expired,
other failures to issuer. Existing F3/53 fixtures and animation code unchanged.

Production defaults OfflineSimulation with no grants. Startup/key save/provider
selection performs no cloud request. Current trusted phone/microphone/cloud consent,
readable protected key and explicit per-phone Windows cloud/Free Tier confirmation
are required. Project eligibility uncertainty must prevent authorization. UI Stop
remains responsive during key checks; grant state is rechecked afterward. Public
ContractVerified does not certify billing status; Google free-tier content terms
require informed consent. Offline mode needs no key or microphone permission.

Local observed .NET10.0.402 Link108 tests pass (61 retained+47 new fake-HTTP cases).
Initial xUnit2009 analyzer rejected a substring Assert.True; changed to Assert.Contains,
then tests passed. No assertion/suite disabled. Revised Windows/Android CI passed at the matching application commit below;
checksums and observed artifact evidence are recorded separately. Physical/live API acceptance is unverified. Original Apple/Shared,
FaceAnimator, VoiceController, ModeIndicator, Windows Core/overlay remain untouched.

One next action: perform the documented offline
physical retest with keys empty/provider OfflineSimulation and microphone denied.
No billable or free cloud test is authorized by this development task.


## Completion CI and matching artifacts

Both supplied applications are from **685a5e883ffcfdca784641d2d086a885409871cb**.
Later closeout edits change canonical documentation/tracking only. Existing
[PR #6](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/6)
remains open into main and unmerged.

| Target | Actual results | Artifacts |
| --- | --- | --- |
| [Windows39494775781](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39494775781) | Windows2025/.NET10.0.402:27 Core+108 Link tests,0 failed/skipped; WPF0 warnings/errors; self-contained win-x64;26 native checks,0 failed | [App/full SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39494775781/artifacts/12297695977), [TRX/native evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39494775781/artifacts/12298140136) |
| [Android39494775790](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39494775790) | JDK17/Gradle8.13/SDK36:102 tests,0 failed/errors/skipped; lint0 errors18 warnings; APK/signature verified | [APK/manifest/public signer](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39494775790/artifacts/12297606597), [JUnit/lint/frames](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39494775790/artifacts/12298040076) |

The push workflows tested the same exact application SHA. PR workflows39494783369
(Windows) and39494783112 (Android) also succeeded; their merge-build artifacts are
not the supplied downloads. No regression suite was disabled. Windows108 includes
47 new fake HTTP cases, plus retained61 codec/authority/actual TLS cases. Tests
cover exact WAV/request/header, allowlist/missing key, safe parsing/safety/empty/
quota/network/provider failures, UTF-8/duplicates/nontext/bounds, header/body timeout,
cancellation, existing authorization/one-response limits and revocation. Android adds
quota-stop/offline/no-retry coverage and retains all gesture/animation/F3 tests.

| Item | SHA-256 |
| --- | --- |
| Windows EXE (CI) | `0b9d42f3e83d28798916942f5fd7eb9b7b36eeae22962be0e01cc719e040202d` |
| Windows DLL (CI) | `80c247ca406e88188950fc80e6f75d499d45a748a2702fdbc922585eebae92e3` |
| Full Windows SHA256SUMS.txt (CI) | `7b564c725eb67c7c271a937635db9778c4700731f1fe36b5d1914d19f578010b` |
| Windows app ZIP (metadata) | `2c7aac08332b9d49fc1a55d85f9c9e0f42a5d636edddd7959801ee616b36a387` |
| Android APK (CI and local rehash) | `92906750e68f46f169ea707b8836c167caf99846bd1ec97b9d3505c346edf100` |
| Android SHA256SUMS.txt (local) | `e8fd455c3cf2f034bab1d8b33a00a9f451e01baf7ab51cc2e23a3f69dd7cda42` |
| Android app ZIP (metadata) | `685b6c30c2d058d597d7155d34ae80fb7b5696c858c4ee844e36a8729583cb30` |
| Android public debug signer | `c48278f165f2c5df743db2ffa3075bfe777f4d9cc4c17c627f50ac9c08c4a2bb` |

Android app/evidence downloaded: packaged APK manifest rehashed successfully and
JUnit independently parsed102/0/0/0. Windows app/evidence downloads were policy403
at productionresultssa15.blob.core.windows.net. Final Windows hashes/test/publish/
upload are observed CI evidence, not a local archive/manifest rehash. No bypass.
Windows is self-contained, unsigned development output; no runtime install needed.
No installer/signing/release was created.

The new APK signer differs from prior amendment39527af1f27b0308194f66306fb8add295c85c9bd76a83d8e7a0b77083ba87a3.
Compare against the actually installed APK before updating. A differing signer needs
a deliberate uninstall/install, losing Android Keystore identity/trust; revoke the
old Windows phone entry and explicitly pair again. Preserve Windows protected data.
No signing private key or application credential was committed/published.

No real Gemini/OpenAI request, user key, cloud test, billing configuration or physical
microphone capture occurred. Public-contract verification is developer-reported;
accessible official Google source corroborates serialization. Real account eligibility,
API behavior/audio quality/consent indicators/fidelity/latency/CPU/multi-device QA
remain unverified. Existing F3/F4-A functional acceptance stays accepted. Follow
[one offline physical sequence](../project/QA.md#f4-b-gemini-amendment--offline-and-future-free-tier-qa)
with keys empty/provider offline/microphone denied. Any real Free Tier test still
requires separate explicit user authorization.

## Exact completion files

Compared with56bcbb5;20 files. No original Apple/Shared or accepted animation source
changed. Application commit685a5e8; later evidence-only edits stay in these paths.

```text
android/app/build.gradle.kts
android/app/src/main/java/dev/bluey/port/live/LiveCoordinator.kt
android/app/src/main/java/dev/bluey/port/live/LivePanel.kt
android/app/src/test/java/dev/bluey/port/live/AudioCoordinatorTest.kt
project/IMPLEMENTATION.md
project/PROTOCOL-DATA-SCHEMA.md
project/QA.md
project/SECURITY.md
protocol/README.md
tracking/CHECKPOINTS.md
tracking/DECISIONS.md
tracking/HANDOFF.md
tracking/SESSION-LOG.md
tracking/STATUS.md
windows/Bluey.Link.Tests/GeminiTests.cs
windows/Bluey.Link/AudioAuthority.cs
windows/Bluey.Link/GeminiAnswers.cs
windows/Bluey.Windows/AI/AiWindow.cs
windows/Bluey.Windows/Connection/WindowsLinkHost.cs
windows/README.md
```

---

# Historical amendment evidence — provider prerequisite before completion

2026-10-20 UTC — existing `port/live-ai-voice`, PR #6 remains open/unmerged.
The user-authorized Gemini amendment supersedes the previous active OpenAI UI.
No real Gemini/OpenAI request, API key, microphone hardware capture or billing
configuration was used. Original Apple/Shared, FaceAnimator, VoiceController,
ModeIndicator, Windows overlay/Core and F3 trust/identity remain unchanged.

Implemented provider-independent work: D012 bounded authenticated audio turn types,
960-byte numbered PCM chunks,5s/240000-byte/250-chunk cap, strict sequence/length/
format validation,8s assembly/20s answer/60s approval bounds; current lease/consent/
Windows authorization reuse; cancellation/backpressure/zeroing/stale replies; completed
answer presentation via existing controller/chirps/captions; separate Gemini DPAPI
purpose/file and explicit Free Tier/cloud UI. Default offline; production OpenAI is
disabled on both sides, without paid fallback or reconnect requests.53 shared F4-B
fixtures cover old live messages and new audio types. Synthetic adapters only.

**Blocked/incomplete:** all four official Google audio/pricing/billing/rate-limit
URLs returned proxy403. ai.google.dev additive environment draft saved, requires
publication. Candidate gemini-3.8-flash/audio MIME/schema/free-tier eligibility remain
unverified. There is no Gemini HTTP adapter, fake HTTP schema/quota/safety suite or
live capability in this revision. UnavailableAudioAnswers prevents authorization
even if keys/checkboxes are set. Do not infer that the candidate is unavailable or
silently switch. Finish the verified adapter after documentation access; do not claim
this amendment complete. Google Free Tier project confirmation cannot prove zero
cost for arbitrary credentials; sensitive conversations are excluded from testing.

Observed locally before CI: .NET10.0.402 Core27/27 and Link61/61 pass, including actual
loopback mTLS audio-ready/approval/revocation and synthetic answer tests; WPF cross-build
0 warnings/errors. Final matching-revision CI/results are recorded below. Existing
OpenAI/F1/F2/F3/F4-A regressions are retained, not disabled. Physical QA unrun.

The completed F3 and F4-A developer-reported functional acceptance stays accepted;
unmeasured performance/latency/resource/device-matrix QA stays unverified.
[QA](../project/QA.md#f4-b-gemini-amendment--offline-and-future-free-tier-qa) supplies
one offline physical sequence and a separately authorized future free-tier procedure.

One next action: publish the saved ai.google.dev documentation-access draft in
cloud environment settings, then resume exact model/free-tier contract verification.


## Revised Android CI evidence

Application revision **5a8978a86e9dea18cb38be0c6a87b5daf176ae82**.
[Android run39463749191](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39463749191)
succeeded:101 tests,0 failures/errors/skips; lint0 errors18 warnings; APK built,
apksigner verified and artifacts uploaded. [APK/SHA256SUMS/public signer](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39463749191/artifacts/12284311183),
[JUnit/lint/frames](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39463749191/artifacts/12284470378).

- APK SHA-256 (actual CI log): `f67f55034dcfbbdf80d5a15346f458f60514f3ba424a79d7c7eedd22f4dbff4c`.
- App ZIP SHA-256 (GitHub metadata): `c6339117115d7176b83ad48e60c0a85200e0b004548e49d7d43936676e4fa056`.
- Public debug signer: `39527af1f27b0308194f66306fb8add295c85c9bd76a83d8e7a0b77083ba87a3`.

Signer differs from prior F4-B artifact14a5c577e03ba05a977e38960cd31181b4b5c5bc6bf416ffc65b64242fcfc992.
An installed app with that signer cannot be updated in place. Deliberate uninstall/
install loses Android identity/trust; revoke the old Windows entry and re-pair.
No automatic identity deletion. Preserve Windows DPAPI/trust data.

Both final Android archive downloads were proxy-blocked at
productionresultssa9.blob.core.windows.net. CI logs/API prove tests/signature/hashes/
upload, not a local manifest rehash. The additive environment draft now includes
that observed artifact host as well as ai.google.dev, requires publication. No bypass.
Windows run39463744341 was queued at the initial Android observation and subsequently
succeeded; its final evidence is recorded below. Earlier superseded/canceled runs are not passes.
The Android PR run39463678121 also passed101/lint18 but has a different debug signer;
do not mix its APK with the supplied manual-run artifact.

Initial local test build briefly failed CS1739 because an opt-in argument was
applied to the fake OpenAiIssuer call; corrected, then Link61 and WPF clean pass.
No assertion or existing suite was disabled. Gemini-specific HTTP/429/safety parsing
is unimplemented/unrun, not a mocked pass. Google contract/pricing/billing remain
unverified; no live call or free-tier acceptance is claimed.


## Revised Windows CI evidence and artifact integrity

The same application revision **5a8978a86e9dea18cb38be0c6a87b5daf176ae82** passed
[Windows run39463744341](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39463744341):
27 Core+61 Link tests,0 failed/skipped; actual loopback mTLS/enrollment/reconnection/
revocation regression retained; WPF build0 warnings/errors; self-contained win-x64
publish;26 native HWND/DPAPI/render checks. Includes actual CurrentUser DPAPI
Gemini/OpenAI file isolation and removal; no real credential. Offscreen rendering
is not a physical animation/click-through/DPI/performance pass.

[Windows application/full SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39463744341/artifacts/12284311803),
[TRX/native/frame evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39463744341/artifacts/12283409683).

| Item | SHA-256 from final CI logs / GitHub metadata |
| --- | --- |
| Windows EXE | `05541aebb3d04fafb5153d50daefc5e9f1e0046803f545d74d2cbf9716ee3afb` |
| Windows DLL | `6c4539d94378053f01a2b201a166737cf7ee299b33e0d508d13f5fa9c056a7fd` |
| Full Windows SHA256SUMS.txt | `f4245afedf87aafa918346b03ce84fbf97cb01d8a6201812b14b09cabd0cf687` |
| Windows app ZIP | `53f6f8c021cfc76d12f01cab2a502085c6a7d0b632bcfbac0a96d0c01b9dd982` |

Windows app/evidence downloads were policy403 at productionresultssa1; Android
app/evidence at productionresultssa9. Both observed hosts were added to the saved
additive documentation/artifact environment draft, requires publication. CI logs/
metadata prove build/test/hash/upload; no local final archive/manifest rehash was
possible. No policy bypass. Both supplied artifact names include the same exact
application SHA. Later tracking-only edits do not change application source.
No Windows runtime installation is needed for this self-contained unsigned dev build.
No installer/signing/release or merge was performed.

All code tests retain existing suites. New checks cover provider denial/defaults,
current consent/free-tier confirmation, strict audio fields and shared53 fixtures,
250 bounded chunks, short/invalid/duplicate/reordered/stale audio, backpressure,
assembly/request timeout, completion, cancellation/wiping, permission denial,
OpenAI fallback rejection, sleep/background/disconnect/withdrawal, duplicate grants,
local caption states/cleanup and existing actual mTLS/revocation. Provider-specific
Gemini HTTP/429/safety schema tests remain blocked/unrun; fake answer tests do not
establish Gemini interoperability or physical audio quality. No real Gemini/OpenAI
request or key was used; Windows constructs DeniedIssuer/UnavailableAudioAnswers,
Android rejects optional OpenAI grants by default, tests inject synthetic adapters.

## Exact amendment files

Compared with dc5d6d9; application revision5a8978a:

```text
android/app/build.gradle.kts
android/app/src/main/java/dev/bluey/port/link/DeviceLink.kt
android/app/src/main/java/dev/bluey/port/link/Wire.kt
android/app/src/main/java/dev/bluey/port/live/AudioProtocol.kt
android/app/src/main/java/dev/bluey/port/live/LiveCoordinator.kt
android/app/src/main/java/dev/bluey/port/live/LivePanel.kt
android/app/src/test/java/dev/bluey/port/link/ProtocolTest.kt
android/app/src/test/java/dev/bluey/port/live/AudioCoordinatorTest.kt
android/app/src/test/java/dev/bluey/port/live/AudioUploadTest.kt
android/app/src/test/java/dev/bluey/port/live/LiveCoordinatorTest.kt
project/IMPLEMENTATION.md
project/PROTOCOL-DATA-SCHEMA.md
project/QA.md
project/SECURITY.md
protocol/README.md
protocol/f4b-cases.jsonl
tracking/CHECKPOINTS.md
tracking/DECISIONS.md
tracking/HANDOFF.md
tracking/SESSION-LOG.md
tracking/STATUS.md
windows/Bluey.Link.Tests/AudioTests.cs
windows/Bluey.Link.Tests/LiveTests.cs
windows/Bluey.Link.Tests/ProtocolTests.cs
windows/Bluey.Link.Tests/TlsTests.cs
windows/Bluey.Link/AudioAuthority.cs
windows/Bluey.Link/AudioProtocol.cs
windows/Bluey.Link/LinkServer.cs
windows/Bluey.Link/LiveAuthority.cs
windows/Bluey.Link/Wire.cs
windows/Bluey.Windows/AI/AiWindow.cs
windows/Bluey.Windows/AI/ProtectedApiKey.cs
windows/Bluey.Windows/Connection/WindowsLinkHost.cs
windows/Bluey.Windows/NativeSmoke.cs
windows/README.md
```

---

# Previous OpenAI implementation evidence (runtime superseded below)

2026-10-20 UTC — F4-B live voice implementation on `port/live-ai-voice`.
Accepted main: **7930efd1bca650ed0358bfc56201950083ea530c**, verified through
origin/main and GitHub PR #5 merged state. Developer reports F4-A functional
physical acceptance; no new agent-operated device tests or measured performance
are inferred. F1/F2/F3/F4-A measured FPS/resources/latency/compatibility remain separate.

Both supplied apps use **`5df67442c2cb5e8967a9caca8de3aba42e4a3099`**. Later closeout edits are canonical documentation
only. [PR #6](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/6) is open into main and unmerged.
No real key, microphone capture, client-secret mint or paid OpenAI call was
performed during development/CI. Real API/device acceptance remains pending a
separate user decision; code/offline checks are complete, not a live-service pass.

## Implemented scope and source comparison

- Windows CurrentUser DPAPI master-key store with owner-only directory ACL, explicit
  Set/Replace/Remove/status and no saved-key redisplay. Corruption/missing access
  fails closed. Set/configure/host startup never calls OpenAI.
- Current trusted TLS lease/pin/Android consent ID plus explicit reviewed Windows
  cloud authorization gates issuance. One locally authorized phone; at most two
  mints per grant,30-second spacing,10-second mint deadline, requested secret TTL30s.
  No general command dispatcher. Consent flags alone cannot grant authority.
- Mini model default; full gpt-realtime-2.1 is an explicit Windows option. GA nested
  session config, text-only output, manual commit/response, no tools/transcription/
  tracing. Current SDK-generated OpenAPI docs verified endpoint/model IDs/WSS/events;
  direct web guides/pricing were proxy403. Price/access/retention not inferred.
- Native AudioRecord only for an explicitly armed/authorized foreground ready session.
  Hold-only defaults on; continuous-awake is a distinct informed opt-in. Native rate
  fallback,31-tap downsample filter, chunk-invariant PCM16LE mono24k/20ms frames,
  bounded buffers/queues. No audio files or transcripts persisted/logged.
- Existing VoiceController gestures/six states and F4-A presentation drive real
  events, local source-derived pentatonic AudioTrack chirps and Windows bubbles.
  Speaking completion waits for local chirp quiet; sleep invalidates delayed work.
  Canceled holds stop/clear hold-only capture, drop late frames/errors and never
  commit. Stale drain completion cannot stop a replacement session.
- Defaults300s/5 responses/60 input seconds/256 response tokens; configurable within
 30–600s/1–20/1–120s/16–1024. Approval duration starts on Windows. A token carries
  remaining1–600 seconds, so request delay never extends the approved deadline.
  Empty audio below100ms produces no response. Local limits are cooperative safeguards.
- Sleep/Stop/pause/background/LAN loss/revocation/withdrawal closes work, cancels
  pending privilege/caption work and requires fresh Start/current Windows approval.
  No automatic live reconnect, hidden microphone or wake-on-restart. Offline demo
  selection stops live mode. Usage is memory-only metadata, not authoritative billing.

Original Mac/iOS/Shared, Android animation/renderer and ModeIndicator math, Windows
Bluey.Core and preview assets remain unchanged. D011 and PORTING-MAP record source
semantics and deliberate differences: hold-only default, bounded explicit restart,
disabled transcript/tools/notes, deterministic three-note chirp and public native APIs.
No new framework/backend. One pinned OkHttp4.12 dependency supplies native Android WSS.

## Actual final checks and artifacts

| Target | Observed final result | Downloads |
| --- | --- | --- |
| [Windows CI39455054721](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39455054721) | Windows-2025/.NET10.0.402;27 core+45 link tests, zero failed/skipped; WPF build0 warnings/errors; self-contained win-x64 publish;23 native checks/13 offscreen frames | [App/full SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39455054721/artifacts/12279802413), [TRX/native evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39455054721/artifacts/12281500075) |
| [Android CI39455054729](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39455054729) | JDK17/Gradle8.13/SDK36;89 tests, zero failures/errors/skips; lint0 errors18 warnings; apksigner verified/debug APK; retained native reference-frame tests | [APK/SHA256SUMS/public signer](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39455054729/artifacts/12280571171), [JUnit/lint/frame evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39455054729/artifacts/12280051809) |

Tests cover current consent/desktop denial, stale mint completion, expiry/rate limits,
actual .NET loopback mutual-TLS ready versus enrollment-only and revoked requests,
no master in issued metadata, fixed GA request serialization through fake HTTP,
response ordering/duplicates/empty commit/errors/limits/cancellation, PCM frame/endianness/
resampling/chunk invariance/out-of-band filtering, hold-only versus continuous policy,
sleep/Stop/pause/disconnect/withdrawal/current capture error, canceled holds/late capture
errors/abandoned drains, offline no capture/provider factory, and all existing suites.
Shared fixtures:40 F3+27 F4-A+28 F4-B, decoded in both languages. Windows native CI
executes real CurrentUser DPAPI key set/replace/corrupt/remove in a temporary directory;
no real OpenAI credential. This does not prove another Windows user context or phone
hardware. Local `.NET` link suite passes45/45. A focused red minimum-duration test
first reproduced liveStop after one-second delay, then passed29-second token metadata
and expiry at original30-second deadline after repair.

Earlier Android failures: first reducer syntax corrected; second ran82 tests with
one outdated LAN-only permission expectation, updated to exact LAN+opt-in mic while
retaining font checks; later89-test run had one fixture firing a microphone error
after release, moved to active capture while preserving stale-error rejection tests.
Superseded canceled runs are not passes. Final warnings retain legacy NSD/nullable
compiler notices and18 lint suggestions/version/manifest/pinned-trust warnings;
new dependency availability adds one warning to F4-A's17. No assertion/test disabled.

## Artifact integrity and install continuity

These are final CI-observed hashes, not local rehashes. Final archive downloads were
policy-blocked at productionresultssa10 (Android) and productionresultssa15 (Windows).
No bypass attempted. Earlier superseded Android a65 APK/certificate/JUnit87/lint/41
frames and Windows2f4 409-file manifest/TRX/native23/13 frames were downloaded and
inspected; they are historical evidence, not the supplied final downloads. Final
builds/test totals/signature/hash/upload are observed in actual CI logs/API metadata.

| Final item | SHA-256 |
| --- | --- |
| Windows EXE | `b72db4cda81180bed0bbbeec0a1b6c2df8025706fcd80563ae5bc92704d89e1c` |
| Windows DLL | `66555d3ef3e8576ebd4e13e1f9de6f6112f5e0d8c97b3210399677c42acac197` |
| Windows full SHA256SUMS.txt | `eff605f918465a43fe7016d1acdda8d20ec6c46c9b9d4c61924574d1ce457f20` |
| Windows ZIP (GitHub metadata) | `9b1e1d2be3ca20fc396f1facb0f3090093584ea04f5fa555efa7df9bbdf3edf2` |
| Android APK | `a3cc890747f9f9e685d578608b608cd980e0574f72d372b8a115dd4cc9037290` |
| Android ZIP (GitHub metadata) | `20849f8c3132e9f687506d2159150c2ec2a41a7e622d2e6b0725834544d1eb41` |
| Android public debug signer | `14a5c577e03ba05a977e38960cd31181b4b5c5bc6bf416ffc65b64242fcfc992` |

The signer differs from delivered F4-A's054f151e74b6035f0a73d2c778250e50197c2057bcd6de327147191b0622f3f4.
An update over that signer requires deliberate uninstall/install, losing Android
identity/data and requiring explicit re-pairing; no silent identity reset was added.
Revoke obsolete peers separately. Preserve Windows DPAPI app data/executable path.
Windows folder is unsigned self-contained x64: no SDK/runtime/IDE install required.
Both manifests are packaged with the downloads. Artifacts expire after14 days.
Do not mix later PR-merge/debug-signer builds with this matching application pair.

## Remaining boundaries and exactly one next action

No physical F4-B mic/routes/interruptions/permission UX, Realtime session, chirp timing,
provider event compatibility, actual latency/FPS/resource usage or billing measured.
Official web pricing/access/retention remains unverified; a narrow doc-domain draft
was saved, not observed activated. Current official SDK docs are the verified GA
contract reference, not proof of account entitlement or live behavior.

Delivered bearer secrets may establish multiple provider sessions until expiry;
existing sessions can outlive expiry. A hostile authorized phone can ignore remote
Stop. Local session/audio/response limits and cooperative closure are not a hard
financial cap or guaranteed remote provider revocation. Review current project/org
budgets/usage notifications and their limits before any separately authorized run.
No capture/OCR/tools/notes/background services/backend or cloud database is included.

**One next action:** the developer decides whether to authorize the documented
[30-second hold-only mini-model device test](../project/QA.md#f4-b-offline-and-future-billable-qa),
after reviewing current pricing/account access/retention and the matching artifacts.
No billable session may be run automatically or inferred from this implementation.

## Exact changed files

- [.github/workflows/android.yml](../.github/workflows/android.yml)
- [.github/workflows/windows.yml](../.github/workflows/windows.yml)
- [android/app/build.gradle.kts](../android/app/build.gradle.kts)
- [android/app/src/main/AndroidManifest.xml](../android/app/src/main/AndroidManifest.xml)
- [android/app/src/main/java/dev/bluey/port/MainActivity.kt](../android/app/src/main/java/dev/bluey/port/MainActivity.kt)
- [android/app/src/main/java/dev/bluey/port/link/DeviceLink.kt](../android/app/src/main/java/dev/bluey/port/link/DeviceLink.kt)
- [android/app/src/main/java/dev/bluey/port/link/Wire.kt](../android/app/src/main/java/dev/bluey/port/link/Wire.kt)
- [android/app/src/main/java/dev/bluey/port/live/AndroidAudio.kt](../android/app/src/main/java/dev/bluey/port/live/AndroidAudio.kt)
- [android/app/src/main/java/dev/bluey/port/live/LiveAdapters.kt](../android/app/src/main/java/dev/bluey/port/live/LiveAdapters.kt)
- [android/app/src/main/java/dev/bluey/port/live/LiveCoordinator.kt](../android/app/src/main/java/dev/bluey/port/live/LiveCoordinator.kt)
- [android/app/src/main/java/dev/bluey/port/live/LivePanel.kt](../android/app/src/main/java/dev/bluey/port/live/LivePanel.kt)
- [android/app/src/main/java/dev/bluey/port/live/LiveProtocol.kt](../android/app/src/main/java/dev/bluey/port/live/LiveProtocol.kt)
- [android/app/src/main/java/dev/bluey/port/live/OpenAiSocket.kt](../android/app/src/main/java/dev/bluey/port/live/OpenAiSocket.kt)
- [android/app/src/main/java/dev/bluey/port/live/Pcm24k.kt](../android/app/src/main/java/dev/bluey/port/live/Pcm24k.kt)
- [android/app/src/main/java/dev/bluey/port/live/RealtimeEngine.kt](../android/app/src/main/java/dev/bluey/port/live/RealtimeEngine.kt)
- [android/app/src/main/java/dev/bluey/port/voice/VoiceController.kt](../android/app/src/main/java/dev/bluey/port/voice/VoiceController.kt)
- [android/app/src/test/java/dev/bluey/port/link/ProtocolTest.kt](../android/app/src/test/java/dev/bluey/port/link/ProtocolTest.kt)
- [android/app/src/test/java/dev/bluey/port/live/LiveCoordinatorTest.kt](../android/app/src/test/java/dev/bluey/port/live/LiveCoordinatorTest.kt)
- [android/app/src/test/java/dev/bluey/port/live/RealtimeTest.kt](../android/app/src/test/java/dev/bluey/port/live/RealtimeTest.kt)
- [android/app/src/test/java/dev/bluey/port/render/FaceRendererTest.kt](../android/app/src/test/java/dev/bluey/port/render/FaceRendererTest.kt)
- [android/app/src/test/java/dev/bluey/port/voice/IndicatorRenderTest.kt](../android/app/src/test/java/dev/bluey/port/voice/IndicatorRenderTest.kt)
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
- [protocol/f4b-cases.jsonl](../protocol/f4b-cases.jsonl)
- [tracking/CHECKPOINTS.md](../tracking/CHECKPOINTS.md)
- [tracking/DECISIONS.md](../tracking/DECISIONS.md)
- [tracking/HANDOFF.md](../tracking/HANDOFF.md)
- [tracking/SESSION-LOG.md](../tracking/SESSION-LOG.md)
- [tracking/STATUS.md](../tracking/STATUS.md)
- [windows/Bluey.Link.Tests/Bluey.Link.Tests.csproj](../windows/Bluey.Link.Tests/Bluey.Link.Tests.csproj)
- [windows/Bluey.Link.Tests/LiveTests.cs](../windows/Bluey.Link.Tests/LiveTests.cs)
- [windows/Bluey.Link.Tests/ProtocolTests.cs](../windows/Bluey.Link.Tests/ProtocolTests.cs)
- [windows/Bluey.Link.Tests/TlsTests.cs](../windows/Bluey.Link.Tests/TlsTests.cs)
- [windows/Bluey.Link/LinkServer.cs](../windows/Bluey.Link/LinkServer.cs)
- [windows/Bluey.Link/LiveAuthority.cs](../windows/Bluey.Link/LiveAuthority.cs)
- [windows/Bluey.Link/LiveProtocol.cs](../windows/Bluey.Link/LiveProtocol.cs)
- [windows/Bluey.Link/OpenAiIssuer.cs](../windows/Bluey.Link/OpenAiIssuer.cs)
- [windows/Bluey.Link/Wire.cs](../windows/Bluey.Link/Wire.cs)
- [windows/Bluey.Windows/AI/AiWindow.cs](../windows/Bluey.Windows/AI/AiWindow.cs)
- [windows/Bluey.Windows/AI/ProtectedApiKey.cs](../windows/Bluey.Windows/AI/ProtectedApiKey.cs)
- [windows/Bluey.Windows/Connection/WindowsLinkHost.cs](../windows/Bluey.Windows/Connection/WindowsLinkHost.cs)
- [windows/Bluey.Windows/ControlsWindow.cs](../windows/Bluey.Windows/ControlsWindow.cs)
- [windows/Bluey.Windows/NativeSmoke.cs](../windows/Bluey.Windows/NativeSmoke.cs)
- [windows/Bluey.Windows/Program.cs](../windows/Bluey.Windows/Program.cs)
- [windows/README.md](../windows/README.md)
