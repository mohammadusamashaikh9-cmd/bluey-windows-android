#requires -Version 5.1
[CmdletBinding(SupportsShouldProcess, ConfirmImpact='Medium')]
param([switch]$ApproveRemoval)
$ErrorActionPreference = 'Stop'
if (!$ApproveRemoval -and !$WhatIfPreference) { throw 'ocr-removal-approval-required: close Bluey, review README, then explicitly use -ApproveRemoval' }
. (Join-Path $PSScriptRoot 'OcrPackage.ps1')
foreach ($package in @(Get-AppxPackage -Name $OcrPackageName -ErrorAction Stop)) {
    if ($package.Publisher -ne $OcrPublisher) { throw 'ocr-package-publisher-mismatch' }
    if ($PSCmdlet.ShouldProcess($OcrPackageName, 'Unregister this current-user developer identity; preserve application data')) {
        Remove-AppxPackage -Package $package.PackageFullName -PreserveApplicationData -ErrorAction Stop
    }
}
# Never deletes the external app folder, LocalAppData/Bluey, BlueyPort or certificates.
