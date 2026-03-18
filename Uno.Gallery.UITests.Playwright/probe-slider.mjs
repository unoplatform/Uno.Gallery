import { chromium } from "@playwright/test";
const b = await chromium.launch();
const ctx = await b.newContext({ viewport: { width: 1440, height: 900 } });
const p = await ctx.newPage();
await p.goto("http://localhost:5000");
await p.waitForLoadState("networkidle");
const btn = p.locator("#uno-enable-accessibility");
if (await btn.isVisible({timeout:10000}).catch(()=>false)) await btn.evaluate(el=>el.click());
await p.locator('[aria-label="Overview"]').first().waitFor({state:"attached",timeout:90000});
await p.waitForTimeout(2000);
// Time the networkidle event in HEADED mode (headless: false)
// to see if analytics polling prevents networkidle from firing quickly
const t0 = Date.now();
const elapsed = () => `${((Date.now()-t0)/1000).toFixed(1)}s`;

console.log(`[${elapsed()}] goto started`);
await p.goto("http://localhost:5000");
console.log(`[${elapsed()}] domcontentloaded reached`);

// Check networkidle with 30s timeout (not 300s - fail fast if it never settles)
console.log(`[${elapsed()}] waiting for networkidle (30s max)...`);
const niPromise = p.waitForLoadState("networkidle", { timeout: 30_000 });
const niTimeout = new Promise(r => setTimeout(() => r('TIMEOUT'), 30_000));
const result = await Promise.race([niPromise.then(() => 'OK'), niTimeout]);
console.log(`[${elapsed()}] networkidle result: ${result}`);

// Regardless, try the accessibility button approach
const btnCount = await p.locator('#uno-enable-accessibility').count();
console.log(`[${elapsed()}] accessibility button count: ${btnCount}`);
const btnVisible = await p.locator('#uno-enable-accessibility').isVisible().catch(() => false);
console.log(`[${elapsed()}] accessibility button isVisible: ${btnVisible}`);

// Click via evaluate
const clickResult = await p.evaluate(() => {
  const btn = document.querySelector('#uno-enable-accessibility');
  if (btn) { btn.click(); return 'clicked'; }
  return 'button not found';
});
console.log(`[${elapsed()}] evaluate click: ${clickResult}`);

// Check for Overview
await p.locator('[aria-label="Overview"]').first().waitFor({ state: 'attached', timeout: 10_000 })
  .then(() => console.log(`[${elapsed()}] OVERVIEW found!`))
  .catch(() => console.log(`[${elapsed()}] OVERVIEW not found in 10s`));

await b.close();
