import React from 'react';
import {
  Alert,
  Box,
  Button,
  CircularProgress,
  Chip,
  Divider,
  Paper,
  Stack,
  Typography,
} from '@mui/material';
import HistoryOutlined from '@mui/icons-material/HistoryOutlined';
import AssignmentTurnedInOutlined from '@mui/icons-material/AssignmentTurnedInOutlined';
import AttachFileOutlined from '@mui/icons-material/AttachFileOutlined';
import { useQueries, useQuery } from '@tanstack/react-query';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { documentApi } from '@shared/components/documents/documentApi';
import { documentTableIds } from '@shared/components/documents/recordTableIds';
import { wfProcessApi } from '../api/wfProcessApi';
import { wfRequestApi, type MailTrackingEntryDto, type WfRequestRecord } from '../api/wfRequestApi';
import { normalizeDynamicControlType } from '../components/DynamicControlRenderer';
import { MailFieldValue } from '../components/MailFieldValue';
import { WorkflowMailPrintoutViewer } from './WorkflowMailPrintoutPage';
import { WorkflowOfficialFormViewer } from './WorkflowOfficialFormPage';
import { printTemplateApi } from '../print-templates/api/printTemplateApi';
import type { PrintTemplateSummary } from '../print-templates/types/printTemplate.types';
import {
  selectDefaultPublishedTemplate,
  selectPublishedTemplates,
} from '../print-templates/runtime/publishedTemplateSelection';
import { useAppTranslation } from '@core/localization/useAppTranslation';
import type { TFunction } from 'i18next';
import { APP_FONT_FAMILY } from '@shared/constants/fontFamilies';

type MailFolder = 'all' | 'inbox' | 'sent' | 'important';

interface MailRecord extends WfRequestRecord {
  processName: string;
  requestedBy: string;
  stepNumber: number;
}

const emptyMailRecord = (): MailRecord => ({
  id: 'empty-mail',
  recId: 0,
  code: null,
  name: '',
  description: null,
  requestDate: '',
  processId: 0,
  employeeId: null,
  requestDetails: null,
  isFinished: false,
  finishedDate: null,
  isStopped: false,
  stoppedDate: null,
  score: 0,
  progress: 0,
  notes: null,
  isActive: true,
  rowVersion: null,
  recVersion: 1,
  dataAreaId: 'dat',
  processName: '',
  requestedBy: '',
  stepNumber: 1,
});

const formatDateTime = (value: string | null, locale?: string): string => {
  if (!value) return '—';
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? value
    : new Intl.DateTimeFormat(locale, { dateStyle: 'medium', timeStyle: 'short' }).format(date);
};

const formatTimelineDate = (value: string, locale?: string): string => {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat(locale, {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: 'numeric',
    minute: '2-digit',
  }).format(date);
};

const getStatus = (request: WfRequestRecord, t: TFunction): string => {
  if (request.isStopped) return t('mail.statuses.stopped');
  if (request.isFinished) return t('mail.statuses.completed');
  return t('mail.statuses.inProgress');
};

const formatElapsed = (request: WfRequestRecord, t: TFunction): string => {
  const started = new Date(request.requestDate).getTime();
  const endedValue = request.finishedDate || request.stoppedDate;
  const ended = endedValue ? new Date(endedValue).getTime() : Date.now();
  if (!Number.isFinite(started) || !Number.isFinite(ended)) return t('mail.notAvailable');
  const minutes = Math.max(0, Math.floor((ended - started) / 60_000));
  return t('mail.hoursMinutes', { hours: Math.floor(minutes / 60), minutes: minutes % 60 });
};

function LabelValue({
  label,
  value,
  compact = false,
}: {
  label: string;
  value: React.ReactNode;
  compact?: boolean;
}) {
  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: '34% minmax(0, 66%)',
        minHeight: compact ? 40 : 48,
        borderBottom: '1px solid #e5e5e5',
      }}
    >
      <Typography
        sx={{
          p: compact ? '6px 10px' : '8px 12px',
          bgcolor: '#f7f7f7',
          color: '#111',
          fontSize: 12,
          fontWeight: 600,
          textAlign: 'start',
        }}
      >
        {label}
      </Typography>
      <Box
        sx={{
          p: compact ? '6px 10px' : '8px 12px',
          bgcolor: '#fff',
          color: '#222',
          fontSize: 12,
          fontWeight: 400,
          minWidth: 0,
          overflowWrap: 'anywhere',
          textAlign: 'start',
        }}
      >
        {value}
      </Box>
    </Box>
  );
}

