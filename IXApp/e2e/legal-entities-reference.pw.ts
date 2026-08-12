import { expect, test } from '@playwright/test';

test('matches the compact Legal entities reference shell and preserves settings', async ({ page }, testInfo) => {
  test.skip(testInfo.project.name !== 'desktop-chromium');
  await page.setViewportSize({ width: 1920, height: 881 });
  await page.goto('/organization-administration/legal-entities');

  await expect(page.getByRole('banner')).toHaveCSS('height', '58px');
  await expect(page.getByRole('button', { name: 'Settings' })).toBeVisible();
  await expect(page.getByRole('navigation', { name: 'Breadcrumbs' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Legal entities' })).toBeVisible();

  const general = page.getByRole('button', { name: 'General' });
  await expect(general).toHaveAttribute('aria-expanded', 'false');
  await expect(general).toHaveCSS('min-height', '45px');

  await page.screenshot({ path: 'test-results/legal-entities-reference.png', fullPage: false });
});
