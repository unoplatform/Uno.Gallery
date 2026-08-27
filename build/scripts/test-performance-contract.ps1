#Requires -Version 7.0
[CmdletBinding()]
param(
    [string] $ScratchRoot = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
if ([string]::IsNullOrWhiteSpace($ScratchRoot)) {
    $ScratchRoot = [IO.Path]::GetTempPath()
}
$scratchBase = [IO.Path]::GetFullPath($ScratchRoot)
$scratch = Join-Path $scratchBase "uno-gallery-performance-contract-$PID-$([Guid]::NewGuid().ToString('N'))"
New-Item -ItemType Directory -Path $scratch -Force | Out-Null

try {
    $artifact = Join-Path $scratch 'wwwroot'
    $framework = Join-Path $artifact '_framework'
    New-Item -ItemType Directory -Path $framework -Force | Out-Null
    [IO.File]::WriteAllBytes((Join-Path $artifact 'index.html'), [byte[]]::new(100))
    [IO.File]::WriteAllBytes((Join-Path $framework 'dotnet.native.fixture.wasm'), [byte[]]::new(1000))
    [IO.File]::WriteAllBytes((Join-Path $framework 'dotnet.native.fixture.wasm.br'), [byte[]]::new(400))
    [IO.File]::WriteAllBytes((Join-Path $framework 'Uno.Gallery.fixture.wasm'), [byte[]]::new(300))
    [IO.File]::WriteAllBytes((Join-Path $framework 'Uno.Gallery.fixture.wasm.br'), [byte[]]::new(120))

    $metricsPath = Join-Path $scratch 'bundle.json'
    & (Join-Path $PSScriptRoot 'measure-wasm-bundle.ps1') `
        -WasmRoot $artifact `
        -TargetName 'WASM-DOM' `
        -OutputPath $metricsPath `
        -BuildCommit 'abcdef0'
    $metrics = Get-Content $metricsPath -Raw | ConvertFrom-Json -Depth 20
    if ($metrics.metrics.rawPayloadBytes -ne 1400 -or
        $metrics.metrics.estimatedBrotliTransferBytes -ne 620 -or
        $metrics.metrics.dotnetNativeBrotliBytes -ne 400) {
        throw 'Bundle fixture produced unexpected size metrics.'
    }

    $run = [ordered]@{
        firstContentfulPaintMs = 100
        shellReadyMs = 200
        firstInputLatencyMs = 5
        searchRenderedMs = 10
        navigationRenderedMs = 20
    }
    $runtime = [ordered]@{
        schemaVersion = 1
        suiteVersion = 1
        generatedAt = [DateTime]::UtcNow.ToString('O')
        buildCommit = 'abcdef0'
        target = 'WASM-DOM'
        browser = [ordered]@{
            version = 'Chrome/fixture'
            headless = $true
            softwareRendering = $true
            locale = 'en-US'
            timezone = 'UTC'
        }
        host = [ordered]@{
            platform = 'fixture'
            release = '1'
            architecture = 'x64'
            nodeVersion = 'v22.0.0'
        }
        runs = [ordered]@{
            cold = @($run, $run, $run, $run, $run)
            warm = @($run, $run, $run, $run, $run)
        }
        summaries = [ordered]@{
            cold = [ordered]@{
                observationCount = 5
                firstContentfulPaintMs = [ordered]@{ minimum = 100; p50 = 100; p75 = 100; maximum = 100 }
                shellReadyMs = [ordered]@{ minimum = 200; p50 = 200; p75 = 200; maximum = 200 }
                firstInputLatencyMs = [ordered]@{ minimum = 5; p50 = 5; p75 = 5; maximum = 5 }
                searchRenderedMs = [ordered]@{ minimum = 10; p50 = 10; p75 = 10; maximum = 10 }
                navigationRenderedMs = [ordered]@{ minimum = 20; p50 = 20; p75 = 20; maximum = 20 }
            }
            warm = [ordered]@{
                observationCount = 5
                firstContentfulPaintMs = [ordered]@{ minimum = 100; p50 = 100; p75 = 100; maximum = 100 }
                shellReadyMs = [ordered]@{ minimum = 200; p50 = 200; p75 = 200; maximum = 200 }
                firstInputLatencyMs = [ordered]@{ minimum = 5; p50 = 5; p75 = 5; maximum = 5 }
                searchRenderedMs = [ordered]@{ minimum = 10; p50 = 10; p75 = 10; maximum = 10 }
                navigationRenderedMs = [ordered]@{ minimum = 20; p50 = 20; p75 = 20; maximum = 20 }
            }
        }
    }
    $runtimePath = Join-Path $scratch 'runtime.json'
    $runtime | ConvertTo-Json -Depth 20 | Set-Content $runtimePath -Encoding utf8NoBOM

    $bundleBudget = [ordered]@{}
    foreach ($name in @(
        'rawPayloadBytes',
        'estimatedBrotliTransferBytes',
        'dotnetNativeWasmBytes',
        'dotnetNativeBrotliBytes'
    )) {
        $value = [double]$metrics.metrics.$name
        $bundleBudget[$name] = [ordered]@{ baseline = $value; maximum = $value + 1 }
    }
    $runtimeBudget = [ordered]@{}
    foreach ($name in @(
        'firstContentfulPaintMs',
        'shellReadyMs',
        'firstInputLatencyMs',
        'searchRenderedMs',
        'navigationRenderedMs'
    )) {
        $value = [double]$runtime.summaries.cold.$name.p75
        $runtimeBudget[$name] = [ordered]@{ baseline = $value; maximum = $value + 1 }
    }
    $budget = [ordered]@{
        schemaVersion = 1
        budgetVersion = 1
        status = 'advisory'
        approvedOn = $null
        minimumRuntimeObservations = 5
        targets = [ordered]@{
            'WASM-DOM' = [ordered]@{
                bundle = $bundleBudget
                runtime = [ordered]@{
                    cold = $runtimeBudget
                    warm = $runtimeBudget
                }
            }
        }
    }
    $budgetPath = Join-Path $scratch 'budget.json'
    $budget | ConvertTo-Json -Depth 20 | Set-Content $budgetPath -Encoding utf8NoBOM

    $reportPath = Join-Path $scratch 'report.json'
    & (Join-Path $PSScriptRoot 'compare-performance-budget.ps1') `
        -BudgetPath $budgetPath `
        -BundleMetricsPath $metricsPath `
        -RuntimeObservationPath $runtimePath `
        -OutputPath $reportPath
    $report = Get-Content $reportPath -Raw | ConvertFrom-Json -Depth 20
    if (-not $report.passed -or @($report.checks).Count -ne 14) {
        throw 'Passing performance fixture did not produce fourteen successful checks.'
    }

    $metrics.metrics.rawPayloadBytes = [double]$bundleBudget.rawPayloadBytes.maximum + 1
    $metrics | ConvertTo-Json -Depth 20 | Set-Content $metricsPath -Encoding utf8NoBOM
    & (Join-Path $PSScriptRoot 'compare-performance-budget.ps1') `
        -BudgetPath $budgetPath `
        -BundleMetricsPath $metricsPath `
        -OutputPath $reportPath
    $report = Get-Content $reportPath -Raw | ConvertFrom-Json -Depth 20
    if ($report.passed) {
        throw 'Advisory performance comparison accepted an over-budget bundle.'
    }

    $budget.status = 'blocking'
    $budget | ConvertTo-Json -Depth 20 | Set-Content $budgetPath -Encoding utf8NoBOM
    $blockingRejected = $false
    try {
        & (Join-Path $PSScriptRoot 'compare-performance-budget.ps1') `
            -BudgetPath $budgetPath `
            -BundleMetricsPath $metricsPath `
            -OutputPath $reportPath
    } catch {
        if ($_.Exception.Message -notmatch 'failed or unevaluated') { throw }
        $blockingRejected = $true
    }
    if (-not $blockingRejected) {
        throw 'Blocking performance comparison accepted an over-budget bundle.'
    }

    $configJson = Get-Content (Join-Path $repoRoot 'build\performance\performance.config.json') -Raw
    $configSchema = Join-Path $repoRoot 'docs\performance\performance-config-v1.schema.json'
    if (-not ($configJson | Test-Json -SchemaFile $configSchema)) {
        throw 'Runtime performance configuration failed schema validation.'
    }
} finally {
    if (Test-Path $scratch) {
        Remove-Item $scratch -Recurse -Force
    }
}

Write-Host 'Performance schemas, measurement, advisory, and blocking contracts passed.'
