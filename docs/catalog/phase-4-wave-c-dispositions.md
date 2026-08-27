# Phase 4 Wave C rendering and platform dispositions

## Implemented

| Capability | Gallery slug | Truthful boundary |
| --- | --- | --- |
| Build and renderer diagnostics | `diagnostics` | Reports compile-time target, backend, execution mode, and availability; no probing or network |
| `SKCanvasElement` | `skia-canvas` | Skia Desktop/Skia WebAssembly only; tests assert page-owned `RenderOverride` completion state, not pixels |
| Composition | `composition-visuals` | Creates a `SpriteVisual` and deterministically changes offset, opacity, and scale |
| System backdrop/AppWindow | `windows-backdrops` | Windows catalog only; changes the existing window and clears the backdrop on unload |
| XAML drag/drop | `drag-drop` | Real app-owned text transfer plus deterministic page-owned action for WebDriver |

Picker initialization now calls WinRT `InitializeWithWindow` only in `WINDOWS`
builds. Geolocation explicitly reports denied, unspecified, empty, and exception
states. WebView uses packaged inline HTML and reports navigation success/failure;
Lottie reports packaged-source initialization; media reports API/codec boundaries.

## Explicitly not added

Evidence is pinned to `unoplatform/uno@7b58a4712ca0fb64395d455dd19c634d2d7126d5`
in `build/manifest-fixtures/upstream-features-v1.json`.

| Capability | Source evidence | Disposition |
| --- | --- | --- |
| `GLCanvasElement` | `src/AddIns/Uno.WinUI.Graphics3DGL/GLCanvasElement.cs`; upstream samples are manual/hardware-dependent | Docs-only deferral: package is not resolved and would add unmeasured bundle cost |
| Camera | `src/SamplesApp/SamplesApp.Samples/Windows_Media/CameraCaptureUISample.xaml.cs` | External companion: physical device and permission required |
| Maps | `.../MapPresenter/MapControl.xaml.cs`; Android manifest requires an API key | External companion: credentialed and network-backed |
| Printing | `src/Uno.UWP/Generated/.../PrintManager.cs` is not implemented on Uno non-Windows targets | Docs-only: Windows OS print UI is not deterministic |
| Vulkan | `src/Uno.UI/Vulkan/IVulkanDevice.skia.cs` | Not applicable: internal experimental host backend, surfaced as identity only |
| Native-view embedding | `.../UIElementTests/UIElement_Native_Child.xaml` | External companion: target-specific native types cannot live in one shared sample |

No new package reference was added.

## Catalog and bundle measurements

Target catalogs exported after Wave C contain **117 Desktop**, **116 DOM
WebAssembly**, and **117 Skia WebAssembly** samples. The renderer-aware
conditional excludes `skia-canvas` from the DOM catalog while retaining it in
both supported Skia catalogs.

The Release Skia WebAssembly `InterpreterAndAOT` publish used the production AOT
profile, `NuGetAudit=false`, and no network-backed sample. The .NET 10.0.10/Uno
Bootstrap combination required `WasmShellEnableDotnetJsFingerprinting=false`
after its default publish failed `UNOWASM001`; this changes asset names, not
sample code. `publish/wwwroot` measured:

| Measurement | Waves A–B recorded baseline | Wave C | Delta |
| --- | ---: | ---: | ---: |
| Raw payload (excluding `.br`, `.gz`, `.map`) | 100,691,982 B | 32,635,866 B | -68,056,116 B |
| Published Brotli payload | 20,631,876 B | 5,238,171 B | -15,393,705 B |
| Published files | 747 | 446 | -301 |

The large negative delta is primarily the corrected production AOT exclusion of
build-time `Uno.AI.XamlGeneration`/Semantic Kernel assemblies and incompatible
transitive contract-version assemblies, not the three new sample pages. The
normal and `EnableVisualRegression=true` app-bundle paths remain separate.
