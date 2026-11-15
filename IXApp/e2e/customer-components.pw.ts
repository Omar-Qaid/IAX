import { expect, test } from '@playwright/test';

test('customer components persist edits, recover failures, maintain logistics and navigate selected records', async ({
  page,
}, testInfo) => {
  test.setTimeout(180_000);
  page.setDefaultTimeout(20_000);
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  const records = [
    {
      recId: 1,
      party: 7,
      accountNumber: 'C-001',
      name: 'Audit customer',
      nameAr: 'عميل المراجعة',
      customerGroupId: 'Retail',
      currencyCode: 'SAR',
      status: 'active',
      inventSiteId: 'S1',
      inventLocationId: 'W1',
      countryRegionId: 'SAU',
    },
    {
      recId: 2,
      party: 8,
      accountNumber: 'C-002',
      name: 'Second customer',
      nameAr: '',
      customerGroupId: 'Retail',
      currencyCode: 'SAR',
      status: 'active',
      inventSiteId: '',
      inventLocationId: '',
      countryRegionId: 'SAU',
    },
  ];
  let reads = 0;
  let updates = 0;
  let refuseUpdate = false;
  let next = 10;
  let contacts: Record<string, unknown>[] = [];
  let addresses: Record<string, unknown>[] = [];
  await page.route('http://localhost:33319/api/**', async (route) => {
    const request = route.request();
    const url = new URL(request.url());
    const p = url.pathname;
    const method = request.method();
    let data: unknown = [];
    if (p.endsWith('/CustTable/list')) {
      reads++;
      data = records;
    } else if (p.includes('/CustTable/quick-update/')) {
      updates++;
      if (refuseUpdate) {
        refuseUpdate = false;
        await route.fulfill({ json: { success: false, message: 'Customer update refused' } });
        return;
      }
      const input = request.postDataJSON();
      const row = records.find((r) => r.recId === Number(p.split('/').at(-1)))!;
      Object.assign(row, input, { customerGroupId: input.custGroupId, nameAr: input.nameAlias });
      data = row;
    } else if (p.includes('/CustTable/') && method === 'DELETE') {
      const index = records.findIndex((r) => r.recId === Number(p.split('/').at(-1)));
      records.splice(index, 1);
      data = true;
    } else if (p.endsWith('/CustGroup'))
      data = [{ custGroupId: 'Retail', name: 'Retail customers' }];
    else if (p.endsWith('/Currency')) data = [{ currencyCode: 'SAR', txt: 'Saudi riyal' }];
    else if (p.endsWith('/country-regions') || p.endsWith('/CountryRegions'))
      data = [{ countryRegionId: 'SAU', isoCode: 'SA', name: 'Saudi Arabia' }];
    else if (p.endsWith('/inventory-dimensions'))
      data = {
        sites: [
          { id: 'S1', code: 'S1', name: 'Site One' },
          { id: 'S2', code: 'S2', name: 'Site Two' },
        ],
        warehouses: [
          { id: 'W1', code: 'W1', name: 'Warehouse One', siteId: 'S1' },
          { id: 'W2', code: 'W2', name: 'Warehouse Two', siteId: 'S2' },
        ],
      };
    else if (p.includes('/LogisticsElectronicAddress/Party/')) {
      if (method === 'POST') {
        const input = request.postDataJSON();
        const location = next++;
        data = { ...input, id: String(location), location };
        contacts.push(data as Record<string, unknown>);
      } else if (method === 'PUT') {
        const input = request.postDataJSON();
        contacts = contacts.map((row) => (row.id === input.id ? input : row));
        data = input;
      } else if (method === 'DELETE') {
        contacts = contacts.filter((row) => row.location !== Number(p.split('/').at(-1)));
        data = true;
      } else data = contacts;
    } else if (p.includes('/LogisticsPostalAddress/Party/')) {
      if (method === 'POST') {
        const input = request.postDataJSON();
        const location = next++;
        data = { ...input, id: String(location), location, address: input.street };
        addresses.push(data as Record<string, unknown>);
      } else if (method === 'PUT') {
        const input = request.postDataJSON();
        data = { ...input, address: input.street };
        addresses = addresses.map((row) =>
          row.id === input.id ? (data as Record<string, unknown>) : row,
        );
      } else if (method === 'DELETE') {
        addresses = addresses.filter((row) => row.location !== Number(p.split('/').at(-1)));
        data = true;
      } else data = addresses;
    } else if (p.includes('/documents/record/'))
      data = { items: [], totalCount: 0, pageNumber: 1, pageSize: 100 };
    else if (p.endsWith('/CustTrans/open-balance'))
      data = { accountNum: url.searchParams.get('accountNum'), reconciled: true, transactions: [] };
    await route.fulfill({ json: { success: true, data } });
  });
  await page.goto('/accounts-receivable/customers/1');
  await expect(page.getByText('C-001 : Audit customer')).toBeVisible({ timeout: 60_000 });
  const edit = () => page.getByRole('button', { name: 'Edit', exact: true }).first();
  await edit().click();
  const name = page.getByRole('textbox', { name: 'Name', exact: true });
  await name.fill('Cancelled name');
  await page.getByRole('button', { name: 'Cancel', exact: true }).first().click();
  await expect(page.getByText('C-001 : Audit customer')).toBeVisible();
  expect(updates).toBe(0);
  await edit().click();
  await name.fill('');
  await page.getByRole('button', { name: 'Save', exact: true }).first().click();
  expect(updates).toBe(0);
  await expect(name).toBeVisible();
  await name.fill('Updated audit customer');
  refuseUpdate = true;
  await page.getByRole('button', { name: 'Save', exact: true }).first().click();
  await expect(page.getByText('Customer update refused', { exact: true })).toBeVisible();
  await expect(name).toHaveValue('Updated audit customer');
  await page.getByRole('combobox', { name: 'Site', exact: true }).click();
  await page.getByRole('option', { name: 'S2 - Site Two', exact: true }).click();
  await page.getByRole('combobox', { name: 'Warehouse', exact: true }).click();
  await expect(page.getByRole('option', { name: 'W1 - Warehouse One', exact: true })).toHaveCount(
    0,
  );
  await page.getByRole('option', { name: 'W2 - Warehouse Two', exact: true }).click();
  await page.getByRole('button', { name: 'Save', exact: true }).first().click();
  await expect(page.getByRole('button', { name: 'Save', exact: true })).toHaveCount(0);
  await expect(page.getByText('C-001 : Updated audit customer')).toBeVisible();
  expect(records[0].inventSiteId).toBe('S2');
  expect(records[0].inventLocationId).toBe('W2');
  await edit().click();
  const contactPanel = page
    .locator('.MuiAccordion-root')
    .filter({ has: page.getByRole('button', { name: 'Contact information', exact: true }) });
  await contactPanel.getByRole('button', { name: 'Add', exact: true }).click();
  const drawer = page.getByRole('dialog');
  await drawer.getByRole('textbox', { name: 'Description', exact: true }).fill('Audit phone');
  await drawer
    .getByRole('textbox', { name: 'Contact number/address', exact: true })
    .fill('0500000000');
  await drawer.getByRole('button', { name: 'OK', exact: true }).click();
  await expect(contactPanel.getByText('Audit phone', { exact: true })).toBeVisible();
  await contactPanel.getByText('Audit phone', { exact: true }).click();
  await contactPanel.getByRole('button', { name: 'Edit', exact: true }).click();
  await drawer
    .getByRole('textbox', { name: 'Contact number/address', exact: true })
    .fill('0511111111');
  await drawer.getByRole('button', { name: 'OK', exact: true }).click();
  await expect(contactPanel.getByText('0511111111', { exact: true })).toBeVisible();
  await contactPanel.getByRole('button', { name: 'Remove', exact: true }).click();
  await expect(contactPanel.getByText('Audit phone', { exact: true })).toHaveCount(0);
  expect(contacts).toHaveLength(0);
  const addressPanel = page
    .locator('.MuiAccordion-root')
    .filter({ has: page.getByRole('button', { name: 'Addresses', exact: true }) });
  await addressPanel.getByRole('button', { name: 'Add', exact: true }).click();
  await drawer
    .getByRole('textbox', { name: 'Name or description', exact: true })
    .fill('Audit address');
  await drawer.getByRole('combobox', { name: 'Country/region', exact: true }).click();
  await page.getByRole('option', { name: 'SAU - Saudi Arabia', exact: true }).click();
  await drawer.getByRole('textbox', { name: 'Street', exact: true }).fill('Audit street');
  await drawer.getByRole('button', { name: 'OK', exact: true }).click();
  await expect(addressPanel.getByText('Audit address', { exact: true })).toBeVisible();
  await addressPanel.getByText('Audit address', { exact: true }).click();
  await addressPanel.getByRole('button', { name: 'Edit', exact: true }).click();
  await drawer.getByRole('textbox', { name: 'Street', exact: true }).fill('Edited audit street');
  await drawer.getByRole('button', { name: 'OK', exact: true }).click();
  await expect(addressPanel.getByText('Edited audit street', { exact: true })).toBeVisible();
  await addressPanel.getByRole('button', { name: 'Remove', exact: true }).click();
  await expect(addressPanel.getByText('Audit address', { exact: true })).toHaveCount(0);
  expect(addresses).toHaveLength(0);
  await page.getByRole('button', { name: 'Cancel', exact: true }).first().click();
  await page.getByRole('button', { name: 'Options', exact: true }).click();
  await page.getByRole('button', { name: 'Record Info', exact: true }).click();
  await expect(page.getByRole('dialog')).toBeVisible();
  await page.keyboard.press('Escape');
  await page.getByRole('button', { name: 'Customer', exact: true }).click();
  await page
    .locator('#customer-action-ribbon')
    .getByRole('button', { name: 'Attachments', exact: true })
    .click();
  await expect(page).toHaveURL(new RegExp('documents/docu-view'));
  await page.goBack();
  await page.getByRole('button', { name: 'Search', exact: true }).click();
  const filter = page.locator('input[placeholder="Filter"]:visible');
  await filter.fill('C-002');
  await page.getByRole('button', { name: /Second customer.*C-002|C-002.*Second customer/ }).click();
  if (testInfo.project.name === 'mobile-chromium') await page.keyboard.press('Escape');
  await expect(page.getByText('C-002 : Second customer')).toBeVisible();
  await page.getByRole('button', { name: 'Customer', exact: true }).click();
  await page
    .locator('#customer-action-ribbon')
    .getByRole('button', { name: 'Open transactions', exact: true })
    .click();
  await expect(page).toHaveURL(new RegExp('customer-open-transactions/C-002$'));
  await page.goto('/accounts-receivable/customers');
  await expect(page.getByRole('link', { name: 'C-001', exact: true })).toBeVisible();
  const before = reads;
  await page.getByRole('button', { name: 'Refresh', exact: true }).click();
  await expect.poll(() => reads).toBeGreaterThan(before);
  await page.getByRole('button', { name: 'Delete', exact: true }).first().click();
  await page.getByRole('dialog').getByRole('button', { name: 'Delete', exact: true }).click();
  await expect(page.getByRole('link', { name: 'C-001', exact: true })).toHaveCount(0);
  expect(records).toHaveLength(1);
});
