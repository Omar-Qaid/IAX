import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export type SysJobScheduleType = 0 | 1 | 2 | 3;
export type SysJobStatus = 0 | 1 | 2 | 3;
export type SysJobExecutionStatus = 0 | 1 | 2 | 3 | 4;

export interface SysBackgroundJobRecord {
  id: string;
  recId: number;
  caption: string;
  jobKey: string;
  description: string | null;
  tenantId: string | null;
  dataAreaId: string;
  scheduleType: SysJobScheduleType;
  recurrenceData: string | null;
  startDateTime: string | null;
  startDateTimeTzId: number | null;
  startDate: string | null;
  startTime: number | null;
  origStartDateTime: string | null;
  origStartDateTimeTzId: number | null;
  endDateTime: string | null;
  endDateTimeTzId: number | null;
  canceledBy: string | null;
  dataPartition: string | null;
  finishing: number | null;
  logLevel: number | null;
  runtimeJob: number | null;
  status: SysJobStatus;
  isEnabled: boolean;
  preventOverlap: boolean;
  schedulingPriority: number;
  schedulingPriorityIsOverridden: number | null;
  critical: number | null;
  monitoringCategory: number | null;
  managed: number | null;
  executingBy: string | null;
  activePeriod: string | null;
  batchGroup: string | null;
  emitBusinessEvent: number | null;
  hasAlert: boolean;
  progress: number;
  recurrenceCount: number;
  maxRetryCount: number;
  retryDelaySeconds: number;
  timeoutSeconds: number;
  payloadJson: string | null;
  runCount: number;
  lastStatus: SysJobExecutionStatus | null;
  lastError: string | null;
  createdAt: string | null;
  createdBy: string | null;
}

export interface SysBackgroundJobExecutionRecord {
  recId: number;
  jobId: number;
  jobCaption: string | null;
  attempt: number;
  trigger: number;
  triggeredByUserId: string | null;
  status: SysJobExecutionStatus;
  startedAt: string | null;
  completedAt: string | null;
  durationMs: number | null;
  output: string | null;
  errorMessage: string | null;
  serverName: string | null;
  createdAt: string;
  alertsProcessed: number | null;
  batchCreatedBy: string | null;
  canceledBy: string | null;
  caption: string | null;
  dataPartition: string | null;
  endDateTimeTzId: number | null;
  finishing: number | null;
  origStartDateTime: string | null;
  origStartDateTimeTzId: number | null;
  startDateTimeTzId: number | null;
  executedBy: string | null;
  runtimeJob: number | null;
  batchGroup: string | null;
  groupSchedulingPriority: number | null;
  jobSchedulingPriority: number | null;
  jobSchedulingPriorityIsOverridden: number | null;
}

type JobDto = Omit<SysBackgroundJobRecord, 'id'>;
export interface SysBackgroundJobTaskRecord {
  maxRetryCount?: number;
  retryDelaySeconds?: number;
  recId: number;
  name: string;
  serviceKey: string;
  payloadJson: string | null;
  executionOrder: number;
  dependsOnTaskId: number | null;
  isEnabled: boolean;
}
const endpoint = '/v1/SysBackgroundJob';
const requireData = <T>(response: ApiResponse<T>): T => {
  if (!response.success || response.data == null)
    throw new ApiError(
      response.message || 'The background-job response did not contain data.',
      500
    );
  return response.data;
};
const decodeRecurrence = (value: string, type: SysJobScheduleType): string => {
  const decoded = atob(value);
  if (type === 2 && decoded.length === 4 && !/^[0-9]+$/.test(decoded)) {
    const bytes = Uint8Array.from(decoded, (character) => character.charCodeAt(0));
    return String(new DataView(bytes.buffer).getInt32(0, true));
  }
  return decoded;
};
const toRecord = (dto: JobDto): SysBackgroundJobRecord => ({
  ...dto,
  recurrenceData: dto.recurrenceData
    ? decodeRecurrence(dto.recurrenceData, dto.scheduleType)
    : null,
  id: String(dto.recId),
});

