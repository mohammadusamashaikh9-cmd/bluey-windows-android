# Handoff

F4-B app/artifact commit **5df67442c2cb5e8967a9caca8de3aba42e4a3099**, port/live-ai-voice, from verified main7930efd.
PR #5 is merged; developer-reported F4-A functional physical acceptance is recorded.
No F4-A reopening; performance/matrix QA remains separate. F4-B code/offline CI passed;
no paid call, real key/mint or microphone capture was used in development/CI.

D011 covers CurrentUser DPAPI key UI, current lease/consent-specific desktop cloud
approval, bounded GA client secrets, foreground hold-only/default or separately
opted-in continuous context, native PCM/chirps and existing captions. No capture/tools/
background service/backend. Window authorization UI minimum30s; token metadata is only
remaining time1–600s. Canceled holds and stale capture/drain callbacks are tested.

[STATUS](STATUS.md) contains exact files, final Windows27+45 tests/23 native checks,
Android89 tests/lint0 errors18 warnings, matching artifacts/hash/public signer links,
actual intermediate failures, SDK-document verification and physical/API limits.
Final archive download is cloud-policy blocked; actual final CI logs/hashes/upload
are recorded. Earlier downloaded artifacts are superseded and must not be mixed.
APK signer differs from F4-A; deliberate reinstall loses trust and needs re-pairing.
Preserve Windows DPAPI data. No silent identity reset or key redistribution.

Client-secret TTL is not revocation of active sessions or a hard financial cap.
Official web pricing/account access/retention and real device/API remain unverified;
doc-network draft saved, needs environment review/save/publish to activate. Current
GA schema/models/events were verified through official SDK-generated OpenAPI docs.

**One next action:** developer decision on a separate, limited billable
[30-second hold-only mini-model test](../project/QA.md#f4-b-offline-and-future-billable-qa)
after current pricing/account review. Do not run a paid session or merge automatically.
