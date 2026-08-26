import React, { useMemo } from 'react';
import { Alert, alpha, Box, CircularProgress, Drawer, IconButton, Typography } from '@mui/material';
import {
  Timeline,
  TimelineConnector,
  TimelineContent,
  TimelineDot,
  TimelineItem,
  TimelineSeparator,
} from '@mui/lab';
import CloseIcon from '@mui/icons-material/Close';
import HistoryIcon from '@mui/icons-material/History';
import HistoryEduIcon from '@mui/icons-material/HistoryEdu';
import UndoIcon from '@mui/icons-material/Undo';
import type { TimelineDotProps } from '@mui/lab/TimelineDot';
import { useQuery } from '@tanstack/react-query';
import { format } from 'date-fns';
import { useTranslation } from 'react-i18next';
import { apiClient } from '@core/api/apiClient';
import type { ApiResponse } from '@core/api/apiResponse';
import { isWorkflowXmlValue, RecordValueDisplay } from './RecordValueDisplay';
import { useLogicalDrawerAnchor } from '@shared/hooks/useLogicalDrawerAnchor';

interface RecordAuditDto {
  recId: number;
  tableName: string;
  recordId: string;
  columnName: string;
  oldValue: string | null;
  newValue: string | null;
  operation: string;
  changedBy: string | null;
  changedOn: string;
}

export interface RecordAuditChange {
  field: string;
  old: string;
  new: string;
}

export interface RecordAuditActivity {
  date: string;
  user: string;
  action: string;
  color?: TimelineDotProps['color'];
  icon?: React.ReactNode;
  changes: RecordAuditChange[];
}

export interface AppRecordAuditDrawerProps {
  open: boolean;
  onClose: () => void;
  tableName: string;
  recordId: string | number | null;
}

const humanizeField = (value: string): string =>
  value
    .replace(/([a-z0-9])([A-Z])/g, '$1 $2')
    .replace(/[_-]+/g, ' ')
    .replace(/^./, (character) => character.toUpperCase());

function EmptyAudit(): React.ReactElement {
  const { t } = useTranslation();
  return (
    <Box
      sx={{
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        justifyContent: 'center',
        p: 4,
        textAlign: 'center',
        opacity: 0.8,
      }}
    >
      <Box
        sx={{
          width: 80,
          height: 80,
          borderRadius: '50%',
          bgcolor: 'action.hover',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          mb: 2,
        }}
      >
        <HistoryEduIcon sx={{ fontSize: 40, color: 'text.disabled' }} />
      </Box>
      <Typography
        variant="subtitle1"
        sx={{ fontWeight: 700, color: 'text.secondary' }}
        gutterBottom
      >
        {t('common.no_history', 'No History')}
      </Typography>
      <Typography variant="body2" color="text.disabled" sx={{ maxWidth: 200 }}>
        {t('common.no_history_msg', 'No audit trail found for this record.')}
      </Typography>
    </Box>
  );
}

function AuditChange({ change }: { change: RecordAuditChange }): React.ReactElement {
  const { t } = useTranslation();
  const structured = isWorkflowXmlValue(change.old) || isWorkflowXmlValue(change.new);
  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: structured ? 'minmax(0, 1fr)' : '100px minmax(0, 1fr)',
        alignItems: 'start',
        columnGap: 1,
        rowGap: 0.35,
        py: 0.65,
        borderBottom: '1px solid',
        borderColor: 'divider',
        '&:last-child': { borderBottom: 0 },
      }}
    >
      <Typography
        variant="caption"
        sx={{ display: 'block', color: 'text.secondary', fontWeight: 600, overflowWrap: 'anywhere' }}
      >
        {t(change.field, humanizeField(change.field))}
      </Typography>
      <Box sx={{ display: structured ? 'grid' : 'flex', alignItems: structured ? 'stretch' : 'center', gap: structured ? 0.5 : 1, flexWrap: 'wrap', minWidth: 0 }}>
        <Box sx={{ minWidth: 0, maxWidth: '100%', bgcolor: (theme) => alpha(theme.palette.error.main, 0.1), px: 0.5, borderRadius: 0.5 }}>
          <RecordValueDisplay value={change.old} tone="old" strikeThrough />
        </Box>
        <Typography variant="caption" color="text.secondary">
          →
        </Typography>
        <Box sx={{ minWidth: 0, maxWidth: '100%', bgcolor: (theme) => alpha(theme.palette.success.main, 0.1), px: 0.5, borderRadius: 0.5, fontWeight: 600 }}>
          <RecordValueDisplay value={change.new} tone="new" />
        </Box>
      </Box>
    </Box>
  );
}

