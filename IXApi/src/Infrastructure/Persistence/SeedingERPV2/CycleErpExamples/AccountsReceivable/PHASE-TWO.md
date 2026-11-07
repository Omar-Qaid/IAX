# Phase two: opt-in sales transactions

`AccountsReceivableTransactionExamples.RunAsync` executes the sales cycle through the owning business services. It is registered separately from preparation and never runs at startup.

```csharp
services.AddAccountsReceivableTransactionExamples();

using var scope = serviceProvider.CreateScope();
var examples = scope.ServiceProvider.GetRequiredService<AccountsReceivableTransactionExamples>();
var report = await examples.RunAsync(new AccountsReceivableTransactionOptions(2026, actorUserId), ct);
```

`ScenarioIds` selects a subset; omitting it runs every implemented scenario in ID order. The run prepares masters first, takes the shared database seeder lock, and records each finished scenario in the existing seed history under `ERPV2.AR.{year}.{id}`.

Because a sale needs stock on hand, this registration also brings in the purchase transaction examples. Run them for the same fiscal year before, or in the same process as, the sales scenarios so the demo item has been received and financially updated.

The host must register the normal Finance Mapster mappings before calling services (as application startup does). `AccountsReceivableCycleStageTests` scans the Finance assembly and verifies the complete cycle against a disposable SQL Server database. Preparation includes the `CustSettlement` number sequence needed by collections, marked settlement and their reversals.

Document request IDs are idempotency references, not invoice or packing-slip numbers. The examples resolve posted documents by `ParmId`; free-text invoices are located by their stable description and use the invoice number allocated by their service. Returns resolve the original invoiced inventory transaction and retain the return number allocated by the return service.

Non-stocked service lines are invoiced directly without a packing slip, inventory movement or cost-of-goods entry. Stocked lines still require delivered, uninvoiced quantity. The mixed scenario ships only the stocked line and invoices both lines.

`CustPaymentPostDto.SettleAutomatically` defaults to `true`, preserving oldest-invoice/selected-invoice settlement for existing callers. Scenario 09 and multi-invoice examples explicitly use `false`, then mark only their own open transactions for separate settlement. A collection's `BankAccountTrans.AmountCur` stays in bank currency; `AmountMst` and `AmountReportingCurrency` use the actual accounting/reporting conversion.

The customer return disposition fixture is `ARV2-RET`, within the mapped sales-line field length. Scenario 12 applies `ARV2-2` through the sales-header service before invoicing; simply seeding a discount code does not select it on an order.

The inventory owner also prepares `InventClosing` and `SettleTransId` sequences, preserving consumed counters. The separate `OrderToCashAcceptanceTests` applies the user's exact 100-at-60 / 10-at-100 plus 200-service example on SQL Server, including physical delivery posting, two 690 invoices, manual 700/680 collections and weighted-average close after 100-at-80 receipt. Its second disposable database verifies corrections independently. These acceptance policies are test-local; they do not change the shared FIFO demo policy or run at startup. See `docs/testing/OrderToCashAcceptance.md` and the 77-case acceptance plan for implemented, unexecuted and missing coverage.

## Implemented scenarios

| ID | Scenario |
|---|---|
| 01 | Stock and service sale to collection |
| 02 | Partial deliveries and invoices |
| 03 | Non-stocked service sales order |
| 04 | Free text invoice |
| 05 | Invoice across multiple deliveries |
| 08 | Partial collection |
| 09 | On-account collection and later settlement |
| 12 | Cash discount |
| 13 | Foreign-currency sale and realized exchange difference |
| 14 | Sales return and customer credit |
| 16 | Cancel undelivered remainder |
| 18 | Settlement reversal |
| 19 | Payment reversal |
| 20 | Validation failures and rollback |

The IDs are kept parallel to the purchase example so the same number means the same shape of cycle on both sides. The gaps are the purchase-only ones: there is no sales equivalent of three-way matching, of a product receipt accrual correction, or of a purchase-order prepayment in this example.

## Boundaries

Not implemented here: customer prepayment through a payment journal, interest notes, collection letters, write-offs, aging, foreign-currency revaluation of open balances, credit-limit blocking, sales quotations, delivery charges on the sales side, and the credit-only and scrap return dispositions. Each one needs a service or a parameter that this example does not configure, and inventing either would make the example describe behavior the system does not have.

## What every scenario asserts

After a scenario finishes posting, `AccountsReceivableReconciliationSnapshot` loads the persisted rows once and all eleven checks run against them. Nothing in the checks recomputes an amount a posting service already decided; they compare what was stored.

| Check | Concern |
|---|---|
| OpenTransactionsValidator | The scenario leaves exactly the open debit and credit it set out to leave |
| CustomerBalanceValidator | Each open row equals its own transaction unsettled remainder |
| CustomerToLedgerValidator | The customer subledger ties to the GL control account |
| LedgerBalanceValidator | Every voucher balances in the accounting and the reporting currency |
| InventoryQuantityValidator | The physically issued quantity, net of anything taken back |
| InventoryValueValidator | Financially updated inventory ties to the inventory account |
| UndeliveredRevenueValidator | The two delivery interim accounts return to zero once invoiced |
| BankPostingValidator | Bank movements tie to the bank account in the accounting currency |
| TaxPostingValidator | Posted output tax ties to the payable sales tax account |
| SettlementValidator | No prepayment is stranded, and every settlement row names what it settled |
| RevenueRecognitionValidator | Revenue equals the invoiced line amounts, so it is recognized by the invoice and not by the delivery |

Sales issues are negative inventory movements, so a delivered order reports a negative quantity and a received return nets it back up. A scenario that cancels its undelivered remainder expects only what was actually delivered.

## Reruns

Each step resolves what already exists before posting again: an existing order, confirmation, packing slip, invoice, return, credit note, collection or reversal is verified and reused. A rerun therefore resumes rather than duplicating, and never reposts completed history. A rejected operation is checked against a fingerprint of every posting table, so a refusal is proven to have written nothing rather than assumed to have.
