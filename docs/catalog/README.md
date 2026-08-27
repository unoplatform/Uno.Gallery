# Catalog export and upstream coverage

`[SamplePage]` remains the only source of truth for Gallery samples. The source
generator emits `SampleManifest.g.cs`; it does not write arbitrary files.

During a normal platform build, CI enables compiler-generated source output.
`build/scripts/export-sample-manifest.ps1` parses that generated C# with Roslyn,
reconstructs the exact schema-v1 JSON returned by `SampleManifest.GetJson()`,
checks deterministic FQN ordering and unique slugs, then writes:

- `sample-manifest.json`;
- `sample-manifest.json.sha256`.

`validate-manifest-contract.ps1` validates the committed schemas, checks the
minimum count and required-slug baseline, and proves the comparator rejects a
missing Gallery slug. The exported JSON and SHA are published for the DOM and Skia WebAssembly build
targets. The Desktop baseline is available for local validation but is not yet
published by CI.
The DOM build also produces an advisory `feature-coverage-report.json`.

## Upstream handoff

`build/manifest-fixtures/upstream-features-v1.json` is a pinned, deliberately
small contract fixture. It proves the schema and comparator while the upstream
repositories do not yet publish authoritative manifests. It is not a complete
inventory and must not be reported as 100 percent upstream coverage.

The intended handoff is:

1. `uno`, `uno.toolkit.ui`, `Uno.Themes`, and `uno.extensions` publish the
   versioned upstream schema with stable feature ID, package, status, docs, and
   canonical minimal sample path.
2. Gallery downloads those pinned manifests in a scheduled or release job.
3. `feature-coverage-v1.json` classifies every stable feature as core, optional,
   external, docs-only, not applicable, or blocked.
4. Missing classification or a stale Gallery slug raises an advisory Gallery
   warning and feeds the owned gap-issue process; it does not block a build or
   an upstream release.

Until step 1 is accepted cross-repository, CI compares the local fixture only
and labels its report accordingly.

Phase-specific decisions and measured deltas are recorded in
`phase-4-wave-ab-dispositions.md` and `phase-4-wave-c-dispositions.md`.
