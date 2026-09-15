import React, { useMemo } from 'react';
import { useBatchJobCommands } from '../components/useBatchJobCommands';
import { BatchJobOverview } from '../components/BatchJobOverview';
import { BatchJobTasks } from '../components/BatchJobTasks';
import { AppDateTimeField } from '@shared/components/fields/AppDateTimeField';
import { Alert } from '@mui/material';
import { usePermission } from '@core/permissions/usePermission';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { sysBackgroundJobApi, type SysBackgroundJobRecord } from '../api/sysBackgroundJobApi';

const scheduleLabels = ['One time', 'Delayed', 'Recurring', 'Cron'];
const jobStatusLabels = ['Ready', 'Withhold', 'Cancelled', 'Completed'];
const executionStatusLabels = ['Waiting', 'Executing', 'Completed', 'Failed', 'Cancelled'];
const formatDateTime = (value: string | null, locale: string) =>
  value
    ? new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' }).format(
        new Date(value)
      )
    : '—';

const emptyJob = (): SysBackgroundJobRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  caption: '',
  jobKey: '',
  description: null,
  tenantId: null,
  scheduleType: 2,
  recurrenceData: null,
  startDateTime: null,
  startDateTimeTzId: null,
  startDate: null,
  startTime: null,
  origStartDateTime: null,
  origStartDateTimeTzId: null,
  endDateTime: null,
  endDateTimeTzId: null,
  canceledBy: null,
  dataPartition: null,
  finishing: 0,
  logLevel: 0,
  runtimeJob: 0,
  status: 0,
  isEnabled: false,
  preventOverlap: true,
  schedulingPriority: 1,
  schedulingPriorityIsOverridden: 0,
  critical: 0,
  monitoringCategory: 0,
  managed: 0,
  executingBy: null,
  activePeriod: null,
  batchGroup: null,
  emitBusinessEvent: 0,
  maxRetryCount: 0,
  retryDelaySeconds: 60,
  timeoutSeconds: 300,
  payloadJson: null,
  runCount: 0,
  lastStatus: null,
  lastError: null,
  createdAt: null,
  createdBy: null,
});

