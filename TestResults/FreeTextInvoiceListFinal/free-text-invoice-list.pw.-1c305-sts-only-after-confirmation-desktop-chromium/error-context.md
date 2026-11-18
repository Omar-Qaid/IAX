# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: free-text-invoice-list.pw.ts >> free text invoice list reuses the sales-list pattern, filters records, deletes drafts and posts only after confirmation
- Location: e2e\free-text-invoice-list.pw.ts:3:1

# Error details

```
Error: expect(locator).toBeEnabled() failed

Locator:  getByRole('button', { name: 'Totals', exact: true })
Expected: enabled
Received: disabled
Timeout:  5000ms

Call log:
  - Expect "toBeEnabled" with timeout 5000ms
  - waiting for getByRole('button', { name: 'Totals', exact: true })
    13 × locator resolved to <button disabled tabindex="-1" type="button" aria-label="Totals" class="MuiButtonBase-root Mui-disabled MuiButton-root MuiButton-text MuiButton-sizeSmall MuiButton-colorPrimary mui-ltr-9oa6ns-MuiButtonBase-root-MuiButton-root">Totals</button>
       - unexpected value "disabled"

```

```yaml
- button "Totals" [disabled]
```

# Test source

```ts
  12  |     recId: number,
  13  |     invoiceId: string,
  14  |     posted = false,
  15  |     customerAccount = 'C-001',
  16  |     invoiceDate = date
  17  |   ) => ({
  18  |     recId,
  19  |     invoiceId,
  20  |     posted,
  21  |     customerAccount,
  22  |     invoiceAccount: customerAccount,
  23  |     description: 'Consulting',
  24  |     invoiceDate,
  25  |     dueDate: date,
  26  |     currencyCode: 'SAR',
  27  |     paymentTerms: 'NET30',
  28  |     paymentMethod: 'BANK',
  29  |     postingProfile: '',
  30  |     defaultDimension: 0,
  31  |     taxGroup: 'VAT',
  32  |     netAmount: 2000,
  33  |     taxAmount: 300,
  34  |     invoiceAmount: 2300,
  35  |     lines: [],
  36  |   });
  37  |   let invoices = [
  38  |     invoice(1, 'DRAFT-1'),
  39  |     invoice(2, 'POSTED-2', true),
  40  |     invoice(3, 'OLD-3', false, 'C-001', '2020-01-01'),
  41  |     invoice(4, 'OTHER-CUSTOMER', false, 'C-002'),
  42  |     invoice(5, 'DRAFT-5'),
  43  |   ];
  44  |   let posts = 0,
  45  |     deletes = 0;
  46  |   await page.route('http://localhost:33319/api/**', async (route) => {
  47  |     const p = new URL(route.request().url()).pathname;
  48  |     const method = route.request().method();
  49  |     let data: unknown = [];
  50  |     if (p.endsWith('/CustTable/list'))
  51  |       data = [
  52  |         {
  53  |           recId: 10,
  54  |           party: 10,
  55  |           accountNumber: 'C-001',
  56  |           name: 'Customer one',
  57  |           customerGroupId: 'Retail',
  58  |           currencyCode: 'SAR',
  59  |           status: 'active',
  60  |         },
  61  |       ];
  62  |     if (p.endsWith('/inventory-dimensions')) data = { sites: [], warehouses: [] };
  63  |     if (p.endsWith('/CustInvoiceTable') && method === 'GET') data = invoices;
  64  |     if (p.endsWith('/CustInvoiceTable/5') && method === 'DELETE') {
  65  |       expect(invoices.find((row) => row.recId === 5)?.posted).toBe(false);
  66  |       invoices = invoices.filter((row) => row.recId !== 5);
  67  |       deletes++;
  68  |       data = {};
  69  |     }
  70  |     if (p.endsWith('/CustInvoiceTable/1/post') && method === 'POST') {
  71  |       posts++;
  72  |       invoices = invoices.map((row) => (row.recId === 1 ? { ...row, posted: true } : row));
  73  |       data = invoices.find((row) => row.recId === 1);
  74  |     }
  75  |     await route.fulfill({ json: { success: true, data } });
  76  |   });
  77  |   await page.goto('/accounts-receivable/customers/10');
  78  |   await expect(page.getByText('C-001 : Customer one')).toBeVisible({ timeout: 60_000 });
  79  |   await page.getByRole('button', { name: 'Invoice', exact: true }).click();
  80  |   await page.getByRole('button', { name: 'Free text invoices', exact: true }).click();
  81  |   await expect(page).toHaveURL(/free-text-invoices\?customerAccount=C-001$/);
  82  |   await expect(page.getByText('DRAFT-1', { exact: true })).toBeVisible();
  83  |   await expect(page.getByText('POSTED-2', { exact: true })).toHaveCount(0);
  84  |   await expect(page.getByText('OLD-3', { exact: true })).toHaveCount(0);
  85  |   await expect(page.getByText('OTHER-CUSTOMER', { exact: true })).toHaveCount(0);
  86  |   await expect(page.getByRole('button', { name: 'New', exact: true })).toBeDisabled();
  87  |   await expect(page.getByRole('button', { name: 'Edit', exact: true })).toBeDisabled();
  88  |   await page.getByRole('combobox', { name: 'Invoice date', exact: true }).click();
  89  |   await page.getByRole('option', { name: 'All dates', exact: true }).click();
  90  |   await expect(page.getByText('OLD-3', { exact: true })).toBeVisible();
  91  |   await page.getByRole('combobox', { name: 'Invoice date', exact: true }).click();
  92  |   await page.getByRole('option', { name: 'Last 30 days', exact: true }).click();
  93  |   await page.getByRole('textbox', { name: 'Invoice number', exact: true }).fill('DRAFT-5');
  94  |   await page.getByText('DRAFT-5', { exact: true }).click();
  95  |   await page.getByRole('button', { name: 'Delete', exact: true }).click();
  96  |   await page.getByRole('dialog').getByRole('button', { name: 'Delete', exact: true }).click();
  97  |   await expect.poll(() => deletes).toBe(1);
  98  |   await page.getByRole('textbox', { name: 'Invoice number', exact: true }).fill('');
  99  |   await page.getByText('DRAFT-1', { exact: true }).click();
  100 |   await page.getByRole('button', { name: 'Totals', exact: true }).click();
  101 |   await expect(
  102 |     page.getByRole('dialog').getByText('Sales tax: 300.00', { exact: true })
  103 |   ).toBeVisible();
  104 |   await page.getByRole('dialog').getByRole('button', { name: 'Close', exact: true }).last().click();
  105 |   await page.getByRole('button', { name: 'Post', exact: true }).click();
  106 |   expect(posts).toBe(0);
  107 |   await page.getByRole('dialog').getByRole('button', { name: 'Post', exact: true }).click();
  108 |   await expect.poll(() => posts).toBe(1);
  109 |   await page.getByRole('combobox', { name: 'Status', exact: true }).click();
  110 |   await page.getByRole('option', { name: 'Posted', exact: true }).click();
  111 |   await page.getByText('DRAFT-1', { exact: true }).click();
> 112 |   await expect(page.getByRole('button', { name: 'Totals', exact: true })).toBeEnabled();
      |                                                                           ^ Error: expect(locator).toBeEnabled() failed
  113 |   await expect(page.getByRole('button', { name: 'Post', exact: true })).toBeDisabled();
  114 |   await expect(page.getByRole('button', { name: 'Delete', exact: true })).toBeDisabled();
  115 |   await page.screenshot({ path: info.outputPath('free-text-invoices.png'), fullPage: true });
  116 | });
  117 | 
```