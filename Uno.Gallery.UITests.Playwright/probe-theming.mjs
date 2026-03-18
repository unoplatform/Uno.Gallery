import { chromium } from "@playwright/test";

const samples = ["Material Palette", "Typography", "Fluent Palette", "Cupertino Palette", "Lightweight Styling"];
for (const name of samples) {
  const b = await chromium.launch({ headless: true });
  const ctx = await b.newContext({ viewport: { width: 1440, height: 900 } });
  const p = await ctx.newPage();
  await p.goto(`http://localhost:5000/#${encodeURIComponent(name)}`);
  await p.waitForLoadState("networkidle");
  await p.waitForTimeout(8000);
  await p.setViewportSize({ width: 1441, height: 900 });
  await p.waitForTimeout(300);
  await p.setViewportSize({ width: 1440, height: 900 });
  await p.waitForTimeout(300);
  await p.evaluate(() => { const btn = document.querySelector("#uno-enable-accessibility"); if (btn) btn.click(); });
  await p.waitForTimeout(5000);
  
  const visible = await p.evaluate(() => {
    const result = [];
    for (const el of document.querySelectorAll("[aria-label]")) {
      const s = getComputedStyle(el);
      if (s.display !== 'none' && s.visibility !== 'hidden' && !el.hasAttribute("hidden")) {
        result.push(el.getAttribute("aria-label"));
      }
    }
    return [...new Set(result)].sort();
  });
  
  console.log(`\n=== "${name}" (${visible.length} visible labeled) ===`);
  visible.slice(0, 40).forEach(l => console.log("  " + l));
  if (visible.length > 40) console.log(`  ... and ${visible.length - 40} more`);
  await b.close();
}
