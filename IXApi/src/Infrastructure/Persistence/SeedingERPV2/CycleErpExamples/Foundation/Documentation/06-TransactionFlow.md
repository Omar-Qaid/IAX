# Transaction Flow

Register AddFoundationTransactionExamples and call FoundationTransactionExamples.RunAsync with fiscal year and actor. It prepares masters, invokes AP scenario 04 for the inclusive taxed vendor expense journal and scenario 07 for a stocked purchase with a delivery charge, then reconciles TaxJournalTrans to the source journal and TaxTrans. A FoundationTransactionReport carries scenario results and actual tax-journal references.
