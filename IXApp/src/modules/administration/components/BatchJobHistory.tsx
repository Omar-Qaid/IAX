import React from 'react';
import { Chip } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { SimpleListPage } from '@patterns/simple-list/SimpleListPage';
import { uiDensity } from '@shared/constants/uiDensity';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import { sysBackgroundJobApi, type SysJobExecutionStatus } from '../api/sysBackgroundJobApi';
const executionStatusLabels = ['Waiting', 'Executing', 'Completed', 'Failed', 'Cancelled'];
const formatDateTime = (value: string | null, locale: string) =>
  value
    ? new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' }).format(
        new Date(value)
      )
    : '—';
const executionTone = (status: SysJobExecutionStatus | null) =>
  status === 2 ? 'success' : status === 3 ? 'error' : status === 1 ? 'warning' : 'default';

export function BatchJobHistory({ jobId }: { jobId: number }): React.ReactElement {
  const { i18n } = useAppTranslation();
  const executions = useQuery({
    queryKey: ['background-job-executions', jobId],
    queryFn: ({ signal }) => sysBackgroundJobApi.executions(jobId, signal),
    enabled: jobId > 0,
  });
  return (
    <SimpleListPage
      title="Batch job history"
      dataGridProps={{
        storageKey: 'administration.batch-job-history.reference-view',
        hideSidebar: false,
        pageSize: 50,
        rowHeight: uiDensity.gridRowHeight,
        headerHeight: uiDensity.gridRowHeight,
      }}
      dataSource={{
        type: 'controlled',
        rows: (executions.data ?? []).map((row) => ({ ...row, id: String(row.recId) })),
        loading: executions.isLoading,
        error:
          jobId <= 0
            ? 'Select a valid batch job.'
            : executions.isError
              ? executions.error.message
              : null,
        refresh: () => {
          void executions.refetch();
        },
      }}
      columns={[
        { field: 'recId', headerName: 'Execution ID', width: 110 },
        { field: 'jobId', headerName: 'Batch job ID', width: 120 },
        {
          field: 'caption',
          headerName: 'Job description',
          width: 230,
          valueGetter: ({ row }) => row.caption ?? row.jobCaption ?? '',
        },
        { field: 'batchCreatedBy', headerName: 'Created by', width: 150 },
        {
          field: 'executedBy',
          headerName: 'Executed by',
          width: 150,
          valueGetter: ({ row }) => row.executedBy ?? row.triggeredByUserId ?? '',
        },
        { field: 'batchGroup', headerName: 'Batch group', width: 120 },
        ...(
          [
            'alertsProcessed',
            'canceledBy',
            'dataPartition',
            'endDateTimeTzId',
            'finishing',
            'origStartDateTime',
            'origStartDateTimeTzId',
            'startDateTimeTzId',
            'runtimeJob',
            'groupSchedulingPriority',
            'jobSchedulingPriority',
            'jobSchedulingPriorityIsOverridden',
            'createdAt',
            'serverName',
            'trigger',
          ] as const
        ).map((field) => ({
          field,
          headerName: field.replace(/([A-Z])/g, ' $1').replace(/^./, (c) => c.toUpperCase()),
          width: 180,
        })),
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
