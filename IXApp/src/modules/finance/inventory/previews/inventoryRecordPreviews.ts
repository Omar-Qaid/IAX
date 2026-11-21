import {
  listRecordLoader,
  pagedRecordLoader,
  registerRecordPreview,
} from '@shared/components/record-preview';
import { releasedProductApi } from '../products/api/releasedProductApi';
import { unitOfMeasureApi } from '../units-of-measure/api/unitOfMeasureApi';
import { INVENTORY_ROUTE_PATHS } from '../routes/inventoryRoutePaths';

/** Record preview entities (D365 hover cards) owned by Inventory. */
export const INVENTORY_RECORD_PREVIEWS = {
  SITE: 'inventSite',
  WAREHOUSE: 'inventLocation',
  RELEASED_PRODUCT: 'releasedProduct',
  UNIT: 'unitOfMeasure',
} as const;

registerRecordPreview(INVENTORY_RECORD_PREVIEWS.RELEASED_PRODUCT, {
  masterRoute: INVENTORY_ROUTE_PATHS.RELEASED_PRODUCTS,
  load: listRecordLoader(releasedProductApi.list, (record) => record.itemNumber),
  fields: [
    {
      labelKey: 'fields.itemNumber',
      defaultLabel: 'Item number',
      value: 'itemNumber',
      lookup: true,
    },
    { labelKey: 'fields.name', defaultLabel: 'Name', value: 'name' },
  ],
});

registerRecordPreview(INVENTORY_RECORD_PREVIEWS.UNIT, {
  masterRoute: INVENTORY_ROUTE_PATHS.UNITS_OF_MEASURE,
  load: listRecordLoader(unitOfMeasureApi.list, (record) => record.symbol),
  fields: [
    { labelKey: 'fields.unit', defaultLabel: 'Unit', value: 'symbol', lookup: true },
    {
      labelKey: 'unitsOfMeasure.decimalPrecision',
      defaultLabel: 'Decimal precision',
      value: 'decimalPrecision',
    },
  ],
});

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
  fromOption: (option) => ({ siteId: option.code, name: option.name ?? '' }),
  fields: [
    { labelKey: 'fields.site', defaultLabel: 'Site', value: 'siteId', lookup: true },
    { labelKey: 'fields.name', defaultLabel: 'Name', value: 'name' },
  ],
});

registerRecordPreview<WarehousePreview>(INVENTORY_RECORD_PREVIEWS.WAREHOUSE, {
  masterRoute: INVENTORY_ROUTE_PATHS.WAREHOUSES,
  load: pagedRecordLoader('/v1/InventLocation', 'InventLocationId'),
  fromOption: (option) => ({ inventLocationId: option.code, name: option.name ?? '' }),
  fields: [
    {
      labelKey: 'fields.warehouse',
      defaultLabel: 'Warehouse',
      value: 'inventLocationId',
      lookup: true,
    },
    { labelKey: 'fields.name', defaultLabel: 'Name', value: 'name' },
  ],
});
