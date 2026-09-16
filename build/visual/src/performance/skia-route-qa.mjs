import { mkdir, readFile, writeFile } from "node:fs/promises";
import { dirname, join, resolve } from "node:path";
import { tmpdir } from "node:os";
import { pathToFileURL } from "node:url";
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
  for (const required of ["wasm", "manifest", "config", "output"]) {
    if (!parsed[required]) throw new Error(`Missing required --${required} argument.`);
  }
  return parsed;
}

function preferredDesign(sample) {
  const value = Number(sample.supportedDesigns?.value ?? 0);
  if ((value & 16) !== 0) return "Agnostic";
  if ((value & 1) !== 0) return "Material";
  if ((value & 2) !== 0) return "Fluent";
  if ((value & 4) !== 0) return "Cupertino";
  if ((value & 8) !== 0) return "Native";
  return "Material";
}

export function selectRoutes(manifest) {
  if (manifest?.schemaVersion !== 2 || !Array.isArray(manifest.samples)) {
    throw new Error("Skia route QA requires a schema-v2 sample manifest.");
  }
  const routes = manifest.samples
    .filter(sample =>
      sample.status?.name === "Stable" &&
      sample.status?.value === 0 &&
      sample.category?.name !== "Canary")
    .map(sample => ({
      slug: sample.slug,
      design: preferredDesign(sample)
    }))
    .sort((left, right) => left.slug.localeCompare(right.slug));
  if (routes.length < 100) {
    throw new Error(`Skia route QA expected at least 100 Stable routes; found ${routes.length}.`);
  }
  return routes;
}

const requiredAccessibleNames = [
  "Gallery navigation",
  "Search samples",
  "Toggle light and dark theme",
  "Material design",
  "Fluent design",
  "Copy direct link"
];

async function waitForFrames(page, count) {
  await page.evaluate(frameCount => new Promise(resolveFrames => {
    let remaining = frameCount;
    const next = () => {
      if (--remaining <= 0) {
        resolveFrames();
      } else {
        requestAnimationFrame(next);
      }
    };
    requestAnimationFrame(next);
  }), count);
}

async function main() {
  const args = parseArguments(process.argv.slice(2));
  const wasmRoot = resolve(args.wasm);
  const outputPath = resolve(args.output);
  const config = JSON.parse(await readFile(resolve(args.config), "utf8"));
  const manifest = JSON.parse(await readFile(resolve(args.manifest), "utf8"));
  const routes = selectRoutes(manifest);
  const profilePath = join(tmpdir(), `uno-gallery-skia-route-qa-${process.pid}`);
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
    await page.waitForFunction(
      () => [...document.querySelectorAll("canvas")]
        .some(canvas => canvas.width > 0 && canvas.height > 0),
      { timeout: config.timeouts.startupMs }
    );
    await page.waitForFunction(
      () => document.querySelectorAll('[id^="uno-semantics-"]').length > 1,
      { timeout: config.timeouts.actionMs }
    );
    await page.waitForFunction(
      names => names.every(name =>
        [...document.querySelectorAll('[id^="uno-semantics-"]')]
          .some(element => element.getAttribute("aria-label") === name)),
      { timeout: config.timeouts.actionMs },
      requiredAccessibleNames
    );
    monitor.assertClean("Skia startup");

    const results = [];
    for (const route of routes) {
      const started = performance.now();
      await page.evaluate(({ slug, design }) => {
        const url = `?design=${encodeURIComponent(design)}#${encodeURIComponent(slug)}`;
        history.replaceState({ slug, design }, "", url);
        dispatchEvent(new PopStateEvent("popstate", { state: { slug, design } }));
      }, route);
      await waitForFrames(page, 4);
      await new Promise(resolveWait => setTimeout(resolveWait, 100));
      const state = await page.evaluate(() => ({
        slug: decodeURIComponent(location.hash.slice(1)),
        canvasReady: [...document.querySelectorAll("canvas")]
          .some(canvas => canvas.width > 0 && canvas.height > 0),
        semanticElementCount:
          document.querySelectorAll('[id^="uno-semantics-"]').length
      }));
      if (state.slug !== route.slug || !state.canvasReady || state.semanticElementCount <= 1) {
        throw new Error(`Skia route '${route.slug}' did not retain its canvas and semantic tree.`);
      }
      monitor.assertClean(`Skia route '${route.slug}'`);
      results.push({
        slug: route.slug,
        design: route.design,
        durationMs: Math.round((performance.now() - started) * 1000) / 1000
      });
    }

    const report = {
      schemaVersion: 1,
      generatedAt: new Date().toISOString(),
      browserVersion: await browser.version(),
      routeCount: results.length,
      semanticElementCount: await page.evaluate(
        () => document.querySelectorAll('[id^="uno-semantics-"]').length
      ),
      verifiedAccessibleNames: requiredAccessibleNames,
      passed: true,
      routes: results
    };
    await mkdir(dirname(outputPath), { recursive: true });
    await writeFile(outputPath, `${JSON.stringify(report, null, 2)}\n`, "utf8");
    console.log(`Skia route QA passed for ${results.length} Stable production routes.`);
  } finally {
    if (page) await page.close().catch(() => {});
    if (browser) await closeBrowser(browser);
    await server.stop();
    await removeDirectory(profilePath);
  }
}

if (import.meta.url === pathToFileURL(process.argv[1]).href) {
  main().catch(error => {
    console.error(error.stack ?? error);
    process.exitCode = 1;
  });
}
