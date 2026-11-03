# Accounts Payable seeding documentation

This package contains the complete proposed Markdown documentation set for AP seed implementation. It includes shared-module ownership, build-first/reference-later dependencies, examples and validation. It does not contain executable ERP implementation or verified repository table mappings.

## Start here

Read documents 01–14 in order, then implement the scenario folders. Build missing shared configuration/master data in its owning module before AP references it. Use actual business services for generated transactions.

## Documents

- [Documentation/01-Scope.md](Documentation/01-Scope.md)
- [Documentation/02-Dependencies.md](Documentation/02-Dependencies.md)
- [Documentation/03-Configuration.md](Documentation/03-Configuration.md)
- [Documentation/04-MasterData.md](Documentation/04-MasterData.md)
- [Documentation/05-PricesDiscountsAndTaxes.md](Documentation/05-PricesDiscountsAndTaxes.md)
- [Documentation/06-TransactionFlow.md](Documentation/06-TransactionFlow.md)
- [Documentation/07-ImpactedTables.md](Documentation/07-ImpactedTables.md)
- [Documentation/08-PhysicalInventoryEffects.md](Documentation/08-PhysicalInventoryEffects.md)
- [Documentation/09-FinancialPostingEffects.md](Documentation/09-FinancialPostingEffects.md)
- [Documentation/10-SettlementAndOpenBalances.md](Documentation/10-SettlementAndOpenBalances.md)
- [Documentation/11-CorrectionsAndReversals.md](Documentation/11-CorrectionsAndReversals.md)
- [Documentation/12-Reconciliation.md](Documentation/12-Reconciliation.md)
- [Documentation/13-ExecutionAndRerun.md](Documentation/13-ExecutionAndRerun.md)
- [Documentation/14-ImplementationPrompt.md](Documentation/14-ImplementationPrompt.md)
- [Reference/Original-Requirements.md](Reference/Original-Requirements.md)
- [Scenarios/01-PurchaseToPayment/README.md](Scenarios/01-PurchaseToPayment/README.md)
- [Scenarios/01-PurchaseToPayment/accounting-entries.md](Scenarios/01-PurchaseToPayment/accounting-entries.md)
- [Scenarios/01-PurchaseToPayment/expected-results.md](Scenarios/01-PurchaseToPayment/expected-results.md)
- [Scenarios/01-PurchaseToPayment/impacted-tables.md](Scenarios/01-PurchaseToPayment/impacted-tables.md)
- [Scenarios/02-PartialReceiptsAndInvoices/README.md](Scenarios/02-PartialReceiptsAndInvoices/README.md)
- [Scenarios/02-PartialReceiptsAndInvoices/accounting-entries.md](Scenarios/02-PartialReceiptsAndInvoices/accounting-entries.md)
- [Scenarios/02-PartialReceiptsAndInvoices/expected-results.md](Scenarios/02-PartialReceiptsAndInvoices/expected-results.md)
- [Scenarios/02-PartialReceiptsAndInvoices/impacted-tables.md](Scenarios/02-PartialReceiptsAndInvoices/impacted-tables.md)
- [Scenarios/03-ServicePurchaseOrder/README.md](Scenarios/03-ServicePurchaseOrder/README.md)
- [Scenarios/03-ServicePurchaseOrder/accounting-entries.md](Scenarios/03-ServicePurchaseOrder/accounting-entries.md)
- [Scenarios/03-ServicePurchaseOrder/expected-results.md](Scenarios/03-ServicePurchaseOrder/expected-results.md)
- [Scenarios/03-ServicePurchaseOrder/impacted-tables.md](Scenarios/03-ServicePurchaseOrder/impacted-tables.md)
- [Scenarios/04-DirectExpenseInvoice/README.md](Scenarios/04-DirectExpenseInvoice/README.md)
- [Scenarios/04-DirectExpenseInvoice/accounting-entries.md](Scenarios/04-DirectExpenseInvoice/accounting-entries.md)
- [Scenarios/04-DirectExpenseInvoice/expected-results.md](Scenarios/04-DirectExpenseInvoice/expected-results.md)
- [Scenarios/04-DirectExpenseInvoice/impacted-tables.md](Scenarios/04-DirectExpenseInvoice/impacted-tables.md)
- [Scenarios/05-InvoiceAcrossMultipleReceipts/README.md](Scenarios/05-InvoiceAcrossMultipleReceipts/README.md)
- [Scenarios/05-InvoiceAcrossMultipleReceipts/accounting-entries.md](Scenarios/05-InvoiceAcrossMultipleReceipts/accounting-entries.md)
- [Scenarios/05-InvoiceAcrossMultipleReceipts/expected-results.md](Scenarios/05-InvoiceAcrossMultipleReceipts/expected-results.md)
- [Scenarios/05-InvoiceAcrossMultipleReceipts/impacted-tables.md](Scenarios/05-InvoiceAcrossMultipleReceipts/impacted-tables.md)
- [Scenarios/06-PriceAndQuantityMatching/README.md](Scenarios/06-PriceAndQuantityMatching/README.md)
- [Scenarios/06-PriceAndQuantityMatching/accounting-entries.md](Scenarios/06-PriceAndQuantityMatching/accounting-entries.md)
- [Scenarios/06-PriceAndQuantityMatching/expected-results.md](Scenarios/06-PriceAndQuantityMatching/expected-results.md)
- [Scenarios/06-PriceAndQuantityMatching/impacted-tables.md](Scenarios/06-PriceAndQuantityMatching/impacted-tables.md)
- [Scenarios/07-DiscountsAndCharges/README.md](Scenarios/07-DiscountsAndCharges/README.md)
- [Scenarios/07-DiscountsAndCharges/accounting-entries.md](Scenarios/07-DiscountsAndCharges/accounting-entries.md)
- [Scenarios/07-DiscountsAndCharges/expected-results.md](Scenarios/07-DiscountsAndCharges/expected-results.md)
- [Scenarios/07-DiscountsAndCharges/impacted-tables.md](Scenarios/07-DiscountsAndCharges/impacted-tables.md)
- [Scenarios/08-PartialPayment/README.md](Scenarios/08-PartialPayment/README.md)
- [Scenarios/08-PartialPayment/accounting-entries.md](Scenarios/08-PartialPayment/accounting-entries.md)
- [Scenarios/08-PartialPayment/expected-results.md](Scenarios/08-PartialPayment/expected-results.md)
- [Scenarios/08-PartialPayment/impacted-tables.md](Scenarios/08-PartialPayment/impacted-tables.md)
- [Scenarios/09-UnsettledPayment/README.md](Scenarios/09-UnsettledPayment/README.md)
- [Scenarios/09-UnsettledPayment/accounting-entries.md](Scenarios/09-UnsettledPayment/accounting-entries.md)
- [Scenarios/09-UnsettledPayment/expected-results.md](Scenarios/09-UnsettledPayment/expected-results.md)
- [Scenarios/09-UnsettledPayment/impacted-tables.md](Scenarios/09-UnsettledPayment/impacted-tables.md)
- [Scenarios/10-PurchaseOrderPrepayment/README.md](Scenarios/10-PurchaseOrderPrepayment/README.md)
- [Scenarios/10-PurchaseOrderPrepayment/accounting-entries.md](Scenarios/10-PurchaseOrderPrepayment/accounting-entries.md)
- [Scenarios/10-PurchaseOrderPrepayment/expected-results.md](Scenarios/10-PurchaseOrderPrepayment/expected-results.md)
- [Scenarios/10-PurchaseOrderPrepayment/impacted-tables.md](Scenarios/10-PurchaseOrderPrepayment/impacted-tables.md)
- [Scenarios/11-PaymentJournalPrepayment/README.md](Scenarios/11-PaymentJournalPrepayment/README.md)
- [Scenarios/11-PaymentJournalPrepayment/accounting-entries.md](Scenarios/11-PaymentJournalPrepayment/accounting-entries.md)
- [Scenarios/11-PaymentJournalPrepayment/expected-results.md](Scenarios/11-PaymentJournalPrepayment/expected-results.md)
- [Scenarios/11-PaymentJournalPrepayment/impacted-tables.md](Scenarios/11-PaymentJournalPrepayment/impacted-tables.md)
- [Scenarios/12-CashDiscount/README.md](Scenarios/12-CashDiscount/README.md)
- [Scenarios/12-CashDiscount/accounting-entries.md](Scenarios/12-CashDiscount/accounting-entries.md)
- [Scenarios/12-CashDiscount/expected-results.md](Scenarios/12-CashDiscount/expected-results.md)
- [Scenarios/12-CashDiscount/impacted-tables.md](Scenarios/12-CashDiscount/impacted-tables.md)
- [Scenarios/13-ForeignCurrencySettlement/README.md](Scenarios/13-ForeignCurrencySettlement/README.md)
- [Scenarios/13-ForeignCurrencySettlement/accounting-entries.md](Scenarios/13-ForeignCurrencySettlement/accounting-entries.md)
- [Scenarios/13-ForeignCurrencySettlement/expected-results.md](Scenarios/13-ForeignCurrencySettlement/expected-results.md)
- [Scenarios/13-ForeignCurrencySettlement/impacted-tables.md](Scenarios/13-ForeignCurrencySettlement/impacted-tables.md)
- [Scenarios/14-PurchaseReturn/README.md](Scenarios/14-PurchaseReturn/README.md)
- [Scenarios/14-PurchaseReturn/accounting-entries.md](Scenarios/14-PurchaseReturn/accounting-entries.md)
- [Scenarios/14-PurchaseReturn/expected-results.md](Scenarios/14-PurchaseReturn/expected-results.md)
- [Scenarios/14-PurchaseReturn/impacted-tables.md](Scenarios/14-PurchaseReturn/impacted-tables.md)
- [Scenarios/15-FinancialCreditNote/README.md](Scenarios/15-FinancialCreditNote/README.md)
- [Scenarios/15-FinancialCreditNote/accounting-entries.md](Scenarios/15-FinancialCreditNote/accounting-entries.md)
- [Scenarios/15-FinancialCreditNote/expected-results.md](Scenarios/15-FinancialCreditNote/expected-results.md)
- [Scenarios/15-FinancialCreditNote/impacted-tables.md](Scenarios/15-FinancialCreditNote/impacted-tables.md)
- [Scenarios/16-CancelRemainingQuantity/README.md](Scenarios/16-CancelRemainingQuantity/README.md)
- [Scenarios/16-CancelRemainingQuantity/accounting-entries.md](Scenarios/16-CancelRemainingQuantity/accounting-entries.md)
- [Scenarios/16-CancelRemainingQuantity/expected-results.md](Scenarios/16-CancelRemainingQuantity/expected-results.md)
- [Scenarios/16-CancelRemainingQuantity/impacted-tables.md](Scenarios/16-CancelRemainingQuantity/impacted-tables.md)
- [Scenarios/17-ReceiptCorrection/README.md](Scenarios/17-ReceiptCorrection/README.md)
- [Scenarios/17-ReceiptCorrection/accounting-entries.md](Scenarios/17-ReceiptCorrection/accounting-entries.md)
- [Scenarios/17-ReceiptCorrection/expected-results.md](Scenarios/17-ReceiptCorrection/expected-results.md)
- [Scenarios/17-ReceiptCorrection/impacted-tables.md](Scenarios/17-ReceiptCorrection/impacted-tables.md)
- [Scenarios/18-SettlementReversal/README.md](Scenarios/18-SettlementReversal/README.md)
- [Scenarios/18-SettlementReversal/accounting-entries.md](Scenarios/18-SettlementReversal/accounting-entries.md)
- [Scenarios/18-SettlementReversal/expected-results.md](Scenarios/18-SettlementReversal/expected-results.md)
- [Scenarios/18-SettlementReversal/impacted-tables.md](Scenarios/18-SettlementReversal/impacted-tables.md)
- [Scenarios/19-PaymentReversal/README.md](Scenarios/19-PaymentReversal/README.md)
- [Scenarios/19-PaymentReversal/accounting-entries.md](Scenarios/19-PaymentReversal/accounting-entries.md)
- [Scenarios/19-PaymentReversal/expected-results.md](Scenarios/19-PaymentReversal/expected-results.md)
- [Scenarios/19-PaymentReversal/impacted-tables.md](Scenarios/19-PaymentReversal/impacted-tables.md)
- [Scenarios/20-ValidationFailures/README.md](Scenarios/20-ValidationFailures/README.md)
- [Scenarios/20-ValidationFailures/accounting-entries.md](Scenarios/20-ValidationFailures/accounting-entries.md)
- [Scenarios/20-ValidationFailures/expected-results.md](Scenarios/20-ValidationFailures/expected-results.md)
- [Scenarios/20-ValidationFailures/impacted-tables.md](Scenarios/20-ValidationFailures/impacted-tables.md)

