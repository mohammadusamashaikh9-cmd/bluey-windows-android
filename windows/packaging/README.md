# Bluey native OCR developer identity

This is an optional, **unsigned developer test bundle**, not an installer or release.
Keep the ordinary portable Windows ZIP as the accepted capture/pointing fallback.
Windows.Media.Ocr needs package identity. This bundle supplies Microsoft's sparse
(external-location) identity while the self-contained WPF application stays in an
ordinary folder. The default loose-manifest route needs no additional .NET runtime,
SDK, Visual Studio, OCR engine/model, certificate, cloud account or Android update.
The separately approved signed route below may need an already-reviewed SignTool.

Windows 10 build19041+ x64 (including the developer's Pro Education22H2/build19045)
or Windows11 x64, a locally installed OCR language and your explicit approval of
Windows Developer Mode and per-user package registration are required. Developer
Mode changes the device's development policy: review Windows' own explanation
before enabling it. If you do not approve that change, stop and use portable mode;
it will continue to show UnavailableIdentity. Nothing here enables it automatically.

The package and app use Bluey.Port.OcrDev / CN=Bluey OCR Development. The MSIX only
contains identity and public logo assets. It is deliberately unsigned and cannot
be installed by double-clicking it; this procedure registers its loose manifest.
The default loose-manifest route ships no certificate or signing private key.
It does not use `Add-AppxPackage -AllowUnsigned`: installing an unsigned MSIX by
that route is a separate Windows11 feature. Windows10 loose developer registration
and external-location APIs exist at19041, but exact sparse activation/OCR on this
laptop is still unverified. If signing/trust or identity activation fails, STOP;
the separately approved signed route below is not an automatic fallback.

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
   policy globally. Windows PowerShell5.1 is already present on Windows10/11; use
   it for Appx commands, not a new PowerShell/SDK installation. Run registration
   and Bluey under the same Windows account as your existing paired application;
   do not switch to a different administrator account.
3. Preview the registration, which validates all bundle file hashes but changes
   nothing. In Windows PowerShell, from the extracted bundle root:

   ```powershell
   .\Register-OcrIdentity.ps1 -WhatIf
   ```

4. **Only if you approve:** enable Developer Mode yourself in Windows Settings
   (Windows10: Update & Security > For developers; Windows11: System > Advanced
   on newer builds or For developers on older builds).
   Organization policy may prohibit this; do not work around it. Explicitly approve
   per-user registration by running:

   ```powershell
   .\Register-OcrIdentity.ps1 -ApproveRegistration
   ```

   The helper first verifies build19041+ and that the native cmdlet supports
   Register with ExternalLocation. An unsupported host fails without policy changes.
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
failure. The file contains only runtime Windows version, declared OS minimum, API-presence
flags, status, stage/HRESULT, counts and check names,
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


## Windows10 compatibility audit and evidence limits

The previous22000 package minimum was a deployment bug for the actual19045 laptop.
MSIX MinVersion is now19041. Managed SupportedOSPlatformVersion is explicitly19041;
the SDK/TFM stays net10.0-windows10.0.22000.0 to preserve the existing projections.
A higher SDK target does not require that runtime OS when the supported minimum is
explicit. Warnings-as-errors/CA1416 audit direct WinRT/managed calls at19041. Native
P/Invokes require the separate documented minimum checks below. Win11 remains valid.

| Used surface | Microsoft availability / reason it fits19045 |
| --- | --- |
| [OcrEngine/RecognizeAsync](https://learn.microsoft.com/en-us/uwp/api/windows.media.ocr.ocrengine) | Windows10/10240, package identity and installed language still required |
| [CreateFreeThreaded frame pool](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.direct3d11captureframepool.createfreethreaded) |1809/17763; Recreate/content/surface/JPEG APIs predate19041 |
| [CreateForWindow desktop interop](https://learn.microsoft.com/en-us/windows/win32/api/windows.graphics.capture.interop/nf-windows-graphics-capture-interop-igraphicscaptureiteminterop-createforwindow) |1903/18362; accepted WGC adapter unchanged |
| [IsCursorCaptureEnabled](https://learn.microsoft.com/en-us/uwp/api/windows.graphics.capture.graphicscapturesession.iscursorcaptureenabled) |2004/19041; no Win11 borderless/programmatic-access API added |
| [AllowExternalContent](https://learn.microsoft.com/en-us/uwp/schemas/appxpackage/uapmanifestschema/element-uap10-allowexternalcontent) / [win32App/mediumIL](https://learn.microsoft.com/en-us/uwp/schemas/appxpackage/uapmanifestschema/element-application) |2004/19041; runFullTrust/unvirtualizedResources retained, no new capabilities |
| [ExternalLocation registration](https://learn.microsoft.com/en-us/uwp/api/windows.management.deployment.packagemanager.registerpackagebyuriasync) |2004/19041; DeveloperMode/ExternalLocationUri options and [Register cmdlet](https://learn.microsoft.com/en-us/powershell/module/appx/add-appxpackage) checked separately from Win11-only unsigned-MSIX installation |
| [NO_PACKAGE_REDIRECTION](https://learn.microsoft.com/en-us/windows/win32/api/shlobj_core/ne-shlobj_core-known_folder_flag) |1703/15063; identity/data-path startup guard unchanged |
| [WDA_EXCLUDEFROMCAPTURE](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowdisplayaffinity) |2004/19041, still best effort; DPI/window/DWM/D3D11 APIs used by the adapter predate19041 |

Actual generated assembly attributes and built MSIX minimum are checked in CI.
Version preflight accepts19041/19045/22000/26100 and rejects19040. Runtime evidence
prints the real host build and API-presence flags. Server2025 CI is NOT a19045
runtime test; it never registers the package or calls OCR with identity. Native
registration, sparse activation, installed language, OCR words/boxes, capture under
identity and profile preservation still require the actual Windows10 laptop test.

Microsoft's current [.NET10 maintained OS matrix](https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md)
does not list Windows10 Pro Education22H2. This is an API/runtime compatibility
build for the developer's already-working .NET10 app, not a claim of vendor lifecycle
or ESU support. No OS upgrade, SDK installation, engine replacement or background
observation is performed. Keep the accepted portable build available.

## Optional signed route — separate explicit approval only

Use this only if you explicitly choose the documented signed sparse-package route,
for example after a trust/activation error. Do not silently retry failed loose
registration with certificates. Record the HRESULT and stop first; policy denial
must not be bypassed. No signing/trust operation below was run in Codex or CI.
[Microsoft sparse guidance](https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/grant-identity-to-nonpackaged-apps)
uses a signed identity package. Loose-development registration is not proof that
Win32 activation/trust succeeds on every Windows10 installation.

If this is needed, use a trusted code-signing certificate you already control with
subject exactly `CN=Bluey OCR Development`, or separately approve creating a short
lived developer certificate. Windows SDK SignTool x64 must be available on the
machine holding the private key. Do not install a heavy SDK merely to attempt the
default loose route. If SignTool is unavailable, stop and plan that optional tool
setup separately; no CLI, package manager or SDK is installed by these instructions.

After explicitly approving **certificate creation** (same Windows user), these
manual commands create a nonexportable local signing key; never export/share it:

```powershell
$ocrCert = New-SelfSignedCertificate -Type CodeSigningCert -Subject 'CN=Bluey OCR Development' -CertStoreLocation Cert:\CurrentUser\My -KeyAlgorithm RSA -KeyLength 2048 -HashAlgorithm SHA256 -KeyExportPolicy NonExportable -NotAfter (Get-Date).AddDays(30)
$ocrThumb = $ocrCert.Thumbprint
$ocrCert | Format-List Subject,Thumbprint,NotAfter
Export-Certificate -Cert $ocrCert -FilePath .\Bluey.Ocr.Dev.cer
```

Inspect the public certificate/thumbprint and approve **trust separately** before
executing the following. Only CurrentUser TrustedPeople is used; do not import into
Root or change machine policy. If organization policy rejects it, stop.

```powershell
Import-Certificate -FilePath .\Bluey.Ocr.Dev.cer -CertStoreLocation Cert:\CurrentUser\TrustedPeople
```

After explicitly approving **signing**, copy the unsigned MSIX, then use your
reviewed SignTool path. The original bundle/MSIX/SHA256SUMS remain unchanged:

```powershell
New-Item -ItemType Directory -Path .\signed
Copy-Item .\Bluey.Ocr.Dev.msix .\signed\Bluey.Ocr.Dev.msix
$signTool = 'C:\path-to-reviewed-Windows-SDK\x64\signtool.exe'
& $signTool sign /fd SHA256 /s My /sha1 $ocrThumb .\signed\Bluey.Ocr.Dev.msix
if ($LASTEXITCODE -ne 0) { throw 'Signing failed; stop' }
& $signTool verify /pa .\signed\Bluey.Ocr.Dev.msix
if ($LASTEXITCODE -ne 0) { throw 'Signature verification failed; stop' }
Get-FileHash .\signed\Bluey.Ocr.Dev.msix -Algorithm SHA256
```

Close Bluey. If the loose developer identity is installed, review and explicitly
remove it using the existing helper before changing route. Only after approving
**signed registration**, run in the same Windows account (replace external path
only with your reviewed app folder; do not move credentials):

```powershell
Add-AppxPackage -Path .\signed\Bluey.Ocr.Dev.msix -ExternalLocation (Resolve-Path .\app).Path -ErrorAction Stop
```

Run the same native self-test and Notepad sequence. No credential/profile migration
or capture consent is implied by registration. The signed route has NOT been tested
on19045 here. To remove a signed nondevelopment package, review its exact identity
and explicitly approve the native removal below; PreserveApplicationData is a
loose/development-package option, so do not use that helper for this signed route:

```powershell
$ocrPackage = Get-AppxPackage -Name Bluey.Port.OcrDev | Where-Object Publisher -eq 'CN=Bluey OCR Development'
$ocrPackage | Select-Object Name,PackageFullName,Publisher
Remove-AppxPackage -Package $ocrPackage.PackageFullName -WhatIf
# Only after approving removal:
Remove-AppxPackage -Package $ocrPackage.PackageFullName
```

This does not delete external binaries or original unvirtualized Bluey/BlueyPort
credentials/settings. Afterwards, remove only the exact developer certificate you
created, if you explicitly approve that cleanup (do not delete other certificates):

```powershell
Remove-Item -LiteralPath "Cert:\CurrentUser\TrustedPeople\$ocrThumb" -Confirm
Remove-Item -LiteralPath "Cert:\CurrentUser\My\$ocrThumb" -DeleteKey -Confirm
```

No private signing key, API credential, certificate trust or registered identity is
shipped by the bundle. Failed registration/OCR is evidence to diagnose, not permission
to upgrade the OS, weaken F3, add cloud OCR or replace the working capture backend.
