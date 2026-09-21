import { beforeEach, describe, expect, it, vi } from 'vitest';
import { apiClient } from '@core/api/apiClient';
import { organizationStructureApi as api } from '@modules/organization/api/organizationStructureApi';

vi.mock('@core/api/apiClient', () => ({
  apiClient: { get: vi.fn(), post: vi.fn(), put: vi.fn() },
}));
describe('Organization structure HTTP contracts', () => {
  beforeEach(() => vi.clearAllMocks());
  it('reads raw DTO arrays and sends the explicit effective date', async () => {
    const rows = [
      {
        id: 7,
        code: 'SH-A',
        name: 'Showroom A',
        type: 4,
        parentOrganizationUnitId: null,
        validFrom: '2026-01-01',
        validTo: null,
      },
    ];
    vi.mocked(apiClient.get).mockResolvedValue({ data: rows });
    const signal = new AbortController().signal;
    expect(await api.units('2026-09-16', signal)).toEqual(rows);
    expect(apiClient.get).toHaveBeenCalledWith('/v1/organization-structure/units', {
      params: { asOf: '2026-09-16' },
      signal,
    });
  });
  it('creates a unit without accepting a company override', async () => {
    vi.mocked(apiClient.post).mockResolvedValue({ data: 12 });
    const draft = {
      code: 'WH-1',
      name: 'Warehouse',
      nameAR: null,
      type: 9,
      parentOrganizationUnitId: null,
      validFrom: '2026-01-01',
      validTo: null,
    };
    expect(await api.create(draft)).toBe(12);
    expect(apiClient.post).toHaveBeenCalledWith('/v1/organization-structure/units', {
      code: 'WH-1',
      name: 'Warehouse',
      nameAlias: null,
      type: 9,
      parentOrganizationUnitId: null,
      validFrom: '2026-01-01',
      validTo: null,
    });
  });
  it('closes a period using PUT rather than deleting the unit', async () => {
    vi.mocked(apiClient.put).mockResolvedValue({ data: 12 });
    await api.close(12, '2026-10-01');
    expect(apiClient.put).toHaveBeenCalledWith('/v1/organization-structure/units/12/close', {
      validTo: '2026-10-01',
    });
  });
  it('retains API failures for the drawer to show without losing the draft', async () => {
    vi.mocked(apiClient.put).mockRejectedValue(new Error('Close positions first'));
    await expect(api.close(12, '2026-10-01')).rejects.toThrow('Close positions first');
  });
});
