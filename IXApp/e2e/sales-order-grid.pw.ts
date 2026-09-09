import { expect, test } from '@playwright/test';

for (const language of ['en', 'ar']) {
  test(`sales grid entry, validation, layout, and virtualization (${language})`, async ({ page }, testInfo) => {
    test.skip(testInfo.project.name !== 'desktop-chromium');
    test.setTimeout(90000);
    await page.setViewportSize({ width: 1680, height: 1000 });
    await page.addInitScript((lang) => localStorage.setItem('i18nextLng', lang), language);
    const lines = Array.from({ length: 1500 }, (_, i) => ({ id: String(i + 1), lineNumber: i + 1, itemNumber: `ITEM-${String(i + 1).padStart(4, '0')}`,
      description: `Sales grid product ${i + 1}`, quantity: 2, unit: 'Pcs', unitPrice: 5, lineTotal: 10, deliveryDate: '2026-09-09', lineType: 3, deliveryType: 0 }));
    let lineReads = 0;
    let writes = 0;
    const pageErrors: string[] = [];
    page.on('pageerror', error => pageErrors.push(error.message));
    // All API calls from this isolated browser are fulfilled with test fixtures.
    await page.route('http://localhost:33319/api/**', route => route.fulfill({ json: { success: true, data: [] } }));
    await page.route('http://localhost:33319/api/v1/SalesTable/**', async route => {
      const request = route.request();
      const path = new URL(request.url()).pathname;
      let data: unknown = [];
      if (path.endsWith('/list')) data = [{ recId: 1, salesId: 'SO-GRID-001', customerAccount: 'C-001', customerName: 'Grid test customer', invoiceAccount: 'C-001', currencyCode: 'SAR',
        customerGroup: 'RETAIL', salesStatus: 'Backorder', documentStatus: 'None', deliveryDate: '2026-09-09', paymentTerms: 'Net 30', deliveryMode: 'Road', orderTotal: 15000 }];
      else if (path.endsWith('/units')) data = { data: [{ symbol: 'Pcs' }, { symbol: 'BOX' }], pageNumber: 1, totalPages: 1, totalRecords: 2 };
      else if (request.method() === 'PUT') {
        const payload = request.postDataJSON();
        const index = lines.findIndex(line => line.id === payload.id);
        lines[index] = { ...lines[index], ...payload, lineTotal: payload.quantity * payload.unitPrice };
        writes++;
        data = lines[index];
      } else if (path.endsWith('/lines')) { lineReads++; data = lines; }
      await route.fulfill({ json: { success: true, data } });
    });
    await page.goto('/accounts-receivable/sales-orders/1');
    const grid = page.getByRole('grid');
    await expect(grid).toHaveAttribute('aria-rowcount', '1500');
    await expect(grid.locator('[data-row-id="1"]')).toBeVisible();
    expect(await grid.locator('[data-row-id]').count()).toBeLessThan(50);
    const quantityCell = grid.locator('[data-row-id="1"] [data-field="quantity"]');
    await quantityCell.click();
    const quantity = quantityCell.getByRole('spinbutton');
    await quantity.fill('0');
    await quantity.press('Tab');
    await expect(quantity).toHaveAttribute('aria-invalid', 'true');
    await grid.locator('[data-row-id="1"] [data-field="unitPrice"]').click();
    await expect(quantity).toBeFocused();
    expect(writes).toBe(0);
    await quantity.fill('12.75');
    await quantity.press('Tab');
    await expect(grid.locator('[data-row-id="1"] [data-field="unit"]').getByRole('combobox')).toBeFocused();
    expect(writes).toBe(1);
    expect(lineReads).toBe(1);
    // A virtualized jump must mount the destination and keep the editor usable.
    await page.keyboard.press('Control+End');
    await expect(grid.locator('[data-row-id="1500"]')).toBeVisible();
    expect(await grid.locator('[data-row-id]').count()).toBeLessThan(60);
    await page.screenshot({ path: `test-results/sales-grid-${language}.png`, fullPage: false });
    expect(pageErrors).toEqual([]);
  });
}
