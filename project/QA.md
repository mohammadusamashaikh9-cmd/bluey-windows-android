# QA and evidence plan

F0 is static foundation validation. F1 execution is recorded in tracking;
F2 pure/build/native CI evidence is recorded in tracking; physical F1/F2 and
wider F3–F6 acceptance remains unverified.
See [CHECKPOINTS](../tracking/CHECKPOINTS.md) for gate ownership/evidence.

## Evidence rules

Record date/time zone, commit, exact command and exit/result, tool/runtime version,
input fixture/seed, platform/device/OS, display/DPI/network setup and artifact
location/hash where relevant. Redact user data. Distinguish observed output from
manual interpretation. A checklist, mock test, CI build or screenshot is not
evidence of another tier. Link failures and state why a check was not run.

## F0 checks

- Verify local branch/HEAD/remotes/status and remote fork parent/default branch.
  Compare the initial tracked tree to the pin; document any divergence.
- Inspect canonical source owners listed in [PORTING-MAP](PORTING-MAP.md). Review
  claims about transport security, model/API assumptions, dates, animation and
  data ownership against actual code. Verify original directories unchanged.
- Enumerate canonical Markdown and skill links; resolve relative files and
  heading anchors where feasible. Check fenced blocks are not treated as links.
  External URLs are not all required to be fetched; identify what was checked.
- Parse each synthetic JSONL fixture line as strict JSON; verify fixture keys and
  valid legacy face values. This is fixture hygiene, not a working codec test.
- Check skill frontmatter (`name`, `description`, folder match, hyphenated names),
  no empty scaffold instructions, and exactly four requested skills. Use bundled
  `quick_validate.py` if its existing runtime dependencies are available; otherwise
  document the limitation and use a small dependency-free equivalent check.
- Run `git diff --check` and review the exact staged path list. After commit,
  check its diff for whitespace and verify push/PR head and base independently.

Exact foundation command results and limitations are recorded in
[SESSION-LOG](../tracking/SESSION-LOG.md). No app/build/device/CI acceptance in F0.

Reproduce the static checks from the repository root with
`python .agents/skills/bluey-checkpoint-qa/scripts/validate_foundation.py`.
The [standard-library helper](../.agents/skills/bluey-checkpoint-qa/scripts/validate_foundation.py)
checks links, the restricted two-field YAML form, strict fixture JSON, an explicit
path allowlist, preserved source/README body and baseline whitespace. It does not
fetch external URLs, compile native code or prove skill behavior. Stage reviewed
files and run it again so Git also checks whitespace in new files.

## Planned deterministic and cross-language tests

| Area | Meaningful cases |
| --- | --- |
| Face math | Known initial state and dt; spring update order; all seven moods; local/remote precedence; stale input; blink double phases; fixed RNG; bounds and large frame gaps |
| Cursor math | TimingCurve inversion/derivative endpoints; flight positions/velocity continuity; redirect mid-flight; one landing; dock/follow fade; press/hover; DPI coordinate round-trips |
| Codec | Byte-by-byte fragmentation, multiple lines/read, multibyte UTF-8 split, escaped newline, malformed JSON, missing face fields, unknown mood, empty/oversized line, invalid base64, EOF mid-frame |
| Correlation/security | Unknown/mismatched/duplicate/late replies, timeout/cancel, bounded pending work, incompatible versions, unknown commands, bad cert/key, expired enrollment, replay/revoke/downgrade |
| Storage | Explicit timestamp epochs/units, atomic crash recovery, corrupt file, migration, stable IDs, pending audio retry/idempotency, export and delete boundaries |
| Voice/context | Permission denial, empty commit, held gesture during connect, text/chirp state, late events after sleep, auth failure, offline reconnect, stale snapshot/ID and capture refusal |
| Optional control/notes | Unapproved effects, injection, secure field, cancel during awaited animation/drag, no replay on reconnect; path traversal, bounded upload and retention/delete |

Use the same language-neutral fixtures in future C# and Kotlin tests. Add negative
fixtures with explicit expected rejection; don't treat source's silent invalid-line
discard as the target security policy. Protocol fuzz tests must respect offline
scope and never contact discovered devices without an authorized test plan.

## Planned CI and artifacts

When build setup is authorized, run inexpensive lint/unit/codec tests on changes,
then native builds in suitable Windows/Android jobs. Pin runtime versions and
report test counts/failed cases. Signing secrets remain in protected CI settings;
never expose them in PR artifacts. CI configuration is future work. F6 records a
single deterministic artifact per target/config with SHA-256 and ties device QA
to that exact digest. Release/signing/upload are separate acceptance boundaries.

