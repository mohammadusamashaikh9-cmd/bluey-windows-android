# Current amendment state — Gemini provider prerequisite blocked

2026-10-20 UTC — existing `port/live-ai-voice`, PR #6 remains open/unmerged.
The user-authorized Gemini amendment supersedes the previous active OpenAI UI.
No real Gemini/OpenAI request, API key, microphone hardware capture or billing
configuration was used. Original Apple/Shared, FaceAnimator, VoiceController,
ModeIndicator, Windows overlay/Core and F3 trust/identity remain unchanged.

Implemented provider-independent work: D012 bounded authenticated audio turn types,
960-byte numbered PCM chunks,5s/240000-byte/250-chunk cap, strict sequence/length/
format validation,8s assembly/20s answer/60s approval bounds; current lease/consent/
Windows authorization reuse; cancellation/backpressure/zeroing/stale replies; completed
answer presentation via existing controller/chirps/captions; separate Gemini DPAPI
purpose/file and explicit Free Tier/cloud UI. Default offline; production OpenAI is
disabled on both sides, without paid fallback or reconnect requests.53 shared F4-B
fixtures cover old live messages and new audio types. Synthetic adapters only.

**Blocked/incomplete:** all four official Google audio/pricing/billing/rate-limit
URLs returned proxy403. ai.google.dev additive environment draft saved, requires
publication. Candidate gemini-3.8-flash/audio MIME/schema/free-tier eligibility remain
unverified. There is no Gemini HTTP adapter, fake HTTP schema/quota/safety suite or
live capability in this revision. UnavailableAudioAnswers prevents authorization
even if keys/checkboxes are set. Do not infer that the candidate is unavailable or
silently switch. Finish the verified adapter after documentation access; do not claim
this amendment complete. Google Free Tier project confirmation cannot prove zero
cost for arbitrary credentials; sensitive conversations are excluded from testing.

Observed locally before CI: .NET10.0.402 Core27/27 and Link61/61 pass, including actual
loopback mTLS audio-ready/approval/revocation and synthetic answer tests; WPF cross-build
0 warnings/errors. Android/Windows CI and revised artifact results pending. Existing
OpenAI/F1/F2/F3/F4-A regressions are retained, not disabled. Physical QA unrun.

