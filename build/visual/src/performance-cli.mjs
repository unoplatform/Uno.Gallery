import { fork } from "node:child_process";
import { access, mkdir, readFile, rm, writeFile } from "node:fs/promises";
import { arch, platform, release, tmpdir } from "node:os";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import puppeteer from "puppeteer";
import { round, summarizeRuns } from "./performance-metrics.mjs";

const sourceDirectory = dirname(fileURLToPath(import.meta.url));

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
  for (const required of ["wasm", "config", "output"]) {
    if (!parsed[required]) {
      throw new Error(`Missing required --${required} argument.`);
    }
  }
  return parsed;
}

function validateConfig(config) {
  if (config?.schemaVersion !== 1 || !Number.isInteger(config.suiteVersion)) {
    throw new Error("Performance configuration must use schemaVersion 1 and an integer suiteVersion.");
  }
  for (const profile of ["cold", "warm"]) {
    if (!Number.isInteger(config.runs?.[profile]) || config.runs[profile] < 1) {
      throw new Error(`Performance configuration requires a positive ${profile} run count.`);
    }
  }
  for (const name of ["shellReady", "firstInput", "searchRendered", "navigationRendered"]) {
    if (!config.marks?.[name]) {
      throw new Error(`Performance configuration is missing marks.${name}.`);
    }
  }
}

async function startServer(wasmRoot) {
  const serverPath = join(sourceDirectory, "performance-server.mjs");
  const child = fork(serverPath, [wasmRoot, "0"], {
    stdio: ["ignore", "inherit", "inherit", "ipc"]
  });
  let ready;
  try {
    ready = await new Promise((resolveReady, reject) => {
      const timer = setTimeout(
        () => settle(reject, new Error("performance server did not start within 30 seconds")),
        30000
      );
      timer.unref();
      const onMessage = message => {
        if (message?.type === "ready") settle(resolveReady, message);
      };
      const onError = error => settle(reject, error);
      const onExit = code => settle(reject, new Error(`performance server exited before readiness (${code})`));
      const settle = (callback, value) => {
        clearTimeout(timer);
        child.off("message", onMessage);
        child.off("error", onError);
        child.off("exit", onExit);
        callback(value);
      };
      child.on("message", onMessage);
      child.once("error", onError);
      child.once("exit", onExit);
    });
  } catch (error) {
    child.kill("SIGKILL");
    await waitForExit(child, 5000);
    throw error;
  }
  return {
    baseUrl: `http://127.0.0.1:${ready.port}/`,
    async stop() {
      if (child.exitCode !== null) return;
      child.send({ type: "stop" });
      if (!await waitForExit(child, 5000)) {
        child.kill("SIGKILL");
        await waitForExit(child, 5000);
      }
    }
  };
}

async function waitForExit(child, timeoutMs) {
  if (child.exitCode !== null) return true;
  return new Promise(resolveWait => {
    const timer = setTimeout(() => settle(false), timeoutMs);
    timer.unref();
    const onExit = () => settle(true);
    const settle = result => {
      clearTimeout(timer);
      child.off("exit", onExit);
      resolveWait(result);
    };
    child.once("exit", onExit);
  });
}

async function removeDirectory(path) {
  for (let attempt = 1; attempt <= 5; attempt++) {
    try {
      await rm(path, { recursive: true, force: true, maxRetries: 3, retryDelay: 200 });
      return;
    } catch (error) {
      if (attempt === 5) throw error;
      await new Promise(resolveWait => setTimeout(resolveWait, attempt * 500));
    }
  }
}

async function withTimeout(promise, timeoutMs, message) {
  let timer;
  try {
    return await Promise.race([
      promise,
      new Promise((_, reject) => {
        timer = setTimeout(() => reject(new Error(message)), timeoutMs);
        timer.unref();
      })
    ]);
  } finally {
    clearTimeout(timer);
  }
}

async function closeBrowser(browser) {
  const child = browser.process();
  try {
    await withTimeout(browser.close(), 10000, "browser did not close within 10 seconds");
  } catch {
    child?.kill("SIGKILL");
    if (child) {
      await waitForExit(child, 5000);
    }
  }
}

