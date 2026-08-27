import { access, mkdir, readFile, writeFile } from "node:fs/promises";
import { dirname, join, resolve } from "node:path";
import { tmpdir } from "node:os";
import {
  closeBrowser,
  launchBrowser,
  monitorPage,
  removeDirectory,
  startServer
} from "./performance-cli.mjs";

function parseArguments(argumentsList) {
  const parsed = {};
  for (let index = 0; index < argumentsList.length; index += 2) {
    const key = argumentsList[index];
    const value = argumentsList[index + 1];
    if (!key?.startsWith("--") || value === undefined) {
      throw new Error(`Expected --name value arguments, got '${key ?? ""}'.`);
    }
    parsed[key.slice(2)] = value;
  }
  for (const required of ["wasm", "config", "renderer", "output"]) {
    if (!parsed[required]) throw new Error(`Missing required --${required} argument.`);
  }
  if (!["DOM", "Skia"].includes(parsed.renderer)) {
    throw new Error(`--renderer must be DOM or Skia, got '${parsed.renderer}'.`);
  }
  return parsed;
}

async function main() {
  const args = parseArguments(process.argv.slice(2));
  const wasmRoot = resolve(args.wasm);
  const outputPath = resolve(args.output);
  const config = JSON.parse(await readFile(resolve(args.config), "utf8"));
  await access(wasmRoot);
  const profilePath = join(tmpdir(), `uno-gallery-startup-${process.pid}-${args.renderer}`);
  const server = await startServer(wasmRoot);
  let browser;
  let page;
  try {
    browser = await launchBrowser(config, profilePath);
    page = await browser.newPage();
    const monitor = monitorPage(page, server.baseUrl);
    await page.emulateTimezone(config.browser.timezone);
    await page.emulateMediaFeatures([
      { name: "prefers-color-scheme", value: config.browser.colorScheme },
      { name: "prefers-reduced-motion", value: config.browser.reducedMotion }
    ]);
    await page.goto(server.baseUrl, {
      waitUntil: "domcontentloaded",
      timeout: config.timeouts.startupMs
    });
    const started = Date.now();
    if (args.renderer === "DOM") {
      await page.waitForSelector('[xamltype="Uno.Gallery.Shell"]', {
        visible: true,
        timeout: config.timeouts.startupMs
      });
    } else {
      await page.waitForFunction(
        () => [...document.querySelectorAll("canvas")]
          .some(canvas => canvas.width > 0 && canvas.height > 0),
        { timeout: config.timeouts.startupMs }
      );
    }
    await page.evaluate(() => new Promise(resolveBrowserFrame =>
      requestAnimationFrame(() => requestAnimationFrame(resolveBrowserFrame))));
    monitor.assertClean(`${args.renderer} startup`);
    const report = {
      schemaVersion: 1,
      generatedAt: new Date().toISOString(),
      renderer: args.renderer,
      browserVersion: await browser.version(),
      readyMs: Date.now() - started,
      passed: true
    };
    await mkdir(dirname(outputPath), { recursive: true });
    await writeFile(outputPath, `${JSON.stringify(report, null, 2)}\n`, "utf8");
    console.log(`${args.renderer} release startup probe passed in ${report.readyMs} ms.`);
  } finally {
    if (page) {
      await page.close().catch(() => {});
    }
    if (browser) {
      await closeBrowser(browser);
    }
    await server.stop();
    await removeDirectory(profilePath);
  }
}

main().catch(error => {
  console.error(error.stack ?? error);
  process.exitCode = 1;
});
