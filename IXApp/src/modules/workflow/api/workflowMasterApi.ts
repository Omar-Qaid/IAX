import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface WorkflowMasterDto {
  recId: number;
  code: string | null;
  name: string | null;
  description: string | null;
  sortOrder: number;
  isActive: boolean;
  rowVersion: string | null;
  recVersion: number;
  dataAreaId: string;
}

export type WorkflowMasterRecord<TDto extends WorkflowMasterDto = WorkflowMasterDto> = TDto & {
  id: string;
};

export const createEmptyWorkflowMaster = <TDto extends WorkflowMasterDto>(
  extras: Omit<
    TDto,
    | 'recId'
    | 'code'
    | 'name'
    | 'description'
    | 'sortOrder'
    | 'isActive'
    | 'rowVersion'
    | 'recVersion'
    | 'dataAreaId'
  >
): WorkflowMasterRecord<TDto> =>
  ({
    id: `new-${crypto.randomUUID()}`,
    recId: 0,
    code: null,
    name: '',
    description: null,
    sortOrder: 0,
    isActive: true,
    rowVersion: null,
    recVersion: 1,
    dataAreaId: 'dat',
    ...extras,
  }) as WorkflowMasterRecord<TDto>;

const requireData = <T>(response: ApiResponse<T>, resourceName: string): T => {
  if (!response.success || response.data == null) {
    throw new ApiError(
      response.message || `The ${resourceName} response did not contain data.`,
      500
    );
  }
  return response.data;
};

export const createWorkflowMasterApi = <TDto extends WorkflowMasterDto>(
  endpoint: string,
  resourceName: string
) => {
  const toRecord = (dto: TDto): WorkflowMasterRecord<TDto> => ({ ...dto, id: String(dto.recId) });
  const toDto = ({ id: _id, ...record }: WorkflowMasterRecord<TDto>): TDto =>
    ({
      ...record,
      code: record.code?.trim() || null,
      name: record.name?.trim() || null,
      description: record.description?.trim() || null,
    }) as unknown as TDto;

  return {
    async list(signal?: AbortSignal): Promise<WorkflowMasterRecord<TDto>[]> {
      const response = await apiClient.get<ApiResponse<TDto[]>>(endpoint, { signal });
      return requireData(response.data, resourceName).map(toRecord);
    },
    async create(record: WorkflowMasterRecord<TDto>): Promise<WorkflowMasterRecord<TDto>> {
      const response = await apiClient.post<ApiResponse<TDto>>(endpoint, toDto(record));
      return toRecord(requireData(response.data, resourceName));
    },
    async update(record: WorkflowMasterRecord<TDto>): Promise<WorkflowMasterRecord<TDto>> {
      const response = await apiClient.put<ApiResponse<TDto>>(
        `${endpoint}/${record.recId}`,
        toDto(record)
      );
      return toRecord(requireData(response.data, resourceName));
    },
    async delete(record: WorkflowMasterRecord<TDto>): Promise<void> {
      const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
      requireData(response.data, resourceName);
    },
  };
};
