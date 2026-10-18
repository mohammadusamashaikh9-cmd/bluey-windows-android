# Current verified state

2026-10-18: F3 implementation in progress on `port/secure-device-link`, based
directly on fetched `origin/main` `a80642ab2aa897e6f80baed081ce2beb3e9a6d3d`.
GitHub confirms F2 PR #3 squash-merged; both F1 Android and F2 Windows are present.
No prior F3 changes existed to transfer. F1 PR #2 remains merged.

The developer reports the F2 functional smoke test accepted. This is user-reported
functional evidence, separate from still-unverified measured frame pacing, CPU,
memory and broader desktop/DPI/visual QA. F1 phone/performance QA remains pending.
No F0–F2 implementation is reopened.

D009 records the F3 pinned mutual-TLS/out-of-band invitation profile. Native Windows
host/NSD/DPAPI/approval/revocation and Android NSD/Keystore/client/foreground settings
are implemented for validation. Shared strict codec fixtures, actual loopback TLS
and regression tests are being run. CI and physical results are not yet claimed.
Apple/iOS/Mac/Shared source and original previews are unchanged. No F4+, privileged
endpoint, plaintext compatibility, installer/signing/release or merge is included.
