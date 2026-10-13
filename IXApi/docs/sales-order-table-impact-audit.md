# Sales Order table impact audit — 2026-10-13

Scope: `SalesTable` quick create, header edit, line add/edit/delete, copy from orders and confirmation journals, delivery addresses, discounts, charges, totals, confirmation, and cancellation. The table checks below use the local development SQL database and exclude `IsDeleted = 1` rows unless stated otherwise.

| Action | Writes | Key reads and relationship | Result |
| --- | --- | --- | --- |
| Quick create | `SalesTable`, number sequence state | `CustTable`, `DirPartyTable`, currency, payment and delivery setup, tax group, customer address/contact | Live create and header persistence passed. A supplied delivery address is now rejected if the customer has no party relationship to establish ownership. |
| Header edit | `SalesTable`; releases an unshared former delivery address and location | Customer/invoice account, site/warehouse, tax, payment and delivery setup | Live save and closed-order guard passed. Header address replacement passed live through the delivery-address API; the old postal address and location were confirmed soft-deleted in SQL. |
| Line add/edit | `SalesLine`, `InventDim`, `InventTransOrigin`, `InventTransOriginSalesLine`, `InventTrans`, `InventSum`, header sales amount | Item/module setup, UOM conversion, tax, site/warehouse, dimensions, price agreements | Missing origin link write fixed. Live new line and quantity conversion passed. |
| Line delete | Soft deletes the line, its open inventory transaction and origin, origin link, line `MarkupTrans`, and an unshared line-specific address/location; adjusts `InventSum` and header sales amount | Sales line must be completely open | Orphaned line charge cleanup fixed. Live charge total returned to zero after deletion. Unique line address cleanup and shared customer and header address retention passed live; postal and location row states were checked in SQL. |
| Copy from order/journal | Same writes as line add | `SalesLine` or `CustConfirmTrans`, item, UOM conversion, dimensions, source and destination currencies | Both sources and quantity factor tested. Reverse-sign copy is rejected until a return-order inventory flow exists. Cross-currency copy is now rejected until an explicit price conversion exists; a live USD-to-SAR attempt returned an error and left the destination with zero lines. |
| Delivery address | `LogisticsLocation`, `LogisticsPostalAddress`, and the selected `SalesTable` or `SalesLine` address reference; releases an unshared former address/location | Customer and existing order addresses; quotation, packing slip, invoice, confirmation, and primary party-location references protect shared or historical addresses | Header and line creation and replacement passed live. Replaced addresses were absent from the lookup and soft-deleted in SQL. |
| Recalculate discounts | `SalesLine` discount fields | `CustParameters`, `InventTableModule`, `PriceDiscTable` | Live recalculation passed. The local database has no price agreement rows, so agreement selection has no live-data coverage. |
| Charges | `MarkupTrans` | Charge code in `MarkupTable`; sales header/line, tax, totals and confirmation | Header and line charge create passed. Canceled-order writes are now rejected; historical reads remain available. |
| Confirm | `CustConfirmJour`, `CustConfirmTrans`, `SalesTable.DocumentStatus`, number sequence state | Customer, sales lines, charges, tax setup, item module | Live snapshot amount, tax and charge passed; journal relationships checked. |
| Cancel | `SalesTable`, `SalesLine`, open `InventTrans`, `InventSum` | Open demand by origin and dimension | Live cancellation and closed-order write guards passed. A missing or insufficient `InventSum` row now aborts cancellation before inventory transactions are closed. Existing confirmation snapshots are retained. |

## Live relationship checks

After applying `20261013110000_BackfillSalesLineOriginLinks`, the local database had 14 active sales lines with inventory transaction IDs and 14 active `InventTransOriginSalesLine` links. Missing or orphaned active links: **0**. Active lines without headers, origins, or inventory transactions: **0**. Duplicate active origin links and orphaned inventory transactions: **0**. Active confirmation lines without journals and journals without lines: **0**. Active line/header charges without their document: **0**. Sales line open demand versus inventory transaction demand, header gross amount versus active line amount, and `InventSum.OnOrder` versus active sales demand all had **0 mismatches** at the checked precision.

The backfill inserted links only where an active line, active order, and active origin matched by company, inventory transaction ID, and sales ID. It did not alter sales quantities or inventory amounts. The migration is repeatable because it skips active links that already exist.

Additional active-reference checks found **0** orders without customers or invoice accounts, lines without items or inventory dimensions, orders or lines with missing delivery addresses, postal addresses without locations, and lines referencing missing tax groups or item tax groups.

The latest local check still had **14** active sales lines with inventory transaction IDs and **0** missing links, orphan links, missing origin rows, missing order delivery addresses, or missing line delivery addresses. The labeled replacement test used order `SO-2026-000015`: postal addresses 7 and 9 were soft-deleted after replacement, address 10 was soft-deleted after deleting its line, and current header address 8 remained active. Each address's location had the corresponding state. The test order was canceled after verification.

The full `IXApi.Tests` assembly passed **236 tests**, with 2 existing skips, using an isolated output directory because a running API process had locked the normal Release output. The focused Sales Order and inventory filter passed 14 tests. These results were rerun after the cross-currency copy guard was added.

## Coverage limits

- The local database has 4 UOM conversion rows, 0 rows in the additional UOM support tables, 0 `PriceDiscTable` rows, and 0 active `MarkupAutoTable`/`MarkupAutoLine` rules. Conversion was exercised; price-agreement selection and automatic-charge application cannot be validated with this data. No automatic-charge application service is wired to Sales Order creation in the current code.
- The Sales Order relationships are logical application relationships. The audit checked active row consistency; it did not introduce SQL foreign keys or change existing entity mappings.
- These checks cover implemented API paths and the listed local data, not every possible tax, currency, return, warehouse, concurrency, or production-data combination.
