# AP transaction examples

Register `AddAccountsPayableTransactionExamples()` in a host that supplies ApplicationDbContext, the current user and the selected company execution context. Resolve `AccountsPayableTransactionExamples` in a fresh scope and call:

```csharp
var report = await examples.RunAsync(new AccountsPayableTransactionOptions(2026, actorUserId), ct);
// Or select distinct catalog IDs:
var report = await examples.RunAsync(new AccountsPayableTransactionOptions(2026, actorUserId, ["01", "02", "03"]), ct);
```

This is an explicit write operation intended for a disposable demo database. Document services own their transactions. A failed later stage preserves earlier committed documents for diagnosis and resume. Business keys and document/request identifiers support reruns. The configuration-only PrepareAsync contract is unchanged.

## Executable catalog

| ID | Example |
|---|---|
|01|Stock 100 x SAR25 plus maintenance 2 x SAR350: SAR3200 net, SAR480 input VAT, SAR3680 payment|
|02|Receive60/invoice40, receive40/invoice60|
|03|Service PO, receipt and invoice without stock movements|
|04|Direct expense invoice journal SAR402.50 inclusive of15% VAT|
|05|One invoice matched across two receipts|
|06|Reject price mismatch and excess quantity before a valid invoice|
|07|SAR50 delivery expense charge plus input VAT on a stock purchase|
|08|SAR1000 partial payment, SAR1875 remains payable|
|09|On-account payment and separate marked settlement|
|11|SAR1000 payment-journal prepayment, apply advance, pay remaining invoice|
|12|2% cash discount within ten days, SAR2817.50 cash clears SAR2875 invoice|
|13|USD805 invoice at3.75 SAR/USD, settlement at3.80 with configured realized FX loss and USD bank|
|14|Return10 units from the original invoice, original tax reversal and credit, settle/pay remainder|
|15|Expense invoice402.50, financial credit115, payment287.50; no inventory|
|16|Receive60, cancel remaining40, invoice/pay received quantity|
|18|Reverse settlement; reopen invoice and payment|
|19|Reverse payment; reopen invoice and reverse bank movement|
|20|Reject invalid invoice price/quantity atomically, then post valid received quantity|

## Integrity and boundaries

After each scenario finishes posting, `AccountsPayableReconciliationSnapshot.LoadAsync` reads its posted state once and the ten checks in `Validation/` run over it, each owning the concern its filename states: expected open balances (`OpenTransactionsValidator`), the open-row/settled-remainder invariant (`VendorBalanceValidator`), vendor subledger to GL control account including realized exchange and utilized cash discount (`VendorToLedgerValidator`), signed accounting and reporting voucher balance (`LedgerBalanceValidator`), physical quantity (`InventoryQuantityValidator`), financial inventory value (`InventoryValueValidator`), receipt accrual reversal and cleared interim accounts (`UninvoicedReceiptValidator`), bank (`BankPostingValidator`), input tax (`TaxPostingValidator`), and cleared advances plus settlement references (`SettlementValidator`).

Main account numbers come from `GeneralLedgerSeedExample.MainAccounts`, the same definition `MainAccountSeeder` inserts. `LedgerAmount` refuses a code absent from that chart rather than returning zero, because a zero would let a renumbered chart satisfy the checks that expect a cleared account.

Reports carry bank movement in the vendor/bank transaction currency and tax in accounting currency SAR. USD scenario amounts must not be added to SAR bank movements without translation.

PO, pending invoice, receipt and posted invoice source references are persisted with distributions, accounting events and subledger/GL links in the posting transaction. Distributions copy the actual GL entries without creating duplicate GL vouchers. Source-document native lifecycle enum states are not implemented; optional numeric lifecycle fields remain unset. Application polymorphic table identifiers are IXApp-compatible identifiers, not D365 AOT table IDs.

PO prepayment invoices (ID10) and historical receipt correction (ID17) are not implemented. A payment-journal prepayment does not substitute for a PO prepayment invoice. Settlement reversal with cash discount or realized FX is explicitly refused because reversing its extra GL postings is not implemented. Workflow, payment export, revaluation, localization, WMS and manufacturing are outside this core demo.

Offline EF migrations add the source references and realized-exchange audit fields. Apply reviewed migrations separately before running against an existing schema; this implementation does not migrate or seed the application database automatically.

## Verification

`AccountsPayablePhaseOneSeedTests` contains the real-model SQLite FK-enabled transaction run and same-database rerun. The opt-in SQL Server equivalent uses only generated `IXApi_APV2_Cycle_<guid>` databases and removes them in finally. Set `IXAPI_AP_PHASE_ONE_SQLSERVER=(localdb)\MSSQLLocalDB` to run it. Verification results must be checked in the current test output; a catalog entry alone is not proof of success.
