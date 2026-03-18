// Probe: full ARIA label dump + canvas rect comparison
import { chromium } from "@playwright/test";

for (const headless of [true, false]) {
  console.log(`\n========== headless: ${headless} ==========`);
  const b = await chromium.launch({ headless });
  const ctx = await b.newContext({
    viewport: { width: 1440, height: 900 },
    screen: { width: 1440, height: 900 },
    deviceScaleFactor: 1,
  });
  const p = await ctx.newPage();

  const t0 = Date.now();
  const elapsed = () => `${((Date.now()-t0)/1000).toFixed(1)}s`;

  console.log(`[${elapsed()}] goto`);
  await p.goto("http://localhost:5000");
  console.log(`[${elapsed()}] domcontentloaded`);

  // Race networkidle vs 20s timeout
  const niResult = await Promise.race([
    p.waitForLoadState("networkidle", { timeout: 25_000 }).then(() => 'OK'),
    new Promise(r => setTimeout(() => r('TIMEOUT-20s'), 20_000)),
  ]);
  console.log(`[${elapsed()}] networkidle: ${niResult}`);

  // Bring the page to the foreground so rAF fires in headed mode
  await p.bringToFront();
  
  // Track resize events
  await p.evaluate(() => { window._resizeCount = 0; window.addEventListener('resize', () => window._resizeCount++); });

  // FIX: force SizeChanged in NavigationView by resizing before accessibility click.
  await p.setViewportSize({ width: 1441, height: 900 });
  await p.waitForTimeout(300);
  await p.setViewportSize({ width: 1440, height: 900 });
  await p.waitForTimeout(300);
  
  const resizesFired = await p.evaluate(() => window._resizeCount);
  console.log(`[${elapsed()}] setViewportSize resize events fired: ${resizesFired}`);

  // Also try manually dispatching resize event
  await p.evaluate(() => window.dispatchEvent(new UIEvent('resize', { bubbles: false, view: window })));
  await p.waitForTimeout(500);
  const resizesFired2 = await p.evaluate(() => window._resizeCount);
  console.log(`[${elapsed()}] after manual dispatchEvent: resize events fired: ${resizesFired2}`);

  // accessibility button - use evaluate click (trusted event)
  const btnLoc = p.locator('#uno-enable-accessibility');
  await btnLoc.waitFor({ state: 'attached', timeout: 15_000 }).catch(() => {});
  console.log(`[${elapsed()}] btn attached`);
  
  await p.evaluate(() => {
    const btn = document.querySelector('#uno-enable-accessibility');
    if (btn) btn.click();
  });
  console.log(`[${elapsed()}] clicked`);
  
  // Check if button was removed (handler fired)
  await p.waitForTimeout(500);
  const btnStillInDom = await p.locator('#uno-enable-accessibility').count();
  console.log(`[${elapsed()}] btn still in DOM after click: ${btnStillInDom}`);
  
  // Check for any new aria elements
  const ariaCount = await p.evaluate(() => document.querySelectorAll('[aria-label]').length);
  console.log(`[${elapsed()}] aria-label count: ${ariaCount}`);
  
  // What aria labels exist?
  const ariaLabels = await p.evaluate(() =>
    Array.from(document.querySelectorAll('[aria-label]')).slice(0,10).map(el => el.getAttribute('aria-label'))
  );
  console.log(`[${elapsed()}] first 10 aria-labels:`, ariaLabels);
  
  // Check window dimensions that could affect NavigationView layout
  const dims = await p.evaluate(() => ({
    innerWidth: window.innerWidth,
    innerHeight: window.innerHeight,
    screenWidth: window.screen.width,
    screenHeight: window.screen.height,
    availWidth: window.screen.availWidth,
    availHeight: window.screen.availHeight,
    devicePixelRatio: window.devicePixelRatio,
    outerWidth: window.outerWidth,
    outerHeight: window.outerHeight,
  }));
  console.log(`[${elapsed()}] window dimensions:`, JSON.stringify(dims, null, 2));
  
  // Dump ALL aria-labels
  const allLabels = await p.evaluate(() =>
    Array.from(document.querySelectorAll('[aria-label]')).map(el => el.getAttribute('aria-label'))
  );
  console.log(`[${elapsed()}] ALL aria-labels (${allLabels.length} total):`, JSON.stringify(allLabels.slice(0, 80)));

  // Check canvas element bounding rect
  const canvasRect = await p.evaluate(() => {
    const canvas = document.querySelector('canvas');
    if (!canvas) return null;
    const r = canvas.getBoundingClientRect();
    return { x: r.x, y: r.y, width: r.width, height: r.height, style: canvas.style.cssText };
  });
  console.log(`[${elapsed()}] canvas rect:`, JSON.stringify(canvasRect));

  // Check document body and html dimensions
  const docDims = await p.evaluate(() => ({
    docClientWidth: document.documentElement.clientWidth,
    docClientHeight: document.documentElement.clientHeight,
    bodyClientWidth: document.body?.clientWidth,
    bodyClientHeight: document.body?.clientHeight,
    visualViewportWidth: window.visualViewport?.width,
    visualViewportHeight: window.visualViewport?.height,
  }));
  console.log(`[${elapsed()}] doc dims:`, JSON.stringify(docDims));

  // Search for pane-related aria labels
  const paneLabels = await p.evaluate(() =>
    Array.from(document.querySelectorAll('[aria-label]'))
      .map(el => el.getAttribute('aria-label'))
      .filter(l => l && (l.includes('Pane') || l.includes('pane') || l.includes('Toggle') || l.includes('Nav') || l.includes('Menu')))
  );
  console.log(`[${elapsed()}] pane/nav/menu labels:`, JSON.stringify(paneLabels));

  // In headed mode: try clicking TogglePaneButton to open the nav pane
  if (!headless) {
    console.log(`[${elapsed()}] Checking for TogglePaneButton...`);
    const toggleBtn = p.locator('[aria-label="TogglePaneButton"]').first();
    const toggleExists = await toggleBtn.count();
    console.log(`[${elapsed()}] TogglePaneButton count: ${toggleExists}`);
    if (toggleExists > 0) {
      await p.evaluate(() => {
        const btn = document.querySelector('[aria-label="TogglePaneButton"]');
        if (btn) btn.click();
      });
      console.log(`[${elapsed()}] clicked TogglePaneButton`);
      await p.waitForTimeout(1000);
      const countAfter = await p.evaluate(() => document.querySelectorAll('[aria-label]').length);
      const hasMenuItemsHost = await p.evaluate(() => !!document.querySelector('[aria-label="MenuItemsHost"]'));
      const hasOverview = await p.evaluate(() => !!document.querySelector('[aria-label="Overview"]'));
      console.log(`[${elapsed()}] after toggle: count=${countAfter}, MenuItemsHost=${hasMenuItemsHost}, Overview=${hasOverview}`);
      if (!hasOverview) {
        // Try clicking again (open if was closed, close if was open)
        await p.evaluate(() => {
          const btn = document.querySelector('[aria-label="TogglePaneButton"]');
          if (btn) btn.click();
        });
        console.log(`[${elapsed()}] clicked TogglePaneButton again`);
        await p.waitForTimeout(1000);
        const countAfter2 = await p.evaluate(() => document.querySelectorAll('[aria-label]').length);
        const hasOverview2 = await p.evaluate(() => !!document.querySelector('[aria-label="Overview"]'));
        console.log(`[${elapsed()}] after 2nd toggle: count=${countAfter2}, Overview=${hasOverview2}`);
      }
    }
  }

  await b.close();
}
