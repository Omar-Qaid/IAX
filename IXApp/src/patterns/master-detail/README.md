# Master-detail pattern

D365 details page for a master record with lines, built from the sales order page
(same record pane, typography, FastTabs, header field groups, master-data fields
and lines area). Parts: `MasterDetailPage`, `MasterDetailTabs`,
`MasterDetailSection`, `MasterDetailFieldGroups` / `MasterDetailFieldGroup`,
`MasterDetailField`, `MasterDetailLines` (import from `@patterns/master-detail`).

The page owns data, selection, edit state and persistence; the pattern only lays
them out. `MasterDetailLinesGrid` follows the page's `editing` state. Its `busy`
flag locks the editors without changing mode. A failed Save retains edit mode
and the line draft; successful Save or Cancel clears the row editor when the
page returns to View. A custom line editor must follow the same rule and flush
its active line before the page Save completes. A draft's existence alone must
never enable editing in View mode.

See [Master-detail page](../../../docs/patterns/MASTER_DETAILS_PAGE.md)
for the option table mapped from sales order. Tests:
`src/test/patterns/MasterDetailPage.test.tsx`.
