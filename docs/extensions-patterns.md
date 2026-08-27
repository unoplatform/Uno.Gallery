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
dotnet publish Uno.Gallery\Uno.Gallery.csproj -c Release -p:TargetFrameworkOverride=net10.0-browserwasm -p:EnableExtensionsPatterns=true -p:UseNativeRendering=true
dotnet test Uno.Gallery.ExtensionsPatterns.Tests\Uno.Gallery.ExtensionsPatterns.Tests.csproj -c Release
```

Supported optional CI targets are Desktop, DOM WebAssembly full AOT, Skia
WebAssembly full AOT, Android, and Windows x86. The optional flavor deliberately
does not reuse the historical Gallery AOT profiles because they predate
Uno.Extensions and route required startup methods through an unsupported
interpreter path. The patterns are local-only: deterministic feeds, contained
culture resources, application settings, embedded JSON defaults, and
DataAnnotations validation. External links are displayed for reference but no
sample performs network I/O.

The Localization page complements rather than replaces the core `x:Uid`/RTL
sample. Storage resolves the named `ApplicationData` `IKeyValueStorage`, not the unkeyed
in-memory registration. Feed refresh uses the existing
`FeedView.Refresh` command rather than replacing the feed. Page-local Storage
and Validation hosts are recreated after unload so a re-entered page never uses
a disposed service provider. The optional pages use the existing Gallery shell
and do not introduce Uno.Extensions Navigation.

Uno.Extensions.Storage 7.2.3 requires `ISettings` in the platform storage
constructor but predates the package's current self-registration fix. The
contained flavor registers a process-lifetime `PatternSettings` fallback for
unpackaged hosts. The sample explicitly resolves the unencrypted
`ApplicationData` named provider to avoid the 7.2.3 encrypted-store byte-array
round-trip defect on unpackaged Windows. Browser and packaged values still flow
through ApplicationData; unpackaged fallback values survive page re-entry but
not process restart. A source-generated `PatternJsonContext` registers the
stored string type so the same path remains valid when reflection-based JSON
serialization is disabled by AOT.

## Integrated-head verification

Package discovery was performed before source edits. The Uno development feed exposes stable 7.2.3 for all five packages, and targeted restore placed the exact packages in the NuGet cache. NuGet.org TLS negotiation failed on the measurement host, but the configured Uno feed and cache completed the restore.

The default and optional generated Desktop manifests contain 114 and 119
entries respectively; all five `extensions-*` slugs are absent from default and
present only in the optional schema-v2 manifest. Each optional sample uses
contract v1 and an explicit repository-relative source path into
`Uno.Gallery.ExtensionsPatterns`.

The linked-assembly gate verifies that trimmed builds retain all
`RegistrationForm` properties and DataAnnotations attributes plus the generated
string serializer metadata. Release validation has exercised all five patterns
against an unprofiled AOT DOM bundle, including repeated FeedView refresh,
page-local host recreation, persisted storage, culture switching, and
invalid/valid form validation. CI publishes both AOT renderers and runs the same
targeted interactions on its dedicated DOM test artifact.

Exact default-versus-optional bundle and startup deltas are emitted by the
versioned performance-budget workflow. Earlier ad-hoc outputs used a different
AOT profile and artifact-counting method and are intentionally not treated as a
release baseline.
