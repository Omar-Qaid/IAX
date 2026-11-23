# Scenario 10 — FIFO close adjustment (catalog INV-21)

Run explicitly with `InventoryTransactionOptions(year, actor, ["10"])`. The module owns item `IV2-{year}-FIFO` in
the FIFO model group `APV2-FIFO`, so this close cannot change any weighted-average example.

| Step | Document | Quantity × cost | Value |
| --- | --- | --- | --- |
| Purchase 1 | `{key}-P1`, November 1 | 100 × 60 | 6,000 |
| Purchase 2 | `{key}-P2`, November 2 | 100 × 66 | 6,600 |
| Sale before close | `{key}-D1` / `{key}-I1`, November 6–7, 150 at 90 | 150 × running average 63 | COGS 9,450 |
| FIFO cost | 100 × 60 + 50 × 66 | | 9,300 |
| Close, November 30 | `InventCloseService.CloseItemAsync` | | Debit inventory 150, credit COGS 150 |
| Remaining | | 50 × 66 | 3,300 |

Purchases, the sales order, delivery, invoice and close all go through the owning services. The flow asserts the
pre-close issue cost of -9,450 and the closed issue cost of -9,300, then runs the shared reconciliation: 50 on
hand, value 3,300, COGS 9,300, close adjustment +150.

A completed close is the terminal state. A rerun reconciles it without posting. The test
`Fifo_close_adjusts_the_running_average_sale_to_fifo_cost_and_replays_without_new_postings` in
`InventorySeedIntegrityTests` checks the figures and that replay adds no movement or voucher.
