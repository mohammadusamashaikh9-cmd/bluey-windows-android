# Session log

Append-only meaningful entries; date basis Asia/Karachi. No synthetic success.

## 2026-10-18 — Foundation baseline and source inspection

User authorized canonical docs, rules, four skills and tracking, followed by a
validated commit/push on existing `planning/foundation` and one PR to `main`;
no merge or app implementation. Repository cloning/fork creation was user-executed.

Observed commands/results:

- `git remote -v`, `git branch --show-current`, `git status --short`,
  `git rev-parse HEAD`, `git branch -vv`, `git rev-parse --verify origin/main`:
  expected origin URL, existing planning branch, clean initial worktree and pin
  `6d84d91456f3bf7ad9b31caf31f19710b0ba8328`. `upstream/main` lookup failed
  because no upstream remote exists; direct URL verification used instead.
- `git diff --exit-code 6d84d91456f3bf7ad9b31caf31f19710b0ba8328 HEAD` and
  baseline tracked-tree diff: empty/pass. `git rev-parse HEAD^{tree}` returned
  `51432419619f17f70e999e5fd244b053bd62bac3`.
- Initial sandbox `git ls-remote` could not connect. Authorized network execution
  of `git ls-remote origin refs/heads/main refs/heads/planning/foundation` and
  `git ls-remote https://github.com/rbrown101010/bluey-by-riley.git refs/heads/main`
  returned the pin for each main; no remote planning branch at baseline.
- `gh repo view mohammadusamashaikh9-cmd/bluey-windows-android --json nameWithOwner,url,isFork,parent,defaultBranchRef`:
  fork true, correct parent, default main, correct URL. This verifies repository
  state, not the historical creation action.
- `rg --files` and worktree ZIP/instruction searches: no prior canonical docs,
  AGENTS or setup archive found; only upstream preview assets under docs.
  Read README/license, shared contracts/palette, face/shape/cursor engines,
  networking/voice, OCR/accessibility/control, settings/secrets, session/notes
  and relevant UI/build/font owners. [PORTING-MAP](../project/PORTING-MAP.md)
  records observed semantics and proposed replacements.
- Official Microsoft Learn/OpenAI documentation pages checked for the candidate
  docs MCP endpoints; [TOOL-POLICY](../project/TOOL-POLICY.md) records sources.
  No global MCP configuration, dependencies or SDKs installed.
- Existing `python` is Python 3.11; `python -c "import yaml"` failed with
  `ModuleNotFoundError`. The skill creator's `quick_validate.py` imports PyYAML,
  so that validator is unavailable without an added dependency. Use a documented
  dependency-free check for these simple two-scalar frontmatters; do not install.

Limitations: Git warned it could not read the user's global ignore file inside
the sandbox. Explicit untracked-path review is required before staging. Source
inspection does not establish Apple runtime behavior, model availability or any
target platform/build/device/CI result. Foundation final checks remain pending.

## 2026-10-18 — F0 static validation

Added a reusable standard-library static checker under the checkpoint skill;
it runs no application logic. First run found skill links one parent too high;
corrected all four skills and reran successfully. Command:
`python .agents/skills/bluey-checkpoint-qa/scripts/validate_foundation.py`.
Observed successful output:

```text
PASS: 23 Markdown files; 173 local links resolved; 10 external link occurrences not fetched by this checker
PASS: 4 skill frontmatters, folder names and nonempty instructions (restricted plain-scalar check; not the PyYAML validator)
PASS: 6 strict JSONL packets, legacy keys, face values and nested tool JSON
PASS: 25 allowed changed paths; original source/build/assets and README body preserved; git diff --check BASE
```

`BASE` is the pinned upstream commit. `python --version` returned 3.11.9;
`gh --version` returned 2.96.0. Reviewed source details against docs: local saved
Swift Date versus Unix notes timestamps, plaintext LAN/no frame cap, declared
legacy fields versus active commands, text/chirp output, recording defaults and
prompt-level versus executable action controls. Explicit port proposals are not
described as existing source capabilities.

Sandbox process launcher subsequently failed twice with
`helper_unknown_error: setup refresh had errors`; authorized escalated execution
ran static/file/Git commands successfully. No automatic approval rejection occurred.
No tool/dependency installation used to resolve this limitation. External links
are not all fetched; only repository metadata/refs and official MCP documentation
were verified live. The PyYAML-based skill validator and behavioral skill tests
were not run; simple frontmatter/link checks passed instead. No build, API,
EXE/APK, CI, application, Apple reference runtime or real-device test ran.

