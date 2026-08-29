import test from "node:test";
import assert from "node:assert/strict";
import { nearestRank, summarizeRuns } from "../src/performance/performance-metrics.mjs";
import { selectRoutes } from "../src/performance/skia-route-qa.mjs";

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

test("Skia route QA selects Stable production routes and preferred designs", () => {
  const samples = Array.from({ length: 100 }, (_, index) => ({
    slug: `sample-${index}`,
    status: { value: 0, name: "Stable" },
    category: { name: "UIComponents" },
    supportedDesigns: { value: index === 0 ? 16 : 1 }
  }));
  samples.push({
    slug: "diagnostics",
    status: { value: 2, name: "Experimental" },
    category: { name: "NonUIFeatures" },
    supportedDesigns: { value: 16 }
  });
  samples.push({
    slug: "hidden-canary",
    status: { value: 0, name: "Stable" },
    category: { name: "Canary" },
    supportedDesigns: { value: 4 }
  });

  const routes = selectRoutes({ schemaVersion: 2, samples });
  assert.equal(routes.length, 100);
  assert.equal(routes.find(route => route.slug === "sample-0").design, "Agnostic");
  assert.ok(!routes.some(route => route.slug === "diagnostics"));
  assert.ok(!routes.some(route => route.slug === "hidden-canary"));
});
