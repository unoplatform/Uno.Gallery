# ADR-0003 WASM URL/History Synchronization

## Status

Accepted

## Context

Uno Gallery on WebAssembly previously used a primitive hash-fragment strategy for deep-linking:

- On startup, the URL hash (e.g. `#Button`, `#Text%20Box`) was read once to select the initial page.
- Navigation view-tree traversal was used to find the matching item instead of the typed navigator.
- No browser history was maintained: navigating between samples did not update the URL, and pressing
  Back/Forward had no effect on the app's page.
- Legacy TypeScript files (`FragmentNavigation.ts`, `LocationHrefNavigation.ts`) contained
  `MonoRuntime`-based subscription stubs that were never wired up from C#.
- Unknown hashes triggered a hard redirect to `/` (full page reload).

After Phase 1 (ADR-0002 generated catalog + typed navigator), all sample lookup can be done via
`IGalleryNavigator`/`ShellNavigator` without nav-tree traversal, enabling a clean URL sync layer.

## Decision

### Canonical URL format

```
?design=<Design>#<lowercase-slug>
```

- `<Design>` is the `Design` enum value (`Material`, `Fluent`, `Cupertino`, `Native`, `Agnostic`).
- `<lowercase-slug>` is the URL-safe slug derived from the sample title by `SlugHelper.DeriveSlug`.
- The overview page uses slug `overview`: `?design=Material#overview`.
- `encodeURIComponent` is applied to the design value in JS; the slug is already URL-safe.

### TypeScript: `BrowserHistory.ts` (namespace `Uno.Gallery.Wasm`)

A new `BrowserHistory` class in the existing TypeScript namespace/bundle provides:

| Method | Purpose |
|---|---|
| `getHash()` | Hash without `#`, or `""` |
| `getDesign()` | `?design=` value, or `""` |
| `subscribe(callback\|null)` | One listener on popstate + hashchange; null removes it |
| `pushState(slug, design)` | New history entry, canonical URL |
| `replaceState(slug, design)` | Replace current entry, canonical URL |
| `replaceDesign(design)` | Update `?design=` only, no new entry |

Deduplication: both `popstate` and `hashchange` can fire for the same Back/Forward navigation.
The listener tracks `_currentHref` and short-circuits when `window.location.href` is unchanged.

`pushState`/`replaceState` update `_currentHref` immediately so that a subsequent popstate for
the same URL is silently skipped.

### C# interop: `BrowserHistoryHandler.Interop.cs`

Uses `[JSImport]` with `[JSMarshalAs<JSType.Function<JSType.String>>] Action<string>?` for
the subscribe parameter. The delegate is stored in a private static `_subscribedCallback` field
(GC root) before it crosses the JS boundary, ensuring the listener cannot outlive the delegate.
Null unsubscribe is supported: passing `null` removes the JS event listeners.

### NavigationOptions flags

One new flag is added:

- `SkipHistory` — suppresses `PushState` in `ShellNavigator.NavigateTo`. Used for startup
  navigation and browser-callback navigation to avoid double-recording entries.
  No-op on non-WASM targets.

Canonicalization (startup and unknown-fragment) is done by calling `BrowserHistoryHandler.ReplaceState`
directly from the call site, not through `NavigateTo`.

### Startup flow (WASM)

1. `BrowserHistoryHandler.GetHash()` and `GetDesign()` read the initial URL state.
2. `Enum.TryParse<Design>` (case-insensitive) on the design value; calls `SetPreferredDesign`
   before any page is created so `SamplePageLayout` picks up the design on first render.
3. Sample lookup (four-step, in order):
   a. Exact `FindBySlug(rawHash)` — canonical slugs.
   b. Exact `FindBySlug(decoded)` — percent-encoded slugs (rare).
   c. Exact `FindByTitle(decoded)` — legacy title-fragment links (`#Button`, `#Text%20Box`).
   d. Case-insensitive `Title.Contains` fallback — partial/legacy fragments (`#Tex` → "Text Box").
