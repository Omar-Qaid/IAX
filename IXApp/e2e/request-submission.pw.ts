import { expect, test } from '@playwright/test';

const ok = (data: unknown) => ({ success: true, data, message: null, errors: [] });

test('shows backend-driven request categories and request types', async ({ page }) => {
  page.on('pageerror', (error) => console.error(`PAGE ERROR: ${error.message}`));
  page.on('console', (message) => {
    if (message.type() === 'error') console.error(`BROWSER ERROR: ${message.text()}`);
  });
  await page.route('**/api/v1/WfRequest/number-sequence', (route) =>
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(ok({ manual: false, previewCode: 'REQ-00001' })),
    })
  );
  await page.route('**/api/v1/WfRequest', (route) =>
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(
        ok([
          {
            recId: 500,
            code: 'REQ-00500',
            name: 'Electronic transaction request',
            description: null,
            requestDate: '2026-08-20T00:00:00Z',
            processId: 100,
            employeeId: null,
            requestDetails: null,
            isFinished: false,
            finishedDate: null,
            isStopped: false,
            stoppedDate: null,
            score: 0,
            progress: 0,
            notes: null,
            attachmentId: null,
            isActive: true,
            rowVersion: null,
            recVersion: 1,
            dataAreaId: 'dat',
          },
          {
            recId: 101,
            code: 'E-ONBOARDING',
            name: 'Employee onboarding',
            description: 'Submit an onboarding request',
            categoryId: 10,
            score: 0,
            canRepeat: true,
            mandatoryDocs: false,
            priorityId: 1,
            processTypeId: 1,
            sysField: false,
            sortOrder: 20,
            usersProcesses: [],
            isActive: true,
            rowVersion: null,
            recVersion: 1,
            dataAreaId: 'dat',
          },
          {
            recId: 200,
            code: 'FIN-PAYMENT',
            name: 'Payment request',
            description: 'Submit a payment request',
            categoryId: 20,
            score: 0,
            canRepeat: true,
            mandatoryDocs: false,
            priorityId: 1,
            processTypeId: 1,
            sysField: false,
            sortOrder: 10,
            usersProcesses: [],
            isActive: true,
            rowVersion: null,
            recVersion: 1,
            dataAreaId: 'dat',
          },
        ])
      ),
    })
  );
  await page.route('**/api/v1/WfCategory', (route) =>
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(
        ok([
          {
            recId: 10,
            code: 'ELECTRONIC',
            name: 'Electronic services',
            description: null,
            sysField: false,
            sortOrder: 10,
            isActive: true,
            rowVersion: null,
            recVersion: 1,
            dataAreaId: 'dat',
          },
          {
            recId: 20,
            code: 'FINANCE',
            name: 'Financial affairs',
            description: 'Financial request services',
            sysField: false,
            sortOrder: 20,
            isActive: true,
            rowVersion: null,
            recVersion: 1,
            dataAreaId: 'dat',
          },
        ])
      ),
    })
  );
  await page.route('**/api/v1/WfProcess', (route) =>
    route.fulfill({
      status: 200,
      contentType: 'application/json',
      body: JSON.stringify(
        ok([
          {
            recId: 100,
            code: 'E-TRANSACTION',
            name: 'Electronic transactions',
            description: 'Submit an electronic transaction request',
            categoryId: 10,
            score: 0,
            canRepeat: true,
            mandatoryDocs: false,
            priorityId: 1,
            processTypeId: 1,
            sysField: false,
            sortOrder: 10,
            usersProcesses: [],
            isActive: true,
            rowVersion: null,
            recVersion: 1,
            dataAreaId: 'dat',
          },
          {
            recId: 101,
            code: 'E-ONBOARDING',
            name: 'Employee onboarding',
            description: 'Submit an onboarding request',
            categoryId: 10,
            score: 0,
            canRepeat: true,
            mandatoryDocs: false,
            priorityId: 1,
            processTypeId: 1,
            sysField: false,
            sortOrder: 20,
            usersProcesses: [],
            isActive: true,
            rowVersion: null,
            recVersion: 1,
            dataAreaId: 'dat',
          },
          {
            recId: 200,
            code: 'FIN-PAYMENT',
            name: 'Payment request',
            description: 'Submit a payment request',
            categoryId: 20,
            score: 0,
            canRepeat: true,
            mandatoryDocs: false,
            priorityId: 1,
            processTypeId: 1,
            sysField: false,
            sortOrder: 10,
            usersProcesses: [],
            isActive: true,
            rowVersion: null,
            recVersion: 1,
            dataAreaId: 'dat',
          },
        ])
      ),
    })
  );

  await page.goto('/workflow/request-submission');

  await expect(page.getByRole('heading', { name: 'Request Submission', level: 1 })).toBeVisible();
  await expect(page.getByText('Electronic services', { exact: true }).first()).toBeVisible();
  await expect(page.getByText('Financial affairs', { exact: true })).toBeVisible();
  await expect(page.getByRole('button', { name: /Electronic transactions/ })).toBeVisible();
  await expect(page.getByRole('combobox', { name: 'Request process' })).toHaveCount(0);
  await expect(page.getByRole('textbox', { name: 'Request name' })).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Submit request' })).toHaveCount(0);

  await page.getByRole('button', { name: /Electronic transactions/ }).click();
  await expect(page).toHaveURL(/\/workflow\/request-from\/10\/100$/);
  await expect(page.getByRole('heading', { name: 'RequestFrom', level: 1 })).toBeVisible();
  await expect(page.getByText('Electronic transactions', { exact: true }).first()).toBeVisible();
  await expect(page.getByText('Employee onboarding', { exact: true })).toHaveCount(0);
  await expect(page.getByText('Payment request', { exact: true })).toHaveCount(0);
  const processFilter = page.getByRole('textbox', { name: 'Filter' });
  await expect(processFilter).toHaveValue('Electronic transactions');
  await processFilter.clear();
  await expect(page.getByText('Employee onboarding', { exact: true }).first()).toBeVisible();
  await expect(page.getByText('Payment request', { exact: true })).toHaveCount(0);
  await expect(page.getByRole('button', { name: 'Submit request' })).toBeVisible();
});
