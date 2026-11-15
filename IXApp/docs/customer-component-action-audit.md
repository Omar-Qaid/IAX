# Customer page component and action audit

Scope: existing customer list/detail, their supported basic ERP actions, balance/open inquiries, postal/electronic addresses, and linked payment-method/payment-term setup. Earlier ERP/full-frontend run failures remain separate evidence. No advanced Service, Commerce, marketing, collection-letter or interest workflows were added.

## Fixes

| Area | Finding and final behavior |
| --- | --- |
| Customer list actions | Empty command tabs had no destinations. The list now exposes implemented selected-customer navigation: Customer, Open transactions, Balance and Sales orders, with applicable permissions. |
| Refresh | The controlled customer list did not supply a refresh callback. Refresh now reloads the API data. |
| Company switching | Customer/detail/quick-create lookup and dimension caches omitted company. Requests and detail component identity now follow the selected company; a behavioral regression switches between customers sharing the same route ID in different companies. |
| CRUD permissions | EnterpriseCrudActions accepted permission props but did not forward them. Edit, New, Delete and Save now respect the supplied permission; Cancel remains available. |
| Failed customer save | The generic detail pattern replaced the editor with an error page. Existing records and active drafts now remain visible with an inline error, allowing correction and retry. Fatal initial load errors retain the existing retry page. |
| Optional values | Omitted optional customer properties could become literal "undefined" during edit/save. The API adapter normalizes them to empty values and preserves bilingual names. |
| Site and warehouse | Warehouse choices included other sites; changing a site retained an incompatible warehouse. Choices now follow the site, and incompatible prior selections are cleared when the site changes. |
| Contact/address save | Drawers closed before persistence completed. They now await the save, prevent duplicate submission, show server failures and retain the draft for retry. Closing/cancel is disabled while saving. |
| Mobile selection | Tapping a grid card did not select its record, leaving Edit/Remove unavailable. Card taps now select records consistently with the table; the checkbox continues to stop event propagation. |
| Search | Customer Search could toggle an invisible filter while the mobile record list was closed. The generic default now opens the record list and ensures the filter is visible. Pages with an explicit onSearch handler retain that handler. |
| Mobile toolbar | Sticky utilities overlapped main actions. Utilities now scroll after the main actions on narrow viewports, and action buttons have explicit accessible names. |
| Address Map | The read action was disabled together with mutation actions in view mode. A saved selected address can now open its map without entering edit mode; mutation actions stay disabled. |
| Contacts label | The basic contact editor was labeled Advanced. It is now accurately labeled Edit. |
| Payment setup | The linked term/method pages only changed local arrays of example data. They now use the existing generic API transport and list-details pattern with company-aware caching, actual API permissions, persistent CRUD and lookup invalidation. Basic codes/descriptions and term days/months or method grace days are editable; other returned DTO fields are preserved on update. Advanced placeholder fields/actions were removed from these basic editors. |
| Related information | Empty/unimplemented FactBoxes and the misleading country-as-address summary were removed. The remaining statistics show actual customer fields. |
| Utilities | Open in new window now opens the current URL safely. Personalize, Page guide and toolbar Notifications have no implemented page handlers and are disabled instead of appearing clickable. Optional callbacks support future actual implementations. |

## Checked workflows

| Workflow | Evidence |
| --- | --- |
| List, create, save-and-open, edit and invalid customer route | customers.pw.ts, desktop and mobile |
| Cancel and required-name validation | customer-components.pw.ts |
| Failed customer save, retained editable draft and retry | customer-components.pw.ts |
| Site/warehouse choice and persisted defaults | customer-components.pw.ts |
| Contact and postal-address add/edit/remove | customer-components.pw.ts, desktop and mobile |
| Contact/address failed save, retry, pending-save duplicate prevention | CustomerLogisticsSave.test.tsx |
| Map in view mode and mutation controls remaining disabled | CustomerReadOnlyMap.test.tsx |
| Record Info and record-bound attachments navigation | customer-components.pw.ts |
| Record Search, select another customer, selected-account inquiry | customer-components.pw.ts |
| Customer list Refresh and confirmed Delete | customer-components.pw.ts |
| Customer Balance/Open transactions/Sell navigation and selected-customer order filter | customer-balance.pw.ts |
| Read-only balance toolbar, currency rows and initial desktop/mobile list state | customer-balance.pw.ts |
| Payment term/method create, reload, edit, delete and preserved returned fields | customer-payment-setup.pw.ts and CustomerPaymentSetupApi.test.ts |
| Company isolation, optional-value normalization and bilingual names | CustomerCompanyIsolation.test.tsx, CustomerApiNormalization.test.ts, existing localization/balance cases |
| Permission enforcement and shared action behavior | CustomerActionPermissions.test.tsx and existing ActionPane tests |
| Customer service/mapping/open-balance reads and invalid-change rollback | customer-maintenance.trx, including an opt-in disposable SQL Server test |

## Validation records

Final scoped runs:

- Frontend unit/component tests: **33 passed, 0 failed**.
- Browser scenarios: **10 passed, 0 failed** across desktop and mobile Chromium.
- Backend customer/mapping/inquiry checks: **19 passed, 0 failed, 0 skipped**, including disposable SQL Server maintenance.
- Production build, targeted ESLint, architecture, UTF-8 and whitespace checks passed.

These are separate runs, not a combined full-ERP result.

Artifacts:

- TestResults/CustomerFrontend/customer-components-unit.json
- TestResults/CustomerFrontend/browser-final/
- TestResults/CustomerFrontend/customer-maintenance.trx
- TestResults/CustomerFrontend/customer-audit-build.log

Browser tests use isolated API fixtures; they do not change customer/application data. The SQL maintenance check uses its own disposable database. This audit adds no backend entities, migrations or posting logic. The work is a scoped customer validation, not a claim that the entire ERP or frontend suite is green. Missing Invoice/Collect/bank-account destination forms remain pending the separately requested D365 screenshots; no unrelated form is substituted for them.

Address/contact dialogs save independently through their existing Organization endpoints. Customer edit permission does not grant those separate backend permissions; refusals are displayed while preserving the draft. Parent Cancel reverts customer fields and does not undo an already-confirmed address/contact save.
