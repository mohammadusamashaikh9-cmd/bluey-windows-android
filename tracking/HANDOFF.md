# Handoff

[F1 PR #2](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/2)
is open from `port/android-face` to `main`; do not merge automatically.
Accepted F0 is main commit `55a1c62abaf3fae19b944fc2335f2f2e8adc2299`.

Last verified Android source: `e231228188a515cef24d0b104d3b1ea5fd2b70e0`,
[successful run 39070720349](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39070720349):
23/23 tests, lint zero errors/11 warnings, debug APK and 17 native offscreen renders.
Later documentation/workflow changes do not change tested app source. No phone
FPS/GPU or complete visual parity claim; F1 remains active.

**Next action:** install the [debug APK](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39070720349/artifacts/12114897856)
on a physical Android phone after matching SHA256SUMS. APK SHA-256:
`9f7d5442ee6089f28c04dac6d34cb676b3e9d3e8d294ed3f53af86b567e83aed`.
Record device/OS/API, resolution and refresh rate, then capture 60 seconds covering moods, gaze/talk,
blinks, idle hops, transitions and background/resume with Demo hidden for visual
comparison. Follow the [F1 procedure](../project/QA.md#android-f1-build-and-comparison).
Do not start F2/network/audio/API work as part of phone acceptance.
