import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen } from '@test/testUtils';
import { queryClient } from '@core/api/queryClient';
import {
  sysNumberSequenceApi,
  type SysNumberSequenceRecord,
} from '@modules/administration/api/sysNumberSequenceApi';
import { SysNumberSequencePage } from '@modules/administration/pages/SysNumberSequencePage';

const sequence: SysNumberSequenceRecord = {
  id: '1',
  recId: 1,
  numberSequence: 'WfActivity',
  txt: 'Workflow activity code',
  latestCleanDateTime: null,
  latestCleanDateTimeTzId: null,
  lowest: 1,
  highest: 999999,
  nextRec: 42,
  blocked: 0,
  format: 'ACT-######',
  continuous: 0,
  cyclic: 0,
  annotatedFormat: '{PREFIX}-{SEQ}',
  cleanAtAccess: 0,
  inUse: 1,
  noIncrement: 0,
  numberSequenceScope: null,
  cleanInterval: null,
  allowChangeUp: 0,
  allowChangeDown: 0,
  manual: 0,
  fetchAheadQty: null,
  fetchAhead: 0,
  modifiedTransactionId: null,
  partition: null,
  isActive: true,
  rowVersion: null,
  isDeleted: false,
  recVersion: 1,
  dataAreaId: 'dat',
};

beforeEach(() => {
  queryClient.clear();
  vi.restoreAllMocks();
  vi.spyOn(sysNumberSequenceApi, 'list').mockResolvedValue([sequence]);
});

describe('SysNumberSequencePage', () => {
  it('uses the activities enterprise list-details lifecycle with backend-shaped data', async () => {
    render(<SysNumberSequencePage />);

    expect(await screen.findByRole('heading', { name: 'Number sequences' })).toBeDefined();
    expect((await screen.findAllByText('WfActivity')).length).toBeGreaterThan(0);
    expect(await screen.findByText('Number sequence code')).toBeDefined();
    expect(screen.getByText('Scope parameters')).toBeDefined();
    expect(screen.getByText('Segments')).toBeDefined();
    expect(screen.getByText('General')).toBeDefined();
    expect(screen.getByText('Automatic cleanup')).toBeDefined();
    expect(screen.getByText('Performance')).toBeDefined();
    expect(screen.getByText('Company')).toBeDefined();
    expect(screen.getByText('Constant')).toBeDefined();
    expect(screen.getByText('Alphanumeric')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Move up' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Move down' })).toBeDefined();
    expect(screen.getByText('Smallest')).toBeDefined();
    expect(screen.getByText('Quantity of numbers')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Edit' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'New' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Delete' })).toBeDefined();
  });
});
