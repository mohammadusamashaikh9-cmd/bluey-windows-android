# Bluey native Windows offline demo

Unofficial source-based port of Riley's Bluey. Windows 11 x64; C#/.NET 10 and WPF.
The original Apple and Android sources are retained unchanged. This application
is entirely offline: no networking, microphone, cloud API, capture, OCR or input
automation. The visual burst does not click anything.

## Run the development artifact (no developer SDK)

Download `bluey-windows-dev-<commit>` from the successful **Windows overlay**
GitHub Actions run linked in the PR/tracking. Extract the entire ZIP into one
folder; run `Bluey.Windows.exe`. It is **self-contained win-x64**, untrimmed,
unsigned, with the .NET 10 Windows Desktop runtime included. Keep all DLLs and
runtime files together; do not move only the EXE. No runtime/SDK, Visual Studio,
WSL, Docker or installer is required. Windows may warn about an unsigned download.
This is a development artifact, not a signed release. CI artifacts expire in 14 days.

`SHA256SUMS.txt` covers every published file except itself. From the extracted folder:

```powershell
Get-Content SHA256SUMS.txt | ForEach-Object {
    $hash, $file = $_ -split '  ', 2
    if ((Get-FileHash -LiteralPath $file -Algorithm SHA256).Hash.ToLowerInvariant() -ne $hash) {
        throw "Checksum mismatch: $file"
    }
}
```

The GitHub artifact archive digest is different from the per-file EXE digest.

## Demo controls and recovery

The separate **Bluey — offline Windows demo** window stays on the taskbar. Minimize
it while observing Bluey; restore it to show a hidden overlay, change settings or
exit. Closing this control window exits the whole application. There is no hidden
tray-only process. The overlay itself is transparent, topmost, nonactivating and
explicitly click-through using Win32 styles and messages; desktop acceptance still
needs the physical tests below.

- Ctrl+Alt+P pins Bluey at the current mouse position. The X/Y fields select a
  location in primary-monitor logical DIPs. Off-primary positions clamp to that display.
- Ctrl+Alt+D returns to the chosen home mode; **Dock** peeks above the phone,
  **Follow** tucks below the screen and produces mouse gaze data only.
- Ctrl+Alt+F toggles follow/dock home preference. Ctrl+Alt+H shows/hides all artwork.
- Ctrl+Alt+T produces the original three-second talk waveform as local data.
- Choose size 48/72/96/120 DIPs, glow, comet/string/no trail and left/center/right
  phone position. Demo mood is pure output for future F3; it does not change the
  desktop eyes into the Android face.
- Show/clear example speech, a temporary label or a purely visual ring/star burst.
  Landing automatically produces the original small nod and ring.

Shortcut conflicts appear in the control window; every command has a button
fallback. Settings are versioned nonsecret JSON in
`%LOCALAPPDATA%\BlueyPort\overlay-v1.json`; unsupported/corrupt data falls back to
defaults with a warning and is retained until a user setting is saved. Defaults
match Swift: 72 DIPs, glow off, shown, comet, phone center, following and listening.

## Develop and validate

Heavy builds belong in cloud/CI. Optional developer builds need SDK **10.0.402**
(pinned in `global.json`); run from `windows/`:

```text
dotnet test Bluey.Core.Tests -c Release --logger "trx;LogFileName=core.trx" --results-directory TestResults
dotnet build Bluey.Windows -c Release
dotnet publish Bluey.Windows -c Release -r win-x64 --self-contained true -p:PublishTrimmed=false -o artifacts/bluey-win-x64
```

Pure tests run on Linux or Windows. Linux WPF cross-compilation uses the official
Windows reference pack and does not execute WPF. CI uses `windows-2025`, the pinned
SDK, actual HWND checks and the real WPF renderer. Its test-only
`Bluey.Windows.exe --self-test <output-folder>` creates/closes an actual overlay,
checks native styles/messages/show/resize properties, resolves bundled Fredoka,
and renders twelve offscreen PNG fixtures. These are renderer exports, not desktop
screenshots. Never present CI's noninteractive desktop as physical laptop QA.

Test dependencies are xUnit + Microsoft's test SDK only; the app/core use no
third-party runtime packages or second UI stack. Fonts are WPF resources linked
from `Shared/Fonts`; original OFL notices are in the artifact `Fonts/` directory.

## Source mapping and limits

`Bluey.Core/TimingCurve.cs` translates Swift `TimingCurve`/`Flight`;
`CursorEngine.cs` translates `CursorEngine`/`CursorMode` and phone gaze/mood output.
`Geometry.cs` owns exact teardrop tangent arcs, eye centers, star points, comet
widths and caption layout; `VisualState.cs` owns blink look/trail springs and timed
effects. `Bluey.Windows/OverlayArt.cs`/`VectorDrawing.cs` replace Core Animation
with native WPF vector draws and a cached body/shadow. `Native.cs`/`OverlayWindow.cs`
replace AppKit; `ControlsWindow.cs`/`SettingsStore.cs` replace menu/hotkeys/defaults.

