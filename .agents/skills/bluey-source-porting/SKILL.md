---
name: bluey-source-porting
description: Map Bluey Swift components and translate their semantics into proposed C# or Kotlin port components when source-based implementation or review is requested.
---

# Source-based translation

Read [PORTING-MAP](../../../project/PORTING-MAP.md) for the source owner and
[DECISIONS](../../../tracking/DECISIONS.md) for confirmed versus proposed choices.
Use the pinned source at `6d84d91456f3bf7ad9b31caf31f19710b0ba8328` unless the
task explicitly changes the baseline. Original Mac/iOS/Shared files are retained.

For the selected symbol, write a compact input/output contract: coordinate origin
and units, clock/epoch, state precedence, callbacks/thread ownership, defaults,
serialization and side effects. Inspect callers before deciding a field/comment
is live behavior. Packet's MP3 comment does not describe the AAC notes path;
Keychain's filename does not mean encrypted OS storage.

Translate pure math/state/codec first, behind a platform adapter. Keep numerical
integration order, easing inversion, cancellation and correlation behavior visible
in review. Replace AppKit/SwiftUI/Network/Vision/AX adapters with separately tested
native APIs; do not approximate semantics using convenient framework defaults.
Confirm target framework and allowed paths before scaffolding.

Use [schema](../../../project/PROTOCOL-DATA-SCHEMA.md) for date and wire units:
Swift local Date serialization differs from Unix timestamps in NotesSync exports.
Security improvements are deliberate divergences; preserve user-facing semantics
without carrying unauthenticated LAN privilege access into the port.

Finish with a source-symbol -> target-symbol mapping, focused deterministic
checks and named unverified platform behavior. Record material differences in
DECISIONS and keep the gate open if its device evidence is missing. Do not turn a
documentation request into target code or run upstream billable demo hooks.