## Planned manual device and visual comparisons

For each capture, save source/target commit, resolution/aspect, density/DPI, font
hashes, refresh rate, device/OS, seed/input trace and time base. Obtain runnable
Apple reference recordings when possible; upstream previews are reference-only.
Use fixed frames for all moods, gaze extremes, closed/squint eyes, talk levels,
cropped body/crown, gradients and caption lengths. Overlay masks/differences and
inspect highlight/clip/anti-aliasing differences. Agree tolerances before approval.

Record transitions at 60 fps where supported: mood-change blink/hop, idle to
sleepy/resting, interrupted flight, home/return, comet/string trails, speech bubble
appear/read/clear. Compare onset, trajectory, settling and frame pacing, not only
end states. Test Android physical landscape/lifecycle and Windows 100/150/200%
DPI, hotkey conflicts and click-through. Measure CPU/battery/thermal behavior and
report conditions; F0 provides no performance measurements.

F3 additionally needs real LAN discovery/auth/reconnect and synchronized face tests
on Windows plus Android. F4 needs explicit live API authorization and current docs;
use mock boundaries until then. F5 needs stop without network/model cooperation.
File a triage record with reproduction, expected/source authority, actual result,
severity, affected gate, evidence and minimal next experiment; no synthetic success.

## Android F1 build and comparison

CI is [android.yml](../.github/workflows/android.yml): Ubuntu 24.04, JDK 17,
Gradle 8.13, SDK 36/build tools 35.0.0. From `android/` run:

```text
gradle --no-daemon --console=plain :app:testDebugUnitTest :app:lintDebug :app:assembleDebug
```

No local JDK/Gradle/SDK is required when using CI. No wrapper JAR is checked in;
the workflow downloads the exact Gradle distribution through setup-gradle. For
optional local builds, provide these exact tools and an SDK path in untracked
`android/local.properties`. Do not install Studio, emulator, WSL or Docker for F1.

Pure Kotlin JUnit cases cover numerical spring integration, first-frame/dt cap,
touch/local/remote/thinking precedence, stale state, talk smoothing/fallback,
blink phases, impulses, mood settling, deterministic random traces, cover scaling,
background-clock suspension and invalid time. Robolectric native graphics on
SDK 35 uses the actual Android Canvas renderer to produce seven fixed mood PNGs,
blink, scaling/crop cases and happy transition keyframes under
`android/app/build/renders/`. It also checks vector bounds, font notices and
the minimal F3 LAN permission allowlist. These are offscreen tests, not physical GPU tests.

Success uploads `bluey-android-debug-<commit>` with `app-debug.apk` and
`SHA256SUMS.txt`; `android-test-evidence-<commit>` contains reports/XML/renders.
Both are retained 14 days. Builds and debug signing happen in CI, not a store
release. Actual run URLs, failures, results and APK digest belong in tracking.

Phone acceptance: Android 8/API 26 or newer; install the exact debug APK/digest,
record model/OS/refresh rate/resolution and hide Demo. Exercise seven moods, gaze
extremes, talk 0/0.5/1, touch, idle wander, normal/double/sleepy blinks, happy
transition and pause/resume. Check fullscreen/cutouts/system-bar swipe and keep
screen awake only while foreground. Compare upstream previews and record a 60 s
trace; report FPS/frame times/thermal behavior from the device, not CI. On Android
16 large displays, OS orientation policy can override landscape; cover crop still
applies. No device is available to this agent; all such outcomes stay UNVERIFIED.

## Windows F2 build and desktop acceptance

See [Windows demo commands and acceptance](../windows/README.md) and
[windows.yml](../.github/workflows/windows.yml). Pure xUnit tests cover cubic
x-inversion/derivatives, trajectory/velocity, carry continuity/cap, landing-once,
mode/fade, drag timing, rotation/press/hover, deterministic blink, gaze/mood,
DPI coordinate round trips, teardrop/palette/eye/star geometry, caption bounds
and trail/effect lifetimes. Run from `windows/` using SDK 10.0.402.

CI builds WPF on Windows, runs real HWND styles/messages/nonactivation checks
and twelve actual offscreen WPF render fixtures, then publishes an unsigned
self-contained win-x64 folder plus per-file SHA-256 sums. A CI message return is
not a user click-routing test. No physical Windows click-through/focus/Alt+Tab,
100/150/200% DPI, capture interaction, visual fidelity, frame pacing, CPU or
8 GB laptop performance result may be inferred. F2 stays active until that
separate evidence is recorded. No screenshot/capture feature is implemented.


