import { beforeEach, describe, expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { loadWfPerformerUsers } from '@modules/workflow/api/wfPerformerApi';

vi.mock('@core/api/apiClient', () => ({
  apiClient: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() },
}));

describe('workflow performer users API', () => {
  beforeEach(() => vi.clearAllMocks());

  it('loads WfUsersPerformers rows for the selected performer and maps stable ids', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({
      data: {
        success: true,
        data: [{ recId: 31, performerId: 7, userID: 42, relatedField: 0, extendedProperties: null }],
      },
    });

    await expect(loadWfPerformerUsers(7)).resolves.toEqual([
      { id: '31', recId: 31, performerId: 7, userID: 42, relatedField: 0, extendedProperties: null },
    ]);
    expect(apiClient.get).toHaveBeenCalledWith('/v1/WfPerformer/7/users', { signal: undefined });
  });
});
