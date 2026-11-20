# AR QA checklist — Settlement, Sales Order, Free Text Invoice

Statuses: Not Tested · Failed · Fixed and Retested · Passed · Blocked · Not Applicable

Verification levels used below:
- **Static** — read the implementation and confirmed wiring//state logic.
- **Automated** — a test in `IXApp/src/test` or `IXApi/Tests` exercises it.
- **Live** — exercised against the running API.

---

## 1. Cross-cutting defects found and fixed

| Module | Page/Action | Scenario | Expected Result | Actual Result | Status | Fix/Evidence |
|---|---|---|---|---|---|---|
| Free Text Invoice | Create (POST `/v1/CustInvoiceTable`) | Save a new invoice | 201 + invoice persisted | **500** `SqlServerRetryingExecutionStrategy does not support user-initiated transactions` | Fixed and Retested | `CreateAsync` called `SaveAsync` outside the retrying execution strategy. Wrapped in `Db.Database.CreateExecutionStrategy().ExecuteAsync(...)` — `FreeTextInvoiceService.cs:58` |
| Free Text Invoice | Update (PUT) | Save an edited invoice | 200 + changes persisted | **500**, same cause | Fixed and Retested | Wrapped — `FreeTextInvoiceService.cs:64` |
| Free Text Invoice | Delete (DELETE) | Delete an unposted invoice | 200 + row removed | **500**, same cause | Fixed and Retested | Extracted `DeleteCoreAsync` and wrapped — `FreeTextInvoiceService.cs:250` |
| Free Text Invoice | Ledger account column | Open an invoice whose line uses any ledger account | Column shows the main account code | Showed a **raw RecId** whenever the account was not in the first 100 lookup rows (server clamps page size to 100) | Fixed and Retested | Server now returns the code per line: `LedgerAccount` added to `FreeTextInvoiceLineDto`, resolved by `LedgerAccountsAsync` for exactly the ids in use; UI renders it and no longer falls back to the RecId |
| Shared (all grids) | CSV export | Export a grid | Header row shows translated labels | Exported the **raw translation keys** (`fields.description`) | Fixed and Retested | `generateCSV` takes a header translator; `DataGrid`/`DataGridBody` pass `t` |
| Shared (all grids) | Autosize columns | Autosize | Width fits the visible label | Measured the **key** length, so widths were wrong | Fixed and Retested | `useGridAutosize` takes `translateHeader` |
| Shared (all grids) | Filter popover / Filters panel / Features panel / filter icon aria-label | Open each | Shows the translated column name | Showed the **raw key** (or untranslated text) | Fixed and Retested | 4 sites now call `t(headerName)` |
| Shared (lookups) | Any server-paged lookup | Scroll through pages | Each row rendered once | React console: *"Encountered two children with the same key"* | Fixed and Retested | `useLookupGridField` de-duplicates accumulated rows by `id` — offset paging can repeat a row when data shifts between page requests |
| Sales Order | Record list pane | Toggle the list pane, reopen the page | Pane visibility is remembered | Not remembered — the V2 move to `MasterDetailPage` dropped persistence that `ListDetailsPage` had | Fixed and Retested | Added `navigationVisibilityStorageKey` to the pattern, defaulting to `ixapp.master-detail.<recordTableName>.list-visible`. `SalesOrderDetailsPage.test.tsx` *"toggles the record sidebar"* now passes |
| Sales Order | Line grid headers (Arabic) | View lines in Arabic | Headers translated | **6 headers had no key in either locale** (Item number, Product name, Unit, Net amount, Site, Warehouse) so Arabic showed English | Fixed and Retested | Added `salesOrder.{itemNumber,productName,unit,netAmount}` to en + ar; pointed Site/Warehouse at existing `fields.site`/`fields.warehouse`. All 13 keys verified present in both locales |
| Settlement / FTI / Sales Order | Column headers | — | `headerName` holds a translation key (grid translates it) | Settlement, free-text-invoice and sales-order line columns passed **pre-translated** text, so the key-based sites double-translated | Fixed and Retested | Converted to keys; all keys verified to resolve in en + ar |

---

## 2. Sales Order — action inventory

Ribbon state extracted from `details/useSalesOrderDocument.tsx` and cross-checked against the
supplied screenshot. Enabled/disabled states **match the screenshot exactly**.

| Action | Group | Wired | Status |
|---|---|---|---|
| Maintain charges | Charges | yes | Static: Passed |
| Recalculate discounts (`total-discount`) | Calculate | yes | Static: Passed |
| Confirmation | Generate | yes | Static: Passed |
| Pro forma confirmation | Generate | yes | Static: Passed |
| Packing slip | Pick and pack | yes | Static: Passed |
| Confirm now | Actions | yes | Static: Passed |
| Sales order confirmations | Journals | yes | Static: Passed |
| Packing slip journal | Journals | yes | Static: Passed |
| Cancel | Maintain | yes | Static: Passed |
| Copy from all | Copy | yes | Static: Passed |
| Copy from journal | Copy | yes | Static: Passed |
| Totals | View | yes | Static: Passed |
| Credit note | Credit note | no | Not Applicable — not implemented, disabled |
| Allocate charges | Charges | no | Not Applicable — not implemented, disabled |
| Sales tax | Tax | no | Not Applicable — not implemented, disabled |
| Confirmed delivery dates | Calculate | no | Not Applicable — not implemented, disabled |
| Supplementary items | Calculate | no | Not Applicable — not implemented, disabled |
| Tiered charges | Calculate | no | Not Applicable — not implemented, disabled |
| Push price and totals | Calculate | no | Not Applicable — not implemented, disabled |
| Service agreement | Apply | no | Not Applicable — not implemented, disabled |
| Quotation confirmation journal | Journals | no | Not Applicable — not implemented, disabled |
| Prepayment | Prepayment | no | Not Applicable — not implemented, disabled |
| Purchase order | New | no | Not Applicable — not implemented, disabled |
| Direct delivery | New | no | Not Applicable — not implemented, disabled |
| Payments | Payments | no | Not Applicable — not implemented, disabled |
| Order events | View | no | Not Applicable — not implemented, disabled |
| Recap | Functions | no | Not Applicable — not implemented, disabled |
| Order holds | Functions | no | Not Applicable — not implemented, disabled |
| Notes | Attachments | no | Not Applicable — not implemented, disabled |

---

## 3. Open items

| Item | Status | Note |
|---|---|---|
| `InventDimensionResolver` transaction | Not Tested | Only AR service that begins a transaction without the execution strategy. It guards on `CurrentTransaction is null`, so it is safe inside the (wrapped) line services that call it; it would throw if ever called standalone. Left unchanged — no reproduction. |
| Live end-to-end posting flows | Blocked | Require an authenticated browser session; the API rejects unauthenticated calls (401). |
