# Protocol and local data model

This is the canonical contract inventory. **Legacy observed** describes the
pinned Swift implementation. **Port planned** is not wire-compatible security
already present upstream. See [security](SECURITY.md) and [flows](APP-FLOWS.md).

## Legacy observed transport

[GooglyLink.swift](../Shared/GooglyLink.swift): DNS-SD type `_googly._tcp`,
TCP listener with an assigned port advertised by the service (no fixed port in
source). `NWParameters(tls:nil,tcp:...)`, TCP no-delay/keepalive with idle 2 s,
peer-to-peer enabled. JSONEncoder emits one UTF-8 object plus byte `0x0A`.
Receive buffers fragmented/coalesced reads and extracts LF-delimited lines.
The receive call's maximum 1 MiB is a **read size, not a total frame/buffer cap**.
Invalid decoded lines are silently discarded. EOF/error cancels the connection.
No authentication, encryption, version field, signature, replay protection,
explicit error envelope or range validation is defined here.

## Legacy packet fields

Every top-level field is optional; Swift's synthesized encoder omits nil fields.
An empty object can decode. Unknown keys are ignored by synthesized Decodable;
unknown `Mood` values fail decoding. Missing required members inside a present
`face` fail decoding. No exclusive packet variant is enforced.

| Field | Swift / JSON type | Meaning |
| --- | --- | --- |
| `face` | FaceState / object | Required nested `gazeX`, `gazeY` Double, `mood` enum, `talk` Double |
| `hello` | String | Device display name; no identity proof |
| `volume` | Double | Commented 0–1 volume channel; current SoundButton controls LiveVoice volume locally |
| `command` | String | Request/event discriminator |
| `audio` | String | Base64; comment says MP3, actual `transcribe` path carries AAC `.m4a` |
| `speech` | Int | Legacy speech identifier; declared contract, not proof of active playback transport |
| `callID` | String | Correlation; MacLink.request assigns a UUID string |
| `tool` | String | Tool name for `tool` |
| `text` | String | Arguments/output encoded as text, token, caption, nested JSON or notes path |
| `image` | String | Base64 JPEG alongside tool result |

Face contract comments specify gaze x/y -1..1 (negative y looks up) and talk
0..1. Mood values are `listening`, `resting`, `thinking`, `talking`, `pointing`,
`happy`, `sleepy`; initial FaceState is `(0,0,listening,0)`. These ranges are
intent, not enforced by the source decoder.

| Observed command / direction | Content and response |
| --- | --- |
| `realtimeToken`, phone -> desktop -> phone | `callID`; reply `text` contains ephemeral token or absent on failure |
| `tool` -> `toolResult`, phone -> desktop -> phone | `tool`, JSON arguments string in `text`; result text and optional image |
| `awake`, `asleep`, phone -> desktop | Session state notifications |
| `wake`, `sleep`, desktop -> phone | Session control via RootView handler |
| `caption`, `captionDone`, phone -> desktop | Partial/final reply in `text` |
| `transcribe` -> `transcribed`, phone -> desktop -> phone | AAC base64; metadata JSON in text; response JSON segments and notesPath |
| `notes`, phone -> desktop -> phone | Session export JSON string; response desktop folder path |

`testVoice`, `playing`, `done`, `stopSpeech` and MP3 playback are mentioned in
Packet comments; the inspected current host switch does not implement those
command cases. Do not implement them merely because a comment lists them.
PhoneServer sends face changes unless below gaze deltas 0.004/talk delta 0.02,
resending at least every second when `send` is called. MacLink pending requests
complete on matching callID or disconnect, with no request deadline in source.

The tool registry in [RealtimeHost](../Mac/Sources/GooglyMac/RealtimeHost.swift)
includes look/point/stop/sleep, web research and optional input/app actions.
OCR IDs (`L`, `W`, `C` prefixes) belong to a screen snapshot; `point_at_spot`
uses a 0–1000 grid. Preserve the distinction between Realtime's function-call ID
and LAN `callID`; they are separate correlation domains.

## Port planned secure evolution

Call the source contract **legacy v0** in docs only; that value is not sent by
Swift. Define a new versioned authenticated channel for Windows/Android before
F3 implementation. Retain LF-JSON payload semantics inside TLS; advertise a
distinct secure service such as proposed `_bluey-sec._tcp`. Final service name,
TLS identity/bootstrap profile and wire envelope require an F3 decision; do not
claim legacy Apple clients can connect. No silent insecure fallback.

Proposed envelope: protocol version, message type, unique message ID, optional
reply-to ID, connection/session identity and typed payload. TLS provides channel
confidentiality/integrity; application sequencing and bounded deduplication prevent
duplicate privileged requests across reconnects. Reject unsupported versions and
unknown privileged commands with structured error codes; do not dispatch them.

Initial validation budgets to confirm with fixtures in F3: control/face line <=64
KiB UTF-8, media line <=8 MiB, bounded total queue <=16 MiB and <=32 pending
requests per peer, ordinary request deadline 15 s. Exclude F5 bulk audio until a
bounded chunk-transfer design exists (5-minute AAC base64 may exceed ordinary
limits). These numbers are new proposals, not upstream capabilities.

Validate strict UTF-8/object types, finite numbers, ranges, string/base64 sizes,
request/response pairing and allowed fields per message. Specify unknown optional
field policy per version; never accept duplicate keys or conflicting variants for
privileged messages. Bound incomplete lines before newline and close abusive peers.
Rate-limit enrollment/token minting; reject stale/replayed approval and snapshots.
Do not retry non-idempotent actions automatically. Surface timeouts and clear
pending callbacks on disconnect. [Legacy fixtures](../.agents/skills/bluey-device-protocol/references/legacy-packets.jsonl)
are synthetic public examples, not runtime test evidence or the new envelope.

