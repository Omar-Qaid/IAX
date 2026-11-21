import { expect, test } from '@playwright/test';
test('customers use the shared compact row, header and typography defaults', async ({
  page,
}, info) => {
  test.skip(info.project.name !== 'desktop-chromium');
  test.setTimeout(120000);
  await page.setViewportSize({ width: 1870, height: 850 });
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  await page.route('http://localhost:33319/api/**', async (route) => {
    const path = new URL(route.request().url()).pathname;
    let data: unknown = [];
    if (path.endsWith('/CustTable/list'))
      data = [
        {
          recId: 1,
          accountNumber: 'C-001',
          name: 'Reference customer',
          customerGroupId: 'LOCAL',
          currencyCode: 'SAR',
        },
        {
          recId: 2,
          accountNumber: 'C-002',
          name: 'Second customer',
          customerGroupId: 'LOCAL',
          currencyCode: 'SAR',
        },
      ];
    await route.fulfill({ json: { success: true, data } });
  });
  await page.goto('/accounts-receivable/customers', { waitUntil: 'domcontentloaded' });
  const code = page.getByRole('link', { name: 'C-001', exact: true });
  await expect(code).toBeVisible({ timeout: 90000 });
  const row = code.locator('xpath=ancestor::*[@role="row"]');
  expect((await row.boundingBox())!.height).toBe(31);
  const header = page
    .getByRole('columnheader')
    .filter({ hasText: /^Account$/ })
    .first();
  expect((await header.boundingBox())!.height).toBe(30);
  expect(await code.evaluate((element) => getComputedStyle(element).fontSize)).toBe('11px');
  await page.screenshot({ path: info.outputPath('customers-compact-grid.png'), fullPage: true });
});
