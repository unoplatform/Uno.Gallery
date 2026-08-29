import pixelmatch from "pixelmatch";
import { PNG } from "pngjs";

export function decodePng(buffer, label = "PNG") {
  try {
    return PNG.sync.read(buffer);
  } catch (error) {
    throw new Error(`${label} is not a valid PNG: ${error.message}`);
  }
}

function applyMasks(image, masks) {
  const result = new PNG({ width: image.width, height: image.height });
  const maskedPixels = new Uint8Array(image.width * image.height);
  let maskedPixelCount = 0;
  image.data.copy(result.data);
  for (const mask of masks) {
    for (let y = mask.y; y < mask.y + mask.height; y++) {
      for (let x = mask.x; x < mask.x + mask.width; x++) {
        const offset = (y * result.width + x) * 4;
        result.data.fill(0, offset, offset + 4);
        const pixelOffset = y * result.width + x;
        if (!maskedPixels[pixelOffset]) {
          maskedPixels[pixelOffset] = 1;
          maskedPixelCount++;
        }
      }
    }
  }
  return { image: result, maskedPixelCount };
}

export function comparePngBuffers(expectedBuffer, actualBuffer, options = {}) {
  const expected = decodePng(expectedBuffer, "baseline");
  const actual = decodePng(actualBuffer, "current");
  if (expected.width !== actual.width || expected.height !== actual.height) {
    return {
      passed: false,
      reason: `dimension mismatch: baseline ${expected.width}x${expected.height}, current ${actual.width}x${actual.height}`,
      width: actual.width,
      height: actual.height,
      differentPixels: null,
      diffRatio: 1,
      diffBuffer: dimensionDiff(expected, actual)
    };
  }

  const masks = options.masks ?? [];
  const maskedExpected = applyMasks(expected, masks);
  const maskedActual = applyMasks(actual, masks);
  const diff = new PNG({ width: actual.width, height: actual.height });
  const differentPixels = pixelmatch(
    maskedExpected.image.data,
    maskedActual.image.data,
    diff.data,
    actual.width,
    actual.height,
    {
      threshold: options.pixelThreshold ?? 0.05,
      includeAA: options.includeAA ?? false,
      diffColor: [255, 0, 255],
      aaColor: [255, 255, 0]
    }
  );
  const comparedPixels = actual.width * actual.height - maskedExpected.maskedPixelCount;
  if (comparedPixels <= 0) {
    return {
      passed: false,
      reason: "masks leave no comparable pixels",
      width: actual.width,
      height: actual.height,
      differentPixels,
      diffRatio: 1,
      diffBuffer: PNG.sync.write(diff)
    };
  }
  const diffRatio = differentPixels / comparedPixels;
  const maxDiffRatio = options.maxDiffRatio ?? 0;
  return {
    passed: diffRatio <= maxDiffRatio,
    reason: diffRatio <= maxDiffRatio ? null
      : `${differentPixels} pixels differ (${(diffRatio * 100).toFixed(6)}%, allowed ${(maxDiffRatio * 100).toFixed(6)}%)`,
    width: actual.width,
    height: actual.height,
    differentPixels,
    diffRatio,
    diffBuffer: PNG.sync.write(diff)
  };
}

function dimensionDiff(expected, actual) {
  const width = Math.max(expected.width, actual.width);
  const height = Math.max(expected.height, actual.height);
  const diff = new PNG({ width, height, fill: true });
  for (let offset = 0; offset < diff.data.length; offset += 4) {
    diff.data[offset] = 255;
    diff.data[offset + 3] = 255;
  }
  return PNG.sync.write(diff);
}
