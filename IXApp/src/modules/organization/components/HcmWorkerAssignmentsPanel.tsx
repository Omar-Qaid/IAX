import React, { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { ColumnDef } from '@shared/components/data-grid/types';
import { localizedName } from '@shared/utilities/localizedName';
import {
  hcmWorkerApi,
  type HcmWorkerOrganizationAssignmentV1Record,
  type HcmWorkerShowroomAssignmentRecord,
} from '../api/hcmWorkerApi';

type OrganizationRow = HcmWorkerOrganizationAssignmentV1Record & { id: string };
type ShowroomRow = HcmWorkerShowroomAssignmentRecord & { id: string };

interface HcmWorkerAssignmentsPanelProps {
  workerId: number;
  company: string;
  type: 'organization' | 'showroom';
  editing?: boolean;
  showFilterRow?: boolean;
}

export function HcmWorkerAssignmentsPanel({
  workerId,
  company,
  type,
  showFilterRow = false,
}: HcmWorkerAssignmentsPanelProps): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const [selectedIds, setSelectedIds] = useState<(string | number)[]>([]);
  const organizationAssignments = useQuery({
    queryKey: ['hcm-worker-organization-assignments-v1', company, workerId],
    queryFn: ({ signal }) => hcmWorkerApi.organizationAssignmentsV1(workerId, signal),
    enabled: workerId > 0 && type === 'organization',
  });
  const showroomAssignments = useQuery({
    queryKey: ['hcm-worker-showroom-assignments', company, workerId],
    queryFn: ({ signal }) => hcmWorkerApi.showroomAssignments(workerId, signal),
    enabled: workerId > 0 && type === 'showroom',
  });

  const organizationRows = useMemo<OrganizationRow[]>(
    () => (organizationAssignments.data ?? []).map((assignment) => ({ ...assignment, id: String(assignment.recId) })),
    [organizationAssignments.data]
  );
  const showroomRows = useMemo<ShowroomRow[]>(
    () => (showroomAssignments.data ?? []).map((assignment) => ({ ...assignment, id: String(assignment.recId) })),
    [showroomAssignments.data]
  );

  const organizationColumns = useMemo<ColumnDef<OrganizationRow>[]>(
    () => [
      {
        field: 'managerName', headerName: t('hcmWorkers.assignments.manager'), minWidth: 220, flex: 1,
        valueGetter: ({ row }) => `${row.managerPersonnelNumber} — ${localizedName({ name: row.managerName, nameAlias: row.managerNameAlias }, isRtl)}`,
      },
      {
        field: 'departmentName', headerName: t('hcmWorkers.fields.department'), minWidth: 190, flex: 1,
        valueGetter: ({ row }) => localizedName({ name: row.departmentName, nameAlias: row.departmentNameAlias }, isRtl),
      },
      {
        field: 'occupationName', headerName: t('hcmWorkers.fields.occupation'), minWidth: 180, flex: 1,
        valueGetter: ({ row }) => localizedName({ name: row.occupationName, nameAlias: row.occupationNameAlias }, isRtl),
      },
      dateColumn<OrganizationRow>('validFrom', t('hcmWorkers.assignments.validFrom')),
      dateColumn<OrganizationRow>('validTo', t('hcmWorkers.assignments.validTo')),
      booleanColumn<OrganizationRow>('isPrimary', t('hcmWorkers.assignments.primary')),
      booleanColumn<OrganizationRow>('isActive', t('hcmWorkers.fields.active')),
    ], [isRtl, t]
  );

  const showroomColumns = useMemo<ColumnDef<ShowroomRow>[]>(
    () => [
      {
        field: 'showroomName', headerName: t('hcmWorkers.fields.showroom'), minWidth: 260, flex: 1,
        valueGetter: ({ row }) => localizedName({ name: row.showroomName, nameAlias: row.showroomNameAlias }, isRtl),
      },
      dateColumn<ShowroomRow>('validFrom', t('hcmWorkers.assignments.validFrom')),
      dateColumn<ShowroomRow>('validTo', t('hcmWorkers.assignments.validTo')),
      booleanColumn<ShowroomRow>('isPrimary', t('hcmWorkers.assignments.primary')),
      booleanColumn<ShowroomRow>('isActive', t('hcmWorkers.fields.active')),
    ], [isRtl, t]
  );

  return type === 'organization' ? (
    <TabularDetailPanel<OrganizationRow>
      rows={organizationRows} columns={organizationColumns} addLabel="" removeLabel=""
      selectedIds={selectedIds} onSelectionChange={setSelectedIds} disabled
      showFilterRow={showFilterRow}
      storageKey="organization.worker.organization-assignments" height={220}
    />
  ) : (
    <TabularDetailPanel<ShowroomRow>
      rows={showroomRows} columns={showroomColumns} addLabel="" removeLabel=""
      selectedIds={selectedIds} onSelectionChange={setSelectedIds} disabled
      showFilterRow={showFilterRow}
      storageKey="organization.worker.showroom-assignments" height={220}
    />
  );
}

function dateColumn<T>(field: keyof T & string, headerName: string): ColumnDef<T> {
  return { field, headerName, width: 135, type: 'date' };
}

function booleanColumn<T>(field: keyof T & string, headerName: string): ColumnDef<T> {
  return { field, headerName, width: 95, type: 'boolean' };
}
