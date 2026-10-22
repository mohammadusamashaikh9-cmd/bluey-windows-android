# Bluey native Windows offline demo

Unofficial source-based port of Riley's Bluey. Windows 11 x64; C#/.NET 10 and WPF.
Original Apple sources are retained unchanged. F3 adds an explicitly started
authenticated local TLS host; F4-A adds offline presentation. F4-B adds opt-in
Android live voice and Windows protected credential issuance. Live AI defaults OFF;
no screen capture, OCR or input automation is implemented. The visual burst does not click anything.

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

## F3-R1: one safe physical pairing retest

The initial phone attempt failed before Windows showed approval. TCP reachability
and the failing TLS/enrollment stage were not captured, so the physical root cause
is **not established**. These builds also prevent a cancelled socket read from being treated as an active
pairing failure that could clear a newer attempt. A real loopback/coroutine
regression tests that mechanism; it does not establish the physical failure cause.
These builds add bounded in-app stage diagnostics on both
platforms, without changing D009, TLS versions, certificates or enrollment rules.
The phone and server certificate fingerprints are supposed to differ.

1. Verify both new artifact checksum files. Stop the old Windows process and
   replace its development folder at the **same executable path already allowed
   by your existing Private-profile application rule**. Keep LocalAppData/Bluey/Link
   intact; do not delete the Windows identity. If you deliberately choose a new
   folder, manually check that the existing application rule names that new EXE.
   Bluey does not create or change firewall policy.
2. Check Android APK signing continuity before updating (the artifact includes
   `SIGNING-CERT.txt`; APK signing identity is separate from both TLS identities). Independent CI runners may use different debug signers.
   If Android rejects the update because its signer differs, uninstall the old
   Bluey app and install the new APK using the phone's package installer. This
   deletes its old app data/Keystore identity. Revoke any old phone entry in the
   Windows trusted-device list, if one exists, and enroll anew; never bypass the
   signature check or restore/export private keys. No local Android SDK is needed.
3. Keep both apps foreground on the same private Wi-Fi. Start Windows secure host
   on its current Wi-Fi IPv4, then Start pairing. Scan/paste that fresh invitation;
   compare the **server** fingerprint with Android's invitation display and tap
   Pair once. Compare the **phone** fingerprint across the two approval displays
   only if Windows reaches ApprovalPending, then explicitly approve on Windows.
4. After either success or failure, immediately choose **Copy safe diagnostics**
   in each app's Connection window/panel. Share only these two text logs, plus
   Windows/Android OS and phone model, approximate attempt time, and artifact
   digests. Logs keep only the latest 96 events, in memory. They contain UTC,
   local attempt IDs, client TCP source port, fixed stages/categories and (on
   Windows) numeric native TLS error/negotiated protocol. Review/redact them before
   sharing. **Do not share the QR, full URI, clipboard invitation, a full settings
   screenshot, private keys, credentials or unrelated Logcat/event logs.**
5. Match Android `TCP_CONNECTED port=N` to Windows `TcpAccepted port=N` near the
   same UTC time (NAT can translate ports). IDs starting A/W are local IDs, not a
   shared credential. No Windows TcpAccepted means this listener did not observe
   the attempt in the retained log; it does not prove which router/firewall/path
   blocked it. `TLS_COMPLETED` on Android means local handshake completion; TLS
   1.3 may still be rejected by Windows before hello. Windows MutualTlsCompleted,
   EnrollmentValidated and ApprovalPending locate progression precisely.

If Session confirmed/SessionReady appears on both, test mouse gaze, mood and Talk
output. Otherwise stop at the recorded failure and supply those safe logs. Actual
phone Keystore/SChannel interoperability, LAN reachability and the physical root
cause remain unverified until this retest; software-key/loopback CI is separate.

## F3-R2: Keystore signing and mutual-TLS retest

