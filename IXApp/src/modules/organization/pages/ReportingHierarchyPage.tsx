import React, { useMemo, useRef, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useCompanyStore } from '@core/company/useCompanyStore';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { DetailValues, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import { organizationStructureApi as api, type PositionReportingLine } from '../api/organizationStructureApi';

type Record = { id: string; recordId: number; code: string; name: string; purpose: string };
type Line = Omit<PositionReportingLine, 'id'> & { id: string };
const today = () => new Date().toISOString().slice(0, 10);

export function ReportingHierarchyPage(): React.ReactElement {
  const company = useCompanyStore((state) => state.currentCompany);
  const [selected, setSelected] = useState<Record | null>(null);
  const positions = useQuery({ queryKey: ['organization-structure', company, 'positions', 'reporting'], queryFn: ({ signal }) => api.positions(today(), signal) });
  const lines = useQuery({ queryKey: ['organization-structure', company, 'reporting-lines', selected?.recordId], queryFn: ({ signal }) => api.reportingLines(selected!.recordId, today(), signal), enabled: Boolean(selected?.recordId) });
  const config: EnterpriseListDetailsConfig<Record> = {
    recordTableName: 'HcmReportingHierarchy',
    dataSource: {
      type: 'remote', key: `reporting-hierarchies-${company}`,
      load: async (signal) => (await api.reportingHierarchies(signal)).map((x) => ({ ...x, id: String(x.id), recordId: x.id })),
      create: async (r) => { const recordId = await api.createReportingHierarchy({ code: r.code.trim(), name: r.name.trim(), purpose: r.purpose.trim() }); return { ...r, id: String(recordId), recordId }; },
      update: async (r) => { await api.updateReportingHierarchy(r.recordId, { code: r.code.trim(), name: r.name.trim(), purpose: r.purpose.trim() }); return r; },
    },
    createRecord: () => ({ id: `new-${crypto.randomUUID()}`, recordId: 0, code: '', name: '', purpose: '' }),
    getPrimaryText: (r) => r.name, getSecondaryText: (r) => r.code,
    matchesSearch: (r, q) => `${r.code} ${r.name} ${r.purpose}`.toLowerCase().includes(q.toLowerCase()),
    getValues: (): DetailValues => ({}), setValues: (r) => r,
    headerFields: [
      { id: 'code', label: 'Code', width: 180, getValue: (r) => r.code, setValue: (r, v) => ({ ...r, code: String(v ?? '') }) },
      { id: 'name', label: 'Name', width: 320, getValue: (r) => r.name, setValue: (r, v) => ({ ...r, name: String(v ?? '') }) },
      { id: 'purpose', label: 'Purpose', width: 240, getValue: (r) => r.purpose, setValue: (r, v) => ({ ...r, purpose: String(v ?? '') }) },
    ],
    onSelectionChange: setSelected,
    sections: ({ record }) => [{ id: 'lines', title: 'Position reporting lines', minHeight: 300, content: <ReportingLinesPanel hierarchyId={record.recordId} lines={lines.data ?? []} positions={positions.data ?? []} loading={lines.isLoading || positions.isLoading} refresh={() => lines.refetch()} /> }],
    permissions: { view: 'Organization.Structure.View', create: 'Organization.Structure.Create', edit: 'Organization.Structure.Edit' },
    validate: (r) => ({ ...(!r.code.trim() ? { code: 'Code is required.' } : {}), ...(!r.name.trim() ? { name: 'Name is required.' } : {}), ...(!r.purpose.trim() ? { purpose: 'Purpose is required.' } : {}) }),
  };
  return <ListDetailsPage variant="enterprise" title="Reporting hierarchies" config={config} />;
}

function ReportingLinesPanel({ hierarchyId, lines, positions, loading, refresh }: { hierarchyId: number; lines: PositionReportingLine[]; positions: Awaited<ReturnType<typeof api.positions>>; loading: boolean; refresh: () => Promise<unknown> }) {
  const ref = useRef<DataGridHandle>(null); const [selectedIds, setSelectedIds] = useState<(string | number)[]>([]);
  const rows = useMemo<Line[]>(() => lines.map((x) => ({ ...x, id: String(x.id) })), [lines]);
  const options = positions.map((p) => ({ value: p.id, label: `${p.code} — ${p.name}` }));
  const columns: ColumnDef<Line>[] = [
    { field: 'subordinatePositionId', headerName: 'Subordinate position', minWidth: 280, flex: 1, editable: true, type: 'singleSelect', valueOptions: options },
    { field: 'managerPositionId', headerName: 'Manager position', minWidth: 280, flex: 1, editable: true, type: 'singleSelect', valueOptions: options },
    { field: 'validFrom', headerName: 'Valid from', width: 135, editable: true, type: 'date' },
    { field: 'validTo', headerName: 'Valid to', width: 135, editable: true, type: 'date' },
    { field: 'isPrimary', headerName: 'Primary', width: 100, editable: true, type: 'boolean' },
  ];
  const save = async (v: Partial<Line>, isNew: boolean) => { const payload = { subordinatePositionId: Number(v.subordinatePositionId), managerPositionId: Number(v.managerPositionId), validFrom: String(v.validFrom), validTo: v.validTo ? String(v.validTo) : null, isPrimary: Boolean(v.isPrimary) }; if (!payload.subordinatePositionId || !payload.managerPositionId) throw new Error('Both positions are required.'); if (payload.subordinatePositionId === payload.managerPositionId) throw new Error('A position cannot manage itself.'); if (isNew) await api.createReportingLine({ reportingHierarchyId: hierarchyId, ...payload }); else await api.updateReportingLine(Number(v.id), payload); await refresh(); };
  const close = async () => { const id = Number(selectedIds.at(-1)); if (!id) return; await api.closeReportingLine(id, today()); setSelectedIds([]); await refresh(); };
  return <TabularDetailPanel rows={rows} columns={columns} addLabel="Add reporting line" removeLabel="Close reporting line" selectedIds={selectedIds} onSelectionChange={setSelectedIds} onAdd={() => ref.current?.startAddRow()} onRemove={close} onRowSave={save} onNewRow={() => ({ id: `new-${crypto.randomUUID()}`, reportingHierarchyId: hierarchyId, subordinatePositionId: 0, managerPositionId: 0, validFrom: today(), validTo: null, isPrimary: true })} gridRef={ref} masterForm disabled={hierarchyId <= 0 || loading} storageKey="organization.reporting.lines" height={270} />;
}
