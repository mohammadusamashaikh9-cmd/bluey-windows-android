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
| F3 secure device link | Active; F3-R2 automated correction passed, physical confirmation pending | Protocol/security implementation + cross-device QA | User-supplied R1 physical logs prove TCP/server pin/EC identity selection, then TLS failure before approval. Source defect: missing Keystore DIGEST_NONE for Conscrypt prehashed signing; corrected new-key policy, actual on-device sign/verify checks and explicit replacement. App `3f6ca5a`: [Windows 39171721298](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39171721298), 27 motion + 26 link tests/build/publish/12 checks; [Android 39171721076](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39171721076), 48 tests/lint/signature/APK. Independent TLS 1.2/1.3 pending/confirmed-session tests pass. Android download verified; Windows download locally proxy-blocked, actual CI evidence observed. Hardware root mechanism and corrected ApprovalPending/live session still UNVERIFIED; [STATUS](STATUS.md), [HANDOFF](HANDOFF.md). |
| F4 voice/context/privacy | Planned | Voice/context implementation + privacy QA | Current API review, authorized live run, permissions/capture/OCR/pointing and disconnect tests; no execution |
| F5 optional control/notes | Planned, optional | Scoped feature owner + safety/data QA | Application confirmation/stop or bounded recording/transcription/retention tests as selected; no execution |
| F6 release/demo | Planned | Release owner + artifact/device QA | Reproducible builds, hashes, CI/device evidence and truthful demo/docs; no execution |

No provisional ZIP/C0/C1 records were present in the checkout. No legacy gate is
counted as complete. If that kit is later supplied, retain its IDs only as aliases
to F0–F6 after checking actual scope/evidence; do not assume `C1` means `F1`.
Never double count fork creation as F0 implementation or infer completion from
an alias. [STATUS](STATUS.md) holds current verified facts; this table owns gate state.
