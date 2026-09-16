import { useEffect, useRef, useState } from 'react';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Stack,
} from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
import type { DataGridHandle } from '@shared/components/data-grid/types';
import { usePermission } from '@core/permissions/usePermission';
import {
  sysBackgroundJobApi,
  type SysBackgroundJobRecord,
  type SysBackgroundJobTaskRecord,
} from '../api/sysBackgroundJobApi';

export function BatchJobTasks({
  job,
  editing,
  onLockChange,
  showFilterRow,
}: {
  job: SysBackgroundJobRecord;
  editing: boolean;
  onLockChange: (locked: boolean) => void;
  showFilterRow: boolean;
}) {
  const client = useQueryClient();
  const [selected, setSelected] = useState<(string | number)[]>([]);
  const [taskEditing, setTaskEditing] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [showLogs, setShowLogs] = useState(false);
  const gridRef = useRef<DataGridHandle>(null);
  const history = useQuery({
    queryKey: ['batch-task-history', job.recId],
    queryFn: ({ signal }) => sysBackgroundJobApi.taskHistory(job.recId, signal),
    enabled: job.recId > 0,
    refetchInterval: showLogs ? 10000 : false,
  });
  const permission = usePermission('System.BackgroundJobs.Edit');
  const tasks = useQuery({
    queryKey: ['batch-tasks', job.recId],
    queryFn: ({ signal }) => sysBackgroundJobApi.tasks(job.recId, signal),
    enabled: job.recId > 0 && job.jobKey === 'BatchTasks',
  });
  const services = useQuery({
    queryKey: ['batch-services'],
    queryFn: ({ signal }) => sysBackgroundJobApi.services(signal),
  });
  const handlers = useQuery({
    queryKey: ['background-job-handlers'],
    queryFn: ({ signal }) => sysBackgroundJobApi.handlers(signal),
  });
  const save = useMutation({
    mutationFn: (rows: SysBackgroundJobTaskRecord[]) =>
      sysBackgroundJobApi.saveTasks(job.recId, rows),
    onSuccess: (rows) => {
      client.setQueryData(['batch-tasks', job.recId], rows);
      setError(null);
    },
    onError: (reason: Error) => setError(reason.message),
  });
  const rows = tasks.data ?? [];
  useUnsavedChanges(taskEditing);
  useEffect(() => {
    onLockChange(taskEditing || save.isPending);
    return () => onLockChange(false);
  }, [taskEditing, save.isPending, onLockChange]);
  const current = rows.find((row) => String(row.recId) === String(selected[0]));
  const disabled =
    editing ||
    taskEditing ||
    job.isEnabled ||
    !permission.hasPermission ||
    save.isPending ||
    job.recId <= 0 ||
    tasks.isError ||
    tasks.isLoading;
  if (job.jobKey !== 'BatchTasks')
    return (
      <Alert severity="info">
        This job executes its selected handler directly. Select BatchTasks when creating a job to
        configure multiple tasks.
      </Alert>
    );
  return (
    <Stack spacing={1}>
      <Button onClick={() => setShowLogs(true)} disabled={job.recId <= 0}>
        Task execution logs
      </Button>
      <Dialog open={showLogs} onClose={() => setShowLogs(false)} fullWidth maxWidth="lg">
        <DialogTitle>Task execution history (latest 100)</DialogTitle>
        <DialogContent>
          {history.isError && <Alert severity="error">{history.error.message}</Alert>}
          <DataGrid
            rows={history.data ?? []}
            getRowId={(row) => row.recId}
            height={350}
            hideToolbar
            hideSidebar
            columns={[
              { field: 'taskName', headerName: 'Task', width: 220 },
              { field: 'attempt', headerName: 'Attempt', width: 90 },
              {
                field: 'status',
                headerName: 'Status',
                width: 120,
                valueGetter: ({ row }) =>
                  ['Pending', 'Running', 'Completed', 'Failed', 'Cancelled'][row.status],
              },
              { field: 'startedAt', headerName: 'Started (UTC)', width: 210 },
              {
                field: 'output',
                headerName: 'Output / error',
                width: 400,
                valueGetter: ({ row }) => row.errorMessage || row.output || '',
              },
            ]}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setShowLogs(false)}>Close</Button>
        </DialogActions>
      </Dialog>
      {job.isEnabled && (
        <Alert severity="info">Disable the job and save its header before editing tasks.</Alert>
      )}
      {(error || tasks.isError) && <Alert severity="error">{error || tasks.error?.message}</Alert>}
      <TabularDetailPanel
        showFilterRow={showFilterRow}
        rows={rows.map((row) => ({ ...row, id: String(row.recId) }))}
        columns={[
          { field: 'recId', headerName: 'Task ID', width: 100 },
          {
            field: 'taskStatus',
            headerName: 'Status',
            width: 120,
            valueGetter: ({ row }) => {
              const last = history.data
                ?.filter((item) => item.taskId === row.recId)
                .sort((a, b) => b.recId - a.recId)[0];
              return last
                ? ['Waiting', 'Executing', 'Completed', 'Failed', 'Cancelled'][last.status]
                : row.isEnabled
                  ? 'Waiting'
                  : 'Withhold';
            },
          },
          { field: 'name', headerName: 'Task description', width: 240, editable: true },
          {
            field: 'serviceKey',
            headerName: 'Service key',
            width: 220,
            type: 'singleSelect',
            editable: true,
            valueOptions: (handlers.data ?? [])
              .filter((key) => key !== 'BatchTasks')
              .map((key) => ({
                value: key,
                label: services.data?.find((service) => service.serviceKey === key)?.name ?? key,
              })),
          },
          {
            field: 'classDescription',
            headerName: 'Class description',
            width: 180,
            valueGetter: ({ row }) =>
              services.data?.find((service) => service.serviceKey === row.serviceKey)?.name ??
              row.serviceKey,
          },
          {
            field: 'company',
            headerName: 'Company accounts',
            width: 120,
            valueGetter: () => job.tenantId ?? '—',
          },
          {
            field: 'conditions',
            headerName: 'Has conditions',
            width: 120,
            valueGetter: ({ row }) => (row.dependsOnTaskId ? 'Yes' : 'No'),
          },
          {
            field: 'startedAt',
            headerName: 'Start date/time',
            width: 180,
            valueGetter: ({ row }) =>
              history.data
                ?.filter((item) => item.taskId === row.recId)
                .sort((a, b) => b.recId - a.recId)[0]?.startedAt ?? '—',
          },
          {
            field: 'completedAt',
            headerName: 'End date/time',
            width: 180,
            valueGetter: ({ row }) =>
              history.data
                ?.filter((item) => item.taskId === row.recId)
                .sort((a, b) => b.recId - a.recId)[0]?.completedAt ?? '—',
          },
          {
            field: 'executionOrder',
            headerName: 'Order',
            type: 'number',
            width: 90,
            editable: true,
          },
          {
            field: 'dependsOnTaskId',
            headerName: 'Depends on task',
            type: 'singleSelect',
            width: 150,
            editable: true,
            valueOptions: [
              { value: '', label: 'None' },
              ...rows.map((row) => ({ value: row.recId, label: row.name })),
            ],
          },
          {
            field: 'maxRetryCount',
            headerName: 'Task retries',
            type: 'number',
            width: 120,
            editable: true,
          },
          {
            field: 'retryDelaySeconds',
            headerName: 'Retry delay (seconds)',
            type: 'number',
            width: 170,
            editable: true,
          },
          {
            field: 'payloadJson',
            headerName: 'Parameters JSON',
            width: 240,
            editable: true,
          },
          {
            field: 'isEnabled',
            headerName: 'Enabled',
            type: 'boolean',
            width: 90,
            editable: true,
          },
        ]}
        addLabel="New"
        removeLabel="Delete"
        selectedIds={selected}
        onSelectionChange={setSelected}
        height={334}
        disabled={disabled}
        gridRef={gridRef}
        masterForm
        onEditingChange={setTaskEditing}
        onNewRow={() => ({
          recId: 0,
          id: '__new__',
          name: '',
          serviceKey: '',
          payloadJson: null,
          executionOrder: Math.max(0, ...rows.map((row) => row.executionOrder)) + 1,
          dependsOnTaskId: null,
          isEnabled: true,
          maxRetryCount: 0,
          retryDelaySeconds: 60,
        })}
        onRowSave={async (values, isNew) => {
          const task = values as Partial<SysBackgroundJobTaskRecord>;
          const executionOrder = Number(task.executionOrder ?? 0);
          const maxRetryCount = Number(task.maxRetryCount ?? 0);
          const retryDelaySeconds = Number(task.retryDelaySeconds ?? 60);
          const dependency = task.dependsOnTaskId ? Number(task.dependsOnTaskId) : null;
          const name = String(task.name ?? '').trim();
          const serviceKey = String(task.serviceKey ?? '');
          const payloadJson = task.payloadJson ? String(task.payloadJson) : null;
          if (!name) throw new Error('Task description is required.');
          if (!serviceKey) throw new Error('Batch service is required.');
          if (executionOrder < 1) throw new Error('Execution order must be at least 1.');
          if (maxRetryCount < 0 || maxRetryCount > 10)
            throw new Error('Task retries must be between 0 and 10.');
          if (retryDelaySeconds < 1 || retryDelaySeconds > 3600)
            throw new Error('Retry delay must be between 1 and 3600 seconds.');
          if (payloadJson) {
            try {
              JSON.parse(payloadJson);
            } catch {
              throw new Error('Parameters must be valid JSON.');
            }
          }
          const recId = isNew ? 0 : Number(task.recId);
          const dependencyTask = dependency
            ? rows.find((row) => row.recId === dependency)
            : undefined;
          if (dependency && (!dependencyTask || dependency === recId))
            throw new Error('Select a valid dependency task.');
          if (dependencyTask && dependencyTask.executionOrder >= executionOrder)
            throw new Error('The dependency must have a lower execution order.');
          const normalized: SysBackgroundJobTaskRecord = {
            recId,
            name,
            serviceKey,
            payloadJson,
            executionOrder,
            dependsOnTaskId: dependency,
            isEnabled: Boolean(task.isEnabled),
            maxRetryCount,
            retryDelaySeconds,
          };
          await save.mutateAsync(
            isNew
              ? [...rows, normalized]
              : rows.map((row) => (row.recId === recId ? normalized : row))
          );
        }}
        onAdd={() => gridRef.current?.startAddRow()}
        onRemove={() => {
          if (current) save.mutate(rows.filter((row) => row.recId !== current.recId));
        }}
        actions={[
          {
            id: 'parameters',
            label: 'Parameters / task details',
            disabled: !current,
            onClick: () => {
              if (current) gridRef.current?.startEditRow(current.recId);
            },
          },
        ]}
      />
    </Stack>
  );
}
