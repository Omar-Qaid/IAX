import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailFieldConfig, DetailValue, DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { markupTableApi, type MarkupTableRecord } from '../api/markupTableApi';
import { taxItemGroupApi } from '../api/taxItemGroupApi';

const emptyMarkup = (): MarkupTableRecord => ({
  id: `new-${crypto.randomUUID()}`, recId: 0, markupCode: '', txt: '', moduleType: 3,
  taxItemGroup: '', taxRateType: 0, taxWithholdItemGroup: 0,
  custType: 1, custPosting: 0, customerLedgerDimension: null,
  vendType: 0, vendPosting: 0, vendorLedgerDimension: null,
  maxAmount: 0, useInMatching: 0,
  includeIntoIntrastatInvoiceValue: 0, includeIntoIntrastatStatisticalValue: 0,
  isShipping: 0, refundable: 0, mcrProrate: 0, mcrBrokerContractFee: 0,
});
const text = (value: DetailValue): string => String(value ?? '');
const number = (value: DetailValue): number => Number(value) || 0;
const nullableNumber = (value: DetailValue): number | null => value === '' || value == null ? null : Number(value);
const bool = (name: string, label: string): DetailFieldConfig => ({ name, label, type: 'boolean' });
const option = (value: number, label: string) => ({ value: String(value), label });
const select = (name: string, label: string, options: { value: string; label: string }[]): DetailFieldConfig => ({ name, label, type: 'select', options });

