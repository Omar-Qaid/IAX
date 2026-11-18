# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: customer-components.pw.ts >> customer components persist edits, recover failures, maintain logistics and navigate selected records
- Location: e2e\customer-components.pw.ts:3:1

# Error details

```
TimeoutError: locator.click: Timeout 20000ms exceeded.
Call log:
  - waiting for getByRole('dialog').getByRole('button', { name: 'Delete', exact: true })
    - locator resolved to <button tabindex="0" type="button" class="MuiButtonBase-root MuiButton-root MuiButton-contained MuiButton-sizeMedium MuiButton-colorError mui-ltr-x4moz5-MuiButtonBase-root-MuiButton-root">…</button>
  - attempting click action
    - waiting for element to be visible, enabled and stable
    - element is visible, enabled and stable
    - scrolling into view if needed
    - done scrolling
    - <div tabindex="-1" role="presentation" class="MuiDialog-container MuiDialog-scrollPaper mui-ltr-45uc01-MuiDialog-container">…</div> intercepts pointer events
  - retrying click action
    - waiting for element to be visible, enabled and stable
    - element is visible, enabled and stable
    - scrolling into view if needed
    - done scrolling
    - <div class="MuiDialogActions-root MuiDialogActions-spacing mui-ltr-zkegts-MuiDialogActions-root">…</div> intercepts pointer events
  - retrying click action
    - waiting 20ms
    2 × waiting for element to be visible, enabled and stable
      - element is visible, enabled and stable
      - scrolling into view if needed
      - done scrolling
      - <div tabindex="-1" role="presentation" class="MuiDialog-container MuiDialog-scrollPaper mui-ltr-45uc01-MuiDialog-container">…</div> intercepts pointer events
    - retrying click action
      - waiting 100ms
    9 × waiting for element to be visible, enabled and stable
      - element is visible, enabled and stable
      - scrolling into view if needed
      - done scrolling
      - <div tabindex="-1" role="presentation" class="MuiDialog-container MuiDialog-scrollPaper mui-ltr-45uc01-MuiDialog-container">…</div> intercepts pointer events
    - retrying click action
      - waiting 500ms
      - waiting for element to be visible, enabled and stable
      - element is visible, enabled and stable
      - scrolling into view if needed
      - done scrolling
      - <button tabindex="0" type="button" class="MuiButtonBase-root MuiButton-root MuiButton-outlined MuiButton-sizeMedium MuiButton-colorInherit mui-ltr-1vgvxuw-MuiButtonBase-root-MuiButton-root">Cancel</button> intercepts pointer events
    - retrying click action
      - waiting 500ms
      - waiting for element to be visible, enabled and stable
      - element is visible, enabled and stable
      - scrolling into view if needed
      - done scrolling
      - <div tabindex="-1" role="presentation" class="MuiDialog-container MuiDialog-scrollPaper mui-ltr-45uc01-MuiDialog-container">…</div> intercepts pointer events
    - retrying click action
      - waiting 500ms
      - waiting for element to be visible, enabled and stable
      - element is visible, enabled and stable
      - scrolling into view if needed
      - done scrolling
      - <div tabindex="-1" role="presentation" class="MuiDialog-container MuiDialog-scrollPaper mui-ltr-45uc01-MuiDialog-container">…</div> intercepts pointer events
    - retrying click action
      - waiting 500ms
    - waiting for element to be visible, enabled and stable
    - element is visible, enabled and stable
    - scrolling into view if needed
    - done scrolling
    - <div tabindex="-1" role="presentation" class="MuiDialog-container MuiDialog-scrollPaper mui-ltr-45uc01-MuiDialog-container">…</div> intercepts pointer events
  - retrying click action
    - waiting 500ms
    - waiting for element to be visible, enabled and stable
    - element is visible, enabled and stable
    - scrolling into view if needed
    - done scrolling
    - <button tabindex="0" type="button" class="MuiButtonBase-root MuiButton-root MuiButton-outlined MuiButton-sizeMedium MuiButton-colorInherit mui-ltr-1vgvxuw-MuiButtonBase-root-MuiButton-root">Cancel</button> intercepts pointer events
  - retrying click action
    - waiting 500ms

```

