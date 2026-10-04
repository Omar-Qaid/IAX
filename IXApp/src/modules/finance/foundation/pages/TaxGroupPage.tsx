import React,{useMemo} from 'react';
import {useQuery} from '@tanstack/react-query';
import {ListDetailsListGridPage} from '@patterns/list-details-listgrid/ListDetailsListGridPage';
import type {DetailValue,DetailValues,EnterpriseListDetailsConfig} from '@patterns/list-details/types';
import {useAppTranslation} from '@core/localization/useAppTranslation';
import {PERMISSIONS} from '@core/permissions/permissions';
import {taxGroupApi,type TaxGroupRecord} from '../api/taxGroupApi';
import {taxTableApi} from '../api/taxTableApi';
import {TaxGroupLinesPanel} from '../components/TaxGroupLinesPanel';

const emptyGroup=():TaxGroupRecord=>({id:`new-${crypto.randomUUID()}`,recId:0,taxGroup:'',taxGroupName:'',taxGroupSetup:1,source:0,taxGroupRounding:0,taxReverseOnCashDisc:0,euTrade_W:0,mandatorySalesDate_W:0,fillSalesDate_W:0,fillVatDueDatePeriodNumber:0,fillVatDueDate_W:0,fillVatDueDateBasedOn:0,fillVatDueDatePeriod:0,taxPrintDetail:0,lines:[]});
const numberValue=(value:DetailValue)=>Number(value)||0;
export function TaxGroupPage():React.ReactElement{
 const {t}=useAppTranslation();
 const taxCodes=useQuery({queryKey:['sales-tax-codes'],queryFn:({signal})=>taxTableApi.list(signal),staleTime:300000});
 const option=(value:number,label:string)=>({value:String(value),label});
 const config=useMemo<EnterpriseListDetailsConfig<TaxGroupRecord>>(()=>({
  recordTableName:'TaxGroupHeading',dataSource:{type:'remote',key:'sales-tax-groups',load:taxGroupApi.list,create:taxGroupApi.create,update:taxGroupApi.update,delete:taxGroupApi.delete},createRecord:emptyGroup,
  getPrimaryText:(r)=>r.taxGroup,getSecondaryText:(r)=>r.taxGroupName,matchesSearch:(r,q)=>`${r.taxGroup} ${r.taxGroupName}`.toLowerCase().includes(q.toLowerCase()),
  getValues:(r):DetailValues=>({taxGroupSetup:String(r.taxGroupSetup),source:String(r.source),taxGroupRounding:String(r.taxGroupRounding),taxReverseOnCashDisc:r.taxReverseOnCashDisc===1,taxPrintDetail:String(r.taxPrintDetail),euTrade_W:r.euTrade_W===1,mandatorySalesDate_W:r.mandatorySalesDate_W===1,fillSalesDate_W:r.fillSalesDate_W===1,fillVatDueDatePeriodNumber:r.fillVatDueDatePeriodNumber,fillVatDueDate_W:r.fillVatDueDate_W===1,fillVatDueDateBasedOn:String(r.fillVatDueDateBasedOn),fillVatDueDatePeriod:String(r.fillVatDueDatePeriod)}),
  setValues:(r,v)=>({...r,taxGroupSetup:numberValue(v.taxGroupSetup),source:numberValue(v.source),taxGroupRounding:numberValue(v.taxGroupRounding),taxReverseOnCashDisc:v.taxReverseOnCashDisc?1:0,taxPrintDetail:numberValue(v.taxPrintDetail),euTrade_W:v.euTrade_W?1:0,mandatorySalesDate_W:v.mandatorySalesDate_W?1:0,fillSalesDate_W:v.fillSalesDate_W?1:0,fillVatDueDatePeriodNumber:numberValue(v.fillVatDueDatePeriodNumber),fillVatDueDate_W:v.fillVatDueDate_W?1:0,fillVatDueDateBasedOn:numberValue(v.fillVatDueDateBasedOn),fillVatDueDatePeriod:numberValue(v.fillVatDueDatePeriod)}),
  headerFields:[{id:'taxGroup',label:t('taxGroup.fields.group','Sales tax group'),getValue:(r)=>r.taxGroup,setValue:(r,value)=>r.recId?r:{...r,taxGroup:String(value).toUpperCase()}},{id:'taxGroupName',label:t('taxGroup.fields.description','Description'),getValue:(r)=>r.taxGroupName,setValue:(r,value)=>({...r,taxGroupName:String(value)})}],
  sections:({record,editing,onRecordChange})=>[
   {id:'general',title:t('common.general','General'),defaultExpanded:true,columns:4,groups:[
    {id:'description',title:t('taxGroup.groups.description','Sales tax group description'),fields:[{name:'taxGroupSetup',label:t('taxGroup.fields.setup','Group setup'),type:'select',options:[option(0,'None'),option(1,'Standard'),option(2,'Custom')]},{name:'source',label:t('taxGroup.fields.source','Source'),type:'select',options:[option(0,'None'),option(1,'Customer'),option(2,'Vendor'),option(3,'Project')]}]},
    {id:'cashDiscount',title:t('taxGroup.groups.cashDiscount','Cash discount'),fields:[{name:'taxReverseOnCashDisc',label:t('taxGroup.fields.reverseCashDiscount','Reverse sales tax on cash discount'),type:'boolean'}]},
    {id:'rounding',title:t('taxGroup.groups.rounding','Sales tax rounding rule'),fields:[{name:'taxGroupRounding',label:t('taxGroup.fields.roundingBy','Rounding by'),type:'select',options:[option(0,'Sales tax codes'),option(1,'Round up'),option(2,'Round down')]}]},
    {id:'invoicing',title:t('taxGroup.groups.invoicing','Invoicing'),fields:[{name:'taxPrintDetail',label:t('taxGroup.fields.print','Print'),type:'select',options:[option(0,'Sales tax codes'),option(1,'Details')]}]},
   ]},
   {id:'setup',title:t('taxGroup.sections.setup','Setup'),defaultExpanded:true,detailsPadding:'8px 10px 12px',content:<TaxGroupLinesPanel record={record} editing={editing} taxCodes={taxCodes.data??[]} onRecordChange={onRecordChange}/>},
   {id:'regional',title:t('taxGroup.sections.regional','Regional and VAT due date settings'),columns:4,groups:[
    {id:'trade',fields:[{name:'euTrade_W',label:t('taxGroup.fields.euTrade','EU trade'),type:'boolean'},{name:'mandatorySalesDate_W',label:t('taxGroup.fields.mandatorySalesDate','Mandatory sales date'),type:'boolean'}]},
    {id:'dates',fields:[{name:'fillSalesDate_W',label:t('taxGroup.fields.fillSalesDate','Fill sales date'),type:'boolean'},{name:'fillVatDueDate_W',label:t('taxGroup.fields.fillVatDueDate','Fill VAT due date'),type:'boolean'}]},
    {id:'basis',fields:[{name:'fillVatDueDateBasedOn',label:t('taxGroup.fields.dueDateBasedOn','VAT due date based on'),type:'select',options:[option(0,'Delivery'),option(1,'Invoice'),option(2,'Payment')]},{name:'fillVatDueDatePeriod',label:t('taxGroup.fields.periodUnit','Period unit'),type:'select',options:[option(0,'Days'),option(1,'Months'),option(2,'Quarters'),option(3,'Years')]}]},
    {id:'duration',fields:[{name:'fillVatDueDatePeriodNumber',label:t('taxGroup.fields.periodNumber','Period duration'),type:'number'}]},
   ]},
  ],
  permissions:{view:PERMISSIONS.TAX_GROUP_VIEW,create:PERMISSIONS.TAX_GROUP_CREATE,edit:PERMISSIONS.TAX_GROUP_EDIT,delete:PERMISSIONS.TAX_GROUP_DELETE},
  validate:(r)=>({...(!r.taxGroup.trim()?{taxGroup:t('validation.required',{field:t('taxGroup.fields.group','Sales tax group')})}:{}),...(!r.taxGroupName.trim()?{taxGroupName:t('validation.required',{field:t('taxGroup.fields.description','Description')})}:{})}),
  advancedFilter:{fieldLabel:t('taxGroup.fields.group','Sales tax group'),getValue:(r)=>r.taxGroup,matches:(r,value)=>`${r.taxGroup} ${r.taxGroupName}`.toLowerCase().includes(value.trim().toLowerCase())},presentation:{mode:'list',listWidth:240,listResizable:true,recordHeaderMinHeight:96},
 }),[t,taxCodes.data]);
 return <ListDetailsListGridPage title={t('taxGroup.title','Sales tax groups')} config={config}/>;
}
