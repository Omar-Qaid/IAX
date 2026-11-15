import { expect, test } from '@playwright/test';
for (const kind of ['terms', 'methods'] as const)
  test('customer payment ' + kind + ' persist basic setup through the API', async ({ page }) => {
    test.setTimeout(100_000);
    page.setDefaultTimeout(15_000);
    await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
    const term = kind === 'terms';
    const endpoint = term ? 'PaymTerm' : 'CustPaymMode';
    const key = term ? 'paymTermId' : 'paymMode';
    const name = term ? 'description' : 'name';
    const days = term ? 'numOfDays' : 'discGraceDays';
    let rows: Record<string, unknown>[] = [];
    let updates = 0;
    await page.route('http://localhost:33319/api/**', async (route) => {
      const request = route.request();
      const p = new URL(request.url()).pathname;
      let data: unknown = [];
      if (p.endsWith('/' + endpoint) && request.method() === 'GET') data = rows;
      if (p.endsWith('/' + endpoint) && request.method() === 'POST') {
        const input = request.postDataJSON();
        expect(input[key]).toBe('TEST');
        expect(input[name]).toBe('Audit setup');
        const created = { ...input, recId: 42, preservedSetting: 999 };
        rows.push(created);
        data = created;
      }
      if (p.endsWith('/' + endpoint + '/42') && request.method() === 'PUT') {
        const input = request.postDataJSON();
        expect(input.preservedSetting).toBe(999);
        expect(input[days]).toBe(15);
        rows = [input];
        updates++;
        data = input;
      }
      if (p.endsWith('/' + endpoint + '/42') && request.method() === 'DELETE') {
        rows = [];
        data = true;
      }
      await route.fulfill({ json: { success: true, data } });
    });
    const path = '/accounts-receivable/customer-payment-' + kind;
    await page.goto(path);
    await page.getByRole('button', { name: 'New', exact: true }).click();
    await page
      .getByRole('textbox', { name: term ? 'Terms of payment' : 'Method of payment', exact: true })
      .fill('TEST');
    await page.getByRole('textbox', { name: 'Description', exact: true }).fill('Audit setup');
    await page
      .getByRole('spinbutton', { name: term ? 'Days' : 'Grace period', exact: true })
      .fill('10');
    await page.getByRole('button', { name: 'Save', exact: true }).click();
    await expect(page.getByRole('button', { name: 'Edit', exact: true })).toBeEnabled();
    expect(rows).toHaveLength(1);
    await page.reload();
    await page.getByRole('button', { name: 'Edit', exact: true }).click();
    await expect(page.getByRole('textbox', { name: 'Description', exact: true })).toHaveValue(
      'Audit setup',
    );
    await page
      .getByRole('spinbutton', { name: term ? 'Days' : 'Grace period', exact: true })
      .fill('15');
    await page.getByRole('button', { name: 'Save', exact: true }).click();
    await expect.poll(() => updates).toBe(1);
    await page.getByRole('button', { name: 'Delete', exact: true }).click();
    await page.getByRole('dialog').getByRole('button', { name: 'Delete', exact: true }).click();
    await expect.poll(() => rows.length).toBe(0);
  });
