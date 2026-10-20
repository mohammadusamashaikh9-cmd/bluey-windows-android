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

### D009 F3-R3 endpoint recovery / 2026-10-19

The developer reports R2 physical mTLS enrollment, live gaze/mood, touch precedence,
Wi-Fi recovery and revocation rejection passed. Restart/reconnect still fails with
TCP timeouts/no discovery. The video does not independently prove Windows was
listening. Source confirms ephemeral bind plus saved endpoint and discovery-only
refresh; it cannot establish the exact physical restart IP/port or DNS failure.

Use TCP 47654 on the explicitly selected private IPv4 for production host starts.
This is a project development default, not a reserved/IANA service claim. Bind
exclusively, report a conflict and leave the host stopped; never silently choose a
new port. No auto-start/preference change: the existing Start secure host action
remains necessary. Test listeners may explicitly request port 0 for isolation.
Android tries the saved endpoint, then that IP's stable port (migrating R2 dynamic
endpoints), and bounded rotating discovery/manual LAN hints. A LAN endpoint is a
routing hint only: preserve saved host UUID/server pin and phone key, and persist
a changed address/port only after the existing pinned mTLS hello/ready checks.
IP changes with no working discovery need an explicit typed LAN endpoint; no LAN
scanning, new QR, new identity, certificate downgrade or pairing shortcut.

Retry ordinary transport failures without erasing trust; bound attempts/backoff and
rotate hints after authentication rejection without accepting them. Explicit
revocation still denies a trusted connect. A fresh authorized invitation plus both
approvals may enroll the same phone key again. Reset Android discovery failure and
stale resolver state on generation changes; offer refresh independent of trust.
No Windows DNS-SD failure cause is assumed without physical/native evidence.

