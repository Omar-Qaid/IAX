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
import PersonOutlineOutlined from '@mui/icons-material/PersonOutlineOutlined';
import { useQueries, useQuery } from '@tanstack/react-query';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type {
  DetailSectionConfig,
  EnterpriseListDetailsConfig,
} from '@patterns/list-details/types';
import { documentApi } from '@shared/components/documents/documentApi';
import { documentTableIds } from '@shared/components/documents/recordTableIds';
import { wfProcessApi } from '../api/wfProcessApi';
import {
  wfRequestApi,
  type MailRequestFieldDto,
  type MailTrackingEntryDto,
  type WfRequestRecord,
} from '../api/wfRequestApi';
import { normalizeDynamicControlType } from '../components/DynamicControlRenderer';
import { SignatureControl } from '../components/DynamicSpecialControls';

type MailFolder = 'all' | 'inbox' | 'sent' | 'important';

interface MailRecord extends WfRequestRecord {
  processName: string;
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
  stepNumber: 1,
});

const formatDateTime = (value: string | null): string => {
  if (!value) return '—';
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? value
    : new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' }).format(date);
};

const formatTimelineDate = (value: string): string => {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  const hour = date.getHours() % 12 || 12;
  const minute = String(date.getMinutes()).padStart(2, '0');
  return `${month}/${day}/${year} ${hour}:${minute} ${date.getHours() >= 12 ? 'PM' : 'AM'}`;
};

const getStatus = (request: WfRequestRecord): string => {
  if (request.isStopped) return 'Stopped';
  if (request.isFinished) return 'Completed';
  return 'In progress';
};

const formatElapsed = (request: WfRequestRecord): string => {
  const started = new Date(request.requestDate).getTime();
  const endedValue = request.finishedDate || request.stoppedDate;
  const ended = endedValue ? new Date(endedValue).getTime() : Date.now();
  if (!Number.isFinite(started) || !Number.isFinite(ended)) return 'Not available';
  const minutes = Math.max(0, Math.floor((ended - started) / 60_000));
  return `${Math.floor(minutes / 60)} hours ${minutes % 60} minutes`;
};

