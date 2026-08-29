# ADR 0005: Native store release strategy

- Status: Proposed
- Date: 2026-08-26

## Context

Gallery already builds distinct Native and Skia Android/iOS applications and
publishes prerelease artifacts through protected Azure service connections.
Windows produces a signed package but has no Store stage. Rebuilding at each
promotion or treating Native and Skia as one product would make certification
and rollback ambiguous.

## Decision

- Stable release branches use the existing NBGV `release/stable/*` flow and
  publish release evidence, but do not automatically target the rolling store
  and web channels.
- Native and Skia artifacts keep separate application identities and evidence.
- A stable artifact is built once, certified, then promoted through staged
  channels without rebuilding.
- Store credentials and approvals remain in protected CI environments.
- Windows Store automation remains disabled until its identity, ownership,
  Partner Center connection, and rollback process are accepted.
- Every promotion publishes the compatibility, quality, performance, known
  limitation, artifact hash, and rollback record described in
  `docs/releases/native-store-delivery-plan.md`.

## Consequences

The repository can define and validate release evidence without containing
credentials. Protected `master` retains the existing rolling web, Play alpha,
and TestFlight deployments. Stable-branch production promotion remains manual
until a dedicated artifact-promotion workflow and store-owner approvals are in
place. Native and Skia results must be reported separately.
