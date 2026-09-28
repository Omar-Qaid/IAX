import React, { useMemo, useRef, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { AppLookupField } from '@shared/components/fields/AppLookupField';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import { localizedName } from '@shared/utilities/localizedName';
import { hcmWorkerApi } from '../api/hcmWorkerApi';
import { hcmShowroomApi, type HcmShowroomWorkerAssignment } from '../api/hcmShowroomApi';

type AssignmentRow = HcmShowroomWorkerAssignment & { id: string };

interface Props {
  showroomId: number;
  company: string;
  history?: boolean;
  showFilterRow?: boolean;
}

export function HcmShowroomAssignmentsPanel({
  showroomId,
  company,
  history = false,
  showFilterRow = false,
}: Props): React.ReactElement {
  const { t, isRtl } = useAppTranslation();
  const queryClient = useQueryClient();
  const gridRef = useRef<DataGridHandle>(null);
  const [selectedIds, setSelectedIds] = useState<(string | number)[]>([]);
  const queryKey = ['hcm-showroom-worker-assignments', company, showroomId] as const;
  const assignments = useQuery({
    queryKey,
    queryFn: ({ signal }) => hcmShowroomApi.workerAssignments(showroomId, signal),
    enabled: showroomId > 0,
  });

  const rows = useMemo<AssignmentRow[]>(
    () =>
      (assignments.data ?? [])
        .filter((assignment) => (history ? !assignment.isPrimary : assignment.isPrimary))
        .map((assignment) => ({ ...assignment, id: String(assignment.recId) })),
    [assignments.data, history]
  );

  const columns = useMemo<ColumnDef<AssignmentRow>[]>(
    () => [
      {
        field: 'hcmWorkerId',
        headerName: t('hcmShowrooms.fields.worker'),
        minWidth: 260,
        flex: 1,
        editable: !history,
        valueGetter: ({ row }) =>
          `${row.personnelNumber} — ${localizedName({ name: row.workerName, nameAlias: row.workerNameAlias }, isRtl)}`,
      renderEditCell: ({ value, onChange, disabled }) => (
          <AppLookupField
            name="hcmWorkerId"
            label={t('hcmShowrooms.fields.worker')}
            value={Number(value) || 0}
            onChange={(next) => onChange(Number(next) || 0)}
            fetchPage={({ pageNumber, pageSize, search, signal }) =>
              hcmWorkerApi.managerLookup({
                pageNumber,
                pageSize,
                search,
                selectedId: Number(value) || undefined,
                signal,
              })
            }
            queryKey={['hcm-showroom-worker-lookup', company, Number(value) || 0]}
          disabled={disabled}
            displayMode="select"
            sideMode="server"
            searchable
            lazyLoading
            pageSize={25}
            required
          />
        ),
      },
      {
        field: 'validFrom',
        headerName: t('hcmWorkers.assignments.validFrom'),
        width: 145,
        type: 'date',
        editable: !history,
      },
      {
        field: 'validTo',
        headerName: t('hcmWorkers.assignments.validTo'),
        width: 145,
        type: 'date',
        editable: !history,
      },
      {
        field: 'isPrimary',
        headerName: t('hcmWorkers.assignments.primary'),
        width: 100,
        type: 'boolean',
      },
      {
        field: 'isActive',
        headerName: t('hcmWorkers.fields.active'),
        width: 100,
        type: 'boolean',
        editable: !history,
      },
    ],
    [company, history, isRtl, t]
  );

  return (
    <TabularDetailPanel<AssignmentRow>
      gridRef={gridRef}
      rows={rows}
      columns={columns}
      addLabel={history ? '' : t('hcmShowrooms.assignments.add')}
      removeLabel=""
      selectedIds={selectedIds}
      onSelectionChange={setSelectedIds}
      onAdd={history || showroomId <= 0 ? undefined : () => gridRef.current?.startAddRow()}
      disabled={history || showroomId <= 0}
      showFilterRow={showFilterRow}
      storageKey={`organization.showroom.${history ? 'history' : 'primary'}-assignments`}
      height={220}
      masterForm={!history}
      onNewRow={() => ({
        id: `new-${crypto.randomUUID()}`,
        recId: 0,
        hcmWorkerId: 0,
        personnelNumber: '',
        workerName: '',
        workerNameAlias: '',
        validFrom: new Date().toISOString().slice(0, 10),
        validTo: null,
        isPrimary: true,
        isActive: true,
      })}
      onRowSave={async (values, isNew) => {
        const row = values as AssignmentRow;
        if (!row.hcmWorkerId)
          throw new Error(t('validation.required', { field: t('hcmShowrooms.fields.worker') }));
        if (!row.validFrom)
          throw new Error(
            t('validation.required', { field: t('hcmWorkers.assignments.validFrom') })
          );
        await hcmShowroomApi.saveWorkerAssignment(showroomId, isNew ? null : row.recId, {
          hcmWorkerId: row.hcmWorkerId,
          validFrom: row.validFrom,
          validTo: row.validTo || null,
          isActive: row.isActive,
        });
        await queryClient.invalidateQueries({ queryKey });
      }}
    />
  );
}