The developer's R1 phone/laptop logs establish TCP acceptance, server-pin validation
and client EC alias/key/chain selection, then handshake failure before enrollment
(October 18, 21:57:00 UTC/client port 53148 and 21:59:10/53192). The code omitted
Android DIGEST_NONE authorization, required by Conscrypt's prehashed EC signing
path. New keys authorize it; this does not change TLS wire algorithms or trust.
The actual phone rejection/root cause and successful correction need this retest.

1. Verify both R2 manifests and use the two builds from the same recorded app
   commit. Replace the stopped Windows application at its existing firewall-allowed
   path; preserve LocalAppData/Bluey/Link. Keep both apps foreground on the same Wi-Fi.
   The APK includes its public SIGNING-CERT.txt. A different CI debug signer requires
   uninstall/install (deletes phone data/old key); never bypass signing checks.
2. On Android Connection choose **Test protected signing identity**. Copy its safe
   diagnostics. Expected new-key results: DIGEST_NONE_AUTHORIZED, SIGN_SHA256_PASSED
   and SIGN_PREHASH_PASSED. A preserved R1 key can report DIGEST_NONE_NOT_AUTHORIZED,
   pass SHA-256, then fail prehashed signing: this directly tests the suspected
   mechanism without a connection. Do not delete that evidence before copying it.
3. Only if the old key is incompatible, choose **Replace phone identity / forget
   host…**, explicitly confirm, revoke the old phone entry on Windows, then run
   the signing check again. Replacement stops the session, deletes the phone key
   and local saved host trust; it never silently replaces an identity or changes
   Windows trust. A fresh APK installation already generates the corrected key.
4. Start Windows host and a fresh five-minute invitation. Compare the server pin
   on Android; tap Pair once. Require Windows ServerCertificateSelected,
   ClientCertificatePresent, MutualTlsCompleted, EnrollmentValidated and
   ApprovalPending. Compare the phone pin across the approval displays, approve
   on Windows, and require SessionReady/SESSION_CONFIRMED. The phone and server
   pins differ by design. Test live mouse gaze/mood/talk only after that session.
5. Immediately copy **only Copy safe diagnostics** on both devices, review/redact,
   and return those texts with OS/model/time/build digests. Match UTC/source ports;
   local A/W IDs differ. Diagnostics v2 adds fixed signing/provider/alert categories,
   Windows HRESULT/native code when available and ClientValidationNotReached when
   TLS fails before its application callback. Unknown provider alerts stay unknown;
   no raw messages, TLS records, secrets, keys, challenge or signature are logged.
   Never share the QR/full URI, settings screenshots or unrelated system logs.

CI forces TLS 1.2 and TLS 1.3 independently, including identity rejection and full
approval/confirmed face delivery. Those are software-key tests, not evidence of
phone Keystore/SChannel interoperability. Production still enables both TLS
versions with exact pinning, mandatory mutual authentication and no downgrade.

## F3-R3: restart and saved-identity reconnection

Windows now listens on **TCP 47654** at the private IPv4 you explicitly select.
Start secure host is still required after application launch; opening the app does
not start a listener. Status/safe diagnostics say `listening=IP:47654` only after
bind succeeds. An occupied/unavailable port leaves the host stopped and reports
that conflict; close the old/conflicting listener or check the reservation, then
retry. There is no silent dynamic-port fallback or automatic firewall change.
47654 is a project development default, not an IANA reservation claim.

