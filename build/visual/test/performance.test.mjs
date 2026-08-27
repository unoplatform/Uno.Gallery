import test from "node:test";
import assert from "node:assert/strict";
import { nearestRank, summarizeRuns } from "../src/performance/performance-metrics.mjs";

test("nearestRank uses the documented discrete percentile", () => {
  assert.equal(nearestRank([9, 1, 7, 3], 0.5), 3);
  assert.equal(nearestRank([9, 1, 7, 3], 0.75), 7);
});

test("summarizeRuns calculates every runtime metric", () => {
  const runs = [1, 2, 3, 4].map(value => ({
    firstContentfulPaintMs: value,
    shellReadyMs: value * 2,
    firstInputLatencyMs: value * 3,
    searchRenderedMs: value * 4,
    navigationRenderedMs: value * 5
  }));
  const summary = summarizeRuns(runs);
  assert.equal(summary.observationCount, 4);
  assert.deepEqual(summary.firstContentfulPaintMs, {
    minimum: 1,
    p50: 2,
    p75: 3,
    maximum: 4
  });
  assert.equal(summary.navigationRenderedMs.p75, 15);
});
