import { Box, Typography } from '@mui/material';
import { useQuery } from '@tanstack/react-query';
import { ListGridField } from '@patterns/list-details-listgrid/ListGridField';
import { sysBackgroundJobApi, type SysBackgroundJobRecord } from '../api/sysBackgroundJobApi';

export function BatchJobOverview({ job }: { job: SysBackgroundJobRecord }) {
  const history = useQuery({
    queryKey: ['background-job-executions', job.recId],
    queryFn: ({ signal }) => sysBackgroundJobApi.executions(job.recId, signal),
    enabled: job.recId > 0,
  });
  const latest = history.data?.slice().sort((a, b) => b.recId - a.recId)[0];
  const date = (value?: string | null) => (value ? new Date(value).toLocaleString() : '—');
  const field = (label: string, value: string | number, unavailable = false) => (
    <Box
      title={
        unavailable ? 'This setting is not supported by the current batch scheduler.' : undefined
      }
    >
      <ListGridField label={label} value={value} underlined />
    </Box>
  );
  const heading = (text: string) => (
    <Typography sx={{ fontSize: 11, fontWeight: 600, textTransform: 'uppercase' }}>
      {text}
    </Typography>
  );
  const priority = ['Low', 'Normal', 'High'][job.schedulingPriority] ?? 'Normal';
  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: {
          xs: '1fr',
          sm: 'repeat(2,minmax(0,1fr))',
          lg: 'repeat(3,minmax(0,1fr))',
          xl: 'repeat(6,minmax(0,1fr))',
        },
        gap: 3,
        '& > div': { display: 'flex', flexDirection: 'column', gap: '10px' },
      }}
    >
      <Box>
        {heading('Identification')}
        {field('Batch job', job.caption)}
        {field('Status', ['Ready', 'Withhold', 'Cancelled', 'Completed'][job.status])}
        {heading('Dates')}
        {field('Actual start date/time', date(latest?.startedAt))}
      </Box>
      <Box>
        {field('End date/time', date(latest?.completedAt ?? job.endDateTime))}
        {field('Scheduled start date/time', date(job.startDateTime))}
        {heading('Administration')}
        {field('Created by', job.createdBy ?? '—')}
        {field('Run by', job.executingBy ?? latest?.triggeredByUserId ?? '—')}
      </Box>
      <Box>
        {field('Company accounts', job.tenantId ?? '—')}
        {field('Monitoring category', job.monitoringCategory ?? 0)}
        {field('Critical job', job.critical ? 'Yes' : 'No')}
        {field('Emit business event', job.emitBusinessEvent ? 'Yes' : 'No')}
        {field('Managed', job.managed ? 'Yes' : 'No')}
      </Box>
      <Box>
        {field('Finishing', job.finishing ?? 0)}
        {field('Batch group', job.batchGroup ?? '—')}
        {field('Last execution group priority', latest?.groupSchedulingPriority ?? '?')}
        {field(
          'Scheduling priority is overridden',
          job.schedulingPriorityIsOverridden ? 'Yes' : 'No'
        )}
      </Box>
      <Box>
        {field('Job scheduling priority', priority)}
        {field('Last execution job priority', latest?.jobSchedulingPriority ?? '?')}
        {field('Log level', job.logLevel ?? 0)}
        {heading('Miscellaneous')}
        {field('Active period', job.activePeriod ?? '—')}
      </Box>
      <Box>
        {field('Execution count', job.runCount)}
        {field(
          'Recurrence text',
          job.scheduleType === 3
            ? `CRON (UTC): ${job.recurrenceData ?? '?'}`
            : job.scheduleType === 2
              ? `Recurrence Data: ${job.recurrenceData}`
              : `One run: ${date(job.startDateTime)}`
        )}
        {history.isError && (
          <Typography color="error">Execution details could not be loaded.</Typography>
        )}
      </Box>
    </Box>
  );
}
