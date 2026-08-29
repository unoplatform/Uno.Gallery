#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $OutputPath,

    [Parameter(Mandatory)]
    [string] $Version,

    [Parameter(Mandatory)]
    [string] $InformationalVersion,

    [string] $SourceBranch = $env:BUILD_SOURCEBRANCH,
    [string] $BuildId = $env:BUILD_BUILDID,
    [string] $BuildUri = $env:BUILD_BUILDURI
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$globalJson = Get-Content (Join-Path $repoRoot 'global.json') -Raw | ConvertFrom-Json
$performanceBudgetPath = Join-Path $repoRoot 'docs\performance\performance-budget-v1.json'
$performanceBudgetJson = Get-Content $performanceBudgetPath -Raw
if (-not ($performanceBudgetJson | Test-Json -SchemaFile (Join-Path $repoRoot 'docs\performance\performance-budget-v1.schema.json'))) {
    throw 'Performance budget does not match schema version 1.'
}
$performanceBudget = $performanceBudgetJson | ConvertFrom-Json
$commitOutput = & git -C $repoRoot rev-parse HEAD
if ($LASTEXITCODE -ne 0 -or $null -eq $commitOutput) {
    throw 'Unable to resolve the release commit SHA.'
}
$commit = $commitOutput.Trim()
if ($commit -notmatch '^[0-9a-f]{40}$') {
    throw 'The resolved release commit is not a full SHA.'
}

$evidence = [ordered]@{
    compatibilityMatrix = 'docs/releases/compatibility-matrix.md'
    qualityChecklist = 'docs/releases/stable-quality-checklist.md'
    performanceBaseline = 'docs/releases/performance-baseline.md'
    performanceBudget = 'docs/performance/performance-budget-v1.json'
    releaseNotesTemplate = 'docs/releases/release-notes-template.md'
}
foreach ($relativePath in $evidence.Values) {
    if (-not (Test-Path (Join-Path $repoRoot $relativePath) -PathType Leaf)) {
        throw "Release evidence file is missing: $relativePath"
    }
}

$metadata = [ordered]@{
    schemaVersion = 1
    generatedAtUtc = [DateTimeOffset]::UtcNow.ToString('O')
    version = $Version
    informationalVersion = $InformationalVersion
    commit = $commit
    sourceBranch = $SourceBranch
    buildId = $BuildId
    buildUri = $BuildUri
    unoSdk = $globalJson.'msbuild-sdks'.'Uno.Sdk'
    performanceBudgetVersion = [int]$performanceBudget.budgetVersion
    performanceBudgetStatus = [string]$performanceBudget.status
    evidence = $evidence
}

$parent = Split-Path -Parent $OutputPath
if ($parent) {
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
}

$metadata | ConvertTo-Json -Depth 5 | Set-Content -Path $OutputPath -Encoding utf8NoBOM
Write-Host "Release metadata written to $OutputPath"
