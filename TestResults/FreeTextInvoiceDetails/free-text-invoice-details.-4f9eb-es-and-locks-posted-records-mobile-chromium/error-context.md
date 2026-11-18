# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: free-text-invoice-details.pw.ts >> free text invoice details edits and saves the active line, cancels changes and locks posted records
- Location: e2e\free-text-invoice-details.pw.ts:3:1

# Error details

```
Test timeout of 180000ms exceeded.
```

```
Error: locator.fill: Test timeout of 180000ms exceeded.
Call log:
  - waiting for locator('input[value="Consulting"]')

```

# Page snapshot

```yaml
- generic [ref=e3]:
  - link "Skip to main content" [ref=e4] [cursor=pointer]:
    - /url: "#main-content"
  - banner [ref=e5]:
    - generic [ref=e6]:
      - button [ref=e8] [cursor=pointer]
      - generic [ref=e11]:
        - button "Notifications" [ref=e12] [cursor=pointer]
        - button "Settings" [ref=e16] [cursor=pointer]
        - button "Account" [ref=e19] [cursor=pointer]:
          - generic [ref=e20]: EA
  - main [ref=e23]:
    - generic [ref=e25]:
      - generic [ref=e28]:
        - generic [ref=e29]:
          - button "Back" [ref=e32] [cursor=pointer]
          - button "Toggle record list" [ref=e36] [cursor=pointer]
          - generic [ref=e40]:
            - button "Edit" [ref=e41] [cursor=pointer]
            - button "New" [ref=e45] [cursor=pointer]
            - button "Delete" [ref=e49] [cursor=pointer]
          - generic [ref=e54]:
            - button "Post" [ref=e55] [cursor=pointer]
            - button "Totals" [ref=e56] [cursor=pointer]
          - generic [ref=e58]:
            - button "Search" [ref=e59] [cursor=pointer]
            - button "Options" [ref=e63] [cursor=pointer]
        - generic [ref=e64]:
          - generic "Personalize" [ref=e65]:
            - button "Personalize" [disabled]
          - generic "Page guide" [ref=e66]:
            - button "Page guide" [disabled]
          - generic "Notifications" [ref=e67]:
            - button "Notifications" [disabled]
          - generic "Attachments" [ref=e68]:
            - button "Attachments" [ref=e69] [cursor=pointer]:
              - generic [ref=e70]: "0"
          - generic "Refresh" [ref=e74]:
            - button "Refresh" [ref=e75] [cursor=pointer]
          - generic "Open in new window" [ref=e78]:
            - button "Open in new window" [ref=e79] [cursor=pointer]
      - generic [ref=e84]:
        - heading "C-001 - Customer one" [level=1] [ref=e86]
        - generic [ref=e87]:
          - generic [ref=e88]:
            - heading [level=3] [ref=e89]:
              - button [expanded] [ref=e90] [cursor=pointer]:
                - paragraph [ref=e92]: Free text invoice header
            - region [ref=e99]:
              - generic [ref=e101]:
                - generic [ref=e102]:
                  - paragraph [ref=e103]: FIELDS.CUSTOMER
                  - generic [ref=e104]:
                    - generic [ref=e105]:
                      - paragraph [ref=e106]: Customer account
                      - generic [ref=e109]:
                        - paragraph [ref=e110]: C-001
                        - button "Edit Customer account" [ref=e111] [cursor=pointer]
                    - generic [ref=e114]:
                      - paragraph [ref=e115]: Name
                      - paragraph [ref=e119]: Customer one
                - generic [ref=e121]:
                  - generic [ref=e122]:
                    - paragraph [ref=e123]: Invoice account
                    - generic [ref=e126]:
                      - paragraph [ref=e127]: C-001
                      - button "Edit Invoice account" [ref=e128] [cursor=pointer]
                  - generic [ref=e131]:
                    - paragraph [ref=e132]: Description
                    - generic [ref=e135]:
                      - paragraph [ref=e136]: Services
                      - button "Edit Description" [ref=e137] [cursor=pointer]
                - generic [ref=e140]:
                  - paragraph [ref=e141]: INVOICE
                  - generic [ref=e142]:
                    - generic [ref=e143]:
                      - paragraph [ref=e144]: Invoice date
                      - generic [ref=e147]:
                        - paragraph [ref=e148]: 2026-11-16
                        - button "Edit Invoice date" [ref=e149] [cursor=pointer]
                    - generic [ref=e152]:
                      - paragraph [ref=e153]: Due date
                      - paragraph [ref=e157]: 2026-11-16
                    - generic [ref=e158]:
                      - paragraph [ref=e159]: Invoice
                      - generic [ref=e162]:
                        - paragraph
                - generic [ref=e164]:
                  - generic [ref=e165]:
                    - paragraph [ref=e166]: Currency
                    - generic [ref=e169]:
                      - paragraph [ref=e170]: SAR
                      - button "Edit Currency" [ref=e171] [cursor=pointer]
                  - generic [ref=e174]:
                    - paragraph [ref=e175]: Posted
                    - paragraph [ref=e179]: "No"
                  - generic [ref=e180]:
                    - paragraph [ref=e181]: Sales tax group
                    - generic [ref=e184]:
                      - paragraph [ref=e185]: VAT
                      - button "Edit Sales tax group" [ref=e186] [cursor=pointer]
                - generic [ref=e189]:
                  - paragraph [ref=e190]: PAYMENT
                  - generic [ref=e191]:
                    - generic [ref=e192]:
                      - paragraph [ref=e193]: Terms of payment
                      - generic [ref=e196]:
                        - paragraph [ref=e197]: NET30
                        - button "Edit Terms of payment" [ref=e198] [cursor=pointer]
                    - generic [ref=e201]:
                      - paragraph [ref=e202]: Method of payment
                      - generic [ref=e205]:
                        - paragraph [ref=e206]: BANK
                        - button "Edit Method of payment" [ref=e207] [cursor=pointer]
                    - generic [ref=e210]:
                      - paragraph [ref=e211]: Posting profile
                      - generic [ref=e214]:
                        - paragraph [ref=e215]: GEN
                        - button "Edit Posting profile" [ref=e216] [cursor=pointer]
          - generic [ref=e219]:
            - heading [level=3] [ref=e220]:
              - button [expanded] [ref=e221] [cursor=pointer]:
                - paragraph [ref=e223]: Invoice lines
            - region [ref=e230]:
              - generic [ref=e232]:
                - generic [ref=e233]:
                  - button "+ Add line" [disabled]
                  - button "Remove" [disabled]
                - grid [ref=e234]:
                  - button "Search" [ref=e238] [cursor=pointer]
                  - generic [ref=e246] [cursor=pointer]:
                    - generic "Select row 71" [ref=e248]:
                      - checkbox [checked] [ref=e249]
                    - generic [ref=e253]:
                      - paragraph [ref=e254]: "Description:"
                      - paragraph [ref=e256]: Consulting
                    - generic [ref=e257]:
                      - generic [ref=e258]:
                        - paragraph [ref=e259]: "Ledger account:"
                        - generic [ref=e260]: "9"
                      - generic [ref=e261]:
                        - paragraph [ref=e262]: "Sales tax group:"
                        - paragraph [ref=e264]: VAT
                      - generic [ref=e265]:
                        - paragraph [ref=e266]: "Item sales tax group:"
                        - paragraph [ref=e268]: Standard
                      - generic [ref=e269]:
                        - paragraph [ref=e270]: "Quantity:"
                        - paragraph [ref=e272]: "1"
                      - generic [ref=e273]:
                        - paragraph [ref=e274]: "Unit price:"
                        - paragraph [ref=e276]: "100"
                      - generic [ref=e277]:
                        - paragraph [ref=e278]: "Amount:"
                        - generic [ref=e279]: "100.00"
                  - generic [ref=e280]:
                    - generic [ref=e281]: "Total: 1 rows"
                    - generic [ref=e282]: 1 selected
          - generic [ref=e283]:
            - heading [level=3] [ref=e284]:
              - button [expanded] [ref=e285] [cursor=pointer]:
                - paragraph [ref=e287]: Line details
            - region [ref=e294]:
              - generic [ref=e296]:
                - tablist "Line details" [ref=e299]:
                  - tab "General" [selected] [ref=e300] [cursor=pointer]
                  - tab "Financial dimensions line" [ref=e301] [cursor=pointer]
                - generic [ref=e303]:
                  - text: Invoice text
                  - paragraph [ref=e305]: Consulting
```

