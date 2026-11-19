# Master-detail page

`src/patterns/master-detail` is the D365 details page for a **master record with
lines** (sales order, purchase order, transfer order, journals). It is built from
the sales order page and uses the same parts, sizes and fonts (`d365` tokens), so
new pages look and behave like sales order without copying its code.

```tsx
import {
  MasterDetailPage,
  MasterDetailTabs,
  MasterDetailSection,
  MasterDetailFieldGroups,
  MasterDetailFieldGroup,
  MasterDetailField,
  MasterDetailLines,
} from '@patterns/master-detail';
```

## Which pattern to use

Any page with a master record and lines uses `master-detail`. It has the full
enterprise feature set, composed from the same shared parts as `list-details`
(so both stay in step):

| Feature                                                                                          | Option                                                                |
| ------------------------------------------------------------------------------------------------ | --------------------------------------------------------------------- |
| Advanced filter pane (right rail → Filter)                                                       | `advancedFilter={{ fields: [{ id, label, getValue }] }}`              |
| Related information side panel (FactBox)                                                         | `relatedInformation={{ title, sections }}`                            |
| Record info / audit (Options menu)                                                               | `recordTableName`, `getAuditRecordId`, `showOptionsMenu`              |
| Attachments                                                                                      | `attachments={{ refTableId, refRecId }}`                              |
| Utility icons (refresh, personalize, guide…)                                                     | `utilities`, `onRefresh` (all locked while `editing`)                 |
| Ribbon tabs ("Sales order", "Free text invoice") and plain commands (Search, tabs not built yet) | `ribbons`, `commands` (locked while `editing`)                        |
| Record list hidden at first, resizable, width remembered, custom search                          | `defaultNavigationOpen`, `navigationWidthStorageKey`, `matchesSearch` |
| Field-config sections (groups, view/edit, master routes, previews)                               | `sections`, `values`, `onFieldChange`, `onFieldEdit`, `canEditField`  |

Sales order and free-text invoice both use master-details. Sales-order details has one canonical implementation.

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

| Part                                                 | What it renders                                                                                                                                                                                                                                                                    |
| ---------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `MasterDetailPage`                                   | Action pane (back, record-pane toggle, your actions), record navigation pane (filter, primary/secondary text, lazy batches, keyboard-resizable, locked while editing), breadcrumb + view label, title, status, tabs, loading/error states.                                         |
| `MasterDetailTabs`                                   | Lines / Header view tabs under the title.                                                                                                                                                                                                                                          |
| `MasterDetailSection`                                | FastTab: bordered collapsible section with title and a summary of key values when collapsed.                                                                                                                                                                                       |
| `MasterDetailFieldGroups` / `MasterDetailFieldGroup` | Header panel: responsive columns of titled (uppercase) field groups.                                                                                                                                                                                                               |
| `MasterDetailField`                                  | One field: label + read-only value with pencil (view), or your editor (edit). Master data gets the open-details icon (view mode only), blue value and the hover record card.                                                                                                       |
| `MasterDetailLines`                                  | Lines area: ARIA toolbar (`actions` = Add line / Remove buttons, plus free `toolbar`), keyboard help / "Saving...", the grid, and the selected line's details.                                                                                                                     |
| `MasterDetailLineDetails`                            | "Line details": scrollable tabs, one tab panel, "select a line" hint.                                                                                                                                                                                                              |
| `MasterDetailTotals`                                 | "Totals" FastTab: label above a right-aligned amount; `emphasis` for the document total.                                                                                                                                                                                           |
| `MasterDetailLinesGrid`                              | Lines grid with the sales order look and the D365 edit flow: rows are edited in place in edit mode, and in view mode a pencil on an editable cell switches the page to edit mode and edits that cell. Props: `editing`, `busy`, `canEdit`, `onEdit`, plus normal `DataGrid` props. |

## Sales order options and their pattern equivalents

| Sales order                                                            | Pattern option                                                    |
| ---------------------------------------------------------------------- | ----------------------------------------------------------------- |
| Record list on the left (`SO-…`, `CUST - name`), filter, toggle button | `records` (`title`, `subtitle`), `filterLabel`, `navigationLabel` |
| Record list locked while editing the header                            | `editing`                                                         |
| "Sales order details \| Standard view"                                 | `subtitle`, `viewLabel`                                           |
| "SO-… : Customer" title, "Backorder" on the right                      | `title`, `status`                                                 |
| Lines / Header tabs                                                    | `tabs={<MasterDetailTabs …/>}`                                    |
| "Sales order header" / "Sales order lines" FastTabs                    | `MasterDetailSection`                                             |
| Header panel groups (DELIVERY ADDRESS, WAREHOUSE…)                     | `MasterDetailFieldGroups` + `MasterDetailFieldGroup`              |
| Site / Warehouse with open-details icon and hover card                 | `MasterDetailField masterRoute=… previewOption=…`                 |
| Pencil on a view value to start editing                                | `MasterDetailField onEdit=…`                                      |
| Editors in edit mode                                                   | `MasterDetailField editing editor={<LookupField …/>}`             |
| Line actions toolbar, keyboard help, "Saving..."                       | `MasterDetailLines toolbar help saving`                           |
| Lines grid with D365 keys                                              | `MasterDetailLines grid={<DataGrid …/>}`                          |
| Line details tabs under the grid                                       | `MasterDetailLines details=…`                                     |
| Loading / "not found" / retry                                          | `loading`, `error`, `onRetry`                                     |

