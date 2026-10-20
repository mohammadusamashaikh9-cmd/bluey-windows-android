# Product requirements

Status: port-planned unless explicitly source-observed. See [TRD](TRD.md) and
[checkpoints](../tracking/CHECKPOINTS.md) for evidence and constraints.

## Purpose and audience

Bring Riley's blueberry companion experience to a Windows desktop with an Android
phone below its screen: expressive eyes on the phone, an animated desktop pointer,
and explicitly enabled voice/context assistance. This is an unofficial port with
upstream attribution, not an endorsed release or a reverse-engineering project.

## Scope

V1 comprises F1–F4: vector face and animation, transparent desktop overlay,
authenticated encrypted local device link, optional user-enabled Realtime voice,
screen capture/OCR, pointing, captions and privacy controls. Face/overlay remain
usable without API credentials; voice and cloud understanding require Internet
and a separately configured OpenAI account. No hosted project backend is required.

Deferred: F5 computer control and meeting notes, web research/report cards,
cross-platform legacy Apple interoperability, multi-phone control ownership,
multi-monitor support beyond a selected primary display, cloud sync and remote
Internet device pairing. F6 packages the accepted scope; it does not silently
promote deferred features into V1. No model availability or price is promised.

Non-goals: modifying the Apple apps, binary decompilation, copying macOS APIs into
Windows, always-on hidden surveillance, autonomous privileged action, a hosted
database/account service, or claiming exact runtime parity from source inspection.

## Stories and acceptance

| User story | Acceptance and gate |
| --- | --- |
| I see Bluey's face on a landscape phone | Vector geometry, seven moods, eye tracking, breathing, blinks and talk bounce compared at fixed states/times; F1 |
| His pointer flies to a target and returns home | Click-through overlay, following/docked/pinned modes, interruption, captions and trails pass visual and interaction checks; F2 |
| Only my paired desktop can drive my phone | Both devices confirm enrollment, authenticate on reconnect, encrypt traffic, revoke trust and reject rogue/replayed requests; F3 |
| I choose when he listens and answers | Mic/cloud disclosure and opt-in, visible awake state, double-tap wake/sleep, hold/release question, explicit stop and permission-denied flow; F4 |
| I ask about something on screen | Explicit capture permission, fresh OCR IDs or normalized coordinates, accurate pointer/bubble placement and useful denial/error feedback; F4 |
| I retain control of private data | Screenshots/transcripts are excluded from logs by default; session export/delete and recording choices are visible; F4, recording extension F5 |
| I optionally ask him to act or keep notes | Separate opt-ins, app-enforced confirmation/stop for actions, bounded recording/retry/retention and honest speaker labels; F5 only |

## Source-observed distinctions

The [upstream README](../README.md) describes Apple operation. Inspected
[LiveVoice](../iOS/Sources/LiveVoice.swift) and
[RealtimeHost](../Mac/Sources/GooglyMac/RealtimeHost.swift) use text output plus
local chirps, not spoken synthesized replies. While awake, speech becomes context;
hold release commits audio and requests a response. The phone also records chunks
for notes upstream; the port separates recording consent from live listening.
Upstream automatic discovery/connection is not authenticated pairing.

Release acceptance requires all selected gate evidence in [QA](QA.md), unresolved
limitations in release documentation and no unsupported claim of complete parity.

F4-A is an explicitly labeled offline simulation of the original voice UX: it uses
mock captions and local animation only. It does not authorize microphone collection,
cloud sessions or desktop capture; separate future permissions default OFF.

F4-B adds explicitly authorized, foreground live voice while retaining offline
simulation. Hold-only audio is the default; continuous-awake context needs a separate
informed choice. Windows owns protected credentials and current-device authorization.
No background recording, capture, tools or hosted service enters this slice.
