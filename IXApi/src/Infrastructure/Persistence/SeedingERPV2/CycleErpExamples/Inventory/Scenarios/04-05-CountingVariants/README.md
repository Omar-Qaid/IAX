# Scenarios 04 and 05 — overage and matching count

Each scenario prepares an independent item and posts an opening adjustment of 90 units at 12. Both generate a
counting line from actual on-hand, freeze it, confirm the observed count, and post through the counting service.

- 04 / CPLUS: count 93. Expected variance +3, inventory increase 36, final inventory 93 / 1,116.
  Inventory profit receives the additional credit of 36. Including the opening adjustment, two movements and two
  vouchers must exist.
- 05 / CMATCH: count 90. Expected variance zero, final inventory 90 / 1,080. The count must be posted without
  a variance voucher or stock movement. Only the opening movement and voucher remain.

Replay checks preserve stock/voucher counts and voucher sequence progress; changed opening lines are rejected.
These cases target CNT-02. Only a passing test result in the acceptance register certifies them; SQL Server
concurrency is not covered by the isolated SQLite integration run.