What stays in the feature (as in `accounts-receivable/sales-order`): data
queries with company-scoped keys, load-by-ID, Edit/Save/Cancel and unsaved
changes, line saving, action-pane ribbon content, dialogs. Follow the
[document page checklist](DOCUMENT_PAGE.md).

## Standard structure for every new feature

Create a feature with the generator rather than copying a whole sales-order page:

```powershell
npm run generate:master-detail -- --feature finance/accounts-receivable/example-order --name ExampleOrder --dry-run
npm run generate:master-detail -- --feature finance/accounts-receivable/example-order --name ExampleOrder
```

The generator refuses to overwrite existing feature folders. It does not register routes, infer permissions, invent API endpoints, or implement posting.

```text
example-order/
  api/exampleOrderApi.ts
  pages/ExampleOrderDetailsPage.tsx
  details/ExampleOrderDetails.tsx
  details/useExampleOrderDocument.ts
  details/exampleOrderModel.ts
  details/ExampleOrderHeaderSections.ts
  lines/ExampleOrderLineColumns.ts
  README.md
```

| File                                 | Responsibility                                                         |
| ------------------------------------ | ---------------------------------------------------------------------- |
| `pages/<Feature>DetailsPage.tsx`     | Thin route entry; resets state for company/document changes.           |
| `details/<Feature>Details.tsx`       | Composes shared master-details UI.                                     |
| `details/use<Feature>Document.ts`    | Feature controller entry, using `useMasterDetailDocument` by default.  |
| `details/<feature>Model.ts`          | Feature document and line UI types; map existing API contracts here.   |
| `details/<Feature>HeaderSections.ts` | Header field groups and sections.                                      |
| `lines/<Feature>LineColumns.ts`      | Line columns, lookups and editors.                                     |
| `api/<feature>Api.ts`                | Adapter contract for existing company-scoped API calls and validation. |

Implement the generated adapter and extend the existing API fields before registering the page. Pass `company`, `documentId`, `source`, `canEdit`, `listPath`, `onSelect` and `onBack` from the route integration. Use a stable source object with a unique feature `queryKey`. Derive `canEdit` from actual permissions and document status. The scaffold supports editing existing documents; new/delete/posting workflows must be connected explicitly to supported feature operations.

The shared controller commits grid edits before document validation and Save. Rejected rows or API errors retain Edit mode. Cancel clears row editors and document drafts; concurrent Save and Cancel are locked during persistence. The feature supplies validation, data mapping and business rules. Standard fields, grids, tabs, Search, totals and layout remain in the pattern. Add optional `lines/` helpers or feature dialogs only when required by feature behavior; do not create empty component wrappers.

`npm run audit:master-detail` checks actual master-details compositions for the common directory/controller/header-section structure. It runs as part of `audit:static`. Generator tests run with `node --test scripts/generate-master-detail.checks.mjs`.

Existing sales-order and invoice controllers keep their specialized posting and draft behavior; they follow the same folder responsibilities without forcing different business workflows into identical logic.

### One file map for all future features

`scripts/master-detail-structure.mjs` is the single source for the eight standard filenames. The generator and audit both use it. Generated README files carry a feature marker so the audit can verify the model, API adapter, thin route page, controller, details composition, header sections and line columns. Renaming a required file away from the standard fails verification. Header sections and line columns may use `.tsx` when custom editors need JSX. Additional helpers are allowed only for feature-specific behavior; they do not replace the standard entry files.

The sales-order and free-text-invoice implementations retain their existing specialized files. Every newly generated feature uses the exact standard file map. Regression checks generate twelve temporary features and confirm that all twelve follow the same map; no sample features or routes are added to the application.

## Existing sales-order and free-text-invoice structure

Both production features now organize their document code by the same responsibilities:

```text
details/
  <Feature>Details.tsx
  use<Feature>Document.ts[x]
  <Feature>HeaderFields.tsx
  <Feature>HeaderSections.ts[x]
  <feature>Totals.tsx
  <Feature>DetailsDialogs.tsx
lines/
  <Feature>LinesGrid.tsx
  <Feature>LineFields.tsx
  <Feature>LineTabs.tsx
  use<Feature>LineColumns.ts[x]
```

Header fields, totals and document dialogs stay in `details/`; grid actions, line editors and line tabs stay in `lines/`. Both line-tab builders use `MasterDetailLineDetails`, both totals builders use `MasterDetailTotals`, and both grids inherit `PatternDataGrid` defaults. Invoice uses `MasterDetailLinesGrid` for page-controlled rows; sales order retains its cell-save and keyboard adapters around the same generic grid. Sales-specific discount, pricing and posting helpers remain feature-owned.

The structural audit verifies these common roles in both existing features. This refactor does not introduce any new feature or route.

## Account contact forms

The existing `/contacts/:kind/:accountNum/:contactId` route uses `MasterDetailPage` for new, view and edit contacts. `ContactPersonPage` remains the route entry; `contacts/details` owns the composition, controller, field mappings, sections and contact model, while `contacts/api/contactPersonApi.ts` retains the existing account-scoped endpoints.

This record form has no sales lines, so it does not add document tabs, a line model or an empty `lines/` folder. Existing address and electronic-contact panels remain in their FastTabs and share the page edit session. The contact controller reuses the generic CRUD state hook; it does not render the list-details UI. Validation/API failures retain the draft, Cancel restores or leaves the new form, successful creation opens the saved contact, and delete requires confirmation.

For master-details forms without line filtering, `searchNavigation` exposes the shared Search command that opens and focuses the record-list filter. Existing document pages continue using their own `onSearch` callbacks for line filters. Contact names and Arabic names remain separate stored/editable values, with localized display-only captions.
