# Current verified state

2026-10-19 (Asia/Karachi): F3-R2 corrects Android Keystore TLS digest authorization
and adds actual on-device signing checks. **Physical ApprovalPending/authenticated
session with this correction remains UNVERIFIED.** Existing PR #4 is open/unmerged
on port/secure-device-link; R2 started clean at 1bb55768bd15b617a4e3586ffacd6b0134e759c3.
No branch reset/rebase or duplicate PR. F1 PR #2 and F2 PR #3 remain merged; F2
functional smoke is user-reported accepted, detailed measured QA stays open.

## Physical evidence and source-confirmed correction

The developer supplied two October 18 phone/laptop attempts correlated by UTC/client
port: 21:57:00/53148 and 21:59:10/53192. Both reach Windows TCP, validate the server
pin and select/provide Android EC key/chain, then fail in TLS. Windows reports
TcpAccepted/TlsStarted/PeerClosed before its certificate callback, mutual TLS,
hello or enrollment. This proves TCP reachability for these attempts and supersedes
R1's uncertainty there. QR/firewall/discovery and differing client/server pins are
not the demonstrated failure. These are user-reported physical observations.

Source-confirmed incompatibility: the Android key was generated with SHA256/384/512
but no DIGEST_NONE. Android's official setDigests contract requires that authorization
for common TLS prehashed signing; Conscrypt CryptoUpcalls requests NONEwithECDSA for
opaque EC keys. KeyMint uses the separately authorized NONE operation. The old
production-spec regression failed in Android CI 39171259060 (43 tests, one expected
failure), then passes with NONE authorized for newly generated P-256 keys.

This is a concrete key-capability defect and evidence-supported correction, not a
claim that the actual phone's provider rejection has been captured or pairing fixed.
No EKU, certificate format, pin, TLS versions, enrollment or storage profile change
was justified. Production still uses pinned mutual TLS 1.2/1.3, the strong five-minute
invitation, both request-bound approvals, protected keys/trust and revocation. No
plaintext fallback, weak TLS hash, trust-all validator or firewall automation.

The off-UI signing check signs a fresh random challenge with SHA256withECDSA and
its SHA-256 digest with NONEwithECDSA, then verifies both with the existing public
certificate. It reports NONE authorization, certificate self-signature/KU/EKU flags
and fixed outcomes, never challenge/signature/private bytes. It runs manually and
before networking. Existing immutable keys are never silently replaced: an explicit
confirmation stops sessions, deletes the phone identity/host trust, then requires
revoking its old Windows trust and fresh enrollment. Fresh installation makes a
corrected key automatically. DPAPI identity is preserved.

Safe diagnostics v2 retain 96 in-memory events with UTC/local IDs/source port,
fixed JCA/provider/recognized TLS alert categories and available Windows native
code/HRESULT. ServerCertificateSelected confirms .NET selection of the same stored
server identity; ClientValidationNotReached distinguishes failures before the app's
client validator. Unknown alerts are not guessed or logged as raw messages. TLS
1.2/1.3 software tests independently reach EnrollmentValidated/ApprovalPending,
confirmation and face delivery; physical Keystore/Schannel remains separate.

## Actual tests, artifacts and checksum evidence

Both retest applications below are from **3f6ca5afb49ec404a2ad0533af28ab890242cdb7**. Later closeout is tracking only.

| Target | Actual outcome | Artifacts |
| --- | --- | --- |
| [Windows CI 39171721298](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39171721298) | Windows-2025/.NET 10.0.402: 27 motion + 26 link tests, no failures/skips; WPF zero warnings/errors; self-contained publish; 12 native checks and 12 offscreen renders | [Windows x64 + SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39171721298/artifacts/12152781645), [TRX/native/render evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39171721298/artifacts/12152926030) |
| [Android CI 39171721076](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39171721076) | JDK17/Gradle8.13/SDK36: 48 tests, no failures/errors/skips; lint zero errors/15 warnings; apksigner verification/debug APK; 17 renders | [APK + SHA256SUMS + SIGNING-CERT](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39171721076/artifacts/12152857235), [JUnit/lint/render evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39171721076/artifacts/12153061376) |

