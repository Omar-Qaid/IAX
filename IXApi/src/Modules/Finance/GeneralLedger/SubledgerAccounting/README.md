# Accounts payable transaction storage

This feature maps AccountingDistribution, SubledgerJournalEntry and SubledgerJournalAccountEntry.
VendInvoiceInfoSubLine is mapped in AccountsPayable/Invoice. These are transaction storage
additions only. They are not registered as seeders and do not execute any purchase, invoice,
payment, settlement or journal operation. Existing posting services are unchanged.

The three accounting tables use SharedEntity, matching their cross-company accounting purpose.
Future consumers must explicitly scope by AccountingLegalEntity, Ledger or VoucherDataAreaId;
there is no automatic company filter for shared tables. VendInvoiceInfoSubLine uses Entity and
inherits company filtering. Its LineRefRecId points to VendInvoiceInfoLine; ParmId identifies the
processing group. JournalRefTableId/JournalRefRecId remain a polymorphic journal reference, so
no receipt-specific foreign key or fabricated table ID is assigned.

Mapped relationships use restrictive delete behavior. Currency references use existing currency
business keys. Decimal amount/rate storage uses decimal(28,12). Only non-localized/basic fields
are included; catch-weight extensions are excluded. Enum values without an existing repository
enum retain numeric storage; no status lifecycle is invented.

AccountingEvent, SourceDocumentHeader and SourceDocumentLine tables are not present in this
repository. Their standard reference fields remain nullable scalar fields with no FK until the
owning framework is implemented. The distribution-to-subledger link table
SubledgerJournalAccountEntryDistribution is also not implemented. Adding these four mappings
does not implement accounting distribution generation, subledger preview/transfer or a complete
D365 source-document accounting framework. Those require a separate transaction-phase change
through the shared posting engine. No CRUD endpoints or direct-insert posting APIs are added.

Microsoft field metadata:
- https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountspayable/transactionline/vendinvoiceinfosubline
- https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountingfoundation/transaction/accountingdistribution
- https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountingfoundation/miscellaneous/subledgerjournalentry
- https://learn.microsoft.com/en-us/common-data-model/schema/core/operationscommon/tables/finance/accountingfoundation/worksheetline/subledgerjournalaccountentry

Apply the generated AddAccountsPayableTransactionStorage migration before using this storage.
No application database is migrated as part of adding these files. Phase-one regression tests
assert these tables remain empty; separate storage tests round-trip rows and reject orphan links
on SQLite and on the opt-in disposable SQL Server current-model fixture.
