# Bluey native OCR developer identity

This is an optional, **unsigned developer test bundle**, not an installer or release.
Keep the ordinary portable Windows ZIP as the accepted capture/pointing fallback.
Windows.Media.Ocr needs package identity. This bundle supplies Microsoft's sparse
(external-location) identity while the self-contained WPF application stays in an
ordinary folder. No additional .NET runtime, SDK, Visual Studio, OCR engine/model,
certificate, cloud account or Android update is required on the laptop.

Windows 11 x64, a locally installed OCR language and your explicit approval of
Windows Developer Mode and per-user package registration are required. Developer
Mode changes the device's development policy: review Windows' own explanation
before enabling it. If you do not approve that change, stop and use portable mode;
it will continue to show UnavailableIdentity. Nothing here enables it automatically.

The package and app use Bluey.Port.OcrDev / CN=Bluey OCR Development. The MSIX only
contains identity and public logo assets. It is deliberately unsigned and cannot
be installed by double-clicking it; this procedure registers its loose manifest.
There are **no certificates to install or trust** and no signing private key.

## Review and explicitly register

1. Close every Windows Bluey process. Keep the installed Android application and
   its identity; no APK replacement/re-pair is needed. Verify the downloaded ZIP
   digest against GitHub's artifact record before extraction. Extract the **whole**
   bundle into a stable user-owned directory (for example `C:\BlueyOcrDev`). Do not
   move/replace files while this identity is registered. Do not delete/rename
   `%LOCALAPPDATA%\Bluey\Link`, `%LOCALAPPDATA%\Bluey\AI` or `%LOCALAPPDATA%\BlueyPort`.
2. Review the included PowerShell files and SHA256SUMS.txt. Windows may block
   downloaded scripts; if so, explicitly unblock only the reviewed files using
   their Properties dialog or `Unblock-File -LiteralPath .\Register-OcrIdentity.ps1`
   (also OcrPackage.ps1 and Unregister-OcrIdentity.ps1). Do not bypass execution
   policy globally. Windows PowerShell 5.1 is already present on Windows 11; use
   it for Appx commands, not a new PowerShell/SDK installation. Run registration
   and Bluey under the same Windows account as your existing paired application;
   do not switch to a different administrator account.
3. Preview the registration, which validates all bundle file hashes but changes
   nothing. In Windows PowerShell, from the extracted bundle root:

   ```powershell
   .\Register-OcrIdentity.ps1 -WhatIf
   ```

4. **Only if you approve:** enable Developer Mode yourself in Windows Settings
   (System > Advanced on newer builds; For developers on older Windows 11 builds).
   Organization policy may prohibit this; do not work around it. Explicitly approve
   per-user registration by running:

   ```powershell
   .\Register-OcrIdentity.ps1 -ApproveRegistration
   ```

   The helper calls `Add-AppxPackage -Register .\identity\AppxManifest.xml
   -ExternalLocation .\app`, for this user only. It never starts Bluey, imports a
   certificate, changes policy, provisions all users, configures firewall or moves
   credentials. If an identity already exists, close Bluey and explicitly unregister
   first. To retain an existing executable location, deliberately copy the complete
   `app` contents there after closing Bluey, then pass `-AppDirectory` with that
   exact directory to both preview and registration. The helper checks its files
   against this bundle's hashes. No copying or overwrite is performed automatically.
5. Start `app\Bluey.Windows.exe` directly (or the exact reviewed AppDirectory).
   The OCR variant refuses startup if the expected identity is missing or Windows
   resolves LocalAppData to a different profile. No pairing/key store is opened
   before that check. Existing credentials/settings remain under the same Windows
   user and original paths; no data migration or identity replacement is intended.
   Host start remains explicit. A different executable path may need your existing
   application firewall rule reviewed manually for later LAN use; offline OCR does
   not need the host, Android, firewall changes or Internet.

## One offline Notepad OCR test

Keep live AI and cloud screen sharing OFF. Put a harmless Notepad document wholly
inside the primary display, away from edges, with these four lines and a readable
font size; avoid capturing unrelated title/status text if it would affect expected
line numbering (record any extra recognized lines honestly):

```text
BLUEY SCREEN TEST
ALPHA BETA
12345
SCREEN CAPTURE WORKING
```

Capture once, review the genuine preview, then Analyze locally. Expect Ready and
actual OCR line/word targets. The document lines must be in top-to-bottom order;
L IDs include any other visible OCR lines, so Notepad chrome may precede the four
document lines. The controlled self-test below verifies exact L1-L4 separately.
Select a document L target and W target, Point, verify Bluey reaches the matching
visible text and the real mouse remains stationary. Return Home, then Stop/discard;
targets and preview must clear. Re-capture once and wait60s: expiry must disable
prior targets. Secure/hidden UIA fields remain excluded before reading labels;
pixel OCR is **not password redaction**. Never capture sensitive fields to test it.

For an exact four-line, real-engine geometry check without Notepad chrome or user
screenshots, explicitly run from the bundle root after registration:

```powershell
$p = Start-Process .\app\Bluey.Windows.exe -ArgumentList '--ocr-self-test', "$PWD\ocr-evidence" -PassThru -Wait
$p.ExitCode
Get-Content .\ocr-evidence\ocr-native-evidence.json
```

Exit0 means real Windows OCR recognized the generated public four-line fixture,
L1-L4/W1-W9, exact word reading order and bounded rectangles, pure animated pointing,
Stop and expiry.
Exit3 means identity/language unavailable; exit1 is a sanitized native/validation
failure. The file contains only status, stage/HRESULT, counts and check names,
never user pixels, recognized user text, paths, keys or provider data. No capture,
listener or Gemini request occurs. An absent OCR language is reported honestly;
the app never downloads a language or model. The actual Notepad test remains a
separate developer-reported gate even if this synthetic-image native test passes.

Stop during pending capture is covered deterministically in the C# suite with a
manually completed capture dependency. It verifies cancellation, no late preview,
retained ownership until cleanup and pixel disposal, with no timed manual clicking.
The same coverage rejects late OCR targets after Stop. This is not a claim that
every GPU driver cancels a native operation instantly.

## Explicit removal and portable fallback

Close Bluey, then review removal with `-WhatIf` and approve only when ready:

```powershell
.\Unregister-OcrIdentity.ps1 -WhatIf
.\Unregister-OcrIdentity.ps1 -ApproveRemoval
```

This removes only the current user's matching developer identity, preserving
application data. It does not delete external binaries, Bluey/BlueyPort files,
keys, trust or certificates. After removal you may delete the extracted developer
bundle and run the complete portable fallback; OCR will again be unavailable,
while accepted capture/UIA/pointing remains. If you enabled Developer Mode solely
for this test, review turning it off yourself after removal. No automatic policy
restore or certificate action occurs. Unregister before moving/updating the bundle.

CI builds and checks both variants, the actual embedded manifests, MSIX contents,
checksums and approval/WhatIf denial. **CI never registers this package or claims
OCR passed with identity.** Real identity/API and laptop text/coordinates must be
validated after the developer's explicit registration. No cloud request is part
of this checkpoint.
