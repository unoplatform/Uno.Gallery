# Phase 4 Wave C rendering and platform dispositions

## Implemented

| Capability | Gallery slug | Truthful boundary |
| --- | --- | --- |
| Build and renderer diagnostics | `diagnostics` | Production-visible Experimental page reports compile-time target, backend, execution mode, and availability; no probing or network |
| `SKCanvasElement` | `skia-canvas` | Skia Desktop/Skia WebAssembly only; page-owned `RenderOverride` completion state is exposed for a supported-host test, while DOM automation cannot execute it |
| Composition | `composition-visuals` | Native/Skia only; creates a `SpriteVisual` and deterministically changes offset, opacity, and scale, while DOM is excluded because child-visual attachment is a no-op |
| System backdrop/AppWindow | `windows-backdrops` | Windows catalog only; changes the existing window and clears the backdrop on unload |
| XAML drag/drop | `drag-drop` | Real text transfer plus deterministic in-app action for WebDriver |

Picker initialization now calls WinRT `InitializeWithWindow` only in `WINDOWS`
builds. Geolocation explicitly reports denied, unspecified, empty, and exception
states. WebView uses self-contained inline HTML and reports navigation success/failure;
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

## Catalog and performance accounting

Target catalogs exported after Wave C contain **117 Desktop**, **115 DOM
WebAssembly**, and **117 Skia WebAssembly** samples. The renderer-aware
conditional excludes `skia-canvas` and `composition-visuals` from the DOM
catalog while retaining them in supported Skia catalogs. Their contract reports contain 29 Desktop, 27 DOM,
and 29 Skia contract-v1 samples.

The DOM target loads all 114 target-compatible Stable samples and runs focused
diagnostics, drag/drop, geolocation, Lottie, and WebView interactions. The
production WebAssembly outputs use unprofiled AOT because historical profiles
route evolving module and generated Uno.Themes initializers through unsupported
Mono interpreter paths. The resulting Skia output reaches a non-empty renderer canvas;
`SKCanvasElement` interaction remains explicit until a Skia semantic
UI-automation host is available.

Wave C adds no package reference. Exact bundle and startup deltas belong to the
versioned performance-budget pipeline: prior ad-hoc outputs used different
fingerprinting and artifact-counting rules and are not a valid release
comparison.
