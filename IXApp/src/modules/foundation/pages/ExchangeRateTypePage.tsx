import React, { useMemo, useState } from 'react';
import { SimpleListPage, type EnterpriseListConfig } from '@patterns/simple-list/SimpleListPage';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';

interface ExchangeRateType { id: string; type: string; name: string; calendar: string }
const INITIAL_EXCHANGE_RATE_TYPES: ExchangeRateType[] = [
  { id: 'rate-average', type: 'Average', name: 'Default average rate', calendar: '' },
  { id: 'rate-budget', type: 'Budget', name: 'Default budget rate', calendar: '' },
  { id: 'rate-closing', type: 'Closing', name: 'Default closing rate', calendar: '' },
  { id: 'rate-default', type: 'Default', name: 'Default global rate', calendar: '' },
];

export function ExchangeRateTypePage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const [records, setRecords] = useState(INITIAL_EXCHANGE_RATE_TYPES);
  const columns = useMemo<ColumnDef<ExchangeRateType>[]>(() => [
    { field: 'type', headerName: 'exchangeRateTypes.fields.type', width: 150, pinned: 'left', editable: true },
    { field: 'name', headerName: 'fields.name', width: 240, editable: true },
    { field: 'calendar', headerName: 'exchangeRateTypes.fields.calendar', minWidth: 220, flex: 1, editable: true },
  ], []);
  const config: EnterpriseListConfig<ExchangeRateType> = {
    contextLabel: t('pages.exchangeRateTypes.title'), viewLabel: t('common.standardView'),
    filterLabel: t('actions.filter'), informationLabel: t('common.information'), searchMode: 'quick',
    searchFields: [{ field: 'type', label: t('exchangeRateTypes.fields.type') }, { field: 'name', label: t('fields.name') }, { field: 'calendar', label: t('exchangeRateTypes.fields.calendar') }],
    locale: currentLanguage.code,
    crud: { editLabel: t('actions.edit'), newLabel: t('actions.new'), deleteLabel: t('actions.delete'), editPermission: 'currency.manage', newPermission: 'currency.manage', deletePermission: 'currency.manage' },
    commands: ['exchangeRates', 'options'].map((id) => ({ id, label: t(`exchangeRateTypes.commands.${id}`) })),
    utilities: { personalizeLabel: t('utilities.personalize'), guideLabel: t('utilities.guide'), notificationsLabel: t('common.notifications'), refreshLabel: t('actions.refresh'), openWindowLabel: t('utilities.openWindow'), notificationCount: 0 },
    advancedFilter: { title: t('filters.title'), addLabel: t('actions.add'), fieldLabel: t('exchangeRateTypes.fields.type'), operatorLabel: t('filters.contains'), applyLabel: t('actions.apply'), resetLabel: t('actions.reset'), getValue: (record) => record.type, matches: (record, value) => record.type.toLocaleLowerCase(currentLanguage.code).includes(value.trim().toLocaleLowerCase(currentLanguage.code)) },
  };
  return <SimpleListPage variant="enterprise" title={t('pages.exchangeRateTypes.title')} enterpriseConfig={config} dataSource={{ type: 'controlled', rows: records }} columns={columns} dataGridProps={{
    storageKey: 'foundation.exchange-rate-types.reference-view', masterForm: true, hideSidebar: false,
    onNewRow: () => ({ id: `rate-${Date.now()}`, type: '', name: '', calendar: '' }),
    onRowSave: (values, isNew) => { const saved = values as ExchangeRateType; setRecords((current) => isNew ? [...current, saved] : current.map((record) => record.id === saved.id ? { ...record, ...saved } : record)); },
  }} />;
}
