import { beforeEach, describe, expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { createEntityApi } from '@core/api/createEntityApi';
import { wfActivityApi } from '@modules/workflow/api/wfActivityApi';
import { wfActivityControlOptionApi } from '@modules/workflow/api/wfActivityControlOptionApi';

vi.mock('@core/api/apiClient', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));
const record = { recId: 7, id: '7', name: ' Example ' };
const api = createEntityApi({
  endpoint: '/v1/Example',
  resourceName: 'example',
  toRecord: (dto: { recId: number; name: string }) => ({ ...dto, id: String(dto.recId) }),
  toDto: ({ id: _id, ...value }: typeof record) => ({ ...value, name: value.name.trim() }),
});
beforeEach(() => vi.clearAllMocks());

describe('generic entity API', () => {
  it('applies the adapter mapping for reads, creates, and updates', async () => {
    const dto = { recId: 7, name: 'Example' };
    vi.mocked(apiClient.get).mockResolvedValue({ data: { success: true, data: [dto] } });
    vi.mocked(apiClient.post).mockResolvedValue({ data: { success: true, data: dto } });
    vi.mocked(apiClient.put).mockResolvedValue({ data: { success: true, data: dto } });
    const controller = new AbortController();
    expect(await api.list(controller.signal)).toEqual([{ ...dto, id: '7' }]);
    expect(apiClient.get).toHaveBeenCalledWith('/v1/Example', { signal: controller.signal });
    expect(await api.create(record)).toEqual({ ...dto, id: '7' });
    expect(await api.update(record)).toEqual({ ...dto, id: '7' });
    expect(apiClient.post).toHaveBeenCalledWith('/v1/Example', dto);
    expect(apiClient.put).toHaveBeenCalledWith('/v1/Example/7', dto);
  });

  it('validates delete responses and propagates API errors', async () => {
    vi.mocked(apiClient.delete).mockResolvedValue({ data: { success: true, data: true } });
    await api.delete(record);
    expect(apiClient.delete).toHaveBeenCalledWith('/v1/Example/7');
    vi.mocked(apiClient.delete).mockResolvedValue({
      data: { success: false, message: 'Denied', data: null },
    });
    await expect(api.delete(record)).rejects.toThrow('Denied');
    vi.mocked(apiClient.post).mockResolvedValue({ data: { success: true, data: null } });
    await expect(api.create(record)).rejects.toThrow('example response did not contain data');
  });

  it('preserves domain-specific normalization instead of normalizing every DTO generically', async () => {
    const activity = {
      recId: 7,
      id: '7',
      name: ' Approval ',
      code: ' ACT ',
      nameAlias: ' Alias ',
      description: ' ',
      extendedProperties: ' {} ',
    };
    vi.mocked(apiClient.put).mockResolvedValue({ data: { success: true, data: activity } });
    await wfActivityApi.update(activity as Parameters<typeof wfActivityApi.update>[0]);
    expect(apiClient.put).toHaveBeenLastCalledWith('/v1/WfActivity/7', {
      recId: 7,
      name: 'Approval',
      code: 'ACT',
      nameAlias: 'Alias',
      description: null,
      extendedProperties: '{}',
    });
    const option = { recId: 3, id: '3', value: ' keep spaces ', name: ' Raw option ' };
    vi.mocked(apiClient.put).mockResolvedValue({ data: { success: true, data: option } });
    await wfActivityControlOptionApi.update(
      option as Parameters<typeof wfActivityControlOptionApi.update>[0]
    );
    expect(apiClient.put).toHaveBeenLastCalledWith('/v1/WfActivityControlsOption/3', {
      recId: 3,
      value: ' keep spaces ',
      name: ' Raw option ',
    });
  });
});
