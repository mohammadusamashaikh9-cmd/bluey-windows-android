# Technical requirements

See [architecture](ARCHITECTURE.md), [schema](PROTOCOL-DATA-SCHEMA.md) and
[decisions](../tracking/DECISIONS.md). Requirements below are new port work;
source observations are identified separately.

## Proposed platform choices, not confirmed support

| Area | Proposal | Proof needed before commitment |
| --- | --- | --- |
| Windows | Native C#/.NET; evaluate WPF versus WinUI with a vector/composition renderer | Transparency, click-through, pacing, mixed DPI, capture exclusion, tray/hotkeys, packaging spike in F2 |
| Android | Kotlin, Compose Canvas/custom drawing; platform audio and NSD | Landscape crop, frame pacing, lifecycle, font rendering and LAN discovery spike in F1/F3 |
| OS/architecture | Windows 11 x64 initial candidate; Android minimum/target SDK and supported CPUs undecided | Confirm official SDK support and available physical devices; record exact matrix before build setup |
| Persistence | App-private versioned JSON initially; Windows user settings plus OS-protected secrets | Atomic writes/recovery, migration, backup exclusions, ACL/secret-store behavior |
| LAN | Authenticated TLS with locally verified device trust; DNS-SD discovery | Pairing threat review, selected platform TLS APIs, firewall/network evidence in F3 |

No SDK version, renderer package or binary dependency is selected by F0.

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
