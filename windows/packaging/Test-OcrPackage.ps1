[CmdletBinding()]
param([Parameter(Mandatory)][string]$BundleRoot,[Parameter(Mandatory)][string]$PortableRoot,[Parameter(Mandatory)][string]$EvidenceRoot)
$ErrorActionPreference = 'Stop'; Set-StrictMode -Version Latest
$checks = [Collections.Generic.List[string]]::new()
function Check([bool]$ok,[string]$code) { if (!$ok) { throw $code }; $checks.Add($code) }
$bundle = (Resolve-Path -LiteralPath $BundleRoot).Path
$portable = (Resolve-Path -LiteralPath $PortableRoot).Path
New-Item -ItemType Directory -Force $EvidenceRoot | Out-Null
$evidence = (Resolve-Path -LiteralPath $EvidenceRoot).Path
. (Join-Path $bundle 'OcrPackage.ps1')
foreach ($build in @(19041,19045,22000,26100)) {
    Assert-OcrWindowsVersion -Version ([version]"10.0.$build.0")
    Check $true ("Version preflight accepts build$build; no runtime claim")
}
$denied = $false; try { Assert-OcrWindowsVersion -Version ([version]'10.0.19040.0') } catch { $denied = $_.Exception.Message.StartsWith('ocr-windows-build-too-old') }
Check $denied 'Version preflight rejects before sparse-package minimum'
$external = Assert-OcrBundle -Root $bundle
Check ($external -eq (Join-Path $bundle 'app')) 'Complete developer-bundle checksums match'
foreach ($action in @(@('Register-OcrIdentity.ps1','ocr-registration-approval-required'),@('Unregister-OcrIdentity.ps1','ocr-removal-approval-required'))) {
    $denied = $false
    try { & (Join-Path $bundle $action[0]) } catch { $denied = $_.Exception.Message.StartsWith($action[1]) }
    Check $denied ($action[0] + ' denies absent explicit approval')
}
$before = & powershell.exe -NoProfile -Command '(Get-AppxPackage -Name Bluey.Port.OcrDev | Measure-Object).Count'
if ($LASTEXITCODE -ne 0) { throw 'Unable to inspect current-user package state' }
Check ([int]$before -eq 0) 'No OCR identity installed in CI'
& powershell.exe -NoProfile -File (Join-Path $bundle 'Register-OcrIdentity.ps1') -WhatIf
Check ($LASTEXITCODE -eq 0) 'Registration WhatIf validates bundle without approval or mutation'
$after = & powershell.exe -NoProfile -Command '(Get-AppxPackage -Name Bluey.Port.OcrDev | Measure-Object).Count'
Check ($LASTEXITCODE -eq 0 -and [int]$after -eq [int]$before) 'WhatIf leaves package registration unchanged'
$sdk = Get-ChildItem "${env:ProgramFiles(x86)}/Windows Kits/10/bin" -Directory | Where-Object { $_.Name -match '^10\.0\.\d+\.0$' -and (Test-Path (Join-Path $_.FullName 'x64/mt.exe')) } | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
if (!$sdk) { throw 'Windows SDK manifest tool unavailable' }
foreach ($variant in @('portable','ocr')) {
    $exe = if ($variant -eq 'portable') { Join-Path $portable 'Bluey.Windows.exe' } else { Join-Path $external 'Bluey.Windows.exe' }
    $destination = Join-Path $evidence "$variant-application-manifest.xml"
    & (Join-Path $sdk.FullName 'x64/mt.exe') "-inputresource:$exe;#1" "-out:$destination"
    if ($LASTEXITCODE -ne 0) { throw 'Embedded PE manifest extraction failed' }
    [xml]$xml = Get-Content -LiteralPath $destination -Raw
    $identity = $xml.SelectSingleNode('/*[local-name()="assembly"]/*[local-name()="msix"]')
    if ($variant -eq 'portable') { Check ($null -eq $identity) 'Actual portable EXE remains without sparse identity' }
    else { Check ($null -ne $identity -and $identity.packageName -eq $OcrPackageName -and $identity.publisher -eq $OcrPublisher -and $identity.applicationId -eq 'Bluey') 'Actual developer EXE embeds matching sparse identity' }
}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [IO.Compression.ZipFile]::OpenRead((Join-Path $bundle 'Bluey.Ocr.Dev.msix'))
try {
    Check ($null -eq $zip.GetEntry('AppxSignature.p7x')) 'Developer MSIX is unsigned; no certificate trust required for loose registration'
    Check (@($zip.Entries | Where-Object { $_.FullName -like '*.exe' -or $_.FullName -like '*.dll' }).Count -eq 0) 'Sparse MSIX contains identity/assets only; executable stays external'
    $reader = [IO.StreamReader]::new($zip.GetEntry('AppxManifest.xml').Open())
    try { [xml]$xml = $reader.ReadToEnd() } finally { $reader.Dispose() }
    Check ($xml.Package.Identity.Name -eq $OcrPackageName) 'Built MSIX manifest matches expected identity'
    Check ($xml.Package.Dependencies.TargetDeviceFamily.MinVersion -eq '10.0.19041.0') 'Actual MSIX OS floor accepts Windows10 build19045'
} finally { $zip.Dispose() }
Check (@(Get-ChildItem $bundle -Recurse -File | Where-Object { $_.Extension -in @('.pfx','.pem','.cer','.key') }).Count -eq 0) 'No signing private key or trust certificate shipped'
$process = Start-Process -FilePath (Join-Path $external 'Bluey.Windows.exe') -ArgumentList '--ocr-self-test', (Join-Path $evidence 'ocr-unregistered') -PassThru -Wait
Check ($process.ExitCode -eq 3) 'Unregistered developer OCR test fails closed with exit3'
$result = Get-Content -LiteralPath (Join-Path $evidence 'ocr-unregistered/ocr-native-evidence.json') -Raw | ConvertFrom-Json
Check ($result.status -eq 'UnavailableIdentity' -and $result.lines -eq 0 -and $result.words -eq 0) 'Unregistered app reports no fabricated OCR recognition'
Check ($result.minimumWindows -eq 'windows10.0.19041.0') 'Compiled OCR assembly declares the19041 supported API floor'
Check (@($result.apiAvailability.PSObject.Properties | Where-Object { !$_.Value }).Count -eq 0) 'Audited APIs are present on CI host; not Windows10 runtime proof'
Check (![string]::IsNullOrWhiteSpace($result.runtimeWindows)) 'Native evidence identifies actual runtime OS'
[ordered]@{ checks=$checks; registration='NOT EXECUTED; explicit developer approval required'; certificates='NONE created/imported/trusted'; ocr='NOT EXECUTED with identity; real Notepad gate pending'; provider='NONE' } | ConvertTo-Json -Depth 5 | Set-Content -Encoding utf8NoBOM (Join-Path $evidence 'ocr-package-evidence.json')
Get-Content -LiteralPath (Join-Path $evidence 'ocr-package-evidence.json')
