# Handoff

F3-R3 continues [PR#4](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/pull/4) / `port/secure-device-link`, open/unmerged.
Both app downloads in [STATUS](STATUS.md) use `d0a7a9b4a5882b5bbbd4da415244d9d2d4115a6c`. Later closeout is tracking
only. Do not create another PR or merge. STATUS lists all20 changed files, observed
CI/artifact evidence, SHA-256 manifests and signer continuity.

R2 mTLS/gaze/mood/touch precedence/Wi-Fi/revocation passes are now user-reported
physical evidence. Restart recovery remains unverified. Confirmed code mechanisms:
port0 with saved endpoint/discovery-only refresh, and Android failed-listener/stale-
resolver latches. Video description does not establish old/new physical IP/port or
Windows listening state. No firewall/TLS root cause is claimed.

R3 binds TCP47654 exclusively only after explicit Start host; conflict leaves it
stopped. Saved-identity recovery tries saved/default/discovery/manual private routes,
retains UUID/pin/key, and persists route only after authenticated ready. Backoff,
foreground lifecycle, D009, bounds and revocation retained. Same-key explicit
re-enrollment is supported; no automatic trust/key reset. Failed discovery can
restart; old callbacks cannot block/publish into a new generation.

Actual app-commit CI: Windows27 core +30 link tests/build/publish/12 native checks;
Android55 tests/lint0 errors15 warnings/signature/APK/17 renders. Both PR merge runs
pass. Downloaded Android checksum/signer/XML/fonts verified. Windows local archive
verification is cloud-proxy blocked; actual CI hash/upload evidence is recorded.

**One next action: run this physical restart/reconnect test**
([full sequence](../windows/README.md#f3-r3-restart-and-saved-identity-reconnection)):

1. Verify matching R3 manifests, preserve Windows DPAPI data and replace the stopped
   app at its existing firewall-allowed path. APK signer differs from delivered R2;
   that upgrade needs uninstall/install and initial enrollment (old phone data/key
   lost). Revoke obsolete entry if needed. This install step is separate from the
   test: establish one authenticated R3 session and record both pins/host UUID.
2. Exit Windows fully while Android remains foreground. Confirm retries retain
   saved trust/offline demo. Reopen Windows and confirm host says stopped; explicitly
   select the Wi-Fi IPv4 and Start secure host. Require `listening=IP:47654`; resolve
   any reported bind conflict and retry Start before judging Android timeouts.
3. Use Reconnect saved host, with **no QR, pairing window, approval, Forget or
   Replace identity**. Require SessionReady/SESSION_CONFIRMED, unchanged UUID/pins
   and restored gaze/mood. If IP changed, refresh discovery or enter Windows'
   displayed private endpoint using saved identity; require the same pinned session.
4. Revoke on Windows: the old identity must fail trusted reconnect. Separately,
   authorize a fresh invitation and both approvals using the same phone key; it
   must enroll without Replace identity. Record actual times/latency, not estimates.
5. Copy only both safe logs after the attempts with OS/model/time/build digests.
   They include Windows stopped/listening state, Android route and UTC/client port.
   Review/redact private LAN metadata; no QR/full URI, keys, credentials, settings
   screenshots, raw traffic or unrelated system logs. Do not infer a listening
   host from Android timeouts alone.

Physical restart/IP/NSD/conflict/re-enrollment confirmation is pending this test.
F1/F2 measured QA stays separate. No F4, new auth protocol, firewall changes or merge.
