import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen } from '@test/testUtils';
import { queryClient } from '@core/api/queryClient';
import { wfProcessApi, type WfProcessRecord } from '@modules/workflow/api/wfProcessApi';
import { printTemplateApi } from '@modules/workflow/print-templates/api/printTemplateApi';
import { PrintTemplatesPage } from '@modules/workflow/print-templates/pages/PrintTemplatesPage';
import {
  createEmptyPrintTemplateDocument,
  type PrintTemplateSummary,
} from '@modules/workflow/print-templates/types/printTemplate.types';

const process: WfProcessRecord = {
  id: '589',
  recId: 589,
  code: 'PROC-589',
  name: 'Request an inventory',
  categoryId: 1,
  score: 0,
  canRepeat: false,
  mandatoryDocs: false,
  priorityId: 1,
  processTypeId: 1,
  sysField: false,
  sortOrder: 1,
  usersProcesses: [],
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId: 'dat',
};

const template: PrintTemplateSummary = {
  templateId: 42,
  processId: 589,
  processName: 'Request an inventory',
  code: 'REQ-A4',
  name: 'Request report',
  description: null,
  pageSize: 'A4',
  orientation: 'portrait',
  language: 'en',
  isDefault: true,
  status: 'draft',
  currentVersionId: null,
  currentVersionNo: null,
  latestVersionNo: 1,
  hasDraft: true,
  isActive: true,
  lastModifiedAt: null,
};

beforeEach(() => {
  queryClient.clear();
  vi.restoreAllMocks();
  vi.spyOn(wfProcessApi, 'list').mockResolvedValue([process]);
  vi.spyOn(wfProcessApi, 'getById').mockResolvedValue(process);
  vi.spyOn(printTemplateApi, 'listByProcess').mockResolvedValue([template]);
});

describe('PrintTemplatesPage', () => {
  it('loads templates through the workflow module and renders translated metadata', async () => {
    render(<PrintTemplatesPage />);

    expect(screen.getByText('Print templates')).toBeDefined();
    expect(await screen.findByText('Request report', {}, { timeout: 10_000 })).toBeDefined();
    expect(screen.getByText('REQ-A4')).toBeDefined();
    expect(await screen.findByDisplayValue('Request an inventory')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Open' })).toBeDefined();
    expect(printTemplateApi.listByProcess).toHaveBeenCalledWith(589, expect.any(AbortSignal));
  });

  it('creates direction-safe empty documents for both supported languages', () => {
    expect(createEmptyPrintTemplateDocument('en')).toMatchObject({
      language: 'en',
      direction: 'ltr',
      schemaVersion: 1,
    });
    expect(createEmptyPrintTemplateDocument('ar')).toMatchObject({
      language: 'ar',
      direction: 'rtl',
      schemaVersion: 1,
    });
  });
});
