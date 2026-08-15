# Master-details grid status

There is no `src/patterns/master-detail-grid` folder. The older proposed dual-grid pattern is not implemented.

For an existing parent/child display, use an implemented pattern only if its contract fits—for example `ListDetailsPage` with `TabularDetailPanel`—or build a module-owned composition. Do not import `MasterDetailGridPage` unless a real source implementation and test suite are added.
