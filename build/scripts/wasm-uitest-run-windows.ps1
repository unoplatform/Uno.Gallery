#Requires -Version 7.0
<#
.SYNOPSIS
    Windows WASM UI-test runner for Uno.Gallery.

.DESCRIPTION
    Mirrors the intent of build/scripts/wasm-uitest-run.sh for Windows / PowerShell 7+.

    The script:
      - Resolves or downloads a matched Chrome for Testing + ChromeDriver win64 pair,
        caching the binaries under $env:LOCALAPPDATA\uno-uitest-chrome\ outside the
        repository so they are never git-tracked.
      - Sets the Uno UITest environment variables consumed by Uno.UITest.Selenium:
            UNO_UITEST_TARGETURI, UNO_UITEST_DRIVERPATH_CHROME,
            UNO_UITEST_CHROME_BINARY_PATH, UNO_UITEST_SCREENSHOT_PATH,
            UNO_UITEST_PLATFORM, UNO_UITEST_PROJECT, UNO_UITEST_LOGFILE,
            UNO_UITEST_WASM_PROJECT, UITEST_TEST_TIMEOUT.
      - Providing UNO_UITEST_DRIVERPATH_CHROME explicitly bypasses the internal
        wmic.exe-based Chrome-version detection in SeleniumDriverManager
        (SeleniumDriverManager.GetVersion()), which is required on Windows 11+
        where wmic.exe is absent by default.
      - Starts dotnet-serve with a tracked process handle, polls the target URL
        until the server is ready, runs dotnet test, and stops only that server
        process (by PID) in the finally block.

    Pre-set UNO_UITEST_DRIVERPATH_CHROME and UNO_UITEST_CHROME_BINARY_PATH to skip
    the download step entirely (useful in CI where those binaries are pre-installed).

    This script has no effect on the Linux CI pipeline.

.PARAMETER WasmOutputPath
    Path to the published WASM wwwroot directory (must contain index.html).
    Mirrors the $UNO_UITEST_WASM_OUTPUT_PATH variable in the Linux script.

    DOM WASM is required for UITests (Skia WASM does not yet expose @xamlautomationid
    HTML attributes needed by the test framework; see Uno issue trackers for status).

    Recommended Windows build command (Debug = fast startup + UIAutomation always on):
        dotnet publish Uno.Gallery\Uno.Gallery.csproj ``
            -p:TargetFrameworkOverride=net10.0-browserwasm ``
            -p:_DefaultMicrosoftNETSdk=Microsoft.NET.Sdk.WebAssembly ``
            -c Debug ``
            -p:UseNativeRendering=true ``
            --ignore-failed-sources ``
            -p:NoWarn=NU1301 ``
            -o <out-dir>
        # Pass '<out-dir>\wwwroot' as -WasmOutputPath.

    For a Release build (requires iOS workload installed or elevated access):
        dotnet publish Uno.Gallery\Uno.Gallery.csproj ``
            -f net10.0-browserwasm -c Release ``
            -p:UseNativeRendering=true ``
            -p:IsUiAutomationMappingEnabled=True ``
            -o <out-dir>
        # Pass '<out-dir>\wwwroot' as -WasmOutputPath.

    Notes:
    - Debug builds run in interpreter mode and start significantly faster in the
      browser than AOT Release builds. The 60-second UITest element-wait timeout
      is comfortable with Debug builds on typical development hardware.
    - UseNativeRendering=true (DOM WASM) is required; Skia WASM UITests are
      blocked until Uno adds @xamlautomationid support to the Skia accessibility
      overlay (tracked in the Uno Platform repository).
    - TargetFrameworkOverride + _DefaultMicrosoftNETSdk=Microsoft.NET.Sdk.WebAssembly
      is the Windows workaround for multi-TFM projects where dotnet restore fails
      on non-installed workloads (e.g. ios on a Windows-only dev machine).

