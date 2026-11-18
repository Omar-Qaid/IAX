import { expect, test } from '@playwright/test';

test('free text invoice details edits and saves the active line, cancels changes and locks posted records', async ({
  page,
}, info) => {
  test.setTimeout(180_000);
  page.setDefaultTimeout(20_000);
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  const date = new Date().toISOString().slice(0, 10);
  let record = {
    recId: 7,
    invoiceId: '',
    customerAccount: 'C-001',
    invoiceAccount: 'C-001',
    description: 'Services',
    invoiceDate: date,
    dueDate: date,
    currencyCode: 'SAR',
    paymentTerms: 'NET30',
    paymentMethod: 'BANK',
    postingProfile: 'GEN',
    defaultDimension: 0,
    taxGroup: 'VAT',
    posted: false,
    netAmount: 100,
    taxAmount: 15,
    invoiceAmount: 115,
    lines: [
      {
        recId: 71,
        lineNum: 1,
        description: 'Consulting',
        quantity: 1,
        unitPrice: 100,
        netAmount: 100,
        taxAmount: 15,
        grossAmount: 115,
        ledgerDimension: 9,
        defaultDimension: 0,
        taxGroup: 'VAT',
        taxItemGroup: 'STANDARD',
      },
    ],
  };
  let saves = 0,
    posts = 0,
    creates = 0;
  await page.route('http://localhost:33319/api/**', async (route) => {
    const url = new URL(route.request().url()),
      path = url.pathname,
      method = route.request().method();
    let data: unknown = [];
    if (path.endsWith('/CustTable/list'))
      data = [
        {
          recId: 10,
          party: 10,
          accountNumber: 'C-001',
          name: 'Customer one',
          nameAr: 'العميل الأول',
          customerGroupId: 'Retail',
          currencyCode: 'SAR',
          invoiceAccount: 'C-001',
          paymTermId: 'NET30',
          paymModeId: 'BANK',
          taxGroupId: 'VAT',
          status: 'active',
        },
      ];
    if (path.endsWith('/Currency')) data = [{ currencyCode: 'SAR', txt: 'Saudi Riyal' }];
    if (path.endsWith('/PaymTerm')) data = [{ paymTermId: 'NET30', description: '30 days' }];
    if (path.endsWith('/CustPaymMode')) data = [{ paymMode: 'BANK', name: 'Bank transfer' }];
    if (path.endsWith('/TaxGroup')) data = [{ taxGroup: 'VAT', taxGroupName: 'VAT' }];
    if (path.endsWith('/tax-groups'))
      data = {
        salesTaxGroups: [{ id: 'VAT', code: 'VAT', name: 'VAT' }],
        itemSalesTaxGroups: [{ id: 'STANDARD', code: 'STANDARD', name: 'Standard' }],
      };
    if (path.endsWith('/ledger-dimensions'))
      data = {
        data: [{ id: 9, code: '400100', name: 'Consulting revenue' }],
        pageNumber: 1,
        totalPages: 1,
        totalRecords: 1,
      };
    if (path.endsWith('/CustInvoiceTable') && method === 'GET') data = [record];
    if (path.endsWith('/CustInvoiceTable/7') && method === 'GET') data = record;
    if (path.endsWith('/CustInvoiceTable/7') && method === 'PUT') {
      const input = route.request().postDataJSON();
      expect(input.lines).toHaveLength(1);
      expect(input.lines[0].description).toBe('Updated consulting');
      expect(input.lines[0].ledgerDimension).toBe(9);
      expect(input.defaultDimension).toBe(0);
      expect(input.lines[0].defaultDimension).toBe(0);
      expect(input.lines[0]).not.toHaveProperty('recId');
      record = {
        ...record,
        ...input,
        lines: [{ ...record.lines[0], ...input.lines[0], recId: 72 }],
      };
      saves++;
      data = record;
    }
    if (path.endsWith('/CustInvoiceTable') && method === 'POST') {
      const input = route.request().postDataJSON();
      expect(input.customerAccount).toBe('C-001');
      expect(input.lines[0].description).toBe('New consulting');
      expect(input.lines[0].ledgerDimension).toBe(9);
      record = {
        ...record,
        ...input,
        recId: 8,
        posted: false,
        lines: [{ ...record.lines[0], ...input.lines[0], recId: 81 }],
      };
      creates++;
      data = record;
    }
    if (path.endsWith('/CustInvoiceTable/8') && method === 'GET') data = record;
    if (path.endsWith('/CustInvoiceTable/7/post')) {
      posts++;
      record = { ...record, posted: true, invoiceId: 'FTI-007' };
      data = record;
    }
    await route.fulfill({ json: { success: true, data } });
  });
  await page.goto('/accounts-receivable/free-text-invoices/7');
  // Title like sales order: "<invoice> : <customer name>".
  await expect(page.getByRole('heading', { name: /: Customer one$/ })).toBeVisible({
    timeout: 60_000,
  });
  await expect(page.getByText('Free text invoice header', { exact: true })).toBeVisible();
  await expect(page.getByText('Invoice lines', { exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Edit', exact: true }).click();
  const line = page.locator('[data-row-id="71"]');
  await line.getByText('Consulting', { exact: true }).dblclick();
  // Save while the grid editor remains active: the page must flush it first.
  await page.locator('input[value="Consulting"]').fill('Updated consulting');
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect.poll(() => saves).toBe(1);
  await expect(page.getByRole('button', { name: 'Edit', exact: true })).toBeEnabled();
  await page.getByRole('button', { name: 'Edit', exact: true }).click();
  await page.getByRole('textbox', { name: 'Description', exact: true }).fill('Discard this');
  await page.getByRole('button', { name: 'Cancel', exact: true }).click();
  expect(saves).toBe(1);
  await expect(page.getByText('Discard this', { exact: true })).toHaveCount(0);
  // Post and Totals sit on the "Free text invoice" ribbon tab, like sales order.
  const openRibbon = async () => {
    const tab = page.getByRole('button', { name: 'Free text invoice', exact: true });
    if ((await tab.getAttribute('aria-expanded')) !== 'true') await tab.click();
  };
  const ribbon = page.locator('#free-text-invoice-action-ribbon');
  await openRibbon();
  await ribbon.getByRole('button', { name: 'Totals', exact: true }).click();
  await expect(page.getByRole('dialog')).toContainText('115.00');
  await page.getByRole('dialog').getByRole('button', { name: 'OK', exact: true }).click();
  await openRibbon();
  await ribbon.getByRole('button', { name: 'Post', exact: true }).click();
  expect(posts).toBe(0);
  await page.getByRole('dialog').getByRole('button', { name: 'Post', exact: true }).click();
  await expect.poll(() => posts).toBe(1);
  await expect(page.getByRole('button', { name: 'Edit', exact: true })).toBeDisabled();
  await expect(page.getByRole('button', { name: 'Delete', exact: true })).toBeDisabled();
  await openRibbon();
  await expect(ribbon.getByRole('button', { name: 'Post', exact: true })).toBeDisabled();
  await page.screenshot({ path: info.outputPath('free-text-invoice-details.png'), fullPage: true });
  await page.getByRole('button', { name: 'New', exact: true }).click();
  await expect(page).toHaveURL(/free-text-invoices\/new\?customerAccount=C-001$/);
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect(
    page.getByRole('alert').filter({ hasText: 'Select a customer and invoice date' })
  ).toBeVisible();
  expect(creates).toBe(0);
  await page.getByRole('button', { name: 'Add line', exact: true }).click();
  await expect(page.locator('[data-row-id]')).toHaveCount(2);
  await page.getByRole('button', { name: 'Remove', exact: true }).click();
  await expect(page.locator('[data-row-id]')).toHaveCount(1);
  const newRow = page.locator('[data-row-id]').first();
  await newRow.dblclick();
  // The first line's description is a plain text editor; header Description has an accessible label.
  const descriptions = page.getByRole('textbox', { name: 'Description', exact: true });
  const gridDescription = page.locator('[data-row-id] input').first();
  await gridDescription.fill('New consulting');
  await page.getByRole('tab', { name: 'Header', exact: true }).click();
  await expect(page.getByText('Invoice lines', { exact: true })).toHaveCount(0);
  await page.getByRole('tab', { name: 'Lines', exact: true }).click();
  await page.locator('[data-row-id]').getByText('New consulting', { exact: true }).dblclick();
  await page.getByRole('combobox', { name: 'Ledger account', exact: true }).click();
  await expect(page.getByRole('option').filter({ hasText: 'Consulting revenue' }).first()).toBeVisible();
  await page.screenshot({ path: info.outputPath('free-text-invoice-line-edit.png') });
  await page.getByRole('option').filter({ hasText: 'Consulting revenue' }).first().click();
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect.poll(() => creates).toBe(1);
  await expect(page).toHaveURL(/free-text-invoices\/8$/);
  await expect(descriptions).toHaveCount(0);
});