function LabelValue({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <Box
      sx={{
        display: 'grid',
        gridTemplateColumns: '34% minmax(0, 66%)',
        minHeight: 48,
        borderBottom: '1px solid #e5e5e5',
      }}
    >
      <Typography
        sx={{
          p: '8px 12px',
          bgcolor: '#f7f7f7',
          color: '#111',
          fontSize: 12,
          fontWeight: 600,
        }}
      >
        {label}
      </Typography>
      <Box
        sx={{
          p: '8px 12px',
          bgcolor: '#fff',
          color: '#222',
          fontSize: 12,
          fontWeight: 400,
          minWidth: 0,
          overflowWrap: 'anywhere',
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
        <Typography sx={{ flex: 1, fontSize: 12.5, fontWeight: 700 }}>Attachments</Typography>
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
          Unable to load attachments.
        </Alert>
      )}
      {previewError && (
        <Alert severity="error" onClose={() => setPreviewError(false)} sx={{ borderRadius: 0 }}>
          Unable to open this attachment.
        </Alert>
      )}
      {!loading && !failed && items.length === 0 && (
        <Typography
          color="text.secondary"
          sx={{ py: 2.5, px: 1.5, textAlign: 'center', fontSize: 11.5 }}
        >
          No attachments
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
          <Box sx={{ minWidth: 0 }}>
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
  return (
    <Box
      dir="ltr"
      sx={{
        minWidth: 0,
        width: '100%',
        maxWidth: 'none',
        height: '100%',
        boxSizing: 'border-box',
        display: 'flex',
        flexDirection: 'column',
        overflow: 'hidden',
        direction: 'ltr',
        color: '#171717',
        fontFamily: 'Inter, Roboto, "Segoe UI", sans-serif',
        textAlign: 'left',
      }}
    >
      <Stack
        direction="row"
        spacing={0.55}
        sx={{
          direction: 'ltr',
          alignItems: 'center',
          justifyContent: 'flex-start',
          mb: 1.5,
          paddingInlineStart: 0.75,
        }}
      >
        <HistoryOutlined sx={{ fontSize: 17 }} />
        <Typography sx={{ fontSize: 13, lineHeight: 1.35, fontWeight: 600 }}>
          Tracking Log
        </Typography>
      </Stack>
      <Box
        sx={{
          position: 'relative',
          flex: 1,
          minHeight: 0,
          direction: 'ltr',
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
              direction: 'ltr',
              textAlign: 'left',
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
                <Box sx={{ minWidth: 0 }}>
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
                  {formatTimelineDate(entry.date)}
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
                  {entry.isCurrent ? 'Processing' : 'Completed'}
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
                  Responsible:
                </Box>
                <Box component="span">{entry.responsible}</Box>
                <Box component="span" sx={{ color: '#555' }}>
                  Action:
                </Box>
                <Box component="span">{entry.action}</Box>
                <Box component="span" sx={{ color: '#555' }}>
                  Notes:
                </Box>
                <Box component="span" sx={{ overflowWrap: 'anywhere' }}>
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

const dynamicFieldValue = (field: MailRequestFieldDto): React.ReactNode => {
  if (normalizeDynamicControlType(field.controlType) === 'signature') {
    return (
      <SignatureControl
        control={{ label: field.labelAr || field.label, hideLabel: true, controlType: 'signature', readOnly: true }}
        value={field.value}
        onChange={() => undefined}
        preview
      />
    );
  }
  return field.value || '—';
};

function MailDetails({ request }: { request: MailRecord }) {
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
        direction: 'ltr',
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
          direction: 'ltr',
          height: '100%',
          minHeight: 0,
          overflow: 'hidden',
          gridColumn: { lg: 2 },
        }}
      >
        {mailDetails.isLoading ? (
          <Box sx={{ display: 'grid', placeItems: 'center', height: '100%' }}><CircularProgress size={22} /></Box>
        ) : mailDetails.isError ? (
          <Alert severity="error">Unable to load workflow tracking history.</Alert>
        ) : (
          <TrackingTimeline entries={details?.history ?? []} />
        )}
      </Box>
      <Stack
        spacing={1.5}
        sx={{
          direction: 'ltr',
          minWidth: 0,
          height: '100%',
          overflowX: 'hidden',
          overflowY: 'auto',
          gridColumn: { lg: 1 },
          gridRow: { lg: 1 },
        }}
      >
        <Paper
          variant="outlined"
          sx={{
            borderRadius: '3px',
            overflow: 'hidden',
            minHeight: 0,
            height: 'auto',
            boxShadow: 'none',
          }}
        >
          <Stack
            direction="row"
            spacing={0.7}
            sx={{
              alignItems: 'center',
              px: '10px',
              height: 38,
              color: '#004b8d',
              borderBottom: '1px solid #e5e5e5',
            }}
          >
            <AssignmentTurnedInOutlined sx={{ fontSize: 17 }} />
            <Typography sx={{ fontSize: 14, fontWeight: 600 }}>Transaction details</Typography>
          </Stack>
          <Box>
            <LabelValue label="Request ID" value={details?.requestId ?? request.recId} />
            <LabelValue label="Process name" value={details?.processName ?? request.processName} />
            <LabelValue label="Request status" value={details?.status ?? getStatus(request)} />
            <LabelValue label="Request date" value={formatDateTime(details?.requestDate ?? request.requestDate)} />
            <LabelValue
              label="Employee name"
              value={
                <Stack direction="row" spacing={0.5} sx={{ alignItems: 'center' }}>
                  <PersonOutlineOutlined sx={{ fontSize: 14 }} />
                  {details?.employeeName ?? '—'}
                </Stack>
              }
            />
            <LabelValue
              label="Employee number"
              value={details?.employeeNumber ?? '—'}
            />
            <LabelValue
              label="Transaction type"
              value={details?.transactionType ?? '—'}
            />
            <LabelValue
              label="Transaction time"
              value={formatDateTime(details?.transactionTime ?? request.requestDate)}
            />
            <LabelValue
              label="Transaction end time"
              value={formatDateTime(details?.transactionEndTime ?? null)}
            />
            {details?.responsibleEmployee && <LabelValue label="Responsible employee" value={details.responsibleEmployee} />}
            {details?.fields.map((field) => (
              <LabelValue
                key={`${field.detailId}-${field.controlDataId ?? field.controlId ?? field.controlOrder}`}
                label={field.labelAr || field.label}
                value={<Box dir={field.labelAr ? 'rtl' : 'ltr'}>{dynamicFieldValue(field)}</Box>}
              />
            ))}
            {mailDetails.isLoading && <Box sx={{ display: 'grid', placeItems: 'center', minHeight: 72 }}><CircularProgress size={20} /></Box>}
            {mailDetails.isError && <Alert severity="error" sx={{ borderRadius: 0 }}>Unable to load request details.</Alert>}
          </Box>
        </Paper>

        <Box
          sx={{
            display: 'grid',
            gridTemplateColumns: { xs: '1fr', sm: 'minmax(180px, .85fr) minmax(220px, 1.15fr)' },
            gap: 1.5,
            alignItems: 'start',
          }}
        >
          <Paper
            variant="outlined"
            sx={{ p: 1.25, borderRadius: 1.25, boxShadow: '0 1px 3px rgba(0,0,0,.07)' }}
          >
            <Typography sx={{ mb: 1, fontSize: 12.5, fontWeight: 750 }}>Request status</Typography>
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
                    left: 0,
                    color: statusColor === 'error' ? 'error.main' : 'primary.main',
                  }}
                />
                <Box sx={{ position: 'absolute', inset: 0, display: 'grid', placeItems: 'center' }}>
                  <Typography sx={{ fontSize: 10, fontWeight: 750 }}>{progress}%</Typography>
                </Box>
              </Box>
              <Box sx={{ minWidth: 0 }}>
                <Typography sx={{ fontSize: 11.5, fontWeight: 750 }}>
                  {getStatus(request)}
                </Typography>
                <Typography color="text.secondary" sx={{ mt: 0.3, fontSize: 8.8 }}>
                  Current stage: Step {request.stepNumber}
                </Typography>
              </Box>
            </Stack>
            <Divider sx={{ my: 1 }} />
            <Typography color="text.secondary" sx={{ fontSize: 8.8 }}>
              Decision date: {formatDateTime(request.requestDate)}
            </Typography>
            <Typography sx={{ mt: 0.65, color: 'error.main', fontSize: 9.5, fontWeight: 650 }}>
              Elapsed time: {formatElapsed(request)}
            </Typography>
            {request.isStopped && (
              <Chip size="small" color="error" label="Stopped" sx={{ mt: 0.75 }} />
            )}
          </Paper>
          <MailAttachments requestId={request.recId} detailIds={details?.fields.map((field) => field.detailId) ?? []} />
        </Box>
      </Stack>
    </Box>
  );
}

const folderMatches = (request: MailRecord, folder: MailFolder): boolean => {
  if (folder === 'inbox') return !request.isFinished && !request.isStopped;
  if (folder === 'sent') return request.isFinished;
  if (folder === 'important') return request.score > 0 || request.progress >= 75;
  return true;
};

export function MailPage(): React.ReactElement {
  const [folder, setFolder] = React.useState<MailFolder>('all');
  const requests = useQuery({
    queryKey: ['workflow', 'mail', 'requests'],
    queryFn: ({ signal }) => wfRequestApi.list(signal),
  });
  const processes = useQuery({
    queryKey: ['workflow', 'mail', 'processes'],
    queryFn: ({ signal }) => wfProcessApi.list(signal),
  });

  const records = React.useMemo(() => {
    const processNames = new Map(
      (processes.data ?? []).map((process) => [
        process.recId,
        process.name || process.code || `Process ${process.recId}`,
      ])
    );
    return [...(requests.data ?? [])]
      .sort(
        (left, right) =>
          new Date(right.requestDate).getTime() - new Date(left.requestDate).getTime()
      )
      .map((request, index): MailRecord => ({
        ...request,
        processName: processNames.get(request.processId) || `Process ${request.processId}`,
        stepNumber: index + 1,
      }))
      .filter((request) => folderMatches(request, folder));
  }, [folder, processes.data, requests.data]);

  const folderButtons: Array<{ id: MailFolder; label: string }> = [
    { id: 'all', label: 'All' },
    { id: 'inbox', label: 'Inbox' },
    { id: 'sent', label: 'Sent' },
    { id: 'important', label: 'Important' },
  ];

  const config: EnterpriseListDetailsConfig<MailRecord> = {
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
    getPrimaryText: (request) => request.name || request.code || `Request ${request.recId}`,
    getSecondaryText: (request) =>
      `${request.processName} · ${getStatus(request)} · ${formatDateTime(request.requestDate)}`,
    matchesSearch: (request, query) =>
      `${request.code ?? ''} ${request.name ?? ''} ${request.description ?? ''} ${request.processName}`
        .toLocaleLowerCase()
        .includes(query.toLocaleLowerCase()),
    getValues: () => ({}),
    setValues: (request) => request,
    headerFields: [
      {
        id: 'request',
        label: 'Request',
        type: 'display',
        disabled: true,
        getValue: (request) => request.code || `#${request.recId}`,
        setValue: (request) => request,
      },
      {
        id: 'process',
        label: 'Process',
        type: 'display',
        disabled: true,
        getValue: (request) => request.processName,
        setValue: (request) => request,
      },
      {
        id: 'status',
        label: 'Status',
        type: 'display',
        disabled: true,
        getValue: getStatus,
        setValue: (request) => request,
      },
      {
        id: 'date',
        label: 'Request date',
        type: 'display',
        disabled: true,
        getValue: (request) => formatDateTime(request.requestDate),
        setValue: (request) => request,
      },
    ],
    sections: ({ record }): DetailSectionConfig[] => [
      {
        id: 'mail-details',
        title: 'Request details',
        hideHeader: true,
        defaultExpanded: true,
        content: <MailDetails key={record.id} request={record} />,
      },
    ],
    filterLabel: 'Filter',
    showAttachmentAction: false,
    presentation: {
      mode: 'list',
      listWidth: 300,
      listMinWidth: 240,
      listMaxWidth: 440,
      listResizable: true,
      masterRowHeight: 64,
      compactRecordHeader: true,
      headerContent: (
        <Box
          sx={{
            direction: 'ltr',
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
      fieldLabel: 'Request',
      getValue: (request) => request.name || request.code,
      matches: (request, value) =>
        `${request.code ?? ''} ${request.name ?? ''}`
          .toLocaleLowerCase()
          .includes(value.trim().toLocaleLowerCase()),
      fields: [
        {
          id: 'request',
          label: 'Request',
          getValue: (request) => request.name || request.code || '',
        },
        { id: 'process', label: 'Process', getValue: (request) => request.processName },
        { id: 'status', label: 'Status', getValue: getStatus },
      ],
    },
    relatedInformation: {
      title: 'Mail information',
      sections: (request) =>
        request
          ? [
              {
                id: 'request',
                label: 'Request',
                items: [
                  { label: 'Request ID', value: request.recId },
                  { label: 'Process ID', value: request.processId },
                  { label: 'Company', value: request.dataAreaId },
                  { label: 'Active', value: request.isActive ? 'Yes' : 'No' },
                ],
              },
            ]
          : [],
    },
  };

  return (
    <Box dir="ltr" sx={{ height: '100%' }}>
      <ListDetailsPage variant="enterprise" title="Mail" config={config} />
    </Box>
  );
}
