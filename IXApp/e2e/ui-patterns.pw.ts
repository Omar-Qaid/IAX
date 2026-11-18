import { expect, test } from '@playwright/test';

for (const language of ['en', 'ar'] as const) {
  for (const mode of ['light', 'dark'] as const) {
    test(`shared customer sections in ${language} ${mode}`, async ({ page }, testInfo) => {
      test.setTimeout(90_000);
      await page.addInitScript(
        ({ language, mode }) => {
          localStorage.setItem('i18nextLng', language);
          localStorage.setItem('ixapp_theme_mode', mode);
          localStorage.removeItem('ixapp_user_preferences');
        },
        { language, mode }
      );
      await page.route('http://localhost:33319/api/**', async (route) => {
        const path = new URL(route.request().url()).pathname;
        const data = path.endsWith('/CustTable/list')
          ? [
              {
                recId: 1,
                party: 7,
                accountNumber: 'C-001',
                name: 'UI review customer',
                nameAr: 'عميل مراجعة الواجهة',
                customerGroupId: 'Retail',
                currencyCode: 'SAR',
                status: 'active',
              },
            ]
          : path.endsWith('/inventory-dimensions')
            ? { sites: [], warehouses: [] }
            : [];
        await route.fulfill({ json: { success: true, data } });
      });
      await page.goto('/accounts-receivable/customers/1');
      const section = page.locator('.MuiAccordion-root[data-page-section]').first();
      await expect(section).toBeVisible({ timeout: 60_000 });
      const header = section.locator('.MuiAccordionSummary-root').first();
      await expect(page.locator('html')).toHaveAttribute('dir', language === 'ar' ? 'rtl' : 'ltr');
      // The section surfaces come from the theme, so these assert the relationship the
      // theme defines rather than literal colors: a literal would re-encode the default
      // palette here and go red for any other color preset even when the theme is right.
      const surfaces = await page.evaluate(
        ([headerEl, sectionEl]) => {
          const read = (el: Element, property: string) =>
            window.getComputedStyle(el).getPropertyValue(property);
          return {
            headerBackground: read(headerEl!, 'background-color'),
            sectionBackground: read(sectionEl!, 'background-color'),
            bodyColor: read(document.body, 'color'),
          };
        },
        [await header.elementHandle(), await section.elementHandle()]
      );

      // The page-section override paints the summary from the same paper surface as the
      // section it belongs to.
      expect(surfaces.headerBackground).toBe(surfaces.sectionBackground);

      // The surface still has to follow the active mode.
      const channels = surfaces.headerBackground.match(/\d+(\.\d+)?/g) ?? [];
      const luminance =
        (0.2126 * Number(channels[0]) +
          0.7152 * Number(channels[1]) +
          0.0722 * Number(channels[2])) /
        255;
      if (mode === 'light') expect(luminance).toBeGreaterThan(0.5);
      else expect(luminance).toBeLessThan(0.5);

      await expect(header).toHaveCSS('min-height', '38px');
      await expect(
        section.getByText(language === 'ar' ? 'الحساب' : 'Account', { exact: true }).first()
      ).toHaveCSS('color', surfaces.bodyColor);
      await expect(header).toHaveAttribute('aria-expanded', 'true');
      await header.click();
      await expect(header).toHaveAttribute('aria-expanded', 'false');
      await expect(header).toHaveCSS('min-height', '38px');
      await header.click();
      await expect(header).toHaveAttribute('aria-expanded', 'true');
      const overflow = await page.evaluate(
        () => document.documentElement.scrollWidth - document.documentElement.clientWidth
      );
      expect(overflow).toBeLessThanOrEqual(1);
      await page.screenshot({ path: testInfo.outputPath('sections.png'), fullPage: true });
    });
  }
}

test('HCM reference pages keep distinct adapters and shared commands', async ({ page }) => {
  test.setTimeout(120_000);
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  const references = [
    { path: 'departments', endpoint: 'HcmDepartment', name: 'Review department' },
    { path: 'nationalities', endpoint: 'HcmNationality', name: 'Review nationality' },
    { path: 'occupations', endpoint: 'HcmOccupation', name: 'Review occupation' },
  ];
  const reads = new Set<string>();
  await page.route('http://localhost:33319/api/**', async (route) => {
    const endpoint = new URL(route.request().url()).pathname.split('/').at(-1);
    const reference = references.find((item) => item.endpoint === endpoint);
    if (reference) reads.add(reference.endpoint);
    await route.fulfill({
      json: {
        success: true,
        data: reference
          ? [
              {
                recId: 1,
                code: 'REF-1',
                name: reference.name,
                nameAlias: '',
                description: '',
                isActive: true,
                dataAreaId: 'dat',
                recVersion: 1,
                rowVersion: null,
              },
            ]
          : [],
      },
    });
  });
  for (const reference of references) {
    await page.goto(`/organization-administration/${reference.path}`);
    await expect(page.getByText(reference.name, { exact: true }).first()).toBeVisible({
      timeout: 60_000,
    });
    await expect(page.getByRole('button', { name: 'New', exact: true }).first()).toBeVisible();
    await expect(page.getByRole('button', { name: 'Edit', exact: true }).first()).toBeVisible();
    expect(reads.has(reference.endpoint)).toBe(true);
  }
});
