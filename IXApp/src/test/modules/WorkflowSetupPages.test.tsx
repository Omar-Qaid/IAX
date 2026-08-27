import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen, waitFor } from '@test/testUtils';
import { queryClient } from '@core/api/queryClient';
import type { WorkflowMasterRecord } from '@modules/workflow/api/workflowMasterApi';
import {
  wfActivityTypeApi,
  wfControlApi,
  wfDataTypeApi,
  wfPriorityApi,
} from '@modules/workflow/api/workflowSetupApis';
import { WfActivityTypesPage } from '@modules/workflow/pages/WfActivityTypesPage';
import { WfControlsPage } from '@modules/workflow/pages/WfControlsPage';
import { WfDataTypesPage } from '@modules/workflow/pages/WfDataTypesPage';
import { WfPrioritiesPage } from '@modules/workflow/pages/WfPrioritiesPage';

const record = (name: string): WorkflowMasterRecord => ({
  id: '1',
  recId: 1,
  code: 'WF-001',
  name,
  description: `${name} description`,
  sortOrder: 1,
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId: 'dat',
});

beforeEach(() => {
  queryClient.clear();
  vi.restoreAllMocks();
});

describe('workflow setup pages', () => {
  it.each([
    ['Workflow activity types', 'Approval activity', wfActivityTypeApi, WfActivityTypesPage],
    ['Workflow data types', 'Text value', wfDataTypeApi, WfDataTypesPage],
    ['Workflow priorities', 'High priority', wfPriorityApi, WfPrioritiesPage],
  ] as const)(
    'renders %s through the WFCategory page lifecycle',
    async (title, name, api, Page) => {
      vi.spyOn(api, 'list').mockResolvedValue([record(name)]);
      render(<Page />);

      expect(screen.getByText(title)).toBeDefined();
      expect(await screen.findByText(name)).toBeDefined();
      expect(screen.getByRole('button', { name: 'Back' })).toBeDefined();
      expect(screen.getByRole('button', { name: 'Edit' })).toBeDefined();
      expect(screen.getByRole('button', { name: 'New' })).toBeDefined();
      expect(screen.getByRole('button', { name: 'Delete' })).toBeDefined();
    }
  );

  it('renders the required control type from the WfControl contract', async () => {
    vi.spyOn(wfControlApi, 'list').mockResolvedValue([
      { ...record('Text box'), controlType: 'TextBox' },
    ]);
    render(<WfControlsPage />);

    expect(screen.getByText('Workflow controls')).toBeDefined();
    expect(await screen.findByText('TextBox')).toBeDefined();
  });

  it('allows an existing activity type to be edited when number-sequence metadata is unavailable', async () => {
    const activityType = record('Approval activity');
    vi.spyOn(wfActivityTypeApi, 'list').mockResolvedValue([activityType]);
    const update = vi.spyOn(wfActivityTypeApi, 'update').mockResolvedValue(activityType);
    render(<WfActivityTypesPage />);

    expect(await screen.findByText('Approval activity')).toBeDefined();
    fireEvent.click(screen.getByRole('button', { name: 'Edit' }));
    fireEvent.click(screen.getByRole('button', { name: 'Save' }));

    await waitFor(() => expect(update).toHaveBeenCalledOnce());
  });
});
