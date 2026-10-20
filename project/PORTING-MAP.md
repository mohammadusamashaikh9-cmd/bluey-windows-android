# Source-to-port map

Baseline `6d84d91456f3bf7ad9b31caf31f19710b0ba8328`; all observations below
come from source, not execution. Original paths remain untouched. Android F1 and Windows F2 have concrete offline implementations under D007/D008;
other target owners remain conceptual components.

| Source authority | Observed semantics to retain | Translate / replace and gate |
| --- | --- | --- |
| [README](../README.md), [LICENSE](../LICENSE) | Riley attribution, Apple instructions, Unlicense | Add clearly separated port status/navigation; F0 |
| [Shared/Palette](../Shared/Palette.swift) | Exact sRGB constants/stops | Literal C#/Kotlin constants with parity checks; F1/F2 |
| [Shared/GooglyLink](../Shared/GooglyLink.swift) | Mood/FaceState/Packet names, UTF-8 LF framing | New bounded codecs and authenticated encrypted transport; retain legacy reference separately; F3 |
| [Shared fonts](../Shared/Fonts/OFL-Fredoka.txt), [Plex license](../Shared/Fonts/OFL-IBMPlex.txt) | Existing TTF assets and redistribution notices | Register/package existing assets after platform choice; F1/F2 |
| [FaceView](../iOS/Sources/FaceView.swift) | FaceAnimator semi-implicit springs, random schedules, precedence, exact draw layers | Kotlin pure animator + Canvas/vector renderer; F1 |
| [BlobShape](../iOS/Sources/BlobShape.swift) | Elliptic path, crown points, gradient/font helpers | Literal paths/gradient; replace Apple font detection; F1 |
| [GooglyApp](../iOS/Sources/GooglyApp.swift) | RootView wiring, double tap, 0.3 s hold, state indicators, lifecycle | Android lifecycle/permission/gesture UI; avoid private screen-corner API; F1/F4 |
| [PairingView](../iOS/Sources/PairingView.swift) | Discovery waiting/dismissal visual | New verified pairing UX; source is not cryptographic pairing; F3 |
| [SoundButton](../iOS/Sources/SoundButton.swift) | Local chirp volume/test and desktop picker | Kotlin settings, authenticated peer selection; F1/F3/F4 |
| [MacLink](../iOS/Sources/MacLink.swift) | Browse/select/retry, callback correlation/disconnect | Android NSD/TLS, trusted identities, deadlines/backpressure; F3 |
| [LiveVoice](../iOS/Sources/LiveVoice.swift) | Wake/hold/release, 24 kHz mono PCM16, text deltas, local chirps, tool forwarding and reconnect context | Android audio + Realtime client with explicit consent and bounded retry; F4 |
| [SessionStore](../iOS/Sources/SessionStore.swift) | Session/entry IDs, kinds, pending chunks, crash closure, rough/proper display filtering | Android private versioned store with explicit date units and migration; F4/F5 |
| [BlueyAppView](../iOS/Sources/BlueyAppView.swift) | Session list/detail, export/copy agent prompt, settings affordances | Native Android accessible UI; notes/report extensions deferred; F4/F5 |
| [ChunkRecorder + NotesSync](../iOS/Sources/ChunkRecorder.swift) | 300 s AAC chunks, index/time metadata, retries, notes export, delete uploaded phone audio on success | Explicit recording opt-in, bounded media upload/idempotency/retention; F5 |
| [CursorOverlay](../Mac/Sources/GooglyMac/CursorOverlay.swift) | TimingCurve/Flight/CursorEngine, teardrop/trails, home/follow/pin, effects/captions | C# pure math plus chosen transparent compositor/window; F2 |
| [PhoneServer](../Mac/Sources/GooglyMac/PhoneServer.swift) | DNS-SD advertise, hello/face broadcast, delta thresholds | Windows secure listener/discovery and single-controller trust policy; F3 |
| [RealtimeHost](../Mac/Sources/GooglyMac/RealtimeHost.swift) | Token minting/session setup, tool routing, OCR pointing, caption/choreography | Windows coordinator, live-checked API adapter; authorization before dispatch; F4; action/notes/research separately deferred |
| [ScreenReader](../Mac/Sources/GooglyMac/ScreenReader.swift) | Main-display snapshot excluding self, OCR L/W IDs, targets, 0–1000 grid, resized JPEG | Windows capture + OCR adapter; fresh snapshot IDs and explicit DPI conversion; F4 |
| [ControlsReader](../Mac/Sources/GooglyMac/ControlsReader.swift) | C IDs, front-app accessible roles/bounds, secure-field lookup | Windows UI Automation candidate; permission/time budget/redaction validation; F4/F5 |
| [ComputerControl](../Mac/Sources/GooglyMac/ComputerControl.swift) | Click/type/key/scroll/drag/open semantics, restore user pointer | Windows input adapter only after stronger policy/stop/approval; F5 |
| [MeetingNotes](../Mac/Sources/GooglyMac/MeetingNotes.swift) | Local audio/session/Markdown layout, per-chunk speakers, transcription fallback | Optional Windows file/transcription adapter, safe path construction; F5 |
| [WebResearch](../Mac/Sources/GooglyMac/WebResearch.swift), [ReportPanel](../Mac/Sources/GooglyMac/ReportPanel.swift) | Responses web search/report plus separate report UI | Deferred product option, not required Realtime infrastructure |
| [Settings](../Mac/Sources/GooglyMac/Settings.swift) | Defaults, home/size/trail/mood preferences | Windows versioned settings; deliberately change computerControl default to off; F2/F5 |
| [Keychain](../Mac/Sources/GooglyMac/Keychain.swift) | Desktop-only long-lived key ownership | Replace plaintext restricted JSON with Windows OS-protected store; F4 |
| [Fonts](../Mac/Sources/GooglyMac/Fonts.swift) | Bundled Fredoka registration/display fallback | Native font registration/package verification; F2 |
| [HotKeys](../Mac/Sources/GooglyMac/HotKeys.swift) | Global trigger/stop commands; Carbon registration | Windows hotkey API with conflict feedback and reachable stop; F2/F5 |
| [AppDelegate](../Mac/Sources/GooglyMac/AppDelegate.swift), [main](../Mac/Sources/GooglyMac/main.swift) | Tray/menu wiring, overlay/server lifecycle, demo environment hooks | Windows shell/startup/settings; do not run API demo hooks during F0; F2 onward |
| [Package.swift](../Package.swift), [project.yml](../project.yml), [Apple build script](../scripts/build-mac.sh) | Existing Apple builds and font packaging | Retain; define independent target build setup only after authorized gates |

