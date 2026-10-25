# F4-C FirstFrame capture handoff

2026-10-25 Asia/Karachi. PR7 on port/screen-understanding remains stacked on
port/live-ai-voice (PR6 unchanged4b38029); both unmerged. Started at2fb8760.

Developer reports five physical FirstFrame/Changed failures; latest DWM578x557,
WGC=Win32594x565, DPI1.25, WindowFrame,156ms. ItemSize mapping is now demonstrated.
The source's first-callback latch rejects null and differing content alike. Exact
physical condition remains unmeasured; no working real preview claim.

Repair uses one sequential consumer, capacity-one notification queue, <=6 frame
acquisitions, <=2 Recreate calls and the existing8s deadline. No undefined buffer
pixels are copied; each frame is retired before Recreate. Final geometry comes
from accepted content plus unchanged measured window bounds. Unknown mappings
remain PreviewOnly, with coordinates/OCR/sharing blocked. Stop cannot publish a
late bitmap; native copy resources stay alive until completion. Diagnostics retain
only bounded numeric/enum frame metadata and stage/HRESULT, never private content.

Tested app f3856770710d789cb7396e6080c13d7950c4c8b2; closeout tracking-only.
Local/Windows238unit pass;45 native checks including actual Recreate setup calls,
without WGC frame acquisition. Android111pass/lint0errors18warnings; both workflows
and stacked PR checks pass. Downloaded Windows app/evidence and rehashed413 files.
EXE78d0eeb41540bcf36b616ee3e83a3d41058f10d879822ae7abd96e82d03cd483.
[Windows folder/manifest](https://github.com/mohammadusamashaikh9-cmd/bluey-windows-android/actions/runs/40222661491/artifacts/12610415501).
Matching optional Android APK/signature and all CI/log/hash links are in STATUS.
Android unchanged: keep installed APK; optional new signer differs and reinstall
would lose phone identity. Android download policy-blocked; its hash is CI-only.
Native fixture DWM=WGC306x203,Win32320x210,DPI1 does not reproduce the laptop failure.
No Gemini/OpenAI call, F5/input, OCR packaging or protected-source change. Physical
acquisition, exact first-frame condition, mapping and Stop timing remain unverified.

One next action: use the new complete Windows artifact folder, keep cloud OFF,
capture the harmless four-line Notepad window once while still; inspect preview
and safe frames. With a verified map, point500,500 and confirm only Bluey moves,
then Home/Stop/discard. Otherwise share only safe diagnostics, never private pixels.
