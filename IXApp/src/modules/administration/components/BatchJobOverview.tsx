import { Box, Typography } from '@mui/material';
import { ListGridField } from '@patterns/list-details-listgrid/ListGridField';
import type { SysBackgroundJobRecord } from '../api/sysBackgroundJobApi';

const statusLabels = ['Ready', 'Withhold', 'Cancelled', 'Completed'];

export function BatchJobOverview({ job }: { job: SysBackgroundJobRecord }) {
  const date = (value?: string | null) => value ? new Date(value).toLocaleString() : '—';
  const field = (label: string, value: string | number) => <ListGridField label={label} value={value} underlined />;
  const heading = (text: string) => <Typography sx={{ fontSize: 11, fontWeight: 700, textTransform: 'uppercase' }}>{text}</Typography>;
  const recurrenceText = job.scheduleType === 3
    ? `CRON (UTC): ${job.recurrenceData ?? '—'}`
    : job.scheduleType === 2
      ? `Occurs every ${job.recurrenceData ?? '—'} seconds.`
      : `One run at ${date(job.origStartDateTime ?? job.startDateTime)}.`;

  return (
    <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', md: 'repeat(2,minmax(0,1fr))', lg: 'repeat(5,minmax(0,1fr))' }, gap: 4, '& > div': { display: 'flex', flexDirection: 'column', gap: '12px' } }}>
      <Box>{heading('Identification')}{field('Job description', job.caption)}{field('Status', statusLabels[job.status] ?? String(job.status))}{heading('Dates')}{field('Actual start date/time', date(job.startDateTime))}</Box>
      <Box>{field('End date/time', date(job.endDateTime))}{field('Scheduled start date/time', date(job.origStartDateTime))}{heading('Administration')}{field('Created by', job.createdBy ?? '—')}</Box>
      <Box>{field('Run by', job.executingBy ?? '—')}{field('Company accounts', job.dataAreaId || '—')}{field('Monitoring category', job.monitoringCategory ?? 0)}{field('Critical job', job.critical ? 'Yes' : 'No')}</Box>
      <Box>{field('Has alert', job.hasAlert ? 'Yes' : 'No')}{field('Progress', Number(job.progress ?? 0).toFixed(2))}{heading('Miscellaneous')}{field('Active period', job.activePeriod ?? '—')}</Box>
      <Box>{field('Recurrence count', job.recurrenceCount ?? 0)}{field('Recurrence text', recurrenceText)}</Box>
    </Box>
  );
}
