---
name: bluey-device-protocol
description: Design, translate or verify Bluey local device framing, schemas, secured pairing and request lifecycle with shared fixtures and future C# and Kotlin interoperability tests.
---

# Local protocol workflow

Start with [PROTOCOL-DATA-SCHEMA](../../../project/PROTOCOL-DATA-SCHEMA.md)
and [SECURITY](../../../project/SECURITY.md). Inspect `GooglyLink`, `PhoneServer`,
`MacLink` and the command's current handler, not just Packet comments. Legacy v0
is a documentation label, not a transmitted version. DNS-SD names/hello are not
authentication. The source's 1 MiB receive argument does not cap a complete line.

Use [legacy-packets.jsonl](references/legacy-packets.jsonl) for small synthetic
positive codec examples: hello, face, correlated tool/result and escaped text.
Each line is a separate packet followed by LF; IDs and captions are public test
values, not credentials. These samples do not establish a secure envelope.

For new protocol work, decide version negotiation and pairing/TLS identity profile
before enabling privileged handlers. Bind bootstrap to verified keys, require
explicit enrollment/revocation, protect stored keys and refuse plaintext downgrade.
Do not invent cryptography. A secure port may intentionally reject legacy clients;
document that boundary instead of weakening trust for compatibility.

Define typed payload variants, byte/queue/rate limits, deadlines, structured errors,
reply correlation, cancellation and idempotency. Test byte fragmentation/coalescing,
split multibyte text, escaped newlines, EOF mid-line, invalid/oversized JSON, unknown
mood/version/command, duplicate keys and late/replayed replies. Carry the same
fixtures and expected outcomes into future C# and Kotlin tests. Add negative
fixtures alongside the specific parser change when implementation is authorized.

Recheck authority and snapshot freshness before tool effects. Never automatically
retry input actions. Bound media separately; AAC chunks cannot inherit small face
frame limits. Distinguish persisted Swift Date epoch from explicit NotesSync Unix
timestamps during migration. Report fixture parsing, actual codec tests, TLS tests
and real-device sync as separate results; no socket/API calls in foundation work.
