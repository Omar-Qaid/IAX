# Master-detail page

`src/patterns/master-detail` is the D365 details page for a **master record with
lines** (sales order, purchase order, transfer order, journals). It is built from
the sales order page and uses the same parts, sizes and fonts (`d365` tokens), so
new pages look and behave like sales order without copying its code.

```tsx
import {
  MasterDetailPage, MasterDetailTabs, MasterDetailSection,
  MasterDetailFieldGroups, MasterDetailFieldGroup, MasterDetailField, MasterDetailLines,
} from '@patterns/master-detail';
```

## Which pattern to use

Any page with a master record and lines uses `master-detail`. It has the full
enterprise feature set, composed from the same shared parts as `list-details`
(so both stay in step):

| Feature | Option |
| --- | --- |
| Advanced filter pane (right rail → Filter) | `advancedFilter={{ fields: [{ id, label, getValue }] }}` |
| Related information side panel (FactBox) | `relatedInformation={{ title, sections }}` |
| Record info / audit (Options menu) | `recordTableName`, `getAuditRecordId`, `showOptionsMenu` |
| Attachments | `attachments={{ refTableId, refRecId }}` |
| Utility icons (refresh, personalize, guide…) | `utilities`, `onRefresh` (all locked while `editing`) |
| Ribbon tabs ("Sales order", "Free text invoice") and plain commands (Search, tabs not built yet) | `ribbons`, `commands` (locked while `editing`) |
| Record list hidden at first, resizable, width remembered, custom search | `defaultNavigationOpen`, `navigationWidthStorageKey`, `matchesSearch` |
| Field-config sections (groups, view/edit, master routes, previews) | `sections`, `values`, `onFieldChange`, `onFieldEdit`, `canEditField` |

Sales order itself stays on `list-details` (its look is the shared default there).

### Example: free text invoice

`free-text-invoice/pages/CustFreeInvoiceDetailsPage.tsx` uses master-detail with
the sales order layout:

- **Lines tab:** compact header FastTab, lines (`MasterDetailLines`), line details (`MasterDetailLineDetails`), totals (`MasterDetailTotals`).
- **Header tab:** General / Setup / Payment FastTabs with `summaryItems`.
- **Title and status:** title `<invoice> : <customer>`, status Posted / Not posted.
- **Action pane:** a "Free text invoice" ribbon (Post, Totals), and Search.
- **Field rendering:** in view mode, fields are plain configs that the engine renders from `values`, with `previewEntity` hover cards and the pencil via `onFieldEdit` / `canEditField`. In edit mode, the page supplies the editors.

The record list starts closed on small screens, where it would cover the page.

## Parts

| Part | What it renders |
| --- | --- |
| `MasterDetailPage` | Action pane (back, record-pane toggle, your actions), record navigation pane (filter, primary/secondary text, lazy batches, keyboard-resizable, locked while editing), breadcrumb + view label, title, status, tabs, loading/error states. |
| `MasterDetailTabs` | Lines / Header view tabs under the title. |
| `MasterDetailSection` | FastTab: bordered collapsible section with title and a summary of key values when collapsed. |
| `MasterDetailFieldGroups` / `MasterDetailFieldGroup` | Header panel: responsive columns of titled (uppercase) field groups. |
| `MasterDetailField` | One field: label + read-only value with pencil (view), or your editor (edit). Master data gets the open-details icon (view mode only), blue value and the hover record card. |
| `MasterDetailLines` | Lines area: ARIA toolbar (`actions` = Add line / Remove buttons, plus free `toolbar`), keyboard help / "Saving...", the grid, and the selected line's details. |
| `MasterDetailLineDetails` | "Line details": scrollable tabs, one tab panel, "select a line" hint. |
| `MasterDetailTotals` | "Totals" FastTab: label above a right-aligned amount; `emphasis` for the document total. |
| `MasterDetailLinesGrid` | Lines grid with the sales order look and the D365 edit flow: rows are edited in place in edit mode, and in view mode a pencil on an editable cell switches the page to edit mode and edits that cell. Props: `editing`, `busy`, `canEdit`, `onEdit`, plus normal `DataGrid` props. |

## Sales order options and their pattern equivalents

| Sales order | Pattern option |
| --- | --- |
| Record list on the left (`SO-…`, `CUST - name`), filter, toggle button | `records` (`title`, `subtitle`), `filterLabel`, `navigationLabel` |
| Record list locked while editing the header | `editing` |
| "Sales order details \| Standard view" | `subtitle`, `viewLabel` |
| "SO-… : Customer" title, "Backorder" on the right | `title`, `status` |
| Lines / Header tabs | `tabs={<MasterDetailTabs …/>}` |
| "Sales order header" / "Sales order lines" FastTabs | `MasterDetailSection` |
| Header panel groups (DELIVERY ADDRESS, WAREHOUSE…) | `MasterDetailFieldGroups` + `MasterDetailFieldGroup` |
| Site / Warehouse with open-details icon and hover card | `MasterDetailField masterRoute=… previewOption=…` |
| Pencil on a view value to start editing | `MasterDetailField onEdit=…` |
| Editors in edit mode | `MasterDetailField editing editor={<LookupField …/>}` |
| Line actions toolbar, keyboard help, "Saving..." | `MasterDetailLines toolbar help saving` |
| Lines grid with D365 keys | `MasterDetailLines grid={<DataGrid …/>}` |
| Line details tabs under the grid | `MasterDetailLines details=…` |
| Loading / "not found" / retry | `loading`, `error`, `onRetry` |

What stays in the feature (as in `accounts-receivable/sales-order`): data
queries with company-scoped keys, load-by-ID, Edit/Save/Cancel and unsaved
changes, line saving, action-pane ribbon content, dialogs. Follow the
[document page checklist](DOCUMENT_PAGE.md).