The completed F3 and F4-A developer-reported functional acceptance stays accepted;
unmeasured performance/latency/resource/device-matrix QA stays unverified.
[QA](../project/QA.md#f4-b-gemini-amendment--offline-and-future-free-tier-qa) supplies
one offline physical sequence and a separately authorized future free-tier procedure.

One next action: publish the saved ai.google.dev documentation-access draft in
cloud environment settings, then resume exact model/free-tier contract verification.

---

# Previous OpenAI implementation evidence (runtime superseded below)

2026-10-20 UTC — F4-B live voice implementation on `port/live-ai-voice`.
Accepted main: **7930efd1bca650ed0358bfc56201950083ea530c**, verified through
origin/main and GitHub PR #5 merged state. Developer reports F4-A functional
physical acceptance; no new agent-operated device tests or measured performance
are inferred. F1/F2/F3/F4-A measured FPS/resources/latency/compatibility remain separate.

Both supplied apps use **`5df67442c2cb5e8967a9caca8de3aba42e4a3099`**. Later closeout edits are canonical documentation
only. [PR #6](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/6) is open into main and unmerged.
No real key, microphone capture, client-secret mint or paid OpenAI call was
performed during development/CI. Real API/device acceptance remains pending a
separate user decision; code/offline checks are complete, not a live-service pass.

## Implemented scope and source comparison

- Windows CurrentUser DPAPI master-key store with owner-only directory ACL, explicit
  Set/Replace/Remove/status and no saved-key redisplay. Corruption/missing access
  fails closed. Set/configure/host startup never calls OpenAI.
- Current trusted TLS lease/pin/Android consent ID plus explicit reviewed Windows
  cloud authorization gates issuance. One locally authorized phone; at most two
  mints per grant,30-second spacing,10-second mint deadline, requested secret TTL30s.
  No general command dispatcher. Consent flags alone cannot grant authority.
- Mini model default; full gpt-realtime-2.1 is an explicit Windows option. GA nested
  session config, text-only output, manual commit/response, no tools/transcription/
  tracing. Current SDK-generated OpenAPI docs verified endpoint/model IDs/WSS/events;
  direct web guides/pricing were proxy403. Price/access/retention not inferred.
- Native AudioRecord only for an explicitly armed/authorized foreground ready session.
  Hold-only defaults on; continuous-awake is a distinct informed opt-in. Native rate
  fallback,31-tap downsample filter, chunk-invariant PCM16LE mono24k/20ms frames,
  bounded buffers/queues. No audio files or transcripts persisted/logged.
- Existing VoiceController gestures/six states and F4-A presentation drive real
  events, local source-derived pentatonic AudioTrack chirps and Windows bubbles.
  Speaking completion waits for local chirp quiet; sleep invalidates delayed work.
  Canceled holds stop/clear hold-only capture, drop late frames/errors and never
  commit. Stale drain completion cannot stop a replacement session.
- Defaults300s/5 responses/60 input seconds/256 response tokens; configurable within
 30–600s/1–20/1–120s/16–1024. Approval duration starts on Windows. A token carries
  remaining1–600 seconds, so request delay never extends the approved deadline.
  Empty audio below100ms produces no response. Local limits are cooperative safeguards.
- Sleep/Stop/pause/background/LAN loss/revocation/withdrawal closes work, cancels
  pending privilege/caption work and requires fresh Start/current Windows approval.
  No automatic live reconnect, hidden microphone or wake-on-restart. Offline demo
  selection stops live mode. Usage is memory-only metadata, not authoritative billing.

Original Mac/iOS/Shared, Android animation/renderer and ModeIndicator math, Windows
Bluey.Core and preview assets remain unchanged. D011 and PORTING-MAP record source
semantics and deliberate differences: hold-only default, bounded explicit restart,
disabled transcript/tools/notes, deterministic three-note chirp and public native APIs.
No new framework/backend. One pinned OkHttp4.12 dependency supplies native Android WSS.

## Actual final checks and artifacts

| Target | Observed final result | Downloads |
| --- | --- | --- |
| [Windows CI39455054721](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39455054721) | Windows-2025/.NET10.0.402;27 core+45 link tests, zero failed/skipped; WPF build0 warnings/errors; self-contained win-x64 publish;23 native checks/13 offscreen frames | [App/full SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39455054721/artifacts/12279802413), [TRX/native evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39455054721/artifacts/12281500075) |
| [Android CI39455054729](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39455054729) | JDK17/Gradle8.13/SDK36;89 tests, zero failures/errors/skips; lint0 errors18 warnings; apksigner verified/debug APK; retained native reference-frame tests | [APK/SHA256SUMS/public signer](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39455054729/artifacts/12280571171), [JUnit/lint/frame evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39455054729/artifacts/12280051809) |

Tests cover current consent/desktop denial, stale mint completion, expiry/rate limits,
actual .NET loopback mutual-TLS ready versus enrollment-only and revoked requests,
no master in issued metadata, fixed GA request serialization through fake HTTP,
response ordering/duplicates/empty commit/errors/limits/cancellation, PCM frame/endianness/
resampling/chunk invariance/out-of-band filtering, hold-only versus continuous policy,
sleep/Stop/pause/disconnect/withdrawal/current capture error, canceled holds/late capture
errors/abandoned drains, offline no capture/provider factory, and all existing suites.
Shared fixtures:40 F3+27 F4-A+28 F4-B, decoded in both languages. Windows native CI
executes real CurrentUser DPAPI key set/replace/corrupt/remove in a temporary directory;
no real OpenAI credential. This does not prove another Windows user context or phone
hardware. Local `.NET` link suite passes45/45. A focused red minimum-duration test
first reproduced liveStop after one-second delay, then passed29-second token metadata
and expiry at original30-second deadline after repair.

Earlier Android failures: first reducer syntax corrected; second ran82 tests with
one outdated LAN-only permission expectation, updated to exact LAN+opt-in mic while
retaining font checks; later89-test run had one fixture firing a microphone error
after release, moved to active capture while preserving stale-error rejection tests.
Superseded canceled runs are not passes. Final warnings retain legacy NSD/nullable
compiler notices and18 lint suggestions/version/manifest/pinned-trust warnings;
new dependency availability adds one warning to F4-A's17. No assertion/test disabled.

## Artifact integrity and install continuity

These are final CI-observed hashes, not local rehashes. Final archive downloads were
policy-blocked at productionresultssa10 (Android) and productionresultssa15 (Windows).
No bypass attempted. Earlier superseded Android a65 APK/certificate/JUnit87/lint/41
frames and Windows2f4 409-file manifest/TRX/native23/13 frames were downloaded and
inspected; they are historical evidence, not the supplied final downloads. Final
builds/test totals/signature/hash/upload are observed in actual CI logs/API metadata.

| Final item | SHA-256 |
| --- | --- |
| Windows EXE | `b72db4cda81180bed0bbbeec0a1b6c2df8025706fcd80563ae5bc92704d89e1c` |
| Windows DLL | `66555d3ef3e8576ebd4e13e1f9de6f6112f5e0d8c97b3210399677c42acac197` |
| Windows full SHA256SUMS.txt | `eff605f918465a43fe7016d1acdda8d20ec6c46c9b9d4c61924574d1ce457f20` |
| Windows ZIP (GitHub metadata) | `9b1e1d2be3ca20fc396f1facb0f3090093584ea04f5fa555efa7df9bbdf3edf2` |
| Android APK | `a3cc890747f9f9e685d578608b608cd980e0574f72d372b8a115dd4cc9037290` |
| Android ZIP (GitHub metadata) | `20849f8c3132e9f687506d2159150c2ec2a41a7e622d2e6b0725834544d1eb41` |
| Android public debug signer | `14a5c577e03ba05a977e38960cd31181b4b5c5bc6bf416ffc65b64242fcfc992` |

The signer differs from delivered F4-A's054f151e74b6035f0a73d2c778250e50197c2057bcd6de327147191b0622f3f4.
An update over that signer requires deliberate uninstall/install, losing Android
identity/data and requiring explicit re-pairing; no silent identity reset was added.
Revoke obsolete peers separately. Preserve Windows DPAPI app data/executable path.
Windows folder is unsigned self-contained x64: no SDK/runtime/IDE install required.
Both manifests are packaged with the downloads. Artifacts expire after14 days.
Do not mix later PR-merge/debug-signer builds with this matching application pair.

## Remaining boundaries and exactly one next action

No physical F4-B mic/routes/interruptions/permission UX, Realtime session, chirp timing,
provider event compatibility, actual latency/FPS/resource usage or billing measured.
Official web pricing/access/retention remains unverified; a narrow doc-domain draft
was saved, not observed activated. Current official SDK docs are the verified GA
contract reference, not proof of account entitlement or live behavior.

Delivered bearer secrets may establish multiple provider sessions until expiry;
existing sessions can outlive expiry. A hostile authorized phone can ignore remote
Stop. Local session/audio/response limits and cooperative closure are not a hard
financial cap or guaranteed remote provider revocation. Review current project/org
budgets/usage notifications and their limits before any separately authorized run.
No capture/OCR/tools/notes/background services/backend or cloud database is included.

**One next action:** the developer decides whether to authorize the documented
[30-second hold-only mini-model device test](../project/QA.md#f4-b-offline-and-future-billable-qa),
after reviewing current pricing/account access/retention and the matching artifacts.
No billable session may be run automatically or inferred from this implementation.

## Exact changed files

- [.github/workflows/android.yml](../.github/workflows/android.yml)
- [.github/workflows/windows.yml](../.github/workflows/windows.yml)
- [android/app/build.gradle.kts](../android/app/build.gradle.kts)
- [android/app/src/main/AndroidManifest.xml](../android/app/src/main/AndroidManifest.xml)
- [android/app/src/main/java/dev/bluey/port/MainActivity.kt](../android/app/src/main/java/dev/bluey/port/MainActivity.kt)
- [android/app/src/main/java/dev/bluey/port/link/DeviceLink.kt](../android/app/src/main/java/dev/bluey/port/link/DeviceLink.kt)
- [android/app/src/main/java/dev/bluey/port/link/Wire.kt](../android/app/src/main/java/dev/bluey/port/link/Wire.kt)
- [android/app/src/main/java/dev/bluey/port/live/AndroidAudio.kt](../android/app/src/main/java/dev/bluey/port/live/AndroidAudio.kt)
- [android/app/src/main/java/dev/bluey/port/live/LiveAdapters.kt](../android/app/src/main/java/dev/bluey/port/live/LiveAdapters.kt)
- [android/app/src/main/java/dev/bluey/port/live/LiveCoordinator.kt](../android/app/src/main/java/dev/bluey/port/live/LiveCoordinator.kt)
- [android/app/src/main/java/dev/bluey/port/live/LivePanel.kt](../android/app/src/main/java/dev/bluey/port/live/LivePanel.kt)
- [android/app/src/main/java/dev/bluey/port/live/LiveProtocol.kt](../android/app/src/main/java/dev/bluey/port/live/LiveProtocol.kt)
- [android/app/src/main/java/dev/bluey/port/live/OpenAiSocket.kt](../android/app/src/main/java/dev/bluey/port/live/OpenAiSocket.kt)
- [android/app/src/main/java/dev/bluey/port/live/Pcm24k.kt](../android/app/src/main/java/dev/bluey/port/live/Pcm24k.kt)
- [android/app/src/main/java/dev/bluey/port/live/RealtimeEngine.kt](../android/app/src/main/java/dev/bluey/port/live/RealtimeEngine.kt)
- [android/app/src/main/java/dev/bluey/port/voice/VoiceController.kt](../android/app/src/main/java/dev/bluey/port/voice/VoiceController.kt)
- [android/app/src/test/java/dev/bluey/port/link/ProtocolTest.kt](../android/app/src/test/java/dev/bluey/port/link/ProtocolTest.kt)
- [android/app/src/test/java/dev/bluey/port/live/LiveCoordinatorTest.kt](../android/app/src/test/java/dev/bluey/port/live/LiveCoordinatorTest.kt)
- [android/app/src/test/java/dev/bluey/port/live/RealtimeTest.kt](../android/app/src/test/java/dev/bluey/port/live/RealtimeTest.kt)
- [android/app/src/test/java/dev/bluey/port/render/FaceRendererTest.kt](../android/app/src/test/java/dev/bluey/port/render/FaceRendererTest.kt)
- [android/app/src/test/java/dev/bluey/port/voice/IndicatorRenderTest.kt](../android/app/src/test/java/dev/bluey/port/voice/IndicatorRenderTest.kt)
- [project/APP-FLOWS.md](../project/APP-FLOWS.md)
- [project/ARCHITECTURE.md](../project/ARCHITECTURE.md)
- [project/DESIGN-FIDELITY.md](../project/DESIGN-FIDELITY.md)
- [project/IMPLEMENTATION.md](../project/IMPLEMENTATION.md)
- [project/PORTING-MAP.md](../project/PORTING-MAP.md)
- [project/PRD.md](../project/PRD.md)
- [project/PROTOCOL-DATA-SCHEMA.md](../project/PROTOCOL-DATA-SCHEMA.md)
- [project/QA.md](../project/QA.md)
- [project/SECURITY.md](../project/SECURITY.md)
- [project/TRD.md](../project/TRD.md)
- [protocol/README.md](../protocol/README.md)
- [protocol/f4b-cases.jsonl](../protocol/f4b-cases.jsonl)
- [tracking/CHECKPOINTS.md](../tracking/CHECKPOINTS.md)
- [tracking/DECISIONS.md](../tracking/DECISIONS.md)
- [tracking/HANDOFF.md](../tracking/HANDOFF.md)
- [tracking/SESSION-LOG.md](../tracking/SESSION-LOG.md)
- [tracking/STATUS.md](../tracking/STATUS.md)
- [windows/Bluey.Link.Tests/Bluey.Link.Tests.csproj](../windows/Bluey.Link.Tests/Bluey.Link.Tests.csproj)
- [windows/Bluey.Link.Tests/LiveTests.cs](../windows/Bluey.Link.Tests/LiveTests.cs)
- [windows/Bluey.Link.Tests/ProtocolTests.cs](../windows/Bluey.Link.Tests/ProtocolTests.cs)
- [windows/Bluey.Link.Tests/TlsTests.cs](../windows/Bluey.Link.Tests/TlsTests.cs)
- [windows/Bluey.Link/LinkServer.cs](../windows/Bluey.Link/LinkServer.cs)
- [windows/Bluey.Link/LiveAuthority.cs](../windows/Bluey.Link/LiveAuthority.cs)
- [windows/Bluey.Link/LiveProtocol.cs](../windows/Bluey.Link/LiveProtocol.cs)
- [windows/Bluey.Link/OpenAiIssuer.cs](../windows/Bluey.Link/OpenAiIssuer.cs)
- [windows/Bluey.Link/Wire.cs](../windows/Bluey.Link/Wire.cs)
- [windows/Bluey.Windows/AI/AiWindow.cs](../windows/Bluey.Windows/AI/AiWindow.cs)
- [windows/Bluey.Windows/AI/ProtectedApiKey.cs](../windows/Bluey.Windows/AI/ProtectedApiKey.cs)
- [windows/Bluey.Windows/Connection/WindowsLinkHost.cs](../windows/Bluey.Windows/Connection/WindowsLinkHost.cs)
- [windows/Bluey.Windows/ControlsWindow.cs](../windows/Bluey.Windows/ControlsWindow.cs)
- [windows/Bluey.Windows/NativeSmoke.cs](../windows/Bluey.Windows/NativeSmoke.cs)
- [windows/Bluey.Windows/Program.cs](../windows/Bluey.Windows/Program.cs)
- [windows/README.md](../windows/README.md)
