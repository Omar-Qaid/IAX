import React, { useMemo, useState } from 'react';
import { Link } from '@mui/material';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailSectionConfig, DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';

interface PaymentMode { id: string; method: string; period: string; description: string; values: DetailValues }
const defaults: DetailValues = { gracePeriod: 0, paymentStatus: 'none', paymentType: 'other', zatcaMethod: 10, lastFileNumber: 0, today: 0, date: '', accountType: 'ledger', paymentAccount: '', bridgingPosting: false, bridgingByBank: false, bridgingAccount: '', bankTransactionType: '', requireMandate: false, draftType: 'noDraft', categoryPurpose: '', chargeBearer: '', localInstrument: '', serviceLevel: '', directDebit: false, autoDrawJournal: false, runExportScript: false, exportScriptName: '', genericExport: false, genericImport: false, exportConfig: '', importConfig: '', exportFormat: '', importFormat: '', returnFormat: '', remittanceFormat: '' };
const INITIAL_RECORDS: PaymentMode[] = ['Cash', 'Check', 'SPAN', 'Transfer'].map((method, index) => ({ id: `payment-${index + 1}`, method, period: 'Invoice', description: method, values: { ...defaults } }));

export function CustPaymMode(): React.ReactElement {
  const { t } = useAppTranslation();
  const [records, setRecords] = useState(INITIAL_RECORDS);
  const option = (value: string, key: string) => ({ value, label: t(`paymentMethods.options.${key}`) });
  const sections = useMemo<DetailSectionConfig[]>(() => [
    { id: 'general', title: t('paymentMethods.sections.general'), groups: [
      { id: 'file', title: t('paymentMethods.groups.file'), columns: 2, fields: [
        { name: 'lastFileNumber', label: t('paymentMethods.fields.lastFileNumber'), type: 'number', width: 65, column: 1, row: 1 },
        { name: 'today', label: t('paymentMethods.fields.today'), type: 'number', width: 65, column: 2, row: 1 },
        { name: 'date', label: t('paymentMethods.fields.date'), type: 'text', width: 100, column: 2, row: 2 },
      ] },
      { id: 'posting', title: t('paymentMethods.groups.posting'), fields: [{ name: 'accountType', label: t('paymentMethods.fields.accountType'), type: 'select', options: [option('ledger', 'ledger')] }, { name: 'paymentAccount', label: t('paymentMethods.fields.paymentAccount') }, { name: 'bridgingPosting', label: t('paymentMethods.fields.bridgingPosting'), type: 'boolean' }] },
      { id: 'bridging', fields: [{ name: 'bridgingByBank', label: t('paymentMethods.fields.bridgingByBank'), type: 'boolean' }, { name: 'bridgingAccount', label: t('paymentMethods.fields.bridgingAccount') }, { name: 'bankTransactionType', label: t('paymentMethods.fields.bankTransactionType') }] },
      { id: 'sepa', title: 'SEPA', fields: [{ name: 'requireMandate', label: t('paymentMethods.fields.requireMandate'), type: 'boolean' }, { name: 'draftType', label: t('paymentMethods.fields.draftType'), type: 'select', options: [option('noDraft', 'noDraft')] }, { name: 'categoryPurpose', label: t('paymentMethods.fields.categoryPurpose') }] },
      { id: 'processing', fields: [{ name: 'chargeBearer', label: t('paymentMethods.fields.chargeBearer') }, { name: 'localInstrument', label: t('paymentMethods.fields.localInstrument') }, { name: 'serviceLevel', label: t('paymentMethods.fields.serviceLevel') }, { name: 'directDebit', label: t('paymentMethods.fields.directDebit'), type: 'boolean' }] },
    ] },
    { id: 'formats', title: t('paymentMethods.sections.fileFormats'), link: <Link component="button" underline="none" sx={{ fontSize: '0.75rem' }}>{t('paymentMethods.actions.setup')}</Link>, groups: [
      { id: 'invoiceUpdate', title: t('paymentMethods.groups.invoiceUpdate'), fields: [{ name: 'autoDrawJournal', label: t('paymentMethods.fields.autoDrawJournal'), type: 'boolean' }] },
      { id: 'script', fields: [{ name: 'runExportScript', label: t('paymentMethods.fields.runExportScript'), type: 'boolean' }, { name: 'exportScriptName', label: t('fields.name') }] },
      { id: 'generic', title: t('paymentMethods.groups.fileFormats'), fields: [{ name: 'genericExport', label: t('paymentMethods.fields.genericExport'), type: 'boolean' }, { name: 'genericImport', label: t('paymentMethods.fields.genericImport'), type: 'boolean' }] },
      { id: 'configuration', fields: [{ name: 'exportConfig', label: t('paymentMethods.fields.exportConfig'), disabled: true }, { name: 'importConfig', label: t('paymentMethods.fields.importConfig'), disabled: true }] },
      { id: 'formatNames', fields: ['exportFormat', 'importFormat', 'returnFormat', 'remittanceFormat'].map((name) => ({ name, label: t(`paymentMethods.fields.${name}`) })) },
    ] },
  ], [t]);
  const config: EnterpriseListDetailsConfig<PaymentMode> = {
    dataSource: { type: 'controlled', records, onRecordsChange: setRecords },
    createRecord: () => ({ id: `payment-${Date.now()}`, method: t('paymentMethods.newMethod'), period: 'Invoice', description: '', values: { ...defaults } }),
    getPrimaryText: (record) => record.method, getSecondaryText: (record) => record.period,
    matchesSearch: (record, query) => record.method.toLocaleLowerCase().includes(query.toLocaleLowerCase()),
    getValues: (record) => record.values, setValues: (record, values) => ({ ...record, values }),
    headerFields: [
      ...(['method', 'period', 'description'] as const).map((id) => ({ id, label: t(`paymentMethods.fields.${id}`), getValue: (record: PaymentMode) => record[id], setValue: (record: PaymentMode, value: string | number | boolean) => ({ ...record, [id]: String(value) }) })),
      ...(['gracePeriod', 'paymentStatus', 'paymentType', 'zatcaMethod'] as const).map((id) => ({ id, label: t(`paymentMethods.fields.${id}`), type: id === 'gracePeriod' || id === 'zatcaMethod' ? 'number' as const : 'text' as const, getValue: (record: PaymentMode) => record.values[id], setValue: (record: PaymentMode, value: string | number | boolean) => ({ ...record, values: { ...record.values, [id]: value } }) })),
    ],
    sections,
    permissions: { view: 'customer.view', create: 'customer.create', edit: 'customer.update', delete: 'customer.delete' },
    validate: (record) => ({ ...(!record.method.trim() ? { method: t('validation.required', { field: t('paymentMethods.fields.method') }) } : {}), ...(!record.period.trim() ? { period: t('validation.required', { field: t('paymentMethods.fields.period') }) } : {}) }),
    advancedFilter: { fieldLabel: t('paymentMethods.fields.method'), getValue: (record) => record.method, matches: (record, value) => record.method.toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()) },
    relatedInformation: { sections: (record) => [
      { id: 'summary', label: t('paymentMethods.sections.general'), defaultExpanded: true, content: record ? `${record.method} · ${record.period}` : t('messages.selectRecord') },
      { id: 'fileFormats', label: t('paymentMethods.sections.fileFormats') },
    ] },
    commands: ['paymentSpecification', 'paymentFeeSetup', 'remittanceFiles', 'fileAnalyze', 'options'].map((id) => ({ id, label: t(`paymentMethods.commands.${id}`), disabled: id === 'fileAnalyze' })),
  };
  return <ListDetailsPage variant="enterprise" title={t('pages.paymentMethods.title')} config={config} />;
}
