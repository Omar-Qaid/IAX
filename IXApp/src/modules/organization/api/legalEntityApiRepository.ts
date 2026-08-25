import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type {
  LegalEntity,
  LegalEntityRecord,
  LegalEntityRepository,
} from '../types/legalEntityTypes';
import { saveLegalEntityImageAttachments } from './legalEntityImageAttachments';

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
  logoFile: _logoFile,
  reportLogoFile: _reportLogoFile,
  ...entity
}: LegalEntityRecord): LegalEntity => ({
  ...entity,
  // Images are managed as document attachments. The API preserves legacy byte[]
  // columns while these null values prevent string-to-byte[] mapping failures.
  logo: null,
  reportLogo: null,
});
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
    const saved = toRecord(requireData(response.data));
    await saveLegalEntityImageAttachments(entity, saved);
    return saved;
  },
  async update(entity) {
    const response = await apiClient.put<ApiResponse<LegalEntity>>(
      `${endpoint}/${entity.recId}`,
      toDto(entity)
    );
    const saved = toRecord(requireData(response.data));
    await saveLegalEntityImageAttachments(entity, saved);
    return saved;
  },
  async delete(entity) {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${entity.recId}`);
    requireData(response.data);
  },
};
