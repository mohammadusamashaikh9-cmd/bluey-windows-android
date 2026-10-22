# QA and evidence plan

F0 is static foundation validation. F1 execution is recorded in tracking;
F2 pure/build/native CI evidence is recorded in tracking; physical F1/F2 and
F3 functional acceptance is developer-reported (including restart/reconnect).
Measured performance, F4-A physical QA and future live F4–F6 acceptance remain unverified.
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

<a id="f4-a-physical-offline-demonstration-pending"></a>

## F4-A physical offline demonstration (functional acceptance reported)

PR #5 merged at7930efd; the developer reports functional physical acceptance.
Retained procedure below is historical QA guidance; FPS/resources/latency and
advanced compatibility remain unmeasured. No agent-operated device test is inferred.

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

## F4-B offline and future billable QA

All automated tests use fake issuers/HTTP handlers/Realtime transports/capture devices.
No key or OpenAI endpoint connection is required. Retain F1–F4-A tests and references.
Exercise consent denial, ready/trusted versus enrollment-only/revoked peers, stale
mint replies, bounds/rates/expiry, exact GA serialization, audio little-endian/frame
conversion and chunk invariance, anti-alias filtering, event ordering/duplicates,
empty commit, response/session/audio limits, errors/cancel, explicit context modes,
pause and late callbacks. Windows CI performs actual CurrentUser DPAPI set/replace/
corrupt/remove checks. Software/fake passes do not prove phone hardware or live API.

Future device test requires a separate explicit user decision authorizing a small
billable session; do not run it as part of this task:

1. Review current official model pricing/access/retention and project budgets/usage
   notifications. Choose mini, hold-only,30 seconds,1 response,5 input seconds and
   64 response tokens. Record the authorized test limits/cost decision; no secret in
   logs/screenshots. Verify both matching artifacts/checksums/signer continuity.
2. Install safely. Different debug signers require deliberate reinstall, which loses
   Android trust/key and needs explicit re-pairing; preserve Windows DPAPI app data.
   Start LAN host explicitly. Verify accepted F3 pairing/face/offline demo first.
3. Configure key on Windows; confirm no provider request. Grant Android mic permission
   and cloud consent via live UI, Start, then authorize the reviewed phone/mode on
   Windows. Double tap wake; verify yellow→blue without mic in hold-only standby.
4. Hold past300ms and speak briefly, release; verify mic stops, thinking→streamed text,
   local chirp/pink border, authenticated Windows bubble, then source-timed cleanup.
   Empty/short holds must not create responses. Usage metadata is not a billing invoice.
5. Test sleep, Stop, pause/background, denied permission/cloud, Windows withdrawal,
   Wi-Fi loss and revocation. Require no late wake/caption/capture/reconnect. Continuous
   mode requires a distinct later opt-in/test authorization; it must never auto-enable.
6. Remove key/withdraw grants, close sessions and compare actual OpenAI usage dashboard
   after propagation. Record actual device/OS/audio route, latency, FPS/resources and
   provider categories, never raw audio/transcripts/keys/tokens. Report failures honestly.

## F4-B Gemini amendment — offline and future free-tier QA

The public Gemini HTTP contract is implemented; real API/physical acceptance is
pending. Public model/audio/free-tier facts were independently verified by the
developer; agent web access remains proxy403, accessible official Google source
verifies HTTP serialization. This does not verify a user's billing project. No API
key or microphone hardware is needed for tests. Synthetic PCM, fake Gemini HTTP/
audio-answer adapters, existing fake OpenAI HTTP/socket and actual .NET mTLS loopback
are distinct evidence. Tests cover WAV bytes/headers, exact model/header/request,
text-only bounds, malformed UTF-8/JSON/duplicates, safety/empty/tool replies, quota,
provider/network failures, missing key/wrong model, deterministic20s headers/body
timeouts, cancellation, authorization/revocation and no retries/fallback.

Offline physical sequence (no cloud authorization):

1. Compare the supplied APK's public signer with the installed app. Update only if
   matching; otherwise deliberately uninstall/reinstall (loses Android Keystore/trust),
   revoke the old phone on Windows and explicitly pair again. Preserve Windows data.
2. Install the matching-commit Windows folder; explicitly start the secure host and
   reconnect the trusted phone. Leave provider OfflineSimulation, both keys empty
   and Android microphone permission denied. Confirm gaze/moods/touch precedence.
3. Run the existing offline voice demonstration: double-tap wake, hold≥300ms,
   release; verify blue asking→purple thinking→pink reply/local chirp→blue listening,
   Windows mock caption and cleanup. Sleep/background must cancel delayed captions.
4. In Windows provider settings select GeminiFree, without entering a key. Verify
   fixed model and Free Tier review copy; without a key/current consents, attempted
   authorization remains denied and does not test connectivity. OpenAI selection
   returns to OfflineSimulation. No microphone capture/cloud request is expected.

**Future live test — requires a separate explicit user authorization; no live test
is authorized in this development task:** recheck the current model/terms and user
project eligibility in official Google documentation/AI Studio. If the fixed model
is unavailable/ineligible, stop; do not replace it or enable billing. User checks
AI Studio Free Tier and
that billing is not enabled, accepts Google content-use terms and separately grants
cloud-audio consent. Configure only a Windows protected Gemini key, choose Gemini,
start Android hold-only with while-in-use microphone permission, then authorize one
phone/question on Windows. Ask one nonsensitive question≤5 seconds; verify completed
answer/border/chirp/Windows bubble. Stop immediately, inspect only sanitized status
and quota metadata, and verify no second automatic request. Do not request billing
upgrade, use sensitive content or treat the application as a zero-cost attestation.
Physical audio quality/latency/resource usage, consent indicators, cap timing and
real Gemini model/safety/quota behavior remain unverified until that authorized test.


