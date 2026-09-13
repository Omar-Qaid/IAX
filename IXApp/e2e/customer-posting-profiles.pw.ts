import { expect, test, type Page } from '@playwright/test';
test.setTimeout(60_000);
const url = '/accounts-receivable/customer-posting-profiles';
const ok = (data: unknown) => ({ success: true, data });
async function setup(page: Page) {
  const profile = {
    recId: 1,
    postingProfile: 'GEN',
    name: 'General',
    settlement: 1,
    interest: 1,
    collectionLetter: 1,
    postingProfileName: '',
    dataAreaId: 'USMF',
    isActive: true,
    rowVersion: 'AQ==',
    recVersion: 1,
  };
  let account = {
    ...profile,
    recId: 10,
    accountCode: 1,
    num: 'Consultant',
    collectionLetterCourse: '',
    summaryLedgerDimension: 11020005,
    clearingLedgerDimension: 0,
    vatPrepaymentsLedgerDimension: 0,
    liabilitiesForDiscountLedgerDimension: 0,
    custInterest: 0,
    depositLedgerDimension: 42,
    endorseLedgerDimension: 0,
    exportSalesLedgerDimension: 0,
    writeOffLedgerDimension: 0,
  };
  const writes: Record<string, unknown>[] = [];
  let profileReads = 0;
  await page.route('**/api/v1/CustLedger', (route) => {
    profileReads++;
    return route.fulfill({ json: ok([profile]) });
  });
  await page.route('**/api/v1/CustLedger/1', (route) => {
    Object.assign(profile, route.request().postDataJSON());
    return route.fulfill({ json: ok(profile) });
  });
  await page.route('**/api/v1/CustLedgerAccounts/profile/GEN', (route) =>
    route.fulfill({ json: ok([account]) })
  );
  await page.route('**/api/v1/CustLedgerAccounts/10', (route) => {
    const body = route.request().postDataJSON();
    writes.push(body);
    account = { ...account, ...body };
    return route.fulfill({ json: ok(account) });
  });
  await page.route('**/api/v1/CustGroup', (route) =>
    route.fulfill({
      json: ok([
        { custGroupId: 'Consultant', name: 'Consultants' },
        { custGroupId: 'Traders', name: 'Trading customers' },
      ]),
    })
  );
  await page.route('**/api/v1/CustTable/list', (route) =>
    route.fulfill({
      json: ok([
        { recId: 100, accountNumber: 'C-100', name: 'Customer One', customerGroupId: 'Consultant' },
      ]),
    })
  );
  await page.goto(url);
  await expect(
    page.getByRole('heading', { name: 'Customer posting profiles', level: 1 })
  ).toBeVisible({ timeout: 30_000 });
  await expect(page.getByLabel('Summary account', { exact: true })).toHaveValue('11020005');
  return { writes, reads: () => profileReads };
}
async function edit(page: Page) {
  await page.getByRole('button', { name: 'Edit account', exact: true }).click();
}
async function code(page: Page, name: string) {
  await page.getByRole('combobox', { name: 'Account code', exact: true }).click();
  await page.getByRole('option', { name, exact: true }).click();
}
async function choose(page: Page, name: string) {
  const input = page.getByRole('textbox', { name: 'Account/Group number', exact: false });
  await expect(input).toBeEnabled();
  await input.click();
  await page.getByRole('dialog').getByRole('button', { name }).click();
}
test('Group uses customer groups; Table uses customers; All clears and disables the lookup', async ({
  page,
}, info) => {
  await page.setViewportSize({ width: 1868, height: 841 });
  const { writes } = await setup(page);
  await edit(page);
  await choose(page, 'Traders - Trading customers');
  await page.getByRole('button', { name: 'Save account', exact: true }).click();
  await expect(page.getByRole('button', { name: 'Edit account', exact: true })).toBeVisible();
  expect(writes[0]).toMatchObject({
    accountCode: 1,
    num: 'Traders',
    depositLedgerDimension: 42,
    rowVersion: 'AQ==',
  });
  await edit(page);
  await code(page, 'Table');
  await expect(
    page.getByRole('textbox', { name: 'Account/Group number', exact: false })
  ).toHaveValue('');
  await choose(page, 'C-100 - Customer One');
  await page.screenshot({ path: info.outputPath('customer-lookup.png'), fullPage: true });
  await page.getByRole('button', { name: 'Save account', exact: true }).click();
  await expect(page.getByRole('button', { name: 'Edit account', exact: true })).toBeVisible();
  expect(writes[1]).toMatchObject({ accountCode: 0, num: 'C-100' });
  await edit(page);
  await code(page, 'All');
  await expect(
    page.getByRole('textbox', { name: 'Account/Group number', exact: true })
  ).toBeDisabled();
  await page.getByRole('button', { name: 'Save account', exact: true }).click();
  await expect(page.getByRole('button', { name: 'Edit account', exact: true })).toBeVisible();
  expect(writes[2]).toMatchObject({ accountCode: 2, num: '' });
});
test('rejects empty and deleted references without submitting', async ({ page }) => {
  const { writes } = await setup(page);
  await edit(page);
  await code(page, 'Table');
  await page.getByRole('button', { name: 'Save account', exact: true }).click();
  await expect(
    page.getByText('Select an existing customer for Table or an existing customer group for Group.')
  ).toBeVisible();
  expect(writes).toHaveLength(0);
  await choose(page, 'C-100 - Customer One');
  await page.route('**/api/v1/CustTable/list', (route) => route.fulfill({ json: ok([]) }));
  await page.getByRole('button', { name: 'Save account', exact: true }).click();
  await expect(
    page.getByText('Select an existing customer for Table or an existing customer group for Group.')
  ).toBeVisible();
  expect(writes).toHaveLength(0);
});
test('opens the shared lookup popup while customer references are loading', async ({ page }) => {
  await setup(page);
  await page.route('**/api/v1/CustTable/list', async (route) => {
    await new Promise((resolve) => setTimeout(resolve, 1_000));
    await route.fulfill({
      json: ok([
        { recId: 100, accountNumber: 'C-100', name: 'Customer One', customerGroupId: 'Consultant' },
      ]),
    });
  });
  await edit(page);
  await code(page, 'Table');
  await page.getByRole('textbox', { name: 'Account/Group number', exact: false }).click();
  await expect(page.getByRole('dialog')).toBeVisible();
  await expect(page.getByRole('dialog').getByRole('progressbar')).toBeVisible();
  await expect(
    page.getByRole('dialog').getByRole('button', { name: 'C-100 - Customer One' })
  ).toBeVisible();
});
test('saved profiles survive SPA navigation from the query cache', async ({ page }) => {
  const state = await setup(page);
  await page.getByRole('button', { name: 'Edit', exact: true }).click();
  await page.getByRole('textbox', { name: 'Description', exact: true }).fill('Updated profile');
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect(page.getByRole('textbox', { name: 'Description', exact: true })).toHaveCount(0);
  await page
    .getByRole('navigation', { name: 'Breadcrumbs' })
    .getByRole('button', { name: 'Accounts Receivable' })
    .click();
  await expect(page).toHaveURL(/\/customers$/);
  await page.goBack();
  await expect(
    page.getByRole('heading', { name: 'Customer posting profiles', level: 1 })
  ).toBeVisible();
  await expect(page.getByText('Updated profile', { exact: true }).first()).toBeVisible();
  expect(state.reads()).toBe(1);
});
test('canceling navigation retains a draft; accepting discards it', async ({ page }) => {
  await setup(page);
  await edit(page);
  await page.getByLabel('Summary account', { exact: true }).fill('12345');
  const canceled = page.waitForEvent('dialog').then((dialog) => dialog.dismiss());
  await page
    .getByRole('navigation', { name: 'Breadcrumbs' })
    .getByRole('button', { name: 'Accounts Receivable' })
    .click();
  await canceled;
  await expect(page).toHaveURL(new RegExp(`${url}$`));
  await expect(page.getByLabel('Summary account', { exact: true })).toHaveValue('12345');
  const accepted = page.waitForEvent('dialog').then((dialog) => dialog.accept());
  await page
    .getByRole('navigation', { name: 'Breadcrumbs' })
    .getByRole('button', { name: 'Accounts Receivable' })
    .click();
  await accepted;
  await expect(page).toHaveURL(/\/customers$/);
});
