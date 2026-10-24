import React from 'react';
import { afterEach, expect, it, vi } from 'vitest';
import { cleanup, render } from '@testing-library/react';
import { apiClient } from '@core/api/apiClient';
import type { EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { EcoResStorageDimensionGroupPage } from '@modules/finance/inventory/pages/EcoResStorageDimensionGroupPage';
import { EcoResTrackingDimensionGroupPage } from '@modules/finance/inventory/pages/EcoResTrackingDimensionGroupPage';
import { storageDimensionGroupApi, type StorageDimensionGroupRecord } from '@modules/finance/inventory/api/storageDimensionGroupApi';
import { trackingDimensionGroupApi, type TrackingDimensionGroupRecord } from '@modules/finance/inventory/api/trackingDimensionGroupApi';

let renderedConfig: EnterpriseListDetailsConfig<StorageDimensionGroupRecord | TrackingDimensionGroupRecord>;
vi.mock('@patterns/list-details/ListDetailsPage', () => ({
  ListDetailsPage: ({ config }: { config: typeof renderedConfig }) => { renderedConfig = config; return null; },
}));
vi.mock('@core/localization/useAppTranslation', () => ({
  useAppTranslation: () => ({ t: (key: string, fallback?: string) => fallback ?? key }),
}));
vi.mock('@core/api/apiClient', () => ({ apiClient: { delete: vi.fn() } }));
afterEach(() => { cleanup(); vi.clearAllMocks(); });

it('creates a basic site and warehouse group without location tracking', () => {
  render(<EcoResStorageDimensionGroupPage />);
  const group = renderedConfig.createRecord() as StorageDimensionGroupRecord;
  expect([group.useSite, group.useWarehouse, group.useLocation]).toEqual([true, true, false]);
  expect(renderedConfig.validate?.({ ...group, groupId: 'BASIC', name: 'Basic' })).toEqual({});
  expect(renderedConfig.validate?.({ ...group, groupId: 'ADV', name: 'Advanced', useLocation: true })).toHaveProperty('useSite');
});

it('rejects batch and serial tracking in the basic group form', () => {
  render(<EcoResTrackingDimensionGroupPage />);
  const group = renderedConfig.createRecord() as TrackingDimensionGroupRecord;
  expect([group.useBatchNumber, group.useSerialNumber]).toEqual([false, false]);
  expect(renderedConfig.validate?.({ ...group, groupId: 'NONE', name: 'No tracking' })).toEqual({});
  expect(renderedConfig.validate?.({ ...group, groupId: 'BATCH', name: 'Batch', useBatchNumber: true })).toHaveProperty('useBatchNumber');
});

it('does not treat false delete responses as successful group deletion', async () => {
  vi.mocked(apiClient.delete).mockResolvedValue({ data: { success: true, data: false } });
  await expect(storageDimensionGroupApi.delete('1')).rejects.toThrow('could not be deleted');
  await expect(trackingDimensionGroupApi.delete('2')).rejects.toThrow('could not be deleted');
});