function AuditActivity({
  activity,
  isLast,
}: {
  activity: RecordAuditActivity;
  isLast: boolean;
}): React.ReactElement {
  const { t } = useTranslation();
  return (
    <TimelineItem
      sx={{
        minHeight: 0,
        '&::before': {
          display: 'none !important',
          content: 'none',
          flex: '0 0 0',
          p: 0,
        },
      }}
    >
      <TimelineSeparator>
        <TimelineDot color={activity.color || 'primary'} sx={{ boxShadow: 'none' }}>
          {activity.icon}
        </TimelineDot>
        {!isLast && <TimelineConnector />}
      </TimelineSeparator>
      <TimelineContent sx={{ minWidth: 0, py: 1.25, pl: 1, pr: 0.5 }}>
        <Box
          sx={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'flex-start',
            mb: 0.5,
          }}
        >
          <Typography variant="subtitle2" sx={{ fontWeight: 600, lineHeight: 1.2 }}>
            {t(`common.${activity.action.toLowerCase()}`, activity.action)}
          </Typography>
          <Typography variant="caption" color="text.secondary" sx={{ whiteSpace: 'nowrap' }}>
            {format(new Date(activity.date), 'MMM dd, HH:mm')}
          </Typography>
        </Box>
        <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.8rem', mb: 1 }}>
          {t('common.by', 'By')}{' '}
          <Box component="span" sx={{ color: 'text.primary', fontWeight: 500 }}>
            {activity.user}
          </Box>
        </Typography>
        <Box
          sx={{
            px: 1,
            py: 0.35,
            bgcolor: 'action.hover',
            borderRadius: 0.75,
            border: '1px solid',
            borderColor: 'divider',
            minWidth: 0,
          }}
        >
          {activity.changes.map((change, index) => (
            <AuditChange key={`${change.field}-${index}`} change={change} />
          ))}
        </Box>
      </TimelineContent>
    </TimelineItem>
  );
}

export function AppRecordAuditDrawer({
  open,
  onClose,
  tableName,
  recordId,
}: AppRecordAuditDrawerProps): React.ReactElement {
  const { t } = useTranslation();
  const drawerAnchor = useLogicalDrawerAnchor('end');
  const audit = useQuery({
    queryKey: ['record-audit', tableName, recordId],
    enabled: open && Boolean(tableName) && recordId != null,
    queryFn: async ({ signal }) => {
      const response = await apiClient.get<ApiResponse<RecordAuditDto[]>>(
        '/v1/SysAuditLog/by-record',
        {
          params: { tableName, recordId: String(recordId), pageNumber: 1, pageSize: 100 },
          signal,
        }
      );
      if (!response.data.success) throw new Error(response.data.message || 'Unable to load audit.');
      return response.data.data ?? [];
    },
  });

  const activities = useMemo<RecordAuditActivity[]>(() => {
    const grouped = new Map<string, RecordAuditActivity>();
    for (const entry of audit.data ?? []) {
      const key = `${entry.changedOn}|${entry.changedBy ?? ''}|${entry.operation}`;
      const change = {
        field: entry.columnName,
        old: entry.oldValue ?? 'NULL',
        new: entry.newValue ?? 'NULL',
      };
      const existing = grouped.get(key);
      if (existing) existing.changes.push(change);
      else
        grouped.set(key, {
          date: entry.changedOn,
          user: entry.changedBy || 'System',
          action: entry.operation,
          changes: [change],
        });
    }
    return [...grouped.values()];
  }, [audit.data]);

  const loading = audit.isLoading || audit.isFetching;
  return (
    <Drawer
      anchor={drawerAnchor}
      open={open}
      onClose={onClose}
      slotProps={{ paper: { sx: { width: { xs: '100vw', sm: 400 }, maxWidth: '100vw', borderInlineStart: 1, borderInlineStartColor: 'divider', borderInlineEnd: 0 } } }}
      sx={{ zIndex: (theme) => theme.zIndex.drawer + 2 }}
    >
      <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
        <Box
          sx={{
            p: 2,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'space-between',
            borderBottom: '1px solid',
            borderColor: 'divider',
            bgcolor: 'action.hover',
          }}
        >
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
            <HistoryIcon color="primary" />
            <Typography variant="subtitle1" sx={{ fontWeight: 700 }}>
              {t('common.recordAudit')}
            </Typography>
          </Box>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
            <IconButton
              onClick={() => void audit.refetch()}
              size="small"
              disabled={loading}
              aria-label={t('actions.refresh', 'Refresh')}
            >
              <UndoIcon
                fontSize="small"
                sx={{
                  animation: loading ? 'spin 1s linear infinite' : 'none',
                  '@keyframes spin': {
                    '0%': { transform: 'rotate(0deg)' },
                    '100%': { transform: 'rotate(360deg)' },
                  },
                }}
              />
            </IconButton>
            <IconButton onClick={onClose} size="small" aria-label={t('actions.close', 'Close')}>
              <CloseIcon fontSize="small" />
            </IconButton>
          </Box>
        </Box>

        <Box sx={{ flex: 1, overflowY: 'auto', p: 1 }}>
          {loading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
              <CircularProgress size={24} />
            </Box>
          ) : audit.error ? (
            <Alert severity="error">
              {audit.error instanceof Error
                ? audit.error.message
                : t('errors.loadFailed', 'Unable to load audit.')}
            </Alert>
          ) : activities.length === 0 ? (
            <EmptyAudit />
          ) : (
            <Timeline
              sx={{
                width: '100%',
                m: 0,
                p: 0,
                '& .MuiTimelineItem-root::before': { display: 'none !important', content: 'none' },
                '& .MuiTimelineContent-root': { minWidth: 0, pr: 0.5 },
              }}
            >
              {activities.map((activity, index) => (
                <AuditActivity
                  key={`${activity.date}-${activity.user}-${activity.action}`}
                  activity={activity}
                  isLast={index === activities.length - 1}
                />
              ))}
            </Timeline>
          )}
        </Box>

        <Box
          sx={{
            py: 0.75,
            px: 2,
            borderTop: '1px solid',
            borderColor: 'divider',
            bgcolor: 'background.paper',
          }}
        >
          <Typography variant="caption" color="text.secondary">
            {t('common.total_activities', {
              count: activities.length,
              defaultValue: `Total activities: ${activities.length}`,
            })}
          </Typography>
        </Box>
      </Box>
    </Drawer>
  );
}
