# Handoff

F2 Windows overlay is implemented on `port/windows-overlay`, from accepted main
merge `15194dec8356385b15dc6712630d45e4761cd1de`. F1 PR #2 is confirmed merged;
its physical-phone/performance QA remains outstanding and Android is unchanged.

Cloud results: 23 pure C# tests passed; WPF cross-build and self-contained win-x64
publish passed using .NET SDK 10.0.402. Windows CI/native/artifact evidence is
pending; no real desktop acceptance is claimed. See [STATUS](STATUS.md).

Exact next task: run the Windows CI workflow for this branch, inspect native HWND
and render evidence and verify the uploaded development artifact SHA-256 manifest.
No merge, signing, installer, release, networking, capture or computer actions.
After CI, physical Windows 11 laptop checks in [Windows instructions](../windows/README.md)
remain required before F2 can be marked fully verified.
