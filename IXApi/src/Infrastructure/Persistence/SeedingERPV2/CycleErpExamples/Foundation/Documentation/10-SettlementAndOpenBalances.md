# Settlement And Open Balances

The reused AP transaction services own vendor payments and settlement. Completed example reports expect zero remaining vendor open balance. Foundation does not implement another settlement algorithm or update balances directly. Amounts and references are read from persisted business-service results and the AP reconciliation checks.
