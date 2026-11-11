import React, { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { queryClient } from '@core/api/queryClient';
import { PERMISSIONS } from '@core/permissions/permissions';
import { useNotifications } from '@shared/hooks/useNotifications';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { uiDensity } from '@shared/constants/uiDensity';
import {
  sysBackgroundJobGroupApi,
  type SysBackgroundJobGroupRecord,
} from '../api/sysBackgroundJobGroupApi';

const queryKey = ['simple-list', 'batch-groups'] as const;

export function SysBackgroundJobGroupPage(): React.ReactElement {
  const navigate = useNavigate();
  const { notifyError, notifySuccess } = useNotifications();
  const { t, currentLanguage } = useAppTranslation();
  const columns = useMemo<ColumnDef<SysBackgroundJobGroupRecord>[]>(() => [
    { field: 'groupCode', headerName: t('pages.batchGroups.groupCode'), width: 170, pinned: 'left', editable: true },
    { field: 'description', headerName: t('fields.description'), minWidth: 260, flex: 1, editable: true },
    { field: 'schedulingPriority', headerName: t('pages.batchGroups.schedulingPriority'), type: 'number', width: 170, editable: true },
    { field: 'maxConcurrency', headerName: t('pages.batchGroups.maxConcurrency'), type: 'number', width: 180, editable: true },
    { field: 'isActive', headerName: t('common.active'), type: 'boolean', width: 110, editable: true },
  ], [t]);
  const refresh = () => queryClient.invalidateQueries({ queryKey });
  const config: EnterpriseListConfig<SysBackgroundJobGroupRecord> = {
    contextLabel: t('pages.batchGroups.title'),
    viewLabel: t('pages.batchGroups.standardView'),
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    searchMode: 'quick',
    locale: currentLanguage.code,
    searchFields: [
      { field: 'groupCode', label: t('pages.batchGroups.groupCode') },
      { field: 'description', label: t('fields.description') },
    ],
    backCommand: { label: t('actions.back'), onClick: () => navigate(-1) },
    showSearchCommand: true,
    recordTableName: 'SysBackgroundJobGroup',
    getAuditRecordId: (record) => record.recId,
    crud: {
      editLabel: t('actions.edit'), newLabel: t('actions.new'), deleteLabel: t('actions.delete'),
      editPermission: PERMISSIONS.BACKGROUND_JOB_EDIT,
      newPermission: PERMISSIONS.BACKGROUND_JOB_CREATE,
      deletePermission: PERMISSIONS.BACKGROUND_JOB_DELETE,
      onDelete: async (rows) => {
        try {
          await Promise.all(rows.map(sysBackgroundJobGroupApi.remove));
          await refresh(); notifySuccess(t('messages.deletedSuccessfully'));
        } catch (error) { notifyError(error instanceof Error ? error.message : t('errors.deleteFailed')); }
      },
    },
    utilities: {
      personalizeLabel: t('utilities.personalize'),
      guideLabel: t('utilities.guide'),
      notificationsLabel: t('common.notifications'),
      refreshLabel: t('actions.refresh'),
      openWindowLabel: t('utilities.openWindow'),
      notificationCount: 0,
    },
    advancedFilter: {
      title: t('actions.filter'),
      addLabel: t('actions.add'),
      fieldLabel: t('pages.batchGroups.filterByGroup'),
      operatorLabel: t('pages.batchGroups.operatorContains'),
      applyLabel: t('actions.apply'),
      resetLabel: t('actions.reset'),
      getValue: (record) => record.groupCode,
      matches: (record, value) => record.groupCode
        .toLocaleLowerCase(currentLanguage.code)
        .includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
  };
  return <SimpleListPage title={t('pages.batchGroups.title')} enterpriseConfig={config}
    dataSource={{ type: 'remote', key: 'batch-groups', load: sysBackgroundJobGroupApi.list }}
    columns={columns} dataGridProps={{
      storageKey: 'administration.batch-groups.reference-view', masterForm: true,
      hideSidebar: false, pageSize: 50,
      rowHeight: uiDensity.gridRowHeight, headerHeight: uiDensity.gridRowHeight,
      onNewRow: () => ({ id: `new-${crypto.randomUUID()}`, recId: 0, groupCode: '',
        description: null, schedulingPriority: 1, maxConcurrency: 1, isActive: true, rowVersion: null }),
      onRowSave: async (values, isNew) => {
        const record = values as SysBackgroundJobGroupRecord;
        if (!record.groupCode.trim()) throw new Error(t('pages.batchGroups.groupCodeRequired'));
        if (record.groupCode.trim().length > 10) throw new Error(t('pages.batchGroups.groupCodeTooLong'));
        if ((record.description?.length ?? 0) > 60) throw new Error(t('pages.batchGroups.descriptionTooLong'));
        if (record.schedulingPriority < 0 || record.schedulingPriority > 2) throw new Error(t('pages.batchGroups.priorityRange'));
        if (record.maxConcurrency < 1 || record.maxConcurrency > 100) throw new Error(t('pages.batchGroups.concurrencyRange'));
        if (isNew || record.recId === 0) await sysBackgroundJobGroupApi.create(record);
        else await sysBackgroundJobGroupApi.update(record);
        await refresh(); notifySuccess(t('messages.savedSuccessfully'));
      },
    }} />;
}
