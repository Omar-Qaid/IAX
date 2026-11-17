import { expect, test } from '@playwright/test';

test('contact form uses the shared master detail and saved-party logistics panels', async ({
  page,
}, info) => {
  test.setTimeout(100_000);
  page.setDefaultTimeout(20_000);
  const invalidPartyRequests: string[] = [];
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  await page.route('http://localhost:33319/api/**', async (route) => {
    const path = new URL(route.request().url()).pathname;
    if (path.includes('/Party/0')) invalidPartyRequests.push(path);
    if (path.endsWith('/ContactPerson/accounts/customer/CUST-300')) {
      await route.fulfill({
        json: { accountNum: 'CUST-300', name: 'Example customer', contacts: [] },
      });
      return;
    }
    await route.fulfill({ json: { success: true, data: [] } });
  });
  await page.goto('/contacts/customer/CUST-300/new');
  await expect(page.getByRole('textbox', { name: 'First name', exact: true })).toBeVisible({
    timeout: 30_000,
  });
  await expect(
    page.getByRole('button', { name: 'Toggle record list', exact: true })
  ).toHaveAttribute('aria-pressed', 'false');
  await expect(page.getByRole('button', { name: 'Contact', exact: true })).toBeDisabled();
  await expect(page.getByRole('button', { name: 'Save', exact: true })).toBeEnabled();
  await page.getByRole('textbox', { name: 'First name', exact: true }).fill('Sara');
  await expect(page.getByRole('button', { name: 'Save', exact: true })).toBeEnabled();
  await expect(page.getByText('Save the contact to add addresses.', { exact: true })).toBeVisible();
  await expect(
    page.getByText('Save the contact to add contact information.', { exact: true })
  ).toBeVisible();
  await page.getByRole('button', { name: 'Show more fields', exact: true }).click();
  await expect(page.getByRole('textbox', { name: 'Initials', exact: true })).toBeVisible();
  await page.getByRole('button', { name: 'Show fewer fields', exact: true }).click();
  expect(invalidPartyRequests).toEqual([]);
  await page.screenshot({ path: info.outputPath('contact-form.png'), fullPage: true });
});
