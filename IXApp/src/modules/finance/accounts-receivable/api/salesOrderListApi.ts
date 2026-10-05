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
  deliveryTerms: string;
  paymentTerms: string;
  orderDate?: string;
  inventSiteId: string;
  inventLocationId: string;
  salesNameAlias: string;
  salesType: number;
  oneTimeCustomer: boolean;
  email: string;
  phone: string;
  deadline?: string;
  customerRequisitionNumber: string;
  campaignId: string;
  taxGroupId: string;
  pricesIncludeSalesTax: boolean;
  salesGroup: string;
  languageId: string;
  deliveryName: string;
  deliveryPostalAddress: string;
  deliveryAddress: string;
  shippingDateConfirmed?: string;
  receiptDateConfirmed?: string;
  deliveryDateControlType: number;
  mpsFullRunCtpStatus: number;
  blindShipment: boolean;
  residentialDestination: boolean;
  excludeFromMasterPlanning: boolean;
  deliveryReason: string;
  exportReason: string;
  shippingCarrier: string;
  carrierId: string;
  carrierGroup: string;
  brokerId: string;
  transportMode: string;
  carrierService: number;
  paymentMethod: string;
  paymentSchedule: string;
  paymentSpecification: string;
  fixedDueDate?: string;
  paymentTermsBaseDate?: string;
  cashDiscountCode: string;
  discountPercent: number;
  totalDiscountPercent: number;
  fixedExchangeRate: number;
  reportingCurrencyFixedExchangeRate: number;
  priceGroup: string;
  lineDiscountGroup: string;
  multiLineDiscountGroup: string;
  totalDiscountGroup: string;
  chargesGroup: string;
  customerRebateGroup: string;
  customerTmaGroup: string;
  rebateReference: string;
  salesPool: string;
  carrierCustomerAccount: string;
  freightZone: string;
  notes: string;
  intercompanyAutoCreateOrders: boolean;
  intercompanyDirectDelivery: boolean;
  intercompanyOrigin: number;
  intercompanyAllowIndirectCreation: boolean;
  releaseStatus: string;
  reservation: number;
}

export type SalesOrderHeaderInput = Pick<
  SalesOrderListRecord,
  | 'invoiceAccount'
  | 'currencyCode'
  | 'customerReference'
  | 'paymentTerms'
  | 'deliveryMode'
  | 'deliveryTerms'
  | 'deliveryDate'
  | 'shippingDateRequested'
  | 'orderDate'
  | 'inventSiteId'
  | 'inventLocationId'
  | 'salesNameAlias'
  | 'salesType'
  | 'oneTimeCustomer'
  | 'email'
  | 'phone'
  | 'deadline'
  | 'customerRequisitionNumber'
  | 'campaignId'
  | 'taxGroupId'
  | 'pricesIncludeSalesTax'
  | 'salesGroup'
  | 'languageId'
  | 'deliveryName'
  | 'deliveryPostalAddress'
  | 'shippingDateConfirmed'
  | 'receiptDateConfirmed'
  | 'deliveryDateControlType'
  | 'mpsFullRunCtpStatus'
  | 'blindShipment'
  | 'residentialDestination'
  | 'excludeFromMasterPlanning'
  | 'deliveryReason'
  | 'exportReason'
  | 'shippingCarrier'
  | 'carrierId'
  | 'carrierGroup'
  | 'brokerId'
  | 'transportMode'
  | 'carrierService'
  | 'paymentMethod'
  | 'paymentSchedule'
  | 'paymentSpecification'
  | 'fixedDueDate'
  | 'paymentTermsBaseDate'
  | 'cashDiscountCode'
  | 'discountPercent'
  | 'totalDiscountPercent'
  | 'fixedExchangeRate'
  | 'priceGroup'
  | 'lineDiscountGroup'
  | 'multiLineDiscountGroup'
  | 'totalDiscountGroup'
  | 'chargesGroup'
  | 'customerRebateGroup'
  | 'customerTmaGroup'
  | 'rebateReference'
  | 'salesPool'
  | 'carrierCustomerAccount'
  | 'freightZone'
  | 'notes'
  | 'intercompanyAutoCreateOrders'
  | 'intercompanyDirectDelivery'
  | 'intercompanyOrigin'
  | 'intercompanyAllowIndirectCreation'
  | 'reservation'
>;

type SalesOrderListDto = Omit<SalesOrderListRecord, 'id'>;

export interface SalesOrderQuickCreateInput {
  customerAccount: string;
  oneTimeCustomer: boolean;
  contact?: string;
  contactType?: string;
  deliveryName?: string;
  deliveryPostalAddress?: number;
  deliveryPostalAddressId?: string;
  taxGroupId?: string;
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
    const response = await apiClient.get<ApiResponse<SalesOrderListDto[]>>('/v1/SalesTable/list', {
      signal,
    });
    if (!response.data.success || !Array.isArray(response.data.data))
      throw new ApiError(
        response.data.message || 'The sales-order list response did not contain data.',
        500
      );
    return response.data.data.map(toRecord);
  },

  async updateHeader(id: string, input: SalesOrderHeaderInput): Promise<void> {
    const response = await apiClient.put<ApiResponse<unknown>>(
      `/v1/SalesTable/${id}/header`,
      input
    );
    if (!response.data.success)
      throw new ApiError(response.data.message || 'The sales order could not be saved.', 500);
  },

  async cancel(id: string): Promise<void> {
    const response = await apiClient.post<ApiResponse<unknown>>(`/v1/SalesTable/${id}/cancel`);
    if (!response.data.success)
      throw new ApiError(response.data.message || 'The sales order could not be cancelled.', 500);
  },

  async create(input: SalesOrderQuickCreateInput): Promise<SalesOrderListRecord> {
    const response = await apiClient.post<ApiResponse<SalesOrderListDto>>(
      '/v1/SalesTable/quick-create',
      input
    );
    if (!response.data.success || !response.data.data)
      throw new ApiError(response.data.message || 'The sales order could not be created.', 500);
    return toRecord(response.data.data);
  },
};
