# ADR 0002 — Generated Sample Catalog (Routes + Manifest)

**Date:** 2026-08-25  
**Status:** Proposed  
**Branch:** `modernization/trust-baseline`

---

## Context

The Gallery app's sample catalog is currently scattered: `App.GetSamples()` is the
sole runtime source of truth, but nothing produces a machine-readable view of the
catalog without running the app.

Two recurring needs surfaced:

1. **Navigation stability** — Feature code and tests hardcode slug strings like
   `"my-control"` as raw string literals with no compile-time link to the catalog.
   A renamed or removed sample silently breaks navigation.

2. **Catalog introspection** — CI tooling, documentation generators, and future
   physical export tasks (JSON file, search index, compatibility manifest) all need
   a stable schema-versioned view of the catalog that is available without launching
   the app.

---

## Decision

Extend `SamplesGenerator` to emit two additional generated files per compilation
target, alongside the existing `App.Samples.g.cs`:

### 1. `SampleRoutes.g.cs` — Route constants

```csharp
internal static class SampleRoutes
{
    public const string MyControl  = @"my-control";
    public const string Oauth2Login = @"oauth2-login";
    // …one entry per unique, non-colliding slug in this target
}
```

**Identifier derivation:** Each slug is split on `-`; the first character of each
segment is uppercased and the segments are joined (PascalCase). A leading `_` is
prepended if the result starts with a digit (e.g. `2cool` → `_2cool`).

**Collision handling (UGG0010 — Error):** When two *different* slugs produce the
same C# identifier (e.g. `a1b` and `a-1b` both yield `A1b` because a digit-start
segment cannot be uppercased to distinguish position), neither constant is emitted.
`GetSamples()` is unaffected. Resolve by setting an explicit `Slug` on one sample.

**UGG0006 interaction:** Duplicate-slug pairs (same slug, UGG0006) produce one
shared constant because the slug value is identical; no UGG0010 fires.

### 2. `SampleManifest.g.cs` — Deterministic JSON catalog

```csharp
internal static class SampleManifest
{
    public static string GetJson() { … }
}
```

`GetJson()` returns a compact, deterministic JSON string at runtime. The JSON is
built inside a generated `StringBuilder`-based method so that no single string
literal exceeds the IL US-heap 64 KB limit. No file I/O, no MSBuild targets, and
no new runtime packages are needed.

**Schema version 1:**

```json
{
  "schemaVersion": 1,
  "samples": [
    {
      "fqn":               "Uno.Gallery.MySamplePage",
      "slug":              "my-sample",
      "title":             "My Sample",
      "category":          { "value": 0, "name": "Controls" },
      "description":       null,
      "glyph":             "\uE8FA",
      "documentationLink": null,
      "sourceSdk":         { "value": 0, "name": "WinUI" },
      "sortOrder":         2147483647,
      "status":            { "value": 0, "name": "Stable" },
      "tags":              [],
      "owner":             null,
      "reviewedOn":        null,
      "relatedSamples":    [],
      "sourcePath":        null,
      "platformConditionals": null
    }
  ]
}
```

Field notes:

| Field | Type | Notes |
|---|---|---|
| `schemaVersion` | `int` | Always `1` in this iteration |
| `fqn` | `string` | Fully-qualified C# type name, FQN-sorted deterministically |
| `slug` | `string` | Final slug (explicit or derived) |
| `category` / `sourceSdk` / `status` | `{value, name}` | Numeric enum value + member name |
| `tags` / `relatedSamples` | `string[]` | Never `null`; empty array when absent |
| `sourcePath` | `string \| null` | Repo-relative path anchored at `Views/`, or `null` for in-memory trees |
| `platformConditionals` | `uint \| null` | Raw `SampleConditionals` flag value; `null` when no `[SampleConditional]` |

**Determinism:** Entries are sorted by fully-qualified type name (ordinal). JSON
escaping is implemented inline in the generator (`AppendJsonString`) without any
runtime package — `"` → `\"`, `\` → `\\`, control chars → `\uXXXX`.

---

## Per-window Samples and Factories

`GetSamples()` is per-compilation-target: the generator reads preprocessor symbols
at compile time and excludes samples whose `[SampleConditional]` flags don't match
the target platform. The manifest follows the same filter — it reflects exactly the
samples present for the target, not the global union.

Each sample entry in `GetSamples()` carries:
- A page factory: `static () => new T()` — zero-alloc, AOT-safe.
- An optional data factory: `static () => new TData()` — same pattern.

The manifest records the FQN of each type but does not reproduce the factories;
those remain in `App.Samples.g.cs`.

---

## Physical Build Artifact

`SampleManifest.g.cs` exposes `GetJson()` as the stable internal contract.
A physical JSON export is produced by WebAssembly CI without adding file I/O to
the generator:

1. The normal compiler emits generated C# into a job-local intermediate folder.
2. `build/scripts/export-sample-manifest.ps1` parses `SampleManifest.g.cs` with
   Roslyn and reconstructs the exact `GetJson()` content.
3. The script validates ordering and unique slugs, then writes
   `sample-manifest.json` and its SHA-256 sidecar.
4. CI validates schema-v1 and target-specific minimum/required-slug baselines,
   then publishes separate DOM and Skia catalog artifacts.

The app is not launched, target assemblies are not loaded through reflection,
and no catalog file is committed as a second source of truth. ADR 0006 defines
the separate upstream feature-classification and drift policy.

No app UI change was made as part of this ADR.

---

## Consequences

**Positive**

- Compile-time–safe navigation: `SampleRoutes.MyControl` instead of `"my-control"`.
- Machine-readable catalog without launching the app.
- UGG0010 surfaces slug/identifier design mistakes at build time.
- Zero runtime package delta; no file I/O in the generator.

**Negative / Trade-offs**

- Adding a new sample requires regenerating `SampleRoutes.g.cs` and `SampleManifest.g.cs`
  (automatic; happens on every build).
- Very large catalogs (> ~100 samples × 600 chars) require the `StringBuilder`-based
  chunking already in place to stay under the IL literal limit.
- The manifest is per-target; cross-target union views require a separate aggregation step.
