import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen } from '@test/testUtils';
import { queryClient } from '@core/api/queryClient';
import { documentApi } from '@shared/components/documents';
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
  window.localStorage.setItem('workflow.print-templates.process-id', '589');
  vi.restoreAllMocks();
  vi.spyOn(wfProcessApi, 'list').mockResolvedValue([process]);
  vi.spyOn(wfProcessApi, 'getById').mockResolvedValue(process);
  vi.spyOn(printTemplateApi, 'listByProcess').mockResolvedValue([template]);
  vi.spyOn(documentApi, 'list').mockResolvedValue({
    items: [],
    pageNumber: 1,
    pageSize: 100,
    totalCount: 0,
  });
});

describe('PrintTemplatesPage', () => {
  it('loads templates through the workflow module and renders translated metadata', async () => {
    render(<PrintTemplatesPage />);

    expect(screen.getByText('Print templates')).toBeDefined();
    expect(await screen.findByText('Request report', {}, { timeout: 10_000 })).toBeDefined();
    expect(screen.getByText('REQ-A4')).toBeDefined();
    expect(await screen.findByDisplayValue('Request an inventory')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Open' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Refresh' }).textContent).toBe('');
    expect(screen.getByRole('button', { name: 'Attachments' })).toBeDisabled();
    expect(screen.getByRole('button', { name: 'Options' })).toBeDisabled();
    expect(printTemplateApi.listByProcess).toHaveBeenCalledWith(589, expect.any(AbortSignal));
  });

  it('toggles column filters from Search and enables Options for the selected template', async () => {
    render(<PrintTemplatesPage />);

    const templateName = await screen.findByText('Request report', {}, { timeout: 10_000 });
    expect(screen.queryByPlaceholderText('Filter value')).not.toBeInTheDocument();
    const utilityRail = screen.getByRole('complementary', { name: 'Information' });
    const railFilter = utilityRail.querySelector<HTMLButtonElement>('button[aria-label="Filter"]');
    expect(railFilter).not.toBeNull();
    expect(railFilter).toHaveAttribute('aria-pressed', 'false');

    const searchCommand = screen
      .getAllByRole('button', { name: 'Search' })
      .find((button) => button.textContent?.trim() === 'Search');
    expect(searchCommand).toBeDefined();
    fireEvent.click(searchCommand!);
    expect(screen.getAllByPlaceholderText('Filter value').length).toBeGreaterThan(0);
    expect(railFilter).toHaveAttribute('aria-pressed', 'true');

    fireEvent.click(railFilter!);
    expect(screen.queryByPlaceholderText('Filter value')).not.toBeInTheDocument();
    expect(railFilter).toHaveAttribute('aria-pressed', 'false');

    fireEvent.click(templateName);
    expect(screen.getByRole('button', { name: 'Options' })).not.toBeDisabled();
    expect(screen.getByRole('button', { name: 'Attachments' })).not.toBeDisabled();

    fireEvent.click(screen.getByRole('button', { name: 'Options' }));
    expect(screen.getByRole('menuitem', { name: 'Record Info' })).toBeDefined();
    expect(screen.getByRole('menuitem', { name: 'Record Audit' })).toBeDefined();
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
