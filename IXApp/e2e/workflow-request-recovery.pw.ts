import { expect, test } from '@playwright/test';

test('request types recover after a failed load and refresh when the company changes', async ({
  page,
}) => {
  test.setTimeout(90_000);
  let failing = true;
  const readCompanies: string[] = [];
  await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  await page.route('http://localhost:33319/api/**', (route) =>
    route.fulfill({ json: { success: true, data: [] } })
  );
  await page.route('http://localhost:33319/api/v1/WfCategory', (route) => {
    const company = route.request().headers()['x-company'] ?? 'USMF';
    return route.fulfill({
      json: {
        success: true,
        data: [
          {
            recId: 10,
            code: 'SERVICES',
            name: `${company} services`,
            description: '',
            isActive: true,
            sortOrder: 1,
            dataAreaId: company,
          },
        ],
      },
    });
  });
  await page.route('http://localhost:33319/api/v1/WfProcess', (route) => {
    const company = route.request().headers()['x-company'] ?? 'USMF';
    readCompanies.push(company);
    return failing
      ? route.fulfill({
          status: 503,
          json: { success: false, message: 'Request types are temporarily unavailable.' },
        })
      : route.fulfill({
          json: {
            success: true,
            data: [
              {
                recId: 100,
                code: 'REQ',
                name: `${company} request`,
                categoryId: 10,
                isActive: true,
                sortOrder: 1,
                dataAreaId: company,
              },
            ],
          },
        });
  });
  await page.route('http://localhost:33319/api/v1/CompanyInfo', (route) =>
    route.fulfill({
      json: {
        success: true,
        data: [
          { recId: 1, dataArea: 'USMF', name: 'US audit company' },
          { recId: 2, dataArea: 'DEMF', name: 'DE audit company' },
        ],
      },
    })
  );
  await page.goto('/workflow/request-submission');
  const error = page.getByRole('alert').filter({ hasText: 'temporarily unavailable' });
  await expect(error).toBeVisible({ timeout: 25_000 });
  await expect(page.getByRole('button', { name: 'USMF request', exact: true })).toHaveCount(0);
  failing = false;
  await error.getByRole('button', { name: 'Refresh', exact: true }).click();
  await expect(page.getByRole('button', { name: 'USMF request', exact: true })).toBeVisible();
  await expect(error).toHaveCount(0);
  // Exercise cache isolation without relying on the shell's global invalidation.
  // Both companies intentionally share a category ID.
  await page.evaluate(async () => {
    const path = '/src/core/company/useCompanyStore.ts';
    const { useCompanyStore } = await import(/* @vite-ignore */ path);
    useCompanyStore.getState().setCompany('DEMF');
  });
  await expect(page.getByRole('button', { name: 'DEMF request', exact: true })).toBeVisible();
  await expect(page.getByRole('button', { name: 'USMF request', exact: true })).toHaveCount(0);
  expect(readCompanies).toContain('DEMF');
  // Also exercise the real company action and its menu, including cache invalidation.
  await page.getByRole('button', { name: 'Company', exact: true }).click();
  await page.getByRole('menuitem', { name: 'USMF US audit company', exact: true }).click();
  await expect(page.getByRole('button', { name: 'USMF request', exact: true })).toBeVisible();
  await expect(page.getByRole('button', { name: 'DEMF request', exact: true })).toHaveCount(0);
});
