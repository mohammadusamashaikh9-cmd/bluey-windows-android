# Handoff

F3 implementation is on `port/secure-device-link`, based directly on F2 squash merge
`a80642ab2aa897e6f80baed081ce2beb3e9a6d3d`. [PR #4](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/4)
is open into main; **do not merge automatically**. F1 PR #2 and F2 PR #3 are merged.
The developer reports F2 functional smoke accepted; F1/F2 measured-performance and
detailed physical QA remain outstanding separately.

Application commit `c27a9d40020a5f1cb351771f8ab00087cd06c166` passed actual Windows
and Android CI: 27 motion + 13 link xUnit tests, 33 Kotlin/native-render tests,
40 shared codec fixtures per language, WPF build/publish, 12 native checks and
lint (zero errors, 15 warnings). See [STATUS](STATUS.md) for exact runs, both
application/evidence downloads, verified hashes and limits. All 409 Windows file
hashes, the APK and original packaged Android fonts/notices were checked.
Subsequent closeout changes only docs/tracking; these are the tested app artifacts.

D009 uses pinned mutual TLS and an out-of-band five-minute 256-bit QR/full URI,
explicit identity approval on both devices, DPAPI/Keystore protected trust and
revocation. `_bluey-sec._tcp` provides address hints only. Initial scope is private
IPv4 on the same LAN, one saved Android host. No plaintext fallback or F4+ endpoint.
Original graphics/motion and Apple source are preserved. No laptop SDK, IDE,
emulator, Docker or WSL is required to run the development builds.

**Exact next action:** install the checksum-verified Windows development folder
and Android debug APK linked in STATUS on the actual laptop/phone, then execute
[the F3 physical sequence](../project/QA.md#f3-automated-and-physical-acceptance):
start Windows secure host on its private Wi-Fi IPv4, scan/paste a fresh invitation,
compare both full fingerprints, explicitly Pair/Approve and record live mouse gaze,
mood/talk updates, touch/stale behavior, background/resume, Wi-Fi loss/reconnect,
host shutdown/restart, revocation and actual latency. Record model/OS/network and
artifact digests; redact the invitation. Physical success and performance cannot
be inferred from CI. No physical-device result has been observed by the agent.
