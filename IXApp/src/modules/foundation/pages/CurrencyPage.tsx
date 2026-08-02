import React, { useMemo, useState } from 'react';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailSectionConfig, DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';

interface CurrencyDetails { id: string; code: string; name: string; symbol: string; arabicName: string; referenceCurrency: boolean; values: DetailValues }
const defaults: DetailValues = { conversion: false, prefix: '', suffix: '', generalRounding: 0, salesRounding: 0, salesMethod: 'normal', purchaseRounding: 0, purchaseMethod: 'normal', priceRounding: 0, priceMethod: 'normal', gender: 'masculine', exchangeRateLimit: 0, satDecimals: 0 };
const INITIAL_CURRENCIES: CurrencyDetails[] = [
  ['AED', 'UAE Dirham', 'د.إ', 'درهم إماراتي'], ['EUR', 'Euro', '€', 'يورو أوروبي'], ['GBP', 'Pound Sterling', '£', 'جنيه إسترليني'],
  ['QAR', 'Qatari Riyal', 'ر.ق', 'ريال قطري'], ['SAR', 'Saudi Riyal', 'ر.س', 'ريال سعودي'], ['USD', 'US Dollar', '$', 'دولار أمريكي'],
].map(([code, name, symbol, arabicName], index) => ({ id: `currency-${index + 1}`, code, name, symbol, arabicName, referenceCurrency: false, values: { ...defaults } }));

export function CurrencyPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const [records, setRecords] = useState(INITIAL_CURRENCIES);
  const normalOption = useMemo(() => [{ value: 'normal', label: t('currencyDetails.options.normal') }], [t]);
  const sections = useMemo<DetailSectionConfig[]>(() => [
    { id: 'converter', title: t('currencyDetails.sections.converter'), groups: [
      { id: 'conversion', title: t('currencyDetails.groups.numericConversion'), fields: [{ name: 'conversion', label: t('currencyDetails.fields.conversion'), type: 'boolean' }] },
      { id: 'affixes', fields: [{ name: 'prefix', label: t('currencyDetails.fields.prefix') }, { name: 'suffix', label: t('currencyDetails.fields.suffix') }] },
    ] },
    { id: 'rounding', title: t('currencyDetails.sections.rounding'), groups: [
      { id: 'general', title: t('currencyDetails.groups.general'), fields: [{ name: 'generalRounding', label: t('currencyDetails.fields.generalRounding'), type: 'number' }] },
      { id: 'sales', title: t('currencyDetails.groups.salesOrders'), fields: [{ name: 'salesRounding', label: t('currencyDetails.fields.roundingRule'), type: 'number' }, { name: 'salesMethod', label: t('currencyDetails.fields.roundingMethod'), type: 'select', options: normalOption }] },
      { id: 'purchase', title: t('currencyDetails.groups.purchaseOrders'), fields: [{ name: 'purchaseRounding', label: t('currencyDetails.fields.roundingRule'), type: 'number' }, { name: 'purchaseMethod', label: t('currencyDetails.fields.roundingMethod'), type: 'select', options: normalOption }] },
      { id: 'prices', title: t('currencyDetails.groups.prices'), fields: [{ name: 'priceRounding', label: t('currencyDetails.fields.roundingRule'), type: 'number' }, { name: 'priceMethod', label: t('currencyDetails.fields.roundingMethod'), type: 'select', options: normalOption }] },
    ] },
    { id: 'gender', title: t('currencyDetails.sections.gender'), groups: [{ id: 'genderValue', fields: [{ name: 'gender', label: t('currencyDetails.fields.gender'), type: 'select', options: [{ value: 'masculine', label: t('currencyDetails.options.masculine') }] }] }] },
    { id: 'electronicInvoices', title: t('currencyDetails.sections.electronicInvoices'), groups: [
      { id: 'exchange', fields: [{ name: 'exchangeRateLimit', label: t('currencyDetails.fields.exchangeRateLimit'), type: 'number' }] },
      { id: 'sat', fields: [{ name: 'satDecimals', label: t('currencyDetails.fields.satDecimals'), type: 'number' }] },
    ] },
  ], [normalOption, t]);
  const textHeader = (id: 'code' | 'name' | 'symbol' | 'arabicName') => ({ id, label: t(`currencyDetails.fields.${id}`), getValue: (record: CurrencyDetails) => record[id], setValue: (record: CurrencyDetails, value: string | number | boolean) => ({ ...record, [id]: String(value) }) });
  const config: EnterpriseListDetailsConfig<CurrencyDetails> = {
    dataSource: { type: 'controlled', records, onRecordsChange: setRecords },
    createRecord: () => ({ id: `currency-${Date.now()}`, code: '', name: '', symbol: '', arabicName: '', referenceCurrency: false, values: { ...defaults } }),
    getPrimaryText: (record) => record.code, getSecondaryText: (record) => record.arabicName,
    matchesSearch: (record, query) => `${record.code} ${record.name} ${record.arabicName}`.toLocaleLowerCase().includes(query.toLocaleLowerCase()),
    getValues: (record) => record.values, setValues: (record, values) => ({ ...record, values }),
    headerFields: [textHeader('code'), textHeader('name'), textHeader('symbol'), textHeader('arabicName'), { id: 'referenceCurrency', label: t('currencyDetails.fields.referenceCurrency'), type: 'boolean', getValue: (record) => record.referenceCurrency, setValue: (record, value) => ({ ...record, referenceCurrency: Boolean(value) }) }],
    sections,
    permissions: { view: 'currency.view', create: 'currency.manage', edit: 'currency.manage', delete: 'currency.manage' },
    validate: (record) => ({ ...(!record.code.trim() ? { code: t('validation.required', { field: t('currencyDetails.fields.code') }) } : {}), ...(!record.name.trim() ? { name: t('validation.required', { field: t('currencyDetails.fields.name') }) } : {}) }),
    advancedFilter: { fieldLabel: t('currencyDetails.fields.code'), getValue: (record) => record.code, matches: (record, value) => record.code.toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()) },
    commands: [{ id: 'options', label: t('customerCommands.options') }],
  };
  return <ListDetailsPage variant="enterprise" title={t('pages.currencies.title')} config={config} />;
}
