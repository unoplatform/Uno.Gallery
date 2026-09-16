# ADR 0006: Upstream feature manifest and Gallery drift

- Status: Proposed
- Date: 2026-08-26

## Context

Gallery has a deterministic target-specific sample manifest, but Uno core,
Toolkit, Themes, and Extensions do not publish one shared feature inventory.
A hand-maintained Gallery sample JSON would duplicate `[SamplePage]`, and a
source generator must not write arbitrary files.

## Decision

- Keep `[SamplePage]` and `SampleManifest.g.cs` as the Gallery source of truth.
- Export physical JSON only from compiler-generated source after a normal build,
  using Roslyn to read constant `StringBuilder.Append` arguments.
- Version and validate the Gallery sample, upstream feature, and feature
  classification schemas independently.
- Require every upstream stable feature to have one Gallery disposition and
  owner. Core coverage must reference a real generated slug.
- Treat missing Gallery coverage as a soft Gallery release check, not an
  upstream release blocker.
- Use the committed pinned fixture only to exercise the contract until upstream
  repositories publish authoritative manifests.

## Consequences

Catalog JSON is deterministic and cannot drift from generated runtime data.
Cross-repository completeness remains explicitly unresolved until upstream
owners accept and publish the schema. Gallery can adopt those manifests without
changing its internal catalog contract.
