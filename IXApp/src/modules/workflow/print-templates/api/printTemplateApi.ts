import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import type { PrintTemplate, PrintTemplateSummary, PublishedPrintTemplate, SavePrintTemplateInput } from '../types/printTemplate.types';

const endpoint = '/v1/print-templates';

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) throw new ApiError(response.message || 'Print template response did not contain data.', 500);
  return response.data;
};

export const printTemplateApi = {
  async listByProcess(processId: number, signal?: AbortSignal): Promise<PrintTemplateSummary[]> {
    const response = await apiClient.get<ApiResponse<PrintTemplateSummary[]>>(`${endpoint}/process/${processId}`, { signal });
    return requireData(response.data);
  },
  async listPublishedByProcess(processId: number, signal?: AbortSignal): Promise<PrintTemplateSummary[]> {
    const response = await apiClient.get<ApiResponse<PrintTemplateSummary[]>>(`${endpoint}/process/${processId}/published`, { signal });
    return requireData(response.data);
  },
  async getPublishedForRequest(requestId: number, templateId: number, signal?: AbortSignal): Promise<PublishedPrintTemplate> {
    const response = await apiClient.get<ApiResponse<PublishedPrintTemplate>>(`${endpoint}/request/${requestId}/template/${templateId}`, { signal });
    return requireData(response.data);
  },
  async get(templateId: number, signal?: AbortSignal): Promise<PrintTemplate> {
    const response = await apiClient.get<ApiResponse<PrintTemplate>>(`${endpoint}/${templateId}`, { signal });
    return requireData(response.data);
  },
  async create(input: SavePrintTemplateInput & { processId: number }): Promise<PrintTemplate> {
    const response = await apiClient.post<ApiResponse<PrintTemplate>>(endpoint, input);
    return requireData(response.data);
  },
  async update(templateId: number, input: SavePrintTemplateInput): Promise<PrintTemplate> {
    const response = await apiClient.put<ApiResponse<PrintTemplate>>(`${endpoint}/${templateId}`, input);
    return requireData(response.data);
  },
  async publish(templateId: number, templateVersionId?: number): Promise<PrintTemplate> {
    const response = await apiClient.post<ApiResponse<PrintTemplate>>(`${endpoint}/${templateId}/publish`, { templateVersionId });
    return requireData(response.data);
  },
  async archive(templateId: number): Promise<PrintTemplate> {
    const response = await apiClient.post<ApiResponse<PrintTemplate>>(`${endpoint}/${templateId}/archive`);
    return requireData(response.data);
  },
  async deleteDraft(templateId: number): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${templateId}`);
    requireData(response.data);
  },
  async validate(templateId: number, signal?: AbortSignal): Promise<{ isValid: boolean; errors: string[] }> {
    const response = await apiClient.get<ApiResponse<{ isValid: boolean; errors: string[] }>>(`${endpoint}/${templateId}/validation`, { signal });
    return requireData(response.data);
  },
};
