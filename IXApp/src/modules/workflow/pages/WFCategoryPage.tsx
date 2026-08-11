import React, { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { queryClient } from '@core/api/queryClient';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useNotifications } from '@shared/hooks/useNotifications';
import { wfCategoryApi, type WfCategoryRecord } from '../api/wfCategoryApi';

const queryKey = ['simple-list', 'workflow-categories'] as const;

export function WFCategoryPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const { notifyError, notifySuccess } = useNotifications();
  const navigate = useNavigate();
  const columns = useMemo<ColumnDef<WfCategoryRecord>[]>(
    () => [
      { field: 'code', headerName: 'wfCategory.fields.code', width: 150, pinned: 'left' },
      {
        field: 'name',
        headerName: 'wfCategory.fields.name',
        minWidth: 220,
        flex: 1,
        editable: true,
      },
      {
        field: 'nameAR',
        headerName: 'wfCategory.fields.nameAR',
        minWidth: 220,
        flex: 1,
        editable: true,
      },
      {
        field: 'sortOrder',
        headerName: 'wfCategory.fields.sortOrder',
        width: 110,
        type: 'number',
        editable: true,
      },
      {
        field: 'sysField',
        headerName: 'wfCategory.fields.systemCategory',
        width: 130,
        type: 'boolean',
      },
    ],
    []
  );
  const refresh = async () => queryClient.invalidateQueries({ queryKey });
  const config: EnterpriseListConfig<WfCategoryRecord> = {
    contextLabel: t('pages.wfCategories.title'),
    viewLabel: t('common.standardView'),
    filterLabel: t('actions.filter'),
    informationLabel: t('common.information'),
    searchMode: 'quick',
    searchFields: [
      { field: 'code', label: t('wfCategory.fields.code') },
      { field: 'name', label: t('wfCategory.fields.name') },
      { field: 'nameAR', label: t('wfCategory.fields.nameAR') },
    ],
    locale: currentLanguage.code,
    backCommand: { label: t('actions.back', 'Back'), onClick: () => navigate(-1) },
    showSearchCommand: true,
    crud: {
      editLabel: t('actions.edit'),
      newLabel: t('actions.new'),
      deleteLabel: t('actions.delete'),
      editPermission: 'Workflow.Categories.Edit',
      newPermission: 'Workflow.Categories.Create',
      deletePermission: 'Workflow.Categories.Delete',
      onDelete: async (rows) => {
        try {
          await Promise.all(rows.map((row) => wfCategoryApi.delete(row)));
          await refresh();
          notifySuccess(t('messages.deletedSuccessfully', 'Deleted successfully'));
        } catch (error) {
          notifyError(
            error instanceof Error ? error.message : t('errors.deleteFailed', 'Delete failed')
          );
        }
      },
    },
    commands: [{ id: 'options', label: t('customerCommands.options') }],
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
      fieldLabel: t('wfCategory.fields.name'),
      operatorLabel: t('filters.contains'),
      applyLabel: t('actions.apply'),
      resetLabel: t('actions.reset'),
      getValue: (record) => record.name,
      matches: (record, value) =>
        (record.name ?? '')
          .toLocaleLowerCase(currentLanguage.code)
          .includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
  };

  return (
    <SimpleListPage
      variant="enterprise"
      title={t('pages.wfCategories.title')}
      enterpriseConfig={config}
      dataSource={{
        type: 'remote',
        key: 'workflow-categories',
        load: (signal) => wfCategoryApi.list(signal),
      }}
      columns={columns}
      dataGridProps={{
        storageKey: 'workflow.categories.reference-view',
        masterForm: true,
        hideSidebar: false,
        rowHeight: 42,
        headerHeight: 40,
        onNewRow: () => ({
          id: `new-${crypto.randomUUID()}`,
          recId: 0,
          code: null,
          name: '',
          nameAR: '',
          description: null,
          descriptionAR: null,
          sysField: false,
          sortOrder: 0,
          isActive: true,
          rowVersion: null,
          recVersion: 1,
          dataAreaId: 'dat',
        }),
        onRowSave: async (values, isNew) => {
          const record = values as WfCategoryRecord;
          if (!record.name?.trim()) {
            throw new Error(t('validation.required', { field: t('wfCategory.fields.name') }));
          }
          if (!record.nameAR?.trim()) {
            throw new Error(t('validation.required', { field: t('wfCategory.fields.nameAR') }));
          }
          if (isNew || record.recId === 0) await wfCategoryApi.create(record);
          else await wfCategoryApi.update(record);
          await refresh();
          notifySuccess(t('messages.savedSuccessfully', 'Saved successfully'));
        },
      }}
    />
  );
}
