#Requires -Version 7.1
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $GeneratedSourcePath,

    [Parameter(Mandatory)]
    [string] $OutputPath,

    [string] $RoslynDir = ''
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not (Test-Path $GeneratedSourcePath -PathType Container)) {
    throw "Generated source directory does not exist: $GeneratedSourcePath"
}

$manifestSources = @(Get-ChildItem $GeneratedSourcePath -Recurse -File -Filter 'SampleManifest.g.cs')
if ($manifestSources.Count -ne 1) {
    throw "Expected exactly one SampleManifest.g.cs under '$GeneratedSourcePath'; found $($manifestSources.Count)."
}

if ($null -eq ('Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree' -as [type])) {
    if (-not $RoslynDir) {
        $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
        Push-Location $repoRoot
        try {
            $resolvedVersion = (& dotnet --version).Trim()
            $sdkLines = @(& dotnet --list-sdks)
        } finally {
            Pop-Location
        }
        $sdkLine = $sdkLines | Where-Object { $_ -match "^$([regex]::Escape($resolvedVersion))\s+\[(.+)\]$" } | Select-Object -First 1
        if ($null -eq $sdkLine -or $sdkLine -notmatch '^(\S+)\s+\[(.+)\]$') {
            throw "Unable to locate the global.json-resolved SDK '$resolvedVersion' in dotnet --list-sdks."
        }
        $RoslynDir = Join-Path $Matches[2] $Matches[1] 'Roslyn' 'bincore'
    }

    foreach ($assemblyName in @('Microsoft.CodeAnalysis.dll', 'Microsoft.CodeAnalysis.CSharp.dll')) {
        $assemblyPath = Join-Path $RoslynDir $assemblyName
        if (-not (Test-Path $assemblyPath -PathType Leaf)) {
            throw "Roslyn assembly not found: $assemblyPath"
        }
        try {
            Add-Type -Path $assemblyPath -ErrorAction Stop
        } catch {
            if ($null -eq ('Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree' -as [type])) {
                throw "Unable to load Roslyn '$assemblyPath' in PowerShell $($PSVersionTable.PSVersion): $($_.Exception.Message)"
            }
            break
        }
    }
}

$source = Get-Content $manifestSources[0].FullName -Raw
$tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText($source)
$root = $tree.GetRoot()
$manifestClass = @($root.DescendantNodes() |
    Where-Object {
        $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax] -and
        $_.Identifier.ValueText -eq 'SampleManifest'
    })
if ($manifestClass.Count -ne 1) {
    throw "Generated source does not contain exactly one SampleManifest class."
}

$getJsonMethod = @($manifestClass[0].Members |
    Where-Object {
        $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax] -and
        $_.Identifier.ValueText -eq 'GetJson'
    })
if ($getJsonMethod.Count -ne 1) {
    throw "Generated SampleManifest does not contain exactly one GetJson method."
}

$jsonBuilder = [System.Text.StringBuilder]::new()
$appendCalls = @($getJsonMethod[0].DescendantNodes() |
    Where-Object {
        $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -and
        $_.Expression -is [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
        $_.Expression.Expression.ToString() -eq 'sb' -and
        $_.Expression.Name.Identifier.ValueText -eq 'Append'
    })
$allAppendCalls = @($getJsonMethod[0].DescendantNodes() |
    Where-Object {
        $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -and
        $_.Expression -is [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
        $_.Expression.Name.Identifier.ValueText -eq 'Append'
    })
$unsupportedSbCalls = @($getJsonMethod[0].DescendantNodes() |
    Where-Object {
        $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] -and
        $_.Expression -is [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax] -and
        $_.Expression.Expression.ToString().StartsWith('sb', [StringComparison]::Ordinal) -and
        $_.Expression.Name.Identifier.ValueText -notin @('Append', 'ToString')
    })
if ($appendCalls.Count -eq 0) {
    throw 'Generated SampleManifest.GetJson contains no sb.Append calls.'
}
if ($appendCalls.Count -ne $allAppendCalls.Count) {
    throw 'Generated SampleManifest.GetJson contains an unsupported chained or non-sb Append call.'
}
if ($unsupportedSbCalls.Count -gt 0) {
    throw "Generated SampleManifest.GetJson contains unsupported StringBuilder call '$($unsupportedSbCalls[0].Expression)'."
}

foreach ($call in $appendCalls) {
    if ($call.ArgumentList.Arguments.Count -ne 1) {
        throw 'Every generated sb.Append call must contain exactly one argument.'
    }
    $expression = $call.ArgumentList.Arguments[0].Expression
    if ($expression -isnot [Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax] -or
        $expression.Token.Value -isnot [string]) {
        throw "Generated manifest append argument is not a constant string: $expression"
    }
    [void]$jsonBuilder.Append([string]$expression.Token.Value)
}

$json = $jsonBuilder.ToString()
$manifest = $json | ConvertFrom-Json -Depth 100
if ($manifest.schemaVersion -notin @(1, 2) -or $null -eq $manifest.samples) {
    throw 'Generated manifest does not match a supported schema-v1/v2 envelope.'
}

$fqns = @($manifest.samples | ForEach-Object { [string]$_.fqn })
$sortedFqns = [string[]]$fqns.Clone()
[Array]::Sort($sortedFqns, [StringComparer]::Ordinal)
if ([string]::Join("`n", $fqns) -ne [string]::Join("`n", $sortedFqns)) {
    throw 'Generated manifest samples are not sorted by fully-qualified type name.'
}
$slugSet = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
foreach ($sample in $manifest.samples) {
    if (-not $slugSet.Add([string]$sample.slug)) {
        throw "Generated manifest contains duplicate sample slug '$($sample.slug)'."
    }
}

$outputDirectory = Split-Path -Parent $OutputPath
if ($outputDirectory) {
    New-Item -ItemType Directory -Path $outputDirectory -Force | Out-Null
}
[System.IO.File]::WriteAllText(
    $OutputPath,
    $json,
    [System.Text.UTF8Encoding]::new($false))

$hash = (Get-FileHash $OutputPath -Algorithm SHA256).Hash.ToLowerInvariant()
[System.IO.File]::WriteAllText(
    "$OutputPath.sha256",
    "$hash  $([System.IO.Path]::GetFileName($OutputPath))`n",
    [System.Text.UTF8Encoding]::new($false))

Write-Host "Exported $(@($manifest.samples).Count) samples to $OutputPath"
Write-Host "SHA256: $hash"
