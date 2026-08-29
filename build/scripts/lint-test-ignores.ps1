#Requires -Version 7.1
<#
.SYNOPSIS
    Lint NUnit [Ignore] attributes and Assert.Ignore calls in UITest C# sources.
    Uses Roslyn (Microsoft.CodeAnalysis.CSharp) so comments and strings are parsed
    from the AST — no custom lexer required.

.DESCRIPTION
    Each active Ignore must supply BOTH:
      1. A GitHub issue URL: https://github.com/unoplatform/Uno.Gallery/issues/<n>
      2. A non-past review date: review-date: YYYY-MM-DD

    Ignore forms detected:
      Attributes : [Ignore(...)], [IgnoreAttribute(...)], [NUnit.Framework.Ignore(...)]
      Invocations: Assert.Ignore(...), NUnit.Framework.Assert.Ignore(...)

    Argument forms accepted: normal, @verbatim, raw ("""), concatenated (+), parenthesized.
    Interpolated / variable / method expressions are a policy violation.

    EXIT CODES  0 = all compliant  |  1 = violations found

.PARAMETER TestRoot
    Root directory scanned recursively for *.cs files.
    Defaults to <repo-root>/Uno.Gallery.UITests.

.PARAMETER RoslynDir
    Directory containing Microsoft.CodeAnalysis.dll and Microsoft.CodeAnalysis.CSharp.dll.
    Auto-detected from the highest installed stable .NET SDK (highest prerelease if no
    stable is installed) when omitted.
#>
[CmdletBinding()]
param(
    [string] $TestRoot  = '',
    [string] $RoslynDir = ''
)
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Repo root is 2 levels up from build/scripts/
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..' '..')).Path

if (-not $TestRoot) { $TestRoot = Join-Path $repoRoot 'Uno.Gallery.UITests' }
if (-not (Test-Path $TestRoot -PathType Container)) {
    Write-Host "[ERROR] TestRoot not found or is not a directory: '$TestRoot'"; exit 1
}
$TestRoot = (Resolve-Path $TestRoot).Path
Write-Host "lint-test-ignores: scanning $TestRoot"

# ---------------------------------------------------------------------------
# SDK selection helper
# ---------------------------------------------------------------------------
# Parse `dotnet --list-sdks` output and return the best SDK object:
#   - Highest installed STABLE SDK wins regardless of any higher prerelease major versions.
#   - If no stable SDK exists, the highest prerelease is chosen.
#   - All comparisons are fully numeric (Major, Minor, Patch; prerelease build/revision).
function Select-BestSdk([string[]]$lines) {
    $sdks = @($lines | ForEach-Object {
        if ($_ -match '^(\S+)\s+\[(.+)\]\s*$') {
            $verStr   = $Matches[1]
            $basePath = $Matches[2]
            if ($verStr -match '^(\d+)\.(\d+)\.(\d+)(.*)') {
                $maj  = [int]$Matches[1]
                $min  = [int]$Matches[2]
                $pat  = [int]$Matches[3]
                $pre  = $Matches[4]   # '' = stable; '-preview.N.BUILD.REV' = prerelease
                # Extract numeric segments from the prerelease suffix for stable ordering
                $preN = @(0, 0, 0)
                if ($pre -match '-[a-zA-Z]+\.(\d+)\.(\d+)\.(\d+)') {
                    $preN = @([int]$Matches[1], [int]$Matches[2], [int]$Matches[3])
                } elseif ($pre -match '-[a-zA-Z]+\.(\d+)\.(\d+)') {
                    $preN = @([int]$Matches[1], [int]$Matches[2], 0)
                }
                [PSCustomObject]@{
                    VersionStr = $verStr
                    Major = $maj; Minor = $min; Patch = $pat
                    Pre   = $pre; PreN  = $preN
                    Base  = $basePath
                }
            }
        }
    } | Where-Object { $_ })

    if (-not $sdks) { return $null }

    # Sort ascending, pick last: stable tier (1) always sorts after prerelease tier (0),
    # so the highest stable wins; highest prerelease wins only when no stable exists.
    $sdks | Sort-Object {
        $stableTier = if ($_.Pre) { 0 } else { 1 }
        $p = $_.PreN
        '{0:D3}{1:D10}{2:D10}{3:D10}{4:D10}{5:D10}{6:D10}' -f
            $stableTier, $_.Major, $_.Minor, $_.Patch, $p[0], $p[1], $p[2]
    } | Select-Object -Last 1
}

# ---------------------------------------------------------------------------
# Locate Roslyn binaries
# ---------------------------------------------------------------------------
if ($null -eq ('Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree' -as [type])) {
    if (-not $RoslynDir) {
        $sdkLines = & dotnet --list-sdks 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Host "[ERROR] 'dotnet --list-sdks' failed — ensure a .NET SDK is installed.`n$sdkLines"
            exit 1
        }
        $best = Select-BestSdk $sdkLines
        if (-not $best) { Write-Host "[ERROR] No .NET SDKs found via 'dotnet --list-sdks'"; exit 1 }
        $RoslynDir = Join-Path $best.Base $best.VersionStr 'Roslyn' 'bincore'
    }

    foreach ($dll in @(
        (Join-Path $RoslynDir 'Microsoft.CodeAnalysis.dll'),
        (Join-Path $RoslynDir 'Microsoft.CodeAnalysis.CSharp.dll')
    )) {
        if (-not (Test-Path $dll)) {
            Write-Host "[ERROR] Roslyn assembly not found: '$dll'`nPass -RoslynDir or install a .NET SDK."
            exit 1
        }
        try {
            Add-Type -Path $dll -ErrorAction Stop
        } catch {
            if ($null -eq ('Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree' -as [type])) {
                throw
            }
            break
        }
    }
}

# ---------------------------------------------------------------------------
# Policy patterns
# ---------------------------------------------------------------------------
$issueUrlRx   = [regex]'https://github\.com/unoplatform/Uno\.Gallery/issues/\d+'
$reviewDateRx = [regex]'review-date:\s*(\d{4}-\d{2}-\d{2})'
$today        = [datetime]::Today

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

# Walk a constant string expression; returns @{OK; Value} or @{OK=$false; Value=reason}
function Get-ConstantString($node) {
    while ($node -is [Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedExpressionSyntax]) {
        $node = $node.Expression
    }
    # Normal / @verbatim / raw string literal — Token.Value gives the processed string content
    if ($node -is [Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax]) {
        $v = $node.Token.Value
        if ($v -is [string]) { return @{ OK = $true; Value = $v } }
    }
    # "a" + "b" concatenation (recurse each side)
    if ($node -is [Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax] -and
        $node.RawKind -eq [int][Microsoft.CodeAnalysis.CSharp.SyntaxKind]::AddExpression) {
        $l = Get-ConstantString $node.Left
        $r = Get-ConstantString $node.Right
        if ($l.OK -and $r.OK) { return @{ OK = $true; Value = $l.Value + $r.Value } }
        return if (-not $l.OK) { $l } else { $r }
    }
    $reason = if ($node -is [Microsoft.CodeAnalysis.CSharp.Syntax.InterpolatedStringExpressionSyntax]) {
        'interpolated string — constant string required'
    } else {
        "$($node.GetType().Name) — constant string required"
    }
    return @{ OK = $false; Value = $reason }
}

# True when an attribute name matches Ignore / IgnoreAttribute / NUnit.Framework.Ignore[Attribute]
function Test-IsIgnoreName([string]$name) {
    $name -in @('Ignore', 'IgnoreAttribute') -or
    $name -match '^(NUnit\.Framework\.|global::NUnit\.Framework\.)Ignore(Attribute)?$'
}

# True when a MemberAccessExpression is Assert.Ignore or NUnit.Framework.Assert.Ignore
function Test-IsIgnoreCall($expr) {
    ($expr -is [Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax]) -and
    ($expr.Name.Identifier.ValueText -eq 'Ignore') -and
    ($expr.Expression.ToString() -in @('Assert', 'NUnit.Framework.Assert', 'global::NUnit.Framework.Assert'))
}

function Test-IgnoreText([string]$label, [string]$text, [ref]$hasViolation) {
    if (-not $issueUrlRx.IsMatch($text)) {
        Write-Host "[FAIL] $label -- missing required GitHub issue URL (https://github.com/unoplatform/Uno.Gallery/issues/<number>)"
        $hasViolation.Value = $true
    }
    $m = $reviewDateRx.Match($text)
    if (-not $m.Success) {
        Write-Host "[FAIL] $label -- missing required field 'review-date: YYYY-MM-DD'"
        $hasViolation.Value = $true; return
    }
    $ds = $m.Groups[1].Value
    $dt = [datetime]::MinValue
    $ok = [datetime]::TryParseExact($ds, 'yyyy-MM-dd',
              [System.Globalization.CultureInfo]::InvariantCulture,
              [System.Globalization.DateTimeStyles]::None, [ref]$dt)
    if (-not $ok) {
        Write-Host "[FAIL] $label -- review-date '$ds' is not a valid ISO-8601 calendar date"
        $hasViolation.Value = $true; return
    }
    if ($dt -lt $today) {
        Write-Host "[FAIL] $label -- review-date '$ds' is in the past (today: $($today.ToString('yyyy-MM-dd'))); resolve the issue or extend the review date"
        $hasViolation.Value = $true
    }
}

# ---------------------------------------------------------------------------
# Main scan
# ---------------------------------------------------------------------------
$hasViolation = $false
$totalIgnores = 0
$files = @(Get-ChildItem -Path $TestRoot -Filter '*.cs' -Recurse -File |
    Where-Object { $_.FullName -notmatch '(?:[\\/])(?:bin|obj)(?:[\\/])' })

if (-not $files) {
    Write-Host 'lint-test-ignores: no *.cs files found under TestRoot — nothing to check.'; exit 0
}

foreach ($file in $files) {
    $relPath = ([IO.Path]::GetRelativePath($repoRoot, $file.FullName)) -replace '\\', '/'
    $tree = [Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree]::ParseText(
                [System.IO.File]::ReadAllText($file.FullName))

    # Syntax errors are violations (with file:line and diagnostic message)
    $errs = @($tree.GetDiagnostics() | Where-Object Severity -eq 'Error')
    foreach ($d in $errs) {
        $ln = $tree.GetLineSpan($d.Location.SourceSpan).StartLinePosition.Line + 1
        Write-Host "[FAIL] ${relPath}:${ln} -- syntax error: $($d.GetMessage())"
        $hasViolation = $true
    }
    if ($errs) { continue }

    $nodes = $tree.GetRoot().DescendantNodes()

    # --- [Ignore] / [IgnoreAttribute] / [NUnit.Framework.Ignore] attributes --
    foreach ($attr in @($nodes | Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.AttributeSyntax] })) {
        if (-not (Test-IsIgnoreName $attr.Name.ToString())) { continue }
        $totalIgnores++
        $line  = $tree.GetLineSpan($attr.Span).StartLinePosition.Line + 1
        $label = "${relPath}:${line}"
        $arg0  = $attr.ArgumentList?.Arguments[0]?.Expression
        if (-not $arg0) {
            Write-Host "[FAIL] $label -- Ignore attribute has no string argument"; $hasViolation = $true; continue
        }
        $cs = Get-ConstantString $arg0
        if (-not $cs.OK) { Write-Host "[FAIL] $label -- $($cs.Value)"; $hasViolation = $true; continue }
        Test-IgnoreText -label $label -text $cs.Value -hasViolation ([ref]$hasViolation)
    }

    # --- Assert.Ignore / NUnit.Framework.Assert.Ignore invocations -----------
    foreach ($inv in @($nodes | Where-Object { $_ -is [Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax] })) {
        if (-not (Test-IsIgnoreCall $inv.Expression)) { continue }
        $totalIgnores++
        $line  = $tree.GetLineSpan($inv.Span).StartLinePosition.Line + 1
        $label = "${relPath}:${line}"
        $arg0  = $inv.ArgumentList?.Arguments[0]?.Expression
        if (-not $arg0) {
            Write-Host "[FAIL] $label -- Assert.Ignore() has no argument"; $hasViolation = $true; continue
        }
        $cs = Get-ConstantString $arg0
        if (-not $cs.OK) { Write-Host "[FAIL] $label -- $($cs.Value)"; $hasViolation = $true; continue }
        Test-IgnoreText -label $label -text $cs.Value -hasViolation ([ref]$hasViolation)
    }
}

Write-Host ''
Write-Host "lint-test-ignores: checked $totalIgnores active ignore(s) across $($files.Count) file(s)."
if ($hasViolation) {
    Write-Host 'lint-test-ignores: FAILED -- fix violations listed above before merging.'
    exit 1
}
Write-Host 'lint-test-ignores: PASSED -- all ignored tests carry a valid issue URL and non-past review-date.'
exit 0