# D365 F&O alignment review — initial findings

Scope confirmed by the user: backend services, API, database and example data. IXApp screens are excluded. No new proprietary business tables, fields or processing rules are authorized as substitutes for standard D365 behavior.

General ledger reference: `src/Modules/Finance/D365_FO_Core_General_Ledger_Full_Cycle.md`. Inventory, purchasing and sales retain their respective cycle documents. The GL document explicitly excludes inventory costing from its own core lab and distinguishes ledger-only examples from bank/customer/vendor subledger activity.

## Evidence and limits

The GL document requires target-build AOT verification. The target D365 version has been requested and is not yet supplied. Public Microsoft CDM metadata is useful evidence, not certification of the complete current application schema. Previously passing 81 tests demonstrated the implemented behavior and consistency checks; they did not certify D365 compatibility.

## Findings

| Area | Repository evidence | Assessment and required follow-up |
|---|---|---|
| Payment journal account identity | `VendPaymentService` writes the vendor summary ledger combination to `LedgerJournalTrans.LedgerDimension`, and bank ledger combination to its offset. Customer payment follows the same pattern. | Conflicts with GL document section 6.2: non-ledger account types require the corresponding dynamic account identity. Preserve posted GL amounts; inspect the actual dynamic-account model and target metadata before migrating journal references. Renaming fields alone will not fix this. |
| Transfer orders | Applied migration creates `InventTransferOrder` with item/quantity on the header and `InventTransferPosting`. | This is a custom simplification. Microsoft's published header is `InventTransferTable` and references `InventLocationIdFrom`, `InventLocationIdTo`, `InventLocationIdTransit`; transfer lines are represented by `InventTransferLine`. Requires a field/relationship mapping and data-preserving conversion, including existing transfer transaction origins. |
| Purchase receipt accrual | Added `PurchReceiptAccrual`, `ReceiptAccrualRecId`, `AccrualReversalAmountMst`. | Custom snapshot design; standard equivalence has not been established. Do not present it as a D365 table or expand fixtures on this design until the standard posting/link model is verified. |
| Vendor settlement | `VendSettlement` uses `InvoiceTransRecId`, `PaymentTransRecId`, `AmountCur`, `AmountMst`, `Voucher`. | A standard table name alone is insufficient evidence of field or settlement-direction compatibility. Verify fields, paired settlement behavior, amounts, company relationships and reversal handling against target metadata before conversion. |
| Counting and FIFO | Added `CountingFrozen`, `CountingConfirmed`, `ScopeItemId`, `ReceiptTransRecId`. | These additions require explicit standard-field and processing verification; none is certified by the existing functional tests. Preserve current count, close and settlement evidence during review. |
| GL journal framework | GL document sections 5, 6 and 13 distinguish journal work data, validation/approval and posted accounting. Existing custom services directly construct posted entries. | IXApi is a separate .NET application, not the D365 X++ runtime. Do not claim it invokes standard D365 posting. Audit its validation/defaulting/approval and posting behavior against the documented framework; if actual D365 integration is intended, use supported journal interfaces and posting paths instead of direct table writes. |

## Migration constraints

### Journal setup diagnostic

Added company-scoped, read-only `GET /api/v1/LedgerJournalTable/{recId}/setup-check`. It reports missing/ambiguous company ledger and journal name, mismatched journal type, posted/system-blocked journals, missing accounting currency, calendar-date coverage, ledger-specific open-period status, and ledger combinations whose main account is missing or belongs to another chart. It does not mistake a vendor/customer/bank identity for a ledger combination.

The result separates errors from unverified requirements and always returns `PostingValidated = false`. Dimensions/account overrides, workflow/security/module access, tax/reversal/layer/rounding, exchange conversions and intercompany behavior are not certified. No tables, fields, migrations or posting writes were added. This is further diagnostic coverage, not completion of the GL blueprint.

## GeneralLedger focused inspection

User selected `GeneralLedger` as the immediate focus. Static inspection found:

- `JournalEntry/Controllers/GeneralJournalAccountEntryController.cs` inherits the generic controller without disabling mutations. Its service inherits generic CRUD with no posting-specific override. This surface must be reviewed for protection of posted entries; it is not the controlled journal posting path required by GL document sections 5 and 13.
- `LedgerJournalTable/Services/LedgerJournalTableService.cs` currently delegates to generic CRUD. The journal controller folders examined expose no explicit validate, simulate or post actions. This is a gap in the inspected surface, not proof that no posting implementation exists elsewhere in the repository.
- `GeneralJournalAccountEntry.cs` contains the document's core account, amount, currency and correction properties. Its navigation relationships are commented out. Inspect the complete EF model/configuration before concluding that foreign keys are absent. Field names alone do not establish correct types, enums, lengths or company/ledger ownership.

Next implementation boundary: protect posted accounting API writes and establish the standard journal validation/posting lifecycle using verified existing fields. Do not introduce custom state columns or assume target-version metadata. Existing posted data must remain intact.

### Posted accounting API protection

Implemented the first boundary: `GeneralJournalEntryController`, `GeneralJournalAccountEntryController` and `SubledgerVoucherGeneralJournalEntryController` disable inherited create/update/delete actions with the repository's `NonAction` convention. Their mutation overrides, including the already non-action bulk methods, reject direct invocation. Read actions remain available. This changes API availability; clients must not use generic CRUD to edit posted accounting. It adds no entities, fields, migrations or database writes.

On 2026-10-29, the corresponding three generic services were also made read-only through `PostedLedgerReadService<T>`. Single/bulk add, update and remove, plus update-with-details, reject before storage access. Read behavior and DI registrations remain unchanged. Existing document posting services use their own transaction/DbContext paths and are not routed through these generic services; those paths remain functional. This is an application service boundary, not a database-level prohibition against all writes.

`PostedLedgerApiTests` checks actual MVC action discovery for all three controllers, including retained reads and absence of all six mutation methods. It also checks all seven generic mutation methods through service interfaces for all three posted entity types. The standard journal validation/approval/posting lifecycle remains outstanding.

### Voucher arithmetic diagnostic

Added read-only `GET /api/v1/LedgerJournalTable/{recId}/voucher-balance-check`, restricted to the current company. It checks the journal's persisted lines separately by company, voucher, accounting date and transaction currency, treating explicit offsets as arithmetically balancing their line. It checks empty journals, missing voucher/date/currency, and empty or two-sided amount lines. Signed correction amounts are not automatically rejected. No schema or posted data changes are involved.

This diagnostic is deliberately not a Validate/Simulate/Post implementation: mixed-currency accounting conversion, offset defaulting, dynamic account resolution, account structures, period access, approval, posting layer, rounding and reversal policies require further verification. Its response explicitly describes that limited scope. Four arithmetic tests cover voucher isolation, date/currency isolation, offsets versus explicit balancing lines, signed amounts and empty/two-sided lines.

Existing applied migrations and current posted rows must not be deleted or silently rewritten. First establish a reviewed mapping of source fields, standard targets, keys, company ownership, units, enums and amounts. Then prepare forward migrations and reconciliation checks. Unverified replacement fields would reproduce the same defect under different names.

No runtime or schema change was made during this initial review. This is not a complete all-table audit.

## Microsoft metadata consulted

- [InventTransferTable](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/inventory/worksheetheader/inventtransfertable)
- [Transfer transit origin relationship to InventTransferLine](https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/supplychain/inventory/transaction/inventtransorigintransfertransitto)
