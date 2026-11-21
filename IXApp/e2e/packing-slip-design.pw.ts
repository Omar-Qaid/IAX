import { expect, test } from '@playwright/test';
test('packing slip panel matches the reference and retains failure input', async ({
  page,
}, info) => {
  test.skip(info.project.name !== 'desktop-chromium');
  test.setTimeout(120000);
  await page.setViewportSize({ width: 1913, height: 871 });
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  await page.route('http://localhost:33319/api/**', async (route) => {
    const request = route.request();
    const path = new URL(request.url()).pathname;
    let data: unknown = [];
    if (path.endsWith('/list'))
      data = [
        {
          recId: 1,
          salesId: 'SO-26-008314',
          customerName: 'Abdullah Rashid Al-Qarni Est.',
          currencyCode: 'SAR',
          salesStatus: 'Backorder',
          documentStatus: 'None',
        },
      ];
    if (path.endsWith('/details'))
      data = {
        recId: 1,
        salesId: 'SO-26-008314',
        customerAccount: 'C-ORDER',
        invoiceAccount: 'C-INV',
        customerName: 'Abdullah Rashid Al-Qarni Est.',
        currencyCode: 'SAR',
        salesStatus: 'Backorder',
        documentStatus: 'None',
      };
    if (path.endsWith('/posting-preview'))
      data = {
        header: {
          id: '1',
          salesId: 'SO-26-008314',
          customerName: 'Abdullah Rashid Al-Qarni Est.',
          currencyCode: 'SAR',
          siteId: 'JED',
        },
        lines: [
          {
            id: '10',
            lineNumber: 1,
            itemId: 'ITEM-001',
            name: 'Test item',
            inventSiteId: 'JED',
            inventLocationId: 'MAIN',
            quantity: 6,
            netAmount: 60,
          },
        ],
      };
    if (path.endsWith('/packing-slips') && request.method() === 'POST') {
      await route.fulfill({
        status: 422,
        json: { success: false, message: 'Stock is insufficient.', data: null },
      });
      return;
    }
    await route.fulfill({ json: { success: true, data } });
  });
  await page.goto('/accounts-receivable/sales-orders/1', { waitUntil: 'domcontentloaded' });
  await page.getByRole('button', { name: 'Sell', exact: true }).click({ timeout: 90000 });
  await page.getByRole('button', { name: 'Packing slip', exact: true }).click();
  const panel = page.getByRole('dialog', { name: 'Packing slip posting' });
  await expect(panel).toBeVisible();
  await expect(panel.getByRole('button', { name: 'Overview', exact: true })).toHaveAttribute(
    'aria-expanded',
    'true'
  );
  await expect(panel.getByRole('button', { name: 'Lines', exact: true })).toHaveAttribute(
    'aria-expanded',
    'true'
  );
  await expect(panel.getByRole('button', { name: 'Line details', exact: true })).toHaveAttribute(
    'aria-expanded',
    'false'
  );
  await expect(panel.getByRole('button', { name: 'Sales orders', exact: true })).toHaveAttribute(
    'aria-expanded',
    'false'
  );
  const bounds = await panel.boundingBox();
  expect(bounds!.x).toBeGreaterThanOrEqual(119);
  await panel.getByRole('spinbutton', { name: 'Update quantity for ITEM-001' }).fill('2');
  await page.screenshot({
    path: info.outputPath('packing-slip-posting-desktop.png'),
    fullPage: true,
  });
  await panel.getByRole('button', { name: 'OK', exact: true }).click();
  await expect(
    panel.getByRole('alert').filter({ hasText: 'Stock is insufficient.' })
  ).toBeVisible();
  await expect(panel.getByRole('spinbutton', { name: 'Update quantity for ITEM-001' })).toHaveValue(
    '2'
  );
});
