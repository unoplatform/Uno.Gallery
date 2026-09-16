import assert from "node:assert/strict";
import test from "node:test";
import { createHash } from "node:crypto";
import { mkdir, rm, writeFile } from "node:fs/promises";
import { resolve } from "node:path";
import { verifyBaselineMetadata } from "../src/runner.mjs";
import { digest } from "../src/config.mjs";

const root = resolve("artifacts", "metadata-test");
const config = {
  suiteVersion: 1,
  samples: [{ id: "one" }, { id: "two" }]
};
const image = Buffer.from("image");
const imageHash = createHash("sha256").update(image).digest("hex");
const runtime = {
  browserVersion: "Chrome/1",
  browserExecutable: "chrome.exe",
  renderer: "ANGLE SwiftShader",
  host: { os: "win32", architecture: "x64" }
};
const metadata = {
  schemaVersion: 1,
  suiteVersion: 1,
  configDigest: digest(config),
  lockDigest: "lock",
  toolDigest: "tool",
  browserVersion: runtime.browserVersion,
  browserExecutable: runtime.browserExecutable,
  renderer: runtime.renderer,
  host: runtime.host,
  samples: [{ id: "one", sha256: imageHash }, { id: "two", sha256: imageHash }]
};

test("missing and stale baseline sets are rejected", async () => {
  await rm(root, { recursive: true, force: true });
  await mkdir(root, { recursive: true });
  await writeFile(resolve(root, "one.png"), image);
  await assert.rejects(
    verifyBaselineMetadata(config, root, metadata, runtime, "lock", "tool"),
    /PNG set is missing or stale/
  );
  await writeFile(resolve(root, "two.png"), image);
  await assert.rejects(
    verifyBaselineMetadata(config, root, { ...metadata, configDigest: "stale" }, runtime, "lock", "tool"),
    /metadata is stale/
  );
  await assert.rejects(
    verifyBaselineMetadata(
      config,
      root,
      { ...metadata, host: { os: "linux", architecture: "x64" } },
      runtime,
      "lock",
      "tool"
    ),
    /provenance is stale/
  );
  await writeFile(resolve(root, "two.png"), Buffer.from("changed"));
  await assert.rejects(
    verifyBaselineMetadata(config, root, metadata, runtime, "lock", "tool"),
    /PNG hash is missing or stale/
  );
  await rm(root, { recursive: true, force: true });
});
