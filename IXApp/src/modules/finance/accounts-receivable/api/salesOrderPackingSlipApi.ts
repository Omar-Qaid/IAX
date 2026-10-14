import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface PackingSlipJournalSummary {
  id: string;
  salesId: string;
  packingSlipId: string;
  version: string;
  deliveryDate: string;
  invoiceDueDate: string;
  terms: string;
  modeOfDelivery: string;
  blindShipment: boolean;
  intercompanyCompanyId: string;
  intercompanyPosted: boolean;
  printed: boolean;
  ownerId: string;
  contractorId: string;
}

export interface PackingSlipJournalLine {
  id: string;
  packingSlipId: string;
  salesId: string;
  originalSalesId: string;
  lineNumber: number;
  itemId: string;
  description: string;
  salesUnit: string;
  quantity: number;
  inventQuantity: number;
  catchWeightQuantity: number;
  siteId: string;
  warehouseId: string;
  batchNumber: string;
  serialNumber: string;
  inventoryTransactionId: string;
  requestedShipDate: string;
  confirmedShipDate: string;
  deliveryType: number;
  amount: number;
  accountingAmount: number;
  fullyMatched: boolean;
  scrap: boolean;
}

export interface PackingSlipJournalDetail {
  header: PackingSlipJournalSummary;
  lines: PackingSlipJournalLine[];
}

export type PackingSlipQuantityMode = 'all' | 'deliverNow' | 'picked';

export interface PackingSlipPostingLine {
  id: string;
  lineNumber: number;
  itemId: string;
  name: string;
  salesCategory: string;
  productNumber: string;
  inventSiteId: string;
  inventLocationId: string;
  wmsLocationId: string;
  catchWeightQuantity: number;
  quantity: number;
  netAmount: number;
}

export interface PackingSlipPostingPreview {
  header: { id: string; salesId: string; customerName: string; currencyCode: string; siteId: string };
  lines: PackingSlipPostingLine[];
}

function unwrap<T>(response: ApiResponse<T>): T {
  if (!response.success || response.data == null)
    throw new Error(response.message || 'The packing slip journal request failed.');
  return response.data;
}

const orderPath = (salesOrderId: string) =>
  `/v1/SalesTable/${encodeURIComponent(salesOrderId)}/packing-slips`;

export const salesOrderPackingSlipApi = {
  async list(salesOrderId: string, signal?: AbortSignal): Promise<PackingSlipJournalSummary[]> {
    return unwrap((await apiClient.get<ApiResponse<PackingSlipJournalSummary[]>>(
      orderPath(salesOrderId), { signal }
    )).data);
  },
  async get(
    salesOrderId: string,
    packingSlipRecId: string,
    signal?: AbortSignal
  ): Promise<PackingSlipJournalDetail> {
    return unwrap((await apiClient.get<ApiResponse<PackingSlipJournalDetail>>(
      `${orderPath(salesOrderId)}/${encodeURIComponent(packingSlipRecId)}`, { signal }
    )).data);
  },
  async postingPreview(
    salesOrderId: string,
    quantityMode: PackingSlipQuantityMode,
    signal?: AbortSignal
  ): Promise<PackingSlipPostingPreview> {
    return unwrap((await apiClient.get<ApiResponse<PackingSlipPostingPreview>>(
      `${orderPath(salesOrderId)}/posting-preview`, { params: { quantityMode }, signal }
    )).data);
  },
  async post(
    salesOrderId: string,
    request: {
      deliveryDate: string;
      quantityMode: PackingSlipQuantityMode;
      posting: boolean;
      requestId: string;
      lines: { salesLineId: string; quantity: number }[];
    }
  ): Promise<PackingSlipJournalSummary> {
    return unwrap((await apiClient.post<ApiResponse<PackingSlipJournalSummary>>(
      orderPath(salesOrderId), request
    )).data);
  },
};
