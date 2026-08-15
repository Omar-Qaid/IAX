# Dashboard module

`pages/DashboardPage.tsx` is the current dashboard/landing page and `index.ts` exposes the module entry. It renders the implemented dashboard UI directly; there is no `src/patterns/dashboard` subsystem or dashboard API/store in this module.

Add business data through a module-owned API/query layer before presenting it as live data. Reusable dashboard composition should be introduced only after more than one real consumer exists.

[Modules](../README.md) · [Dashboard pattern status](../../../docs/patterns/DASHBOARD_PAGE.md)
