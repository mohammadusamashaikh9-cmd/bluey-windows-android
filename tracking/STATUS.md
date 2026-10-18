# Current verified state

As of 2026-10-18 (Asia/Karachi), F2 Windows implementation/build/native CI checks pass;
physical Windows desktop acceptance remains UNVERIFIED.

- Accepted baseline: main merge `15194dec8356385b15dc6712630d45e4761cd1de`.
  GitHub API confirms [F1 PR #2](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/2)
  merged at this SHA. Branch `port/windows-overlay` was created directly from
  fetched `origin/main`, never from the Android feature branch.
- F0 is accepted. F1 implementation remains merged; its outstanding phone
  visual/lifecycle/input and performance QA is retained, without reopening code.
- Implemented: pure Kotlin source-math animator, seven moods, original vector face
  and gradients/clipping, cached vector blur, original fonts/OFL notices, offline
  secondary demo controls, touch gaze, foreground-only frame clock and immersive
  sensor-landscape activity. No system permissions requested.
- [CI run 39070720349](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39070720349)
  completed successfully: Kotlin compilation, 23 tests (17 pure + 6 native renderer),
  lint and debug APK. XML confirms zero failures/errors/skips. Lint has zero errors
  and 11 warnings; 17 offscreen PNGs were produced and reviewed against previews.
- [Debug APK + SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39070720349/artifacts/12114897856)
  and [test/lint/render evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39070720349/artifacts/12114808444)
  are CI artifacts retained for 14 days. Artifact archive digest is not the APK
  digest; downloaded APK matches its SHA256SUMS:
  `9f7d5442ee6089f28c04dac6d34cb676b3e9d3e8d294ed3f53af86b567e83aed`.
  All six packaged font/OFL files match accepted Git blobs byte-for-byte.
- F2 now has a pure C# motion/geometry/effect library and native WPF/Win32
  primary-monitor demo, separate recoverable controls, hotkeys and local settings.
  D008 selects .NET 10/WPF, Windows 11 x64; no third-party runtime libraries.
- Cloud Linux .NET SDK 10.0.402: 27 pure xUnit tests passed, zero failures/skips.
  WPF cross-build and self-contained win-x64 publish passed, zero warnings/errors.
  These Linux results do not execute Windows desktop behavior.
- [Windows CI 39116112594](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39116112594)
  at code commit `f3506e00737f66502d2a3f5cf7961251e1291708` succeeded on
  windows-2025/.NET SDK 10.0.402: **27 tests, 0 failed/skipped**, WPF build
  **0 warnings/errors**, self-contained win-x64 publish, **9 native checks**
  and **12 offscreen frames**. Native checks use actual HWND styles/messages,
  show/refit/recovery and bundled Fredoka, not mocked windows. PR run
  [39116116581](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39116116581)
  passed the same checks against the proposed merge tree.
- [Windows development artifact + SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39116112594/artifacts/12132670694)
  and [native/TRX/render evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39116112594/artifacts/12132560972)
  are uploaded, retained 14 days. Development ZIP is approximately 65.5 MB,
  self-contained/untrimmed/unsigned; no laptop SDK/IDE or desktop runtime install.
  Downloaded and verified all 406 published file hashes; reviewed the corrected
  label/speech frames and all body/trail fixtures. EXE SHA-256:
  `29d00ec3cfce61dd382eea81a66b6859bac93ec263f279d6a1d6784c1dcb9868`. Original embedded TTFs match byte-for-byte;
  OFL notice text matches with checkout newline normalization.
- [F2 PR #3](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/3)
  is open into main, mergeable and unmerged. Tracking closeout changes no Windows source, tests or workflow; the tested
  source commit above is authoritative.
- Apple/Mac/iOS/Shared/build/history, previews and Android files are unchanged.
  No networking, pairing, microphone, OpenAI, OCR, capture, computer actions or
  backend were added. No installer, signing, release or merge is authorized.
- F2 stays active: real click-through/focus/Alt+Tab, DPI/display changes, external
  desktop capture interaction, visual fidelity, frame pacing and CPU/memory on
  the ~8 GB laptop are UNVERIFIED. F3–F6 remain planned.

See [CHECKPOINTS](CHECKPOINTS.md), [DECISIONS](DECISIONS.md),
[SESSION-LOG](SESSION-LOG.md) and [Windows instructions](../windows/README.md).
