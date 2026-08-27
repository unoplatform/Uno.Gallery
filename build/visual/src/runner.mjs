import { fork } from "node:child_process";
import { createHash } from "node:crypto";
import { readFile, readdir, rm, mkdir, writeFile, copyFile } from "node:fs/promises";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import puppeteer from "puppeteer";
import { PNG } from "pngjs";
import { comparePngBuffers } from "./comparator.mjs";
import { digest } from "./config.mjs";

const moduleDirectory = dirname(fileURLToPath(import.meta.url));

async function startServer(wasmRoot) {
  const serverPath = join(moduleDirectory, "server.mjs");
  const child = fork(serverPath, [wasmRoot, "0"], { stdio: ["ignore", "inherit", "inherit", "ipc"] });
  const ready = await new Promise((resolveReady, reject) => {
    const timer = setTimeout(() => reject(new Error("visual server did not start within 30 seconds")), 30000);
    child.once("error", reject);
    child.once("exit", code => reject(new Error(`visual server exited before readiness (${code})`)));
    child.on("message", message => {
      if (message?.type === "ready") {
        clearTimeout(timer);
        resolveReady(message);
      }
    });
  });
  return {
    baseUrl: `http://127.0.0.1:${ready.port}/`,
    pid: ready.pid,
    async stop() {
      if (child.exitCode !== null) return;
      child.send({ type: "stop" });
      await Promise.race([
        new Promise(resolveExit => child.once("exit", resolveExit)),
        new Promise(resolveTimeout => setTimeout(resolveTimeout, 5000))
      ]);
      if (child.exitCode === null) child.kill();
    }
  };
}

async function launchBrowser(config, profilePath) {
  await rm(profilePath, { recursive: true, force: true });
  await mkdir(profilePath, { recursive: true });
  const softwareArgs = config.browser.softwareRendering ? [
    "--use-angle=swiftshader",
    "--enable-unsafe-swiftshader",
    "--disable-gpu-sandbox"
  ] : [];
  return puppeteer.launch({
    headless: config.browser.headless,
    userDataDir: profilePath,
    args: [
      `--lang=${config.browser.locale}`,
      "--disable-background-networking",
      "--disable-breakpad",
      "--disable-component-update",
      "--disable-default-apps",
      "--disable-features=Translate,MediaRouter,OptimizationHints",
      "--disable-sync",
      "--force-color-profile=srgb",
      "--no-first-run",
      "--no-default-browser-check",
      ...softwareArgs
    ],
    defaultViewport: config.viewport
  });
}

async function captureSample(page, baseUrl, sample, config, currentPath) {
  const pageErrors = [];
  const failedRequests = [];
  const onPageError = error => pageErrors.push(error.message);
  const onRequestFailed = request => failedRequests.push(
    `${request.url()}: ${request.failure()?.errorText ?? "failed"}`
  );
  page.on("pageerror", onPageError);
  page.on("requestfailed", onRequestFailed);
  try {
    await navigateClientSide(page, baseUrl, sample, config);
    await page.evaluate(selector => document.querySelector(selector)?.remove(), config.capture.loadingSelector);
    await page.evaluate(async settledFrames => {
      if (document.fonts?.ready) await document.fonts.ready;
      for (let frame = 0; frame < settledFrames; frame++) {
        await new Promise(resolveFrame => requestAnimationFrame(() => resolveFrame()));
      }
    }, config.capture.settledFrames);
    const canonical = await page.evaluate(() => ({
      design: new URL(location.href).searchParams.get("design"),
      slug: location.hash.slice(1)
    }));
    if (canonical.slug !== sample.slug
        || canonical.design?.toLowerCase() !== sample.design.toLowerCase()) {
      throw new Error(`route did not canonicalize to ?design=${sample.design}#${sample.slug}: ${JSON.stringify(canonical)}`);
    }
    if (pageErrors.length) {
      throw new Error(`page error(s): ${pageErrors.join("; ")}`);
    }
    const screenshot = await captureSettledScreenshot(
      page,
      config.capture.settledFrames,
      config.capture.minContentPixels
    );
    await writeFile(currentPath, screenshot);
  } finally {
    page.off("pageerror", onPageError);
    page.off("requestfailed", onRequestFailed);
  }
}

