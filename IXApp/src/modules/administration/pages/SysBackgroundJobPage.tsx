import React, { useMemo } from 'react';
import { useBatchJobCommands } from '../components/useBatchJobCommands';
import { BatchJobTasks } from '../components/BatchJobTasks';
import { AppDateTimeField } from '@shared/components/fields/AppDateTimeField';
import { AppLookupField } from '@shared/components/fields/AppLookupField';
import { Alert } from '@mui/material';
import { usePermission } from '@core/permissions/usePermission';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailValues,
  DetailSectionConfig,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { sysBackgroundJobApi, type SysBackgroundJobRecord } from '../api/sysBackgroundJobApi';
import { sysBackgroundJobGroupApi } from '../api/sysBackgroundJobGroupApi';
import { batchJobActivePeriodApi } from '../api/batchJobActivePeriodApi';

const scheduleLabels = ['One time', 'Delayed', 'Recurring', 'Cron'];
const jobStatusLabels = ['Waiting', 'Withhold', 'Canceled', 'Ended'];
const executionStatusLabels = ['Waiting', 'Executing', 'Completed', 'Failed', 'Cancelled'];
const lookupValue = (value: string | number | (string | number)[] | null) =>
  Array.isArray(value) ? (value[0] ?? '') : (value ?? '');
const toDetailValues = (record: SysBackgroundJobRecord): DetailValues =>
  Object.fromEntries(
    Object.entries(record).map(([key, value]) => [key, value ?? ''])
  ) as DetailValues;
const monitoringCategoryLabels = [
  'Undefined',
  'Integration',
  'Workflow',
  'Store Order Synchronizer Job',
  'Assortment Details Job',
  'Assortment Lookup Job',
  'Transaction Sales Trans Mark Multi Job',
  'Statement Calculate Multi Job',
  'Sync Orders Scheduler Task',
  'Internal Org Update Channel Job',
  'Sales Form Letter Invoice Task',
  'Retail kit configure approval job',
  'Retail kit prices per company job',
];
const formatDateTime = (value: string | null, locale: string) =>
  value
    ? new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' }).format(
        new Date(value)
      )
    : '—';
const recurrenceText = (job: SysBackgroundJobRecord, locale: string) => {
  if (job.scheduleType === 3) return `CRON (UTC): ${job.recurrenceData ?? '—'}`;
  if (job.scheduleType === 2) {
    try {
      const value = JSON.parse(job.recurrenceData ?? '') as {
        unit?: string;
        interval?: number;
        endAfter?: number;
        endBy?: string;
      };
      const ending = value.endAfter
        ? ` Ends after ${value.endAfter} occurrences.`
        : value.endBy
          ? ` Ends by ${value.endBy}.`
          : '';
      return `Occurs every ${value.interval ?? 1} ${value.unit ?? 'intervals'}.${ending}`;
    } catch {
      return `Occurs every ${job.recurrenceData ?? '—'} seconds.`;
    }
  }
  return `One run at ${formatDateTime(job.origStartDateTime ?? job.startDateTime, locale)}.`;
};

