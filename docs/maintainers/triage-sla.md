# Triage service levels

| Event | Target |
|---|---|
| New issue receives complete classification | Five business days |
| New pull request receives its first maintainer response | Seven business days |
| `priority/critical-urgent` investigation starts | Two business days |
| Reporter responds to `triage/needs-information` | Fourteen calendar days |
| Dependency, coverage, quality, and ownership review | Quarterly |

## Automation

`.github/workflows/triage-sla.yml` runs on weekdays and can be dispatched
manually. It labels and comments once on breached issues or pull requests, then
writes the current breach list to the workflow summary. The workflow removes
the breach label after classification or first maintainer response.

Automation applies only to items created on or after September 1, 2026, skips
draft and bot pull requests, and leaves `triage/needs-information` items to the
fourteen-day reporter-response process in the same workflow. It creates at most
ten notification comments per run. The initial backlog follows the manual reset
process in `backlog-triage.md`.

The workflow intentionally does not assign people, infer severity, merge pull
requests, or close issues. Those decisions require maintainer judgment.

The two-business-day `priority/critical-urgent` investigation target is tracked
manually because determining whether a substantive investigation began cannot
be inferred reliably from labels or comment counts.

For post-adoption issues carrying `triage/needs-information`, the workflow reads
the label event and reporter comments from the issue timeline. It closes the
issue only when the reporter has not replied for fourteen days, after posting a
deduplicated explanation. Reporter responses are surfaced for maintainer
reevaluation instead of being closed.

## Maintainer response

For each reported breach:

1. confirm the item still applies to the current stable build;
2. assign the required labels and owner;
3. record the next action or blocking reason;
4. link an upstream issue where appropriate;
5. remove `triage/untriaged` only when classification is complete.
