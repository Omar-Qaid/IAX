# Scenario 08 — movement journal

The module owns an independent MOVE item. Journal services receive 10 at 10 and issue 2 at 10, selecting the seeded
maintenance expense ledger dimension explicitly on both movement lines. Expected final stock/value is 8/80.

Reconciliation requires offset entries -100 and +20 on that selected account, no adjustment profit/loss entries,
zero sales COGS, two stock movements and two balanced GL vouchers. Existing documents are checked for item, date,
unit, quantity, cost, dimension and selected offset before replay. The integration test checks repeat counts,
voucher sequence preservation and rejection of changed opening facts.

Only actual passing test evidence certifies MOV-01; see the acceptance register. The runner uses owning services
for stock and ledger effects and never writes movement or balance rows directly.
