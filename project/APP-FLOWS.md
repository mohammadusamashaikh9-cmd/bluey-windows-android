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
No F4/F5 event handler is enabled by pairing.
