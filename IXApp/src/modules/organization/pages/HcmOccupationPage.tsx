import React, { useMemo } from 'react';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { queryClient } from '@core/api/queryClient';
import { useNotifications } from '@shared/hooks/useNotifications';
import { uiDensity } from '@shared/constants/uiDensity';
import { hcmOccupationApi, type HcmReferenceRecord } from '../api/hcmReferenceApi';
import { useNavigate } from 'react-router-dom';

const queryKey = ['simple-list', 'hcm-occupations'] as const;

export function HcmOccupationPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const { notifyError, notifySuccess } = useNotifications();
  const navigate = useNavigate();

  const columns = useMemo<ColumnDef<HcmReferenceRecord>[]>(() => [
    { field: 'code', headerName: 'hcmOccupations.fields.code', width: 140, pinned: 'left', editable: true },
    { field: 'name', headerName: 'fields.name', minWidth: 220, flex: 1, editable: true },
    { field: 'nameAlias', headerName: 'fields.nameAlias', minWidth: 220, flex: 1, editable: true },
    { field: 'description', headerName: 'fields.description', minWidth: 260, flex: 1.5, editable: true },
  ], []);

  const refresh = async () => queryClient.invalidateQueries({ queryKey });

  const config: EnterpriseListConfig<HcmReferenceRecord> = {
    contextLabel: t('hcmOccupations.title'),
    viewLabel: t('common.standardView'),
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    searchMode: 'quick',
    searchFields: [
      { field: 'code', label: t('hcmOccupations.fields.code') },
      { field: 'name', label: t('fields.name') },
      { field: 'nameAlias', label: t('fields.nameAlias') },
    ],
    locale: currentLanguage.code,
    backCommand: { label: t('actions.back'), onClick: () => navigate(-1) },
    showSearchCommand: true,
    recordTableName: 'HcmOccupation',
    getAuditRecordId: (record) => record.recId,
    crud: {
      editLabel: t('actions.edit'),
      newLabel: t('actions.new'),
      deleteLabel: t('actions.delete'),
      editPermission: PERMISSIONS.ORGANIZATION_STRUCTURE_VIEW,
      newPermission: PERMISSIONS.ORGANIZATION_STRUCTURE_VIEW,
      deletePermission: PERMISSIONS.ORGANIZATION_STRUCTURE_VIEW,
      onDelete: async (rows) => {
        try {
          await Promise.all(rows.map((row) => hcmOccupationApi.delete(row)));
          await refresh();
          notifySuccess(t('messages.deletedSuccessfully'));
        } catch (error) {
          notifyError(error instanceof Error ? error.message : t('errors.deleteFailed'));
        }
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
      title: t('filters.title'),
      addLabel: t('actions.add'),
      fieldLabel: t('hcmOccupations.fields.code'),
      operatorLabel: t('filters.contains'),
      applyLabel: t('actions.apply'),
      resetLabel: t('actions.reset'),
      getValue: (record) => record.code,
      matches: (record, value) =>
        record.code.toLocaleLowerCase(currentLanguage.code).includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
  };

  return (
    <SimpleListPage
      title={t('hcmOccupations.title')}
      enterpriseConfig={config}
      dataSource={{
        type: 'remote',
        key: 'hcm-occupations',
        load: (signal) => hcmOccupationApi.list(signal),
      }}
      columns={columns}
      dataGridProps={{
        storageKey: 'organization.hcm-occupations.reference-view',
        masterForm: true,
        hideSidebar: false,
        rowHeight: uiDensity.gridRowHeight,
        headerHeight: uiDensity.gridRowHeight,
        onNewRow: () => ({
          id: `new-${crypto.randomUUID()}`,
          recId: 0,
          code: '',
          name: '',
          nameAlias: '',
          description: '',
          isActive: true,
          dataAreaId: 'dat',
          recVersion: 1,
          rowVersion: null,
        }),
        onRowSave: async (values, isNew) => {
          const record = values as HcmReferenceRecord;
          if (!record.code.trim()) throw new Error(t('validation.required', { field: t('hcmOccupations.fields.code') }));
          if (!record.name.trim()) throw new Error(t('validation.required', { field: t('fields.name') }));
          if (isNew || record.recId === 0) {
            await hcmOccupationApi.create(record);
          } else {
            await hcmOccupationApi.update(record);
          }
          await refresh();
          notifySuccess(t('messages.savedSuccessfully'));
        },
      }}
    />
  );
}
