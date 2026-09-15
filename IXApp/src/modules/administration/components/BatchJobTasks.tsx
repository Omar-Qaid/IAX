import { useEffect, useState } from 'react';
import { useUnsavedChanges } from '@shared/hooks/useUnsavedChanges';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import {
  Alert,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  MenuItem,
  Stack,
  Switch,
  FormControlLabel,
  TextField,
} from '@mui/material';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { TabularDetailPanel } from '@patterns/list-details/TabularDetailPanel';
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
}: {
  job: SysBackgroundJobRecord;
  editing: boolean;
  onLockChange: (locked: boolean) => void;
}) {
  const client = useQueryClient();
  const [selected, setSelected] = useState<(string | number)[]>([]);
  const [draft, setDraft] = useState<SysBackgroundJobTaskRecord | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [showLogs, setShowLogs] = useState(false);
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
      setDraft(null);
      setError(null);
    },
    onError: (reason: Error) => setError(reason.message),
  });
  const rows = tasks.data ?? [];
  useUnsavedChanges(draft !== null);
  useEffect(() => {
    onLockChange(draft !== null || save.isPending);
    return () => onLockChange(false);
  }, [draft, save.isPending, onLockChange]);
  const current = rows.find((row) => String(row.recId) === String(selected[0]));
  const disabled =
    editing ||
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
          { field: 'name', headerName: 'Task description', width: 240 },
          { field: 'serviceKey', headerName: 'Class name', width: 220 },
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
          { field: 'executionOrder', headerName: 'Order', width: 90 },
          { field: 'dependsOnTaskId', headerName: 'Depends on task', width: 130 },
          { field: 'isEnabled', headerName: 'Enabled', type: 'boolean', width: 90 },
        ]}
        addLabel="New"
        removeLabel="Delete"
        selectedIds={selected}
        onSelectionChange={setSelected}
        height={334}
        disabled={disabled}
        onAdd={() =>
          setDraft({
            recId: 0,
            name: '',
            serviceKey: '',
            payloadJson: null,
            executionOrder: Math.max(0, ...rows.map((row) => row.executionOrder)) + 1,
            dependsOnTaskId: null,
            isEnabled: true,
          })
        }
        onRemove={() => {
          if (current) save.mutate(rows.filter((row) => row.recId !== current.recId));
        }}
        actions={[
          {
            id: 'parameters',
            label: 'Parameters / task details',
            disabled: !current,
            onClick: () => {
              if (current) setDraft({ ...current });
            },
          },
        ]}
      />
      <Dialog
        open={draft !== null}
        onClose={() => {
          if (!save.isPending) setDraft(null);
        }}
        fullWidth
        maxWidth="sm"
      >
        <DialogTitle>Batch task details</DialogTitle>
        <DialogContent>
          <Stack spacing={2} sx={{ pt: 1 }}>
            {error && <Alert severity="error">{error}</Alert>}
            <TextField
              label="Task description"
              value={draft?.name ?? ''}
              onChange={(e) => draft && setDraft({ ...draft, name: e.target.value })}
            />
            <TextField
              select
              label="Batch service"
              value={draft?.serviceKey ?? ''}
              onChange={(e) => draft && setDraft({ ...draft, serviceKey: e.target.value })}
            >
              {(handlers.data ?? [])
                .filter((key) => key !== 'BatchTasks')
                .map((key) => (
                  <MenuItem key={key} value={key}>
                    {services.data?.find((service) => service.serviceKey === key)?.name ?? key} (
                    {key})
                  </MenuItem>
                ))}
            </TextField>
            <TextField
              label="Execution order"
              type="number"
              value={draft?.executionOrder ?? 1}
              onChange={(e) =>
                draft && setDraft({ ...draft, executionOrder: Number(e.target.value) })
              }
            />
            <TextField
              label="Task retries (idempotent services only)"
              type="number"
              value={draft?.maxRetryCount ?? 0}
              onChange={(e) =>
                draft && setDraft({ ...draft, maxRetryCount: Number(e.target.value) })
              }
            />
            <TextField
              label="Retry delay (seconds)"
              type="number"
              value={draft?.retryDelaySeconds ?? 60}
              onChange={(e) =>
                draft && setDraft({ ...draft, retryDelaySeconds: Number(e.target.value) })
              }
            />
            <TextField
              select
              label="Depends on task"
              value={draft?.dependsOnTaskId ?? ''}
              onChange={(e) =>
                draft &&
                setDraft({
                  ...draft,
                  dependsOnTaskId: e.target.value ? Number(e.target.value) : null,
                })
              }
            >
              <MenuItem value="">None</MenuItem>
              {rows
                .filter(
                  (row) =>
                    row.recId !== draft?.recId && row.executionOrder < (draft?.executionOrder ?? 0)
                )
                .map((row) => (
                  <MenuItem key={row.recId} value={row.recId}>
                    {row.name}
                  </MenuItem>
                ))}
            </TextField>
            <TextField
              label="Parameters JSON"
              multiline
              minRows={4}
              value={draft?.payloadJson ?? ''}
              onChange={(e) => draft && setDraft({ ...draft, payloadJson: e.target.value || null })}
            />
            <FormControlLabel
              label="Enabled"
              control={
                <Switch
                  checked={draft?.isEnabled ?? true}
                  onChange={(_, checked) => draft && setDraft({ ...draft, isEnabled: checked })}
                />
              }
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button disabled={save.isPending} onClick={() => setDraft(null)}>
            Cancel
          </Button>
          <Button
            disabled={disabled || !draft?.name.trim() || !draft.serviceKey}
            onClick={() => {
              if (!draft) return;
              try {
                if (draft.payloadJson) JSON.parse(draft.payloadJson);
              } catch {
                setError('Parameters must be valid JSON.');
                return;
              }
              save.mutate(
                draft.recId
                  ? rows.map((row) => (row.recId === draft.recId ? draft : row))
                  : [...rows, draft]
              );
            }}
          >
            Save
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