export function MarkupTablePage(): React.ReactElement {
  const { t } = useAppTranslation();
  const itemTaxGroups = useQuery({
    queryKey: ['tax-item-groups'],
    queryFn: ({ signal }) => taxItemGroupApi.list(signal),
    staleTime: 300000,
  });
  const postingTypes = useMemo(() => [
    option(0, 'Ledger account'), option(1, 'Customer/Vendor'), option(2, 'Item'),
  ], []);
  const postingCategories = useMemo(() => [
    option(0, 'None'), option(1, 'Sales order charge'), option(2, 'Purchase order charge'),
    option(3, 'Sales order discount'), option(4, 'Purchase order discount'),
  ], []);
  const config = useMemo<EnterpriseListDetailsConfig<MarkupTableRecord>>(() => ({
    recordTableName: 'MarkupTable',
    dataSource: {
      type: 'remote', key: 'markup-table', load: markupTableApi.list,
      create: markupTableApi.create, update: markupTableApi.update, delete: markupTableApi.delete,
    },
    createRecord: emptyMarkup,
    getPrimaryText: (record) => record.markupCode,
    getSecondaryText: (record) => record.txt,
    matchesSearch: (record, query) => `${record.markupCode} ${record.txt} ${record.taxItemGroup}`.toLowerCase().includes(query.toLowerCase()),
    getValues: (record): DetailValues => ({
      moduleType: String(record.moduleType), taxItemGroup: record.taxItemGroup,
      taxRateType: record.taxRateType, taxWithholdItemGroup: record.taxWithholdItemGroup,
      custType: String(record.custType), custPosting: String(record.custPosting), customerLedgerDimension: record.customerLedgerDimension ?? '',
      vendType: String(record.vendType), vendPosting: String(record.vendPosting), vendorLedgerDimension: record.vendorLedgerDimension ?? '',
      maxAmount: record.maxAmount, useInMatching: record.useInMatching === 1,
      includeIntoIntrastatInvoiceValue: record.includeIntoIntrastatInvoiceValue === 1,
      includeIntoIntrastatStatisticalValue: record.includeIntoIntrastatStatisticalValue === 1,
      isShipping: record.isShipping === 1, refundable: record.refundable === 1,
      mcrProrate: record.mcrProrate === 1, mcrBrokerContractFee: record.mcrBrokerContractFee === 1,
    }),
    setValues: (record, values) => ({
      ...record,
      moduleType: number(values.moduleType), taxItemGroup: text(values.taxItemGroup),
      taxRateType: number(values.taxRateType), taxWithholdItemGroup: number(values.taxWithholdItemGroup),
      custType: number(values.custType), custPosting: number(values.custPosting), customerLedgerDimension: nullableNumber(values.customerLedgerDimension),
      vendType: number(values.vendType), vendPosting: number(values.vendPosting), vendorLedgerDimension: nullableNumber(values.vendorLedgerDimension),
      maxAmount: number(values.maxAmount), useInMatching: values.useInMatching ? 1 : 0,
      includeIntoIntrastatInvoiceValue: values.includeIntoIntrastatInvoiceValue ? 1 : 0,
      includeIntoIntrastatStatisticalValue: values.includeIntoIntrastatStatisticalValue ? 1 : 0,
      isShipping: values.isShipping ? 1 : 0, refundable: values.refundable ? 1 : 0,
      mcrProrate: values.mcrProrate ? 1 : 0, mcrBrokerContractFee: values.mcrBrokerContractFee ? 1 : 0,
    }),
    headerFields: [
      { id: 'markupCode', label: 'Charges code', getValue: (r) => r.markupCode, setValue: (r, value) => r.recId ? r : { ...r, markupCode: String(value) } },
      { id: 'txt', label: t('fields.description', 'Description'), getValue: (r) => r.txt, setValue: (r, value) => ({ ...r, txt: String(value) }) },
      { id: 'taxItemGroup', label: 'Item sales tax group', type: 'select', options: (itemTaxGroups.data ?? []).map((x) => ({ value: x.taxItemGroup, label: `${x.taxItemGroup} - ${x.name}` })), getValue: (r) => r.taxItemGroup, setValue: (r, value) => ({ ...r, taxItemGroup: String(value) }) },
      { id: 'moduleType', label: 'Module', type: 'select', options: [option(1, 'Inventory'), option(2, 'Purchase'), option(3, 'Sales')], getValue: (r) => String(r.moduleType), setValue: (r, value) => ({ ...r, moduleType: Number(value) }) },
      { id: 'mcrProrate', label: 'Prorate', type: 'boolean', getValue: (r) => r.mcrProrate === 1, setValue: (r, value) => ({ ...r, mcrProrate: value ? 1 : 0 }) },
    ],
    sections: [
      { id: 'posting', title: 'Posting', defaultExpanded: true, columns: 3, groups: [
        { id: 'debit', title: 'Debit', fields: [select('custType', 'Type', postingTypes), select('custPosting', 'Posting', postingCategories), { name: 'customerLedgerDimension', label: 'Account', type: 'number' }] },
        { id: 'matching', title: 'Matching', fields: [bool('useInMatching', 'Use in matching'), { name: 'maxAmount', label: 'Maximum amount', type: 'number' }, bool('mcrBrokerContractFee', 'Broker contract fee')] },
        { id: 'credit', title: 'Credit', fields: [select('vendType', 'Type', postingTypes), select('vendPosting', 'Posting', postingCategories), { name: 'vendorLedgerDimension', label: 'Account', type: 'number' }] },
      ] },
      { id: 'foreignTrade', title: 'Foreign trade', defaultExpanded: true, columns: 2, groups: [
        { id: 'invoice', fields: [bool('includeIntoIntrastatInvoiceValue', 'Intrastat invoice value')] },
        { id: 'statistics', fields: [bool('includeIntoIntrastatStatisticalValue', 'Intrastat statistical value')] },
      ] },
      { id: 'commerce', title: 'Commerce', defaultExpanded: false, columns: 3, groups: [
        { id: 'shipping', fields: [bool('isShipping', 'Shipping charge')] },
        { id: 'refund', fields: [bool('refundable', 'Refundable')] },
        { id: 'tax', fields: [{ name: 'taxRateType', label: 'Tax rate type', type: 'number' }, { name: 'taxWithholdItemGroup', label: 'Withholding tax item group', type: 'number' }] },
      ] },
    ],
    commands: [
      { id: 'translations', label: 'Translations', disabled: true },
      { id: 'external-codes', label: 'External codes', disabled: true },
    ],
    permissions: {
      view: PERMISSIONS.MARKUP_TABLE_VIEW, create: PERMISSIONS.MARKUP_TABLE_CREATE,
      edit: PERMISSIONS.MARKUP_TABLE_EDIT, delete: PERMISSIONS.MARKUP_TABLE_DELETE,
    },
    validate: (record) => ({
      ...(!record.markupCode.trim() ? { markupCode: t('validation.required', { field: 'Charges code' }) } : {}),
      ...(!record.txt.trim() ? { txt: t('validation.required', { field: t('fields.description', 'Description') }) } : {}),
      ...(!record.taxItemGroup.trim() ? { taxItemGroup: t('validation.required', { field: 'Item sales tax group' }) } : {}),
    }),
    advancedFilter: { fieldLabel: 'Charges code', getValue: (r) => r.markupCode, matches: (r, value) => `${r.markupCode} ${r.txt}`.toLowerCase().includes(value.trim().toLowerCase()) },
    presentation: { mode: 'list', listWidth: 258, listResizable: true },
  }), [itemTaxGroups.data, postingCategories, postingTypes, t]);

  return <ListDetailsPage variant="enterprise" title="Charges codes" config={config} />;
}
