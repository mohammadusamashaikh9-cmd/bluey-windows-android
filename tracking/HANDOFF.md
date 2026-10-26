# F4-C Windows10 OCR compatibility handoff

2026-10-26 Asia/Karachi. PR7/port/screen-understanding remains stacked on unchanged
port/live-ai-voice/PR6 at4b38029; both unmerged. Start e5abe8a. Actual developer OS
is Windows10 Pro Education22H2/build19045; accepted63d7269 WGC/JPEG/preview/125%DPI
pointing/Home/discard remains authoritative and unchanged.

Tested application/artifacts e36d7619fe03ef02a37af58566b4380979f655bb; closeout
tracking-only. Package MinVersion/managed SupportedOS/TargetPlatformMin are19041,
compile SDK/TFM remains22000. API audit/analyzer matches19041, Windows11 retained.
Runtime OCR metadata identifies actual OS/minimum/API presence. Registration checks
actual OS and native Register/ExternalLocation pair; internal set names are not a
contract (initial939d23d CI failure fixed e36d761 without weakening gates).

Windows40315042806 passes239unit/3manifest/48native/24package checks; Android
40315043601 passes111tests/lint0errors18warnings. PR40315048406/40315048410 pass.
No registration, developer signing certificate creation/trust, provider request,
OS change or data/profile migration in Codex/CI. Server2025 is not19045 runtime proof.
All bundle hashes verified on runner; local download blockedsa2, not rehashed.

[OCR developer bundle/README/manifest](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40315042806/artifacts/12646935593).
ZIP SHA256c73d1c82cbf47fa1a0a64260d63d44fe4fc5057ced995f3d5f51cc505b071c64.
[Portable fallback](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40315042806/artifacts/12646656630).
Exact18files, hashes/logs and optional unchanged Android outputs in STATUS.
Keep installed APK; CI signer differs. Original keys, F3, capture/OCR/geometry and
animations untouched. .NET10 lifecycle support is separate from target compatibility.

Loose developer registration is a documented Windows10 API, not Win11-only unsigned
MSIX installation. Exact19045 activation/native OCR is UNVERIFIED. README provides
a separate manual signed route only with explicit creation/trust/signing/registration
approval; no silent fallback or SDK/certificate installation. Stop on policy/errors.
One next action: review bundle/checksums and explicitly approved local setup, run
native public-text self-test on19045, then harmless Notepad OCR/word pointing/Home/
Stop/expiry with cloud OFF. Retain sanitized OS/API/stage/HRESULT/counts on failure.
No new OCR library until an evidenced failure and comparison; no OS upgrade/F5.
