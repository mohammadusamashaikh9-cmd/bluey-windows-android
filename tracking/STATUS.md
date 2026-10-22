# Current F4-C stacked implementation

2026-10-22 Asia/Karachi. Explicitly authorized before final F4-B physical retest;
this is not F4-B acceptance. Base origin/port/live-ai-voice verified4b380294e883e669d29000cd844962db88439e58;
accepted main7930efd unchanged. Branch port/screen-understanding targets the F4-B
branch. Do not modify/merge PR6 or merge the F4-C PR.

Implemented pure Bluey.Screen maps/snapshots/targets/consent/workflow/choreography,
WGC/D3D11 one-snapshot capture with selectable primary/window and preview, read-only
UIA, supported WinRT OCR adapter/availability guard, local pointing/Home/Stop, and
optional same-request Gemini WAV+JPEG/structured target context. No Android app or
LAN schema change. No screenshot/audio/private text/key logging or persistence.

Exact native OCR deployment blocker: Microsoft requires package identity; current
portable unsigned build has none. UI reports UnavailableIdentity, not fabricated
recognition. No package install/registration or external OCR is added. Real capture,
UIA targets and point-at-spot remain independent; synthetic demo labels all declared
annotations. Google image/structured pages policy403; current official Google SDK
source verifies inline multipart/JSON schema shape; actual fixed-model multimodal
behavior and arbitrary account eligibility remain unverified. D013 records limits.

Local actual results: Screen29, Link140 tests pass; WPF/net10 Windows22000 build
0 warnings/errors. Core/Windows native/Android CI and development artifacts pending.
No real provider call/key/personal screenshot or OS input action was used. F4-B's
five reported API attempts/two503s/no proven genuine visible answer stay pending.

One next action after CI: developer runs the offline stages of the combined F4-B/
F4-C QA sequence, recording the OCR identity blocker separately from capture/UIA/
pointing; no cloud test occurs without separately current explicit authorization.

# Retained F4-B evidence (no new acceptance)

# Current F4-B response repair — PR #6

2026-10-22 Asia/Karachi. Continue port/live-ai-voice from reviewed7c846a3;
no new PR/merge/F4-C. Developer-reported physical requests reached Gemini but no
visible answer is proven: five requests,two503s,nonzero input/output usage. The
other three outcomes are unknown. F2 pointing demo text is not a Gemini reply.

Confirmed source/test defects:503/400 and empty MAX_TOKENS outcomes were collapsed;
request sent candidateCount with128 tokens/no low thinking. Seven local regression
cases reproduced the request/classification gaps; eight authority cases reproduced
loss of the specific failure on LAN. Baseline58278fe Android CI39671465812 ran103
tests with one intentional failure: coalesced reply/done plus disabled chirps skips
pink before a frame. This is not proof of physical provider text/response contents.

Repair: fixed model, no candidateCount, low thinking,512 combined thinking/output
tokens (up to4× prior allowance; still one question/5s/20s/256 display characters).
Eleven narrow liveStop outcomes validated in both languages/66 shared fixtures;
actionable status,8 safe local metadata records, no private text/body/audio/keys.
400ms minimum local replying interval with unchanged250ms border transition and
stale completion guards. Public synthetic trace spans real adapter fake HTTP,
authority/TLS, Android coordinator/controller and native WPF caption checks.
Offline preview now read-only OFF status plus an action opening existing opt-in
settings; opening it grants nothing. Actual permissions/Stop remain in LivePanel.

Local .NET10.0.402: Core27/Link124 pass,0 failed/skipped. Matching Windows/Android
CI and development uploads pass below; physical retest remains unverified.
Official new/thinking/error docs still policy403; supplied compatibility contract
and accessible official Google SDK source are separately attributed in D012.
No real Gemini/OpenAI call,credential,private audio or cloud test used by Codex.
Original Apple/Shared, animation engines/controllers and protected storage unchanged.

One next action after matching CI artifacts: developer performs the single physical
acceptance sequence in project/QA.md; any one-question cloud request requires the
developer's separate current Free Tier review and explicit authorization.

