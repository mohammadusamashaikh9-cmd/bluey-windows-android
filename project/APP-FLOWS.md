# App flows and event transitions

Use [schema](PROTOCOL-DATA-SCHEMA.md) for wire fields and [security](SECURITY.md)
for new authorization gates. State names below from Swift are source-observed;
secure pairing/privacy transitions are port-planned.

## Connect and follow

Upstream: phone starts DNS-SD browsing, selects a remembered service name or the
first result, connects TCP, exchanges `hello`, receives `face`. It retries after
1.5 s; the desktop listener retries after 2 s. Phone backgrounding stops its link;
foregrounding restarts it. A service picker is not proof of device identity.

Port: disconnected -> discovering -> user selects device -> verifying pairing on
both screens -> trusted encrypted connection -> passive follow. Denied/expired
enrollment returns to unpaired. Known devices reconnect only after authentication;
changed keys require explicit re-enrollment. Revocation cancels pending requests.
No trusted peer leaves the face in local idle, with clear connection status.

## Face and cursor

Windows cursor engine: `following` (pointer tucked below screen, eyes track user's
mouse), `docked` (visible above phone), `pinned(point)` (fly then hold). A redirect
replans from current position/velocity. Phone gaze prefers touch override, then
fresh remote state, then idle wander; thinking overrides the chosen gaze.
The phone treats face input older than 2.5 s as stale. In upstream follow mode,
mouse idle >5 s becomes sleepy and >15 s resting when not awake. These are visual
states, distinct from stopping the mic/session.

## Wake, ask, reply, sleep

| Trigger | Observed phone transition / event | Port additions |
| --- | --- | --- |
| Double tap or desktop wake | `asleep` -> `waking`, request mic and `realtimeToken`; connect WSS -> `listening`, send `awake` | Show cloud/mic disclosure and require opt-in before capture or minting |
| Hold >=0.3 s | `beginAsk`; listening/speaking -> `asking`; asleep can wake | Provide accessible alternative and handle denial without partial active state |
| Release hold | Commit input audio; `response.create`; -> `thinking` | Guard empty/short input, duplicate releases and pending connection |
| Response text delta | -> `speaking`, local chirp; `caption` to desktop | State means replying, not spoken TTS; preserve readable captions |
| Tool call | Phone forwards `tool` with arguments; desktop replies `toolResult`; phone sends output/image to API | Per-tool authorization, schema and fresh-snapshot checks before use |
| Response done | `captionDone`, save reply; quiet returns to listening | Surface provider/permission errors and cancellation |
| Double tap/sleep | Close socket, stop audio, finish recording, end session; `asleep` | Release resources; disable auto-resume after explicit sleep |
| Connection drop while awake | Re-mint/reconnect, resume with recent local text; recording can continue upstream | Bound retry, visible disconnected state, no action replay; disclose local recording separately |

An upstream caption stays readable for
`min(max(2.8, 1.2 + wordCount * 0.32), 12) + 3` seconds after completion.
Pointing choreography holds targets at least 2.2 s and can return home after
5 s quiet; inspect `choreograph` when translating rather than approximating timers.

## Screen context and optional F5

F4: ask -> approved capture -> OCR/accessible targets + screenshot -> tool result
-> phone forwards image/text to API -> point by fresh ID or 0–1000 coordinate ->
caption positioned around target -> return home. Denied capture returns an error,
never a fabricated snapshot. Capture/upload needs explicit disclosure.

F5 actions: opt-in -> explicit user request -> validated proposed operation ->
local confirmation for consequential effects -> execute serially -> observe result.
Stop/revoke/disconnect cancels queued work and releases held input; new approval
is needed to resume. The upstream six-second stop window is not the port contract.

F5 notes: independent recording opt-in -> private bounded chunks -> authenticated
upload -> optional cloud transcription -> local session merge/export. Upstream
uses 300 s AAC chunks, per-chunk speaker letters, notes sync and retries. The port
must make retry/retention/deletion visible and not claim speaker identity across
chunks. Deleting Android records must state whether desktop copies still exist.

## F3 implemented connection flow

