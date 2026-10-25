# Checkpoints and evidence

States: **planned** = defined, not started; **active** = scoped work in progress;
**blocked** = named dependency prevents exit; **verified** = every listed exit
check has observed evidence. Owner roles are accountable work scopes, not proof
that someone has been assigned or performed tests.

| Gate | State | Accountable scope | Exit evidence |
| --- | --- | --- | --- |
| Fork creation (separate) | Verified remote state; creation user-executed | Repository owner | [Fork URL](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android); GitHub metadata confirms parent `rbrown101010/bluey-by-riley`, default `main`; both remote mains resolved to `6d84d91456f3bf7ad9b31caf31f19710b0ba8328` on 2026-10-18. Creation action itself was not performed/observed by this agent. |
| F0 foundation | Verified; accepted | Foundation author/reviewer | [Canonical navigation](../PORTING.md), [map](../project/PORTING-MAP.md), [rules](../AGENTS.md), four skills; static results in [session log](SESSION-LOG.md). [PR #1](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/1) merged at accepted `55a1c62abaf3fae19b944fc2335f2f2e8adc2299`, confirmed through GitHub. |
| F1 Android vector/animation | Implementation merged; physical acceptance UNVERIFIED | Android implementation + visual QA | [Run 39070720349](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39070720349) at `e231228188a515cef24d0b104d3b1ea5fd2b70e0` passed Kotlin compilation, 23 unit/native-render tests, lint and debug packaging; APK/evidence uploaded. See [QA](../project/QA.md#android-f1-build-and-comparison) and [session log](SESSION-LOG.md). [PR #2](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/2) merged at `15194dec8356385b15dc6712630d45e4761cd1de`; phone install/FPS/GPU/input/lifecycle comparisons remain unrun. |
| F2 Windows transparent overlay | Implementation merged; functional smoke user-reported accepted, measured QA UNVERIFIED | Windows implementation + visual QA | [PR #3](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/3) squash-merged at `a80642ab2aa897e6f80baed081ce2beb3e9a6d3d`, GitHub verified. Prior [CI 39116112594](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39116112594) passed 27 pure tests, native build/publish, 9 HWND/render checks and 12 frames. Developer reports accepted functional smoke; no new physical logs/measurements supplied. Detailed desktop/DPI/capture/fidelity and frame/CPU/memory measurements remain separate and unverified. |
| F3 secure device link | Implementation merged; functional QA developer-reported accepted | Protocol/security implementation + cross-device QA | PR #4 squash-merged at `32a38c045e6de87e9d79d44d1114a4f0c76d2b8d`, GitHub verified. Developer reports real mTLS, gaze, mood, touch precedence, Wi-Fi recovery, revocation and full Windows restart / explicit host start / saved authenticated reconnect without QR or identity replacement passed. Prior R3 CI evidence retained in SESSION-LOG. Measured latency/FPS/resources, multiple-device and advanced compatibility QA remain UNVERIFIED. |
| F4-A offline voice foundation | Merged; functional physical QA developer-reported accepted, measured QA unverified | Native voice/presentation + privacy QA | D010; app `e0dcda03ca2168ada223bb9f01d7fb231a506202`. [Windows39335639475](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39335639475):27 core+35 link/native build/publish/16 checks/13 frames. [Android39335639487](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39335639487):70 tests/lint0 errors17 warnings/APK/41 frames. Bounded authenticated captions; OFF consents, no live audio/provider/token endpoint. Matching artifacts/checksums and signer warning in [STATUS](STATUS.md). PR #5 merged at7930efd; developer-reported functional acceptance, measured pacing/resources remain unverified. |
| F4-B native live voice | Response repair code/CI verified; physical/provider acceptance pending | Native voice/security + device/API QA | D012, app `36287bcdcb95496c3da8f61973a51fc1e337913f`. Windows39674027367:27+124 tests/29 native checks; Android39674027307:111 tests/lint0 errors18 warnings. Matching artifacts in [STATUS](STATUS.md). Developer reports prior requests reached Gemini but no visible reply; revised physical acceptance unverified. No real Codex API calls. |
| F5 optional control/notes | Planned, optional | Scoped feature owner + safety/data QA | Application confirmation/stop or bounded recording/transcription/retention tests as selected; no execution |
| F6 release/demo | Planned | Release owner + artifact/device QA | Reproducible builds, hashes, CI/device evidence and truthful demo/docs; no execution |

No provisional ZIP/C0/C1 records were present in the checkout. No legacy gate is
counted as complete. If that kit is later supplied, retain its IDs only as aliases
to F0–F6 after checking actual scope/evidence; do not assume `C1` means `F1`.
Never double count fork creation as F0 implementation or infer completion from
an alias. [STATUS](STATUS.md) holds current verified facts; this table owns gate state.

## F4-B Gemini amendment / 2026-10-20 UTC

Partial implementation, provider prerequisite blocked (D012). Bounded F3 audio
transfer, isolated Gemini DPAPI/UI, dormant OpenAI and synthetic completed-answer
path added. Local Core27/Link61 tests pass; WPF cross-build0 warnings/errors.
Official Google docs proxy403 prevents required candidate/free-tier/schema verification;
Gemini authorization fails closed and HTTP adapter/tests remain pending. No cloud
request, real key or microphone hardware used. CI/artifacts pending; physical/live
acceptance not claimed. F3/F4-A accepted functional evidence remains unchanged.

Final amendment evidence: app5a8978a86e9dea18cb38be0c6a87b5daf176ae82;
Windows39463744341 succeeds Core27/Link61/native26/WPF0 warnings-errors/self-contained;
Android39463749191 succeeds101 tests/lint0 errors18 warnings/signature/debug APK.
Matching app artifacts/manifests uploaded; actual hashes/public signer/links and
exact35 paths in STATUS. Downloads blocked by policy at sa1/sa9; additive draft
saved, requires publication. Android signer39527af1… differs from previous14a5c577…,
so deliberate reinstall/re-pairing may be required; no automatic trust deletion.
No Google/OpenAI call/key/hardware capture; synthetic adapters only. Documentation
contract prerequisite and Gemini HTTP implementation remain unresolved; gate partial.
PR #6 updated in place, no new PR/merge. One next action: publish the saved Google
and observed artifact-domain draft, then resume exact contract verification.

## F4-B HTTP completion / 2026-10-21 Asia/Karachi

Developer supplied independent public gemini-3.8-flash/audio/WAV/Free Tier contract
verification, resolving implementation prerequisite while user-project eligibility
remains unverified. Six official pages still local403; agent read official Google
REST/SDK sources. Implemented bounded fixed-model GeminiAnswers on existing authority/
DPAPI, enabled only behind current phone/Windows authorization and Free Tier review.
No new trust/protocol architecture, dependencies or animation rewrite. Fake HTTP
coverage47 cases plus retained61 Link tests pass locally (108 total); initial xUnit
substring analyzer issue corrected. CI/artifacts pending, physical/live untested.
No real provider request/key, hardware microphone or billing configuration used.

Completion evidence: implementation685a5e883ffcfdca784641d2d086a885409871cb.
Windows CI39494775781 passes Core27/Link108/native26/WPF clean/self-contained x64;
Android CI39494775790 passes102 tests/lint0 errors18 warnings/debug APK signature.
Matching artifacts/checksums uploaded; Android manifest and JUnit downloaded/verified.
Windows archive download blocked at sa15, CI evidence only. Exact hashes/artifacts/
20 changed paths and signer-change reinstall guidance in STATUS. No real provider
request, key, hardware microphone, billing, new PR, merge or later milestone.
Public-contract implementation complete; user eligibility and physical/cloud
acceptance remain unverified. Next: one offline physical retest, provider offline,
keys empty and microphone denied; any live test needs separate authorization.


## F4-B response repair / 2026-10-22 Asia/Karachi

Active, not physical-verified. Developer reports five Gemini requests,two503s and
usage but no proven visible answer; the three other outcomes remain unknown.
Seven local fake-HTTP cases reproduced request/classification gaps; eight authority
cases reproduced collapsed LAN status. Intentional regression commit58278fe Android
CI39671465812:103 tests,one failure proving coalesced reply/done skips pink with
chirps disabled. This does not identify physical provider response contents.
Current repair local Core27/Link124 pass. Final CI/artifacts pass below;
shared synthetic trace covers reply and caption boundaries. D012 records budget,
error policy, privacy and limited physical attribution. All F3/F4-A acceptance and
unmeasured performance boundaries remain unchanged.


Response repair final app 36287bcdcb95496c3da8f61973a51fc1e337913f: [Windows39674027367](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39674027367) passes
Core27/Link124/WPF0 warnings-errors/29 native checks/self-contained x64;
[Android39674027307](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39674027307) passes111 tests/lint0 errors18 warnings/APK signature.
Matching artifacts/manifests uploaded; exact hashes/34 paths/signer in STATUS.
Local downloads blocked at sa2/sa10, CI evidence only. Earlier PR test39673123298
failed one scheduler-dependent20ms cleanup assertion; awaited entry/cleanup retains
all byte/no-stale-reply assertions and passes final CI. Physical/provider repaired
behavior and user project eligibility remain UNVERIFIED. No real Codex API call.


## F4-C stacked checkpoint / 2026-10-22 Asia/Karachi

Active code/CI slice, explicitly authorized before F4-B physical acceptance. Base
4b38029 on port/live-ai-voice; PR7 targets that branch, no PR6 modification/merge.
Application fc832f8890d8dc0f8e565b74a4a79b4687343828: Windows CI Core27,Screen32,
Link141 pass; native35 checks and WPF0 warnings/errors; Android111 tests/lint0 errors
18 warnings. Matching Windows39834969073/Android39834970366 artifacts/evidence
uploaded with CI SHA manifests. Final downloads policy-blocked at sa19/sa9; earlier
6f471fa downloads rehashed413 Windows entries+APK, superseded. STATUS records
exact links/hashes/signer. Native codec/UIA checks use public fixtures; WGC capture
not executed. Documentation closeout changes no application code. Native OCR deployment BLOCKED for portable output: Microsoft
requires package identity. No false recognition/capture claim or silent installation.
F4-B/F4-C physical/provider acceptance stays unverified. See D013 and combined QA.


## F4-C native capture repair / 2026-10-24 Asia/Karachi

Physical gate remains pending: developer observed truncated titles and failed capture
before preview, so OCR/UIA/pointing was not accepted. Red native d32b1e4 Windows
40006819441 reproduced Unicode title failure and shared D3D E_NOINTERFACE. Microsoft
header identifies a mistyped IDXGIDevice IID;16efb78 two-line fix passes native setup.
Final app fdc9572ee9e305cc489d9784a50f865943355cf5: local/Windows Core27,Screen35,Link141 and42 native
checks pass, WPF0 warnings/errors; Android111 tests/lint0 errors18 warnings pass.
Windows40008459044 / Android40008459792 publish matching artifacts/manifests.
Windows413-file manifest and EXE rehashed after download; exact links/hashes in
STATUS. Native probes do not start capture/read pixels; physical primary/window
first-frame/preview remains UNVERIFIED. OCR package identity remains separately
BLOCKED; no packaging/provider/F5 or PR6 modification/merge. Last8 safe diagnostics
contain no title/pixels/private text/credentials. Canonical QA gives one local-only
retest; next is developer physical primary and Notepad checks with cloud OFF.


## F4-C ItemSize geometry repair / 2026-10-25 Asia/Karachi

Developer-reported Unicode fix accepted; two Notepad attempts fail ItemSize/Changed
before frames, no preview, cloud OFF. Actual laptop sizes are unobserved; no capture/
OCR/pointing physical pass. Old API-extents equality is removed as an allocation
precondition, not replaced with a guessed coordinate scale. WGC bounds the pool;
measured rectangles and DPI context govern verified mapping, unknowns preview-only
with all coordinate/target/share paths blocked. Freshness retains both bounds.

Final app df1eb62d52ea325a16fda67d2e9624097c4e1303: Windows40133905687 Core27/Screen53/Link141,44 native
checks/WPF0 warnings-errors pass; Android40133905040 111 tests/lint0 errors18 warnings.
Matching artifacts/manifests uploaded; finalWindows413 entries/EXE rehashed. STATUS
contains24 paths/hashes/links. CI public window DWM=WGC306x203,Win32320x210,1x,exact
mapping; border mismatch cases are synthetic. No native frame acquisition in CI.
First-frame/preview/physical point accuracy remains UNVERIFIED; OCR identity and F4-B
provider gates stay separate. PR6/7 open, unmerged. Next is one local Notepad retest,
cloud OFF, preserving preview-only denial and reporting only safe geometry metadata.

F4-C FirstFrame repair /2026-10-25 Asia/Karachi: developer reports five real
FirstFrame/Changed failures; DWM578x557/WGC=Win32594x565/DPI1.25/WindowFrame confirms
prior ItemSize mapping, not capture success. Source latch conflates null and size
mismatch. Bounded sequential acquisition/Recreate and safe frame diagnostics now
implemented; local regression tests pass. Windows/native and Android CI pending
source push. Real preview/pointing remains UNVERIFIED; OCR packaging and F4-B live
provider gate remain separate. No cloud call or input operation authorized/run.

FirstFrame final automated gate: application f3856770710d789cb7396e6080c13d7950c4c8b2;
Windows40222661491 succeeds with238unit/45native checks, clean WPF and self-contained
x64 artifact; Android40222661934 succeeds with111tests/lint0errors18warnings/APK.
Stacked PR40222665650/40222665604 checks also pass. Downloaded Windows/evidence and
rehash413manifest entries; EXE78d0eeb41540bcf36b616ee3e83a3d41058f10d879822ae7abd96e82d03cd483.
Native Recreate is exercised without StartCapture; no real first frame in CI.
Exact physical null-versus-size diagnosis, successful preview/500,500 pointing,
Stop cleanup/exclusion/DPI/performance remain UNVERIFIED. No cloud provider calls.

## F4-C physical laptop JPEG diagnosis / 2026-10-26 Asia/Karachi

Gate remains **active**, successful real preview **not observed**. PR #7 head
91c9d3f; application artifact f3856770 (same binary as reviewed head). Existing
Windows folder verified against413 manifest entries, all match. See STATUS for
full binary hashes, paths and current build limitation.

Safe diagnostic read directly from the running Bluey GUI after the requested
harmless Notepad test (cloud OFF; no screenshot retained in the repository):

```text
capture#1 Window JpegEncode NativeFailure HR=0x88982F41 516ms DWM=578x557 WGC=578x557 delta=0x0 Win32=594x565 dpiScale=1.25 map=VisibleBounds initial=594x565 frames=2 recreates=1 [f1 null=False content=578x557 buffer=594x565 pool=594x565 item=594x565 Recreate 359ms; f2 null=False content=578x557 buffer=578x557 pool=578x557 item=594x565 Accepted 391ms]
```

| Evidence tier | Observed result |
| --- | --- |
| Physical application pipeline | Selected HWND/WGC item/D3D/session/frame/recreation/bitmap copy succeed; first failing stage is JpegEncode. No Snapshot/WPF preview. No null frame or movement failure in this attempt. |
| Independent local native codec | Invalid ImageQuality metadata call reproduces0x88982F41. Correct encoder-creation option yields578x557 and1100x619 JPEG round trips with generated contrast preserved. No screen capture or production assembly executed by probe. |
| Local source correction | ImageQuality moved to CreateAsync options; production native smoke adds laptop-sized nonblank round trip. No geometry/consent/provider/capture-backend changes. |
| Corrected production build/tests | UNRUN: .NET SDK10.0.402 absent; CI requires approved push to existing PR #7. |
| Corrected physical acceptance | UNVERIFIED: real preview, coordinate accuracy, UIA text, stationary-mouse pointing and Stop/discard. Portable OCR identity unavailable. |

Microsoft references: [WIC error codes](https://learn.microsoft.com/en-us/windows/win32/wic/-wic-codec-error-codes),
[encoding options](https://learn.microsoft.com/en-us/windows/apps/develop/media-authoring-processing/bitmapencoder-options-reference),
[BitmapEncoder](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.imaging.bitmapencoder).
No Gemini request/upload, fallback capture or toolchain installation. Preserve
previous CI evidence as historical; it does not validate this unbuilt repair.
