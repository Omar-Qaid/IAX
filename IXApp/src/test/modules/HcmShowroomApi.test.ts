import { beforeEach, describe, expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { hcmShowroomApi } from '@modules/organization/api/hcmShowroomApi';

vi.mock('@core/api/apiClient', () => ({
  apiClient: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() },
}));

describe('hcmShowroomApi', () => {
  beforeEach(() => vi.clearAllMocks());

  it('loads showroom worker assignments and saves new and existing rows', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({ data: { success: true, data: [] } });
    vi.mocked(apiClient.post).mockResolvedValue({ data: { success: true, data: true } });
    vi.mocked(apiClient.put).mockResolvedValue({ data: { success: true, data: true } });
    const request = {
      hcmWorkerId: 21,
      validFrom: '2026-09-28',
      validTo: null,
      isActive: true,
    };

    await hcmShowroomApi.workerAssignments(7);
    await hcmShowroomApi.saveWorkerAssignment(7, null, request);
    await hcmShowroomApi.saveWorkerAssignment(7, 31, request);

    expect(apiClient.get).toHaveBeenCalledWith('/v1/HcmShowroom/7/worker-assignments', {
      signal: undefined,
    });
    expect(apiClient.post).toHaveBeenCalledWith('/v1/HcmShowroom/7/worker-assignments', request);
    expect(apiClient.put).toHaveBeenCalledWith('/v1/HcmShowroom/7/worker-assignments/31', request);
  });
});
