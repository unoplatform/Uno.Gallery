const metricNames = [
  "firstContentfulPaintMs",
  "shellReadyMs",
  "firstInputLatencyMs",
  "searchRenderedMs",
  "navigationRenderedMs"
];

export function nearestRank(values, percentile) {
  if (!Array.isArray(values) || values.length === 0) {
    throw new Error("nearestRank requires at least one observation");
  }
  if (!(percentile > 0 && percentile <= 1)) {
    throw new Error(`percentile must be in (0, 1], got ${percentile}`);
  }
  const sorted = [...values].sort((left, right) => left - right);
  return sorted[Math.ceil(percentile * sorted.length) - 1];
}

export function summarizeRuns(runs) {
  if (!Array.isArray(runs) || runs.length === 0) {
    throw new Error("summarizeRuns requires at least one run");
  }
  const summary = { observationCount: runs.length };
  for (const name of metricNames) {
    const values = runs.map(run => {
      const value = run[name];
      if (!Number.isFinite(value) || value < 0) {
        throw new Error(`invalid ${name} observation: ${value}`);
      }
      return value;
    });
    summary[name] = {
      minimum: round(Math.min(...values)),
      p50: round(nearestRank(values, 0.5)),
      p75: round(nearestRank(values, 0.75)),
      maximum: round(Math.max(...values))
    };
  }
  return summary;
}

export function round(value) {
  return Math.round(value * 1000) / 1000;
}
