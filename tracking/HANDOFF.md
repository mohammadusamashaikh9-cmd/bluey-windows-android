# Handoff

F3-R1 updates existing [PR #4](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/4)
on `port/secure-device-link`; **do not merge**. Starting commit was
`24d44b794f1caf272f5d134793951e1ee4f0e37e`. Both revised retest artifacts use app
commit `9da4ecb148ab7f36a563150e5ed360a5242fadf2`; later closeout is tracking only.
[STATUS](STATUS.md) contains exact file changes, test/evidence/artifact links,
verified SHA-256 values and the debug-signer comparison.

Confirmed repairs are bounded safe stage diagnostics, separation of discovery
status, and a cancellation guard preventing cancelled socket IOException handlers
from clearing a newer pairing attempt. The guard has a real socket/coroutine
red/green regression. No physical TCP/TLS root cause is established. D009 remains:
mutual TLS, exact pin, high-entropy five-minute invitation, explicit approvals,
protected keys/trust, revocation, bounds and no downgrade. EC alias selection is
extracted/tested, not replaced with a new security profile.

Actual app-commit validation: Windows 27 motion + 18 link tests, WPF build/publish,
12 native checks/12 renders; Android 42 tests, lint zero errors/15 warnings, verified
APK signature/packaging, 17 renders. Both codecs retain 40 shared fixtures. Downloaded
all four final artifacts; all 409 Windows manifest hashes and APK digest match.
Software-key JSSE and .NET no-EKU P-256 TLS 1.2/1.3 passes are not phone hardware
Keystore/SChannel evidence. F1/F2 measured performance remains open separately;
F2 functional smoke is user-reported accepted.

**One precise next action:** run the [single-attempt physical retest](../windows/README.md#f3-r1-one-safe-physical-pairing-retest)
with these exact builds:

1. Verify checksums; stop old Windows Bluey and replace at its existing firewall-
   allowed EXE path. Keep its LocalAppData/Bluey/Link DPAPI identity intact.
2. The revised APK signer differs from the previously delivered c27a9d4 APK.
   If that is installed, uninstall/install (old app data and phone key are lost),
   revoke any old Windows phone entry, and re-enroll. Do not bypass signature
   checks or export private keys; no local SDK is required.
3. Keep both apps foreground on the same Wi-Fi. Start host on its current private
   IPv4; generate/scan a fresh invitation, compare the server pin across Windows
   and Android, tap Pair **once**. If ApprovalPending appears, compare the phone pin
   across the approval displays and explicitly approve. The two identities' pins
   are expected to differ.
4. Immediately copy **only Copy safe diagnostics** from both apps after success/
   failure. Supply those texts plus OS/model/approximate attempt time and artifact
   digests. Review/redact before sharing; never supply the QR/full URI, settings
   screenshots, credentials, private keys, raw traffic or unrelated system logs.
5. Correlate UTC and client TCP source port (A/W IDs are local). No retained Windows
   TcpAccepted means this listener did not observe that attempt, not proof of a
   particular firewall/router cause. Android TLS_COMPLETED is local completion;
   Windows MutualTlsCompleted/EnrollmentValidated/ApprovalPending locate progress.
   If both sides confirm the session, test gaze/mood/talk and record latency;
   otherwise return the two safe logs at the failing stage.

Physical reachability, provider/key behavior, successful enrollment/synchronization
and Wi-Fi recovery remain unverified until this device retest. No F4, architecture
rewrite, firewall automation, new PR, signing-key publication or merge is authorized.
