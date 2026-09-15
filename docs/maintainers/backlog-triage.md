# Backlog triage

## Goal

Every open issue and pull request must have a current classification, an owner,
and a next action. The target is 100 percent classified and owned; reducing the
raw issue count is not itself a success measure.

## Required classification

Apply the following to every open issue:

- one `kind/*` label describing the work;
- one or more `area/*` labels identifying the owning surface;
- one `priority/*` label;
- an assignee or named owning team;
- a milestone, or an explicit backlog disposition;
- platform, renderer, and design-system scope when applicable;
- an upstream issue link when the root cause does not belong to Gallery.

Remove `triage/untriaged` only after all required fields are present.

## Revalidation order

1. Reproduce against the current stable Uno.Sdk and Gallery deployment.
2. Close already-fixed reports with the first known fixed version or commit.
3. Consolidate duplicates into the issue with the strongest reproduction.
4. Transfer or link defects whose root cause belongs upstream.
5. Give abandoned or conflicted pull requests notice before closing them.
6. Apply `triage/needs-information` when required evidence is missing.
7. Assign an owner, priority, milestone, and next review date to remaining work.

## Priority

| Label | Meaning | Response |
|---|---|---|
| `priority/critical-urgent` | Startup, data loss, release blocker, or a silently exposed P0 defect | Investigation starts within two business days |
| `priority/important-soon` | Stable sample or core workflow is unusable without a reasonable workaround | Staff for the current or next milestone |
| `priority/important-longterm` | Important architectural, coverage, or quality investment | Keep an owner and quarterly review date |
| `priority/backlog` | Valid work with lower current impact | Review during backlog planning |
| `priority/awaiting-more-evidence` | Value or reproduction is not yet sufficient | Request evidence and apply the response window |

## Coverage disposition

Feature requests must be classified as one of:

- covered in core Gallery;
- covered in an optional Gallery flavor;
- external companion;
- documentation only;
- not applicable;
- blocked, with owner, issue, and reason.

## Cadence

- Weekly: classify new intake and review SLA breaches.
- Monthly: review P0/P1 ownership, stale pull requests, and upstream blockers.
- Quarterly: review the complete backlog, dependency exceptions, coverage drift,
  performance trends, and area ownership.

The scheduled `Triage SLA` workflow reports incompletely classified issues older
than five business days and pull requests without a maintainer response after
seven business days. It does not decide priority. It closes only post-adoption
`triage/needs-information` items whose reporter did not respond during the
documented fourteen-day window.
