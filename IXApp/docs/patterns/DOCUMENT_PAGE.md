# Document pattern

`DocumentPage` composes:

- `PageHeader` with optional string badge;
- optional `ActionPane` content;
- bordered header and lines surfaces;
- optional totals in a responsive `xs=12`, `sm=6`, `md=4` region;
- optional dialogs.

`DocumentHeader`, `DocumentLines`, and `DocumentTotals` are small reusable wrappers. `patterns/document/useDocumentPage.ts` manages a local document and lines collection; `shared/hooks/useDocumentPage.ts` is a separate legacy async hook.

Lifecycle mutations are not supplied by the pattern and must be implemented by the module.

## Reference implementation: sales order

New header-and-lines documents (purchase order, transfer order, journals with
lines…) copy the structure of
[`accounts-receivable/sales-order`](../../src/modules/finance/accounts-receivable/sales-order/README.md):

```
<feature>/
  api/          typed clients: list, get(id), update header, lines, lookup pages
  pages/        <Doc>ListPage, <Doc>DetailsPage (wiring only), journals
  details/      use<Doc>Document (state + actions), use<Doc>DocumentData (queries),
                view builders (totals, header panel, line fields, line tabs, header sections),
                <doc>DetailsConfig.ts (master routes, previews), use<Doc>Options (enums)
  lines/        <Doc>LinesGrid and its cells
  components/   dialogs (quick create, posting)
```

## Checklist for a new document page

1. **Query keys** — add a company-scoped factory in the module's `queries/`
   (`list(company)`, `order(company, id)` with lines/totals/... below it). Use it
   for every `useQuery` and invalidation; refresh after changes with one
   `invalidate<Doc>(id)`.
2. **Load one record** — a backend `GET {id}/details` endpoint in the list shape;
   the page queries it by ID; the record pane reuses the list query.
3. **Edit lifecycle** — Edit / Save / Cancel in the action pane; Save writes only
   changed lines; `useUnsavedChanges` while a draft exists; pass `editing` to
   list-details sections the page controls.
4. **Lines grid** — shared `DataGrid` line editing (D365 keys: arrows/Tab save
   and move, Alt+N new line, Esc cancel); lookups in cells with
   `DataGridLookupField`.
5. **Master fields** — `masterRoute` for open details (shown in view mode only)
   and a registered record preview; pass the loaded option so cards need no
   request. See [Record preview](../shared/record-preview.md).
6. **Large lookups** — server-paged (`fetchPage`), never load every page.
7. **Translations** — every label through `t()` with English and Arabic entries.
8. **Tests** — list page (rows, filters, previews) and details page (load by ID,
   unknown ID, header edit/save/cancel, line editing, tab switching).
9. **Type-check** — run `npx tsc --noEmit -p tsconfig.app.json`. (`tsconfig.json`
   only references other configs, so `tsc -p .` checks nothing.)
