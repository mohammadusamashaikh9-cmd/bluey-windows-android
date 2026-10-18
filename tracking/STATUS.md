# Current verified state

2026-10-19 (Asia/Karachi): F3-R1 repairs observability and a cancellation/error-handler
race. Both revised applications build and pass automated validation. **The actual
physical pairing failure's TCP/TLS root cause remains unconfirmed.**

- Existing branch/PR retained: `port/secure-device-link`, [PR #4](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/4), open/unmerged.
  R1 started clean at `24d44b794f1caf272f5d134793951e1ee4f0e37e`; no reset/rebase,
  duplicate PR or merge. F1 PR #2/F2 PR #3 remain merged; F2 main baseline is
  `a80642ab2aa897e6f80baed081ce2beb3e9a6d3d`.
- F2 functional smoke is user-reported accepted; detailed physical/performance QA
  for F1/F2 remains open separately. Neither implementation is reopened.
- Both retest application artifacts below are from **the same revised app commit**
  `9da4ecb148ab7f36a563150e5ed360a5242fadf2`. Later closeout edits are tracking only.

## Physical evidence versus confirmed code defects

The developer reports Windows starts/listens on private IPv4, Wi-Fi is Private,
and a Private-profile application-scoped inbound TCP rule exists. Android displays
a fresh invitation and Pair is tapped, then reports generic failure; Windows never
shows approval. Phone-to-listener TCP reachability was not definitively captured.
These are user-reported observations, not agent-performed device tests. Different
phone/server TLS certificate fingerprints are expected, not a failure cause.

Confirmed source defects/repairs:

- Android collapsed pre-approval errors into one message; Windows silently discarded
  failures. Red/green tests reproduce missing diagnostics. Both now expose fixed
  stages/categories and at most 96 in-memory entries, with local attempt ID, UTC
  and client TCP source port. Safe-copy excludes input/URI/secret, packet content,
  certificate subject, private key, exception message and raw TLS traffic.
- A cancelled blocking socket read can throw IOException. Its old active-error
  handler could publish failure and clear a newer pairing intent. A real loopback
  coroutine regression failed before an active-context guard and passes afterward.
  Cancelled attempts now stop before those state mutations. This mechanism is
  confirmed in software; it is not proven to have caused the reported phone failure.
- Discovery status is separate, so late DNS-SD callbacks cannot replace the displayed
  connection failure. The EC key manager is extracted with its existing selection
  policy and opaque Keystore key access, now observable and testable.

[D009](DECISIONS.md) remains unchanged in security profile: platform mutual TLS
1.2/1.3, exact server pin, five-minute 256-bit invitation, explicit request-bound
approvals, DPAPI/Keystore protection, revocation and all wire bounds. No plaintext
fallback, new trust mode, certificate/digest replacement or firewall automation.
No general EC-alias/EKU incompatibility was demonstrated: real JSSE software-key
handshakes and Windows Android-shaped P-256/no-KU-or-EKU leaves pass TLS 1.2/1.3.
Actual phone Keystore/SChannel behavior remains unverified.

## Actual results and retest downloads

| Target | Observed results at the revised app commit | Downloads |
| --- | --- | --- |
| [Windows CI 39167980064](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39167980064) | windows-2025/.NET 10.0.402; 27 motion + 18 link tests, zero failures/skips; WPF build zero warnings/errors; self-contained publish; 12 native checks and 12 offscreen renders | [Windows x64 + SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39167980064/artifacts/12151577200), [TRX/native/render evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39167980064/artifacts/12151338396) |
| [Android CI 39167980079](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39167980079) | JDK 17/Gradle 8.13/SDK 36; 42 tests, zero failures/errors/skips; lint zero errors/15 warnings; signature verification and debug APK; 17 offscreen renders | [APK + SHA256SUMS + SIGNING-CERT](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39167980079/artifacts/12150539268), [JUnit/lint/render evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39167980079/artifacts/12151446782) |

PR merge-tree runs [Windows 39167984704](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39167984704)
and [Android 39167984471](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39167984471)
also pass. Both retain all 40 shared protocol fixtures and existing animation,
revocation/no-downgrade and renderer checks. New tests cover alias/engine selection,
nonexport of opaque keys, real JSSE TLS 1.2/1.3/missing identity, .NET Android-shaped
certificates, diagnostics privacy/bounds, and cancelled real socket IO. No mocked
TLS result is used as evidence of hardware interoperability.

Downloaded all four final artifacts and inspected XML/native/lint/signing evidence.
Verified all **409 Windows file hashes**, APK checksum, APK v2 public signer versus
CI apksigner report, and original packaged Android font/OFL bytes. Files are
unsigned/self-contained untrimmed Windows x64 and Android debug 0.3.1-f3-r1; no
laptop SDK, IDE, emulator, Docker or WSL required. Artifacts retain for 14 days.

SHA-256:

- `Bluey.Windows.exe`: `ed0119d049581f35ea219310b229603c30575b25a70a607c2c664b14edafbcee`
- `Bluey.Windows.dll`: `aeb61e372f1225bd7a307093e703aba7b14ded9216205fde4d23c58f8bd5a22d`
- Windows `SHA256SUMS.txt`: `f2052a5826a10f2e0013fb091c48fd8f6514e684bc421aa4cc17fe88fee7ba45`
- `app-debug.apk`: `d0ba7da6c45d412093e325d4c35547601016879112b7c9a5d8fba015c23a1b1c`

APK signer continuity was **not preserved** against the previously delivered
`c27a9d4` APK (APK digest `3bb839420c5cf050a4022ac21e4f174e4d084ee44cbf99d33a5ed172d618f38d`):
old signer `3b9672e457c4e410703eda61d035c457e8b067f79dca2e1725a988c34ce98596`,
new signer `1ee1aef4c6f8eb8c7b0b2ceddf289ebdaf7d3eac2755f9c4a6aa7369020a7bff`.
That baseline needs uninstall/install and fresh enrollment. Uninstall deletes its
old data/Keystore identity; revoke any old Windows phone trust entry if present.
Keep the Windows DPAPI identity. These APK signing certificates are separate from
both TLS identities. No signing private key was exported, committed or published.

## Exact R1 changed files (relative to the starting commit)

- `.github/workflows/android.yml`
- `.github/workflows/windows.yml`
- `android/app/build.gradle.kts`
- `android/app/src/main/java/dev/bluey/port/link/AndroidIdentity.kt`
- `android/app/src/main/java/dev/bluey/port/link/ClientIdentityKeyManager.kt`
- `android/app/src/main/java/dev/bluey/port/link/ConnectionDiagnostics.kt`
- `android/app/src/main/java/dev/bluey/port/link/ConnectionPanel.kt`
- `android/app/src/main/java/dev/bluey/port/link/DeviceLink.kt`
- `android/app/src/test/java/dev/bluey/port/link/CancellationBoundaryTest.kt`
- `android/app/src/test/java/dev/bluey/port/link/ClientIdentityKeyManagerTest.kt`
- `android/app/src/test/java/dev/bluey/port/link/DiagnosticTest.kt`
- `android/app/src/test/java/dev/bluey/port/link/FailureCategoryTest.kt`
- `project/QA.md`
- `tracking/CHECKPOINTS.md`
- `tracking/DECISIONS.md`
- `tracking/HANDOFF.md`
- `tracking/SESSION-LOG.md`
- `tracking/STATUS.md`
- `windows/Bluey.Link.Tests/DiagnosticTests.cs`
- `windows/Bluey.Link.Tests/TlsTests.cs`
- `windows/Bluey.Link/ConnectionDiagnostics.cs`
- `windows/Bluey.Link/LinkServer.cs`
- `windows/Bluey.Windows/Connection/ConnectionWindow.cs`
- `windows/Bluey.Windows/Connection/WindowsLinkHost.cs`
- `windows/README.md`

Original Mac/iOS/Shared, Apple builds/history, previews and existing pure Windows
and Android motion/render source are unchanged. No F4+, capture, computer actions,
backend/relay, installer/release or merge. Physical TCP/TLS root cause, hardware
key interoperability, successful pairing/live sync/Wi-Fi recovery and latency
still need the exact single-attempt retest in [HANDOFF](HANDOFF.md).