const formatFileSize = (bytes: number | null): string => {
  if (!bytes) return '';
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
};

function MailAttachments({ requestId, detailIds }: { requestId: number; detailIds: number[] }) {
  const { t } = useAppTranslation();
  const [previewError, setPreviewError] = React.useState(false);
  const attachments = useQuery({
    queryKey: ['documents', documentTableIds.wfRequest, requestId],
    queryFn: ({ signal }) => documentApi.list(documentTableIds.wfRequest, requestId, signal),
    enabled: requestId > 0,
  });
  const detailAttachments = useQueries({
    queries: [...new Set(detailIds.filter((id) => id > 0))].map((detailId) => ({
      queryKey: ['documents', documentTableIds.wfRequestDetail, detailId],
      queryFn: ({ signal }: { signal: AbortSignal }) =>
        documentApi.list(documentTableIds.wfRequestDetail, detailId, signal),
    })),
  });
  const items = React.useMemo(() => {
    const combined = [
      ...(attachments.data?.items ?? []),
      ...detailAttachments.flatMap((query) => query.data?.items ?? []),
    ];
    return [...new Map(combined.map((item) => [item.id, item])).values()];
  }, [attachments.data?.items, detailAttachments]);
  const loading = attachments.isLoading || detailAttachments.some((query) => query.isLoading);
  const failed = attachments.isError || detailAttachments.some((query) => query.isError);

  return (
    <Paper variant="outlined" sx={{ borderRadius: 1.25, overflow: 'hidden' }}>
      <Stack
        direction="row"
        spacing={0.75}
        sx={{ alignItems: 'center', px: 1.25, minHeight: 36, borderBottom: '1px solid #e2e2e2' }}
      >
        <AttachFileOutlined sx={{ fontSize: 16 }} />
        <Typography sx={{ flex: 1, fontSize: 12.5, fontWeight: 700 }}>
          {t('mail.attachments.title')}
        </Typography>
        <Typography color="text.secondary" sx={{ fontSize: 11 }}>
          ({items.length})
        </Typography>
      </Stack>
      {loading && (
        <Box sx={{ display: 'grid', placeItems: 'center', minHeight: 72 }}>
          <CircularProgress size={20} />
        </Box>
      )}
      {failed && (
        <Alert severity="error" sx={{ borderRadius: 0 }}>
          {t('mail.attachments.loadError')}
        </Alert>
      )}
      {previewError && (
        <Alert severity="error" onClose={() => setPreviewError(false)} sx={{ borderRadius: 0 }}>
          {t('mail.attachments.openError')}
        </Alert>
      )}
      {!loading && !failed && items.length === 0 && (
        <Typography
          color="text.secondary"
          sx={{ py: 2.5, px: 1.5, textAlign: 'center', fontSize: 11.5 }}
        >
          {t('mail.attachments.none')}
        </Typography>
      )}
      {items.map((item) => (
        <Button
          key={item.id}
          fullWidth
          onClick={() => {
            setPreviewError(false);
            void documentApi.preview(item).catch(() => setPreviewError(true));
          }}
          sx={{
            display: 'grid',
            gridTemplateColumns: '28px minmax(0, 1fr) auto',
            gap: 1,
            minHeight: 51,
            px: 1.25,
            borderRadius: 0,
            borderBottom: '1px solid #eeeeee',
            color: 'text.primary',
            textAlign: 'start',
            textTransform: 'none',
          }}
        >
          <AttachFileOutlined color="error" sx={{ fontSize: 18 }} />
          <Box dir="auto" sx={{ minWidth: 0, textAlign: 'start' }}>
            <Typography noWrap sx={{ fontSize: 11.5, fontWeight: 600 }}>
              {item.name || item.fileName}
            </Typography>
            <Typography noWrap color="text.secondary" sx={{ fontSize: 10 }}>
              {item.originalFileName || item.documentTypeName}
            </Typography>
          </Box>
          <Typography color="text.secondary" sx={{ fontSize: 9.5 }}>
            {formatFileSize(item.fileSize)}
          </Typography>
        </Button>
      ))}
    </Paper>
  );
}

