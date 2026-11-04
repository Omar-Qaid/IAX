# Current scope: configuration and master data only

The implemented CycleErpExamples entry point prepares and validates the current GL, Tax,
Inventory, Cash/Bank and AP configuration/master fixtures, then returns an ordered preparation
report with actual persisted references. It reuses the phase-one seeders and their integrity gate.

Purchase orders, confirmations, receipts, pending/posted invoices, vendor balances, inventory
movements, payment/settlement, reversals and posted accounting are outside the current runtime.
The 20 transaction scenario folders are future reference documents only. No ISeedScenario is
registered and no business transaction service is called by this package.

Configuration/master writes belong to their owning module seeders. The generic seed framework
handles dependency ordering, company scope, the database guard, history and atomic commit.
CycleErpExamples validates its embedded manifests and captures its report before that commit.

Completion in this scope means the masters are stored and validated, reruns preserve their IDs
and row counts, and transaction tables remain empty in an isolated example database. It does not
mean a purchase-to-payment cycle has executed or that future posting functionality is verified.
