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
  const missing = (label: string) => field(label, 'Not available', true);
  const heading = (text: string) => (
    <Typography sx={{ fontSize: 11, fontWeight: 600, textTransform: 'uppercase' }}>
      {text}
    </Typography>
  );
  const priority = ['Low', 'Normal', 'High'][job.priority] ?? 'Normal';
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
        {field('Job description', job.name)}
        {field('Status', ['Ready', 'Withhold', 'Cancelled', 'Completed'][job.status])}
        {heading('Dates')}
        {field('Actual start date/time', date(latest?.startedAt ?? job.lastRunAt))}
      </Box>
      <Box>
        {field('End date/time', date(latest?.completedAt))}
        {field('Scheduled start date/time', date(job.nextRunAt))}
        {heading('Administration')}
        {field('Created by', job.createdBy ?? '—')}
        {field('Run by', latest?.triggeredByUserId ?? '—')}
      </Box>
      <Box>
        {field('Company accounts', job.tenantId ?? '—')}
        {missing('Monitoring category')}
        {missing('Critical job')}
        {missing('Has alert')}
      </Box>
      <Box>
        {missing('Progress')}
        {missing('Batch group')}
        {missing('Group scheduling priority')}
        {missing('Scheduling priority is overridden')}
      </Box>
      <Box>
        {field('Job scheduling priority', priority)}
        {field('Effective scheduling priority', priority)}
        {field('Save job to history', 'Always')}
        {heading('Miscellaneous')}
        {missing('Active period')}
      </Box>
      <Box>
        {field('Execution count', job.runCount)}
        {field(
          'Recurrence text',
          job.scheduleType === 3
            ? `CRON (UTC): ${job.cronExpression}`
            : job.scheduleType === 2
              ? `Every ${job.intervalSeconds} seconds`
              : `One run: ${date(job.runAt)}`
        )}
        {history.isError && (
          <Typography color="error">Execution details could not be loaded.</Typography>
        )}
      </Box>
    </Box>
  );
}
