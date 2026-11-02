---
name: d365-cycle-gap-analysis
description: Review this ERP against verified standard D365 F&O BASIC functionality and produce a prioritized written gap analysis across the four cycles - sales (order to cash), purchase (procure to pay), inventory, and general ledger - including complete transaction cycles with their inventory and accounting effects. Use when asked to review, audit, gap-analyse or check alignment of any Finance module against D365 F&O, or when asked what is missing, partial or not verified in a cycle. Analysis only: do not modify code until implementation is explicitly requested.
---

# D365 F&O basic cycle gap analysis

You are an expert in Microsoft Dynamics 365 Finance & Operations core ERP functionality, ERP
architecture, and ASP.NET Core.

Review the existing ERP and identify gaps against **verified standard D365 F&O basic ERP
functionality**. The goal is functional alignment with D365 F&O within the scope below, including
complete transaction cycles and their inventory and accounting effects.

## Scope

### Sales cycle (order to cash)
Required setup, customers, order headers and lines, pricing and discounts, basic taxes,
confirmation, inventory reservation, packing slip, invoicing, customer payments, and settlement.

Include partial deliveries, partial invoicing, remaining quantities, cancellations, basic sales
returns, credit notes, and applicable corrections or reversals.

### Purchase cycle (procure to pay)
Required setup, vendors, order headers and lines, pricing and discounts, basic taxes, confirmation,
product receipt, vendor invoicing, vendor payments, and settlement.

Include partial receipts, partial invoicing, remaining quantities, cancellations, basic purchase
returns, credit notes, and applicable corrections or reversals.

### Inventory
Released items, units and conversions, basic sites and warehouses, applicable inventory dimensions,
item groups, item model groups, posting setup, receipts, issues, transfers, adjustments, counting,
reservations, and on-hand calculations.

Review physical and financial updates, inventory transaction statuses, basic inventory valuation,
the supported costing method, recalculation, and inventory closing where applicable.

Use **basic** inventory management, without advanced Warehouse management processes.

### General ledger
Legal-entity ledger setup, chart of accounts, main accounts, financial dimensions, account
structures, fiscal calendars and periods, currencies and exchange rates, journal names, journals and
lines, validation, posting, vouchers, reversals, and year-end closing.

Review accounting integration from sales, purchasing, inventory, customer payments, and vendor
payments. Include posting profiles, balanced entries, ledger balances, and reconciliation between
source transactions, subledgers, and the general ledger.

### Supporting areas
Include Accounts receivable, Accounts payable, Cash and bank, and basic tax functionality **only
where necessary** to complete these four cycles. Do not expand them into separate full-module
projects.

## Scan before proposing anything

Before proposing changes, scan and understand the existing:

- Entities, tables, relationships, and configurations.
- APIs, services, UI flows, and integrations.
- Document statuses, transitions, and transaction relationships.
- Validations, number sequences, and applicable workflows.
- Inventory movements, quantity calculations, costing, and physical/financial updates.
- Posting logic, posting profiles, vouchers, reversals, payments, and settlement.

Trace each cycle from setup through completion. Include normal processing, partial processing,
returns, cancellations, and applicable corrections. Use practical examples to explain expected
quantities, statuses, inventory effects, and accounting entries.

## Report each gap as

- **Existing behavior**, with relevant code references.
- **Expected standard D365 F&O behavior**, with supporting Microsoft documentation.
- **Missing or inconsistent functionality.**
- **Affected existing components and dependencies.**
- **Priority and the minimum change needed** for alignment.

Clearly distinguish **implemented**, **partially implemented**, **missing**, and **not verified**.
Do not mark something missing merely because it was not found in the first file inspected.

## Exclusions

- Reports, dashboards, analytics, and report development are excluded at this stage. A written gap
  analysis is still required.
- Exclude advanced or optional functionality, including advanced WHS, production, manufacturing,
  master planning, transportation, budgeting, advanced credit management, and advanced automation.
- Include workflows only where necessary for the agreed basic cycle.
- Do not interpret "full cycle" as requiring every D365 F&O feature or configuration option.

## Rules

- Match standard business behavior within this scope; do not copy D365's internal database structure
  unnecessarily.
- Reuse existing components and preserve the existing architecture wherever they can support the
  verified requirements.
- **Do not invent** entities, business rules, validations, statuses, or posting logic. Justify any
  proposed new component using a verified standard requirement.
- **Flag uncertainties**, configuration dependencies, and version-dependent behavior instead of
  guessing.
- Perform only focused checks needed to understand and verify behavior; do not perform deep testing.

## Order of work

Start with **scope confirmation** and **code analysis**, then provide the **prioritized gap
analysis**.

**Do not modify code until implementation is explicitly requested.**

## Where findings go

Each Finance module keeps its own `GAPS.md`, indexed at `src/Modules/Finance/GAPS.md`. Record
findings there in the same change, following the conventions in that index: move closed gaps out of
*Open gaps*, record a deliberate restriction with the exact message the code raises, put
unverifiable D365 behavior under *Flagged uncertainties*, and keep `Last updated` current.
