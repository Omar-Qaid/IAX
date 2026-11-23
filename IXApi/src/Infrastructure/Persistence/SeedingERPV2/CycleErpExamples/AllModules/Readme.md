# Full cycle across all modules

`AddAllModulesTransactionExamples()` registers the all-modules seed plus every module's transaction examples. `AllModulesTransactionExamples.RunAsync(fiscalYear, actor)` then runs one basic D365 cycle on that seed, in this order:

| # | Module | Cycle | Scenarios |
|---|---|---|---|
| 1 | Accounts payable | Procure to pay | AP 01–20 |
| 2 | Inventory | Movement, adjustment, counting, transfer order and transfer journal, close | Inventory 01–09 |
| 3 | Accounts receivable | Order to cash, including prepayment and overpayment | AR 01–20 |
| 4 | Cash and bank | Payments and collections through the bank | CB 01 |
| 5 | General ledger | Journal, then reversal | GL 01 |
| 6 | Tax | Input and output tax, reconciled to their sources | Tax 01–02 |
| 7 | Foundation | Taxed journal and charges | Foundation 05 |

Every stage posts through the business service that owns the document, so no ledger, subledger or inventory row is inserted directly. Each stage resolves what it would post before posting, so a rerun verifies and resumes.

## Checks after the cycle

- The trial balance of the company nets to zero.
- `CustTrans` equals the main accounts behind the customer posting profiles' summary accounts.
- `VendTrans` equals the main accounts behind the vendor posting profiles' summary accounts.
- `BankAccountTrans` equals the main accounts behind the bank accounts.

Any difference fails the run.

## Reference steps that are skipped

These steps of the reference full-cycle example are not run, rather than approximated:

- the opening capital journal: the seeded chart has no equity account yet;
- an expense journal with input VAT on a ledger line: the journal engine refuses ledger-line tax by name;
- bank statement reconciliation, customer aging and write-off: out of basic scope;
- period close: covered by the GL service tests, and skipped here so the example company stays postable.