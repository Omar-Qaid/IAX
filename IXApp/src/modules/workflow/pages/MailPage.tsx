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
import AccessTimeOutlined from '@mui/icons-material/AccessTimeOutlined';
import AssignmentTurnedInOutlined from '@mui/icons-material/AssignmentTurnedInOutlined';
import AttachFileOutlined from '@mui/icons-material/AttachFileOutlined';
import PersonOutlineOutlined from '@mui/icons-material/PersonOutlineOutlined';
import { useQuery } from '@tanstack/react-query';
import { ListDetailsPage } from '@patterns/list-details/ListDetailsPage';
import type { DetailSectionConfig, EnterpriseListDetailsConfig } from '@patterns/list-details/types';
import { documentApi } from '@shared/components/documents/documentApi';
import { documentTableIds } from '@shared/components/documents/recordTableIds';
import { wfProcessApi } from '../api/wfProcessApi';
import { wfRequestApi, type WfRequestRecord } from '../api/wfRequestApi';
import { getTemporaryMailDetails } from './mailTemporaryData';

type MailFolder = 'all' | 'inbox' | 'sent' | 'important';

interface MailRecord extends WfRequestRecord {
  processName: string;
  stepNumber: number;
}

const emptyMailRecord = (): MailRecord => ({
  id: 'empty-mail', recId: 0, code: null, name: '', description: null,
  requestDate: '', processId: 0, employeeId: null, requestDetails: null,
  isFinished: false, finishedDate: null, isStopped: false, stoppedDate: null,
  score: 0, progress: 0, notes: null, isActive: true, rowVersion: null,
  recVersion: 1, dataAreaId: 'dat', processName: '', stepNumber: 1,
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
  return `${year}/${day}/${month} ${hour}:${minute} ${date.getHours() >= 12 ? 'PM' : 'AM'}`;
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
    <Box sx={{ display: 'grid', gridTemplateColumns: 'minmax(72px, 34%) 1fr', minHeight: 48, borderBottom: '1px solid #e8e8e8' }}>
      <Typography sx={{ p: '8px 10px', bgcolor: '#f7f7f7', fontSize: 11.5, fontWeight: 650 }}>{label}</Typography>
      <Box sx={{ p: '8px 10px', fontSize: 11.5, minWidth: 0, overflowWrap: 'anywhere' }}>{value}</Box>
    </Box>
  );
}

