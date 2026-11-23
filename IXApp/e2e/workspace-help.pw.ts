import { expect, test } from '@playwright/test';

for (const language of ['en', 'ar']) {
  test(`workspace help opens and closes from the shell (${language})`, async ({ page }, info) => {
    await page.addInitScript((value) => localStorage.setItem('i18nextLng', value), language);
    await page.route('http://localhost:33319/api/**', (route) =>
      route.fulfill({ json: { success: true, data: [] } })
    );
    await page.goto('/dashboard');
    const title = language === 'ar' ? 'مساعدة مساحة العمل' : 'Workspace help';
    const helpName = language === 'ar' ? /مساعدة/ : /^Help$/;
    const header = page.getByRole('banner');
    const openHelp = async () => {
      if (info.project.name === 'mobile-chromium') {
        await header
          .getByRole('button', { name: language === 'ar' ? /الحساب/ : 'Account', exact: false })
          .click();
        await page.getByRole('menuitem', { name: helpName }).click();
      } else {
        await header.getByRole('button', { name: helpName }).click();
      }
    };
    await openHelp();
    const dialog = page.getByRole('dialog', { name: title });
    await expect(dialog).toBeVisible();
    await expect(dialog.getByRole('heading', { level: 2 })).toHaveCount(4);
    await page.keyboard.press('Escape');
    await expect(dialog).toHaveCount(0);
    await openHelp();
    await dialog.getByRole('button').click();
    await expect(dialog).toHaveCount(0);
  });
}
