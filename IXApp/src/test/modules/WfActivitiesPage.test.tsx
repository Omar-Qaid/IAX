import React from 'react';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { render, screen } from '@test/testUtils';
import { queryClient } from '@core/api/queryClient';
import { wfActivityApi, type WfActivityRecord } from '@modules/workflow/api/wfActivityApi';
import { WfActivitiesPage } from '@modules/workflow/pages/WfActivitiesPage';

const activity: WfActivityRecord = {
  id: '1', recId: 1, code: 'ACT-001', name: 'Manager review',
  description: null, sortOrder: 0, activityTypeId: 1, stepId: 10,
  performerId: 2, score: 5, sysNotificationTemplateId: null, alertingBySystem: true,
  alertingByEmail: false, alertingBySms: false, alertingByWhatsApp: false,
  showPreviousSteps: true, showPreviousDocs: false, mandatoryDocs: false,
  autoPassEnabled: false, autoPassingHrs: 0, extendedProperties: null, isActive: true,
  rowVersion: null, recVersion: 1, dataAreaId: 'dat',
};

beforeEach(() => {
  queryClient.clear();
  vi.restoreAllMocks();
  vi.spyOn(wfActivityApi, 'list').mockResolvedValue([activity]);
});

describe('WfActivitiesPage', () => {
  it('uses the Steps list-details lifecycle with backend-shaped activity data', async () => {
    render(<WfActivitiesPage />);
    expect(await screen.findByRole('heading', { name: 'Workflow activities' })).toBeDefined();
    expect((await screen.findAllByText('Manager review')).length).toBeGreaterThan(0);
    expect(screen.getByText('Activity configuration')).toBeDefined();
    expect(screen.getByText('Automatic passing hours')).toBeDefined();
    expect(screen.getByRole('button', { name: 'Edit' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'New' })).toBeDefined();
    expect(screen.getByRole('button', { name: 'Delete' })).toBeDefined();
  });
});
