import React from 'react';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import type { TaxPeriodInterval, TaxPeriodRecord } from '../api/taxPeriodApi';

const dateOnly = (value: unknown): string => String(value ?? '').slice(0, 10);

export function TaxPeriodIntervalsPanel({
  record,
  editing,
  onRecordChange,
}: {
  record: TaxPeriodRecord;
  editing: boolean;
  onRecordChange: (record: TaxPeriodRecord) => void;
}): React.ReactElement {
  const gridRef = React.useRef<DataGridHandle>(null);
  const [selectedIds, setSelectedIds] = React.useState<(string | number)[]>([]);
  const columns = React.useMemo<ColumnDef<TaxPeriodInterval>[]>(() => [
    { field: 'fromDate', headerName: 'From date', type: 'date', width: 170, editable: true, valueGetter: ({ row }) => dateOnly(row.fromDate) },
    { field: 'toDate', headerName: 'To date', type: 'date', width: 170, editable: true, valueGetter: ({ row }) => dateOnly(row.toDate) },
    { field: 'closed', headerName: 'Blocked for settlement process', type: 'boolean', minWidth: 230, flex: 1, editable: true, valueGetter: ({ row }) => row.closed === 1 },
  ], []);
  const newRow = (): Partial<TaxPeriodInterval> => ({
    id: `new-${crypto.randomUUID()}`, recId: 0, taxPeriod: record.taxPeriod,
    fromDate: new Date().toISOString().slice(0, 10), toDate: new Date().toISOString().slice(0, 10), closed: 0,
  });
  const saveRow = (partial: Partial<TaxPeriodInterval>, isNew: boolean) => {
    const row = partial as TaxPeriodInterval;
    const normalized = { ...row, fromDate: dateOnly(row.fromDate), toDate: dateOnly(row.toDate), closed: Number(row.closed) === 1 ? 1 : 0 } as TaxPeriodInterval;
    onRecordChange({ ...record, intervals: isNew ? [...record.intervals, normalized] : record.intervals.map((item) => item.id === normalized.id ? normalized : item) });
    setSelectedIds([normalized.id]);
  };
  const remove = () => {
    const selected = new Set(selectedIds.map(String));
    onRecordChange({ ...record, intervals: record.intervals.filter((row) => !selected.has(String(row.id))) });
    setSelectedIds([]);
  };
  return (
    <TabularDetailPanel
      gridRef={gridRef}
      rows={record.intervals}
      columns={columns}
      selectedIds={selectedIds}
      onSelectionChange={setSelectedIds}
      addLabel="Add"
      removeLabel="Remove"
      disabled={!editing}
      onAdd={editing ? () => gridRef.current?.startAddRow() : undefined}
      onRemove={editing ? remove : undefined}
      masterForm
      onNewRow={newRow}
      onRowSave={saveRow}
      storageKey="tax-periods.intervals"
      height={410}
    />
  );
}
