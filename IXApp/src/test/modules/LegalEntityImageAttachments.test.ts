import { beforeEach, describe, expect, it, vi } from 'vitest';
import { queryClient } from '@core/api/queryClient';
import { documentApi } from '@shared/components/documents/documentApi';
import {
  LEGAL_ENTITY_REPORT_LOGO,
  LEGAL_ENTITY_TABLE_ID,
  saveLegalEntityImageAttachments,
} from '@modules/organization/api/legalEntityImageAttachments';
import type { LegalEntityRecord } from '@modules/organization/types/legalEntityTypes';

const record = (values: Partial<LegalEntityRecord> = {}): LegalEntityRecord =>
  ({ id: '1', recId: 1, ...values }) as LegalEntityRecord;

beforeEach(() => {
  queryClient.clear();
  vi.restoreAllMocks();
});

describe('legal-entity image attachments', () => {
  it('replaces the report logo through managed document storage', async () => {
    const file = new File(['image'], 'report.png', { type: 'image/png' });
    vi.spyOn(documentApi, 'list').mockResolvedValue({
      items: [
        {
          id: 42,
          name: LEGAL_ENTITY_REPORT_LOGO,
        } as Awaited<ReturnType<typeof documentApi.list>>['items'][number],
      ],
      pageNumber: 1,
      pageSize: 100,
      totalCount: 1,
    });
    const remove = vi.spyOn(documentApi, 'remove').mockResolvedValue();
    vi.spyOn(documentApi, 'types').mockResolvedValue([
      {
        id: 3,
        typeId: 'Image',
        name: 'Image',
        typeGroup: 3,
        kind: 'Image',
        filePlace: 0,
        description: null,
        allowedExtensions: ['.png'],
        allowedMimeTypes: ['image/png'],
        maxFileSizeBytes: 2_000_000,
      },
    ]);
    const create = vi.spyOn(documentApi, 'create').mockResolvedValue({ id: 43 } as never);

    await saveLegalEntityImageAttachments(record({ reportLogoFile: file }), record());

    expect(remove).toHaveBeenCalledWith(42);
    expect(create).toHaveBeenCalledWith(
      LEGAL_ENTITY_TABLE_ID,
      1,
      expect.objectContaining({
        typeId: 'Image',
        name: LEGAL_ENTITY_REPORT_LOGO,
        file,
      })
    );
    expect(LEGAL_ENTITY_TABLE_ID).toBe(1703791781);
  });
});
