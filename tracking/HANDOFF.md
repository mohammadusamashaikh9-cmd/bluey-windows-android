# Handoff

2026-10-25 Asia/Karachi. Existing PR7/port/screen-understanding remains stacked on
port/live-ai-voice; PR6 stays unchanged4b38029/open. Neither merged. No F5/OCR
packaging/provider call, new backend or animation/security rewrite.

Developer confirms picker fixed; two physical Notepad captures reject at ItemSize/
Changed before frames (203/110ms), actual dimensions unknown. Old equality gate
conflates WGC pixels with DWM visible bounds. WGC now sizes the bounded pool; pure
CaptureGeometry uses exact visible or measured physical Win32 frame rectangles.
Per-monitor-v2 context is scoped/restored for GetWindowRect. Keep selection/frame
bounds separately and reject stale/oversized/inconsistent data. Unknown transforms
can preview but every coordinate/target/analysis/share path rejects, with UI controls
disabled. Verified Surface modes require actual measured-bound consistency. No
stretch/crop/warp or origin inferred from a DPI ratio.

Final app df1eb62d52ea325a16fda67d2e9624097c4e1303: Windows40133905687 passes Core27/Screen53/Link141,
44 native checks, WPF0 warnings/errors, self-contained x64. Android40133905040 passes
111 tests/lint0 errors18 warnings; matching artifacts/manifests uploaded. PR checks
40133910135/40133909958 pass. Windows final413-file manifest and EXE independently
rehashed after download; links/hashes/exact24 paths in STATUS. Later closeout edits
tracking only. Android code unchanged; keep current APK to avoid debug signer churn.

Safe dimension diagnostics: DWM/WGC/Win32 widths/heights, absolute differences,
DPI scale, mapping mode, stage/outcome/ms; no title, origin, pixels, private text or
key. CI public window306x203 DWM/WGC,320x210 Win32,delta0,mapVisibleBounds at1x.
This is NOT laptop mismatch evidence. Generated JPEG scale1100x619 and pure border/
DPI/unknown-map/cleanup tests pass; no WGC StartCapture/desktop pixels in CI.
Physical preview/frame/coordinate/driver/DPI/performance and F4-B provider behavior
remain unverified. OCR identity remains a separate blocker, not a capture prerequisite.

One next action: developer runs [single Notepad geometry retest](../project/QA.md#f4-c-itemsize-geometry-regression-retest),
cloud OFF. With verified mapping test local spots/return home/Stop. PREVIEW ONLY must
keep pointing/sharing blocked; report safe numeric diagnostics only, no private image.