## F3 automated and physical acceptance

Both language suites execute all 40 shared `protocol/v1-cases.jsonl` cases.
Run `dotnet test windows/Bluey.Link.Tests -c Release` for real loopback mTLS
(enrollment approval/confirm, reconnect/revoke, wrong pin/secret, missing/unknown
certificate, plaintext rejection, role rejection, slot cleanup and fast latest-slot
publishing) plus deterministic codec/invitation/rate/sequence/freshness tests.
Windows CI additionally runs real CurrentUser DPAPI identity/trust reload,
persistent revocation and corruption rejection, alongside F2 regression checks.
Android CI tests the same codec corpus, bootstrap limits, pin matching, bounded
retry schedule, and once-only fresh delivery into the unchanged animator.

Physical acceptance remains pending. Install the exact checksum-verified Windows
folder and debug APK from the same tested commit. Record laptop/phone model, OS,
Wi-Fi/router topology, private IPv4, permissions and artifact digests. Start host,
pair with the full QR/URI, compare both full fingerprints, explicitly approve on
both devices; confirm authenticated status and gaze following mouse movement.
Exercise all seven Windows-generated moods and Talk output; verify touch override,
2.5-second stale behavior, disconnect/offline demo, background/resume, host restart,
Wi-Fi disable/re-enable and device revocation. Try wrong/expired invitation and
changed server identity; no face must reach an untrusted session. Record actual
latency distribution and frame/CPU/memory impact without inferring these from CI.
Do not record invitation secrets or private keys in submitted evidence. Existing
F1/F2 measured-performance acceptance remains separate. F2 functional acceptance
is user-reported; no new physical observations are claimed by the agent.

### F3-R1 diagnostic evidence and retest

On 2026-10-19 (Asia/Karachi), the developer reported: Windows host launches/listens
on private IPv4; Wi-Fi is Private; an application-scoped inbound Private TCP allow
rule exists; Android displays a fresh invitation and Pair is explicitly tapped;
Android reports generic failure and Windows never shows pending approval. TCP
reachability was not definitively observed. These are user-reported observations,
not agent-performed device tests. Different server/client certificate pins are
expected and are not a defect.

The confirmed implementation defect was loss of failure-stage evidence: Android
collapsed all pre-approval exceptions into one message and Windows swallowed
failures without an accessible stage log. The physical network/TLS root cause
remains unknown. F3-R1 adds fixed-vocabulary bounded in-memory logs, local attempt
IDs and client source ports. It instruments pin verification, client alias/chain/
key selection, local handshake, hello/enrollment, approval and session closure.
No packet, exception message, certificate subject, secret or full URI is logged.
A source-level cancellation defect is also repaired: a cancelled blocking socket
read could raise IOException, allowing the active-error handler to clear a newer
pairing intent. A real loopback read/coroutine regression failed before the active
context guard. This mechanism is distinct from any unproven physical root cause.

