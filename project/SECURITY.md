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

## F3 implemented security profile (D009, 2026-10-18)

The port uses platform TLS 1.2/1.3 with mutual certificate proof and exact SHA-256
DER pinning, never legacy `_googly._tcp` plaintext. See the executable
[wire contract](../protocol/README.md) and [D009](../tracking/DECISIONS.md).
Windows supplies a five-minute 256-bit invitation and its certificate fingerprint
through an out-of-band QR/full URI. Android must explicitly approve that identity;
Windows must explicitly approve the phone's certificate. DNS-SD names provide no
authentication. An unknown certificate is admitted to an enrollment-only TLS
session only during an active invitation; it receives no face state until the
secret, both approvals and confirmation succeed. The TLS signature proves private
key possession; checking only a JSON pin would not authenticate a peer.

Windows identity PFX and trust ledger use CurrentUser DPAPI and atomic replacement
under LocalAppData/Bluey/Link. Android's P-256 signing key is nonexportable through
Android Keystore; saved host identity/endpoint are AES-GCM encrypted using another
Keystore key in noBackupFilesDir. Backups and cleartext HTTP are disabled. The
invitation is never persisted or logged. Trust storage errors fail closed; the app
does not silently regenerate an unreadable identity. Revoke on Windows closes
active sessions and rejects reconnection. Android Forget removes saved server trust;
it does not remotely revoke the phone. After interrupted enrollment, explicitly
revoke/forget before trying again.

Possession of the complete QR/URI plus erroneous host approval can enroll a hostile
device. OS compromise, malicious QR scanner/clipboard tools and physical access to
an unlocked approving device are outside this bootstrap's protection. Use a trusted
system scanner/private transfer and compare the full displayed host fingerprint.
The custom URI scheme can be registered by other apps; manual paste into Bluey is
the fallback. Local endpoint/connection metadata remains visible to the LAN. No privileged commands are accepted. F4-A only adds the bounded presentation
allowlist below. F3 functional Keystore/SChannel pairing, Wi-Fi recovery and saved
restart reconnect are now developer-reported accepted; measured latency and
advanced device compatibility remain unverified.

