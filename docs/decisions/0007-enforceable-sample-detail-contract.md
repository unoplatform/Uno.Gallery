# ADR 0007: Enforceable sample-detail contract

## Status

Proposed

## Context

`SamplePageAttribute` historically defaulted to Stable, so requiring complete
metadata from every Stable runtime value would incorrectly mark untouched pages
as reviewed. Free-form compatibility arrays would also make target reporting
ambiguous and difficult to validate.

## Decision

Contract v1 is opt-in for legacy pages and mandatory whenever a sample explicitly
authors `Status = SampleStatus.Stable`. `UGG0011` blocks compilation when an
explicit Stable or contract-v1 page lacks required detail metadata. Design and
renderer support use stable `[Flags]` enums. The manifest records both the
numeric flag value and deterministic member names, plus whether Status was
explicitly authored.

The producer advances to sample-manifest schema version 2. The original strict
schema-v1 document remains unchanged because required fields and
`additionalProperties: false` make the new shape incompatible in both
directions. All v2 fields are present with zero, empty, or null defaults for
legacy entries.

A deterministic per-target report hard-fails incomplete contract-v1 metadata,
an explicit Stable entry outside contract v1, a new implicit-Stable slug outside
the frozen grandfathered allowlist, or configured count/slug regressions. It
reports both contract-v1 and legacy backlog counts; the first migration wave is
not represented as catalog-wide completion.

## Consequences

- Stable quality claims become compile-time enforceable and auditable.
- Compatibility metadata is machine-readable without string parsing.
- Existing implicit-Stable pages remain buildable and visible as backlog.
- Contract metadata adds detail to sample pages only when present.
- Any future incompatible meaning or field removal requires a schema-version
  bump and coordinated consumer migration.
