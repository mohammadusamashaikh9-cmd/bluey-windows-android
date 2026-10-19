# Current verified state

2026-10-19 UTC: F3-R3 repairs saved-identity endpoint recovery after host restart.
Existing branch `port/secure-device-link` / [PR #4](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/4) remains open and
unmerged. R3 started clean at `78d4a6133f4ae9a7c3ec71061123e25cd2dbff45`; no reset/rebase/new PR/merge.
Both supplied apps use **`d0a7a9b4a5882b5bbbd4da415244d9d2d4115a6c`**; subsequent closeout is tracking only.

## Physical evidence and confirmed code defects

The developer now reports R2 real Windows/Android authenticated mTLS pairing,
live mouse gaze, mood synchronization, Android touch precedence, Wi-Fi recovery
and intentional revocation rejection passed. These are user-reported physical QA,
not agent-operated tests. No device/OS matrix or measured latency was supplied.
This supersedes R2's pending functional pairing status. F3 still needs restart QA;
F1/F2 detailed visual/performance acceptance remains separate. F1 PR #2 and F2 PR #3
stay merged; F2 functional smoke remains user-reported accepted.

Restart reproduction: after Windows exits/reopens and the user starts the secure
host, Android reports repeated TCP_CONNECTING/TCP_TIMEOUT/no discovered host over
3–5 minutes. The supplied video description does not independently establish what
Windows was listening on at each attempt; physical old/new IP/port and the precise
DNS-SD failure were not captured. No firewall/TLS/identity failure is inferred.

Confirmed source failure mechanism: Windows binds port0, which requests an OS-assigned
port; Android persists that port and has no endpoint fallback without discovery.
A restart can therefore leave it retrying a stale endpoint indefinitely. Red/green
regressions also reproduce Android retaining a failed discovery listener and its
busy resolver across stop/start, blocking recovery of the next discovery generation.
These defects are confirmed in code/tests; the precise physical route is not claimed.

## Implemented behavior and security boundary

- Production listens on **TCP47654**, bound exclusively to the explicitly selected
  private IPv4. Conflict/unavailable-port errors leave host stopped with clear UI;
  there is no silent dynamic fallback. Host startup still requires Start secure host
  after launch. No new auto-start preference/listener or firewall mutation.
- Android tries saved endpoint, saved IP:47654 (legacy R2 dynamic-port migration),
  then bounded rotating discovery hints; an explicit manual private endpoint is
  also available. Every route retains saved host UUID/pin and the existing phone key.
  Persist a changed endpoint only after pinned mTLS and authenticated hello/ready.
- TCP refusal/timeouts and rejected untrusted hints remain retryable with existing
  bounded backoff/foreground lifecycle. Storage/key faults stop; ordinary failures
  never erase trust, start enrollment or replace identity. Disconnect stops retries.
- Discovery generation cleanup repairs failed-start/stale-resolver latches. Refresh
  discovery does not forget trust. If DHCP changes IP while multicast is unavailable,
  type the displayed private endpoint using the saved identity; no scan/new QR needed.
- D009 mutual TLS, exact pin, strong five-minute invitation, explicit request-bound
  approvals, protected stores, revocation and all protocol bounds remain. Revoke
  blocks connect; fresh authorized enrollment can reuse the same phone key.
- Safe Android diagnostics v3 include only validated private route address/port in
  addition to bounded fixed stages/categories. Windows safe-copy includes actual
  host stopped/listening state and endpoint. Review/redact LAN metadata before sharing.

## Actual final validation and artifacts

| Target | Observed outcome at the matching app commit | Downloads |
| --- | --- | --- |
| [Windows CI 39308264542](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39308264542) | Windows-2025/.NET10.0.402; 27 core + 30 link tests passed, zero skipped/failed; WPF zero warnings/errors; self-contained publish; 12 native checks/12 offscreen renders | [Windows x64 + SHA256SUMS](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39308264542/artifacts/12215465316), [TRX/native/render evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39308264542/artifacts/12215011329) |
| [Android CI 39308263802](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39308263802) | JDK17/Gradle8.13/SDK36; 55 tests passed, zero failures/errors/skips; lint zero errors/15 warnings; apksigner verification/debug APK; 17 renders | [APK + SHA256SUMS + SIGNING-CERT](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39308263802/artifacts/12215091175), [JUnit/lint/render evidence](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39308263802/artifacts/12215106118) |

PR merge-tree [Windows39308271399](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39308271399) and
[Android39308271303](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/39308271303) also pass. Local final
`dotnet test windows/Bluey.Link.Tests -c Release` passes30/30; generated TRX is
`windows/TestResults/r3-final.trx` (ignored, not committed). Unchanged shared corpus
still has40 fixtures. Existing animation/TLS/pinning/revocation/cleanup tests retained.

Actual TLS tests cover server exit while its client is authenticated, immediate
stable-port restart with saved trust/no invitation, ready/face delivery, port
conflict and retry, host not listening, stale port and changed loopback IP/port,
wrong-pin rejection, revoked connect and explicit same-key re-enrollment. Kotlin
covers absent/delayed/multiple/malformed hints, legacy-port migration, manual LAN
routing, unchanged UUID/pin, retry classification and old NSD callback generations.
NSD callbacks are simulated; these are not physical multicast/Android Keystore tests.

Downloaded APK/evidence: checksum verified, APK-v2 public signer matches CI report,
55-test JUnit/lint XML and17 renders inspected, all6 font/OFL assets match source.
Windows app/evidence local downloads were blocked by this cloud proxy at
`productionresultssa1.blob.core.windows.net`. Actual Windows CI logs show tests,
native checks, hashes and uploaded artifacts; **no local R3 Windows archive/per-file
verification is claimed**. No proxy bypass, rehosting or verification disabling.
Artifacts retain14 days. Windows is self-contained/untrimmed/unsigned win-x64,
requiring no extra laptop runtime/SDK; Android is debug0.3.3-f3-r3/versionCode6.

SHA-256 (Windows: observed CI; APK: also checked after download):

- `Bluey.Windows.exe`: `7c4f029ad98862efec5daac3bc44f8a580194120475f1eb8568c143faad52c4f`
- `Bluey.Windows.dll`: `7cb4e4dc3e14c22821a405e2f6e3a67ba219897a49fd9431ce6a8b253cbfd937`
- Windows `SHA256SUMS.txt`: `26e3b78cd9d756d005f63c03e7d8d4c0198f75d21ca7da5f3302b41348cfe899`
- `app-debug.apk`: `86306a39eb8a21d280b327da464a4f8f2b6c37d2fd27c9a1c6343a915c93cc85`

Debug signer changed versus delivered R2: old `de4459d57c8d3441698a3aba9a37dc4a7ffeef67a87925aaf235d7c47e540134`,
new `2e78aada585a01b285af3dc4baed41a0764da841ab7c468a0d7e8ca74dbd52c1`. Updating that baseline requires uninstall/install and
initial R3 enrollment, deleting its old phone app data/key. This is a development
packaging limitation, not reconnection behavior. The restart test then must reuse
that R3 identity without a QR or key replacement. Preserve Windows DPAPI identity
and revoke any obsolete phone entry. APK signer differs from TLS identities; no
signing private key was exported, committed or published.

## Exact R3 changed files (relative to R2 closeout)

- `android/app/build.gradle.kts`
- `android/app/src/main/java/dev/bluey/port/link/ConnectionDiagnostics.kt`
- `android/app/src/main/java/dev/bluey/port/link/ConnectionPanel.kt`
- `android/app/src/main/java/dev/bluey/port/link/DeviceLink.kt`
- `android/app/src/main/java/dev/bluey/port/link/Discovery.kt`
- `android/app/src/main/java/dev/bluey/port/link/ReconnectEndpoints.kt`
- `android/app/src/test/java/dev/bluey/port/link/DiscoveryLifecycleTest.kt`
- `android/app/src/test/java/dev/bluey/port/link/ReconnectEndpointsTest.kt`
- `project/QA.md`
- `tracking/CHECKPOINTS.md`
- `tracking/DECISIONS.md`
- `tracking/HANDOFF.md`
- `tracking/SESSION-LOG.md`
- `tracking/STATUS.md`
- `windows/Bluey.Link.Tests/DiagnosticTests.cs`
- `windows/Bluey.Link.Tests/TlsTests.cs`
- `windows/Bluey.Link/LinkServer.cs`
- `windows/Bluey.Windows/Connection/ConnectionWindow.cs`
- `windows/Bluey.Windows/Connection/WindowsLinkHost.cs`
- `windows/README.md`

Original Mac/iOS/Shared/previews and target animation/render math remain unchanged.
No F4, input/capture automation, relay/backend, architecture rewrite or merge.
R3 physical restart, route recovery under actual NSD/DHCP conditions, conflict UI
and same-key re-enrollment still need the single [HANDOFF](HANDOFF.md) retest.