async function openAppPage(browser, baseUrl, config) {
  const page = await browser.newPage();
  try {
    await page.emulateTimezone(config.browser.timezone);
    await page.emulateMediaFeatures([
      { name: "prefers-color-scheme", value: config.browser.colorScheme },
      { name: "prefers-reduced-motion", value: config.browser.reducedMotion }
    ]);
    const warmup = config.capture.warmup;
    await page.goto(
      `${baseUrl}?design=${encodeURIComponent(warmup.design)}#${encodeURIComponent(warmup.slug)}`,
      { waitUntil: "domcontentloaded", timeout: config.capture.timeoutMs }
    );
    await page.waitForFunction(
      prefix => performance.getEntriesByType("mark").some(entry => entry.name.startsWith(`${prefix}.`)),
      { timeout: config.capture.timeoutMs },
      config.capture.readyMark
    );
    await new Promise(resolveDelay => setTimeout(resolveDelay, warmup.delayMs));
    return page;
  } catch (error) {
    await page.close();
    throw error;
  }
}

async function navigateClientSide(page, baseUrl, sample, config) {
  const current = await page.evaluate(() => ({
    slug: location.hash.slice(1),
    design: new URL(location.href).searchParams.get("design")
  }));
  if (current.slug === sample.slug
      && current.design?.toLowerCase() === sample.design.toLowerCase()) {
    return;
  }
  if (current.slug === sample.slug) {
    const intermediate = sample.slug === "overview"
      ? { slug: "button", design: "Material" }
      : { slug: "overview", design: "Material" };
    await dispatchRoute(page, intermediate, config);
  }
  await dispatchRoute(page, sample, config);
}

async function dispatchRoute(page, route, config) {
  const count = await page.evaluate(
    prefix => performance.getEntriesByType("mark").filter(entry => entry.name.startsWith(`${prefix}.`)).length,
    config.capture.readyMark
  );
  await page.evaluate(({ slug, design }) => {
    const url = `?design=${encodeURIComponent(design)}#${encodeURIComponent(slug)}`;
    history.replaceState({ slug, design }, "", url);
    dispatchEvent(new PopStateEvent("popstate", { state: { slug, design } }));
  }, route);
  await page.waitForFunction(
    ({ prefix, count }) =>
      performance.getEntriesByType("mark").filter(entry => entry.name.startsWith(`${prefix}.`)).length > count,
    { timeout: config.capture.timeoutMs },
    { prefix: config.capture.readyMark, count }
  );
}

async function captureSettledScreenshot(page, settledFrames, minContentPixels) {
  const started = Date.now();
  let previousHash;
  let identicalCaptures = 0;
  let lastContentPixels = 0;
  for (let attempt = 0; attempt < 600; attempt++) {
    await page.evaluate(async frames => {
      for (let frame = 0; frame < frames; frame++) {
        await new Promise(resolveFrame => requestAnimationFrame(() => resolveFrame()));
      }
    }, settledFrames);
    const screenshot = await page.screenshot({ type: "png", fullPage: false });
    const image = PNG.sync.read(screenshot);
    let contentPixels = 0;
    for (let offset = 0; offset < image.data.length; offset += 4) {
      if (image.data[offset] < 245 || image.data[offset + 1] < 245 || image.data[offset + 2] < 245) {
        contentPixels++;
      }
    }
    lastContentPixels = contentPixels;
    if (contentPixels < minContentPixels) {
      identicalCaptures = 0;
      previousHash = undefined;
      await new Promise(resolveDelay => setTimeout(resolveDelay, 100));
      continue;
    }
    const hash = createHash("sha256").update(screenshot).digest("hex");
    identicalCaptures = hash === previousHash ? identicalCaptures + 1 : 1;
    if (identicalCaptures >= 3 && Date.now() - started >= 1500) {
      return screenshot;
    }
    previousHash = hash;
    await new Promise(resolveDelay => setTimeout(resolveDelay, 100));
  }
  throw new Error(`page pixels remained below ${minContentPixels} content pixels or unstable (${lastContentPixels})`);
}

export async function verifyBaselineMetadata(config, baselineDir, metadata, browserVersion, lockDigest, toolDigest) {
  const expectedIds = config.samples.map(sample => sample.id);
  const actualIds = metadata?.samples?.map(sample => sample.id);
  if (metadata?.schemaVersion !== 1 || metadata?.suiteVersion !== config.suiteVersion) {
    throw new Error("baseline metadata schema/suite is missing or stale");
  }
  if (metadata.configDigest !== digest(config) || metadata.lockDigest !== lockDigest
      || metadata.toolDigest !== toolDigest) {
    throw new Error("baseline metadata is stale for config, lockfile, or visual tool sources");
  }
  if (metadata.browserVersion !== browserVersion) {
    throw new Error(`baseline browser is stale: expected ${browserVersion}, metadata has ${metadata.browserVersion}`);
  }
  if (JSON.stringify(actualIds) !== JSON.stringify(expectedIds)) {
    throw new Error("baseline sample list/order is missing or stale");
  }
  const expectedPngs = expectedIds.map(id => `${id}.png`).sort();
  const actualPngs = (await readdir(baselineDir)).filter(name => name.endsWith(".png")).sort();
  if (JSON.stringify(actualPngs) !== JSON.stringify(expectedPngs)) {
    throw new Error(`baseline PNG set is missing or stale: expected ${expectedPngs.join(", ")}, found ${actualPngs.join(", ")}`);
  }
}

