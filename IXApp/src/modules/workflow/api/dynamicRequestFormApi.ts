import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface DynamicRequestOptionFeature { requireFileUpload: boolean; sendAlertMessage: boolean; alertMessage: string; performerIds: number[]; showOtherControls: boolean; visibleControlIds: number[] }
export interface DynamicRequestOption { optionId: number; value: string; label: string; labelAlias?: string | null; score: number; sortOrder: number; featureConfiguration?: DynamicRequestOptionFeature }
export interface DynamicRequestValidation { validationId: number; type: string; expression: string | null; operator: string | null; value: string | null; mask: string | null; errorMessage: string; errorMessageAlias?: string | null; severity: string; sortOrder: number }
export interface DynamicRequestCondition { sourceControlId: number; operator: string; value: string }
export interface DynamicRequestControl { requestControlId: number; controlId: number; code: string; label: string; labelAr: string | null; labelColor: string | null; controlType: string; referenceType?: string | null; sortOrder: number; columnSpan?: number; score: number; required: boolean; readOnly: boolean; uniqueKey: boolean; usedAsCriteria: boolean; defaultValue: string | null; visibilityCondition: DynamicRequestCondition | null; options: DynamicRequestOption[]; validations: DynamicRequestValidation[] }
export interface DynamicRequestFormDefinition { processId: number; processName: string; processDescription: string | null; mandatoryDocuments?: boolean; controls: DynamicRequestControl[] }
export interface DynamicRequestSubmit { processId: number; values: Array<{ requestControlId: number; value: string }>; optionFeatureValues: Array<{ optionId: number; fileValue: string }> }
export interface DynamicRequestUpload { file: File; requestControlId: number | null; optionId: number | null }
export interface DynamicRequestAttachmentOwner { requestControlId: number; optionId: number | null; detailRecId: number }
export interface DynamicRequestSubmitResult { startingStepId?: number; assignmentIds?: number[]; requestId: number; code: string | null; score: number; attachmentOwners: DynamicRequestAttachmentOwner[] }
export interface DynamicRequestValidationError { requestControlId: number; controlName: string; errorMessage: string; severity: string }
export interface DynamicRequestValidationResult { success: boolean; errors: DynamicRequestValidationError[] }
export interface DynamicRequestLookupPage { data: DynamicRequestOption[]; pageNumber: number; totalPages: number; totalRecords: number }

const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null) throw new ApiError(response.message || 'The dynamic request response did not contain data.', 500);
  return response.data;
};

export const dynamicRequestFormApi = {
  async validate(submission: DynamicRequestSubmit, signal?: AbortSignal): Promise<DynamicRequestValidationResult> {
    const response = await apiClient.post<DynamicRequestValidationResult>('/v1/WfRequest/validate-submission', submission, { signal });
    return response.data;
  },
  async getDefinition(processId: number, signal?: AbortSignal): Promise<DynamicRequestFormDefinition> {
    const response = await apiClient.get<ApiResponse<DynamicRequestFormDefinition>>(`/v1/WfRequest/form-definition/${processId}`, { signal });
    return requireData(response.data);
  },
  async getReferenceOptions(processId: number, requestControlId: number, params: { pageNumber: number; pageSize: number; search: string; signal?: AbortSignal }): Promise<DynamicRequestLookupPage> {
    const response = await apiClient.get<ApiResponse<DynamicRequestLookupPage>>(
      `/v1/WfRequest/form-definition/${processId}/controls/${requestControlId}/options`,
      { params: { pageNumber: params.pageNumber, pageSize: params.pageSize, search: params.search || undefined }, signal: params.signal }
    );
    return requireData(response.data);
  },
  async submit(submission: DynamicRequestSubmit, uploads: DynamicRequestUpload[] = []): Promise<DynamicRequestSubmitResult> {
    if (uploads.length > 0) {
      const form = new FormData();
      form.append('submission', JSON.stringify({ ...submission, uploadTargets: uploads.map(({ requestControlId, optionId }) => ({ requestControlId, optionId })) }));
      for (const upload of uploads) form.append('files', upload.file);
      const response = await apiClient.post<ApiResponse<DynamicRequestSubmitResult>>('/v1/WfRequest/submit-with-files', form, {
        headers: { 'Content-Type': 'multipart/form-data' },
      });
      return requireData(response.data);
    }
    const response = await apiClient.post<ApiResponse<DynamicRequestSubmitResult>>('/v1/WfRequest/submit', submission);
    return requireData(response.data);
  },
};
