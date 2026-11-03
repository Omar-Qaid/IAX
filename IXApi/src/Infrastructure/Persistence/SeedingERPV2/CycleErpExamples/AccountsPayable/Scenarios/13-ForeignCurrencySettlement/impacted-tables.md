# Impacted tables

## Required paths

Exchange rates, invoice, vendor/open/settlement, bank, GL.

Use the reference inventory in [Impacted tables](../../Documentation/07-ImpactedTables.md). These are business areas; actual target-ERP table/field names must be filled after repository inspection.

## Mapping fields required per operation

Document actual table/entity; owner module; primary/business key; company/tenant fields; insert/update/read; source and target links; quantity and status fields; amount/currency fields; receipt/invoice allocation; voucher and reversal reference; responsible service and validation query.

Separate configuration reads, draft writes and generated posted writes. Tables specific to PO invoicing must not be imposed on a direct journal route. Record zero-effect stages as such. Do not manually insert into generated posting tables.
