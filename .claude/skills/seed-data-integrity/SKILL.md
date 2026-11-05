---
name: seed-data-integrity
description: Review, implement, and repair seed-data integrity across modules. Use when creating or refactoring ERP seeders, fixing missing cross-module references, ordering seed dependencies, or verifying repeatable tenant/company-scoped seeding.
---

# Seed-data integrity

Review and enforce seed-data integrity across all modules.

## Review before changing

- Inspect the **actual** relationships in the entities and their configurations, not the shape the
  seeder assumes.
- Seed **dependencies first**, and reuse records from their owning modules instead of re-creating
  them locally. A module that needs a main account, a unit or a site takes the one its owning
  seeder created.
- Establish the dependency order explicitly (the seeder's `DependsOn`), and make sure the order
  the runner uses matches the order the data requires.

## Validate

- **Foreign keys** — every reference resolves to a record that exists by the time it is written.
- **Required fields** — no row is seeded with an empty value that a validator or posting service
  will later reject.
- **Uniqueness** — respect every unique index and natural key; never rely on an auto-assigned
  `RecId` colliding or not colliding by chance.
- **Tenant/company scope** — every row carries the right `DataAreaId`, and shared/global records
  are seeded once at the right scope rather than per company.
- **Seed data must not contradict a validator.** If a service would refuse to save the row the
  seeder writes, that is an integrity defect, not a quirk. Converge the offending field the way the
  existing repeatable seeders do, and leave the operator's other choices alone.

## Idempotency

- Prevent duplicates and make reruns idempotent: check for the existing row before adding, and for
  a repeatable seeder converge only the fields the code requires.
- Never let a rerun create a second copy, reset an operator's configuration, or change an existing
  record's business meaning (a costing model, a posted amount) as a side effect.

## Transaction data

- Create transaction data through the **appropriate business services**, never by inserting
  documents, inventory transactions or ledger entries directly. The service owns the statuses,
  quantities, vouchers and postings.
- Setup and master data may be seeded directly; anything that posts goes through its service.

## Finish the job

- Fix the integrity issues found.
- Add focused unit tests for the validation logic, not broad suites.
- Run the build and the relevant checks.
- Continue until complete, and report any remaining blockers concisely.
