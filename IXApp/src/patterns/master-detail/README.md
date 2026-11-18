# Master-detail pattern

D365 details page for a master record with lines, built from the sales order page
(same record pane, typography, FastTabs, header field groups, master-data fields
and lines area). Parts: `MasterDetailPage`, `MasterDetailTabs`,
`MasterDetailSection`, `MasterDetailFieldGroups` / `MasterDetailFieldGroup`,
`MasterDetailField`, `MasterDetailLines` (import from `@patterns/master-detail`).

The page owns data, selection, edit state and persistence; the pattern only lays
them out. See [Master-detail page](../../../docs/patterns/MASTER_DETAILS_PAGE.md)
for the option table mapped from sales order. Tests:
`src/test/patterns/MasterDetailPage.test.tsx`.
