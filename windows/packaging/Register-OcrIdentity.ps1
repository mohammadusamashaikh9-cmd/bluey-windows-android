#requires -Version 5.1
[CmdletBinding(SupportsShouldProcess, ConfirmImpact='Medium')]
param([switch]$ApproveRegistration, [string]$AppDirectory)
$ErrorActionPreference = 'Stop'
if (!$ApproveRegistration -and !$WhatIfPreference) { throw 'ocr-registration-approval-required: review README, then explicitly use -ApproveRegistration' }
. (Join-Path $PSScriptRoot 'OcrPackage.ps1')
$external = Assert-OcrBundle -Root $PSScriptRoot -AppDirectory $AppDirectory
Assert-OcrWindowsVersion -Version ([Environment]::OSVersion.Version)
$registration = Get-Command Add-AppxPackage -ErrorAction Stop
if (@($registration.ParameterSets | Where-Object { $_.Name -eq 'Register' -and $_.Parameters.Name -contains 'ExternalLocation' }).Count -eq 0) { throw 'ocr-external-registration-unavailable: use Windows PowerShell5.1; stop rather than changing policy or installing tools automatically' }
if ($PSCmdlet.ShouldProcess($OcrPackageName, 'Register current-user developer identity at the reviewed external app location')) {
    if (!(Test-OcrDeveloperMode)) { throw 'ocr-developer-mode-required: review and enable Developer Mode yourself in Windows Settings; this script will not change policy' }
    if (@(Get-AppxPackage -Name $OcrPackageName -ErrorAction Stop).Count -ne 0) { throw 'ocr-identity-already-registered: close Bluey and explicitly unregister before replacing or moving this developer build' }
    Add-AppxPackage -Register (Join-Path $PSScriptRoot 'identity/AppxManifest.xml') -ExternalLocation $external -ErrorAction Stop
    Write-Output 'OCR identity registered for this Windows user. No app was started and no credential, pairing data or certificate trust was changed.'
}
