import assert from "node:assert/strict";
import test from "node:test";
import { PNG } from "pngjs";
import { comparePngBuffers, decodePng } from "../src/comparator.mjs";

function png(width, height, color = [0, 0, 0, 255]) {
  const image = new PNG({ width, height });
  for (let offset = 0; offset < image.data.length; offset += 4) {
    image.data.set(color, offset);
  }
  return PNG.sync.write(image);
}

test("identical pixels pass", () => {
  const image = png(3, 2);
  const result = comparePngBuffers(image, image, { maxDiffRatio: 0 });
  assert.equal(result.passed, true);
  assert.equal(result.differentPixels, 0);
});

test("changed pixels fail and masks are explicit", () => {
  const baseline = png(2, 2);
  const changed = decodePng(baseline);
  changed.data.set([255, 255, 255, 255], 0);
  const current = PNG.sync.write(changed);
  assert.equal(comparePngBuffers(baseline, current, { maxDiffRatio: 0 }).passed, false);
  assert.equal(comparePngBuffers(baseline, current, {
    maxDiffRatio: 0,
    masks: [{ x: 0, y: 0, width: 1, height: 1 }]
  }).passed, true);
});

test("dimension mismatch and malformed PNG fail clearly", () => {
  const mismatch = comparePngBuffers(png(2, 2), png(3, 2));
  assert.equal(mismatch.passed, false);
  assert.match(mismatch.reason, /dimension mismatch/);
  assert.throws(() => decodePng(Buffer.from("not png"), "baseline"), /not a valid PNG/);
});

test("overlapping masks cannot make unmasked differences pass", () => {
  const baseline = png(2, 2);
  const changed = decodePng(baseline);
  changed.data.set([255, 255, 255, 255], 8);
  const current = PNG.sync.write(changed);
  const result = comparePngBuffers(baseline, current, {
    maxDiffRatio: 0,
    masks: [
      { x: 0, y: 0, width: 2, height: 1 },
      { x: 0, y: 0, width: 2, height: 1 }
    ]
  });
  assert.equal(result.passed, false);
  assert.equal(result.differentPixels, 1);
  assert.equal(result.diffRatio, 0.5);
});
