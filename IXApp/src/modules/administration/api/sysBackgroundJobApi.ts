import { ApiError } from '@core/api/apiError';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export type SysJobScheduleType = 0 | 1 | 2 | 3;
export type SysJobStatus = 0 | 1 | 2 | 3;
export type SysJobExecutionStatus = 0 | 1 | 2 | 3 | 4;

export interface SysBackgroundJobRecord {
  id: string;
  recId: number;
  name: string;
  jobKey: string;
  description: string | null;
  tenantId: string | null;
  scheduleType: SysJobScheduleType;
  cronExpression: string | null;
  intervalSeconds: number | null;
  runAt: string | null;
  nextRunAt: string | null;
  status: SysJobStatus;
  isEnabled: boolean;
  preventOverlap: boolean;
  priority: number;
  maxRetryCount: number;
  retryDelaySeconds: number;
  timeoutSeconds: number;
  payloadJson: string | null;
  runCount: number;
  lastRunAt: string | null;
  lastStatus: SysJobExecutionStatus | null;
  lastError: string | null;
  createdAt: string | null;
  createdBy: string | null;
}

export interface SysBackgroundJobExecutionRecord {
  recId: number;
  jobId: number;
  jobName: string | null;
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
    throw new ApiError(response.message || 'The background-job response did not contain data.', 500);
  return response.data;
};
const toRecord = (dto: JobDto): SysBackgroundJobRecord => ({ ...dto, id: String(dto.recId) });

export const sysBackgroundJobApi = {
  async services(signal?: AbortSignal): Promise<{ serviceKey: string; name: string }[]> {
    return requireData((await apiClient.get<ApiResponse<{ serviceKey: string; name: string }[]>>(`${endpoint}/services`, { signal })).data);
  },
  async taskHistory(jobId: number, signal?: AbortSignal): Promise<(SysBackgroundJobExecutionRecord & { taskId: number; taskName: string })[]> {
    return requireData((await apiClient.get<ApiResponse<(SysBackgroundJobExecutionRecord & { taskId: number; taskName: string })[]>>(`${endpoint}/${jobId}/tasks/history`, { signal })).data);
  },
  async tasks(jobId: number, signal?: AbortSignal): Promise<SysBackgroundJobTaskRecord[]> {
    return requireData((await apiClient.get<ApiResponse<SysBackgroundJobTaskRecord[]>>(`${endpoint}/${jobId}/tasks`, { signal })).data);
  },
  async saveTasks(jobId: number, tasks: SysBackgroundJobTaskRecord[]): Promise<SysBackgroundJobTaskRecord[]> {
    return requireData((await apiClient.put<ApiResponse<SysBackgroundJobTaskRecord[]>>(`${endpoint}/${jobId}/tasks`, tasks)).data);
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
      name: record.name.trim(), jobKey: record.jobKey, description: record.description?.trim() || null,
      scheduleType: record.scheduleType, cronExpression: record.cronExpression?.trim() || null,
      intervalSeconds: record.intervalSeconds, runAt: record.runAt, isEnabled: record.isEnabled,
      preventOverlap: record.preventOverlap, priority: record.priority, maxRetryCount: record.maxRetryCount,
      retryDelaySeconds: record.retryDelaySeconds, timeoutSeconds: record.timeoutSeconds,
      payloadJson: record.payloadJson?.trim() || null,
    });
    return toRecord(requireData(response.data));
  },
  async update(record: SysBackgroundJobRecord): Promise<SysBackgroundJobRecord> {
    const response = await apiClient.put<ApiResponse<JobDto>>(`${endpoint}/${record.recId}/schedule`, {
      name: record.name.trim(),
      scheduleType: record.scheduleType, cronExpression: record.cronExpression?.trim() || null,
      intervalSeconds: record.intervalSeconds, runAt: record.runAt, isEnabled: record.isEnabled,
      preventOverlap: record.preventOverlap, priority: record.priority, maxRetryCount: record.maxRetryCount,
      retryDelaySeconds: record.retryDelaySeconds, timeoutSeconds: record.timeoutSeconds,
      payloadJson: record.payloadJson?.trim() || null, description: record.description?.trim() || null,
    });
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
      `${endpoint}/${id}/executions`, { signal, params: { pageSize: 100 } }
    );
    return requireData(response.data);
  },
};
