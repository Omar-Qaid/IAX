import React, { useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { queryClient } from '@core/api/queryClient';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useNotifications } from '@shared/hooks/useNotifications';
import type { WorkflowMasterDto, WorkflowMasterRecord } from '../api/workflowMasterApi';

interface WorkflowSetupApi<TDto extends WorkflowMasterDto> {
  list(signal?: AbortSignal): Promise<WorkflowMasterRecord<TDto>[]>;
  create(record: WorkflowMasterRecord<TDto>): Promise<WorkflowMasterRecord<TDto>>;
  update(record: WorkflowMasterRecord<TDto>): Promise<WorkflowMasterRecord<TDto>>;
  delete(record: WorkflowMasterRecord<TDto>): Promise<void>;
}

export interface WorkflowSetupField<TDto extends WorkflowMasterDto> {
  field: keyof WorkflowMasterRecord<TDto> & string;
  labelKey: string;
  width?: number;
  required?: boolean;
}

interface WorkflowSetupListPageProps<TDto extends WorkflowMasterDto> {
  titleKey: string;
  resourceKey: string;
  api: WorkflowSetupApi<TDto>;
  createRecord: () => WorkflowMasterRecord<TDto>;
  generatedCode: boolean;
  permissions?: { create: string; edit: string; delete: string };
  extraFields?: WorkflowSetupField<TDto>[];
}

export function WorkflowSetupListPage<TDto extends WorkflowMasterDto>({
  titleKey,
  resourceKey,
  api,
  createRecord,
  generatedCode,
  permissions,
  extraFields = [],
}: WorkflowSetupListPageProps<TDto>): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const { notifyError, notifySuccess } = useNotifications();
  const navigate = useNavigate();
  const queryKey = useMemo(() => ['simple-list', resourceKey] as const, [resourceKey]);
  const columns = useMemo<ColumnDef<WorkflowMasterRecord<TDto>>[]>(
    () => [
      { field: 'code', headerName: 'workflowSetup.fields.code', width: 140, pinned: 'left', editable: !generatedCode },
      { field: 'name', headerName: 'workflowSetup.fields.name', minWidth: 190, flex: 1, editable: true },
      { field: 'nameAR', headerName: 'workflowSetup.fields.nameAR', minWidth: 190, flex: 1, editable: true },
      { field: 'description', headerName: 'workflowSetup.fields.description', minWidth: 220, flex: 1, editable: true },
      { field: 'descriptionAR', headerName: 'workflowSetup.fields.descriptionAR', minWidth: 220, flex: 1, editable: true },
      ...extraFields.map<ColumnDef<WorkflowMasterRecord<TDto>>>((field) => ({
        field: field.field,
        headerName: field.labelKey,
        width: field.width ?? 150,
        editable: true,
      })),
      { field: 'sortOrder', headerName: 'workflowSetup.fields.sortOrder', width: 110, type: 'number', editable: true },
    ],
    [extraFields, generatedCode]
  );
  const refresh = async () => queryClient.invalidateQueries({ queryKey });
  const config: EnterpriseListConfig<WorkflowMasterRecord<TDto>> = {
    contextLabel: t(titleKey), viewLabel: t('common.standardView'), filterLabel: t('actions.filter'),
    informationLabel: t('common.information'), searchMode: 'quick', locale: currentLanguage.code,
    searchFields: [
      { field: 'code', label: t('workflowSetup.fields.code') },
      { field: 'name', label: t('workflowSetup.fields.name') },
      { field: 'nameAR', label: t('workflowSetup.fields.nameAR') },
    ],
    backCommand: { label: t('actions.back', 'Back'), onClick: () => navigate(-1) },
    showSearchCommand: true,
    crud: {
      editLabel: t('actions.edit'), newLabel: t('actions.new'), deleteLabel: t('actions.delete'),
      editPermission: permissions?.edit, newPermission: permissions?.create, deletePermission: permissions?.delete,
      onDelete: async (rows) => {
        try {
          await Promise.all(rows.map((row) => api.delete(row)));
          await refresh();
          notifySuccess(t('messages.deletedSuccessfully', 'Deleted successfully'));
        } catch (error) {
          notifyError(error instanceof Error ? error.message : t('errors.deleteFailed', 'Delete failed'));
        }
      },
    },
    commands: [{ id: 'options', label: t('customerCommands.options') }],
    utilities: { personalizeLabel: t('utilities.personalize'), guideLabel: t('utilities.guide'), notificationsLabel: t('common.notifications'), refreshLabel: t('actions.refresh'), openWindowLabel: t('utilities.openWindow'), notificationCount: 0 },
    advancedFilter: {
      title: t('filters.title'), addLabel: t('actions.add'), fieldLabel: t('workflowSetup.fields.name'),
      operatorLabel: t('filters.contains'), applyLabel: t('actions.apply'), resetLabel: t('actions.reset'),
      getValue: (record) => record.name,
      matches: (record, value) => (record.name ?? '').toLocaleLowerCase(currentLanguage.code).includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
  };

  return <SimpleListPage variant="enterprise" title={t(titleKey)} enterpriseConfig={config}
    dataSource={{ type: 'remote', key: resourceKey, load: (signal) => api.list(signal) }} columns={columns}
    dataGridProps={{ storageKey: `workflow.${resourceKey}.reference-view`, masterForm: true, hideSidebar: false, rowHeight: 42, headerHeight: 40,
      onNewRow: createRecord,
      onRowSave: async (values, isNew) => {
        const record = values as WorkflowMasterRecord<TDto>;
        if (!generatedCode && !record.code?.trim()) throw new Error(t('validation.required', { field: t('workflowSetup.fields.code') }));
        if (!record.name?.trim()) throw new Error(t('validation.required', { field: t('workflowSetup.fields.name') }));
        if (!record.nameAR?.trim()) throw new Error(t('validation.required', { field: t('workflowSetup.fields.nameAR') }));
        const missingExtra = extraFields.find((field) => field.required && !String(record[field.field] ?? '').trim());
        if (missingExtra) throw new Error(t('validation.required', { field: t(missingExtra.labelKey) }));
        if (isNew || record.recId === 0) await api.create(record); else await api.update(record);
        await refresh(); notifySuccess(t('messages.savedSuccessfully', 'Saved successfully'));
      },
    }} />;
}
