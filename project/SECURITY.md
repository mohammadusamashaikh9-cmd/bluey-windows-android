# Port security and privacy model

Scope: source-informed design, not a security audit or verified secure product.
See [schema](PROTOCOL-DATA-SCHEMA.md), [architecture](ARCHITECTURE.md) and [QA](QA.md).

## Assets and trust boundaries

Protect long-lived API credentials, ephemeral client tokens, mic/audio/transcripts,
screenshots/OCR/accessible field text, paired keys, action authority and local notes.
Boundaries are LAN discovery -> trusted channel, phone -> desktop privileges,
desktop/phone -> OpenAI, private storage -> export, and model/tool output -> action.
Assume hostile peers on Wi-Fi, spoofed service names, malformed/oversized traffic,
prompt injection in screen content and interrupted/crashed sessions. OS compromise
is outside V1's guarantees; least privilege and secret separation still matter.

## Source observations, not port guarantees

- [GooglyLink](../Shared/GooglyLink.swift) disables TLS and has an unbounded line
  accumulation path. [PhoneServer](../Mac/Sources/GooglyMac/PhoneServer.swift)
  admits any connecting phone and routes commands without device authentication.
- [Keychain.swift](../Mac/Sources/GooglyMac/Keychain.swift) uses directory 0700/file
  0600 permissions on JSON, not Apple Keychain encryption. Long-lived key stays on
  Mac, but ephemeral token crosses the plaintext LAN link.
- [Settings](../Mac/Sources/GooglyMac/Settings.swift) defaults computerControl true.
  Host code checks setting, Accessibility trust, six-second stop window, selected
  shortcuts and focused secure fields. Consequential-action confirmation and
  treating screen text as untrusted are in model instructions, not a universal
  application-level approval gate. Do not advertise them as enforced isolation.
- Awake mic context and 300 s recording chunks run together upstream. Desktop
  notes and audio persist; phone deletion is not a verified desktop purge.

## Planned threat controls and acceptance

| Threat | Required control | Evidence before acceptance |
| --- | --- | --- |
| Spoofed/malicious LAN peer or interception | User-initiated enrollment, verified key exchange, TLS identity pinning/authentication, confirmation on both screens, pairing timeout and throttling | F3 rogue-device, wrong-key, MITM simulation and expired-code rejection |
| Downgrade/replay/stolen trust | No plaintext fallback; version check, revocation, connection-bound deduplication; protected key material | F3 reconnect, revoked peer, repeated request and incompatible version tests |
| Memory exhaustion or token abuse | Byte/queue/rate bounds and deadlines before privileged dispatch; token mint permission only for active trusted session | F3 fuzz/oversize/slow sender and mint-throttling checks |
| Sensitive cloud upload | Separate mic/cloud and screen consent, visible active status, capture selection/preview policy, no silent recording | F4 denial/revocation tests and observed network boundary |
| Prompt injection and unapproved actions | Treat screen/model output as data; allowlisted tools; explicit local grants; app-enforced consequential-action confirmation | F5 hostile screen fixture cannot authorize sending, deleting, purchasing or entering secrets |
| Stale/canceled control work | Fresh snapshot identifiers, recheck authority immediately before effects, stop cancels queues and releases held input | F5 stop during flight/drag/await, disconnect and revoke tests |
| Path traversal/private-data leakage | Validate UUIDs/indexes, construct paths beneath approved roots, exclude private payloads from logs/artifacts, redacted exports | F5 path and retention/delete tests |

Pairing profile proposal: short-lived enrollment window, QR transfer of a desktop
public-key fingerprint plus strong one-time secret, authenticated bootstrap bound
to that key, both-screen confirmation, then stored peer identities. Review a
standard platform-supported TLS/bootstrap design; do not invent cryptography or
treat a short displayed code as sufficient entropy. Exact profile and recovery
remain an F3 design decision. No token/capture/action endpoints before trust.

## Privacy and action policy

Port defaults: passive face/overlay allowed; mic/cloud, screen sharing, recording,
and computer control off until separately enabled. Explain that Realtime sends
audio to OpenAI while awake, even between hold-to-ask questions. Permission and
consent withdrawal stop new collection/upload. Show network/reconnect and recording
state; avoid hidden indefinite capture or retry. Check current provider retention,
account controls and model/cost before live testing; no promises derived from code.

Secrets stay outside Git and ordinary settings; logs must not include raw API error
bodies that may contain private data. Store peer private keys using OS protection.
Recording requires a visible explicit retention limit; warn about remaining desktop
copies during phone deletion. Export must be deliberate and contain no credentials.

F5 must block password/code/payment entry and system lock/logout/force-quit actions,
require scoped confirmation for irreversible effects and keep a local emergency
stop reachable without model/network cooperation. Stop remains latched until the
user resumes; the source's short cool-down is not sufficient. Approval is tied to
one operation/target and expires on state changes. No unattended privileged agent.