[Windows instructions](../windows/README.md#f3-r1-one-safe-physical-pairing-retest)
provide the exact single-attempt paired-device retest and safe diagnostic-copy
procedure, including executable-path firewall continuity and Android debug-signer
reinstall/re-enrollment. Do not replace this with PowerShell connection-state
polling or screenshots of the invitation.

Regression coverage uses actual .NET TLS 1.2/1.3 with Android-shaped P-256 leaves
without KU/EKU, plus actual JDK JSSE handshakes through the production Android
client key manager with ephemeral **software** keys. Missing identity/pin failures,
alias selection, opaque-key nonexport, stage categorization, privacy and 96-event
bounds are checked. These tests cannot demonstrate a real Android hardware key or
Windows-to-phone interoperability. Existing motion/render/protocol/revocation and
no-downgrade tests remain required. Actual final run/artifact results are in tracking.

### F3-R2 / 2026-10-19 (Asia/Karachi)

Physical evidence supplied by the developer: two October 18 attempts correlated
at 21:57:00 UTC/53148 and 21:59:10 UTC/53192. TCP is accepted on Windows; Android
verifies the server pin and supplies EC key/chain; TLS fails before Windows client
validation/mutual-TLS completion, hello or enrollment. Firewall/QR/discovery and
different identities' fingerprints are not demonstrated causes.

Source-confirmed defect: Android generation authorizes SHA digests but not NONE,
while Conscrypt requests NONEwithECDSA for opaque-key TLS signing. Regression
ClientKeyPolicyTest failed at the old policy (43 tests, one expected failure in
Android CI 39171259060). Authorize NONE for new keys without weakening TLS hashes.
Run the actual existing-key SHA256withECDSA and prehashed NONEwithECDSA sign/verify
checks on-device. Explicitly replace an incompatible immutable key and forget local
host trust only after user confirmation; revoke its Windows entry and re-enroll.
Never export the private key, challenge/signature, URI or provider exception text.

Software tests independently exercise TLS 1.2/1.3, exact certificate presentation,
proof-of-possession/completion, EnrollmentValidated/ApprovalPending, confirmation,
face delivery, revocation and missing/untrusted identity rejection. JCA signing
checks test matching/mismatched keys; Robolectric checks the production key spec,
not actual KeyMint enforcement. Windows native error/HRESULT and fixed Android
provider/alert diagnostics are safe categories, not raw traffic or definitive alert
codes when the provider exposes none. Actual Keystore/Schannel rejection mechanism
and successful correction stay pending the [R2 physical sequence](../windows/README.md#f3-r2-keystore-signing-and-mutual-tls-retest).

### F3-R3 / 2026-10-19 UTC

User-reported R2 physical passes: authenticated mTLS pairing, live gaze and mood,
Android touch precedence, Wi-Fi reconnection and revocation rejection. The restart
video reports repeated TCP timeouts/no host discovery over 3–5 minutes; it does
not independently prove Windows listened at each attempt. Physical old/new IP/port
and the exact DNS-SD failure remain uncaptured.

Source-confirmed defects: default port-0 binding makes saved endpoints unstable;
Android has no fixed-port/manual fallback when NSD is absent. NSD start failure
keeps a non-null listener; pause while resolving keeps its busy latch, preventing
new generation resolution. Red tests reproduce the port conflict being ignored
via ephemeral bind and both discovery lifecycle failures. R3 binds TCP47654 with
explicit conflict/no fallback, preserves host-start consent, rotates bounded
pinned-identity endpoint hints, and keeps ordinary transport failures retryable.
New endpoint persistence occurs only after authenticated hello/ready checks.

Focused tests must cover a host exiting with a live authenticated client and
restarting at the default port with existing trust and no invitation; stale port,
changed loopback IP/port and wrong-pin rejection; host not listening; conflicting
port/retry; revoked identity rejection followed by explicit same-key enrollment.
Kotlin tests cover missing/late/untrusted hints, fixed-port migration, manual LAN
address validation, immutable saved UUID/pin, retry classification and stale NSD
callback generations. NSD callback simulation is not real multicast evidence.
Retain all existing animation, protected-store and protocol tests.

Retest using [the one R3 sequence](../windows/README.md#f3-r3-restart-and-saved-identity-reconnection).
Record Windows actual running/stopped endpoint alongside Android's selected route,
unchanged TLS identities, confirmed session after restart, conflict handling,
IP-change recovery and intentional revocation/re-enrollment. No new physical R3
pass or measured latency is inferred from builds/loopback/software-key tests.

## F4-A physical offline demonstration (pending)

Install matching-commit apps and verify SHA256SUMS first. Compare APK signer to the
installed APK: CI debug signers are ephemeral. If update is rejected for a differing
signer, uninstall/install loses Android data/Keystore identity and requires explicit
new enrollment; revoke the old peer. Never reset trust silently. Preserve Windows
DPAPI app data. This packaging step is separate from the F3 reconnect regression.

Start Windows host explicitly; establish saved authenticated connection. On Android,
open Offline simulation for disclosure and optional corner fallback, then close it.
Double tap the face: yellow waking → solid blue listening. Hold >=300ms: thick pulsing
blue asking. Release: purple thinking → streamed mock reply/pink speaking and Windows
bubble → blue listening → source-timed caption cleanup. Short taps must not ask.
Double tap sleep during response, pause activity and disconnect Wi-Fi separately:
no late callback may wake the session; Windows captions must clear on disconnect.
Repeat disconnected to demonstrate local states. Drag/touch wins over voice; sleep
restores fresh remote gaze/mood. Compare cutouts, corners and landscape to visible
screen and source previews; measure FPS/resources/latency separately. No mic/privacy
indicator, external provider traffic or capture permission should appear.