## Scenario folder contents

Each scenario contains README.md, expected-results.md, impacted-tables.md and accounting-entries.md. All values are demo assumptions; adapt account roles, tax configuration, supported corrections and service names after source inspection.

## Delivery acceptance

Working seed execution must pass company-scoped reconciliations and a second-run duplicate check. Unsupported behavior is a blocker, not a successful skipped test. Original supplied reference is preserved under Reference.


ACCOUNTS PAYABLE — CLASSIFIED TABLE LIST
65 unique names extracted from the supplied document.
Classification only: names are preserved as supplied, without metadata validation.
Shared tables are listed once under their primary category.

1. CONFIGURATION — ACCOUNTS PAYABLE AND PAYMENTS (7)
VendParameters
VendGroup
VendPosting
PaymTerm
VendPaymModeTable
CashDisc
LedgerJournalName

2. CONFIGURATION — TAX (5)
TaxTable
TaxGroupHeading
TaxGroupData
TaxItemGroupHeading
TaxOnItem

3. CONFIGURATION / MASTER DATA — GENERAL LEDGER AND CURRENCY (7)
Ledger
Currency
ExchangeRateType
ExchangeRateCurrencyPair
ExchangeRate
MainAccount
DimensionAttributeValueCombination

4. CONFIGURATION — INVENTORY POSTING (3)
InventPosting
InventModelGroup
InventParameters

