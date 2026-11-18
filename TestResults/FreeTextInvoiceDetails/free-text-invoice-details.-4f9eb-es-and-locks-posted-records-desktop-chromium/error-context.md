# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: free-text-invoice-details.pw.ts >> free text invoice details edits and saves the active line, cancels changes and locks posted records
- Location: e2e\free-text-invoice-details.pw.ts:3:1

# Error details

```
Error: expect(locator).toContainText(expected) failed

Locator: getByRole('alert')
Expected substring: "Select a customer and invoice date"
Error: strict mode violation: getByRole('alert') resolved to 2 elements:
    1) <div role="alert" class="MuiPaper-root MuiPaper-elevation MuiPaper-rounded MuiPaper-elevation0 MuiAlert-root MuiAlert-colorError MuiAlert-standard mui-ltr-1aaj667-MuiPaper-root-MuiAlert-root">…</div> aka getByRole('alert').filter({ hasText: 'Select a customer and invoice' })
    2) <div role="alert" direction="up" class="MuiPaper-root MuiPaper-elevation MuiPaper-rounded MuiPaper-elevation6 MuiAlert-root MuiAlert-colorSuccess MuiAlert-filled mui-ltr-fv3ouf-MuiPaper-root-MuiAlert-root">…</div> aka getByRole('alert').filter({ hasText: 'Invoice posted.' })

Call log:
  - Expect "toContainText" with timeout 5000ms
  - waiting for getByRole('alert')

```

# Page snapshot

