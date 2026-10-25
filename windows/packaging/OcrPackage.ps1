# Definitions only. No registration, trust, policy changes or application execution.
Set-StrictMode -Version Latest
$OcrPackageName = 'Bluey.Port.OcrDev'
$OcrPublisher = 'CN=Bluey OCR Development'
function Assert-OcrBundle {
    param([Parameter(Mandatory)][string]$Root, [string]$AppDirectory)
    $rootPath = (Resolve-Path -LiteralPath $Root).Path
    if (!$AppDirectory) { $AppDirectory = Join-Path $rootPath 'app' }
    $appPath = (Resolve-Path -LiteralPath $AppDirectory).Path
    $manifestPath = Join-Path $rootPath 'identity/AppxManifest.xml'
    [xml]$xml = Get-Content -LiteralPath $manifestPath -Raw
    if ($xml.Package.Identity.Name -ne $OcrPackageName -or $xml.Package.Identity.Publisher -ne $OcrPublisher -or $xml.Package.Applications.Application.Executable -ne 'Bluey.Windows.exe') { throw 'ocr-package-identity-mismatch' }
    $sums = @(Get-Content -LiteralPath (Join-Path $rootPath 'SHA256SUMS.txt'))
    if ($sums.Count -lt 10 -or $sums.Count -gt 1024) { throw 'ocr-package-checksum-count' }
    foreach ($line in $sums) {
        if ($line -cnotmatch '^([0-9a-f]{64})  ([^:]+)$') { throw 'ocr-package-checksum-format' }
        $expected = $Matches[1]; $relative = $Matches[2]
        if ([IO.Path]::IsPathRooted($relative) -or ($relative -split '[/\\]') -contains '..') { throw 'ocr-package-checksum-path' }
        if ($relative.StartsWith('app/')) { $path = Join-Path $appPath $relative.Substring(4) }
        else { $path = Join-Path $rootPath $relative }
        if (!(Test-Path -LiteralPath $path -PathType Leaf)) { throw 'ocr-package-checksum-mismatch' }
        # Windows PowerShell 5.1 Get-FileHash internals inherit WhatIf and may
        # produce no hash. Direct read-only .NET hashing preserves outer WhatIf.
        $stream = [IO.File]::OpenRead($path); $hash = [Security.Cryptography.SHA256]::Create()
        try { $actual = [BitConverter]::ToString($hash.ComputeHash($stream)).Replace('-','').ToLowerInvariant() }
        finally { $hash.Dispose(); $stream.Dispose() }
        if ($actual -ne $expected) { throw 'ocr-package-checksum-mismatch' }
    }
    return $appPath
}
function Test-OcrDeveloperMode {
    $state = Get-ItemProperty -LiteralPath 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock' -ErrorAction SilentlyContinue
    return $null -ne $state -and $state.PSObject.Properties.Name -contains 'AllowDevelopmentWithoutDevLicense' -and $state.AllowDevelopmentWithoutDevLicense -eq 1
}
