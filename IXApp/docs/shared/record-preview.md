# Record preview

`src/shared/components/record-preview` shows the D365 F&O record preview: hovering or focusing a master-data value (customer account, customer group, site…) opens a small card with that record's key fields. It works the same in grids and forms, and each entity is defined once.

## Exports

- `RecordPreview`: wraps any value; resolves the entity from `entity` or `masterRoute`, renders children unchanged when none applies or the key is empty.
- `RecordPreviewCard`: the presentational D365 card (labels over underlined values, linked values in the link colour, pointer toward the field). Also used by grid lookups for their option preview.
- `useRecordPreview(entity, key)`: hover/focus timing, keyboard behaviour and the TanStack Query load.
- `registerRecordPreview`, `getRecordPreview`, `resolveRecordPreviewEntity`: the entity registry.
- `pagedRecordLoader(endpoint, keyField)`: loads one record from any `BaseController` `/paged` endpoint filtered by its key. `listRecordLoader(loadList, getKey)`: finds it in a list that is already loaded and cached.
- Types `RecordPreviewDefinition`, `RecordPreviewField`, `RecordPreviewRow`; constants for the delays, cache time and `recordPreviewQueryKeys`.

## Registering an entity

A module registers its entities in `previews/<module>RecordPreviews.ts` and that file is imported once from `app/configuration/recordPreviews.ts`.

```ts
registerRecordPreview<CustomerGroupRecord>(ACCOUNTS_RECEIVABLE_RECORD_PREVIEWS.CUSTOMER_GROUP, {
  masterRoute: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_GROUPS,
  load: pagedRecordLoader('/v1/CustGroup', 'CustGroupId'),
  fields: [
    { labelKey: 'fields.customerGroup', defaultLabel: 'Customer group', value: 'custGroupId', lookup: true },
    { labelKey: 'fields.description', defaultLabel: 'Description', value: 'name' },
  ],
});
```

`fields[].value` is a property name or a function (for localized or computed text); `lookup: true` marks a reference to another master record. Use existing route constants and `fields.*` translation keys.

## Registered entities

Like D365, a card shows only the fields that identify the record, not the whole record.

| Entity | Card | Defined in |
| --- | --- | --- |
| Customer | Name | `accounts-receivable/previews` |
| Customer group | Customer group, Description | `accounts-receivable/previews` |
| Terms of payment | Terms of payment, Description | `accounts-receivable/previews` |
| Method of payment | Method of payment, Description | `accounts-receivable/previews` |
| Site | Site, Name | `inventory/previews` |
| Warehouse | Warehouse, Name | `inventory/previews` |
| Currency | Currency, Name | `foundation/previews` |
| Sales tax group | Sales tax group, Description | `foundation/previews` |
| Item sales tax group | Item sales tax group, Description | `foundation/previews` |
| Tax authority | Tax authority, Name | `foundation/previews` |
| Mode of delivery | Mode of delivery, Description | `foundation/previews` (no master form: use `previewEntity`) |
| Delivery terms | Delivery terms, Description | `foundation/previews` (no master form: use `previewEntity`) |
| Postal address | Location, Effective, Address (record passed by the page) | `shared/components/logistics` |

An entity without `load` (postal address) is previewed only where the page passes the `record` it already holds; elsewhere its value renders unchanged.

## Where it applies

| Place | How it is enabled |
| --- | --- |
| Form fields of the list-details pattern | Automatically, when the field's `masterRoute` belongs to a registered entity, or explicitly with `previewEntity`. |
| `LookupField`, `LookupGridField`, `DataGridLookupField`, `LookupDisplayField` | Automatically by `masterRoute`, or explicitly with `previewEntity`. |
| Grid columns | `preview: { entity }` on the `ColumnDef`; the key is the cell value unless `getKey` is given. |
| Any other value | `<RecordPreview entity="customer" recordKey={account}>…</RecordPreview>` |

## Behaviour

Opens 500 ms after the pointer or keyboard focus rests on the value, never over an open dropdown list. Stays open while the pointer is on the card; closes on leaving, typing or Esc. The record is fetched only when the card opens and is cached for five minutes per record. Missing records show "No details found."; failed loads show an error line instead of the fields.

[Shared layer](../shared.md) · [Lookups](lookups.md) · [Data grid](data-grid.md)
