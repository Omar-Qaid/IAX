# Scope: prepared masters, and the sales cycle on top of them

`AccountsReceivableCycleSeeder.PrepareAsync` prepares and validates the GL, Tax, Inventory,
Cash/Bank, AP and AR configuration and master fixtures, then returns an ordered preparation report
with the actual persisted references. It reuses the phase-one seeders and their integrity gate.

`AccountsReceivableTransactionExamples.RunAsync` is the separate, opt-in entry point that executes
sales orders, confirmations, deliveries, invoices, free text invoices, collections, settlements,
returns, cancellations and reversals through the owning business services. Nothing in this package
writes a document, an inventory movement or a ledger entry directly.

Configuration and master writes belong to their owning module seeders. The generic seed framework
handles dependency ordering, company scope, the database guard, history and atomic commit.
CycleErpExamples validates its embedded manifests and captures its report before that commit.

Completion in the preparation scope means the masters are stored and validated, reruns preserve
their IDs and row counts, and no transaction row is written. Completion in the transaction scope
means every selected scenario posted and all eleven reconciliation checks passed against the
persisted rows.

Out of scope in both: anything the module does not implement. See the boundaries list in
[phase two](../PHASE-TWO.md) rather than assuming a D365 feature is covered because it exists in
D365.