const toSchedulePayload = (record: SysBackgroundJobRecord) => ({
  caption: record.caption.trim(),
  description: record.description?.trim() || null,
  scheduleType: record.scheduleType,
  recurrenceData: record.recurrenceData?.trim() ? btoa(record.recurrenceData.trim()) : null,
  startDateTime: record.startDateTime,
  startDateTimeTzId: record.startDateTimeTzId,
  startDate: record.startDate,
  startTime: record.startTime,
  isEnabled: record.isEnabled,
  preventOverlap: record.preventOverlap,
  schedulingPriority: record.schedulingPriority,
  activePeriod: record.activePeriod,
  batchGroup: record.batchGroup,
  critical: record.critical ?? 0,
  monitoringCategory: record.monitoringCategory ?? 0,
  logLevel: record.logLevel ?? 0,
  managed: record.managed ?? 0,
  emitBusinessEvent: record.emitBusinessEvent ?? 0,
  maxRetryCount: record.maxRetryCount,
  retryDelaySeconds: record.retryDelaySeconds,
  timeoutSeconds: record.timeoutSeconds,
  payloadJson: record.payloadJson?.trim() || null,
});

export const sysBackgroundJobApi = {
  async services(signal?: AbortSignal): Promise<{ serviceKey: string; name: string }[]> {
    return requireData(
      (
        await apiClient.get<ApiResponse<{ serviceKey: string; name: string }[]>>(
          `${endpoint}/services`,
          { signal }
        )
      ).data
    );
  },
  async taskHistory(
    jobId: number,
    signal?: AbortSignal
  ): Promise<(SysBackgroundJobExecutionRecord & { taskId: number; taskName: string })[]> {
    return requireData(
      (
        await apiClient.get<
          ApiResponse<(SysBackgroundJobExecutionRecord & { taskId: number; taskName: string })[]>
        >(`${endpoint}/${jobId}/tasks/history`, { signal })
      ).data
    );
  },
  async tasks(jobId: number, signal?: AbortSignal): Promise<SysBackgroundJobTaskRecord[]> {
    return requireData(
      (
        await apiClient.get<ApiResponse<SysBackgroundJobTaskRecord[]>>(
          `${endpoint}/${jobId}/tasks`,
          { signal }
        )
      ).data
    );
  },
  async saveTasks(
    jobId: number,
    tasks: SysBackgroundJobTaskRecord[]
  ): Promise<SysBackgroundJobTaskRecord[]> {
    return requireData(
      (
        await apiClient.put<ApiResponse<SysBackgroundJobTaskRecord[]>>(
          `${endpoint}/${jobId}/tasks`,
          tasks
        )
      ).data
    );
  },
  async list(signal?: AbortSignal): Promise<SysBackgroundJobRecord[]> {
    const response = await apiClient.get<ApiResponse<JobDto[]>>(endpoint, {
      signal,
      params: { pageSize: 500 },
    });
    return requireData(response.data).map(toRecord);
  },
  async handlers(signal?: AbortSignal): Promise<string[]> {
    const response = await apiClient.get<ApiResponse<string[]>>(`${endpoint}/handlers`, { signal });
    return requireData(response.data);
  },
  async create(record: SysBackgroundJobRecord): Promise<SysBackgroundJobRecord> {
    const response = await apiClient.post<ApiResponse<JobDto>>(endpoint, {
      ...toSchedulePayload(record),
      jobKey: record.jobKey,
    });
    return toRecord(requireData(response.data));
  },
  async update(record: SysBackgroundJobRecord): Promise<SysBackgroundJobRecord> {
    const response = await apiClient.put<ApiResponse<JobDto>>(
      `${endpoint}/${record.recId}/schedule`,
      toSchedulePayload(record)
    );
    return toRecord(requireData(response.data));
  },
  async remove(record: SysBackgroundJobRecord): Promise<void> {
    const response = await apiClient.delete<ApiResponse<boolean>>(`${endpoint}/${record.recId}`);
    requireData(response.data);
  },
  async trigger(id: number): Promise<void> {
    const response = await apiClient.post<ApiResponse<number>>(`${endpoint}/${id}/trigger`);
    requireData(response.data);
  },
  async pause(id: number): Promise<void> {
    const response = await apiClient.put<ApiResponse<boolean>>(`${endpoint}/${id}/pause`);
    requireData(response.data);
  },
  async resume(id: number): Promise<void> {
    const response = await apiClient.put<ApiResponse<boolean>>(`${endpoint}/${id}/resume`);
    requireData(response.data);
  },
  async cancel(id: number): Promise<void> {
    const response = await apiClient.put<ApiResponse<boolean>>(`${endpoint}/${id}/cancel`);
    requireData(response.data);
  },
  async executions(id: number, signal?: AbortSignal): Promise<SysBackgroundJobExecutionRecord[]> {
    const response = await apiClient.get<ApiResponse<SysBackgroundJobExecutionRecord[]>>(
      `${endpoint}/${id}/executions`,
      { signal, params: { pageSize: 100 } }
    );
    return requireData(response.data);
  },
};
