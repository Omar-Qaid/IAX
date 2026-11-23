import { expect, test } from '@playwright/test';

test('positions settings and notification drawers below the global header', async ({
  page,
}, testInfo) => {
  test.skip(testInfo.project.name !== 'desktop-chromium');
  test.setTimeout(60_000);
  await page.route('http://localhost:33319/api/**', (route) =>
    route.fulfill({ json: { success: true, data: [] } })
  );
  await page.goto('/foundation/exchange-rate-types');
  const headerHeight = await page
    .getByRole('banner')
    .evaluate((header) => header.getBoundingClientRect().height);

  await page.getByRole('banner').getByRole('button', { name: 'Settings' }).click();
  let drawer = page.locator('.MuiDrawer-paper');
  await expect(drawer.getByText('Settings', { exact: true })).toBeVisible();
  await expect(drawer).toHaveCSS('top', `${headerHeight}px`);
  await expect(drawer).toHaveCSS('height', `${page.viewportSize()!.height - headerHeight}px`);
  await drawer.getByRole('button', { name: 'Close' }).click();

  await page.getByRole('banner').getByRole('button', { name: 'Notifications' }).click();
  drawer = page.locator('.MuiDrawer-paper');
  await expect(drawer.getByText('Notifications', { exact: true })).toBeVisible();
  await expect(drawer).toHaveCSS('top', `${headerHeight}px`);
  await drawer.screenshot({ path: 'test-results/notification-drawer.png' });
  await drawer.getByRole('button', { name: 'Settings', exact: true }).click();
  await expect(drawer.getByText('Settings', { exact: true })).toBeVisible();
  await expect(drawer.getByText('Notifications', { exact: true })).toHaveCount(0);
  await drawer.getByRole('button', { name: 'Close' }).click();
  await expect(drawer).toHaveCount(0);
});