Android retains the saved host UUID/pin and its existing phone key. It tries the
saved endpoint, that IP at 47654 (for R2's old dynamic ports), then bounded rotating
matching DNS-SD hints. A candidate still must pass exact pinned mutual TLS and
hello/ready identity checks before a changed endpoint is saved. Ordinary TCP
refusal/timeouts while the host is stopped remain retryable; neither those nor a
rejected hint erase trust or authorize a different server. Explicit Disconnect
stops retrying. Explicit Pair still requires a fresh invitation and both approvals;
a revoked phone can re-enroll with the same key after the user authorizes it.

Use **Refresh discovery** without forgetting trust. If DHCP changed the laptop's
IP and multicast discovery is unavailable, enter its displayed private IPv4 and
port in Android and choose **Use endpoint with saved identity**. This is an
untrusted route, not re-enrollment; there is no LAN scan or pin replacement.
Stable port handles unchanged-IP restarts without discovery. Automatic discovery
success is not guaranteed on networks that block multicast; typed routing is the
fallback for IP changes. Safe logs now include the selected private endpoint and
Windows running/stopped state; review/redact LAN metadata before sharing.

One physical restart/reconnect test:

1. Verify the matching R3 artifacts/manifests and keep Windows DPAPI data intact.
   Check APK SIGNING-CERT.txt: independently signed CI debug builds may require a
   one-time uninstall/install and initial enrollment, which loses the old phone
   app data/key. This packaging limitation is separate from the restart test;
   never bypass signature checks or replace identity during restart recovery.
2. Establish an authenticated R3 session once, record both pins/host UUID and
   endpoint, and confirm gaze/mood. Exit Windows fully while Android stays foreground.
   Check Android retries/offline demo without forgetting its saved identity.
3. Reopen Windows. Confirm status says stopped before clicking Start secure host.
   Select the same Wi-Fi IPv4, start host, and confirm `listening=IP:47654`.
   Tap Android Reconnect saved host if needed. Do not start pairing, scan a QR,
   Forget, Replace identity or approve enrollment. Require SessionReady and
   SESSION_CONFIRMED and unchanged pins/UUID with resumed live gaze/mood.
4. If IP changed, use discovery refresh or type the displayed new private endpoint;
   require the same pin/UUID and confirmed session without QR. If bind failed,
   resolve its reported conflict and explicitly retry Start host; do not attribute
   Android timeouts to TLS while Windows says stopped.
5. Revoke the phone on Windows: reconnect must fail. For the separate authorized
   re-enrollment check, start a fresh invitation and approve both sides using the
   same phone key; no identity replacement is needed. Copy only both safe logs
   after the tests, with times/OS/model/build hashes; no QR/URI/keys/raw traffic.

Developer reports R2 real mTLS, gaze/mood, touch precedence, Wi-Fi reconnect and
revocation rejection passed. This R3 restart correction, DNS-SD/network behavior
and latency still require the above real-device retest. CI is separate evidence.

## F4-A offline voice presentation

Existing controls → Offline voice caption preview streams fixed text through the
existing bubble renderer, then clears after the source reading interval. It needs
no host, API key or permission. Microphone/cloud/screen consents are separately
planned and OFF; no live capture is possible. For the cross-device demo, explicitly
start the secure host and connect the updated Android app using saved F3 trust.
On Android use Offline simulation instructions, double tap wake, hold300ms, release.
Matching F4-A APK and Windows commits are required. Mock captions appear on Windows; sleep/pause/disconnect clears the remote bubble.
The old Show speech / Clear speech controls remain available for F2 comparison.
CI evidence includes a Windows voice-preview frame and Android border/face composites,
with regression tests for restoring the original remote face after local interaction.
Use the [physical QA sequence](../project/QA.md#f4-a-physical-offline-demonstration-pending).

## F4-B live AI configuration (OFF by default)

Controls → Live AI settings. Set a key once using the password field (saved values
are never displayed). Replace/Remove stop all live authorizations first. Storage is
CurrentUser DPAPI with an owner-only directory ACL under LocalAppData/Bluey/AI.
Missing/corrupt/inaccessible keys fail closed; no auto regeneration. File removal
revokes local availability, not SSD snapshots/backups or a key already stolen.

Saving a key, opening controls and starting the LAN host make no OpenAI request.
Android must Start with cloud consent/OS mic permission, then Windows displays the
current trusted device/pin and hold-only/continuous preference. Review current
OpenAI pricing and budgets; explicitly authorize that selected device/session.
Only its subsequent wake can request a30-second client secret. Default mini model,
text-only output/local chirps,300s/5 responses/60s input/256 response-token safeguards.
The full gpt-realtime-2.1 option is explicit. No auto live reconnect. Stop, sleep,
background, LAN loss/revocation withdraw current live consent. No capture or tools.

Client secrets can be reused until expiry, and sessions can outlive expiry. Local
limits/cooperative phone closure do not guarantee a hard financial cap or malicious
client revocation. Use separate OpenAI Platform project/organization budgets and
usage notifications; they are alerts/controls with their own limits, not a local
proof of spend. No paid API request is authorized for CI/development.

Hold-only touch cancellation stops capture and clears uncommitted audio without
requesting a response; continuous-awake context retains its separately opted-in policy.
Provider text completion waits for the local chirp to finish before returning the
phone to listening; sleep/Stop invalidates that delayed completion.
Late capture errors/drain completions are generation-bound and cannot stop a
replacement live session or commit an abandoned question.
Offline tests distinguish an active microphone interruption from a late error
after a stopped capture; neither can reactivate an abandoned turn.
The configured session minimum is30 seconds, counted from Windows authorization;
the token carries only remaining time (possibly below30 after network/user delay).
The phone never receives an extended deadline to compensate for that delay.

## Current PR #6 Gemini amendment

Offline simulation defaults on; OpenAI production invocation is disabled. Provider
settings manage separately named CurrentUser DPAPI Gemini/OpenAI credentials without
redisplay or connectivity tests. Gemini uses only gemini-3.8-flash generateContent,
short inline WAV and text replies; one5-second question/512 combined thinking/output tokens (low thinking)/20s per
approval. No tools, grounding, generated speech, retries or paid fallback. No API
request occurs on startup, saving a key or provider selection. User-project Free Tier
eligibility is unverified: do not authorize if uncertain or billing-enabled. Review
Google content-use terms; avoid sensitive conversations. Configure Gemini key only
on Windows, select GeminiFree BEFORE starting Android consent, then select the exact
current phone and check cloud/Free Tier confirmations before authorizing ONE question.
Stop/sleep/disconnect/revoke/background cancels work; another question needs fresh
Start/approval. Never treat local limits as an attestation of zero cost.
No API key is needed for offline QA; keep keys empty, microphone denied and provider
OfflineSimulation. See project/QA.md for the offline physical retest and a separate
future free-tier test requiring explicit user authorization. No live test ran in CI.

## F4-C Screen Understanding (stacked development slice)

Open Screen Understanding from existing controls. Default capture/cloud OFF; choose
primary display or a visible window wholly within it, Capture once, review pixels,
Analyze locally and select L/W/C or a0..1000 spot to move Bluey's overlay. No actual
mouse/input action is performed. Home or STOP/Ctrl+Alt+S remains accessible.
The explicitly marked synthetic fixture uses public generated pixels and declared
annotations; it is not a real screenshot or OCR success.

**Known deployment blocker:** Microsoft lists Windows.Media.Ocr as requiring package
identity. This unsigned portable build has none, so native OCR is explicitly
unavailable. Read-only UIA targets/spot pointing and native capture remain separate.
No package registration, installer, OCR download or external process is silently
used. Physical WGC/click-through/exclusion/DPI/quality/resource QA is still pending.

Optional cloud image context: choose GeminiFree/Start current phone consent before
capture; review exact preview, select current phone and approve that snapshot, then
authorize one question in existing AI settings. It uses the same protected Gemini
key and one image+audio request. No screenshot is sent to Android; audio-only remains
available. Screenshot consent is separate from audio permission, and Google Free
Tier/project/privacy review still applies. Discard sensitive content; no automatic
redaction/crop is claimed. No real Gemini call was used in development/CI.
