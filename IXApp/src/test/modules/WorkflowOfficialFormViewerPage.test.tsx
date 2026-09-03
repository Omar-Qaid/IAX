import React from 'react';
import { describe, expect, it, vi } from 'vitest';
import { screen, waitFor } from '@testing-library/react';
import { render } from '@test/testUtils';
import type { WfRequestRecord } from '@modules/workflow/api/wfRequestApi';
import { WorkflowOfficialFormViewer } from '@modules/workflow/report-viewer/pages/WorkflowOfficialFormViewerPage';

import { reportDesignerApi } from '@shared/components/report-designer';

vi.spyOn(reportDesignerApi, 'getPublishedForRecord').mockResolvedValue({ templateId: 17, processId: 7, code: 'OFFICIAL', name: 'Official request form', document: { schemaVersion: 1, language: 'en', direction: 'ltr', page: { size: 'A4', orientation: 'portrait', margins: { top: 15, right: 15, bottom: 15, left: 15 } }, header: [{ id: 'title', type: 'text', value: 'Approved official layout' }], sections: [{ id: 'amount', type: 'field', label: 'Total sales', binding: { sourceType: 'requestControl', requestControlId: 2101 } }], footer: [], missingFieldBehavior: 'empty' } } as any);

vi.mock('@modules/workflow/api/wfRequestApi', () => ({
  wfRequestApi: { mailDetails: vi.fn().mockResolvedValue({ requestId: 42, processName: 'Daily closing', status: 'Completed', requestDate: '2026-08-25T08:00:00Z', employeeName: 'Employee', employeeNumber: 'D141', transactionType: 'Completed', transactionTime: '2026-08-25T08:00:00Z', transactionEndTime: '2026-08-25T09:00:00Z', responsibleEmployee: null, history: [], fields: [{ detailId: 1, controlId: 100, controlDataId: 2101, label: 'Total sales', labelAr: '', value: '5239', valueAr: '', valueEn: '5239', controlType: 'number', controlOrder: 1 }] }) },
}));

vi.mock('@shared/components/report-viewer/reportCompany', () => ({
  fetchreportCompany: vi.fn().mockResolvedValue({ name: 'USMF', companyCode: 'HBMC' }),
  toReportCompany: (_entity: unknown, code: string) => ({ name: code || 'Company', companyCode: code }),
}));

const request: WfRequestRecord = { id: '42', recId: 42, code: 'REQ-42', name: 'Request', description: null, requestDate: '2026-08-25T08:00:00Z', processId: 7, employeeId: 9, requestDetails: null, isFinished: true, finishedDate: '2026-08-25T09:00:00Z', isStopped: false, stoppedDate: null, score: 0, progress: 100, notes: null, isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'HBMC' };

describe('WorkflowOfficialFormViewer', () => {
  it('loads the published template and resolves request-control values at runtime', async () => {
    render(<WorkflowOfficialFormViewer open request={request} templateId={17} onClose={() => undefined} />);
    await waitFor(() => expect(screen.getByText('Approved official layout')).toBeInTheDocument());
    expect(screen.getByText('5239')).toBeInTheDocument();
    expect(screen.getByRole('region', { name: 'Official request form' })).toBeInTheDocument();
  });
});
