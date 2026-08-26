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
  vi.spyOn(wfCategoryApi, 'listPage').mockResolvedValue({ rows: [category], totalCount: 1 });
});

describe('WFCategoryPage', () => {
  it('uses the exchange-rate-types simple-list lifecycle with backend-shaped rows', async () => {
    render(<WFCategoryPage />);

    expect(screen.getByText('Workflow categories')).toBeDefined();
    expect(await screen.findByText('Procurement')).toBeDefined();
    expect(screen.getByText('WFC-0002')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Back' })).toBeDefined();
    expect(screen.queryByRole('button', { name: 'Personalize' })).toBeNull();
    expect(screen.queryByRole('button', { name: 'Page guide' })).toBeNull();
    expect(screen.queryByRole('button', { name: 'Notifications' })).toBeNull();
    expect(screen.getByRole('button', { name: 'Refresh' })).toBeDefined();
    fireEvent.click(screen.getByRole('button', { name: 'Edit' }));
    expect(screen.getByRole('button', { name: 'Save' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeDefined();
    expect(wfCategoryApi.listPage).toHaveBeenCalledWith(
      expect.objectContaining({ page: 0, pageSize: 50, isFirstPage: true })
    );
  });

  it('opens record information from the Options menu after Search', async () => {
    render(<WFCategoryPage />);

    expect(await screen.findByText('Procurement')).toBeDefined();
    fireEvent.click(screen.getByRole('button', { name: 'Options' }));
    expect(screen.getByRole('menuitem', { name: 'Record Info' })).toBeDefined();
    expect(screen.getByRole('menuitem', { name: 'Record Audit' })).toBeDefined();

    fireEvent.click(screen.getByRole('menuitem', { name: 'Record Info' }));
    expect(screen.getByText('Sys Field')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Close' })).toBeDefined();
  });
});