.PARAMETER Port
    TCP port on which dotnet-serve listens. The UITest framework connects to
    http://localhost:<Port>. Defaults to 5000, matching the Linux CI default and
    the UNO_UITEST_TARGETURI set by wasm-uitest-run.sh.

.PARAMETER Configuration
    Build configuration passed to dotnet test. Defaults to 'Release'.

.PARAMETER TestFilter
    Optional VSTest / NUnit filter expression passed verbatim to dotnet test --filter.
    Leave empty to run the full UITest suite.

.PARAMETER TestTier
    Named test tier: Smoke, Interaction, or All. Smoke runs the generated-catalog
    batch plus its contract tests. Interaction runs the existing curated suite.
    Defaults to All. When TestFilter is also supplied, both filters must match.

.PARAMETER ArtifactPath
    Root directory for screenshots, NUnit XML results, and the nunit-log.txt.
    Defaults to <repo-root>\build\artifacts\wasm-uitests.

.EXAMPLE
    # Build Debug DOM WASM (recommended for Windows dev - fast startup, UIAutomation on):
    dotnet publish Uno.Gallery\Uno.Gallery.csproj `
        -p:TargetFrameworkOverride=net10.0-browserwasm `
        -p:_DefaultMicrosoftNETSdk=Microsoft.NET.Sdk.WebAssembly `
        -c Debug -p:UseNativeRendering=true `
        --ignore-failed-sources -p:NoWarn=NU1301 -o C:\tmp\gallery-uitest

    # Run target-aware catalog smoke against the built output:
    pwsh -File build\scripts\wasm-uitest-run-windows.ps1 `
        -WasmOutputPath "C:\tmp\gallery-uitest\wwwroot" `
        -TestTier Smoke

