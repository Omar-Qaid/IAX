import React, { useMemo } from 'react';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailSectionConfig, DetailValue, DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { currencyApi, type CurrencyRecord } from '../api/currencyApi';

const emptyCurrency = (): CurrencyRecord => ({
  id: `new-${crypto.randomUUID()}`, recId: 0, currencyCode: '', currencyCodeIso: '', txt: '', symbol: '',
  isEuro: 0, roundOffSales: 0, roundOffTypeSales: 0, roundOffPurch: 0, roundOffTypePurch: 0,
  roundOffPrice: 0, roundOffTypePrice: 0, roundingPrecision: 0, ltmRoundOffLineAmount: 0,
  ltmRoundOffTypeLineAmount: 0, isActive: true, rowVersion: null, recVersion: 1, dataAreaId: 'dat',
});

const numberValue = (value: DetailValue): number => Number(value) || 0;

export function CurrencyPage(): React.ReactElement {
  const { t } = useAppTranslation();
  const roundOffOptions = useMemo(() => [
    { value: '0', label: t('currencyDetails.options.normal') },
    { value: '1', label: 'Round down' },
    { value: '2', label: 'Round up' },
  ], [t]);
  const sections = useMemo<DetailSectionConfig[]>(() => [
    {
      id: 'rounding', title: t('currencyDetails.sections.rounding'), groups: [
        { id: 'general', title: t('currencyDetails.groups.general'), fields: [{ name: 'roundingPrecision', label: t('currencyDetails.fields.generalRounding'), type: 'number' }] },
        { id: 'sales', title: t('currencyDetails.groups.salesOrders'), fields: [{ name: 'roundOffSales', label: t('currencyDetails.fields.roundingRule'), type: 'number' }, { name: 'roundOffTypeSales', label: t('currencyDetails.fields.roundingMethod'), type: 'select', options: roundOffOptions }] },
        { id: 'purchase', title: t('currencyDetails.groups.purchaseOrders'), fields: [{ name: 'roundOffPurch', label: t('currencyDetails.fields.roundingRule'), type: 'number' }, { name: 'roundOffTypePurch', label: t('currencyDetails.fields.roundingMethod'), type: 'select', options: roundOffOptions }] },
        { id: 'prices', title: t('currencyDetails.groups.prices'), fields: [{ name: 'roundOffPrice', label: t('currencyDetails.fields.roundingRule'), type: 'number' }, { name: 'roundOffTypePrice', label: t('currencyDetails.fields.roundingMethod'), type: 'select', options: roundOffOptions }] },
        { id: 'lineAmount', title: 'Line amount', fields: [{ name: 'ltmRoundOffLineAmount', label: t('currencyDetails.fields.roundingRule'), type: 'number' }, { name: 'ltmRoundOffTypeLineAmount', label: t('currencyDetails.fields.roundingMethod'), type: 'select', options: roundOffOptions }] },
      ],
    },
  ], [roundOffOptions, t]);

  const config: EnterpriseListDetailsConfig<CurrencyRecord> = {
    recordTableName: 'Currency',
    dataSource: {
      type: 'remote', key: 'foundation-currencies',
      load: (signal) => currencyApi.list(signal), create: currencyApi.create, update: currencyApi.update, delete: currencyApi.delete,
    },
    createRecord: emptyCurrency,
    getPrimaryText: (record) => record.currencyCode,
    getSecondaryText: (record) => record.txt,
    matchesSearch: (record, query) => `${record.currencyCode} ${record.currencyCodeIso} ${record.txt}`.toLocaleLowerCase().includes(query.toLocaleLowerCase()),
    getValues: (record): DetailValues => ({
      roundingPrecision: record.roundingPrecision, roundOffSales: record.roundOffSales,
      roundOffTypeSales: String(record.roundOffTypeSales), roundOffPurch: record.roundOffPurch,
      roundOffTypePurch: String(record.roundOffTypePurch), roundOffPrice: record.roundOffPrice,
      roundOffTypePrice: String(record.roundOffTypePrice), ltmRoundOffLineAmount: record.ltmRoundOffLineAmount,
      ltmRoundOffTypeLineAmount: String(record.ltmRoundOffTypeLineAmount),
    }),
    setValues: (record, values) => ({
      ...record, roundingPrecision: numberValue(values.roundingPrecision), roundOffSales: numberValue(values.roundOffSales),
      roundOffTypeSales: numberValue(values.roundOffTypeSales), roundOffPurch: numberValue(values.roundOffPurch),
      roundOffTypePurch: numberValue(values.roundOffTypePurch), roundOffPrice: numberValue(values.roundOffPrice),
      roundOffTypePrice: numberValue(values.roundOffTypePrice), ltmRoundOffLineAmount: numberValue(values.ltmRoundOffLineAmount),
      ltmRoundOffTypeLineAmount: numberValue(values.ltmRoundOffTypeLineAmount),
    }),
    headerFields: [
      { id: 'currencyCode', label: t('currencyDetails.fields.code'), getValue: (record) => record.currencyCode, setValue: (record, value) => ({ ...record, currencyCode: String(value).toUpperCase() }) },
      { id: 'currencyCodeIso', label: 'ISO code', getValue: (record) => record.currencyCodeIso, setValue: (record, value) => ({ ...record, currencyCodeIso: String(value).toUpperCase() }) },
      { id: 'txt', label: t('currencyDetails.fields.name'), getValue: (record) => record.txt, setValue: (record, value) => ({ ...record, txt: String(value) }) },
      { id: 'symbol', label: t('currencyDetails.fields.symbol'), getValue: (record) => record.symbol, setValue: (record, value) => ({ ...record, symbol: String(value) }) },
      { id: 'isEuro', label: 'Euro currency', type: 'boolean', getValue: (record) => record.isEuro === 1, setValue: (record, value) => ({ ...record, isEuro: value ? 1 : 0 }) },
    ],
    sections,
    permissions: { view: 'currency.view', create: 'currency.manage', edit: 'currency.manage', delete: 'currency.manage' },
    validate: (record) => ({
      ...(!record.currencyCode.trim() ? { currencyCode: t('validation.required', { field: t('currencyDetails.fields.code') }) } : {}),
      ...(!record.currencyCodeIso.trim() ? { currencyCodeIso: t('validation.required', { field: 'ISO code' }) } : {}),
      ...(!record.txt.trim() ? { txt: t('validation.required', { field: t('currencyDetails.fields.name') }) } : {}),
    }),
    advancedFilter: { fieldLabel: t('currencyDetails.fields.code'), getValue: (record) => record.currencyCode, matches: (record, value) => record.currencyCode.toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()) },
  };

  return <ListDetailsPage variant="enterprise" title={t('pages.currencies.title')} config={config} />;
}
