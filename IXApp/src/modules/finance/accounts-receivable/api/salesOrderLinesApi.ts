import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type { LogisticsPostalAddress } from '@shared/types/logistics';

export interface SalesOrderLineRecord {
  id: string;
  lineNumber: number;
  itemNumber: string;
  productName?: string;
  description: string;
  quantity: number;
  unit: string;
  unitPrice: number;
  lineTotal: number;
  itemType?: string;
  arabicName?: string;
  salesCategory?: number;
  lineType?: number;
  preRelatedInvoices?: string;
  deliveryType?: number;
  lineDeliveryType?: number;
  sourcingOrigin?: number;
  excludeFromMasterPlanning?: boolean;
  deliveryDateControlType?: number;
  mpsFullRunCtpStatus?: number;
  shipCarrierDlvType?: number;
  planningPriority?: number;
  directDelivery?: boolean;
  site?: string;
  warehouse?: string;
  deliveryDate?: string;
  inventTransId?: string;
  inventDimId?: string;
  configId?: string;
  inventSizeId?: string;
  inventColorId?: string;
  inventStyleId?: string;
  inventVersionId?: string;
  currencyCode?: string;
  salesStatus?: string;
  customerLineNumber?: number;
  intercompanyOrigin?: number;
  stopped?: boolean;
  preventPartialDelivery?: boolean;
  batchNumber?: string;
  serialNumber?: string;
  location?: string;
  inventoryStatus?: string;
  licensePlate?: string;
  itemReferenceNumber?: string;
  itemReferenceType?: number;
  itemReferenceLot?: string;
  priceUnit?: number;
  usePriceAgreement?: boolean;
  costPrice?: number;
  lineDiscount?: number;
  lineDiscountPercent?: number;
  multiLineDiscount?: number;
  multiLineDiscountPercent?: number;
  salesMarkup?: number;
  excludeFromRebate?: boolean;
  excludeFromRebateManagement?: boolean;
  overDeliveryPercent?: number;
  underDeliveryPercent?: number;
  remainSalesPhysical?: number;
  remainSalesFinancial?: number;
  salesDeliverNow?: number;
  inventDeliverNow?: number;
  packingUnit?: string;
  packingUnitQuantity?: number;
  deliveryMode?: string;
  deliveryTerms?: string;
  shippingDateRequested?: string;
  shippingDateConfirmed?: string;
  receiptDateConfirmed?: string;
      customerReference?: string;
  deliveryName?: string;
  deliveryPostalAddress?: string;
  returnLotId?: string;
  reservation?: number;
  autoBatchReservation?: boolean;
  sameBatchSelection?: boolean;
  scrap?: boolean;
  taxGroup?: string;
  taxItemGroup?: string;
  ledgerDimension?: number;
  ledgerDimensionDisplay?: string;
  salesGroup?: string;
  createdAt?: string;
  defaultDimension?: number;
  financialTag?: number;
  intrastatCommodity?: number;
}
export interface SalesUnit {
  symbol: string;
}
export interface SalesItem {
  itemNumber: string;
  name: string;
  unit?: string;
  unitPrice?: number;
  itemType?: string;
}
export interface InventoryDimensionOption {
  id: string;
  code: string;
  name: string;
  siteId?: string;
  [key: string]: unknown;
}
export interface SalesOrderDeliveryAddressOption {
  value: string;
  label: string;
  address: string;
  isPrimary: boolean;
  postalAddress: LogisticsPostalAddress;
}
export interface CreatedDeliveryAddress {
  id: string;
  description: string;
  address: string;
}
export interface SalesOrderTotals {
  currencyCode: string;
  grossAmount: number;
  lineDiscount: number;
  multiLineDiscount: number;
  totalDiscount: number;
  subtotal: number;
  totalCharges: number;
  salesTax: number;
  invoiceAmount: number;
  quantity: number;
  costValue: number;
}
function unwrap<T>(response: ApiResponse<T>): T {
  if (!response.success || response.data == null)
    throw new Error(response.message || 'Unable to load sales order data.');
  return response.data;
}
export const salesOrderLinesApi = {
  async createDeliveryAddress(orderId: string, lineId: string | undefined, address: LogisticsPostalAddress) {
    const response = await apiClient.post<ApiResponse<CreatedDeliveryAddress>>(
      `/v1/SalesTable/${encodeURIComponent(orderId)}/delivery-addresses`,
      {
        lineId: lineId ?? null,
        address: {
          id: '', location: 0, locationId: address.locationId ?? '',
          description: address.description.trim(), address: '', primary: false,
          street: address.street ?? '', city: address.city ?? '', state: address.state ?? '',
          zipCode: address.zipCode ?? '', county: address.county ?? '',
          countryRegionId: address.countryRegionId, districtName: address.district ?? '',
          validFrom: address.validFrom || null, validTo: address.validTo || null, roles: [],
        },
      }
    );
    return unwrap(response.data);
  },
  async deliveryAddresses(orderId: string, signal?: AbortSignal) {
    return unwrap(
      (
        await apiClient.get<ApiResponse<SalesOrderDeliveryAddressOption[]>>(
          `/v1/SalesTable/${encodeURIComponent(orderId)}/delivery-addresses`,
          { signal }
        )
      ).data
    );
  },
  async totals(id: string, signal?: AbortSignal) {
    return unwrap(
      (
        await apiClient.get<ApiResponse<SalesOrderTotals>>(
          `/v1/SalesTable/${encodeURIComponent(id)}/totals`,
          { signal }
        )
      ).data
    );
  },
  async inventoryDimensions(signal?: AbortSignal) {
    return unwrap(
      (
        await apiClient.get<
          ApiResponse<{
            sites: InventoryDimensionOption[];
            warehouses: InventoryDimensionOption[];
          }>
        >('/v1/SalesTable/inventory-dimensions', { signal })
      ).data
    );
  },
  async taxGroups(signal?: AbortSignal) {
    return unwrap(
      (
        await apiClient.get<
          ApiResponse<{
            salesTaxGroups: InventoryDimensionOption[];
            itemSalesTaxGroups: InventoryDimensionOption[];
          }>
        >('/v1/SalesTable/tax-groups', { signal })
      ).data
    );
  },
  async ledgerDimensions({
    pageNumber,
    pageSize,
    search,
    signal,
  }: {
    pageNumber: number;
    pageSize: number;
    search: string;
    signal?: AbortSignal;
  }) {
    return unwrap(
      (
        await apiClient.get<ApiResponse<{
          data: InventoryDimensionOption[];
          pageNumber: number;
          totalPages: number;
          totalRecords: number;
        }>>(
          '/v1/SalesTable/ledger-dimensions',
          { params: { pageNumber, pageSize, search }, signal }
        )
      ).data
    );
  },
  async returnLots({
    itemNumber,
    pageNumber,
    pageSize,
    search,
    signal,
  }: {
    itemNumber: string;
    pageNumber: number;
    pageSize: number;
    search: string;
    signal?: AbortSignal;
  }) {
    return unwrap(
      (
        await apiClient.get<ApiResponse<{
          data: InventoryDimensionOption[];
          pageNumber: number;
          totalPages: number;
          totalRecords: number;
        }>>(
          '/v1/SalesTable/return-lots',
          { params: { itemNumber, pageNumber, pageSize, search }, signal }
        )
      ).data
    );
  },
  async batchNumbers({
    itemNumber, pageNumber, pageSize, search, signal,
  }: { itemNumber: string; pageNumber: number; pageSize: number; search: string; signal?: AbortSignal }) {
    return unwrap((await apiClient.get<ApiResponse<{
      data: InventoryDimensionOption[]; pageNumber: number; totalPages: number; totalRecords: number;
    }>>('/v1/SalesTable/batch-numbers', { params: { itemNumber, pageNumber, pageSize, search }, signal })).data);
  },
  async serialNumbers({
    itemNumber, pageNumber, pageSize, search, signal,
  }: { itemNumber: string; pageNumber: number; pageSize: number; search: string; signal?: AbortSignal }) {
    return unwrap((await apiClient.get<ApiResponse<{
      data: InventoryDimensionOption[]; pageNumber: number; totalPages: number; totalRecords: number;
    }>>('/v1/SalesTable/serial-numbers', { params: { itemNumber, pageNumber, pageSize, search }, signal })).data);
  },
  async list(id: string, signal?: AbortSignal) {
    return unwrap(
      (
        await apiClient.get<ApiResponse<SalesOrderLineRecord[]>>(
          `/v1/SalesTable/${encodeURIComponent(id)}/lines`,
          { signal }
        )
      ).data
    );
  },
  async add(
    id: string,
    input: {
      itemNumber: string;
      quantity: number;
      unitPrice: number;
      priceUnit?: number;
      usePriceAgreement?: boolean;
      description?: string;
      unit?: string;
      deliveryDate?: string;
      salesCategory?: number;
      lineType?: number;
      deliveryType?: number;
      customerLineNumber?: number;
      inventSiteId?: string;
      inventLocationId?: string;
      configId?: string;
      inventSizeId?: string;
      inventColorId?: string;
      inventStyleId?: string;
      inventVersionId?: string;
      batchNumber?: string;
      serialNumber?: string;
      taxGroup?: string;
      taxItemGroup?: string;
      returnLotId?: string;
      reservation?: number;
      autoBatchReservation?: boolean;
      sameBatchSelection?: boolean;
      scrap?: boolean;
      ledgerDimension?: number;
      salesGroup?: string;
      lineDiscount?: number;
      lineDiscountPercent?: number;
    }
  ) {
    return unwrap(
      (
        await apiClient.post<ApiResponse<SalesOrderLineRecord>>(
          `/v1/SalesTable/${encodeURIComponent(id)}/lines`,
          input
        )
      ).data
    );
  },
  async update(id: string, line: SalesOrderLineRecord) {
    return unwrap(
      (
        await apiClient.put<ApiResponse<SalesOrderLineRecord>>(
          `/v1/SalesTable/${encodeURIComponent(id)}/lines/${encodeURIComponent(line.id)}`,
          {
            ...line,
            inventSiteId: line.site,
            inventLocationId: line.warehouse,
          }
        )
      ).data
    );
  },
  async remove(id: string, lineId: string) {
    return unwrap(
      (
        await apiClient.delete<ApiResponse<{ deleted: boolean }>>(
          `/v1/SalesTable/${encodeURIComponent(id)}/lines/${encodeURIComponent(lineId)}`
        )
      ).data
    );
  },
  async units({
    signal,
    ...params
  }: {
    pageNumber: number;
    pageSize: number;
    search: string;
    signal?: AbortSignal;
  }) {
    return unwrap(
      (
        await apiClient.get<
          ApiResponse<{
            data: SalesUnit[];
            pageNumber: number;
            totalPages: number;
            totalRecords: number;
          }>
        >('/v1/SalesTable/units', { params, signal })
      ).data
    );
  },
  async items({
    signal,
    ...params
  }: {
    pageNumber: number;
    pageSize: number;
    search: string;
    signal?: AbortSignal;
  }) {
    return unwrap(
      (
        await apiClient.get<
          ApiResponse<{
            data: SalesItem[];
            pageNumber: number;
            totalPages: number;
            totalRecords: number;
          }>
        >('/v1/SalesTable/items', { params, signal })
      ).data
    );
  },
};
