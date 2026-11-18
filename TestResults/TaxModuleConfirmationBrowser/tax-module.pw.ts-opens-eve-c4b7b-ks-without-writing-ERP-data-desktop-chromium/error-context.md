# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: tax-module.pw.ts >> opens every Tax page and preserves saved links without writing ERP data
- Location: e2e\tax-module.pw.ts:11:1

# Error details

```
Test timeout of 180000ms exceeded.
```

```
Error: page.waitForResponse: Test timeout of 180000ms exceeded.
```

# Test source

```ts
  1  | import { expect, test } from '@playwright/test';
  2  | 
  3  | const pages = [
  4  |   { path: '/tax/authorities', legacy: '/foundation/tax-authorities', title: 'Sales tax authorities', endpoint: '/TaxAuthorityAddress' },
  5  |   { path: '/tax/settlement-periods', legacy: '/foundation/tax-periods', title: 'Sales tax settlement periods', endpoint: '/TaxPeriodHead' },
  6  |   { path: '/tax/codes', legacy: '/foundation/tax-codes', title: 'Sales tax codes', endpoint: '/TaxTable' },
  7  |   { path: '/tax/groups', legacy: '/foundation/tax-groups', title: 'Sales tax groups', endpoint: '/TaxGroup' },
  8  |   { path: '/tax/item-groups', legacy: '/foundation/tax-item-groups', title: 'Item sales tax groups', endpoint: '/TaxItemGroup' },
  9  | ];
  10 | 
  11 | test('opens every Tax page and preserves saved links without writing ERP data', async ({ page }) => {
  12 |   test.setTimeout(180_000);
  13 |   const errors: string[] = [];
  14 |   const writes: string[] = [];
  15 |   page.on('pageerror', (error) => errors.push(error.message));
  16 |   await page.addInitScript(() => window.localStorage.setItem('i18nextLng', 'en'));
  17 |   await page.route('**/api/**', async (route) => {
  18 |     if (!['GET', 'OPTIONS'].includes(route.request().method()) && new URL(route.request().url()).pathname.includes('/Tax')) writes.push(route.request().url());
  19 |     await route.fulfill({ json: { success: true, data: [] } });
  20 |   });
  21 | 
  22 |   for (const item of pages) {
  23 |     await test.step(item.title, async () => {
> 24 |       const response = page.waitForResponse((result) => new URL(result.url()).pathname.endsWith(item.endpoint) && result.request().method() === 'GET');
     |                             ^ Error: page.waitForResponse: Test timeout of 180000ms exceeded.
  25 |       await page.goto(item.path);
  26 |       expect((await response).ok()).toBe(true);
  27 |       await expect(page.getByText(item.title, { exact: true }).filter({ visible: true }).first()).toBeVisible();
  28 |       await page.goto(`${item.legacy}?company=dat#details`);
  29 |       await expect(page).toHaveURL(`${test.info().project.use.baseURL ?? 'http://127.0.0.1:4173'}${item.path}?company=dat#details`);
  30 |       await expect(page.getByText(item.title, { exact: true }).filter({ visible: true }).first()).toBeVisible();
  31 |     });
  32 |   }
  33 |   expect(writes).toEqual([]);
  34 |   expect(errors).toEqual([]);
  35 | 
  36 |   for (const item of pages) {
  37 |     const loaded = page.waitForResponse((response) => new URL(response.url()).pathname.endsWith(item.endpoint));
  38 |     await page.goto(`${item.path}?company=dat#confirmation`);
  39 |     await loaded;
  40 |     await expect(page.getByText(item.title, { exact: true }).last()).toBeVisible({ timeout: 30_000 });
  41 |     await expect(page).toHaveURL(`${item.path}?company=dat#confirmation`);
  42 | 
  43 |     await page.goto(`${item.legacy}?company=dat#confirmation`);
  44 |     await expect(page).toHaveURL(`${item.path}?company=dat#confirmation`);
  45 |     await expect(page.getByText(item.title, { exact: true }).last()).toBeVisible();
  46 |   }
  47 |   expect(errors).toEqual([]);
  48 |   expect(writes).toEqual([]);
  49 | });
  50 | 
```