5. MASTER DATA — VENDORS, PARTIES AND CONTACTS (7)
VendTable
DirPartyTable
DirPartyLocation
LogisticsLocation
LogisticsPostalAddress
LogisticsElectronicAddress
VendBankAccount

6. MASTER DATA — COMPANY BANK ACCOUNTS (1)
BankAccountTable

7. MASTER / REFERENCE DATA — PRODUCTS, INVENTORY AND PRICES (8)
EcoResProduct
InventTable
InventTableModule
InventSite
InventLocation
InventDim
EcoResCategory
PriceDiscTable

8. TRANSACTIONS — PURCHASE ORDERS AND CONFIRMATION (4)
PurchTable
PurchLine
PurchConfirmJour
PurchConfirmTrans

9. TRANSACTIONS — PRODUCT RECEIPTS (2)
VendPackingSlipJour
VendPackingSlipTrans

10. TRANSACTIONS — INVENTORY PHYSICAL / FINANCIAL UPDATES (2)
InventTransOrigin
InventTrans

11. TRANSACTIONS — PENDING VENDOR INVOICES (3)
VendInvoiceInfoTable
VendInvoiceInfoLine
VendInvoiceInfoSubLine

12. TRANSACTIONS — POSTED VENDOR INVOICES (2)
VendInvoiceJour
VendInvoiceTrans

13. TRANSACTIONS — VENDOR BALANCES AND SETTLEMENT (3)
VendTrans
VendTransOpen
VendSettlement

14. TRANSACTIONS — INVOICE / PAYMENT JOURNALS AND BANK (3)
LedgerJournalTable
LedgerJournalTrans
BankAccountTrans

15. TRANSACTIONS — TAX AND CHARGES (2)
TaxTrans
MarkupTrans

16. ACCOUNTING — DISTRIBUTIONS, SUBLEDGER AND GENERAL LEDGER (6)
AccountingDistribution
SubledgerJournalEntry
SubledgerJournalAccountEntry
GeneralJournalEntry
GeneralJournalAccountEntry
SubledgerVoucherGeneralJournalEntry