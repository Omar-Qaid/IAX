# Reconciliation

FoundationCycleValidator checks embedded manifests and captures master reference reports inside the master transaction. FoundationIntegrityValidator checks company-scoped parents and logical customer/schedule/dimension/tax links. The transaction entry point checks TaxJournalTrans source IDs, date, currency, tax groups, amounts and operation dimension against the source journal and TaxTrans. Reused AP checks reconcile posted GL, vendor, inventory, bank and tax effects.
