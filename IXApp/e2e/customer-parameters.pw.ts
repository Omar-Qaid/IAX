import { expect, test, type Page } from '@playwright/test';

test.setTimeout(60_000);
const path = '/accounts-receivable/customer-parameters';
async function setup(page: Page, failFirst = false, missing = false) {
  let record: Record<string, unknown> = {
    recId: 4,
    key: 0,
    dataAreaId: 'USMF',
    rowVersion: 'AQ==',
    recVersion: 1,
    isActive: true,
    custPostingProfile: 'GEN',
    paymTermId: 'NET30',
    currencyCode: 'SAR',
    creditLimit: 250,
    taxOnPrepayment: 0,
    reservation: 2,
    salesOrderType: 3,
    creditLimitCheck: 1,
    partition: 44,
  };
  const writes: Record<string, unknown>[] = [];
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  await page.route('http://localhost:33319/api/**', (route) =>
    route.fulfill({ json: { success: true, data: [] } })
  );
  await page.route('http://localhost:33319/api/v1/CustParameters**', async (route) => {
    expect(route.request().headers()['x-company']).toBe('USMF');
    if (route.request().method() === 'PUT') {
      const body = route.request().postDataJSON();
      writes.push(body);
      if (failFirst) {
        failFirst = false;
        await route.fulfill({
          status: 409,
          json: { success: false, message: 'Record changed. Please retry.' },
        });
        return;
      }
      record = {
        ...body,
        rowVersion: `${Number(record.recVersion) + 1}`,
        recVersion: Number(record.recVersion) + 1,
      };
      await route.fulfill({ json: { success: true, data: record } });
      return;
    }
    await route.fulfill({ json: { success: true, data: missing ? [] : [record] } });
  });
  await page.goto(path);
  return {
    writes,
    configure: () => {
      missing = false;
    },
  };
}

test('customer parameters persist changes, reuse the returned version and survive reload', async ({
  page,
}) => {
  const { writes } = await setup(page);
  const paymentTerm = page.getByRole('textbox', { name: 'Terms of payment', exact: true });
  await expect(paymentTerm).toHaveValue('NET30');
  await paymentTerm.fill('NET60');
  await page.getByLabel('Sales tax on prepayments', { exact: true }).check();
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect(page.getByRole('button', { name: 'Save', exact: true })).toBeDisabled();
  expect(writes[0]).toMatchObject({
    paymTermId: 'NET60',
    taxOnPrepayment: 1,
    rowVersion: 'AQ==',
    reservation: 2,
    salesOrderType: 3,
    creditLimitCheck: 1,
    partition: 44,
  });
  await paymentTerm.fill('NET90');
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect(page.getByRole('button', { name: 'Save', exact: true })).toBeDisabled();
  expect(writes[1]).toMatchObject({ paymTermId: 'NET90', rowVersion: '2', recVersion: 2 });
  await page.reload();
  await expect(paymentTerm).toHaveValue('NET90');
});

test('customer parameter validation and rejected saves keep the draft available', async ({
  page,
}) => {
  const { writes } = await setup(page, true);
  const paymentTerm = page.getByRole('textbox', { name: 'Terms of payment', exact: true });
  await paymentTerm.fill('TOO-LONG-TERM');
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('at most 10');
  expect(writes).toHaveLength(0);
  await paymentTerm.fill('NET45');
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect(page.getByRole('alert')).toContainText('Record changed');
  await expect(paymentTerm).toHaveValue('NET45');
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect(page.getByRole('button', { name: 'Save', exact: true })).toBeDisabled();
  expect(writes).toHaveLength(2);
});

test('an unconfigured company shows no invented parameters and can refresh', async ({ page }) => {
  const state = await setup(page, false, true);
  await expect(page.getByRole('alert')).toContainText('have not been configured');
  await expect(page.getByRole('button', { name: 'Save', exact: true })).toHaveCount(0);
  state.configure();
  await page.getByRole('button', { name: 'Refresh', exact: true }).click();
  await expect(page.getByRole('textbox', { name: 'Terms of payment', exact: true })).toHaveValue(
    'NET30'
  );
});
