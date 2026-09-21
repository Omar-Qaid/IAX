import { localizedName } from '@shared/utilities/localizedName';
import React, { useMemo, useRef, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import {
  organizationStructureApi,
  type WorkerOrganizationAssignment,
} from '../api/organizationStructureApi';

const currentDate = (): string => new Date().toISOString().slice(0, 10);
type Row = WorkerOrganizationAssignment & { id: string; positionName: string };

interface HcmWorkerAssignmentsPanelProps {
  workerId: number;
  company: string;
  editing: boolean;
}

export function HcmWorkerAssignmentsPanel({
  workerId,
  company,
  editing,
}: HcmWorkerAssignmentsPanelProps): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const gridRef = useRef<DataGridHandle>(null);
  const [selectedIds, setSelectedIds] = useState<(string | number)[]>([]);
  const asOf = currentDate();

  const assignments = useQuery({
    queryKey: ['hcm-worker-assignments', company, workerId, asOf],
    queryFn: ({ signal }) => organizationStructureApi.workerAssignments(workerId, asOf, signal),
    enabled: workerId > 0,
  });
  const positions = useQuery({
    queryKey: ['organization-structure', company, 'positions', asOf],
    queryFn: ({ signal }) => organizationStructureApi.positions(asOf, signal),
    enabled: workerId > 0,
  });
  const roles = useQuery({
    queryKey: ['organization-structure', company, 'roles'],
    queryFn: ({ signal }) => organizationStructureApi.roles(signal),
    enabled: workerId > 0,
  });

  const rows = useMemo<Row[]>(
    () =>
      (assignments.data ?? []).map((assignment) => {
        const role = roles.data?.find((item) => item.id === assignment.organizationRoleId);
        return {
          ...assignment,
          id: String(assignment.assignmentId),
          positionName: localizedName(
            positions.data?.find((position) => position.id === assignment.positionId),
            isRtl
          ),
          roleCode: role
            ? `${role.code} — ${localizedName(role, isRtl)}`
            : (assignment.roleCode ?? ''),
        };
      }),
    [assignments.data, positions.data, roles.data, isRtl]
  );

  const columns = useMemo<ColumnDef<Row>[]>(
    () => [
      {
        field: 'positionId',
        headerName: t('hcmWorkers.assignments.position'),
        minWidth: 250,
        flex: 1,
        editable: editing,
        type: 'singleSelect',
        valueOptions: (positions.data ?? []).map((position) => ({
          value: position.id,
          label: `${position.code} — ${localizedName(position, isRtl)}`,
        })),
      },
      { field: 'roleCode', headerName: t('hcmWorkers.assignments.role'), minWidth: 210 },
      {
        field: 'validFrom',
        headerName: t('hcmWorkers.assignments.validFrom'),
        width: 135,
        editable: editing,
        type: 'date',
      },
      {
        field: 'validTo',
        headerName: t('hcmWorkers.assignments.validTo'),
        width: 135,
        editable: editing,
        type: 'date',
      },
      {
        field: 'isPrimary',
        headerName: t('hcmWorkers.assignments.primary'),
        type: 'boolean',
        width: 95,
        editable: editing,
      },
    ],
    [editing, positions.data, t, isRtl]
  );

  const save = async (values: Partial<Row>, isNew: boolean): Promise<void> => {
    const positionId = Number(values.positionId) || 0;
    const validFrom = String(values.validFrom ?? '');
    const validTo = values.validTo ? String(values.validTo) : null;
    const isPrimary = Boolean(values.isPrimary);
    if (positionId <= 0) throw new Error(t('hcmWorkers.assignments.positionRequired'));
    if (!validFrom) throw new Error(t('hcmWorkers.assignments.validFromRequired'));
    if (validTo && validTo <= validFrom) {
      throw new Error(t('hcmWorkers.assignments.validToAfterValidFrom'));
    }

    if (isNew) {
      await organizationStructureApi.assignWorker({
        workerId,
        positionId,
        validFrom,
        validTo,
        isPrimary,
      });
    } else {
      await organizationStructureApi.updateWorkerAssignment(
        Number(values.assignmentId ?? values.id),
        { positionId, validFrom, validTo, isPrimary }
      );
    }
    await assignments.refetch();
  };

  const close = async (): Promise<void> => {
    const id = Number(selectedIds.at(-1));
    if (!id) return;
    await organizationStructureApi.closeAssignment(id, currentDate());
    setSelectedIds([]);
    await assignments.refetch();
  };

  const disabled =
    !editing || workerId <= 0 || assignments.isLoading || positions.isLoading || roles.isLoading;

  return (
    <TabularDetailPanel
      rows={rows}
      columns={columns}
      addLabel={t('hcmWorkers.assignments.add')}
      removeLabel={t('hcmWorkers.assignments.close')}
      selectedIds={selectedIds}
      onSelectionChange={setSelectedIds}
      onAdd={() => gridRef.current?.startAddRow()}
      onRemove={close}
      onRowSave={editing ? save : undefined}
      onNewRow={() => ({
        id: `new-${crypto.randomUUID()}`,
        assignmentId: 0,
        workerId,
        positionId: null,
        organizationUnitId: 0,
        organizationRoleId: 0,
        roleCode: null,
        isPrimary: false,
        validFrom: currentDate(),
        validTo: null,
        positionName: '',
      })}
      gridRef={gridRef}
      masterForm
      disabled={disabled}
      storageKey="organization.worker.assignments"
      height={220}
    />
  );
}
