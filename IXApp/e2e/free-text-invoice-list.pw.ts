import { expect, test } from '@playwright/test';

test('free text invoice list reuses the sales-list pattern, filters records, deletes drafts and posts only after confirmation', async ({
  page,
}, info) => {
  test.setTimeout(180_000);
  page.setDefaultTimeout(20_000);
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  const today = new Date();
  const date = `${today.getFullYear()}-${String(today.getMonth() + 1).padStart(2, '0')}-${String(today.getDate()).padStart(2, '0')}`;
  const invoice = (
    recId: number,
    invoiceId: string,
    posted = false,
    customerAccount = 'C-001',
    invoiceDate = date
  ) => ({
    recId,
    invoiceId,
    posted,
    customerAccount,
    invoiceAccount: customerAccount,
    description: 'Consulting',
    invoiceDate,
    dueDate: date,
    currencyCode: 'SAR',
    paymentTerms: 'NET30',
    paymentMethod: 'BANK',
    postingProfile: '',
    defaultDimension: 0,
    taxGroup: 'VAT',
    netAmount: 2000,
    taxAmount: 300,
    invoiceAmount: 2300,
    lines: [],
  });
  let invoices = [
    invoice(1, 'DRAFT-1'),
    invoice(2, 'POSTED-2', true),
    invoice(3, 'OLD-3', false, 'C-001', '2020-01-01'),
    invoice(4, 'OTHER-CUSTOMER', false, 'C-002'),
    invoice(5, 'DRAFT-5'),
  ];
  let posts = 0,
    deletes = 0;
  await page.route('http://localhost:33319/api/**', async (route) => {
    const p = new URL(route.request().url()).pathname;
    const method = route.request().method();
    let data: unknown = [];
    if (p.endsWith('/CustTable/list'))
      data = [
        {
          recId: 10,
          party: 10,
          accountNumber: 'C-001',
          name: 'Customer one',
          customerGroupId: 'Retail',
          currencyCode: 'SAR',
          status: 'active',
        },
      ];
    if (p.endsWith('/inventory-dimensions')) data = { sites: [], warehouses: [] };
    if (p.endsWith('/CustInvoiceTable') && method === 'GET') data = invoices;
    if (p.endsWith('/CustInvoiceTable/5') && method === 'DELETE') {
      expect(invoices.find((row) => row.recId === 5)?.posted).toBe(false);
      invoices = invoices.filter((row) => row.recId !== 5);
      deletes++;
      data = {};
    }
    if (p.endsWith('/CustInvoiceTable/1/post') && method === 'POST') {
      posts++;
      invoices = invoices.map((row) => (row.recId === 1 ? { ...row, posted: true } : row));
      data = invoices.find((row) => row.recId === 1);
    }
    await route.fulfill({ json: { success: true, data } });
  });
  await page.goto('/accounts-receivable/customers/10');
  await expect(page.getByText('C-001 : Customer one')).toBeVisible({ timeout: 60_000 });
  await page.getByRole('button', { name: 'Invoice', exact: true }).click();
  await page.getByRole('button', { name: 'Free text invoices', exact: true }).click();
  await expect(page).toHaveURL(/free-text-invoices\?customerAccount=C-001$/);
  await expect(page.getByText('DRAFT-1', { exact: true })).toBeVisible();
  await expect(page.getByText('POSTED-2', { exact: true })).toHaveCount(0);
  await expect(page.getByText('OLD-3', { exact: true })).toHaveCount(0);
  await expect(page.getByText('OTHER-CUSTOMER', { exact: true })).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'New', exact: true })).toBeEnabled();
  await expect(page.getByRole('button', { name: 'Edit', exact: true })).toBeEnabled();
  await page.getByRole('combobox', { name: 'Invoice date', exact: true }).click();
  await page.getByRole('option', { name: 'All dates', exact: true }).click();
  await expect(page.getByText('OLD-3', { exact: true })).toBeVisible();
  await page.getByRole('combobox', { name: 'Invoice date', exact: true }).click();
  await page.getByRole('option', { name: 'Last 30 days', exact: true }).click();
  await page.getByRole('textbox', { name: 'Invoice number', exact: true }).fill('DRAFT-5');
  await page.locator('[data-row-id="5"]').getByText('Consulting', { exact: true }).click();
  await page.getByRole('button', { name: 'Delete', exact: true }).click();
  await page.getByRole('dialog').getByRole('button', { name: 'Delete', exact: true }).click();
  await expect.poll(() => deletes).toBe(1);
  await page.getByRole('textbox', { name: 'Invoice number', exact: true }).fill('');
  await page.locator('[data-row-id="1"]').getByText('Consulting', { exact: true }).click();
  await page.getByRole('button', { name: 'Totals', exact: true }).click();
  await expect(
    page.getByRole('dialog').getByText('Sales tax: 300.00', { exact: true })
  ).toBeVisible();
  await page.getByRole('dialog').getByRole('button', { name: 'Close', exact: true }).last().click();
  await page.getByRole('button', { name: 'Post', exact: true }).click();
  expect(posts).toBe(0);
  await page.getByRole('dialog').getByRole('button', { name: 'Post', exact: true }).click();
  await expect.poll(() => posts).toBe(1);
  await page.getByRole('combobox', { name: 'Status', exact: true }).click();
  await page.getByRole('option', { name: 'Posted', exact: true }).click();
  await page.locator('[data-row-id="1"]').getByText('Consulting', { exact: true }).click();
  await expect(page.getByRole('button', { name: 'Totals', exact: true })).toBeEnabled();
  await expect(page.getByRole('button', { name: 'Post', exact: true })).toBeDisabled();
  await expect(page.getByRole('button', { name: 'Delete', exact: true })).toBeDisabled();
  await page.screenshot({ path: info.outputPath('free-text-invoices.png'), fullPage: true });
});
