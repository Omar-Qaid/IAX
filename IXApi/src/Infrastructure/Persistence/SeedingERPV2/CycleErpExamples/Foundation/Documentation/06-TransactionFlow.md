# Transaction Flow

Register AddFoundationTransactionExamples and call FoundationTransactionExamples.RunAsync with fiscal year and actor. It prepares masters, invokes AP scenario 04 for the inclusive taxed vendor expense journal and scenario 07 for a stocked purchase with a delivery charge, then reconciles TaxJournalTrans to the source journal and TaxTrans. A FoundationTransactionReport carries scenario results and actual tax-journal references.

After reconciliation, TaxTransGeneralJournalAccountEntrySeeder links the AP-04 tax fixture to its uniquely matched tax ledger entry. It checks company, voucher/date, posting type, configured input-tax dimension, main account, currency and both amounts before inserting associations in a serializable transaction. Reruns reuse matching IDs; conflicts and ambiguous matches fail. The report exposes the persisted IDs in TaxAccountingAssociationIds. No unrelated historical documents are backfilled by this example.
