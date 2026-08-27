# Uno.Extensions app patterns optional flavor

The contained sources live outside the Gallery project directory in `Uno.Gallery.ExtensionsPatterns` and are linked only when `EnableExtensionsPatterns=true`. The default Gallery does not reference the Extensions packages, compile or scan the pages, embed their configuration, or change startup/navigation.

## Package mapping

Uno.Sdk 6.6.42 maps its Extensions feature group to stable version **7.2.3**:

| Pattern | Package |
| --- | --- |
| MVUX / FeedView | `Uno.Extensions.Reactive.WinUI` 7.2.3 |
| Localization | `Uno.Extensions.Localization.WinUI` 7.2.3 |
| Storage | `Uno.Extensions.Storage.WinUI` 7.2.3 |
| Configuration | `Uno.Extensions.Configuration` 7.2.3 |
| Validation | `Uno.Extensions.Validation` 7.2.3 |

Validation is not an Uno.Sdk feature token, so it is an explicit conditional reference at the matching stable version. The optional graph also resolves `Microsoft.Extensions.Logging.Debug` 9.0.14 and `Uno.Core.Extensions.Disposables` 4.1.1 to satisfy the Extensions graph without changing default versions.

## Build and test

```powershell
dotnet build Uno.Gallery\Uno.Gallery.csproj -c Release -p:TargetFrameworkOverride=net10.0-desktop -p:EnableExtensionsPatterns=true
dotnet publish Uno.Gallery\Uno.Gallery.csproj -c Release -f net10.0-browserwasm -p:EnableExtensionsPatterns=true -p:UseNativeRendering=true
dotnet test Uno.Gallery.ExtensionsPatterns.Tests\Uno.Gallery.ExtensionsPatterns.Tests.csproj -c Release
```

Supported optional CI targets are Desktop, DOM WebAssembly AOT, Skia WebAssembly AOT, Android, and Windows x86. The patterns are local-only: deterministic feeds, contained culture resources, application settings, embedded JSON defaults, and DataAnnotations validation. External links are displayed for reference but no sample performs network I/O.

The Localization page complements rather than replaces the core `x:Uid`/RTL sample. The optional pages use the existing Gallery shell and do not introduce Uno.Extensions Navigation.

## Integrated-head verification

Package discovery was performed before source edits. The Uno development feed exposes stable 7.2.3 for all five packages, and targeted restore placed the exact packages in the NuGet cache. NuGet.org TLS negotiation failed on the measurement host, but the configured Uno feed and cache completed the restore.

At integrated HEAD `74a01633`, Release builds passed for Desktop, Android, and Windows x86. The default and optional generated Desktop manifests contain 114 and 119 entries respectively; all five `extensions-*` slugs are absent from default and present only in the optional manifest.

Release Skia WebAssembly Interpreter+AOT measurements used the same source, SDK, and Windows host:

| Measurement | Default | Optional | Delta |
| --- | ---: | ---: | ---: |
| Raw app bundle (excluding `.br`, `.gz`, `.map`) | 69,883,692 B | 71,113,682 B | +1,229,990 B |
| Equivalent Brotli payload (`CompressionLevel.Optimal`) | 20,105,617 B | 20,549,179 B | +443,562 B |
| `dotnet.native.wasm` | 26,524,741 B | 26,902,130 B | +377,389 B |
| App-bundle files | 172 | 202 | +30 |

The default app bundle contains no `Uno.Extensions.*` assets. The optional bundle adds Configuration, Core, Hosting, Localization, Localization.WinUI, Reactive, Reactive.UI, Serialization, Storage, Storage.UI, and Validation WebCIL assets.

On this Windows host, the .NET 10.0.10/SkiaSharp 3.119.2 AOT linker required `WasmAllowUndefinedSymbols=true` for WebGL imports; without it, `wasm-ld` fails on symbols such as `glGetStringi`. Both DOM and Skia AOT links completed with that diagnostic allowance. CI intentionally builds on the existing Linux WebAssembly image without the allowance and runs targeted DOM interactions there.

Cold startup time is not asserted from this single local run: normal production builds compile performance marks out, and the host has no pinned browser harness. The startup payload proxy (`dotnet.native.wasm`) increased by 377,389 B. A certified timing delta requires the repository's documented ten-cold/ten-warm pinned-browser procedure.
