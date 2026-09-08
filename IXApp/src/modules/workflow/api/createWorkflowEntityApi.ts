import { createEntityApi, type EntityApiConfig } from '@core/api/createEntityApi';
import { listWorkflowProcessRows } from './workflowProcessList';

export function createWorkflowEntityApi<TDto, TRecord extends { recId: number }>(
  config: EntityApiConfig<TDto, TRecord> & { processField: string }
) {
  const api = createEntityApi(config);
  return {
    ...api,
    async list(signal?: AbortSignal, processId?: number): Promise<TRecord[]> {
      if (processId === undefined) return api.list(signal);
      const rows = await listWorkflowProcessRows<TDto>(
        config.endpoint,
        config.processField,
        processId,
        signal
      );
      return rows.map(config.toRecord);
    },
  };
}
