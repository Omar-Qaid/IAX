import React, { useMemo } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailFieldConfig, DetailValue, DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { PERMISSIONS } from '@core/permissions/permissions';
import { currencyApi } from '../api/currencyApi';
import { taxPeriodApi } from '../api/taxPeriodApi';
import { taxTableApi, type TaxTableRecord } from '../api/taxTableApi';

const emptyTaxCode = (): TaxTableRecord => ({
  id: `new-${crypto.randomUUID()}`, recId: 0, taxCode: '', taxName: '', taxPeriod: '', taxAccountGroup: 'STANDARD', taxCurrencyCode: 'SAR', source: 0,
  taxOnTax: '', taxUnit: '', taxBase: 0, taxCalcMethod: 0, taxLimitBase: 0, taxIncludeInTax: 0, negativeTax: 0, unrealizedTax: 0,
  taxAllowLineDiscountOnTaxPerUnit: 0, taxRoundOff: 0, taxRoundOffType: 0, roundDeductibleFirst: 0, printCode: '', paymentTaxCode: '',
  taxJurisdictionCode: '', taxType_W: 0, taxCountryRegionType: 0, notEuSalesList: 0, excludeFromInvoice: 0, taxPurchaseTax: 0,
  taxPackagingTax: 0, taxWriteSelection: 0, reconcileAmountOrigin: 0, repFieldBaseOutgoing: 0, repFieldBaseOutgoingCreditNote: 0,
  repFieldTaxOutgoing: 0, repFieldTaxOutgoingCreditNote: 0, repFieldBaseIncoming: 0, repFieldBaseIncomingCreditNote: 0,
  repFieldTaxIncoming: 0, repFieldTaxIncomingCreditNote: 0, repFieldBaseUseTax: 0, repFieldBaseUseTaxCreditNote: 0,
  repFieldUseTax: 0, repFieldUseTaxCreditNote: 0, repFieldBaseUseTaxOffset: 0, repFieldBaseUseTaxOffsetCreditNote: 0,
  repFieldUseTaxOffset: 0, repFieldUseTaxOffsetCreditNote: 0, repFieldTaxFreeSales: 0, repFieldTaxFreeSalesCreditNote: 0,
  repFieldTaxFreeBuy: 0, repFieldTaxFreeBuyCreditNote: 0, taxValue: 0,
});
const numberValue = (value: DetailValue): number => Number(value) || 0;
const textValue = (value: DetailValue): string => String(value ?? '');
const bool = (name: string, label: string): DetailFieldConfig => ({ name, label, type: 'boolean' });
const num = (name: string, label: string): DetailFieldConfig => ({ name, label, type: 'number' });