.EXAMPLE
    # Filtered Given_MainPage smoke test with a custom artifact directory:
    pwsh -File build\scripts\wasm-uitest-run-windows.ps1 `
        -WasmOutputPath "C:\tmp\gallery-uitest\wwwroot" `
        -TestFilter "FullyQualifiedName~Given_MainPage" `
        -ArtifactPath "C:\temp\gallery-uitest-artifacts"

.NOTES
    Chrome for Testing binaries are cached under:
        $env:LOCALAPPDATA\uno-uitest-chrome\<version>\chrome-win64\chrome.exe
        $env:LOCALAPPDATA\uno-uitest-chrome\<version>\chromedriver-win64\chromedriver.exe

    Downloads contact the official Chrome-for-Testing HTTPS endpoints:
        https://googlechromelabs.github.io/chrome-for-testing/  (version metadata JSON)
        https://storage.googleapis.com/chrome-for-testing-public  (binary ZIPs)
    No offline checksum verification is available from the official metadata API.

    EXCLUSIVE EXECUTION — the script acquires a per-user Windows named mutex
        Global\UnoGallery-WasmUITest-ChromeCache-<SID>
    (where <SID> is the current user's Windows security identifier) for its
    entire run before touching the shared Chrome cache.  A concurrent
    invocation by the same user fails immediately with an actionable message
    rather than corrupting the cache or producing spurious 'session not created'
    Selenium failures.  Different OS users each hold their own lock and never
    contend with each other.  If a previous run crashed, PowerShell abandons
    the mutex automatically on process exit; the next invocation recovers it
    and logs a warning.

    TARGETED PROCESS CLEANUP — stale chrome.exe / chromedriver.exe processes
    left by a prior crash are stopped by exact PID before tests start and
    again after tests finish.  Identification is path-based: only processes
    whose full executable path begins with the script-managed cache root
    ($env:LOCALAPPDATA\uno-uitest-chrome\) are stopped.  System or user Chrome
    instances at any other path are never touched.  Stop-Process -Name and
    taskkill by image name are not used.

    IMPORTANT — execute this script with pwsh -File; do NOT dot-source it.
    The script sets environment variables that are intentionally process-local
    so they do not leak back into the calling shell:
        . .\wasm-uitest-run-windows.ps1          # WRONG: variables pollute caller env
        pwsh -File .\wasm-uitest-run-windows.ps1 # CORRECT

    To skip the download step, pre-set (both must be provided):
        $env:UNO_UITEST_DRIVERPATH_CHROME    = "<directory containing chromedriver.exe>"
        $env:UNO_UITEST_CHROME_BINARY_PATH   = "<path to chrome.exe>"
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [string] $WasmOutputPath,

    [ValidateRange(1, 65535)]
    [int]    $Port = 5000,

    [string] $Configuration = 'Release',

    [string] $TestFilter,

    [ValidateSet('Smoke', 'Interaction', 'All')]
    [string] $TestTier = 'All',

    [string] $ArtifactPath
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# ============================================================
# 1. Resolve repository root from $PSScriptRoot\..\..\
# ============================================================
$RepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..' '..')).Path

if (-not (Test-Path (Join-Path $RepoRoot 'Uno.Gallery.UITests'))) {
    throw "Cannot locate repository root. " +
          "Expected 'Uno.Gallery.UITests' under '$RepoRoot'. " +
          "Ensure the script lives at build\scripts\ inside the repository."
}

# ============================================================
# 2. Derived paths and defaults
# ============================================================
if (-not $ArtifactPath) {
    $ArtifactPath = Join-Path $RepoRoot 'build' 'artifacts' 'wasm-uitests'
}

$ScreenshotPath = Join-Path $ArtifactPath 'screenshots' 'wasm'
$NUnitResultXml = Join-Path $RepoRoot     'build' 'TestResult.xml'
$UITestProject  = Join-Path $RepoRoot     'Uno.Gallery.UITests'
$WasmProject    = Join-Path $RepoRoot     'Uno.Gallery' 'Uno.Gallery.csproj'

# Normalise WASM output path to an absolute path
if (-not [System.IO.Path]::IsPathRooted($WasmOutputPath)) {
    $WasmOutputPath = Join-Path (Get-Location).Path $WasmOutputPath
}
$WasmOutputPath = [System.IO.Path]::GetFullPath($WasmOutputPath)

if (-not (Test-Path (Join-Path $WasmOutputPath 'index.html'))) {
    Write-Error (@"
WASM output not found or incomplete at: '$WasmOutputPath'
The directory must contain index.html (i.e. the wwwroot from a WASM publish).

Build it first with:
    dotnet publish '$WasmProject' ``
        -f net10.0-browserwasm -c Release ``
        -p:IsUiAutomationMappingEnabled=True ``
        -o <publish-dir>

Then pass '<publish-dir>\wwwroot' as the -WasmOutputPath parameter.
"@)
    exit 1
}

$null = New-Item -ItemType Directory -Path $ScreenshotPath -Force

Write-Host '=== Uno Gallery WASM UI Tests (Windows) ===' -ForegroundColor Cyan
Write-Host "  Repo root     : $RepoRoot"
Write-Host "  WASM output   : $WasmOutputPath"
Write-Host "  Port          : $Port"
Write-Host "  Configuration : $Configuration"
Write-Host "  Test tier     : $TestTier"
Write-Host "  Artifacts     : $ArtifactPath"
if ($TestFilter) { Write-Host "  Filter        : $TestFilter" }

# ============================================================
# 2b. Browser-cache root (resolved unconditionally so the lock,
#     the pre/post-run cleanup, and the download step all use
#     the same path regardless of whether a download is needed).
# ============================================================
if (-not $env:LOCALAPPDATA) {
    Write-Error ('$env:LOCALAPPDATA is not set or is empty. ' +
                 'This variable is required to locate the Chrome-for-Testing cache directory. ' +
                 'Ensure the script runs in a user session with a valid LOCALAPPDATA path.') -ErrorAction Continue
    exit 1
}
$CacheRoot = Join-Path $env:LOCALAPPDATA 'uno-uitest-chrome'

# ============================================================
# Helper — stop chrome.exe / chromedriver.exe whose full
# executable path begins under the script-managed cache root.
# Process identification is strictly path-based; processes at
# other locations (system or user browser installs) are never
# stopped.  Stop-Process -Name and taskkill by image name are
# intentionally avoided.
# ============================================================
function Stop-CachedBrowserProcess {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory)]
        [string] $CacheRoot,
        [string] $Label = 'stale'
    )

    # Canonical prefix with a trailing separator for a reliable StartsWith check.
    $resolvedRoot = [System.IO.Path]::GetFullPath($CacheRoot)
    if (-not $resolvedRoot.EndsWith('\')) { $resolvedRoot += '\' }

    $stopped = 0
    foreach ($proc in (Get-Process -ErrorAction SilentlyContinue)) {
        # Pre-filter by executable name to skip the expensive path reads for
        # the vast majority of running processes.
        if ($proc.Name -notin @('chrome', 'chromedriver')) { continue }

        $exePath = $null
        try {
            # Works for same-bitness, same-user processes we have read access to.
            $exePath = $proc.MainModule.FileName
        }
        catch {
            # Access-denied is common for sandboxed/child processes; fall back to CIM.
            try {
                $cim = Get-CimInstance -ClassName Win32_Process `
                    -Filter "ProcessId = $($proc.Id)" `
                    -Property ExecutablePath `
                    -ErrorAction Stop
                $exePath = $cim.ExecutablePath
            }
            catch {
                # Cannot determine path (process may have already exited); skip.
            }
        }

        if (-not $exePath) { continue }

        # Normalise the path before comparison: collapses relative segments
        # and standardises separators so path representations compare correctly.
        try { $exePath = [System.IO.Path]::GetFullPath($exePath) }
        catch { continue }   # Malformed path; cannot compare safely.

        if ($exePath.StartsWith($resolvedRoot, [System.StringComparison]::OrdinalIgnoreCase)) {
            Write-Host "  Stopping $Label PID $($proc.Id): $exePath" -ForegroundColor Yellow
            Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
            $stopped++
        }
    }

    if ($stopped -gt 0) {
        Write-Host "  Stopped $stopped $Label cached-browser process(es)." -ForegroundColor Yellow
    }
    else {
        Write-Host "  No $Label cached-browser processes found under cache root."
    }
}

