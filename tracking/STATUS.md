# Current verified state

As of 2026-10-18 (Asia/Karachi), Android F1 implementation and CI checks pass;
physical-device acceptance remains UNVERIFIED.

- Repository/branch: `mohammadusamashaikh9-cmd/bluey-windows-android`, existing
  `port/android-face`, started clean at accepted foundation
  `55a1c62abaf3fae19b944fc2335f2f2e8adc2299`. PR #1 is confirmed merged at that SHA.
- [F1 PR #2](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/2)
  targets main and is unmerged. Android source tested at
  `e231228188a515cef24d0b104d3b1ea5fd2b70e0`; later tracking/workflow closeout does
  not change those Android sources.
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
- Original Mac/iOS/Shared/Apple builds and preview assets remain unchanged. No
  local Java/Gradle/SDK installation, Studio, emulator, Docker or WSL was used.
  No Windows/network/pairing/audio/OpenAI/OCR/control/notes/backend code was added.
- F0 accepted. F1 stays active pending phone visual/input/lifecycle/fullscreen and
  FPS/thermal acceptance. F2–F6 remain planned. No EXE, store or release result.

See [CHECKPOINTS](CHECKPOINTS.md), [DECISIONS](DECISIONS.md) and
[SESSION-LOG](SESSION-LOG.md) for authority, commands, failed runs and limitations.
