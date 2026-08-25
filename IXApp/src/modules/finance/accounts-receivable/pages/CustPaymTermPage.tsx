import React, { useMemo, useState } from 'react';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailSectionConfig, DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';

interface PaymentTerm { id: string; term: string; description: string; values: DetailValues }
const termDefaults: DetailValues = { paymentMethod: 'net', months: 0, days: 0, paymentSchedule: '', paymentDay: '', cutoffDay: 0, defaultTerms: false, dueDateUpdate: 'noUpdate', cashAccount: '', cashPayment: false, certifiedCompanyCheck: false, paymentType: '', creditCheck: 'normal' };
const INITIAL_TERMS: PaymentTerm[] = [
  ['07 Days', 7], ['105 Days', 105], ['120 Days', 120], ['30 Days', 30], ['60 Days', 60],
  ['75 Days', 75], ['90 Days', 90], ['Advance', 0], ['D0', 0],
].map(([term, days], index) => ({ id: `term-${index + 1}`, term: String(term), description: String(term), values: { ...termDefaults, days: Number(days) } }));

export function CustPaymTerm(): React.ReactElement {
  const { t } = useAppTranslation();
  const [records, setRecords] = useState(INITIAL_TERMS);
  const sections = useMemo<DetailSectionConfig[]>(() => [
    { id: 'setup', title: t('paymentTerms.sections.setup'), groups: [
      { id: 'method', fields: [{ name: 'paymentMethod', label: t('paymentTerms.fields.paymentMethod'), type: 'select', options: [{ value: 'net', label: t('paymentTerms.options.net') }] }, { name: 'cashPayment', label: t('paymentTerms.fields.cashPayment'), type: 'boolean' }] },
      { id: 'period', fields: [{ name: 'months', label: t('paymentTerms.fields.months'), type: 'number' }, { name: 'days', label: t('paymentTerms.fields.days'), type: 'number' }] },
      { id: 'schedule', fields: [{ name: 'paymentSchedule', label: t('paymentTerms.fields.paymentSchedule') }, { name: 'paymentDay', label: t('paymentTerms.fields.paymentDay') }] },
      { id: 'cutoff', fields: [{ name: 'cutoffDay', label: t('paymentTerms.fields.cutoffDay'), type: 'number', disabled: true }, { name: 'defaultTerms', label: t('paymentTerms.fields.defaultTerms'), type: 'boolean' }] },
      { id: 'update', fields: [{ name: 'dueDateUpdate', label: t('paymentTerms.fields.dueDateUpdate'), type: 'select', disabled: true, options: [{ value: 'noUpdate', label: t('paymentTerms.options.noUpdate') }] }, { name: 'cashAccount', label: t('paymentTerms.fields.cashAccount'), disabled: true }] },
    ] },
    { id: 'other', title: t('paymentTerms.sections.other'), groups: [
      { id: 'certified', fields: [{ name: 'certifiedCompanyCheck', label: t('paymentTerms.fields.certifiedCompanyCheck'), type: 'boolean' }] },
      { id: 'type', fields: [{ name: 'paymentType', label: t('paymentTerms.fields.paymentType') }] },
      { id: 'credit', fields: [{ name: 'creditCheck', label: t('paymentTerms.fields.creditCheck'), type: 'select', disabled: true, options: [{ value: 'normal', label: t('paymentTerms.options.normal') }] }] },
    ] },
  ], [t]);

  const config: EnterpriseListDetailsConfig<PaymentTerm> = {
    dataSource: { type: 'controlled', records, onRecordsChange: setRecords },
    createRecord: () => ({ id: `term-${Date.now()}`, term: t('paymentTerms.newTerm'), description: '', values: { ...termDefaults } }),
    getPrimaryText: (record) => record.term,
    getSecondaryText: (record) => record.description,
    matchesSearch: (record, query) => `${record.term} ${record.description}`.toLocaleLowerCase().includes(query.toLocaleLowerCase()),
    getValues: (record) => record.values,
    setValues: (record, values) => ({ ...record, values }),
    headerFields: (['term', 'description'] as const).map((id) => ({ id, label: t(`paymentTerms.fields.${id}`), getValue: (record: PaymentTerm) => record[id], setValue: (record: PaymentTerm, value: string | number | boolean) => ({ ...record, [id]: String(value) }) })),
    sections,
    permissions: { view: 'customer.view', create: 'customer.create', edit: 'customer.update', delete: 'customer.delete' },
    validate: (record) => ({ ...(!record.term.trim() ? { term: t('validation.required', { field: t('paymentTerms.fields.term') }) } : {}), ...(!record.description.trim() ? { description: t('validation.required', { field: t('paymentTerms.fields.description') }) } : {}) }),
    advancedFilter: { fieldLabel: t('paymentTerms.fields.term'), getValue: (record) => record.term, matches: (record, value) => record.term.toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()) },
    commands: [{ id: 'translations', label: t('paymentTerms.commands.translations') }],
  };
  return <ListDetailsPage variant="enterprise" title={t('pages.paymentTerms.title')} config={config} />;
}
