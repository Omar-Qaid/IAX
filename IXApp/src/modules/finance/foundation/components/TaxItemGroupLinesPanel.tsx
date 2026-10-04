import React from 'react';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import type { TaxTableRecord } from '../api/taxTableApi';
import type { TaxItemGroupLine, TaxItemGroupRecord } from '../api/taxItemGroupApi';

export function TaxItemGroupLinesPanel({
  record,
  editing,
  taxCodes,
  onRecordChange,
}: {
  record: TaxItemGroupRecord;
  editing: boolean;
  taxCodes: TaxTableRecord[];
  onRecordChange: (record: TaxItemGroupRecord) => void;
}): React.ReactElement {
  const gridRef = React.useRef<DataGridHandle>(null);
  const [selectedIds, setSelectedIds] = React.useState<(string | number)[]>([]);
  const codeOptions = React.useMemo(
    () => taxCodes.map((tax) => ({ value: tax.taxCode, label: `${tax.taxCode} - ${tax.taxName}` })),
    [taxCodes]
  );
  const columns = React.useMemo<ColumnDef<TaxItemGroupLine>[]>(() => [
    { field: 'taxCode', headerName: 'Sales tax code', type: 'singleSelect', valueOptions: codeOptions, width: 205, editable: true },
    { field: 'taxValue', headerName: 'Percentage/Amount', type: 'number', width: 175 },
    { field: 'taxCodeName', headerName: 'Name', minWidth: 260, flex: 1 },
  ], [codeOptions]);
  const newRow = (): Partial<TaxItemGroupLine> => ({
    id: `new-${crypto.randomUUID()}`,
    recId: 0,
    taxItemGroup: record.taxItemGroup,
    taxCode: taxCodes.find((tax) => !record.lines.some((line) => line.taxCode === tax.taxCode))?.taxCode ?? '',
    taxExemptCode: 'NONE',
    taxCodeName: '',
    taxValue: 0,
  });
  const saveRow = (partial: Partial<TaxItemGroupLine>, isNew: boolean) => {
    const row = partial as TaxItemGroupLine;
    const tax = taxCodes.find((item) => item.taxCode === row.taxCode);
    if (!tax || record.lines.some((item) => item.id !== row.id && item.taxCode === row.taxCode)) return;
    const normalized: TaxItemGroupLine = {
      ...row,
      taxItemGroup: record.taxItemGroup,
      taxCode: tax.taxCode,
      taxExemptCode: row.taxExemptCode?.trim().toUpperCase() || 'NONE',
      taxCodeName: tax.taxName,
      taxValue: tax.taxValue,
    };
    onRecordChange({
      ...record,
      lines: isNew
        ? [...record.lines, normalized]
        : record.lines.map((item) => item.id === normalized.id ? normalized : item),
    });
    setSelectedIds([normalized.id]);
  };
  const remove = () => {
    const selected = new Set(selectedIds.map(String));
    onRecordChange({ ...record, lines: record.lines.filter((line) => !selected.has(String(line.id))) });
    setSelectedIds([]);
  };
  return (
    <TabularDetailPanel
      gridRef={gridRef}
      rows={record.lines}
      columns={columns}
      selectedIds={selectedIds}
      onSelectionChange={setSelectedIds}
      addLabel="Add"
      removeLabel="Remove"
      disabled={!editing || taxCodes.length === 0}
      onAdd={editing ? () => gridRef.current?.startAddRow() : undefined}
      onRemove={editing ? remove : undefined}
      masterForm
      onNewRow={newRow}
      onRowSave={saveRow}
      storageKey="tax-item-groups.lines"
      height={500}
    />
  );
}
