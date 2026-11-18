# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: contact-form.pw.ts >> contact form uses the shared master detail and saved-party logistics panels
- Location: e2e\contact-form.pw.ts:3:1

# Error details

```
Error: expect(locator).toBeVisible() failed

Locator: getByRole('heading', { name: 'Contacts', exact: true })
Expected: visible
Timeout: 30000ms
Error: element(s) not found

Call log:
  - Expect "toBeVisible" with timeout 30000ms
  - waiting for getByRole('heading', { name: 'Contacts', exact: true })

```

```yaml
- link "Skip to main content":
  - /url: "#main-content"
- banner:
  - button
  - button "Notifications"
  - button "Settings"
  - button "Account": EA
- main:
  - button "Back" [disabled]
  - button "Toggle record list"
  - button "Save"
  - button "Cancel"
  - button "Contact" [disabled]
  - button "Search" [disabled]
  - button "Options" [disabled]
  - button "Personalize" [disabled]
  - button "Page guide" [disabled]
  - button "Notifications" [disabled]
  - button "Attachments" [disabled]
  - button "Refresh" [disabled]
  - button "Open in new window" [disabled]
  - text: "Contacts: New Record | CUST-300 : Example customer"
  - heading "General" [level=3]:
    - button "General" [expanded]:
      - paragraph: General
  - region:
    - button "Show more fields"
    - paragraph: Contact
    - paragraph: Contact ID
    - paragraph: Automatic
    - paragraph: Contact for
    - paragraph: "CUST-300 : Example customer"
    - paragraph: First name
    - textbox "First name"
    - paragraph: Middle name
    - textbox "Middle name"
    - paragraph: Last name prefix
    - textbox "Last name prefix"
    - paragraph: Last name
    - textbox "Last name"
    - paragraph: Arabic name
    - textbox "Arabic name"
    - paragraph: Other information
    - paragraph: Address books
    - textbox "Address books"
    - paragraph: Language
    - textbox "Language": en-US
    - paragraph: Personal information
    - paragraph: Gender
    - combobox "Gender": None
    - button "Open"
    - paragraph: Marital status
    - combobox "Marital status": None
    - button "Open"
    - paragraph: Status
    - paragraph: Inactive
    - switch
    - paragraph: "No"
  - heading "Addresses" [level=3]:
    - button "Addresses" [expanded]:
      - paragraph: Addresses
  - region:
    - text: Save the contact to add addresses.
    - button "Add" [disabled]
    - button "Remove" [disabled]
    - button "Edit" [disabled]
    - button "Map" [disabled]
    - grid
  - heading "Contact information" [level=3]:
    - button "Contact information" [expanded]:
      - paragraph: Contact information
  - region:
    - text: Save the contact to add contact information.
    - button "Add" [disabled]
    - button "Remove" [disabled]
    - button "Edit" [disabled]
    - grid
```

# Test source

```ts
  1  | import { expect, test } from '@playwright/test';
  2  | 
  3  | test('contact form uses the shared master detail and saved-party logistics panels', async ({
  4  |   page,
  5  | }, info) => {
  6  |   test.setTimeout(100_000);
  7  |   page.setDefaultTimeout(20_000);
  8  |   const invalidPartyRequests: string[] = [];
  9  |   await page.addInitScript(() => localStorage.setItem('i18nextLng', 'en'));
  10 |   await page.route('http://localhost:33319/api/**', async (route) => {
  11 |     const path = new URL(route.request().url()).pathname;
  12 |     if (path.includes('/Party/0')) invalidPartyRequests.push(path);
  13 |     if (path.endsWith('/ContactPerson/accounts/customer/CUST-300')) {
  14 |       await route.fulfill({
  15 |         json: { accountNum: 'CUST-300', name: 'Example customer', contacts: [] },
  16 |       });
  17 |       return;
  18 |     }
  19 |     await route.fulfill({ json: { success: true, data: [] } });
  20 |   });
  21 |   await page.goto('/contacts/customer/CUST-300/new');
> 22 |   await expect(page.getByRole('heading', { name: 'Contacts', exact: true })).toBeVisible({
     |                                                                              ^ Error: expect(locator).toBeVisible() failed
  23 |     timeout: 30_000,
  24 |   });
  25 |   await expect(
  26 |     page.getByRole('button', { name: 'Toggle record list', exact: true })
  27 |   ).toHaveAttribute('aria-pressed', 'false');
  28 |   await expect(page.getByRole('button', { name: 'Contact', exact: true })).toBeDisabled();
  29 |   await expect(page.getByRole('button', { name: 'Save', exact: true })).toBeEnabled();
  30 |   await page.getByRole('textbox', { name: 'First name', exact: true }).fill('Sara');
  31 |   await expect(page.getByRole('button', { name: 'Save', exact: true })).toBeEnabled();
  32 |   await expect(page.getByText('Save the contact to add addresses.', { exact: true })).toBeVisible();
  33 |   await expect(
  34 |     page.getByText('Save the contact to add contact information.', { exact: true })
  35 |   ).toBeVisible();
  36 |   await page.getByRole('button', { name: 'Show more fields', exact: true }).click();
  37 |   await expect(page.getByRole('textbox', { name: 'Initials', exact: true })).toBeVisible();
  38 |   await page.getByRole('button', { name: 'Show fewer fields', exact: true }).click();
  39 |   expect(invalidPartyRequests).toEqual([]);
  40 |   await page.screenshot({ path: info.outputPath('contact-form.png'), fullPage: true });
  41 | });
  42 | 
```