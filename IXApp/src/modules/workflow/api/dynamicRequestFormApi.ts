import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface DynamicRequestOptionFeature { requireFileUpload: boolean; sendAlertMessage: boolean; alertMessage: string; performerIds: number[]; showOtherControls: boolean; visibleControlIds: number[] }
export interface DynamicRequestOption { optionId: number; value: string; label: string; score: number; sortOrder: number; featureConfiguration?: DynamicRequestOptionFeature }
export interface DynamicRequestValidation { validationId: number; type: string; expression: string | null; operator: string | null; value: string | null; mask: string | null; errorMessage: string; severity: string; sortOrder: number }
export interface DynamicRequestCondition { sourceControlId: number; operator: string; value: string }
export interface DynamicRequestControl { requestControlId: number; controlId: number; code: string; label: string; labelAr: string | null; labelColor: string | null; controlType: string; sortOrder: number; columnSpan?: number; score: number; required: boolean; readOnly: boolean; uniqueKey: boolean; usedAsCriteria: boolean; defaultValue: string | null; visibilityCondition: DynamicRequestCondition | null; options: DynamicRequestOption[]; validations: DynamicRequestValidation[] }
export interface DynamicRequestFormDefinition { processId: number; processName: string; processDescription: string | null; controls: DynamicRequestControl[] }
export interface DynamicRequestSubmit { processId: number; values: Array<{ requestControlId: number; value: string }>; optionFeatureValues: Array<{ optionId: number; fileValue: string }> }
export interface DynamicRequestAttachmentOwner { requestControlId: number; optionId: number | null; detailRecId: number }
export interface DynamicRequestSubmitResult { requestId: number; code: string | null; score: number; attachmentOwners: DynamicRequestAttachmentOwner[] }

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) throw new ApiError(response.message || 'The dynamic request response did not contain data.', 500);
  return response.data;
};

export const dynamicRequestFormApi = {
  async getDefinition(processId: number, signal?: AbortSignal): Promise<DynamicRequestFormDefinition> {
    const response = await apiClient.get<ApiResponse<DynamicRequestFormDefinition>>(`/v1/WfRequest/form-definition/${processId}`, { signal });
    return requireData(response.data);
  },
  async submit(submission: DynamicRequestSubmit): Promise<DynamicRequestSubmitResult> {
    const response = await apiClient.post<ApiResponse<DynamicRequestSubmitResult>>('/v1/WfRequest/submit', submission);
    return requireData(response.data);
  },
};
