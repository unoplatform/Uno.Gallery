import { Page, expect } from '@playwright/test';

/** Maximum time to wait for the Uno WASM runtime to fully initialize.
 * Headed mode cold-starts can take >90 s on the first page load because the
 * browser is building the WASM JIT cache from scratch.  230 s gives enough
 * headroom (below the 240 s test timeout) while still catching genuine hangs.
 */
export const WASM_INIT_TIMEOUT = 230_000;

/** Standard timeout for a navigation action / element appearance. */
export const NAV_TIMEOUT = 20_000;

/** Disables ShowMeTheXAML Hot Design at app startup for stable Playwright runs. */
const APP_START_QUERY = '?hotdesign=0';

/**
 * Navigates to the gallery root and waits for the Uno WASM runtime to be
 * ready. The sidebar "Overview" item being visible is used as the readiness
 * signal.
 */
export async function gotoApp(page: Page): Promise<void> {
  // Propagate the WASM budget to every page-level operation so that
  // individual expect() / waitFor calls don't cap out at the Playwright
  // default (5 s) before the WASM runtime has finished bootstrapping.
  page.setDefaultTimeout(WASM_INIT_TIMEOUT);
  page.setDefaultNavigationTimeout(WASM_INIT_TIMEOUT);

  await page.goto(`/${APP_START_QUERY}`);
  // 'networkidle' typically fires within 5-10 s for a localhost WASM app
  // because the WASM bundle downloads quickly.  It is a reliable signal that
  // the Uno runtime has finished bootstrapping its initial downloads.
  await page.waitForLoadState('networkidle', { timeout: WASM_INIT_TIMEOUT });

  // Give the WASM bootstrap a moment to render the accessibility button into
  // the DOM after the network goes quiet.
  await page.waitForTimeout(500);

  // HEADED-MODE FIX: In cache-warm headed Chrome, the Uno WASM bootstraps
  // at the full 1440×900 dimension immediately — no layout "delta" fires —
  // so NavigationView.SizeChanged never runs and the pane stays in its XAML
  // initial state (LeftMinimal / collapsed). A viewport resize cycle forces
  // Uno to re-measure every control, which triggers SizeChanged, which sets
  // PaneDisplayMode=Left before the accessibility tree is built.
  // This is a no-op-safe change: headless mode handles the extra resize
  // gracefully and the count stays at 643.
  await page.setViewportSize({ width: 1441, height: 900 });
  await page.waitForTimeout(300);
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.waitForTimeout(300);

  // Uno Platform WASM (Skia renderer) renders to a <canvas> and exposes an
  // "Enable accessibility" button on first load. Clicking it builds the
  // parallel ARIA/DOM tree that Playwright's locators depend on.
  //
  // The button can be off-screen (opacity 0 / zero-size) so Playwright's
  // isVisible() returns false in a fresh browser context — especially in
  // headed mode.  We wait for it to be *attached* to the DOM, then click via
  // JS to bypass all viewport / visibility checks.  If it never appears
  // (accessibility already enabled), the catch() is a safe no-op.
  await page
    .locator('#uno-enable-accessibility')
    .waitFor({ state: 'attached', timeout: WASM_INIT_TIMEOUT })
    .then(() =>
      page.evaluate(() => {
        const btn = document.querySelector('#uno-enable-accessibility') as HTMLElement | null;
        if (btn) btn.click();
      }),
    )
    .catch(() => {
      // Button absent → accessibility already active; safe to continue.
    });

  // Give the WASM ARIA tree a moment to build after the accessibility click.
  await page.waitForTimeout(500);

  // Wait until the sidebar "Overview" nav item is attached to the
  // accessibility tree. This is the true readiness signal: the content area
  // has many aria-labels long before the sidebar nav is built, so a plain
  // element-count check fires too early and causes flakiness on the first run.
  await page
    .locator('[aria-label="Overview"]')
    .first()
    .waitFor({ state: 'attached', timeout: WASM_INIT_TIMEOUT });

  // Restore to the standard per-action timeout now that the app is up.
  page.setDefaultTimeout(NAV_TIMEOUT);
  page.setDefaultNavigationTimeout(NAV_TIMEOUT);
}

/**
 * Expands a top-level sidebar category (e.g. "Theming", "UI components").
 * Uno WASM surfaces text labels via aria-label, not DOM textContent.
 */