Open Windows Secure device connection, select its private IPv4, Start host, then
Start pairing. Android Connection accepts the OS-scanned or privately pasted full
invitation and displays the host UUID/fingerprint. Pair explicitly approves it;
TLS verifies that exact certificate before the enrollment secret is sent. Compare
the phone certificate fingerprint displayed on both screens and Approve on Windows.
Only the confirmed `ready` session supplies live face updates.

Discovery selection reconnects using an already saved identity; it never trusts a
new name. Stop pairing/expiration cancels enrollment. Revocation removes host trust
and closes the affected socket. Android Disconnect retains trust, Forget removes
it, and backgrounding closes sockets/discovery until foreground resume. Secure
retry backs off from one to fifteen seconds; authentication/protocol failures stop
retry until explicit user action. Disconnected animation retains its offline demo.
F4-A adds only allowlisted nonprivileged presentation after ready; no live F4 or
F5 privileged handler is enabled by pairing.

## F4-A offline preview

Android Offline simulation explains planned permissions, double-tap wake/sleep and
300ms hold/release. Mock captions run locally disconnected or present over an already
approved F3 connection. Connection transitions/pause/sleep cancel the current mock
turn; future connection never replays offline events. Windows existing controls offer
an Offline voice caption preview without starting a listener. Sleep does not revoke
trusted peers. Live continuous awake audio is a future separately consented flow.

## F4-B explicit live flow

Offline simulation stays independent. On Android open Live AI settings, explicitly
enable live and cloud processing, request OS microphone permission (capture stays
OFF), choose hold-only or separately informed continuous-awake, then Start. Windows
must already have an explicitly started authenticated host. Windows Live AI settings
shows the exact consenting device/pin/mode; configure protected key, model/local
limits, review pricing/budgets and explicitly authorize that selected session.

Only then double tap the face to wake. A short-lived credential establishes fixed
OpenAI WSS; session.created→session.update→validated session.updated gates readiness.
Hold-only mic starts at the accepted300ms hold; release stops/drains capture and
commits only >=100ms of buffered audio. A committed acknowledgment precedes explicit
response.create. Empty input returns listening without a response. Continuous mode
streams all awake audio, responds only to asks and has the same total-input limit.
Real text deltas drive pink speaking/local chirp and existing Windows captions.
Transcription is disabled; assistant text is labeled AI reply, not user transcript.

Sleep/Stop, consent withdrawal, provider failure, pause/background, LAN loss/revoke
close work and return asleep with no automatic restart/reconnect. Start and current
Windows authorization must be repeated. A fresh credential is acquired on that new
explicit flow; there is no indefinite token refresh loop or transcript replay.
Elapsed time and provider-reported usage metadata remain visible in memory after
stop; new Start resets the session counters. No audio/transcript file persistence.

## F4-C screen flow

Open Screen Understanding. Explicitly choose primary display or a wholly contained
visible window, Capture once, review the in-memory preview, then Analyze locally.
OCR availability and UIA targets are separate: portable OCR is blocked by the
Microsoft package-identity requirement. C-targets use visible labels/roles only,
never field values. Point by a listed ID or validated0..1000 spot; only Bluey's
vector overlay moves. Home/Stop are reachable; Ctrl+Alt+S cancels screen work.
Synthetic fixture is explicitly labeled as generated pixels/declared annotations.

For optional cloud context, select GeminiFree and Start current Android consent
before capture/review. Select the exact current consenting phone in Screen
Understanding and approve THIS screenshot separately; then authorize one Free Tier
question in existing AI settings. An existing unused grant can also receive a
separate image approval before the hold. No capture/analysis/key-save/share button
sends an API request. A completed authorized audio turn attaches at most one image
to the same Gemini request. Do not share sensitive pixels; discard/reselect a
harmless window (no automatic redaction/crop). Replacement, age60s, display/DPI/window
geometry change, sleep/Stop/background/consent change/disconnect/revocation invalidates
context. Completion erases media; a validated optional point survives only as
bounded geometry until its original expiry or Stop. Invalid target leaves an answer
without pointing. Existing pink/chirp/caption lifecycle remains F4-B.
