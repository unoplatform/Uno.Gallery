import { access } from "node:fs/promises";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import { loadConfig } from "./config.mjs";
import { writeReport } from "./report.mjs";
import { runVisual } from "./runner.mjs";

const visualRoot = resolve(dirname(fileURLToPath(import.meta.url)), "..");
const command = process.argv[2];
const args = new Map();
for (let index = 3; index < process.argv.length; index += 2) {
  args.set(process.argv[index], process.argv[index + 1]);
}
const reportPath = join(visualRoot, "artifacts", "report.json");
let report;
try {
  const config = await loadConfig(join(visualRoot, "visual.config.json"));
  if (command === "validate") {
    console.log(`Validated ${config.samples.length} visual pilot samples.`);
    process.exit(0);
  }
  if (!["compare", "update"].includes(command)) {
    throw new Error("usage: node src/cli.mjs <compare|update|validate> [--wasm <published-wwwroot>]");
  }
  const wasmRoot = args.get("--wasm");
  if (!wasmRoot) throw new Error("--wasm <published-wwwroot> is required");
  await access(join(resolve(wasmRoot), "index.html"));
  if (command === "update") {
    if (process.env.CI || process.env.TF_BUILD || process.env.BUILD_BUILDID) {
      throw new Error("baseline update is forbidden in CI");
    }
    if (process.env.VISUAL_ACCEPT_BASELINES !== "1") {
      throw new Error("local update requires both the update command and VISUAL_ACCEPT_BASELINES=1");
    }
  }
  report = await runVisual({ mode: command, config, visualRoot, wasmRoot });
  await writeReport(reportPath, report);
  for (const result of report.results) {
    console.log(`${result.passed ? "PASS" : "FAIL"} ${result.id}`
      + (result.diffRatio == null ? "" : ` ${(result.diffRatio * 100).toFixed(6)}% (${result.differentPixels} px)`));
  }
  console.log(`Browser ${report.browserVersion}; viewport ${report.viewport}; server PID ${report.serverPid}`);
  if (!report.passed) process.exitCode = 1;
} catch (error) {
  report = { passed: false, mode: command, error: error.stack ?? error.message, results: [] };
  await writeReport(reportPath, report);
  console.error(error.message);
  process.exitCode = 1;
}
