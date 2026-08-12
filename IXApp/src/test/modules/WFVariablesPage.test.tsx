import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render as testingLibraryRender } from '@testing-library/react';
import { MemoryRouter } from 'react-router-dom';
import { render, screen } from '@test/testUtils';
import { AppProviders } from '@app/providers/AppProviders';
import { queryClient } from '@core/api/queryClient';
import { wfVariableApi, type WfVariableRecord } from '@modules/workflow/api/wfVariableApi';
import { wfDataTypeApi } from '@modules/workflow/api/workflowSetupApis';
import { wfProcessApi } from '@modules/workflow/api/wfProcessApi';
import { WFVariablesPage } from '@modules/workflow/pages/WFVariablesPage';

const variable: WfVariableRecord = {
  id: '1',
  recId: 1,
  code: 'VAR-001',
  name: 'Requested amount',
  nameAR: 'المبلغ المطلوب',
  description: null,
  descriptionAR: null,
  dataTypeId: 2,
  processId: 10,
  sortOrder: 1,
  dataType: null,
  process: null,
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId: 'dat',
};

beforeEach(() => {
  queryClient.clear();
  vi.restoreAllMocks();
  vi.spyOn(wfVariableApi, 'list').mockResolvedValue([variable]);
  vi.spyOn(wfDataTypeApi, 'list').mockResolvedValue([]);
  vi.spyOn(wfProcessApi, 'getById').mockRejectedValue(new Error('not loaded in this test'));
});

describe('WFVariablesPage', () => {
  it('uses the currencies list-details lifecycle with backend-shaped variable data', async () => {
    render(<WFVariablesPage />);

    expect(await screen.findByRole('heading', { name: 'Workflow variables' })).toBeDefined();
    expect((await screen.findAllByText('Requested amount')).length).toBeGreaterThan(0);
    expect(screen.getByText('Variable configuration')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Edit' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'New' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Delete' })).toBeDefined();
  });

  it('shows only variables related to the process supplied by the Processes page', async () => {
    vi.spyOn(wfVariableApi, 'list').mockResolvedValue([
      variable,
      { ...variable, id: '2', recId: 2, processId: 20, name: 'Unrelated variable' },
    ]);

    testingLibraryRender(
      <MemoryRouter initialEntries={['/workflow/variables?processId=10']}>
        <AppProviders>
          <WFVariablesPage />
        </AppProviders>
      </MemoryRouter>
    );

    expect((await screen.findAllByText('Requested amount')).length).toBeGreaterThan(0);
    expect(screen.queryByText('Unrelated variable')).toBeNull();
  });

  it('keeps an empty page usable with a transparent empty-data watermark', async () => {
    vi.spyOn(wfVariableApi, 'list').mockResolvedValue([]);

    render(<WFVariablesPage />);

    expect(await screen.findByRole('heading', { name: 'Workflow variables' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'New' })).toBeDefined();
    expect(screen.getByText('Variable configuration')).toBeDefined();
    expect(screen.getByText('Variable code')).toBeDefined();
    expect(screen.getByText('No records found')).toBeDefined();
    expect(screen.getByText('There are no records to display.')).toBeDefined();
  });
});
