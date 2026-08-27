import assert from "node:assert/strict";
import test from "node:test";
import { readFile } from "node:fs/promises";
import { resolve } from "node:path";
import { validateConfig } from "../src/config.mjs";

const config = JSON.parse(await readFile(resolve("visual.config.json"), "utf8"));

test("committed pilot config is valid and representative", () => {
  assert.doesNotThrow(() => validateConfig(config));
  assert.equal(config.samples.length, 14);
});

test("duplicate IDs and routes are rejected", () => {
  const invalid = structuredClone(config);
  invalid.samples[1].id = invalid.samples[0].id;
  assert.throws(() => validateConfig(invalid), /duplicate sample id/);
  invalid.samples[1].id = "unique-id";
  invalid.samples[1].design = invalid.samples[0].design;
  invalid.samples[1].slug = invalid.samples[0].slug;
  assert.throws(() => validateConfig(invalid), /duplicate canonical route/);
});

test("loose tolerances and invalid masks are rejected", () => {
  const loose = structuredClone(config);
  loose.comparison.maxDiffRatio = 0.01;
  assert.throws(() => validateConfig(loose), /not tight/);
  const masked = structuredClone(config);
  masked.samples[0].masks = [{ x: 1199, y: 0, width: 2, height: 1 }];
  assert.throws(() => validateConfig(masked), /invalid mask/);
});
