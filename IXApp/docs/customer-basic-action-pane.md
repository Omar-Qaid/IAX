# Customer action pane — basic ERP

The supplied customer action-pane and customer-balance screenshots are the UI references. Work starts in the Customer tab. Prior test/report changes in the checkout are preserved.

| Action                                                                                                                                                                                                        | Current result                                                                                                                                                          |
| ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Contact information                                                                                                                                                                                           | Enabled for a saved customer with a party. Expands, scrolls to and focuses the existing contact-information section. This is not a newly invented contact-person form.  |
| Balance                                                                                                                                                                                                       | Enabled for saved customers with `AccountsReceivable.Transactions.View`. Opens `CustOpenBalanceCurrency`.                                                               |
| Attachments                                                                                                                                                                                                   | Enabled for saved customers. Opens the existing document form with the selected `CustTable` record reference.                                                           |
| Transactions / open transactions                                                                                                                                                                              | Read-only Open transactions inquiry enabled using the typed open-balance API and shared list pattern. The complete transaction/settlement forms still need screenshots. |
| Bank accounts                                                                                                                                                                                                 | Backend exists; no frontend customer-bank form exists. Awaiting its D365 screenshot.                                                                                    |
| Contact persons                                                                                                                                                                                               | No confirmed frontend form. Awaiting its D365 screenshot.                                                                                                               |
| Balance Details / Aging periods                                                                                                                                                                               | Backend inquiries exist, but their form layout has not been supplied. Awaiting screenshots.                                                                             |
| Forecast, global transactions, party reassociation, credit cards, summary update, product filters, advanced notes, catalogs, customer service, registration searches, electronic document properties, rebates | Removed from the basic Customer tab; no disabled placeholder destinations are advertised.                                                                               |

The former empty top-level customer command placeholders are removed. Sell now contains Sales orders, which reuses the existing list and filters it to the selected customer. Existing edit/new/delete and the shared options controls remain. Other transaction tabs can be implemented later when their workflows and D365 forms are confirmed.

## Balance

Both `/accounts-receivable/customer-balances/:accountNum` and `/accounts-payable/vendor-balances/:accountNum` use one layout. Customer entry is wired now; vendor balance is available for reuse when the vendor master frontend is implemented. No vendor master form has been created.

Read-only API projections are `GET /api/v1/CustTrans/balance-currency?accountNum=...` and the corresponding `VendTrans` route. They use the existing master, party, ledger, transaction and open-transaction tables in the selected company. They do not alter posting, settlement, schema or application data. Existing `open-balance` responses are unchanged.

The new read model returns separate transaction currencies, stored accounting balances, and customer reporting balances after reporting settlements. `VendTrans` currently has no reporting-currency amount fields; vendor reporting values remain unavailable. Related account balances have separate backend permission checks because they are fetched through their own protected endpoint.

## Validation

- Focused backend: `PartyOpenBalanceCurrencyTests` and `OpenBalanceReadTests` — 15 passed.
- Current focused UI/localization run: 18 passed. Separate open-transactions API tests: 3 passed. These cover required account scope, cancellation and API errors; they do not constitute a complete-suite pass.
- Browser: desktop Chromium and mobile Chromium — 2 passed. Both rendered balance screenshots were reviewed. Browser cases use isolated API mocks; backend read tests use isolated SQLite databases.
- Final production build, targeted ESLint, architecture and UTF-8 encoding checks passed.
- The unrestricted full frontend run was stopped after worker-termination timeouts and failures in LegalEntityPage, RepresentativePages, ProcessBuilderPage and SalesOrderDetailsPage. It is not a green run. The one-worker follow-up also failed to finish in the bounded validation window and was stopped; no complete-suite pass is claimed. These failures have not been proven pre-existing.
- The fresh full backend run encountered a SQL scratch-database disposal timeout in PostingConcurrencyTests; it is not a green run. Cleanup command timeout was aligned with the fixture’s 120-second create timeout; its focused rerun passed 5 tests, with 0 failed and 0 skipped. The failed broad run was interrupted and remains non-green. A read-only sys.databases check found no disposable test databases created by today’s run remaining.

The balance page now follows the last supplied image: compact summary columns, underlined account field, selected first currency row, narrow columns, no cell borders, functional Open transactions and shared Options controls. A shared-grid fix makes its filler respect the border setting.

See [the per-tab and per-option D365 research](customer-action-pane-research.md) for sources, scope decisions and unverified destinations.

The screenshot references authorize the balance form. They do not establish the layout of other D365 forms; those remain pending the user's screenshots.

## Generic balance action pane update

The balance page now uses ListDetailsPage (enterprise) with shared EditableViewField outputs and DataGrid. Its header follows the supplied generic action pane: Back, record-list toggle, disabled Edit/New/Delete, Customer ribbon, Search, Options and existing utility controls. Open transactions is inside Customer; Search toggles currency-grid filters. No balance or customer mutation is wired to the disabled CRUD controls. The generic pattern receives the existing saved customer/vendor record for Options and attachments.

Focused balance/action-pane rerun: 13 passed. Desktop/mobile Chromium: 2 passed. Targeted lint, architecture and UTF-8 checks passed. Production build passed. Browser assertions exercise disabled CRUD, Search filters and Customer → Open transactions, then the existing customer sales-order navigation. Earlier broad-run failures remain unchanged evidence.

The shared record list now opens initially on desktop, matching the supplied active blue list-toggle image. It starts closed on mobile so the navigation drawer does not cover the balance. The browser regression checks the correct initial toggle state for each viewport.

## Customer component/action audit

See [customer-component-action-audit.md](customer-component-action-audit.md) for current defects fixed, scope, and verified results. Current focused runs: 33 frontend tests, 10 desktop/mobile browser scenarios, and 19 backend checks passed. Earlier broad runs remain separate evidence.