# Page snapshot

```yaml
- generic [ref=f1e1]:
  - link [ref=f1e2] [cursor=pointer]:
    - /url: "#main-content"
    - text: Skip to main content
  - banner [ref=f1e3]:
    - button [ref=f1e5] [cursor=pointer]
    - generic [ref=f1e8]:
      - button [ref=f1e9] [cursor=pointer]
      - button [ref=f1e13] [cursor=pointer]
      - button [ref=f1e16] [cursor=pointer]:
        - generic [ref=f1e17]: EA
  - main [ref=f1e19]:
    - generic [ref=f1e20]:
      - generic [ref=f1e21]:
        - button [ref=f1e24] [cursor=pointer]: Back
        - generic [ref=f1e29]:
          - button [ref=f1e30] [cursor=pointer]: Edit
          - button [ref=f1e34] [cursor=pointer]: New
          - button [ref=f1e38] [cursor=pointer]: Delete
        - generic [ref=f1e43]:
          - button [ref=f1e44] [cursor=pointer]: Customer
          - button [ref=f1e45] [cursor=pointer]: View contacts
          - button [ref=f1e46] [cursor=pointer]: Transactions
          - button [ref=f1e47] [cursor=pointer]: Open transactions
          - button [ref=f1e48] [cursor=pointer]: Balance
          - button [ref=f1e49] [cursor=pointer]: Sales orders
        - generic [ref=f1e51]:
          - button [ref=f1e52] [cursor=pointer]: Search
          - button [ref=f1e56] [cursor=pointer]: Options
      - generic [ref=f1e57]:
        - button [ref=f1e59] [cursor=pointer]:
          - generic [ref=f1e60]: "0"
        - button [ref=f1e65] [cursor=pointer]
        - button [ref=f1e69] [cursor=pointer]
    - grid [ref=f1e73]:
      - button [ref=f1e76] [cursor=pointer]
      - generic [ref=f1e80] [cursor=pointer]:
        - checkbox [checked] [ref=f1e83]
        - generic [ref=f1e86]:
          - paragraph [ref=f1e87]: "Account:"
          - link [ref=f1e89]:
            - /url: /accounts-receivable/customers/1
            - text: C-001
          - generic [ref=f1e90]: Active
        - paragraph [ref=f1e93]: "Name:"
        - paragraph [ref=f1e94]: "Arabic name:"
        - paragraph [ref=f1e95]: "Invoice account:"
        - paragraph [ref=f1e96]: "Customer group:"
        - paragraph [ref=f1e97]: "Currency:"
        - paragraph [ref=f1e98]: "Phone:"
        - paragraph [ref=f1e99]: "Sales tax group:"
        - paragraph [ref=f1e100]: "Terms of payment:"
      - generic [ref=f1e102] [cursor=pointer]:
        - checkbox [ref=f1e105]
        - generic [ref=f1e108]:
          - paragraph [ref=f1e109]: "Account:"
          - link [ref=f1e111]:
            - /url: /accounts-receivable/customers/2
            - text: C-002
          - generic [ref=f1e112]: Active
        - paragraph [ref=f1e115]: "Name:"
        - paragraph [ref=f1e116]: "Arabic name:"
        - paragraph [ref=f1e117]: "Invoice account:"
        - paragraph [ref=f1e118]: "Customer group:"
        - paragraph [ref=f1e119]: "Currency:"
        - paragraph [ref=f1e120]: "Phone:"
        - paragraph [ref=f1e121]: "Sales tax group:"
        - paragraph [ref=f1e122]: "Terms of payment:"
      - generic [ref=f1e123]:
        - generic [ref=f1e124]: "Total: 2 rows"
        - generic [ref=f1e125]: 1 selected
  - dialog [active] [ref=f1e128]:
    - heading "Confirm deletion Close" [level=2] [ref=f1e129]:
      - generic [ref=f1e133]: Confirm deletion
      - button "Close" [ref=f1e134] [cursor=pointer]
    - paragraph [ref=f1e138]: Delete 1 selected record(s)?
    - generic [ref=f1e139]:
      - button "Cancel" [ref=f1e140] [cursor=pointer]
      - button "Delete" [ref=f1e141] [cursor=pointer]
```