### F4-B response repair — one physical acceptance sequence

Codex/CI use synthetic PCM and fake HTTP exclusively. No new real Gemini/OpenAI
request is authorized by this code task. Previous physical evidence is developer-
reported: five Gemini3.8 requests, two503 errors, nonzero input/output tokens,
no proven visible answer. The F2 "pointing at this spot" bubble is demo text.
Do not classify the remaining three responses without evidence.

1. Install matching-commit artifacts, checking manifests and installed APK signer.
   Different debug signer: deliberately reinstall/re-pair/revoke the old phone;
   do not delete Windows protected data. Explicitly start host and reconnect.
2. Leave provider OfflineSimulation: open Offline simulation. Verify the permission
   rows read as statuses, the settings action opens "Gemini hold-only — opt-in"
   without capture/consent, and an offline mock reply/bubble clears normally.
3. **Only if the developer separately authorizes ONE real Free Tier request:** review
   current project eligibility/billing/content-use consent, choose GeminiFree, Start
   Android hold-only consent and authorize that exact phone once on Windows. Ask one
   nonsensitive question≤5s. Expect thinking→pink reply→listening, the actual answer
   once in Windows, then source reading-time cleanup. Do not press the F2 demo bubble.
4. If no answer, record only Windows provider status (attempt/code/ms/byte and char
   counts), Android live status and approximate UTC.503 means later manual retry;
   budget/empty means no usable text. Do not share provider bodies, keys, QR strings,
   transcripts or audio. No automatic second request is allowed. Stop after this
   single attempt; sleep/background/withdrawal must discard late captions.

Automated regressions must distinguish503/429/400/401/403,timeout/network,empty,
MAX_TOKENS without visible text,safety and completed text. Success uses one shared
synthetic trace across fake HTTP→authority→TLS→Android voice/presentation→native
WPF caption; physical display, microphone quality, provider response compatibility,
resource use and latency still require the above separately authorized observation.
The budget increases128→512 combined thinking/output tokens (up to4× potential
output allowance), with low thinking, one question and a256-character caption cap;
this is not a cost guarantee or proof of Free Tier eligibility.

## F4-C combined offline and future provider acceptance

Code/CI acceptance is separate from physical acceptance. F4-B repaired fake tests
passed, but five prior developer API requests (two503s) produced no verified genuine
Windows reply. Neither that gate nor F4-C physical functionality is accepted here.
Codex/CI use public synthetic pixels/audio/fake HTTP only; no real cloud request.

Combined later physical sequence (cloud steps require separate current user approval):

1. Use the matching Windows build; Android source/protocol unchanged, so the current
   F4-B phone APK remains compatible. Compare APK signers before replacing a debug
   APK; differing signer requires deliberate reinstall/re-pair/revocation of old
   identity. Preserve Windows protected files. Authenticate F3 and verify normal
   gaze, moods, touch priority and F4-A offline gesture/border behavior.
2. Open Screen Understanding. With cloud OFF, choose a harmless visible third-party
   window wholly inside primary display, containing known public text/buttons.
   Capture once, verify the OS indicator and accurate preview; Cancel/discard works.
3. Analyze locally. Record native OCR status honestly: this portable distribution
   reports UnavailableIdentity because Microsoft requires package identity. Do not
   mark L/W recognition passed until a supported identity deployment is available.
   Inspect available UIA C labels/roles/rectangles (no password/hidden values), and
   point at a target or a known0..1000 spot. Only Bluey's overlay moves, not the OS
   mouse. Return Home/Stop works; no action is injected. Synthetic demo is explicitly
   declared fixture annotations, not a capture/recognition pass.
4. Test100/150/200% scaling, primary offsets if available, a moved/resized/closed
   selected window and display/DPI change. Old IDs must fail; expiry60s, replacement,
   Stop/Ctrl+Alt+S and disconnect discard pixels/preview/approval and stale points.
5. Only after separately checking Free Tier/project/privacy terms and authorizing
   ONE real question: select GeminiFree, Start current phone hold-only consent,
   capture/review a harmless snapshot, select that exact phone and explicitly
   approve screen sharing, then grant one question in existing Windows AI settings.
   Ask≤5s: "Bluey, what text is visible on my screen?" No extra image/OCR call occurs.
6. Verify a genuine non-demo answer in Windows, pink reply/chirp/listening, optional
   correct target pointing and source caption cleanup. A valid answer without a
   target is allowed. Stop/sleep/background/withdrawal/revocation must discard
   pending screen context/replies; reconnect never restores approval. If it fails,
   retain only safe outcome/attempt/time/counts; separate F4-B provider failures
   from F4-C capture/identity/OCR/coordinate behavior. Do not automatically retry.

Automated evidence includes pure maps/freshness/consent/secure-field/deadline tests,
injected capture/OCR cancellation, fake multimodal HTTP/structured responses, actual
TLS revocation and existing animation/protocol tests. Native CI only uses generated
public bitmaps/read-only UIA on its own harmless window. It never captures a real
desktop/private image; WGC permission/capture fidelity and CPU/FPS remain unverified.