interface TrackingTimelineProps {
  entries: MailTrackingEntryDto[];
}

export function TrackingTimeline({ entries }: TrackingTimelineProps): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  return (
    <Box
      sx={{
        minWidth: 0,
        width: '100%',
        maxWidth: 'none',
        height: '100%',
        boxSizing: 'border-box',
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
        color: '#171717',
        fontFamily: APP_FONT_FAMILY,
        textAlign: 'start',
      }}
    >
      <Stack
        direction="row"
        spacing={0.55}
        sx={{
          alignItems: 'center',
          justifyContent: 'flex-start',
          mb: 1.5,
          paddingInlineStart: 0.75,
        }}
      >
        <HistoryOutlined sx={{ fontSize: 17 }} />
        <Typography sx={{ fontSize: 13, lineHeight: 1.35, fontWeight: 600 }}>
          {t('mail.trackingLog')}
        </Typography>
      </Stack>
      <Box
        sx={{
          position: 'relative',
          flex: 1,
          minHeight: 0,
          overflowY: 'auto',
          overflowX: 'hidden',
          px: 0.5,
          pb: 0.5,
          scrollbarWidth: 'thin',
          scrollbarColor: '#b8b8b8 transparent',
          '&::-webkit-scrollbar': { width: 6 },
          '&::-webkit-scrollbar-thumb': { bgcolor: '#b8b8b8', borderRadius: 3 },
          '&::-webkit-scrollbar-track': { bgcolor: 'transparent' },
          '&::before': {
            content: '""',
            position: 'absolute',
            insetInlineStart: 14,
            top: 12,
            bottom: 14,
            width: '1px',
            bgcolor: '#e5e7eb',
          },
        }}
      >
        {entries.map((entry) => (
          <Box
            key={entry.assignmentId}
            sx={{
              position: 'relative',
              display: 'grid',
              gridTemplateColumns: '20px minmax(0, 1fr)',
              columnGap: '10px',
              width: '100%',
              textAlign: 'start',
              mb: 3.25,
            }}
          >
            <Box
              sx={{
                position: 'relative',
                gridColumn: 1,
                minWidth: 0,
              }}
            >
              <Box
                sx={{
                  position: 'absolute',
                  zIndex: 1,
                  insetInlineStart: '50%',
                  top: 24,
                  transform: 'translateX(-50%)',
                  display: 'grid',
                  placeItems: 'center',
                  width: entry.isCurrent ? 14 : 11,
                  height: entry.isCurrent ? 14 : 11,
                  borderRadius: '50%',
                  bgcolor: entry.isCurrent ? '#fff' : '#d8dadd',
                  border: entry.isCurrent ? '2px solid #1976d2' : 0,
                  boxShadow: '0 0 0 2px #fff',
                  '&::before': entry.isCurrent
                    ? {
                        content: '""',
                        width: 6,
                        height: 6,
                        borderRadius: '50%',
                        bgcolor: '#1976d2',
                      }
                    : undefined,
                  '&::after':
                    entry.isCompleted && !entry.isCurrent
                      ? {
                          content: '"✓"',
                          color: '#fff',
                          fontSize: 8,
                          lineHeight: 1,
                          fontWeight: 800,
                        }
                      : undefined,
                }}
              />
            </Box>
            <Paper
              variant="outlined"
              sx={{
                gridColumn: 2,
                minWidth: 0,
                width: '100%',
                boxSizing: 'border-box',
                p: '14px 12px',
                borderRadius: '6px',
                border: `1px solid ${entry.isCurrent ? '#1976d2' : '#e5e7eb'}`,
                bgcolor: '#fff',
                boxShadow: '0 1px 2px rgba(0,0,0,.02)',
              }}
            >
              <Stack
                direction="row"
                sx={{ justifyContent: 'space-between', alignItems: 'flex-start', gap: 1.25 }}
              >
                <Box dir="auto" sx={{ minWidth: 0, textAlign: 'start' }}>
                  <Typography
                    sx={{
                      color: entry.isCurrent ? '#171717' : '#444',
                      fontSize: 13,
                      lineHeight: 1.45,
                      fontWeight: entry.isCurrent ? 600 : 500,
                    }}
                  >
                    {entry.title}
                  </Typography>
                  <Typography
                    sx={{
                      mt: 0.15,
                      color: entry.isCurrent ? '#1976d2' : '#757575',
                      fontSize: 11,
                      lineHeight: 1.5,
                    }}
                  >
                    {entry.stage}
                  </Typography>
                </Box>
                <Typography
                  sx={{
                    flexShrink: 0,
                    color: '#777',
                    fontSize: 11,
                    lineHeight: 1.6,
                  }}
                >
                  {formatTimelineDate(entry.date, currentLanguage.code)}
                </Typography>
              </Stack>
              <Box sx={{ display: 'flex', mt: 0.65, justifyContent: 'flex-start' }}>
                <Box
                  sx={{
                    px: entry.isCurrent ? 0.875 : 0.75,
                    py: entry.isCurrent ? 0.375 : 0.25,
                    borderRadius: '3px',
                    bgcolor: entry.isCurrent ? '#1976d2' : '#e8e8e8',
                    color: entry.isCurrent ? '#fff' : '#666',
                    fontSize: 10,
                    lineHeight: 1.35,
                    fontWeight: 600,
                  }}
                >
                  {entry.isCurrent ? t('mail.statuses.processing') : t('mail.statuses.completed')}
                </Box>
              </Box>
              <Box
                sx={{
                  mt: 1.15,
                  display: 'grid',
                  gridTemplateColumns: 'max-content minmax(0, 1fr)',
                  columnGap: '6px',
                  rowGap: 0.55,
                  color: entry.isCurrent ? '#171717' : '#737373',
                  fontSize: 11,
                  lineHeight: 1.7,
                }}
              >
                <Box component="span" sx={{ color: '#555' }}>
                  {t('mail.timeline.responsible')}
                </Box>
                <Box component="span" dir="auto">
                  {entry.responsible}
                </Box>
                <Box component="span" sx={{ color: '#555' }}>
                  {t('mail.timeline.action')}
                </Box>
                <Box component="span" dir="auto">
                  {entry.action}
                </Box>
                <Box component="span" sx={{ color: '#555' }}>
                  {t('mail.timeline.notes')}
                </Box>
                <Box component="span" dir="auto" sx={{ overflowWrap: 'anywhere' }}>
                  {entry.notes}
                </Box>
              </Box>
            </Paper>
          </Box>
        ))}
      </Box>
    </Box>
  );
}

