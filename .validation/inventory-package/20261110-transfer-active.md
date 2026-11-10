# Transfer order verification in progress

No dotnet/testhost job was active before this run. Inspect package-20261110-transfer.log/.trx before launching a
duplicate. Scenario 06 uses a new TRANS item, opens 90 at cost 12, ships 10, receives 6 (4 remain in transit), then
receives 4. Assertions check exact warehouse quantity/value at each stage and conservation of 90 / 1080 at site.

Inventory now owns InventTransferTable and InventTransferJour sequences, which were absent from its seed package.
The existing transfer service consumes the journal sequence on each shipment/receipt. The opt-in package registers
that service. The integration theory covers final reconciliation, same-database replay and conflict preservation.
TRF-02 remains unverified until the actual outcome is read; other transfer cases are not claimed.
