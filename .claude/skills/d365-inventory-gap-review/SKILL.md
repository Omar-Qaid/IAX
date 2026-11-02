---
name: d365-inventory-gap-review
description: Review the IXApi Inventory module against verified standard BASIC D365 F&O behavior and produce a dependency-ordered gap analysis (required setup → master data → basic transactions → purchase/sales integration → required costing and closing → general ledger integration). Analysis only by default; implement only when explicitly requested, prerequisites first. Use for any "find the gaps", "review against D365", "what is missing in inventory", "next gap" request touching IXApi/src/Modules/Finance/Inventory, on-hand, costing, inventory close, or inventory posting.
---

# Basic D365 F&O inventory gap review of IXApi

Review the existing ERP and identify gaps against **verified standard BASIC** D365 F&O
functionality only.

Keep the scope minimal: the essential setup, master data, transactions and accounting needed to
complete ordinary business operations. **Do not expand scope simply because a feature exists in
D365 F&O — "standard" does not automatically mean "required."** Match standard business behavior
within this scope without unnecessarily copying D365's internal database structure.

Companion skill: `d365-parity` governs how anything found here gets built. The module keeps a living
ledger at `IXApi/src/Modules/Finance/Inventory/GAPS.md` — read it first, verify its rows against
source, and update it in the same change that alters behavior.

## Inventory scope — start here

**Required setup and master data:** basic inventory parameters and number sequences; units and
necessary conversions; item groups and item model groups; the basic inventory dimensions the
existing business needs; released items; sites and warehouses; required inventory posting accounts.

**Basic transactions:** opening inventory balances; receipts and issues; adjustments; transfers;
counting and quantity corrections; basic reservations; on-hand and available quantities; purchase
receipts and sales deliveries; physical and financial inventory updates; basic costing and
valuation; recalculation and closing **only where required by the selected costing method**; the
related General ledger postings.

Do not require additional costing methods or inventory dimensions unless explicitly requested.

Inventory only. Do **not** begin a full review of Sales orders, Purchase orders or General ledger
until asked; inspect related modules only when needed to understand an Inventory dependency.
Accounts receivable, Accounts payable, Cash and bank and basic tax are touched only as necessary to
complete these cycles — never reviewed or implemented as whole modules.

## Work order

1. Required setup and configuration
2. Master data
3. Basic transactions
4. Purchase and sales integration
5. Required costing and closing
6. General ledger integration

Resolve prerequisites before dependent transactions: if receipt posting depends on missing item
setup or posting accounts, identify those setup gaps first. **Never bypass missing setup with
hard-coded values or transaction workarounds.**

## Review method

Before recommending any change:

- Read the repository instructions and understand the existing architecture.
- Inspect relevant entities, relationships, configurations, **seed data**, services, validators,
  APIs and UI flows. Seed data that contradicts a validator is a real setup gap.
- Trace actual execution paths from setup through posting.
- Check transaction statuses, quantities, remaining quantities, document references and company
  scope.
- Trace inventory updates and accounting entries.
- Search for existing functionality before declaring it missing.
- Reuse existing components wherever they support the required standard behavior.

Distinguish **Implemented / Partially implemented / Missing / Not verified**, and distinguish
missing *configuration* from missing *code*. Never mark something missing merely because it was
absent from the first file inspected.

## Standard alignment

- Verify expected behavior using official Microsoft documentation, referenced for the specific
  behavior being recommended.
- Explain configuration dependencies. Flag uncertainty and version-dependent behavior instead of
  guessing; insufficient evidence → **Not verified**, with what would verify it. Never present an
  assumption as a confirmed requirement.
- Do not invent entities, fields, statuses, workflows, validations, business rules or posting logic.
- Recommend a new component only when a verified basic requirement cannot be supported by the
  existing design.
- Preserve the existing architecture and avoid unrelated refactoring.

## Gap analysis format

For each in-scope gap: process step; implementation status; existing behavior with `file:line`
references; verified expected D365 F&O behavior; the missing or inconsistent functionality; affected
components; prerequisites and dependencies; priority; minimum correction needed.

Use short practical examples where they clarify quantities, statuses, inventory effects or
accounting entries.

Priority order:

1. Missing setup and master data that block basic processing.
2. Incorrect quantities, inventory balances or accounting entries.
3. Missing steps needed to complete the basic cycle.
4. Other necessary basic validations.

**End by identifying the first prerequisite to fix and why it comes first.**

## Strict exclusions

Do not analyze, recommend or implement:

- **Financial dimension derivation, derived dimensions, derivation rules, or automatic population of
  dimensions through derivation.** Basic financial dimensions and account structures remain in
  scope; derivation is excluded.
- Advanced Warehouse management (WHS), warehouse work, waves, advanced picking.
- Production or manufacturing. Master planning. Transportation management. Budgeting.
- Advanced credit management. Advanced pricing or promotion engines. Advanced automation.
- Optional approval workflows. Intercompany processes. Additional optional modules or features.
- Reports, printed document layouts, dashboards, analytics.
- Broad architectural refactoring. Deep testing, exhaustive test matrices, performance benchmarking.

Existing out-of-scope functionality does **not** need to be removed. Leave it unchanged.

## Work phases

**Analysis (default).** Inspect and analyze only; provide a concise written gap analysis. Do not
modify code, schema, configuration or data. A written gap analysis is required even though ERP
report development is excluded. Do not ask for routine confirmation when the scope is already
clear; ask only when a material ambiguity cannot be resolved from the repository or verified
references.

**Implementation (only when explicitly requested).** Fix only authorized, verified basic gaps.
Start with prerequisites, then master data, then transactions and integrations. Make the **minimum**
necessary change. Perform focused builds and checks appropriate to the change. Explain what
changed, what was checked and any remaining uncertainty.
