import React from 'react';
import { afterEach, expect, it, vi } from 'vitest';
import { cleanup, render } from '@testing-library/react';
import { apiClient } from '@core/api/apiClient';
import type { EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { InventJournalNamePage } from '@modules/finance/inventory/pages/InventJournalNamePage';
import { inventJournalNameApi, type InventJournalNameRecord } from '@modules/finance/inventory/api/inventJournalNameApi';

let renderedConfig: EnterpriseListDetailsConfig<InventJournalNameRecord>;
vi.mock('@patterns/list-details/ListDetailsPage', () => ({
  ListDetailsPage: ({ config }: { config: typeof renderedConfig }) => { renderedConfig = config; return null; },
}));
vi.mock('@tanstack/react-query', () => ({ useQuery: () => ({ data: { numberSequences: [] } }) }));
vi.mock('@core/localization/useAppTranslation', () => ({
  useAppTranslation: () => ({ t: (key: string, fallback?: string) => fallback ?? key }),
}));
vi.mock('@core/api/apiClient', () => ({ apiClient: { delete: vi.fn() } }));
afterEach(() => { cleanup(); vi.clearAllMocks(); });

it('requires a voucher sequence and retains posted journal detail', () => {
  render(<InventJournalNamePage />);
  const journal = { ...renderedConfig.createRecord(), journalNameId: 'COUNT', description: 'Count' };
  expect(renderedConfig.validate?.(journal)).toHaveProperty('voucherNumberSequenceTable');
  expect(renderedConfig.validate?.({ ...journal, voucherNumberSequenceTable: 7 })).toEqual({});
  expect(renderedConfig.validate?.({ ...journal, voucherNumberSequenceTable: 7, deletePostedLines: true })).toHaveProperty('detailSummary');
  const sections = typeof renderedConfig.sections === 'function'
    ? renderedConfig.sections({ record: journal, editing: true, onRecordChange: vi.fn() })
    : renderedConfig.sections;
  const fields = sections.flatMap((section) => section.groups?.flatMap((entry) => entry.fields) ?? []);
  for (const name of ['detailSummary', 'deletePostedLines', 'reservation'])
    expect(fields.find((field) => field.name === name)?.disabled).toBe(true);
});

it('does not accept a false journal name deletion response', async () => {
  vi.mocked(apiClient.delete).mockResolvedValue({ data: { success: true, data: false } });
  await expect(inventJournalNameApi.delete({ recId: 4 } as InventJournalNameRecord)).rejects.toThrow('could not be deleted');
});
