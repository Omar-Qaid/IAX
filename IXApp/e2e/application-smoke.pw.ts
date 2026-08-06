import { expect, test } from '@playwright/test';

test('opens the authenticated development workspace and customers list', async ({ page }, testInfo) => {
  test.setTimeout(60_000);
  await page.goto('/dashboard');
  await expect(page.getByText('Business overview')).toBeVisible();

  await page.goto('/accounts-receivable/customers');
  if (testInfo.project.name === 'mobile-chromium') {
    const navigationDialog = page.getByRole('dialog');
    if (await navigationDialog.isVisible()) await navigationDialog.getByRole('button').first().click();
  }
  await expect(page.getByRole('heading', { name: 'Standard view' })).toBeVisible({ timeout: 30_000 });
  await expect(page.getByText('Contoso Retail Americas').first()).toBeVisible({ timeout: 30_000 });
});

test('switches the application document to RTL for Arabic', async ({ page }) => {
  await page.goto('/dashboard');
  await page.evaluate(() => window.localStorage.setItem('i18nextLng', 'ar'));
  await page.reload();
  await expect(page.locator('html')).toHaveAttribute('dir', 'rtl');
});
