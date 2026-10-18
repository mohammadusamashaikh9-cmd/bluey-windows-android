# Handoff

F3-R2 continues [PR #4](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/4) / `port/secure-device-link`, open/unmerged.
Both application downloads in [STATUS](STATUS.md) are from `3f6ca5afb49ec404a2ad0533af28ab890242cdb7`;
subsequent closeout is tracking only. Do not merge or create another PR.

New physical logs establish TCP/pin/EC identity selection, then mutual-TLS failure
before enrollment. The concrete source defect is missing Android DIGEST_NONE
permission for Conscrypt prehashed EC signing. New keys authorize it. Existing keys
are immutable, retained until explicit user replacement. This is evidence-supported
correction, not confirmation of the physical provider rejection or a successful fix.
D009 pinning, mTLS, strong invitation, approvals, storage and revocation remain.

Actual final CI: Windows 27 motion + 26 link tests/build/publish/12 native checks;
Android 48 tests, lint zero errors/15 warnings/signature verification/APK/17 renders.
Independent TLS 1.2/1.3 tests reach pending/confirmed face delivery, not hardware
acceptance. APK/evidence downloaded and verified; Windows archive local download is
cloud-proxy blocked, CI evidence/checksums/uploads are observed. STATUS records exact
links, hashes, signer comparison and the 21 changed files. F1/F2 measured QA is separate.

**One next action: perform this R2 physical retest** ([full procedure](../windows/README.md#f3-r2-keystore-signing-and-mutual-tls-retest)):

1. Verify manifests and stop/replace Windows at its existing firewall-allowed EXE
   path; preserve LocalAppData/Bluey/Link. Install the matching R2 APK. Its signer
   differs from delivered R1, so uninstall/install if that baseline is installed
   (deletes phone data/key), and revoke any old phone entry on Windows. Never
   bypass signing checks or export keys.
2. Keep both apps foreground on the same Wi-Fi. Android Connection → Test protected
   signing identity. Require DIGEST_NONE_AUTHORIZED, SIGN_SHA256_PASSED and
   SIGN_PREHASH_PASSED. Copy safe diagnostics before replacement if a preserved
   old key lacks NONE and fails prehash. Only then explicitly confirm Replace phone
   identity / forget host, revoke its old Windows entry, and repeat the signing check.
   Fresh install already creates a corrected key. Other failures need their safe
   category investigated; do not silently rotate/retry around them.
3. Start Windows host/pairing, scan a fresh invitation, compare the server pin and
   tap Pair once. Require MutualTlsCompleted → EnrollmentValidated → ApprovalPending
   on Windows. Compare the phone pin across approval displays, approve explicitly,
   and require SessionReady/SESSION_CONFIRMED. Different phone/server pins are expected.
4. Only after confirmation, move the mouse and exercise gaze/mood/talk; record
   observable synchronization, latency and later Wi-Fi-loss/reconnect behavior.
5. Immediately copy only Copy safe diagnostics from both apps; review/redact and
   return both texts with OS/model/time/artifact digests. Correlate UTC/client port,
   not local A/W IDs. If signing or TLS fails, stop at that recorded stage. Never
   share the QR/full URI, screenshots of settings, credentials, private keys,
   challenges/signatures, raw TLS or unrelated Logcat/system logs.

Success is the actual phone reaching Windows ApprovalPending, followed by both
approvals and authenticated synchronization. That remains pending developer testing.