## Translation discipline

Map a source symbol's inputs, outputs, units, time basis, ownership and side effects
before translating. Keep animation/codec tests separate from platform adapters.
Record deliberate differences in [DECISIONS](../tracking/DECISIONS.md); do not
preserve insecure mechanics merely for parity. Pin source references during work
so future upstream changes cannot silently redefine acceptance.

ScreenReader requests accurate Vision OCR, up to 400 line observations, with a
600-word threshold checked per line (not a hard global word cap). JPEG max edge is
1100 and quality 0.72. ControlsReader defaults to 140 results, a 0.45 s traversal
budget, 5000 visited elements and 0.25 s AX messaging timeout. These are observed
implementation choices, not proven cross-platform performance requirements.

## F2 concrete symbol map

| Swift symbol | C# owner | Boundary |
| --- | --- | --- |
| TimingCurve, Flight | `windows/Bluey.Core/TimingCurve.cs` | Pure easing inversion, derivatives, trajectory |
| CursorEngine, CursorMode | `windows/Bluey.Core/CursorEngine.cs`, `Primitives.cs` | Pure mode/flight/fade/rotation/press/hover/gaze |
| Teardrop, renderTrail, renderString, effects | `Geometry.cs`, `VisualState.cs` in Bluey.Core | Pure paths, history, spring and lifetimes |
| TeardropLayer, CursorView | `windows/Bluey.Windows/VectorDrawing.cs`, `OverlayArt.cs` | Cached vector body, native WPF drawing/text |
| CursorOverlay, layerPoint | `windows/Bluey.Windows/OverlayWindow.cs`, `Native.cs` | Native transparent primary HWND; physical-to-DIP mapping, no layer y flip |
| Settings, HotKeys, AppDelegate, Fonts | `SettingsStore.cs`, `ControlsWindow.cs`, `Program.cs`, `VectorDrawing.cs` | Offline UI/hotkeys/settings and embedded original fonts only |

## F4-A concrete symbol map

| Source owner | Native owner | Observed boundary |
| --- | --- | --- |
| GooglyApp.RootView hold/double tap | Android MainActivity pointer adapter, voice/VoiceController | 300 ms hold; monotonic gesture arbitration; sleep/pause generation cancellation |
| GooglyApp.ModeIndicator | voice/ModeIndicator + ModeIndicatorRenderer | Exact look/pulse/transition math; native Canvas/public Android corners, adjustable fallback |
| LiveVoice.State/beginAsk/endAsk/caption events | voice/VoiceController + OfflineProvider | Six modes; deterministic mock events; no audio, chirps or provider network |
| RootView.syncFace / FaceView | voice/VoiceFace + existing FaceAnimator data override | Touch > local voice > fresh remote > demo; original springs preserved |
| RealtimeHost.updateCaption/captionDone | Bluey.Link/Presentation + Bluey.Windows/VoicePresentation | Strict TLS-only presentation; source reading time; existing vector bubble/fade |
| GooglyLink caption/state reference | PresentationCodec / shared f4a-cases | Versioned exact fields; no legacy generalized commands/token/media |
