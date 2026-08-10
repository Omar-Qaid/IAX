import { expect, test } from '@playwright/test';

test('renders the D365 two-column module navigation surface', async ({ page }, testInfo) => {
  test.skip(testInfo.project.name !== 'desktop-chromium');
  await page.goto('/dashboard');

  const appNavigation = page.getByRole('navigation', { name: 'Application navigation' });
  await appNavigation.getByText('Modules', { exact: true }).click();
  await appNavigation.getByText('Accounts Receivable', { exact: true }).click();

  const panel = page.locator('[data-module-nav-panel="true"]');
  await expect(panel).toBeVisible();
  await expect(panel).toHaveCSS('width', '760px');
  await expect(panel.getByRole('button', { name: 'Expand All' })).toBeVisible();
  await expect(panel.getByRole('button', { name: 'Collapse All' })).toBeVisible();
  await panel.getByRole('button', { name: 'Expand All' }).click();
  await panel.screenshot({ path: 'test-results/module-navigation-panel.png' });
});