export function SysBackgroundJobPage(): React.ReactElement {
  const { t, i18n } = useAppTranslation();
  const queryClient = useQueryClient();
  const batchCommands = useBatchJobCommands();
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
                name: 'scheduledStartText',
                label: 'Scheduled start date/time',
                type: 'display',
                disabled: true,
              },
              {
                name: 'endDateTime',
                label: 'Actual end date/time',
                type: 'display',
                disabled: true,
              },
              { name: 'batchGroup', label: 'Batch group', type: 'text' },
              { name: 'activePeriod', label: 'Active period', type: 'text' },
              { name: 'critical', label: 'Critical job', type: 'boolean' },
              { name: 'monitoringCategory', label: 'Monitoring category', type: 'number' },
              { name: 'managed', label: 'Managed', type: 'boolean' },
              { name: 'emitBusinessEvent', label: 'Emit business event', type: 'boolean' },
            ],
          },
        ],
      },
      {
        id: 'administration-details',
        title: 'Administration and execution details',
        defaultExpanded: false,
        groups: [
          {
            id: 'dto-details',
            columns: 3,
            fields: [
              ['recId', 'Batch job ID'],
              ['tenantId', 'Company accounts'],
              ['canceledBy', 'Canceled by'],
              ['dataPartition', 'Data partition'],
              ['finishing', 'Finishing'],
              ['logLevel', 'Log level'],
              ['runtimeJob', 'Runtime job'],
              ['executingBy', 'Run by'],
              ['origStartDateTime', 'Original start date/time'],
              ['origStartDateTimeTzId', 'Original start time zone ID'],
              ['endDateTimeTzId', 'End time zone ID'],
              ['schedulingPriorityIsOverridden', 'Scheduling priority is overridden'],
              ['runCount', 'Execution count'],
              ['lastStatus', 'Last execution status'],
              ['lastError', 'Last error'],
              ['createdAt', 'Created date/time'],
              ['createdBy', 'Created by'],
            ].map(([name, label]) => ({ name, label, type: 'display' as const, disabled: true })),
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
              { name: 'recurrenceData', label: 'Recurrence data', type: 'text' },
              { name: 'startDateTimeTzId', label: 'Start time zone ID', type: 'number' },
              {
                name: 'startDate',
                label: 'Start date (UTC)',
                renderOwnLabel: true,
                render: ({ value, editing, disabled, onChange }) => (
                  <AppDateTimeField
                    label="Start date (UTC)"
                    includeTime={false}
                    value={String(value ?? '').slice(0, 10)}
                    disabled={disabled || !editing}
                    onChange={(next) => onChange(next ? `${next}T00:00:00Z` : '')}
                  />
                ),
              },
              { name: 'startTime', label: 'Start time (seconds)', type: 'number' },
              {
                name: 'startDateTime',
                label: 'Start at (UTC)',
                renderOwnLabel: true,
                render: ({ value, editing, disabled, onChange }) => {
                  const text = String(value ?? '');
                  const date = text ? new Date(text) : null;
                  const input =
                    date && !Number.isNaN(date.getTime()) ? date.toISOString().slice(0, 16) : '';
                  return (
                    <AppDateTimeField
                      label="Start at (UTC)"
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
                name: 'schedulingPriority',
                label: 'Scheduling Priority',
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
    getPrimaryText: (job) => job.caption,
    getSecondaryText: (job) => `${job.jobKey} · ${jobStatusLabels[job.status]}`,
    matchesSearch: (job, query) =>
      `${job.caption} ${job.jobKey} ${job.description ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (job) => ({
      ...job,
      description: job.description ?? '',
      jobKey: job.jobKey,
      statusText: jobStatusLabels[job.status],
      scheduleType: String(job.scheduleType),
      startDateTime: job.startDateTime ?? '',
      scheduledStartText: formatDateTime(job.startDateTime, i18n.language),
      origStartDateTime: formatDateTime(job.origStartDateTime, i18n.language),
      createdAt: formatDateTime(job.createdAt, i18n.language),
      lastStatus: job.lastStatus == null ? '—' : executionStatusLabels[job.lastStatus],
      schedulingPriorityIsOverridden: job.schedulingPriorityIsOverridden ? 'Yes' : 'No',
      endDateTime: formatDateTime(job.endDateTime, i18n.language),
      recurrenceData: job.recurrenceData ?? '',
      isEnabled: job.isEnabled,
      preventOverlap: job.preventOverlap,
      schedulingPriority: String(job.schedulingPriority ?? 1),
      maxRetryCount: job.maxRetryCount,
      retryDelaySeconds: job.retryDelaySeconds,
      timeoutSeconds: job.timeoutSeconds,
      payloadJson: job.payloadJson ?? '',
      batchGroup: job.batchGroup ?? '',
      activePeriod: job.activePeriod ?? '',
      critical: Boolean(job.critical),
      monitoringCategory: job.monitoringCategory ?? 0,
      managed: Boolean(job.managed),
      emitBusinessEvent: Boolean(job.emitBusinessEvent),
    }),
    setValues: (job, values) => ({
      ...job,
      description: String(values.description || '') || null,
      jobKey: String(values.jobKey || ''),
      scheduleType: Number(values.scheduleType) as SysBackgroundJobRecord['scheduleType'],
      recurrenceData: String(values.recurrenceData || '') || null,
      startDateTime: String(values.startDateTime || '') || null,
      isEnabled: Boolean(values.isEnabled),
      preventOverlap: Boolean(values.preventOverlap),
      schedulingPriority: Number(values.schedulingPriority ?? 1),
      maxRetryCount: Number(values.maxRetryCount || 0),
      retryDelaySeconds: Number(values.retryDelaySeconds || 0),
      timeoutSeconds: Number(values.timeoutSeconds || 0),
      payloadJson: String(values.payloadJson || '') || null,
      batchGroup: String(values.batchGroup || '') || null,
      activePeriod: String(values.activePeriod || '') || null,
      critical: values.critical ? 1 : 0,
      monitoringCategory: Number(values.monitoringCategory ?? 0),
      startDateTimeTzId:
        values.startDateTimeTzId === '' || values.startDateTimeTzId == null
          ? null
          : Number(values.startDateTimeTzId),
      startDate: String(values.startDate || '') || null,
      startTime:
        values.startTime === '' || values.startTime == null ? null : Number(values.startTime),
      managed: values.managed ? 1 : 0,
      emitBusinessEvent: values.emitBusinessEvent ? 1 : 0,
    }),
    headerFields: [
      {
        id: 'caption',
        label: 'Batch job',
        width: 330,
        getValue: (job) => job.caption,
        setValue: (job, value) => ({ ...job, caption: String(value) }),
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
      ...(!job.caption.trim() ? { caption: 'Batch job caption is required.' } : {}),
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
