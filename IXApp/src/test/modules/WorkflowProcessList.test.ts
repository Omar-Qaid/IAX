import { beforeEach, describe, expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { listWorkflowProcessRows } from '@modules/workflow/api/workflowProcessList';
import { wfVariableApi } from '@modules/workflow/api/wfVariableApi';
import { wfStepApi } from '@modules/workflow/api/wfStepApi';
import { wfActivityApi } from '@modules/workflow/api/wfActivityApi';
import { wfActivityControlApi } from '@modules/workflow/api/wfActivityControlApi';
import { wfActivityControlValidationApi } from '@modules/workflow/api/wfActivityControlValidationApi';
import { wfActivityControlOptionApi } from '@modules/workflow/api/wfActivityControlOptionApi';
import { wfRequestControlApi } from '@modules/workflow/api/wfRequestControlApi';
import { wfRequestControlValidationApi } from '@modules/workflow/api/wfRequestControlValidationApi';
import { wfRequestControlOptionApi } from '@modules/workflow/api/wfRequestControlOptionApi';
import { wfTransitionApi } from '@modules/workflow/api/wfTransitionApi';

vi.mock('@core/api/apiClient', () => ({ apiClient: { get: vi.fn() } }));
const get = vi.mocked(apiClient.get);
beforeEach(() => get.mockReset());

const scopes = [
  [wfVariableApi, 'ProcessId'],
  [wfStepApi, 'ProcessId'],
  [wfActivityApi, 'Step.ProcessId'],
  [wfActivityControlApi, 'Activity.Step.ProcessId'],
  [wfActivityControlValidationApi, 'ActivityControl.Activity.Step.ProcessId'],
  [wfActivityControlOptionApi, 'ActivityControl.Activity.Step.ProcessId'],
  [wfRequestControlApi, 'ProcessId'],
  [wfRequestControlValidationApi, 'RequestControl.ProcessId'],
  [wfRequestControlOptionApi, 'RequestControl.ProcessId'],
  [wfTransitionApi, 'ProcessId'],
] as const;

describe('process-scoped workflow loading', () => {
  it.each(scopes)('filters related records on the server (%#)', async (api, field) => {
    get.mockResolvedValue({ data: { success: true, data: [], pagination: { totalPages: 0 } } });
    const controller = new AbortController();
    await api.list(controller.signal, 42);
    const [url, config] = get.mock.calls[0];
    expect(url).toMatch(/\/paged$/);
    expect(config?.signal).toBe(controller.signal);
    const params = config?.params as URLSearchParams;
    expect(params.get('Filters[0].Field')).toBe(field);
    expect(params.get('Filters[0].Value')).toBe('42');
    expect(params.get('Filters[0].Operator')).toBe('equals');
  });

  it('loads every page in stable order without downloading other processes', async () => {
    get.mockResolvedValueOnce({
      data: { success: true, data: [{ recId: 1 }], pagination: { totalPages: 2 } },
    });
    get.mockResolvedValueOnce({
      data: { success: true, data: [{ recId: 2 }], pagination: { totalPages: 2 } },
    });
    expect(await listWorkflowProcessRows('/v1/WfStep', 'ProcessId', 42)).toEqual([
      { recId: 1 },
      { recId: 2 },
    ]);
    expect(
      get.mock.calls.map(([, config]) => (config?.params as URLSearchParams).get('PageNumber'))
    ).toEqual(['1', '2']);
    for (const [, config] of get.mock.calls) {
      expect((config?.params as URLSearchParams).get('SortField')).toBe('RecId');
      expect((config?.params as URLSearchParams).get('Filters[0].Value')).toBe('42');
    }
  });

  it('does not issue requests after cancellation or for an invalid process', async () => {
    const controller = new AbortController();
    controller.abort();
    await expect(
      listWorkflowProcessRows('/v1/WfStep', 'ProcessId', 42, controller.signal)
    ).rejects.toBeDefined();
    await expect(listWorkflowProcessRows('/v1/WfStep', 'ProcessId', 0)).rejects.toThrow(
      'valid workflow process'
    );
    expect(get).not.toHaveBeenCalled();
  });

  it('rejects malformed pagination instead of requesting pages indefinitely', async () => {
    get.mockResolvedValue({ data: { success: true, data: [{ recId: 1 }], pagination: {} } });
    await expect(listWorkflowProcessRows('/v1/WfStep', 'ProcessId', 42)).rejects.toThrow(
      'Invalid pagination'
    );
    expect(get).toHaveBeenCalledTimes(1);
  });

  it('rejects a premature empty page rather than silently truncating a process', async () => {
    get.mockResolvedValue({ data: { success: true, data: [], pagination: { totalPages: 3 } } });
    await expect(listWorkflowProcessRows('/v1/WfStep', 'ProcessId', 42)).rejects.toThrow(
      'before all records'
    );
    expect(get).toHaveBeenCalledTimes(1);
  });

  it('reports failures instead of returning incomplete data', async () => {
    get.mockResolvedValueOnce({
      data: { success: true, data: [{ recId: 1 }], pagination: { totalPages: 2 } },
    });
    get.mockResolvedValueOnce({ data: { success: false, data: null, message: 'Load failed' } });
    await expect(listWorkflowProcessRows('/v1/WfStep', 'ProcessId', 42)).rejects.toThrow(
      'Load failed'
    );
  });
});
