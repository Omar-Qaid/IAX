# GL-01 reclassification and reversal

The shared setup supplies the company ledger, open January period, SAR currency, account structures, and maintenance and delivery expense dimensions. The GL package supplies journal name GLV2-GEN and its voucher sequence.

On January 15 of the selected fiscal year, journal `GLV2-{year}-01` debits maintenance expense (`681001`) by 125 SAR and credits delivery expense (`681003`) by 125 SAR. Reversal on January 16 restores both balances. The journal and line services create drafts, and the posting and reversal services create accounting.

Before posting, the journal is validated and then simulated. The simulation must report 125 debit and 125 credit and must leave no accounting entry and no posted flag behind.

Verification checks the source line against the requested accounts, amount, currency and date before posting or replay. Each posted voucher must have a company-scoped voucher association and two balanced accounting entries with the expected dimension and signed amount.

The scenario key is `ERPV2.GL.{year}.01`. A rerun finds the existing journal by name and the reversal by `OriginalJournalNum`, verifies both, and posts nothing again.

The SQL Server test runs the shared module setup twice and the GL example twice. Completed seeder IDs and table row counts must remain stable. The original and reversal journal numbers must remain the same on replay.

The fixture uses a generated database name. It retains the database for inspection.