PR merge-tree [Windows 39171723385](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39171723385) and
[Android 39171723392](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39171723392) also pass. Local final
`dotnet test Bluey.Link.Tests -c Release` passes all 26 tests; result
`windows/TestResults/r2-final.trx` is ignored/generated, not committed. Both codecs
retain the unchanged 40-case fixture corpus. New tests cover the actual production
key specification, JCA SHA/prehash matching and mismatched public key, unavailable
opaque key without export, fixed error privacy, pre-validation socket closure,
independent TLS versions through confirmation/face delivery, and rejection/revocation.
No mock TLS or software key is called hardware Keystore/Schannel proof.

Downloaded Android app/evidence: APK checksum matches; parsed APK-v2 public signer
matches CI apksigner report; 48-test JUnit/lint XML and 17 renders inspected; all six
original font/OFL assets match source bytes. Windows app/evidence downloads were
blocked by this cloud proxy at `productionresultssa7.blob.core.windows.net`.
Windows build/tests/checksums/upload are observed in actual CI logs and API metadata;
**no local Windows archive/per-file re-verification is claimed for R2**.
A draft adds only that host, preserving existing domains; saved, requires publish,
not applied. No bypass/relay/rehosting or disabled verification was used.

Windows folder is self-contained, untrimmed, unsigned win-x64 (no extra desktop
runtime/SDK). Android is debug 0.3.2-f3-r2/versionCode5. Artifact retention: 14 days.
Use these linked matching-commit builds, not a later independent debug rebuild.

SHA-256 (Windows values from actual CI; Android independently checked locally):

- `Bluey.Windows.exe`: `8fc454681c871f14b658479de3a9538262aea06807f4404f5535da9d5fa09af9`
- `Bluey.Windows.dll`: `b8eb51f668cadb274af26e6d6e40405dc48ad09a604eed599a671a50f2a8622a`
- Windows `SHA256SUMS.txt`: `f21fb5ad82961b1a881f8b100272e45b90ee74492ae195c541f5e0120e156563`
- `app-debug.apk`: `51b0e4f6478bf457f840594b210677636ff3212ccf09e3aab5f535e43e6b7da6`

Debug signer differs from delivered R1: old `1ee1aef4c6f8eb8c7b0b2ceddf289ebdaf7d3eac2755f9c4a6aa7369020a7bff`,
new `de4459d57c8d3441698a3aba9a37dc4a7ffeef67a87925aaf235d7c47e540134`. Updating that baseline needs uninstall/install and new
phone identity/enrollment. This deletes phone data and its old key; preserve Windows
DPAPI identity and revoke any old phone entry. No signing private key was exported,
committed or uploaded. APK signer is separate from both TLS identities.

## Exact R2 changed files (relative to R1 closeout)

- `android/app/build.gradle.kts`
- `android/app/src/main/java/dev/bluey/port/link/AndroidIdentity.kt`
- `android/app/src/main/java/dev/bluey/port/link/ConnectionDiagnostics.kt`
- `android/app/src/main/java/dev/bluey/port/link/ConnectionPanel.kt`
- `android/app/src/main/java/dev/bluey/port/link/DeviceLink.kt`
- `android/app/src/main/java/dev/bluey/port/link/IdentitySigning.kt`
- `android/app/src/test/java/dev/bluey/port/link/ClientIdentityKeyManagerTest.kt`
- `android/app/src/test/java/dev/bluey/port/link/ClientKeyPolicyTest.kt`
- `android/app/src/test/java/dev/bluey/port/link/FailureCategoryTest.kt`
- `android/app/src/test/java/dev/bluey/port/link/IdentitySigningTest.kt`
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
- `windows/README.md`

Original Mac/iOS/Shared, previews and existing Windows/Android motion/render source
are unchanged. No F4+, networking rewrite, hosted relay, firewall automation, new
PR or merge. Phone signing/handshake, ApprovalPending/session, live synchronization,
Wi-Fi recovery and physical latency remain unverified for R2. Follow the single
retest in [HANDOFF](HANDOFF.md).
