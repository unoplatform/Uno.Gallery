import { createHash } from "node:crypto";
import { readFile } from "node:fs/promises";

const idPattern = /^[a-z0-9]+(?:-[a-z0-9]+)*$/;
const designs = new Set(["Material", "Fluent", "Agnostic"]);

export async function loadConfig(path) {
  const config = JSON.parse(await readFile(path, "utf8"));
  validateConfig(config);
  return config;
}

export function validateConfig(config) {
  if (config?.schemaVersion !== 1 || config?.suiteVersion !== 1) {
    throw new Error("visual config requires schemaVersion=1 and suiteVersion=1");
  }
  const viewport = config.viewport;
  if (!Number.isInteger(viewport?.width) || !Number.isInteger(viewport?.height)
      || viewport.width < 320 || viewport.height < 320
      || viewport.deviceScaleFactor !== 1) {
    throw new Error("viewport must have integer dimensions >= 320 and deviceScaleFactor=1");
  }
  if (!/^Chrome\/\d+\.\d+\.\d+\.\d+$/.test(config.browser?.expectedVersion ?? "")) {
    throw new Error("browser.expectedVersion must pin an exact Chrome for Testing version");
  }
  if (!Number.isInteger(config.capture?.settledFrames) || config.capture.settledFrames < 2
      || !Number.isInteger(config.capture?.timeoutMs) || config.capture.timeoutMs < 1000
      || !Number.isInteger(config.capture?.minContentPixels) || config.capture.minContentPixels < 1
      || typeof config.capture?.loadingSelector !== "string"
      || typeof config.capture?.readyMark !== "string" || !config.capture.readyMark.startsWith("app.")) {
    throw new Error("capture must define an app-owned ready mark, >=2 settled frames, and a timeout");
  }
  if (!designs.has(config.capture?.warmup?.design)
      || !idPattern.test(config.capture?.warmup?.slug ?? "")
      || !Number.isInteger(config.capture?.warmup?.delayMs)
      || config.capture.warmup.delayMs < 0) {
    throw new Error("capture warmup route/delay is invalid");
  }
  const comparison = config.comparison;
  if (!(comparison?.pixelThreshold >= 0 && comparison.pixelThreshold <= 1)
      || !(comparison?.maxDiffRatio >= 0 && comparison.maxDiffRatio <= 0.001)
      || typeof comparison.includeAA !== "boolean") {
    throw new Error("comparison tolerance is missing or not tight (maxDiffRatio must be <= 0.001)");
  }
  if (!Array.isArray(config.samples) || config.samples.length < 10 || config.samples.length > 15) {
    throw new Error("pilot must contain 10-15 samples");
  }

  const ids = new Set();
  const routes = new Set();
  for (const sample of config.samples) {
    if (!idPattern.test(sample?.id ?? "") || !idPattern.test(sample?.slug ?? "")) {
      throw new Error(`invalid sample id/slug: ${JSON.stringify(sample)}`);
    }
    if (!designs.has(sample.design) || typeof sample.area !== "string" || sample.area.length === 0) {
      throw new Error(`invalid design/area for ${sample.id}`);
    }
    if (ids.has(sample.id)) {
      throw new Error(`duplicate sample id: ${sample.id}`);
    }
    const route = `${sample.design}:${sample.slug}`;
    if (routes.has(route)) {
      throw new Error(`duplicate canonical route: ${route}`);
    }
    ids.add(sample.id);
    routes.add(route);
    if (!Array.isArray(sample.masks)) {
      throw new Error(`masks must be an array for ${sample.id}`);
    }
    for (const mask of sample.masks) {
      if (![mask.x, mask.y, mask.width, mask.height].every(Number.isInteger)
          || mask.x < 0 || mask.y < 0 || mask.width < 1 || mask.height < 1
          || mask.x + mask.width > viewport.width || mask.y + mask.height > viewport.height) {
        throw new Error(`invalid mask for ${sample.id}`);
      }
    }
  }

  const requiredAreas = [
    "shell/overview", "Material control", "Fluent control", "design tokens",
    "layout", "collection", "Toolkit layout", "Toolkit control",
    "accessibility", "localization"
  ];
  const areas = new Set(config.samples.map(sample => sample.area));
  for (const area of requiredAreas) {
    if (!areas.has(area)) {
      throw new Error(`pilot is missing required area: ${area}`);
    }
  }
}

export function canonicalJson(value) {
  if (Array.isArray(value)) {
    return `[${value.map(canonicalJson).join(",")}]`;
  }
  if (value && typeof value === "object") {
    return `{${Object.keys(value).sort().map(key =>
      `${JSON.stringify(key)}:${canonicalJson(value[key])}`).join(",")}}`;
  }
  return JSON.stringify(value);
}

export function digest(value) {
  return createHash("sha256").update(
    typeof value === "string" || Buffer.isBuffer(value) ? value : canonicalJson(value)
  ).digest("hex");
}
