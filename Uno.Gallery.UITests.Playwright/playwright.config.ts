import { defineConfig, devices } from '@playwright/test';

// A persistent shell env override can redirect JUnit output to stale suite files
// (e.g., 03-ui-components-results.xml), causing uploads to mismatch current runs.
// Force the configured output path for deterministic uploads.
delete process.env.PLAYWRIGHT_JUNIT_OUTPUT_FILE;

const workersFromEnv = Number.parseInt(process.env.PW_WORKERS ?? '', 10);
const workerCount = Number.isFinite(workersFromEnv) && workersFromEnv > 0 ? workersFromEnv : undefined;

/**
 * Playwright configuration for Uno Gallery WASM tests.
 * Assumes the app is already running at http://localhost:5000.
 * Start it first with: dotnet run --framework net10.0-browserwasm
 */
export default defineConfig({
  testDir: './tests',

  // Run tests in parallel by default. PW_WORKERS can still be used to force
  // a specific worker count.
  fullyParallel: true,
  workers: workerCount,

  // Each test gets up to 5 min – cold WASM JIT cache builds on this machine
  // can exceed 3 min when Chrome is under memory pressure mid-suite.
  timeout: 300_000,

  // Each expect() assertion gets up to 30 s before failing.
  // This prevents flakiness caused by the WASM accessibility tree being
  // populated slightly after gotoApp() returns.
  expect: { timeout: 30_000 },
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 1,

  reporter: [
    ['html', { outputFolder: 'playwright-report', open: 'never' }],
    ['junit', { outputFile: 'results/junit/playwright-results.xml' }],
    ['list'],
    ['./testquality/upload-reporter.mjs'],
  ],

  use: {
    baseURL: 'http://localhost:5000',

    // Keep traces / screenshots for debugging failures
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'on-first-retry',

    // Uno WASM controls can be slow to respond
    actionTimeout: 15_000,
    navigationTimeout: 30_000,
  },

  projects: [
    {
      name: 'chrome',
      use: {
        ...devices['Desktop Chrome'],
        // Use a large viewport so the full sidebar is visible
        viewport: { width: 1440, height: 900 },
      },
    },
  ],
});
