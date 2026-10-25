# F4-C OCR developer identity handoff

2026-10-26 Asia/Karachi. PR7/port/screen-understanding stays stacked on unchanged
port/live-ai-voice/PR6 at4b38029; neither merged. Accepted63d7269 real WGC/JPEG/
preview/125%DPI center pointing/stationary mouse/Home/discard is preserved.

Tested app/artifacts6da41321de73c18b7fafe0889ab6ee05a0530d22; closeout tracking-only.
D014 adds a separate unsigned sparse/external identity developer build and preserves
portable fallback. Explicit approval/WhatIf helpers never create/trust certificates,
change policy, self-register, start Bluey or migrate data. Developer must separately
review/enable Developer Mode and approve per-user registration, under the same
Windows account. OCR startup refuses missing/wrong identity or redirected profile
before settings/trust/key services. Existing protected paths/identities unchanged.

Windows40231453791:239unit,2manifest contracts,47native,14built-package/approval
checks pass; both self-contained profiles compile. Android40231454103:111pass,
lint0errors18warnings. Stacked PR40231458418/40231458451 pass. Initial41ec WhatIf
hash failure fixed19bd with direct .NET read-only hashing; no approval disabled.
No package registration/certificate action or registered OCR run in CI. Unregistered
OCR app correctly exits3/UnavailableIdentity/0lines/0words. Runtime OCR remains open.

[OCR bundle/manifest/helpers/README](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40231453791/artifacts/12612952571).
ZIP SHA256fa63ba8d31e4ca3f2832b4ebe210acdd67e8e3f2dda1712b7aef767c224c7ba6.
[Portable fallback](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40231453791/artifacts/12613710856).
All checksums/evidence/exact28files in STATUS. Download policy blockedsa14; CI hashes
are not locally rehashed. Keep installed Android APK; optional new signer differs.

One next action: review the bundle/instructions and, only if approving Developer
Mode and registration, run the single harmless Notepad OCR/word pointing/Home/Stop/
expiry sequence in windows/packaging/README.md. Optional --ocr-self-test checks real
engine on public four-line pixels after registration: exact L1-L4/W1-W9/bounds.
Notepad chrome may itself add L IDs; do not invent/filter IDs. Native API/language,
actual laptop word boxes and registered-profile behavior remain UNVERIFIED.
Keep cloud OFF, preserve keys/trust, retain portable fallback and both open PRs.
No Gemini call, alternate OCR engine, F5/input action or background observation.
