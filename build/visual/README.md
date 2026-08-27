# Deterministic visual-regression pilot

This tool captures 14 deliberately stable Gallery routes in a Release
Skia-WebAssembly build. It is Windows-only for approval: Puppeteer 25.8.0
installs Chrome for Testing 152.0.7977.42, runs it at `1200x900` and device scale
1, requests `en-US`, UTC, light color scheme, reduced motion, sRGB, and Chromium
SwiftShader. The app uses its bundled OpenSans, Material, and symbol fonts.

The tool serves an existing publish; it never builds another app artifact. Its
server binds loopback on an ephemeral port and the harness stops only the exact
child PID it started. Every route is canonical:
`?design=<Material|Fluent|Agnostic>#<slug>`. Capture waits for the app-owned
`app.visual_ready` performance mark (emitted after four app render frames),
`document.fonts.ready`, and repeated groups of four browser frames until three
pixel-identical captures are observed. After the app mark, the harness removes
only Uno's bootstrap loader overlay and rejects blank/loader frames by pixel
coverage. Every CLI run starts a clean
browser profile and process. Each process performs one unrecorded canonical
overview warm-up so WebAssembly startup caches cannot be mistaken for page
readiness; the 14 recorded routes then navigate in that isolated run's tab.

## Commands

```powershell
cd build\visual
npm ci --no-audit --no-fund
npm test
npm run validate
npm run compare -- --wasm ..\..\artifacts\visual-wasm\wwwroot
```

Comparison writes `artifacts/current/*.png`, `artifacts/diff/*.png`,
`artifacts/report.json`, and `artifacts/report.html`. A missing, extra, malformed,
wrong-sized, or config/browser/lock-stale baseline fails before approval.

Pixelmatch uses a per-channel threshold of `0.05`, excludes its anti-alias
classification, and permits at most `0.01%` different unmasked pixels (90 pixels
at this viewport). The pilot currently has **no masks**. A future mask must be an
explicit rectangle in `visual.config.json`, is hashed into baseline metadata,
and requires the same review as a baseline image.

## Local baseline update

Publish from the repository root, then opt in twice:

```powershell
dotnet restore Uno.Gallery\Uno.Gallery.csproj `
  -p:TargetFrameworkOverride=net10.0-browserwasm -r browser-wasm
dotnet publish Uno.Gallery\Uno.Gallery.csproj `
  -f net10.0-browserwasm -p:TargetFrameworkOverride=net10.0-browserwasm `
  -r browser-wasm -p:WasmGenerateAppBundle=true `
  -c Release -p:UseNativeRendering=false `
  -p:EnableVisualRegression=true `
  -o artifacts\visual-wasm

$env:VISUAL_ACCEPT_BASELINES = '1'
npm run update -- --wasm ..\..\artifacts\visual-wasm\wwwroot
Remove-Item Env:VISUAL_ACCEPT_BASELINES
```

The update command refuses to run when `CI`, `TF_BUILD`, or `BUILD_BUILDID` is
present. Review every PNG and `baselines/metadata.json`; never regenerate from a
developer server, non-Windows host, system browser, or normal app build.
