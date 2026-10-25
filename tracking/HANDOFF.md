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

Local screen70pass after a red9fail/2pass regression; native Windows/Android CI
results will be recorded after the source push. No provider call/key, OS input,
OCR packaging, original Apple/Android/animation/F3 code changes.

One next action: use the new complete Windows artifact folder, keep cloud OFF,
capture the harmless four-line Notepad window once while still; inspect preview
and safe frames. With a verified map, point500,500 and confirm only Bluey moves,
then Home/Stop/discard. Otherwise share only safe diagnostics, never private pixels.