function MailDetails({ request }: { request: MailRecord }) {
  const { t, currentLanguage } = useAppTranslation();
  const mailDetails = useQuery({
    queryKey: ['workflow', 'mail', 'request-details', request.recId],
    queryFn: ({ signal }) => wfRequestApi.mailDetails(request.recId, signal),
    enabled: request.recId > 0,
  });
  const details = mailDetails.data;
  const progress = Math.max(0, Math.min(100, request.progress));
  const statusColor = request.isStopped ? 'error' : request.isFinished ? 'success' : 'primary';

  return (
    <Box
      sx={{
        height: { xs: 'auto', lg: 'calc(100dvh - 188px)' },
        minHeight: 0,
        overflow: 'hidden',
        display: 'grid',
        gridTemplateColumns: {
          xs: '1fr',
          lg: 'minmax(460px, 1.45fr) minmax(420px, 1fr)',
        },
        gap: 1.5,
        alignItems: 'stretch',
      }}
    >
      <Box
        sx={{
          height: '100%',
          minHeight: 0,
          overflow: 'hidden',
          gridColumn: { lg: 2 },
        }}
      >
        {mailDetails.isLoading ? (
          <Box sx={{ display: 'grid', placeItems: 'center', height: '100%' }}>
            <CircularProgress size={22} />
          </Box>
        ) : mailDetails.isError ? (
          <Alert severity="error">{t('mail.errors.tracking')}</Alert>
        ) : (
          <TrackingTimeline entries={details?.history ?? []} />
        )}
      </Box>
      <Stack
        spacing={1.5}
        sx={{
          minWidth: 0,
          height: '100%',
          overflowX: 'hidden',
          overflowY: { xs: 'auto', lg: 'hidden' },
          overscrollBehavior: 'contain',
          gridColumn: { lg: 1 },
          gridRow: { lg: 1 },
        }}
      >
        <Box
          sx={{
            minHeight: 0,
            flex: { xs: '0 0 auto', lg: '1 1 0' },
            display: 'flex',
            flexDirection: 'column',
          }}
        >
          <Stack
            direction="row"
            spacing={0.55}
            sx={{
              alignItems: 'center',
              justifyContent: 'flex-start',
              flexShrink: 0,
              mb: 1.5,
              paddingInlineStart: 0.75,
            }}
          >
            <AssignmentTurnedInOutlined sx={{ fontSize: 17 }} />
            <Typography sx={{ fontSize: 13, lineHeight: 1.35, fontWeight: 600 }}>
              {t('mail.transactionDetails')}
            </Typography>
          </Stack>
          <Box
            sx={{
              flex: 1,
              minHeight: 0,
              overflow: 'hidden',
              display: 'flex',
              flexDirection: 'column',
            }}
          >
            <Box
              role="region"
              aria-label={t('mail.scrollableData')}
              tabIndex={0}
              sx={{
                flex: 1,
                minHeight: 0,
                overflowX: 'hidden',
                overflowY: { xs: 'visible', lg: 'scroll' },
                scrollbarGutter: 'stable',
                scrollbarWidth: 'thin',
                px: 0.5,
                pb: 0.5,
                scrollbarColor: '#b8b8b8 transparent',
                '&::-webkit-scrollbar': { width: 10 },
                '&::-webkit-scrollbar-track': { bgcolor: 'transparent' },
                '&::-webkit-scrollbar-thumb': {
                  bgcolor: '#b8b8b8',
                  border: '2px solid transparent',
                  borderRadius: 8,
                },
                '&:focus-visible': { outline: '2px solid #2f6fed', outlineOffset: -2 },
              }}
            >
              {details && (
                <Paper
                  variant="outlined"
                  sx={{
                    overflow: 'hidden',
                    borderRadius: '6px',
                    borderColor: '#e5e7eb',
                    bgcolor: '#fff',
                    boxShadow: '0 1px 2px rgba(0,0,0,.02)',
                  }}
                >
                  <Box
                    sx={{
                      display: 'grid',
                      gridTemplateColumns: { xs: '1fr', lg: 'repeat(2, minmax(0, 1fr))' },
                    }}
                  >
                    {details.fields.map((field) => {
                      const controlType = normalizeDynamicControlType(field.controlType);
                      const fullWidth = [
                        'signature',
                        'longtext',
                        'file',
                        'table',
                        'label',
                      ].includes(controlType);
                      return (
                        <Box
                          key={`${field.detailId}-${field.controlDataId ?? field.controlId ?? field.controlOrder}`}
                          sx={{
                            minWidth: 0,
                            gridColumn: { xs: '1', lg: fullWidth ? '1 / -1' : 'auto' },
                          }}
                        >
                          <LabelValue
                            compact
                            label={
                              currentLanguage.code === 'ar'
                                ? field.labelAr || field.label
                                : field.label || field.labelAr
                            }
                            value={
                              <Box dir="auto">
                                <MailFieldValue field={field} />
                              </Box>
                            }
                          />
                        </Box>
                      );
                    })}
                  </Box>
                </Paper>
              )}
              {mailDetails.isLoading && (
                <Box sx={{ display: 'grid', placeItems: 'center', minHeight: 72 }}>
                  <CircularProgress size={20} />
                </Box>
              )}
              {mailDetails.isError && (
                <Alert severity="error" sx={{ borderRadius: 0 }}>
                  {t('mail.errors.details')}
                </Alert>
              )}
            </Box>
          </Box>
        </Box>

        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', sm: 'minmax(180px, .85fr) minmax(220px, 1.15fr)' },
            gap: 1.5,
            alignItems: 'start',
            flexShrink: 0,
          }}
        >
          <Paper
            variant="outlined"
            sx={{ p: 1.25, borderRadius: 1.25, boxShadow: '0 1px 3px rgba(0,0,0,.07)' }}
          >
            <Typography sx={{ mb: 1, fontSize: 12.5, fontWeight: 750 }}>
              {t('mail.requestStatus')}
            </Typography>
            <Stack direction="row" spacing={1.1} sx={{ alignItems: 'center' }}>
              <Box sx={{ position: 'relative', display: 'inline-flex', flexShrink: 0 }}>
                <CircularProgress
                  variant="determinate"
                  value={100}
                  size={42}
                  thickness={3.5}
                  sx={{ color: '#e5e5e5' }}
                />
                <CircularProgress
                  variant="determinate"
                  value={progress}
                  size={42}
                  thickness={3.5}
                  sx={{
                    position: 'absolute',
                    insetInlineStart: 0,
                    color: statusColor === 'error' ? 'error.main' : 'primary.main',
                  }}
                />
                <Box sx={{ position: 'absolute', inset: 0, display: 'grid', placeItems: 'center' }}>
                  <Typography sx={{ fontSize: 10, fontWeight: 750 }}>{progress}%</Typography>
                </Box>
              </Box>
              <Box sx={{ minWidth: 0 }}>
                <Typography sx={{ fontSize: 11.5, fontWeight: 750 }}>
                  {getStatus(request, t)}
                </Typography>
                <Typography color="text.secondary" sx={{ mt: 0.3, fontSize: 8.8 }}>
                  {t('mail.currentStage', { step: request.stepNumber })}
                </Typography>
              </Box>
            </Stack>
            <Divider sx={{ my: 1 }} />
            <Typography color="text.secondary" sx={{ fontSize: 8.8 }}>
              {t('mail.decisionDate', {
                date: formatDateTime(request.requestDate, currentLanguage.code),
              })}
            </Typography>
            <Typography sx={{ mt: 0.65, color: 'error.main', fontSize: 9.5, fontWeight: 650 }}>
              {t('mail.elapsedTime', { time: formatElapsed(request, t) })}
            </Typography>
            {request.isStopped && (
              <Chip
                size="small"
                color="error"
                label={t('mail.statuses.stopped')}
                sx={{ mt: 0.75 }}
              />
            )}
          </Paper>
          <MailAttachments
            requestId={request.recId}
            detailIds={details?.fields.map((field) => field.detailId) ?? []}
          />
        </Box>
      </Stack>
    </Box>
  );
}

