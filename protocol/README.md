# Bluey secure link v1

`v1-cases.jsonl` is the single C#/Kotlin codec corpus: outer JSON provides a name,
expected acceptance, Base64 chunks of an actual UTF-8/LF stream, and exact decoded
message values for valid streams. Both suites
feed identical bytes through incremental framing and strict decoding. The public
`test-server.pem` is an identity-test certificate; no associated private key is
stored in the repository.

Each protected-channel message has exactly `v` (integer 1), `type`, `id` (canonical
lowercase UUID), `session` (server-generated UUID), `seq` (nonnegative Int64), and
`payload`. Each direction starts at sequence 0 and increments exactly. The server
sends `hello` first; its session binds all subsequent messages. IDs cannot repeat
within the last 256 messages. A reconnect creates a new TLS connection/session.
No version negotiation or legacy plaintext fallback is supported.

Maximum frame: 4096 bytes excluding LF, including incomplete input. Strict UTF-8,
JSON depth <=4, no duplicate keys, no unknown fields, no empty frames or truncated
EOF. Numeric fields must be JSON numbers, finite and within range. Sequence,
version and time fields use unsigned decimal integer syntax within signed Int64.
Strings are nonempty, bounded, and reject control characters. Each direction is
limited to 120 messages per one-second window. All failures close
the connection; the optional bounded `error` envelope never exposes credentials.

| Type | Exact payload |
| --- | --- |
| hello | hostId: UUID, pin: lowercase SHA-256 certificate DER hex |
| enroll | deviceId: UUID, name: <=64 characters, secret: 43 Base64URL characters |
| approve | requestId: enrollment message UUID, clientPin: certificate DER pin |
| confirm | requestId: enrollment message UUID |
| connect / ready | hostId: UUID, deviceId: UUID |
| face | gazeX/gazeY: [-1,1], mood: enum, talk: [0,1], atMs: nonnegative monotonic ms |
| ping / pong | empty object |
| error | code: denied / expired / revoked / protocol / busy |

Moods: listening, resting, thinking, talking, pointing, happy, sleepy. Face updates
are only legal from an authenticated host after `ready`. Clients send heartbeat
`ping` each second; the host accepts only `ping` after enrollment/connection.

Enrollment: pinned mutual TLS → hello → enroll → Windows approval → approve →
client confirm → persist Windows trust → ready → persist Android trust. Android's
explicit Pair action approves the displayed out-of-band host identity. No face is
sent before trust commits. An interrupted final commit can leave one side trusted;
revoke/forget and explicitly enroll again. Enrollment expires after five minutes,
allows fewer than eight attempts, one pending approval (90 seconds), and is consumed
once. Host supports eight trusted peers and four concurrent handshakes/sessions.

TLS/connect/normal frame deadlines are five seconds; writes are bounded to two
seconds by cancellation/socket close. Frame completion has one deadline, so
trickled bytes cannot extend it. Unapproved enrollment waits at most 90 seconds.
Latest face is a single slot; there is no accumulated animation queue. Sender
checks every 16 ms, retains Swift thresholds .004 gaze/.02 talk plus mood change,
and refreshes after one second only while renderer input is younger than 2.5 s.
Receiver anchors sender monotonic time against local receipt time, rejects reverse
time or >2.5 s accumulated delay (>5 s forward drift), and supplies each fresh
packet to the animator once. Existing touch precedence and 2.5-second stale logic
remain intact. The first sample has no synchronized-clock latency guarantee.

Discovery `_bluey-sec._tcp` is an endpoint hint only. Bootstrap URI format:
`bluey-secure://pair?host=PRIVATE_IPV4&port=PORT&id=HOST_UUID&pin=SHA256&secret=BASE64URL_256_BITS&exp=UNIX_SECONDS`.
The complete URI must move out of band via the displayed QR or private manual
paste. No secret is advertised in DNS-SD or stored on Android. Private IPv4 is the
initial supported scope; no IPv6, routed-public or multi-interface claim.

## F4-A presentation only

After authenticated ready, updated Android may send `presentation` with exact
`turnId,state,event,text` fields. [Canonical bounds/lifecycle](../project/PROTOCOL-DATA-SCHEMA.md#f4-a-presentation-extension-d010)
and27 shared `f4a-cases.jsonl` fixtures supplement the unchanged40 original fixtures.
No token/audio/tool/capture command is enabled. Both apps must be updated; old
F3 hosts reject the extension. The queue is32/session, total caption2048 UTF-16
units, each delta256, with existing4096-byte framing and sequence/rate checks.

## F4-B live authorization

27 additional f4b-cases fixtures are tested by both languages. Types, directions,
limits and revocation are specified in [canonical schema](../project/PROTOCOL-DATA-SCHEMA.md#f4-b-live-authorization-extension-d011).
Only short-lived client secrets cross the authenticated LAN; no master key/audio/
transcripts/tools/capture. Consent declarations alone cannot mint: desktop authorization
must match current trusted TLS lease and consent. Existing40+27 fixtures retained.