Engine input/output contract: single UI-thread owner; monotonic seconds injected
with deterministic random draws; top-left/y-down DIPs; body transform is squish,
then rotation about the tip, then translation. The adapter maps physical cursor
coordinates through primary-monitor origin and per-window DPI. Manifest requests
PerMonitorV2; display/DPI changes refit and clamp the existing primary target.
Only the primary display is supported. Fullscreen, multiple desktops, mixed-DPI
monitor movement and display-capture exclusion are not promised. No capture API
or capture-exclusion policy is implemented in F2.

Numerical and source review evidence is in canonical tracking and
[DESIGN-FIDELITY](../project/DESIGN-FIDELITY.md). Core Animation's spring pop, blur,
text metrics, antialiasing and compositor differ from WPF and need visual
calibration. Source's 0.6-distance redirect carry cap intentionally limits speed
continuity on very short redirects; it preserves heading. Original cursor pupils
have no additional catchlight layer; the body radial shine is retained.

## Required physical Windows acceptance — UNVERIFIED

Use the exact artifact/hash, Windows build, primary resolution/refresh and laptop
power mode. Keep F1's outstanding phone performance QA separate.

1. On Windows 11 x64, run and minimize controls. Click/drag/type in normal apps
   beneath both the opaque character and transparent parts. Confirm no interception,
   focus change or overlay Alt+Tab entry. Hide/show using both hotkey and controls;
   test shortcut conflicts, recovery and exit.
2. At 100/150/200% primary DPI, test native pointer placement, corners, size,
   dock/follow, display resize/reconnect and caption top/bottom bounds. Mixed-monitor
   support remains outside acceptance.
3. Record equivalent Swift-reference states/transitions for each size/glow/trail:
   interrupted flights, capped redirects, launch/landing, press/hover, blink,
   speech appear/clear and temporary labels. Review source preview composition;
   do not infer timing from unseeded GIFs or call rendered PNGs pixel parity.
4. Test interaction with an external desktop recording tool; record whether Bluey
   appears and whether normal app capture is affected. This app performs no capture.
5. Record 60 seconds of frame times, median/p95/dropped frames and CPU/memory on the
   ~8 GB laptop, with glow and string/comet separately. 60 Hz and p95 <=16.7 ms
   are targets, not achieved claims. WPF layered-window performance needs measurement.

## Native API references

Microsoft Learn's [layered window hit-testing](https://learn.microsoft.com/en-us/windows/win32/winmsg/window-features#layered-windows)
states that WS_EX_TRANSPARENT on a layered window passes mouse events underneath.
[Extended window styles](https://learn.microsoft.com/en-us/windows/win32/winmsg/extended-window-styles)
defines NOACTIVATE and TOOLWINDOW.
[WM_DPICHANGED](https://learn.microsoft.com/en-us/windows/win32/hidpi/wm-dpichanged)
and [RegisterHotKey](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-registerhotkey)
guide DPI/refit and conflict/no-repeat behavior. These were read during F2;
the API contracts are not evidence that a particular laptop behaved correctly.

## F3 secure phone demonstration

Open **Secure device connection** in the existing control window. Refresh local
addresses, select the laptop's private IPv4 Wi-Fi/LAN address, and Start secure
host. If Windows Firewall asks, allow the application on your private home network
only. Bluey does not modify firewall rules or request administrator privileges.
Start pairing; on Android open Connection, scan the Windows QR with a trusted
system scanner (or privately paste the entire displayed URI), compare the host
fingerprint and choose Pair. Back on Windows, approve the requesting phone/cert.
The Android status must say authenticated TLS before testing live gaze/mood/talk.
Use Windows Follow, phone position, mood and Talk output to exercise synchronization.

Stop pairing invalidates the invitation; Stop host closes sessions. Trusted-device
Revoke closes that device's session and rejects reconnect. Android Forget clears
its server trust. If enrollment was interrupted at final confirmation, revoke and
forget on both sides before a fresh invitation. Leave Android foreground; background
closes its network connection. On resume it reconnects with the saved pin, without
plaintext fallback. Windows restart changes the port; DNS-SD supplies its new
endpoint, authenticated with the stored full identity. Full URI paste works when
discovery is unavailable. Initial scope is private IPv4 on the same LAN.

Development artifacts remain self-contained, unsigned Windows x64 folders; no
laptop SDK, IDE, WSL, Docker or extra desktop runtime is required. The Android APK
is a debug development build. Verify supplied SHA256SUMS before use. Physical
pairing, Wi-Fi loss/reconnect, OS-protected key behavior, latency and existing
performance measurements remain pending until recorded on the actual devices.