```yaml
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
        - button "Notifications" [ref=e21] [cursor=pointer]:
          - generic [ref=e22]: "1"
        - button "Settings" [ref=e26] [cursor=pointer]
        - button "Help" [ref=e29] [cursor=pointer]
        - button "Account" [ref=e32] [cursor=pointer]:
          - generic [ref=e33]: EA
  - generic [ref=e34]:
    - navigation "Application navigation" [ref=e35]:
      - generic [ref=e36]:
        - button "Toggle navigation" [ref=e38] [cursor=pointer]
        - generic [ref=e41]:
          - button "Home" [ref=e43] [cursor=pointer]
          - generic [ref=e47] [cursor=pointer]
          - generic [ref=e50] [cursor=pointer]
          - generic [ref=e53] [cursor=pointer]
          - generic [ref=e56] [cursor=pointer]
    - main [ref=e59]:
      - generic [ref=e61]:
        - generic [ref=e64]:
          - generic [ref=e65]:
            - button "Back" [ref=e68] [cursor=pointer]
            - button "Toggle record list" [ref=e72] [cursor=pointer]
            - generic [ref=e76]:
              - button "Save" [ref=e77] [cursor=pointer]
              - button "Cancel" [ref=e81] [cursor=pointer]
            - generic [ref=e86]:
              - button "Post" [disabled]
              - button "Totals" [disabled]
            - generic [ref=e88]:
              - button "Search" [ref=e89] [cursor=pointer]
              - button "Options" [ref=e93] [cursor=pointer]
          - generic [ref=e94]:
            - generic "Personalize" [ref=e95]:
              - button "Personalize" [disabled]
            - generic "Page guide" [ref=e96]:
              - button "Page guide" [disabled]
            - generic "Notifications" [ref=e97]:
              - button "Notifications" [disabled]
            - generic "Refresh" [ref=e98]:
              - button "Refresh" [ref=e99] [cursor=pointer]
            - generic "Open in new window" [ref=e102]:
              - button "Open in new window" [ref=e103] [cursor=pointer]
        - generic [ref=e108]:
          - heading "New free text invoice" [level=1] [ref=e110]
          - generic [ref=e111]:
            - generic [ref=e112]:
              - heading [level=3] [ref=e113]:
                - button [expanded] [ref=e114] [cursor=pointer]:
                  - paragraph [ref=e116]: Information
              - region [ref=e123]:
                - alert [ref=e126]:
                  - generic [ref=e130]: Select a customer and invoice date. Add at least one line with a description, ledger account, positive quantity and non-negative unit price.
            - generic [ref=e131]:
              - heading [level=3] [ref=e132]:
                - button [expanded] [ref=e133] [cursor=pointer]:
                  - paragraph [ref=e135]: Free text invoice header
              - region [ref=e142]:
                - generic [ref=e144]:
                  - generic [ref=e145]:
                    - paragraph [ref=e146]: FIELDS.CUSTOMER
                    - generic [ref=e147]:
                      - generic [ref=e152]:
                        - generic [ref=e153]: Customer account
                        - generic [ref=e154]:
                          - combobox "Customer account"
                          - group
                      - generic [ref=e155]:
                        - paragraph [ref=e156]: Name
                        - generic [ref=e159]:
                          - paragraph
                  - generic [ref=e161]:
                    - generic [ref=e166]:
                      - generic [ref=e167]: Invoice account
                      - generic [ref=e168]:
                        - combobox "Invoice account"
                        - group
                    - generic [ref=e169]:
                      - paragraph [ref=e170]: Description
                      - generic [ref=e174]:
                        - textbox "Description"
                        - group
                  - generic [ref=e175]:
                    - paragraph [ref=e176]: INVOICE
                    - generic [ref=e177]:
                      - generic [ref=e178]:
                        - paragraph [ref=e179]: Invoice date
                        - generic [ref=e183]:
                          - textbox "Invoice date": 2026-11-16
                          - group
                      - generic [ref=e184]:
                        - paragraph [ref=e185]: Due date
                        - generic [ref=e188]:
                          - paragraph
                      - generic [ref=e189]:
                        - paragraph [ref=e190]: Invoice
                        - generic [ref=e193]:
                          - paragraph
                  - generic [ref=e195]:
                    - generic [ref=e200]:
                      - generic [ref=e201]: Currency
                      - generic [ref=e202]:
                        - combobox "Currency"
                        - group
                    - generic [ref=e203]:
                      - paragraph [ref=e204]: Posted
                      - paragraph [ref=e208]: "No"
                    - generic [ref=e213]:
                      - generic [ref=e214]: Sales tax group
                      - generic [ref=e215]:
                        - combobox "Sales tax group"
                        - group
                  - generic [ref=e216]:
                    - paragraph [ref=e217]: PAYMENT
                    - generic [ref=e218]:
                      - generic [ref=e223]:
                        - generic [ref=e224]: Terms of payment
                        - generic [ref=e225]:
                          - combobox "Terms of payment"
                          - group
                      - generic [ref=e230]:
                        - generic [ref=e231]: Method of payment
                        - generic [ref=e232]:
                          - combobox "Method of payment"
                          - group
                      - generic [ref=e233]:
                        - paragraph [ref=e234]: Posting profile
                        - generic [ref=e238]:
                          - textbox "Posting profile"
                          - group
            - generic [ref=e239]:
              - heading [level=3] [ref=e240]:
                - button [expanded] [ref=e241] [cursor=pointer]:
                  - paragraph [ref=e243]: Invoice lines
              - region [ref=e250]:
                - generic [ref=e252]:
                  - generic [ref=e253]:
                    - button "+ Add line" [ref=e254] [cursor=pointer]
                    - button "Remove" [ref=e255] [cursor=pointer]
                  - grid [ref=e256]:
                    - generic [ref=e258]:
                      - generic [ref=e261]:
                        - textbox "Filter" [ref=e265]
                        - group
                      - generic [ref=e266]: 1 ROW
                    - generic [ref=e270]:
                      - generic [ref=e272]:
                        - columnheader [ref=e274] [cursor=pointer]:
                          - heading "Description" [level=6] [ref=e275]
                          - button "Open column menu for Description" [ref=e276]
                        - columnheader [ref=e281] [cursor=pointer]:
                          - heading "Ledger account" [level=6] [ref=e282]
                          - button "Open column menu for Ledger account" [ref=e283]
                        - columnheader [ref=e288] [cursor=pointer]:
                          - heading "Sales tax group" [level=6] [ref=e289]
                          - button "Open column menu for Sales tax group" [ref=e290]
                        - columnheader [ref=e295] [cursor=pointer]:
                          - heading "Item sales tax group" [level=6] [ref=e296]
                          - button "Open column menu for Item sales tax group" [ref=e297]
                        - columnheader [ref=e302] [cursor=pointer]:
                          - heading "Quantity" [level=6] [ref=e303]
                          - button "Open column menu for Quantity" [ref=e304]
                        - columnheader [ref=e309] [cursor=pointer]:
                          - heading "Unit price" [level=6] [ref=e310]
                          - button "Open column menu for Unit price" [ref=e311]
                        - columnheader [ref=e316] [cursor=pointer]:
                          - heading "Amount" [level=6] [ref=e317]
                          - button "Open column menu for Amount" [ref=e318]
                        - status [ref=e322]
                      - row [selected] [ref=e325] [cursor=pointer]:
                        - gridcell [ref=e326]:
                          - paragraph
                        - gridcell [ref=e327]:
                          - paragraph
                        - gridcell [ref=e328]:
                          - paragraph
                        - gridcell [ref=e329]:
                          - paragraph
                        - gridcell [ref=e330]:
                          - paragraph [ref=e331]: "1"
                        - gridcell [ref=e332]:
                          - paragraph [ref=e333]: "0"
                        - gridcell [ref=e334]:
                          - paragraph [ref=e335]: —
                    - generic [ref=e336]:
                      - generic [ref=e337]: "Total: 1 rows"
                      - generic [ref=e338]: 1 selected
                      - button "Show grid sidebar" [ref=e339] [cursor=pointer]
                  - text: Amounts, sales tax and due date are calculated when you save.
            - generic [ref=e342]:
              - heading [level=3] [ref=e343]:
                - button [expanded] [ref=e344] [cursor=pointer]:
                  - paragraph [ref=e346]: Line details
              - region [ref=e353]:
                - generic [ref=e355]:
                  - tablist "Line details" [ref=e358]:
                    - tab "General" [selected] [ref=e359] [cursor=pointer]
                    - tab "Financial dimensions line" [ref=e360] [cursor=pointer]
                  - generic [ref=e362]:
                    - text: Invoice text
                    - generic [ref=e363]:
                      - paragraph
        - complementary "Information" [ref=e364]:
          - generic "Filter" [ref=e365]:
            - button "Filter" [pressed] [ref=e366] [cursor=pointer]
          - generic "Information" [ref=e369]:
            - button "Information" [ref=e370] [cursor=pointer]
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
> 73 |   await expect(page.getByRole('alert')).toContainText('Select a customer and invoice date');
     |                                         ^ Error: expect(locator).toContainText(expected) failed
  74 |   expect(creates).toBe(0);
  75 |   const newRow = page.locator('[data-row-id]').first();
  76 |   await newRow.dblclick();
  77 |   await page.locator('input').filter({ hasNot: page.locator('[type="hidden"]') }).count();
  78 |   // The first line's description is a plain text editor; header Description has an accessible label.
  79 |   const descriptions = page.getByRole('textbox', { name: 'Description', exact: true });
  80 |   const gridDescription = page.locator('[data-row-id] input').first();
  81 |   await gridDescription.fill('New consulting');
  82 |   await page.getByRole('combobox', { name: 'Ledger account', exact: true }).click();
  83 |   await page.getByRole('option').filter({ hasText: '400100' }).first().click();
  84 |   await page.getByRole('button', { name: 'Save', exact: true }).click();
  85 |   await expect.poll(() => creates).toBe(1);
  86 |   await expect(page).toHaveURL(/free-text-invoices\/8$/);
  87 |   await expect(descriptions).toHaveCount(0);
  88 | });
  89 | 
```