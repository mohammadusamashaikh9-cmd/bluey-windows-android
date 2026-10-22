# Technical requirements

See [architecture](ARCHITECTURE.md), [schema](PROTOCOL-DATA-SCHEMA.md) and
[decisions](../tracking/DECISIONS.md). Requirements below are new port work;
source observations are identified separately.

## Platform choices and verification boundaries

| Area | Proposal | Proof needed before commitment |
| --- | --- | --- |
| Windows | F2 selects C#/.NET 10 + WPF/Win32 vectors, Windows 11 x64; D008 | Transparency, click-through, pacing, mixed DPI, capture exclusion, tray/hotkeys, packaging spike in F2 |
| Android | F1 selected Kotlin/Compose Canvas, API 26+, compile/target 36; D007 | Landscape crop/lifecycle/font/device pacing acceptance pending; platform audio/NSD remain future F3/F4 |
| OS/architecture | Windows 11 x64 selected for F2; Android API 26+/target 36 under D007, physical matrix open | Confirm official SDK support and available physical devices; record exact matrix before build setup |
| Persistence | App-private versioned JSON initially; Windows user settings plus OS-protected secrets | Atomic writes/recovery, migration, backup exclusions, ACL/secret-store behavior |
| LAN | Authenticated TLS with locally verified device trust; DNS-SD discovery | Pairing threat review, selected platform TLS APIs, firewall/network evidence in F3 |

F0 selected no SDK or renderer package. F1 now pins versions in `android/` and
D007; its actual build/test evidence is recorded in tracking. Windows F2 is selected under D008; physical desktop acceptance remains open. API 26 is the code minimum, not a tested physical support matrix.

## Functional boundaries

Windows owns pointer state, capture/OCR/control adapters, settings, protected
long-lived credentials, token minting and optional note files. Android owns face,
gestures, mic/chirps, the Realtime client and session records. Keep pure math,
codec and state transitions independently testable; avoid shared mutable UI state
across audio, network and render threads. Android has no desktop master API key.

DNS-SD is discovery only. Handle denial, no peers, multiple names, stale services,
firewall blocks and network changes. Confirm one active trusted controller for V1.
Apply framing/backpressure/schema limits before dispatch; stale control work must
not execute after disconnect, sleep, stop or trust revocation.

## Nonfunctional acceptance targets

These are proposed budgets to measure, not observed performance:

- Target smooth 60 Hz on the recorded reference devices; report frame-time median,
  p95 and dropped frames over a 60 s trace. Start with p95 render work <=16.7 ms;
  revise only with a dated decision and evidence. Respect reduced-motion choices.
- F3 target p95 face-update transit <=100 ms on a documented local Wi-Fi setup;
  publish device/network conditions and sample count, separating rendering delay.
- UI, audio and socket operations must not block each other. Bound queues/memory,
  retry backoff, request deadlines and recording storage; stop mic on sleep.
- Test display scaling at 100/150/200%, landscape aspect ratios, permission denial,
  offline operation and lifecycle suspend/resume. Primary display is the initial
  coordinate authority; no untested multi-monitor guarantee.
- Use accessible labels and keyboard settings/stop affordances. Do not copy private
  iOS screen-corner API usage into Android. Visual parity does not excuse inaccessible controls.
- Logs contain event/error categories and correlation IDs, never secrets or raw
  screen/audio/transcript content. Authentication failures fail closed.

## Observed Apple baseline

[Package.swift](../Package.swift) declares macOS 14 and Swift tools 5.9;
[project.yml](../project.yml) declares iOS 17. These are Apple build facts, not
Windows/Android minimum versions. No target application, target CI or native
toolchain validation exists at F0. API schema, model capability, account access,
pricing and current provider retention terms require a live official-doc check
before F4 and before any billable test.

## F4-A checkpoint

D010 implements offline Kotlin voice simulation plus an authenticated v1 presentation
extension. No audio/provider/screen implementation is included. Six source modes,
300ms hold,250ms indicator transition and source pulse math are independently tested.
Future live F4 slices still require separate current consent and API review.

## F4-B native live requirements implemented

Opt-in live audio uses PCM16LE mono24kHz; AudioRecord tries24k/48k/44.1k/16k, bounded
native buffers and streaming rational resampling (31-tap Hamming FIR before downsample).
Frames are20ms/960 bytes; partial tails are discarded on stop. Realtime output is
text only; local source-derived pentatonic chirps use AudioTrack. One pinned OkHttp
4.12 dependency supplies Android WSS; transport application queues/messages are capped.
Default limits300s/5 replies/60 input seconds/256 reply tokens; user-configurable
within600s/20/120s/1024. These are cooperative safeguards, not financial guarantees.
Physical capture support/routes, API interoperability, pacing and costs unverified.

Public Android semantics checked2026-10-20 UTC: [AudioRecord](https://developer.android.com/reference/android/media/AudioRecord)
requires RECORD_AUDIO, guarantees44.1kHz support while other rates depend on device,
and exposes the configured sample rate. Native capture validates initialization and
reported rate, bounds native buffers and falls back after unsupported constructors.
[Microphone while-in-use restrictions](https://developer.android.com/develop/background-work/services/fgs/service-types#microphone)
were inspected: F4-B deliberately uses only a foreground Activity, no microphone
foreground/background service, and stops on pause. Physical routes/interruptions still
need testing. Documentation inspection is not hardware evidence.

## F4-C implemented bounds

Windows11 SDK projection TFM22000; WGC/D3D11 one-frame capture with8s deadline,
input<=16Mi pixels, JPEG<=1100px edge/768KiB at quality.72. Capture/encoding/analysis/
HTTP stay outside render callbacks. Analysis caller deadline5s; UIA450ms/depth10/
queue512/visited5000/140 controls, one native-worker slot. Native provider RPC cannot
be forcibly aborted; late results are dropped and new work cannot accumulate there.
Line400/word600 IDs are hard caps. Native OCR requires identity/installed language;
no downloads or unsupported unpackaged fallback. Snapshot TTL60s. Image-aware
request<=1600000 bytes, response<=16384 bytes, same20s/512-token/one-question F4-B
bounds; audio-only request remains330000 bytes. These are limits, not measured
physical performance/latency/FPS promises.
