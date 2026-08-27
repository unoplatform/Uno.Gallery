# Performance harness

Production bundle metrics:

```powershell
./build/scripts/measure-wasm-bundle.ps1 `
  -WasmRoot <publish>\wwwroot `
  -TargetName WASM-DOM `
  -Flavor core `
  -OutputPath <artifacts>\wasm-bundle-metrics.json
```

Pinned runtime observations use the browser installed by `build/visual`:

```powershell
Set-Location build/visual
npm ci --no-audit --no-fund
npm run performance -- `
  --wasm <instrumented-publish>\wwwroot `
  --config ..\performance\performance.config.json `
  --output ..\performance\artifacts\runtime-observation.json `
  --commit <git-sha>
```

Use a new publish directory. Reusing a WebAssembly output can retain old
fingerprinted runtime files and invalidates size and startup evidence.