export async function runVisual({ mode, config, visualRoot, wasmRoot }) {
  const baselineDir = join(visualRoot, "baselines");
  const artifactDir = join(visualRoot, "artifacts");
  const currentDir = join(artifactDir, "current");
  const diffDir = join(artifactDir, "diff");
  const profileDir = join(artifactDir, "browser-profile");
  await rm(currentDir, { recursive: true, force: true });
  await rm(diffDir, { recursive: true, force: true });
  await mkdir(currentDir, { recursive: true });
  await mkdir(diffDir, { recursive: true });
  const lockDigest = digest(await readFile(join(visualRoot, "package-lock.json")));
  const toolDigest = await digestToolSources(join(visualRoot, "src"));

  const server = await startServer(resolve(wasmRoot));
  let browser;
  let page;
  try {
    browser = await launchBrowser(config, profileDir);
    const browserVersion = await browser.version();
    if (browserVersion !== config.browser.expectedVersion) {
      throw new Error(`browser version mismatch: expected ${config.browser.expectedVersion}, launched ${browserVersion}`);
    }
    if (mode === "compare") {
      const metadata = JSON.parse(await readFile(join(baselineDir, "metadata.json"), "utf8"));
      await verifyBaselineMetadata(config, baselineDir, metadata, browserVersion, lockDigest, toolDigest);
    }
    page = await openAppPage(browser, server.baseUrl, config);

    const results = [];
    for (const sample of config.samples) {
      const currentPath = join(currentDir, `${sample.id}.png`);
      try {
        await captureSample(page, server.baseUrl, sample, config, currentPath);
      } catch (error) {
        throw new Error(`${sample.id}: ${error.message}`, { cause: error });
      }
      if (mode === "update") {
        await mkdir(baselineDir, { recursive: true });
        await copyFile(currentPath, join(baselineDir, `${sample.id}.png`));
        results.push({ id: sample.id, passed: true, updated: true });
        continue;
      }
      const comparison = comparePngBuffers(
        await readFile(join(baselineDir, `${sample.id}.png`)),
        await readFile(currentPath),
        { ...config.comparison, masks: sample.masks }
      );
      await writeFile(join(diffDir, `${sample.id}.png`), comparison.diffBuffer);
      results.push({
        id: sample.id,
        passed: comparison.passed,
        differentPixels: comparison.differentPixels,
        diffRatio: comparison.diffRatio,
        reason: comparison.reason
      });
    }

    if (mode === "update") {
      await writeFile(join(baselineDir, "metadata.json"), JSON.stringify({
        schemaVersion: 1,
        suiteVersion: config.suiteVersion,
        generatedAtUtc: new Date().toISOString(),
        host: { os: process.platform, architecture: process.arch },
        browserVersion,
        viewport: config.viewport,
        renderer: "Skia-WASM / Chromium SwiftShader",
        fonts: config.fonts,
        configDigest: digest(config),
        lockDigest,
        toolDigest,
        samples: config.samples.map(({ id, slug, design, area, masks }) => ({
          id, slug, design, area, masks
        }))
      }, null, 2) + "\n");
    }
    return {
      passed: results.every(result => result.passed),
      mode,
      serverPid: server.pid,
      browserVersion,
      viewport: `${config.viewport.width}x${config.viewport.height}@${config.viewport.deviceScaleFactor}`,
      tolerance: config.comparison,
      results
    };
  } finally {
    await closeBounded(page);
    await closeBounded(browser);
    await server.stop();
    await rm(profileDir, { recursive: true, force: true });
  }
}

async function closeBounded(resource) {
  if (!resource) return;
  await Promise.race([
    resource.close().catch(() => undefined),
    new Promise(resolveTimeout => setTimeout(resolveTimeout, 10000))
  ]);
}

async function digestToolSources(sourceDirectory) {
  const names = (await readdir(sourceDirectory)).filter(name => name.endsWith(".mjs")).sort();
  const contents = await Promise.all(names.map(async name =>
    `${name}\n${await readFile(join(sourceDirectory, name), "utf8")}`
  ));
  return digest(contents.join("\n"));
}
