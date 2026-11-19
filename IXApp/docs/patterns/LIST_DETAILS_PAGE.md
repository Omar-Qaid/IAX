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
<ListDetailsPage<SetupRecord>
  variant="enterprise"
  title={t('pages.setup.title')}
  config={{
    recordTableName: 'YourExistingTable',
    dataSource: { type: 'controlled', records, onRecordsChange: setRecords },
    createRecord,
    getPrimaryText: (record) => record.code,
    getSecondaryText: (record) => record.name,
    getValues,
    setValues,
    headerFields,
    permissions,
    validate,
    sections: [
      { id: 'general', title: 'General', groups: generalGroups },
      { id: 'setup', title: 'Setup', groups: setupGroups },
    ],
  }}
/>
```

The module supplies its own record type, field groups and validation. Nested setup grids can use TabularDetailPanel to participate in the shared edit session.
