#Requires -Version 7.0
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $BudgetPath,

    [Parameter(Mandatory)]
    [string[]] $BundleMetricsPath,

    [string] $RuntimeObservationPath = '',

    [Parameter(Mandatory)]
    [string] $OutputPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$budgetSchema = Join-Path $repoRoot 'docs\performance\performance-budget-v1.schema.json'
$bundleSchema = Join-Path $repoRoot 'docs\performance\wasm-bundle-metrics-v1.schema.json'
$runtimeSchema = Join-Path $repoRoot 'docs\performance\runtime-observation-v1.schema.json'
$reportSchema = Join-Path $repoRoot 'docs\performance\performance-report-v1.schema.json'

function Read-ValidatedJson([string] $Path, [string] $SchemaPath, [string] $Description) {
    $json = Get-Content $Path -Raw
    $schemaErrors = @()
    if (-not ($json | Test-Json -SchemaFile $SchemaPath -ErrorAction SilentlyContinue -ErrorVariable +schemaErrors)) {
        throw "$Description '$Path' failed schema validation: $([string]::Join(' | ', $schemaErrors))"
    }
    return $json | ConvertFrom-Json -Depth 100
}

function Get-TargetBudget([object] $Budget, [string] $Target) {
    $property = $Budget.targets.PSObject.Properties[$Target]
    if ($null -eq $property) {
        throw "Performance budget has no target '$Target'."
    }
    return $property.Value
}

$checks = [System.Collections.Generic.List[object]]::new()
function Add-Check(
    [string] $Target,
    [string] $Scope,
    [string] $Metric,
    [object] $Actual,
    [object] $Limit,
    [bool] $Evaluated,
    [string] $Reason = ''
) {
    $baseline = [double]$Limit.baseline
    $maximum = [double]$Limit.maximum
    if ($maximum -lt $baseline) {
        throw "$Target $Scope.$Metric maximum '$maximum' is lower than baseline '$baseline'."
    }
    $hasActual = $null -ne $Actual
    $passed = $Evaluated -and $hasActual -and [double]$Actual -le $maximum
    $checkReason = if (-not [string]::IsNullOrWhiteSpace($Reason)) {
        $Reason
    }
    elseif (-not $hasActual) {
        'Metric was not produced.'
    }
    elseif (-not $passed) {
        "Actual $Actual exceeds maximum $maximum."
    }
    else {
        $null
    }
    $checks.Add([ordered]@{
        target = $Target
        scope = $Scope
        metric = $Metric
        evaluated = $Evaluated
        actual = if ($hasActual) { [double]$Actual } else { $null }
        baseline = $baseline
        maximum = $maximum
        passed = $passed
        reason = $checkReason
    })
}

$budget = Read-ValidatedJson $BudgetPath $budgetSchema 'Performance budget'
$bundleCommitsByTarget = @{}
$bundleMetricNames = @(
    'rawPayloadBytes',
    'estimatedBrotliTransferBytes',
    'dotnetNativeWasmBytes',
    'dotnetNativeBrotliBytes'
)
foreach ($metricsPath in $BundleMetricsPath) {
    $metrics = Read-ValidatedJson $metricsPath $bundleSchema 'Bundle metrics'
    if ($metrics.flavor -ne 'core') {
        throw "Budget comparison accepts core bundle metrics only; '$metricsPath' is '$($metrics.flavor)'."
    }
    if ($null -ne $metrics.buildCommit) {
        $bundleCommitsByTarget[[string]$metrics.target] = [string]$metrics.buildCommit
    }
    $targetBudget = Get-TargetBudget $budget ([string]$metrics.target)
    foreach ($metricName in $bundleMetricNames) {
        Add-Check `
            -Target $metrics.target `
            -Scope 'bundle' `
            -Metric $metricName `
            -Actual $metrics.metrics.$metricName `
            -Limit $targetBudget.bundle.$metricName `
            -Evaluated $true
    }
}

if (-not [string]::IsNullOrWhiteSpace($RuntimeObservationPath)) {
    $runtime = Read-ValidatedJson $RuntimeObservationPath $runtimeSchema 'Runtime observation'
    if ([string]$runtime.configuration.configSha256 -cne [string]$budget.runtimeConfigSha256) {
        throw "Runtime observation config '$($runtime.configuration.configSha256)' does not match budget '$($budget.runtimeConfigSha256)'."
    }
    if ([string]$runtime.configuration.toolSha256 -cne [string]$budget.runtimeToolSha256) {
        throw "Runtime observation tool '$($runtime.configuration.toolSha256)' does not match budget '$($budget.runtimeToolSha256)'."
    }
    $bundleCommit = $bundleCommitsByTarget[[string]$runtime.target]
    if ($null -ne $runtime.buildCommit -and
        $null -ne $bundleCommit -and
        [string]$runtime.buildCommit -cne [string]$bundleCommit) {
        throw "Runtime observation commit '$($runtime.buildCommit)' does not match bundle commit '$bundleCommit'."
    }
    $targetBudget = Get-TargetBudget $budget ([string]$runtime.target)
    if ($null -eq $targetBudget.runtime) {
        throw "Performance budget target '$($runtime.target)' has no runtime budget."
    }
    $runtimeMetricNames = @(
        'firstContentfulPaintMs',
        'shellReadyMs',
        'firstInputLatencyMs',
        'searchRenderedMs',
        'navigationRenderedMs'
    )
    foreach ($scope in @('cold', 'warm')) {
        $count = [int]$runtime.summaries.$scope.observationCount
        $enoughObservations = $count -ge [int]$budget.minimumRuntimeObservations
        $reason = if ($enoughObservations) {
            ''
        } else {
            "Only $count observations were produced; $($budget.minimumRuntimeObservations) are required."
        }
        foreach ($metricName in $runtimeMetricNames) {
            Add-Check `
                -Target $runtime.target `
                -Scope $scope `
                -Metric $metricName `
                -Actual $runtime.summaries.$scope.$metricName.p75 `
                -Limit $targetBudget.runtime.$scope.$metricName `
                -Evaluated $enoughObservations `
                -Reason $reason
        }
    }
}

if ($checks.Count -eq 0) {
    throw 'Performance comparison produced no checks.'
}
$passed = @($checks | Where-Object { -not $_.passed }).Count -eq 0
$report = [ordered]@{
    schemaVersion = 1
    budgetVersion = [int]$budget.budgetVersion
    enforcement = [string]$budget.status
    generatedAt = [DateTime]::UtcNow.ToString('O')
    passed = $passed
    checks = $checks
}
$json = $report | ConvertTo-Json -Depth 20
if (-not ($json | Test-Json -SchemaFile $reportSchema)) {
    throw 'Generated performance report does not match schema version 1.'
}
$outputDirectory = Split-Path $OutputPath -Parent
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
$json | Set-Content $OutputPath -Encoding utf8NoBOM

if (-not $passed) {
    $failedChecks = @($checks | Where-Object { -not $_.passed })
    $message = "Performance budget has $($failedChecks.Count) failed or unevaluated check(s)."
    if ($budget.status -eq 'blocking') {
        throw $message
    }
    Write-Host "##vso[task.logissue type=warning]$message"
    Write-Warning $message
} else {
    Write-Host "Performance budget v$($budget.budgetVersion) passed ($($checks.Count) checks, $($budget.status))."
}
