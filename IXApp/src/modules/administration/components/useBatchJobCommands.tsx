import { BatchJobRecurrenceForm } from './BatchJobRecurrenceForm';
import { AppActionDrawer } from '@shared/components/dialogs/AppActionDrawer';
import { useState } from 'react';
import {
  Alert,
  Button,
  Stack,
  TextField,
  List,
  ListItemButton,
  ListItemText,
  Typography,
} from '@mui/material';
import { useQueryClient } from '@tanstack/react-query';
import { usePermission } from '@core/permissions/usePermission';
import type { ListDetailsCommand } from '@patterns/list-details/types';
import {
  sysBackgroundJobApi as api,
  type SysBackgroundJobRecord as Job,
} from '../api/sysBackgroundJobApi';

type Action =
  'Batch job history' | 'Recurrence' | 'Change status' | 'Remove recurrence' | 'Copy batch job';
export function useBatchJobCommands(renderHistory: (id: number) => React.ReactNode) {
  const client = useQueryClient();
  const edit = usePermission('System.BackgroundJobs.Edit').hasPermission;
  const create = usePermission('System.BackgroundJobs.Create').hasPermission;
  const cancel = usePermission('System.BackgroundJobs.Cancel').hasPermission;
  const [action, setAction] = useState<Action | null>(null);
  const [job, setJob] = useState<Job | null>(null);
  const [status, setStatus] = useState('withhold');
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState('');
  const close = () => {
    if (!busy) {
      setAction(null);
      setError('');
    }
  };
  const commands: ListDetailsCommand<Job>[] = (
    [
      'Batch job history',
      'Recurrence',
      'Change status',
      'Remove recurrence',
      'Copy batch job',
    ] as Action[]
  ).map((label) => ({
    id: label,
    label,
    requiresSelection: true,
    disabled:
      busy ||
      (label === 'Copy batch job'
        ? !create
        : label === 'Batch job history'
          ? false
          : label === 'Change status'
            ? !edit && !cancel
            : !edit),
    onClick: (record) => {
      if (!record) return;
      setError('');
      setStatus(record.status === 1 ? 'ready' : 'withhold');
      setJob({
        ...record,
        ...(label === 'Copy batch job' ? { name: `${record.name} (copy)` } : {}),
      });
      setAction(label);
    },
  }));
  const save = async () => {
    if (!job) return;
    setBusy(true);
    setError('');
    try {
      if (action === 'Recurrence') {
        if (
          job.scheduleType === 2 &&
          (!Number.isInteger(job.intervalSeconds) || (job.intervalSeconds ?? 0) <= 0)
        )
          throw new Error('Enter a positive whole-number interval.');
        if (job.scheduleType === 3 && !job.cronExpression?.trim())
          throw new Error('Enter a CRON expression.');
        if (job.scheduleType < 2 && !job.runAt) throw new Error('Select a start date/time.');
      }
      if (action === 'Change status') {
        if (status === 'ready') await api.resume(job.recId);
        else if (status === 'withhold') await api.pause(job.recId);
        else await api.cancel(job.recId);
      } else if (action === 'Remove recurrence') {
        await api.update({
          ...job,
          scheduleType: 0,
          runAt: job.nextRunAt ?? new Date().toISOString(),
          intervalSeconds: null,
          cronExpression: null,
          isEnabled: false,
        });
      } else if (action === 'Copy batch job') {
        if (!job.name.trim()) throw new Error('Enter a name for the copy.');
        const tasks = job.jobKey === 'BatchTasks' ? await api.tasks(job.recId) : [];
        const copy = await api.create({ ...job, isEnabled: false });
        try {
          if (tasks.length) {
            const saved = await api.saveTasks(
              copy.recId,
              tasks.map((task) => ({ ...task, recId: 0, dependsOnTaskId: null }))
            );
            const ids = new Map(
              tasks.map((task) => [
                task.recId,
                saved.find((row) => row.executionOrder === task.executionOrder)!.recId,
              ])
            );
            await api.saveTasks(
              copy.recId,
              saved.map((task) => ({
                ...task,
                dependsOnTaskId: (() => {
                  const source = tasks.find((row) => row.executionOrder === task.executionOrder)!;
                  return source.dependsOnTaskId == null ? null : ids.get(source.dependsOnTaskId)!;
                })(),
              }))
            );
          }
        } catch (reason) {
          throw new Error(
            `Copy ${copy.recId} was created disabled, but task copying failed. Review its tasks before enabling it. ${reason instanceof Error ? reason.message : reason}`
          );
        }
      } else await api.update(job);
      await client.invalidateQueries({ queryKey: ['list-details', 'background-jobs'] });
      await client.invalidateQueries({ queryKey: ['background-job-executions'] });
      setAction(null);
    } catch (reason) {
      setError(reason instanceof Error ? reason.message : String(reason));
      await client.invalidateQueries({ queryKey: ['list-details', 'background-jobs'] });
    } finally {
      setBusy(false);
    }
  };
  return {
    commands,
    busy: busy || action !== null,
    dialog: (
      <AppActionDrawer
        open={action !== null}
        onClose={close}
        title={
          action === 'Change status'
            ? 'Select new status'
            : action === 'Recurrence'
              ? 'Define recurrence'
              : (action ?? '')
        }
        width={action === 'Batch job history' ? 1200 : action === 'Recurrence' ? 540 : 480}
        busy={busy}
        actions={
          <>
            <Button disabled={busy} onClick={close}>
              {action === 'Batch job history' ? 'Close' : 'Cancel'}
            </Button>
            {action !== 'Batch job history' && (
              <Button
                disabled={
                  busy || (action === 'Change status' && (status === 'cancelled' ? !cancel : !edit))
                }
                variant="contained"
                onClick={() => void save()}
              >
                {busy ? 'Saving…' : 'OK'}
              </Button>
            )}
          </>
        }
      >
        <Stack spacing={2} sx={{ pt: 1 }}>
          {error && <Alert severity="error">{error}</Alert>}
          {action === 'Batch job history' && job && renderHistory(job.recId)}
          {action === 'Change status' && (
            <List
              aria-label="Select new status"
              role="listbox"
              sx={{ border: 1, borderColor: 'divider', p: 0 }}
            >
              <Typography sx={{ p: 1, borderBottom: 1, borderColor: 'divider', fontSize: 12 }}>
                Select new status
              </Typography>
              {[
                { value: 'withhold', label: 'Withhold', allowed: edit },
                { value: 'cancelled', label: 'Canceling', allowed: cancel },
                { value: 'ready', label: 'Waiting', allowed: edit },
              ].map((option) => (
                <ListItemButton
                  key={option.value}
                  role="option"
                  aria-selected={status === option.value}
                  selected={status === option.value}
                  disabled={busy || !option.allowed}
                  onClick={() => setStatus(option.value)}
                >
                  <ListItemText primary={option.label} />
                </ListItemButton>
              ))}
            </List>
          )}
          {action === 'Remove recurrence' && (
            <Alert severity="info">
              Remove the repeating schedule and disable future automatic runs. Existing execution
              history is retained. A running execution is not aborted.
            </Alert>
          )}
          {action === 'Copy batch job' && (
            <>
              <TextField
                label="Job description"
                value={job?.name ?? ''}
                disabled={busy}
                onChange={(e) => setJob(job && { ...job, name: e.target.value })}
              />
              <Alert severity="info">
                Copies configuration, tasks, parameters, and dependencies. The new job starts
                disabled with no execution history.
              </Alert>
              <TextField
                label="Company accounts"
                value={job?.tenantId ?? 'Current company'}
                slotProps={{ input: { readOnly: true } }}
                helperText="The copy uses the current authorized company."
              />
              <TextField
                label="Run by"
                value="Current authenticated user"
                slotProps={{ input: { readOnly: true } }}
              />
              {job && job.scheduleType < 2 && (
                <TextField
                  label="Scheduled start date/time (UTC)"
                  type="datetime-local"
                  slotProps={{ inputLabel: { shrink: true } }}
                  value={job.runAt ? new Date(job.runAt).toISOString().slice(0, 16) : ''}
                  disabled={busy}
                  onChange={(e) =>
                    setJob({ ...job, runAt: e.target.value ? `${e.target.value}:00Z` : null })
                  }
                />
              )}
              <TextField
                label="Recurrence"
                multiline
                minRows={4}
                value={
                  job?.scheduleType === 3
                    ? (job.cronExpression ?? '')
                    : job?.scheduleType === 2
                      ? `Repeat every ${job.intervalSeconds} seconds`
                      : 'One time'
                }
                slotProps={{ input: { readOnly: true } }}
              />
            </>
          )}
          {action === 'Recurrence' && job && (
            <BatchJobRecurrenceForm job={job} disabled={busy} onChange={setJob} />
          )}
        </Stack>
      </AppActionDrawer>
    ),
  };
}
