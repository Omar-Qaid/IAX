import { expect, test } from '@playwright/test';

test('lists, creates, and edits a customer through the Customers pages', async ({ page }) => {
  test.setTimeout(90_000);
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  const path = '/accounts-receivable/customers';
  const records = [
    { recId: 1, accountNumber: 'C-001', name: 'Existing customer', customerGroupId: 'Retail', currencyCode: 'SAR' },
  ];
  await page.route('http://localhost:33319/api/**', async (route) => {
    const request = route.request();
    const endpoint = new URL(request.url()).pathname;
    let data: unknown = [];
    if (endpoint.endsWith('/inventory-dimensions')) data = { sites: [], warehouses: [] };
    if (endpoint.endsWith('/CustTable/list')) data = records;
    if (endpoint.endsWith('/CustGroup')) data = [{ custGroupId: 'Retail', name: 'Retail customers' }];
    if (endpoint.endsWith('/country-regions')) data = [{ countryRegionId: 'SAU', isoCode: 'SA' }];
    if (endpoint.endsWith('/Currency')) data = [{ currencyCode: 'SAR', txt: 'Saudi riyal' }];
    if (endpoint.endsWith('/CustTable/quick-create')) {
      const input = request.postDataJSON();
      expect(input).toMatchObject({ name: 'New customer', custGroupId: 'Retail', countryRegionId: 'SAU' });
      const created = { recId: 2, accountNumber: 'C-002', name: input.name, customerGroupId: input.custGroupId, currencyCode: 'SAR' };
      records.push(created);
      data = created;
    }
    if (endpoint.endsWith('/CustTable/quick-update/2')) {
      const input = request.postDataJSON();
      expect(input.name).toBe('Edited customer');
      records[1].name = input.name;
      data = records[1];
    }
    await route.fulfill({ json: { success: true, data } });
  });

  await page.goto(path);
  const existing = page.getByRole('link', { name: 'C-001' });
  await expect(existing).toHaveAttribute('href', `${path}/1`);
  await existing.click();
  await expect(page.getByText('C-001 : Existing customer')).toBeVisible();
  await page.goBack();
  await page.getByRole('button', { name: 'New', exact: true }).click();
  const drawer = page.getByRole('dialog', { name: 'Create customer' });
  await drawer.getByRole('textbox', { name: 'Name', exact: true }).fill('New customer');
  for (const [field, option] of [['Customer category', 'Retail'], ['Customer group', 'Retail customers'], ['Country/region', 'SAU - SA']]) {
    await drawer.getByRole('combobox', { name: field, exact: true }).click();
    await page.getByRole('option', { name: option, exact: true }).click();
  }
  await drawer.getByRole('button', { name: 'Save and open' }).click();
  await expect(page).toHaveURL(new RegExp(`${path}/2$`));
  await expect(page.getByText('C-002 : New customer')).toBeVisible();
  await page.getByRole('button', { name: 'Edit', exact: true }).and(page.locator('button:enabled')).click();
  await page.getByRole('textbox', { name: 'Name', exact: true }).fill('Edited customer');
  await page.getByRole('button', { name: 'Save', exact: true }).and(page.locator('button:enabled')).click();
  await expect(page.getByText('C-002 : Edited customer')).toBeVisible();
  await page.goto(path);
  await expect(page.getByText('Edited customer', { exact: true })).toBeVisible();
  await page.goto(`${path}/999`);
  await expect(page.getByText('Customer not found.', { exact: true })).toBeVisible({ timeout: 15_000 });
  await expect(page.getByText('C-001 : Existing customer')).toHaveCount(0);
  await expect(page.getByText('C-002 : Edited customer')).toHaveCount(0);
});