export async function expandCategory(
  page: Page,
  category: string,
): Promise<void> {
  const categoryItem = page.locator(`[aria-label="${category}"]`).first();
  await expect(categoryItem).toBeVisible({ timeout: NAV_TIMEOUT });
  await categoryItem.click();
  await page.waitForTimeout(600);
}

/**
 * Navigates directly to a named sample page using URL hash navigation.
 *
 * Uno Gallery's `IsThereSampleFilteredByArgs()` reads `window.location.hash`
 * at WASM startup, URL-decodes it, and navigates to the first
 * NavigationViewItem whose Content contains the decoded search term
 * (case-insensitive).  This is the only reliable navigation mechanism for
 * tests: sidebar sub-items and the search suggestion popup are never exposed
 * in the Uno WASM HTML accessibility tree.
 *
 * The `category` parameter is kept for documentation/readability only.
 */
export async function navigateToSample(
  page: Page,
  category: string,
  sampleName: string,
): Promise<void> {
  page.setDefaultTimeout(WASM_INIT_TIMEOUT);
  page.setDefaultNavigationTimeout(WASM_INIT_TIMEOUT);

  // Navigate via URL hash so IsThereSampleFilteredByArgs picks it up on boot.
  // encodeURIComponent turns spaces to %20; the C# side calls
  // Uri.UnescapeDataString before the Contains search so multi-word names work.
  //
  // Use 'domcontentloaded' instead of the default 'load' / 'networkidle' so the
  // WASM bundle download time is counted inside the StickyTitle waitFor window
  // below (which has the full WASM_INIT_TIMEOUT budget), not as overhead before it.
  await page.goto(`/${APP_START_QUERY}#${encodeURIComponent(sampleName)}`, { waitUntil: 'domcontentloaded' });
  await page.waitForTimeout(500);

  // Viewport resize forces NavigationView.SizeChanged → PaneDisplayMode=Left.
  // Identical to gotoApp's fix: without it the pane stays in LeftMinimal and
  // the accessibility tree is built in the wrong layout.
  await page.setViewportSize({ width: 1441, height: 900 });
  await page.waitForTimeout(300);
  await page.setViewportSize({ width: 1440, height: 900 });
  await page.waitForTimeout(300);

  // Enable Uno's ARIA/DOM accessibility tree (Skia canvas renderer).
  await page
    .locator('#uno-enable-accessibility')
    .waitFor({ state: 'attached', timeout: WASM_INIT_TIMEOUT })
    .then(() =>
      page.evaluate(() => {
        const btn = document.querySelector('#uno-enable-accessibility') as HTMLElement | null;
        if (btn) btn.click();
      }),
    )
    .catch(() => {
      // Button absent → accessibility already active; safe to continue.
    });
  await page.waitForTimeout(500);

  // StickyTitle is the Uno AutomationId for the sample page header container.
  // Its presence confirms the sample page loaded (not the Overview default).
  // We wait for 'visible' (not just 'attached') because StickyTitle is
  // Visibility.Collapsed in the Mobile VisualState; 'attached' would return
  // immediately even when the header is CSS-hidden.
  await page
    .locator('[aria-label="StickyTitle"]')
    .first()
    .waitFor({ state: 'visible', timeout: WASM_INIT_TIMEOUT });

  page.setDefaultTimeout(NAV_TIMEOUT);
  page.setDefaultNavigationTimeout(NAV_TIMEOUT);
}

/**
 * Clicks the "Overview" top-level navigation item.
 */
export async function navigateToOverview(page: Page): Promise<void> {
  await page.locator('[aria-label="Overview"]').first().click();
  await page.waitForTimeout(600);
}

/**
 * Asserts that a sample page has loaded by confirming the StickyTitle header
 * is visible.  `navigateToSample` already waits for StickyTitle to be visible,
 * so this is a fast confirmation check (effectively instant).
 *
 * The `title` parameter is kept for call-site readability only.
 */
export async function expectSampleHeadingVisible(
  page: Page,
  title: string,
): Promise<void> {
  // `navigateToSample` waits for StickyTitle to be visible before returning,
  // so this assertion should resolve immediately on any sample page.
  await expect(
    page.locator('[aria-label="StickyTitle"]').first(),
  ).toBeVisible({ timeout: NAV_TIMEOUT });
}