4. On match: `NavigateTo(sample, SkipNavSync | SkipHistory)` → `ReplaceState(sample.Slug, design)`.
5. On no match (unknown fragment): navigate to overview, then `ReplaceState("overview", design)`.
6. Overview / empty hash: navigate to overview, then `ReplaceState("overview", design)`.

### Browser Back/Forward

After initial navigation, `BrowserHistoryHandler.Subscribe` is called once. A `DispatcherQueue`
is captured on the UI thread before the call and passed into the callback via closure. The callback
receives `"slug\ndesign"` and dispatches to the UI queue:

- Parses slug and design from the state string.
- Calls `SetPreferredDesign(design)` so the next created page renders in the correct design.
- Calls `NavigateToSlug(slug, SkipNavSync | SkipHistory)` — no new history entry, no nav-sync.
- On unknown slug: navigates to overview and calls `ReplaceState("overview", ...)` directly.

Analytics (`AnalyticsService.TrackView`) still fires for every navigation, including back/forward.

### Mobile share URL

`SamplePageLayout.GetShareUri(string sampleSlug)` produces canonical slug-based share URLs:
`https://gallery.platform.uno/#<slug>`. Existing title-based links received from the outside
continue to work via the four-step startup lookup (steps 3 and 4 in the startup flow above).

### Design tab changes

`SamplePageLayout.OnLayoutRadioButtonChecked` calls `BrowserHistoryHandler.ReplaceDesign(design)`
on WASM. This updates `?design=` in the current URL without adding a history entry.
`SetPreferredDesign` updates the static `_design` field; future pages created by navigation
pick up the new preference automatically.

### Old-link compatibility

| Old URL format | Resolution |
|---|---|
| `#Button` | Slug lookup (case-insensitive) |
| `#Text%20Box` | URL-decode → exact title match |
| `#Tex` (partial) | Contains fallback |
| Unknown fragment | Canonicalize to `#overview` (no hard redirect) |
| Mobile share URL `https://gallery.platform.uno/#<title>` | Same three-step lookup; canonical slug (`#<slug>`) in URL after load |

### Retired / deleted

- `FragmentNavigation.ts`, `LocationHrefNavigation.ts`, `FragmentNavigationHandler.Interop.cs`,
  `FragmentNavigationHandler.cs`, `LocationHrefNavigationHandler.Interop.cs`,
  `LocationHrefNavigationHandler.cs`: **deleted**. Their `MonoRuntime`-based subscribe stubs were
  never called from C#; all usages replaced by `BrowserHistoryHandler`.
- The hard-redirect `LocationHrefNavigationHandler.CurrentLocationHref = "/"` is removed; unknown
  fragments are now canonicalized with `replaceState` without a page reload.

## Consequences

### Positive

- Browser Back/Forward correctly navigates between gallery samples.
- Shareable canonical URLs: `?design=Material#button`.
- Legacy share links (`#Button`, `#Text%20Box`) continue to work and are silently canonicalized.
- Mobile share produces canonical slug URLs (`https://gallery.platform.uno/#button`).
- No MonoRuntime legacy APIs. One `[JSImport]` delegate callback, zero dead TS files.
- Design tab selection survives Back/Forward navigation (design is encoded in the URL).

### Known limitations

- **Same-page design change via Back/Forward**: If the user navigates away from page A, then presses
  Back to page A (same `ViewType`), `ShellNavigator.NavigateTo` is a no-op and the design tabs on
  the already-displayed `SamplePageLayout` will not visually update. A page re-create or explicit
  tab-sync call would be needed to fix this edge case.
- **Native tab**: The `Native` design tab is only visible on iOS/macOS/Android. URLs with
  `?design=Native` on WASM silently fall back to the first available design.
- **No native activation**: WASM history handler does not integrate with Uno's native activation
  path. Future native rendering activation (`UseNativeRendering=true`) may require additional
  interop to forward URL state into the native shell.

## Future work

- Consider native activation path integration for `UseNativeRendering=true` builds.
