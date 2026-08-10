import { expect, test } from '@playwright/test';

test('renders the shared D365 navigation proportions', async ({ page }, testInfo) => {
  test.skip(testInfo.project.name !== 'desktop-chromium');
  await page.goto('/dashboard');

  const navigation = page.getByRole('navigation', { name: 'Application navigation' });
  await expect(navigation).toBeVisible();
  await expect(navigation).toHaveCSS('width', '60px');
  await navigation.getByRole('button', { name: 'Toggle navigation' }).click();
  await expect(navigation).toHaveCSS('width', '249px');

  for (const label of ['Home', 'Favorites', 'Recent', 'Workspaces', 'Modules']) {
    await expect(navigation.getByText(label, { exact: true })).toBeVisible();
  }

  const labelPositions = await Promise.all(
    ['Home', 'Favorites', 'Recent', 'Workspaces', 'Modules'].map(async (label) =>
      navigation.getByText(label, { exact: true }).boundingBox()
    )
  );
  expect(labelPositions.every((position, index) => index === 0 || position!.y > labelPositions[index - 1]!.y)).toBe(true);

  const sectionLabels = ['Favorites', 'Recent', 'Workspaces', 'Modules'];
  for (const label of sectionLabels) {
    const row = navigation.getByText(label, { exact: true }).locator('..').locator('..');
    await expect(row).toHaveCSS('height', '52px');
  }

  await navigation.screenshot({ path: 'test-results/sidebar-navigation.png' });
});
