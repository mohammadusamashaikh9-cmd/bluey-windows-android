# Handoff

F2 Windows overlay is implemented on `port/windows-overlay`, from accepted main
merge `15194dec8356385b15dc6712630d45e4761cd1de`. F1 PR #2 is confirmed merged;
its physical-phone/performance QA remains outstanding and Android is unchanged.

[PR #3](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/3)
is open into main; do not merge automatically. Windows source at
`f3506e00737f66502d2a3f5cf7961251e1291708` passed
[CI 39116112594](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39116112594):
27 pure tests, WPF build/publish, nine actual native checks and twelve offscreen
frames. Artifact is self-contained win-x64, unsigned, with SHA256SUMS, retained
14 days. See [STATUS](STATUS.md) for artifact/evidence links and verified EXE digest. All 406 file hashes and
original embedded fonts were checked; corrected label/speech renders reviewed.

Exact next task: on the Windows 11 x64 laptop, extract the development artifact,
verify SHA256SUMS and execute the primary-monitor desktop acceptance sequence in
[Windows instructions](../windows/README.md#required-physical-windows-acceptance--unverified).
Record click-through/focus/Alt+Tab, hide/show/recovery, 100/150/200% DPI, external
capture interaction, equivalent source/target animation and 60 s frame/CPU/memory
measurements tied to that artifact hash. No SDK/IDE/WSL/Docker install is needed.
F2 remains active until physical evidence exists. No F3, installer, signing,
release, networking, capture feature or computer actions are included.
