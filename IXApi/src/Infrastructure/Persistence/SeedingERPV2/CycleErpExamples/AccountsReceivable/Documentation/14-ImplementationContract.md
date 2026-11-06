# Implementation contract

Prepare Accounts Receivable configuration and master data by reusing the existing module-owned
seeders and integrity validators, and execute the sales cycle only through the services that own each
document. CycleErpExamples owns the opt-in entry points, the embedded example descriptions and the
reports.

Keep JSON and module fixtures consistent, validate definitions before mutation, validate stored
references before commit, resolve RecIds at runtime by company and business key, and report the real
completed seeder order. Preserve existing data and fail on incompatible setup.

Do not insert a document, an inventory movement or a ledger entry directly, in either scope. Do not
add a scenario for behavior the module does not implement, and do not configure a parameter that does
not exist in order to make a scenario look complete. When a D365 behavior has no implementation here,
record it in the boundaries list instead.

Reuse what the purchase example already owns rather than seeding a parallel copy of it. New shared
setup belongs to its owning module seeder, not to this folder.