function RequestedByHeaderValue({ requestId }: { requestId: number }) {
  const mailDetails = useQuery({
    queryKey: ['workflow', 'mail', 'request-details', requestId],
    queryFn: ({ signal }) => wfRequestApi.mailDetails(requestId, signal),
    enabled: requestId > 0,
  });

  return (
    <Typography
      component="span"
      dir="auto"
      noWrap
      title={mailDetails.data?.employeeName || undefined}
      sx={{ fontSize: 12, lineHeight: 1.35, color: 'text.primary' }}
    >
      {mailDetails.isLoading ? '...' : mailDetails.data?.employeeName || '-'}
    </Typography>
  );
}

const folderMatches = (request: MailRecord, folder: MailFolder): boolean => {
  if (folder === 'inbox') return !request.isFinished && !request.isStopped;
  if (folder === 'sent') return request.isFinished;
  if (folder === 'important') return request.score > 0 || request.progress >= 75;
  return true;
};

export function MailPage(): React.ReactElement {
  const { t, currentLanguage } = useAppTranslation();
  const [folder, setFolder] = React.useState<MailFolder>('all');
  const [printRequest, setPrintRequest] = React.useState<MailRecord | null>(null);
  const [selectedMailRequest, setSelectedMailRequest] = React.useState<MailRecord | null>(null);
  const [officialFormSelection, setOfficialFormSelection] = React.useState<{
    request: MailRecord;
    templateId: number;
  } | null>(null);
  const requests = useQuery({
    queryKey: ['workflow', 'mail', 'requests'],
    queryFn: ({ signal }) => wfRequestApi.list(signal),
  });
  const processes = useQuery({
    queryKey: ['workflow', 'mail', 'processes'],
    queryFn: ({ signal }) => wfProcessApi.list(signal),
  });
  const selectedProcessTemplates = useQuery({
    queryKey: ['workflow', 'print-templates', 'mail-menu', selectedMailRequest?.processId ?? 0],
    queryFn: ({ signal }) =>
      printTemplateApi.listPublishedByProcess(selectedMailRequest?.processId ?? 0, signal),
    enabled: (selectedMailRequest?.processId ?? 0) > 0,
  });
  const records = React.useMemo(() => {
    const processNames = new Map(
      (processes.data ?? []).map((process) => [
        process.recId,
        process.name || process.code || t('mail.processFallback', { id: process.recId }),
      ])
    );
    return [...(requests.data ?? [])]
      .sort(
        (left, right) =>
          new Date(right.requestDate).getTime() - new Date(left.requestDate).getTime()
      )
      .map((request, index): MailRecord => ({
        ...request,
        processName:
          processNames.get(request.processId) ||
          t('mail.processFallback', { id: request.processId }),
        requestedBy: request.requesterName || '—',
        stepNumber: index + 1,
      }))
      .filter((request) => folderMatches(request, folder));
  }, [folder, processes.data, requests.data, t]);

  const folderButtons: Array<{ id: MailFolder; label: string }> = [
    { id: 'all', label: t('mail.folders.all') },
    { id: 'inbox', label: t('mail.folders.inbox') },
    { id: 'sent', label: t('mail.folders.sent') },
    { id: 'important', label: t('mail.folders.important') },
  ];

  const config: EnterpriseListDetailsConfig<MailRecord> = {
    recordTableName: 'WfRequest',
    filterStorageKey: 'workflow.mail',
    readOnly: true,
    dataSource: {
      type: 'controlled',
      records,
      onRecordsChange: () => undefined,
      loading: requests.isLoading || processes.isLoading,
      error:
        requests.error instanceof Error
          ? requests.error.message
          : processes.error instanceof Error
            ? processes.error.message
            : null,
      refresh: async () => {
        await Promise.all([requests.refetch(), processes.refetch()]);
      },
    },
    createRecord: emptyMailRecord,
    onSelectionChange: setSelectedMailRequest,
    getPrimaryText: (request) =>
      t('mail.requestListTitle', {
        code: request.code || `#${request.recId}`,
        processName: request.processName,
      }),
    getSecondaryText: (request) =>
      t('mail.requestedBySummary', {
        name: request.requestedBy,
        date: formatDateTime(request.requestDate, currentLanguage.code),
      }),
    getProgress: (request) => request.progress,
    progressLabel: t('mail.fields.progress'),
    matchesSearch: (request, query) =>
      `${request.code ?? ''} ${request.name ?? ''} ${request.description ?? ''} ${request.processName}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: () => ({}),
    setValues: (request) => request,
    headerFields: [
      {
        id: 'request',
        label: t('mail.fields.request'),
        type: 'display',
        disabled: true,
        getValue: (request) => request.code || `#${request.recId}`,
        setValue: (request) => request,
      },
      {
        id: 'process',
        label: t('mail.fields.process'),
        type: 'display',
        disabled: true,
        getValue: (request) => request.processName,
        setValue: (request) => request,
      },
      {
        id: 'status',
        label: t('mail.fields.status'),
        type: 'display',
        disabled: true,
        getValue: (request) => getStatus(request, t),
        setValue: (request) => request,
      },
      {
        id: 'date',
        label: t('mail.fields.requestDate'),
        type: 'display',
        disabled: true,
        getValue: (request) => formatDateTime(request.requestDate, currentLanguage.code),
        setValue: (request) => request,
      },
      {
        id: 'requestedBy',
        label: t('mail.fields.requestedBy'),
        type: 'display',
        disabled: true,
        getValue: (request) => request.recId,
        setValue: (request) => request,
        render: ({ value }) => <RequestedByHeaderValue requestId={Number(value)} />,
      },
    ],
    sections: ({ record }): DetailSectionConfig[] => [
      {
        id: 'mail-details',
        title: t('mail.requestDetails'),
        hideHeader: true,
        defaultExpanded: true,
        content: <MailDetails key={record.id} request={record} />,
      },
    ],
    commands: (selectedRequest) => {
      const officialTemplates = selectPublishedTemplates(
        selectedRequest?.processId === selectedMailRequest?.processId
          ? selectedProcessTemplates.data
          : undefined
      );
      return [
        ...officialTemplates.map((template: PrintTemplateSummary) => ({
          id: `print-template-${template.templateId}`,
          label: template.isDefault
            ? `${template.name} (${t('mail.print.defaultTemplate')})`
            : template.name,
          menuLabel: t('mail.view'),
          requiresSelection: true,
          onClick: (request: MailRecord | null) => {
            if (request) setOfficialFormSelection({ request, templateId: template.templateId });
          },
        })),
        {
          id: 'default-printout',
          label: t('mail.print.defaultPrint'),
          menuLabel: t('mail.view'),
          requiresSelection: true,
          onClick: (request: MailRecord | null) => {
            if (!request) return;
            const defaultTemplate = selectDefaultPublishedTemplate(officialTemplates);
            if (defaultTemplate) {
              setOfficialFormSelection({
                request,
                templateId: defaultTemplate.templateId,
              });
              return;
            }
            setPrintRequest(request);
          },
        },
      ];
    },
    filterLabel: t('mail.filter'),
    showAttachmentAction: false,
    presentation: {
      mode: 'list',
      listWidth: 300,
      listMinWidth: 240,
      listMaxWidth: 440,
      listResizable: true,
      masterRowHeight: 82,
      compactRecordHeader: true,
      headerContent: (
        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: 'repeat(4, minmax(0, 1fr))',
            alignItems: 'center',
            width: '100%',
            height: 46,
            p: '5px 6px',
            boxSizing: 'border-box',
            borderBottom: '1px solid #e1e1e1',
          }}
        >
          {folderButtons.map((item) => (
            <Button
              key={item.id}
              size="small"
              variant={folder === item.id ? 'contained' : 'text'}
              onClick={() => setFolder(item.id)}
              sx={{
                width: '100%',
                minWidth: 0,
                minHeight: 32,
                px: '14px',
                borderRadius: '3px',
                bgcolor: folder === item.id ? '#1f5fa8' : 'transparent',
                color: folder === item.id ? '#fff' : '#004b8d',
                boxShadow: 'none',
                fontSize: 12,
                fontWeight: 500,
                textTransform: 'none',
                '&:hover': {
                  bgcolor: folder === item.id ? '#1b5596' : 'rgba(0,75,141,.06)',
                  boxShadow: 'none',
                },
              }}
            >
              {item.label}
            </Button>
          ))}
        </Box>
      ),
    },
    attachments: {
      refTableId: documentTableIds.wfRequest,
      getRefRecId: (request) => request.recId,
    },
    advancedFilter: {
      fieldLabel: t('mail.fields.request'),
      getValue: (request) => request.name || request.code,
      matches: (request, value) =>
        `${request.code ?? ''} ${request.name ?? ''}`
          .toLocaleLowerCase()
          .includes(value.trim().toLocaleLowerCase()),
      fields: [
        {
          id: 'request',
          label: t('mail.fields.request'),
          getValue: (request) => request.name || request.code || '',
        },
        {
          id: 'process',
          label: t('mail.fields.process'),
          getValue: (request) => request.processName,
        },
        {
          id: 'status',
          label: t('mail.fields.status'),
          getValue: (request) => getStatus(request, t),
        },
      ],
    },
    relatedInformation: {
      title: t('mail.information'),
      sections: (request) =>
        request
          ? [
              {
                id: 'request',
                label: t('mail.fields.request'),
                items: [
                  { label: t('mail.requestId'), value: request.recId },
                  { label: t('mail.processId'), value: request.processId },
                  { label: t('common.company'), value: request.dataAreaId },
                  {
                    label: t('common.active'),
                    value: request.isActive ? t('common.yes') : t('common.no'),
                  },
                ],
              },
            ]
          : [],
    },
  };

  return (
    <Box sx={{ height: '100%' }}>
      {officialFormSelection ? (
        <WorkflowOfficialFormViewer
          open
          request={officialFormSelection.request}
          templateId={officialFormSelection.templateId}
          onClose={() => setOfficialFormSelection(null)}
        />
      ) : printRequest ? (
        <WorkflowMailPrintoutViewer
          open
          request={printRequest}
          onClose={() => setPrintRequest(null)}
        />
      ) : (
        <ListDetailsPage variant="enterprise" title={t('nav.mail')} config={config} />
      )}
    </Box>
  );
}
