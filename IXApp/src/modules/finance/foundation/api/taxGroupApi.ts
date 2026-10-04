import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface TaxGroupLineDto {
  recId: number; taxGroup: string | null; taxCode: string | null; taxExemptCode: string | null;
  exemptTax: number; useTax: number; intracomVat: number; reverseCharge_W: number;
  taxCodeName: string | null; taxValue: number | null;
}
export interface TaxGroupLine extends TaxGroupLineDto { id: string }
export interface TaxGroupDto {
  recId: number; taxGroup: string; taxGroupName: string; taxGroupSetup: number; source: number; taxGroupRounding: number;
  taxReverseOnCashDisc: number; euTrade_W: number; mandatorySalesDate_W: number; fillSalesDate_W: number;
  fillVatDueDatePeriodNumber: number; fillVatDueDate_W: number; fillVatDueDateBasedOn: number; fillVatDueDatePeriod: number;
  taxPrintDetail: number; lines: TaxGroupLineDto[];
}
export interface TaxGroupRecord extends Omit<TaxGroupDto, 'lines'> { id: string; lines: TaxGroupLine[] }
const endpoint = '/v1/TaxGroup';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) throw new ApiError(response.message || 'The sales tax group response did not contain data.', 500);
  return response.data;
};
const toLine = (line: TaxGroupLineDto): TaxGroupLine => ({ ...line, id: line.recId ? String(line.recId) : `new-${crypto.randomUUID()}` });
const toRecord = (dto: TaxGroupDto): TaxGroupRecord => ({ ...dto, id: String(dto.recId), lines: (dto.lines ?? []).map(toLine) });
const toDto = ({ id: _id, lines, ...record }: TaxGroupRecord): TaxGroupDto => ({
  ...record, taxGroup: record.taxGroup.trim().toUpperCase(), taxGroupName: record.taxGroupName.trim(),
  lines: lines.map(({ id: _lineId, ...line }) => ({ ...line, taxGroup: record.taxGroup.trim().toUpperCase(), taxCode: line.taxCode?.trim().toUpperCase() ?? '', taxExemptCode: line.taxExemptCode?.trim().toUpperCase() || 'NONE' })),
});
export const taxGroupApi = {
  async list(signal?: AbortSignal): Promise<TaxGroupRecord[]> { const response = await apiClient.get<ApiResponse<TaxGroupDto[]>>(endpoint,{signal}); return requireData(response.data).map(toRecord); },
  async create(record: TaxGroupRecord): Promise<TaxGroupRecord> { const response = await apiClient.post<ApiResponse<TaxGroupDto>>(endpoint,toDto(record)); return toRecord(requireData(response.data)); },
  async update(record: TaxGroupRecord): Promise<TaxGroupRecord> { const response = await apiClient.put<ApiResponse<TaxGroupDto>>(`${endpoint}/${record.recId}`,toDto(record)); return toRecord(requireData(response.data)); },
  async delete(record: TaxGroupRecord): Promise<void> { const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`); requireData(response.data); },
};
