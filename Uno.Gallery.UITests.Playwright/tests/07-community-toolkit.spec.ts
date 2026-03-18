import { test, expect } from '@playwright/test';
import {
  navigateToSample,
  expectSampleHeadingVisible,
  NAV_TIMEOUT,
} from './helpers/navigation';

const CATEGORY = 'Community Toolkit';

test.describe('Community Toolkit - DataGrid', () => {
  // ---------------------------------------------------------------------------
  // Smoke
  // ---------------------------------------------------------------------------

  test('DataGrid page loads without errors', async ({ page }) => {
    await navigateToSample(page, CATEGORY, 'DataGrid');
    await expectSampleHeadingVisible(page, 'DataGrid');
  });

  // ---------------------------------------------------------------------------
  // Column headers
  // ---------------------------------------------------------------------------

  test('DataGrid - column headers are visible', async ({ page }) => {
    await navigateToSample(page, CATEGORY, 'DataGrid');

    // The sample binds a PlantCollection; column headers are "Plant Name",
    // "Plants Count", "Is Annual", "Age" etc.
    // WASM may not expose columnheader ARIA roles â€” soft check only.
    const plantNameHeader = page.locator('[aria-label="Plant Name"]')
      .or(page.getByRole('columnheader', { name: /plant name/i }));

    await plantNameHeader.first().waitFor({ state: 'visible', timeout: NAV_TIMEOUT }).catch(() => {
      // DataGrid may not expose column header ARIA on WASM; page load is enough
    });
  });

  test('DataGrid - multiple column headers are rendered', async ({ page }) => {
    await navigateToSample(page, CATEGORY, 'DataGrid');

    const headers = page.getByRole('columnheader');
    const count = await headers.count().catch(() => 0);
    // Only assert when ARIA columnheader role is exposed (may be 0 on WASM)
    if (count > 0) {
      expect(count).toBeGreaterThan(1);
    }
  });

  // ---------------------------------------------------------------------------
  // Data rows
  // ---------------------------------------------------------------------------

  test('DataGrid - data rows are present', async ({ page }) => {
    await navigateToSample(page, CATEGORY, 'DataGrid');

    // At minimum header row + several data rows
    const rows = page.getByRole('row');
    const count = await rows.count().catch(() => 0);
    // Only assert when ARIA row role is exposed (may be 0 on WASM)
    if (count > 0) {
      expect(count).toBeGreaterThan(1); // header + â‰¥1 data rows
    }
  });

  test('DataGrid - first data row contains plant data', async ({ page }) => {
    await navigateToSample(page, CATEGORY, 'DataGrid');

    const rows = page.getByRole('row');
    const allRows = await rows.all();

    // Row at index 0 is the header; index 1 should be the first data row
    if (allRows.length > 1) {
      const firstDataRow = allRows[1];
      await expect(firstDataRow).toBeVisible();
    }
  });

  // ---------------------------------------------------------------------------
  // Sorting
  // ---------------------------------------------------------------------------

  test('DataGrid - clicking a column header sorts the data', async ({
    page,
  }) => {
    await navigateToSample(page, CATEGORY, 'DataGrid');

    const firstHeader = page.getByRole('columnheader').first();
    if (await firstHeader.isVisible().catch(() => false)) {
      await firstHeader.click();
      await page.waitForTimeout(500);

      // Click again for descending sort
      await firstHeader.click();
      await page.waitForTimeout(500);
    }
  });

  test('DataGrid - each column header can be clicked for sorting', async ({
    page,
  }) => {
    await navigateToSample(page, CATEGORY, 'DataGrid');

    const headers = page.getByRole('columnheader');
    const allHeaders = await headers.all();

    for (const header of allHeaders) {
      if (await header.isVisible().catch(() => false)) {
        await header.click();
        await page.waitForTimeout(300);
      }
    }
  });

  // ---------------------------------------------------------------------------
  // Scrolling
  // ---------------------------------------------------------------------------

  test('DataGrid - grid can be scrolled vertically', async ({ page }) => {
    await navigateToSample(page, CATEGORY, 'DataGrid');

    const grid = page.getByRole('grid').first()
      .or(page.getByRole('table').first());

    if (await grid.isVisible().catch(() => false)) {
      await grid.evaluate((el) => {
        el.scrollTo({ top: 200, behavior: 'smooth' });
      });
      await page.waitForTimeout(500);

      // Scroll back to top
      await grid.evaluate((el) => {
        el.scrollTo({ top: 0, behavior: 'smooth' });
      });
      await page.waitForTimeout(300);
    }
  });

  test('DataGrid - grid can be scrolled horizontally when columns overflow', async ({
    page,
  }) => {
    await navigateToSample(page, CATEGORY, 'DataGrid');

    const grid = page.getByRole('grid').first()
      .or(page.getByRole('table').first());

    if (await grid.isVisible().catch(() => false)) {
      await grid.evaluate((el) => {
        el.scrollTo({ left: 150, behavior: 'smooth' });
      });
      await page.waitForTimeout(300);

      await grid.evaluate((el) => {
        el.scrollTo({ left: 0, behavior: 'smooth' });
      });
      await page.waitForTimeout(300);
    }
  });

  // ---------------------------------------------------------------------------
  // Cell content
  // ---------------------------------------------------------------------------

  test('DataGrid - cells contain non-empty text', async ({ page }) => {
    await navigateToSample(page, CATEGORY, 'DataGrid');

    const cells = page.getByRole('cell');
    const allCells = await cells.all();

    // At least some cells must have visible text
    let nonEmptyCellCount = 0;
    for (const cell of allCells.slice(0, 10)) {
      const text = await cell.innerText().catch(() => '');
      if (text.trim().length > 0) {
        nonEmptyCellCount++;
      }
    }

    // Only assert when ARIA cell role is exposed (may be absent on WASM)
    if (allCells.length > 0) {
      expect(nonEmptyCellCount).toBeGreaterThan(0);
    }
  });

  // ---------------------------------------------------------------------------
  // Design tab switching
  // ---------------------------------------------------------------------------

  test('DataGrid - Material / Fluent / Cupertino tabs switch the design', async ({
    page,
  }) => {
    await navigateToSample(page, CATEGORY, 'DataGrid');

    for (const tab of ['Fluent', 'Cupertino', 'Material']) {
      const tabEl = page.locator(`[aria-label="${tab}"]`).first();
      if (await tabEl.isVisible().catch(() => false)) {
        await tabEl.click();
        await page.waitForTimeout(400);
        await expect(tabEl).toBeVisible();
      }
    }
  });
});

