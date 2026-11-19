import { expect, test } from '@playwright/test';

test('customer tab exposes confirmed basic actions and opens the screenshot-aligned balance', async ({
  page,
}, testInfo) => {
  test.setTimeout(120_000);
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  await page.route('http://localhost:33319/api/**', async (route) => {
    const url = new URL(route.request().url());
    let data: unknown = [];
    if (url.pathname.endsWith('/CustTable/list'))
      data = [
        {
          recId: 1,
          party: 1,
          accountNumber: 'C-001',
          name: 'Example customer',
          nameAr: 'عميل الاختبار',
          customerGroupId: 'Retail',
          currencyCode: 'SAR',
          status: 'active',
        },
      ];
    if (url.pathname.endsWith('/CustTrans/open-balance')) {
      expect(url.searchParams.get('accountNum')).toBe('C-001');
      data = {
        accountNum: 'C-001',
        asOfDate: '2026-11-15',
        reconciled: true,
        transactions: [
          {
            recId: 11,
            invoice: 'INV-001',
            voucher: 'V-001',
            transDate: '2026-11-01',
            dueDate: '2026-11-30',
            currencyCode: 'SAR',
            amountCur: 1000,
            settleAmountCur: 300,
            openAmountCur: 700,
            openAmountMst: 700,
            daysOverdue: 0,
          },
        ],
      };
    }
    if (url.pathname.endsWith('/SalesTable/list'))
      data = [
        {
          recId: 1,
          salesId: 'SO-C001',
          customerAccount: 'C-001',
          customerName: 'Example customer',
          salesStatus: 'Open',
          documentStatus: 'None',
          orderTotal: 700,
          currencyCode: 'SAR',
        },
        {
          recId: 2,
          salesId: 'SO-C002',
          customerAccount: 'C-002',
          customerName: 'Other customer',
          salesStatus: 'Open',
          documentStatus: 'None',
          orderTotal: 900,
          currencyCode: 'SAR',
        },
      ];
    if (url.pathname.endsWith('/SalesTable/1/packing-slips'))
      data = [
        { id: '10', salesId: 'SO-C001', packingSlipId: 'PS-C001', deliveryDate: '2026-11-15' },
      ];
    if (url.pathname.endsWith('/inventory-dimensions')) data = { sites: [], warehouses: [] };
    if (url.pathname.endsWith('/CustTrans/balance-currency')) {
      expect(url.searchParams.get('accountNum')).toBe('C-001');
      data = {
        recId: 1,
        accountNum: 'C-001',
        name: 'Example customer',
        nameAlias: 'عميل الاختبار',
        currencyCode: 'SAR',
        accountingCurrency: 'SAR',
        reportingCurrency: '',
        balanceMst: 707782.41,
        balanceReporting: null,
        creditMax: 1000,
        reconciled: true,
        relatedAccounts: [],
        currencies: [
          {
            currencyCode: 'SAR',
            amountCur: 707782.41,
            amountMst: 707782.41,
            amountReporting: null,
          },
        ],
      };
    }
    await route.fulfill({ json: { success: true, data } });
  });
  await page.goto('/accounts-receivable/customers/1');
  await expect(page.getByText('C-001 : Example customer')).toBeVisible({ timeout: 60_000 });
  await page.getByRole('button', { name: 'Customer', exact: true }).click();
  const ribbon = page.locator('#customer-action-ribbon');
  await expect(ribbon.getByRole('button', { name: 'Balance', exact: true })).toBeEnabled();
  await expect(ribbon.getByRole('button', { name: 'Attachments', exact: true })).toBeEnabled();
  await expect(page.getByRole('button', { name: 'Forecast', exact: true })).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Credit cards', exact: true })).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Global transactions', exact: true })).toHaveCount(
    0
  );
  await page.getByRole('button', { name: 'Balance', exact: true }).click();
  await expect(page).toHaveURL(/customer-balances\/C-001$/);
  await expect(page.getByRole('heading', { name: 'Customer balance', exact: true })).toBeVisible({
    timeout: 30_000,
  });
  await expect(page.getByText('-706,782.41', { exact: true })).toBeVisible();
  await expect(page.getByText('Accounting currency balance', { exact: true })).toBeVisible();
  await expect(
    page.getByRole('button', { name: 'Toggle record list', exact: true })
  ).toHaveAttribute(
    'aria-pressed',
    testInfo.project.name === 'desktop-chromium' ? 'true' : 'false'
  );
  await expect(page.getByRole('button', { name: 'Edit', exact: true })).toBeDisabled();
  await expect(page.getByRole('button', { name: 'New', exact: true })).toBeDisabled();
  await expect(page.getByRole('button', { name: 'Delete', exact: true })).toBeDisabled();
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  await expect(page.locator('[role="grid"] input:not([type="radio"])').first()).toBeVisible();
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  await page.screenshot({ path: testInfo.outputPath('customer-balance.png'), fullPage: true });
  const overflows = await page.evaluate(
    () => document.documentElement.scrollWidth > document.documentElement.clientWidth + 1
  );
  expect(overflows).toBe(false);
  const selectedRow = page.locator('[data-row-id="currency-SAR"]');
  await expect(selectedRow).toHaveAttribute('aria-selected', 'true');
  await expect(selectedRow.locator('[data-field="amountMst"]')).toHaveCSS(
    'border-bottom-width',
    '0px'
  );
  await expect(selectedRow.locator(':scope > div').last()).toHaveCSS('border-bottom-width', '0px');
  await page.getByRole('button', { name: 'Customer', exact: true }).click();
  await page.getByRole('button', { name: 'Open transactions', exact: true }).click();
  await expect(page).toHaveURL(/customer-open-transactions\/C-001$/);
  await expect(page.getByText('INV-001', { exact: true })).toBeVisible({ timeout: 30_000 });
  await expect(page.getByRole('button', { name: 'Post', exact: true })).toHaveCount(0);
  await page.goBack();
  await page.getByRole('button', { name: 'Back', exact: true }).click();
  await expect(page).toHaveURL(/customers\/1$/);
  await page.getByRole('button', { name: 'Sell', exact: true }).click();
  await page.getByRole('button', { name: 'Sales order', exact: true }).click();
  await expect(page.getByRole('combobox', { name: 'Customer account', exact: true })).toHaveText(
    'C-001 - Example customer'
  );
  await expect(page.getByRole('button', { name: 'Save', exact: true })).toBeEnabled({
    timeout: 30_000,
  });
  await page.getByRole('button', { name: 'Cancel', exact: true }).click();
  await page.getByRole('button', { name: 'Sell', exact: true }).click();
  await page.getByRole('button', { name: 'Packing slips', exact: true }).click();
  await expect(page).toHaveURL(/customer-packing-slips\/C-001$/);
  await expect(page.getByText('PS-C001', { exact: true })).toBeVisible({ timeout: 30_000 });
  await page.goBack();
  await expect(page.getByText('C-001 : Example customer')).toBeVisible();
  await page.getByRole('button', { name: 'Sell', exact: true }).click();
  await page.getByRole('button', { name: 'Orders', exact: true }).click();
  await page.getByRole('menuitem', { name: 'Sales orders', exact: true }).click();
  await expect(page).toHaveURL(/sales-orders\?customerAccount=C-001$/);
  await expect(page.getByText('SO-C001', { exact: true })).toBeVisible({ timeout: 30_000 });
  await expect(page.getByText('SO-C002', { exact: true })).toHaveCount(0);
});
