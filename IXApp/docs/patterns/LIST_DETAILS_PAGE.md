# List-details pattern

`ListDetailsPage` supports two contracts.

## Standard variant

The legacy/default variant accepts `title`, optional subtitle/action pane, `DataGridProps`, optional detail pane, loading, selection, and dialogs.

## Enterprise variant

`variant="enterprise"` accepts `EnterpriseListDetailsConfig<T>`. It supports static, controlled, or remote repositories; list or grid master presentation; search/filter and related-information panels; CRUD permissions; synchronous/async validation; typed detail sections/fields; number-sequence metadata; optional pane resizing/persistence; and confirmation around destructive or dirty transitions.

Module examples include currency, legal entity, number sequence, workflow process/step/activity, payment mode, and payment term pages. Define mapping functions (`getValues`/`setValues`) and `createRecord` at the module boundary. Remote repositories receive an `AbortSignal` for loads and explicit create/update/delete functions.

Use stable record IDs and do not mix a second uncontrolled form state with the pattern draft.

## Enterprise setup defaults

Use list-details for setup pages such as customer payment terms, tax codes, tax groups and exchange rates. Enterprise pages share the action pane, title/status row, Standard view label, detail sections, utility rail and resizable navigation. Sections appear together without Lines/Header document tabs. CRUD, validation, Save, Cancel and Escape remain shared.

Use master-details for sales orders and other header/line documents. The module supplies fields, validation, permissions and API calls.

```tsx
<ListDetailsPage<Document>
  variant="enterprise"
  title={`${document.number} : ${document.name}`}
  config={{
    recordTableName: 'YourExistingTable',
    breadcrumb: { listPath: listRoute, labelKey: 'yourModule.document' },
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
        id: 'lines',
        tab: 'lines',
        title: 'Lines',
        hideHeader: true,
        content: (
          <TabularDetailPanel<Line>
            rows={record.lines}
            columns={lineColumns}
            addLabel="Add line"
            removeLabel="Remove"
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
      {
        id: 'totals',
        tab: 'lines',
        title: 'Totals',
        content: <MasterDetailTotals items={buildTotals(record)} />,
      },
      { id: 'general', tab: 'header', title: 'General', groups: headerGroups },
      { id: 'setup', tab: 'header', title: 'Setup', groups: setupGroups },
    ],
  }}
/>
```

The identifiers in this blueprint are the module's own types, field groups and business functions. No page-specific title markup, tab state, grid styling or Save/Cancel coordinator is needed. `TabularDetailPanel` participates in the page edit session automatically.
