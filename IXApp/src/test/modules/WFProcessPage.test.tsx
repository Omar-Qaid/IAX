import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { render } from '@test/testUtils';
import { queryClient } from '@core/api/queryClient';
import { wfProcessApi, type WfProcessRecord } from '@modules/workflow/api/wfProcessApi';
import { wfCategoryApi } from '@modules/workflow/api/wfCategoryApi';
import { wfPriorityApi, wfProcessTypeApi } from '@modules/workflow/api/workflowSetupApis';
import { WFProcessPage } from '@modules/workflow/pages/WFProcessPage';

const process: WfProcessRecord = {
  id: '10',
  recId: 10,
  code: 'WF-0010',
  name: 'Purchase approval',
  nameAR: 'اعتماد المشتريات',
  description: 'Purchase approval workflow',
  descriptionAR: null,
  categoryId: 2,
  score: 10,
  canRepeat: true,
  mandatoryDocs: true,
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

beforeEach(() => {
  queryClient.clear();
  vi.restoreAllMocks();
  vi.spyOn(wfProcessApi, 'list').mockResolvedValue([process]);
  vi.spyOn(wfCategoryApi, 'getById').mockResolvedValue({
    id: '2',
    recId: 2,
    code: 'PUR',
    name: 'Purchasing',
    nameAR: 'المشتريات',
    description: null,
    descriptionAR: null,
    sysField: false,
    sortOrder: 1,
    isActive: true,
    rowVersion: null,
    recVersion: 1,
    dataAreaId: 'dat',
  });
  vi.spyOn(wfPriorityApi, 'list').mockResolvedValue([
    {
      id: '1',
      recId: 1,
      code: 'HIGH',
      name: 'High',
      nameAR: 'عالي',
      description: null,
      descriptionAR: null,
      sortOrder: 1,
      isActive: true,
      rowVersion: null,
      recVersion: 1,
      dataAreaId: 'dat',
    },
  ]);
  vi.spyOn(wfProcessTypeApi, 'list').mockResolvedValue([
    {
      id: '1',
      recId: 1,
      code: 'STANDARD',
      name: 'Standard',
      nameAR: 'قياسي',
      description: null,
      descriptionAR: null,
      sortOrder: 1,
      isActive: true,
      rowVersion: null,
      recVersion: 1,
      dataAreaId: 'dat',
    },
  ]);
});

describe('WFProcessPage', () => {
  it('uses the currencies list-details lifecycle and backend-shaped data', async () => {
    const user = userEvent.setup();
    render(<WFProcessPage />);

    expect(await screen.findByRole('heading', { name: 'Workflow processes' })).toBeDefined();
    expect((await screen.findAllByText('Purchase approval')).length).toBeGreaterThan(0);
    expect(screen.getByText('Process configuration')).toBeDefined();
    expect(await screen.findByRole('textbox', { name: 'Category' })).toHaveValue('Purchasing');
    expect(await screen.findByRole('textbox', { name: 'Priority' })).toHaveValue('HIGH - High');
    expect(await screen.findByRole('combobox', { name: 'Process type' })).toHaveTextContent(
      'STANDARD - Standard'
    );
    await user.click(screen.getByRole('button', { name: 'Edit' }));
    expect(screen.getByRole('button', { name: 'Save' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Variables' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Steps' })).toBeDefined();
  });

  it('enforces only the two backend validator requirements', async () => {
    const user = userEvent.setup();
    render(<WFProcessPage />);

    await screen.findByText('Purchase approval workflow');
    await user.click(screen.getByRole('button', { name: 'New' }));
    await user.click(screen.getByRole('button', { name: 'Save' }));

    expect(await screen.findAllByText(/required/i)).toHaveLength(2);
  });
});
