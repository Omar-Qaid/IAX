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

Scrolling is part of the shared layout: the action pane, title and Lines/Header
tabs stay fixed; only the details below them scroll. The viewport constrains its
flex children, and wheel scrolling stays inside the details region. Pages should
provide their content without adding an outer page scrollbar or height workaround.
Free-text invoice uses this default directly, matching sales-order details.

Document typography also uses shared primitives with sales order:
`DocumentHeader` supplies the theme's h5 title, body2 subtitle/status and their
spacing; `DocumentFieldLabel` supplies plain caption and lookup label sizing.
Grouped fields use matching heading, row and column gaps. New pages should use
these defaults instead of adding their own title font size or label spacing.

Shell breadcrumbs are part of the pattern as well. A page can set
`breadcrumb={{ listPath: '/accounts-receivable/free-text-invoices', labelKey: 'freeTextInvoices.freeTextInvoice' }}`.
The shell builds the module/list ancestors from navigation metadata and adds the
document caption. Without configuration it infers the parent URL and uses the
document description as the caption, while preserving existing detailed route
metadata. Use `label` for an already-localized caption, or `breadcrumb={false}`
for an embedded page that should not own the shell trail. Registrations update
with the page and are removed on unmount. Free-text invoice uses this contract
for both existing and new records.

See [Master-detail page](../../../docs/patterns/MASTER_DETAILS_PAGE.md)
for the option table mapped from sales order. Tests:
`src/test/patterns/MasterDetailPage.test.tsx`.
