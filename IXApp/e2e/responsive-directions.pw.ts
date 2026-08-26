import { expect, test } from '@playwright/test';

const viewports = [
  { name: 'wide-desktop', width: 1920, height: 1080 },
  { name: 'desktop-1536', width: 1536, height: 864 },
  { name: 'desktop-1440', width: 1440, height: 900 },
  { name: 'laptop-1280', width: 1280, height: 800 },
  { name: 'laptop', width: 1024, height: 768 },
  { name: 'tablet', width: 768, height: 1024 },
  { name: 'large-mobile', width: 430, height: 932 },
  { name: 'mobile', width: 390, height: 844 },
  { name: 'mobile-360', width: 360, height: 800 },
  { name: 'small-mobile', width: 320, height: 568 },
] as const;

for (const viewport of viewports) {
  for (const language of ['en', 'ar'] as const) {
    test(`${viewport.name} has no page-level overflow in ${language}`, async ({ page }, testInfo) => {
      test.skip(testInfo.project.name !== 'desktop-chromium');
      await page.setViewportSize(viewport);
      await page.goto('/dashboard');
      await page.evaluate((nextLanguage) => {
        window.localStorage.setItem('i18nextLng', nextLanguage);
        window.localStorage.removeItem('ixapp_user_preferences');
      }, language);
      await page.reload();

      await expect(page.locator('html')).toHaveAttribute('dir', language === 'ar' ? 'rtl' : 'ltr');
      await expect(page.locator('main')).toBeVisible();

      const overflow = await page.evaluate(() => ({
        document: document.documentElement.scrollWidth - document.documentElement.clientWidth,
        body: document.body.scrollWidth - document.body.clientWidth,
      }));
      expect(overflow.document).toBeLessThanOrEqual(1);
      expect(overflow.body).toBeLessThanOrEqual(1);
    });
  }
}

for (const language of ['en', 'ar'] as const) {
  test(`settings drawer opens from logical end in ${language}`, async ({ page }, testInfo) => {
    test.skip(testInfo.project.name !== 'desktop-chromium');
    await page.setViewportSize({ width: 1024, height: 768 });
    await page.goto('/dashboard');
    await page.evaluate((nextLanguage) => {
      window.localStorage.setItem('i18nextLng', nextLanguage);
      window.localStorage.removeItem('ixapp_user_preferences');
    }, language);
    await page.reload();

    await page.getByRole('button', { name: language === 'ar' ? 'الإعدادات' : 'Settings' }).click();
    const drawer = page.locator('.MuiDrawer-paper[data-drawer-anchor="right"]');
    await expect(drawer).toBeVisible();

    if (language === 'ar') {
      await expect.poll(async () => (await drawer.boundingBox())?.x ?? Number.POSITIVE_INFINITY)
        .toBeLessThanOrEqual(10);
    } else {
      await expect.poll(async () => {
        const box = await drawer.boundingBox();
        return box ? Math.abs(box.x + box.width - 1024) : Number.POSITIVE_INFINITY;
      }).toBeLessThanOrEqual(10);
    }
  });
}
