import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen } from '@test/testUtils';
import { queryClient } from '@core/api/queryClient';
import { wfProcessApi } from '@modules/workflow/api/wfProcessApi';
import { wfStepApi, type WfStepRecord } from '@modules/workflow/api/wfStepApi';
import { WFStepsPage } from '@modules/workflow/pages/WFStepsPage';

const step: WfStepRecord = {
  id: '1',
  recId: 1,
  code: 'STEP-001',
  name: 'Manager approval',
  description: null,
  processId: 10,
  sortOrder: 1,
  score: 5,
  autoPassingHrs: 24,
  allMandatory: true,
  sysField: false,
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId: 'dat',
};

beforeEach(() => {
  queryClient.clear();
  vi.restoreAllMocks();
  vi.spyOn(wfStepApi, 'list').mockResolvedValue([step]);
  vi.spyOn(wfProcessApi, 'getById').mockRejectedValue(new Error('not loaded in this test'));
});

describe('WFStepsPage', () => {
  it('uses the Variables list-details lifecycle with backend-shaped step data', async () => {
    render(<WFStepsPage />);

    expect(await screen.findByRole('heading', { name: 'Workflow steps' })).toBeDefined();
    expect((await screen.findAllByText('Manager approval')).length).toBeGreaterThan(0);
    expect(screen.getByText('Step configuration')).toBeDefined();
    expect(screen.getByText('Automatic passing hours')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Edit' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'New' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Delete' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Workflow activities' })).toBeDefined();
  });
});