const formatFileSize = (bytes: number | null): string => {
  if (!bytes) return '';
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(0)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(2)} MB`;
};

function MailAttachments({ requestId }: { requestId: number }) {
  const [previewError, setPreviewError] = React.useState(false);
  const attachments = useQuery({
    queryKey: ['documents', documentTableIds.wfRequest, requestId],
    queryFn: ({ signal }) => documentApi.list(documentTableIds.wfRequest, requestId, signal),
    enabled: requestId > 0,
  });
  const items = attachments.data?.items ?? [];

  return (
    <Paper variant="outlined" sx={{ borderRadius: 1.25, overflow: 'hidden' }}>
      <Stack direction="row" spacing={0.75} sx={{ alignItems: 'center', px: 1.25, minHeight: 36, borderBottom: '1px solid #e2e2e2' }}>
        <AttachFileOutlined sx={{ fontSize: 16 }} />
        <Typography sx={{ flex: 1, fontSize: 12.5, fontWeight: 700 }}>Attachments</Typography>
        <Typography color="text.secondary" sx={{ fontSize: 11 }}>({attachments.data?.totalCount ?? 0})</Typography>
      </Stack>
      {attachments.isLoading && <Box sx={{ display: 'grid', placeItems: 'center', minHeight: 72 }}><CircularProgress size={20} /></Box>}
      {attachments.isError && <Alert severity="error" sx={{ borderRadius: 0 }}>Unable to load attachments.</Alert>}
      {previewError && <Alert severity="error" onClose={() => setPreviewError(false)} sx={{ borderRadius: 0 }}>Unable to open this attachment.</Alert>}
      {!attachments.isLoading && !attachments.isError && items.length === 0 && (
        <Typography color="text.secondary" sx={{ py: 2.5, px: 1.5, textAlign: 'center', fontSize: 11.5 }}>No attachments</Typography>
      )}
      {items.map((item) => (
        <Button
          key={item.id}
          fullWidth
          onClick={() => { setPreviewError(false); void documentApi.preview(item).catch(() => setPreviewError(true)); }}
          sx={{ display: 'grid', gridTemplateColumns: '28px minmax(0, 1fr) auto', gap: 1, minHeight: 51, px: 1.25, borderRadius: 0, borderBottom: '1px solid #eeeeee', color: 'text.primary', textAlign: 'start', textTransform: 'none' }}
        >
          <AttachFileOutlined color="error" sx={{ fontSize: 18 }} />
          <Box sx={{ minWidth: 0 }}>
            <Typography noWrap sx={{ fontSize: 11.5, fontWeight: 600 }}>{item.name || item.fileName}</Typography>
            <Typography noWrap color="text.secondary" sx={{ fontSize: 10 }}>{item.originalFileName || item.documentTypeName}</Typography>
          </Box>
          <Typography color="text.secondary" sx={{ fontSize: 9.5 }}>{formatFileSize(item.fileSize)}</Typography>
        </Button>
      ))}
    </Paper>
  );
}

function TrackingHistory({ request }: { request: MailRecord }) {
  const { history } = React.useMemo(() => getTemporaryMailDetails(request), [request]);
  return (
    <Box sx={{ minWidth: 0, width: '100%', maxWidth: 242, height: '100%', mx: 'auto', display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
      <Stack direction="row" spacing={0.55} sx={{ alignItems: 'center', justifyContent: 'flex-start', mb: 1.05, pr: 0.75 }}>
        <AccessTimeOutlined sx={{ fontSize: 16 }} />
        <Typography sx={{ fontSize: 11.5, fontWeight: 650 }}>Tracking history</Typography>
      </Stack>
      <Box sx={{ position: 'relative', flex: 1, minHeight: 0, overflowY: 'auto', overflowX: 'hidden', pl: 2.35, pr: 0.5, pb: 0.5, scrollbarWidth: 'thin', scrollbarColor: '#b8b8b8 transparent', '&::-webkit-scrollbar': { width: 6 }, '&::-webkit-scrollbar-thumb': { bgcolor: '#b8b8b8', borderRadius: 3 }, '&::-webkit-scrollbar-track': { bgcolor: 'transparent' }, '&::before': { content: '""', position: 'absolute', left: 6, top: 11, bottom: 14, width: 1, bgcolor: '#dedede' } }}>
        {history.map((entry) => (
          <Box key={entry.id} sx={{ position: 'relative', mb: 1.65 }}>
            <Box sx={{ position: 'absolute', left: -19, top: 24, width: 10, height: 10, borderRadius: '50%', bgcolor: entry.current ? '#0078d4' : '#d1d1d1', border: `2px solid ${entry.current ? '#0078d4' : '#d1d1d1'}`, boxShadow: '0 0 0 2px #fff', '&::after': entry.completed && !entry.current ? { content: '"✓"', position: 'absolute', inset: -4, color: '#fff', fontSize: 8, lineHeight: '14px', textAlign: 'center', fontWeight: 800 } : undefined }} />
            <Paper variant="outlined" sx={{ minHeight: 188, p: '12px 10px 11px', borderRadius: '6px', borderColor: entry.current ? '#1683e6' : '#e1e1e1', bgcolor: entry.current ? '#f1f7ff' : '#fff', boxShadow: '0 1px 3px rgba(0,0,0,.07)' }}>
              <Stack direction="row" sx={{ justifyContent: 'space-between', gap: 1 }}>
                <Box sx={{ minWidth: 0 }}>
                  <Typography sx={{ fontSize: 10.5, fontWeight: 750 }}>{entry.title}</Typography>
                  <Typography color="primary" sx={{ mt: 0.25, fontSize: 10 }}>{entry.subtitle}</Typography>
                </Box>
                <Typography color="text.secondary" sx={{ flexShrink: 0, fontSize: 8 }}>{formatTimelineDate(entry.date)}</Typography>
              </Stack>
              <Box sx={{ display: 'flex', mt: 0.65, justifyContent: 'flex-start' }}>
                <Box sx={{ px: 0.75, py: 0.25, borderRadius: '2px', bgcolor: entry.current ? '#0078d4' : '#e7e7e7', color: entry.current ? '#fff' : '#777', fontSize: 8.5, fontWeight: 700 }}>{entry.current ? 'Processing' : 'Completed'}</Box>
              </Box>
              <Typography sx={{ mt: 1.05, fontSize: 9.8, lineHeight: 1.45 }}><Box component="span" color="text.secondary">Responsible: </Box>{entry.actor}</Typography>
              <Typography sx={{ mt: 0.55, fontSize: 9.8, lineHeight: 1.45 }}><Box component="span" color="text.secondary">Action: </Box>{entry.action}</Typography>
              <Typography sx={{ mt: 0.55, fontSize: 9.8, lineHeight: 1.5 }}><Box component="span" color="text.secondary">Notes: </Box>{entry.details}</Typography>
            </Paper>
          </Box>
        ))}
      </Box>
    </Box>
  );
}

function MailDetails({ request }: { request: MailRecord }) {
  const details = React.useMemo(() => getTemporaryMailDetails(request), [request]);
  const progress = Math.max(0, Math.min(100, request.progress));
  const statusColor = request.isStopped ? 'error' : request.isFinished ? 'success' : 'primary';

  return (
    <Box sx={{ direction: 'ltr', height: { xs: 'auto', lg: 'calc(100dvh - 188px)' }, minHeight: 0, overflow: 'hidden', display: 'grid', gridTemplateColumns: { xs: '1fr', lg: 'minmax(460px, 1.55fr) minmax(210px, .72fr)' }, gap: 2, alignItems: 'stretch' }}>
      <Box sx={{ direction: 'ltr', height: '100%', minHeight: 0, overflow: 'hidden', gridColumn: { lg: 2 } }}><TrackingHistory request={request} /></Box>
      <Stack spacing={1.5} sx={{ direction: 'ltr', minWidth: 0, height: 'fit-content', overflow: 'hidden', gridColumn: { lg: 1 }, gridRow: { lg: 1 } }}>
        <Paper variant="outlined" sx={{ borderRadius: 1.25, overflow: 'hidden', minHeight: 354, boxShadow: '0 1px 3px rgba(0,0,0,.10)' }}>
            <Stack direction="row" spacing={0.7} sx={{ alignItems: 'center', px: 1.25, minHeight: 36, color: 'primary.main' }}>
              <AssignmentTurnedInOutlined sx={{ fontSize: 17 }} />
              <Typography sx={{ fontSize: 12.5, fontWeight: 750 }}>Transaction details</Typography>
            </Stack>
            <Box>
              <LabelValue label="Employee name" value={<Stack direction="row" spacing={0.5} sx={{ alignItems: 'center' }}><PersonOutlineOutlined sx={{ fontSize: 14 }} />{details.assignment.assignee}</Stack>} />
              <LabelValue label="Employee number" value={request.employeeId ? `F${request.employeeId}` : '—'} />
              <LabelValue label="Transaction type" value={request.description || details.assignment.activity} />
              <LabelValue label="Transaction time" value={formatDateTime(details.assignment.assignedAt)} />
              <LabelValue label="Transaction end time" value={formatDateTime(details.assignment.finishedAt)} />
              <LabelValue label="Notes" value={details.processData.activityDetails} />
            </Box>
        </Paper>

        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: 'minmax(180px, .85fr) minmax(220px, 1.15fr)' }, gap: 1.5, alignItems: 'start' }}>
          <Paper variant="outlined" sx={{ p: 1.25, borderRadius: 1.25, boxShadow: '0 1px 3px rgba(0,0,0,.07)' }}>
            <Typography sx={{ mb: 1, fontSize: 12.5, fontWeight: 750 }}>Request status</Typography>
            <Stack direction="row" spacing={1.1} sx={{ alignItems: 'center' }}>
              <Box sx={{ position: 'relative', display: 'inline-flex', flexShrink: 0 }}>
                <CircularProgress variant="determinate" value={100} size={42} thickness={3.5} sx={{ color: '#e5e5e5' }} />
                <CircularProgress variant="determinate" value={progress} size={42} thickness={3.5} sx={{ position: 'absolute', left: 0, color: statusColor === 'error' ? 'error.main' : 'primary.main' }} />
                <Box sx={{ position: 'absolute', inset: 0, display: 'grid', placeItems: 'center' }}><Typography sx={{ fontSize: 10, fontWeight: 750 }}>{progress}%</Typography></Box>
              </Box>
              <Box sx={{ minWidth: 0 }}>
                <Typography sx={{ fontSize: 11.5, fontWeight: 750 }}>{getStatus(request)}</Typography>
                <Typography color="text.secondary" sx={{ mt: 0.3, fontSize: 8.8 }}>Current stage: Step {request.stepNumber}</Typography>
              </Box>
            </Stack>
            <Divider sx={{ my: 1 }} />
            <Typography color="text.secondary" sx={{ fontSize: 8.8 }}>Decision date: {formatDateTime(request.requestDate)}</Typography>
            <Typography sx={{ mt: 0.65, color: 'error.main', fontSize: 9.5, fontWeight: 650 }}>Elapsed time: {formatElapsed(request)}</Typography>
            {request.isStopped && <Chip size="small" color="error" label="Stopped" sx={{ mt: 0.75 }} />}
          </Paper>
          <MailAttachments requestId={request.recId} />
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
  const requests = useQuery({ queryKey: ['workflow', 'mail', 'requests'], queryFn: ({ signal }) => wfRequestApi.list(signal) });
  const processes = useQuery({ queryKey: ['workflow', 'mail', 'processes'], queryFn: ({ signal }) => wfProcessApi.list(signal) });

  const records = React.useMemo(() => {
    const processNames = new Map((processes.data ?? []).map((process) => [process.recId, process.name || process.code || `Process ${process.recId}`]));
    return [...(requests.data ?? [])]
      .sort((left, right) => new Date(right.requestDate).getTime() - new Date(left.requestDate).getTime())
      .map((request, index): MailRecord => ({ ...request, processName: processNames.get(request.processId) || `Process ${request.processId}`, stepNumber: index + 1 }))
      .filter((request) => folderMatches(request, folder))
      ;
  }, [folder, processes.data, requests.data]);

  const folderButtons: Array<{ id: MailFolder; label: string }> = [
    { id: 'all', label: 'All' }, { id: 'inbox', label: 'Inbox' },
    { id: 'sent', label: 'Sent' }, { id: 'important', label: 'Important' },
  ];

  const config: EnterpriseListDetailsConfig<MailRecord> = {
    readOnly: true,
    dataSource: {
      type: 'controlled', records, onRecordsChange: () => undefined,
      loading: requests.isLoading || processes.isLoading,
      error: requests.error instanceof Error ? requests.error.message : processes.error instanceof Error ? processes.error.message : null,
      refresh: async () => { await Promise.all([requests.refetch(), processes.refetch()]); },
    },
    createRecord: emptyMailRecord,
    getPrimaryText: (request) => request.name || request.code || `Request ${request.recId}`,
    getSecondaryText: (request) => `${request.processName} · ${getStatus(request)} · ${formatDateTime(request.requestDate)}`,
    matchesSearch: (request, query) => `${request.code ?? ''} ${request.name ?? ''} ${request.description ?? ''} ${request.processName}`.toLocaleLowerCase().includes(query.toLocaleLowerCase()),
    getValues: () => ({}),
    setValues: (request) => request,
    headerFields: [
      { id: 'request', label: 'Request', type: 'display', disabled: true, getValue: (request) => request.code || `#${request.recId}`, setValue: (request) => request },
      { id: 'process', label: 'Process', type: 'display', disabled: true, getValue: (request) => request.processName, setValue: (request) => request },
      { id: 'status', label: 'Status', type: 'display', disabled: true, getValue: getStatus, setValue: (request) => request },
      { id: 'date', label: 'Request date', type: 'display', disabled: true, getValue: (request) => formatDateTime(request.requestDate), setValue: (request) => request },
    ],
    sections: ({ record }): DetailSectionConfig[] => [{
      id: 'mail-details', title: 'Request details', hideHeader: true, defaultExpanded: true,
      content: <MailDetails key={record.id} request={record} />,
    }],
    filterLabel: 'Filter',
    showAttachmentAction: false,
    presentation: {
      mode: 'list', listWidth: 300, listMinWidth: 240, listMaxWidth: 440, listResizable: true,
      masterRowHeight: 64, compactRecordHeader: true,
      headerContent: <Stack direction="row" spacing={0.1} sx={{ direction: 'ltr', minHeight: 34, px: 0.65, alignItems: 'center', justifyContent: 'space-between', borderBottom: '1px solid #e5e7eb' }}>
        {folderButtons.map((item) => (
          <Button key={item.id} size="small" variant={folder === item.id ? 'contained' : 'text'} onClick={() => setFolder(item.id)} sx={{ minWidth: 36, height: 22, px: 0.65, borderRadius: 3, fontSize: 9.5, textTransform: 'none' }}>{item.label}</Button>
        ))}
      </Stack>,
    },
    attachments: { refTableId: documentTableIds.wfRequest, getRefRecId: (request) => request.recId },
    advancedFilter: {
      fieldLabel: 'Request', getValue: (request) => request.name || request.code,
      matches: (request, value) => `${request.code ?? ''} ${request.name ?? ''}`.toLocaleLowerCase().includes(value.trim().toLocaleLowerCase()),
      fields: [
        { id: 'request', label: 'Request', getValue: (request) => request.name || request.code || '' },
        { id: 'process', label: 'Process', getValue: (request) => request.processName },
        { id: 'status', label: 'Status', getValue: getStatus },
      ],
    },
    relatedInformation: {
      title: 'Mail information',
      sections: (request) => request ? [{ id: 'request', label: 'Request', items: [
        { label: 'Request ID', value: request.recId }, { label: 'Process ID', value: request.processId },
        { label: 'Company', value: request.dataAreaId }, { label: 'Active', value: request.isActive ? 'Yes' : 'No' },
      ] }] : [],
    },
  };

  return <Box dir="ltr" sx={{ height: '100%' }}><ListDetailsPage variant="enterprise" title="Mail" config={config} /></Box>;
}
