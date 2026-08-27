import assert from "node:assert/strict";
import test from "node:test";
import { mkdir, rm, writeFile } from "node:fs/promises";
import { resolve } from "node:path";
import { verifyBaselineMetadata } from "../src/runner.mjs";
import { digest } from "../src/config.mjs";

const root = resolve("artifacts", "metadata-test");
const config = {
  suiteVersion: 1,
  samples: [{ id: "one" }, { id: "two" }]
};
const metadata = {
  schemaVersion: 1,
  suiteVersion: 1,
  configDigest: digest(config),
  lockDigest: "lock",
  toolDigest: "tool",
  browserVersion: "Chrome/1",
  samples: [{ id: "one" }, { id: "two" }]
};

test("missing and stale baseline sets are rejected", async () => {
  await rm(root, { recursive: true, force: true });
  await mkdir(root, { recursive: true });
  await writeFile(resolve(root, "one.png"), "");
  await assert.rejects(
    verifyBaselineMetadata(config, root, metadata, "Chrome/1", "lock", "tool"),
    /PNG set is missing or stale/
  );
  await writeFile(resolve(root, "two.png"), "");
  await assert.rejects(
    verifyBaselineMetadata(config, root, { ...metadata, configDigest: "stale" }, "Chrome/1", "lock", "tool"),
    /metadata is stale/
  );
  await rm(root, { recursive: true, force: true });
});
