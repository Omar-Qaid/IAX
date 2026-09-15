import React, { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { queryClient } from '@core/api/queryClient';
import { PERMISSIONS } from '@core/permissions/permissions';
import { useNotifications } from '@shared/hooks/useNotifications';
import { uiDensity } from '@shared/constants/uiDensity';
import {
  sysBackgroundJobGroupApi,
  type SysBackgroundJobGroupRecord,
} from '../api/sysBackgroundJobGroupApi';

const queryKey = ['simple-list', 'batch-groups'] as const;

export function SysBackgroundJobGroupPage(): React.ReactElement {
  const navigate = useNavigate();
  const { notifyError, notifySuccess } = useNotifications();
  const columns = useMemo<ColumnDef<SysBackgroundJobGroupRecord>[]>(() => [
    { field: 'groupCode', headerName: 'Batch group', width: 170, pinned: 'left', editable: true },
    { field: 'description', headerName: 'Description', minWidth: 260, flex: 1, editable: true },
    { field: 'schedulingPriority', headerName: 'Scheduling priority', type: 'number', width: 170, editable: true },
    { field: 'maxConcurrency', headerName: 'Maximum concurrency', type: 'number', width: 180, editable: true },
    { field: 'isActive', headerName: 'Active', type: 'boolean', width: 110, editable: true },
  ], []);
  const refresh = () => queryClient.invalidateQueries({ queryKey });
  const config: EnterpriseListConfig<SysBackgroundJobGroupRecord> = {
    contextLabel: 'Batch groups', viewLabel: 'Standard view', filterLabel: 'Filter',
    informationLabel: 'Information', searchMode: 'quick', locale: 'en',
    searchFields: [
      { field: 'groupCode', label: 'Batch group' },
      { field: 'description', label: 'Description' },
    ],
    backCommand: { label: 'Back', onClick: () => navigate(-1) },
    showSearchCommand: true,
    recordTableName: 'SysBackgroundJobGroup',
    getAuditRecordId: (record) => record.recId,
    crud: {
      editLabel: 'Edit', newLabel: 'New', deleteLabel: 'Delete',
      editPermission: PERMISSIONS.BACKGROUND_JOB_EDIT,
      newPermission: PERMISSIONS.BACKGROUND_JOB_CREATE,
      deletePermission: PERMISSIONS.BACKGROUND_JOB_DELETE,
      onDelete: async (rows) => {
        try {
          await Promise.all(rows.map(sysBackgroundJobGroupApi.remove));
          await refresh(); notifySuccess('Batch group deleted.');
        } catch (error) { notifyError(error instanceof Error ? error.message : 'Delete failed.'); }
      },
    },
    utilities: {
      personalizeLabel: 'Personalize', guideLabel: 'Guide', notificationsLabel: 'Notifications',
      refreshLabel: 'Refresh', openWindowLabel: 'Open in new window', notificationCount: 0,
    },
    advancedFilter: {
      title: 'Filter', addLabel: 'Add', fieldLabel: 'Batch group', operatorLabel: 'Contains',
      applyLabel: 'Apply', resetLabel: 'Reset', getValue: (record) => record.groupCode,
      matches: (record, value) => record.groupCode.toLowerCase().includes(value.trim().toLowerCase()),
    },
  };
  return <SimpleListPage title="Batch groups" enterpriseConfig={config}
    dataSource={{ type: 'remote', key: 'batch-groups', load: sysBackgroundJobGroupApi.list }}
    columns={columns} dataGridProps={{
      storageKey: 'administration.batch-groups.reference-view', masterForm: true,
      hideSidebar: false, pageSize: 50,
      rowHeight: uiDensity.gridRowHeight, headerHeight: uiDensity.gridRowHeight,
      onNewRow: () => ({ id: `new-${crypto.randomUUID()}`, recId: 0, groupCode: '',
        description: null, schedulingPriority: 1, maxConcurrency: 1, isActive: true, rowVersion: null }),
      onRowSave: async (values, isNew) => {
        const record = values as SysBackgroundJobGroupRecord;
        if (!record.groupCode.trim()) throw new Error('Batch group is required.');
        if (record.groupCode.trim().length > 10) throw new Error('Batch group cannot exceed 10 characters.');
        if ((record.description?.length ?? 0) > 60) throw new Error('Description cannot exceed 60 characters.');
        if (record.schedulingPriority < 0 || record.schedulingPriority > 2) throw new Error('Priority must be 0, 1, or 2.');
        if (record.maxConcurrency < 1 || record.maxConcurrency > 100) throw new Error('Maximum concurrency must be between 1 and 100.');
        if (isNew || record.recId === 0) await sysBackgroundJobGroupApi.create(record);
        else await sysBackgroundJobGroupApi.update(record);
        await refresh(); notifySuccess('Batch group saved.');
      },
    }} />;
}
