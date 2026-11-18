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
Error: locator.click: Test timeout of 180000ms exceeded.
Call log:
  - waiting for getByRole('option').filter({ hasText: '400100' }).first()

```

# Page snapshot

```yaml
- generic [ref=e1]:
  - generic [ref=e3]:
    - link "Skip to main content" [ref=e4] [cursor=pointer]:
      - /url: "#main-content"
    - banner [ref=e5]:
      - generic [ref=e6]:
        - button "App launcher" [ref=e8] [cursor=pointer]
        - paragraph [ref=e12]: Finance and Operations
        - button "Search for a page" [ref=e14] [cursor=pointer]
        - generic [ref=e18]:
          - button "Company" [ref=e19] [cursor=pointer]:
            - paragraph [ref=e20]: USMF
          - button "Notifications" [ref=e21] [cursor=pointer]
          - button "Settings" [ref=e25] [cursor=pointer]
          - button "Help" [ref=e28] [cursor=pointer]
          - button "Account" [ref=e31] [cursor=pointer]:
            - generic [ref=e32]: EA
    - generic [ref=e33]:
      - navigation "Application navigation" [ref=e34]:
        - generic [ref=e35]:
          - button "Toggle navigation" [ref=e37] [cursor=pointer]
          - generic [ref=e40]:
            - button "Home" [ref=e42] [cursor=pointer]
            - generic [ref=e46] [cursor=pointer]
            - generic [ref=e49] [cursor=pointer]
            - generic [ref=e52] [cursor=pointer]
            - generic [ref=e55] [cursor=pointer]
      - main [ref=e58]:
        - generic [ref=e60]:
          - generic [ref=e63]:
            - generic [ref=e64]:
              - button "Back" [ref=e67] [cursor=pointer]
              - button "Toggle record list" [ref=e71] [cursor=pointer]
              - generic [ref=e75]:
                - button "Save" [ref=e76] [cursor=pointer]
                - button "Cancel" [ref=e80] [cursor=pointer]
              - generic [ref=e85]:
                - button "Post" [disabled]
                - button "Totals" [disabled]
              - generic [ref=e87]:
                - button "Search" [ref=e88] [cursor=pointer]
                - button "Options" [ref=e92] [cursor=pointer]
            - generic [ref=e93]:
              - generic "Personalize" [ref=e94]:
                - button "Personalize" [disabled]
              - generic "Page guide" [ref=e95]:
                - button "Page guide" [disabled]
              - generic "Notifications" [ref=e96]:
                - button "Notifications" [disabled]
              - generic "Refresh" [ref=e97]:
                - button "Refresh" [ref=e98] [cursor=pointer]
              - generic "Open in new window" [ref=e101]:
                - button "Open in new window" [ref=e102] [cursor=pointer]
          - generic [ref=e107]:
            - paragraph [ref=e108]: Free text invoices | Standard view
            - heading "New free text invoice" [level=5] [ref=e109]
            - tablist "Free text invoices" [ref=e112]:
              - tab "Lines" [selected] [ref=e113] [cursor=pointer]
              - tab "Header" [ref=e114] [cursor=pointer]
            - generic [ref=e116]:
              - generic [ref=e117]:
                - heading [level=3] [ref=e118]:
                  - button [expanded] [ref=e119] [cursor=pointer]:
                    - paragraph [ref=e121]: Information
                - region [ref=e128]:
                  - alert [ref=e131]:
                    - generic [ref=e135]: Select a customer and invoice date. Add at least one line with a description, ledger account, positive quantity and non-negative unit price.
              - generic [ref=e136]:
                - heading [level=3] [ref=e137]:
                  - button [expanded] [ref=e138] [cursor=pointer]:
                    - paragraph [ref=e140]: Free text invoice header
                - region [ref=e147]:
                  - generic [ref=e149]:
                    - generic [ref=e150]:
                      - paragraph [ref=e151]: CUSTOMER
                      - generic [ref=e152]:
                        - generic [ref=e156]:
                          - generic [ref=e157]: Customer account
                          - generic [ref=e158]:
                            - combobox "Customer account" [ref=e159]: Customer one
                            - button "Open" [ref=e161] [cursor=pointer]
                            - group
                        - generic [ref=e164]:
                          - paragraph [ref=e165]: Name
                          - paragraph [ref=e168]: Customer one
                    - generic [ref=e170]:
                      - generic [ref=e174]:
                        - generic [ref=e175]: Invoice account
                        - generic [ref=e176]:
                          - combobox "Invoice account" [ref=e177]: Customer one
                          - button "Open" [ref=e179] [cursor=pointer]
                          - group
                      - generic [ref=e182]:
                        - paragraph [ref=e183]: Description
                        - generic [ref=e186]:
                          - textbox "Description" [ref=e187]
                          - group
                    - generic [ref=e188]:
                      - paragraph [ref=e189]: INVOICE
                      - generic [ref=e190]:
                        - generic [ref=e191]:
                          - paragraph [ref=e192]: Invoice date
                          - generic [ref=e195]:
                            - textbox "Invoice date" [ref=e196]: 2026-11-16
                            - group
                        - generic [ref=e197]:
                          - paragraph [ref=e198]: Due date
                          - generic [ref=e200]:
                            - paragraph
                        - generic [ref=e201]:
                          - paragraph [ref=e202]: Invoice
                          - generic [ref=e204]:
                            - paragraph
                    - generic [ref=e206]:
                      - generic [ref=e210]:
                        - generic [ref=e211]: Currency
                        - generic [ref=e212]:
                          - combobox "Currency" [ref=e213]: Saudi Riyal
                          - button "Open" [ref=e215] [cursor=pointer]
                          - group
                      - generic [ref=e218]:
                        - paragraph [ref=e219]: Posted
                        - paragraph [ref=e222]: "No"
                      - generic [ref=e226]:
                        - generic [ref=e227]: Sales tax group
                        - generic [ref=e228]:
                          - combobox "Sales tax group" [ref=e229]: VAT
                          - button "Open" [ref=e231] [cursor=pointer]
                          - group
                    - generic [ref=e234]:
                      - paragraph [ref=e235]: PAYMENT
                      - generic [ref=e236]:
                        - generic [ref=e240]:
                          - generic [ref=e241]: Terms of payment
                          - generic [ref=e242]:
                            - combobox "Terms of payment" [ref=e243]: 30 days
                            - button "Open" [ref=e245] [cursor=pointer]
                            - group
                        - generic [ref=e251]:
                          - generic [ref=e252]: Method of payment
                          - generic [ref=e253]:
                            - combobox "Method of payment" [ref=e254]: Bank transfer
                            - button "Open" [ref=e256] [cursor=pointer]
                            - group
                        - generic [ref=e259]:
                          - paragraph [ref=e260]: Posting profile
                          - generic [ref=e263]:
                            - textbox "Posting profile" [ref=e264]
                            - group
              - generic [ref=e265]:
                - heading [level=3] [ref=e266]:
                  - button [expanded] [ref=e267] [cursor=pointer]:
                    - paragraph [ref=e269]: Invoice lines
                - region [ref=e276]:
                  - generic [ref=e278]:
                    - generic [ref=e279]:
                      - button "+ Add line" [ref=e280] [cursor=pointer]
                      - button "Remove" [ref=e281] [cursor=pointer]
                    - grid [ref=e282]:
                      - generic [ref=e284]:
                        - generic [ref=e287]:
                          - textbox "Filter" [ref=e291]
                          - group
                        - generic [ref=e292]: 1 ROW
                      - generic [ref=e296]:
                        - generic [ref=e298]:
                          - columnheader [ref=e300] [cursor=pointer]:
                            - heading "Description" [level=6] [ref=e301]
                            - button "Open column menu for Description" [ref=e302]
                          - columnheader [ref=e307] [cursor=pointer]:
                            - heading "Ledger account" [level=6] [ref=e308]
                            - button "Open column menu for Ledger account" [ref=e309]
                          - columnheader [ref=e314] [cursor=pointer]:
                            - heading "Sales tax group" [level=6] [ref=e315]
                            - button "Open column menu for Sales tax group" [ref=e316]
                          - columnheader [ref=e321] [cursor=pointer]:
                            - heading "Item sales tax group" [level=6] [ref=e322]
                            - button "Open column menu for Item sales tax group" [ref=e323]
                          - columnheader [ref=e328] [cursor=pointer]:
                            - heading "Quantity" [level=6] [ref=e329]
                            - button "Open column menu for Quantity" [ref=e330]
                          - columnheader [ref=e335] [cursor=pointer]:
                            - heading "Unit price" [level=6] [ref=e336]
                            - button "Open column menu for Unit price" [ref=e337]
                          - columnheader [ref=e342] [cursor=pointer]:
                            - heading "Amount" [level=6] [ref=e343]
                            - button "Open column menu for Amount" [ref=e344]
                          - status [ref=e348]
                        - row [selected] [ref=e351]:
                          - gridcell [ref=e352]:
                            - textbox [ref=e356]: New consulting
                          - gridcell [ref=e357]:
                            - generic [ref=e360]:
                              - generic [ref=e361]: Ledger account
                              - generic [ref=e362]:
                                - combobox "Ledger account" [expanded] [active] [ref=e363]
                                - button "Close" [ref=e365] [cursor=pointer]
                                - group
                          - gridcell [ref=e368]:
                            - generic [ref=e371]:
                              - combobox [ref=e372] [cursor=pointer]: Select…
                              - textbox
                          - gridcell [ref=e373]:
                            - generic [ref=e376]:
                              - combobox [ref=e377] [cursor=pointer]: Select…
                              - textbox
                          - gridcell [ref=e378]:
                            - spinbutton [ref=e382]: "1"
                          - gridcell [ref=e383]:
                            - spinbutton [ref=e387]: "0"
                          - gridcell [ref=e388]:
                            - paragraph [ref=e389]: —
                      - generic [ref=e390]:
                        - generic [ref=e391]: "Total: 1 rows"
                        - generic [ref=e392]: 1 selected
                        - button "Show grid sidebar" [ref=e393] [cursor=pointer]
                    - text: Amounts, sales tax and due date are calculated when you save.
              - generic [ref=e396]:
                - heading [level=3] [ref=e397]:
                  - button [expanded] [ref=e398] [cursor=pointer]:
                    - paragraph [ref=e400]: Line details
                - region [ref=e407]:
                  - generic [ref=e409]:
                    - tablist "Line details" [ref=e412]:
                      - tab "General" [selected] [ref=e413] [cursor=pointer]
                      - tab "Financial dimensions line" [ref=e414] [cursor=pointer]
                    - generic [ref=e416]:
                      - text: Invoice text
                      - generic [ref=e417]:
                        - paragraph
          - complementary "Information" [ref=e418]:
            - generic "Filter" [ref=e419]:
              - button "Filter" [pressed] [ref=e420] [cursor=pointer]
            - generic "Information" [ref=e423]:
              - button "Information" [ref=e424] [cursor=pointer]
  - listbox "Ledger account" [ref=e428]:
    - option "Consulting revenue" [ref=e429] [cursor=pointer]
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
  50 |   await page.locator('input[value="Consulting"]').fill('Updated consulting');
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
> 82 |   await page.getByRole('option').filter({ hasText: '400100' }).first().click();
     |                                                                        ^ Error: locator.click: Test timeout of 180000ms exceeded.
  83 |   await page.getByRole('button', { name: 'Save', exact: true }).click();
  84 |   await expect.poll(() => creates).toBe(1);
  85 |   await expect(page).toHaveURL(/free-text-invoices\/8$/);
  86 |   await expect(descriptions).toHaveCount(0);
  87 | });
  88 | 
```