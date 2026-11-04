# Accounts payable transaction storage

This feature maps AccountingDistribution, SubledgerJournalEntry, SubledgerJournalAccountEntry,
SourceDocumentHeader, SourceDocumentLine, AccountingEvent and
SubledgerJournalAccountEntryDistribution.
VendInvoiceInfoSubLine is mapped in AccountsPayable/Invoice. These are transaction storage
additions only. They are not registered as seeders and do not execute any purchase, invoice,
payment, settlement or journal operation. Existing posting services are unchanged.

The accounting and source-document tables use SharedEntity, matching their cross-company accounting purpose.
Future consumers must explicitly scope by AccountingLegalEntity, Ledger or VoucherDataAreaId;
there is no automatic company filter for shared tables. VendInvoiceInfoSubLine uses Entity and
inherits company filtering. Its LineRefRecId points to VendInvoiceInfoLine; ParmId identifies the
processing group. JournalRefTableId/JournalRefRecId remain a polymorphic journal reference, so
no receipt-specific foreign key or fabricated table ID is assigned.

Mapped relationships use restrictive delete behavior. Currency references use existing currency
business keys. Decimal amount/rate storage uses decimal(28,12). Only non-localized/basic fields
are included; catch-weight extensions are excluded. Enum values without an existing repository
enum retain numeric storage; no status lifecycle is invented.

SourceDocumentHeader, SourceDocumentLine, AccountingEvent and
SubledgerJournalAccountEntryDistribution now have shared mapped entities. Foreign keys connect
source lines and accounting events to their header; distributions to their header, line and
accounting/finalization events; subledger journals to their accounting event; and distribution
links to both distributions and subledger account entries. Self/parent references use restrictive
deletes. Required/nullable fields follow the referenced Microsoft metadata. SourceRelationType,
TypeEnumName/TypeEnumValue and lifecycle enums are stored without invented metadata IDs or status
meanings. StateContainer is stored as nullable binary data without inventing serialization.

The AP transaction phase now initializes source references for purchase orders, pending invoices, receipts, posted invoices and returns. PurchaseSourceAccounting captures distributions, accounting events and subledger links from the actual posted GL entries within the owning service transaction. It does not create a second GL voucher. Native source-document lifecycle processing and subledger preview are not implemented; numeric lifecycle/type projections without a verified repository enum remain unset.

Phase-one preparation still populates configuration and master data only. The separate opt-in transaction runner is documented in SeedingERPV2/CycleErpExamples/AccountsPayable/PHASE-TWO.md.

Microsoft field metadata:
- https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountingfoundation/transactionheader/sourcedocumentheader
- https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountingfoundation/transactionline/sourcedocumentline
- https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountingfoundation/transaction/accountingevent
- https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountingfoundation/transaction/subledgerjournalaccountentrydistribution
- https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountspayable/transactionline/vendinvoiceinfosubline
- https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountingfoundation/transaction/accountingdistribution
- https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountingfoundation/miscellaneous/subledgerjournalentry
- https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountingfoundation/worksheetline/subledgerjournalaccountentry

Apply reviewed source-document migrations before using an existing database. No application database is migrated or seeded automatically. Phase-one regression tests assert these tables remain empty; transaction tests verify their population, GL linkage and repeatability. Historical storage-only validation results predate the transaction integration and do not certify its current behavior.
