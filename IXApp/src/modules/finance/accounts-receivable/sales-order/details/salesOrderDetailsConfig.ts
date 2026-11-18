import { TAX_ROUTE_PATHS } from '@modules/finance/tax/routes/taxRoutePaths';
import type { PostalAddressPreview } from '@shared/components/logistics';
import { ACCOUNTS_RECEIVABLE_ROUTE_PATHS } from '../../routes/accountsReceivableRoutePaths';
import { FOUNDATION_ROUTE_PATHS } from '../../../foundation/routes/foundationRoutePaths';
import { FOUNDATION_RECORD_PREVIEWS } from '../../../foundation/previews/foundationRecordPreviews';
import { INVENTORY_ROUTE_PATHS } from '../../../inventory/routes/inventoryRoutePaths';
import type { SalesOrderListRecord } from '../api/salesOrderListApi';
import type { SalesOrderDeliveryAddressOption, SalesOrderLineRecord } from '../api/salesOrderLinesApi';

/*
 * Static field configuration of the sales order document: which fields are
 * master data (open-details route, record preview) and how addresses preview.
 */

export const toAddressPreview = (
  option: SalesOrderDeliveryAddressOption | undefined
): PostalAddressPreview | undefined =>
  option
    ? { location: option.label, validFrom: option.postalAddress?.validFrom, address: option.address }
    : undefined;

export type DetailLine = SalesOrderLineRecord;

export const headerMasterRoutes: Partial<Record<keyof SalesOrderListRecord, string>> = {
  customerAccount: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMERS,
  invoiceAccount: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMERS,
  customerGroup: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_GROUPS,
  currencyCode: FOUNDATION_ROUTE_PATHS.CURRENCIES,
  inventSiteId: INVENTORY_ROUTE_PATHS.SITES,
  inventLocationId: INVENTORY_ROUTE_PATHS.WAREHOUSES,
  taxGroupId: TAX_ROUTE_PATHS.TAX_GROUPS,
  paymentTerms: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_PAYMENT_TERMS,
  paymentMethod: ACCOUNTS_RECEIVABLE_ROUTE_PATHS.CUSTOMER_PAYMENT_METHODS,
  chargesGroup: FOUNDATION_ROUTE_PATHS.CHARGES_CODES,
};
// Masters with a registered preview but no master form route.
export const headerPreviewEntities: Partial<Record<keyof SalesOrderListRecord, string>> = {
  deliveryMode: FOUNDATION_RECORD_PREVIEWS.DELIVERY_MODE,
  deliveryTerms: FOUNDATION_RECORD_PREVIEWS.DELIVERY_TERMS,
};
// Line master fields: master form route (open details) and preview entity.
export const lineMasters: Partial<Record<keyof DetailLine, { masterRoute?: string; entity?: string }>> = {
  site: { masterRoute: INVENTORY_ROUTE_PATHS.SITES },
  warehouse: { masterRoute: INVENTORY_ROUTE_PATHS.WAREHOUSES },
  unit: { masterRoute: INVENTORY_ROUTE_PATHS.UNITS_OF_MEASURE },
  packingUnit: { masterRoute: INVENTORY_ROUTE_PATHS.UNITS_OF_MEASURE },
  taxGroup: { masterRoute: TAX_ROUTE_PATHS.TAX_GROUPS },
  taxItemGroup: { masterRoute: TAX_ROUTE_PATHS.TAX_ITEM_GROUPS },
  deliveryMode: { entity: FOUNDATION_RECORD_PREVIEWS.DELIVERY_MODE },
  deliveryTerms: { entity: FOUNDATION_RECORD_PREVIEWS.DELIVERY_TERMS },
};
export const salesOrderRibbonPinnedStorageKey = 'sales-order.action-pane.ribbon-pinned';
