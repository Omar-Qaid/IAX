import { expect, test } from '@playwright/test';

test('renders the shared D365 Finance and Operations top bar', async ({ page }, testInfo) => {
  test.skip(testInfo.project.name !== 'desktop-chromium');
  await page.goto('/organization-administration/legal-entities');

  const header = page.getByRole('banner');
  await expect(header).toBeVisible();
  await expect(header).toHaveCSS('height', '55px');
  await expect(header).toHaveCSS('background-color', 'rgb(11, 11, 11)');
  await expect(header.getByText('Finance and Operations', { exact: true })).toBeVisible();
  await expect(header.getByRole('button', { name: 'App launcher' })).toBeVisible();
  await expect(header.getByRole('button', { name: 'Search' })).toBeVisible();
  await expect(header.getByRole('button', { name: 'Notifications' })).toBeVisible();
  await expect(header.getByRole('button', { name: 'Settings' })).toBeVisible();
  await header.screenshot({ path: 'test-results/top-bar-navigation.png' });
});
