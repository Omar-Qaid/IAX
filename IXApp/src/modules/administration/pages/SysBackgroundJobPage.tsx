import React, { useMemo } from 'react';
import { useBatchJobCommands } from '../components/useBatchJobCommands';
import { BatchJobOverview } from '../components/BatchJobOverview';
import { BatchJobTasks } from '../components/BatchJobTasks';
import { AppDateTimeField } from '@shared/components/fields/AppDateTimeField';
import { Alert, Chip, Typography } from '@mui/material';
import { DataGrid } from '@shared/components/data-grid/DataGrid';
import { usePermission } from '@core/permissions/usePermission';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import {
  sysBackgroundJobApi,
  type SysBackgroundJobRecord,
  type SysJobExecutionStatus,
} from '../api/sysBackgroundJobApi';

const scheduleLabels = ['One time', 'Delayed', 'Recurring', 'Cron'];
const jobStatusLabels = ['Ready', 'Withhold', 'Cancelled', 'Completed'];
const executionStatusLabels = ['Waiting', 'Executing', 'Completed', 'Failed', 'Cancelled'];
const formatDateTime = (value: string | null, locale: string) =>
  value
    ? new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' }).format(
        new Date(value)
      )
    : '—';
const executionTone = (status: SysJobExecutionStatus | null) =>
  status === 2 ? 'success' : status === 3 ? 'error' : status === 1 ? 'warning' : 'default';

const emptyJob = (): SysBackgroundJobRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  name: '',
  jobKey: '',
  description: null,
  tenantId: null,
  scheduleType: 2,
  cronExpression: null,
  intervalSeconds: 300,
  runAt: null,
  nextRunAt: null,
  status: 0,
  isEnabled: false,
  preventOverlap: true,
  priority: 1,
  maxRetryCount: 0,
  retryDelaySeconds: 60,
  timeoutSeconds: 300,
  payloadJson: null,
  runCount: 0,
  lastRunAt: null,
  lastStatus: null,
  lastError: null,
  createdAt: null,
  createdBy: null,
});

function ExecutionHistory({ jobId }: { jobId: number }): React.ReactElement {
  const { i18n } = useAppTranslation();
  const executions = useQuery({
    queryKey: ['background-job-executions', jobId],
    queryFn: ({ signal }) => sysBackgroundJobApi.executions(jobId, signal),
    enabled: jobId > 0,
  });
  if (jobId <= 0)
    return (
      <Typography sx={{ fontSize: 12, color: 'text.secondary' }}>
        Save the batch job to view execution history.
      </Typography>
    );
  if (executions.isLoading)
    return (
      <Typography sx={{ fontSize: 12, color: 'text.secondary' }}>
        Loading execution history…
      </Typography>
    );
  if (executions.isError)
    return (
      <Typography color="error" sx={{ fontSize: 12 }}>
        Execution history could not be loaded.
      </Typography>
    );
  if (!executions.data?.length)
    return (
      <Typography sx={{ fontSize: 12, color: 'text.secondary' }}>
        No executions have been recorded.
      </Typography>
    );
  return (
    <DataGrid
      rows={executions.data}
      getRowId={(row) => row.recId}
      height={300}
      hideToolbar
      hideSidebar
      columns={[
        { field: 'recId', headerName: 'Execution ID', width: 110 },
        { field: 'attempt', headerName: 'Attempt', width: 90 },
        {
          field: 'status',
          headerName: 'Status',
          width: 130,
          renderCell: ({ row }) => (
            <Chip
              size="small"
              label={executionStatusLabels[row.status]}
              color={executionTone(row.status)}
            />
          ),
        },
        {
          field: 'startedAt',
          headerName: 'Started',
          width: 180,
          valueGetter: ({ row }) => formatDateTime(row.startedAt, i18n.language),
        },
        {
          field: 'completedAt',
          headerName: 'Completed',
          width: 180,
          valueGetter: ({ row }) => formatDateTime(row.completedAt, i18n.language),
        },
        { field: 'durationMs', headerName: 'Duration (ms)', width: 120 },
        {
          field: 'output',
          headerName: 'Output / error',
          width: 350,
          valueGetter: ({ row }) => row.errorMessage || row.output || '—',
        },
      ]}
    />
  );
}

