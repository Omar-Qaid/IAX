# Foundation transaction examples

Register `AddFoundationTransactionExamples()` and resolve `FoundationTransactionExamples` in a fresh scope. Call `RunAsync(new FoundationExampleOptions(2026, actor), ct)`.

Preparation runs first. The example reuses AP scenario 04 (inclusive taxed expense journal) and 07 (stock purchase with an expense charge), then returns a `FoundationTransactionReport` with the actual tax-journal references and reused posted scenario results. Its scope is `FoundationTransactions`.

TaxTrans, TaxJournalTrans and MarkupTrans come from business services. TaxJournalTrans retains its journal-line/source-table keys and calculated amounts in the posting transaction. No standalone fake transaction row is inserted by a seeder.

Master preparation is atomic; document operations retain their own commit boundaries. Earlier committed documents survive a later scenario failure for diagnosis and rerun. Posted journals lacking tax details are rejected; historical backfill and manual tax adjustment are not implemented by this package.

See Scenarios/05-TaxJournalAndCharges and Documentation/09-FinancialPostingEffects.md for numeric expectations. SQL Server opt-in tests and provider limitations must be reported from current test output. No application database is automatically migrated or seeded.
