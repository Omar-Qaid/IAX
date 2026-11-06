# Impacted tables

## Preparation only

`CompanyInfo`, `Ledger`, `LedgerChartOfAccounts`, `MainAccount`, `DimensionHierarchy`,
`LedgerStructure`, `DimensionAttributeValueCombination`, `FiscalCalendar*`, `Currency`,
`ExchangeRate*`, `TaxTable`, `TaxData`, `TaxLedgerAccountGroup`, `TaxGroupHeading`, `TaxGroupData`,
`TaxOnItem`, `TaxItemGroupHeading`, `InventPosting`, `InventDim`, `InventSite`, `InventLocation`,
`InventTable`, `InventTableModule`, `BankAccountTable`, `LedgerJournalName`, `SysNumberSequence`,
`CustGroup`, `CustLedger`, `CustLedgerAccounts`, `CustParameters`, `SalesParameters`,
`CustPaymModeTable`, `CashDisc`, `ReturnReasonCode`, `ReturnDispositionCode`, `DirPartyTable`,
`DirPartyLocation`, `LogisticsLocation`, `LogisticsPostalAddress`, `LogisticsElectronicAddress`,
`CustTable`, `PriceDiscTable`.

## Transactions

| Area | Tables |
|---|---|
| Sales order | `SalesTable`, `SalesLine` |
| Inventory | `InventTransOrigin`, `InventTrans`, `InventSum` |
| Confirmation | `CustConfirmJour`, `CustConfirmTrans` |
| Delivery | `CustPackingSlipJour`, `CustPackingSlipTrans` |
| Free text invoice | `CustInvoiceTable`, `CustInvoiceLine` |
| Posted invoice | `CustInvoiceJour`, `CustInvoiceTrans` |
| Customer balance | `CustTrans`, `CustTransOpen` |
| Settlement | `CustSettlement` |
| Collection | `LedgerJournalTable`, `LedgerJournalTrans`, `BankAccountTrans` |
| Tax | `TaxTrans` |
| General ledger | `GeneralJournalEntry`, `GeneralJournalAccountEntry`, `SubledgerJournalEntry`, `AccountingDistribution`, `SourceDocumentHeader`, `SourceDocumentLine` |

## Tracing

Do not join on `InvoiceId` or `Voucher` alone. Follow
`SalesLine.InventTransId` to `InventTransOrigin` to `InventTrans`,
`CustTransOpen.RefRecId` to `CustTrans.RecId`, and
`CustSettlement.TransRecId` with its `SettlementVoucher`, always within the same company.
