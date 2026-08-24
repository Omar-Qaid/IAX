import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface WfRequestDto {
  recId: number;
  code: string | null;
  name: string | null;
  description: string | null;
  requestDate: string;
  processId: number;
  employeeId: number | null;
  requestDetails: string | null;
  isFinished: boolean;
  finishedDate: string | null;
  isStopped: boolean;
  stoppedDate: string | null;
  score: number;
  progress: number;
  notes: string | null;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}

export interface WfRequestRecord extends WfRequestDto {
  id: string;
}

export interface MailRequestFieldDto {
  detailId: number;
  controlId: number | null;
  controlDataId: number | null;
  label: string;
  labelAr: string;
  value: string;
  valueAr: string;
  valueEn: string;
  controlType: string;
  controlOrder: number;
}

export interface MailTrackingEntryDto {
  assignmentId: number;
  title: string;
  stage: string;
  responsible: string;
  action: string;
  date: string;
  notes: string;
  isCurrent: boolean;
  isCompleted: boolean;
}

export interface MailRequestDetailsDto {
  requestId: number;
  processName: string;
  status: string;
  requestDate: string;
  employeeName: string;
  employeeNumber: string;
  transactionType: string;
  transactionTime: string;
  transactionEndTime: string | null;
  responsibleEmployee: string | null;
  fields: MailRequestFieldDto[];
  history: MailTrackingEntryDto[];
}

const endpoint = '/v1/WfRequest';

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(response.message || 'The workflow-request response did not contain data.', 500);
  }
  return response.data;
};

const toRecord = (dto: WfRequestDto): WfRequestRecord => ({ ...dto, id: String(dto.recId) });
const toDto = ({ id: _id, ...record }: WfRequestRecord): WfRequestDto => ({
  ...record,
  code: record.code?.trim() || null,
  name: record.name?.trim() || null,
  description: record.description?.trim() || null,
  requestDetails: record.requestDetails?.trim() || null,
  notes: record.notes?.trim() || null,
});

export const wfRequestApi = {
  async list(signal?: AbortSignal): Promise<WfRequestRecord[]> {
    const response = await apiClient.get<ApiResponse<WfRequestDto[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async mailDetails(requestId: number, signal?: AbortSignal): Promise<MailRequestDetailsDto> {
    const response = await apiClient.get<ApiResponse<MailRequestDetailsDto>>(
      `${endpoint}/${requestId}/mail-details`,
      { signal }
    );
    return requireData(response.data);
  },
  async create(record: WfRequestRecord): Promise<WfRequestRecord> {
    const response = await apiClient.post<ApiResponse<WfRequestDto>>(endpoint, toDto(record));
    return toRecord(requireData(response.data));
  },
  async update(record: WfRequestRecord): Promise<WfRequestRecord> {
    const response = await apiClient.put<ApiResponse<WfRequestDto>>(
      `${endpoint}/${record.recId}`,
      toDto(record)
    );
    return toRecord(requireData(response.data));
  },
  async delete(record: WfRequestRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
};
