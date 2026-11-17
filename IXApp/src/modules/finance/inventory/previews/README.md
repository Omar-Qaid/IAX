# Inventory record previews

`inventoryRecordPreviews.ts` registers the D365 hover cards for sites and warehouses (ID and name) and exports their entity names as `INVENTORY_RECORD_PREVIEWS`. Each entry declares its `masterRoute`, so every field, lookup or column pointing at the site or warehouse form shows the card automatically.

Imported once from `app/configuration/recordPreviews.ts`.

[Record preview](../../../../shared/components/record-preview/README.md)
