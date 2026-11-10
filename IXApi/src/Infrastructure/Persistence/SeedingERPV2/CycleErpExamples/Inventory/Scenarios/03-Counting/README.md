# Scenario 03 — counting shortage

Run explicitly with `InventoryTransactionOptions(year, actor, ["03"])`. Module-owned item `IV2-{year}-COUNT`
starts at zero. A posted adjustment creates the documented opening balance of 90 units at cost 12 (value 1,080).

The counting service generates one line from actual on-hand in APV2-WH. The posting service freezes the count,
captures 90 on hand, confirms 88 counted, and posts the difference of -2. Expected final inventory is 88 units,
value 1,056, with 24 debited to inventory loss and zero cost of goods sold. Both vouchers must balance.

Stable journal descriptions allow a partially completed run to resume. The runner validates existing item,
dimensions, date, unit, frozen on-hand, counted quantity and difference. A posted count is reconciled on replay
without creating another movement or consuming another voucher. The integration test verifies the final result,
replay counts, sequence preservation and rejection of a changed opening line.

This covers CNT-01 only after an actual passing run is recorded in the acceptance register. It does not certify
CNT-02 overage/no-difference cases or concurrent counting protection.
