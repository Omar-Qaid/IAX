import { expect, test } from '@playwright/test';

for (const language of ['en', 'ar']) {
  test(`sales grid entry, validation, layout, and virtualization (${language})`, async ({
    page,
  }, testInfo) => {
    test.skip(testInfo.project.name !== 'desktop-chromium');
    test.setTimeout(90000);
    await page.setViewportSize({ width: 1680, height: 1000 });
    await page.addInitScript((lang) => localStorage.setItem('i18nextLng', lang), language);
    const lines = Array.from({ length: 1500 }, (_, i) => ({
      id: String(i + 1),
      lineNumber: i + 1,
      itemNumber: `ITEM-${String(i + 1).padStart(4, '0')}`,
      description: `Sales grid product ${i + 1}`,
      quantity: 2,
      unit: 'Pcs',
      unitPrice: 5,
      lineTotal: 10,
      deliveryDate: '2026-09-09',
      lineType: 3,
      deliveryType: 0,
    }));
    let lineReads = 0;
    let writes = 0;
    let adds = 0;
    let deletes = 0;
    let failNextUpdate = false;
    const pageErrors: string[] = [];
    page.on('pageerror', (error) => pageErrors.push(error.message));
    // All API calls from this isolated browser are fulfilled with test fixtures.
    await page.route('http://localhost:33319/api/**', (route) =>
      route.fulfill({ json: { success: true, data: [] } })
    );
    await page.route('http://localhost:33319/api/v1/SalesTable/**', async (route) => {
      const request = route.request();
      const path = new URL(request.url()).pathname;
      let data: unknown = [];
      if (path.endsWith('/list'))
        data = [
          {
            recId: 1,
            salesId: 'SO-GRID-001',
            customerAccount: 'C-001',
            customerName: 'Grid test customer',
            invoiceAccount: 'C-001',
            currencyCode: 'SAR',
            customerGroup: 'RETAIL',
            salesStatus: 'Backorder',
            documentStatus: 'None',
            deliveryDate: '2026-09-09',
            paymentTerms: 'Net 30',
            deliveryMode: 'Road',
            orderTotal: 15000,
          },
        ];
      else if (path.endsWith('/units'))
        data = {
          data: [{ symbol: 'Pcs' }, { symbol: 'BOX' }],
          pageNumber: 1,
          totalPages: 1,
          totalRecords: 2,
        };
      else if (path.endsWith('/items'))
        data = {
          data: [{ itemNumber: 'ITEM-NEW', name: 'New test product', unit: 'Pcs', unitPrice: 5 }],
          pageNumber: 1,
          totalPages: 1,
          totalRecords: 1,
        };
      else if (request.method() === 'POST') {
        const payload = request.postDataJSON();
        const added = {
          ...lines[0],
          ...payload,
          id: '1501',
          lineNumber: 1501,
          description: 'New test product',
          lineTotal: payload.quantity * payload.unitPrice,
        };
        lines.push(added);
        adds++;
        data = added;
      } else if (request.method() === 'DELETE') {
        lines.splice(
          lines.findIndex((line) => line.id === path.split('/').at(-1)),
          1
        );
        deletes++;
        data = { deleted: true };
      } else if (request.method() === 'PUT') {
        if (failNextUpdate) {
          failNextUpdate = false;
          await route.fulfill({
            status: 404,
            json: { success: false, message: 'Test save failed. Please retry.' },
          });
          return;
        }
        const payload = request.postDataJSON();
        const index = lines.findIndex((line) => line.id === payload.id);
        lines[index] = {
          ...lines[index],
          ...payload,
          lineTotal: payload.quantity * payload.unitPrice,
        };
        writes++;
        data = lines[index];
      } else if (path.endsWith('/lines')) {
        lineReads++;
        data = lines;
      }
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
    if (language === 'en') {
      const timings: number[] = [];
      for (const field of [
        'unitPrice',
        'quantity',
        'unitPrice',
        'quantity',
        'unitPrice',
        'quantity',
      ]) {
        const elapsed = await grid.locator(`[data-row-id="1"] [data-field="${field}"]`).evaluate(
          (cell) =>
            new Promise<number>((resolve, reject) => {
              const started = performance.now();
              const timeout = window.setTimeout(() => {
                cell.removeEventListener('focusin', onFocus);
                reject(new Error('Cell editor did not receive focus'));
              }, 3000);
              function onFocus(event: Event) {
                if (!(event.target instanceof HTMLInputElement)) return;
                cell.removeEventListener('focusin', onFocus);
                clearTimeout(timeout);
                resolve(performance.now() - started);
              }
              cell.addEventListener('focusin', onFocus);
              for (const type of ['mousedown', 'mouseup', 'click'])
                cell.dispatchEvent(
                  new MouseEvent(type, { bubbles: true, cancelable: true, button: 0 })
                );
            })
        );
        timings.push(elapsed);
      }
      console.log('Unchanged-cell focus milliseconds:', JSON.stringify(timings));
      await testInfo.attach('cell-focus-timings', {
        body: JSON.stringify(timings),
        contentType: 'application/json',
      });
      expect(writes).toBe(0);
    }
    await expect(grid.locator('nav')).toBeHidden();
    await expect(quantityCell).toHaveCSS('outline-width', '1px');
    await expect(quantity).toHaveCSS('outline-style', 'none');
    expect(
      await quantity.evaluate(
        (input) => getComputedStyle(input.closest('.MuiInput-root')!, '::before').display
      )
    ).toBe('none');
    await quantity.fill('0');
    await quantity.press('Tab');
    await expect(quantity).toHaveAttribute('aria-invalid', 'true');
    await grid.locator('[data-row-id="1"] [data-field="unitPrice"]').click();
    await expect(quantity).toBeFocused();
    expect(writes).toBe(0);
    await quantity.fill('12.75');
    await quantity.press('Tab');
    await expect(
      grid.locator('[data-row-id="1"] [data-field="unit"]').getByRole('combobox')
    ).toBeFocused();
    expect(writes).toBe(1);
    expect(lineReads).toBe(1);
    const unit = grid.locator('[data-row-id="1"] [data-field="unit"]').getByRole('combobox');
    await unit.press('ArrowDown');
    await page.getByRole('option', { name: 'BOX', exact: true }).click();
    await expect(unit).toBeEnabled();
    expect(writes).toBe(2);
    await unit.press('Control+f');
    const itemFilter = grid.locator('[data-grid-filter-field="itemNumber"]');
    await itemFilter.fill('ITEM-0001');
    await expect(grid).toHaveAttribute('aria-rowcount', '1');
    await itemFilter.fill('');
    await expect(grid).toHaveAttribute('aria-rowcount', '1500');
    expect(lineReads).toBe(1);
    // Column widths and fixed item identity survive edits and horizontal jumps.
    const itemCell = grid.locator('[data-row-id="1"] [data-field="itemNumber"]');
    const originalWidth = (await itemCell.boundingBox())!.width;
    const resize = grid.locator('[data-grid-resize-handle="itemNumber"]').last();
    const grip = (await resize.boundingBox())!;
    await page.mouse.move(grip.x + grip.width / 2, grip.y + 10);
    await page.mouse.down();
    await page.mouse.move(grip.x + grip.width / 2 + (language === 'ar' ? -40 : 40), grip.y + 10, {
      steps: 5,
    });
    await page.mouse.up();
    await expect
      .poll(async () => (await itemCell.boundingBox())!.width)
      .toBeGreaterThan(originalWidth + 20);
    await quantityCell.click();
    await quantity.fill('99');
    await quantity.press('Escape');
    await expect(quantity).toHaveValue('12.75');
    expect(writes).toBe(2);
    // A virtualized jump must mount the destination and keep the editor usable.
    await page.keyboard.press('Control+End');
    await expect(grid.locator('[data-row-id="1500"]')).toBeVisible();
    expect(await grid.locator('[data-row-id]').count()).toBeLessThan(60);
    await page.screenshot({ path: `test-results/sales-grid-${language}.png`, fullPage: false });
    if (language === 'en') {
      await page.keyboard.press('Insert');
      const itemLookup = grid.getByRole('textbox', { name: 'Item number', exact: true });
      await expect(itemLookup).toBeFocused();
      await itemLookup.click();
      await page.getByText('ITEM-NEW', { exact: true }).click();
      const addedQuantity = grid.locator('[data-row-id="new-sales-line"]').getByRole('spinbutton');
      await expect(addedQuantity).toBeFocused();
      await addedQuantity.fill('4.25');
      await addedQuantity.press('Tab');
      const addedUnit = grid
        .locator('[data-row-id="1501"] [data-field="unit"]')
        .getByRole('combobox');
      await expect(addedUnit).toBeFocused();
      expect(adds).toBe(1);
      await addedUnit.press('Alt+Delete');
      await expect(grid).toHaveAttribute('aria-rowcount', '1500');
      expect(deletes).toBe(1);
      expect(lineReads).toBe(1);
      failNextUpdate = true;
      const retryCell = grid.locator('[data-row-id="1500"] [data-field="quantity"]');
      await retryCell.click();
      const retryInput = retryCell.getByRole('spinbutton');
      await retryInput.fill('7.25');
      await retryInput.press('Tab');
      await expect(page.getByRole('alert')).toContainText('Test save failed. Please retry.');
      await expect(retryInput).toHaveValue('7.25');
      await expect(retryInput).toBeFocused();
      await retryInput.press('Tab');
      await expect(
        grid.locator('[data-row-id="1500"] [data-field="unit"]').getByRole('combobox')
      ).toBeFocused();
    }
    expect(pageErrors).toEqual([]);
  });
}
