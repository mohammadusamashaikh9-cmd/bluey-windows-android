# Handoff

2026-10-24 Asia/Karachi. PR7 stays on port/screen-understanding, base port/live-ai-voice.
PR6 head4b38029 remains open/unmerged and unchanged; do not merge either PR.
Previous environment attempt made no edits. Resumed cleanad5f7aa in working repo.

Repair app fdc9572ee9e305cc489d9784a50f865943355cf5: explicit Unicode GetWindowTextW and corrected
Microsoft IDXGIDevice IID. Native red d32b1e4/40006819441 reproduced title truncation
and E_NOINTERFACE in shared setup; two-line fix16efb78/40007491657 passes the same
contracts. Full native capture success on the laptop is NOT inferred from that.

Eight bounded in-memory safe diagnostic rows expose selection/support/item/device/
pool/session/first-frame/copy/encode/validation stages, HRESULT and elapsed counts.
No title, handle, pixels, OCR/private text, key, raw exception or traffic is logged.
UI offers selectable diagnostics, safe failure guidance and existing Stop/discard.
One-shot capture, preview, F3 and cloud consent, DPI/bounds/zeroing remain intact.
No OCR packaging, provider call/key, fallback/dependency/server or F5 actions.

Local Core27/Screen35/Link141 pass; WPF0 warnings/errors. Windows40008459044:
203 unit tests,42 native checks, self-contained x64. Android40008459792:111 tests,
lint0 errors18 warnings, matching APK/signature. PR40008467356/40008467447 also pass.
Windows app/evidence downloaded; all413 manifest entries and EXE hash reverified.
Exact artifacts/hashes/14 repair paths in STATUS; later closeout is tracking-only.
Android code unchanged: retain installed APK to avoid debug-signer/re-enrollment
churn. New optional regression APK signer differs; never silently replace identity.

Native checks prove actual HWND titles and separate window/primary item + device/
pool/session construction, NOT first-frame screenshots: CI never StartCapture.
Invalid-window stage/HRESULT privacy is exercised. OCR identity blocker remains
separate; physical preview/DPI/driver/exclusion/resource and F4-B provider behavior
remain unverified. Original Apple/Android/animation/link/protected-key code unchanged.

One next action: developer executes [local-only capture retest](../project/QA.md#f4-c-capture-repair-physical-retest-local-only)
(primary and Notepad independently), cloud OFF; on failure copy only safe diagnostic
kind/stage/outcome/HRESULT/time. No packaging/cloud/API test is authorized here.
