# Active regression handoff

Started after the 8-test adjustment pass. Exec session: 91366.

The PowerShell job runs IXApi.Tests first, then InventoryCycle.Tests sequentially. Build output is isolated in `.validation/inventory-package/bin/`; no application database is seeded.

Inspect before launching another run:

- `package-20261110-full.log` / `.trx`
- `package-20261110-cycle.log` / `.trx`
- `package-20261110-regression-exit.json` (written only after both commands finish)

Final source includes the three corrected stage case labels mentioned in `20261110-adjustment-outcome.md`. Do not treat the focused result as proof of full regression or full acceptance. Preserve any failure logs, reproduce in-scope failures, and repair before certification. The recurring automation must remain active.