F0 static requirements pass. Commit/push/PR and staged-new-file whitespace checks
will be recorded only after their commands complete; F1–F6 remain planned.

## 2026-10-18 — Staged foundation checks

`git add -- AGENTS.md README.md PORTING.md project tracking .agents/skills/bluey-source-porting .agents/skills/bluey-animation-fidelity .agents/skills/bluey-device-protocol .agents/skills/bluey-checkpoint-qa`
staged exactly the 25 allowed files. `git diff --cached --check` passed and the
static helper passed again with the same counts after staging new files.
`git diff --cached --stat` / `--name-only` confirmed docs, four skills, one
synthetic fixture and one static validation helper only. Git reported expected
LF-to-CRLF working-copy conversion warnings under existing configuration; no
configuration or original source encoding was changed. Publication still pending.

## 2026-10-18 — Publication privacy rejection

The validated task commit was created locally. `git push -u origin planning/foundation`
was rejected by GitHub `GH007` because the configured author email is private.
No remote planning branch was created; `git ls-remote` still showed only baseline
main, and `gh pr list --head planning/foundation --base main --state open` was empty.
This was a GitHub email-privacy rejection, not an automatic approval rejection.

`gh api user --jq '{login: .login, id: .id}'` confirmed the authenticated owner
and numeric ID. [GitHub's official email reference](https://docs.github.com/en/enterprise-cloud%40latest/account-and-profile/reference/email-addresses-reference)
documents the ID-based no-reply format. The task-created, unpushed commit will
use that no-reply identity through a command-local override; no global/repository
identity setting, account privacy preference or published history will be changed.

## 2026-10-18 — Foundation publication verified

After repeating the static checker and `git diff --cached --check`, the unpushed
task commit's author/committer were corrected with
`git -c user.email=296289031+mohammadusamashaikh9-cmd@users.noreply.github.com commit --amend --no-edit --reset-author`.
`git log -1 --format='%H %ae %ce'` confirmed both no-reply fields. No pre-existing
or published commit was rewritten. Resulting foundation commit:
`5cd0ec24dab4d169d8ca231c126807489ecb4430`.

`git push -u origin planning/foundation` succeeded without force. `git ls-remote`
confirmed that SHA for the planning branch and unchanged main at the upstream pin.
`gh pr create --repo mohammadusamashaikh9-cmd/bluey-windows-android --base main --head planning/foundation --title 'Establish source-verified Windows and Android port foundation' --body-file <temporary UTF-8 body file>`
created [PR #1](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/1).
The body describes final scope, actual checks and limitations. PR attached to this
Codex chat using the app artifact tool.

`gh pr view 1 --repo mohammadusamashaikh9-cmd/bluey-windows-android --json url,state,isDraft,baseRefName,headRefName,headRefOid,files,commits,mergedAt`
confirmed OPEN, base main, head planning/foundation at the foundation SHA, exactly
25 allowed changed files, non-draft and mergedAt null. `git status --short` was
empty and `git show --format= --check HEAD` passed. No merge/release performed.

This follow-up updates only current tracking/publication evidence in the same PR.
Repeat the static checker and staged whitespace check before its commit, use the
same command-local no-reply identity, push normally and verify current remote/PR
head equals local HEAD. No application or device verification is added by this
tracking closeout. Next owner action: review PR #1 and its provisional decisions.

## 2026-10-18 — F1 baseline and implementation

User authorized full native Android face slice, CI builds/tests, commit/push on
existing `port/android-face` and one PR to main, without merge. `git remote -v`,
branch/HEAD/status confirmed correct fork, clean branch at accepted foundation
`55a1c62abaf3fae19b944fc2335f2f2e8adc2299`. Direct remote main lookup matched;
`gh pr view 1 --json state,mergedAt,mergeCommit` confirmed PR #1 MERGED at that SHA.
This supersedes earlier current-state notes describing it as open.

Read AGENTS and the three requested local skills, relevant product/technical/QA
docs and original FaceView/BlobShape/Palette/fonts. Visually inspected original
expressions.png and listening.png. No JDK/Gradle/Kotlin executable found on PATH.
D007 records tool/renderer choices before implementation; no local SDK/tool install.

Wrote pure Kotlin state/springs with clock/random injection; vector renderer with
source geometry, palette, clipping/layer order and cached vector blur; lifecycle
gated fullscreen Compose demo with secondary mood/gaze/talk controls; deterministic
JUnit and native offscreen-render cases; minimal read-only-permission Android CI
with test evidence and debug APK artifact upload. No Windows/network/audio/API
implementation. Original source/assets/build paths diff from foundation is empty.
`git diff --check` passed for tracked edits; staged/CI checks pending. No compile,
unit, lint or rendering result is claimed until the actual CI run returns.

### F1 first CI setup correction

Initial push received a transient empty server reply; normal retry succeeded.
[Run 39070247973](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39070247973)
at `f958f7563efd44405aa866377177e4cfc5b39911` failed in SDK setup, before Gradle:
the action's default `tools platform-tools` included the discontinued `tools`
package (`Failed to find package 'tools'`). Set packages explicitly to
`platform-tools` and suppress verbose license text; SDK 36/build-tools remain
explicitly installed in CI. Source review also corrected the radial shine endpoint
to transparent white, retaining the source hue during alpha interpolation.

### F1 compilation and first test results

[Run 39070333603](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39070333603)
at `5d3a2b9a7eb2f54007da157910f5409feba961b0` compiled Android and test Kotlin,
then ran 23 tests: 22 passed, one rendered-pixel assertion failed. All 17 pure
animator/geometry tests passed. The failed assertion expected the center of a
fully blinked eye not to be white, but Swift deliberately preserves a 0.06-open
sliver. Correct the test to sample above the sliver and explicitly require the
center to remain white; no renderer change is needed for this failure. Save its
image before assertions so future failures retain the reproduction. Also remove
nullable render-path and old Kotlin compiler-DSL warnings. Lint/APK packaging
were not completed in this failed run; offscreen render evidence was uploaded.

### F1 successful CI and visual evidence

[Run 39070720349](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39070720349)
at `e231228188a515cef24d0b104d3b1ea5fd2b70e0` completed SUCCESS. Actual command
from `android/`:

```text
gradle --no-daemon --console=plain :app:testDebugUnitTest :app:lintDebug :app:assembleDebug
```

Gradle reported BUILD SUCCESSFUL in 1m12s. Downloaded the test evidence using
`gh run download 39070720349 --name android-test-evidence-e231228188a515cef24d0b104d3b1ea5fd2b70e0 --dir build/f1-evidence/verified/evidence`.
Parsed JUnit XML: FaceAnimatorTest 17 tests, FaceRendererTest 6, all zero
failures/errors/skips. Native graphics ran API 35/JDK 17. Counted 17 PNGs.
Lint XML has 0 errors and 11 warnings: pinned versions/target SDK notices (6),
fixed-orientation policy (1), future backup extraction rules (1), demo launcher
icon (1), optional KTX bitmap style suggestions (2). No suppression or baseline
hides them. SDK 36 is the selected compatible F1 target, not store-readiness.

Inspected generated listening/happy/sleepy frames against original listening and
expressions previews: silhouette, crown, gradients, highlight, eye spacing,
happy arcs/mouth and sleepy clipping follow the source. Capture states/poses are
not identical and upstream previews have no deterministic trace metadata; this is
qualitative geometry/composition review, not pixel equality or physical acceptance.
Android blur/antialiasing/text metrics and GPU display remain calibration items.

[APK artifact 12114897856](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39070720349/artifacts/12114897856)
contains app-debug.apk and SHA256SUMS.txt; archive size 11,028,390 bytes.
[Evidence artifact 12114808444](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39070720349/artifacts/12114808444)
contains reports/XML/renders. `gh api .../actions/runs/39070720349/artifacts`
confirmed IDs/names/sizes. Archive SHA-256 is separate from APK SHA-256.
No local SDK was installed; evidence downloads remain under ignored `build/`.

Opened and attached [PR #2](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/2),
head port/android-face, base main, no merge. Updated canonical current owners to
describe accepted F0 and implemented F1. Workflow also runs on main after eventual
owner merge. Follow-up is tracking/workflow only; tested Android sources remain
unchanged. F1 physical-phone acceptance stays UNVERIFIED; F2–F6 remain planned.

Local validation so far: 23 Markdown files / 177 local links and anchors resolved,
Android XML parsed, exact F1 path allowlist and original-source preservation passed,
and staged `git diff --check` passed. Counts will change with added evidence links;
repeat final checks after closeout. F0's helper has a foundation-only allowlist,
so F1 uses scoped link/XML/path checks rather than pretending that old scope fits.

### F1 APK inspection

Downloaded the successful APK with
`gh run download 39070720349 --name bluey-android-debug-e231228188a515cef24d0b104d3b1ea5fd2b70e0 --dir build/f1-evidence/verified/apk`.
Python hashlib SHA-256 matched CI's SHA256SUMS file:
`9f7d5442ee6089f28c04dac6d34cb676b3e9d3e8d294ed3f53af86b567e83aed`.
APK is 11,489,883 bytes and contains DEX/manifest. ZIP inspection verified all
four original TTFs and both OFL notices against the accepted foundation Git blobs.
An initial comparison against Windows working-copy bytes differed for the text
notice because of CRLF conversion; canonical Git-blob comparison passed for all
six files. No font/license modification was made. Also inspected successful
blink/resting/thinking PNGs: the blink sliver is present, z glyphs and thought dots
render, and paths remain dynamic. This is artifact/offscreen evidence only.

### F1 closeout checks

Final scoped Python/static check passed: 23 canonical/skill Markdown files,
179 relative links/anchors, Android XML parsing, 27 changed paths inside the
F1 allowlist, no original Apple/source/assets/build changes, and no Android
source difference from successful CI commit `e231228188a515cef24d0b104d3b1ea5fd2b70e0`.
`git diff --check` passed. APK metadata is documented separately from CI archive
digests; no device result inferred. PR #2 verified OPEN, main base, correct head,
mergedAt null. Commit/push of this tracking/workflow closeout and final remote
head/clean-worktree checks follow; no merge or additional feature work authorized.

## 2026-10-18 (Asia/Karachi) — F2 Windows implementation

User authorized .NET 10/WPF/Win32 Windows 11 x64, complete offline F2, commit/push
and one PR into main without merge. Clean checkout originally at F0; native
`git ls-remote origin refs/heads/main` returned accepted F1 merge
`15194dec8356385b15dc6712630d45e4761cd1de`. Fetched main and created
`port/windows-overlay` from `origin/main`. GitHub API independently confirms
PR #2 merged at that SHA. No branch from `port/android-face`; no Android edits.

Read AGENTS and source-porting/animation/checkpoint skills, F2 authority docs and
original CursorOverlay/Settings/HotKeys/AppDelegate/Fonts/Palette. Inspected cursor
and flight PNGs plus middle comet/string GIF frames; the latter show tail layering
and phone tether but are not seeded numerical or physical Windows references.
D008 records the authorized target and source-to-C# boundaries before scaffolding.

Installed official SDK 10.0.402 from builds.dotnet.microsoft.com into cloud-only
`/workspace/bluey-tools/dotnet`, with SHA-512 checked against official .NET 10
release metadata. No local laptop SDK/IDE install. Cloud first-run required
writable DOTNET_CLI_HOME/NUGET_PACKAGES under /workspace, then an approved SDK
IPC/mutex sandbox escalation. No signature/checksum/TLS bypass.

Git proxy read access worked immediately. Initial API/Learn requests received
proxy 403; saved additive `api.github.com` and `learn.microsoft.com` destinations,
then retried after propagation and both worked. No new credential was requested.
Read official Microsoft Learn extended window styles, layered-window hit-testing,
WM_DPICHANGED and RegisterHotKey: layered WS_EX_TRANSPARENT passes input through;
HTTRANSPARENT alone has same-thread limits. Implementation combines layered/
transparent/toolwindow/noactivate, MA_NOACTIVATE, no-activate positioning and a
regular reachable control window. Physical input/focus behavior is still unverified.

Actual cloud commands from `windows/` (SDK helper only initializes cloud paths):
- `dotnet test Bluey.Core.Tests -c Release --logger 'trx;LogFileName=core.trx' --results-directory TestResults`: first execution 20 passed/3 failed. Independent source review corrected three test expectations: launch(.5)=0.7585734, above-target caption y=262, and a second launch puff when redirecting still near home. Rerun: **23 passed, 0 failed, 0 skipped**.
- `dotnet build Bluey.Windows -c Release`: first build exposed WPF/Core VisualState name collision; explicit alias fixed it. Rerun: **passed, 0 warnings/errors** using Windows reference pack; WPF not executed on Linux.
- `dotnet publish Bluey.Windows -c Release -r win-x64 --self-contained true -p:PublishTrimmed=false -o artifacts/bluey-win-x64`: **passed**, approximately 142 MB extracted. Native CI execution and artifact upload are next.

The F0 checker has a foundation-only change allowlist; it is not appropriate for
authorized F2. Use scoped links/manifest parsing, original-source preservation,
actual C# tests/build and whitespace checks. No application networking, microphone,
capture, input injection, backend, Docker, WSL, installer or release was added.

### F2 first Windows CI diagnosis

Initial unpublished push was rejected by GitHub email privacy. Corrected only the
new unpublished commit using verified API user ID/login no-reply metadata; did
not change account privacy or existing history. Pushed `a157c7413aa502057f80a9d06bee6969babf7d3e`
and opened [PR #3](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/3),
head `port/windows-overlay`, base main, unmerged.

Windows run [39114965265](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39114965265)
passed tests, native WPF build and publish; native smoke step failed. Runnable
artifact upload was correctly skipped; failure evidence was uploaded. Log/artifact
redirect hosts were blocked by environment proxy 403. Saved the two observed
hostnames in environment draft; runtime access still requires settings propagation.
Added explicit GitHub error annotation to the native smoke step so a failure's
actual exception is available through the already-working GitHub API, rather
than relying solely on the inaccessible log download. This is diagnostic only;
no failing assertion disabled or behavior acceptance weakened.

Native exception observed through run 39115191543 annotations: WPF rejects
`ClipToBounds=true` on a Window (`Window.CoerceClipToBounds`), during overlay
construction. Removed that Window assignment; the inner OverlayArt Canvas already
clips artwork. This fixes startup without weakening the native check. Original
source and pure engine unchanged. Also corrected lingering Windows-planned text
in PORTING/TRD to the D008-selected implementation boundary.

Further native run 39115349241 still failed after the clipping correction, before
a normal failure report was written. Logs became accessible and showed the
runner's diagnostic file lookup failed, hiding the process exit/exception. Added
test-only AppDomain fatal-exception recording and redirected process stdout/stderr;
normal assertions remain intact. Artifact redirects also used observed storage
host productionresultssa0.blob.core.windows.net; saved additively.

Integration review moved remaining effect sampling, spring-pop timing and speech
path construction into the pure library (Presentation/Geometry). Normalized spring
initial velocity to animation-distances/second, matching source API units. Added
four deterministic cases for spring/endpoints/cloud-tail paths: cloud **27 passed,
0 failed/skipped**; WPF cross-build **0 warnings/errors**. This is independent of
the unresolved native smoke diagnosis; no platform result is promoted.

Run 39115541861 captured the actual fatal exception: nonfinite engine coordinates
during WPF MediaContext.Resize/HWND creation. Native ConfigureOverlay's frame
change can dispatch Rendering before FitPrimary initializes DisplayTransform;
zero DPI caused nonfinite logical mouse coordinates. Added a lifecycle guard
(`IsLoaded`, initialized DPI, and not resizing) before ticking; clear the HWND
after close. The engine's finite-input assertion remains. Native smoke also refits
a live HWND and checks opaque blueberry interior pixels in each render, not just
nonempty images. These checks use real WPF/Win32, not mocked styles.

### F2 Windows CI success

[Push run 39115697048](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39115697048)
at exact source commit `533d30bc711e5a1f75d2f8816b1b855ee97fa673` succeeded.
[PR run 39115700400](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39115700400)
also succeeded on proposed merge tree `9cb38cc84c05d96f88901eae7d784c62621a74ed`.
Runner windows-2025, reported Microsoft Windows NT 10.0.26100.0; SDK 10.0.402.
Actual commands are in `.github/workflows/windows.yml` and Windows README.

- Pure tests: **27 passed, 0 failed/skipped** on Windows (PR log duration 174 ms).
- Native WPF build: **0 warnings, 0 errors**; self-contained untrimmed win-x64 publish passed.
- Native smoke: **9 passed, 0 failed**: actual layered/transparent/noactivate/toolwindow styles, visible HWND, foreground unchanged on show in CI session, actual HTTRANSPARENT/MA_NOACTIVATE messages, primary-DPI/refit dimensions, hidden-window recovery, embedded Fredoka resolution, twelve current WPF renders. Pixel checks require transparent outside and an opaque blue body interior. No physical click injection or desktop capture.
- Uploaded [development folder](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39115697048/artifacts/12132207156) (65,541,730-byte archive) with per-file SHA256SUMS, original font notices and LICENSE, plus [TRX/native/render evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39115697048/artifacts/12132103530). Retention 14 days. No installer/signing/release.

First successful artifact downloads redirected to additional observed storage
hosts productionresultssa8 (PR) and productionresultssa5 (push). Added both while
preserving earlier allowed hosts; current downloads still need runtime settings
propagation. No archive/hash/image inspection is claimed yet. API logs independently
confirm the checks and uploads. Physical desktop/capture/fidelity/performance
acceptance remains UNVERIFIED; original Apple and Android sources are unchanged.

### F2 artifact inspection found and corrected label placement

Downloaded push run 39115697048 after network propagation. All **406 published
files** matched the per-file SHA256SUMS; self-contained coreclr and WPF framework
were present. Original OFL text matched after newline normalization (Windows
checkout uses CRLF). TRX independently confirms 27 executed/passed, no skipped
or failed tests; native JSON confirms nine checks and twelve actual WPF frames.

Reviewed a contact sheet of all twelve frames against source PNGs/GIF frames and
literal geometry/timings. Body/rim/eyes/trails appear, but temporary-label glyphs
escaped their background: FormattedText was centered in its maximum measurement
width even though the background used the narrower measured text width. Set the
label's text alignment width to the measured width. Added a native bitmap
regression: the fixed speech fixtures' body/speech/label must draw no pixels in
the rightmost 200 pixels of the 1000-DIP canvas. Existing smoke checks had not
covered glyph containment. Rebuild/re-run and fresh final artifacts are required.

Qualitative source comparison also shows WPF glow is narrower/sharper than the
upstream preview; do not claim Core Graphics blur parity. Exact geometry/colors/
timings remain source-matched numerically; kernel/text/pacing calibration and
physical desktop acceptance remain open.

### F2 final source CI

The label fix source commit `f3506e00737f66502d2a3f5cf7961251e1291708` passed
[push run 39116112594](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39116112594)
and [PR run 39116116581](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39116116581).
All 27 pure tests, WPF build (0 warnings/errors), self-contained publish, nine
native checks and twelve render fixtures (including label containment) pass.
Final [development archive](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39116112594/artifacts/12132670694)
is 65,541,894 bytes; [test evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39116112594/artifacts/12132560972)
is uploaded. The original four embedded font binaries were also extracted from
the prior built assembly with System.Resources.ResourceReader and SHA-256 matched
against each original Shared TTF; no font conversion or new art asset. The same
resource links and assets are unchanged in final source.

Final artifact host is observed productionresultssa17.blob.core.windows.net,
saved additively for download verification. Final tracking closeout changes no
Windows implementation/test/CI files after the successful source commit above.

Final artifact access propagated successfully. Downloaded run 39116112594 using
`gh run download`; **all 406 files** match SHA256SUMS, and the manifest covers
every published file except itself. Final EXE SHA-256:
`29d00ec3cfce61dd382eea81a66b6859bac93ec263f279d6a1d6784c1dcb9868`.
Published directory total including manifest: 147,550,957 bytes.
Final TRX independently confirms 27 executed/passed, 0 failed/errors/notExecuted;
native JSON confirms nine checks, no failures. Reviewed the corrected speech-top
and speech-center frames: temporary labels now contain their text. Body/glow,
comet and string fixtures also inspected. ResourceReader extraction of final
assembly confirms all four original TTF binaries are byte-identical. OFL notices
are text-identical with Windows checkout CRLF normalization.

The final code remains `f3506e00737f66502d2a3f5cf7961251e1291708`; closeout
updates only canonical evidence/guidance, not tests/application/workflow. F2 is
implementation/CI-passed, physical acceptance UNVERIFIED. F1 PR #2 remains merged
with outstanding phone/performance QA, and F3–F6 remain planned.

Final scoped closeout checks: **24 Markdown files / 188 local links and anchors**,
C# project/props/manifest XML, **40 files** inside authorized F2 paths, original
Apple/Android/Shared/previews/builds and upstream README body preserved, Windows
source/tests/workflow identical to successful code commit, and `git diff --check`
passed. Final tracking commit/push and PR metadata confirmation follow. No merge.

## 2026-10-18 UTC — F3 secure device link implementation and CI

- User resolved the prerequisite: GitHub confirms F2 PR #3 squash-merged into main
  `a80642ab2aa897e6f80baed081ce2beb3e9a6d3d`. Fetched it, verified both F1/F2 source
  trees and clean worktree, then created `port/secure-device-link` directly from
  that baseline. No previous F3 source changes needed transfer. F1 PR #2 remains
  merged. F2 functional acceptance is user-reported; measured QA is not inferred.
- Read project authority/local protocol, source-porting and checkpoint skills;
  inspected legacy GooglyLink/PhoneServer/MacLink and the existing engine/animator.
  D009 recorded the concrete security profile before relying on it. Verified
  official Microsoft DNS-SD/TLS/DPAPI and Android NSD/Keystore/permission semantics.
- Implemented bounded v1 strict codecs/40 shared fixtures with expected values,
  pinned mutual TLS, five-minute 256-bit out-of-band QR/URI enrollment, explicit
  request/identity-bound approvals, protected storage, trust/revocation, native
  discovery, bounded foreground reconnect and latest-slot face synchronization.
  Android uses the existing animator with once-only active-clock receipt; original
  math/render source and Apple assets remain unchanged. No F4+ capability added.
- Actual early CI failures were fixed: missing Kotlin discovery brace, redundant
  Windows ProtectedData package (NU1510), and Windows nonexportable-key re-export
  during DPAPI initialization. The final implementation exports the new PFX once
  before loading its nonexportable runtime certificate. Local plaintext rejection
  test initially needed its network stream retained before socket half-close.
- Final app source `c27a9d40020a5f1cb351771f8ab00087cd06c166`:
  `/workspace/bluey-tools/dotnet-run test windows/Bluey.Link.Tests -c Release
  --logger 'trx;LogFileName=link.trx' --results-directory windows/TestResults`
  passed 13/13 locally (zero skipped). An earlier implementation WPF cross-build
  passed with zero warnings/errors on Linux SDK 10.0.402; final compilation is
  verified by Windows CI below. No physical Windows behavior is implied.
- Windows [push CI 39125741305](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39125741305)
  passed 27 Core + 13 Link tests, WPF build (0 warnings/errors), self-contained
  win-x64 publish, 12 actual DPAPI/HWND/render checks and 12 PNGs. TRX/native JSON
  downloaded and inspected: no failures/skips. Actual TLS tests include failed
  authentication, both approvals, expiry logic, request/identity mismatches,
  revoke/reconnect, disconnect cleanup, slot timeouts and real socket backpressure.
- Android [push CI 39125741198](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39125741198)
  passed compilation, 33 tests (17 animation, 6 renderer, 10 link), lint (0 errors,
  15 warnings) and debug APK. JUnit XML confirms no failures/errors/skips; 17 PNGs.
  Target 36 is retained; Nearby is optional for Android 16 opt-in restrictions.
  Custom X509 lint warning was inspected: exact DER pin/date/P-256 verification
  is intentional for the self-signed out-of-band profile, not accept-all trust.
- PR merge-tree runs 39125744104 (Windows) and 39125744102 (Android) also passed.
  Both push-run application artifacts and evidence downloads were retrieved.
  Verified all 409 Windows manifest hashes and APK SHA256SUMS; packaged Android
  fonts/OFL files match source bytes. EXE SHA-256:
  `03cca249cfedf1858ba6abd521330c9d35c7c9bc507f53a51b542e19a795e0a9`;
  APK SHA-256:
  `3bb839420c5cf050a4022ac21e4f174e4d084ee44cbf99d33a5ed172d618f38d`.
  [STATUS](STATUS.md) owns exact artifact links (14-day retention).
- Static review: no diff in Mac/iOS/Shared, original Apple build/assets/previews,
  Android animator/renderer or Bluey.Core; changed Markdown file links resolve and
  whitespace checks pass. Opened one [PR #4](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/4)
  into main; no merge. Cloud startup guidance was saved as an onboarding draft;
  review/save/publish in environment settings is needed only for reuse.
- Physical pairing, Keystore/SChannel device interoperability, NSD/firewall/OS
  permission behavior, Wi-Fi loss/reconnect, live synchronization and measured
  latency remain UNVERIFIED. F1/F2 performance QA remains open separately. Exact
  next action is the paired-device artifact acceptance sequence in HANDOFF.
