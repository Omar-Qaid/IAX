import React, { useMemo } from 'react';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { queryClient } from '@core/api/queryClient';
import { useNotifications } from '@shared/hooks/useNotifications';
import { exchangeRateTypeApi, type ExchangeRateTypeRecord } from '../api/exchangeRateTypeApi';

const queryKey = ['simple-list', 'foundation-exchange-rate-types'] as const;

export function ExchangeRateTypePage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const { notifyError, notifySuccess } = useNotifications();
  const columns = useMemo<ColumnDef<ExchangeRateTypeRecord>[]>(() => [
    { field: 'type', headerName: 'exchangeRateTypes.fields.type', width: 150, pinned: 'left', editable: true },
    { field: 'name', headerName: 'fields.name', minWidth: 240, flex: 1, editable: true },
  ], []);
  const refresh = async () => queryClient.invalidateQueries({ queryKey });
  const config: EnterpriseListConfig<ExchangeRateTypeRecord> = {
    contextLabel: t('pages.exchangeRateTypes.title'),
    searchMode: 'quick',
    searchFields: [{ field: 'type', label: t('exchangeRateTypes.fields.type') }, { field: 'name', label: t('fields.name') }],
    recordTableName: 'ExchangeRateType',
    getAuditRecordId: (record) => record.recId,
    crud: {
      editPermission: PERMISSIONS.CURRENCY_MANAGE, newPermission: PERMISSIONS.CURRENCY_MANAGE, deletePermission: PERMISSIONS.CURRENCY_MANAGE,
      onDelete: async (rows) => {
        try {
          await Promise.all(rows.map((row) => exchangeRateTypeApi.delete(row)));
          await refresh();
          notifySuccess(t('messages.deletedSuccessfully'));
        } catch (error) {
          notifyError(error instanceof Error ? error.message : t('errors.deleteFailed'));
        }
      },
    },
    commands: [{ id: 'exchangeRates', label: t('exchangeRateTypes.commands.exchangeRates') }],
    advancedFilter: { fieldLabel: t('exchangeRateTypes.fields.type'), operatorLabel: t('filters.contains'), getValue: (record) => record.type, matches: (record, value) => record.type.toLocaleLowerCase(currentLanguage.code).includes(value.trim().toLocaleLowerCase(currentLanguage.code)) },
  };
  return <SimpleListPage title={t('pages.exchangeRateTypes.title')} enterpriseConfig={config}
    dataSource={{ type: 'remote', key: 'foundation-exchange-rate-types', load: (signal) => exchangeRateTypeApi.list(signal) }} columns={columns} dataGridProps={{
    storageKey: 'foundation.exchange-rate-types.reference-view', masterForm: true, hideSidebar: false,
    onNewRow: () => ({ id: `new-${crypto.randomUUID()}`, recId: 0, type: '', name: '', isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'dat' }),
    onRowSave: async (values, isNew) => {
      const record = values as ExchangeRateTypeRecord;
      if (!record.type.trim()) throw new Error(t('validation.required', { field: t('exchangeRateTypes.fields.type') }));
      if (!record.name.trim()) throw new Error(t('validation.required', { field: t('fields.name') }));
      if (isNew || record.recId === 0) await exchangeRateTypeApi.create(record);
      else await exchangeRateTypeApi.update(record);
      await refresh();
      notifySuccess(t('messages.savedSuccessfully'));
    },
  }} />;
}
