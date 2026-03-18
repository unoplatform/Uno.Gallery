/**
 * TEMPORARY diagnostic test — captures ARIA snapshots to understand
 * the navigation structure. Delete after diagnosis is complete.
 */
import { test, expect } from '@playwright/test';
import { gotoApp, NAV_TIMEOUT } from './helpers/navigation';

test('ARIA_PROBE: capture all labels after clicking Theming', async ({ page }) => {
  await gotoApp(page);

  // First, see what elements have aria-label="Theming" BEFORE clicking
  const themingCount = await page.locator('[aria-label="Theming"]').count();
  console.log(`[aria-label="Theming"] count on Overview page: ${themingCount}`);

  // Get all aria-labels before clicking Theming
  const beforeLabels = await page.evaluate(() => {
    const els = document.querySelectorAll('[aria-label]');
    return Array.from(els).map(el => ({
      label: el.getAttribute('aria-label'),
      tag: el.tagName,
      role: el.getAttribute('role'),
    })).filter(x => x.label);
  });
  console.log('ALL "Theming" labeled elements before click:',
    beforeLabels.filter(x => x.label?.toLowerCase().includes('theming')));

  // Click the Theming category
  await page.locator('[aria-label="Theming"]').first().click();
  await page.waitForTimeout(2000);

  // Get aria-labels after
  const allLabels = await page.evaluate(() => {
    const els = document.querySelectorAll('[aria-label]');
    return Array.from(els).map(el => el.getAttribute('aria-label')).filter(Boolean);
  });

  const fs = await import('fs');
  fs.writeFileSync('test-results/theming-aria-labels.txt', allLabels.join('\n'));

  const interesting = allLabels.filter(l =>
    l.toLowerCase().includes('palette') ||
    l.toLowerCase().includes('typography') ||
    l.toLowerCase().includes('lightweight') ||
    l.toLowerCase().includes('fluent') ||
    l.toLowerCase().includes('cupertino') ||
    l.toLowerCase().includes('theme') ||
    l.toLowerCase().includes('material')
  );
  console.log('Interesting labels after clicking Theming:', interesting);
  console.log('Total labels:', allLabels.length);
  console.log('All labels (joined):', allLabels.join(' | '));

  expect(allLabels.length).toBeGreaterThan(5);
});

test('ARIA_PROBE: what is visible on overview page', async ({ page }) => {
  await gotoApp(page);

  const allLabels = await page.evaluate(() => {
    const els = document.querySelectorAll('[aria-label]');
    return Array.from(els).map(el => el.getAttribute('aria-label')).filter(Boolean);
  });

  const fs = await import('fs');
  fs.writeFileSync('test-results/overview-aria-labels.txt', allLabels.join('\n'));
  console.log('Overview labels (first 100):', allLabels.slice(0, 100).join(', '));
  expect(allLabels.length).toBeGreaterThan(5);
});
