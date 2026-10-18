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

## D009 / 2026-10-18 — F3 authenticated local face link

Accepted F3 scope starts from F2 squash merge `a80642ab2aa897e6f80baed081ce2beb3e9a6d3d`.
Use `_bluey-sec._tcp` DNS-SD, TLS 1.2/1.3 with platform TLS and mutual certificate
proof of possession, and strict version-1 LF-JSON. Legacy `_googly._tcp` is never
used. DNS names/TXT/IP addresses are routing hints, not authentication.

Bootstrap uses a Windows-displayed QR/deep link containing the SHA-256 fingerprint
of the exact server certificate, host UUID, private IPv4 endpoint, five-minute
expiry and a CSPRNG 256-bit one-time enrollment secret. Android verifies that pin
before sending the secret. Native Android handles the QR URI from an OS scanner;
pasting the complete URI is the fallback, with identical entropy and checks.
No app camera permission/scanning dependency is needed. QRCoder is the sole new
Windows UI dependency, for enrollment QR encoding only, not character rendering.
A short numeric code, unauthenticated TOFU, plaintext compatibility, self-designed
key exchange and hosted enrollment/relay were rejected.

Android explicitly approves the displayed host before enrollment; Windows then
approves the requesting client certificate fingerprint. The client confirms that
approval on the same TLS channel before Windows persists trust and sends faces.
Certificate pinning authenticates the intended bootstrap peer against an active
LAN interceptor; client certificates bind subsequent authentication to the
Android Keystore private key. A copied enrollment QR remains a bearer invitation:
keep it private, limit attempts, expire/consume it, and inspect the requesting
fingerprint on Windows. Compromised operating systems and malicious user approval
are outside this profile. Lost confirmation may require explicit re-enrollment;
it never weakens authentication.

Windows protects its PFX and trust ledger with CurrentUser DPAPI, separately from
nonsecret overlay settings. Android uses a nonexportable P-256 Android Keystore
signing key and AES-GCM Keystore protection for pinned host metadata in no-backup
storage. Self-signed certificates are accepted only by exact pin, or temporarily
on the enrollment-only host path while an invitation is open. Missing client
certificates are explicitly rejected: .NET ClientCertificateRequired merely
requests a certificate and does not itself enforce presence. No face or other
trusted session is available before approval. Revocation cancels live sessions.

Initial scope is private IPv4 LAN, one saved Android host, at most eight trusted
phones, four concurrent sockets and one pending enrollment. Socket/TLS work and
certificate/storage operations run off render threads. Face queues hold only the
latest sample, writes/handshakes have deadlines, unchanged data refreshes every
second while the UI produces fresh input; significant thresholds remain 0.004
for gaze and 0.02 for talk. Strict 4096-byte frames, depth 4, per-direction sequence,
connection UUID and a bounded recent-ID window reject malformed/replayed data.
Face monotonic timestamps detect growing delivery lag without synchronized clocks;
Android animator receipt time and the original 2.5-second stale rule are preserved.

Official Microsoft Learn DNS service registration, TLS and DPAPI documentation
and Android NSD/Keystore/local-network permission documentation were read. SDK 36
remains pinned: INTERNET is needed; Android 16 local-network restrictions are
opt-in and temporarily use NEARBY_WIFI_DEVICES. Android 17 enforces
ACCESS_LOCAL_NETWORK for target 37+, so a future target bump requires adapting
the permission flow. F3 declares/request Nearby only when needed on API 33+;
no location, microphone, storage or camera permission. Physical permission/NSD,
Keystore/mTLS interoperability, firewall, pairing, latency and Wi-Fi loss still
require the produced artifacts on real devices. No F4/F5 endpoint exists.

### D009 F3-R1 implementation note / 2026-10-19 (Asia/Karachi)

The first reported physical pairing failure has no confirmed network/TLS root
cause. Preserve D009 without algorithm/certificate/bootstrap changes. Add only
bounded developer-facing fixed-stage diagnostics and testable extraction of the
existing EC client-alias policy. No trust-all callback, TLS downgrade, insecure
enrollment, automatic certificate replacement or firewall changes are justified.
Both apps retain at most 96 in-memory events; safe-copy includes local attempt ID,
UTC and client source port for correlation, never payload/URI/secret, exception
message, certificate subject or key material. Signing certificates of independently
built debug APKs are public metadata and may differ; verify/report their SHA-256,
and explicitly uninstall/re-enroll when necessary instead of persisting/exporting
CI signing keys. Hardware Keystore/SChannel and physical connectivity still need
one paired-device retest with the produced artifacts.

F3-R1 additionally guards the active failure-handler boundary after a cancelled
blocking socket read: cancellation-caused IOException must not publish an active
failure or clear a newer pairing intent. A real loopback/coroutine regression
fails before the guard and passes after it. This preserves the existing lifecycle
and enrollment authority; it does not establish the physical failure's cause.

### D009 F3-R2 compatibility correction / 2026-10-19 (Asia/Karachi)

User-reported October 18 attempts at 21:57:00 UTC/53148 and 21:59:10 UTC/53192
establish TCP acceptance, exact server pin verification and EC identity selection,
then failure before mutual TLS or enrollment. This supersedes R1's unknown TCP
reachability; the actual provider rejection is still unobserved.

Source investigation confirms client generation omits DIGEST_NONE. Android's
[setDigests documentation](https://developer.android.com/reference/android/security/keystore/KeyGenParameterSpec.Builder#setDigests(java.lang.String...))
says TLS keys usually require that authorization because TLS hashes before signing.
[Conscrypt CryptoUpcalls](https://github.com/google/conscrypt/blob/master/common/src/main/java/org/conscrypt/CryptoUpcalls.java)
uses NONEwithECDSA for opaque EC TLS keys; Android Keystore's ECDSA SignatureSpi
passes KM_DIGEST_NONE to KeyMint for that operation. SHA-only authorizations cannot
permit it. Authorize DIGEST_NONE in newly generated P-256 identities alongside the
existing SHA digests. This authorizes signing a TLS-computed digest, not an unhashed
TLS signature or weaker wire algorithm. No certificate extension/EKU, TLS version,
pin, approval or storage change is justified by the current evidence.

Run local SHA256withECDSA and NONEwithECDSA(SHA-256(challenge)) sign/verify checks
using the existing opaque key/certificate off the UI thread. Never log challenge,
signature or key; report fixed outcomes and whether NONE is authorized. Existing
key permissions are immutable: never silently regenerate identity. Offer explicit
confirmed replacement and forget local host trust; tell the user to revoke the old
phone on Windows and enroll anew. The old certificate stays intact until that
choice. Test TLS 1.2/1.3 independently without a production version override.
Phone confirmation of this correction and ApprovalPending/session remain pending.

R2 Windows diagnostics use the documented
[ServerCertificateSelectionCallback](https://learn.microsoft.com/en-us/dotnet/api/system.net.security.sslserverauthenticationoptions.servercertificateselectioncallback?view=net-10.0)
to select the same stored identity and record a fixed event, without logging SNI.
[Schannel AcceptSecurityContext](https://learn.microsoft.com/en-us/windows/win32/secauthn/acceptsecuritycontext--schannel)
return categories inform numeric native-error labels; no unavailable TLS alert is
invented. Unknown/cancelled errors remain distinct and carry no provider text.