# Test source

```ts
  1  | import { expect, test } from '@playwright/test';
  2  | 
  3  | test('free text invoice details edits and saves the active line, cancels changes and locks posted records', async ({ page }, info) => {
  4  |   test.setTimeout(180_000);
  5  |   await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  6  |   const date = new Date().toISOString().slice(0, 10);
  7  |   let record = { recId: 7, invoiceId: '', customerAccount: 'C-001', invoiceAccount: 'C-001', description: 'Services', invoiceDate: date, dueDate: date, currencyCode: 'SAR', paymentTerms: 'NET30', paymentMethod: 'BANK', postingProfile: 'GEN', defaultDimension: 0, taxGroup: 'VAT', posted: false, netAmount: 100, taxAmount: 15, invoiceAmount: 115, lines: [{ recId: 71, lineNum: 1, description: 'Consulting', quantity: 1, unitPrice: 100, netAmount: 100, taxAmount: 15, grossAmount: 115, ledgerDimension: 9, defaultDimension: 0, taxGroup: 'VAT', taxItemGroup: 'STANDARD' }] };
  8  |   let saves = 0, posts = 0, creates = 0;
  9  |   await page.route('http://localhost:33319/api/**', async (route) => {
  10 |     const url = new URL(route.request().url()), path = url.pathname, method = route.request().method();
  11 |     let data: unknown = [];
  12 |     if (path.endsWith('/CustTable/list')) data = [{ recId: 10, party: 10, accountNumber: 'C-001', name: 'Customer one', nameAr: 'العميل الأول', customerGroupId: 'Retail', currencyCode: 'SAR', invoiceAccount: 'C-001', paymTermId: 'NET30', paymModeId: 'BANK', taxGroupId: 'VAT', status: 'active' }];
  13 |     if (path.endsWith('/Currency')) data = [{ currencyCode: 'SAR', txt: 'Saudi Riyal' }];
  14 |     if (path.endsWith('/PaymTerm')) data = [{ paymTermId: 'NET30', description: '30 days' }];
  15 |     if (path.endsWith('/CustPaymMode')) data = [{ paymMode: 'BANK', name: 'Bank transfer' }];
  16 |     if (path.endsWith('/TaxGroup')) data = [{ taxGroup: 'VAT', taxGroupName: 'VAT' }];
  17 |     if (path.endsWith('/tax-groups')) data = { salesTaxGroups: [{ id: 'VAT', code: 'VAT', name: 'VAT' }], itemSalesTaxGroups: [{ id: 'STANDARD', code: 'STANDARD', name: 'Standard' }] };
  18 |     if (path.endsWith('/ledger-dimensions')) data = { data: [{ id: 9, code: '400100', name: 'Consulting revenue' }], pageNumber: 1, totalPages: 1, totalRecords: 1 };
  19 |     if (path.endsWith('/CustInvoiceTable') && method === 'GET') data = [record];
  20 |     if (path.endsWith('/CustInvoiceTable/7') && method === 'GET') data = record;
  21 |     if (path.endsWith('/CustInvoiceTable/7') && method === 'PUT') {
  22 |       const input = route.request().postDataJSON();
  23 |       expect(input.lines).toHaveLength(1);
  24 |       expect(input.lines[0].description).toBe('Updated consulting');
  25 |       expect(input.lines[0].ledgerDimension).toBe(9);
  26 |       expect(input.lines[0]).not.toHaveProperty('recId');
  27 |       record = { ...record, ...input, lines: [{ ...record.lines[0], ...input.lines[0], recId: 72 }] };
  28 |       saves++; data = record;
  29 |     }
  30 |     if (path.endsWith('/CustInvoiceTable') && method === 'POST') {
  31 |       const input = route.request().postDataJSON();
  32 |       expect(input.customerAccount).toBe('C-001');
  33 |       expect(input.lines[0].description).toBe('New consulting');
  34 |       expect(input.lines[0].ledgerDimension).toBe(9);
  35 |       record = { ...record, ...input, recId: 8, posted: false, lines: [{ ...record.lines[0], ...input.lines[0], recId: 81 }] };
  36 |       creates++; data = record;
  37 |     }
  38 |     if (path.endsWith('/CustInvoiceTable/8') && method === 'GET') data = record;
  39 |     if (path.endsWith('/CustInvoiceTable/7/post')) { posts++; record = { ...record, posted: true, invoiceId: 'FTI-007' }; data = record; }
  40 |     await route.fulfill({ json: { success: true, data } });
  41 |   });
  42 |   await page.goto('/accounts-receivable/free-text-invoices/7');
  43 |   await expect(page.getByText('C-001 - Customer one', { exact: true })).toBeVisible({ timeout: 60_000 });
  44 |   await expect(page.getByText('Free text invoice header', { exact: true })).toBeVisible();
  45 |   await expect(page.getByText('Invoice lines', { exact: true })).toBeVisible();
  46 |   await page.getByRole('button', { name: 'Edit', exact: true }).click();
  47 |   const line = page.locator('[data-row-id="71"]');
  48 |   await line.getByText('Consulting', { exact: true }).dblclick();
  49 |   // Save while the grid editor remains active: the page must flush it first.
> 50 |   await page.locator('input[value="Consulting"]').fill('Updated consulting');
     |                                                   ^ Error: locator.fill: Test timeout of 180000ms exceeded.
  51 |   await page.getByRole('button', { name: 'Save', exact: true }).click();
  52 |   await expect.poll(() => saves).toBe(1);
  53 |   await expect(page.getByRole('button', { name: 'Edit', exact: true })).toBeEnabled();
  54 |   await page.getByRole('button', { name: 'Edit', exact: true }).click();
  55 |   await page.getByRole('textbox', { name: 'Description', exact: true }).fill('Discard this');
  56 |   await page.getByRole('button', { name: 'Cancel', exact: true }).click();
  57 |   expect(saves).toBe(1);
  58 |   await expect(page.getByText('Discard this', { exact: true })).toHaveCount(0);
  59 |   await page.getByRole('button', { name: 'Totals', exact: true }).click();
  60 |   await expect(page.getByRole('dialog')).toContainText('115.00');
  61 |   await page.getByRole('dialog').getByRole('button', { name: 'OK', exact: true }).click();
  62 |   await page.getByRole('button', { name: 'Post', exact: true }).click();
  63 |   expect(posts).toBe(0);
  64 |   await page.getByRole('dialog').getByRole('button', { name: 'Post', exact: true }).click();
  65 |   await expect.poll(() => posts).toBe(1);
  66 |   await expect(page.getByRole('button', { name: 'Edit', exact: true })).toBeDisabled();
  67 |   await expect(page.getByRole('button', { name: 'Delete', exact: true })).toBeDisabled();
  68 |   await expect(page.getByRole('button', { name: 'Post', exact: true })).toBeDisabled();
  69 |   await page.screenshot({ path: info.outputPath('free-text-invoice-details.png'), fullPage: true });
  70 |   await page.getByRole('button', { name: 'New', exact: true }).click();
  71 |   await expect(page).toHaveURL(/free-text-invoices\/new\?customerAccount=C-001$/);
  72 |   await page.getByRole('button', { name: 'Save', exact: true }).click();
  73 |   await expect(page.getByRole('alert').filter({ hasText: 'Select a customer and invoice date' })).toBeVisible();
  74 |   expect(creates).toBe(0);
  75 |   const newRow = page.locator('[data-row-id]').first();
  76 |   await newRow.dblclick();
  77 |   // The first line's description is a plain text editor; header Description has an accessible label.
  78 |   const descriptions = page.getByRole('textbox', { name: 'Description', exact: true });
  79 |   const gridDescription = page.locator('[data-row-id] input').first();
  80 |   await gridDescription.fill('New consulting');
  81 |   await page.getByRole('combobox', { name: 'Ledger account', exact: true }).click();
  82 |   await page.getByRole('option').filter({ hasText: '400100' }).first().click();
  83 |   await page.getByRole('button', { name: 'Save', exact: true }).click();
  84 |   await expect.poll(() => creates).toBe(1);
  85 |   await expect(page).toHaveURL(/free-text-invoices\/8$/);
  86 |   await expect(descriptions).toHaveCount(0);
  87 | });
  88 | 
```