async function launchBrowser(config, profilePath) {
  await removeDirectory(profilePath);
  await mkdir(profilePath, { recursive: true });
  const softwareArguments = config.browser.softwareRendering
    ? ["--use-angle=swiftshader", "--enable-unsafe-swiftshader", "--disable-gpu-sandbox"]
    : [];
  const browser = await puppeteer.launch({
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
      "--no-default-browser-check",
      "--no-first-run",
      ...softwareArguments
    ],
    defaultViewport: config.viewport
  });
  const version = await browser.version();
  if (version !== config.browser.expectedVersion) {
    await closeBrowser(browser);
    throw new Error(`Browser version mismatch: expected ${config.browser.expectedVersion}, launched ${version}.`);
  }
  return browser;
}

function monitorPage(page, baseUrl) {
  const origin = new URL(baseUrl).origin;
  const failures = [];
  const isLocal = value => {
    try {
      return new URL(value).origin === origin;
    } catch {
      return false;
    }
  };
  page.on("pageerror", error => failures.push(error.message));
  page.on("requestfailed", request => {
    if (isLocal(request.url())) {
      failures.push(`${request.url()}: ${request.failure()?.errorText ?? "failed"}`);
    }
  });
  page.on("response", response => {
    if (isLocal(response.url()) && response.status() >= 400) {
      failures.push(`${response.url()}: HTTP ${response.status()}`);
    }
  });
  return {
    assertClean(context) {
      if (failures.length > 0) {
        throw new Error(`${context} browser failure(s): ${failures.join("; ")}`);
      }
    }
  };
}

async function waitForEntry(page, entryType, name, timeoutMs) {
  await page.waitForFunction(
    ({ entryType, name }) => performance.getEntriesByType(entryType).some(entry => entry.name === name),
    { timeout: timeoutMs },
    { entryType, name }
  );
  return page.evaluate(
    ({ entryType, name }) => {
      const entries = performance.getEntriesByType(entryType).filter(entry => entry.name === name);
      const entry = entries[entries.length - 1];
      return { startTime: entry.startTime, duration: entry.duration };
    },
    { entryType, name }
  );
}

async function observeRun(browser, baseUrl, config, cacheEnabled) {
  const page = await browser.newPage();
  const monitor = monitorPage(page, baseUrl);
  try {
    await page.emulateTimezone(config.browser.timezone);
    await page.emulateMediaFeatures([
      { name: "prefers-color-scheme", value: config.browser.colorScheme },
      { name: "prefers-reduced-motion", value: config.browser.reducedMotion }
    ]);
    await page.setCacheEnabled(cacheEnabled);
    await page.goto(baseUrl, { waitUntil: "domcontentloaded", timeout: config.timeouts.startupMs });
    const shellReady = await waitForEntry(
      page,
      "mark",
      config.marks.shellReady,
      config.timeouts.startupMs
    );
    const firstContentfulPaint = await waitForEntry(
      page,
      "paint",
      "first-contentful-paint",
      config.timeouts.startupMs
    );

    const searchInput = await page.waitForSelector(
      config.actions.searchSelector,
      { timeout: config.timeouts.actionMs, visible: true }
    );
    const observesFirstInput = await page.evaluate(() => {
      if (!globalThis.PerformanceObserver?.supportedEntryTypes?.includes("first-input")) {
        return false;
      }
      globalThis.__unoGalleryFirstInputDelay = null;
      globalThis.__unoGalleryFirstInputObserver = new PerformanceObserver(list => {
        const entry = list.getEntries()[0];
        globalThis.__unoGalleryFirstInputDelay =
          Math.max(0, entry.processingStart - entry.startTime);
      });
      globalThis.__unoGalleryFirstInputObserver.observe({ type: "first-input", buffered: true });
      return true;
    });
    const clickStarted = await page.evaluate(() => performance.now());
    await searchInput.click();
    const firstInput = await waitForEntry(
      page,
      "mark",
      config.marks.firstInput,
      config.timeouts.actionMs
    );
    if (observesFirstInput) {
      await page.waitForFunction(
        () => globalThis.__unoGalleryFirstInputDelay !== null,
        { timeout: config.timeouts.actionMs }
      );
    }
    const firstInputEventDelay = observesFirstInput
      ? await page.evaluate(() => globalThis.__unoGalleryFirstInputDelay)
      : null;

    await page.evaluate(name => performance.clearMeasures(name), config.marks.searchRendered);
    await searchInput.type(config.actions.searchQuery);
    const search = await waitForEntry(
      page,
      "measure",
      config.marks.searchRendered,
      config.timeouts.actionMs
    );
    await page.evaluate(() => new Promise(resolveFrame =>
      requestAnimationFrame(() => requestAnimationFrame(resolveFrame))));
    const latestSearch = await page.evaluate(name => {
      const entries = performance.getEntriesByName(name, "measure");
      return entries[entries.length - 1].duration;
    }, config.marks.searchRendered);

    await page.evaluate(name => performance.clearMeasures(name), config.marks.navigationRendered);
    await page.evaluate(({ slug, design }) => {
      const url = `?design=${encodeURIComponent(design)}#${encodeURIComponent(slug)}`;
      history.replaceState({ slug, design }, "", url);
      dispatchEvent(new PopStateEvent("popstate", { state: { slug, design } }));
    }, {
      slug: config.actions.navigationSlug,
      design: config.actions.navigationDesign
    });
    const navigation = await waitForEntry(
      page,
      "measure",
      config.marks.navigationRendered,
      config.timeouts.actionMs
    );
    monitor.assertClean("performance run");

    return {
      firstContentfulPaintMs: round(firstContentfulPaint.startTime),
      shellReadyMs: round(shellReady.startTime),
      firstInputLatencyMs: round(
        firstInputEventDelay ?? Math.max(0, firstInput.startTime - clickStarted)
      ),
      searchRenderedMs: round(latestSearch ?? search.duration),
      navigationRenderedMs: round(navigation.duration)
    };
  } finally {
    await withTimeout(page.close(), 5000, "performance page did not close within 5 seconds");
  }
}