R3 API semantics were checked against Microsoft Learn
[TcpListener construction](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener.-ctor?view=net-10.0)
(port0 requests an assigned port) and
[ExclusiveAddressUse](https://learn.microsoft.com/en-us/dotnet/api/system.net.sockets.tcplistener.exclusiveaddressuse?view=net-10.0)
(set before starting), plus Android's [NSD lifecycle guidance](https://developer.android.com/develop/connectivity/wifi/use-nsd).
No claimed physical Windows DNS-SD cause or universal multicast reachability.

## D010 / 2026-10-20 (Asia/Karachi) — F4-A offline voice presentation

F3 PR #4 is squash-merged at `32a38c045e6de87e9d79d44d1114a4f0c76d2b8d`. Developer
reports all specified functional pairing/gaze/mood/touch/Wi-Fi/revocation/full-app-
restart reconnection passes. Accept functional F3; keep measured latency/FPS/resources,
multi-device/OS compatibility and advanced QA open. Do not reopen F3 implementation.

F4-A is a pure monotonic Kotlin six-state offline controller, deterministic injected
mock reply events and generation-bound cancellation. Compose adapters own gestures,
public WindowInsets rounded-corner/cutout queries and Canvas. Preserve ModeIndicator
colors/borders/pulse/250ms ease-in-out; use dp for pt-equivalent units, public per-
corner radii on API31+, adjustable55dp fallback otherwise, clip cutout bounds and
keep labels inside safe insets. No private corner API. Touch > explicit local voice
> fresh authenticated remote > demo fallback; add only a data override to the existing
animator, preserving springs/vector math and remote receive/stale pipeline.

Keep protocol v1 envelope/framing/sequences/4096-byte limit. Add only phone-to-host
`presentation` after authenticated ready: UUID turnId, six-state enum, event
state/delta/done/clear/cancel, bounded text. Delta chunks <=256 UTF-16 units, total
caption <=2048; state/done/clear/cancel text empty. Only speaking carries deltas/done.
Generation/turn/session checks cancel stale work. One active presentation owner,
bounded32-message per-session client queue, no offline replay, host cleanup on
sleep/disconnect/revoke and source read time clamp(1.2+words*.32,2.8,12)+3 seconds.
This is a nonprivileged presentation allowlist, not tools/voice-token/media dispatch.
Old F3 binaries reject the extension; matching F4-A apps are required for simulation.

Microphone/cloud/screen permissions are separate planned states, all off and not
grantable here. Explain future continuous awake audio, but no AudioRecord/media/
capture/API client is created, no permission added, no audio/chirp output recorded.
Original chirps are represented by synthetic talk animation only. No key UI/store
is needed for offline testing; defer optional protected master-key setup to F4-B.
Future GA Realtime client-secret boundary must require an authenticated active F3
session plus explicit current mic/cloud authorization, request/expiry/rate bounds
and cancellation/revocation; screen consent independent. No token endpoint or fake
credential exists in F4-A. Follow current official GA client_secrets documentation
in F4-B, not upstream beta token shapes/model constants. No live/billable request.

## D011 / 2026-10-20 UTC — opt-in native live text voice

F4-A PR #5 merged at `7930efd1bca650ed0358bfc56201950083ea530c`, verified through
GitHub and origin/main. Its physical functional acceptance is developer-reported;
measured fidelity/performance remains separate. F4-B keeps the accepted controller,
indicator, face spring math, F3 TLS/trust and F4-A presentation path.

Windows owns a separate CurrentUser DPAPI master-key store and explicit, in-memory,
one-phone authorization bound to the current TLS lease, pin and Android consent ID.
Configuring a key or starting the LAN host never calls OpenAI. Android must explicitly
start live mode with OS microphone permission and cloud consent, then Windows must
review/authorize that exact active phone. Each authorization allows at most two
30-second client-secret requests, separated by 30 seconds, one active live phone,
and a bounded session deadline. Stop/revoke/disconnect cancels mint work and signals
closure. Delivered secrets cannot be reliably revoked by this local host: cooperative
phone closure and TTL are not a hard provider/billing cap. No tools/media on LAN.

Use GA `POST /v1/realtime/client_secrets`, nested realtime session config, text-only
output, PCM16 little-endian mono24kHz input, manual commit/response with VAD disabled,
no transcription/tracing/tools. Default model gpt-realtime-2.1-mini; Windows permits
gpt-realtime-2.1 as an explicit higher-cost option. Current official generated SDK
OpenAPI documentation verifies these model IDs, TTL10–7200 seconds and session shapes.
Direct official web guides/pricing are currently cloud-policy blocked; documentation
access draft saved, no pricing numbers/access entitlement inferred. Recheck official
pricing and account availability before any separately authorized billable test.

Android uses one pinned OkHttp4.12 WebSocket dependency because public Android APIs
at min26 do not supply a suitable WebSocket client. No relay or second UI framework.
Native AudioRecord starts only during authorized foreground live activity; streaming
conversion emits bounded20ms PCM frames. Hold-only capture defaults on; separate
informed continuous-awake opt-in matches source context but consumes more audio.
Manual asks only, text deltas with local synthesized chirps, no generated cloud speech.
Offline provider remains independent and never initializes microphone/network provider.

Default local safeguards:300s session,5 responses,60s total input audio,256 output
tokens/response, bounded queues and no automatic live reconnect. Windows can lower/
raise within hard local ranges (600s/20/120s/1024 maxima). Explicit user wake can
request the remaining refresh; sleep/background/disconnect stops capture/provider
and invalidates callbacks. Provider response IDs, local gesture generations and LAN
request/envelope IDs remain separate. Usage is memory-only metadata, not billing proof.
No raw audio/transcripts logged or persisted. No API requests or real keys in dev/CI.

D011 timing clarification: UI-configured session duration is30–600 seconds from
Windows authorization. Live-token sessionSeconds is remaining time,1–600, so a
30-second approval remains usable after transport/user delay without extension.
A deterministic one-second-delay regression first reproduced the rejection, then
verified29-second metadata and expiry at the original deadline.

## D012 / 2026-10-20 UTC — Gemini amendment, host-side bounded audio turns

The authorized amendment selects OfflineSimulation (default), GeminiFree (explicit
Windows selection) and dormant optional OpenAIRealtime. Gemini must keep its own
CurrentUser DPAPI credential; no Gemini credential travels to Android. Reuse D009
TLS and D011 current lease/consent/desktop authorization rather than creating a
second trust system. Hold-only PCM16LE mono24kHz turns are capped at5 seconds,
240000 bytes,250 960-byte chunks, individually under the unchanged4096-byte frame
cap. Only an authorized completed turn can reach a host-side audio-answer adapter.
Cancellation/disconnect/revocation clear buffers and invalidate delayed results.
No continuous Gemini recording, automatic retries, reconnect requests, provider
fallback, tools, grounding or generated speech. Replies are completed text, not
pretended provider streaming. Existing VoiceController/animation code remains intact.

GeminiFree requires a separately reviewed Free Tier/non-billing-project confirmation
and informed Google cloud-audio consent; arbitrary credentials cannot prove zero
cost. Quota exhaustion ends authorization and offers offline simulation. Google
free-tier content may be used to improve products; avoid sensitive conversations.
Initial budget is one answer,5 seconds input,128 output tokens,20-second request
and60-second local authorization. Delivery/revocation cannot undo submitted audio.

**Contract prerequisite currently blocked:** all four user-provided official Google
pages returned proxy403 on2026-10-20. An additive ai.google.dev environment draft
was saved, requires publication. Neither gemini-3.8-flash availability/free-tier
audio eligibility nor current request/response contract is verified. Do not guess,
silently choose another model or implement/enable a provider-specific HTTP adapter
before verification. Implement/test provider-independent bounded transfer and key/
consent isolation meanwhile; production Gemini authorization fails closed until a
verified adapter exists. Test adapters use synthetic bytes/text only. This is an
external documentation blocker, not evidence that the candidate model is absent.
The previous OpenAI adapter remains source-preserved but production-disabled for
this amendment; no paid fallback is permitted. A new SDK/plugin or hosted relay
would add complexity without resolving this prerequisite and is rejected.
