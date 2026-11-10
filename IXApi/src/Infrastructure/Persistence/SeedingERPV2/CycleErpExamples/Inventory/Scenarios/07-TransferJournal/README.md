# Scenario 07 — same-site transfer journal

Inventory owns the independent TJOUR fixture. An opening adjustment creates 90 units at cost 12. The journal
services create and post one transfer line for 10 from APV2-WH to APV2-WH2 under APV2-SITE.

Expected source quantity/value is 80/960; destination is 10/120; transit remains zero. Site total stays 90/1080.
The transfer creates two stock movements, with three including opening. Reconciliation checks the summaries,
movement origins, inventory value, zero COGS and balanced opening GL voucher. The service handles both legs
atomically; the runner never inserts balance or movement rows.

Stable journal descriptions allow replay; existing header, line, date, unit and both dimensions are validated.
The integration test checks no additional movements or ledger vouchers and preserves voucher sequence progress.
TRF-01 remains unverified until its actual passing result is recorded in the acceptance register.
