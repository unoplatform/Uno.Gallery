# Phase 4 Waves A–B coverage dispositions

This record supplements the pinned, deliberately incomplete upstream fixture. It
does not claim that the fixture is an authoritative inventory of Uno or Toolkit.

## Implemented coverage

| API | Gallery sample | Demonstrated behavior |
| --- | --- | --- |
| `AnimatedIcon` | `animated-icon` | Built-in `AnimatedAcceptVisualSource`, `NormalOff`/`NormalOn` marker transitions, and `FallbackIconSource` |
| `CardContentControl` | `card-content-control` | `Elevation`, `ShadowColor`, `IsClickable`, activation, and deterministic configuration changes |
| `ItemsRepeaterExtensions` | `itemsrepeater` | Single selection plus viewport-driven offline `ISupportIncrementalLoading` |
| `InputExtensions` and `CommandExtensions` | `toolkit-extensions` | Return-key/focus/dismiss settings and meaningful command parameters |
| `ResourceExtensions` and `VisualStateManagerExtensions` | `toolkit-extensions` | Locally scoped lightweight resources and attached named states |
| `SelectorExtensions` and `TabBarSelectorBehavior` | `toolkit-extensions` | `PipsPager`, `FlipView`, and `TabBar` synchronization |

`GridExtensions` is not present in Uno.Toolkit.WinUI 9.0.3 and is therefore not
classified as covered.

## Startup and platform integration

`ExtendedSplashScreen` is a startup/window control. A navigable Gallery page
would not exercise its contract truthfully because the application window and
startup handoff already exist by the time a sample is opened. It remains
documentation-only until Gallery has an isolated startup-head integration
harness.

`StatusBar` changes operating-system chrome and has no truthful cross-platform
result on desktop or WebAssembly. `TabBarItemExtensions` responds when an already
selected tab is clicked and targets navigation or scrolling hosts. Neither is
claimed by the deterministic cross-platform `toolkit-extensions` sample.

## Simple theme spike

Uno.Sdk 6.6.42 maps `SimpleTheme` to `Uno.Simple.WinUI` 7.0.3 and
`Uno.Toolkit.WinUI.Simple` 9.0.3. The exact stable packages were absent from the
available package cache; targeted restore then failed at the NuGet TLS handshake.
Consequently resource compatibility could not be proven across Desktop, Windows,
Android, and WebAssembly, and no Simple tab or package reference was retained.
An incremental production WebAssembly bundle delta for Simple is **not
available** because an exact-package build could not be produced.

The existing Material and Fluent resources remain unchanged, and Cupertino
continues to be selected only through its contained design templates.

## Measured catalog and bundle change

Using the integrated `4a6918be` baseline and the same Release Skia WebAssembly
Interpreter+AOT publish settings:

| Measurement | Baseline | Waves A–B | Delta |
| --- | ---: | ---: | ---: |
| Exported sample manifest entries | 111 | 114 | +3 |
| Production WebAssembly raw payload | 100,527,722 B | 100,691,982 B | +164,260 B |
| Production WebAssembly Brotli payload | 20,601,875 B | 20,631,876 B | +30,001 B |

Raw payload excludes `.br`, `.gz`, and source-map files; Brotli payload is the
sum of published `.br` files. File count remained 747.
