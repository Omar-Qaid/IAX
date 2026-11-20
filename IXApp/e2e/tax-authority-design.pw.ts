import { expect, test } from '@playwright/test';

test('tax authority matches the compact reference layout and saves address edits', async ({
  page,
}, testInfo) => {
  test.skip(testInfo.project.name !== 'desktop-chromium');
  test.setTimeout(120000);
  await page.setViewportSize({ width: 1870, height: 850 });
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  let authority = {
    recId: 1,
    taxAuthority: 'GAZT',
    name: 'General Authority of Zakat and Tax',
    taxAuthorityId: 'GAZT',
    accountNum: 'V-00192',
    phone: '',
    mobile: '',
    fax: '',
    sms: '',
    telex: '',
    extension: '',
    pager: '',
    email: '',
    url: '',
    address: '',
    roundOff: 0,
    roundOffType: 0,
    taxReportLayout: 0,
    useDefaultLayout: 0,
    separateTaxSummary: 0,
    printBlankPage: 0,
  };
  let saves = 0;
  await page.route('http://localhost:33319/api/**', async (route) => {
    const path = new URL(route.request().url()).pathname;
    let data: unknown = [];
    if (path.endsWith('/TaxAuthorityAddress')) data = [authority];
    if (path.endsWith('/Vendor')) data = [{ accountNum: 'V-00192', name: 'Tax authority vendor' }];
    if (path.endsWith('/TaxAuthorityAddress/1') && route.request().method() === 'PUT') {
      authority = { ...authority, ...route.request().postDataJSON() };
      data = authority;
      saves++;
    }
    await route.fulfill({ json: { success: true, data } });
  });
  await page.goto('/tax/authorities', { waitUntil: 'domcontentloaded' });
  await expect(page.getByText('Sales tax authorities', { exact: true }).first()).toBeVisible({ timeout: 90000 });
  await expect(page.getByText('GAZT', { exact: true }).first()).toBeVisible();
  await expect(page.getByRole('textbox', { name: 'Address', exact: true })).toHaveAttribute('readonly', '');
  await page.getByRole('button', { name: 'New', exact: true }).last().click();
  await expect(page.getByRole('button', { name: 'Save', exact: true })).toBeEnabled();
  await expect(page.getByRole('textbox', { name: 'Address', exact: true })).not.toHaveAttribute('readonly', '');
  const telephone = await page.getByLabel('Telephone', { exact: true }).boundingBox();
  const mobile = await page.getByLabel('Mobile phone', { exact: true }).boundingBox();
  const fax = await page.getByLabel('Fax', { exact: true }).boundingBox();
  expect(telephone!.x).toBeLessThan(mobile!.x);
  expect(mobile!.x).toBeLessThan(fax!.x);
  expect(Math.abs(telephone!.y - mobile!.y)).toBeLessThan(3);
  await page.screenshot({
    path: testInfo.outputPath('tax-authorities-desktop.png'),
    fullPage: true,
  });
  await page.getByRole('textbox', { name: 'Address', exact: true }).fill('Riyadh\nSaudi Arabia');
  await page.getByRole('button', { name: 'Save', exact: true }).click();
  await expect.poll(() => saves).toBe(1);
  await expect(page.getByRole('textbox', { name: 'Address', exact: true })).toHaveValue('Riyadh\nSaudi Arabia');
});
