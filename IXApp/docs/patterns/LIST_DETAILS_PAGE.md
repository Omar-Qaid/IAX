# List-details pattern

`ListDetailsPage` supports two contracts.

## Standard variant

The legacy/default variant accepts `title`, optional subtitle/action pane, `DataGridProps`, optional detail pane, loading, selection, and dialogs.

## Enterprise variant

`variant="enterprise"` accepts `EnterpriseListDetailsConfig<T>`. It supports static, controlled, or remote repositories; list or grid master presentation; search/filter and related-information panels; CRUD permissions; synchronous/async validation; typed detail sections/fields; number-sequence metadata; optional pane resizing/persistence; and confirmation around destructive or dirty transitions.

Module examples include currency, legal entity, number sequence, workflow process/step/activity, payment mode, and payment term pages. Define mapping functions (`getValues`/`setValues`) and `createRecord` at the module boundary. Remote repositories receive an `AbortSignal` for loads and explicit create/update/delete functions.

Use stable record IDs and do not mix a second uncontrolled form state with the pattern draft.

## Sales-order details defaults

Every new `variant="enterprise"` page inherits the sales-order page chrome: flat action pane, title/status row, document description and Standard view label, Lines/Header tabs, FastTabs with four-column field groups, utility rail, and a collapsed, resizable 280px navigation pane. Grid defaults and page/line Save, Cancel and Escape behavior are shared as well. Sales order itself now uses these defaults instead of supplying its own header and tabs.

The module supplies its record fields, line columns, validation, permissions and API calls. Sales posting and other sales-specific actions remain in the module; use `commands`/ribbon groups for the matching actions of the new entity.

Use `subtitle` for the document description and `status` for a record status. Assign `tab: 'lines'` or `tab: 'header'` to a section; sections without a tab appear in both views. Inactive sections stay mounted, preserving unfinished line drafts. The default tabs manage their own selection; use `detailTabs.value`/`onChange` only when the page needs controlled navigation. `detailTabs: false` explicitly selects a single-view form. Existing custom `detailHeader` content is preserved and gets no additional tabs unless `detailTabs` is provided.

```tsx
<ListDetailsPage<Document>
  variant="enterprise"
  title={`${document.number} : ${document.name}`}
  config={{
    recordTableName: 'YourExistingTable',
    subtitle: 'Document details',
    status: (record) => record.status,
    dataSource: { type: 'controlled', records, onRecordsChange: setRecords },
    createRecord,
    getPrimaryText: (record) => record.number,
    getSecondaryText: (record) => record.name,
    getValues,
    setValues,
    headerFields: [],
    permissions,
    validate,
    sections: ({ record, editing, onRecordChange }) => [
      { id: 'summary', tab: 'lines', title: 'Document header', groups: summaryGroups },
      {
        id: 'lines', tab: 'lines', title: 'Lines', hideHeader: true,
        content: (
          <TabularDetailPanel<Line>
            rows={record.lines}
            columns={lineColumns}
            addLabel="Add line" removeLabel="Remove"
            selectedIds={selectedLineIds}
            onSelectionChange={setSelectedLineIds}
            onAdd={() => onRecordChange(addLine(record))}
            onRemove={() => onRecordChange(removeLines(record, selectedLineIds))}
            onRowSave={(line) => {
              validateLine(line); // Throw to retain the row draft and block page Save.
              onRecordChange(replaceLine(record, line));
            }}
          />
        ),
      },
      { id: 'line-details', tab: 'lines', title: 'Line details', groups: lineDetailGroups },
      { id: 'totals', tab: 'lines', title: 'Totals', content: <MasterDetailTotals items={buildTotals(record)} /> },
      { id: 'general', tab: 'header', title: 'General', groups: headerGroups },
      { id: 'setup', tab: 'header', title: 'Setup', groups: setupGroups },
    ],
  }}
/>
```

The identifiers in this blueprint are the module's own types, field groups and business functions. No page-specific title markup, tab state, grid styling or Save/Cancel coordinator is needed. `TabularDetailPanel` participates in the page edit session automatically.
