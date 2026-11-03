# Office supplies purchasing example

This is a realistic **fictional** example. Supplier names, premises, contacts and bank numbers
are demonstration data, rather than records of actual businesses or payment destinations.
The example's accounting currency is SAR. Stable APV2 identifiers are retained across reruns;
database-generated record IDs are resolved at execution time.

## Phase-one master data

| Record | Identifier in company `dat` | Example |
|---|---|---|
| Local supplier | APV2-dat-LOCAL | Riyadh Office Supplies and Services (Demo), SAR, NET30 |
| USD supplier | APV2-dat-USD | Gulf Office Equipment Trading (Demo), USD, NET30 |
| Stocked item | APV2-STOCK | A4 copier paper, 500-sheet ream; one EA is one ream |
| Service item | APV2-SVC | Printer preventive maintenance; one EA is one visit |
| Storage | APV2-SITE / APV2-WH / APV2-DIM | Riyadh distribution site and office-supplies warehouse |
| Company bank | APV2-BANK | SAR treasury operating account linked to bank asset 181001 |
| Payment method | APV2 | Bank payment, vendor disbursement journal APV2-PAY |
| Terms | APV2-N30 | Net 30 days |
| Optional early-payment setup | APV2-2 | 2 percent within 10 days; baseline payment below occurs after that window |
| Tax intersection | APV2 / APV2 / APV2-15 | Vendor group and item group share a configured 15-percent input-tax code |

Both suppliers are modeled as domestic suppliers in Riyadh; the second quotes in USD. USD
billing does not by itself represent an import scenario. Their primary address, party location,
email and default bank reference point to the same supplier's records. Address geography uses
Riyadh/Al Olaya/12211; the building numbers and premises are fictional. Emails use `.invalid`.
Bank numbers are synthetic numeric examples, with no IBAN, SWIFT or bank integration supplied.

The paper item uses FIFO, physical/financial posting and receipt accrual. The maintenance item
uses a nonstocked model group. Both have purchasing units, item-tax groups, primary supplier,
item-group and model-group assignments. The service product also has a category assignment.

## Supplier quotations

| Supplier | Item | Currency | Price per EA | Minimum quantity |
|---|---|---|---:|---:|
| Local | APV2-STOCK | SAR | 25.00 | 1 |
| Local | APV2-SVC | SAR | 350.00 | 1 |
| USD | APV2-STOCK | USD | 7.00 | 1 |

Quotes are effective from January 1 through December 31 of the requested fiscal year and
reference APV2-DIM. The configured illustrative USD-to-SAR rate is 3.75 with display factor One.
These are example quotations and rate data, not market-price lookups.

## Next-phase business exercise

Assume an invoice dated April 15, 2026 from the local supplier:

| Line | Quantity | Unit price SAR | Net SAR | Configured tax SAR | Gross SAR |
|---|---:|---:|---:|---:|---:|
| Paper reams | 100 | 25.00 | 2,500.00 | 375.00 | 2,875.00 |
| Maintenance visits | 2 | 350.00 | 700.00 | 105.00 | 805.00 |
| Total | | | **3,200.00** | **480.00** | **3,680.00** |

NET30 produces a May 15, 2026 due date. Paying on that date is outside the configured 10-day
cash-discount window. These amounts are calculated from the persisted supplier prices, tax rate
and payment terms in the integrity tests.

For phase two, the intended exercise is order, confirmation, receipt of 100 reams, supplier
invoice and payment/settlement of SAR 3,680. The invoice's net accounting roles are inventory
2,500, maintenance expense 700, recoverable input tax 480 and AP control credit 3,680. Receipt
interim/accrual entries and their reversals must come from the existing posting services.
These transaction stages have **not** been executed by phase-one seeding.

## Integrity gate

Before commit, the runner checks mapped string lengths and validates the example's selected
company, ledger/chart/account combinations through the existing account-structure validator,
open fiscal period/year, effective exchange rate, voucher sequences, bank/journal/method links,
supplier/party/address/bank relationships, warehouse/site links, item/model/category assignments,
quotation currencies and units, and tax intersection/posting references. A missing, inactive,
duplicate or inconsistent reference rejects and rolls back the phase, including seed history.

The prior generic fixture is not silently renamed or overwritten if already installed. Its
values will conflict with the new named example; use a fresh demo database or explicitly reconcile
the existing demo configuration first. Application data is never automatically deleted for this.

See [PHASE-ONE.md](PHASE-ONE.md) for invocation, table mappings and validation evidence.
