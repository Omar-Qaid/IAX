import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface SalesOrderListRecord {
  id: string;
  recId: number;
  salesId: string;
  customerAccount: string;
  customerName: string;
  invoiceAccount: string;
  customerGroup: string;
  currencyCode: string;
  salesStatus: string;
  documentStatus: string;
  deliveryDate: string;
  shippingDateRequested: string;
  orderTotal: number;
  customerReference: string;
  deliveryMode: string;
  paymentTerms: string;
}

type SalesOrderListDto = Omit<SalesOrderListRecord, 'id'>;

export interface SalesOrderQuickCreateInput {
  customerAccount: string;
  oneTimeCustomer: boolean;
  contact?: string;
  deliveryName?: string;
  deliveryPostalAddress?: number;
  customerReference?: string;
  invoiceAccount?: string;
  currencyCode?: string;
  paymentTerms?: string;
  paymentMethod?: string;
  salesName?: string;
  salesGroup?: string;
  inventSiteId?: string;
  inventLocationId?: string;
  customerRequisitionNumber?: string;
  intercompany?: boolean;
  intercompanyCompanyId?: string;
  requestedReceiptDate?: string;
  requestedShipDate?: string;
  deliveryDateControlType?: number;
  confirmDates?: boolean;
  deliveryMode?: string;
  deliveryTerms?: string;
}

const toRecord = (order: SalesOrderListDto): SalesOrderListRecord => ({
  ...order,
  id: String(order.recId),
});

export const salesOrderListApi = {
  async list(signal?: AbortSignal): Promise<SalesOrderListRecord[]> {
    const response = await apiClient.get<ApiResponse<SalesOrderListDto[]>>('/v1/SalesTable/list', { signal });
    if (!response.data.success || !Array.isArray(response.data.data))
      throw new ApiError(response.data.message || 'The sales-order list response did not contain data.', 500);
    return response.data.data.map(toRecord);
  },

  async create(input: SalesOrderQuickCreateInput): Promise<SalesOrderListRecord> {
    const response = await apiClient.post<ApiResponse<SalesOrderListDto>>('/v1/SalesTable/quick-create', input);
    if (!response.data.success || !response.data.data)
      throw new ApiError(response.data.message || 'The sales order could not be created.', 500);
    return toRecord(response.data.data);
  },
};
