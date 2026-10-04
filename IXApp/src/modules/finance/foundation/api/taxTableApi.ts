import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface TaxTableDto {
  recId: number; taxCode: string; taxName: string; taxPeriod: string; taxAccountGroup: string; taxCurrencyCode: string;
  source: number; taxOnTax: string; taxUnit: string; taxBase: number; taxCalcMethod: number; taxLimitBase: number;
  taxIncludeInTax: number; negativeTax: number; unrealizedTax: number; taxAllowLineDiscountOnTaxPerUnit: number;
  taxRoundOff: number; taxRoundOffType: number; roundDeductibleFirst: number; printCode: string; paymentTaxCode: string;
  taxJurisdictionCode: string; taxType_W: number; taxCountryRegionType: number; notEuSalesList: number; excludeFromInvoice: number;
  taxPurchaseTax: number; taxPackagingTax: number; taxWriteSelection: number; reconcileAmountOrigin: number;
  repFieldBaseOutgoing: number; repFieldBaseOutgoingCreditNote: number; repFieldTaxOutgoing: number; repFieldTaxOutgoingCreditNote: number;
  repFieldBaseIncoming: number; repFieldBaseIncomingCreditNote: number; repFieldTaxIncoming: number; repFieldTaxIncomingCreditNote: number;
  repFieldBaseUseTax: number; repFieldBaseUseTaxCreditNote: number; repFieldUseTax: number; repFieldUseTaxCreditNote: number;
  repFieldBaseUseTaxOffset: number; repFieldBaseUseTaxOffsetCreditNote: number; repFieldUseTaxOffset: number; repFieldUseTaxOffsetCreditNote: number;
  repFieldTaxFreeSales: number; repFieldTaxFreeSalesCreditNote: number; repFieldTaxFreeBuy: number; repFieldTaxFreeBuyCreditNote: number;
  taxValue: number;
}
export interface TaxTableRecord extends TaxTableDto { id: string }
const endpoint = '/v1/TaxTable';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) throw new ApiError(response.message || 'The sales tax code response did not contain data.', 500);
  return response.data;
};
const toRecord = (dto: TaxTableDto): TaxTableRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: TaxTableRecord): TaxTableDto => ({ ...record, taxCode: record.taxCode.trim().toUpperCase(), taxName: record.taxName.trim() });
export const taxTableApi = {
  async list(signal?: AbortSignal): Promise<TaxTableRecord[]> { const response = await apiClient.get<ApiResponse<TaxTableDto[]>>(endpoint, { signal }); return requireData(response.data).map(toRecord); },
  async create(record: TaxTableRecord): Promise<TaxTableRecord> { const response = await apiClient.post<ApiResponse<TaxTableDto>>(endpoint, toDto(record)); return toRecord(requireData(response.data)); },
  async update(record: TaxTableRecord): Promise<TaxTableRecord> { const response = await apiClient.put<ApiResponse<TaxTableDto>>(`${endpoint}/${record.recId}`, toDto(record)); return toRecord(requireData(response.data)); },
  async delete(record: TaxTableRecord): Promise<void> { const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`); requireData(response.data); },
};
