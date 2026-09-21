import React, { useMemo, useRef, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import { organizationStructureApi, type WorkerOrganizationAssignment } from '../api/organizationStructureApi';

const today = () => new Date().toISOString().slice(0, 10);
type Row = WorkerOrganizationAssignment & { id: string; positionName: string };

export function HcmWorkerAssignmentsPanel({ workerId, company }: { workerId: number; company: string }): React.ReactElement {
  const gridRef = useRef<DataGridHandle>(null);
  const [selectedIds, setSelectedIds] = useState<(string | number)[]>([]);
  const assignments = useQuery({ queryKey: ['hcm-worker-assignments', company, workerId, today()], queryFn: ({ signal }) => organizationStructureApi.workerAssignments(workerId, today(), signal), enabled: workerId > 0 });
  const positions = useQuery({ queryKey: ['organization-structure', company, 'positions', 'worker-assignment'], queryFn: ({ signal }) => organizationStructureApi.positions(today(), signal), enabled: workerId > 0 });
  const roles = useQuery({ queryKey: ['organization-structure', company, 'roles'], queryFn: ({ signal }) => organizationStructureApi.roles(signal), enabled: workerId > 0 });
  const rows = useMemo<Row[]>(() => (assignments.data ?? []).map((assignment) => {
    const role = roles.data?.find((item) => item.id === assignment.organizationRoleId);
    return { ...assignment, id: String(assignment.assignmentId), positionName: positions.data?.find((position) => position.id === assignment.positionId)?.name ?? '', roleCode: role ? `${role.code} — ${role.name}` : assignment.roleCode ?? '' };
  }), [assignments.data, positions.data, roles.data]);
  const columns = useMemo<ColumnDef<Row>[]>(() => [
    { field: 'positionId', headerName: 'Position', minWidth: 250, flex: 1, editable: true, type: 'singleSelect', valueOptions: (positions.data ?? []).map((position) => ({ value: position.id, label: `${position.code} — ${position.name}` })) },
    { field: 'roleCode', headerName: 'Role', minWidth: 210 },
    { field: 'validFrom', headerName: 'Valid from', width: 135, editable: true, type: 'date' },
    { field: 'validTo', headerName: 'Valid to', width: 135, editable: true, type: 'date' },
    { field: 'isPrimary', headerName: 'Primary', type: 'boolean', width: 95, editable: true },
  ], [positions.data]);
  const save = async (values: Partial<Row>, isNew: boolean) => {
    const positionId = Number(values.positionId) || 0;
    const validFrom = String(values.validFrom ?? '');
    const validTo = values.validTo ? String(values.validTo) : null;
    const isPrimary = Boolean(values.isPrimary);
    if (positionId <= 0) throw new Error('Position is required.');
    if (!validFrom) throw new Error('Valid from is required.');
    if (validTo && validTo <= validFrom) throw new Error('Valid to must be later than valid from.');
    if (isNew) await organizationStructureApi.assignWorker({ workerId, positionId, validFrom, validTo, isPrimary });
    else await organizationStructureApi.updateWorkerAssignment(Number(values.assignmentId ?? values.id), { positionId, validFrom, validTo, isPrimary });
    await assignments.refetch();
  };
  const close = async () => { const id = Number(selectedIds.at(-1)); if (!id) return; await organizationStructureApi.closeAssignment(id, today()); setSelectedIds([]); await assignments.refetch(); };
  return <TabularDetailPanel rows={rows} columns={columns} addLabel="Add assignment" removeLabel="Close assignment" selectedIds={selectedIds} onSelectionChange={setSelectedIds} onAdd={() => gridRef.current?.startAddRow()} onRemove={close} onRowSave={save} onNewRow={() => ({ id: `new-${crypto.randomUUID()}`, assignmentId: 0, workerId, positionId: null, organizationUnitId: 0, organizationRoleId: 0, roleCode: null, isPrimary: false, validFrom: today(), validTo: null, positionName: '' })} gridRef={gridRef} masterForm disabled={workerId <= 0 || assignments.isLoading || positions.isLoading || roles.isLoading} storageKey="organization.worker.assignments" height={220} />;
}