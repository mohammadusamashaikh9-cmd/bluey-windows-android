# F4-C OCR identity handoff

2026-10-26 Asia/Karachi. Existing PR7/port/screen-understanding, base
port/live-ai-voice; PR6 unchanged4b38029, both unmerged. Startff61152.
Capture/preview/VisibleBounds125%DPI/center pointing/stationary mouse/Home/discard
are developer-accepted on63d7269 and are not reopened.

D014 adds a separate self-contained OCR developer identity build and unsigned
sparse MSIX/loose manifest. Portable fallback retained. Registration/removal require
explicit approval switches, WhatIf and manually reviewed Windows Developer Mode;
no automatic certificate/policy/package action in app/CI. Original LocalAppData,
DPAPI keys/pairing/settings preserved; OCR startup guard fails closed on identity
or profile mismatch. Existing NativeAnalysis/Targets remain authoritative.

Both variants compile locally, manifest contracts pass after2initial failures;
Windows/native/Android CI will record exact evidence after source push. New
--ocr-self-test runs real native OCR only after approved registration on generated
public four-line pixels, no user capture/network. CI must not claim registered OCR.
Deterministic pending-capture/OCR Stop tests cover late results without manual
click timing. Real laptop text/word geometry remains unverified.

One next action: review artifact/instructions and, only if approving developer
registration, run the single harmless Notepad OCR/word pointing/Home/Stop/expiry
sequence in windows/packaging/README.md. Keep cloud OFF, Android unchanged and
both PRs unmerged. No Gemini call, alternate engine, input action or data reset.

# F4-C laptop capture repair handoff

2026-10-26 Asia/Karachi. PR #7 stays open/unmerged on port/screen-understanding,
base port/live-ai-voice. PR #6 remains unchanged4b38029. Tested app/CI63d7269;
JPEG correction7b35434, CI-only baseline installer pin63d7269. Primary checkout
preserved; active managed worktree contains the published repair.

Physical failure was JPEG metadata0x88982F41 after successful WGC acquisition,
not a remaining FirstFrame failure. ImageQuality Single0.72 now goes to encoder
creation. No geometry/retry/consent/backend/provider/F3/animation/Apple/Android changes.

Real Notepad capture succeeds687/265/281ms,578x557 VisibleBounds at125%DPI after
one pool recreation. Developer confirmed genuine preview and500/500 center flight
with stationary OS mouse;11 UIA targets directly observed. Portable OCR correctly
UnavailableIdentity. Developer confirmed Home/Stop clearing; direct final read
shows no snapshot and disabled actions, without independently timing Stop vs expiry.
Cloud stayed OFF; no Gemini call or screenshot upload.

Windows40226759821/40226763230 pass238unit/46native, WPF0warnings/errors.
Android40226763197 passes111tests, no failure/error/skip; APK unchanged locally.
Application artifact12611119362/evidence12611084399 downloaded, ZIP hashes and all
413 app files verified. Local app folder is primary checkout's ignored
build/f4c-native-debug/ci-63d7269/app. Full links/hashes and failed setup evidence
are in STATUS/CHECKPOINTS. No local .NET SDK/Visual Studio installed.

One next task: explicitly test Stop during active Notepad capture and measure
that late pixels never republish. Keep cloud OFF, preserve identities and both
open PRs; no F5 or OCR packaging. Broader DPI/driver/performance QA stays pending.

# F4-C FirstFrame capture handoff

2026-10-25 Asia/Karachi. PR7 on port/screen-understanding remains stacked on
port/live-ai-voice (PR6 unchanged4b38029); both unmerged. Started at2fb8760.

Developer reports five physical FirstFrame/Changed failures; latest DWM578x557,
WGC=Win32594x565, DPI1.25, WindowFrame,156ms. ItemSize mapping is now demonstrated.
The source's first-callback latch rejects null and differing content alike. Exact
physical condition remains unmeasured; no working real preview claim.

Repair uses one sequential consumer, capacity-one notification queue, <=6 frame
acquisitions, <=2 Recreate calls and the existing8s deadline. No undefined buffer
pixels are copied; each frame is retired before Recreate. Final geometry comes
from accepted content plus unchanged measured window bounds. Unknown mappings
remain PreviewOnly, with coordinates/OCR/sharing blocked. Stop cannot publish a
late bitmap; native copy resources stay alive until completion. Diagnostics retain
only bounded numeric/enum frame metadata and stage/HRESULT, never private content.

Tested app f3856770710d789cb7396e6080c13d7950c4c8b2; closeout tracking-only.
Local/Windows238unit pass;45 native checks including actual Recreate setup calls,
without WGC frame acquisition. Android111pass/lint0errors18warnings; both workflows
and stacked PR checks pass. Downloaded Windows app/evidence and rehashed413 files.
EXE78d0eeb41540bcf36b616ee3e83a3d41058f10d879822ae7abd96e82d03cd483.
[Windows folder/manifest](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40222661491/artifacts/12610415501).
Matching optional Android APK/signature and all CI/log/hash links are in STATUS.
Android unchanged: keep installed APK; optional new signer differs and reinstall
would lose phone identity. Android download policy-blocked; its hash is CI-only.
Native fixture DWM=WGC306x203,Win32320x210,DPI1 does not reproduce the laptop failure.
No Gemini/OpenAI call, F5/input, OCR packaging or protected-source change. Physical
acquisition, exact first-frame condition, mapping and Stop timing remain unverified.

One next action: use the new complete Windows artifact folder, keep cloud OFF,
capture the harmless four-line Notepad window once while still; inspect preview
and safe frames. With a verified map, point500,500 and confirm only Bluey moves,
then Home/Stop/discard. Otherwise share only safe diagnostics, never private pixels.
