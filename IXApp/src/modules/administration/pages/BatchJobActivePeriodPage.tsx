import React, { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { queryClient } from '@core/api/queryClient';
import { PERMISSIONS } from '@core/permissions/permissions';
import { useNotifications } from '@shared/hooks/useNotifications';
import { uiDensity } from '@shared/constants/uiDensity';
import {
  batchJobActivePeriodApi,
  type BatchJobActivePeriodRecord,
} from '../api/batchJobActivePeriodApi';

const queryKey = ['simple-list', 'batch-job-active-periods'] as const;
const minimumTime = 0;
const maximumTime = 86_399;

export function BatchJobActivePeriodPage(): React.ReactElement {
  const navigate = useNavigate();
  const { notifyError, notifySuccess } = useNotifications();
  const columns = useMemo<ColumnDef<BatchJobActivePeriodRecord>[]>(
    () => [
      { field: 'periodId', headerName: 'Active period', width: 150, pinned: 'left', editable: true },
      { field: 'name', headerName: 'Name', minWidth: 220, flex: 1, editable: true },
      { field: 'fromTimeUtc', headerName: 'From time UTC', type: 'number', width: 145, editable: true },
      { field: 'toTimeUtc', headerName: 'To time UTC', type: 'number', width: 145, editable: true },
      { field: 'fromTimeLocal', headerName: 'From time local', type: 'number', width: 150, editable: true },
      { field: 'toTimeLocal', headerName: 'To time local', type: 'number', width: 150, editable: true },
      { field: 'timeZoneFollowed', headerName: 'Time zone followed', type: 'number', width: 165, editable: true },
      { field: 'isActive', headerName: 'Active', type: 'boolean', width: 100, editable: true },
    ],
    []
  );
  const refresh = () => queryClient.invalidateQueries({ queryKey });
  const config: EnterpriseListConfig<BatchJobActivePeriodRecord> = {
    contextLabel: 'Batch job active periods', viewLabel: 'Standard view', filterLabel: 'Filter',
    informationLabel: 'Information', searchMode: 'quick', locale: 'en',
    searchFields: [
      { field: 'periodId', label: 'Active period' },
      { field: 'name', label: 'Name' },
    ],
    backCommand: { label: 'Back', onClick: () => navigate(-1) },
    showSearchCommand: true,
    recordTableName: 'BatchJobActivePeriod',
    getAuditRecordId: (record) => record.recId,
    crud: {
      editLabel: 'Edit', newLabel: 'New', deleteLabel: 'Delete',
      editPermission: PERMISSIONS.BACKGROUND_JOB_EDIT,
      newPermission: PERMISSIONS.BACKGROUND_JOB_CREATE,
      deletePermission: PERMISSIONS.BACKGROUND_JOB_DELETE,
      onDelete: async (rows) => {
        try {
          await Promise.all(rows.map(batchJobActivePeriodApi.remove));
          await refresh();
          notifySuccess('Active period deleted.');
        } catch (error) {
          notifyError(error instanceof Error ? error.message : 'Delete failed.');
        }
      },
    },
    utilities: {
      personalizeLabel: 'Personalize', guideLabel: 'Guide',
      notificationsLabel: 'Notifications', refreshLabel: 'Refresh',
      openWindowLabel: 'Open in new window', notificationCount: 0,
    },
    advancedFilter: {
      title: 'Filter', addLabel: 'Add', fieldLabel: 'Active period', operatorLabel: 'Contains',
      applyLabel: 'Apply', resetLabel: 'Reset', getValue: (record) => record.periodId,
      matches: (record, value) => record.periodId.toLowerCase().includes(value.trim().toLowerCase()),
    },
  };

  return <SimpleListPage title="Batch job active periods" enterpriseConfig={config}
    dataSource={{ type: 'remote', key: 'batch-job-active-periods', load: batchJobActivePeriodApi.list }}
    columns={columns} dataGridProps={{
      storageKey: 'administration.batch-job-active-periods.reference-view', masterForm: true,
      hideSidebar: false, pageSize: 50,
      rowHeight: uiDensity.gridRowHeight, headerHeight: uiDensity.gridRowHeight,
      onNewRow: () => ({ id: `new-${crypto.randomUUID()}`, recId: 0, periodId: '', name: null,
        fromTimeUtc: minimumTime, toTimeUtc: maximumTime, fromTimeLocal: minimumTime,
        toTimeLocal: maximumTime, timeZoneFollowed: 0, isActive: true }),
      onRowSave: async (values, isNew) => {
        const record = values as BatchJobActivePeriodRecord;
        if (!record.periodId.trim()) throw new Error('Active period is required.');
        if (record.periodId.trim().length > 10) throw new Error('Active period cannot exceed 10 characters.');
        if ((record.name?.length ?? 0) > 150) throw new Error('Name cannot exceed 150 characters.');
        for (const value of [record.fromTimeUtc, record.toTimeUtc, record.fromTimeLocal, record.toTimeLocal]) {
          if (!Number.isInteger(value) || value < minimumTime || value > maximumTime)
            throw new Error('Time values must be whole seconds between 0 and 86399.');
        }
        if (record.fromTimeUtc > record.toTimeUtc)
          throw new Error('From time UTC cannot be later than To time UTC.');
        if (record.fromTimeLocal > record.toTimeLocal)
          throw new Error('From time local cannot be later than To time local.');
        if (isNew || record.recId === 0) await batchJobActivePeriodApi.create(record);
        else await batchJobActivePeriodApi.update(record);
        await refresh();
        notifySuccess('Active period saved.');
      },
    }} />;
}
