import { expect, test } from '@playwright/test';

test('renders Exchange rate types with the D365 list-page layout', async ({ page }, testInfo) => {
  test.skip(testInfo.project.name !== 'desktop-chromium');
  await page.route('**/v1/ExchangeRateType', async (route) => {
    await route.fulfill({
      contentType: 'application/json',
      body: JSON.stringify({
        success: true,
        data: [
          { recId: 1, name: 'Standard', description: 'Standard exchange rate', isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'dat' },
          { recId: 2, name: 'Budget', description: 'Budget exchange rate', isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'dat' },
        ],
      }),
    });
  });
  await page.goto('/foundation/exchange-rate-types');

  await expect(page.getByRole('button', { name: 'Back' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Edit' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'New', exact: true })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Delete' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Exchange rates' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Options' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Search', exact: true }).last()).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Standard view' })).toBeVisible();

  const filter = page.getByRole('textbox', { name: 'Filter' });
  await expect(filter).toBeVisible();
  await expect(filter.locator('..')).toHaveCSS('height', '37px');

  const grid = page.getByRole('grid');
  await expect(grid).toBeVisible();
  await expect(page.getByRole('button', { name: 'Choose columns' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Features' })).toBeVisible();
  await grid.screenshot({ path: 'test-results/exchange-rate-types-grid.png' });
  await page.screenshot({ path: 'test-results/exchange-rate-types-page.png', fullPage: true });
});