export function TaxTablePage(): React.ReactElement {
  const { t } = useAppTranslation();
  const periods = useQuery({ queryKey: ['tax-periods'], queryFn: ({ signal }) => taxPeriodApi.list(signal), staleTime: 300000 });
  const currencies = useQuery({ queryKey: ['foundation-currencies'], queryFn: ({ signal }) => currencyApi.list(signal), staleTime: 300000 });
  const option = (value: number, label: string) => ({ value: String(value), label });
  const select = (name: string, label: string, options: { value: string; label: string }[]): DetailFieldConfig => ({ name, label, type: 'select', options });
  const reportFields = (prefix: 'Outgoing' | 'Incoming' | 'UseTax' | 'UseTaxOffset', labels: string[]) => {
    const taxStem = prefix === 'Outgoing' || prefix === 'Incoming' ? `Tax${prefix}` : prefix;
    return [
      num(`repFieldBase${prefix}`, labels[0]), num(`repField${taxStem}`, labels[1]),
      num(`repFieldBase${prefix}CreditNote`, `${labels[0]} credit note`), num(`repField${taxStem}CreditNote`, `${labels[1]} credit note`),
    ];
  };
  const config = useMemo<EnterpriseListDetailsConfig<TaxTableRecord>>(() => ({
    recordTableName: 'TaxTable',
    dataSource: { type: 'remote', key: 'sales-tax-codes', load: taxTableApi.list, create: taxTableApi.create, update: taxTableApi.update, delete: taxTableApi.delete },
    createRecord: emptyTaxCode,
    getPrimaryText: (record) => record.taxCode,
    getSecondaryText: (record) => record.taxName,
    matchesSearch: (record, query) => `${record.taxCode} ${record.taxName} ${record.taxPeriod}`.toLowerCase().includes(query.toLowerCase()),
    getValues: (r): DetailValues => ({ ...r, source: String(r.source), taxBase: String(r.taxBase), taxCalcMethod: String(r.taxCalcMethod), taxLimitBase: String(r.taxLimitBase), taxRoundOffType: String(r.taxRoundOffType), taxWriteSelection: String(r.taxWriteSelection), reconcileAmountOrigin: String(r.reconcileAmountOrigin), taxCountryRegionType: String(r.taxCountryRegionType) }),
    setValues: (r, v) => {
      const next = { ...r } as TaxTableRecord;
      const stringFields = ['taxPeriod','taxAccountGroup','taxCurrencyCode','paymentTaxCode','printCode','taxOnTax','taxUnit','taxJurisdictionCode'] as const;
      const enumFields = ['source','taxBase','taxCalcMethod','taxLimitBase','taxRoundOffType','taxWriteSelection','reconcileAmountOrigin','taxCountryRegionType'] as const;
      const boolFields = ['taxIncludeInTax','negativeTax','unrealizedTax','taxAllowLineDiscountOnTaxPerUnit','roundDeductibleFirst','notEuSalesList','excludeFromInvoice','taxPurchaseTax','taxPackagingTax'] as const;
      stringFields.forEach((field) => { next[field] = textValue(v[field]); });
      enumFields.forEach((field) => { next[field] = numberValue(v[field]); });
      boolFields.forEach((field) => { next[field] = v[field] ? 1 : 0; });
      Object.keys(v).filter((key) => key === 'taxRoundOff' || key.startsWith('repField')).forEach((key) => { (next as unknown as Record<string, unknown>)[key] = numberValue(v[key]); });
      return next;
    },
    headerFields: [
      { id: 'taxCode', label: t('taxTable.fields.code', 'Sales tax code'), getValue: (r) => r.taxCode, setValue: (r, value) => r.recId ? r : { ...r, taxCode: String(value).toUpperCase() } },
      { id: 'taxName', label: t('taxTable.fields.name', 'Name'), getValue: (r) => r.taxName, setValue: (r, value) => ({ ...r, taxName: String(value) }) },
      { id: 'taxValue', label: t('taxTable.fields.percentageAmount', 'Percentage/Amount'), type: 'number', getValue: (r) => r.taxValue, setValue: (r, value) => ({ ...r, taxValue: numberValue(value) }) },
      { id: 'taxType_W', label: t('taxTable.fields.taxType', 'Type of tax'), type: 'select', options: [option(0,'Standard VAT'),option(1,'Reduced'),option(2,'Zero'),option(3,'Exempt')], getValue: (r) => String(r.taxType_W), setValue: (r, value) => ({ ...r, taxType_W: numberValue(value) }) },
    ],
    sections: [
      { id: 'general', title: t('common.general','General'), defaultExpanded: true, columns: 5, groups: [
        { id: 'references', title: t('taxTable.groups.references','References'), fields: [select('taxPeriod', t('taxTable.fields.settlementPeriod','Settlement period'), (periods.data ?? []).map((x) => ({ value: x.taxPeriod, label: `${x.taxPeriod} - ${x.name}` })))] },
        { id: 'ledger', fields: [{ name:'taxAccountGroup', label:t('taxTable.fields.ledgerPostingGroup','Ledger posting group') }, select('taxCurrencyCode',t('taxTable.fields.currency','Sales tax currency'),(currencies.data ?? []).map((x) => ({ value:x.currencyCode,label:`${x.currencyCode} - ${x.txt}` })))] },
        { id: 'conditional', title:t('taxTable.groups.conditional','Conditional sales tax'), fields:[{name:'paymentTaxCode',label:t('taxTable.fields.paymentTaxCode','Payment sales tax code')}] },
        { id: 'packing', title:t('taxTable.groups.packingDuty','Packing duty'), fields:[{name:'taxJurisdictionCode',label:t('taxTable.fields.sortCode','Sort code')}] },
        { id: 'invoicing', title:t('taxTable.groups.invoicing','Invoicing'), fields:[bool('excludeFromInvoice',t('taxTable.fields.excludeFromInvoice','Exclude from invoice')),{name:'printCode',label:t('taxTable.fields.printCode','Print code')}] },
      ]},
      { id:'calculation', title:t('taxTable.sections.calculation','Calculation'), defaultExpanded:true, columns:6, groups:[
        {id:'parameters',title:t('taxTable.groups.parameters','Calculation parameters'),fields:[select('taxBase',t('taxTable.fields.origin','Origin'),[option(0,'Percentage of net amount'),option(1,'Percentage of gross amount')]),bool('reconcileAmountOrigin',t('taxTable.fields.reconcile','Reconcile amount origin'))]},
        {id:'marginal',fields:[select('taxLimitBase',t('taxTable.fields.marginalBase','Marginal base'),[option(0,'None'),option(1,'Invoice'),option(2,'Line')]),bool('negativeTax',t('taxTable.fields.negative','Allow negative sales tax percentage'))]},
        {id:'method',fields:[select('taxCalcMethod',t('taxTable.fields.method','Calculation method'),[option(0,'Line'),option(1,'Total')]),{name:'taxOnTax',label:t('taxTable.fields.taxOnTax','Sales tax on sales tax')}]},
        {id:'unit',fields:[{name:'taxUnit',label:t('taxTable.fields.unit','Unit')},bool('taxAllowLineDiscountOnTaxPerUnit',t('taxTable.fields.lineDiscount','Allow line discount on tax per unit'))]},
        {id:'rounding',title:t('taxTable.groups.rounding','Sales tax rounding rule'),fields:[num('taxRoundOff',t('taxTable.fields.roundingPrecision','Rounding precision')),select('taxRoundOffType',t('taxTable.fields.roundingMethod','Rounding method'),[option(0,'Normal'),option(1,'Round up'),option(2,'Round down')])]},
        {id:'packingCalc',title:t('taxTable.groups.packingDuty','Packing duty'),fields:[bool('taxPackagingTax',t('taxTable.fields.packing','Classify as packing duty')),bool('taxIncludeInTax',t('taxTable.fields.beforeTax','Calculate before sales tax'))]},
      ]},
      { id:'reportSetup',title:t('taxTable.sections.reportSetup','Report setup'),defaultExpanded:true,columns:4,groups:[
        {id:'sale',title:t('taxTable.groups.sale','Sale'),fields:[...reportFields('Outgoing',['Taxable sales','Sales tax payable']),num('repFieldTaxFreeSales','Tax-free sale'),num('repFieldTaxFreeSalesCreditNote','Tax-free sale credit note')]},
        {id:'purchase',title:t('taxTable.groups.purchase','Purchase'),fields:[...reportFields('Incoming',['Taxable purchases','Sales tax receivable']),num('repFieldTaxFreeBuy','Tax-free purchase'),num('repFieldTaxFreeBuyCreditNote','Tax-free purchase credit note')]},
        {id:'import',title:t('taxTable.groups.import','Import'),fields:reportFields('UseTaxOffset',['Taxable import','Offset taxable import'])},
        {id:'useTax',title:t('taxTable.groups.useTax','Use tax'),fields:[...reportFields('UseTax',['Use tax base','Use tax']),select('taxCountryRegionType',t('taxTable.fields.countryRegion','Country/region type'),[option(0,'Domestic'),option(1,'EU'),option(2,'Foreign')])]},
      ]},
    ],
    permissions:{view:PERMISSIONS.TAX_CODE_VIEW,create:PERMISSIONS.TAX_CODE_CREATE,edit:PERMISSIONS.TAX_CODE_EDIT,delete:PERMISSIONS.TAX_CODE_DELETE},
    validate:(r)=>({ ...(!r.taxCode.trim()?{taxCode:t('validation.required',{field:t('taxTable.fields.code','Sales tax code')})}:{}), ...(!r.taxName.trim()?{taxName:t('validation.required',{field:t('taxTable.fields.name','Name')})}:{}) }),
    advancedFilter:{fieldLabel:t('taxTable.fields.code','Sales tax code'),getValue:(r)=>r.taxCode,matches:(r,value)=>`${r.taxCode} ${r.taxName}`.toLowerCase().includes(value.trim().toLowerCase())},
    presentation:{mode:'list',listWidth:258,listResizable:true},
  }),[currencies.data,periods.data,t]);
  return <ListDetailsPage variant="enterprise" title={t('taxTable.title','Sales tax codes')} config={config}/>;
}