# Test source

```ts
  122 |   await edit().click();
  123 |   const name = page.getByRole('textbox', { name: 'Name', exact: true });
  124 |   await name.fill('Cancelled name');
  125 |   await page.getByRole('button', { name: 'Cancel', exact: true }).first().click();
  126 |   await expect(page.getByText('C-001 : Audit customer')).toBeVisible();
  127 |   expect(updates).toBe(0);
  128 |   await edit().click();
  129 |   await name.fill('');
  130 |   await page.getByRole('button', { name: 'Save', exact: true }).first().click();
  131 |   expect(updates).toBe(0);
  132 |   await expect(name).toBeVisible();
  133 |   await name.fill('Updated audit customer');
  134 |   refuseUpdate = true;
  135 |   await page.getByRole('button', { name: 'Save', exact: true }).first().click();
  136 |   await expect(page.getByText('Customer update refused', { exact: true })).toBeVisible();
  137 |   await expect(name).toHaveValue('Updated audit customer');
  138 |   await page.getByRole('combobox', { name: 'Site', exact: true }).click();
  139 |   await page.getByRole('option', { name: 'S2 - Site Two', exact: true }).click();
  140 |   await page.getByRole('combobox', { name: 'Warehouse', exact: true }).click();
  141 |   await expect(page.getByRole('option', { name: 'W1 - Warehouse One', exact: true })).toHaveCount(
  142 |     0,
  143 |   );
  144 |   await page.getByRole('option', { name: 'W2 - Warehouse Two', exact: true }).click();
  145 |   await page.getByRole('button', { name: 'Save', exact: true }).first().click();
  146 |   await expect(page.getByRole('button', { name: 'Save', exact: true })).toHaveCount(0);
  147 |   await expect(page.getByText('C-001 : Updated audit customer')).toBeVisible();
  148 |   expect(records[0].inventSiteId).toBe('S2');
  149 |   expect(records[0].inventLocationId).toBe('W2');
  150 |   await edit().click();
  151 |   const contactPanel = page
  152 |     .locator('.MuiAccordion-root')
  153 |     .filter({ has: page.getByRole('button', { name: 'Contact information', exact: true }) });
  154 |   await contactPanel.getByRole('button', { name: 'Add', exact: true }).click();
  155 |   const drawer = page.getByRole('dialog');
  156 |   await drawer.getByRole('textbox', { name: 'Description', exact: true }).fill('Audit phone');
  157 |   await drawer
  158 |     .getByRole('textbox', { name: 'Contact number/address', exact: true })
  159 |     .fill('0500000000');
  160 |   await drawer.getByRole('button', { name: 'OK', exact: true }).click();
  161 |   await expect(contactPanel.getByText('Audit phone', { exact: true })).toBeVisible();
  162 |   await contactPanel.getByText('Audit phone', { exact: true }).click();
  163 |   await contactPanel.getByRole('button', { name: 'Edit', exact: true }).click();
  164 |   await drawer
  165 |     .getByRole('textbox', { name: 'Contact number/address', exact: true })
  166 |     .fill('0511111111');
  167 |   await drawer.getByRole('button', { name: 'OK', exact: true }).click();
  168 |   await expect(contactPanel.getByText('0511111111', { exact: true })).toBeVisible();
  169 |   await contactPanel.getByRole('button', { name: 'Remove', exact: true }).click();
  170 |   await expect(contactPanel.getByText('Audit phone', { exact: true })).toHaveCount(0);
  171 |   expect(contacts).toHaveLength(0);
  172 |   const addressPanel = page
  173 |     .locator('.MuiAccordion-root')
  174 |     .filter({ has: page.getByRole('button', { name: 'Addresses', exact: true }) });
  175 |   await addressPanel.getByRole('button', { name: 'Add', exact: true }).click();
  176 |   await drawer
  177 |     .getByRole('textbox', { name: 'Name or description', exact: true })
  178 |     .fill('Audit address');
  179 |   await drawer.getByRole('combobox', { name: 'Country/region', exact: true }).click();
  180 |   await page.getByRole('option', { name: 'SAU - Saudi Arabia', exact: true }).click();
  181 |   await drawer.getByRole('textbox', { name: 'Street', exact: true }).fill('Audit street');
  182 |   await drawer.getByRole('button', { name: 'OK', exact: true }).click();
  183 |   await expect(addressPanel.getByText('Audit address', { exact: true })).toBeVisible();
  184 |   await addressPanel.getByText('Audit address', { exact: true }).click();
  185 |   await addressPanel.getByRole('button', { name: 'Edit', exact: true }).click();
  186 |   await drawer.getByRole('textbox', { name: 'Street', exact: true }).fill('Edited audit street');
  187 |   await drawer.getByRole('button', { name: 'OK', exact: true }).click();
  188 |   await expect(addressPanel.getByText('Edited audit street', { exact: true })).toBeVisible();
  189 |   await addressPanel.getByRole('button', { name: 'Remove', exact: true }).click();
  190 |   await expect(addressPanel.getByText('Audit address', { exact: true })).toHaveCount(0);
  191 |   expect(addresses).toHaveLength(0);
  192 |   await page.getByRole('button', { name: 'Cancel', exact: true }).first().click();
  193 |   await page.getByRole('button', { name: 'Options', exact: true }).click();
  194 |   await page.getByRole('button', { name: 'Record Info', exact: true }).click();
  195 |   await expect(page.getByRole('dialog')).toBeVisible();
  196 |   await page.keyboard.press('Escape');
  197 |   await page.getByRole('button', { name: 'Customer', exact: true }).click();
  198 |   await page
  199 |     .locator('#customer-action-ribbon')
  200 |     .getByRole('button', { name: 'Attachments', exact: true })
  201 |     .click();
  202 |   await expect(page).toHaveURL(new RegExp('documents/docu-view'));
  203 |   await page.goBack();
  204 |   await page.getByRole('button', { name: 'Search', exact: true }).click();
  205 |   const filter = page.locator('input[placeholder="Filter"]:visible');
  206 |   await filter.fill('C-002');
  207 |   await page.getByRole('button', { name: /Second customer.*C-002|C-002.*Second customer/ }).click();
  208 |   if (testInfo.project.name === 'mobile-chromium') await page.keyboard.press('Escape');
  209 |   await expect(page.getByText('C-002 : Second customer')).toBeVisible();
  210 |   await page.getByRole('button', { name: 'Customer', exact: true }).click();
  211 |   await page
  212 |     .locator('#customer-action-ribbon')
  213 |     .getByRole('button', { name: 'Open transactions', exact: true })
  214 |     .click();
  215 |   await expect(page).toHaveURL(new RegExp('customer-open-transactions/C-002$'));
  216 |   await page.goto('/accounts-receivable/customers');
  217 |   await expect(page.getByRole('link', { name: 'C-001', exact: true })).toBeVisible();
  218 |   const before = reads;
  219 |   await page.getByRole('button', { name: 'Refresh', exact: true }).click();
  220 |   await expect.poll(() => reads).toBeGreaterThan(before);
  221 |   await page.getByRole('button', { name: 'Delete', exact: true }).first().click();
> 222 |   await page.getByRole('dialog').getByRole('button', { name: 'Delete', exact: true }).click();
      |                                                                                       ^ TimeoutError: locator.click: Timeout 20000ms exceeded.
  223 |   await expect(page.getByRole('link', { name: 'C-001', exact: true })).toHaveCount(0);
  224 |   expect(records).toHaveLength(1);
  225 | });
  226 | 
```