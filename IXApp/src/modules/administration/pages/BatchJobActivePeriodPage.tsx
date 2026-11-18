import React, { useMemo } from 'react';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { queryClient } from '@core/api/queryClient';
import { PERMISSIONS } from '@core/permissions/permissions';
import { useNotifications } from '@shared/hooks/useNotifications';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import {
  batchJobActivePeriodApi,
  type BatchJobActivePeriodRecord,
} from '../api/batchJobActivePeriodApi';

const queryKey = ['simple-list', 'batch-job-active-periods'] as const;
const minimumTime = 0;
const maximumTime = 86_399;

export function BatchJobActivePeriodPage(): React.ReactElement {
  const { notifyError, notifySuccess } = useNotifications();
  const { t } = useAppTranslation();
  const columns = useMemo<ColumnDef<BatchJobActivePeriodRecord>[]>(
    () => [
      { field: 'periodId', headerName: t('pages.batchJobActivePeriods.periodId'), width: 150, pinned: 'left', editable: true },
      { field: 'name', headerName: t('fields.name'), minWidth: 220, flex: 1, editable: true },
      { field: 'fromTimeUtc', headerName: t('pages.batchJobActivePeriods.fromTimeUtc'), type: 'number', width: 145, editable: true },
      { field: 'toTimeUtc', headerName: t('pages.batchJobActivePeriods.toTimeUtc'), type: 'number', width: 145, editable: true },
      { field: 'fromTimeLocal', headerName: t('pages.batchJobActivePeriods.fromTimeLocal'), type: 'number', width: 150, editable: true },
      { field: 'toTimeLocal', headerName: t('pages.batchJobActivePeriods.toTimeLocal'), type: 'number', width: 150, editable: true },
      { field: 'timeZoneFollowed', headerName: t('pages.batchJobActivePeriods.timeZoneFollowed'), type: 'number', width: 165, editable: true },
      { field: 'isActive', headerName: t('common.active'), type: 'boolean', width: 100, editable: true },
    ],
    [t]
  );
  const refresh = () => queryClient.invalidateQueries({ queryKey });
  const config: EnterpriseListConfig<BatchJobActivePeriodRecord> = {
    contextLabel: t('pages.batchJobActivePeriods.title'),
    viewLabel: t('pages.batchJobActivePeriods.standardView'),
    searchMode: 'quick',
    searchFields: [
      { field: 'periodId', label: t('pages.batchJobActivePeriods.periodId') },
      { field: 'name', label: t('fields.name') },
    ],
    recordTableName: 'BatchJobActivePeriod',
    getAuditRecordId: (record) => record.recId,
    crud: {
      editPermission: PERMISSIONS.BACKGROUND_JOB_EDIT,
      newPermission: PERMISSIONS.BACKGROUND_JOB_CREATE,
      deletePermission: PERMISSIONS.BACKGROUND_JOB_DELETE,
      onDelete: async (rows) => {
        try {
          await Promise.all(rows.map(batchJobActivePeriodApi.remove));
          await refresh();
          notifySuccess(t('messages.deletedSuccessfully'));
        } catch (error) {
          notifyError(error instanceof Error ? error.message : t('errors.deleteFailed'));
        }
      },
    },
    advancedFilter: {
      title: t('actions.filter'),
      fieldLabel: t('pages.batchJobActivePeriods.periodId'),
      operatorLabel: t('pages.batchJobActivePeriods.operatorContains'),
      getValue: (record) => record.periodId,
    },
  };

  return <SimpleListPage title={t('pages.batchJobActivePeriods.title')} enterpriseConfig={config}
    dataSource={{ type: 'remote', key: 'batch-job-active-periods', load: batchJobActivePeriodApi.list }}
    columns={columns} dataGridProps={{
      storageKey: 'administration.batch-job-active-periods.reference-view', masterForm: true,
      hideSidebar: false, pageSize: 50,

      onNewRow: () => ({ id: `new-${crypto.randomUUID()}`, recId: 0, periodId: '', name: null,
        fromTimeUtc: minimumTime, toTimeUtc: maximumTime, fromTimeLocal: minimumTime,
        toTimeLocal: maximumTime, timeZoneFollowed: 0, isActive: true }),
      onRowSave: async (values, isNew) => {
        const record = values as BatchJobActivePeriodRecord;
        if (!record.periodId.trim()) throw new Error(t('pages.batchJobActivePeriods.periodIdRequired'));
        if (record.periodId.trim().length > 10) throw new Error(t('pages.batchJobActivePeriods.periodIdTooLong'));
        if ((record.name?.length ?? 0) > 150) throw new Error(t('pages.batchJobActivePeriods.nameTooLong'));
        for (const value of [record.fromTimeUtc, record.toTimeUtc, record.fromTimeLocal, record.toTimeLocal]) {
          if (!Number.isInteger(value) || value < minimumTime || value > maximumTime)
            throw new Error(t('pages.batchJobActivePeriods.timeRange'));
        }
        if (record.fromTimeUtc > record.toTimeUtc)
          throw new Error(t('pages.batchJobActivePeriods.utcOrder'));
        if (record.fromTimeLocal > record.toTimeLocal)
          throw new Error(t('pages.batchJobActivePeriods.localOrder'));
        if (isNew || record.recId === 0) await batchJobActivePeriodApi.create(record);
        else await batchJobActivePeriodApi.update(record);
        await refresh();
        notifySuccess(t('messages.savedSuccessfully'));
      },
    }} />;
}
