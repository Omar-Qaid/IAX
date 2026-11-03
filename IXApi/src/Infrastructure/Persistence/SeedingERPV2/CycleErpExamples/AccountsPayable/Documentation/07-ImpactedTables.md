# Impacted tables and mapping inventory

This is the reference-table inventory supplied in the requirements, not an exhaustive D365 schema or a verified map of the target ERP. Version, configuration and document path can change the tables used. Do not fabricate missing workflow/matching/prepayment table names. Inspect metadata and repository code to complete those paths.

| Area | Reference tables | Purpose |
|---|---|---|
| AP setup | `VendParameters`, `VendGroup`, `VendPosting`, `PaymTerm`, `VendPaymModeTable`, `CashDisc` | Parameters, groups, profiles and payment defaults |
| GL setup | `Ledger`, `Currency`, `ExchangeRateType`, `ExchangeRateCurrencyPair`, `ExchangeRate`, `MainAccount`, `DimensionAttributeValueCombination`, `LedgerJournalName` | Ledger, currency, accounts, combinations and journals |
| Tax setup | `TaxTable`, `TaxGroupHeading`, `TaxGroupData`, `TaxItemGroupHeading`, `TaxOnItem` | Codes and applicable group relationships |
| Inventory setup | `InventPosting`, `InventModelGroup`, `InventParameters` | Physical/financial posting configuration |
| Vendor identity | `VendTable`, `DirPartyTable`, `DirPartyLocation`, `LogisticsLocation`, `LogisticsPostalAddress`, `LogisticsElectronicAddress`, `VendBankAccount` | Vendor, party, address and bank relationships |
| Bank master | `BankAccountTable` | Company bank and ledger link |
| Products/storage/prices | `EcoResProduct`, `InventTable`, `InventTableModule`, `InventSite`, `InventLocation`, `InventDim`, `EcoResCategory`, `PriceDiscTable` | Products, purchasing defaults, storage and prices |
| Purchase order | `PurchTable`, `PurchLine`, `InventTransOrigin`, `InventTrans` | Order and expected inventory |
| Confirmation | `PurchConfirmJour`, `PurchConfirmTrans`, `PurchTable`, `PurchLine` | Confirmation snapshot and status |
| Receipt | `VendPackingSlipJour`, `VendPackingSlipTrans`, `PurchLine`, `InventTrans` | Received quantities and physical update |
| Pending invoice | `VendInvoiceInfoTable`, `VendInvoiceInfoLine`, `VendInvoiceInfoSubLine` | Pending document and receipt allocations |
| Posted PO invoice | `VendInvoiceJour`, `VendInvoiceTrans`, `VendTrans`, `VendTransOpen`, `TaxTrans`, `InventTrans` | Financial inventory, liability and recoverable tax |
| Direct invoice/payment journal | `LedgerJournalTable`, `LedgerJournalTrans`, `VendTrans`, `VendTransOpen` | Journal-specific posting path |
| Bank payment | `BankAccountTrans` | Bank movement when bank-offset path is used |
| Settlement | `VendSettlement`, `VendTransOpen`, `VendTrans` | Applications and remaining amounts |
| Charges | `MarkupTrans` | Charges for supported document paths |
| Accounting distribution | `AccountingDistribution` | Distribution in applicable source-document paths |
| Subledger accounting | `SubledgerJournalEntry`, `SubledgerJournalAccountEntry` | Applicable subledger journal path |
| GL and voucher bridge | `GeneralJournalEntry`, `GeneralJournalAccountEntry`, `SubledgerVoucherGeneralJournalEntry` | Ledger entries and voucher trace |

## Required repository mapping

For every actual affected entity document: module; physical table; primary/business key; company/tenant scope; foreign keys; source-document link; insert/update/read; quantity/status fields; transaction/accounting currency fields; posting date; voucher link; reversal link; responsible service; scenario validation.

Separate expected, physical and financial inventory updates. A PO invoice does not imply a new physical receipt. A journal invoice must not be required to create PO invoice-line rows. Settled vendor transactions remain historical records even when no open amount remains.

For each scenario capture actual document IDs and affected row IDs before/after execution. Trace GL through the supported voucher bridge plus company and date context, rather than assuming voucher text is globally unique.

Unknown mappings remain marked **unverified** until inspected. This document is a starting inventory, not permission to write directly into these tables.
