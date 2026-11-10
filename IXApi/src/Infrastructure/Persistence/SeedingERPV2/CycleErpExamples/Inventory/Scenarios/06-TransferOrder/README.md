# Scenario 06 — transfer with partial receipt

The module seeds an independent TRANS item and transfer order/journal sequences. The opt-in flow opens 90 units
at 12 through the adjustment service, then uses InventTransferOrderService for a transfer of 10 from APV2-WH
through APV2-TRN to APV2-WH2, all under APV2-SITE.

Shipment expects source/transit/destination quantities 80/10/0. Receipt of 6 expects 80/4/6. Final receipt of 4
expects 80/0/10. Each warehouse value must equal its quantity times 12; site quantity/value must remain 90/1080.
The final order has no remaining shipment or receipt quantity and exactly three transfer journals.

Stable transfer and posting-request identifiers permit replay. Existing header/line facts are validated, and
the integration test checks no duplicate movements, ledger vouchers or transfer journals on rerun. Acceptance
TRF-02 is recorded only after an actual passing isolated test. This does not cover inter-site financial transfer.
