import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';

export interface ProcessSchedule {
  enabled: boolean;
  frequency: 'daily' | 'weekly' | 'monthly' | 'yearly';
  startsAt: string;
  timeZone: string;
  sourceType: 'employee' | 'showroom' | 'table';
  sourceTable: string;
  sourceRecord: string;
  mappings: { targetControlId: string; sourceField: string }[];
  version?: string | null;
}

export const processBuilderSchedules = {
  async load(processId: string, signal?: AbortSignal) {
    const { data } = await apiClient.get<ApiResponse<ProcessSchedule>>(`/v1/WFProcessScheduled/${processId}`, { signal });
    if (!data.success) throw new Error(data.message || 'Unable to load the schedule.');
    return data.data;
  },
  async save(processId: string, schedule: ProcessSchedule) {
    // DateTime is required by the server even when saving a disabled draft.
    const { data } = await apiClient.put<ApiResponse<ProcessSchedule>>(`/v1/WFProcessScheduled/${processId}`, {
      ...schedule, startsAt: schedule.startsAt || '0001-01-01T00:00:00',
    });
    if (!data.success || !data.data) throw new Error(data.message || 'Unable to save the schedule.');
    return data.data;
  },
};
