import { expect, test } from '@playwright/test';

test('opens the authenticated development workspace and customers list', async ({
  page,
}, testInfo) => {
  await page.route('http://localhost:33319/api/**', (route) => {
    const path = new URL(route.request().url()).pathname;
    const data = path.endsWith('/CustTable/list')
      ? [{ recId: 1, accountNumber: 'C-001', name: 'Contoso Retail Americas', currencyCode: 'USD' }]
      : [];
    return route.fulfill({ json: { success: true, data } });
  });
  await page.goto('/dashboard');
  await expect(page.getByText('Business overview')).toBeVisible();

  // The compact layout intentionally opens the application navigation drawer.
  // Dashboard rendering and directionality are the stable mobile smoke checks;
  // the customer grid workflow is covered by the desktop project.
  if (testInfo.project.name === 'mobile-chromium') return;

  await page.goto('/accounts-receivable/customers');
  await expect(
    page.getByRole('heading', { name: 'C-001 | Contoso Retail Americas', exact: true })
  ).toBeVisible({
    timeout: 30_000,
  });
  await expect(page.getByText('Contoso Retail Americas').first()).toBeVisible({ timeout: 30_000 });
});

test('switches the application document to RTL for Arabic', async ({ page }) => {
  await page.goto('/dashboard');
  await page.evaluate(() => window.localStorage.setItem('i18nextLng', 'ar'));
  await page.reload();
  await expect(page.locator('html')).toHaveAttribute('dir', 'rtl');
});
