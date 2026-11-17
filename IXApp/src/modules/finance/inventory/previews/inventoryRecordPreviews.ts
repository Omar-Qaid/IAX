import { pagedRecordLoader, registerRecordPreview } from '@shared/components/record-preview';
import { INVENTORY_ROUTE_PATHS } from '../routes/inventoryRoutePaths';

/** Record preview entities (D365 hover cards) owned by Inventory. */
export const INVENTORY_RECORD_PREVIEWS = {
  SITE: 'inventSite',
  WAREHOUSE: 'inventLocation',
} as const;

// Server DTO fields shown by the cards (the paged loader returns raw DTOs).
interface SitePreview {
  siteId: string;
  name: string;
}
interface WarehousePreview {
  inventLocationId: string;
  name: string;
}

registerRecordPreview<SitePreview>(INVENTORY_RECORD_PREVIEWS.SITE, {
  masterRoute: INVENTORY_ROUTE_PATHS.SITES,
  load: pagedRecordLoader('/v1/InventSite', 'SiteId'),
  fields: [
    { labelKey: 'fields.site', defaultLabel: 'Site', value: 'siteId', lookup: true },
    { labelKey: 'fields.name', defaultLabel: 'Name', value: 'name' },
  ],
});

registerRecordPreview<WarehousePreview>(INVENTORY_RECORD_PREVIEWS.WAREHOUSE, {
  masterRoute: INVENTORY_ROUTE_PATHS.WAREHOUSES,
  load: pagedRecordLoader('/v1/InventLocation', 'InventLocationId'),
  fields: [
    { labelKey: 'fields.warehouse', defaultLabel: 'Warehouse', value: 'inventLocationId', lookup: true },
    { labelKey: 'fields.name', defaultLabel: 'Name', value: 'name' },
  ],
});
