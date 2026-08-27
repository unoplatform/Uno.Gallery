#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $WasmRoot,

    [Parameter(Mandatory)]
    [string] $TargetName,

    [Parameter(Mandatory)]
    [string] $OutputPath,

    [ValidateSet('core', 'instrumented', 'extensions', 'visual')]
    [string] $Flavor = 'core',

    [string] $BuildCommit = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$resolvedRoot = (Resolve-Path $WasmRoot).Path
$files = @(Get-ChildItem $resolvedRoot -Recurse -File)
if ($files.Count -eq 0) {
    throw "WebAssembly artifact '$resolvedRoot' contains no files."
}

$rawFiles = @($files | Where-Object {
    $_.Extension -notin @('.br', '.gz', '.map')
})
$brotliFiles = @($files | Where-Object Extension -eq '.br')
$rawByPath = @{}
foreach ($file in $files) {
    $rawByPath[$file.FullName] = $file
}

[long]$estimatedTransferBytes = 0
foreach ($file in $rawFiles) {
    $brotliPath = "$($file.FullName).br"
    $gzipPath = "$($file.FullName).gz"
    if ($rawByPath.ContainsKey($brotliPath)) {
        $estimatedTransferBytes += $rawByPath[$brotliPath].Length
    }
    elseif ($rawByPath.ContainsKey($gzipPath)) {
        $estimatedTransferBytes += $rawByPath[$gzipPath].Length
    }
    else {
        $estimatedTransferBytes += $file.Length
    }
}

$nativeWasm = @($rawFiles | Where-Object {
    $_.Name -match '^dotnet\.native(?:\.[^.]+)?\.wasm$'
})
if ($nativeWasm.Count -ne 1) {
    throw "Expected one dotnet.native*.wasm in '$resolvedRoot'; found $($nativeWasm.Count)."
}
$nativeBrotliPath = "$($nativeWasm[0].FullName).br"
$nativeBrotliBytes = if ($rawByPath.ContainsKey($nativeBrotliPath)) {
    [long]$rawByPath[$nativeBrotliPath].Length
} else {
    $null
}

$frameworkRoot = Join-Path $resolvedRoot '_framework'
[long]$managedWebcilBytes = @($rawFiles | Where-Object {
    $_.Extension -eq '.wasm' -and
    $_.FullName.StartsWith($frameworkRoot, [StringComparison]::OrdinalIgnoreCase) -and
    $_.FullName -ne $nativeWasm[0].FullName
} | Measure-Object Length -Sum).Sum

$commit = if ([string]::IsNullOrWhiteSpace($BuildCommit)) {
    $null
} else {
    $BuildCommit.Trim().ToLowerInvariant()
}
if ($null -ne $commit -and $commit -notmatch '^[0-9a-f]{7,40}$') {
    throw "BuildCommit must be a 7-40 character hexadecimal revision, got '$BuildCommit'."
}

$report = [ordered]@{
    schemaVersion = 1
    generatedAt = [DateTime]::UtcNow.ToString('O')
    buildCommit = $commit
    target = $TargetName
    flavor = $Flavor
    metrics = [ordered]@{
        fileCount = $rawFiles.Count
        rawPayloadBytes = [long]($rawFiles | Measure-Object Length -Sum).Sum
        precompressedBrotliBytes = [long]($brotliFiles | Measure-Object Length -Sum).Sum
        estimatedBrotliTransferBytes = $estimatedTransferBytes
        dotnetNativeWasmBytes = [long]$nativeWasm[0].Length
        dotnetNativeBrotliBytes = $nativeBrotliBytes
        managedWebcilBytes = $managedWebcilBytes
    }
}

$json = $report | ConvertTo-Json -Depth 10
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$schemaPath = Join-Path $repoRoot 'docs\performance\wasm-bundle-metrics-v1.schema.json'
if (-not ($json | Test-Json -SchemaFile $schemaPath)) {
    throw 'Generated WebAssembly bundle metrics do not match schema version 1.'
}

$outputDirectory = Split-Path $OutputPath -Parent
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
$json | Set-Content $OutputPath -Encoding utf8NoBOM
Write-Host "Measured $TargetName ($Flavor): raw=$($report.metrics.rawPayloadBytes), estimated Brotli transfer=$estimatedTransferBytes."
