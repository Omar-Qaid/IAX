import { expect, test } from '@playwright/test';
test('sales order invoice ribbon opens invoice-account settlement without removed columns', async ({
  page,
}, info) => {
  test.skip(info.project.name !== 'desktop-chromium');
  test.setTimeout(120000);
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  let loadedAccount = '';
  const order = {
    recId: 1,
    salesId: 'SO-SETTLE',
    customerAccount: 'C-ORDER',
    invoiceAccount: 'C-INV',
    customerName: 'Order customer',
    currencyCode: 'SAR',
    salesStatus: 'Backorder',
    documentStatus: 'None',
  };
  await page.route('http://localhost:33319/api/**', async (route) => {
    const url = new URL(route.request().url());
    let data: unknown = [];
    if (url.pathname.endsWith('/SalesTable/list')) data = [order];
    if (url.pathname.endsWith('/SalesTable/1/details')) data = order;
    if (url.pathname.endsWith('/customer-settlements/open-transactions')) {
      loadedAccount = url.searchParams.get('accountNum') ?? '';
      data = {
        accountNum: 'C-INV',
        customerName: 'Invoice customer',
        dataAreaId: 'dat',
        customerCurrency: 'SAR',
        accountingCurrency: 'SAR',
        customerBalanceMst: 100,
        reconciled: true,
        transactions: [
          {
            recId: 10,
            transRecId: 20,
            invoice: 'INV-001',
            voucher: 'V-001',
            description: 'Invoice',
            currencyCode: 'SAR',
            dueDate: '2026-11-20',
            transDate: '2026-11-01',
            cashDiscDate: '',
            amountCur: 100,
            amountMst: 100,
            originalAmountCur: 100,
            originalAmountMst: 100,
            settledAmountCur: 0,
            settledAmountMst: 0,
            possibleCashDisc: 0,
            cashDiscTaken: 0,
            worker: 'W1',
            department: 'D1',
            project: 'P1',
            costCenter: 'CC1',
          },
        ],
      };
    }
    await route.fulfill({ json: { success: true, data } });
  });
  await page.goto('/accounts-receivable/sales-orders/1', { waitUntil: 'domcontentloaded' });
  await page
    .getByRole('button', { name: 'Invoice', exact: true })
    .first()
    .click({ timeout: 90000 });
  await page.getByRole('button', { name: 'Open transactions', exact: true }).click();
  const panel = page.getByRole('dialog', { name: /Settle transactions for Invoice customer/ });
  await expect(panel).toBeVisible();
  expect(loadedAccount).toBe('C-INV');
  const filterInput = panel.locator('input[data-grid-filter-field="invoice"]');
  await expect(filterInput).toHaveCount(0);
  await expect(panel.getByText('INV-001', { exact: true })).toBeVisible();
  for (const label of ['Worker ID', 'Project', 'Department', 'Cost center'])
    await expect(panel.getByRole('columnheader', { name: label, exact: true })).toHaveCount(0);
  await page.screenshot({ path: info.outputPath('sales-order-settlement.png'), fullPage: true });
});
