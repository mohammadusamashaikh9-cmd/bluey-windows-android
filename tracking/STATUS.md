# Current verified state

2026-10-18 UTC: F3 implementation and automated validation pass; physical Windows ↔
Android pairing, synchronization and latency acceptance remain **UNVERIFIED**.

- Branch `port/secure-device-link` was created directly from fetched main
  `a80642ab2aa897e6f80baed081ce2beb3e9a6d3d`, the verified F2 PR #3 squash merge.
  Both native applications were present; no prior F3 changes needed transfer.
  F1 PR #2 remains merged at `15194dec8356385b15dc6712630d45e4761cd1de`.
- The developer reports **F2 functional smoke accepted**. No new physical logs or
  measurements were supplied. Detailed desktop/DPI/capture/fidelity and measured
  frame pacing/CPU/memory QA remain separate and unverified. F1 phone/performance
  QA is retained; neither F1 nor F2 implementation is reopened.
- Application source: `c27a9d40020a5f1cb351771f8ab00087cd06c166`.
  [PR #4](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/4)
  targets main and is unmerged. Subsequent closeout changes only canonical evidence.

## Implemented F3 boundary

[D009](DECISIONS.md) selects platform TLS 1.2/1.3 with mutual certificate proof,
out-of-band exact server pin plus five-minute 256-bit invitation, explicit approval
on both devices, request-bound host approval, protected trust and revocation.
DNS-SD `_bluey-sec._tcp` is an untrusted endpoint hint. No plaintext fallback.
Windows uses DPAPI and native DNS-SD; Android uses Keystore signing/AES-GCM,
platform NSD, foreground-only connection lifetime and bounded secure reconnect.

Strict [v1 contract/fixtures](../protocol/README.md) preserve all seven moods,
gaze/talk ranges and LF JSON with frame/rate/deadline/session limits. Latest face
slots keep socket work outside animation; Android delivers each fresh packet once
into the existing animator, retaining touch precedence and stale behavior. Initial
scope is private IPv4 LAN, one saved Android host and eight trusted Windows peers.

## Actual validation and artifacts

Both push runs below tested the same application commit above. The PR merge-tree
runs [Windows 39125744104](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39125744104)
and [Android 39125744102](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39125744102)
also passed.

| Target | Observed results | Downloads |
| --- | --- | --- |
| Windows [CI 39125741305](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39125741305) | windows-2025/.NET 10.0.402; 27 motion + 13 link tests, zero failed/skipped; WPF build zero warnings/errors; self-contained publish; 12 actual native checks and 12 offscreen renders | [Development folder + SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39125741305/artifacts/12136511403), [TRX/native/render evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39125741305/artifacts/12135683844) |
| Android [CI 39125741198](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39125741198) | JDK 17/Gradle 8.13/SDK 36; 33 tests, zero failures/errors/skips; 17 offscreen renders; lint zero errors, 15 warnings; debug APK | [APK + SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39125741198/artifacts/12136176936), [JUnit/lint/render evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39125741198/artifacts/12135928058) |

Downloaded all four artifacts. Verified **all 409 Windows file hashes** and the APK
against their manifests, and all original packaged Android font/OFL bytes against
source. Artifacts are retained 14 days. Windows is unsigned/self-contained,
untrimmed win-x64; no laptop SDK/IDE or extra desktop runtime is required. Android
is a development debug APK, not a store release.

- `Bluey.Windows.exe` SHA-256:
  `03cca249cfedf1858ba6abd521330c9d35c7c9bc507f53a51b542e19a795e0a9`
- `app-debug.apk` SHA-256:
  `3bb839420c5cf050a4022ac21e4f174e4d084ee44cbf99d33a5ed172d618f38d`

The 13 link tests include real TLS enrollment/approval/confirm, authentication
failures, reconnect/revoke, plaintext rejection, identity mismatch, pending-phone
cleanup, slot deadlines and actual socket backpressure. Both codecs execute the
same **40 fixtures and expected decoded values**. Windows native checks include
real DPAPI identity/trust reload, persistent revocation and corruption rejection.
Cloud Linux also passed all 13 link tests; an earlier implementation passed WPF
cross-compilation. Final WPF compilation is verified by Windows CI above.

Lint warnings are recorded, not suppressed: pinned custom X509 trust is intentional
and validates exact DER pin, P-256 key and validity; remaining warnings cover pinned
dependency/target versions, manifest/backup/orientation metadata and KTX style.
This is not a physical Android Keystore or cross-device TLS result.

Mac/iOS/Shared source, Apple builds/history, previews and existing Windows/Android
pure animation/render source are unchanged. No F4+, microphone, cloud API/token,
capture/OCR, computer actions, backend, installer/signing/release or merge exists.

Exact next action: follow [HANDOFF](HANDOFF.md) to pair these checksum-verified
builds on the actual Windows laptop and Android phone and record synchronization,
Wi-Fi loss/reconnect and measured latency. F3 remains active until that evidence.
