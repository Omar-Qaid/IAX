# Impacted Tables

FoundationTableCatalog and Data/tables.json are the executable inventory. FoundationCycleValidator requires every master type to be mapped and populated and captures its physical table name, row count and record IDs before commit. FoundationIntegrityValidator checks mapped foreign keys and key logical references. See Coverage.md for ownership. Transaction services also write AP, inventory, bank and GL tables; those writes are business operations, not extra master seeders.