const emptyJob = (): SysBackgroundJobRecord => ({
  id: `new-${crypto.randomUUID()}`,
  recId: 0,
  caption: '',
  jobKey: '',
  description: null,
  tenantId: null,
  dataAreaId: '',
  scheduleType: 2,
  recurrenceData: '3600',
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
  hasAlert: false,
  progress: 0,
  recurrenceCount: 0,
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
  const [taskFilterVisible, setTaskFilterVisible] = React.useState(false);
  const canRun = usePermission('System.BackgroundJobs.Run').hasPermission;
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
  const batchGroups = useQuery({
    queryKey: ['batch-groups', 'lookup'],
    queryFn: ({ signal }) => sysBackgroundJobGroupApi.list(signal),
  });
  const activePeriods = useQuery({
    queryKey: ['batch-job-active-periods', 'lookup'],
    queryFn: ({ signal }) => batchJobActivePeriodApi.list(signal),
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
            columns: 5,
            fields: [
              { name: 'caption', label: 'Job description', type: 'text' },
              {
                name: 'actualStartText',
                label: 'Actual start date/time',
                type: 'display',
                disabled: true,
              },
              { name: 'executingBy', label: 'Run by', type: 'display', disabled: true },
              { name: 'hasAlert', label: 'Has alert', type: 'boolean', disabled: true },
              {
                name: 'recurrenceCount',
                label: 'Recurrence count',
                type: 'number',
                disabled: true,
              },
              { name: 'statusText', label: 'Status', type: 'display', disabled: true },
              {
                name: 'scheduledStartDateTime',
                label: 'Scheduled start date/time',
                renderOwnLabel: true,
                render: ({ value, editing, disabled, onChange }) => {
                  const text = String(value ?? '');
                  const date = text ? new Date(text) : null;
                  const input =
                    date && !Number.isNaN(date.getTime()) ? date.toISOString().slice(0, 16) : '';
                  return (
                    <AppDateTimeField
                      label="Scheduled start date/time"
                      value={input}
                      disabled={disabled || !editing}
                      onChange={(next) =>
                        onChange(next ? `${next}${next.length === 16 ? ':00' : ''}Z` : '')
                      }
                    />
                  );
                },
              },
              { name: 'dataAreaId', label: 'Company', type: 'display', disabled: true },
              { name: 'progress', label: 'Progress', type: 'number', disabled: true },
              {
                name: 'recurrenceText',
                label: 'Recurrence text',
                type: 'display',
                multiline: true,
                disabled: true,
              },
              { name: 'endDateTimeText', label: 'End date/time', type: 'display', disabled: true },
              { name: 'createdBy', label: 'Created by', type: 'display', disabled: true },
              {
                name: 'monitoringCategory',
                label: 'Monitoring category',
                type: 'select',
                options: monitoringCategoryLabels.map((label, value) => ({
                  value: String(value),
                  label,
                })),
              },
              {
                name: 'logLevel',
                label: 'Save job to history',
                type: 'select',
                options: ['Always', 'Errors only', 'Never'].map((label, value) => ({
                  value: String(value),
                  label,
                })),
              },
              { name: 'critical', label: 'Critical job', type: 'boolean' },
              {
                name: 'activePeriod',
                label: 'Active period',
                renderOwnLabel: true,
                render: ({ value, editing, disabled, onChange }) => (
                  <AppLookupField
                    name="activePeriod"
                    label="Active period"
                    value={String(value ?? '')}
                    disabled={disabled || !editing}
                    displayMode="select"
                    options={(activePeriods.data ?? [])
                      .filter((period) => period.isActive || period.periodId === value)
                      .map((period) => ({
                        id: period.periodId,
                        code: period.periodId,
                        name: period.periodId,
                        description: period.name ?? undefined,
                      }))}
                    onChange={(next) => onChange(lookupValue(next))}
                  />
                ),
              },
              {
                name: 'batchGroup',
                label: 'Batch group',
                renderOwnLabel: true,
                render: ({ value, editing, disabled, onChange }) => (
                  <AppLookupField
                    name="batchGroup"
                    label="Batch group"
                    value={String(value ?? '')}
                    disabled={disabled || !editing}
                    displayMode="select"
                    options={(batchGroups.data ?? [])
                      .filter((group) => group.isActive || group.groupCode === value)
                      .map((group) => ({
                        id: group.groupCode,
                        code: group.groupCode,
                        name: group.groupCode,
                        description: group.description ?? undefined,
                      }))}
                    onChange={(next) => onChange(lookupValue(next))}
                  />
                ),
              },
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
              { name: 'description', label: 'Description', type: 'text' as const, multiline: true },
              {
                name: 'jobKey',
                label: 'Handler',
                type: 'select' as const,
                options: (handlers.data ?? []).map((value) => ({ value, label: value })),
              },
              {
                name: 'scheduleType',
                label: 'Schedule',
                type: 'select' as const,
                options: scheduleLabels.map((label, value) => ({ value: String(value), label })),
              },
              { name: 'managed', label: 'Managed', type: 'boolean' as const },
              { name: 'emitBusinessEvent', label: 'Emit business event', type: 'boolean' as const },
              ...[
                ['recId', 'Batch job ID'],
                ['tenantId', 'Tenant'],
                ['canceledBy', 'Canceled by'],
                ['dataPartition', 'Data partition'],
                ['finishing', 'Finishing'],
                ['runtimeJob', 'Runtime job'],
                ['origStartDateTimeTzId', 'Original start time zone ID'],
                ['endDateTimeTzId', 'End time zone ID'],
                ['schedulingPriorityIsOverridden', 'Scheduling priority is overridden'],
                ['runCount', 'Execution count'],
                ['lastStatus', 'Last execution status'],
                ['lastError', 'Last error'],
                ['createdAt', 'Created date/time'],
              ].map(([name, label]) => ({ name, label, type: 'display' as const, disabled: true })),
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
    [activePeriods.data, batchGroups.data, handlers.data]
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
      refreshIntervalMs: 2_000,
    },
    createRecord: emptyJob,
    getPrimaryText: (job) => job.caption,
    getSecondaryText: (job) => `${job.jobKey} · ${jobStatusLabels[job.status]}`,
    matchesSearch: (job, query) =>
      `${job.caption} ${job.jobKey} ${job.description ?? ''}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: (job) => ({
      ...toDetailValues(job),
      caption: job.caption,
      description: job.description ?? '',
      jobKey: job.jobKey,
      statusText: jobStatusLabels[job.status],
      scheduleType: String(job.scheduleType),
      startDateTime: job.startDateTime ?? '',
      scheduledStartDateTime: job.origStartDateTime ?? job.startDateTime ?? '',
      actualStartText: formatDateTime(job.startDateTime, i18n.language),
      endDateTimeText: formatDateTime(job.endDateTime, i18n.language),
      recurrenceText: recurrenceText(job, i18n.language),
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
      logLevel: String(job.logLevel ?? 0),
      managed: Boolean(job.managed),
      emitBusinessEvent: Boolean(job.emitBusinessEvent),
    }),
    setValues: (job, values) => ({
      ...job,
      caption: String(values.caption || ''),
      description: String(values.description || '') || null,
      jobKey: String(values.jobKey || ''),
      scheduleType: Number(values.scheduleType) as SysBackgroundJobRecord['scheduleType'],
      recurrenceData: String(values.recurrenceData || '') || null,
      startDateTime: String(values.scheduledStartDateTime || values.startDateTime || '') || null,
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
      logLevel: Number(values.logLevel ?? 0),
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
    headerFields: [],
    sections: ({ record, editing }) => [
      ...sections.map((section) => ({
        ...section,
        defaultExpanded: section.id === 'identification' ? true : section.defaultExpanded,
        groups: section.groups?.map((group) => ({
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
            showFilterRow={taskFilterVisible}
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
    ],
    onSearch: () => setTaskFilterVisible((visible) => !visible),
    permissions: {
      view: 'System.BackgroundJobs.View',
      create: 'System.BackgroundJobs.Create',
      edit: 'System.BackgroundJobs.Edit',
      delete: 'System.BackgroundJobs.Delete',
    },
    validate: (job) => ({
      ...(!job.caption.trim() ? { caption: 'Batch job caption is required.' } : {}),
      ...(!job.jobKey.trim() ? { jobKey: 'A registered handler is required.' } : {}),
      ...(job.scheduleType >= 2 && !job.recurrenceData?.trim()
        ? { recurrenceData: 'Recurrence data is required.' }
        : {}),
      ...(job.scheduleType < 2 && !job.startDateTime
        ? { scheduledStartDateTime: 'A scheduled start date/time is required.' }
        : {}),
      ...(job.maxRetryCount < 0 || job.maxRetryCount > 10
        ? { maxRetryCount: 'Maximum retries must be between 0 and 10.' }
        : {}),
      ...(job.retryDelaySeconds < 1 || job.retryDelaySeconds > 3600
        ? { retryDelaySeconds: 'Retry delay must be between 1 and 3600 seconds.' }
        : {}),
      ...(job.timeoutSeconds < 1 || job.timeoutSeconds > 86400
        ? { timeoutSeconds: 'Timeout must be between 1 and 86400 seconds.' }
        : {}),
      ...(() => {
        if (!job.payloadJson?.trim()) return {};
        try {
          JSON.parse(job.payloadJson);
          return {};
        } catch {
          return { payloadJson: 'Parameters must contain valid JSON.' };
        }
      })(),
    }) as Record<string, string>,
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
