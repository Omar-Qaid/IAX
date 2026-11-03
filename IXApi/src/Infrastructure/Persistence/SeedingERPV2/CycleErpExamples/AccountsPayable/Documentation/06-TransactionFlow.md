# Transaction flow and business-service boundaries

## Process

1. Create PO header and lines with resolved references and prices.
2. Confirm using the purchasing service.
3. Post the selected receipt quantities and dates.
4. Create pending invoice linked to eligible order/receipt quantities.
5. Match price, quantity, tax and tolerances; approve if the supported path requires it.
6. Post invoice through the invoice service.
7. Select open invoices and create payment journal lines.
8. Generate payment output only if supported and configured for testing.
9. Post payment and settle now or later as required by the scenario.
10. Validate source, subledger, inventory, tax, bank and GL results.

## Write boundaries

Configuration and master-data seeders may use approved persistence/services following the existing architecture. Transaction examples must call business services. Never directly insert generated InventTrans, VendTrans, VendTransOpen, TaxTrans or GL entries to make a scenario pass.

## States and quantities

Track ordered, cancelled, received, invoiced and returned quantities separately. Validate remaining receipt and invoice quantities using the application's own definitions. Partial processing can split inventory records; do not assert one physical row per PO line. Preserve receipt-to-invoice allocations and original-document links.

Pending invoice save, matching, approval, payment proposal and payment-file generation do not by themselves establish a posted ledger transaction.

## Transactions and failures

Each business posting should commit its document, inventory/subledger/tax/GL effects and operation completion marker atomically where architecture permits. Do not wrap an entire multi-document cycle in an unsafe long transaction. Resume from durable completed steps; use idempotency keys and uniqueness guards. If a service controls its own commit, record progress after reconciling the persisted operation and recover safely from a crash before checkpoint write.
