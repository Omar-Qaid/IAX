import { test, expect } from '@playwright/test';

test('sales tax code posting group lookup provides preview and open details', async ({ page }, info) => {
  test.skip(info.project.name !== 'desktop-chromium');
  test.setTimeout(120000);
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  let savedGroup = '';
  await page.route('http://localhost:33319/api/**', async (route) => {
    const path = new URL(route.request().url()).pathname;
    let data: unknown = [];
    const code = { recId: 1, taxCode: 'VAT15', taxName: 'Standard VAT', taxPeriod: 'MONTH', taxCurrencyCode: 'SAR', taxAccountGroup: savedGroup || 'VAT-STD', taxValue: 15 };
    if (path.endsWith('/TaxTable')) data = [code];
    if (path.endsWith('/TaxLedgerAccountGroup')) data = [{ recId: 1, taxAccountGroup: 'VAT-STD', name: 'Standard VAT' }, { recId: 2, taxAccountGroup: 'VAT-EXP', name: 'Export VAT' }];
    if (path.endsWith('/TaxPeriodHead')) data = [{ recId: 1, taxPeriod: 'MONTH', name: 'Monthly' }];
    if (path.endsWith('/Currency')) data = [{ recId: 1, currencyCode: 'SAR', txt: 'Saudi Riyal' }];
    if (path.endsWith('/TaxTable/1') && route.request().method() === 'PUT') {
      savedGroup = route.request().postDataJSON().taxAccountGroup;
      data = { ...code, ...route.request().postDataJSON() };
    }
    await route.fulfill({ json: { success: true, data } });
  });
  await page.goto('/tax/codes', { waitUntil: 'domcontentloaded' });
  await expect(page.getByRole('button', { name: 'Open Ledger posting group details', exact: true })).toBeVisible({ timeout: 90000 });
  const preview = page.getByText('VAT-STD - Standard VAT', { exact: true }).first();
  await preview.hover();
  await expect(page.getByText('VAT-STD - Standard VAT', { exact: true }).last()).toBeVisible();
  await expect(page.getByRole('button', { name: 'Open Settlement period details', exact: true })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Open Sales tax currency details', exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Edit', exact: true }).click();
  await page.getByRole('combobox', { name: 'Ledger posting group', exact: true }).click();
  await page.getByRole('option', { name: /VAT-EXP/ }).click();
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect.poll(() => savedGroup).toBe('VAT-EXP');
  await page.getByRole('button', { name: 'Open Ledger posting group details', exact: true }).click();
  await expect(page).toHaveURL(/\/tax\/ledger-posting-groups/);
});

test('ledger posting groups edit, cancel, persist account IDs and reopen', async ({
  page,
}, info) => {
  test.skip(info.project.name !== 'desktop-chromium');
  test.setTimeout(120000);
  await page.setViewportSize({ width: 1870, height: 850 });
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  let group = {
    recId: 1,
    taxAccountGroup: 'VAT-STD',
    name: 'Standard VAT',
    taxOutgoingLedgerDimension: null as number | null,
    taxIncomingLedgerDimension: 12,
    taxNonDeductibleTaxLedgerDimension: 99,
  };
  let saves = 0;
  await page.route('http://localhost:33319/api/**', async (route) => {
    const path = new URL(route.request().url()).pathname;
    let data: unknown = [];
    if (path.endsWith('/TaxLedgerAccountGroup')) data = [group];
    if (path.endsWith('/TaxLedgerAccountGroup/ledger-accounts'))
      data = [
        { id: 11, code: '21062203', name: '21062203' },
        { id: 12, code: '21062204', name: '21062204' },
      ];
    if (path.endsWith('/TaxLedgerAccountGroup/1') && route.request().method() === 'PUT') {
      group = { ...group, ...route.request().postDataJSON() };
      saves++;
      data = group;
    }
    await route.fulfill({ json: { success: true, data } });
  });
  await page.goto('/tax/ledger-posting-groups', { waitUntil: 'domcontentloaded' });
  await expect(page.getByText('Ledger posting groups', { exact: true }).first()).toBeVisible({
    timeout: 90000,
  });
  await expect(page.getByText('VAT-STD', { exact: true }).first()).toBeVisible({ timeout: 90000 });
  await page.getByRole('button', { name: 'Edit', exact: true }).click();
  await expect(page.getByRole('button', { name: 'Cancel', exact: true })).toBeVisible();
  const payable = page.getByRole('combobox', { name: 'Sales tax payable', exact: true });
  await payable.click();

  await page.getByRole('option', { name: /21062203/ }).first().click();
  await page.getByRole('button', { name: 'Cancel', exact: true }).click();
  expect(saves).toBe(0);
  await page.getByRole('button', { name: 'Edit', exact: true }).click();
  await payable.click();
  await page.getByRole('option', { name: /21062203/ }).first().click();
  await page.screenshot({ path: info.outputPath('tax-account-group.png'), fullPage: true });
  const labels = ['Sales tax receivable', 'Use tax expense', 'Use tax payable', 'Settlement account', 'Vendor cash discount', 'Customer cash discount'];
  for (const label of labels) {
    const input = page.getByRole('combobox', { name: label, exact: true });
    await input.fill('21062203');
    await page.getByRole('option', { name: /21062203/ }).first().click();
  }
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect.poll(() => saves).toBe(1);
  expect(group.taxOutgoingLedgerDimension).toBe(11);
  expect(group.taxNonDeductibleTaxLedgerDimension).toBe(99);
  for (const key of ['taxIncomingLedgerDimension', 'taxOffsetUseTaxLedgerDimension', 'taxUseTaxLedgerDimension', 'taxReportLedgerDimension', 'cashDiscountIncomingLedgerDimension', 'cashDiscountOutgoingLedgerDimension']) {
    expect((group as Record<string, unknown>)[key]).toBe(11);
  }
  await page.reload({ waitUntil: 'domcontentloaded' });
  await expect(page.getByText('21062203', { exact: true }).first()).toBeVisible();
  await expect(page.getByRole('button', { name: /^Preview / })).toHaveCount(0);
});
