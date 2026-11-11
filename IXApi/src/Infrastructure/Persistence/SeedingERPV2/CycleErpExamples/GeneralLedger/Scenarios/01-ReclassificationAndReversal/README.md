# GL-01 reclassification and reversal

The shared setup supplies the company ledger, open January period, SAR currency, account structures, and maintenance and delivery expense dimensions. The GL package supplies journal name GLV2-GEN and its voucher sequence.

On January 15 of the selected fiscal year, the journal debits maintenance expense by 125 SAR and credits delivery expense by 125 SAR. Reversal on January 16 restores both balances. The journal and line services create drafts, and the posting and reversal services create accounting.

Verification checks the source line against the requested accounts, amount, currency and date before posting or replay. Each posted voucher must have a company-scoped voucher association and two balanced accounting entries with the expected dimension and signed amount.

The SQL Server test runs the shared module setup twice and the GL example twice. Completed seeder IDs and table row counts must remain stable. The original and reversal journal numbers must remain the same on replay.

The fixture uses a generated database name. It retains the database for inspection.
