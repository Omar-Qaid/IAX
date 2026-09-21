import { beforeEach, describe, expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { hcmWorkerApi, type HcmWorkerDto } from '@modules/organization/api/hcmWorkerApi';

vi.mock('@core/api/apiClient', () => ({
  apiClient: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

const worker = (recId: number, personnelNumber: string): HcmWorkerDto => ({
  recId,
  personnelNumber,
  person: recId + 100,
  name: personnelNumber,
  nameAlias: personnelNumber,
  occupationName: 'Occupation',
  occupationId: 1,
  genderId: 1,
  nationalityId: 1,
  hireDate: null,
  birthDate: null,
  isActive: true,
});

describe('hcmWorkerApi identity mapping', () => {
  beforeEach(() => vi.clearAllMocks());

  it('uses RecId as the unique list selection and persistence identity', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({
      data: { success: true, data: [worker(11, 'W-11'), worker(12, 'W-12')] },
    });

    const records = await hcmWorkerApi.list();

    expect(records.map((record) => record.id)).toEqual(['11', '12']);
    expect(records.map((record) => record.recordId)).toEqual([11, 12]);

    vi.mocked(apiClient.put).mockResolvedValue({
      data: { success: true, data: worker(12, 'W-12') },
    });
    await hcmWorkerApi.update(records[1]);

    expect(apiClient.put).toHaveBeenCalledWith(
      '/v1/HcmWorker/12',
      expect.objectContaining({
        recId: 12,
        personnelNumber: 'W-12',
        name: 'W-12',
        nameAlias: 'W-12',
      })
    );
  });

  it('rejects a response without a valid RecId instead of duplicating selection ids', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({
      data: { success: true, data: [{ ...worker(1, 'W-1'), recId: undefined }] },
    });

    await expect(hcmWorkerApi.list()).rejects.toThrow('invalid record identifier');
  });

  it('maps lookup RecId values to selectable option ids', async () => {
    vi.mocked(apiClient.get).mockResolvedValue({
      data: {
        success: true,
        data: [
          { recId: 7, code: 'MGR', name: 'Manager', nameAlias: 'مدير' },
          { recId: 8, code: 'SUP', name: 'Supervisor', nameAlias: 'مشرف' },
        ],
      },
    });

    await expect(hcmWorkerApi.lookup('Occupation')).resolves.toEqual([
      { id: 7, code: 'MGR', name: 'Manager', nameAlias: 'مدير' },
      { id: 8, code: 'SUP', name: 'Supervisor', nameAlias: 'مشرف' },
    ]);
    expect(apiClient.get).toHaveBeenCalledWith('/v1/Occupation', { signal: undefined });
  });
});
