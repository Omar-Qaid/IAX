# Scope and implementation boundaries

This package is an implementation specification for basic Accounts Payable demo seeding. It is not executable code or evidence that an ERP implementation has passed testing. Actual entity names, enum values, service methods and supported corrections must be verified against the repository.

## Included

Vendor setup, stocked and service purchasing, purchase confirmation, receipts, pending invoices, invoice matching, invoice posting, direct expense invoices, payments, settlement, advances, discounts, foreign currency, returns and corrections. Include partial processing and reconciliation.

## Excluded

WHS, warehouse work, waves, production, manufacturing, advanced procurement, advanced workflow design and financial dimension derivation. Use existing basic dimensions where required by posting validation.

## Ownership

Accounts Payable owns its configuration, vendors and scenario orchestration. General Ledger owns accounts and ledger configuration. Inventory owns products, storage setup and inventory posting configuration. Tax and Bank own their respective setup. Purchasing owns purchase order business operations when implemented as a separate module.

## Completion

Deliver working seeders, business-service integration, documented table mappings, numerical expected results and an execution report. A missing business capability must be implemented in its owning module or explicitly reported as blocked. Never silently skip a failed scenario or claim full D365 equivalence from the table names alone.
