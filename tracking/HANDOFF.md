# Handoff

F4-A app/evidence commit **`e0dcda03ca2168ada223bb9f01d7fb231a506202`**, branch `port/ai-voice-foundation`.
Accepted main is merged F3 `32a38c0`; F3 functional QA is developer-reported accepted
including full Windows restart, explicit host start and saved authenticated reconnect.
Measured performance/matrix QA remains open. Do not merge the F4-A PR.

D010: offline six-state Kotlin controller, exact indicator math/native public-corner
Canvas, preserved face math/remote target, strict session-bound presentation and
Windows existing bubble. Planned mic/cloud/screen consents all OFF; no audio, AI,
key configuration, token endpoint, media/capture or tools. No API key needed.

[STATUS](STATUS.md) contains all46 changed files, source comparisons, 27+35 Windows
and70 Android passing tests, 16 native checks/13 Windows frames, 41 Android frames,
CI/artifact URLs, file/ZIP/checksum hashes and actual failed-intermediate evidence.
Android APK/checksum/signer/XML/frames downloaded and inspected. Windows archive
local verification is cloud-policy blocked; actual CI build/test/hash/upload passed.
Use only the matching app-commit downloads; later PR merge artifacts use another signer.
APK signer differs from R3, so that update requires uninstall/install and explicit
new enrollment (old Android data/key lost). Preserve Windows DPAPI state. No runtime
code silently replaces identity. F4-A real-device UX/performance is not yet verified.

**One next action:** execute the [physical offline demonstration](../project/QA.md#f4-a-physical-offline-demonstration-pending):
verify manifests/install, explicitly start secure host, connect saved/re-enrolled
phone, double tap wake, hold300 ms/release, verify mock stream/Windows bubble/cleanup,
then interrupt with sleep, pause and disconnect without late reactivation. Record
actual device/OS/build/time/results. No live mic/cloud/capture authorization.
