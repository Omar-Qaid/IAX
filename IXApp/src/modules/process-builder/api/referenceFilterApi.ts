import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface ReferenceFilterField {
  name: string;
  dataType: 'string' | 'integer' | 'decimal' | 'date' | 'boolean';
  nullable: boolean;
  isForeignKey: boolean;
  operators: string[];
}

export interface ReferenceFilterValuePage {
  data: Array<{ value: string; label: string; labelAlias?: string | null }>;
  pageNumber: number;
  totalPages: number;
  totalRecords: number;
}

const textOperators = ['equals', 'notEquals', 'contains', 'startsWith', 'endsWith', 'isEmpty', 'isNotEmpty'];
const comparableOperators = ['equals', 'notEquals', 'greaterThan', 'greaterThanOrEqual', 'lessThan', 'lessThanOrEqual', 'isEmpty', 'isNotEmpty'];
const equalityOperators = ['equals', 'notEquals', 'isEmpty', 'isNotEmpty'];
const field = (name: string, dataType: ReferenceFilterField['dataType'], nullable = false, isForeignKey = false): ReferenceFilterField => ({
  name,
  dataType,
  nullable,
  isForeignKey,
  operators: isForeignKey || dataType === 'boolean' ? equalityOperators : dataType === 'string' ? textOperators : comparableOperators,
});

export const referenceFilterFieldsFallback = (referenceType: 'Employee' | 'Showroom'): ReferenceFilterField[] => {
  const common = [
    field('CreatedAt', 'date', true), field('CreatedBy', 'string', true),
    field('DataAreaId', 'string'), field('IsActive', 'boolean'), field('IsDeleted', 'boolean'),
    field('LastModifiedAt', 'date', true), field('LastModifiedBy', 'string', true),
    field('OwnerAccountId', 'string', true), field('RecId', 'integer'), field('RecVersion', 'integer'),
  ];
  const specific = referenceType === 'Employee'
    ? [
        field('BirthDate', 'date', true), field('GenderId', 'integer', false, true),
        field('HireDate', 'date', true), field('NationalityId', 'integer', false, true),
        field('OccupationId', 'integer', false, true), field('Person', 'integer', false, true),
        field('PersonnelNumber', 'string'), field('UserId', 'string', true, true),
      ]
    : [field('Party', 'integer', false, true), field('PersonnelNumber', 'string')];
  return [...common, ...specific].sort((left, right) => left.name.localeCompare(right.name));
};

const data = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) throw new ApiError(response.message || 'Reference filter data is unavailable.', 500);
  return response.data;
};

export const referenceFilterApi = {
  async fields(referenceType: 'Employee' | 'Showroom', signal?: AbortSignal): Promise<ReferenceFilterField[]> {
    try {
      const response = await apiClient.get<ApiResponse<ReferenceFilterField[]>>(
        `/v1/WfRequest/reference-filters/${referenceType}/fields`, { signal }
      );
      return data(response.data);
    } catch (error) {
      if (signal?.aborted) throw error;
      return referenceFilterFieldsFallback(referenceType);
    }
  },
  async values(referenceType: 'Employee' | 'Showroom', field: string, params: { pageNumber: number; pageSize: number; search: string; signal?: AbortSignal }): Promise<ReferenceFilterValuePage> {
    const response = await apiClient.get<ApiResponse<ReferenceFilterValuePage>>(
      `/v1/WfRequest/reference-filters/${referenceType}/fields/${encodeURIComponent(field)}/values`,
      { params: { pageNumber: params.pageNumber, pageSize: params.pageSize, search: params.search || undefined }, signal: params.signal }
    );
    return data(response.data);
  },
};