## Response-repair delivery evidence

Both downloads are built/tested at **36287bcdcb95496c3da8f61973a51fc1e337913f**. Subsequent closeout edits
change canonical documentation only. Existing [PR #6](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/6)
remains open and unmerged. Original/animation/key-store paths are unchanged.

| Target | Actual CI results | Matching artifacts |
| --- | --- | --- |
| [Windows 39674027367](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39674027367) | Windows2025/.NET10.0.402: Core27+Link124,0 failed/skipped; WPF0 warnings/errors;29 native checks/13 renders; self-contained unsigned win-x64 | [App + full SHA256SUMS.txt](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39674027367/artifacts/12381980060), [TRX/native evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39674027367/artifacts/12380199443) |
| [Android 39674027307](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39674027307) | JDK17/Gradle8.13/SDK36:111 tests,0 failures/errors/skips; native bitmap tests; lint0 errors18 warnings; APK signature verified | [APK + SHA256SUMS.txt + public signer](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39674027307/artifacts/12380439015), [JUnit/lint/render evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39674027307/artifacts/12380314227) |

Local .NET Core27/Link124 also pass. Final PR Windows39674033798 passes; earlier
PR39673123298 failed one retained cancellation test because its20ms sleep raced
async cleanup. The test now waits for provider entry and a terminal cleanup outcome,
then asserts every byte wiped/no stale reply, bounded5s. No assertion or production
cancellation behavior removed. Initial failing regression cases and intermediate
CI are retained in SESSION-LOG. Android compiler also reports three warnings in
unchanged test files: nullable alias string and two deprecated NSD host accesses.

| Item | Observed SHA-256 |
| --- | --- |
| Windows EXE (CI) | `f5c1f178640efbeb6cc9d2e2355ed93e9ea3a80dfa153a5f92f6941ff6a58e5f` |
| Windows DLL (CI) | `08e1207f1c4c9183805b10b11b1636bd17ee82eee854fab36d4f9c4fffd2a12c` |
| Full Windows SHA256SUMS.txt (CI) | `f23ac6fc0f3fc8159445d2322b0020d98471c38829e46e3c4eaccc0946018f0c` |
| Windows app ZIP (upload metadata) | `0cd20ec6e73aba43587180f9f845f3f6d926509ed12c74be5c210166f0a84b33` |
| Android APK (CI) | `73276555f3d04d50b6732c8d150c4a3230a0cf0c300f59734bced34c6b2b2e02` |
| Android app ZIP (upload metadata) | `e7bd9f0d5240267c7c2fb561a0f982af90c743ea4f99c630734b439ac0998e8d` |
| Android public debug signer | `b30fe5e09b7e180211497aaeb11ffdb056e68ccbf5e3b4cf09d9c7dfd9ce5909` |

The manifests and evidence are uploaded (14-day development artifacts); local
archive downloads were policy403 at productionresultssa2/sa10.blob.core.windows.net.
Hashes/test/native results above are actual CI/upload evidence, not local archive
rehashes. Windows needs no separate desktop runtime; this is not an installer/release.
Android is versionCode11/0.4.4-f4b-gemini-response. Its signer differs from the prior
published c48278f165f2c5df743db2ffa3075bfe777f4d9cc4c17c627f50ac9c08c4a2bb.
Compare installed signer before updating. A mismatch requires deliberate reinstall,
which loses Android Keystore/F3 trust: revoke the old Windows phone entry and pair
explicitly again. Preserve Windows DPAPI files; no automatic identity reset occurs.

Security evidence: no real Gemini/OpenAI request, user credential, hardware audio
capture or cloud test used by Codex/CI. Fake HTTP/synthetic PCM plus actual loopback
TLS and composed cross-language/native caption tests are distinct from physical
interoperability. Offline default, current per-phone cloud/Free Tier authorization,
one-question limit, pinned mTLS, DPAPI/Keystore, revocation, bounded transport and
no retry/fallback remain. Safe diagnostics retain8 code/time/size records, no private
content. The three non503 physical outcomes, revised actual provider behavior,
physical pink/caption/latency/audio/resource behavior and project eligibility remain
unverified. A public API contract and passing tests do not certify free account use.

One next action: the developer performs the [single physical acceptance sequence](../project/QA.md#f4-b-response-repair--one-physical-acceptance-sequence),
including separate current Free Tier/one-request authorization if choosing the cloud
step; record only safe Windows attempt/code/ms/counts and Android status. No automatic
cloud test, second request, merge or F4-C is authorized.

## Exact response-repair changed files

- [android/app/build.gradle.kts](../android/app/build.gradle.kts)
- [android/app/src/main/java/dev/bluey/port/MainActivity.kt](../android/app/src/main/java/dev/bluey/port/MainActivity.kt)
- [android/app/src/main/java/dev/bluey/port/live/LiveCoordinator.kt](../android/app/src/main/java/dev/bluey/port/live/LiveCoordinator.kt)
- [android/app/src/main/java/dev/bluey/port/live/LiveProtocol.kt](../android/app/src/main/java/dev/bluey/port/live/LiveProtocol.kt)
- [android/app/src/main/java/dev/bluey/port/voice/VoicePreview.kt](../android/app/src/main/java/dev/bluey/port/voice/VoicePreview.kt)
- [android/app/src/test/java/dev/bluey/port/link/ProtocolTest.kt](../android/app/src/test/java/dev/bluey/port/link/ProtocolTest.kt)
- [android/app/src/test/java/dev/bluey/port/live/AudioCoordinatorTest.kt](../android/app/src/test/java/dev/bluey/port/live/AudioCoordinatorTest.kt)
- [project/IMPLEMENTATION.md](../project/IMPLEMENTATION.md)
- [project/PROTOCOL-DATA-SCHEMA.md](../project/PROTOCOL-DATA-SCHEMA.md)
- [project/QA.md](../project/QA.md)
- [project/SECURITY.md](../project/SECURITY.md)
- [protocol/README.md](../protocol/README.md)
- [protocol/f4b-cases.jsonl](../protocol/f4b-cases.jsonl)
- [protocol/gemini-response-trace.json](../protocol/gemini-response-trace.json)
- [tracking/CHECKPOINTS.md](../tracking/CHECKPOINTS.md)
- [tracking/DECISIONS.md](../tracking/DECISIONS.md)
- [tracking/HANDOFF.md](../tracking/HANDOFF.md)
- [tracking/SESSION-LOG.md](../tracking/SESSION-LOG.md)
- [tracking/STATUS.md](../tracking/STATUS.md)
- [windows/Bluey.Link.Tests/AudioTests.cs](../windows/Bluey.Link.Tests/AudioTests.cs)
- [windows/Bluey.Link.Tests/Bluey.Link.Tests.csproj](../windows/Bluey.Link.Tests/Bluey.Link.Tests.csproj)
- [windows/Bluey.Link.Tests/GeminiTests.cs](../windows/Bluey.Link.Tests/GeminiTests.cs)
- [windows/Bluey.Link.Tests/ProtocolTests.cs](../windows/Bluey.Link.Tests/ProtocolTests.cs)
- [windows/Bluey.Link.Tests/TlsTests.cs](../windows/Bluey.Link.Tests/TlsTests.cs)
- [windows/Bluey.Link/AudioAuthority.cs](../windows/Bluey.Link/AudioAuthority.cs)
- [windows/Bluey.Link/AudioOutcome.cs](../windows/Bluey.Link/AudioOutcome.cs)
- [windows/Bluey.Link/GeminiAnswers.cs](../windows/Bluey.Link/GeminiAnswers.cs)
- [windows/Bluey.Link/LiveAuthority.cs](../windows/Bluey.Link/LiveAuthority.cs)
- [windows/Bluey.Link/LiveProtocol.cs](../windows/Bluey.Link/LiveProtocol.cs)
- [windows/Bluey.Windows/AI/AiWindow.cs](../windows/Bluey.Windows/AI/AiWindow.cs)
- [windows/Bluey.Windows/Bluey.Windows.csproj](../windows/Bluey.Windows/Bluey.Windows.csproj)
- [windows/Bluey.Windows/NativeSmoke.cs](../windows/Bluey.Windows/NativeSmoke.cs)
- [windows/Bluey.Windows/VoicePresentation.cs](../windows/Bluey.Windows/VoicePresentation.cs)
- [windows/README.md](../windows/README.md)

# Previous F4-B HTTP-completion evidence (superseded by response repair)

2026-10-21 Asia/Karachi — continuing existing port/live-ai-voice / PR #6 from
56bcbb573a50362e2abf88c8325978e171486836. No new PR or merge.

D012 now distinguishes developer-reported independent public model/audio/WAV/Free
Tier contract verification from still-unverified user account eligibility and real
provider/device behavior. All six official Google pages remain proxy403 locally;
agent inspected official REST cookbook and python-genai sources for HTTP shape.
No real Gemini/OpenAI request, credential, microphone hardware or billing was used.

Implemented GeminiAnswers through existing IAudioAnswers/CurrentUser DPAPI Gemini
store: fixed gemini-3.8-flash HTTPS v1beta generateContent, x-goog-api-key header,
short inline audio/wav from exact RIFF/WAVE PCM16 mono24k conversion, text-only
single candidate/128 output tokens,20s timeout,330000-byte request/16384-byte response
bounds. No redirects, retries, tools, grounding, speech or provider/model fallback.
Strict response JSON/UTF-8/duplicates, safety, empty/tool/media output and status
handling; sanitized bounded text only. Owned binary buffers cleared, no raw bodies/
audio/transcripts/credentials logged or persisted. Existing stop codes map quota to
limit (Android offers offline), safety/credentials to denied, timeout to expired,
other failures to issuer. Existing F3/53 fixtures and animation code unchanged.

Production defaults OfflineSimulation with no grants. Startup/key save/provider
selection performs no cloud request. Current trusted phone/microphone/cloud consent,
readable protected key and explicit per-phone Windows cloud/Free Tier confirmation
are required. Project eligibility uncertainty must prevent authorization. UI Stop
remains responsive during key checks; grant state is rechecked afterward. Public
ContractVerified does not certify billing status; Google free-tier content terms
require informed consent. Offline mode needs no key or microphone permission.

Local observed .NET10.0.402 Link108 tests pass (61 retained+47 new fake-HTTP cases).
Initial xUnit2009 analyzer rejected a substring Assert.True; changed to Assert.Contains,
then tests passed. No assertion/suite disabled. Revised Windows/Android CI passed at the matching application commit below;
checksums and observed artifact evidence are recorded separately. Physical/live API acceptance is unverified. Original Apple/Shared,
FaceAnimator, VoiceController, ModeIndicator, Windows Core/overlay remain untouched.

One next action: perform the documented offline
physical retest with keys empty/provider OfflineSimulation and microphone denied.
No billable or free cloud test is authorized by this development task.


## Completion CI and matching artifacts

Both supplied applications are from **685a5e883ffcfdca784641d2d086a885409871cb**.
Later closeout edits change canonical documentation/tracking only. Existing
[PR #6](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/6)
remains open into main and unmerged.

| Target | Actual results | Artifacts |
| --- | --- | --- |
| [Windows39494775781](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39494775781) | Windows2025/.NET10.0.402:27 Core+108 Link tests,0 failed/skipped; WPF0 warnings/errors; self-contained win-x64;26 native checks,0 failed | [App/full SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39494775781/artifacts/12297695977), [TRX/native evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39494775781/artifacts/12298140136) |
| [Android39494775790](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39494775790) | JDK17/Gradle8.13/SDK36:102 tests,0 failed/errors/skipped; lint0 errors18 warnings; APK/signature verified | [APK/manifest/public signer](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39494775790/artifacts/12297606597), [JUnit/lint/frames](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39494775790/artifacts/12298040076) |

The push workflows tested the same exact application SHA. PR workflows39494783369
(Windows) and39494783112 (Android) also succeeded; their merge-build artifacts are
not the supplied downloads. No regression suite was disabled. Windows108 includes
47 new fake HTTP cases, plus retained61 codec/authority/actual TLS cases. Tests
cover exact WAV/request/header, allowlist/missing key, safe parsing/safety/empty/
quota/network/provider failures, UTF-8/duplicates/nontext/bounds, header/body timeout,
cancellation, existing authorization/one-response limits and revocation. Android adds
quota-stop/offline/no-retry coverage and retains all gesture/animation/F3 tests.

| Item | SHA-256 |
| --- | --- |
| Windows EXE (CI) | `0b9d42f3e83d28798916942f5fd7eb9b7b36eeae22962be0e01cc719e040202d` |
| Windows DLL (CI) | `80c247ca406e88188950fc80e6f75d499d45a748a2702fdbc922585eebae92e3` |
| Full Windows SHA256SUMS.txt (CI) | `7b564c725eb67c7c271a937635db9778c4700731f1fe36b5d1914d19f578010b` |
| Windows app ZIP (metadata) | `2c7aac08332b9d49fc1a55d85f9c9e0f42a5d636edddd7959801ee616b36a387` |
| Android APK (CI and local rehash) | `92906750e68f46f169ea707b8836c167caf99846bd1ec97b9d3505c346edf100` |
| Android SHA256SUMS.txt (local) | `e8fd455c3cf2f034bab1d8b33a00a9f451e01baf7ab51cc2e23a3f69dd7cda42` |
| Android app ZIP (metadata) | `685b6c30c2d058d597d7155d34ae80fb7b5696c858c4ee844e36a8729583cb30` |
| Android public debug signer | `c48278f165f2c5df743db2ffa3075bfe777f4d9cc4c17c627f50ac9c08c4a2bb` |

Android app/evidence downloaded: packaged APK manifest rehashed successfully and
JUnit independently parsed102/0/0/0. Windows app/evidence downloads were policy403
at productionresultssa15.blob.core.windows.net. Final Windows hashes/test/publish/
upload are observed CI evidence, not a local archive/manifest rehash. No bypass.
Windows is self-contained, unsigned development output; no runtime install needed.
No installer/signing/release was created.

The new APK signer differs from prior amendment39527af1f27b0308194f66306fb8add295c85c9bd76a83d8e7a0b77083ba87a3.
Compare against the actually installed APK before updating. A differing signer needs
a deliberate uninstall/install, losing Android Keystore identity/trust; revoke the
old Windows phone entry and explicitly pair again. Preserve Windows protected data.
No signing private key or application credential was committed/published.

No real Gemini/OpenAI request, user key, cloud test, billing configuration or physical
microphone capture occurred. Public-contract verification is developer-reported;
accessible official Google source corroborates serialization. Real account eligibility,
API behavior/audio quality/consent indicators/fidelity/latency/CPU/multi-device QA
remain unverified. Existing F3/F4-A functional acceptance stays accepted. Follow
[one offline physical sequence](../project/QA.md#f4-b-gemini-amendment--offline-and-future-free-tier-qa)
with keys empty/provider offline/microphone denied. Any real Free Tier test still
requires separate explicit user authorization.

## Exact completion files

Compared with56bcbb5;20 files. No original Apple/Shared or accepted animation source
changed. Application commit685a5e8; later evidence-only edits stay in these paths.

```text
android/app/build.gradle.kts
android/app/src/main/java/dev/bluey/port/live/LiveCoordinator.kt
android/app/src/main/java/dev/bluey/port/live/LivePanel.kt
android/app/src/test/java/dev/bluey/port/live/AudioCoordinatorTest.kt
project/IMPLEMENTATION.md
project/PROTOCOL-DATA-SCHEMA.md
project/QA.md
project/SECURITY.md
protocol/README.md
tracking/CHECKPOINTS.md
tracking/DECISIONS.md
tracking/HANDOFF.md
tracking/SESSION-LOG.md
tracking/STATUS.md
windows/Bluey.Link.Tests/GeminiTests.cs
windows/Bluey.Link/AudioAuthority.cs
windows/Bluey.Link/GeminiAnswers.cs
windows/Bluey.Windows/AI/AiWindow.cs
windows/Bluey.Windows/Connection/WindowsLinkHost.cs
windows/README.md
```

---

# Historical amendment evidence — provider prerequisite before completion

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
0 warnings/errors. Final matching-revision CI/results are recorded below. Existing
OpenAI/F1/F2/F3/F4-A regressions are retained, not disabled. Physical QA unrun.

The completed F3 and F4-A developer-reported functional acceptance stays accepted;
unmeasured performance/latency/resource/device-matrix QA stays unverified.
[QA](../project/QA.md#f4-b-gemini-amendment--offline-and-future-free-tier-qa) supplies
one offline physical sequence and a separately authorized future free-tier procedure.

One next action: publish the saved ai.google.dev documentation-access draft in
cloud environment settings, then resume exact model/free-tier contract verification.


## Revised Android CI evidence

Application revision **5a8978a86e9dea18cb38be0c6a87b5daf176ae82**.
[Android run39463749191](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39463749191)
succeeded:101 tests,0 failures/errors/skips; lint0 errors18 warnings; APK built,
apksigner verified and artifacts uploaded. [APK/SHA256SUMS/public signer](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39463749191/artifacts/12284311183),
[JUnit/lint/frames](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39463749191/artifacts/12284470378).

- APK SHA-256 (actual CI log): `f67f55034dcfbbdf80d5a15346f458f60514f3ba424a79d7c7eedd22f4dbff4c`.
- App ZIP SHA-256 (GitHub metadata): `c6339117115d7176b83ad48e60c0a85200e0b004548e49d7d43936676e4fa056`.
- Public debug signer: `39527af1f27b0308194f66306fb8add295c85c9bd76a83d8e7a0b77083ba87a3`.

Signer differs from prior F4-B artifact14a5c577e03ba05a977e38960cd31181b4b5c5bc6bf416ffc65b64242fcfc992.
An installed app with that signer cannot be updated in place. Deliberate uninstall/
install loses Android identity/trust; revoke the old Windows entry and re-pair.
No automatic identity deletion. Preserve Windows DPAPI/trust data.

Both final Android archive downloads were proxy-blocked at
productionresultssa9.blob.core.windows.net. CI logs/API prove tests/signature/hashes/
upload, not a local manifest rehash. The additive environment draft now includes
that observed artifact host as well as ai.google.dev, requires publication. No bypass.
Windows run39463744341 was queued at the initial Android observation and subsequently
succeeded; its final evidence is recorded below. Earlier superseded/canceled runs are not passes.
The Android PR run39463678121 also passed101/lint18 but has a different debug signer;
do not mix its APK with the supplied manual-run artifact.

Initial local test build briefly failed CS1739 because an opt-in argument was
applied to the fake OpenAiIssuer call; corrected, then Link61 and WPF clean pass.
No assertion or existing suite was disabled. Gemini-specific HTTP/429/safety parsing
is unimplemented/unrun, not a mocked pass. Google contract/pricing/billing remain
unverified; no live call or free-tier acceptance is claimed.


## Revised Windows CI evidence and artifact integrity

The same application revision **5a8978a86e9dea18cb38be0c6a87b5daf176ae82** passed
[Windows run39463744341](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39463744341):
27 Core+61 Link tests,0 failed/skipped; actual loopback mTLS/enrollment/reconnection/
revocation regression retained; WPF build0 warnings/errors; self-contained win-x64
publish;26 native HWND/DPAPI/render checks. Includes actual CurrentUser DPAPI
Gemini/OpenAI file isolation and removal; no real credential. Offscreen rendering
is not a physical animation/click-through/DPI/performance pass.

[Windows application/full SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39463744341/artifacts/12284311803),
[TRX/native/frame evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39463744341/artifacts/12283409683).

| Item | SHA-256 from final CI logs / GitHub metadata |
| --- | --- |
| Windows EXE | `05541aebb3d04fafb5153d50daefc5e9f1e0046803f545d74d2cbf9716ee3afb` |
| Windows DLL | `6c4539d94378053f01a2b201a166737cf7ee299b33e0d508d13f5fa9c056a7fd` |
| Full Windows SHA256SUMS.txt | `f4245afedf87aafa918346b03ce84fbf97cb01d8a6201812b14b09cabd0cf687` |
| Windows app ZIP | `53f6f8c021cfc76d12f01cab2a502085c6a7d0b632bcfbac0a96d0c01b9dd982` |

Windows app/evidence downloads were policy403 at productionresultssa1; Android
app/evidence at productionresultssa9. Both observed hosts were added to the saved
additive documentation/artifact environment draft, requires publication. CI logs/
metadata prove build/test/hash/upload; no local final archive/manifest rehash was
possible. No policy bypass. Both supplied artifact names include the same exact
application SHA. Later tracking-only edits do not change application source.
No Windows runtime installation is needed for this self-contained unsigned dev build.
No installer/signing/release or merge was performed.

All code tests retain existing suites. New checks cover provider denial/defaults,
current consent/free-tier confirmation, strict audio fields and shared53 fixtures,
250 bounded chunks, short/invalid/duplicate/reordered/stale audio, backpressure,
assembly/request timeout, completion, cancellation/wiping, permission denial,
OpenAI fallback rejection, sleep/background/disconnect/withdrawal, duplicate grants,
local caption states/cleanup and existing actual mTLS/revocation. Provider-specific
Gemini HTTP/429/safety schema tests remain blocked/unrun; fake answer tests do not
establish Gemini interoperability or physical audio quality. No real Gemini/OpenAI
request or key was used; Windows constructs DeniedIssuer/UnavailableAudioAnswers,
Android rejects optional OpenAI grants by default, tests inject synthetic adapters.

## Exact amendment files

Compared with dc5d6d9; application revision5a8978a:

```text
android/app/build.gradle.kts
android/app/src/main/java/dev/bluey/port/link/DeviceLink.kt
android/app/src/main/java/dev/bluey/port/link/Wire.kt
android/app/src/main/java/dev/bluey/port/live/AudioProtocol.kt
android/app/src/main/java/dev/bluey/port/live/LiveCoordinator.kt
android/app/src/main/java/dev/bluey/port/live/LivePanel.kt
android/app/src/test/java/dev/bluey/port/link/ProtocolTest.kt
android/app/src/test/java/dev/bluey/port/live/AudioCoordinatorTest.kt
android/app/src/test/java/dev/bluey/port/live/AudioUploadTest.kt
android/app/src/test/java/dev/bluey/port/live/LiveCoordinatorTest.kt
project/IMPLEMENTATION.md
project/PROTOCOL-DATA-SCHEMA.md
project/QA.md
project/SECURITY.md
protocol/README.md
protocol/f4b-cases.jsonl
tracking/CHECKPOINTS.md
tracking/DECISIONS.md
tracking/HANDOFF.md
tracking/SESSION-LOG.md
tracking/STATUS.md
windows/Bluey.Link.Tests/AudioTests.cs
windows/Bluey.Link.Tests/LiveTests.cs
windows/Bluey.Link.Tests/ProtocolTests.cs
windows/Bluey.Link.Tests/TlsTests.cs
windows/Bluey.Link/AudioAuthority.cs
windows/Bluey.Link/AudioProtocol.cs
windows/Bluey.Link/LinkServer.cs
windows/Bluey.Link/LiveAuthority.cs
windows/Bluey.Link/Wire.cs
windows/Bluey.Windows/AI/AiWindow.cs
windows/Bluey.Windows/AI/ProtectedApiKey.cs
windows/Bluey.Windows/Connection/WindowsLinkHost.cs
windows/Bluey.Windows/NativeSmoke.cs
windows/README.md
```

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
