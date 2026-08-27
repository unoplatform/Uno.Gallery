import assert from "node:assert/strict";
import test from "node:test";
import { mkdir, readFile, rename, rm, writeFile } from "node:fs/promises";
import { resolve } from "node:path";
import { recoverBaselineSet, replaceBaselineSet } from "../src/runner.mjs";

const root = resolve("artifacts", "baseline-test");
const baseline = resolve(root, "baselines");
const staging = resolve(root, ".baselines-staging");
const backup = resolve(root, ".baselines-backup");

test("baseline replacement is whole-set and interrupted swaps recover", async () => {
  await rm(root, { recursive: true, force: true });
  await mkdir(baseline, { recursive: true });
  await mkdir(staging, { recursive: true });
  await writeFile(resolve(baseline, "old.txt"), "old");
  await writeFile(resolve(staging, "new.txt"), "new");

  await replaceBaselineSet(baseline, staging, backup);
  assert.equal(await readFile(resolve(baseline, "new.txt"), "utf8"), "new");
  await assert.rejects(readFile(resolve(baseline, "old.txt")));

  await rename(baseline, backup);
  await recoverBaselineSet(baseline, backup);
  assert.equal(await readFile(resolve(baseline, "new.txt"), "utf8"), "new");
  await rm(root, { recursive: true, force: true });
});
