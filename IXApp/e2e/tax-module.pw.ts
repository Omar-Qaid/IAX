import { expect, test } from '@playwright/test';

const pages = [
  {
    path: '/tax/authorities',
    legacy: '/foundation/tax-authorities',
    title: 'Sales tax authorities',
    endpoint: '/TaxAuthorityAddress',
  },
  {
    path: '/tax/settlement-periods',
    legacy: '/foundation/tax-periods',
    title: 'Sales tax settlement periods',
    endpoint: '/TaxPeriodHead',
  },
  {
    path: '/tax/codes',
    legacy: '/foundation/tax-codes',
    title: 'Sales tax codes',
    endpoint: '/TaxTable',
  },
  {
    path: '/tax/groups',
    legacy: '/foundation/tax-groups',
    title: 'Sales tax groups',
    endpoint: '/TaxGroup',
  },
  {
    path: '/tax/item-groups',
    legacy: '/foundation/tax-item-groups',
    title: 'Item sales tax groups',
    endpoint: '/TaxItemGroup',
  },
];

test('opens every Tax page and preserves saved links without writing ERP data', async ({
  page,
}) => {
  test.setTimeout(180_000);
  const errors: string[] = [];
  const writes: string[] = [];
  page.on('pageerror', (error) => {
    errors.push(error.message);
    console.error('Browser startup error:', error.message);
  });
  page.on('console', (message) => {
    if (message.type() === 'error') console.error('Browser console:', message.text());
  });
  page.on('requestfailed', (request) =>
    console.error('Failed request:', request.url(), request.failure()?.errorText)
  );
  page.on('response', (response) => {
    if (response.status() >= 400)
      console.error('HTTP asset error:', response.status(), response.url());
  });
  await page.addInitScript(() => window.localStorage.setItem('i18nextLng', 'en'));
  await page.route('http://localhost:33319/api/**', async (route) => {
    if (
      !['GET', 'OPTIONS'].includes(route.request().method()) &&
      new URL(route.request().url()).pathname.includes('/Tax')
    )
      writes.push(route.request().url());
    await route.fulfill({ json: { success: true, data: [] } });
  });

  for (const item of pages) {
    await test.step(item.title, async () => {
      await page.goto(item.path);
      await expect(
        page.getByText(item.title, { exact: true }).filter({ visible: true }).first()
      ).toBeVisible({ timeout: 60_000 });
      for (const legacy of [item.legacy, `${item.legacy}/`, `${item.legacy.toUpperCase()}/`]) {
        await page.goto(`${legacy}?company=dat#details`);
        await expect(page).toHaveURL(
          `${test.info().project.use.baseURL ?? 'http://127.0.0.1:4173'}${item.path}?company=dat#details`
        );
        await expect(
          page.getByText(item.title, { exact: true }).filter({ visible: true }).first()
        ).toBeVisible();
      }
    });
  }
  expect(writes).toEqual([]);
  expect(errors).toEqual([]);
});
