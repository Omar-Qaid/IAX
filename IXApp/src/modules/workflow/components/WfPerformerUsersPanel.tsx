import React, { forwardRef, useImperativeHandle, useMemo, useRef, useState } from 'react';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { ColumnDef, DataGridHandle } from '@shared/components/data-grid/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { localizedName } from '@shared/utilities/localizedName';
import { AppLookupField } from '@shared/components/fields/AppLookupField';
import { loadWfPerformerWorkerLookup, type WfPerformerUserOption } from '../api/wfPerformerApi';

interface Props {
  userIds: number[];
  userOptions: WfPerformerUserOption[];
  editing: boolean;
  onChange: (userIds: number[], userOptions: WfPerformerUserOption[]) => void;
  showFilterRow?: boolean;
}

export interface WfPerformerUsersPanelHandle {
  savePendingRow: () => Promise<{
    userIds: number[];
    userOptions: WfPerformerUserOption[];
  }>;
}

interface PerformerWorkerRow {
  id: string;
  workerId: number;
  code: string;
  name: string;
  nameAlias?: string | null;
}

export const WfPerformerUsersPanel = forwardRef<WfPerformerUsersPanelHandle, Props>(
  function WfPerformerUsersPanel(
    { userIds = [], userOptions = [], editing, onChange, showFilterRow = false },
    ref
  ): React.ReactElement {
    const { t, isRtl } = useAppTranslation();
    const gridRef = useRef<DataGridHandle>(null);
    const editingRowRef = useRef(false);
    const currentIdsRef = useRef(userIds);
    const currentOptionsRef = useRef(userOptions);
    const selectedOptionRef = useRef<WfPerformerUserOption | null>(null);
    const [selectedIds, setSelectedIds] = useState<(string | number)[]>([]);

    currentIdsRef.current = userIds;
    currentOptionsRef.current = userOptions;

    const optionById = useMemo(
      () => new Map(userOptions.map((option) => [option.id, option])),
      [userOptions]
    );
    const rows = useMemo<PerformerWorkerRow[]>(
      () =>
        userIds.map((workerId) => {
          const option = optionById.get(workerId);
          return {
            id: String(workerId),
            workerId,
            code: option?.code ?? String(workerId),
            name: option?.name ?? String(workerId),
            nameAlias: option?.nameAlias,
          };
        }),
      [optionById, userIds]
    );

    const updateUsers = (nextIds: number[], nextOption?: WfPerformerUserOption | null) => {
      const options = new Map(currentOptionsRef.current.map((option) => [option.id, option]));
      if (nextOption) options.set(nextOption.id, nextOption);
      const nextOptions = nextIds.flatMap((id) => {
        const option = options.get(id);
        return option ? [option] : [];
      });
      currentIdsRef.current = nextIds;
      currentOptionsRef.current = nextOptions;
      onChange(nextIds, nextOptions);
    };

    useImperativeHandle(ref, () => ({
      savePendingRow: async () => {
        if (editingRowRef.current) {
          const saved = await gridRef.current?.saveEdit();
          if (!saved) throw new Error(t('wfPerformers.validation.workerRequired'));
        }
        return {
          userIds: currentIdsRef.current,
          userOptions: currentOptionsRef.current,
        };
      },
    }));

    const columns = useMemo<ColumnDef<PerformerWorkerRow>[]>(
      () => [
        {
          field: 'workerId',
          headerName: t('wfPerformers.fields.worker'),
          minWidth: 280,
          flex: 1,
          type: 'number',
          editable: editing,
          renderCell: ({ row }) => `${row.code} - ${localizedName(row, isRtl)}`,
          renderEditCell: ({ value, onChange: changeValue, disabled }) => (
            <AppLookupField
              name="workerId"
              label={t('wfPerformers.fields.worker')}
              value={Number(value) || 0}
              onChange={(next, option) => {
                const selectedOption = Array.isArray(option) ? option[0] : option;
                selectedOptionRef.current = selectedOption
                  ? {
                      id: Number(selectedOption.id),
                      code: selectedOption.code,
                      name: selectedOption.name,
                      nameAlias: selectedOption.nameAlias,
                    }
                  : null;
                changeValue(Number(next) || 0);
              }}
              fetchPage={({ pageNumber, pageSize, search, signal }) =>
                loadWfPerformerWorkerLookup({
                  pageNumber,
                  pageSize,
                  search,
                  selectedId: Number(value) || undefined,
                  signal,
                })
              }
              queryKey={['workflow-performer-worker-lookup', Number(value) || 0]}
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
      ],
      [editing, isRtl, t]
    );

    return (
      <TabularDetailPanel<PerformerWorkerRow>
        gridRef={gridRef}
        rows={rows}
        columns={columns}
        addLabel={t('wfPerformers.actions.addWorker')}
        removeLabel={t('wfPerformers.actions.removeWorker')}
        selectedIds={selectedIds}
        onSelectionChange={setSelectedIds}
        onAdd={async () => {
          if (!editing) return;
          if (editingRowRef.current) {
            const saved = await gridRef.current?.saveEdit();
            if (!saved) return;
          }
          gridRef.current?.startAddRow();
        }}
        onRemove={() => {
          if (!editing) return;
          const selected = new Set(selectedIds.map(String));
          updateUsers(userIds.filter((workerId) => !selected.has(String(workerId))));
          setSelectedIds([]);
        }}
        disabled={!editing}
        showFilterRow={showFilterRow}
        storageKey="workflow.performers.users"
        height={260}
        masterForm
        onEditingChange={(isEditing) => {
          editingRowRef.current = isEditing;
          if (!isEditing) selectedOptionRef.current = null;
        }}
        onNewRow={() => ({
          id: `new-${crypto.randomUUID()}`,
          workerId: 0,
          code: '',
          name: '',
        })}
        onRowSave={(values, isNew) => {
          const workerId = Number(values.workerId) || 0;
          if (!workerId) throw new Error(t('wfPerformers.validation.workerRequired'));
          const previousId = Number(values.id);
          if (
            (isNew && userIds.includes(workerId)) ||
            (!isNew && workerId !== previousId && userIds.includes(workerId))
          ) {
            throw new Error(t('wfPerformers.validation.workerDuplicate'));
          }
          const nextIds = isNew
            ? [...userIds, workerId]
            : userIds.map((id) => (id === previousId ? workerId : id));
          updateUsers(nextIds, selectedOptionRef.current);
          setSelectedIds([String(workerId)]);
        }}
      />
    );
  }
);
