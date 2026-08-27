import { fork } from "node:child_process";
import { createHash } from "node:crypto";
import { access, readFile, readdir, rm, mkdir, writeFile, copyFile, rename } from "node:fs/promises";
import { basename, dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import puppeteer from "puppeteer";
import { PNG } from "pngjs";
import { comparePngBuffers } from "./comparator.mjs";
import { digest } from "./config.mjs";

const moduleDirectory = dirname(fileURLToPath(import.meta.url));

async function startServer(wasmRoot) {
  const serverPath = join(moduleDirectory, "server.mjs");
  const child = fork(serverPath, [wasmRoot, "0"], { stdio: ["ignore", "inherit", "inherit", "ipc"] });
  let ready;
  try {
    ready = await new Promise((resolveReady, reject) => {
      const onError = error => settle(reject, error);
      const onExit = code => settle(reject, new Error(`visual server exited before readiness (${code})`));
      const onMessage = message => {
        if (message?.type === "ready") settle(resolveReady, message);
      };
      const timer = setTimeout(
        () => settle(reject, new Error("visual server did not start within 30 seconds")),
        30000
      );
      timer.unref();
      const settle = (callback, value) => {
        clearTimeout(timer);
        child.off("error", onError);
        child.off("exit", onExit);
        child.off("message", onMessage);
        callback(value);
      };
      child.once("error", onError);
      child.once("exit", onExit);
      child.on("message", onMessage);
    });
  } catch (error) {
    await terminateChild(child);
    throw error;
  }
  return {
    baseUrl: `http://127.0.0.1:${ready.port}/`,
    pid: ready.pid,
    async stop() {
      if (child.exitCode !== null) return;
      child.send({ type: "stop" });
      if (!await waitForExit(child, 5000)) {
        await terminateChild(child);
      }
    }
  };
}

async function waitForExit(child, timeoutMs) {
  if (child.exitCode !== null) return true;
  return new Promise(resolveWait => {
    const onExit = () => settle(true);
    const timer = setTimeout(() => settle(false), timeoutMs);
    timer.unref();
    const settle = result => {
      clearTimeout(timer);
      child.off("exit", onExit);
      resolveWait(result);
    };
    child.once("exit", onExit);
  });
}

async function terminateChild(child) {
  if (child.exitCode !== null) return;
  child.kill("SIGKILL");
  await waitForExit(child, 5000);
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

function monitorPage(page, baseUrl) {
  const origin = new URL(baseUrl).origin;
  const errors = [];
  const failures = [];
  const isLocal = value => {
    try { return new URL(value).origin === origin; } catch { return false; }
  };
  const onPageError = error => errors.push(error.message);
  const onRequestFailed = request => {
    if (isLocal(request.url())) {
      failures.push(`${request.url()}: ${request.failure()?.errorText ?? "failed"}`);
    }
  };
  const onResponse = response => {
    if (isLocal(response.url()) && response.status() >= 400) {
      failures.push(`${response.url()}: HTTP ${response.status()}`);
    }
  };
  page.on("pageerror", onPageError);
  page.on("requestfailed", onRequestFailed);
  page.on("response", onResponse);
  return {
    reset() {
      errors.length = 0;
      failures.length = 0;
    },
    assertClean(context) {
      if (errors.length || failures.length) {
        throw new Error(
          `${context} browser failure(s): ${[...errors, ...failures].join("; ")}`
        );
      }
    },
    dispose() {
      page.off("pageerror", onPageError);
      page.off("requestfailed", onRequestFailed);
      page.off("response", onResponse);
    }
  };
}

async function captureSample(page, monitor, baseUrl, sample, config, currentPath) {
  monitor.reset();
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
    const screenshot = await captureSettledScreenshot(
      page,
      config.capture.settledFrames,
      config.capture.minContentPixels
    );
    monitor.assertClean(sample.id);
    await writeFile(currentPath, screenshot);
  } catch (error) {
    monitor.assertClean(sample.id);
    throw error;
  }
}

async function openAppPage(browser, baseUrl, config) {
  const page = await browser.newPage();
  const monitor = monitorPage(page, baseUrl);
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
    await page.evaluate(async () => {
      if (document.fonts?.ready) await document.fonts.ready;
    });
    monitor.assertClean("warmup");
    monitor.reset();
    return { page, monitor };
  } catch (error) {
    monitor.dispose();
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

export async function verifyBaselineMetadata(config, baselineDir, metadata, runtime, lockDigest, toolDigest) {
  const expectedIds = config.samples.map(sample => sample.id);
  const actualIds = metadata?.samples?.map(sample => sample.id);
  if (metadata?.schemaVersion !== 1 || metadata?.suiteVersion !== config.suiteVersion) {
    throw new Error("baseline metadata schema/suite is missing or stale");
  }
  if (metadata.configDigest !== digest(config) || metadata.lockDigest !== lockDigest
      || metadata.toolDigest !== toolDigest) {
    throw new Error("baseline metadata is stale for config, lockfile, or visual tool sources");
  }
  if (metadata.browserVersion !== runtime.browserVersion) {
    throw new Error(`baseline browser is stale: expected ${runtime.browserVersion}, metadata has ${metadata.browserVersion}`);
  }
  if (metadata.browserExecutable !== runtime.browserExecutable
      || metadata.renderer !== runtime.renderer
      || metadata.host?.os !== runtime.host.os
      || metadata.host?.architecture !== runtime.host.architecture) {
    throw new Error("baseline host, browser executable, or renderer provenance is stale");
  }
  if (JSON.stringify(actualIds) !== JSON.stringify(expectedIds)) {
    throw new Error("baseline sample list/order is missing or stale");
  }
  const expectedPngs = expectedIds.map(id => `${id}.png`).sort();
  const actualPngs = (await readdir(baselineDir)).filter(name => name.endsWith(".png")).sort();
  if (JSON.stringify(actualPngs) !== JSON.stringify(expectedPngs)) {
    throw new Error(`baseline PNG set is missing or stale: expected ${expectedPngs.join(", ")}, found ${actualPngs.join(", ")}`);
  }
  for (const sample of metadata.samples) {
    const imagePath = join(baselineDir, `${sample.id}.png`);
    const actualHash = createHash("sha256").update(await readFile(imagePath)).digest("hex");
    if (!/^[0-9a-f]{64}$/.test(sample.sha256 ?? "") || sample.sha256 !== actualHash) {
      throw new Error(`baseline PNG hash is missing or stale for ${sample.id}`);
    }
  }
}

export async function runVisual({ mode, config, visualRoot, wasmRoot }) {
  const baselineDir = join(visualRoot, "baselines");
  const baselineStagingDir = join(visualRoot, ".baselines-staging");
  const baselineBackupDir = join(visualRoot, ".baselines-backup");
  const artifactDir = join(visualRoot, "artifacts");
  const currentDir = join(artifactDir, "current");
  const diffDir = join(artifactDir, "diff");
  const profileDir = join(artifactDir, "browser-profile");
  await recoverBaselineSet(baselineDir, baselineBackupDir);
  await rm(currentDir, { recursive: true, force: true });
  await rm(diffDir, { recursive: true, force: true });
  await rm(baselineStagingDir, { recursive: true, force: true });
  await mkdir(currentDir, { recursive: true });
  await mkdir(diffDir, { recursive: true });
  if (mode === "update") await mkdir(baselineStagingDir, { recursive: true });
  const lockDigest = digest(normalizeText(await readFile(join(visualRoot, "package-lock.json"), "utf8")));
  const toolDigest = await digestToolSources(join(visualRoot, "src"));

  const server = await startServer(resolve(wasmRoot));
  let browser;
  let page;
  let monitor;
  try {
    browser = await launchBrowser(config, profileDir);
    const browserVersion = await browser.version();
    if (browserVersion !== config.browser.expectedVersion) {
      throw new Error(`browser version mismatch: expected ${config.browser.expectedVersion}, launched ${browserVersion}`);
    }
    const browserExecutable = basename(browser.process()?.spawnfile ?? "");
    if (!/^chrome(?:\.exe)?$/i.test(browserExecutable)) {
      throw new Error(`unexpected browser executable '${browserExecutable}'`);
    }
    if (mode === "compare") {
      // Provenance is verified after the page is open and the actual WebGL renderer is observed.
    }
    ({ page, monitor } = await openAppPage(browser, server.baseUrl, config));
    const renderer = await observeRenderer(page);
    if (config.browser.softwareRendering && !renderer.toLowerCase().includes("swiftshader")) {
      throw new Error(`software rendering was requested but Chromium reported '${renderer}'`);
    }
    const runtime = {
      browserVersion,
      browserExecutable,
      renderer,
      host: { os: process.platform, architecture: process.arch }
    };
    if (mode === "compare") {
      const metadata = JSON.parse(await readFile(join(baselineDir, "metadata.json"), "utf8"));
      await verifyBaselineMetadata(config, baselineDir, metadata, runtime, lockDigest, toolDigest);
    }

    const results = [];
    for (const sample of config.samples) {
      const currentPath = join(currentDir, `${sample.id}.png`);
      try {
        await captureSample(page, monitor, server.baseUrl, sample, config, currentPath);
      } catch (error) {
        throw new Error(`${sample.id}: ${error.message}`, { cause: error });
      }
      if (mode === "update") {
        await copyFile(currentPath, join(baselineStagingDir, `${sample.id}.png`));
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
      const samples = [];
      for (const { id, slug, design, area, masks } of config.samples) {
        const image = await readFile(join(baselineStagingDir, `${id}.png`));
        samples.push({
          id, slug, design, area, masks,
          sha256: createHash("sha256").update(image).digest("hex")
        });
      }
      const metadata = {
        schemaVersion: 1,
        suiteVersion: config.suiteVersion,
        generatedAtUtc: new Date().toISOString(),
        host: runtime.host,
        browserVersion,
        browserExecutable,
        viewport: config.viewport,
        renderer,
        fonts: config.fonts,
        configDigest: digest(config),
        lockDigest,
        toolDigest,
        samples
      };
      await writeFile(
        join(baselineStagingDir, "metadata.json"),
        JSON.stringify(metadata, null, 2) + "\n"
      );
      await verifyBaselineMetadata(
        config,
        baselineStagingDir,
        metadata,
        runtime,
        lockDigest,
        toolDigest
      );
      await replaceBaselineSet(baselineDir, baselineStagingDir, baselineBackupDir);
    }
    return {
      passed: results.every(result => result.passed),
      mode,
      serverPid: server.pid,
      browserVersion,
      browserExecutable,
      renderer,
      host: runtime.host,
      viewport: `${config.viewport.width}x${config.viewport.height}@${config.viewport.deviceScaleFactor}`,
      tolerance: config.comparison,
      results
    };
  } finally {
    monitor?.dispose();
    await closeBounded(page);
    await closeBounded(browser);
    await server.stop();
    await rm(profileDir, { recursive: true, force: true });
    await rm(baselineStagingDir, { recursive: true, force: true });
    await recoverBaselineSet(baselineDir, baselineBackupDir);
  }
}

async function closeBounded(resource) {
  if (!resource) return;
  let timer;
  const closed = await Promise.race([
    resource.close().then(() => true).catch(() => true),
    new Promise(resolveTimeout => {
      timer = setTimeout(() => resolveTimeout(false), 10000);
      timer.unref();
    })
  ]);
  clearTimeout(timer);
  if (!closed && typeof resource.process === "function") {
    const process = resource.process();
    if (process) await terminateChild(process);
  }
}

async function digestToolSources(sourceDirectory) {
  const names = (await readdir(sourceDirectory)).filter(name => name.endsWith(".mjs")).sort();
  const contents = await Promise.all(names.map(async name =>
    `${name}\n${normalizeText(await readFile(join(sourceDirectory, name), "utf8"))}`
  ));
  return digest(contents.join("\n"));
}

function normalizeText(value) {
  return value.replace(/\r\n?/g, "\n");
}

async function observeRenderer(page) {
  return page.evaluate(() => {
    const canvas = document.createElement("canvas");
    const context = canvas.getContext("webgl") ?? canvas.getContext("experimental-webgl");
    if (!context) return "WebGL unavailable";
    const extension = context.getExtension("WEBGL_debug_renderer_info");
    return extension
      ? context.getParameter(extension.UNMASKED_RENDERER_WEBGL)
      : context.getParameter(context.RENDERER);
  });
}

export async function replaceBaselineSet(baselineDir, stagingDir, backupDir) {
  let hadBaseline = true;
  try {
    await rename(baselineDir, backupDir);
  } catch (error) {
    if (error.code !== "ENOENT") throw error;
    hadBaseline = false;
  }

  try {
    await rename(stagingDir, baselineDir);
    if (hadBaseline) await rm(backupDir, { recursive: true, force: true });
  } catch (error) {
    if (hadBaseline) {
      await rm(baselineDir, { recursive: true, force: true });
      await rename(backupDir, baselineDir);
    }
    throw error;
  }
}

export async function recoverBaselineSet(baselineDir, backupDir) {
  const baselineExists = await pathExists(baselineDir);
  const backupExists = await pathExists(backupDir);
  if (!baselineExists && backupExists) {
    await rename(backupDir, baselineDir);
  } else if (baselineExists && backupExists) {
    await rm(backupDir, { recursive: true, force: true });
  }
}

async function pathExists(path) {
  try {
    await access(path);
    return true;
  } catch {
    return false;
  }
}
