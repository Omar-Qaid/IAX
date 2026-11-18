import React, { useMemo } from 'react';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { queryClient } from '@core/api/queryClient';
import { useNotifications } from '@shared/hooks/useNotifications';
import type { createHcmReferenceApi, HcmReferenceRecord } from '../api/hcmReferenceApi';
import { useNavigate } from 'react-router-dom';

interface HcmReferencePageProps {
  titleKey: string;
  codeLabelKey: string;
  recordTableName: string;
  sourceKey: string;
  storageKey: string;
  api: ReturnType<typeof createHcmReferenceApi>;
}

/** Shared organization reference grid; adapters and record identities stay feature-owned. */
export function HcmReferencePage({
  titleKey,
  codeLabelKey,
  recordTableName,
  sourceKey,
  storageKey,
  api,
}: HcmReferencePageProps): React.ReactElement {
  const queryKey = ['simple-list', sourceKey] as const;
  const { t, currentLanguage } = useAppTranslation();
  const { notifyError, notifySuccess } = useNotifications();
  const navigate = useNavigate();

  const columns = useMemo<ColumnDef<HcmReferenceRecord>[]>(
    () => [
      { field: 'code', headerName: codeLabelKey, width: 140, pinned: 'left', editable: true },
      { field: 'name', headerName: 'fields.name', minWidth: 220, flex: 1, editable: true },
      {
        field: 'nameAlias',
        headerName: 'fields.nameAlias',
        minWidth: 220,
        flex: 1,
        editable: true,
      },
      {
        field: 'description',
        headerName: 'fields.description',
        minWidth: 260,
        flex: 1.5,
        editable: true,
      },
    ],
    [codeLabelKey]
  );

  const refresh = async () => queryClient.invalidateQueries({ queryKey });

  const config: EnterpriseListConfig<HcmReferenceRecord> = {
    contextLabel: t(titleKey),
    searchMode: 'quick',
    searchFields: [
      { field: 'code', label: t(codeLabelKey) },
      { field: 'name', label: t('fields.name') },
      { field: 'nameAlias', label: t('fields.nameAlias') },
    ],
    locale: currentLanguage.code,
    backCommand: { label: t('actions.back'), onClick: () => navigate(-1) },
    showSearchCommand: true,
    recordTableName,
    getAuditRecordId: (record) => record.recId,
    crud: {
      editPermission: PERMISSIONS.ORGANIZATION_STRUCTURE_VIEW,
      newPermission: PERMISSIONS.ORGANIZATION_STRUCTURE_VIEW,
      deletePermission: PERMISSIONS.ORGANIZATION_STRUCTURE_VIEW,
      onDelete: async (rows) => {
        try {
          await Promise.all(rows.map((row) => api.delete(row)));
          await refresh();
          notifySuccess(t('messages.deletedSuccessfully'));
        } catch (error) {
          notifyError(error instanceof Error ? error.message : t('errors.deleteFailed'));
        }
      },
    },
    advancedFilter: {
      title: t('filters.title'),
      fieldLabel: t(codeLabelKey),
      operatorLabel: t('filters.contains'),
      getValue: (record) => record.code,
      matches: (record, value) =>
        record.code
          .toLocaleLowerCase(currentLanguage.code)
          .includes(value.trim().toLocaleLowerCase(currentLanguage.code)),
    },
  };

  return (
    <SimpleListPage
      title={t(titleKey)}
      enterpriseConfig={config}
      dataSource={{
        type: 'remote',
        key: sourceKey,
        load: (signal) => api.list(signal),
      }}
      columns={columns}
      dataGridProps={{
        storageKey,
        masterForm: true,
        hideSidebar: false,
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
          if (!record.code.trim())
            throw new Error(t('validation.required', { field: t(codeLabelKey) }));
          if (!record.name.trim())
            throw new Error(t('validation.required', { field: t('fields.name') }));
          if (isNew || record.recId === 0) {
            await api.create(record);
          } else {
            await api.update(record);
          }
          await refresh();
          notifySuccess(t('messages.savedSuccessfully'));
        },
      }}
    />
  );
}
