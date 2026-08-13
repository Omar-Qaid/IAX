import { expect, test } from '@playwright/test';

test.beforeEach(async ({ page }) => {
  await page.goto('/process-builder/new');
  await expect(page.getByRole('heading', { name: 'Process Builder' })).toBeVisible();
});

test('keeps the reference three-panel authoring layout on desktop', async ({ page }, testInfo) => {
  test.skip(testInfo.project.name !== 'desktop-chromium');

  await expect(page.getByRole('tablist', { name: 'Process Builder navigation' })).toBeVisible();
  await expect(page.getByRole('tablist', { name: 'Process Builder workspaces' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Process Information' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Open process structure' })).toBeHidden();
  await expect(page.getByRole('button', { name: 'Open settings' })).toBeHidden();

  await page.screenshot({ path: 'test-results/process-builder-desktop.png', fullPage: false });
});

test('uses temporary structure and settings drawers on mobile', async ({ page }, testInfo) => {
  test.skip(testInfo.project.name !== 'mobile-chromium');

  const structure = page.getByRole('button', { name: 'Open process structure' });
  const settings = page.getByRole('button', { name: 'Open settings' });
  await expect(structure).toBeVisible();
  await expect(settings).toBeVisible();
  await expect(page.getByText('Workflow Designer', { exact: true })).toBeVisible();

  await structure.click();
  await expect(page.getByRole('heading', { name: 'Process structure' })).toBeVisible();
  await expect(page.getByRole('tablist', { name: 'Process Builder navigation' })).toBeVisible();
  await page.getByRole('button', { name: 'Close process structure' }).click();

  await settings.click();
  await expect(page.getByRole('heading', { name: 'Settings' })).toBeVisible();
  await expect(page.getByRole('heading', { name: 'Process Information' })).toBeVisible();
  await page.screenshot({ path: 'test-results/process-builder-mobile-settings.png', fullPage: false });
});