## Local session and settings schemas

Observed [SessionStore](../iOS/Sources/SessionStore.swift): `sessions.json` is an
array of BlueySession (`id` UUID, `started` Date, optional `ended`, `entries`,
optional `notesPath`, optional `pending`). Entry fields: UUID `id`, kind
`heard|asked|reply|report|transcript`, `text`, `time`, optional `itemID`, `speaker`.
PendingChunk contains `index`, `started`. The unconfigured Swift JSONEncoder Date
format uses seconds since Apple's 2001 reference epoch, unlike the explicitly
assembled Unix timestamps in NotesSync wire exports. Never conflate these formats.
Crash-open sessions close at last entry/start; displayEntries suppresses rough
heard entries up to latest proper transcript time. Saves are atomic and debounced.

NotesSync export uses Unix seconds for session started/ended and entry time,
`transcribing` count, and display entries (kind/text/time/optional speaker).
`transcribe` metadata includes session UUID, index, chunk `started` and
`sessionStarted` Unix seconds. Returned segment start is a chunk-relative offset.
Desktop writes `Bluey Notes/<date> <id-prefix>/notes.md`, `session.json`, `audio/`.
Speaker letters are scoped to each chunk. No hosted database is involved.

Port Android schema proposal: `schemaVersion`, explicit UTC Unix milliseconds,
stable session/entry UUIDs, ordered entries and optional pending media references
in app-private storage. Add atomic replacement, migration, corruption recovery,
export and explicit deletion/retention. Exclude audio, tokens and sensitive text
from backups by policy; verify platform behavior. Do not migrate Swift timestamps
without recording source format. Room/database storage is optional if measured
scale justifies it, not an F0 dependency.

Windows settings proposal: versioned per-user nonsecret preferences (size, glow,
visibility, phone position, follow, captions, trail, mood), trust metadata and
separate OS-protected secrets. Use DPAPI/Credential Manager or an equivalent
reviewed OS store after API selection; do not copy upstream plaintext `keys.json`.
Ephemeral tokens remain memory-only; API request/response content is not a local
database. OpenAI Realtime schema/capabilities/model and price require live checks
at F4; source string constants are historical implementation, not availability.

## Implemented Windows–Android v1 (F3)

[protocol/README.md](../protocol/README.md) defines the exact protected-channel
schema, enrollment state machine, UUID/sequence/size/deadline limits and freshness
rules. [v1-cases.jsonl](../protocol/v1-cases.jsonl) is executed by both native-language
suites. The legacy Apple framing above remains source reference only. The port's
service identity is `_bluey-sec._tcp`; it intentionally rejects insecure legacy
clients and accepts no tool/voice/capture/action command.

## F4-A presentation extension (D010)

Version 1 adds only `presentation` after authenticated `ready`. Payload has exactly
`turnId` (canonical UUID), `state` (asleep/waking/listening/asking/thinking/speaking),
`event` (state/delta/done/clear/cancel), `text` (0–256 UTF-16 units, valid Unicode,
no control characters). Only nonempty speaking deltas carry text; done requires
speaking; cancel requires asleep. Captions total at most2048 units. Existing4096-byte
frames, sequence/session/replay checks,120 messages/sec and I/O deadlines remain.
Shared `protocol/f4a-cases.jsonl` supplements the unchanged40 F3 fixtures.

Android sends through a32-slot session-only queue on IO; disconnect destroys it.
No offline replay. Windows grants one presentation owner per authenticated session;
its reducer validates legal state/turn ordering and retires recent64 turns. An old
session cannot clear a replacement owner. Disconnect/revocation clears captions.
Interrupted streams expire15s after the last delta; complete captions use the source
`clamp(1.2 + words*0.32, 2.8, 12) + 3` seconds. No privileged message is enabled.
An older F3 host fails closed on the new type; update both apps before this demo.

## F4-B live authorization extension (D011)

Protocol v1 retains exact4096-byte newline frames and replay/session/sequence rules.
Only these typed messages are added after authenticated ready:

| Type/direction | Exact payload |
| --- | --- |
| liveConsent phone→host | consentId UUID; microphone/cloud/continuous booleans |
| liveGrant host→phone | authorizationId UUID; consentId UUID |
| liveRequest phone→host | authorizationId, consentId, requestId UUIDs |
| liveToken host→phone | authorizationId/requestId UUIDs; secret printable ASCII16–512; expiresAt Unix seconds; model allowlist; sessionSeconds30–600; responses1–20; audioSeconds1–120; maxTokens16–1024 |
| liveStop either | code enum stopped/denied/expired/limit/issuer/disconnected |

Consent is a declaration from an authenticated phone, not desktop authorization.
Windows explicitly approves its current TLS lease/pin/consent. Changed consent,
reconnect, stop and revocation invalidate approval. One phone is authorized at a
time, two mints maximum per grant with30-second spacing; mint deadline10 seconds.
Provider expiry must be future and at most60 seconds away (requested TTL30).
No master key, arbitrary provider URL, audio, tool or capture payload is accepted.
Live queues are bounded: phone LAN32, host live8; credential replies are memory-only.
`protocol/f4b-cases.jsonl` adds26 shared fixtures; old40+27 remain unchanged.

Audio goes directly from phone to fixed OpenAI WSS, never across LAN or to Windows.
Provider event IDs/response IDs, local gesture generation/turn UUID and LAN envelope/
request IDs are independent. F4-A presentation remains the sole caption path. Older
apps reject unknown live commands; update both apps before live use. Manual refresh
requires a fresh Start/current Windows approval; no automatic live reconnect loop.
