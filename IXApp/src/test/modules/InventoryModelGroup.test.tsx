import React from 'react';
import { afterEach, expect, it, vi } from 'vitest';
import { cleanup, render } from '@testing-library/react';
import { apiClient } from '@core/api/apiClient';
import type { EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { InventModelGroupPage } from '@modules/finance/inventory/pages/InventModelGroupPage';
import { inventModelGroupApi, type InventModelGroupRecord } from '@modules/finance/inventory/api/inventModelGroupApi';

let renderedConfig: EnterpriseListDetailsConfig<InventModelGroupRecord>;
vi.mock('@patterns/list-details/ListDetailsPage', () => ({
  ListDetailsPage: ({ config }: { config: typeof renderedConfig }) => { renderedConfig = config; return null; },
}));
vi.mock('@core/localization/useAppTranslation', () => ({
  useAppTranslation: () => ({ t: (key: string, fallback?: string) => fallback ?? key }),
}));
vi.mock('@core/api/apiClient', () => ({ apiClient: { delete: vi.fn() } }));
afterEach(() => { cleanup(); vi.clearAllMocks(); });

it('starts with a valid stocked FIFO model and rejects unsupported policy changes', () => {
  render(<InventModelGroupPage />);
  const group = { ...renderedConfig.createRecord(), modelGroupId: 'FIFO', name: 'Basic FIFO' };
  expect([group.inventModel, group.stockedProduct, group.postOnHandFinancial]).toEqual([0, true, true]);
  expect(renderedConfig.validate?.(group)).toEqual({});
  expect(renderedConfig.validate?.({ ...group, inventModel: 3 })).toHaveProperty('inventModel');
  expect(renderedConfig.validate?.({ ...group, negativePhysical: true })).toHaveProperty('inventModel');
  expect(renderedConfig.validate?.({ ...group, postOnHandFinancial: false })).toHaveProperty('inventModel');
  const sections = typeof renderedConfig.sections === 'function'
    ? renderedConfig.sections({ record: group, editing: true, onRecordChange: vi.fn() })
    : renderedConfig.sections;
  const fields = sections.flatMap((section) => section.groups?.flatMap((entry) => entry.fields) ?? []);
  expect(fields.find((field) => field.name === 'inventModel')?.options).toEqual([{ value: '0', label: 'FIFO' }]);
  for (const name of ['inventModel', 'stockedProduct', 'postOnHandFinancial', 'negativePhysical', 'reserveByDate'])
    expect(fields.find((field) => field.name === name)?.disabled).toBe(true);
});

it('does not treat a false delete response as successful model group deletion', async () => {
  vi.mocked(apiClient.delete).mockResolvedValue({ data: { success: true, data: false } });
  await expect(inventModelGroupApi.delete({ id: '7' } as InventModelGroupRecord)).rejects.toThrow('could not be deleted');
});