export function SysBackgroundJobPage(): React.ReactElement {
  const { t, i18n } = useAppTranslation();
  const queryClient = useQueryClient();
  const batchCommands = useBatchJobCommands((id) => <ExecutionHistory jobId={id} />);
  const [commandError, setCommandError] = React.useState<string | null>(null);
  const [busy, setBusy] = React.useState(false);
  const [taskLocked, setTaskLocked] = React.useState(false);
  const canRun = usePermission('System.BackgroundJobs.Run').hasPermission;
  const canEdit = usePermission('System.BackgroundJobs.Edit').hasPermission;
  const canCancel = usePermission('System.BackgroundJobs.Cancel').hasPermission;
  const execute = async (action: () => Promise<void>) => {
    setBusy(true);
    setCommandError(null);
    try {
      await action();
      await refresh();
      await queryClient.invalidateQueries({ queryKey: ['background-job-executions'] });
    } catch (reason) {
      setCommandError(reason instanceof Error ? reason.message : String(reason));
    } finally {
      setBusy(false);
    }
  };
  const handlers = useQuery({
    queryKey: ['background-job-handlers'],
    queryFn: ({ signal }) => sysBackgroundJobApi.handlers(signal),
  });
  const refresh = () =>
    queryClient.invalidateQueries({ queryKey: ['list-details', 'background-jobs'] });
  const sections = useMemo<DetailSectionConfig[]>(
    () => [
      {
        id: 'identification',
        title: 'Batch job',
        groups: [
          {
            id: 'identity',
            title: 'Identification',
            columns: 3,
            fields: [
              { name: 'description', label: 'Description', type: 'text', multiline: true },
              {
                name: 'jobKey',
                label: 'Handler',
                type: 'select',
                options: (handlers.data ?? []).map((value) => ({ value, label: value })),
              },
              { name: 'statusText', label: 'Status', type: 'display', disabled: true },
              {
                name: 'scheduleType',
                label: 'Schedule',
                type: 'select',
                options: scheduleLabels.map((label, value) => ({ value: String(value), label })),
              },
              {
                name: 'nextRunAt',
                label: 'Scheduled start date/time',
                type: 'display',
                disabled: true,
              },
              {
                name: 'lastRunAt',
                label: 'Actual start date/time',
                type: 'display',
                disabled: true,
              },
            ],
          },
        ],
      },
      {
        id: 'schedule',
        title: 'Recurrence and reliability',
        defaultExpanded: false,
        groups: [
          {
            id: 'schedule',
            columns: 3,
            fields: [
              { name: 'intervalSeconds', label: 'Interval (seconds)', type: 'number' },
              { name: 'cronExpression', label: 'CRON expression', type: 'text' },
              {
                name: 'runAt',
                label: 'Run at (UTC)',
                renderOwnLabel: true,
                render: ({ value, editing, disabled, onChange }) => {
                  const text = String(value ?? '');
                  const date = text ? new Date(text) : null;
                  const input =
                    date && !Number.isNaN(date.getTime()) ? date.toISOString().slice(0, 16) : '';
                  return (
                    <AppDateTimeField
                      label="Run at (UTC)"
                      value={input}
                      disabled={disabled || !editing}
                      onChange={(next) =>
                        onChange(next ? `${next}${next.length === 16 ? ':00' : ''}Z` : '')
                      }
                    />
                  );
                },
              },
              { name: 'isEnabled', label: 'Enabled', type: 'boolean' },
              {
                name: 'priority',
                label: 'Priority',
                type: 'select',
                options: ['Low', 'Normal', 'High'].map((label, value) => ({
                  value: String(value),
                  label,
                })),
              },
              { name: 'preventOverlap', label: 'Prevent overlap', type: 'boolean' },
              { name: 'maxRetryCount', label: 'Maximum retries', type: 'number' },
              { name: 'retryDelaySeconds', label: 'Retry delay (seconds)', type: 'number' },
              { name: 'timeoutSeconds', label: 'Timeout (seconds)', type: 'number' },
            ],
          },
        ],
      },
      {
        id: 'payload',
        title: 'Execution payload',
        defaultExpanded: false,
        groups: [
          {
            id: 'payload',
            fields: [
              {
                name: 'payloadJson',
                label: 'Parameters JSON',
                type: 'text',
                multiline: true,
                rows: 4,
              },
            ],
          },
        ],
      },
    ],
    [handlers.data]
  );
  const config: EnterpriseListDetailsConfig<SysBackgroundJobRecord> = {
    interactionLocked: busy || taskLocked || batchCommands.busy,
    detailHeader: commandError ? (
      <Alert severity="error" onClose={() => setCommandError(null)}>
        {commandError}
      </Alert>
    ) : undefined,
    recordTableName: 'SysBackgroundJob',
    dataSource: {
      type: 'remote',
      key: 'background-jobs',
      load: sysBackgroundJobApi.list,
      create: sysBackgroundJobApi.create,
      update: sysBackgroundJobApi.update,
      delete: sysBackgroundJobApi.remove,
    },
    createRecord: emptyJob,
    getPrimaryText: (job) => job.name,
    getSecondaryText: (job) => `${job.jobKey} · ${jobStatusLabels[job.status]}`,
    matchesSearch: (job, query) =>
      `${job.name} ${job.jobKey} ${job.description ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (job) => ({
      description: job.description ?? '',
      jobKey: job.jobKey,
      statusText: jobStatusLabels[job.status],
      scheduleType: String(job.scheduleType),
      nextRunAt: formatDateTime(job.nextRunAt, i18n.language),
      lastRunAt: formatDateTime(job.lastRunAt, i18n.language),
      intervalSeconds: job.intervalSeconds ?? '',
      cronExpression: job.cronExpression ?? '',
      runAt: job.runAt ?? '',
      isEnabled: job.isEnabled,
      preventOverlap: job.preventOverlap,
      priority: String(job.priority ?? 1),
      maxRetryCount: job.maxRetryCount,
      retryDelaySeconds: job.retryDelaySeconds,
      timeoutSeconds: job.timeoutSeconds,
      payloadJson: job.payloadJson ?? '',
    }),
    setValues: (job, values) => ({
      ...job,
      description: String(values.description || '') || null,
      jobKey: String(values.jobKey || ''),
      scheduleType: Number(values.scheduleType) as SysBackgroundJobRecord['scheduleType'],
      intervalSeconds: values.intervalSeconds === '' ? null : Number(values.intervalSeconds),
      cronExpression: String(values.cronExpression || '') || null,
      runAt: String(values.runAt || '') || null,
      isEnabled: Boolean(values.isEnabled),
      preventOverlap: Boolean(values.preventOverlap),
      priority: Number(values.priority ?? 1),
      maxRetryCount: Number(values.maxRetryCount || 0),
      retryDelaySeconds: Number(values.retryDelaySeconds || 0),
      timeoutSeconds: Number(values.timeoutSeconds || 0),
      payloadJson: String(values.payloadJson || '') || null,
    }),
    headerFields: [
      {
        id: 'name',
        label: 'Batch job',
        width: 330,
        getValue: (job) => job.name,
        setValue: (job, value) => ({ ...job, name: String(value) }),
      },
    ],
    sections: ({ record, editing }) => [
      ...sections.map((section) => ({
        ...section,
        ...(section.id === 'identification' && !editing
          ? { groups: undefined, content: <BatchJobOverview job={record} /> }
          : {}),
        groups:
          section.id === 'identification' && !editing
            ? undefined
            : section.groups?.map((group) => ({
                ...group,
                fields: group.fields.map((field) =>
                  field.name === 'jobKey' ? { ...field, disabled: record.recId > 0 } : field
                ),
              })),
      })),
      {
        id: 'tasks',
        title: 'Batch tasks',
        content: (
          <BatchJobTasks
            key={record.recId}
            job={record}
            editing={editing}
            onLockChange={setTaskLocked}
          />
        ),
      },
      {
        id: 'history',
        title: 'Batch job history',
        content: <ExecutionHistory jobId={record.recId} />,
        defaultExpanded: false,
      },
    ],
    commands: (job) => [
      ...batchCommands.commands,
      {
        id: 'run',
        label: job?.lastStatus === 3 ? 'Retry (new run)' : 'Execute now',
        disabled: busy || !canRun || !job?.isEnabled || job.status !== 0,
        requiresSelection: true,
        onClick: (record) => {
          if (
            record &&
            (record.lastStatus !== 3 ||
              window.confirm(
                'Retry starts a new run from the beginning. Completed tasks may run again. Continue?'
              ))
          )
            void execute(() => sysBackgroundJobApi.trigger(record.recId));
        },
      },
      {
        id: 'pause-resume',
        label: job?.status === 1 ? 'Ready' : 'Withhold',
        disabled: busy || !canEdit || (job?.status !== 0 && job?.status !== 1),
        requiresSelection: true,
        onClick: (record) => {
          if (record)
            void execute(() =>
              record.status === 1
                ? sysBackgroundJobApi.resume(record.recId)
                : sysBackgroundJobApi.pause(record.recId)
            );
        },
      },
      {
        id: 'cancel',
        label: 'Cancel job',
        disabled: busy || !canCancel || job?.status === 2,
        requiresSelection: true,
        onClick: (record) => {
          if (record) void execute(() => sysBackgroundJobApi.cancel(record.recId));
        },
      },
    ],
    permissions: {
      view: 'System.BackgroundJobs.View',
      create: 'System.BackgroundJobs.Create',
      edit: 'System.BackgroundJobs.Edit',
      delete: 'System.BackgroundJobs.Delete',
    },
    validate: (job) => ({
      ...(!job.name.trim() ? { name: 'Batch job name is required.' } : {}),
      ...(!job.jobKey.trim() ? { jobKey: 'A registered handler is required.' } : {}),
    }),
    presentation: { mode: 'list', listWidth: 300, headerMaxWidth: 760 },
  };
  return (
    <>
      <ListDetailsPage
        variant="enterprise"
        title={t('pages.backgroundJobs.title', { defaultValue: 'Batch jobs' })}
        config={config}
      />
      {batchCommands.dialog}
    </>
  );
}
