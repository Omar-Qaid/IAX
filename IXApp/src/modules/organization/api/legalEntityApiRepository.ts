import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type {
  LegalEntity,
  LegalEntityRecord,
  LegalEntityRepository,
} from '../types/legalEntityTypes';

const endpoint = '/v1/CompanyInfo';
const toRecord = (entity: LegalEntity): LegalEntityRecord => ({
  ...entity,
  id: String(entity.recId),
});
const toDto = ({
  id: _id,
  inHierarchy: _inHierarchy,
  useForFinancialConsolidation: _useForFinancialConsolidation,
  useForFinancialElimination: _useForFinancialElimination,
  fullName: _fullName,
  ...entity
}: LegalEntityRecord): LegalEntity => entity;
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(response.message || 'The legal-entity response did not contain data.', 500);
  return response.data;
};

export const legalEntityApiRepository: LegalEntityRepository = {
  async list(signal) {
    const response = await apiClient.get<ApiResponse<LegalEntity[]>>(endpoint, { signal });
    return requireData(response.data).map(toRecord);
  },
  async create(entity) {
    const response = await apiClient.post<ApiResponse<LegalEntity>>(endpoint, toDto(entity));
    return toRecord(requireData(response.data));
  },
  async update(entity) {
    const response = await apiClient.put<ApiResponse<LegalEntity>>(
      `${endpoint}/${entity.recId}`,
      toDto(entity)
    );
    return toRecord(requireData(response.data));
  },
  async delete(entity) {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${entity.recId}`);
    requireData(response.data);
  },
};
