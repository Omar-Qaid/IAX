import { expect, test } from '@playwright/test';

test('positions settings and notification drawers below the global header', async ({ page }, testInfo) => {
  test.skip(testInfo.project.name !== 'desktop-chromium');
  await page.goto('/foundation/exchange-rate-types');

  await page.getByRole('banner').getByRole('button', { name: 'Settings' }).click();
  let drawer = page.locator('.MuiDrawer-paper');
  await expect(drawer.getByText('Settings', { exact: true })).toBeVisible();
  await expect(drawer).toHaveCSS('top', '60px');
  await expect(drawer).toHaveCSS('height', '660px');
  await drawer.getByRole('button', { name: 'Close' }).click();

  await page.getByRole('banner').getByRole('button', { name: 'Notifications' }).click();
  drawer = page.locator('.MuiDrawer-paper');
  await expect(drawer.getByText('Notifications', { exact: true })).toBeVisible();
  await expect(drawer).toHaveCSS('top', '60px');
  await drawer.screenshot({ path: 'test-results/notification-drawer.png' });
});
