import { expect, test } from '@playwright/test';

test('customer payment journal list and editable lines preserve save, settlement and posting boundaries', async ({
  page,
}) => {
  test.setTimeout(150_000);
  page.setDefaultTimeout(20_000);
  await page.setViewportSize({ width: 1880, height: 1000 });
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  const header = {
    recId: 21,
    dataAreaId: 'dat',
    journalNum: 'PAY-0021',
    journalName: 'CPS',
    name: 'Customer receipts',
    journalType: 1,
    posted: 0,
    systemBlocked: 0,
    currencyCode: 'SAR',
    defaultDimension: 0,
    offsetAccountType: 0,
    offsetLedgerDimension: 100,
    fixedOffsetAccount: 0,
    postedDateTime: '',
    numOfLines: 1,
  };
  let lines: Record<string, unknown>[] = [
    {
      recId: 31,
      dataAreaId: 'dat',
      journalNum: header.journalNum,
      lineNum: 1,
      voucher: 'RV-0031',
      transDate: '2026-11-21T00:00:00',
      company: 'dat',
      accountType: 1,
      ledgerDimensionName: 'C-001',
      ledgerDimension: 0,
      amountCurDebit: 0,
      amountCurCredit: 3375,
      currencyCode: 'SAR',
      offsetAccountType: 0,
      offsetLedgerDimension: 100,
      txt: 'Receipt',
      invoice: '',
      paymMode: 'SPAN',
      paymReference: '',
      documentNum: '',
      documentDate: '2026-11-21T00:00:00',
      due: '2026-11-21T00:00:00',
      noEdit: 0,
      markedInvoiceRecId: 0,
      settleVoucher: 0,
      rowVersion: 'preserve-token',
      preservedSetting: 42,
    },
  ];
  let headers = [header];
  let saves = 0;
  let posts = 0;
  await page.route('http://localhost:33319/api/**', async (route) => {
    const req = route.request();
    const path = new URL(req.url()).pathname;
    let data: unknown = [];
    if (path.endsWith('/LedgerJournalTable') && req.method() === 'POST') {
      const input = req.postDataJSON();
      expect(input.journalName).toBe('CPS');
      expect(input.name).toBe('New receipt batch');
      data = { ...header, ...input, recId: 22, journalNum: 'PAY-0022', numOfLines: 0 };
      headers.push(data as typeof header);
    } else if (path.endsWith('/LedgerJournalTable/22') && req.method() === 'DELETE') {
      headers = headers.filter((row) => row.recId !== 22);
      data = true;
    } else if (path.endsWith('/LedgerJournalTable/by-type/1')) data = headers;
    else if (path.endsWith('/LedgerJournalTable')) data = headers;
    else if (path.endsWith('/LedgerJournalTable/21')) data = header;
    else if (path.endsWith('/LedgerJournalName')) data = [{ ...header, recId: 1 }];
    else if (path.endsWith('/offset-accounts'))
      data = [
        { id: 100, code: '11010202-SAL-NAJ', name: 'Cash', accountType: 0 },
        { id: 200, code: 'BANK-1', name: 'Main bank', accountType: 5 },
      ];
    else if (path.endsWith('/CustTable/list'))
      data = [
        {
          recId: 1,
          accountNumber: 'C-001',
          name: 'Najran Cash',
          currencyCode: 'SAR',
          paymModeId: 'SPAN',
        },
        { recId: 2, accountNumber: 'C-002', name: 'Contoso', currencyCode: 'SAR' },
      ];
    else if (path.endsWith('/Currency'))
      data = [{ recId: 1, currencyCode: 'SAR', txt: 'Saudi riyal' }];
    else if (path.endsWith('/CustPaymMode'))
      data = [
        { recId: 1, paymMode: 'SPAN', name: 'Card', accountType: 0, paymentLedgerDimension: 100 },
      ];
    else if (path.endsWith('/LedgerJournalTrans') && req.method() === 'POST') {
      const input = req.postDataJSON();
      expect(input.ledgerDimensionName).toBe('C-001');
      expect(input.offsetAccountType).toBe(5);
      expect(input.offsetLedgerDimension).toBe(200);
      data = { ...input, recId: 32, lineNum: 2 };
      lines.push(data as Record<string, unknown>);
    } else if (path.endsWith('/LedgerJournalTrans/32') && req.method() === 'DELETE') {
      lines = lines.filter((row) => row.recId !== 32);
      data = true;
    } else if (path.includes('/LedgerJournalTrans/by-journal/'))
      data = lines.filter((row) => path.endsWith(`/by-journal/${row.journalNum}`));
    else if (path.endsWith('/LedgerJournalTrans')) data = lines;
    else if (path.endsWith('/LedgerJournalTrans/31') && req.method() === 'PUT') {
      const input = req.postDataJSON();
      expect(input.rowVersion).toBe('preserve-token');
      expect(input.preservedSetting).toBe(42);
      lines = [input];
      saves++;
      data = input;
    } else if (path.endsWith('/open-transactions'))
      data = {
        transactions: [
          {
            recId: 1,
            transRecId: 900,
            invoice: 'INV-900',
            currencyCode: 'SAR',
            amountCur: 3375,
            transDate: '2026-10-20',
          },
        ],
      };
    else if (path.endsWith('/21/validate'))
      return route.fulfill({
        json: { valid: true, errors: [], unverifiedChecks: [], scope: 'Static validation only.' },
      });
    else if (path.endsWith('/21/post')) {
      posts++;
      header.posted = 1;
      return route.fulfill({ json: { journalNum: header.journalNum } });
    }
    await route.fulfill({ json: { success: true, data } });
  });
  await page.goto('/accounts-receivable/LedgerJournalListCustPaym');
  await page.getByRole('button', { name: 'New', exact: true }).click();
  const journalName = page
    .locator('[role="gridcell"][data-field="journalName"]')
    .getByRole('combobox');
  await journalName.click();
  await page.getByRole('option', { name: /CPS/ }).click();
  await page.getByRole('textbox', { name: 'Description', exact: true }).fill('New receipt batch');
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect(page.getByRole('link', { name: 'PAY-0022', exact: true })).toBeVisible();
  await page.screenshot({ path: '../.validation/LedgerJournalListCustPaym.png', fullPage: true });
  await page.locator('[data-row-id="22"] [data-field="name"]').click();
  await page.getByRole('button', { name: 'Delete', exact: true }).click();
  await page.getByRole('dialog').getByRole('button', { name: 'Delete', exact: true }).click();
  await expect.poll(() => headers.length).toBe(1);
  await page.getByRole('link', { name: 'PAY-0021', exact: true }).click();
  await expect(page).toHaveURL(/LedgerJournalTransCustPaym\/21/);
  await expect(page.getByText('Najran Cash', { exact: true }).first()).toBeVisible();
  await page.getByRole('button', { name: 'Edit Description', exact: true }).click();
  await page.getByRole('textbox', { name: 'txt', exact: true }).fill('Updated receipt');
  await expect(page.getByRole('button', { name: 'Post', exact: true })).toBeDisabled();
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect.poll(() => saves).toBe(1);
  expect(posts).toBe(0);
  await page.getByRole('button', { name: 'Settle transactions', exact: true }).click();
  await page.getByRole('button', { name: 'Mark', exact: true }).click();
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect.poll(() => saves).toBe(2);
  expect(lines[0].markedInvoiceRecId).toBe(900);
  expect(posts).toBe(0);
  await page.getByRole('button', { name: 'Edit Account', exact: true }).click();
  await page
    .locator('[role="gridcell"][data-field="ledgerDimensionName"]')
    .getByRole('combobox')
    .click();
  await page.getByRole('option', { name: /C-002/ }).click();
  await expect(page).toHaveURL(/LedgerJournalTransCustPaym\/21/);
  await expect(page.locator('[role="gridcell"][data-field="invoice"]')).toHaveText('');
  await page.getByRole('button', { name: 'Cancel', exact: true }).click();
  await expect(page.locator('[role="gridcell"][data-field="invoice"]')).toHaveText('INV-900');
  await page.getByRole('button', { name: '+ New', exact: true }).click();
  await page
    .locator('[role="gridcell"][data-field="ledgerDimensionName"]')
    .getByRole('combobox')
    .click();
  await page.getByRole('option', { name: /C-001/ }).click();
  await page.getByRole('spinbutton', { name: 'Credit', exact: true }).fill('125');
  await page
    .locator('[role="gridcell"][data-field="offsetAccountType"]')
    .getByRole('combobox')
    .click();
  await page.getByRole('option', { name: 'Bank', exact: true }).click();
  await page
    .locator('[role="gridcell"][data-field="offsetLedgerDimension"]')
    .getByRole('combobox')
    .click();
  await page.getByRole('option', { name: /BANK-1/ }).click();
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect.poll(() => lines.length).toBe(2);
  await page.getByRole('button', { name: 'Delete', exact: true }).click();
  await page.getByRole('dialog').getByRole('button', { name: 'Delete', exact: true }).click();
  await expect.poll(() => lines.length).toBe(1);
  await page.getByRole('button', { name: 'Validate', exact: true }).click();
  await expect(page.getByText('Static validation passed.')).toBeVisible();
  await page.getByRole('dialog').getByRole('button', { name: 'Close' }).click();
  await page.locator('[role="gridcell"][data-field="transDate"]').scrollIntoViewIfNeeded();
  await page.screenshot({ path: '../.validation/LedgerJournalTransCustPaym.png', fullPage: true });
  await page.getByRole('button', { name: 'Post', exact: true }).click();
  await page.getByRole('dialog').getByRole('button', { name: 'Post', exact: true }).click();
  await expect.poll(() => posts).toBe(1);
  await expect(page.getByText('Posted journal — lines are read-only.')).toBeVisible();
  await expect(page.getByRole('button', { name: 'Edit', exact: true })).toBeDisabled();
  await expect(page.getByRole('button', { name: '+ New', exact: true })).toBeDisabled();
});
