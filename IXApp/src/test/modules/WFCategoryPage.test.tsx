import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { fireEvent, render, screen } from '@test/testUtils';
import { queryClient } from '@core/api/queryClient';
import { wfCategoryApi, type WfCategoryRecord } from '@modules/workflow/api/wfCategoryApi';
import { WFCategoryPage } from '@modules/workflow/pages/WFCategoryPage';

const category: WfCategoryRecord = {
  id: '2',
  recId: 2,
  code: 'WFC-0002',
  name: 'Procurement',
  description: null,
  sysField: false,
  sortOrder: 2,
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId: 'dat',
};

beforeEach(() => {
  queryClient.clear();
  vi.restoreAllMocks();
  vi.spyOn(wfCategoryApi, 'list').mockResolvedValue([category]);
});

describe('WFCategoryPage', () => {
  it('uses the exchange-rate-types simple-list lifecycle with backend-shaped rows', async () => {
    render(<WFCategoryPage />);

    expect(screen.getByText('Workflow categories')).toBeDefined();
    expect(await screen.findByText('Procurement')).toBeDefined();
    expect(screen.getByText('WFC-0002')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Back' })).toBeDefined();
    fireEvent.click(screen.getByRole('button', { name: 'Edit' }));
    expect(screen.getByRole('button', { name: 'Save' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDefined();
  });
});
