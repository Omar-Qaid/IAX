import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ListDetailsListGridPage } from '@patterns/list-details-listgrid/ListDetailsListGridPage';
import type { DetailValue, DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { taxPeriodApi, type TaxPeriodRecord } from '../api/taxPeriodApi';
import { taxAuthorityApi } from '../api/taxAuthorityApi';
import { TaxPeriodIntervalsPanel } from '../components/TaxPeriodIntervalsPanel';

const emptyTaxPeriod = (): TaxPeriodRecord => ({
  id: `new-${crypto.randomUUID()}`, recId: 0, taxPeriod: '', name: '', taxAuthority: '', paymentCode: '', qtyUnit: 1,
  periodUnit: 0, notGenerateOffsetTaxTrans: 0, reportAdjustment: 0, useBatch: 0, activePeriodForBatchJobs: '', intervals: [],
});
const numberValue = (value: DetailValue): number => Number(value) || 0;

export function TaxPeriodPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const taxAuthorities = useQuery({
    queryKey: ['tax-authorities'],
    queryFn: ({ signal }) => taxAuthorityApi.list(signal),
    staleTime: 300000,
  });
  const taxAuthorityOptions = useMemo(
    () => (taxAuthorities.data ?? []).map((authority) => ({
      value: authority.taxAuthority,
      label: `${authority.taxAuthority} - ${authority.name}`,
    })),
    [taxAuthorities.data]
  );
  const periodUnitOptions = useMemo(() => [
    { value: '0', label: t('taxPeriod.options.days', 'Days') },
    { value: '1', label: t('taxPeriod.options.months', 'Months') },
    { value: '2', label: t('taxPeriod.options.quarters', 'Quarters') },
    { value: '3', label: t('taxPeriod.options.years', 'Years') },
  ], [t]);
  const config = useMemo<EnterpriseListDetailsConfig<TaxPeriodRecord>>(() => ({
    recordTableName: 'TaxPeriodHead',
    dataSource: { type: 'remote', key: 'tax-periods', load: taxPeriodApi.list, create: taxPeriodApi.create, update: taxPeriodApi.update, delete: taxPeriodApi.delete },
    createRecord: emptyTaxPeriod,
    getPrimaryText: (record) => record.taxPeriod,
    getSecondaryText: (record) => record.name,
    matchesSearch: (record, query) => `${record.taxPeriod} ${record.name} ${record.taxAuthority}`.toLowerCase().includes(query.toLowerCase()),
    getValues: (record): DetailValues => ({
      taxAuthority: record.taxAuthority, paymentCode: record.paymentCode, periodUnit: String(record.periodUnit), qtyUnit: record.qtyUnit,
      useBatch: record.useBatch === 1, activePeriodForBatchJobs: record.activePeriodForBatchJobs,
      notGenerateOffsetTaxTrans: record.notGenerateOffsetTaxTrans === 1, reportAdjustment: record.reportAdjustment === 1,
    }),
    setValues: (record, values) => ({
      ...record, taxAuthority: String(values.taxAuthority ?? '').toUpperCase(), paymentCode: String(values.paymentCode ?? ''),
      periodUnit: numberValue(values.periodUnit), qtyUnit: numberValue(values.qtyUnit), useBatch: values.useBatch ? 1 : 0,
      activePeriodForBatchJobs: String(values.activePeriodForBatchJobs ?? ''), notGenerateOffsetTaxTrans: values.notGenerateOffsetTaxTrans ? 1 : 0,
      reportAdjustment: values.reportAdjustment ? 1 : 0,
    }),
    headerFields: [
      { id: 'taxPeriod', label: t('taxPeriod.fields.settlementPeriod', 'Settlement period'), disabled: false, getValue: (x) => x.taxPeriod, setValue: (x, value) => x.recId ? x : { ...x, taxPeriod: String(value) } },
      { id: 'name', label: t('taxPeriod.fields.description', 'Description'), getValue: (x) => x.name, setValue: (x, value) => ({ ...x, name: String(value) }) },
    ],
    sections: ({ record, editing, onRecordChange }) => [
      {
        id: 'general', title: t('common.general', 'General'), defaultExpanded: true, columns: 6,
        groups: [
          { id: 'authority', fields: [{ name: 'taxAuthority', label: t('taxPeriod.fields.authority', 'Authority'), type: 'select', options: taxAuthorityOptions }] },
          { id: 'payment', fields: [{ name: 'paymentCode', label: t('taxPeriod.fields.paymentTerms', 'Terms of payment') }] },
          { id: 'unit', fields: [{ name: 'periodUnit', label: t('taxPeriod.fields.intervalUnit', 'Period interval unit'), type: 'select', options: periodUnitOptions }] },
          { id: 'duration', fields: [{ name: 'qtyUnit', label: t('taxPeriod.fields.intervalDuration', 'Period interval duration'), type: 'number' }] },
          { id: 'batch', fields: [{ name: 'useBatch', label: t('taxPeriod.fields.useBatch', 'Use batch processing for sales tax settlement'), type: 'boolean' }] },
          { id: 'controls', fields: [
            { name: 'activePeriodForBatchJobs', label: t('taxPeriod.fields.activePeriod', 'Active period for batch jobs'), disabled: true },
            { name: 'notGenerateOffsetTaxTrans', label: t('taxPeriod.fields.preventOffset', 'Prevent generating offset tax transactions'), type: 'boolean' },
          ] },
        ],
      },
      {
        id: 'intervals', title: t('taxPeriod.sections.intervals', 'Period intervals'), defaultExpanded: true, detailsPadding: '8px 10px 12px',
        content: <TaxPeriodIntervalsPanel record={record} editing={editing} onRecordChange={onRecordChange} />,
      },
    ],
    permissions: { view: PERMISSIONS.TAX_PERIOD_VIEW, create: PERMISSIONS.TAX_PERIOD_CREATE, edit: PERMISSIONS.TAX_PERIOD_EDIT, delete: PERMISSIONS.TAX_PERIOD_DELETE },
    validate: (record) => ({
      ...(!record.taxPeriod.trim() ? { taxPeriod: t('validation.required', { field: t('taxPeriod.fields.settlementPeriod', 'Settlement period') }) } : {}),
      ...(!record.name.trim() ? { name: t('validation.required', { field: t('taxPeriod.fields.description', 'Description') }) } : {}),
      ...(!record.taxAuthority.trim() ? { taxAuthority: t('validation.required', { field: t('taxPeriod.fields.authority', 'Authority') }) } : {}),
      ...(record.qtyUnit < 1 ? { qtyUnit: t('taxPeriod.validation.duration', 'Period interval duration must be at least 1.') } : {}),
    }),
    advancedFilter: { fieldLabel: t('taxPeriod.fields.settlementPeriod', 'Settlement period'), getValue: (record) => record.taxPeriod, matches: (record, value) => `${record.taxPeriod} ${record.name}`.toLowerCase().includes(value.trim().toLowerCase()) },
    presentation: { mode: 'list', listWidth: 244, listResizable: true, recordHeaderMinHeight: 96 },
  }), [periodUnitOptions, t, taxAuthorityOptions]);

  return <ListDetailsListGridPage title={t('taxPeriod.title', 'Sales tax settlement periods')} config={config} />;
}
