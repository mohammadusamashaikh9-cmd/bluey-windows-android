[CmdletBinding()]
param([string]$OutputDirectory = 'artifacts/bluey-ocr-dev')
$ErrorActionPreference = 'Stop'; Set-StrictMode -Version Latest
New-Item -ItemType Directory -Force $OutputDirectory | Out-Null
$root = (Resolve-Path -LiteralPath $OutputDirectory).Path
$app = Join-Path $root 'app'
dotnet publish (Join-Path $PSScriptRoot '../Bluey.Windows') -c Release -r win-x64 --self-contained true -p:PublishTrimmed=false -p:BlueyOcrIdentity=true -o $app
if ($LASTEXITCODE -ne 0) { throw 'OCR identity publish failed' }
Copy-Item (Join-Path $PSScriptRoot 'identity') $root -Recurse -Force
foreach ($file in @('OcrPackage.ps1','Register-OcrIdentity.ps1','Unregister-OcrIdentity.ps1','README.md')) { Copy-Item (Join-Path $PSScriptRoot $file) $root -Force }
$assets = Join-Path $root 'identity/Assets'; New-Item -ItemType Directory -Force $assets | Out-Null
Add-Type -AssemblyName System.Drawing
foreach ($size in @(44,50,150)) {
    $bitmap = New-Object System.Drawing.Bitmap($size,$size)
    $graphics = [Drawing.Graphics]::FromImage($bitmap)
    try {
        $graphics.Clear([Drawing.Color]::Transparent)
        $brush = New-Object Drawing.SolidBrush([Drawing.Color]::FromArgb(255,49,78,157))
        try { $graphics.FillEllipse($brush,2,2,$size-4,$size-4) } finally { $brush.Dispose() }
        $name = if ($size -eq 50) { 'StoreLogo.png' } else { "Square${size}x${size}Logo.png" }
        $bitmap.Save((Join-Path $assets $name),[Drawing.Imaging.ImageFormat]::Png)
    } finally { $graphics.Dispose(); $bitmap.Dispose() }
}
$sdk = Get-ChildItem "${env:ProgramFiles(x86)}/Windows Kits/10/bin" -Directory | Where-Object { $_.Name -match '^10\.0\.\d+\.0$' -and (Test-Path (Join-Path $_.FullName 'x64/makeappx.exe')) } | Sort-Object { [version]$_.Name } -Descending | Select-Object -First 1
if (!$sdk) { throw 'Windows SDK MakeAppx unavailable; no unrelated tool installation attempted' }
$make = Join-Path $sdk.FullName 'x64/makeappx.exe'
# Microsoft sparse-package recipe uses /nv because the executable is external.
# Schema/contracts and actual embedded PE metadata are validated separately in CI.
& $make pack /o /d (Join-Path $root 'identity') /nv /p (Join-Path $root 'Bluey.Ocr.Dev.msix')
if ($LASTEXITCODE -ne 0) { throw 'Sparse package creation failed' }
# Deliberately unsigned. No certificate creation/import/trust, package registration or policy change.
Get-ChildItem $root -Recurse -File | Where-Object { $_.Name -ne 'SHA256SUMS.txt' } | Sort-Object FullName | ForEach-Object {
    $relative = [IO.Path]::GetRelativePath($root,$_.FullName).Replace('\','/')
    '{0}  {1}' -f (Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant(),$relative
} | Set-Content -Encoding utf8NoBOM (Join-Path $root 'SHA256SUMS.txt')
foreach ($file in @('app/Bluey.Windows.exe','app/Bluey.Windows.dll','Bluey.Ocr.Dev.msix','SHA256SUMS.txt')) {
    '{0}  {1}' -f (Get-FileHash -LiteralPath (Join-Path $root $file) -Algorithm SHA256).Hash.ToLowerInvariant(),$file
}