Android remains targetSdk 36. Official [local network permission guidance](https://developer.android.com/privacy-and-security/local-network-permission)
requires ACCESS_LOCAL_NETWORK for target 37+ on Android 17; it is not requested by
this target-36 build. Android 16 local-network restrictions are opt-in and use
NEARBY_WIFI_DEVICES; the connection panel offers an explicit request if that opt-in
is enabled. Normal NSD/LAN use requires INTERNET; no location/camera/microphone
permission is requested. [NSD](https://developer.android.com/develop/connectivity/wifi/use-nsd)
and [Keystore](https://developer.android.com/privacy-and-security/keystore)
use platform APIs. Revisit permission behavior when raising targetSdk.

## F4-A offline consent boundary

Microphone capture, cloud processing and Windows screen capture are three separate
planned permissions, all OFF and ungrantable in F4-A. Android manifest remains LAN-only;
no audio/capture library, recorder, provider client, token handler or API-key UI exists.
The preview discloses that a future authorized live awake session may stream audio
continuously even without a hold. A simulated wake never authorizes such collection.

Future F4-B client-secret issuance must require an active authenticated F3 session,
current explicit microphone/cloud authorization, bounded rate/expiry and revocation;
Windows screen capture requires separate current authorization. Long-lived credentials
belong in Windows protected storage, never Android settings or the protocol. Storage/UI
is deferred until needed; no key required here. No minting endpoint is implemented.

Current GA boundary reference: OpenAI Realtime client secrets,
`POST /v1/realtime/client_secrets`, configurable `expires_after` and `session`.
Official [generated SDK source](https://github.com/openai/openai-python/blob/main/src/openai/resources/realtime/client_secrets.py)
was inspected2026-10-20 Asia/Karachi. Its docs state expiry controls new session creation,
and an established session can outlive that expiry: future stop/revoke must close the
session too. Direct developers.openai.com/platform.openai.com guide fetches failed in
this cloud; no beta request shape or live API call was used. Recheck current GA docs
before F4-B implementation. No secret is minted or stored by F4-A.

## F4-B credential, consent and revocation boundary

Live defaults OFF after every app launch. Windows master key is an independent
CurrentUser DPAPI blob under LocalAppData/Bluey/AI/openai.dpapi, with a current-user
owner-only directory ACL. Set refuses overwrite; Replace/Remove are explicit and
withdraw live grants first. No saved-key redisplay. Missing/corrupt/inaccessible
states fail closed. Atomic ciphertext replacement; plaintext bytes are zeroed where
owned. Managed header/token strings remain process-memory objects until collection.
Deletion removes protected files, not guaranteed SSD/snapshot copies. OS/user-account
compromise is outside CurrentUser protection; rotate a compromised key separately.

Only a ready/nonrevoked F3 TLS peer can submit live consent. Windows action approves
the reviewed current lease/pin/consent/model/limits after current billable cloud
consent; phone flags alone do not grant authority. Key configuration and host startup
make no provider request. Mint runs off render/LAN-reader threads with cancellation;
stale results are dropped after consent/authorization/connection changes. Generic
error categories only; no raw API error body, audio, transcript, key or token logging.

Android requests RECORD_AUDIO only from explicit live UI. OS permission alone does
not arm capture. Cloud consent and continuous-awake context are separate OFF opt-ins.
Hold-only defaults on. AudioRecord is constructed only for an authorized foreground
ready session when capture policy permits; pause/background/sleep/Stop/LAN loss/
revocation disables callbacks, closes provider, cancels/stops/releases audio, clears
pending turns/captions. No foreground/background recording service. Choosing offline
simulation also stops live mode. No microphone/provider adapter in offline tests.

Client secrets are bearer capabilities: TTL limits new-session creation, not session
lifetime or replay into multiple provider sessions. The cooperative phone closes on
withdrawal and local limits; a hostile previously authorized phone may ignore Stop
or reuse/exfiltrate a delivered secret until expiry. The desktop has no guaranteed
remote provider revocation mechanism in this profile. Local caps are not a hard
financial spending cap. User must review current OpenAI project budgets/usage alerts,
pricing, availability and retention before a separately authorized billable run.
No real key, mint or paid request is used in development or CI.

Official API contract inspected2026-10-20 UTC: [client-secret endpoint/docs](https://github.com/openai/openai-python/blob/main/src/openai/resources/realtime/client_secrets.py),
[TTL](https://github.com/openai/openai-python/blob/main/src/openai/types/realtime/client_secret_create_params.py),
[session/models/text modalities](https://github.com/openai/openai-python/blob/main/src/openai/types/realtime/realtime_session_create_request_param.py),
[PCM formats](https://github.com/openai/openai-python/blob/main/src/openai/types/realtime/realtime_audio_formats_param.py),
[manual commit](https://github.com/openai/openai-python/blob/main/src/openai/types/realtime/input_audio_buffer_commit_event_param.py),
[response metadata/limits](https://github.com/openai/openai-python/blob/main/src/openai/types/realtime/realtime_response_create_params_param.py),
[server events](https://github.com/openai/openai-python/blob/main/src/openai/types/realtime/realtime_server_event.py),
[WSS bearer handshake](https://github.com/openai/openai-python/blob/main/src/openai/resources/realtime/realtime.py).
These are current official generated OpenAPI docs/source, not upstream beta shapes.
Direct [guide](https://developers.openai.com/api/docs/guides/realtime) and
[pricing](https://developers.openai.com/api/docs/pricing) fetches returned proxy403;
network-domain draft saved but not activated. No price/retention/account entitlement
is inferred. Current model IDs are documented in the official SDK; actual access and
provider behavior require the later authorized test. No paid endpoint was contacted.
