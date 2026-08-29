#Requires -Version 7.0
[CmdletBinding()]
param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

& (Join-Path $PSScriptRoot 'generate-pseudo-resources.ps1') -Check
& (Join-Path $PSScriptRoot 'lint-resources.ps1')
& (Join-Path $PSScriptRoot 'lint-accessibility-metadata.ps1')

Write-Host 'Localization and accessibility lint passed.'