# ============================================================
# 2c. Acquire an exclusive named mutex for the full script run.
#     Two concurrent invocations by the same OS user share the
#     same cache root and would race to clean/launch the cached
#     browser; this lock prevents that.  The mutex name is
#     suffixed with the current user's Windows SID so the lock
#     protects the same per-user cache across sessions without
#     blocking other users.  The try/finally below wraps
#     sections 3-6 and releases the mutex in all exit paths.
# ============================================================
$currentUserSid = $null
try {
    $currentUserSid = ([System.Security.Principal.WindowsIdentity]::GetCurrent()).User.Value
}
catch {
    Write-Error ("Cannot resolve current user SID: $_`n" +
                 "A stable SID is required to construct a per-user mutex name. " +
                 "Ensure the script runs in a valid Windows user context.") -ErrorAction Continue
    exit 1
}
$cacheLockName  = "Global\UnoGallery-WasmUITest-ChromeCache-$currentUserSid"
$cacheLock      = [System.Threading.Mutex]::new($false, $cacheLockName)
$cacheLockOwned = $false

try {
    try   { $cacheLockOwned = $cacheLock.WaitOne(0) }
    catch [System.Threading.AbandonedMutexException] {
        # The previous holder crashed; .NET automatically grants us ownership.
        $cacheLockOwned = $true
        Write-Host ('  WARNING: recovered abandoned lock — previous run may have crashed ' +
                    'without releasing the mutex.') -ForegroundColor Yellow
    }

    if (-not $cacheLockOwned) {
        $lockMsg  = "EXCLUSIVE LOCK UNAVAILABLE: another instance of"
        $lockMsg += " wasm-uitest-run-windows.ps1 is already running`n"
        $lockMsg += "and holds the Chrome-cache lock.`n`n"
        $lockMsg += "  Mutex : $cacheLockName`n"
        $lockMsg += "  Cache : $CacheRoot`n`n"
        $lockMsg += "Wait for the other run to finish.  If it has crashed, the mutex is`n"
        $lockMsg += "released automatically when its pwsh process exits — wait for that`n"
        $lockMsg += "process to exit (or terminate it), then re-run this script."
        Write-Error $lockMsg -ErrorAction Continue
        exit 1
    }
    Write-Host "  Exclusive lock acquired ($cacheLockName)." -ForegroundColor Green

# ============================================================
# 3. Resolve Chrome for Testing binaries
#    Honour pre-set env vars; download only the missing piece(s).
# ============================================================
$ChromeDriverDir  = $env:UNO_UITEST_DRIVERPATH_CHROME
$ChromeBinaryPath = $env:UNO_UITEST_CHROME_BINARY_PATH

if (-not $ChromeDriverDir -or -not $ChromeBinaryPath) {
    Write-Host "`n--- Chrome for Testing: resolving from official JSON ---" -ForegroundColor Yellow

    $CftJsonUri = 'https://googlechromelabs.github.io/chrome-for-testing/last-known-good-versions-with-downloads.json'

    Write-Host "  Querying: $CftJsonUri"
    $cft     = Invoke-RestMethod -Uri $CftJsonUri -UseBasicParsing
    $stable  = $cft.channels.Stable
    $version = $stable.version
    Write-Host "  Stable version: $version"

    $cacheDir = Join-Path $CacheRoot $version
    $null = New-Item -ItemType Directory -Path $cacheDir -Force

    # A unique ID per script invocation prevents concurrent download/extraction races.
    $invocationId = [System.Guid]::NewGuid().ToString('N')

    # -- ChromeDriver win64 --
    if (-not $ChromeDriverDir) {
        $cdDir = Join-Path $cacheDir 'chromedriver-win64'
        $cdExe = Join-Path $cdDir    'chromedriver.exe'

        if (-not (Test-Path $cdExe)) {
            $cdEntry = $stable.downloads.chromedriver | Where-Object { $_.platform -eq 'win64' }
            if (-not $cdEntry) {
                throw "win64 chromedriver URL not found in Chrome for Testing JSON for version $version."
            }
            Write-Host "  Downloading ChromeDriver: $($cdEntry.url)"
            # Per-invocation temp paths prevent concurrent download/extraction races.
            $cdZipTemp     = Join-Path $cacheDir "chromedriver-$invocationId.zip"
            $cdExtractTemp = Join-Path $cacheDir "chromedriver-tmp-$invocationId"
            try {
                Invoke-WebRequest -Uri $cdEntry.url -OutFile $cdZipTemp -UseBasicParsing
                Expand-Archive -Path $cdZipTemp -DestinationPath $cdExtractTemp
                # Publish the completed versioned folder to the shared cache.
                # If another invocation already won the race, discard only our temp copy.
                if (-not (Test-Path $cdExe)) {
                    try {
                        Move-Item -Path (Join-Path $cdExtractTemp 'chromedriver-win64') `
                                  -Destination $cdDir -ErrorAction Stop
                    }
                    catch {
                        Write-Host "  ChromeDriver cache already published by a concurrent invocation."
                    }
                }
            }
            finally {
                # Remove only this invocation's targeted temp paths.
                if (Test-Path $cdZipTemp)     { Remove-Item -Path $cdZipTemp     -Force -ErrorAction SilentlyContinue }
                if (Test-Path $cdExtractTemp) { Remove-Item -Path $cdExtractTemp -Recurse -Force -ErrorAction SilentlyContinue }
            }
        }
        else {
            Write-Host "  ChromeDriver already cached: $cdDir"
        }

        if (-not (Test-Path $cdExe)) {
            throw "chromedriver.exe not found after extraction; expected at '$cdExe'."
        }
        $ChromeDriverDir = $cdDir
    }

    # -- Chrome browser win64 --
    if (-not $ChromeBinaryPath) {
        $chromeBrowserDir = Join-Path $cacheDir 'chrome-win64'
        $chromeExe        = Join-Path $chromeBrowserDir 'chrome.exe'

        if (-not (Test-Path $chromeExe)) {
            $chromeEntry = $stable.downloads.chrome | Where-Object { $_.platform -eq 'win64' }
            if (-not $chromeEntry) {
                throw "win64 chrome URL not found in Chrome for Testing JSON for version $version."
            }
            Write-Host "  Downloading Chrome: $($chromeEntry.url)"
            # Per-invocation temp paths prevent concurrent download/extraction races.
            $chromeZipTemp     = Join-Path $cacheDir "chrome-$invocationId.zip"
            $chromeExtractTemp = Join-Path $cacheDir "chrome-tmp-$invocationId"
            try {
                Invoke-WebRequest -Uri $chromeEntry.url -OutFile $chromeZipTemp -UseBasicParsing
                Expand-Archive -Path $chromeZipTemp -DestinationPath $chromeExtractTemp
                # Publish the completed versioned folder to the shared cache.
                # If another invocation already won the race, discard only our temp copy.
                if (-not (Test-Path $chromeExe)) {
                    try {
                        Move-Item -Path (Join-Path $chromeExtractTemp 'chrome-win64') `
                                  -Destination $chromeBrowserDir -ErrorAction Stop
                    }
                    catch {
                        Write-Host "  Chrome cache already published by a concurrent invocation."
                    }
                }
            }
            finally {
                # Remove only this invocation's targeted temp paths.
                if (Test-Path $chromeZipTemp)     { Remove-Item -Path $chromeZipTemp     -Force -ErrorAction SilentlyContinue }
                if (Test-Path $chromeExtractTemp) { Remove-Item -Path $chromeExtractTemp -Recurse -Force -ErrorAction SilentlyContinue }
            }
        }
        else {
            Write-Host "  Chrome already cached: $chromeBrowserDir"
        }

        if (-not (Test-Path $chromeExe)) {
            throw "chrome.exe not found after extraction; expected at '$chromeExe'."
        }
        $ChromeBinaryPath = $chromeExe
    }
}
else {
    Write-Host "`n--- Chrome: using pre-set environment variables ---"
}

Write-Host "  ChromeDriver dir : $ChromeDriverDir"
Write-Host "  Chrome binary    : $ChromeBinaryPath"

# ============================================================
# 3b. Pre-run: stop stale cached-browser processes left by a
#     prior crash, before the tests spin up new ones.
# ============================================================
Write-Host "`n--- Pre-run: stopping stale cached-browser processes ---" -ForegroundColor Yellow
Stop-CachedBrowserProcess -CacheRoot $CacheRoot -Label 'stale (pre-run)'

# ============================================================
# 4. Export Uno UITest environment variables
#
#    UNO_UITEST_DRIVERPATH_CHROME  — the directory containing chromedriver.exe.
#      Setting this causes SeleniumAppConfigurator.GetChromeDriver() to call
#      SeleniumDriverManager.Chrome.FromDriverPath(), which passes the directory
#      directly to new ChromeDriver(directory, options).  This code path does NOT
#      call SeleniumDriverManager.GetVersion() (which shells out to wmic.exe),
#      therefore wmic.exe is never required and the tests run on Windows 11+.
#
#    UNO_UITEST_CHROME_BINARY_PATH — path to chrome.exe.
#      Prevents Selenium from attempting to locate the system Chrome installation
#      via the registry or well-known paths; the downloaded binary is used instead.
# ============================================================
$env:UNO_UITEST_TARGETURI          = "http://localhost:$Port"
$env:UNO_UITEST_DRIVERPATH_CHROME  = $ChromeDriverDir
$env:UNO_UITEST_CHROME_BINARY_PATH = $ChromeBinaryPath
$env:UNO_UITEST_SCREENSHOT_PATH    = $ScreenshotPath
$env:UNO_UITEST_PLATFORM           = 'Browser'
$env:UNO_UITEST_PROJECT            = $UITestProject
$env:UNO_UITEST_LOGFILE            = Join-Path $ScreenshotPath 'nunit-log.txt'
$env:UNO_UITEST_WASM_PROJECT       = $WasmProject
$env:UITEST_TEST_TIMEOUT           = '60m'

Write-Host "`n--- Uno UITest environment ---"
Write-Host "  UNO_UITEST_TARGETURI          : $env:UNO_UITEST_TARGETURI"
Write-Host "  UNO_UITEST_DRIVERPATH_CHROME  : $env:UNO_UITEST_DRIVERPATH_CHROME"
Write-Host "  UNO_UITEST_CHROME_BINARY_PATH : $env:UNO_UITEST_CHROME_BINARY_PATH"
Write-Host "  UNO_UITEST_SCREENSHOT_PATH    : $env:UNO_UITEST_SCREENSHOT_PATH"
Write-Host "  UNO_UITEST_PLATFORM           : $env:UNO_UITEST_PLATFORM"
Write-Host "  UNO_UITEST_PROJECT            : $env:UNO_UITEST_PROJECT"
Write-Host "  UNO_UITEST_LOGFILE            : $env:UNO_UITEST_LOGFILE"

# ============================================================
# 5. Locate dotnet-serve
# ============================================================
$dotnetServePath = $null

$dotnetServeCmd = Get-Command 'dotnet-serve' -ErrorAction SilentlyContinue
if ($dotnetServeCmd) {
    $dotnetServePath = $dotnetServeCmd.Source
}
else {
    # Check the standard .NET global tools install location
    $candidate = Join-Path $env:USERPROFILE '.dotnet' 'tools' 'dotnet-serve.exe'
    if (Test-Path $candidate) {
        $dotnetServePath = $candidate
    }
}

if (-not $dotnetServePath) {
    throw @"
dotnet-serve is not installed or not on PATH.

Install it with:
    dotnet tool install -g dotnet-serve

If you installed it to a non-default location, add that location to PATH before
running this script.
"@
}

Write-Host "`n--- dotnet-serve: $dotnetServePath"

# ============================================================
# 6. Start server, poll for readiness, run tests, stop server
# ============================================================
$serverProcess = $null
$testExitCode  = 0

try {
    # Start dotnet-serve with an explicit process handle so we can stop it by PID.
    Write-Host "`n--- Starting WASM server on port $Port ---" -ForegroundColor Yellow
    $serverProcess = Start-Process `
        -FilePath     $dotnetServePath `
        -ArgumentList "-p $Port -d `"$WasmOutputPath`"" `
        -NoNewWindow `
        -PassThru

    Write-Host "  dotnet-serve PID: $($serverProcess.Id)"

    # Poll the server until it responds or the timeout expires.
    $targetUri   = "http://localhost:$Port"
    $maxWaitSec  = 60
    $stopwatch   = [System.Diagnostics.Stopwatch]::StartNew()
    $serverReady = $false

    Write-Host "  Waiting for server at $targetUri ..."
    while ($stopwatch.Elapsed.TotalSeconds -lt $maxWaitSec) {
        try {
            $null = Invoke-WebRequest -Uri $targetUri -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
            $serverReady = $true
            break
        }
        catch {
            Start-Sleep -Milliseconds 500
        }
    }

    if (-not $serverReady) {
        throw "dotnet-serve did not respond at '$targetUri' within $maxWaitSec seconds."
    }
    Write-Host ("  Server ready in {0:F1}s." -f $stopwatch.Elapsed.TotalSeconds) -ForegroundColor Green

    # Build the dotnet test argument array.
    $testArgs = @(
        'test',
        $UITestProject,
        '-c', $Configuration,
        '--logger', 'console;verbosity=normal',
        '--logger', "nunit;LogFileName=$NUnitResultXml",
        '--blame-hang-timeout', '60m',
        '-v', 'm'
    )
    $categoryFilter = if ($TestTier -eq 'All') { $null } else { "TestCategory=$TestTier" }
    if ($TestFilter -and $categoryFilter) {
        $testArgs += '--filter', "($TestFilter)&$categoryFilter"
    }
    elseif ($TestFilter) {
        $testArgs += '--filter', $TestFilter
    }
    elseif ($categoryFilter) {
        $testArgs += '--filter', $categoryFilter
    }

    Write-Host "`n--- Running dotnet test ---" -ForegroundColor Yellow
    Write-Host "  dotnet $($testArgs -join ' ')"
    Write-Host ''

    # Run tests.  Use a local $ErrorActionPreference so a non-zero exit code does
    # not throw before the finally block can clean up the server process.
    $local:ErrorActionPreference = 'Continue'
    & dotnet @testArgs
    $testExitCode = $LASTEXITCODE
    $local:ErrorActionPreference = 'Stop'
}
finally {
    # Stop only the dotnet-serve process we started, identified by its PID.
    if ($null -ne $serverProcess -and -not $serverProcess.HasExited) {
        Write-Host "`n--- Stopping dotnet-serve (PID $($serverProcess.Id)) ---" -ForegroundColor Yellow
        Stop-Process -Id $serverProcess.Id -Force -ErrorAction SilentlyContinue
        Write-Host "  Server stopped."
    }

    # Post-run: stop cached-browser processes leaked by this test run.
    # Wrapped in try/catch so any cleanup exception is surfaced as a warning
    # without masking the original test failure or its exit code.
    Write-Host "`n--- Post-run: stopping leaked cached-browser processes ---" -ForegroundColor Yellow
    try {
        Stop-CachedBrowserProcess -CacheRoot $CacheRoot -Label 'leaked (post-run)'
    }
    catch {
        Write-Host "  WARNING: post-run cleanup threw an exception: $_" -ForegroundColor Yellow
    }
}

}   # end of outer mutex try (section 2c)
finally {
    # Release the exclusive Chrome-cache lock in all exit paths.
    # ReleaseMutex and Dispose are each individually wrapped so neither
    # can mask the other or any in-flight exception from the protected body.
    if ($cacheLockOwned) {
        try   { $cacheLock.ReleaseMutex() }
        catch { Write-Host "  WARNING: mutex release failed: $_" -ForegroundColor Yellow }
        Write-Host "`n--- Exclusive lock released. ---" -ForegroundColor DarkGray
    }
    try   { $cacheLock.Dispose() }
    catch { Write-Host "  WARNING: mutex dispose failed: $_" -ForegroundColor Yellow }
}

# ============================================================
# 7. Summary and exit
# ============================================================
Write-Host ''
Write-Host '=== Results ===' -ForegroundColor Cyan
Write-Host "  NUnit XML   : $NUnitResultXml"
Write-Host "  Screenshots : $ScreenshotPath"
Write-Host "  Log file    : $env:UNO_UITEST_LOGFILE"
Write-Host "  Exit code   : $testExitCode"

if ($testExitCode -ne 0) {
    Write-Error ("dotnet test exited with code $testExitCode. " +
                 "Inspect '$NUnitResultXml' and '$ScreenshotPath' for details.") -ErrorAction Continue
    exit $testExitCode
}

Write-Host 'Tests completed successfully.' -ForegroundColor Green
