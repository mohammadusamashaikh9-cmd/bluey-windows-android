# F4-C Windows10 OCR compatibility handoff

2026-10-26 Asia/Karachi. PR7/port/screen-understanding, base unchanged
port/live-ai-voice/PR6 at4b38029; both unmerged. Start e5abe8a. Actual developer OS
is Windows10 Pro Education22H2/build19045. Accepted63d7269 capture/125%DPI pointing
and Home/discard ran on that machine; preserve WGC/JPEG/geometry/F3/keys/animations.

D014 correction: MSIX MinVersion and managed SupportedOSPlatformVersion19041;
retain SDK/TFM22000 and Windows11 compatibility. API audit fits19041; compiler
CA1416 remains an error. Runtime OCR evidence adds actual OS/minimum/API presence.
Approval-gated registration checks actual OS and native Register/ExternalLocation.
Documented loose developer API is distinct from Win11 unsigned-MSIX installation.
Actual19045 sparse activation/native OCR remains unverified; optional manual signed
procedure requires separate explicit approval, never automatic fallback. No cert,
policy, package registration, OS upgrade, new OCR engine or provider call in CI.

Local manifest contracts failed2/3 then pass3; OCR profile builds clean. Full CI/
artifacts follow source push. No Windows10 runtime claim from Server2025 CI.
Portable fallback and original protected data/profile guards remain. Android/source
Apple files unchanged; existing APK retained. Vendor.NET10 support lifecycle is
separate from the developer's already-working runtime and this compatibility test.

One next action: review the bundle/README, explicitly approve local setup, then run
the19045 native public-text self-test followed by harmless Notepad OCR/word pointing/
Home/Stop/expiry with cloud OFF. Retain sanitized error metadata and stop on failure;
do not silently sign/trust/retry, replace OCR or alter the OS.