async function collectRuns(baseUrl, config, profileRoot) {
  const cold = [];
  let browserVersion;
  for (let index = 0; index < config.runs.cold; index++) {
    const profilePath = join(profileRoot, `cold-${index}`);
    const browser = await launchBrowser(config, profilePath);
    browserVersion ??= await browser.version();
    try {
      cold.push(await observeRun(browser, baseUrl, config, false));
    } finally {
      await closeBrowser(browser);
    }
    console.log(`Cold run ${index + 1}/${config.runs.cold}: ${JSON.stringify(cold[cold.length - 1])}`);
  }

  const warmProfile = join(profileRoot, "warm");
  const browser = await launchBrowser(config, warmProfile);
  browserVersion ??= await browser.version();
  const warm = [];
  try {
    await observeRun(browser, baseUrl, config, true);
    for (let index = 0; index < config.runs.warm; index++) {
      warm.push(await observeRun(browser, baseUrl, config, true));
      console.log(`Warm run ${index + 1}/${config.runs.warm}: ${JSON.stringify(warm[warm.length - 1])}`);
    }
  } finally {
    await closeBrowser(browser);
  }
  return { cold, warm, browserVersion };
}

async function main() {
  const args = parseArguments(process.argv.slice(2));
  const wasmRoot = resolve(args.wasm);
  const configPath = resolve(args.config);
  const outputPath = resolve(args.output);
  await access(wasmRoot);
  const config = JSON.parse(await readFile(configPath, "utf8"));
  validateConfig(config);
  if (args.cold) config.runs.cold = Number(args.cold);
  if (args.warm) config.runs.warm = Number(args.warm);
  validateConfig(config);
  const buildCommit = args.commit?.toLowerCase() ?? null;
  if (buildCommit !== null && !/^[0-9a-f]{7,40}$/.test(buildCommit)) {
    throw new Error("--commit must be a 7-40 character hexadecimal revision.");
  }

  const profileRoot = join(tmpdir(), `uno-gallery-performance-${process.pid}`);
  const server = await startServer(wasmRoot);
  try {
    const { cold, warm, browserVersion } = await collectRuns(server.baseUrl, config, profileRoot);
    const report = {
      schemaVersion: 1,
      suiteVersion: config.suiteVersion,
      generatedAt: new Date().toISOString(),
      buildCommit,
      target: config.target,
      browser: {
        version: browserVersion,
        headless: config.browser.headless,
        softwareRendering: config.browser.softwareRendering,
        locale: config.browser.locale,
        timezone: config.browser.timezone
      },
      host: {
        platform: platform(),
        release: release(),
        architecture: arch(),
        nodeVersion: process.version
      },
      runs: { cold, warm },
      summaries: {
        cold: summarizeRuns(cold),
        warm: summarizeRuns(warm)
      }
    };
    await mkdir(dirname(outputPath), { recursive: true });
    await writeFile(outputPath, `${JSON.stringify(report, null, 2)}\n`, "utf8");
    console.log(`Runtime performance observations written to ${outputPath}`);
  } finally {
    await server.stop();
    await removeDirectory(profileRoot);
  }
}

main().catch(error => {
  console.error(error.stack ?? error);
  process.exitCode = 1;
});
