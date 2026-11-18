import React from 'react';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import type { TaxTableRecord } from '../api/taxTableApi';
import type { TaxGroupLine, TaxGroupRecord } from '../api/taxGroupApi';

export function TaxGroupLinesPanel({ record, editing, taxCodes, onRecordChange }: {
  record: TaxGroupRecord; editing: boolean; taxCodes: TaxTableRecord[]; onRecordChange: (record: TaxGroupRecord) => void;
}): React.ReactElement {
  const gridRef = React.useRef<DataGridHandle>(null);
  const [selectedIds,setSelectedIds] = React.useState<(string|number)[]>([]);
  const codeOptions = React.useMemo(() => taxCodes.map((tax) => ({ value: tax.taxCode, label: `${tax.taxCode} - ${tax.taxName}` })),[taxCodes]);
  const columns = React.useMemo<ColumnDef<TaxGroupLine>[]>(() => [
    {field:'taxCode',headerName:'Sales tax code',type:'singleSelect',valueOptions:codeOptions,width:175,editable:true},
    {field:'exemptTax',headerName:'Exempt',type:'boolean',width:90,editable:true,valueGetter:({row})=>row.exemptTax===1},
    {field:'taxExemptCode',headerName:'Exempt code',width:145,editable:true},
    {field:'intracomVat',headerName:'Exempt code (zero-rated)',type:'boolean',width:190,editable:true,valueGetter:({row})=>row.intracomVat===1},
    {field:'reverseCharge_W',headerName:'Reverse charge',type:'boolean',width:130,editable:true,valueGetter:({row})=>row.reverseCharge_W===1},
    {field:'useTax',headerName:'Use tax',type:'boolean',width:95,editable:true,valueGetter:({row})=>row.useTax===1},
    {field:'taxValue',headerName:'Percentage/Amount',type:'number',width:150,editable:false},
    {field:'taxCodeName',headerName:'Name',minWidth:180,flex:1,editable:false},
  ],[codeOptions]);
  const newRow = (): Partial<TaxGroupLine> => ({id:`new-${crypto.randomUUID()}`,recId:0,taxGroup:record.taxGroup,taxCode:taxCodes[0]?.taxCode ?? '',taxExemptCode:'NONE',exemptTax:0,useTax:0,intracomVat:0,reverseCharge_W:0,taxCodeName:taxCodes[0]?.taxName ?? '',taxValue:taxCodes[0]?.taxValue ?? 0});
  const saveRow = (partial: Partial<TaxGroupLine>,isNew:boolean) => {
    const row = partial as TaxGroupLine;
    const tax = taxCodes.find((item)=>item.taxCode===row.taxCode);
    const normalized: TaxGroupLine = {...row,taxGroup:record.taxGroup,taxCode:row.taxCode ?? '',taxExemptCode:row.taxExemptCode?.trim().toUpperCase() || 'NONE',exemptTax:Number(row.exemptTax)===1?1:0,useTax:Number(row.useTax)===1?1:0,intracomVat:Number(row.intracomVat)===1?1:0,reverseCharge_W:Number(row.reverseCharge_W)===1?1:0,taxCodeName:tax?.taxName ?? '',taxValue:tax?.taxValue ?? 0};
    const withoutDuplicate = record.lines.filter((item)=>item.id===normalized.id || item.taxCode!==normalized.taxCode);
    onRecordChange({...record,lines:isNew?[...withoutDuplicate,normalized]:withoutDuplicate.map((item)=>item.id===normalized.id?normalized:item)});
    setSelectedIds([normalized.id]);
  };
  const remove = () => { const selected=new Set(selectedIds.map(String)); onRecordChange({...record,lines:record.lines.filter((line)=>!selected.has(String(line.id)))}); setSelectedIds([]); };
  return <TabularDetailPanel gridRef={gridRef} rows={record.lines} columns={columns} selectedIds={selectedIds} onSelectionChange={setSelectedIds} addLabel="Add" removeLabel="Remove" disabled={!editing} onAdd={editing?()=>gridRef.current?.startAddRow():undefined} onRemove={editing?remove:undefined} masterForm onNewRow={newRow} onRowSave={saveRow} storageKey="tax-groups.lines" height={220}/>;
}
