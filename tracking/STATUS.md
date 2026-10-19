# Current verified state

2026-10-20 Asia/Karachi: F4-A on `port/ai-voice-foundation`, based on verified
`origin/main` / `32a38c045e6de87e9d79d44d1114a4f0c76d2b8d`.
GitHub confirms PR #4 squash-merged. F3 functional QA is developer-reported accepted:
mTLS pairing, gaze/moods, Android touch precedence, Wi-Fi recovery, revocation,
full Windows restart, explicit host start and saved authenticated reconnect without
new QR or replacement identity. This supersedes the R3 pending-retest status.
Measured latency/FPS/CPU/memory, multiple-device and advanced compatibility QA
remain UNVERIFIED; F1/F2 detailed visual/performance QA stays separate.

F4-A implementation is in progress under D010. No live AI/audio/capture capability.
Local protocol suite: 35 passed, zero failed/skipped, including actual software-key
mutual-TLS presentation delivery after ready, pre-ready denial and revocation cleanup.
This is not Android Keystore/physical desktop acceptance. CI/artifact evidence pending.
